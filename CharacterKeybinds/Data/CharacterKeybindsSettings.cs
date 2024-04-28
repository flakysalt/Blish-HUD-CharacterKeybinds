using System;
using System.Collections.Generic;
using System.IO;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace flakysalt.CharacterKeybinds.Data
{
	public class CharacterKeybindsSettings
	{
		public SettingCollection settingsCollection { get; private set; }

		public SettingEntry<string> gw2KeybindsFolder;
		public SettingEntry<KeyBinding> optionsKeybind;
		public SettingEntry<bool> changeKeybindsWhenSwitchingSpecialization;
		public SettingEntry<bool> displayCornerIcon;

		public SettingCollection internalSettingsCollection { get; private set; }

		public SettingEntry<List<CharacterKeybind>> characterKeybinds;
		public SettingEntry<List<Point>> clickPositions;


		public CharacterKeybindsSettings(SettingCollection settings) 
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
			new KeyBinding(Keys.F11),
				() => "Options Menu Keybind");

			changeKeybindsWhenSwitchingSpecialization = settings.DefineSetting(
				"Change Keybinds When Switching Specialization",
				true);

			displayCornerIcon = settings.DefineSetting(nameof(displayCornerIcon), true, () => "Show corner icon");

			internalSettingsCollection = settings.AddSubCollection("internal Settings");
			characterKeybinds = internalSettingsCollection.DefineSetting("fishLastCaughtTime", new List<CharacterKeybind>());
			clickPositions = internalSettingsCollection.DefineSetting("fishLastCaughtTime", ClickPosLocations.importMarkerLocations);
		}
	}
}
