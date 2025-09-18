using flakysalt.CharacterKeybinds.Data;
using flakysalt.CharacterKeybinds.Services;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD;
using flakysalt.CharacterKeybinds.Util;

namespace flakysalt.CharacterKeybinds.Model
{
    public class CharacterKeybindsModel
    {
        private readonly Logger _logger = Logger.GetLogger<CharacterKeybindsModel>();
        public CharacterKeybindsSettings Settings { get; }
        private readonly Gw2ApiService _apiService;

        // Account Data
        private Dictionary<Profession, List<Specialization>> _professionEliteSpecialization = new Dictionary<Profession, List<Specialization>>();
        private List<Character> _characters = new List<Character>();

        public string CurrentKeybinds { get; set; }

        private Action OnCharactersChanged;
        private Action OnKeymapChanged;

        public CharacterKeybindsModel(CharacterKeybindsSettings settings, Gw2ApiService apiService) 
        {
            Settings = settings;
            _apiService = apiService;
        }

        #region Data Loading

        public async Task LoadResourcesAsync()
        {
            try
            {
                // Load characters, professions, and specializations from the API
                var characters = await _apiService.GetCharactersAsync();
                var professions = await _apiService.GetProfessionsAsync();
                var specializations = await _apiService.GetSpecializationsAsync();

                // Set the data
                _characters = characters.ToList();
                
                // Handle legacy data migration if needed
                if (Settings.characterKeybinds.Value.Any() && !Settings.Keymaps.Value.Any())
                {
                    //TODO RH move this into its own tab and dont do it automatically
                    var keymaps = SaveDataMigration.MigrateToKeymaps(Settings.characterKeybinds.Value, specializations);
                    Settings.Keymaps.Value = keymaps;
                }

                // Process elite specializations
                foreach (var specialization in specializations)
                {
                    if (!specialization.Elite) continue;

                    Profession profession = professions.First(p => p.Id == specialization.Profession);
                    AddProfessionEliteSpecialization(profession, specialization);
                }

                // Notify listeners that data has changed
                OnCharactersChanged?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load resources from API");
                throw;
            }
        }

        public void ClearResources()
        {
            _professionEliteSpecialization.Clear();
            // Don't invoke change events here - wait for reloading
        }

        #endregion

        #region Actions and Event Binding

        public void BindCharacterDataChanged(Action onCharactersChanged)
        {
            OnCharactersChanged += onCharactersChanged;
        }
        
        public void BindKeymapChanged(Action onKeymapChanged)
        {
            OnKeymapChanged += onKeymapChanged;
        }

        #endregion

        #region Getters

        public List<string> GetCharacterNames()
        {
            return _characters.Select(character => character.Name).ToList();
        }
        
        public Character GetCharacter(string name)
        {
            return _characters.FirstOrDefault(character => character.Name == name);
        }

        public Profession GetProfession(string name)
        {
            return _professionEliteSpecialization.FirstOrDefault(character => character.Key.Id == name).Key;
        }
        
        public Specialization GetProfessionSpecialization(string name)
        {
            foreach (var keyValuePair in _professionEliteSpecialization)
            {
                foreach (var specialization in keyValuePair.Value)
                {
                    if (specialization.Name == name)
                        return specialization;
                }
            }
            return null;
        }

        public List<Character> GetCharacters()
        {
            return _characters.ToList();
        }

        public List<LocalizedSpecialization> GetProfessionSpecializations(string characterName)
        {
            var character = _characters.FirstOrDefault(c => c.Name == characterName);

            if (character == null) 
                return new List<LocalizedSpecialization>();

            var professionKey = _professionEliteSpecialization.Keys.FirstOrDefault(p => p.Id == character.Profession);
            if(professionKey == null) 
                return new List<LocalizedSpecialization>();
            
            List<LocalizedSpecialization> localizedSpecializations = new List<LocalizedSpecialization>();
            
            foreach (var specialization in _professionEliteSpecialization[professionKey])
            {
                LocalizedSpecialization localizedSpec = new LocalizedSpecialization
                {
                    displayName = specialization.Name,
                    id = specialization.Id
                };
                localizedSpecializations.Add(localizedSpec);
            }
            return localizedSpecializations;
        }

        public List<string> GetKeymapsNames()
        {
            return Settings.characterKeybinds.Value.Select(specialization => specialization.keymap).ToList();
        }
        
        public List<Keymap> GetKeymaps()
        {
            return Settings.Keymaps.Value;
        }

        public string GetDefaultKeybind()
        {
            return Settings.defaultKeybinds.Value;
        }

        public string GetKeybindsFolder()
        {
            return Settings.gw2KeybindsFolder.Value;
        }

        public Keymap GetKeymapName(string characterName, Specialization specialization) 
        {
            foreach (var keybindData in Settings.Keymaps.Value)
            {
                if (keybindData.CharacterName == characterName)
                {
                    //special case for core builds
                    if (!specialization.Elite && keybindData.SpecialisationId == Keymap.CoreSpecializationId)
                    {
                        return keybindData;
                    }

                    if (specialization.Id == keybindData.SpecialisationId)
                    {
                        return keybindData;
                    }
                }
            }
            
            //check in extra loop to make sure we always find tailored keybinds first
            foreach (var keybindData in Settings.Keymaps.Value)
            {
                if (keybindData.CharacterName == characterName)
                {
                    //Check for profession wildcard
                    if (keybindData.SpecialisationId == Keymap.AllSpecializationId)
                    {
                        return keybindData;
                    }
                }
            }
            return null;
        }

        #endregion

        #region Setters and Modifiers

        public void AddProfessionEliteSpecialization(Profession profession, Specialization specialization)
        {
            if (!_professionEliteSpecialization.ContainsKey(profession))
            {
                _professionEliteSpecialization[profession] = new List<Specialization>();
            }
            _professionEliteSpecialization[profession].Add(specialization);
        }

        public void SetDefaultKeymap(string keymap)
        {
            Settings.defaultKeybinds.Value = keymap;
        }

        public void AddKeymap()
        {
            var keymap = new Keymap();
            Settings.Keymaps.Value.Add(keymap);
            OnKeymapChanged?.Invoke();
        }

        public void UpdateKeymap(Keymap oldKeymap, Keymap newKeymap)
        {
            if (TryGetKeymap(oldKeymap, out var foundMap))
            {
                Settings.Keymaps.Value[Settings.Keymaps.Value.IndexOf(foundMap)] = newKeymap;
                OnKeymapChanged?.Invoke();
            }
        }

        public void RemoveKeymap(Keymap keymap)
        {
            if (!TryGetKeymap(keymap, out var foundMap)) return;
            if (Settings.Keymaps.Value.Remove(foundMap))
            {
                OnKeymapChanged?.Invoke();
            }
        }
        
        private bool TryGetKeymap(Keymap map, out Keymap foundMap)
        {
            foundMap = Settings.Keymaps.Value.FirstOrDefault(e => 
                e.CharacterName == map.CharacterName &&
                e.SpecialisationId == map.SpecialisationId &&
                e.KeymapName == map.KeymapName);
            return foundMap != null;
        } 

        #endregion
    }
}
