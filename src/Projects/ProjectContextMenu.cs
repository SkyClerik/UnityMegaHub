#nullable enable
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace UnityMegaHub
{
    public class ProjectContextMenu
    {
        public static ContextMenuStrip Create(Func<ProjectItem?> getSelectedProj, Action deleteAction, Action<ProjectItem> launchAction, Func<Dictionary<string, string>> getEditors, HubSettings settings, Action refreshTemplates)
        {
            var menu = new ContextMenuStrip();

            var runItem = menu.Items.Add("Запустить проект");
            runItem.Click += (s, e) => { var p = getSelectedProj(); if (p != null) launchAction(p); };

            // ИСПРАВЛЕНО: Добавили кнопку создания кастомного шаблона
            var makeTemplateItem = menu.Items.Add("Создать шаблон из проекта");
            makeTemplateItem.Click += (s, e) => { var p = getSelectedProj(); if (p != null) TemplateCreatorService.CreateFromProject(p, settings, refreshTemplates); };

            var deleteItem = menu.Items.Add("Удалить из списка хаба");
            deleteItem.Click += (s, e) => deleteAction();

            var openWithItem = new ToolStripMenuItem("Открыть с помощью...");
            menu.Items.Add(openWithItem);

            menu.Opening += (s, e) =>
            {
                openWithItem.DropDown.Items.Clear();
                var editors = getEditors();
                var proj = getSelectedProj();

                if (editors.Count == 0) { openWithItem.DropDown.Items.Add("Редакторы не найдены."); return; }

                foreach (var pair in editors)
                {
                    var item = openWithItem.DropDown.Items.Add($"{pair.Key}");
                    item.Click += (src, ev) => ProjectLauncherService.LaunchDirect(pair.Value, proj?.Path);
                }
            };

            return menu;
        }

        public static ContextMenuStrip CreateEditorsOnlyMenu(Dictionary<string, string> editors)
        {
            var menu = new ContextMenuStrip();
            if (editors.Count == 0) { menu.Items.Add("Редакторы не найдены. Проверьте пути в Settings."); return menu; }

            foreach (var pair in editors)
            {
                var item = menu.Items.Add($"{pair.Key}");
                item.Click += (src, ev) => ProjectLauncherService.LaunchDirect(pair.Value, null);
            }
            return menu;
        }
    }
}
