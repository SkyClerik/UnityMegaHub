#nullable enable
using System;
using System.IO;
using System.Windows.Forms;

namespace UnityMegaHub
{
    public class TemplateContextMenu
    {
        public static ContextMenuStrip Create(Func<TemplateItem?> getSelected, Action triggerCreate, Action hideFromList, Action reloadAll)
        {
            var menu = new ContextMenuStrip();

            var createItem = menu.Items.Add("Создать проект из шаблона");
            createItem.Click += (s, e) => triggerCreate.Invoke();

            var hideItem = menu.Items.Add("Удалить из списка хаба");
            hideItem.Click += (s, e) => hideFromList.Invoke();

            var deleteItem = menu.Items.Add("Удалить с устройства полностью");
            deleteItem.Click += (s, e) =>
            {
                var template = getSelected();
                if (template == null) return;

                var result = MessageBox.Show($"Вы уверены, что хотите НАВСЕГДА удалить файл шаблона {template.ZipFileName} с жесткого диска?",
                    "Удаление с устройства", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        // Удаляем сам архив .zip
                        if (!string.IsNullOrEmpty(template.ZipFilePath) && File.Exists(template.ZipFilePath))
                            File.Delete(template.ZipFilePath);

                        // Удаляем картинку-обложку, если она есть рядом
                        if (!string.IsNullOrEmpty(template.ImagePath) && File.Exists(template.ImagePath))
                            File.Delete(template.ImagePath);

                        reloadAll.Invoke();
                    }
                    catch (Exception ex) { MessageBox.Show($"Ошибка при удалении файла: {ex.Message}"); }
                }
            };

            return menu;
        }
    }
}
