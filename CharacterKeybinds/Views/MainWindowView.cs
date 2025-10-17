using System;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.Managers;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace flakysalt.CharacterKeybinds.Views
{
    public class MainWindowView : View , IDisposable
    {
        public CharacterKeybindsTab KeybindsTab { get; private set; }
        public KeybindMigrationTab MigrationTab { get; private set; }

        private TabbedWindow2 window;

        public Tab SelectedTab => window.SelectedTab;


        public event EventHandler<ValueChangedEventArgs<Tab>> TabChanged;
        public event EventHandler<EventArgs> WindowShown;


        public MainWindowView(ContentsManager contentsManager,
            AsyncTexture2D windowBackgroundTexture, Rectangle windowRegion, Rectangle contentRegion)
        {
            window = new TabbedWindow2(windowBackgroundTexture,windowRegion,contentRegion)
            {
                Emblem = contentsManager.GetTexture("images/logo.png"),
                Parent = GameService.Graphics.SpriteScreen,
                Title = "Character Keybinds",
                SavesPosition = true,
                Id = $"flakysalt_{nameof(CharacterKeybinds)}",
                CanClose = true,
                CanResize = true
            };
            InitializeTabs(contentsManager);
            window.Shown += WindowShownEvent;
            window.TabChanged += TabChangedEvent;
        }

        private void TabChangedEvent(object sender, ValueChangedEventArgs<Tab> e)
        {
            TabChanged?.Invoke(sender,e);
        }
        
        private void WindowShownEvent(object sender, EventArgs e)
        {
            WindowShown?.Invoke(sender,e);
        }

        private void InitializeTabs(ContentsManager contentsManager)
        {
            // Create Character Keybinds tab
            KeybindsTab = new CharacterKeybindsTab();
            MigrationTab = new KeybindMigrationTab();
            var keybindsTabItem = new Tab(contentsManager.GetTexture("images/logo_small.png"), () => KeybindsTab, "Character Keybinds");
            var migrationTabItem = new Tab(contentsManager.GetTexture("images/logo_small.png"), () => MigrationTab, "Migration");
            
            KeybindsTab.OnAddButtonClicked += (e, s) =>
            {
                window.SelectedTab = keybindsTabItem;
            };
            
            window.Tabs.Add(keybindsTabItem);
            window.Tabs.Add(migrationTabItem);

            if (window.Tabs.Count > 0)
            {
                window.SelectedTab = keybindsTabItem;
            }
        }
        
        public void Show()
        {
            window.Show();
        }

        public void ToggleWindow()
        {
            window.ToggleWindow();
        }

        public void Dispose()
        {
        }
    }
}
