using System;
using System.Collections.Generic;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using flakysalt.CharacterKeybinds.Resources;
using Microsoft.Xna.Framework;

namespace flakysalt.CharacterKeybinds.Views
{
    public class KeybindMigrationTab : View 
    {
        private FlowPanel mainFlowPanel;
        private Checkbox confirmationCheckbox;
        private StandardButton startMigrtionButton,deleteOldDataButton;

        private Label resultLabel, explinationLabel;
        
        public EventHandler OnMigrateClicked;
        public EventHandler OnDeleteClicked;

        protected override void Build(Container buildPanel)
        {
            mainFlowPanel = new FlowPanel
            {
                ControlPadding = new Vector2(0, 10),
                HeightSizingMode = SizingMode.Fill,
                Width = buildPanel.Width,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = buildPanel,
            };
            
            new Label
            {
                Parent = mainFlowPanel,
                Width = mainFlowPanel.Width,
                Text = Loca.migration,
                Font = GameService.Content.DefaultFont18
            };
            
            explinationLabel = new Label
            {
                Parent = mainFlowPanel,
                Width = mainFlowPanel.Width,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Text = "This tab allows you to migrate your keybindings from the old version of the module to the new version.\n" +
                       "Some specialization might no be translated perfectly so please check after the process if your keybinds look fine.\n\n" +
                       "Dont worry, you will not lose your old keybind data in this process. You can delete it afterwards if you want to.\n" +
                       "(This will probably be the only time you have to do this!)"
            };
            startMigrtionButton = new StandardButton
            {
                Parent = mainFlowPanel,
                Width = mainFlowPanel.Width,
                Text = "Start Migration",
                BasicTooltipText = "Starts the migration process from the old data to the new data format."
            };
            
            confirmationCheckbox = new Checkbox()
            {
                Padding = new Thickness(50,0,0,0),
                Parent = mainFlowPanel,
                Width = mainFlowPanel.Width,
                Text = "I understand deleting old data is irreversible.",
                Checked = false
            };
            
            deleteOldDataButton = new StandardButton()
            {
                Parent = mainFlowPanel,
                Width = mainFlowPanel.Width,
                Text = "Delete Old Keybind Data",
                BasicTooltipText = "Deletes the old keybind data from the previous version of the module.",
                Enabled = confirmationCheckbox.Checked
            };
            
            resultLabel = new Label
            {
                Parent = mainFlowPanel,
                Width = mainFlowPanel.Width,
                Text = "",
                AutoSizeHeight = true,
                AutoSizeWidth = true
            };
            
            confirmationCheckbox.CheckedChanged += (s, e) =>
            {
                deleteOldDataButton.Enabled = confirmationCheckbox.Checked;
            };
            
                
            startMigrtionButton.Click += (sender, args) => OnMigrateClicked?.Invoke(sender, args);
            deleteOldDataButton.Click += (sender, args) => OnDeleteClicked?.Invoke(sender, args);
            
            base.Build(buildPanel);
        }
        public void SetDeletionText()
        {
            resultLabel.Text = "Old Data Deleted Successfully.";
            resultLabel.TextColor = Color.LimeGreen;
        }

        public void SetMigrationResult(List<string> result)
        {
            resultLabel.Text = "";
            if(result.Count == 0)
            {
                resultLabel.Text = "Migration completed successfully with no issues.";
                resultLabel.TextColor = Color.LimeGreen;
            }
            else
            {
                resultLabel.TextColor = Color.OrangeRed;
                resultLabel.Text = "Problems found during migration. Please check the following specialcations manually:\n";

                foreach (var VARIABLE in result)
                {
                    resultLabel.Text += VARIABLE + "\n";
                }
            }
        }
    }
}