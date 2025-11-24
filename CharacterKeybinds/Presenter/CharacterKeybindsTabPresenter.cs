using Blish_HUD.Graphics.UI;
using flakysalt.CharacterKeybinds.Views;
using Blish_HUD;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using flakysalt.CharacterKeybinds.Views.UiElements;
using flakysalt.CharacterKeybinds.Model;
using System.Globalization;
using System.Linq;
using flakysalt.CharacterKeybinds.Resources;
using flakysalt.CharacterKeybinds.Util;
using flakysalt.CharacterKeybinds.Data;
using Microsoft.Xna.Framework;
using flakysalt.CharacterKeybinds.Services;
using Gw2Sharp.WebApi.V2.Models;
using File = System.IO.File;

namespace flakysalt.CharacterKeybinds.Presenter
{
    public class CharacterKeybindsTabPresenter : Presenter<CharacterKeybindsTab, CharacterKeybindsModel>,
        IDisposable
    {
        private readonly Logger Logger = Logger.GetLogger<CharacterKeybindsTabPresenter>();
        private int errorRetryCount = 0;

        private static object taskLock = new object();
        private static bool isTaskStarted;

        private double _updateCharactersRunningTime;
        private double _updateTime = 5_000;

        private readonly Gw2ApiService _apiService;
        private CharacterKeybindsSettings _settingsModel;
        private readonly AutoClickerView _autoClicker;

        public CharacterKeybindsTabPresenter(
            CharacterKeybindsTab view,
            CharacterKeybindsModel model,
            Gw2ApiService apiService,
            CharacterKeybindsSettings settingsModel,
            AutoClickerView autoClickerView) : base(view, model)
        {
            _apiService = apiService;
            _autoClicker = autoClickerView;
            _settingsModel = settingsModel;

            AttachToGameServices();
            AttachViewHandler();
            AttachModelHandler();

            _ = LoadCharacterInformationAsync();
            DoUpdateView();
        }

        public void Update(GameTime gameTime)
        {
            _updateCharactersRunningTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_updateCharactersRunningTime > _updateTime)
            {
                _updateCharactersRunningTime = 0;
                Task.Run(LoadCharacterInformationAsync);
            }
        }

        private void AttachToGameServices()
        {
            try
            {
                GameService.Overlay.UserLocaleChanged += OnLocaleChange;
                GameService.Gw2Mumble.PlayerCharacter.NameChanged += PlayerCharacter_NameChanged;
                GameService.Gw2Mumble.PlayerCharacter.SpecializationChanged += PlayerCharacter_SpecializationChanged;
                _apiService.SubtokenUpdated += OnSubtokenUpdated;

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "[CharacterKeybindSettingsPresenter] Failed to attach to Game Services");
            }
        }
        private void OnSubtokenUpdated(object sender, ValueEventArgs<IEnumerable<TokenPermission>> e)
        {
            Task.Run(LoadCharacterInformationAsync);
            SetUpdateInterval(5_000);
        }
        void OnLocaleChange(object sender, ValueEventArgs<CultureInfo> info)
        {
            Model.ClearResources();
            Task.Run(LoadCharacterInformationAsync);
            SetUpdateInterval(5_000);
        }

        public void Dispose()
        {
            GameService.Overlay.UserLocaleChanged -= OnLocaleChange;
            GameService.Gw2Mumble.PlayerCharacter.NameChanged -= PlayerCharacter_NameChanged;
            GameService.Gw2Mumble.PlayerCharacter.SpecializationChanged -= PlayerCharacter_SpecializationChanged;
            _apiService.SubtokenUpdated -= OnSubtokenUpdated;

            View.OnAddButtonClicked -= OnAddButtonPressed;
            View.OnApplyDefaultKeymapClicked -= OnApplyDefaultKeymap;
            View.OnDefaultKeymapChanged -= OnChangeDefaultKeymap;
        }

        private void AttachViewHandler()
        {
            try
            {
                View.OnAddButtonClicked += OnAddButtonPressed;
                View.OnApplyDefaultKeymapClicked += OnApplyDefaultKeymap;
                View.OnDefaultKeymapChanged += OnChangeDefaultKeymap;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to attach to View");
            }
        }

        private void AttachModelHandler()
        {
            try
            {
                Model.BindCharacterDataChanged(UpdateView);
                Model.BindKeymapChanged(UpdateView);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to attach to Model");
            }
        }

        protected override void UpdateView()
        {
            //check for permissions first
            try
            {
                View?.ClearKeybindEntries();

                var keybindsFolder = Model.GetKeybindsFolder();
                View?.SetDefaultKeybindOptions(CharacterKeybindFileUtil.GetKeybindFiles(keybindsFolder),
                    Model.GetDefaultKeybind());

                foreach (var keymap in Model.GetKeymaps() ?? Enumerable.Empty<Keymap>())
                {
                    int iconAssetId = 0;

                    var character = Model.GetCharacter(keymap.CharacterName);
                    if (character != null)
                    {
                        var spec = Model.GetSpecializationById(keymap.SpecialisationId);
                        if (spec?.ProfessionIconBig != null)
                        {
                            iconAssetId =
                                int.Parse(Path.GetFileNameWithoutExtension(spec.ProfessionIconBig.Value.Url.AbsoluteUri));
                        }
                        else
                        {
                            iconAssetId =
                                int.Parse(Path.GetFileNameWithoutExtension(Model.GetProfession(character.Profession).IconBig
                                    .Url.AbsoluteUri));
                        }
                    }

                    var container = View?.AddKeybind();
                    if (container == null) continue;

                    View?.SetKeybindOptions(container, Model.GetCharacterNames(),
                        Model.GetProfessionSpecializations(keymap.CharacterName),
                        CharacterKeybindFileUtil.GetKeybindFiles(keybindsFolder));
                    View?.SetKeybindValues(container, keymap, iconAssetId);
                    View?.AttachListeners(container, OnApplyKeymap, OnKeymapChange, OnKeymapRemoved);
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal($"UpdateView Failed: {ex}");
            }
            finally
            {

                View?.SetErrorInfoIcon(HasErrors(), Model.IsDataLoaded);
            }
            base.UpdateView();
        }

        async Task<List<string>> HasErrors()
        {
            List<string> errors = new List<string>();

            if (await _apiService.IsApiAvailable() == false)
            {
                errors.Add(Loca.errorMessageMissingApiDown);
            }
            if (_apiService.HasSubtoken() == false)
            {
                errors.Add(Loca.errorMessageMissingSubtoken);
            }
            
            if (_apiService.HasSubtoken() && _apiService.HasRequiredPermissions() == false)
            {
                errors.Add(Loca.errorMessageMissingApiPermissions);
            }

            if (Model.NeedsMigration)
            {
                errors.Add(Loca.errorMessageNeedsMigration);
            }

            if (Model.KeybindsFoldersValid == false)
            {
                errors.Add(Loca.errorMessageInvalidFolder);
            }

            return errors;
        }


        public void OnApplyKeymap(object sender, Keymap characterKeybind)
        {
            _ = ChangeKeybinds(characterKeybind.KeymapName, Model.GetKeybindsFolder());
        }

        public void OnApplyDefaultKeymap(object sender, string keymap)
        {
            _ = ChangeKeybinds(keymap, Model.GetKeybindsFolder());
        }

        public void OnChangeDefaultKeymap(object sender, string keymap)
        {
            Model.SetDefaultKeymap(keymap);
        }

        public void OnAddButtonPressed(object sender, EventArgs args)
        {
            AddKeybindEntry();
        }
        public void OnKeybindTabSelected(object sender, EventArgs args)
        {
            UpdateView();
        }

        public void OnKeymapChange(object sender, KeymapEventArgs keymapArgs)
        {
            Model.UpdateKeymap(keymapArgs.OldCharacterKeymap, keymapArgs.NewCharacterKeymap);
        }

        public void OnKeymapRemoved(object sender, Keymap characterKeybind)
        {
            Model.RemoveKeymap(characterKeybind);
        }

        public void AddKeybindEntry()
        {
            Model.AddKeymap();
        }

        public async Task SetupKeybinds(string newCharacterName = "", int specialization = -1)
        {
            try
            {
                if (string.IsNullOrEmpty(newCharacterName)) return;

                var currentSpecialization = await _apiService.GetSpecializationAsync(specialization);

                string keymap;
                if (_settingsModel.useDefaultKeybinds.Value)
                {
                    keymap = Model.GetKeymapName(newCharacterName, currentSpecialization)?.KeymapName ??
                             Model.GetDefaultKeybind();
                }
                else
                {
                    keymap = Model.GetKeymapName(newCharacterName, currentSpecialization)?.KeymapName;
                }

                if (keymap != Model.CurrentKeybinds && !string.IsNullOrEmpty(keymap))
                {
                    await ChangeKeybinds(keymap, Model.GetKeybindsFolder());
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Error Setting up keybinds\n{e}");
            }
            finally
            {
                isTaskStarted = false;
            }
        }

        async Task ChangeKeybinds(string sourceFileName, string keybindsFolder)
        {
            Model.CurrentKeybinds = sourceFileName;

            string sourceFile = Path.Combine(keybindsFolder, "Cache", $"{sourceFileName}.xml");
            string destFile = Path.Combine(keybindsFolder, "CharacterKeybinds.xml");

            try
            {
                if (!File.Exists(Path.Combine(keybindsFolder, $"{sourceFileName}.xml"))) return;
                CharacterKeybindFileUtil.MoveAllXmlFiles(keybindsFolder, Path.Combine(keybindsFolder, "Cache"));
                File.Copy(sourceFile, destFile);
                await _autoClicker.ClickInOrder();
            }
            catch (Exception e)
            {
                Logger.Error($"Error copying files\n{e}");
            }
            finally
            {
                if (File.Exists(destFile))
                {
                    File.Delete(destFile);
                }

                CharacterKeybindFileUtil.MoveAllXmlFiles(Path.Combine(keybindsFolder, "Cache"), keybindsFolder);
            }
        }

        private void PlayerCharacter_SpecializationChanged(object sender, ValueEventArgs<int> newSpecialization)
        {
            if (_settingsModel.changeKeybindsWhenSwitchingSpecialization.Value == false) return;
            lock (taskLock)
            {
                if (!isTaskStarted)
                {
                    isTaskStarted = true;
                    Task.Run(() => SetupKeybinds(GameService.Gw2Mumble.PlayerCharacter.Name, newSpecialization.Value));
                }
            }
        }

        private void PlayerCharacter_NameChanged(object sender, ValueEventArgs<string> newCharacterName)
        {
            lock (taskLock)
            {
                if (!isTaskStarted)
                {
                    isTaskStarted = true;
                    Task.Run(() =>
                        SetupKeybinds(newCharacterName.Value, GameService.Gw2Mumble.PlayerCharacter.Specialization));
                }
            }
        }

        private async Task LoadCharacterInformationAsync()
        {
            if (!_apiService.HasSubtoken())
            {
                return;
            }
            
            try
            {
                View.SetSpinner(true);
                await Model.LoadResourcesAsync();
                SetUpdateInterval(300_000); // Set to 5 minute after initial load
                errorRetryCount = 0;
            }
            catch (Exception e)
            {
                errorRetryCount++;
                if (errorRetryCount % 5 == 0)
                {
                    Logger.Error($"Failed to load data from the API! Retries: {errorRetryCount} \n {e}");

                }
                Logger.Info($"Failed to load data from the API \n {e}");
            }
            finally
            {
                View.SetSpinner(false);
            }
        }

        private void SetUpdateInterval(double interval)
        {
            _updateTime = interval;
        }
    }
}
