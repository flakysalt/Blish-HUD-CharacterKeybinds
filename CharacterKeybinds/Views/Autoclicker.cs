using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using Keyboard = Blish_HUD.Controls.Intern.Keyboard;
using flakysalt.CharacterKeybinds.Data;
using flakysalt.CharacterKeybinds.Model;

using flakysalt.CharacterKeybinds.Views.UiElements;
using System.Linq;
using Blish_HUD.Modules.Managers;

namespace flakysalt.CharacterKeybinds.Views
{
	public class Autoclicker
	{
        public  StandardWindow WindowView;
        private StandardButton ToggleVisibilityButton, resetPositionButton, testClickerButton;

        private bool markerVisible;
        private List<DraggableMarker> markers = new List<DraggableMarker>();
        CharacterKeybindsSettings settingsModel;

        public async void Init(CharacterKeybindsSettings settingsModel, ContentsManager ContentsManager)
        {
            this.settingsModel = settingsModel;

            var windowBackgroundTexture = AsyncTexture2D.FromAssetId(155997);
            var _emblem = ContentsManager.GetTexture("images/logo.png");

            WindowView = new StandardWindow(
                windowBackgroundTexture,
                new Rectangle(25, 26, 560, 649),
                new Rectangle(40, 50, 540, 590),
                new Point(560, 400))
            {
                Emblem = _emblem,
                Parent = GameService.Graphics.SpriteScreen,
                Title = "Troubleshoot Window",
                SavesPosition = true,
                Id = $"flakysalt_{nameof(Autoclicker)}",
                CanClose = true
            };

            var mainFlowPanel = new FlowPanel()
            {
                Size = WindowView.Size,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(5, 2),
                OuterControlPadding = new Vector2(0, 15),
                Parent = WindowView
            };
            var troubleshootText = new Label
            {
                Text = "Only change the position of the markes if you erxperience problems. \n\n" +
                "Enable the markers by pressing 'Toggle Marker Visibility'\n" +
                "Move the marker to the following positions if they do not allign automatically:\n" +
                "1. The Options menu tab\n" +
                "2. The dropdown at the bottom right of the options menu\n" +
                "3. The first entry of the dropdown when opening it\n" +
                "4. The 'Yes' button of the confirmation popup when importing key binds",
                Width = mainFlowPanel.Width,
                Height = 150,
                Parent = mainFlowPanel,
            };

            var buttonFlowPanel = new FlowPanel()
            {
                Size = WindowView.Size,
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(5, 2),
                OuterControlPadding = new Vector2(0, 15),
                Parent = mainFlowPanel
            };

            ToggleVisibilityButton = new StandardButton()
            {
                Text = "Toggle Marker Visibility",
                Width = 160,
                Parent = buttonFlowPanel,
            };

            resetPositionButton = new StandardButton()
            {
                Text = "Reset Marker Positions",
                Width = 160,
                Parent = buttonFlowPanel
            };

            testClickerButton = new StandardButton()
            {
                Text = "Test Markers",
                BasicTooltipText = "This will simulate the sequence of clicks",
                Width = 160,
                Parent = buttonFlowPanel
            };

            resetPositionButton.Click += ResetMarkerPositions;
			testClickerButton.Click += TestClickerButton_Click;
            ToggleVisibilityButton.Click += ToggleVisibilityButton_Click;
			GameService.Graphics.SpriteScreen.Resized += SpriteScreen_Resized;

			WindowView.Hidden += AutoClickWindow_Hidden;
            SpawnImportClickZones();
        }

		private void TestClickerButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
		{
            Task.Run(ClickInOrder);
        }

        private void AutoClickWindow_Hidden(object sender, System.EventArgs e)
		{
            markerVisible = false;
            foreach (var marker in markers)
            {
                marker.Visible = markerVisible;
            }
        }

		public void Dispose() 
        {
            resetPositionButton.Click -= ResetMarkerPositions;

            ToggleVisibilityButton.Click -= ToggleVisibilityButton_Click;
            GameService.Graphics.SpriteScreen.Resized -= SpriteScreen_Resized;

            for (int i = 0; i < markers.Count; i++)
            {
                markers[i].OnMarkerReleased -= Marker_OnMarkerReleased;
                markers[i].Dispose();
            }
            markers.Clear();
            WindowView?.Dispose();
        }

        private void SpriteScreen_Resized(object sender, ResizedEventArgs e)
		{
            SetMarkerPositions();
        }

		private void UI_UISizeChanged(object sender, ValueEventArgs<Gw2Sharp.Mumble.Models.UiSize> e)
		{
            markers[0].Location = new Point ((int)(GameService.Graphics.SpriteScreen.Size.X * 0.6f),
                (int)(GameService.Graphics.SpriteScreen.Size.Y * 0.6f));
        }

		private void ToggleVisibilityButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
		{
            markerVisible = !markerVisible;
            foreach (var marker in markers) 
            {
                marker.Visible = markerVisible;
            }
		}
        private void SetMarkerPositions() 
        {
            for (int i = 0; i < settingsModel.clickPositions.Value.Count; i++)
            {
                markers[i].Location = ScreenScenter() + settingsModel.clickPositions.Value[i];
            }
        }

        private void ResetMarkerPositions(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            settingsModel.clickPositions.Value = ClickPositions.importClickPositions;
            SetMarkerPositions();
        }

        private void SimulateClick_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
		{
            Task.Run(ClickInOrder);
        }

        public async Task ClickInOrder() 
        {
            ScreenNotification.ShowNotification("Switching keybinds... ", ScreenNotification.NotificationType.Red, duration:3);
            var keyboardShortcut = settingsModel.optionsKeybind.Value.PrimaryKey;
            await Task.Delay(1000);
            Keyboard.Stroke((Blish_HUD.Controls.Extern.VirtualKeyShort)keyboardShortcut);
            await Task.Delay(300);

            foreach (var marker in markers)
            {
                marker.SimulateClick();
                await Task.Delay(200);
            }
            await Task.Delay(500);
            Keyboard.Stroke((Blish_HUD.Controls.Extern.VirtualKeyShort)keyboardShortcut);
        }

        private void SpawnImportClickZones()
		{
            for (int i = 0; i < 4; i++) 
            {
                DraggableMarker clickZone = new DraggableMarker(i+1)
                {
                    Visible = false,
                };
                markers.Add(clickZone);
            }
            SetMarkerPositions();

            foreach (var marker in markers)
            {
				marker.OnMarkerReleased += Marker_OnMarkerReleased;
            }
        }

		private void Marker_OnMarkerReleased(object sender, Point e)
		{
            settingsModel.clickPositions.Value = markers.Select(marker => marker.Location- ScreenScenter()).ToList();
        }

		Point ScreenScenter() 
        {
            return new Point(GameService.Graphics.SpriteScreen.Size.X / 2, GameService.Graphics.SpriteScreen.Size.Y / 2);
        }
	}
}
