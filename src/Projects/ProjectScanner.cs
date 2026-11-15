#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace UnityMegaHub
{
    public class ProjectScanner
    {
        private static readonly string CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "projects.json");
        private readonly HubSettings _settings;

        public ProjectScanner(HubSettings settings) => _settings = settings;

        public List<ProjectItem> LoadCachedProjects()
        {
            if (!File.Exists(CachePath)) return new List<ProjectItem>();
            try
            {
                string json = File.ReadAllText(CachePath);
                return JsonSerializer.Deserialize<List<ProjectItem>>(json) ?? new List<ProjectItem>();
            }
            catch { return new List<ProjectItem>(); }
        }

        public void SaveProjectsToCache(List<ProjectItem> projects)
        {
            try
            {
                string json = JsonSerializer.Serialize(projects, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(CachePath, json);
            }
            catch { }
        }

        public List<ProjectItem> ScanDirectories()
        {
            var detectedProjects = new List<ProjectItem>();
            var cachedProjects = LoadCachedProjects();

            var cacheMap = new Dictionary<string, string?>();
            foreach (var p in cachedProjects)
                if (!cacheMap.ContainsKey(p.Path)) cacheMap[p.Path] = p.ImagePath;

            foreach (string rootDir in _settings.ProjectDirectories)
            {
                if (!Directory.Exists(rootDir)) continue;

                if (Directory.Exists(Path.Combine(rootDir, "ProjectSettings")))
                {
                    TryAddProject(rootDir, cacheMap, detectedProjects);
                    continue;
                }

                try
                {
                    foreach (string subDir in Directory.GetDirectories(rootDir))
                    {
                        if (Directory.Exists(Path.Combine(subDir, "ProjectSettings")))
                        {
                            TryAddProject(subDir, cacheMap, detectedProjects);
                        }
                        else
                        {
                            try
                            {
                                foreach (string deepDir in Directory.GetDirectories(subDir))
                                {
                                    if (Directory.Exists(Path.Combine(deepDir, "ProjectSettings")))
                                        TryAddProject(deepDir, cacheMap, detectedProjects);
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch { }
            }

            SaveProjectsToCache(detectedProjects);
            return detectedProjects;
        }

        private void TryAddProject(string path, Dictionary<string, string?> cacheMap, List<ProjectItem> list)
        {
            var item = CreateProjectItem(path);
            if (item != null)
            {
                if (cacheMap.TryGetValue(item.Path, out string? imgPath) && File.Exists(imgPath))
                    item.ImagePath = imgPath;
                list.Add(item);
            }
        }

        private ProjectItem? CreateProjectItem(string projectPath)
        {
            try
            {
                string versionPath = Path.Combine(projectPath, "ProjectSettings", "ProjectVersion.txt");
                string version = "Unknown";

                if (File.Exists(versionPath))
                {
                    string[] lines = File.ReadAllLines(versionPath);
                    foreach (string line in lines)
                    {
                        // ИСПРАВЛЕНО: Ищем строго строку m_EditorVersion и игнорируем строку с ревизией
                        if (line.StartsWith("m_EditorVersion:"))
                        {
                            int index = line.IndexOf(':');
                            if (index != -1)
                            {
                                version = line.Substring(index + 1).Trim();
                                break; // Нашли чистую версию — выходим из цикла
                            }
                        }
                    }
                }

                var dirInfo = new DirectoryInfo(projectPath);
                var item = new ProjectItem
                {
                    Name = dirInfo.Name,
                    Path = projectPath,
                    Version = version,
                    Modified = dirInfo.LastWriteTime,
                    Platform = "Win64"
                };

                string[] extensions = { "*.png", "*.jpg", "*.jpeg" };
                foreach (var ext in extensions)
                {
                    foreach (var file in Directory.GetFiles(projectPath, ext))
                    {
                        item.AvailableImages.Add(file);
                    }
                }

                if (string.IsNullOrEmpty(item.ImagePath) && item.AvailableImages.Count > 0)
                {
                    item.ImagePath = item.AvailableImages[0];
                }

                return item;
            }
            catch { return null; }
        }

    }
}
