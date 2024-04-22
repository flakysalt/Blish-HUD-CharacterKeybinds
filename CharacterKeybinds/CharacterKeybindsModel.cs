using System;
using System.Collections.Generic;
using System.IO;
using Blish_HUD.Input;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework.Input;

namespace flakysalt.CharacterKeybinds.Model
{
	public class CharacterKeybindsModel 
	{
		public SettingEntry<string> gw2KeybindsFolder;
		public SettingEntry<KeyBinding> optionsKeybind;
		public SettingEntry<bool> switchKeybindOnSpecializationsSwitch;


		public SettingCollection settingsCollection { get; private set; }
		public SettingCollection internalSettings;

		public IEnumerable<Character> charactersApiResponse { get; private set; }

		public class KeybindIdentifier 
		{
			public string CharacterName;
			public string Specialization;
		}


		private Dictionary<KeybindIdentifier, string> keybindRelations;


		public void Init(SettingCollection settings) 
		{

			this.settingsCollection = settings;
			string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			string targetFolderPath = Path.Combine(documentsPath, "Guild Wars 2", "InputBinds");


			gw2KeybindsFolder = settings.DefineSetting(
			"GW2 Keybind Path",
			targetFolderPath,
			() => "Path to the Keybinds folder",
			() => "");

			optionsKeybind = settings.DefineSetting(nameof(optionsKeybind),
			new KeyBinding(Keys.F11));

			switchKeybindOnSpecializationsSwitch = settings.DefineSetting(
				"Switch Keybinds on Specialization change",
				false);
		}

		public async void LoadCharactersAsync(Gw2ApiManager Gw2ApiManager)
		{
			var apiKeyPermissions = new List<TokenPermission>
			{
				TokenPermission.Account, // this permission can be used to check if your module got a token at all because every api key has this persmission.
                TokenPermission.Characters // this is the permission we actually require here to get the character names
            };

			if (!Gw2ApiManager.HasPermissions(apiKeyPermissions))
			{
				//_characterNamesLabel.Text = "api permissions are missing or api sub token not available yet";
				return;
			}
			charactersApiResponse = new List<Character>();

			try
			{
				charactersApiResponse = await Gw2ApiManager.Gw2ApiClient.V2.Characters.AllAsync();
			}
			catch (Exception e)
			{
				//Logger.Info($"Failed to get currencies from api.\n {e}");
			}
		}
	}
}
