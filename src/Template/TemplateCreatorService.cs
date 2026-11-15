#nullable enable
using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace UnityMegaHub
{
    public class TemplateCreatorService
    {
        public static void CreateFromProject(ProjectItem proj, HubSettings settings, Action refreshTemplatesUI)
        {
            if (settings.TemplateDirectories.Count == 0)
            {
                MessageBox.Show("В настройках не указана папка для шаблонов!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string targetDir = settings.TemplateDirectories[0];

            using (Form inputForm = new Form())
            {
                inputForm.Text = "Создание нового шаблона";
                inputForm.Size = new System.Drawing.Size(380, 260); // Чуть увеличили высоту для запаса
                inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputForm.StartPosition = FormStartPosition.CenterParent;
                inputForm.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);

                Label lblName = new Label { Text = "Имя шаблона:", ForeColor = System.Drawing.Color.White, Location = new System.Drawing.Point(20, 15), Width = 200 };
                TextBox txtName = new TextBox { Text = proj.Name + "_Template", Location = new System.Drawing.Point(20, 35), Width = 320, BackColor = System.Drawing.Color.FromArgb(50, 50, 50), ForeColor = System.Drawing.Color.White, BorderStyle = BorderStyle.FixedSingle };

                Label lblDesc = new Label { Text = "Описание (что внутри):", ForeColor = System.Drawing.Color.White, Location = new System.Drawing.Point(20, 75), Width = 200 };

                // ИСПРАВЛЕНО: Сначала включаем Multiline, затем задаем высоту, чтобы текст не наползал
                TextBox txtDesc = new TextBox { Multiline = true, Location = new System.Drawing.Point(20, 95), Width = 320, Height = 60, BackColor = System.Drawing.Color.FromArgb(50, 50, 50), ForeColor = System.Drawing.Color.White, BorderStyle = BorderStyle.FixedSingle };
                txtDesc.Text = "Кастомная архитектура, готовые скрипты.";

                Button btnOk = new Button { Text = "ОК", DialogResult = DialogResult.OK, Location = new System.Drawing.Point(150, 180), Size = new System.Drawing.Size(90, 28), FlatStyle = FlatStyle.Flat, ForeColor = System.Drawing.Color.White };
                Button btnCan = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel, Location = new System.Drawing.Point(250, 180), Size = new System.Drawing.Size(90, 28), FlatStyle = FlatStyle.Flat, ForeColor = System.Drawing.Color.White };

                inputForm.Controls.AddRange(new Control[] { lblName, txtName, lblDesc, txtDesc, btnOk, btnCan });
                inputForm.AcceptButton = btnOk;

                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    string templateName = txtName.Text.Trim().Replace(" ", "_");
                    if (string.IsNullOrEmpty(templateName)) return;

                    string zipPath = Path.Combine(targetDir, templateName + ".zip");
                    if (File.Exists(zipPath))
                    {
                        MessageBox.Show("Шаблон с таким именем уже существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    Cursor.Current = Cursors.WaitCursor;
                    try
                    {
                        using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                        {
                            // ИСПРАВЛЕНО: Пакуем описание в текстовый файл прямо внутрь zip-архива
                            var descEntry = archive.CreateEntry("about.txt");
                            using (var writer = new StreamWriter(descEntry.Open()))
                            {
                                writer.Write(txtDesc.Text.Trim());
                            }

                            string[] foldersToPack = { "Assets", "Packages", "ProjectSettings" };
                            foreach (var folder in foldersToPack)
                            {
                                string sourceFolder = Path.Combine(proj.Path, folder);
                                if (Directory.Exists(sourceFolder)) PackDirectoryToZip(sourceFolder, archive, folder);
                            }
                        }

                        if (!string.IsNullOrEmpty(proj.ImagePath) && File.Exists(proj.ImagePath))
                        {
                            File.Copy(proj.ImagePath, Path.Combine(targetDir, templateName + Path.GetExtension(proj.ImagePath)), true);
                        }

                        MessageBox.Show($"Шаблон успешно создан!", "Успех!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        refreshTemplatesUI.Invoke();
                    }
                    catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}"); }
                    finally { Cursor.Current = Cursors.Default; }
                }
            }
        }

        private static void PackDirectoryToZip(string sourceDir, ZipArchive archive, string baseFolderInZip)
        {
            string[] files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string relativePath = file.Substring(sourceDir.Length + 1);
                string entryName = Path.Combine(baseFolderInZip, relativePath).Replace("\\", "/");
                archive.CreateEntryFromFile(file, entryName);
            }
        }
    }
}
