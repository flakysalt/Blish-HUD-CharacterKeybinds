using Blish_HUD.Graphics.UI;
using flakysalt.CharacterKeybinds.Views;
using Blish_HUD.Modules.Managers;
using Blish_HUD;
using System;
using System.Threading.Tasks;
using System.IO;
using flakysalt.CharacterKeybinds.Views.UiElements;
using CharacterKeybinds.Model;
using Gw2Sharp.WebApi.V2.Models;
using System.Collections.Generic;
using System.Linq;
using flakysalt.CharacterKeybinds.Util;
using flakysalt.CharacterKeybinds.Data;
using Microsoft.Xna.Framework;

namespace flakysalt.CharacterKeybinds.Presenter
{
    public class CharacterKeybindSettingsPresenter : Presenter<CharacterKeybindWindow, CharacterKeybindModel>, IDisposable
    {
        private readonly Logger Logger = Logger.GetLogger<CharacterKeybindSettingsPresenter>();

        private static object taskLock = new object();
        private static bool isTaskStarted = false;

        private double _updateCharactersRunningTime;
        private double updateTime = 5_000;

        Gw2ApiManager _Gw2ApiManager;
        Autoclicker _autoClicker;


        public CharacterKeybindSettingsPresenter(CharacterKeybindWindow view, CharacterKeybindModel model
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
        }
        private void AttachViewHandler()
        {
            View.BindAddEntryButton(AddKeybindEntry);
        }

        private void AttachModelHandler()
        {
            Model.BindCharacterDataChanged(OnKeymapsChanged);
            Model.BindKeymapChanged(OnKeymapsChanged);
        }

        void OnKeymapsChanged() 
        {
            View.ClearKeybindEntries();

            foreach (var keymap in Model.GetKeymaps()) 
            {
                var container = View.AddKeybind();
                View.SetKeybindOptions(container, Model.GetCharacterNames(),Model.GetProfessionSpecializations(keymap.characterName), CharacterKeybindJsonUtil.GetKeybindFiles(Model.GetKeybindsFolder()));
                View.SetKeybindValues(container, keymap);
                View.AttachListeners(container,OnApplyKeymap, OnKeymapChange, OnKeymapRemoved);
            }
        }
        public void OnApplyKeymap(object sender, Keymap keymap)
        {
			_ = ChangeKeybinds(keymap.keymapName, Model.GetKeybindsFolder());
        }

        public void OnKeymapChange(object sender, KeymapEventArgs keymapArgs)
        {
            Model.UpdateKeymap(keymapArgs.oldKeymap, keymapArgs.newKeymap);
        }

        public void OnKeymapRemoved(object sender, Keymap keymap)
        {
            Model.RemoveKeymap(keymap);
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

                //get current character object
                var currentSpecialization = await _Gw2ApiManager.Gw2ApiClient.V2.Specializations.GetAsync(specialization);
                var keymap = Model.GetKeymapName(newCharacterName, currentSpecialization);

                if (keymap != null) 
                {
                    await ChangeKeybinds(keymap.keymapName, Model.GetKeybindsFolder());
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
            string sourceFile = Path.Combine(keybindsFolder, "Cache", $"{sourceFileName}.xml");
            string destFile = Path.Combine(keybindsFolder, "CharacterKeybinds.xml");

            try
            {
                if (!System.IO.File.Exists(Path.Combine(keybindsFolder, $"{sourceFileName}.xml"))) return;
                CharacterKeybindJsonUtil.MoveAllXmlFiles(keybindsFolder, Path.Combine(keybindsFolder, "Cache"));
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
                CharacterKeybindJsonUtil.MoveAllXmlFiles(Path.Combine(keybindsFolder, "Cache"), keybindsFolder);
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

                Profession profesion = professions.First(p => p.Name == specialization.Profession);
                Model.AddProfessionEliteSpecialization(profesion, specialization);
            }
        }
    }
}

