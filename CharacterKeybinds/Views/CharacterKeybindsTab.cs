using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Blish_HUD.Content;
using flakysalt.CharacterKeybinds.Resources;
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
        private Label defaultKeybindsLabel;
        private Image errorInfoIcon;
        
        public EventHandler<string> OnApplyDefaultKeymapClicked;
        public EventHandler<string> OnDefaultKeymapChanged;
        public EventHandler OnAddButtonClicked;

        protected override void Build(Container buildPanel)
        {
            
            mainFlowPanel = new FlowPanel
            {
                ControlPadding = new Vector2(0, 10),
                HeightSizingMode = SizingMode.Fill,
                Size = buildPanel.ContentRegion.Size,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = buildPanel
            };
            
            // Create spinner inside this tab
            _spinner = new LoadingSpinner()
            {
                Parent = buildPanel,
                BasicTooltipText = Loca.apiLoadingHint,
                Location = new Point(buildPanel.ContentRegion.Size.X / 2 - 32, buildPanel.ContentRegion.Size.Y / 2 - 32),
                Size = new Point(64, 64),
                ZIndex = 100,
                Visible = false
            };
            
            defaultKeybindsLabel = new Label()
            {
                Parent = mainFlowPanel,
                Width = mainFlowPanel.Width,
                BasicTooltipText = Loca.defaultKeybindHint,
                Text = Loca.defaultKeybind,
                Font = GameService.Content.DefaultFont18
            };
            
            var texture = AsyncTexture2D.FromAssetId(155018);
            errorInfoIcon = new Image(texture)
            {
                Parent = buildPanel,
                Size = new Point(48, 48),
                Visible = false,
                Location = new Point(mainFlowPanel.Right - 64, mainFlowPanel.Top + 16)
            };
            
            var defaultKeybindFlowPanel = new FlowPanel()
            {
                Height = 30,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                Parent = mainFlowPanel,
                WidthSizingMode = SizingMode.Fill
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
                Text = Loca.apply,
                Parent = defaultKeybindFlowPanel
            };
            
            new Label()
            {
                Parent = mainFlowPanel,
                Width = mainFlowPanel.Width,
                BasicTooltipText = Loca.characterSpecificKeybindsHint,
                Text = Loca.characterSpecificKeybinds,
                Font = GameService.Content.DefaultFont18
            };
            
            scrollView = new FlowPanel
            {
                CanScroll = true,
                ShowBorder = true,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = mainFlowPanel,
                HeightSizingMode = SizingMode.Fill,
                WidthSizingMode = SizingMode.Fill
            };
            
            keybindScrollView = new FlowPanel
            {
                OuterControlPadding = new Vector2(0, 10),
                ControlPadding = new Vector2(0,5),
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = scrollView,
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill
            };
            
            new Scrollbar(scrollView)
            {
                Height = scrollView.Height
            };
            
            addEntryButton = new StandardButton()
            {
                Text = Loca.addNewBindingButtonText,
                Parent = scrollView,
                Width = buildPanel.ContentRegion.Size.X
            };

            addEntryButton.Click += (sender, args) => OnAddButtonClicked?.Invoke(sender, args);
            applyDefaultKeybindButton.Click += (sender, args) => OnApplyDefaultKeymapClicked?.Invoke(sender, defaultKeybindDropdown.SelectedItem);
            defaultKeybindDropdown.ValueChanged += (sender, args) => OnDefaultKeymapChanged?.Invoke(sender, args.CurrentValue);
            buildPanel.Resized += (sender, args) =>
            {
                mainFlowPanel.Size = buildPanel.ContentRegion.Size;
                addEntryButton.Width = buildPanel.ContentRegion.Size.X;
                _spinner.Location = new Point(buildPanel.ContentRegion.Size.X / 2 - 32, buildPanel.ContentRegion.Size.Y  / 2 - 32);
                errorInfoIcon.Location = new Point(mainFlowPanel.Right - 64, mainFlowPanel.Top + 16);
            };
            base.Build(buildPanel);
        }

        public void SetSpinner(bool state)
        {
            _spinner.Visible = state;
        }
        
        public void SetErrorInfoIcon(bool isValid, bool isDataLoaded,string error)
        {
            errorInfoIcon.BasicTooltipText = error;
            errorInfoIcon.Visible = !isValid;
            SetKeybindContainerEnabled(isValid && isDataLoaded);
        }

        private void SetKeybindContainerEnabled(bool enabled)
        {
            var flowContainers = keybindScrollView.GetChildrenOfType<KeybindFlowContainer>();
            foreach (var container in flowContainers)
            {
                container.SetEnabled(enabled);
            }
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
                Parent = keybindScrollView
            };

            return keybindFlowContainer;
        }

        public void SetKeybindOptions(KeybindFlowContainer keybindFlowContainer,
            List<string> characterList,
            List<LocalizedSpecialization> specializations,
            List<string> keymaps)
        {
            keybindFlowContainer.SetDropdownContent(keybindFlowContainer.CharacterNameDropdown, characterList);
            keybindFlowContainer.SetSpecializationContent(specializations);
            keybindFlowContainer.SetDropdownContent(keybindFlowContainer.KeymapDropdown, keymaps);
        }

        public void SetKeybindValues(KeybindFlowContainer keybindFlowContainer, Keymap characterKeybind, int iconId)
        {
            keybindFlowContainer.SetValues(characterKeybind);
            keybindFlowContainer.SetProfessionIcon(iconId);
        }
        
        public void AttachListeners(KeybindFlowContainer keybindFlowContainer,
            EventHandler<Keymap> onApplyAction,
            EventHandler<KeymapEventArgs> onDataChanged,
            EventHandler<Keymap> onDeleteAction) 
        {
            keybindFlowContainer.AttachListeners(onApplyAction, onDataChanged, onDeleteAction);
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

