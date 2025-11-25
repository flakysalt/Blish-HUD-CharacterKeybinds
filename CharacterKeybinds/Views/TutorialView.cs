using System;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using flakysalt.CharacterKeybinds.Data;
using flakysalt.CharacterKeybinds.Resources;
using Microsoft.Xna.Framework;
using View = Blish_HUD.Graphics.UI.View;

namespace flakysalt.CharacterKeybinds.Views
{
    public class TutorialView : View
    {
        Label descriptionTextBox, headerTextBox, panelCounterTextBox;
        Image tutorialImage,CloseButton;
        StandardButton NextButton, PreviousButton;
        FlowPanel panelCounterFlowPanel;
        private Panel mainPanel;

        private TutorialData data;
        private int currentPanelIndex;

        private readonly Action onCloseAction;

        public TutorialView(Action onCloseAction)
        {
            this.onCloseAction = onCloseAction;
            Build(GameService.Graphics.SpriteScreen);
        }
        
        protected sealed override void Build(Container buildPanel)
        {
            mainPanel = new Panel
            {
                Parent = buildPanel,
                Width = buildPanel.Width,
                Height = buildPanel.Height,
                BackgroundColor = new Color(0,0,0,200)
            };
            
            headerTextBox = new Label
            {
                Parent = mainPanel,
                Width = mainPanel.Width,
                HorizontalAlignment = HorizontalAlignment.Center,
                AutoSizeHeight = true,
                Location = new Point(0,50),
                Font = GameService.Content.DefaultFont32,
            };
            CalculateCenteredHorizontalPosition(mainPanel, headerTextBox);
            
            tutorialImage = new Image
            {
                Parent = mainPanel,
                Location = new Point(0,150),
                Width = 1920/2,
                Height = 1080/2,
            };
            CalculateCenteredHorizontalPosition(mainPanel, tutorialImage);
            
            descriptionTextBox = new Label
            {
                Parent = mainPanel,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Width = Math.Min(mainPanel.Width - 100, 800),
                Height = 800,
                Location = new Point(0, tutorialImage.Bottom + 50),
                Font = GameService.Content.DefaultFont18,
            };
            CalculateCenteredHorizontalPosition(mainPanel, descriptionTextBox);
            
            panelCounterFlowPanel = new FlowPanel
            {
                Parent = mainPanel,
                Height = 30,
                Width = 340,
                ControlPadding = new Vector2(10,0),
                FlowDirection = ControlFlowDirection.LeftToRight,
                Location = new Point(0, mainPanel.Bottom - 200)
            };
            CalculateCenteredHorizontalPosition(mainPanel, panelCounterFlowPanel);

            
            PreviousButton = new StandardButton
            {
                Parent = panelCounterFlowPanel,
                Width = 100,
                Height = 30,
                Text = TutorialLoca.previousButtonText,
            };
            
            panelCounterTextBox = new Label
            {
                Parent = panelCounterFlowPanel,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 100,
                Height = 30,
            };
            
            NextButton = new StandardButton
            {
                Parent = panelCounterFlowPanel,
                Text = TutorialLoca.nextButtonText,
                Width = 100,
                Height = 30,
            };
            
            CloseButton = new Image(AsyncTexture2D.FromAssetId(156012))
            {
                Parent = mainPanel,
                Width = 32,
                Height = 32,
                Location = new Point(mainPanel.Width - 42, 10),
                BasicTooltipText = TutorialLoca.closeButtonText,
            };
            
            
            CloseButton.Click += CloseButtonOnClick;
            NextButton.Click += (s, e) =>
            {
                if (currentPanelIndex < data.Panels.Count - 1)
                {
                    currentPanelIndex++;
                    UpdateContent(data.Panels[currentPanelIndex]);
                }
            };
            
            PreviousButton.Click += (s, e) =>
            {
                if (currentPanelIndex > 0)
                {
                    currentPanelIndex--;
                    UpdateContent(data.Panels[currentPanelIndex]);
                }
            };
            buildPanel.Resized += OnResize;
        }
        private void OnResize(object sender, ResizedEventArgs e)
        {
            mainPanel.Size = e.CurrentSize;
            panelCounterFlowPanel.Location = new Point(panelCounterFlowPanel.Location.X, mainPanel.Bottom - 200);
            CloseButton.Location = new Point(mainPanel.Width - 42, 10);
            
            CalculateCenteredHorizontalPosition(mainPanel, headerTextBox);
            CalculateCenteredHorizontalPosition(mainPanel, tutorialImage);
            CalculateCenteredHorizontalPosition(mainPanel, descriptionTextBox);
            CalculateCenteredHorizontalPosition(mainPanel, panelCounterFlowPanel);
            

        }

        public void Show(TutorialData data)
        {
            this.data = data;
            currentPanelIndex = 0;

            headerTextBox.Text = data.Header;
            UpdateContent(data.Panels[currentPanelIndex]);
            mainPanel.Show();
        }
        private void UpdateContent(TutorialPanel panelData)
        {
            NextButton.Enabled = currentPanelIndex < data.Panels.Count - 1;
            PreviousButton.Enabled = currentPanelIndex > 0;
            
            tutorialImage.Texture = GameService.Content.GetTexture(panelData.ImagePath);
            descriptionTextBox.Text = panelData.Description;
            
            descriptionTextBox.Text = "This is a long description to test\n how the \ntext wrapping and alignment works in this tutorial view.\n It should be centered and properly wrapped within the designated area.\n" +
                                      "Here is some more text\n" +
                                      "and we keep going to see how it handles multiple lines of text.\n ";
            
            panelCounterTextBox.Text = $"{currentPanelIndex + 1} / {data.Panels.Count}";
        }
        
        private void CalculateCenteredHorizontalPosition(Control container, Control objectToCenter)
        {
            objectToCenter.Location = new Point((container.Width - objectToCenter.Width) / 2, objectToCenter.Location.Y);
        }

        private void CloseButtonOnClick(object sender, EventArgs e)
        {
            mainPanel.Hide();
            //TODO uncomment this
            //onCloseAction.Invoke();
        }
    }
}
