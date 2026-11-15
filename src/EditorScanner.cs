#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace UnityMegaHub
{
    public class EditorScanner
    {
        private readonly HubSettings _settings;

        public EditorScanner(HubSettings settings)
        {
            _settings = settings;
        }

        // Сканирует указанные директории и возвращает словарь: [Версия Unity] -> [Путь к Unity.exe]
        public Dictionary<string, string> ScanForEditors()
        {
            var foundEditors = new Dictionary<string, string>();

            foreach (string rootDir in _settings.EditorDirectories)
            {
                if (!Directory.Exists(rootDir)) continue;

                try
                {
                    // Ищем все файлы Unity.exe в подпапках (включая Unity Hub структуру вида Editor/202X.X.X/Editor/Unity.exe)
                    string[] exeFiles = Directory.GetFiles(rootDir, "Unity.exe", SearchOption.AllDirectories);

                    foreach (string exePath in exeFiles)
                    {
                        string version = GetUnityVersionFromFile(exePath);
                        if (version != "Unknown" && !foundEditors.ContainsKey(version))
                        {
                            foundEditors.Add(version, exePath);
                        }
                    }
                }
                catch { /* Игнорируем ошибки доступа к системным папкам */ }
            }

            return foundEditors;
        }

        private string GetUnityVersionFromFile(string exePath)
        {
            try
            {
                // Вытаскиваем точную версию сборки движка прямо из метаданных Windows-файла Unity.exe
                var versionInfo = FileVersionInfo.GetVersionInfo(exePath);
                string? fileVersion = versionInfo.FileVersion;

                if (!string.IsNullOrEmpty(fileVersion))
                {
                    // Версия часто пишется с хэшем сборки, например "2022.3.12f1_c34a...", отсекаем лишнее
                    int underscoreIndex = fileVersion.IndexOf('_');
                    if (underscoreIndex != -1)
                    {
                        return fileVersion.Substring(0, underscoreIndex).Trim();
                    }
                    return fileVersion.Trim();
                }
            }
            catch { }

            return "Unknown";
        }
    }
}
