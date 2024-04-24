using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Settings.UI.Views;
using Microsoft.Xna.Framework;
using flakysalt.CharacterKeybinds.Model;
using System.Linq;
using Blish_HUD.Modules.Managers;
using System.IO;
using flakysalt.CharacterKeybinds.Util;


namespace flakysalt.CharacterKeybinds.Views
{
	class SettingsWindow : View
	{
        private CharacterKeybindsModel model;

        private FlowPanel _settingFlowPanel;

        private ViewContainer _lastSettingContainer;

        private StandardButton reportBugButton;

        public SettingsWindow(CharacterKeybindsModel model) 
        {
            this.model = model;
        }

        protected override void Build(Container buildPanel)
		{
            _settingFlowPanel = new FlowPanel()
            {
                Size = buildPanel.Size,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(5, 2),
                OuterControlPadding = new Vector2(10, 15),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                AutoSizePadding = new Point(0, 15),
                Parent = buildPanel
            };

            foreach (var setting in model.settingsCollection.Where(s => s.SessionDefined))
            {
                IView settingView;

                if ((settingView = SettingView.FromType(setting, _settingFlowPanel.Width)) != null)
                {
                    _lastSettingContainer = new ViewContainer()
                    {
                        WidthSizingMode = SizingMode.AutoSize,
                        HeightSizingMode = SizingMode.AutoSize,
                        Parent = _settingFlowPanel
                    };

                    _lastSettingContainer.Show(settingView);

                    if (settingView is SettingsView subSettingsView)
                    {
                        subSettingsView.LockBounds = false;
                    }
                }
            }
            var buttonFlowPanel = new FlowPanel()
            {
                Size = buildPanel.Size,
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(5, 2),
                OuterControlPadding = new Vector2(10, 15),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                AutoSizePadding = new Point(0, 15),
                Parent = _settingFlowPanel
            };

            reportBugButton = new StandardButton
            {
                Parent = _settingFlowPanel,
                Text = "Report a Bug",
                Location = new Microsoft.Xna.Framework.Point(100, 0)
            };

            reportBugButton.Click += ReportBugButton_Click;

        }

		private void ReportBugButton_Click(object sender, MouseEventArgs e)
		{
            System.Diagnostics.Process.Start("https://github.com/flakysalt/Blish-HUD-CharacterKeybinds/issues");
        }

		protected override void Unload()
		{
            reportBugButton.Click -= ReportBugButton_Click;
            base.Unload();
		}
	}
}
