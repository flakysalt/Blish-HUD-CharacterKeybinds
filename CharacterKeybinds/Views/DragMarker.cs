using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Blish_HUD.Controls.Intern;
using Mouse = Blish_HUD.Controls.Intern.Mouse;
using Blish_HUD;

namespace CharacterKeybinds.Views
{
	class DragMarker : Container
	{

		Point startDragMouseOffset;
		Image image;

		bool Dragging;


		public DragMarker(int order = 0) 
		{

			Width = 20;
			Height = 20;
			Parent = GameService.Graphics.SpriteScreen;

/*			image = new Image()
			{
				BackgroundColor = Color.AliceBlue,
				Width = Width,
				Height = Height,
				Parent = this,
				Visible = true
			};*/
			var text = new Label
			{
				Text = order == 0 ? "" : order.ToString(),
				Parent = this,
				Size = Size,
				BackgroundColor = Color.Black,
				HorizontalAlignment = HorizontalAlignment.Center
			};

			LeftMouseButtonPressed += Image_LeftMouseButtonPressed;
			LeftMouseButtonReleased += DragMarker_LeftMouseButtonReleased;

		}

		private void DragMarker_LeftMouseButtonReleased(object sender, Blish_HUD.Input.MouseEventArgs e)
		{
			Dragging = false;
		}

		private void Image_LeftMouseButtonPressed(object sender, Blish_HUD.Input.MouseEventArgs e)
		{
			startDragMouseOffset = Location - Input.Mouse.Position;
			Dragging = true;
		}

		public override void UpdateContainer(GameTime gameTime)
		{
			base.UpdateContainer(gameTime);

			if (Dragging)
			{
				Location = Input.Mouse.Position + startDragMouseOffset;

				//startDrag = Input.Mouse.Position;
			}
		}

		// This offset calculation is needed because the mouse does not consider the UI scale when simulaating a click!
		public Point CalculateClickOffset() 
		{
			var offset = new Point((int)((Location.X + (Width / 2)) * GameService.Graphics.UIScaleMultiplier),
				(int)((Location.Y + (Height / 2)) * GameService.Graphics.UIScaleMultiplier));
			var clickPos = Location - offset;

			return clickPos;

		}

		public void SimulateClick() 
		{
			Mouse.Click(MouseButton.LEFT,Location.X - CalculateClickOffset().X, Location.Y - CalculateClickOffset().Y);
		}
	}
}
