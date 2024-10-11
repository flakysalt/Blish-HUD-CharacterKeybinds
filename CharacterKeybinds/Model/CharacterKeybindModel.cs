using flakysalt.CharacterKeybinds.Data;
using flakysalt.CharacterKeybinds.Model;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace flakysalt.CharacterKeybinds.Model
{
	public class CharacterKeybindModel
	{
		CharacterKeybindsSettings _settings;

		//AccountData
		Dictionary<Profession, HashSet<Specialization>> ProfessionEliteSpecialization = new Dictionary<Profession, HashSet<Specialization>>();
		IEnumerable<Character> characters = new List<Character>();

		public string currentKeybinds { get; set; }

		private Action OnCharactersChanged;
		private Action OnKeymapChanged;

		public CharacterKeybindModel(CharacterKeybindsSettings settings) 
		{
			_settings = settings;
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

		public List<string> GetProfessionSpecializations(string characterName)
		{
			var character = characters.FirstOrDefault(c => c.Name == characterName);

			if (character == null)
				return new List<string>();


			var professionKey = ProfessionEliteSpecialization.Keys.FirstOrDefault(p => p.Name == character.Profession);

			return ProfessionEliteSpecialization[professionKey].Select(specialization => specialization.Name).ToList();
		}

		public List<string> GetKeymapsNames()
		{
			return _settings.characterKeybinds.Value.Select(specialization => specialization.keymapName).ToList();
		}
		public List<Keymap> GetKeymaps()
		{
			return _settings.characterKeybinds.Value;
		}

		public string GetDefaultKeybind()
		{
			return _settings.defaultKeybinds.Value;
		}

		public string GetKeybindsFolder()
		{
			return _settings.gw2KeybindsFolder.Value;
		}

		public Keymap GetKeymapName(string characterName, Specialization specialization) 
		{
			foreach (var keybindData in _settings.characterKeybinds.Value)
			{
				if (keybindData.characterName == characterName)
				{
					//special case for core builds
					if (!specialization.Elite && keybindData.specializationName == "Core")
					{
						return keybindData;
					}

					if (specialization.Name == keybindData.specializationName)
					{
						return keybindData;
					}

					if (keybindData.specializationName == "All Specialization")
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
			this.characters = characters;
			OnCharactersChanged.Invoke();
		}
		public void RemoveKeymap(Keymap keymap)
		{
			var element = _settings.characterKeybinds.Value.Find(e =>
			e.keymapName == keymap.keymapName &&
			e.characterName == keymap.characterName &&
			e.specializationName == keymap.specializationName);

			if (element != null)
			{
				_settings.characterKeybinds.Value.Remove(element);
				OnKeymapChanged.Invoke();
			}
		}

		public void AddKeymap()
		{
			_settings.characterKeybinds.Value.Add(new Keymap());
			OnKeymapChanged.Invoke();
		}

		public void UpdateKeymap(Keymap oldValue, Keymap newValue)
		{
			if (oldValue == null)
				return;

			int index = _settings.characterKeybinds.Value.FindIndex(e =>
			e.keymapName == oldValue.keymapName &&
			e.characterName == oldValue.characterName &&
			e.specializationName == oldValue.specializationName);

			if (index != -1)
			{
				_settings.characterKeybinds.Value[index] = newValue;
				OnKeymapChanged.Invoke();
			}
		}

		public void AddProfessionEliteSpecialization(Profession profession, Specialization eliteSpecialization)
		{
			if (!ProfessionEliteSpecialization.ContainsKey(profession))
			{
				ProfessionEliteSpecialization[profession] = new HashSet<Specialization>();
			}

			ProfessionEliteSpecialization[profession].Add(eliteSpecialization);
		}

		public void SetDefaultKeymap(string keymap)
		{
			_settings.defaultKeybinds.Value = keymap;
		}

		#endregion

	}
}
