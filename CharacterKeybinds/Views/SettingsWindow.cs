using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Settings.UI.Views;
using Microsoft.Xna.Framework;
using flakysalt.CharacterKeybinds.Data;
using System.Linq;
using System.IO;
using Blish_HUD.Modules.Managers;
using flakysalt.CharacterKeybinds.Util;
using System;

namespace flakysalt.CharacterKeybinds.Views
{
    public class SettingsWindow : View
	{
        private CharacterKeybindsSettings model;
        private AssignmentWindow assignmentWindow;
        private AutoclickView autoClickWindow;

        private FlowPanel _settingFlowPanel;
        private ViewContainer _lastSettingContainer;
        private StandardButton reportBugButton, fairMacroUseButton;

        private StandardButton openAssignmentWindowButton, openAutoClickerSettingsWindowButton;

        DirectoriesManager directoriesManager;
        Blish_HUD.Logger logger;


        public SettingsWindow(CharacterKeybindsSettings model, AssignmentWindow assignmentWindow, AutoclickView autoclickView, DirectoriesManager directoriesManager, Blish_HUD.Logger logger)
		{
            this.model = model;

            this.assignmentWindow = assignmentWindow;
            this.autoClickWindow = autoclickView;

            this.directoriesManager = directoriesManager;
            this.logger = logger;
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

            openAssignmentWindowButton = new StandardButton
            {
                Parent = _settingFlowPanel,
                Left = 10,
                Size = new Point(200, 30),
                Text = "Settings"
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
                Width = _settingFlowPanel.Width,
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(5, 2),
                HeightSizingMode = SizingMode.AutoSize,
                AutoSizePadding = new Point(0, 15),
                Parent = _settingFlowPanel
            };

            reportBugButton = new StandardButton
            {
                Parent = _settingFlowPanel,
                Size = new Point(200, 30),
                Text = "Report a Bug"
            };
            fairMacroUseButton = new StandardButton
            {
                Parent = _settingFlowPanel,
                Left = 10,
                Size = new Point(200, 30),
                Text = "Arenanet Macro Policy"
            };

            openAutoClickerSettingsWindowButton = new StandardButton
            {
                Parent = _settingFlowPanel,
                Size = new Point(100, 30),
                Text = "Debug"
            };

            ImportLegacyKeybinds();

            reportBugButton.Click += ReportBugButton_Click;
			fairMacroUseButton.Click += FairMacroUseButton_Click;

			openAssignmentWindowButton.Click += OpenAssignmentWindowButton_Click;

            openAutoClickerSettingsWindowButton.Click += OpenAutoClickerSettingsWindowButton_Click;
        }

		private void OpenAssignmentWindowButton_Click(object sender, MouseEventArgs e)
		{
            assignmentWindow?.AssignmentView.Show();
        }

		private void OpenAutoClickerSettingsWindowButton_Click(object sender, MouseEventArgs e)
		{
            autoClickWindow?.AutoClickWindow.Show();

        }

		private void ImportLegacyKeybinds()
        {
            if (File.Exists(Path.Combine(directoriesManager.GetFullDirectoryPath("keybind_storage"), "characterMap.json"))) 
            {
                try
                {
                    string loadJson = File.ReadAllText(Path.Combine(directoriesManager.GetFullDirectoryPath("keybind_storage"), "characterMap.json"));
                    var characterSpecializations = CharacterKeybindJsonUtil.DeserializeCharacterList(loadJson);

                    model.characterKeybinds.Value = characterSpecializations;

                    File.Delete(Path.Combine(directoriesManager.GetFullDirectoryPath("keybind_storage"), "characterMap.json"));
                }
                catch (Exception e)
                {
                    logger.Error($"Could not import legacy bindings. \n {e}");
                }
            }
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
