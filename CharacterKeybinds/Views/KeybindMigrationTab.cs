using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;

namespace flakysalt.CharacterKeybinds.Views
{
    public class KeybindMigrationTab : View 
    {
        
        private FlowPanel mainFlowPanel;

        protected override void Build(Container buildPanel)
        {
            mainFlowPanel = new FlowPanel
            {
                ControlPadding = new Vector2(0, 10),
                HeightSizingMode = SizingMode.Fill,
                Width = buildPanel.Width,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = buildPanel
            };
            
            new Label()
            {
                Parent = mainFlowPanel,
                Width = mainFlowPanel.Width,
                Text = "Migration Tab",
                Font = GameService.Content.DefaultFont18
            };
            base.Build(buildPanel);
        }
    }
}