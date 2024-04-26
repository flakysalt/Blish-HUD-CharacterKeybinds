using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Settings.UI.Views;
using Microsoft.Xna.Framework;
using flakysalt.CharacterKeybinds.Data;
using System.Linq;
using System.IO;

namespace flakysalt.CharacterKeybinds.Views
{
    public class SettingsWindow : View
	{
        private CharacterKeybindsModel model;

        private Label ErrorLabel;
        private FlowPanel _settingFlowPanel;
        private ViewContainer _lastSettingContainer;
        private StandardButton reportBugButton, fairMacroUseButton;

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
                Size = new Point(200, 50),
                Text = "Report a Bug"
            };
            fairMacroUseButton = new StandardButton
            {
                Parent = _settingFlowPanel,
                Left = 10,
                Size = new Point(200, 50),
                Text = "Arenanet Macro Policy"
            };
            ErrorLabel = new Label
            {
                Parent = _settingFlowPanel,
                TextColor = Color.Red,
                Size = new Point(buildPanel.Width, 50),
                Text = "Error: Selected Keybindsfolder does not exist!",
                Visible = !Directory.Exists(model.gw2KeybindsFolder.Value)
            };

			model.gw2KeybindsFolder.PropertyChanged += Gw2KeybindsFolder_PropertyChanged;
            reportBugButton.Click += ReportBugButton_Click;
			fairMacroUseButton.Click += FairMacroUseButton_Click;
        }

		private void Gw2KeybindsFolder_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
            ErrorLabel.Visible = !Directory.Exists(model.gw2KeybindsFolder.Value);
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
            model.gw2KeybindsFolder.PropertyChanged -= Gw2KeybindsFolder_PropertyChanged;

            base.Unload();
		}
	}
}
