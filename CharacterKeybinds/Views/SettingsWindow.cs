using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Settings.UI.Views;
using Microsoft.Xna.Framework;
using flakysalt.CharacterKeybinds.Model;

using System.Linq;
using Blish_HUD.Modules.Managers;

namespace flakysalt.CharacterKeybinds.Views
{
    public class SettingsWindow : View
	{
        private CharacterKeybindsSettings model;
        private CharacterKeybindsTab characterKeybindWindow;
        private Autoclicker troubleshootWindow;

        private FlowPanel _settingFlowPanel;
        private ViewContainer _lastSettingContainer;
        private StandardButton reportBugButton, fairMacroUseButton;

        private StandardButton characterKeybindSettinsButton, openTroubleshootWindowButton, faqButton;
        
        public SettingsWindow(CharacterKeybindsSettings model, CharacterKeybindsTab assignmentWindow, Autoclicker autoclickView)
		{
            this.model = model;

            this.characterKeybindWindow = assignmentWindow;
            this.troubleshootWindow = autoclickView;
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

            var topButtonPanel = new FlowPanel()
            {
                Width = _settingFlowPanel.Width,
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(5, 2),
                HeightSizingMode = SizingMode.AutoSize,
                AutoSizePadding = new Point(0, 15),
                Parent = _settingFlowPanel,
            };

            characterKeybindSettinsButton = new StandardButton
            {
                Parent = topButtonPanel,
                Left = 10,
                Size = new Point(200, 50),
                Text = "Keybind Settings"
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
            var bottombuttonFlowPanel = new FlowPanel()
            {
                Width = _settingFlowPanel.Width,
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(5, 2),
                HeightSizingMode = SizingMode.AutoSize,
                AutoSizePadding = new Point(0, 15),
                Parent = _settingFlowPanel,
            };

            faqButton = new StandardButton
            {
                Parent = bottombuttonFlowPanel,
                Size = new Point(200, 30),
                Text = "Help / FAQ"
            };

            reportBugButton = new StandardButton
            {
                Parent = bottombuttonFlowPanel,
                Size = new Point(200, 30),
                Text = "Report a Bug"
            };
            fairMacroUseButton = new StandardButton
            {
                Parent = bottombuttonFlowPanel,
                Left = 10,
                Size = new Point(200, 30),
                Text = "Arenanet Macro Policy"
            };

            openTroubleshootWindowButton = new StandardButton
            {
                Parent = _settingFlowPanel,
                Size = new Point(200, 30),
                Text = "Troubleshoot"
            };

			faqButton.Click += FaqButton_Click;
            reportBugButton.Click += ReportBugButton_Click;
			fairMacroUseButton.Click += FairMacroUseButton_Click;
			characterKeybindSettinsButton.Click += OpenCharacterKeybindsSettingButton_Click;
            openTroubleshootWindowButton.Click += OpenTroubleshootWindowButton_Click;
        }

		private void FaqButton_Click(object sender, MouseEventArgs e)
		{
            System.Diagnostics.Process.Start("https://blishhud.com/modules/?module=flakysalt.CharacterKeybinds");
        }

        private void OpenCharacterKeybindsSettingButton_Click(object sender, MouseEventArgs e)
		{
            characterKeybindWindow?.Show();
        }

		private void OpenTroubleshootWindowButton_Click(object sender, MouseEventArgs e)
		{
            troubleshootWindow?.WindowView.Show();
        }

		private void ReportBugButton_Click(object sender, MouseEventArgs e)
		{
            System.Diagnostics.Process.Start("https://github.com/flakysalt/Blish-HUD-CharacterKeybinds/issues");
        }
        private void FairMacroUseButton_Click(object sender, MouseEventArgs e)
        {
            System.Diagnostics.Process.Start("https://help.guildwars2.com/hc/en-us/articles/360013762153-Policy-Macros-and-Macro-Use");
        }

        protected override void Unload()
		{
            reportBugButton.Click -= ReportBugButton_Click;
            fairMacroUseButton.Click -= FairMacroUseButton_Click;

            base.Unload();
		}
	}
}
