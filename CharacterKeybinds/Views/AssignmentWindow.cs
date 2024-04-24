﻿using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.Managers;
using CharacterKeybinds.Data;
using flakysalt.CharacterKeybinds.Model;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Blish_HUD.Input;
using CharacterKeybinds.Util;
using CharacterKeybinds.Views;
using System.Linq;

namespace flakysalt.CharacterKeybinds.Views
{
	class AssignmentWindow : View
    {
        private static readonly Logger Logger = Logger.GetLogger<AssignmentWindow>();

        Gw2ApiManager Gw2ApiManager;
        CharacterKeybindsModel model;
        DirectoriesManager directoriesManager;
        AutoclickView autoclickView;

        public StandardWindow AssignmentView;
        private StandardButton addEntryButton;
        private FlowPanel scrollView, mainFlowPanel;
        private Label blockerOverlay;

        Dictionary<String, List<Specialization>> professionSpezialisations = new Dictionary<String, List<Specialization>>();
        List<KeybindFlowContainerData> keybindUIData = new List<KeybindFlowContainerData>();

        IEnumerable<Profession> professionsResponse = new List<Profession>();
        IEnumerable<Character> characterResponse = new List<Character>();


        private double _updateCharactersRunningTime;
        private static bool hasPlayerData;
        private double updateTime = hasPlayerData ? 100_000 : 5_000;


		protected override void Unload()
		{
            hasPlayerData = false;
            base.Unload();
        }

		public async Task Init(ContentsManager ContentsManager, Gw2ApiManager Gw2ApiManager, CharacterKeybindsModel model, DirectoriesManager directoriesManager, AutoclickView autoclickView) 
		{
            this.model = model;
            this.Gw2ApiManager = Gw2ApiManager;
            this.directoriesManager = directoriesManager;
            this.autoclickView = autoclickView;

            var windowBackgroundTexture = AsyncTexture2D.FromAssetId(155997);
            var _emblem = ContentsManager.GetTexture("images/logo.png");


            AssignmentView = new StandardWindow(
                windowBackgroundTexture,
                new Rectangle(25, 26, 560, 640),
                new Rectangle(40, 50, 540, 590))
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
                Parent = AssignmentView,
                ZIndex = 4,
                HorizontalAlignment  = HorizontalAlignment.Center,
                Size = AssignmentView.Size,
                Visible = false,
                Text = "",
                BackgroundColor = Microsoft.Xna.Framework.Color.Transparent
            };

            mainFlowPanel = new FlowPanel()
            {
                Size = AssignmentView.Size,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(5, 2),
                OuterControlPadding = new Vector2(0, 15),
                Parent = AssignmentView
            };

            addEntryButton = new StandardButton()
            {
                Text = "Add Binding (Loading Characters...)",
                Parent = mainFlowPanel,
                Width = mainFlowPanel.Width - 20,
                Enabled = false
            };

            scrollView = new FlowPanel
            {
                CanScroll = true,
                ShowBorder = true,
                Size = new Point(mainFlowPanel.Size.X, 400),
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = mainFlowPanel 
            };


            var bottomButtons = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.LeftToRight,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = mainFlowPanel
            };

/*            var openClickerOptions = new StandardButton()
            {
                Text = "Open Clicker options",
                Parent = bottomButtons,
            };*/

            LoadMappingFromDisk();

            await LoadResources();

            var test = GameService.Gw2Mumble.PlayerCharacter.Name;
            addEntryButton.Click += OnAddKeybindClick;
			AssignmentView.Hidden += AssignmentView_Hidden;
			//openClickerOptions.Click += OpenClickerOptions_Click;
            GameService.Gw2Mumble.PlayerCharacter.NameChanged += PlayerCharacter_NameChanged;
            GameService.Gw2Mumble.PlayerCharacter.SpecializationChanged += PlayerCharacter_SpecializationChanged; ;
        }

		private void PlayerCharacter_SpecializationChanged(object sender, ValueEventArgs<int> newSpezialisation)
		{
            if (!model.onlyChangeKeybindsOnCharacterChange.Value) 
            {
                Task.Run(() => SetupKeybinds(GameService.Gw2Mumble.PlayerCharacter.Name, newSpezialisation.Value));
            }
        }

        private void PlayerCharacter_NameChanged(object sender, ValueEventArgs<string> newCharacterName)
		{
            Task.Run(() => SetupKeybinds(newCharacterName.Value, GameService.Gw2Mumble.PlayerCharacter.Specialization));
        }
        public async Task SetupKeybinds(string newCharacterName = "", int spezialisation = -1)
        {
            if (string.IsNullOrEmpty(newCharacterName)) return;

            //get current character object
            var currentSpezialisation = await Gw2ApiManager.Gw2ApiClient.V2.Specializations.GetAsync(spezialisation);

            //check for specific name/profess
            KeybindFlowContainerData selectedCharacterData = null;
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

            string sourceFile =Path.Combine(model.gw2KeybindsFolder.Value, $"{selectedCharacterData.keymapDropdown.SelectedItem}.xml");         
            string destFile = Path.Combine(model.gw2KeybindsFolder.Value, "00000000.xml");

            System.IO.File.Copy(sourceFile, destFile);
            await autoclickView.ClickInOrder();
            System.IO.File.Delete(destFile);
        }


        private void OpenClickerOptions_Click(object sender, MouseEventArgs e)
		{
            autoclickView.AutoClickWindow.Show();

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
            //if (!hasPlayerData) return;

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

            if (!System.IO.File.Exists(Path.Combine(directoriesManager.GetFullDirectoryPath("keybind_storage"), "characterMap.json")))
            {
                System.IO.File.Create(Path.Combine(directoriesManager.GetFullDirectoryPath("keybind_storage"), "characterMap.json"));
            }
            var characterKeybindJson = CharacterKeybindJsonUtil.SerializeCharacterList(characterSpecializations);
            System.IO.File.WriteAllText(Path.Combine(directoriesManager.GetFullDirectoryPath("keybind_storage"), "characterMap.json"), characterKeybindJson);
        }


        private async Task LoadResources()
		{
            IEnumerable<Specialization> testResponse = new List<Specialization>();
            try
            {
                professionsResponse = await Gw2ApiManager.Gw2ApiClient.V2.Professions.AllAsync();
                testResponse = await Gw2ApiManager.Gw2ApiClient.V2.Specializations.AllAsync();

            }
            catch (Exception e)
            {
                Logger.Info($"Failed to get spezializations from api.\n Exception {e}");
            }

            foreach (var test in testResponse) 
            {
                if (!test.Elite) continue;
                if (professionSpezialisations.ContainsKey(test.Profession))
                {
                    professionSpezialisations[test.Profession].Add(test);
                }
                else 
                {
                    professionSpezialisations[test.Profession] = new List<Specialization> { test };
                }
            }
        }

        private async Task LoadCharacters()
        {

            var apiKeyPermissions = new List<TokenPermission>
            {
                TokenPermission.Account, // this permission can be used to check if your module got a token at all because every api key has this persmission.
                TokenPermission.Characters // this is the permission we actually require here to get the character names
            };

            if (!Gw2ApiManager.HasPermissions(apiKeyPermissions))
            {
                blockerOverlay.Text = "API token missing or not available yet.\n" +
                    "Make sure you have added an API token to Blish HUD and it has the neccessary permissions\n\n"+
                    "(All already setup keybinds will still work!)";
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
                Logger.Info("Failed to get character names from api.");
            }
        }

        void LoadMappingFromDisk()
        {
            if (!System.IO.File.Exists(Path.Combine(directoriesManager.GetFullDirectoryPath("keybind_storage"), "characterMap.json")))
            {
                System.IO.File.Create(Path.Combine(directoriesManager.GetFullDirectoryPath("keybind_storage"), "characterMap.json"));
            }

            string loadJson = System.IO.File.ReadAllText(Path.Combine(directoriesManager.GetFullDirectoryPath("keybind_storage"), "characterMap.json"));
            var characterSpecializations = CharacterKeybindJsonUtil.DeserializeCharacterList(loadJson);
            
            if (characterSpecializations == null) return;
            foreach (var binding in characterSpecializations) 
            {
                AddKeybind(binding.characterName, binding.spezialisation, binding.keymap);
            }
        }

        private void OnAddKeybindClick(object sender, MouseEventArgs e)
        {
            var uielement = AddKeybind();
            UpdateKeybind(uielement);
        }

        private void UpdateKeybind(KeybindFlowContainerData keybindFlowContainer) 
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

        private KeybindFlowContainerData AddKeybind(string selectedName = "",
            string selectedSpezialisations = "",
            string selectedKeymap = "")
		{
			var keybindFlowContainer = new KeybindFlowContainerData(selectedName, selectedSpezialisations, selectedKeymap)
			{
				Parent = scrollView,
				Size = new Point(scrollView.Width, 80),
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
            return keybindFlowContainer;
        }
	}
}
