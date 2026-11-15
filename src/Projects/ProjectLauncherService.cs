#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace UnityMegaHub
{
    public class ProjectLauncherService
    {
        public static void Launch(ProjectItem proj, Dictionary<string, string> cachedEditors, Action triggerMenuShow)
        {
            string cleanProjectVersion = proj.Version.Trim().Replace("\r", "").Replace("\n", "");
            string? foundExePath = null; string? fallbackExePath = null; string fallbackVersion = string.Empty;

            foreach (var pair in cachedEditors)
            {
                string cleanEditorVersion = pair.Key.Trim();
                string baseProject = cleanProjectVersion.Split('f', 'b', 'a')[0].Trim('.');
                string baseEditor = cleanEditorVersion.Split('f', 'b', 'a')[0].Trim('.');

                if (baseProject.Equals(baseEditor, StringComparison.OrdinalIgnoreCase) || cleanEditorVersion.StartsWith(baseProject, StringComparison.OrdinalIgnoreCase))
                { foundExePath = pair.Value; break; }

                if (cleanProjectVersion.Split('.').Length >= 2 && cleanEditorVersion.Split('.').Length >= 2 && cleanProjectVersion.Split('.')[0] == cleanEditorVersion.Split('.')[0] && cleanProjectVersion.Split('.')[1] == cleanEditorVersion.Split('.')[1])
                { fallbackExePath = pair.Value; fallbackVersion = pair.Key; }
            }

            if (foundExePath == null && fallbackExePath != null)
            {
                if (MessageBox.Show($"Точная версия {cleanProjectVersion} не найдена.\nЗапустить на ближайшей версии: {fallbackVersion}?", "Авто-подбор редактора", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) != DialogResult.OK)
                    return;
                foundExePath = fallbackExePath;
            }

            if (foundExePath != null) LaunchDirect(foundExePath, proj.Path);
            else
            {
                MessageBox.Show($"Подходящая версия Unity для проекта не найдена в путях. Выберите редактор вручную.");
                triggerMenuShow.Invoke();
            }
        }

        public static void LaunchDirect(string exePath, string? projectPath)
        {
            try
            {
                var startInfo = new ProcessStartInfo(exePath);

                if (!string.IsNullOrEmpty(projectPath))
                {
                    // Обычный запуск конкретного рабочего проекта
                    startInfo.Arguments = $"-projectPath \"{projectPath}\"";
                }
                else
                {
                    // ИСПРАВЛЕНО: Генерируем путь к временной пустой папке в Temp
                    string tempEmptyProjectPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "UnityMegaHub_EmptyTemplate");

                    // Если папки нет, движок сам создаст там чистую структуру при старте
                    startInfo.Arguments = $"-createProject \"{tempEmptyProjectPath}\"";
                }

                Process.Start(startInfo);
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка запуска: {ex.Message}"); }
        }

    }
}
