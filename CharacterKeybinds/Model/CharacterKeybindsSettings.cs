using System;
using System.Collections.Generic;
using System.IO;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using flakysalt.CharacterKeybinds.Resources;
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
		public SettingEntry<bool> experiencedFtue;


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
			() => SettingsLoca.keybindsDirectorySetting,
			() => SettingsLoca.keybindsDirectoryHint);

			optionsKeybind = settings.DefineSetting(nameof(optionsKeybind),
			new KeyBinding(Keys.F11),
				() => SettingsLoca.optionsMenuKeybindsSetting);
			
			useDefaultKeybinds = settings.DefineSetting(
				"Use Default Keybinds",
				true,
				() => SettingsLoca.useDefaultKeybindsSetting,
				() => SettingsLoca.useDefaultKeybindsHint);

			changeKeybindsWhenSwitchingSpecialization = settings.DefineSetting(
				"Change Keybinds When Switching Specialization",
				true,
				() => SettingsLoca.changeKeybindsOnSpecSwitchSetting,
				() => SettingsLoca.changeKeybindsOnSpecSwitchHint);

			displayCornerIcon = settings.DefineSetting(nameof(displayCornerIcon),
				true,
				() => SettingsLoca.showCornerIconSetting,
				() => SettingsLoca.showCornerIconHint);
			
			autoClickSpeedMultiplier = settings.DefineSetting(nameof(autoClickSpeedMultiplier),
				1.0f,
				() => SettingsLoca.autoClickSpeedMultiplierSetting,
				() => SettingsLoca.autoClickSpeedMultiplierHint);
			autoClickSpeedMultiplier.SetRange(0.5f,2.5f);

			internalSettingsCollection = settings.AddSubCollection("internal Settings");
			
			experiencedFtue = internalSettingsCollection.DefineSetting(nameof(experiencedFtue), false);

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
