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
        private double updateTime = 5_000;

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
            var test = GameService.Gw2Mumble.PlayerCharacter.Name;
            _updateCharactersRunningTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_updateCharactersRunningTime > updateTime)
            {
                _updateCharactersRunningTime = 0;
                Task.Run(LoadCharacterInformationAsync);
            }
        }

        private void AttachToGameServices()
        {
            GameService.Gw2Mumble.PlayerCharacter.NameChanged += PlayerCharacter_NameChanged;
            GameService.Gw2Mumble.PlayerCharacter.SpecializationChanged += PlayerCharacter_SpecializationChanged;

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
            View.OnAddButtonClicked += OnAddButtonPressed;
            View.OnApplyDefaultKeymapClicked += OnApplyDefaultKeymap;
            View.OnDefaultKeymapChanged += OnChangeDefaultKeymap;
        }

        private void AttachModelHandler()
        {
            Model.BindCharacterDataChanged(OnKeymapsChanged);
            Model.BindKeymapChanged(OnKeymapsChanged);
        }

        void OnKeymapsChanged() 
        {
            View.ClearKeybindEntries();

            View.SetDefaultKeybindOptions(CharacterKeybindFileUtil.GetKeybindFiles(Model.GetKeybindsFolder()),
                Model.GetDefaultKeybind());

            foreach (var keymap in Model.GetKeymaps())
            {
                var iconAssetId = 0;
                var backgroundId = 0;

                var character = Model.GetCharacter(keymap.characterName);
                if (character != null)
                {
                    iconAssetId = int.Parse(Path.GetFileNameWithoutExtension(Model.GetProfession(character.Profession).Icon.Url.AbsoluteUri));
                }
                
                var container = View.AddKeybind();
                View.SetKeybindOptions(container, Model.GetCharacterNames(),Model.GetProfessionSpecializations(keymap.characterName), CharacterKeybindFileUtil.GetKeybindFiles(Model.GetKeybindsFolder()));
                View.SetKeybindValues(container, keymap,iconAssetId);
                View.AttachListeners(container,OnApplyKeymap, OnKeymapChange, OnKeymapRemoved);
            }
        }
        public void OnApplyKeymap(object sender, CharacterKeybind characterKeybind)
        {
			_ = ChangeKeybinds(characterKeybind.keymap, Model.GetKeybindsFolder());
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
            Model.UpdateKeymap(keymapArgs.OldCharacterKeybind, keymapArgs.NewCharacterKeybind);
        }

        public void OnKeymapRemoved(object sender, CharacterKeybind characterKeybind)
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
                var keymap = Model.GetKeymapName(newCharacterName, currentSpecialization)?.keymap ?? Model.GetDefaultKeybind();

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
                Model.SetCharacters( await _Gw2ApiManager.Gw2ApiClient.V2.Characters.AllAsync());
                OnKeymapsChanged();
            }
            catch (Exception e)
            {
                Logger.Info($"Failed to load data from the API \n {e}");
            }
        }

        private async Task LoadResources()
        {
            IEnumerable<Specialization> specializations = new List<Specialization>();
            IEnumerable<Profession> professions = new List<Profession>();

            professions = await _Gw2ApiManager.Gw2ApiClient.V2.Professions.AllAsync();
            specializations = await _Gw2ApiManager.Gw2ApiClient.V2.Specializations.AllAsync();

            foreach (var specialization in specializations)
            {
                if (!specialization.Elite) continue;

                Profession profesion = professions.First(p => p.Id == specialization.Profession);
                Model.AddProfessionEliteSpecialization(profesion, specialization);
            }
        }
    }
}

