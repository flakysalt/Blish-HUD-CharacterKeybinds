using System;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.Managers;
using flakysalt.CharacterKeybinds.Resources;
using flakysalt.CharacterKeybinds.Services;
using Microsoft.Xna.Framework;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace flakysalt.CharacterKeybinds.Views
{
    public class MainWindowView : View , IDisposable
    {
        public CharacterKeybindsTab KeybindsTab { get; private set; }
        public KeybindMigrationTab MigrationTab { get; private set; }

        private readonly TabbedWindow2 window;

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
                Title = Loca.moduleName,
                SavesPosition = true,
                Id = $"flakysalt_{nameof(CharacterKeybinds)}",
                CanClose = true,
                CanResize = true,
                SavesSize = true
            };
            InitializeTabs(contentsManager);
            window.Shown += WindowShownEvent;
            window.TabChanged += TabChangedEvent;
            window.Resized += OnResized;
            LocaService.Instance.LocaleChanged += (s, e) =>
            {
                window.Title = Loca.moduleName;
            };

            //setting the window size so the background image is scaled properly
            window.Size = new Point(670, 600);

        }

        private void OnResized(object sender, ResizedEventArgs e)
        {
            int newWidth = MathHelper.Clamp(e.CurrentSize.X, 660, e.CurrentSize.X);
            int newHeight = MathHelper.Clamp(e.CurrentSize.Y, 250, e.CurrentSize.Y);
            window.Size = new Point(newWidth, newHeight);
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
            
            var icon = AsyncTexture2D.FromAssetId(784346);
            var icon2 = AsyncTexture2D.FromAssetId(157113);

            var keybindsTabItem = new Tab(contentsManager.GetTexture("images/Character_Keybinds_key_32.png"), () => KeybindsTab, "Character Keybinds");
            //var keybindsTabItem = new Tab(icon2, () => KeybindsTab, Loca.moduleName);

            var migrationTabItem = new Tab(icon, () => MigrationTab, Loca.migration);
            
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
            window.Resized -= OnResized;
            window.TabChanged -= TabChangedEvent;
            window.Shown -= WindowShownEvent;
            window.Dispose();
        }
    }
}
