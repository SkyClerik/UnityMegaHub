#nullable enable
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace UnityMegaHub
{
    public class CreateProjectForm : Form
    {
        private TextBox _txtName = null!;
        private TextBox _txtPath = null!;
        public string ProjectName => _txtName.Text.Trim();
        public string ProjectPath => _txtPath.Text.Trim();
        public string FullPath => Path.Combine(ProjectPath, ProjectName);

        public CreateProjectForm(TemplateItem template, HubSettings settings)
        {
            InitializeUI(template, settings);
        }

        private void InitializeUI(TemplateItem template, HubSettings settings)
        {
            this.Text = $"Создание проекта из: {template.Name}";
            this.Size = new Size(500, 240);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(40, 40, 40);

            Label lblName = new Label { Text = "Имя нового проекта:", ForeColor = Color.White, Location = new Point(20, 20), Width = 150 };
            _txtName = new TextBox { Location = new Point(20, 45), Width = 440, BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Text = "NewUnityProject" };

            Label lblPath = new Label { Text = "Папка для создания проекта:", ForeColor = Color.White, Location = new Point(20, 85), Width = 200 };
            _txtPath = new TextBox { Location = new Point(20, 110), Width = 340, BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            // ИСПРАВЛЕНО: Автоматически подставляем первую рабочую папку проектов из настроек
            if (settings.ProjectDirectories.Count > 0)
            {
                _txtPath.Text = settings.ProjectDirectories[0];
            }

            Button btnBrowse = new Button { Text = "Обзор...", Location = new Point(370, 108), Size = new Size(90, 24), FlatStyle = FlatStyle.Flat, ForeColor = Color.White };
            btnBrowse.Click += (s, e) => { using (FolderBrowserDialog fbd = new FolderBrowserDialog()) { if (fbd.ShowDialog() == DialogResult.OK) _txtPath.Text = fbd.SelectedPath; } };

            Button btnCreate = new Button { Text = "Создать", Location = new Point(260, 160), Size = new Size(90, 30), FlatStyle = FlatStyle.Flat, ForeColor = Color.White, DialogResult = DialogResult.OK };
            Button btnCancel = new Button { Text = "Отмена", Location = new Point(370, 160), Size = new Size(90, 30), FlatStyle = FlatStyle.Flat, ForeColor = Color.White, DialogResult = DialogResult.Cancel };
            btnCreate.Click += BtnCreate_Click;

            this.Controls.AddRange(new Control[] { lblName, _txtName, lblPath, _txtPath, btnBrowse, btnCreate, btnCancel });
            this.AcceptButton = btnCreate; this.CancelButton = btnCancel;
        }

        private void BtnCreate_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProjectName))
            {
                MessageBox.Show("Введите корректное имя проекта.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }
            if (!Directory.Exists(ProjectPath))
            {
                MessageBox.Show("Указанная папка для создания не существует.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }
        }
    }
}
