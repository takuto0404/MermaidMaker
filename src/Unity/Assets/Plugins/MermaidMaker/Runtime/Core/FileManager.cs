using System.Collections.Generic;
using System.IO;
using System.Linq;
using File = System.IO.File;

namespace Plugins.MermaidMaker.Runtime.Core
{
    public static class FileManager
    {
        public static void WriteFile(string fileName,string text,string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var filePath = Path.Combine(path, $"{fileName}.md");

            if (!File.Exists(filePath))
            {
                var file = File.Create(filePath);
                file.Close();
            }
            File.WriteAllText(filePath,text);
        }

        public static List<string> GetAllMarkdownFiles(string path)
        {
            if (!Directory.Exists(path))
            {
                return new List<string>();
            }
            var directoryInfo = new DirectoryInfo(path);
            var fileInfos = directoryInfo.GetFiles("*.md").ToList();
            fileInfos.RemoveAll(item => item.Name == ".md");
            return fileInfos.Select(file => file.Name.Split(".")[0]).ToList();
        }

        public static int GetNumberOfDefaultFiles(string path)
        {
            for (var i = 0;; i++)
            {
                if (!File.Exists(Path.Combine(path, $"ClassDiagram{i + 1}.md")))
                {
                    return i + 1;
                }
            }
        }
    }
}