using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using flakysalt.CharacterKeybinds.Views;
using flakysalt.CharacterKeybinds.Model;

// You can rename the namespace to whatever you want. Including the module name in the namespace is good idea.
// If you change this in the future again after you already released a module version, you should let freesnow know about it.
// Because they have to update that in the Sentry Blish Bug tracker, too.
// This namespace does not have to match the namespace in the manifest.json. They are not related. 
// (Side note: the manifest.json namespace has to be set once and must NOT be changed after a module was released)
namespace ExampleBlishhudModule
{
    [Export(typeof(Module))]
    public class CharacterKeybindModule : Module
    {
        private static readonly Logger Logger = Logger.GetLogger<CharacterKeybindModule>();

        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        public override IView GetSettingsView() => new SettingsWindow(settingsModel, DirectoriesManager);

        public CharacterKeybindsModel settingsModel;

        #region Views
        private AssignmentWindow moduleWindowView;
        #endregion

        [ImportingConstructor]
        public CharacterKeybindModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            ExampleModuleInstance = this;
        }

        protected override void DefineSettings(SettingCollection settings)
        {

            settingsModel = new CharacterKeybindsModel();
            settingsModel.Init(settings);
        }
        protected override async Task LoadAsync()
        {
            settingsModel.LoadCharactersAsync(Gw2ApiManager);


            var windowBackgroundTexture = AsyncTexture2D.FromAssetId(155997);

            moduleWindowView = new AssignmentWindow();
            moduleWindowView.Init(ContentsManager);


            // Create some UI
            await CreateGw2StyleWindowThatDisplaysAllCurrencies(windowBackgroundTexture);
            CreateWindowWithCharacterNames();
            CreateCornerIconWithContextMenu();
        }

        // Allows your module to run logic such as updating UI elements,
        // checking for conditions, playing audio, calculating changes, etc.
        // This method will block the primary Blish HUD loop, so any long
        // running tasks should be executed on a separate thread to prevent
        // slowing down the overlay.
        protected override void Update(GameTime gameTime)
        {
            _notificationRunningTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_notificationRunningTime > 60_000)
            {
                _notificationRunningTime = 0;
                // Show a notification in the middle of the screen
                ScreenNotification.ShowNotification("The examples module shows this message every 60 seconds!", ScreenNotification.NotificationType.Warning);
            }

            _updateCharactersRunningTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_updateCharactersRunningTime > 5_000)
            {
                _updateCharactersRunningTime = 0;
                // we use Task.Run here to prevent blocking the update loop with a possibly long running task
                Task.Run(GetCharacterNamesFromApiAndShowThemInLabel);
            }
        }

        // For a good module experience, your module should clean up ANY and ALL entities
        // and controls that were created and added to either the World or SpriteScreen.
        // Be sure to remove any tabs added to the Director window, CornerIcons, etc.
        protected override void Unload()
        {
            // it is best practise to unsubscribe from events. That is typically done inside of .Dispose() or Module.Unload().
            // Unsubscribing only works if you subscribed with a named method (e.g. += MyMethod;).
            // It doesnt work with lambda expressions (e.g. += () => 2+2;)
            // Not unsubscribing from events can result in the event subscriber (right side) being kept alive by the event publisher (left side).
            // This can lead to memory leaks and bugs where an object, that shouldnt exist aynmore,
            // still responds to events and is messing with your module.
            //_enumExampleSetting.SettingChanged -= UpdateCharacterWindowColor;

            // Unload() can be called on your module anytime. Even while it is currently loading and creating the objects.
            // Because of that you always have to check if the objects you want to access in Unload() are not null.
            // This can be done by using if null checks or by using the null-condition operator ?. (question mark with dot).
            _cornerIcon?.Dispose();
            _contextMenuStrip?.Dispose();
            _charactersFlowPanel?.Dispose(); // this will dispose the child labels we added as well
            _exampleWindow?.Dispose();
            // only .Dispose() textures you created yourself or loaded from your ref folder
            // NEVER .Dipose() textures from DatAssetCache because those textures are shared between modules and blish.
            _mugTexture?.Dispose(); 

            // All static members must be manually unset
            // Static members are not automatically cleared and will keep a reference to your,
            // module unless manually unset.
            ExampleModuleInstance = null;
        }
        
        private void CreateCornerIconWithContextMenu()
        {
            // Add a menu icon in the top left next to the other icons in guild wars 2 (e.g. inventory icon, Mail icon)
            // Priority: Determines the position relative to cornerIcons of other modules
            // because of that it MUST be set to a constant random value.
            // Do not recalculate this value on every module start up. Just use a constant value.
            // It has to be random to prevent that two modules use the same priority (e.g. "4") which would cause the cornerIcons to be in 
            // a different position on every startup.
            _cornerIcon = new CornerIcon()
            {
                Icon = _mugTexture,
                BasicTooltipText = $"My Corner Icon Tooltip for {Name}",
                Priority = 1645843523,
                Parent = GameService.Graphics.SpriteScreen
            };

            // Clicking on the cornerIcon shows/hides the example window
            _cornerIcon.Click += (s, e) => _exampleWindow.ToggleWindow();

            // Add a right click menu to the corner icon
            _contextMenuStrip = new ContextMenuStrip();
            _contextMenuStrip.AddMenuItem("A");
            var bMenuItem = _contextMenuStrip.AddMenuItem("B");
            var bSubMenuStrip = new ContextMenuStrip();
            bSubMenuStrip.AddMenuItem("B1");
            bSubMenuStrip.AddMenuItem("B2");
            bMenuItem.Submenu = bSubMenuStrip;
            _cornerIcon.Menu = _contextMenuStrip;
        }

        private void CreateWindowWithCharacterNames()
        {
            _charactersFlowPanel = new FlowPanel()
            {
                BackgroundColor = Color.Blue,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Location = new Point(200, 200),
                Parent = GameService.Graphics.SpriteScreen,
            };

            new Label() // this label is used as heading
            {
                Text = "My Characters:",
                TextColor = Color.Red,
                Font = GameService.Content.DefaultFont32,
                ShowShadow = true,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                //Location     = new Point(2, 0), // without a FlowPanel as parent, you can set the exact position inside the parent this way
                Parent = _charactersFlowPanel
            };

            _characterNamesLabel = new Label() // this label will be used to display the character names requested from the API
            {
                Text = "getting data from api...",
                TextColor = Color.DarkGray,
                Font = GameService.Content.DefaultFont32,
                ShowShadow = true,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                //Location     = new Point(2, 50), // without a FlowPanel as parent, you can set the exact position inside the parent this way
                Parent = _charactersFlowPanel
            };
        }

        private async Task GetCharacterNamesFromApiAndShowThemInLabel()
        {
            // Some API requests need an api key. e.g. for accessing account data like inventory or bank content.
            // Because of security reasons blish hud gives you an api subToken you can use instead of the real api key the user entered in blish.
            // Make sure that you added the api key permissions you need in the manifest.json.
            // Don't set them to '"optional": false' if you dont plan to handle that the user may disable certain permissions for your module.
            // e.g. the api request further down in this code needs the "characters" permission.
            // The api subToken may not be available when your module is loaded.
            // Because of that api requests, which require an api key, may fail when they are called in Initialize() or LoadAsync().
            // Or the user can delete the api key or add a new api key with the wrong permissions while your module is already running.
            // To handle those cases you could subscribe to Gw2ApiManager.SubtokenUpdated event (not shown here).
            // Nevertheless you should call Gw2ApiManager.HasPermissions() before every api request that requires an api key.
            var apiKeyPermissions = new List<TokenPermission>
            {
                TokenPermission.Account, // this permission can be used to check if your module got a token at all because every api key has this persmission.
                TokenPermission.Characters // this is the permission we actually require here to get the character names
            };

            if (!Gw2ApiManager.HasPermissions(apiKeyPermissions))
            {
                _characterNamesLabel.Text = "api permissions are missing or api sub token not available yet";
                return;
            }

            // even when the api request and api subToken are okay, the api requests can still fail for various reasons.
            // Examples are timeouts or the api is down or the api randomly responds with an error code instead of the correct response.
            // Because of that always use try catch when doing api requests to catch api request exceptions.
            // otherwise api request exceptions can crash your module and blish hud.
            IEnumerable<Character> charactersApiResponse = new List<Character>();
            try
            {
                // request characters endpoint from api
                charactersApiResponse = await Gw2ApiManager.Gw2ApiClient.V2.Characters.AllAsync();
            }
            catch (Exception e)
            {
                // Warning:
                // Blish Hud uses the tool Sentry in combination with the ErrorSubmissionModule to upload ERROR and FATAL log entries to a web server.
                // Because of that you must not use Logger.Error() or .Fatal() to log api response exceptions. Sometimes the GW2 api
                // can be down for up to a few days. That triggers a lot of api exceptions which would end up spamming the Sentry tool.
                // Instead use Logger.Info() or .Warn() if you want to log api response errors. Those do not get stored by the Sentry tool.
                // But you do not have to log api response exceptions. Just make sure that your module has no issues with failing api requests.
                Logger.Info("Failed to get character names from api.");
            }

            // extract character names from api response and show them inside a label
            var characterNames = charactersApiResponse.Select(c => c.Name).ToList();
            var characterNamesText = string.Join("\n", characterNames);
            _characterNamesLabel.Text = characterNamesText;
        }

        private async Task CreateGw2StyleWindowThatDisplaysAllCurrencies(AsyncTexture2D windowBackgroundTexture)
        {
            var apiKeyPermissions = new List<TokenPermission>
            {
                TokenPermission.Account, // this permission can be used to check if your module got a token at all because every api key has this persmission.
                TokenPermission.Characters // this is the permission we actually require here to get the character names
            };

            if (!Gw2ApiManager.HasPermissions(apiKeyPermissions))
            {
                //_characterNamesLabel.Text = "api permissions are missing or api sub token not available yet";
                return;
            }
            IEnumerable<Character> charactersApiResponse = new List<Character>();

            try
            {
                charactersApiResponse = await Gw2ApiManager.Gw2ApiClient.V2.Characters.AllAsync();
            }
            catch (Exception e)
            {
                Logger.Info($"Failed to get currencies from api.\n {e}");
            }

            // create a window with gw2 window style.
            _exampleWindow = new StandardWindow(
                windowBackgroundTexture,
                new Rectangle(25, 26, 560, 640),
                new Rectangle(40, 50, 540, 590))
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "Example Window Title",
                Emblem = _mugTexture,
                Subtitle = "Example Subtitle",
                Location = new Point(300, 300),
                SavesPosition = true,
                Id = $"{nameof(CharacterKeybindModule)}_My_Unique_ID_123" // Id has to be unique not only in your module but also within blish core and any other module
            };

            // add a panel to the window
            var currenciesFlowPanel = new FlowPanel
            {
                Title = "currencies",
                FlowDirection = ControlFlowDirection.LeftToRight,
                Width = 500,
                CanCollapse = true,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = _exampleWindow,
            };


/*			foreach (var professions in professionResponse)
            {
                var iconAssetId = int.Parse(Path.GetFileNameWithoutExtension(professions.Icon.Url.AbsoluteUri));
                var tooltipText = $"{professions.Name}";
                new Image(AsyncTexture2D.FromAssetId(iconAssetId))
                {
                    BasicTooltipText = tooltipText,
                    Size = new Point(40),
                    Parent = currenciesFlowPanel,
                };
            }*/
/*            foreach (var spezialisation in SpecializationApiResponse)
            {
                if (spezialisation.Elite)
                {
                    var iconAssetId = int.Parse(Path.GetFileNameWithoutExtension(spezialisation.Background.Url.AbsoluteUri));
                    var tooltipText = $"{spezialisation.Name}";
                    new Image(AsyncTexture2D.FromAssetId(iconAssetId))
                    {
                        BasicTooltipText = tooltipText,
                        Width = 300,
                        Height = 40,
                        Parent = currenciesFlowPanel,
                    };
                }

            }*/

/*            ObservableCollection<string> characternames = new ObservableCollection<string>(charactersApiResponse.Select(c => c.Name).ToList());
            var dropdown = new Dropdown()
            {
                Parent = currenciesFlowPanel,
                Height = 70
            };
            charactersApiResponse.Select(c => c.Name).ToList().ForEach(e => dropdown.Items.Add(e));*/

            _exampleWindow.Show();
        }

        internal static CharacterKeybindModule ExampleModuleInstance;
        private SettingEntry<bool> _boolExampleSetting;
        private SettingEntry<int> _valueRangeExampleSetting;
        private SettingEntry<int> _hiddenIntExampleSetting;
        private SettingEntry<int> _hiddenIntExampleSetting2;
        private SettingEntry<string> _stringExampleSetting;
        private SettingCollection _internalExampleSettingSubCollection;
        private Texture2D _mugTexture;
        private CornerIcon _cornerIcon;
        private ContextMenuStrip _contextMenuStrip;
        private Label _characterNamesLabel;
        private FlowPanel _charactersFlowPanel;
        private StandardWindow _exampleWindow;
        private double _notificationRunningTime;
        private double _updateCharactersRunningTime;
    }
}
