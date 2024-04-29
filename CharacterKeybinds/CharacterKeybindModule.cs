using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using flakysalt.CharacterKeybinds.Views;
using flakysalt.CharacterKeybinds.Data;

namespace flakysalt.CharacterKeybinds
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
        public override IView GetSettingsView() => new SettingsWindow(settingsModel, moduleWindowView,autoclickerView, DirectoriesManager, Logger);

        public CharacterKeybindsSettings settingsModel;

        #region Views
        private CharacterKeybindWindow moduleWindowView;
        public TroubleshootWindow autoclickerView;

        #endregion

        [ImportingConstructor]
        public CharacterKeybindModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            moduleInstance = this;
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            settingsModel = new CharacterKeybindsSettings(settings);
        }

        protected override async Task LoadAsync()
        {
            _cornerTexture = ContentsManager.GetTexture("images/logo_small.png");
            moduleWindowView = new CharacterKeybindWindow(Logger);
            autoclickerView = new TroubleshootWindow();

            autoclickerView.Init(settingsModel, ContentsManager);
            await moduleWindowView.Init(ContentsManager, Gw2ApiManager, settingsModel, DirectoriesManager, autoclickerView);

            CreateCornerIconWithContextMenu();
        }

        private void CreateCornerIconWithContextMenu()
        {
            _cornerIcon = new CornerIcon()
            {
                Icon = _cornerTexture,
                BasicTooltipText = $"{Name}",
                Priority = 1,
                Parent = GameService.Graphics.SpriteScreen,
                Visible = settingsModel.displayCornerIcon.Value,
                DynamicHide = true
            };
            _cornerIcon.Click += (s, e) => moduleWindowView.WindowView.ToggleWindow();
			settingsModel.displayCornerIcon.PropertyChanged += (sender,e) => _cornerIcon.Visible = settingsModel.displayCornerIcon.Value;
        }

		protected override void Update(GameTime gameTime)
        {
            moduleWindowView.Update(gameTime);
        }

        protected override void Unload()
        {
            moduleWindowView?.WindowView?.Dispose();
            moduleWindowView?.Dispose();

            autoclickerView?.Dispose();

            _cornerIcon?.Dispose();
            _cornerTexture?.Dispose();


            moduleWindowView = null;
            autoclickerView = null;
            moduleInstance = null;
        }
    }
}
