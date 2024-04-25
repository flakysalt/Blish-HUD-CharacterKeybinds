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
            settingsModel = new CharacterKeybindsModel(settings);
        }
        protected override async Task LoadAsync()
        {
            _cornerTexture = ContentsManager.GetTexture("images/logo.png");
            moduleWindowView = new AssignmentWindow(Logger);
            autoclickerView = new AutoclickView();

            await moduleWindowView.Init(ContentsManager, Gw2ApiManager, settingsModel, DirectoriesManager, autoclickerView);
            autoclickerView.Init(settingsModel);

            CreateCornerIconWithContextMenu();
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

        protected override void Update(GameTime gameTime)
        {
            autoclickerView.Update();
            moduleWindowView.Update(gameTime);
        }

        protected override void Unload()
        {
            moduleWindowView?.AssignmentView?.Dispose();
            autoclickerView?.AutoClickWindow?.Dispose();
            _cornerIcon?.Dispose();
            _cornerTexture?.Dispose();
            moduleInstance = null;
        }
        

    }
}
