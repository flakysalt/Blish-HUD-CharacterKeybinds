using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using Keyboard = Blish_HUD.Controls.Intern.Keyboard;
using flakysalt.CharacterKeybinds.Data;
using flakysalt.CharacterKeybinds.Views.UiElements;
using System.Linq;

namespace flakysalt.CharacterKeybinds.Views
{
	public class AutoclickView
	{
        public  StandardWindow AutoClickWindow;
        private Label positionDebugLabel,characterDebugLabel;
        private StandardButton ToggleVisibilityButton, simulateClick, resetPositionButton;

        private bool markerVisible;
        private List<DraggableMarker> markers = new List<DraggableMarker>();
        CharacterKeybindsSettings settingsModel;

        public async void Init(CharacterKeybindsSettings settingsModel)
        {
            this.settingsModel = settingsModel;
            var windowBackgroundTexture = AsyncTexture2D.FromAssetId(155997);
            AutoClickWindow = new StandardWindow(
                windowBackgroundTexture,
                new Rectangle(25, 26, 560, 649),
                new Rectangle(40, 50, 540, 590))
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "Debug Window",
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
                Parent = AutoClickWindow
            };


            ToggleVisibilityButton = new StandardButton()
            {
                Text = "Toggle Marker Visibility",
                Width = 200,
                Parent = mainFlowPanel,
            };
            resetPositionButton = new StandardButton()
            {
                Text = "Reset Marker Positions",
                Width = 200,
                Parent = mainFlowPanel
            };

            simulateClick = new StandardButton()
            {
                Text = "Simulate Clicks!",
                Parent = mainFlowPanel,
                Width = 200
            };

			positionDebugLabel = new Label
			{
				Text = "",
				Width = 200,
				Height = 200,
				Parent = mainFlowPanel,
				//Visible = false
			};

            characterDebugLabel = new Label
            {
                Text = "",
                Width = 200,
                Height = 200,
                Parent = mainFlowPanel,
                //Visible = false
            };

            resetPositionButton.Click += ResetMarkerPositions;

            ToggleVisibilityButton.Click += ToggleVisibilityButton_Click;
            simulateClick.Click += SimulateClick_Click;
			GameService.Graphics.SpriteScreen.Resized += SpriteScreen_Resized;

			AutoClickWindow.Hidden += AutoClickWindow_Hidden;
            SpawnImportClickZones();
        }

		private void AutoClickWindow_Hidden(object sender, System.EventArgs e)
		{
            markerVisible = false;
            foreach (var marker in markers)
            {
                marker.Visible = markerVisible;
            }
        }

        public void UpdateSelectedCharacter(string newCharName, string newSpecname) 
        {
            characterDebugLabel.Text = $"Current Character: {newCharName}\n" +
                $"Current Spec: {newSpecname}";
        }

		public void Dispose() 
        {
            resetPositionButton.Click -= ResetMarkerPositions;

            ToggleVisibilityButton.Click -= ToggleVisibilityButton_Click;
            simulateClick.Click -= SimulateClick_Click;
            GameService.Graphics.SpriteScreen.Resized -= SpriteScreen_Resized;

            for (int i = 0; i < markers.Count; i++)
            {
                markers[i].OnMarkerReleased -= Marker_OnMarkerReleased;
                markers[i].Dispose();
            }
            markers.Clear();
            AutoClickWindow?.Dispose();
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
            settingsModel.clickPositions.Value = ClickPosLocations.importMarkerLocations;
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

        public void Update() 
        {
            string markerPositionsString = "";

            for (int i = 0; i < markers.Count; i++)
            {
                markerPositionsString += $"Marker{i + 1} Loc: {markers[i].Location} \n";
            }
            positionDebugLabel.Text = markerPositionsString +
               $"\n ScreenSize: {GameService.Graphics.SpriteScreen.Size}" ;
        }
	}
}
