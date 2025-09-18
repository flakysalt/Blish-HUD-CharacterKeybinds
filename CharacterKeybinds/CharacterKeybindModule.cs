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
using System.ComponentModel;
using flakysalt.CharacterKeybinds.Model;
using flakysalt.CharacterKeybinds.Presenter;
using flakysalt.CharacterKeybinds.Util;

namespace flakysalt.CharacterKeybinds
{
    [Export(typeof(Module))]
    public class CharacterKeybindModule : Module
    {
        internal static CharacterKeybindModule moduleInstance;

        private Texture2D _cornerTexture;
        private CornerIcon _cornerIcon;

        private ContentsManager ContentsManager => ModuleParameters.ContentsManager;
        private Gw2ApiManager Gw2ApiManager => ModuleParameters.Gw2ApiManager;

        public override IView GetSettingsView() =>
            new SettingsWindow(_settingsModel, _moduleWindowView, _autoClickerView);

        private CharacterKeybindsSettings _settingsModel;

        #region Views

        private CharacterKeybindsTab _moduleWindowView;
        private CharacterKeybindSettingsPresenter _presenter;

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
            _cornerTexture = ContentsManager.GetTexture("images/logo_small.png");
            _autoClickerView = new Autoclicker();
            _autoClickerView.Init(_settingsModel, ContentsManager);

            LoadModuleWindow();

            CreateCornerIconWithContextMenu();
            return Task.CompletedTask;
        }

        private void LoadModuleWindow()
        {
            _moduleWindowView = new CharacterKeybindsTab(ContentsManager);
            var model = new CharacterKeybindModel(_settingsModel);
            _presenter =
                new CharacterKeybindSettingsPresenter(_moduleWindowView, model, Gw2ApiManager, _autoClickerView);
        }

        private void CreateCornerIconWithContextMenu()
        {
            if (_settingsModel.displayCornerIcon.Value)
            {
                _cornerIcon = new CornerIcon()
                {
                    Icon = _cornerTexture,
                    BasicTooltipText = $"{Name}",
                    Priority = 1,
                    Parent = GameService.Graphics.SpriteScreen,
                    Visible = true
                };
                _cornerIcon.Click += (s, e) => _moduleWindowView.ToggleWindow();
            }

            _settingsModel.displayCornerIcon.PropertyChanged += EnableOrCreateCornerIcon;
        }

        private void EnableOrCreateCornerIcon(object sender, PropertyChangedEventArgs e)
        {
            //TODO i dont know why it enables but this should work as a workaround for now
            if (_cornerIcon == null)
            {
                _cornerIcon = new CornerIcon()
                {
                    Icon = _cornerTexture,
                    BasicTooltipText = $"{Name}",
                    Priority = 1,
                    Parent = GameService.Graphics.SpriteScreen,
                };
            }

            _cornerIcon.Visible = _settingsModel.displayCornerIcon.Value;
        }


        protected override void Update(GameTime gameTime)
        {
            _presenter.Update(gameTime);
        }

        protected override void Unload()
        {
            _moduleWindowView?.Dispose();

            _autoClickerView?.Dispose();

            _cornerIcon?.Dispose();
            _cornerTexture?.Dispose();

            _moduleWindowView = null;
            _autoClickerView = null;
            moduleInstance = null;
        }
    }
}