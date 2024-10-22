using Blish_HUD.Controls;
using flakysalt.CharacterKeybinds.Data;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Content;
using Blish_HUD.Input;

namespace flakysalt.CharacterKeybinds.Views.UiElements
{
    public class KeymapEventArgs : EventArgs
    {
        public Keymap OldCharacterKeymap;
        public Keymap NewCharacterKeymap;
    }

    public class KeybindFlowContainer : FlowPanel
    {
        private StandardButton RemoveButton { get;}
        private StandardButton ApplyButton { get; }
        private Image ProfessionImage { get; }
        public Dropdown CharacterNameDropdown { get; }
        public Dropdown SpecializationDropdown { get; }
        public Dropdown KeymapDropdown { get; }

        private readonly string _defaultCharacterEntry = "Select Character";
        private readonly string _defaultKeybindsEntry = "Keybinds";
        private readonly string _defaultSpecializationEntry = "Specialization";

        private const string _coreSpecialization = "Core";
        private const string _wildcardSpecialization = "All Specialization";

        public event EventHandler<Keymap> OnApply;
        public event EventHandler<KeymapEventArgs> OnDataChanged;
        public event EventHandler<Keymap> OnRemove;

        private Keymap _oldCharacterKeymap;
        
        private List<LocalizedSpecialization> _localizedSpecializations;

        public KeybindFlowContainer()
        {
            OuterControlPadding = new Vector2(10,0);
            ControlPadding = new Vector2(2,0);
            Height = 45;
            FlowDirection = ControlFlowDirection.LeftToRight;
            
            ProfessionImage = new Image
            {
                Parent = this,
                Size = new Point(30, 30),
            };

            CharacterNameDropdown = new Dropdown
            {
                Height = 30,
                Parent = this,
                Width = 120
            };

            SpecializationDropdown = new Dropdown
            {
                Height = 30,
                Parent = this,
                Width = 120
            };
            SpecializationDropdown.Items.Add(_wildcardSpecialization);
            SpecializationDropdown.Items.Add(_coreSpecialization);

            KeymapDropdown = new Dropdown
            {
                Height = 30,
                Parent = this,
                Width = 120
            };
            
            ApplyButton = new StandardButton
            {
                Parent = this,
                Text = "Apply",
                Size = new Point(60, 30),
            };
            
            RemoveButton = new StandardButton
            {
                Parent = this,
                Text = "Delete",
                Size = new Point(60, 30),
                
            };
            
            KeymapDropdown.ValueChanged += OnKeymapChanged;
            SpecializationDropdown.ValueChanged += OnSpecializationChanged;
            CharacterNameDropdown.ValueChanged += OnCharacterChanged;
            ApplyButton.Click += OnApplyClick;
            RemoveButton.Click += OnRemoveClick;
        }

        public void SetDropdownContent(Dropdown dropdown, List<string> values)
        {
            values.ForEach(e => dropdown.Items.Add(e));
        }
        public void SetSpecializationContent(List<LocalizedSpecialization> values)
        {
            _localizedSpecializations = values;
            values.ForEach(e => SpecializationDropdown.Items.Add(e.displayName));
        }

        public void SetValues(Keymap keymap)
        {
            _oldCharacterKeymap = keymap;
            CharacterNameDropdown.SelectedItem = string.IsNullOrEmpty(keymap.CharacterName) ? _defaultCharacterEntry : keymap.CharacterName;
            
            switch (keymap.SpecialisationId)
            {
                case 0:
                    SpecializationDropdown.SelectedItem = _defaultSpecializationEntry;
                    break;
                case Keymap.CoreSpecializationId:
                    SpecializationDropdown.SelectedItem = _coreSpecialization;
                    break;
                case Keymap.AllSpecializationId:
                    SpecializationDropdown.SelectedItem = _wildcardSpecialization;
                    break;
                default: 
                    SpecializationDropdown.SelectedItem = _localizedSpecializations.FirstOrDefault( e=> e.id == keymap.SpecialisationId)?.displayName;
                    break;
            }
            
            KeymapDropdown.SelectedItem = string.IsNullOrEmpty(keymap.KeymapName) ? _defaultKeybindsEntry : keymap.KeymapName;
        }

        public void SetProfessionIcon(int iconId)
        {
            ProfessionImage.Texture = AsyncTexture2D.FromAssetId(iconId);
        }

        public void AttachListeners(EventHandler<Keymap> OnApplyAction,
            EventHandler<KeymapEventArgs> OnDataChanged,
            EventHandler<Keymap> OnDeleteAction) 
        {
            this.OnDataChanged += OnDataChanged;
            this.OnApply += OnApplyAction;
            this.OnRemove += OnDeleteAction;

            RemoveButton.Click += (o, eventArgs) =>
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
                NewCharacterKeymap = GetKeymap(),
                OldCharacterKeymap = this._oldCharacterKeymap
            };
        }

        Keymap GetKeymap()
        {
            int specialisationId = 0;

            if (SpecializationDropdown.SelectedItem != _defaultSpecializationEntry)
            {
                switch (SpecializationDropdown.SelectedItem)
                {
                    case _coreSpecialization:
                        specialisationId = Keymap.CoreSpecializationId;
                        break;
                    case _wildcardSpecialization:
                        specialisationId = Keymap.AllSpecializationId;
                        break;
                    default: 
                        specialisationId = _localizedSpecializations.FirstOrDefault( e=> e.displayName == SpecializationDropdown.SelectedItem).id;
                        break;
                }
            }
            
            return new Keymap
            {
                CharacterName = CharacterNameDropdown.SelectedItem == _defaultCharacterEntry ? null : CharacterNameDropdown.SelectedItem,
                SpecialisationId = specialisationId,
                KeymapName = KeymapDropdown.SelectedItem == _defaultKeybindsEntry ? null : KeymapDropdown.SelectedItem,
            };
        }

        void OnKeymapChanged(object sender, ValueChangedEventArgs args)
        {
            ApplyButton.Enabled = KeymapDropdown.SelectedItem != _defaultKeybindsEntry;

            OnDataChanged?.Invoke(this, GetKeymapArgs());
            this._oldCharacterKeymap = GetKeymap();
        }
        void OnSpecializationChanged(object sender, ValueChangedEventArgs args)
        {
            OnDataChanged?.Invoke(this, GetKeymapArgs());
            this._oldCharacterKeymap = GetKeymap();
        }        
        void OnCharacterChanged(object sender, ValueChangedEventArgs args)
        {
            SpecializationDropdown.SelectedItem = _defaultSpecializationEntry;

            OnDataChanged?.Invoke(this, GetKeymapArgs());
            this._oldCharacterKeymap = GetKeymap();
        }
        void OnApplyClick(object sender, MouseEventArgs args)
        {
            OnApply?.Invoke(sender, GetKeymap());

        }
        void OnRemoveClick(object sender, MouseEventArgs args)
        {
            OnRemove?.Invoke(0, GetKeymap());
            DisposeEvents();
            Dispose();
        }

        public void DisposeEvents()
        {
            KeymapDropdown.ValueChanged -= OnKeymapChanged;
            SpecializationDropdown.ValueChanged -= OnSpecializationChanged;
            CharacterNameDropdown.ValueChanged -= OnCharacterChanged;
            ApplyButton.Click -= OnApplyClick;
            RemoveButton.Click -= OnRemoveClick;
        }
    }
}
