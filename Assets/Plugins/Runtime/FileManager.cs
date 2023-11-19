using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using File = System.IO.File;

namespace Plugins.Runtime
{
    public static class FileManager
    {
        private static string MermaidMakerFolderPath => Path.Combine(Application.dataPath, "MermaidMaker");
        

        public static void WriteFile(string fileName,string text)
        {
            if (!Directory.Exists(MermaidMakerFolderPath))
            {
                Directory.CreateDirectory(MermaidMakerFolderPath);
            }
            var filePath = Path.Combine(MermaidMakerFolderPath, $"{fileName}.md");

            if (!File.Exists(filePath))
            {
                var file = File.Create(filePath);
                file.Close();
            }
            File.WriteAllText(filePath,text);
        }

        public static List<string> GetAllMarkdownFiles()
        {
            if (!Directory.Exists(MermaidMakerFolderPath))
            {
                return new List<string>();
            }
            var directoryInfo = new DirectoryInfo(MermaidMakerFolderPath);
            var fileInfos = directoryInfo.GetFiles("*.md").ToList();
            fileInfos.RemoveAll(item => item.Name == ".md");
            return fileInfos.Select(file => file.Name.Split(".")[0]).ToList();
        }

        public static int GetNumberOfDefaultFiles()
        {
            for (var i = 0;; i++)
            {
                if (!File.Exists(Path.Combine(MermaidMakerFolderPath, $"ClassDiagram{i + 1}.md")))
                {
                    return i + 1;
                }
            }
        }
    }
}