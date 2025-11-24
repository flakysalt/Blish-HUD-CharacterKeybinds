using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi.V2.Models;
using System.Net;
using flakysalt.CharacterKeybinds.Data;
using Newtonsoft.Json;

namespace flakysalt.CharacterKeybinds.Services
{
    public class Gw2ApiService
    {
        
        private readonly Gw2ApiManager _apiManager;
        private readonly Logger _logger = Logger.GetLogger<Gw2ApiService>();
        
        private readonly string apiStatusWebsiteUrl = "https://status.gw2efficiency.com/api/";
        private ApiStatusResponse statusResponse;
        
        private DateTime _lastApiStatusCheck = DateTime.MinValue;
        
        public event EventHandler<ValueEventArgs<IEnumerable<TokenPermission>>> SubtokenUpdated;
        
        
        public Gw2ApiService(Gw2ApiManager apiManager)
        {
            _apiManager = apiManager;
            _apiManager.SubtokenUpdated += OnApiManagerSubtokenUpdated;
        }
        private void OnApiManagerSubtokenUpdated(object sender, ValueEventArgs<IEnumerable<TokenPermission>> e)
        {
            SubtokenUpdated?.Invoke(sender, e);
        }
        
        public async Task<bool> IsApiAvailable()
        {
            if(_lastApiStatusCheck.AddMinutes(5) < DateTime.Now)
            {
                await UpdateApiStatus();
            }
            
            var upCount = statusResponse.Data.Count(e => e.Status == 200);
            var totalCount = statusResponse.Data.Count;

            return (upCount / (double)totalCount) > 0.9;
        }

        private async Task UpdateApiStatus()
        {
            WebRequest request = WebRequest.Create(apiStatusWebsiteUrl);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                _lastApiStatusCheck = DateTime.Now;
                string json = await reader.ReadToEndAsync();
                statusResponse = JsonConvert.DeserializeObject<ApiStatusResponse>(json);
            }
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
        public bool HasSubtoken()
        {
            return _apiManager.HasSubtoken;
        }

        public async Task<IEnumerable<Character>> GetCharactersAsync()
        {
            try
            {
                return await _apiManager.Gw2ApiClient.V2.Characters.AllAsync();
            }
            catch (Exception ex)
            {
                _logger.Info(ex, "Failed to fetch characters from API");
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
                _logger.Info(ex, "Failed to fetch professions from API");
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
                _logger.Info(ex, "Failed to fetch specializations from API");
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
                _logger.Info(ex, $"Failed to fetch specialization {specializationId} from API");
                throw;
            }
        }
    }
}
