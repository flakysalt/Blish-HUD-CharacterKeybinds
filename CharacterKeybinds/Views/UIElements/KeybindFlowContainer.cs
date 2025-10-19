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

    public sealed class KeybindFlowContainer : FlowPanel
    {
        private StandardButton RemoveButton { get;}
        private StandardButton ApplyButton { get; }
        private Image ProfessionImage { get; }
        public Dropdown CharacterNameDropdown { get; }
        public Dropdown SpecializationDropdown { get; }
        public Dropdown KeymapDropdown { get; }
        
        readonly int _minDropdownWidth = 130;

        private const string DefaultCharacterEntry = "Select Character";
        private const string DefaultKeybindsEntry = "Keybinds";
        private const string DefaultSpecializationEntry = "Specialization";

        private const string CoreSpecialization = "Core";
        private const string WildcardSpecialization = "All Specialization";
        private const string InvalidSpecialization = "Invalid";

        public event EventHandler<Keymap> OnApply;
        public event EventHandler<KeymapEventArgs> OnDataChanged;
        public event EventHandler<Keymap> OnRemove;

        private Keymap _oldCharacterKeymap;
        
        private List<LocalizedSpecialization> _localizedSpecializations;

        public KeybindFlowContainer()
        {
            OuterControlPadding = new Vector2(10,0);
            ControlPadding = new Vector2(2,0);
            FlowDirection = ControlFlowDirection.LeftToRight;
            WidthSizingMode = SizingMode.Fill;
            HeightSizingMode = SizingMode.AutoSize;
            
            ProfessionImage = new Image
            {
                Parent = this,
                Size = new Point(30, 30),
            };

            CharacterNameDropdown = new Dropdown
            {
                Height = 30,
                Parent = this,
                Width = _minDropdownWidth
            };

            SpecializationDropdown = new Dropdown
            {
                Height = 30,
                Parent = this,
                Width = _minDropdownWidth,
            };
            SpecializationDropdown.Items.Add(WildcardSpecialization);
            SpecializationDropdown.Items.Add(CoreSpecialization);

            KeymapDropdown = new Dropdown
            {
                Height = 30,
                Parent = this,
                Width = _minDropdownWidth
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
            Resized += OnResized;
        }

        private void OnResized(object sender, ResizedEventArgs e)
        {
            //40 magic number padding for scrollbar
            var desiredSize = (int)(e.CurrentSize.X - ProfessionImage.Width - ApplyButton.Width - RemoveButton.Width - ControlPadding.X * 6 - 40) / 3;
            var width = MathHelper.Clamp(desiredSize, _minDropdownWidth, desiredSize);
            CharacterNameDropdown.Width = width;
            SpecializationDropdown.Width = width;
            KeymapDropdown.Width = width;
        }


        public void SetEnabled(bool enabled)
        {
            CharacterNameDropdown.Enabled = enabled;
            SpecializationDropdown.Enabled = enabled;
            KeymapDropdown.Enabled = enabled;
            ApplyButton.Enabled = enabled;
            RemoveButton.Enabled = enabled;
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
            CharacterNameDropdown.SelectedItem = string.IsNullOrEmpty(keymap.CharacterName) ? DefaultCharacterEntry : keymap.CharacterName;
            
            switch (keymap.SpecialisationId)
            {
                case 0:
                    SpecializationDropdown.SelectedItem = DefaultSpecializationEntry;
                    break;
                case Keymap.CoreSpecializationId:
                    SpecializationDropdown.SelectedItem = CoreSpecialization;
                    break;
                case Keymap.AllSpecializationId:
                    SpecializationDropdown.SelectedItem = WildcardSpecialization;
                    break;
                case Keymap.Invalid:
                    SpecializationDropdown.SelectedItem = InvalidSpecialization;
                    break;
                default: 
                    SpecializationDropdown.SelectedItem = _localizedSpecializations.FirstOrDefault( e=> e.id == keymap.SpecialisationId)?.displayName;
                    break;
            }
            
            KeymapDropdown.SelectedItem = string.IsNullOrEmpty(keymap.KeymapName) ? DefaultKeybindsEntry : keymap.KeymapName;
        }

        public void SetProfessionIcon(int iconId)
        {
            ProfessionImage.Texture = AsyncTexture2D.FromAssetId(iconId);
        }

        public void AttachListeners(EventHandler<Keymap> onApplyAction,
            EventHandler<KeymapEventArgs> onDataChanged,
            EventHandler<Keymap> onDeleteAction) 
        {
            OnDataChanged += onDataChanged;
            OnApply += onApplyAction;
            OnRemove += onDeleteAction;

            RemoveButton.Click += (o, eventArgs) =>
            {
                OnDataChanged -= onDataChanged;
                OnApply -= onApplyAction;
                OnRemove -= onDeleteAction;
            };
        }

        KeymapEventArgs GetKeymapArgs()
        {
            return new KeymapEventArgs
            {
                NewCharacterKeymap = GetKeymap(),
                OldCharacterKeymap = _oldCharacterKeymap
            };
        }

        Keymap GetKeymap()
        {
            int specialisationId = 0;

            if (SpecializationDropdown.Items.Contains(SpecializationDropdown.SelectedItem) || SpecializationDropdown.SelectedItem == InvalidSpecialization)
            {
                switch (SpecializationDropdown.Items.IndexOf(SpecializationDropdown.SelectedItem))
                {
                    case 0:
                        specialisationId = Keymap.AllSpecializationId;
                        break;
                    case 1:
                        specialisationId = Keymap.CoreSpecializationId;
                        break;
                    default: 
                        LocalizedSpecialization localizedSpecialization = _localizedSpecializations.FirstOrDefault( e=> e.displayName == SpecializationDropdown.SelectedItem);
                        specialisationId = localizedSpecialization?.id ?? Keymap.Invalid;
                        break;
                }
            }
            
            return new Keymap
            {
                CharacterName = CharacterNameDropdown.Items.Contains(CharacterNameDropdown.SelectedItem) ? CharacterNameDropdown.SelectedItem : null,
                SpecialisationId = specialisationId,
                KeymapName = KeymapDropdown.Items.Contains(KeymapDropdown.SelectedItem) ? KeymapDropdown.SelectedItem : null,
            };
        }

        private void OnKeymapChanged(object sender, ValueChangedEventArgs args)
        {
            ApplyButton.Enabled = KeymapDropdown.SelectedItem != DefaultKeybindsEntry;

            OnDataChanged?.Invoke(this, GetKeymapArgs());
            this._oldCharacterKeymap = GetKeymap();
        }

        private void OnSpecializationChanged(object sender, ValueChangedEventArgs args)
        {
            OnDataChanged?.Invoke(this, GetKeymapArgs());
            this._oldCharacterKeymap = GetKeymap();
        }

        private void OnCharacterChanged(object sender, ValueChangedEventArgs args)
        {
            SpecializationDropdown.SelectedItem = DefaultSpecializationEntry;

            OnDataChanged?.Invoke(this, GetKeymapArgs());
            _oldCharacterKeymap = GetKeymap();
        }

        private void OnApplyClick(object sender, MouseEventArgs args)
        {
            OnApply?.Invoke(sender, GetKeymap());

        }

        private void OnRemoveClick(object sender, MouseEventArgs args)
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
