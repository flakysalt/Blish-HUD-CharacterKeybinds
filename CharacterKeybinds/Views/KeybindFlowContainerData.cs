using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CharacterKeybinds.Views
{
	class KeybindFlowContainerData : FlowPanel
	{
        public StandardButton removeButton { get; private set; }
        public Image professionImage { get; private set; }
		public Dropdown characterNameDropdown { get; private set; }
		public Dropdown specializationDropdown { get; private set; }
        public Dropdown keymapDropdown { get; private set; }

        public void SetKeymapOptions(List<string> options) 
        {
            SetDropdownOptions(keymapDropdown, options);

        }
        public void SetSpecializationOptions(List<string> options)
        {
            SetDropdownOptions(specializationDropdown, options);

        }
        public void SetNameOptions(List<string> options)
        {
            SetDropdownOptions(characterNameDropdown, options);
        }

        private void SetDropdownOptions(Dropdown dropdown,List<string> options)
        {
            dropdown.Items.Clear();

            foreach (var option in options)
            {
                dropdown.Items.Add(option);
            }
            dropdown.Enabled = options.Count > 0;
            removeButton.Enabled = options.Count > 0;
            //dropdown.SelectedItem = string.IsNullOrEmpty(selectedOption) ? defaultOptionPlaceholder : selectedOption;
        }

        public KeybindFlowContainerData(string selectedCharacter = "",
            string selectedSpezialisations = "",
            string selectedKeymap = "")
        {
            
            FlowDirection = ControlFlowDirection.LeftToRight;
            professionImage = new Image
            {
                Parent = this,
                Size = new Point(30, 30)
            };

            characterNameDropdown = new Dropdown
            {
                Parent = this,
                Size = new Point(130, 30),
                Enabled = false
            };
            characterNameDropdown.SelectedItem = string.IsNullOrEmpty(selectedCharacter) ? "Select Character": selectedCharacter;

			characterNameDropdown.PropertyChanged += (e,v)=> 
            {
                specializationDropdown.Enabled = characterNameDropdown.SelectedItem != "Select Character" && characterNameDropdown.Items.Count > 0;
            };


            specializationDropdown = new Dropdown
            {
                Parent = this,
                Size = new Point(130, 30),
                Enabled = false
            };
            specializationDropdown.SelectedItem = string.IsNullOrEmpty(selectedSpezialisations) ? "Specialization" : selectedSpezialisations;


            keymapDropdown = new Dropdown
            {
                Parent = this,
                Size = new Point(130, 30),
                Enabled = false
            };
            keymapDropdown.SelectedItem = string.IsNullOrEmpty(selectedKeymap) ? "Keybinds": selectedKeymap;

            removeButton = new StandardButton
            {
                Parent = this,
                Text = "Delete",
                Size = new Point(70, 30),
                Enabled = false
            };

            removeButton.Click += (o, eventArgs) => {
                this.Dispose();
            };
        }

	}
}
