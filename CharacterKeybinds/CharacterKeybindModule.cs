using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using flakysalt.CharacterKeybinds.Views;
using flakysalt.CharacterKeybinds.Model;
using flakysalt.CharacterKeybinds.Presenter;
using flakysalt.CharacterKeybinds.Services;
using ContentService = flakysalt.CharacterKeybinds.Services.ContentService;

namespace flakysalt.CharacterKeybinds
{
    [Export(typeof(Module))]
    public class CharacterKeybindModule : Module
    {
        internal static CharacterKeybindModule moduleInstance;
        
        private ContentsManager ContentsManager => ModuleParameters.ContentsManager;
        private Gw2ApiManager Gw2ApiManager => ModuleParameters.Gw2ApiManager;
        public override IView GetSettingsView() =>
            new SettingsWindow(_settingsModel, _moduleWindow, _autoClickerView);

        private CharacterKeybindsSettings _settingsModel;

        #region Views

        private CharacterKeybindsWindow _moduleWindow;
        private CharacterKeybindsTab _moduleTabView;
        private CharacterKeybindPresenter _presenter;
        private CharacterKeybindsCornerButton _cornerButtonView;

        private Autoclicker _autoClickerView;

        #endregion

        [ImportingConstructor]
        public CharacterKeybindModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(
            moduleParameters)
        {
            moduleInstance = this;
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            _settingsModel = new CharacterKeybindsSettings(settings);
        }

        protected override Task LoadAsync()
        {
            // In your module initialization
            var contentService = new ContentService(ContentsManager);
            _cornerButtonView = new CharacterKeybindsCornerButton(contentService,_settingsModel);
            
            _autoClickerView = new Autoclicker();
            _autoClickerView.Init(_settingsModel, ContentsManager);

            LoadModuleWindow();

            return Task.CompletedTask;
        }

        private void LoadModuleWindow()
        {
            var apiService = new Gw2ApiService(Gw2ApiManager);
            
            // Create the window that will hold all tabs
            _moduleWindow = new CharacterKeybindsWindow(ContentsManager,
                AsyncTexture2D.FromAssetId(155997),
                new Rectangle(24, 30, 545, 600),
                new Rectangle(82, 30, 467, 600)
                );
            
            // Get access to the main tab and set up its presenter
            _moduleTabView = _moduleWindow.GetCharacterKeybindsTab();
            var model = new CharacterKeybindsModel(_settingsModel, apiService);
            _presenter =
                new CharacterKeybindPresenter(_moduleTabView, model, apiService, _settingsModel, _autoClickerView);
            
            _cornerButtonView.OnCornerButtonClicked += _moduleWindow.ToggleWindow;
        }

        protected override void Update(GameTime gameTime)
        {
            _presenter.Update(gameTime);
        }

        protected override void Unload()
        {
            _cornerButtonView.OnCornerButtonClicked -= _moduleWindow.ToggleWindow;

            _moduleWindow?.Dispose();
            _autoClickerView?.Dispose();
            _cornerButtonView?.Dispose();

            _cornerButtonView = null;
            _moduleWindow = null;
            _moduleTabView = null;
            _autoClickerView = null;
            moduleInstance = null;
            Logger.GetLogger<CharacterKeybindModule>().Info(GameService.Graphics.SpriteScreen.ToString());
        }
    }
}