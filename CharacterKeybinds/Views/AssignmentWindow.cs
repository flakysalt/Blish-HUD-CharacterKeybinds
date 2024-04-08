using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.Managers;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Rectangle = Microsoft.Xna.Framework.Rectangle;


namespace flakysalt.CharacterKeybinds.Views
{
	class AssignmentWindow : View
	{

        private StandardWindow AssignmentView;
        private FlowPanel scrollView;

        private List<Dropdown> dropdowns = new List<Dropdown>();



        public async void Init(ContentsManager ContentsManager) 
		{
            var windowBackgroundTexture = AsyncTexture2D.FromAssetId(155997);
            //var _windowEmblem = ContentsManager.GetTexture("Emblem.png");



            AssignmentView = new StandardWindow(
                windowBackgroundTexture,
                new Rectangle(40, 26, 913, 691),
                new Rectangle(45, 40, 839, 650))
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "Character Keybinds",
                WidthSizingMode = SizingMode.Standard,
                HeightSizingMode = SizingMode.Standard,
                SavesPosition = true,
                CanResize = true,
                SavesSize = true,
                Id = $"flakysalt_{nameof(CharacterKeybinds)}",
                //Emblem = _windowEmblem,
                CanClose = true
                //Size = new Point(360, 360)
            };

            var mainFlowPanel = new FlowPanel()
            {
                Size = AssignmentView.Size,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(5, 2),
                OuterControlPadding = new Vector2(0, 15),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                Parent = AssignmentView
            };

            var addEntryButton = new StandardButton()
            {
                Text = "+",
                Parent = mainFlowPanel,
            };

            scrollView = new FlowPanel
            {
                CanScroll = true,
                Size = new Point(mainFlowPanel.Size.X, 400),
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = mainFlowPanel
            };


            var bottomButtons = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.LeftToRight,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = mainFlowPanel
            };

            var ExportButton = new StandardButton()
            {
                Text = "Export",
                Parent = bottomButtons,
            };

            var ExportAndBindButton = new StandardButton()
            {
                Text = "Export and Bind",
                Parent = bottomButtons,
            };

			addEntryButton.Click += AddEntryButton_Click;

            AssignmentView.Show();
        }

		private void AddEntryButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
		{
            var flowPanel = new FlowPanel
            {
                Parent = scrollView,
                Size = new Point(scrollView.Width, 40),
                CanScroll = false,
                FlowDirection = ControlFlowDirection.SingleTopToBottom
            };

            var dropdown = new Dropdown
            {
                Parent = flowPanel,
                Size = new Point(100, 30),
                Location = new Point(0, 0),
            };

            string newItem = RandomUtil.GetRandom(0, 10).ToString();

            dropdown.Items.Add(newItem);
            dropdown.SelectedItem = newItem;

            dropdowns.Add(dropdown);

            var removeButton = new StandardButton
            {
                Parent = flowPanel,
                Text = "Remove",
                Size = new Point(100, 30),
                Location = new Point(110, 0)
            };

            removeButton.Click += (o, eventArgs) => {
                dropdowns.Remove(dropdown); // Remove the dropdown from the list when its flowPanel is removed
                flowPanel.Dispose();
            };
            scrollView.RecalculateLayout();
        }
	}
}
