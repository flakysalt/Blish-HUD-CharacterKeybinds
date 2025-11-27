using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Blish_HUD.Content;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using flakysalt.CharacterKeybinds.Data.Tutorial;
using flakysalt.CharacterKeybinds.Model;
using flakysalt.CharacterKeybinds.Presenter;
using flakysalt.CharacterKeybinds.Services;
using flakysalt.CharacterKeybinds.Views;
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
            new SettingsWindow(_settingsModel, mainWindowView, _autoClickerView);

        private CharacterKeybindsSettings _settingsModel;

        #region Views

        private MainWindowView mainWindowView;
        private MainWindowPresenter mainWindowPresenter;
        private CharacterKeybindsCornerButton _cornerButtonView;

        private AutoClickerView _autoClickerView;
        private ContentService contentService;
        private Gw2ApiService apiService;
        private MainWindowModel mainWindowModel;
        private TutorialView tutorialView;

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
            CreateServices();

            var locaService = new LocaService();

            CreateViews();
            CreatePresenters();
            AttachEvents();

            return Task.CompletedTask;
        }
        
        private void CreatePresenters()
        {
            mainWindowPresenter =
                new MainWindowPresenter(apiService,_settingsModel, mainWindowView,new MainWindowModel());
        }

        private void CreateViews()
        {
            _cornerButtonView = new CharacterKeybindsCornerButton(contentService,_settingsModel);
            _autoClickerView = new AutoClickerView(_settingsModel, ContentsManager);
            
            mainWindowView = new MainWindowView(ContentsManager,
                AsyncTexture2D.FromAssetId(155997),
                new Rectangle(24, 30, 545, 600),
                new Rectangle(82, 30, 467, 600)
            );

            Ftue();
        }
        private void Ftue()
        {
            tutorialView = new TutorialView(_settingsModel);

            if (_settingsModel.experiencedFtue.Value == false && 
                _settingsModel.Keymaps.Value.Count == 0)
            {
                tutorialView.Show(new SetupTutorial());
                GameService.Overlay.BlishHudWindow.Hide();
            }
        }

        private void CreateServices()
        {
            contentService = new ContentService(ContentsManager);
            apiService = new Gw2ApiService(Gw2ApiManager);
        }

        private void AttachEvents()
        {
            _cornerButtonView.OnCornerButtonClicked += mainWindowView.ToggleWindow;
        }

        protected override void Update(GameTime gameTime)
        {
            mainWindowPresenter.Update(gameTime);
        }

        protected override void Unload()
        {
            _cornerButtonView.OnCornerButtonClicked -= mainWindowView.ToggleWindow;

            mainWindowView?.Dispose();
            _autoClickerView?.Dispose();
            _cornerButtonView?.Dispose();

            _cornerButtonView = null;
            mainWindowView = null;
            _autoClickerView = null;
            moduleInstance = null;
        }
    }
}