﻿using Newtonsoft.Json;
using System.Collections.Generic;
using flakysalt.CharacterKeybinds.Data;
using System.IO;
using System.Linq;

namespace flakysalt.CharacterKeybinds.Util
{
	static class CharacterKeybindJsonUtil
	{
        public static string SerializeCharacterList(List<Keymap> characterList)
        {
            return JsonConvert.SerializeObject(characterList, Formatting.Indented);
        }

        public static List<Keymap> DeserializeCharacterList(string jsonString)
        {
            return JsonConvert.DeserializeObject<List<Keymap>>(jsonString);
        }

        public static void MoveAllXmlFiles(string sourcePath, string destinationPath)
        {
            string[] fileEntries = Directory.GetFiles(sourcePath, "*.xml");

            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            foreach (string filePath in fileEntries)
            {
                string fileName = Path.GetFileName(filePath);
                string destPath = Path.Combine(destinationPath, fileName);
                System.IO.File.Move(filePath, destPath);
            }
        }
        public static List<string> GetKeybindFiles(string path) 
        {
            string[] xmlFiles = Directory.GetFiles(path, "*.xml");

            for (int i = 0; i < xmlFiles.Length; i++)
            {
                xmlFiles[i] = Path.GetFileNameWithoutExtension(xmlFiles[i]);
            }

            return xmlFiles.ToList();
        }
    }
}
