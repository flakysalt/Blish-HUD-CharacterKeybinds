using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD;
using flakysalt.CharacterKeybinds.Data;
using flakysalt.CharacterKeybinds.Model;
using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;

namespace flakysalt.CharacterKeybinds.Util
{
    public class SaveDataMigration
    {
        public static List<Keymap> MigrateToKeymaps(List<CharacterKeybind> characterKeybinds, IEnumerable<Specialization> specializations)
        {
            try
            {
                List<Keymap> migratedKeymaps = new List<Keymap>();
                foreach (var keymap in characterKeybinds)
                {
                    int id = 0;
                    switch (keymap.spezialisation)
                    {
                        case "All Specialization":
                            id = Keymap.AllSpecializationId;
                            break;
                        case "Core":
                            id = Keymap.CoreSpecializationId;
                            break;
                        default:
                            Specialization specialization = specializations.FirstOrDefault(e=> e.Name == keymap.spezialisation);

                            if (specialization == null)
                            {                
                                Logger.GetLogger<SaveDataMigration>().Warn($"Unable to find specialization {keymap.spezialisation} and migrate to new data");
                                //continue;
                            }

                            id = specialization.Id;
                            break;
                    }
                    
                    migratedKeymaps.Add(new Keymap
                    {
                        KeymapName = keymap.keymap,
                        CharacterName = keymap.characterName,
                        SpecialisationId = id
                    });
                }

                return migratedKeymaps;
            }
            catch (Exception e)
            {
                Logger.GetLogger<SaveDataMigration>().Fatal(e.Message, e.StackTrace);
                throw;
            }
        }
    }
}