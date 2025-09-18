using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Blish_HUD.Controls.Intern;
using Mouse = Blish_HUD.Controls.Intern.Mouse;
using Blish_HUD;
using System;
using Blish_HUD.Content;

namespace flakysalt.CharacterKeybinds.Views.UiElements
{
	internal sealed class DraggableMarker : Container
	{
		public event EventHandler<Point> OnMarkerReleased;

		private Point _startDragMouseOffset;
		private bool _dragging;
		private const int MarkerTextureId = 1863840;

		public DraggableMarker(int order = 0) 
		{
			var cursorTexture = AsyncTexture2D.FromAssetId(MarkerTextureId);

			Width = 32;
			Height = 32;
			Parent = GameService.Graphics.SpriteScreen;
			ZIndex = 1000;
			new Image(cursorTexture)
			{
				Parent = this,
				Size = Size
			};
			new Label
			{
				Text = order == 0 ? "" : order.ToString(),
				TextColor = Color.White,
				ShadowColor = Color.Black,
				ShowShadow = true,
				Parent = this,
				Size = Size,
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Top
			};
			
			LeftMouseButtonPressed += Image_LeftMouseButtonPressed;
			LeftMouseButtonReleased += DragMarker_LeftMouseButtonReleased;
		}
		
		public override void UpdateContainer(GameTime gameTime)
		{
			base.UpdateContainer(gameTime);
			if (_dragging) Location = Input.Mouse.Position + _startDragMouseOffset;
		}
		
		public void SimulateClick() 
		{
			var offset = CalculateClickOffset();
			Mouse.Click(MouseButton.LEFT,Location.X - offset.X, Location.Y - offset.Y);
		}

		private void DragMarker_LeftMouseButtonReleased(object sender, Blish_HUD.Input.MouseEventArgs e)
		{
			_dragging = false;
			OnMarkerReleased?.Invoke(this,Location);
		}

		private void Image_LeftMouseButtonPressed(object sender, Blish_HUD.Input.MouseEventArgs e)
		{
			_startDragMouseOffset = Location - Input.Mouse.Position;
			_dragging = true;
		}
		
		// This offset calculation is needed because the mouse does not consider the UI scale when simulaating a click!
		private Point CalculateClickOffset() 
		{
			var offset = new Point((int)(Location.X * GameService.Graphics.UIScaleMultiplier),
				(int)(Location.Y * GameService.Graphics.UIScaleMultiplier));
			var clickPos = Location - offset;

			return clickPos;
		}
	}
}
