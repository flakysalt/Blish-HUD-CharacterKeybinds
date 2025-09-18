using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi.V2.Models;

namespace flakysalt.CharacterKeybinds.Services
{
    public class Gw2ApiService
    {
        private readonly Gw2ApiManager _apiManager;
        private readonly Logger _logger = Logger.GetLogger<Gw2ApiService>();

        public Gw2ApiService(Gw2ApiManager apiManager)
        {
            _apiManager = apiManager;
        }

        public bool HasRequiredPermissions()
        {
            var apiKeyPermissions = new List<TokenPermission>
            {
                TokenPermission.Account,
                TokenPermission.Characters
            };
            return _apiManager.HasPermissions(apiKeyPermissions);
        }

        public async Task<IEnumerable<Character>> GetCharactersAsync()
        {
            try
            {
                return await _apiManager.Gw2ApiClient.V2.Characters.AllAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to fetch characters from API");
                throw;
            }
        }

        public async Task<IEnumerable<Profession>> GetProfessionsAsync()
        {
            try
            {
                return await _apiManager.Gw2ApiClient.V2.Professions.AllAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to fetch professions from API");
                throw;
            }
        }

        public async Task<IEnumerable<Specialization>> GetSpecializationsAsync()
        {
            try
            {
                return await _apiManager.Gw2ApiClient.V2.Specializations.AllAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to fetch specializations from API");
                throw;
            }
        }
        
        public async Task<Specialization> GetSpecializationAsync(int specializationId)
        {
            try
            {
                return await _apiManager.Gw2ApiClient.V2.Specializations.GetAsync(specializationId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to fetch specialization {specializationId} from API");
                throw;
            }
        }
    }
}
