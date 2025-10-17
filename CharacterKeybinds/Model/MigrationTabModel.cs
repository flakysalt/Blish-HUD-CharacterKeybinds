using System.Collections.Generic;
using System.Threading.Tasks;
using flakysalt.CharacterKeybinds.Services;
using flakysalt.CharacterKeybinds.Util;

namespace flakysalt.CharacterKeybinds.Model
{
    public class MigrationTabModel
    {
        public CharacterKeybindsSettings Settings { get; }
        private readonly Gw2ApiService _apiService;
        
        public MigrationTabModel(CharacterKeybindsSettings settings, Gw2ApiService apiService) 
        {
            Settings = settings;
            _apiService = apiService;
        }

        public async Task<List<string>> MigrateKeybindings()
        {
            var specializations = await _apiService.GetSpecializationsAsync();
            var keymaps = SaveDataMigration.MigrateToKeymaps(Settings.characterKeybinds.Value, specializations, out var migrationReport);
            Settings.Keymaps.Value = keymaps;
            return migrationReport;
        }
    }
}