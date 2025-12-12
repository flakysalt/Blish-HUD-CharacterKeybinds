using Blish_HUD.Controls;
using flakysalt.CharacterKeybinds.Data;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Input;
using flakysalt.CharacterKeybinds.Resources;
using Dropdown = flakysalt.CharacterKeybinds.Views.UiElements;

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
        public Dropdown<string>  CharacterNameDropdown { get; }
        public Dropdown<string> SpecializationDropdown { get; }
        public Dropdown<string> KeymapDropdown { get; }
        
        readonly int _minDropdownWidth = 130;
        readonly int maxDropdownHeight = 300;

        private string DefaultCharacterEntry => Loca.defaultCharacterEntry;
        private string DefaultKeybindsEntry => Loca.defaultKeybindsEntry;
        private string DefaultSpecializationEntry => Loca.defaultSpecializationEntry;

        private string CoreSpecialization => Loca.coreSpecializationName;
        private string WildcardSpecialization = Loca.wildcardSpecializationName;
        private string InvalidSpecialization = Loca.invalidSpecializationName;
        
        public event EventHandler<Keymap> OnApply;
        public event EventHandler<KeymapEventArgs> OnDataChanged;
        public event EventHandler<Keymap> OnRemove;

        private Keymap _oldCharacterKeymap;
        
        private List<LocalizedSpecialization> _localizedSpecializations;

        public KeybindFlowContainer()
        {
            _oldCharacterKeymap = new Keymap();
            OuterControlPadding = new Vector2(10,0);
            ControlPadding = new Vector2(2,0);
            FlowDirection = ControlFlowDirection.LeftToRight;
            WidthSizingMode = SizingMode.Fill;
            HeightSizingMode = SizingMode.AutoSize;
            CanScroll = false;
            
            ProfessionImage = new Image
            {
                Parent = this,
                Size = new Point(32, 32),
            };

            CharacterNameDropdown = new Dropdown<string>
            {
                Height = 30,
                Parent = this,
                Width = _minDropdownWidth,
                PanelHeight = maxDropdownHeight
            };

            SpecializationDropdown = new Dropdown<string>
            {
                Padding = new Thickness(22,0,0,0),
                
                Height = 30,
                Parent = this,
                Width = _minDropdownWidth,
                PanelHeight = maxDropdownHeight
            };
            SpecializationDropdown.Items.Add(WildcardSpecialization);
            SpecializationDropdown.Items.Add(CoreSpecialization);

            KeymapDropdown = new Dropdown<string>
            {
                Height = 30,
                Parent = this,
                Width = _minDropdownWidth,
                PanelHeight = maxDropdownHeight
            };
            
            ApplyButton = new StandardButton
            {
                Parent = this,
                Text = Loca.apply,
                Size = new Point(60, 30),
            };
            
            RemoveButton = new StandardButton
            {
                Parent = this,
                Text = Loca.delete,
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

        public void SetDropdownContent(Dropdown<string> dropdown, List<string> values)
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
            _oldCharacterKeymap = new Keymap(keymap);
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
                CharacterName = CharacterNameDropdown.SelectedItem.Equals(DefaultCharacterEntry) ? null : CharacterNameDropdown.SelectedItem,
                SpecialisationId = specialisationId,
                KeymapName = KeymapDropdown.Items.Contains(KeymapDropdown.SelectedItem) ? KeymapDropdown.SelectedItem : null,
            };
        }

        private void OnKeymapChanged(object sender, ValueChangedEventArgs<string> args)
        {
            ApplyButton.Enabled = KeymapDropdown.SelectedItem != DefaultKeybindsEntry;

            OnDataChanged?.Invoke(this, GetKeymapArgs());
            this._oldCharacterKeymap = GetKeymap();
        }

        private void OnSpecializationChanged(object sender, ValueChangedEventArgs<string> args)
        {
            OnDataChanged?.Invoke(this, GetKeymapArgs());
            this._oldCharacterKeymap = GetKeymap();
        }

        private void OnCharacterChanged(object sender, ValueChangedEventArgs<string> args)
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
            OnRemove?.Invoke(0, _oldCharacterKeymap);
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
