#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace UnityMegaHub
{
    [System.ComponentModel.DesignerCategory("")]
    public class TemplatesControl : UserControl
    {
        private readonly HubSettings _settings;
        private DataGridView _gridTemplates = null!;
        private List<TemplateItem> _templates = new List<TemplateItem>();
        private Bitmap _defaultIcon = null!;
        private ImagePopupForm _popupForm = null!;
        private ContextMenuStrip _contextMenu = null!;
        private readonly Action _onProjectCreated;

        public TemplatesControl(HubSettings settings, Action onProjectCreated)
        {
            _settings = settings;
            _onProjectCreated = onProjectCreated;
            this.BackColor = Color.FromArgb(40, 40, 40);
            _popupForm = new ImagePopupForm();

            CreateDefaultIcon();
            InitializeUI();
            LoadTemplates();

            this.VisibleChanged += (s, e) => { if (this.Visible) LoadTemplates(); };
        }

        private void CreateDefaultIcon()
        {
            _defaultIcon = new Bitmap(32, 32);
            using (Graphics g = Graphics.FromImage(_defaultIcon))
            {
                g.Clear(Color.FromArgb(55, 70, 55));
                using (Pen pen = new Pen(Color.DarkGreen, 2)) g.DrawRectangle(pen, 4, 4, 24, 24);
            }
        }

        private void InitializeUI()
        {
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = Color.FromArgb(35, 35, 35) };
            Label lblTitle = new Label { Text = "Доступные кастомные шаблоны проектов:", ForeColor = Color.White, Location = new Point(10, 14), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            Button btnRefresh = new Button { Text = "🔄", Location = new Point(790, 10), Size = new Size(35, 25), FlatStyle = FlatStyle.Flat, ForeColor = Color.White };
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right; // ИСПРАВЛЕНО
            btnRefresh.Click += (s, e) => LoadTemplates();
            topPanel.Controls.AddRange(new Control[] { lblTitle, btnRefresh });
            this.Controls.Add(topPanel);

            _gridTemplates = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.FromArgb(40, 40, 40), RowHeadersVisible = false, AllowUserToAddRows = false, ReadOnly = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BorderStyle = BorderStyle.None, GridColor = Color.FromArgb(50, 50, 50), RowTemplate = { Height = 45 } };
            SetupGridStyles();
            this.Controls.Add(_gridTemplates);
            _gridTemplates.BringToFront();
        }

        private void SetupGridStyles()
        {
            _gridTemplates.EnableHeadersVisualStyles = false;
            _gridTemplates.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            _gridTemplates.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _gridTemplates.DefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45);
            _gridTemplates.DefaultCellStyle.ForeColor = Color.White;
            _gridTemplates.DefaultCellStyle.SelectionBackColor = Color.FromArgb(60, 80, 60);

            _gridTemplates.Columns.Add(new DataGridViewImageColumn { Name = "Icon", HeaderText = "", Image = _defaultIcon, Width = 40, ImageLayout = DataGridViewImageCellLayout.Zoom, ReadOnly = true });
            var btnCol = new DataGridViewButtonColumn { Name = "Create", HeaderText = "", Width = 80, FlatStyle = FlatStyle.Flat };
            btnCol.DefaultCellStyle.BackColor = Color.FromArgb(45, 75, 45);
            _gridTemplates.Columns.Add(btnCol);

            _gridTemplates.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Template Name", ReadOnly = true });
            _gridTemplates.Columns.Add(new DataGridViewTextBoxColumn { Name = "Version", HeaderText = "Target Unity", ReadOnly = true });
            _gridTemplates.Columns.Add(new DataGridViewTextBoxColumn { Name = "Desc", HeaderText = "Description", ReadOnly = true });

            _gridTemplates.CellMouseEnter += (s, e) => HandleMouseHover(e, true);
            _gridTemplates.CellMouseLeave += (s, e) => HandleMouseHover(e, false);
            _gridTemplates.CellContentClick += GridTemplates_CellContentClick;

            // Настраиваем правый клик через наше новое контекстное меню
            _contextMenu = TemplateContextMenu.Create(GetSelectedTemplate, ExecuteCreation, () => { var t = GetSelectedTemplate(); if (t != null) { _templates.Remove(t); RenderRows(); } }, LoadTemplates);
            _gridTemplates.CellMouseDown += (s, e) => { if (e.Button == MouseButtons.Right && e.RowIndex >= 0) { _gridTemplates.ClearSelection(); _gridTemplates.Rows[e.RowIndex].Selected = true; _contextMenu.Show(Cursor.Position); } };
        }

        private TemplateItem? GetSelectedTemplate() =>
            _gridTemplates.SelectedRows.Count == 0
                ? null
                : _templates.Find(t => t.ZipFilePath == _gridTemplates.SelectedRows[0].Cells["Desc"].Tag?.ToString());

        public void LoadTemplates() { _templates = TemplateManager.GetAvailableTemplates(_settings); RenderRows(); }

        private void RenderRows()
        {
            _gridTemplates.Rows.Clear();
            foreach (var t in _templates)
            {
                Image icon = _defaultIcon;
                // ИСПРАВЛЕНО: Безопасное чтение файла картинки через поток памяти без блокировки файла на диске
                if (!string.IsNullOrEmpty(t.ImagePath) && File.Exists(t.ImagePath))
                {
                    try
                    {
                        using (var stream = new FileStream(t.ImagePath, FileMode.Open, FileAccess.Read))
                        {
                            icon = Image.FromStream(stream);
                        }
                    }
                    catch { }
                }
                int rowIndex = _gridTemplates.Rows.Add(icon, "Создать...", t.Name, t.TargetUnityVersion, t.Description);
                _gridTemplates.Rows[rowIndex].Cells["Desc"].Tag = t.ZipFilePath;
            }
        }

        private void HandleMouseHover(DataGridViewCellEventArgs e, bool isEnter)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 0 && isEnter)
            {
                var t = _templates[e.RowIndex];
                Image img = _defaultIcon;

                // ИСПРАВЛЕНО: Безопасное чтение для превью-картинки всплывающего окна
                if (!string.IsNullOrEmpty(t.ImagePath) && File.Exists(t.ImagePath))
                {
                    try
                    {
                        using (var stream = new FileStream(t.ImagePath, FileMode.Open, FileAccess.Read))
                        {
                            img = Image.FromStream(stream);
                        }
                    }
                    catch { }
                }
                _popupForm.ShowImage(img, new Point(Cursor.Position.X + 20, Cursor.Position.Y + 10));
            }
            else _popupForm.Hide();
        }

        private void GridTemplates_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && _gridTemplates.Columns[e.ColumnIndex].Name == "Create")
            {
                // Принудительно выделяем строку, по кнопке которой кликнули
                _gridTemplates.ClearSelection();
                _gridTemplates.Rows[e.RowIndex].Selected = true;
                ExecuteCreation();
            }
        }


        private void ExecuteCreation()
        {
            var template = GetSelectedTemplate();
            if (template == null) return;

            // ИСПРАВЛЕНО: Передаем _settings, чтобы форма вытянула дефолтную папку
            using (var form = new CreateProjectForm(template, _settings))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Directory.CreateDirectory(form.FullPath);
                        if (!string.IsNullOrEmpty(template.ZipFilePath) && File.Exists(template.ZipFilePath)) ZipFile.ExtractToDirectory(template.ZipFilePath, form.FullPath);
                        MessageBox.Show($"Проект успешно развернут по пути:\n{form.FullPath}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        _onProjectCreated.Invoke();
                    }
                    catch (Exception ex) { MessageBox.Show($"Ошибка при распаковке: {ex.Message}"); }
                }
            }
        }
    }
}
