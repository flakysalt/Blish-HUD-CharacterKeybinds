using System;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using flakysalt.CharacterKeybinds.Data;
using flakysalt.CharacterKeybinds.Model;
using flakysalt.CharacterKeybinds.Resources;
using Microsoft.Xna.Framework;
using ContentService = flakysalt.CharacterKeybinds.Services.ContentService;
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
        private CharacterKeybindsSettings _settings;

        public static TutorialView Instance { get; private set; }

        public TutorialView(CharacterKeybindsSettings settings)
        {
            Instance = this;
            _settings = settings;
            Build(GameService.Graphics.SpriteScreen);
        }
        
        protected sealed override void Build(Container buildPanel)
        {
            mainPanel = new Panel
            {
                Parent = buildPanel,
                Width = buildPanel.Width,
                Height = buildPanel.Height,
                BackgroundColor = new Color(0,0,0,230),
                Visible = false,
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
                Width = (int)(1920 * 0.6f),
                Height = (int)(1080 * 0.6f),
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
                Height = 50,
                Width = 500,
                ControlPadding = new Vector2(10,0),
                FlowDirection = ControlFlowDirection.LeftToRight,
                Location = new Point(0, mainPanel.Bottom - 200)
            };
            CalculateCenteredHorizontalPosition(mainPanel, panelCounterFlowPanel);

            
            PreviousButton = new StandardButton
            {
                Parent = panelCounterFlowPanel,
                Width = 150,
                Height = panelCounterFlowPanel.Height,
                Text = TutorialLoca.previousButtonText,
            };
            
            panelCounterTextBox = new Label
            {
                Parent = panelCounterFlowPanel,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 150,
                Height = panelCounterFlowPanel.Height,
            };
            
            NextButton = new StandardButton
            {
                Parent = panelCounterFlowPanel,
                Text = TutorialLoca.nextButtonText,
                Width = 150,
                Height = panelCounterFlowPanel.Height,
            };
            
            CloseButton = new Image(AsyncTexture2D.FromAssetId(156012))
            {
                Parent = mainPanel,
                Width = 64,
                Height = 64,
                Location = new Point(mainPanel.Width - 74, 10),
                BasicTooltipText = TutorialLoca.closeButtonText,
            };
            
            
            CloseButton.Click += CloseButtonOnClick;
            mainPanel.Click += (s, e) =>
            {
                if (PreviousButton.MouseOver || NextButton.MouseOver || CloseButton.MouseOver)
                    return;
                
                if (currentPanelIndex < data.Panels.Count - 1)
                {
                    currentPanelIndex++;
                    UpdateContent(data.Panels[currentPanelIndex]);
                }
            };
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
            mainPanel.Size = mainPanel.Parent.Size;
            panelCounterFlowPanel.Location = new Point(panelCounterFlowPanel.Location.X, mainPanel.Bottom - 200);
            CloseButton.Location = new Point(mainPanel.Width - 74, 10);
            
            headerTextBox.Width = mainPanel.Width;
            descriptionTextBox.Width = Math.Min(mainPanel.Width - 100, 800);
            
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
            OnResize(this, null);
        }
        private void UpdateContent(TutorialPanel panelData)
        {
            NextButton.Enabled = currentPanelIndex < data.Panels.Count - 1;
            PreviousButton.Enabled = currentPanelIndex > 0;

            tutorialImage.Texture = ContentService.Instance.GetTexture(panelData.ImagePath);
            descriptionTextBox.Text = panelData.Description;
            panelCounterTextBox.Text = $"{currentPanelIndex + 1} / {data.Panels.Count}";
        }
        
        private void CalculateCenteredHorizontalPosition(Control container, Control objectToCenter)
        {
            objectToCenter.Location = new Point((container.Width - objectToCenter.Width) / 2, objectToCenter.Location.Y);
        }

        private void CloseButtonOnClick(object sender, EventArgs e)
        {
            mainPanel.Hide();
            _settings.experiencedFtue.Value = true;
        }
    }
}
