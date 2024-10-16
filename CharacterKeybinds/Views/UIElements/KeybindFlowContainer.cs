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
        public CharacterKeybind OldCharacterKeybind;
        public CharacterKeybind NewCharacterKeybind;
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

        public event EventHandler<CharacterKeybind> OnApply;
        public event EventHandler<KeymapEventArgs> OnDataChanged;
        public event EventHandler<CharacterKeybind> OnRemove;

        private CharacterKeybind _oldCharacterKeybind;

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
                this._oldCharacterKeybind = GetKeymap();
            };

            specializationDropdown.ValueChanged += (e, v) =>
            {
                OnDataChanged?.Invoke(this, GetKeymapArgs());
                this._oldCharacterKeybind = GetKeymap();
            };

            characterNameDropdown.ValueChanged += (e, v) =>
            {
                specializationDropdown.SelectedItem = defaultSpecializationEntry;

                OnDataChanged?.Invoke(this, GetKeymapArgs());
                this._oldCharacterKeybind = GetKeymap();
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

        public void SetValues(CharacterKeybind characterKeybind)
        {
            _oldCharacterKeybind = characterKeybind;
            characterNameDropdown.SelectedItem = string.IsNullOrEmpty(characterKeybind.characterName) ? defaultCharacterEntry : characterKeybind.characterName;
            specializationDropdown.SelectedItem = string.IsNullOrEmpty(characterKeybind.spezialisation) ? defaultSpecializationEntry : characterKeybind.spezialisation;
            keymapDropdown.SelectedItem = string.IsNullOrEmpty(characterKeybind.keymap) ? defaultKeybindsEntry : characterKeybind.keymap;
        }

        public void SetProfessionIcon(int iconId)
        {
            professionImage.Texture = AsyncTexture2D.FromAssetId(iconId);
        }

        public void AttachListeners(EventHandler<CharacterKeybind> OnApplyAction,
            EventHandler<KeymapEventArgs> OnDataChanged,
            EventHandler<CharacterKeybind> OnDeleteAction) 
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
                NewCharacterKeybind = GetKeymap(),
                OldCharacterKeybind = this._oldCharacterKeybind
            };
        }

        CharacterKeybind GetKeymap() 
        {
            return new CharacterKeybind
            {
                characterName = characterNameDropdown.SelectedItem == defaultCharacterEntry ? null : characterNameDropdown.SelectedItem,
                spezialisation = specializationDropdown.SelectedItem == defaultSpecializationEntry ? null : specializationDropdown.SelectedItem,
                keymap = keymapDropdown.SelectedItem == defaultKeybindsEntry ? null : keymapDropdown.SelectedItem,
            };
        }

	}
}
