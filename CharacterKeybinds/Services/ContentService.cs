using Blish_HUD;
using Blish_HUD.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using Blish_HUD.Modules.Managers;

namespace flakysalt.CharacterKeybinds.Services
{
    public class ContentService
    {
        private readonly ContentsManager _contentsManager;
        private readonly Logger _logger = Logger.GetLogger<ContentService>();

        public ContentService(ContentsManager contentsManager)
        {
            _contentsManager = contentsManager;
        }

        public Texture2D GetTexture(string path)
        {
            try
            {
                return _contentsManager.GetTexture(path);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to load texture from path: {path}");
                return null;
            }
        }
    }
}
