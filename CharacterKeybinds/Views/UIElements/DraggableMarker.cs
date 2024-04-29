using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Blish_HUD.Controls.Intern;
using Mouse = Blish_HUD.Controls.Intern.Mouse;
using Blish_HUD;
using System;
using Blish_HUD.Content;

namespace flakysalt.CharacterKeybinds.Views.UiElements
{
	class DraggableMarker : Container
	{
		Point startDragMouseOffset;
		bool Dragging;

		public event EventHandler<Point> OnMarkerReleased;

		public DraggableMarker(int order = 0) 
		{
			var cursorTexture = AsyncTexture2D.FromAssetId(1863840);

			Width = 32;
			Height = 32;
			Parent = GameService.Graphics.SpriteScreen;
			ZIndex = 1000;
			var image = new Image(cursorTexture)
			{
				Parent = this,
				Size = Size
			};
			var text = new Label
			{
				Text = order == 0 ? "" : order.ToString(),
				TextColor = Color.White,
				ShadowColor = Color.Black,
				ShowShadow = true,
				Parent = this,
				Size = Size,
				//Location = new Point(3,0),
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Top
			};


			LeftMouseButtonPressed += Image_LeftMouseButtonPressed;
			LeftMouseButtonReleased += DragMarker_LeftMouseButtonReleased;

		}

		private void DragMarker_LeftMouseButtonReleased(object sender, Blish_HUD.Input.MouseEventArgs e)
		{
			Dragging = false;
			OnMarkerReleased.Invoke(this,Location);
		}

		private void Image_LeftMouseButtonPressed(object sender, Blish_HUD.Input.MouseEventArgs e)
		{
			startDragMouseOffset = Location - Input.Mouse.Position;
			Dragging = true;
		}

		public override void UpdateContainer(GameTime gameTime)
		{
			base.UpdateContainer(gameTime);
			if (Dragging) Location = Input.Mouse.Position + startDragMouseOffset;
		}

		// This offset calculation is needed because the mouse does not consider the UI scale when simulaating a click!
		public Point CalculateClickOffset() 
		{
			var offset = new Point((int)(Location.X * GameService.Graphics.UIScaleMultiplier),
				(int)(Location.Y * GameService.Graphics.UIScaleMultiplier));
			var clickPos = Location - offset;

			return clickPos;
		}

		public void SimulateClick() 
		{
			Mouse.Click(MouseButton.LEFT,Location.X - CalculateClickOffset().X, Location.Y - CalculateClickOffset().Y);
		}
	}
}
