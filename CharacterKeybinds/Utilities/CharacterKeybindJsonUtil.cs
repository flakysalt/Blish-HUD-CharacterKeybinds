using Newtonsoft.Json;
using System.Collections.Generic;
using CharacterKeybinds.Data;


namespace CharacterKeybinds.Util
{
	class CharacterKeybindJsonUtil
	{
        public static string SerializeCharacterList(List<CharacterKeybind> characterList)
        {
            return JsonConvert.SerializeObject(characterList, Formatting.Indented);
        }

        public static List<CharacterKeybind> DeserializeCharacterList(string jsonString)
        {
            return JsonConvert.DeserializeObject<List<CharacterKeybind>>(jsonString);
        }
    }
}
