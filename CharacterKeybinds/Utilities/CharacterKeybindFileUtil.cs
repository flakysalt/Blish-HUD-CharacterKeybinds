using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace flakysalt.CharacterKeybinds.Util
{
	static class CharacterKeybindFileUtil
	{
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
        
        public static bool KeybindFileExists(string folderPath, string filename) 
        {
            string path = Path.Combine(folderPath,filename+".xml");
            return File.Exists(path);
        }
    }
}
