using Blish_HUD.Controls;
using flakysalt.CharacterKeybinds.Data;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace flakysalt.CharacterKeybinds.Views.UiElements
{
    public class KeymapEventArgs : EventArgs
    {
        public Keymap oldKeymap;
        public Keymap newKeymap;
    }

    public class KeybindFlowContainer : FlowPanel
    {
        public StandardButton removeButton { get; private set; }
        public StandardButton applyButton { get; private set; }
        public Image professionImage { get; private set; }
        public Dropdown characterNameDropdown { get; private set; }
        public Dropdown specializationDropdown { get; private set; }
        public Dropdown keymapDropdown { get; private set; }

        private string defaultCharacterEntry = "Select Character";
        private string defaultKeybindsEntry = "Keybinds";
        private string defaultSpecializationEntry = "Specialization";

        private string coreSpecialization = "Core";
        private string wildcardSpecialization = "All Specialization";

        public delegate void MyHandler(object p1, object p2);


        public event EventHandler<Keymap> OnApply;

        public event EventHandler<KeymapEventArgs> OnDataChanged;

        public event EventHandler<Keymap> OnRemove;

        public Keymap oldKeymap;

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

        private void SetDropdownOptions(Dropdown dropdown, List<string> options)
        {
            dropdown.Items.Clear();

            foreach (var option in options)
            {
                dropdown.Items.Add(option);
            }
            dropdown.Enabled = options.Count > 0;
            removeButton.Enabled = options.Count > 0;
        }

        public KeybindFlowContainer()
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
                Size = new Point(120, 30),
            };

            specializationDropdown = new Dropdown
            {
                Parent = this,
                Size = new Point(120, 30),
            };
            specializationDropdown.Items.Add(wildcardSpecialization);
            specializationDropdown.Items.Add(coreSpecialization);

            keymapDropdown = new Dropdown
            {
                Parent = this,
                Size = new Point(120, 30),
            };

            removeButton = new StandardButton
            {
                Parent = this,
                Text = "Delete",
                Size = new Point(70, 30),
            };
            applyButton = new StandardButton
            {
                Parent = this,
                Text = "Apply",
                Size = new Point(60, 30),
                Enabled = true,
            };

            keymapDropdown.ValueChanged += (e, v) =>
            {
                applyButton.Enabled = keymapDropdown.SelectedItem != defaultKeybindsEntry;

                OnDataChanged?.Invoke(this, GetKeymapArgs());
                this.oldKeymap = GetKeymap();
            };

            specializationDropdown.ValueChanged += (e, v) =>
            {
                OnDataChanged?.Invoke(this, GetKeymapArgs());
                this.oldKeymap = GetKeymap();
            };

            characterNameDropdown.ValueChanged += (e, v) =>
            {
                specializationDropdown.SelectedItem = defaultSpecializationEntry;

                OnDataChanged?.Invoke(this, GetKeymapArgs());
                this.oldKeymap = GetKeymap();
            };

            applyButton.Click += (o, eventArgs) =>
            {
                OnApply?.Invoke(o, GetKeymap());
            };
            removeButton.Click += (o, eventArgs) =>
            {
                OnRemove?.Invoke(0, GetKeymap());
                this.Dispose();
            };
        }

        public void SetDropdownContent(Dropdown dropdown, List<string> values)
        {
            values.ForEach(e => dropdown.Items.Add(e));
        }

        public void SetValues(Keymap keymap)
        {
            oldKeymap = keymap;
            characterNameDropdown.SelectedItem = string.IsNullOrEmpty(keymap.characterName) ? defaultCharacterEntry : keymap.characterName;
            specializationDropdown.SelectedItem = string.IsNullOrEmpty(keymap.specializationName) ? defaultSpecializationEntry : keymap.specializationName;
            keymapDropdown.SelectedItem = string.IsNullOrEmpty(keymap.keymapName) ? defaultKeybindsEntry : keymap.keymapName;
        }

        public void AttachListeners(EventHandler<Keymap> OnApplyAction,
            EventHandler<KeymapEventArgs> OnDataChanged,
            EventHandler<Keymap> OnDeleteAction) 
        {
            this.OnDataChanged += OnDataChanged;
            this.OnApply += OnApplyAction;
            this.OnRemove += OnDeleteAction;

            removeButton.Click += (o, eventArgs) =>
            {
                this.OnDataChanged -= OnDataChanged;
                this.OnApply -= OnApplyAction;
                this.OnRemove -= OnDeleteAction;
            };
        }

        KeymapEventArgs GetKeymapArgs()
        {
            return new KeymapEventArgs
            {
                newKeymap = GetKeymap(),
                oldKeymap = this.oldKeymap
            };
        }

        Keymap GetKeymap() 
        {
            return new Keymap
            {
                characterName = characterNameDropdown.SelectedItem == defaultCharacterEntry ? null : characterNameDropdown.SelectedItem,
                specializationName = specializationDropdown.SelectedItem == defaultSpecializationEntry ? null : specializationDropdown.SelectedItem,
                keymapName = keymapDropdown.SelectedItem == defaultKeybindsEntry ? null : keymapDropdown.SelectedItem,
            };
        }

	}
}
