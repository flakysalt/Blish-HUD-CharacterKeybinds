using Blish_HUD.Controls;
using flakysalt.CharacterKeybinds.Data;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Blish_HUD.Content;

namespace flakysalt.CharacterKeybinds.Views.UiElements
{
    public class KeymapEventArgs : EventArgs
    {
        public Keymap oldKeymap;
        public Keymap newKeymap;
    }

    public class KeybindFlowContainer : FlowPanel
    {
        private StandardButton removeButton { get;}
        private StandardButton applyButton { get; }
        private Image professionImage { get; }
        public Dropdown characterNameDropdown { get; }
        public Dropdown specializationDropdown { get; }
        public Dropdown keymapDropdown { get; }

        private string defaultCharacterEntry = "Select Character";
        private string defaultKeybindsEntry = "Keybinds";
        private string defaultSpecializationEntry = "Specialization";

        private string coreSpecialization = "Core";
        private string wildcardSpecialization = "All Specialization";

        public event EventHandler<Keymap> OnApply;
        public event EventHandler<KeymapEventArgs> OnDataChanged;
        public event EventHandler<Keymap> OnRemove;

        private Keymap oldKeymap;

        public KeybindFlowContainer()
        {
            OuterControlPadding = new Vector2(10,0);
            ControlPadding = new Vector2(2,0);
            Height = 45;
            FlowDirection = ControlFlowDirection.LeftToRight;
            
            professionImage = new Image
            {
                Parent = this,
                Size = new Point(30, 30),
            };

            characterNameDropdown = new Dropdown
            {
                Height = 30,
                Parent = this,
                Width = 120
            };

            specializationDropdown = new Dropdown
            {
                Height = 30,
                Parent = this,
                Width = 120
            };
            specializationDropdown.Items.Add(wildcardSpecialization);
            specializationDropdown.Items.Add(coreSpecialization);

            keymapDropdown = new Dropdown
            {
                Height = 30,
                Parent = this,
                Width = 120
            };
            
            applyButton = new StandardButton
            {
                Parent = this,
                Text = "Apply",
                Size = new Point(60, 30),
            };
            
            removeButton = new StandardButton
            {
                Parent = this,
                Text = "Delete",
                Size = new Point(60, 30),
                
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

        public void SetProfessionIcon(int iconId)
        {
            professionImage.Texture = AsyncTexture2D.FromAssetId(iconId);
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
