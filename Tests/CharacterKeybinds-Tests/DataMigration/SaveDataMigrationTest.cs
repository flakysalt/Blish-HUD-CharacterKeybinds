using NUnit.Framework;

using flakysalt.CharacterKeybinds.Data;
using flakysalt.CharacterKeybinds.Util;

namespace flakysalt.CharacterKeybinds.Tests
{
    public class SaveDataMigrationTest
    {
        [Test]
        public void MigrateKeybindsToKeymaps()
        {
            //Setup
            List<CharacterKeybind> characterKeybinds = new List<CharacterKeybind>();
        
            characterKeybinds.Add(new CharacterKeybind(){characterName = "Zinnia Firekeeper",spezialisation = "All Specialization", keymap = "2"});
            characterKeybinds.Add(new CharacterKeybind(){characterName = "Zinnia Firekeeper",spezialisation = "Core", keymap = "Ele"});
            characterKeybinds.Add(new CharacterKeybind(){characterName = "Zinnia Firekeeper",spezialisation = "Tempest", keymap = "test"});
            characterKeybinds.Add(new CharacterKeybind(){characterName = "Sangonomiya Kokomí",spezialisation = "Dragonhunter", keymap = "Power DH"});
            
            var keymaps = SaveDataMigration.MigrateToKeymaps(characterKeybinds, null);

        
        }
    }
}