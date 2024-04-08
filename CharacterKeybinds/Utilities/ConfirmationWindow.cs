using System;
using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace flakysalt.CharacterKeybinds.Util
{
	class ConfirmationWindow : Container
	{

        private readonly Rectangle _normalizedWindowRegion = new Rectangle(0, 0, 371, 200);

        public ConfirmationWindow(string message, Action onConfirm = null, Action onCancel = null) 
        {
            Parent = Graphics.SpriteScreen;
            BackgroundColor = Color.Black * 0.8f;
            Size = new Point(_normalizedWindowRegion.Width, _normalizedWindowRegion.Height);
            ZIndex = int.MaxValue - 2;
            Visible = false;

            ShowConfirmationDialog(message,onConfirm,onCancel);
        }

        protected override void OnShown(EventArgs e)
        {
            Invalidate();
            base.OnShown(e);
        }

        public void ShowConfirmationDialog(string message, Action onConfirm = null, Action onCancel = null)
        {

            // Add the message as a label
            var messageLabel = new Label
            {
                Parent = this,
                Location = new Point(10, 50), // Adjust as needed
                Text = message,
                AutoSizeWidth = true
            };

            // Add confirm button
            var confirmButton = new StandardButton
            {
                Parent = this,
                Text = "Confirm",
                Location = new Point(25, 150)
            };
            confirmButton.Click += (sender, e) => {
                onConfirm?.Invoke();
                this.Hide();
            };

            // Add cancel button
            var cancelButton = new StandardButton
            {
                Parent = this,
                Text = "Cancel",
                Location = new Point(175, 150)
            };

            cancelButton.Click += (sender, e) => {
                onCancel?.Invoke();
                this.Hide();
            };
        }


        private Rectangle _windowRegion;

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            var parent = this.Parent;

            if (parent != null)
            {
                _size = parent.Size;

                var distanceInwards = new Point(_size.X / 2 - _normalizedWindowRegion.Width / 2,
                                                _size.Y / 2 - _normalizedWindowRegion.Height / 2);

                _windowRegion = _normalizedWindowRegion.OffsetBy(distanceInwards);

                this.ContentRegion = _windowRegion;
            }
        }
    }
}
