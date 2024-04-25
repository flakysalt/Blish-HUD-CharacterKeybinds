using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Keyboard = Blish_HUD.Controls.Intern.Keyboard;
using flakysalt.CharacterKeybinds.Data;
using flakysalt.CharacterKeybinds.Views.UiElements;

namespace flakysalt.CharacterKeybinds.Views
{
	public class AutoclickView : IDisposable
	{
        public  StandardWindow AutoClickWindow;
        private Label debugLabel;
        private StandardButton ToggleVisibilityButton, simulateClick;

        private bool markerVisible;
        private List<DraggableMarker> markers = new List<DraggableMarker>();
        CharacterKeybindsModel settingsModel;

        public async void Init(CharacterKeybindsModel settingsModel)
        {
            this.settingsModel = settingsModel;
            var windowBackgroundTexture = AsyncTexture2D.FromAssetId(155997);
            AutoClickWindow = new StandardWindow(
                windowBackgroundTexture,
                new Rectangle(25, 26, 560, 640),
                new Rectangle(40, 50, 540, 590))
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "Character Keybinds",
                SavesPosition = true,
                Id = $"flakysalt_{nameof(AutoclickView)}",
                CanClose = true
            };

            var mainFlowPanel = new FlowPanel()
            {
                Size = AutoClickWindow.Size,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(5, 2),
                OuterControlPadding = new Vector2(0, 15),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                Parent = AutoClickWindow
            };
            debugLabel = new Label
            {
                Text = "",
                Width = 200,
                Height = 300,
                Parent = mainFlowPanel
            };

            ToggleVisibilityButton = new StandardButton()
            {
                Text = "Toggle Visibility",
                Parent = mainFlowPanel,
            };

            var simulateClick = new StandardButton()
            {
                Text = "Click!",
                Parent = mainFlowPanel,
            };

			ToggleVisibilityButton.Click += ToggleVisibilityButton_Click;
            simulateClick.Click += SimulateClick_Click;
			GameService.Graphics.SpriteScreen.Resized += SpriteScreen_Resized;
            SpawnImportClickZones();
        }

        public void Dispose() 
        {
            ToggleVisibilityButton.Click += ToggleVisibilityButton_Click;
            simulateClick.Click += SimulateClick_Click;
            GameService.Graphics.SpriteScreen.Resized += SpriteScreen_Resized;
            for (int i = 0; i < markers.Count; i++)
            {
                markers[i].Dispose();
            }
            markers.Clear();
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
            for (int i = 0; i < markers.Count; i++)
            {
                markers[i].Location = ScreenScenter() + ClickPosLocations.importMarkerLocations[i];
            }
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
        }

        Point ScreenScenter() 
        {
            return new Point(GameService.Graphics.SpriteScreen.Size.X / 2, GameService.Graphics.SpriteScreen.Size.Y / 2);
        }

        public void Update() 
        {
            string markerPositionsString = "";

            for (int i = 0; i < markers.Count; i++)
            {
                markerPositionsString += $"Marker{i + 1} Loc: {markers[i].Location} \n";
            }
            debugLabel.Text = markerPositionsString +
               $"\n ScreenSize: {GameService.Graphics.SpriteScreen.Size}" ;
        }
	}
}
