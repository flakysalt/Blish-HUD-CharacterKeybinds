using System;
using System.ComponentModel;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using flakysalt.CharacterKeybinds.Model;
using Microsoft.Xna.Framework.Graphics;

namespace flakysalt.CharacterKeybinds.Views
{
    public class CharacterKeybindsCornerButton : IDisposable
    {
        private Texture2D _cornerTexture;
        private Services.ContentService contentService;
        private CornerIcon cornerIcon;
        
        public Action OnCornerButtonClicked = () => { };
        private readonly CharacterKeybindsSettings _settingsModel;

        public CharacterKeybindsCornerButton(Services.ContentService contentService, CharacterKeybindsSettings settings)
        {
            this.contentService = contentService;
            _settingsModel = settings;
            LoadCornerIcon();
        }
        
        private void LoadCornerIcon()
        {
            EnableOrCreateCornerIcon(null, null);

            cornerIcon.Click += CornerIconClicked;
            _settingsModel.displayCornerIcon.PropertyChanged += EnableOrCreateCornerIcon;

        }

        private void EnableOrCreateCornerIcon(object sender, PropertyChangedEventArgs e)
        {
            if (cornerIcon == null)
            {
                _cornerTexture = contentService.GetTexture("images/logo_small.png");
                cornerIcon = new CornerIcon
                {
                    Icon = _cornerTexture,
                    BasicTooltipText = "Character Keybinds",
                    //Priority = 1,
                    Parent = GameService.Graphics.SpriteScreen,
                    Visible = _settingsModel.displayCornerIcon.Value
                };
            }
            cornerIcon.Visible = _settingsModel.displayCornerIcon.Value;
        }

        private void CornerIconClicked(object sender, MouseEventArgs e)
        {
            OnCornerButtonClicked.Invoke();
        }

        public  void Dispose()
        {
            _cornerTexture?.Dispose();
            cornerIcon.Click -= CornerIconClicked;
            _settingsModel.displayCornerIcon.PropertyChanged -= EnableOrCreateCornerIcon;

            cornerIcon?.Dispose();
            cornerIcon = null;
        }
    }
}