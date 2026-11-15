#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace UnityMegaHub
{
    public class TemplateManager
    {
        private static readonly string TemplatesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");

        public static List<TemplateItem> GetAvailableTemplates(HubSettings settings)
        {
            var list = new List<TemplateItem>();

            foreach (string rootDir in settings.TemplateDirectories)
            {
                if (!Directory.Exists(rootDir)) continue;

                string[] zipFiles = Directory.GetFiles(rootDir, "*.zip");

                foreach (string zipPath in zipFiles)
                {
                    var fileInfo = new FileInfo(zipPath);
                    string nameWithoutExt = Path.GetFileNameWithoutExtension(zipPath);

                    var item = new TemplateItem
                    {
                        Name = nameWithoutExt.Replace("_", " ").Replace("-", " "),
                        ZipFileName = fileInfo.Name,
                        ZipFilePath = zipPath,
                        Description = "Пользовательский шаблон.", // Дефолтный текст
                        TargetUnityVersion = "Unknown"
                    };

                    string pngPath = Path.Combine(rootDir, nameWithoutExt + ".png");
                    string jpgPath = Path.Combine(rootDir, nameWithoutExt + ".jpg");

                    if (File.Exists(pngPath)) item.ImagePath = pngPath;
                    else if (File.Exists(jpgPath)) item.ImagePath = jpgPath;
                    else item.ImagePath = null;

                    // ИСПРАВЛЕНО: Читаем версию Unity и кастомное описание из архива за один проход
                    ExtractDataFromZip(zipPath, item);
                    list.Add(item);
                }
            }

            return list;
        }

        private static void ExtractDataFromZip(string zipPath, TemplateItem item)
        {
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        // 1. Читаем версию Unity
                        if (entry.FullName.EndsWith("ProjectSettings/ProjectVersion.txt", StringComparison.OrdinalIgnoreCase))
                        {
                            using (StreamReader reader = new StreamReader(entry.Open()))
                            {
                                string? line;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    if (line.StartsWith("m_EditorVersion:"))
                                    {
                                        int index = line.IndexOf(':');
                                        if (index != -1) item.TargetUnityVersion = line.Substring(index + 1).Trim();
                                    }
                                }
                            }
                        }
                        // 2. Читаем наше созданное кастомное описание о коде
                        else if (entry.FullName.Equals("about.txt", StringComparison.OrdinalIgnoreCase))
                        {
                            using (StreamReader reader = new StreamReader(entry.Open()))
                            {
                                item.Description = reader.ReadToEnd().Trim();
                            }
                        }
                    }
                }
            }
            catch { }
            if (string.IsNullOrEmpty(item.TargetUnityVersion)) item.TargetUnityVersion = "2022.3.29f1";
        }

    }
}
