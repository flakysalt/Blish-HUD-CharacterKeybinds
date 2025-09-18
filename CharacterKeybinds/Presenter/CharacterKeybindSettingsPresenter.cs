using Blish_HUD.Graphics.UI;
using flakysalt.CharacterKeybinds.Views;
using Blish_HUD.Modules.Managers;
using Blish_HUD;
using System;
using System.Threading.Tasks;
using System.IO;
using flakysalt.CharacterKeybinds.Views.UiElements;
using flakysalt.CharacterKeybinds.Model;
using Gw2Sharp.WebApi.V2.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using flakysalt.CharacterKeybinds.Util;
using flakysalt.CharacterKeybinds.Data;
using Microsoft.Xna.Framework;

namespace flakysalt.CharacterKeybinds.Presenter
{
    public class CharacterKeybindSettingsPresenter : Presenter<CharacterKeybindsTab, CharacterKeybindModel>, IDisposable
    {
        private readonly Logger Logger = Logger.GetLogger<CharacterKeybindSettingsPresenter>();

        private static object taskLock = new object();
        private static bool isTaskStarted;

        private double _updateCharactersRunningTime;
        private double _updateTime = 5_000;

        Gw2ApiManager _Gw2ApiManager;
        Autoclicker _autoClicker;


        public CharacterKeybindSettingsPresenter(CharacterKeybindsTab view, CharacterKeybindModel model
            , Gw2ApiManager apiManager, Autoclicker autoclicker) : base(view, model)
        {
            _Gw2ApiManager = apiManager;
            _autoClicker = autoclicker;
            
            AttachToGameServices();
            AttachViewHandler();
            AttachModelHandler();

            _ = LoadCharacterInformationAsync();
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
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "[CharacterKeybindSettingsPresenter] Failed to attach to Game Services");
            }
        }

        void OnLocaleChange(object sender ,ValueEventArgs<CultureInfo> info)
        {
            Model.ClearResources();
            Task.Run(LoadCharacterInformationAsync);
            SetUpdateInterval(5_000);
        }

        public void Dispose()
        {
            GameService.Gw2Mumble.PlayerCharacter.NameChanged -= PlayerCharacter_NameChanged;
            GameService.Gw2Mumble.PlayerCharacter.SpecializationChanged -= PlayerCharacter_SpecializationChanged;
            
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
                Logger.Error(e, "[CharacterKeybindSettingsPresenter]Failed to attach to View");
            }
        }

        private void AttachModelHandler()
        {
            try
            {
                Model.BindCharacterDataChanged(OnKeymapsChanged);
                Model.BindKeymapChanged(OnKeymapsChanged);
            }
            catch (Exception e)
            {
                Logger.Error(e, "[CharacterKeybindSettingsPresenter]Failed to attach to Model");
            }
        }

        void OnKeymapsChanged() 
        {
            try
            {
                View?.SetSpinner(true);

                View?.ClearKeybindEntries();

                var keybindsFolder = Model?.GetKeybindsFolder();
                if (keybindsFolder == null) throw new InvalidOperationException("Keybinds folder is not initialized.");

                View?.SetDefaultKeybindOptions(CharacterKeybindFileUtil.GetKeybindFiles(keybindsFolder),
                    Model.GetDefaultKeybind());

                foreach (var keymap in Model.GetKeymaps() ?? Enumerable.Empty<Keymap>())
                {
                    int iconAssetId = 0;

                    var character = Model.GetCharacter(keymap.CharacterName);
                    if (character != null)
                    {
                        iconAssetId =
                            int.Parse(Path.GetFileNameWithoutExtension(Model.GetProfession(character.Profession).Icon
                                .Url.AbsoluteUri));
                    }

                    var container = View?.AddKeybind();
                    if (container == null) continue;

                    View?.SetKeybindOptions(container, Model.GetCharacterNames(),
                        Model.GetProfessionSpecializations(keymap.CharacterName),
                        CharacterKeybindFileUtil.GetKeybindFiles(keybindsFolder));
                    View?.SetKeybindValues(container, keymap, iconAssetId);
                    View?.AttachListeners(container, OnApplyKeymap, OnKeymapChange, OnKeymapRemoved);
                }

                View?.SetSpinner(false);
            }
            catch (Exception ex)
            {
                Logger.Fatal($"Exception in OnKeymapsChanged: {ex}");
            }
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
                
                var currentSpecialization = await _Gw2ApiManager.Gw2ApiClient.V2.Specializations.GetAsync(specialization);
                var keymap = Model.GetKeymapName(newCharacterName, currentSpecialization)?.KeymapName ?? Model.GetDefaultKeybind();

                if (keymap != Model.currentKeybinds)
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
            Model.currentKeybinds = sourceFileName;

            string sourceFile = Path.Combine(keybindsFolder, "Cache", $"{sourceFileName}.xml");
            string destFile = Path.Combine(keybindsFolder, "CharacterKeybinds.xml");

            try
            {
                if (!System.IO.File.Exists(Path.Combine(keybindsFolder, $"{sourceFileName}.xml"))) return;
                CharacterKeybindFileUtil.MoveAllXmlFiles(keybindsFolder, Path.Combine(keybindsFolder, "Cache"));
                System.IO.File.Copy(sourceFile, destFile);
                await _autoClicker.ClickInOrder();
            }
            catch (Exception e)
            {
                Logger.Error($"Error copying files\n{e}");
            }
            finally
            {
                if (System.IO.File.Exists(destFile))
                {
                    System.IO.File.Delete(destFile);
                }
                CharacterKeybindFileUtil.MoveAllXmlFiles(Path.Combine(keybindsFolder, "Cache"), keybindsFolder);
            }

        }
        private void PlayerCharacter_SpecializationChanged(object sender, ValueEventArgs<int> newSpecialization)
        {
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
                    Task.Run(() => SetupKeybinds(newCharacterName.Value, GameService.Gw2Mumble.PlayerCharacter.Specialization));
                }
            }
        }

        private async Task LoadCharacterInformationAsync()
        {
            var apiKeyPermissions = new List<TokenPermission>
            {
                TokenPermission.Account,
                TokenPermission.Characters
            };

            View.SetBlocker(!_Gw2ApiManager.HasPermissions(apiKeyPermissions));
            try
            {
                await LoadResources();
                OnKeymapsChanged();
            }
            catch (Exception e)
            {
                Logger.Info($"Failed to load data from the API \n {e}");
            }
        }

        private async Task LoadResources()
        {
            Model.SetCharacters( await _Gw2ApiManager.Gw2ApiClient.V2.Characters.AllAsync());

            IEnumerable<Specialization> specializations = new List<Specialization>();
            IEnumerable<Profession> professions = new List<Profession>();
            
            professions = await _Gw2ApiManager.Gw2ApiClient.V2.Professions.AllAsync();
            specializations = await _Gw2ApiManager.Gw2ApiClient.V2.Specializations.AllAsync();

            if (Model.Settings.characterKeybinds.Value.Any() || !Model.Settings.Keymaps.Value.Any())
            {
               var keymaps = SaveDataMigration.MigrateToKeymaps(Model.Settings.characterKeybinds.Value, specializations);
               Model.Settings.Keymaps.Value = keymaps;
            }

            


            foreach (var specialization in specializations)
            {
                if (!specialization.Elite) continue;

                Profession profesion = professions.First(p => p.Id == specialization.Profession);
                Model.AddProfessionEliteSpecialization(profesion, specialization);
            }
            SetUpdateInterval(60_000);
        }

        private void SetUpdateInterval(double interval)
        {
            _updateTime = interval;
        }
    }
}

