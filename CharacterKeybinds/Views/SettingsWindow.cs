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

        internal DirectoriesManager DirectoriesManager;

        private StandardButton setupButton, restoreButton, reportBugButton;

        public SettingsWindow(CharacterKeybindsModel model, DirectoriesManager DirectoriesManager) 
        {
            this.model = model;
            this.DirectoriesManager = DirectoriesManager;
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

            setupButton = new StandardButton
            {
                Parent = buttonFlowPanel,
                Text = "Setup",
                Location = new Microsoft.Xna.Framework.Point(100, 0)
            };
            restoreButton = new StandardButton
            {
                Parent = buttonFlowPanel,
                Text = "Restore",
                Location = new Microsoft.Xna.Framework.Point(100, 0)
            };

            reportBugButton = new StandardButton
            {
                Parent = _settingFlowPanel,
                Text = "Report a Bug",
                Location = new Microsoft.Xna.Framework.Point(100, 0)
            };

            setupButton.Click += SetupButton_Click;
			restoreButton.Click += RestoreButton_Click;
            reportBugButton.Click += ReportBugButton_Click;

        }

		private void RestoreButton_Click(object sender, MouseEventArgs e)
		{
            var confirmationWindow = new ConfirmationWindow("Do you really want to restore your old keybinds?",
                onConfirm: () => CopyKeybindFiles(model.gw2KeybindsFolder.Value, DirectoriesManager.GetFullDirectoryPath("keybind_storage")));

            confirmationWindow.Show();
        }

        private void SetupButton_Click(object sender, MouseEventArgs e)
		{
            CopyKeybindFiles(model.gw2KeybindsFolder.Value, DirectoriesManager.GetFullDirectoryPath("keybind_storage"));
		}

		private void ReportBugButton_Click(object sender, MouseEventArgs e)
		{
            System.Diagnostics.Process.Start("https://github.com/flakysalt/Blish-HUD-CharacterKeybinds/issues");
        }

        void CopyKeybindFiles(string sourcePath, string outputPath) 
        {
            string[] xmlFiles = Directory.GetFiles(sourcePath, "*.xml");

            foreach (string file in xmlFiles)
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(outputPath, fileName);
                File.Copy(file, destFile, true); // The 'true' parameter allows overwriting existing files
            }
        }
		protected override void Unload()
		{
            setupButton.Click -= SetupButton_Click;
            restoreButton.Click -= RestoreButton_Click;
            reportBugButton.Click -= ReportBugButton_Click;
            base.Unload();
		}
	}
}
