using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Blish_HUD.Input;
using flakysalt.CharacterKeybinds.Util;
using flakysalt.CharacterKeybinds.Data;
using flakysalt.CharacterKeybinds.Views.UiElements;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace flakysalt.CharacterKeybinds.Views
{
	public class CharacterKeybindWindow : View
    {
        private readonly Logger Logger;

        Gw2ApiManager Gw2ApiManager;
        CharacterKeybindsSettings model;
        DirectoriesManager directoriesManager;
        TroubleshootWindow troubleshootWindow;

        public StandardWindow WindowView;
        private StandardButton addEntryButton;
        private FlowPanel scrollView, mainFlowPanel;
        private Label blockerOverlay;

        Dictionary<string, List<Specialization>> professionSpezialisations = new Dictionary<string, List<Specialization>>();
        List<KeybindFlowContainer> keybindUIData = new List<KeybindFlowContainer>();

        IEnumerable<Profession> professionsResponse = new List<Profession>();
        IEnumerable<Character> characterResponse = new List<Character>();


        private double _updateCharactersRunningTime;
        private static bool hasPlayerData;
        private double updateTime = hasPlayerData ? 100_000 : 5_000;

        private static object taskLock = new object();
        private static bool isTaskStarted = false;

        public CharacterKeybindWindow(Logger Logger) 
        {
            this.Logger = Logger;
        }

        protected override void Unload()
		{
            hasPlayerData = false;
            base.Unload();
        }

		public async Task Init(ContentsManager ContentsManager,
            Gw2ApiManager Gw2ApiManager,
            CharacterKeybindsSettings model,
            DirectoriesManager directoriesManager,
            TroubleshootWindow autoclickView) 
		{
            this.model = model;
            this.Gw2ApiManager = Gw2ApiManager;
            this.directoriesManager = directoriesManager;
            this.troubleshootWindow = autoclickView;

            var windowBackgroundTexture = AsyncTexture2D.FromAssetId(155997);
            var _emblem = ContentsManager.GetTexture("images/logo.png");

            WindowView = new StandardWindow(
                windowBackgroundTexture,
                new Rectangle(25, 26, 600, 600),
                new Rectangle(40, 50, 580, 550))
            {
                Emblem = _emblem,
                Parent = GameService.Graphics.SpriteScreen,
                Title = "Character Keybinds",
                SavesPosition = true,
                Id = $"flakysalt_{nameof(CharacterKeybinds)}",
                CanClose = true
            };

            blockerOverlay = new Label()
            {
                Parent = WindowView,
                ZIndex = 4,
                HorizontalAlignment  = HorizontalAlignment.Center,
                Size = WindowView.Size,
                Visible = false,
                Text = "",
                BackgroundColor = Microsoft.Xna.Framework.Color.Black
            };

            mainFlowPanel = new FlowPanel()
            {
                Size = WindowView.ContentRegion.Size,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(5, 2),
                OuterControlPadding = new Vector2(0, 15),
                Parent = WindowView
            };

            addEntryButton = new StandardButton()
            {
                Text = "Add Binding (Loading Characters...)",
                Parent = mainFlowPanel,
                Width = mainFlowPanel.Width - 20,
                Enabled = false
            };

            var ScrollViewPanel = new FlowPanel
            {
                Size = mainFlowPanel.Size,
                FlowDirection = ControlFlowDirection.LeftToRight,
                Parent = mainFlowPanel
            };

            scrollView = new FlowPanel
            {
                CanScroll = true,
                ShowBorder = true,
                Size = new Point(ScrollViewPanel.Size.X- 20, ScrollViewPanel.Height),
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = ScrollViewPanel
            };

            var scrollbar = new Scrollbar(scrollView)
            {
                Height = ScrollViewPanel.Height
            };


            var bottomButtons = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.LeftToRight,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = mainFlowPanel
            };

/*          This is for debugging only
            var openClickerOptions = new StandardButton()
            {
                Text = "Open Clicker options",
                Parent = bottomButtons,
            };
            openClickerOptions.Click += OpenClickerOptions_Click; */


            //LoadMappingFromDisk();
            LoadMappingFromSettings();

            await LoadResources();

            addEntryButton.Click += OnAddKeybindClick;
			WindowView.Hidden += AssignmentView_Hidden;
            GameService.Gw2Mumble.PlayerCharacter.NameChanged += PlayerCharacter_NameChanged;
            GameService.Gw2Mumble.PlayerCharacter.SpecializationChanged += PlayerCharacter_SpecializationChanged;

            GameService.Gw2Mumble.CurrentMap.MapChanged += CurrentMap_MapChanged;
        }

		private void CurrentMap_MapChanged(object sender, ValueEventArgs<int> e)
		{

            //if(Blish_HUD.Contexts.FestivalContext.)
            //935 SAB
            var test = GameService.Gw2Mumble.CurrentMap.Id;

        }

		private void PlayerCharacter_SpecializationChanged(object sender, ValueEventArgs<int> newSpezialisation)
		{
            lock (taskLock) 
            {
                if (model.changeKeybindsWhenSwitchingSpecialization.Value && !isTaskStarted)
                {
                    isTaskStarted = true;
                    Task.Run(() => SetupKeybinds(GameService.Gw2Mumble.PlayerCharacter.Name, newSpezialisation.Value));
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

        public async Task SetupKeybinds(string newCharacterName = "", int spezialisation = -1)
        {
            try
            {
                if (string.IsNullOrEmpty(newCharacterName)) return;

                //get current character object
                var currentSpezialisation = await Gw2ApiManager.Gw2ApiClient.V2.Specializations.GetAsync(spezialisation);

                //check for specific name/profess
                KeybindFlowContainer selectedCharacterData = null;
                foreach (var keybindData in keybindUIData)
                {
                    if (keybindData.characterNameDropdown.SelectedItem == newCharacterName)
                    {
                        //special case for core builds
                        if (!currentSpezialisation.Elite && keybindData.specializationDropdown.SelectedItem == "Core")
                        {
                            selectedCharacterData = keybindData;
                        }

                        if (keybindData.specializationDropdown.SelectedItem == currentSpezialisation.Name)
                        {
                            selectedCharacterData = keybindData;
                        }
                    }
                }

                //if none matched, we check for a wildcard instead
                if (selectedCharacterData == null)
                {
                    foreach (var keybindData in keybindUIData)
                    {
                        if (keybindData.characterNameDropdown.SelectedItem == newCharacterName)
                        {
                            //special case for core builds
                            if (keybindData.specializationDropdown.SelectedItem == "All Spezialisations")
                            {
                                selectedCharacterData = keybindData;
                            }
                        }
                    }
                }
                if (selectedCharacterData == null || selectedCharacterData.keymapDropdown.SelectedItem == "None") return;

                await ChangeKeybinds(selectedCharacterData.keymapDropdown.SelectedItem);
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

        async Task ChangeKeybinds(string sourceFileName) 
        {
            try 
            {
                if (!System.IO.File.Exists(Path.Combine(model.gw2KeybindsFolder.Value, $"{sourceFileName}.xml"))) return;

                string sourceFile = Path.Combine(model.gw2KeybindsFolder.Value, "Cache", $"{sourceFileName}.xml");
                string destFile = Path.Combine(model.gw2KeybindsFolder.Value, "00000000.xml");

                MoveAllXmlFiles(model.gw2KeybindsFolder.Value, Path.Combine(model.gw2KeybindsFolder.Value, "Cache"));
                System.IO.File.Copy(sourceFile, destFile);
                await troubleshootWindow.ClickInOrder();
                System.IO.File.Delete(destFile);
                MoveAllXmlFiles(Path.Combine(model.gw2KeybindsFolder.Value, "Cache"), model.gw2KeybindsFolder.Value);
            }
            catch (Exception e)
            {
                Logger.Error($"Error copying files\n{e}");
            }

        }


        void MoveAllXmlFiles(string sourcePath,string destinationPath) 
        {
            string[] fileEntries = Directory.GetFiles(sourcePath, "*.xml");

            if (!Directory.Exists(destinationPath)) 
            {
                Directory.CreateDirectory(destinationPath);
            }

            foreach (string filePath in fileEntries)
            {
                string fileName = Path.GetFileName(filePath);
                string destPath = Path.Combine(destinationPath, fileName);
				System.IO.File.Move(filePath, destPath);
            }
        }

        private void OpenClickerOptions_Click(object sender, MouseEventArgs e)
		{
            troubleshootWindow.WindowView.Show();
        }

		public void Update(GameTime gameTime) 
        {
            var test = GameService.Gw2Mumble.PlayerCharacter.Name;
            _updateCharactersRunningTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_updateCharactersRunningTime > updateTime) 
            {
                _updateCharactersRunningTime = 0;
                Task.Run(LoadCharacters);
            }
        }

		private void AssignmentView_Hidden(object sender, EventArgs e)
		{
            List<CharacterKeybind> characterSpecializations = new List<CharacterKeybind>();

            foreach (var keybindData in keybindUIData) 
            {
                CharacterKeybind keybind = new CharacterKeybind
                {
                    characterName = keybindData.characterNameDropdown.SelectedItem,
                    spezialisation = keybindData.specializationDropdown.SelectedItem,
                    keymap = keybindData.keymapDropdown.SelectedItem
                };
                characterSpecializations.Add(keybind);
            }
            model.characterKeybinds.Value = characterSpecializations;
        }

        private async Task LoadResources()
		{
            IEnumerable<Specialization> specializations = new List<Specialization>();
            try
            {
                professionsResponse = await Gw2ApiManager.Gw2ApiClient.V2.Professions.AllAsync();
                specializations = await Gw2ApiManager.Gw2ApiClient.V2.Specializations.AllAsync();

            }
            catch (Exception e)
            {
                Logger.Info($"Failed to get spezializations from api.\n Exception {e}");
            }

            foreach (var specialization in specializations) 
            {
                if (!specialization.Elite) continue;
                if (professionSpezialisations.ContainsKey(specialization.Profession))
                {
                    professionSpezialisations[specialization.Profession].Add(specialization);
                }
                else 
                {
                    professionSpezialisations[specialization.Profession] = new List<Specialization> { specialization };
                }
            }
        }

        private async Task LoadCharacters()
        {

            var apiKeyPermissions = new List<TokenPermission>
            {
                TokenPermission.Account,
                TokenPermission.Characters
            };

            if (!Gw2ApiManager.HasPermissions(apiKeyPermissions))
            {
                blockerOverlay.Text = "API token missing or not available yet.\n\n" +
                    "Make sure you have added an API token to Blish HUD \nand it has the neccessary permissions!\n"+
                    "(Previously setup keybinds will still work!)";
                blockerOverlay.Visible = true;

                return;
            }
            try
            {
                characterResponse = await Gw2ApiManager.Gw2ApiClient.V2.Characters.AllAsync();
                addEntryButton.Enabled = true;
                blockerOverlay.Visible = false;
                addEntryButton.Text = "Add Binding";

                foreach (var binding in keybindUIData) 
                {
                    UpdateKeybind(binding);
                }
            }
            catch (Exception e)
            {
                Logger.Info($"Failed to get character names from api.\n {e}");
            }
        }

        void LoadMappingFromSettings()
        {
            foreach (var binding in model.characterKeybinds.Value)
            {
                AddKeybind(binding.characterName, binding.spezialisation, binding.keymap);
            }
        }

        private void OnAddKeybindClick(object sender, MouseEventArgs e)
        {
            if (!Directory.Exists(model.gw2KeybindsFolder.Value)) return;

            var uielement = AddKeybind();
            UpdateKeybind(uielement);
        }

        private void UpdateKeybind(KeybindFlowContainer keybindFlowContainer) 
        {
            string[] xmlFiles = Directory.GetFiles(model.gw2KeybindsFolder.Value, "*.xml");

            for (int i = 0; i < xmlFiles.Length; i++)
            {
                xmlFiles[i] = Path.GetFileNameWithoutExtension(xmlFiles[i]);
            }

            keybindFlowContainer.SetKeymapOptions(xmlFiles.ToList());

            keybindFlowContainer.SetNameOptions(characterResponse.Select(character => character.Name).ToList());

            List<Character> charcterList = characterResponse as List<Character>;
            Character currentCharacter = charcterList.Find(item => keybindFlowContainer.characterNameDropdown.SelectedItem == item.Name);

            if (currentCharacter == null) return;

            foreach (var profession in professionsResponse)
            {
                if (currentCharacter.Profession == profession.Name)
                {
                    var iconAssetId = int.Parse(Path.GetFileNameWithoutExtension(profession.Icon.Url.AbsoluteUri));
                    keybindFlowContainer.professionImage.Texture = AsyncTexture2D.FromAssetId(iconAssetId);
                }
            }

            foreach (var profession in professionSpezialisations)
            {
                if (currentCharacter.Profession == profession.Key)
                {
                    List<string> specializationNames = new List<string> { "All Spezialisations", "Core" };
                    specializationNames.AddRange(profession.Value.Select(specialization => specialization.Name));

                    keybindFlowContainer.SetSpecializationOptions(specializationNames);
                }
            }
        }

        private KeybindFlowContainer AddKeybind(string selectedName = "",
            string selectedSpezialisations = "",
            string selectedKeymap = "")
		{
			var keybindFlowContainer = new KeybindFlowContainer(selectedName, selectedSpezialisations, selectedKeymap)
			{
				Parent = scrollView,
				Size = new Point(scrollView.Width, 50),
				CanScroll = false,
				FlowDirection = ControlFlowDirection.LeftToRight
			};

			keybindUIData.Add(keybindFlowContainer);

			keybindFlowContainer.characterNameDropdown.ValueChanged += (o, eventArgs) =>
            {
                List<Character> charcterList = characterResponse as List<Character>;
                Character currentCharacter = charcterList.Find(item => keybindFlowContainer.characterNameDropdown.SelectedItem == item.Name);

                if (currentCharacter == null) return;

                foreach (var profession in professionsResponse)
                {
                    if (currentCharacter.Profession == profession.Name)
                    {
                        var iconAssetId = int.Parse(Path.GetFileNameWithoutExtension(profession.Icon.Url.AbsoluteUri));
                        keybindFlowContainer.professionImage.Texture = AsyncTexture2D.FromAssetId(iconAssetId);
                    }
                }

                foreach (var profession in professionSpezialisations)
                {
                    if (currentCharacter.Profession == profession.Key)
                    {
                        //probably localize this later
                        List<string> specializationNames = new List<string> { "All Spezialisations", "Core" };
                        keybindFlowContainer.specializationDropdown.SelectedItem = "All Spezialisations";
                        specializationNames.AddRange(profession.Value.Select(specialization => specialization.Name));

                        keybindFlowContainer.SetSpecializationOptions(specializationNames);
                    }
                }
                keybindFlowContainer.keymapDropdown.Enabled = true;
                keybindFlowContainer.specializationDropdown.Enabled = true;
            };

            keybindFlowContainer.removeButton.Click += (sender, e) =>
            {
                keybindUIData.Remove(keybindFlowContainer);
            };

            keybindFlowContainer.OnApplyPressed += (sender, selectedKeybinds) =>
            {
                Task.Run(() => ChangeKeybinds(selectedKeybinds));

            };
            return keybindFlowContainer;
        }
	}
}
