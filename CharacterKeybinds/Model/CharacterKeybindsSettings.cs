using System;
using System.Collections.Generic;
using System.IO;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using flakysalt.CharacterKeybinds.Data;

namespace flakysalt.CharacterKeybinds.Model
{
	public class CharacterKeybindsSettings
	{
		public SettingCollection settingsCollection { get; private set; }
		
		public SettingEntry<float> autoClickSpeedMultiplier { get; private set; }

		public SettingEntry<string> gw2KeybindsFolder;

		public SettingEntry<string> defaultKeybinds;
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
			() => "Path to the Keybinds folder. Usually somewhere inside your documents folder",
			() => "");

			optionsKeybind = settings.DefineSetting(nameof(optionsKeybind),
			new KeyBinding(Keys.F11),
				() => "Options Menu Keybind");

			changeKeybindsWhenSwitchingSpecialization = settings.DefineSetting(
				"Change Keybinds When Switching Specialization",
				true);

			displayCornerIcon = settings.DefineSetting(nameof(displayCornerIcon), true, () => "Show corner icon");
			autoClickSpeedMultiplier = settings.DefineSetting(nameof(autoClickSpeedMultiplier),
				1.0f,
				() => "Keybindings Apply Speed",()=> "Adjusts how fast the keybindings will be applied.\nLower this if your system is weaker and has trouble applying the keybindings.");
			autoClickSpeedMultiplier.SetRange(0.5f,2.5f);

			internalSettingsCollection = settings.AddSubCollection("internal Settings");

			defaultKeybinds = internalSettingsCollection.DefineSetting("defaultKeybinds", "");
			characterKeybinds = internalSettingsCollection.DefineSetting("keybinds", new List<CharacterKeybind>());
			clickPositions = internalSettingsCollection.DefineSetting("clickpos", ClickPositions.importClickPositions);
		}
	}
}
