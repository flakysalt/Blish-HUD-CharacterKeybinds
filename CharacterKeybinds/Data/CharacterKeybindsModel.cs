using System;
using System.IO;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework.Input;

namespace flakysalt.CharacterKeybinds.Data
{
	public class CharacterKeybindsModel 
	{
		public SettingEntry<string> gw2KeybindsFolder;
		public SettingEntry<KeyBinding> optionsKeybind;
		public SettingEntry<bool> onlyChangeKeybindsOnCharacterChange;
		public SettingEntry<bool> displayCornerIcon;


		public SettingCollection settingsCollection { get; private set; }

		public CharacterKeybindsModel(SettingCollection settings) 
		{
			settingsCollection = settings;
			string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			string targetFolderPath = Path.Combine(documentsPath, "Guild Wars 2", "InputBinds");


			gw2KeybindsFolder = settings.DefineSetting(
			"GW2 Keybind Path",
			targetFolderPath,
			() => "Path to the Keybinds folder",
			() => "");

			optionsKeybind = settings.DefineSetting(nameof(optionsKeybind),
			new KeyBinding(Keys.F11));

			onlyChangeKeybindsOnCharacterChange = settings.DefineSetting(
				"Only change keybinds when switching characters",
				true);

			displayCornerIcon = settings.DefineSetting(nameof(displayCornerIcon), true, () => "Show corner icon");
		}
	}
}
