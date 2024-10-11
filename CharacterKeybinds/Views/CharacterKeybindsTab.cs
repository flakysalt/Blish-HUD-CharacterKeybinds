using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.Managers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using flakysalt.CharacterKeybinds.Presenter;
using flakysalt.CharacterKeybinds.Views.UiElements;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using flakysalt.CharacterKeybinds.Data;

namespace flakysalt.CharacterKeybinds.Views
{
	public class CharacterKeybindsTab : View<CharacterKeybindSettingsPresenter>, IDisposable
    {
	    private TabbedWindow2 WindowView;
        private StandardButton addEntryButton,applyDefaultKeybindButton;
        private FlowPanel scrollView, mainFlowPanel, keybindScrollView;
        private Label blockerOverlay;
        private Dropdown defaultKeybindDropdown;
        
        public EventHandler<string> OnApplyDefaultKeymapClicked;
        public EventHandler<string> OnDefaultKeymapChanged;
        public EventHandler OnAddButtonClicked;
        
        public void Dispose()
		{
            base.Unload();
        }

		IView SomeViewMethod() 
		{
			Logger.GetLogger<CharacterKeybindsTab>().Debug("test");
			return this;
		}

		public CharacterKeybindsTab(ContentsManager ContentsManager)
		{
			WindowView = new TabbedWindow2(
				AsyncTexture2D.FromAssetId(155997),
				new Rectangle(24, 30, 545, 600),
				new Rectangle(82, 30, 467, 600))
			{
				Emblem = ContentsManager.GetTexture("images/logo.png"),
				Parent = GameService.Graphics.SpriteScreen,
				Title = "Character Keybinds",
				Size = new Point(645, 700),
				SavesPosition = true,
				Id = $"flakysalt_{nameof(CharacterKeybinds)}",
				CanClose = true
			};


			//TODO Remove the tabs
			var tab1 = new Tab(ContentsManager.GetTexture("images/logo_small.png"), SomeViewMethod, "test name");
			WindowView.Tabs.Add(tab1);

			WindowView.TabChanged += WindowView_TabChanged;


			blockerOverlay = new Label()
			{
				Parent = WindowView,
				ZIndex = 4,
				HorizontalAlignment = HorizontalAlignment.Center,
				Size = WindowView.Size,
				Visible = false,
				Text = "",
				BackgroundColor = Color.Black
			};

			mainFlowPanel = new FlowPanel()
			{
				ControlPadding = new Vector2(0,10),
				Size = WindowView.ContentRegion.Size,
				FlowDirection = ControlFlowDirection.SingleTopToBottom,
				Parent = WindowView
			};
			
			new Label()
			{
				Parent = mainFlowPanel,
				Width = mainFlowPanel.Width,
				
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
				OuterControlPadding = new Vector2(0,10),
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
			applyDefaultKeybindButton.Click += (sender, args) => OnApplyDefaultKeymapClicked?.Invoke(sender,defaultKeybindDropdown.SelectedItem);
			defaultKeybindDropdown.ValueChanged += (sender, args)=> OnDefaultKeymapChanged?.Invoke(sender, args.CurrentValue);
		}

		private void WindowView_TabChanged(object sender, ValueChangedEventArgs<Tab> e)
		{
			Logger.GetLogger<CharacterKeybindsTab>().Debug("test");
		}

		public void Show()
		{
			WindowView.Show();
		}

		public void SetAddButtonState(bool state)
        {
            addEntryButton.Enabled = state;
            addEntryButton.Text = state ? "+ Add Binding" : "Add Binding (Loading Characters...)";
        }

        public void SetDefaultKeybindOptions(List<string>options, string selectedOption)
        {
	        defaultKeybindDropdown.Items.Clear();
	        options.ForEach(e => defaultKeybindDropdown.Items.Add(e));
	        defaultKeybindDropdown.SelectedItem = selectedOption;
        }

        public void ToggleWindow()
        {
	        WindowView.ToggleWindow();
        }
        
        public void SetBlocker(bool visibility) 
        {
            SetAddButtonState(!visibility);
            blockerOverlay.Text = "API token missing or not available yet.\n\n" +
                "Make sure you have added an API token to Blish HUD \nand it has the neccessary permissions!\n" +
                "(Previously setup keybinds will still work!)";

            blockerOverlay.Visible = visibility;
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
            List<string> specializations,
            List<string> keymaps)
        {
            keybindFlowContainer.SetDropdownContent(keybindFlowContainer.characterNameDropdown, charaters);
            keybindFlowContainer.SetDropdownContent(keybindFlowContainer.specializationDropdown, specializations);
            keybindFlowContainer.SetDropdownContent(keybindFlowContainer.keymapDropdown, keymaps);
        }

        public void SetKeybindValues(KeybindFlowContainer keybindFlowContainer, Keymap keymap, int iconId)
        {
            keybindFlowContainer.SetValues(keymap);
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
	            keybindScrollView.RemoveChild(keybindScrollView.Children[i]);
            }
        }
    }
}
