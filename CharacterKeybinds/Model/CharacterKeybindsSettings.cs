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
		//displayed settings
		public SettingCollection settingsCollection { get; private set; }
		public SettingEntry<float> autoClickSpeedMultiplier { get; private set; }
		public SettingEntry<string> gw2KeybindsFolder;
		public SettingEntry<string> defaultKeybinds;
		public SettingEntry<KeyBinding> optionsKeybind;
		public SettingEntry<bool> useDefaultKeybinds;
		public SettingEntry<bool> changeKeybindsWhenSwitchingSpecialization;
		public SettingEntry<bool> displayCornerIcon;

		//invisible settings
		public SettingCollection internalSettingsCollection { get; private set; }
		public SettingEntry<List<Keymap>> Keymaps;
		public SettingEntry<List<Point>> clickPositions;
		
		private static string TargetFolderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Guild Wars 2", "InputBinds");
		
		//legacy data
		public SettingEntry<List<CharacterKeybind>> characterKeybinds;
		
		public CharacterKeybindsSettings(SettingCollection settings) 
		{
			settingsCollection = settings;

			gw2KeybindsFolder = settings.DefineSetting(
			"GW2 Keybind Path",
			TargetFolderPath,
			"Keybins Folder Path",
			"Path to the Keybinds folder.\n Usually somewhere inside your documents folder");

			optionsKeybind = settings.DefineSetting(nameof(optionsKeybind),
			new KeyBinding(Keys.F11),
				() => "Options Menu Keybind");
			useDefaultKeybinds = settings.DefineSetting(
				"Use Default Keybinds",
				true,
				"Use Default Keybinds",
				"Switching to default keybinds when no others are defined for a character or specialization.");

			changeKeybindsWhenSwitchingSpecialization = settings.DefineSetting(
				"Change Keybinds When Switching Specialization",
				true,
				"Change keybinds When Switching Specialization",
				"Automatically change keybinds when switching elite specializations on the same character.");

			displayCornerIcon = settings.DefineSetting(nameof(displayCornerIcon),
				true,
				"Show corner icon",
				"Show/Hide the corner icon to open the keybinds window.");
			autoClickSpeedMultiplier = settings.DefineSetting(nameof(autoClickSpeedMultiplier),
				1.0f,
				"Keybindings Apply Speed",
				"Adjusts how fast the keybindings will be applied.\nLower (Left) this if your system is weaker and has trouble applying the keybindings.");
			autoClickSpeedMultiplier.SetRange(0.5f,2.5f);

			internalSettingsCollection = settings.AddSubCollection("internal Settings");

			defaultKeybinds = internalSettingsCollection.DefineSetting("defaultKeybinds", "");
			
			Keymaps = internalSettingsCollection.DefineSetting("Keymaps", new List<Keymap>());
			clickPositions = internalSettingsCollection.DefineSetting("clickpos", ClickPositions.importClickPositions);
			
			//legacy, use "Keymaps" instead
			characterKeybinds = internalSettingsCollection.DefineSetting("keybinds", new List<CharacterKeybind>());
		}

		public bool IsSaveFolderValid()
		{
			return Directory.Exists(TargetFolderPath);
		}
	}
}
