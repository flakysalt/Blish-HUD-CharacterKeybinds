using System;

namespace flakysalt.CharacterKeybinds.Data
{
	[Obsolete("Use Keymap class instead")]
	[Serializable]
	public class CharacterKeybind
	{
		public string characterName;
		public string spezialisation;
		public string keymap;
	}
	
	[Serializable]
	public class Keymap
	{
		public Keymap(Keymap map)
		{
			CharacterName = map.CharacterName;
			SpecialisationId = map.SpecialisationId;
			KeymapName = map.KeymapName;
		}
		public Keymap() { }
		
		public string CharacterName;
		//1- for Core, -2 for All, -10 for invalid
		public int SpecialisationId = 0;
		public string KeymapName;
		
		public const int CoreSpecializationId = -1;
		public const int AllSpecializationId = -2;
		public const int Invalid = -10;


	}
}
