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
	public class CharacterKeybindWindow : View<CharacterKeybindSettingsPresenter>, IDisposable
    {
        public WindowBase2 WindowView;
        public StandardButton addEntryButton;
        public FlowPanel scrollView, mainFlowPanel;
        public Label blockerOverlay;

        List<KeybindFlowContainer> keybindUIData = new List<KeybindFlowContainer>();

        public void Dispose()
		{

            base.Unload();
        }

		public CharacterKeybindWindow(ContentsManager ContentsManager)
		{

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
				CanClose = true,
			};

			blockerOverlay = new Label()
			{
				Parent = WindowView,
				ZIndex = 4,
				HorizontalAlignment = HorizontalAlignment.Center,
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
				Size = new Point(ScrollViewPanel.Size.X - 20, ScrollViewPanel.Height),
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

		}

        public void SetAddButtonState(bool state)
        {
            addEntryButton.Enabled = state;
            addEntryButton.Text = state ? "Add Binding" : "Add Binding (Loading Characters...)";

        }

        public void BindAddEntryButton(Action onAddButton)
        {
            addEntryButton.Click += (s, e) => onAddButton();
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
                Parent = scrollView,
                Size = new Point(scrollView.Width, 50),
                CanScroll = false,
                FlowDirection = ControlFlowDirection.LeftToRight
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

        public void SetKeybindValues(KeybindFlowContainer keybindFlowContainer, Keymap keymap)
        {
            keybindFlowContainer.SetValues(keymap);
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
            for (int i = scrollView.Children.Count - 1; i >= 0; i--)
            {
                scrollView.RemoveChild(scrollView.Children[i]);
            }
        }
    }
}
