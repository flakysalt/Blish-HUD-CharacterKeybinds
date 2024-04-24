using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using flakysalt.CharacterKeybinds.Views;
using flakysalt.CharacterKeybinds.Model;
using CharacterKeybinds.Views;

namespace ExampleBlishhudModule
{
    [Export(typeof(Module))]
    public class CharacterKeybindModule : Module
    {

        internal static CharacterKeybindModule moduleInstance;

        private static readonly Logger Logger = Logger.GetLogger<CharacterKeybindModule>();

        private Texture2D _cornerTexture;
        private CornerIcon _cornerIcon;

        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        public override IView GetSettingsView() => new SettingsWindow(settingsModel);

        public CharacterKeybindsModel settingsModel;

        public AutoclickView autoclickerView;

        #region Views
        private AssignmentWindow moduleWindowView;
        #endregion

        [ImportingConstructor]
        public CharacterKeybindModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            moduleInstance = this;
        }

        protected override void DefineSettings(SettingCollection settings)
        {

            settingsModel = new CharacterKeybindsModel();
            settingsModel.Init(settings);
        }
        protected override async Task LoadAsync()
        {
            settingsModel.LoadCharactersAsync(Gw2ApiManager);

            _cornerTexture = ContentsManager.GetTexture("images/logo.png");
            var windowBackgroundTexture = AsyncTexture2D.FromAssetId(155997);

            moduleWindowView = new AssignmentWindow();
            autoclickerView = new AutoclickView();

            await moduleWindowView.Init(ContentsManager, Gw2ApiManager, settingsModel, DirectoriesManager, autoclickerView);
            autoclickerView.Init(GameService.Input, settingsModel);

            //await CreateGw2StyleWindowThatDisplaysAllCurrencies(windowBackgroundTexture);
            CreateCornerIconWithContextMenu();
        }

		protected override void Update(GameTime gameTime)
        {
            autoclickerView.Update();
            moduleWindowView.Update(gameTime);
        }

        // For a good module experience, your module should clean up ANY and ALL entities
        // and controls that were created and added to either the World or SpriteScreen.
        // Be sure to remove any tabs added to the Director window, CornerIcons, etc.
        protected override void Unload()
        {
            moduleWindowView?.AssignmentView?.Dispose();
            _cornerIcon?.Dispose();
            _cornerTexture?.Dispose();
            moduleInstance = null;
        }
        
        private void CreateCornerIconWithContextMenu()
        {
            _cornerIcon = new CornerIcon()
            {
                Icon = _cornerTexture,
                BasicTooltipText = $"{Name}",
                Priority = 1645843523,
                Parent = GameService.Graphics.SpriteScreen
            };
            _cornerIcon.Click += (s, e) => moduleWindowView.AssignmentView.ToggleWindow();
        }
    }
}
