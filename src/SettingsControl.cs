#nullable enable
using System;
using System.Drawing;
using System.Windows.Forms;

namespace UnityMegaHub
{
    [System.ComponentModel.DesignerCategory("")]
    public class SettingsControl : UserControl
    {
        private readonly HubSettings _settings;
        private ListBox _lstEditors = null!;
        private ListBox _lstProjects = null!;
        private ListBox _lstTemplates = null!; // ИСПРАВЛЕНО: Поле для списка шаблонов

        public SettingsControl(HubSettings settings)
        {
            _settings = settings;
            this.BackColor = Color.FromArgb(40, 40, 40);
            InitializeUI();
        }

        private void InitializeUI()
        {
            Label lblEditors = new Label { Text = "Пути к редакторам Unity:", ForeColor = Color.White, Location = new Point(20, 20), Width = 200 };
            _lstEditors = new ListBox { Location = new Point(20, 45), Size = new Size(400, 75), BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White };
            Button btnAddEditor = new Button { Text = "Добавить", Location = new Point(430, 45), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAddEditor.Click += (s, e) => RequestAndSavePath(_lstEditors, "editor");

            Label lblProjects = new Label { Text = "Папки с проектами:", ForeColor = Color.White, Location = new Point(20, 140), Width = 200 };
            _lstProjects = new ListBox { Location = new Point(20, 165), Size = new Size(400, 75), BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White };
            Button btnAddProject = new Button { Text = "Добавить", Location = new Point(430, 165), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAddProject.Click += (s, e) => RequestAndSavePath(_lstProjects, "project");

            // ИСПРАВЛЕНО: Добавили UI элементы для секции шаблонов
            Label lblTemplates = new Label { Text = "Папки с шаблонами (.zip):", ForeColor = Color.White, Location = new Point(20, 260), Width = 200 };
            _lstTemplates = new ListBox { Location = new Point(20, 285), Size = new Size(400, 75), BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White };
            Button btnAddTemplate = new Button { Text = "Добавить", Location = new Point(430, 285), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAddTemplate.Click += (s, e) => RequestAndSavePath(_lstTemplates, "template");

            _settings.EditorDirectories.ForEach(p => _lstEditors.Items.Add(p));
            _settings.ProjectDirectories.ForEach(p => _lstProjects.Items.Add(p));
            _settings.TemplateDirectories.ForEach(p => _lstTemplates.Items.Add(p)); // ИСПРАВЛЕНО: Заполняем при старте

            this.Controls.AddRange(new Control[] {
                lblEditors, _lstEditors, btnAddEditor,
                lblProjects, _lstProjects, btnAddProject,
                lblTemplates, _lstTemplates, btnAddTemplate // ИСПРАВЛЕНО
            });
        }

        private void RequestAndSavePath(ListBox listBox, string type)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.AddToRecent = false;
                string startPath = string.Empty;

                if (type == "project" && _settings.ProjectDirectories.Count > 0)
                    startPath = _settings.ProjectDirectories[0];
                else if (type == "editor" && _settings.EditorDirectories.Count > 0)
                    startPath = _settings.EditorDirectories[0];
                else if (type == "template" && _settings.TemplateDirectories.Count > 0)
                    startPath = _settings.TemplateDirectories[0];

                if (!string.IsNullOrEmpty(startPath) && Directory.Exists(startPath))
                {
                    // ИСПРАВЛЕНО: Добавляем финальный слэш, чтобы принудительно зайти внутрь целевой папки
                    if (!startPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    {
                        startPath += Path.DirectorySeparatorChar;
                    }
                    fbd.SelectedPath = startPath;
                }

                else if (Directory.Exists(@"D:\Developing"))
                {
                    fbd.SelectedPath = @"D:\Developing";
                }

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    _settings.AddPath(fbd.SelectedPath, type);
                    listBox.Items.Add(fbd.SelectedPath);
                }
            }
        }

    }
}
