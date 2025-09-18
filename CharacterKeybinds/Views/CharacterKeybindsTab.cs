using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using flakysalt.CharacterKeybinds.Views.UiElements;
using flakysalt.CharacterKeybinds.Data;

namespace flakysalt.CharacterKeybinds.Views
{
	public class CharacterKeybindsTab : View
    {
        private StandardButton addEntryButton, applyDefaultKeybindButton;
        private FlowPanel scrollView, mainFlowPanel, keybindScrollView;
        private Dropdown defaultKeybindDropdown;
        private LoadingSpinner _spinner;
        private Label _blockerOverlay;
        
        public EventHandler<string> OnApplyDefaultKeymapClicked;
        public EventHandler<string> OnDefaultKeymapChanged;
        public EventHandler OnAddButtonClicked;

        protected override void Build(Container buildPanel)
        {
            mainFlowPanel = new FlowPanel
            {
                ControlPadding = new Vector2(0, 10),
                HeightSizingMode = SizingMode.Fill,
                Width = buildPanel.Width,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = buildPanel
            };
            
            // Create spinner inside this tab
            _spinner = new LoadingSpinner()
            {
                Parent = mainFlowPanel,
                Location = new Point(mainFlowPanel.Width / 2 - 32, mainFlowPanel.Height / 2 - 32),
                Size = new Point(64, 64),
                ZIndex = 100,
                Visible = false
            };
            
            // Create blocker overlay specific to this tab
            _blockerOverlay = new Label()
            {
                Parent = mainFlowPanel,
                ZIndex = 90,
                HorizontalAlignment = HorizontalAlignment.Center,
                Size = mainFlowPanel.Size,
                Visible = false,
                Text = "",
                BackgroundColor = Color.Black
            };
            
            new Label()
            {
                Parent = mainFlowPanel,
                Width = mainFlowPanel.Width,
                BasicTooltipText = "Applies these keybindings in case there are no specific ones setup for a character.",
                Text = "Default Keybinds",
                Font = GameService.Content.DefaultFont18
            };
            
            var defaultKeybindFlowPanel = new FlowPanel()
            {
                Height = 30,
                Width = mainFlowPanel.Width,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                Parent = mainFlowPanel
            };
            
            defaultKeybindDropdown = new Dropdown
            {
                Parent = defaultKeybindFlowPanel,
                Height = 30,
            };
            
            applyDefaultKeybindButton = new StandardButton()
            {
                Width = 60,
                Height = 30,
                Text = "Apply",
                Parent = defaultKeybindFlowPanel
            };
            
            new Label()
            {
                Parent = mainFlowPanel,
                Width = mainFlowPanel.Width,
                BasicTooltipText = "Keybinding to use for a specific character or specializations",
                Text = "Character Specific Keybinds",
                Font = GameService.Content.DefaultFont18
            };
            
            scrollView = new FlowPanel
            {
                CanScroll = true,
                ShowBorder = true,
                Width = mainFlowPanel.Width,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = mainFlowPanel,
                HeightSizingMode = SizingMode.Fill,
            };
            
            keybindScrollView = new FlowPanel
            {
                OuterControlPadding = new Vector2(0, 10),
                Width = scrollView.Width,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = scrollView,
                HeightSizingMode = SizingMode.AutoSize
            };
            
            new Scrollbar(scrollView)
            {
                Height = scrollView.Height
            };
            
            addEntryButton = new StandardButton()
            {
                Text = "+ Add Binding",
                Parent = scrollView,
                Width = scrollView.Width
            };

            addEntryButton.Click += (sender, args) => OnAddButtonClicked?.Invoke(sender, args);
            applyDefaultKeybindButton.Click += (sender, args) => OnApplyDefaultKeymapClicked?.Invoke(sender, defaultKeybindDropdown.SelectedItem);
            defaultKeybindDropdown.ValueChanged += (sender, args) => OnDefaultKeymapChanged?.Invoke(sender, args.CurrentValue);
            base.Build(buildPanel);
        }

		public void SetAddButtonState(bool state)
        {
            addEntryButton.Enabled = state;
            addEntryButton.Text = state ? "+ Add Binding" : "Add Binding (Loading Characters...)";
        }

        public void SetSpinner(bool state)
        {
            _spinner.Visible = state;
        }
        
        public void SetBlocker(bool visibility)
        {
            _blockerOverlay.Text = "API token missing or not available yet.\n\n" +
                "Make sure you have added an API token to Blish HUD \nand it has the neccessary permissions!\n" +
                "(Previously setup keybinds will still work!)";

            _blockerOverlay.Visible = visibility;
        }

        public void SetDefaultKeybindOptions(List<string>options, string selectedOption)
        {
	        defaultKeybindDropdown.Items.Clear();
	        options.ForEach(e => defaultKeybindDropdown.Items.Add(e));
	        defaultKeybindDropdown.SelectedItem = selectedOption;
        }

        public KeybindFlowContainer AddKeybind()
        {
            var keybindFlowContainer = new KeybindFlowContainer()
            {
                Parent = keybindScrollView,
                Width = keybindScrollView.Width,
                CanScroll = false,
                FlowDirection = ControlFlowDirection.LeftToRight,
            };

            return keybindFlowContainer;
        }

        public void SetKeybindOptions(KeybindFlowContainer keybindFlowContainer,
            List<string> charaters,
            List<LocalizedSpecialization> specializations,
            List<string> keymaps)
        {
            keybindFlowContainer.SetDropdownContent(keybindFlowContainer.CharacterNameDropdown, charaters);
            keybindFlowContainer.SetSpecializationContent(specializations);
            keybindFlowContainer.SetDropdownContent(keybindFlowContainer.KeymapDropdown, keymaps);
        }

        public void SetKeybindValues(KeybindFlowContainer keybindFlowContainer, Keymap characterKeybind, int iconId)
        {
            keybindFlowContainer.SetValues(characterKeybind);
            keybindFlowContainer.SetProfessionIcon(iconId);

        }
        
        public void AttachListeners(KeybindFlowContainer keybindFlowContainer,
            EventHandler<Keymap> OnApplyAction,
            EventHandler<KeymapEventArgs> OnDataChanged,
            EventHandler<Keymap> OnDeleteAction) 
        {
            keybindFlowContainer.AttachListeners(OnApplyAction, OnDataChanged, OnDeleteAction);
        }

        public void ClearKeybindEntries()
        {
            for (int i = keybindScrollView.Children.Count - 1; i >= 0; i--)
            {
	            ((KeybindFlowContainer)keybindScrollView.Children[i]).DisposeEvents();
	            keybindScrollView.RemoveChild(keybindScrollView.Children[i]);
            }
        }
    }
}

