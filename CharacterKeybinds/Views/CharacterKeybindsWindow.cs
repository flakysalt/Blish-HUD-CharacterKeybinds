using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Microsoft.Xna.Framework;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace flakysalt.CharacterKeybinds.Views
{
    public class CharacterKeybindsWindow : TabbedWindow2
    {
        private CharacterKeybindsTab _keybindsTab;

        public CharacterKeybindsWindow(ContentsManager contentsManager,
            AsyncTexture2D windowBackgroundTexture, Rectangle windowRegion, Rectangle contentRegion) : 
            base(windowBackgroundTexture, windowRegion, contentRegion, new Point(645, 700))
        {

            Emblem = contentsManager.GetTexture("images/logo.png");
            Parent = GameService.Graphics.SpriteScreen;
            Title = "Character Keybinds";
            SavesPosition = true;
            Id = $"flakysalt_{nameof(CharacterKeybinds)}";
            CanClose = true;

            // Initialize the character keybinds tab
            InitializeTabs(contentsManager);
            
            // Add tab changed event after tabs are initialized
            TabChanged += WindowView_TabChanged;
        }

        private void InitializeTabs(ContentsManager contentsManager)
        {
            // Create Character Keybinds tab
            _keybindsTab = new CharacterKeybindsTab();
            var migrationTab = new KeybindMigrationTab();
            var keybindsTabItem = new Tab(contentsManager.GetTexture("images/logo_small.png"), () => _keybindsTab, "Character Keybinds");
            var migrationTabItem = new Tab(contentsManager.GetTexture("images/logo_small.png"), () => migrationTab, "Migration");


            Tabs.Add(keybindsTabItem);
            Tabs.Add(migrationTabItem);

            if (Tabs.Count > 0)
            {
                SelectedTab = keybindsTabItem;
            }
        }

        private void WindowView_TabChanged(object sender, ValueChangedEventArgs<Tab> e)
        {
            Logger.GetLogger<CharacterKeybindsWindow>().Debug($"Tab changed to: {e.NewValue.Name}");
        }

        public CharacterKeybindsTab GetCharacterKeybindsTab()
        {
            return _keybindsTab;
        }

    }
}
