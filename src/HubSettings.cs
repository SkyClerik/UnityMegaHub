#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityMegaHub
{
    public class HubSettings
    {
        public List<string> EditorDirectories { get; private set; } = new List<string>();
        public List<string> ProjectDirectories { get; private set; } = new List<string>();
        public List<string> TemplateDirectories { get; private set; } = new List<string>(); // ИСПРАВЛЕНО: Новый список

        public HubSettings()
        {
            LoadSettings();
        }

        public void LoadSettings()
        {
            EditorDirectories.Clear();
            ProjectDirectories.Clear();
            TemplateDirectories.Clear();

            string editorsRaw = Properties.Settings.Default.EditorPaths;
            string projectsRaw = Properties.Settings.Default.ProjectPaths;
            string templatesRaw = Properties.Settings.Default.TemplatePaths; // ИСПРАВЛЕНО: Читаем шаблоны

            if (!string.IsNullOrEmpty(editorsRaw))
                EditorDirectories.AddRange(editorsRaw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));

            if (!string.IsNullOrEmpty(projectsRaw))
                ProjectDirectories.AddRange(projectsRaw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));

            if (!string.IsNullOrEmpty(templatesRaw))
                TemplateDirectories.AddRange(templatesRaw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
        }

        public void AddPath(string path, string type)
        {
            if (type == "editor" && !EditorDirectories.Contains(path))
            {
                EditorDirectories.Add(path);
                Properties.Settings.Default.EditorPaths = string.Join(";", EditorDirectories);
            }
            else if (type == "project" && !ProjectDirectories.Contains(path))
            {
                ProjectDirectories.Add(path);
                Properties.Settings.Default.ProjectPaths = string.Join(";", ProjectDirectories);
            }
            else if (type == "template" && !TemplateDirectories.Contains(path)) // ИСПРАВЛЕНО: Сохраняем шаблоны
            {
                TemplateDirectories.Add(path);
                Properties.Settings.Default.TemplatePaths = string.Join(";", TemplateDirectories);
            }

            Properties.Settings.Default.Save();
            LoadSettings();
        }
    }
}
