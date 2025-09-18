using flakysalt.CharacterKeybinds.Data;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace flakysalt.CharacterKeybinds.Model
{
	public class CharacterKeybindModel
	{
		public CharacterKeybindsSettings Settings { get; }

		//AccountData
		Dictionary<Profession, List<Specialization>> ProfessionEliteSpecialization = new Dictionary<Profession, List<Specialization>>();
		List<Character> characters = new List<Character>();

		public string currentKeybinds { get; set; }

		private Action OnCharactersChanged;
		private Action OnKeymapChanged;

		public CharacterKeybindModel(CharacterKeybindsSettings settings) 
		{
			Settings = settings;
		}

		#region Actions

		public void BindCharacterDataChanged(Action onAddButton)
		{
			OnCharactersChanged += onAddButton;
		}
		public void BindKeymapChanged(Action onAddButton)
		{
			OnKeymapChanged += onAddButton;
		}

		#endregion

		#region Getter

		public List<string> GetCharacterNames()
		{
			return characters.Select(character => character.Name).ToList();
		}
		public Character GetCharacter(string Name)
		{
			return characters.FirstOrDefault(character => character.Name == Name);
		}

		public Profession GetProfession(string Name)
		{
			return ProfessionEliteSpecialization.FirstOrDefault(character => character.Key.Id == Name).Key;
		}
		public Specialization GetProfessionSpecialization(string Name)
		{
			foreach (var keyValuePair in ProfessionEliteSpecialization)
			{
				foreach (var specialization in keyValuePair.Value)
				{
					if (specialization.Name == Name)
						return specialization;
				}
			}
			return null;
		}

		public List<Character> GetCharacters()
		{
			return (List<Character>)characters;
		}

		public List<LocalizedSpecialization> GetProfessionSpecializations(string characterName)
		{
			var character = characters.FirstOrDefault(c => c.Name == characterName);

			if (character == null) 
				return new List<LocalizedSpecialization>();

			var professionKey = ProfessionEliteSpecialization.Keys.FirstOrDefault(p => p.Id == character.Profession);
			if(professionKey == null) 
				return new List<LocalizedSpecialization>();
			
			List<LocalizedSpecialization> localizedSpecializations = new List<LocalizedSpecialization>();

			
			foreach (var specialization in ProfessionEliteSpecialization[professionKey])
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

		#region Setter

		public void SetCharacters(IReadOnlyCollection<Character> characters)
		{
			this.characters = characters.ToList();
			OnCharactersChanged.Invoke();
		}
		
		public void RemoveKeymap(Keymap characterKeybind)
		{
			var element = Settings.Keymaps.Value.Find(e =>
			e.KeymapName == characterKeybind.KeymapName &&
			e.CharacterName == characterKeybind.CharacterName &&
			e.SpecialisationId == characterKeybind.SpecialisationId);
			if (element != null)
			{
				Settings.Keymaps.Value.Remove(element);
				OnKeymapChanged.Invoke();
			}
		}

		public void AddKeymap()
		{
			Settings.Keymaps.Value.Add(new Keymap());
			OnKeymapChanged.Invoke();
		}

		public void UpdateKeymap(Keymap oldValue, Keymap newValue)
		{
			if (oldValue == null)
				return;

			int index = Settings.Keymaps.Value.FindIndex(e =>
			e.KeymapName == oldValue.KeymapName &&
			e.CharacterName == oldValue.CharacterName &&
			e.SpecialisationId == oldValue.SpecialisationId);

			if (index != -1)
			{
				Settings.Keymaps.Value[index] = newValue;
				OnKeymapChanged.Invoke();
			}
		}

		public void AddProfessionEliteSpecialization(Profession profession, Specialization eliteSpecialization)
		{
			Profession existingProfession = ProfessionEliteSpecialization.Keys.FirstOrDefault(p => p.Id == profession.Id);
			
			if (existingProfession == null)
			{
				ProfessionEliteSpecialization[profession] = new List<Specialization>();
				existingProfession = profession;
			}

			if (ProfessionEliteSpecialization[existingProfession].All(e => eliteSpecialization.Id != e.Id))
			{
				ProfessionEliteSpecialization[existingProfession].Add(eliteSpecialization);
			}
		}

		public void SetDefaultKeymap(string keymap)
		{
			Settings.defaultKeybinds.Value = keymap;
		}

		public void ClearResources()
		{
			characters.Clear();
			ProfessionEliteSpecialization.Clear();
		}

		#endregion

	}
}
