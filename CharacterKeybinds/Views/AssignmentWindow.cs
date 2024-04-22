using Blish_HUD;
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
        private TextBox blockerOverlay;

        Dictionary<String, List<Specialization>> professionSpezialisations = new Dictionary<String, List<Specialization>>();
        List<KeybindFlowContainerData> keybindUIData = new List<KeybindFlowContainerData>();

        IEnumerable<Profession> professionsResponse = new List<Profession>();
        IEnumerable<Character> characterResponse = new List<Character>();


        private double _updateCharactersRunningTime;
        private static bool hasPlayerData;
        private double updateTime = hasPlayerData ? 100_000 : 5_000;


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

            blockerOverlay = new TextBox()
            {
                Parent = AssignmentView,
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

            var ExportButton = new StandardButton()
            {
                Text = "Export",
                Parent = bottomButtons,
            };

            var ExportAndBindButton = new StandardButton()
            {
                Text = "Export and Bind",
                Parent = bottomButtons,
            };
            var openClickerOptions = new StandardButton()
            {
                Text = "Open Clicker options",
                Parent = bottomButtons,
            };

            await LoadResources();

            addEntryButton.Click += OnAddKeybindClick;
			AssignmentView.Hidden += AssignmentView_Hidden;
			openClickerOptions.Click += OpenClickerOptions_Click;
            GameService.Gw2Mumble.PlayerCharacter.NameChanged += PlayerCharacter_NameChanged;
            GameService.Gw2Mumble.PlayerCharacter.SpecializationChanged += PlayerCharacter_SpecializationChanged; ;
        }

		private void PlayerCharacter_SpecializationChanged(object sender, ValueEventArgs<int> newSpezialisation)
		{
            if (model.switchKeybindOnSpecializationsSwitch.Value) 
            {
                Task.Run(() => SetupKeybinds(GameService.Gw2Mumble.PlayerCharacter.Name, newSpezialisation.Value));
            }
        }

        private void PlayerCharacter_NameChanged(object sender, ValueEventArgs<string> newCharacterName)
		{
            Task.Run(() => SetupKeybinds(newCharacterName.Value, GameService.Gw2Mumble.PlayerCharacter.Specialization));
        }
        public async Task SetupKeybinds(string newCharacterName = "", int spezialisation = 0)
        {
            //get current character object
            Character newCharacter = null;
            var currentSpezialisation = await Gw2ApiManager.Gw2ApiClient.V2.Specializations.GetAsync(spezialisation);

            foreach (var character in characterResponse)
            {
                if (character.Name == newCharacterName)
                {
                    newCharacter = character;
                }
            }
            if (newCharacter == null) return;

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

            string sourceFile =Path.Combine(directoriesManager.GetFullDirectoryPath("keybind_storage"),$"{selectedCharacterData.keymapDropdown.SelectedItem}.xml");         
            string destFile =Path.Combine(model.gw2KeybindsFolder.Value, "00000000.xml");

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
            _updateCharactersRunningTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_updateCharactersRunningTime > updateTime) 
            {
                _updateCharactersRunningTime = 0;
                Task.Run(LoadCharacters);
            }
        }

		private void AssignmentView_Hidden(object sender, EventArgs e)
		{
            if (!hasPlayerData) return;

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
                blockerOverlay.Text = "api permissions are missing or api sub token not available yet";
                blockerOverlay.Visible = true;

                return;
            }
            try
            {
                characterResponse = await Gw2ApiManager.Gw2ApiClient.V2.Characters.AllAsync();
                addEntryButton.Enabled = true;
                addEntryButton.Text = "Add Binding";
                LoadMappingFromDisk();
            }
            catch (Exception e)
            {
                Logger.Info("Failed to get character names from api.");
            }
        }

        void LoadMappingFromDisk()
        {
            if (hasPlayerData) return;
            hasPlayerData = true;


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
            AddKeybind();
        }

        private void AddKeybind(string selectedCharacter = "",
            string selectedSpezialisations = "",
            string selectedKeymap = "")
		{
            var flowPanel = new FlowPanel
            {
                Parent = scrollView,
                Size = new Point(scrollView.Width, 40),
                CanScroll = false,
                FlowDirection = ControlFlowDirection.LeftToRight
            };
            var characterImage = new Image
            {
                Parent = flowPanel,
                Size = new Point(30, 30)
            };

            var characterDropdown = new Dropdown
            {
                Parent = flowPanel,
                Size = new Point(130, 30)
            };
            var spezialisationDropdown = new Dropdown
            {
                Parent = flowPanel,
                Size = new Point(130, 30)
            };

            var keymapDropdown = new Dropdown
            {
                Parent = flowPanel,
                Size = new Point(130, 30)
            };

            KeybindFlowContainerData data = new KeybindFlowContainerData();
            data.characterNameDropdown = characterDropdown;
            data.specializationDropdown = spezialisationDropdown;
            data.keymapDropdown = keymapDropdown;
            keybindUIData.Add(data);

            string[] xmlFiles = Directory.GetFiles(directoriesManager.GetFullDirectoryPath("keybind_storage"), "*.xml");

            foreach (string file in xmlFiles)
            {
                keymapDropdown.Items.Add(Path.GetFileNameWithoutExtension(file));
            }

            spezialisationDropdown.Items.Add("...");

            characterDropdown.Items.Add("Select Character...");
            foreach (var character in characterResponse)
            {
                characterDropdown.Items.Add(character.Name);
            }
            characterDropdown.SelectedItem = string.IsNullOrEmpty(selectedCharacter) ? "Select Character..." : selectedCharacter;
            characterDropdown.ValueChanged += (o, eventArgs) => 
            {
                spezialisationDropdown.Items.Clear();
                List<Character> charcterList = characterResponse as List<Character>;
                Character currentCharacter = charcterList.Find(item => characterDropdown.SelectedItem == item.Name);

                if (currentCharacter == null) 
                {
                    spezialisationDropdown.Enabled = false;
                    return;
                }

                foreach (var profession in professionsResponse) 
                {
                    if (currentCharacter.Profession == profession.Name) 
                    {
                        var iconAssetId = int.Parse(Path.GetFileNameWithoutExtension(profession.Icon.Url.AbsoluteUri));
                        characterImage.Texture = AsyncTexture2D.FromAssetId(iconAssetId);
                    }
                }

                spezialisationDropdown.Enabled = true;
                spezialisationDropdown.Items.Add("All Spezialisations");
                spezialisationDropdown.Items.Add("Core");
                spezialisationDropdown.SelectedItem = "All Spezialisations";


                foreach (var profession in professionSpezialisations) 
                {
                    if (currentCharacter.Profession == profession.Key) 
                    {
                        foreach (var eliteSpec in profession.Value) 
                        {
                            spezialisationDropdown.Items.Add(eliteSpec.Name);
                        }
                    }
                }
            };

            //Code Duplication from above, there is probably a better way
            if (!string.IsNullOrEmpty(selectedCharacter))
            {
                spezialisationDropdown.Items.Clear();
                List<Character> charcterList = characterResponse as List<Character>;
                Character currentCharacter = charcterList.Find(item => characterDropdown.SelectedItem == item.Name);

                if (currentCharacter == null)
                {
                    spezialisationDropdown.Enabled = false;
                    return;
                }

                foreach (var profession in professionsResponse)
                {
                    if (currentCharacter.Profession == profession.Name)
                    {
                        var iconAssetId = int.Parse(Path.GetFileNameWithoutExtension(profession.Icon.Url.AbsoluteUri));
                        characterImage.Texture = AsyncTexture2D.FromAssetId(iconAssetId);
                    }
                }

                spezialisationDropdown.Enabled = true;
                spezialisationDropdown.Items.Add("All Spezialisations");
                spezialisationDropdown.Items.Add("Core");

                foreach (var profession in professionSpezialisations)
                {
                    if (currentCharacter.Profession == profession.Key)
                    {
                        foreach (var eliteSpec in profession.Value)
                        {
                            spezialisationDropdown.Items.Add(eliteSpec.Name);
                        }
                    }
                }
            };

            spezialisationDropdown.SelectedItem = string.IsNullOrEmpty(selectedSpezialisations) ? "All Spezialisations" : selectedSpezialisations;
            keymapDropdown.SelectedItem = string.IsNullOrEmpty(selectedKeymap) ? "None" : selectedKeymap;
            var removeButton = new StandardButton
            {
                Parent = flowPanel,
                Text = "Remove",
                Size = new Point(70, 30),
            };

            removeButton.Click += (o, eventArgs) => {
                keybindUIData.Remove(data);
                flowPanel.Dispose();
            };
            scrollView.RecalculateLayout();
        }
	}
}
