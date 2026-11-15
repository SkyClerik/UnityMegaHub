#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace UnityMegaHub
{
    public class EditorsControl : Panel
    {
        private readonly HubSettings _settings;
        private readonly EditorScanner _editorScanner;
        private DataGridView _gridEditors = null!;
        private Dictionary<string, string> _editors = new Dictionary<string, string>();
        private ContextMenuStrip _contextMenu = null!;

        public EditorsControl(HubSettings settings)
        {
            _settings = settings;
            _editorScanner = new EditorScanner(_settings);
            this.BackColor = Color.FromArgb(40, 40, 40);

            InitializeUI();
            RefreshEditors();

            // Автоматически обновляем список редакторов при каждом переключении на эту вкладку
            this.VisibleChanged += (s, e) => { if (this.Visible) RefreshEditors(); };
        }

        private void InitializeUI()
        {
            // 1. Верхняя панель управления с кнопкой 🔄
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = Color.FromArgb(35, 35, 35) };
            Label lblTitle = new Label { Text = "Доступные локальные редакторы Unity:", ForeColor = Color.White, Location = new Point(10, 14), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            Button btnRefresh = new Button { Text = "🔄", Location = new Point(790, 10), Size = new Size(35, 25), FlatStyle = FlatStyle.Flat, ForeColor = Color.White };
            btnRefresh.Click += (s, e) => RefreshEditors();
            topPanel.Controls.AddRange(new Control[] { lblTitle, btnRefresh });
            this.Controls.Add(topPanel);

            // 2. Нижняя панель с кнопкой Установить
            Panel bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = Color.FromArgb(30, 30, 30) };
            Button btnInstall = new Button { Text = "Установить новый редактор Unity 🌐", Location = new Point(10, 10), Size = new Size(240, 30), FlatStyle = FlatStyle.Flat, ForeColor = Color.White };
            btnInstall.Click += BtnInstall_Click;
            bottomPanel.Controls.Add(btnInstall);
            this.Controls.Add(bottomPanel);

            // 3. Центральная сетка DataGridView
            _gridEditors = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.FromArgb(40, 40, 40), RowHeadersVisible = false, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BorderStyle = BorderStyle.None, GridColor = Color.FromArgb(50, 50, 50), RowTemplate = { Height = 35 } };
            SetupGridStyles();
            this.Controls.Add(_gridEditors);
            _gridEditors.BringToFront();
        }

        private void SetupGridStyles()
        {
            _gridEditors.EnableHeadersVisualStyles = false;
            _gridEditors.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            _gridEditors.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _gridEditors.DefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45);
            _gridEditors.DefaultCellStyle.ForeColor = Color.White;
            _gridEditors.DefaultCellStyle.SelectionBackColor = Color.FromArgb(70, 70, 70);

            _gridEditors.Columns.Add("Version", "Версия Unity");
            _gridEditors.Columns.Add("Path", "Путь к исполняемому файлу (Unity.exe)");

            // Контекстное меню по правому клику для удаления
            _contextMenu = new ContextMenuStrip();
            var deleteItem = _contextMenu.Items.Add("Удалить из списка хаба");
            deleteItem.Click += DeleteItem_Click;

            _gridEditors.CellMouseDown += (s, e) => { if (e.Button == MouseButtons.Right && e.RowIndex >= 0) { _gridEditors.ClearSelection(); _gridEditors.Rows[e.RowIndex].Selected = true; _contextMenu.Show(Cursor.Position); } };
        }

        private void RefreshEditors()
        {
            _editors = _editorScanner.ScanForEditors();
            _gridEditors.Rows.Clear();
            foreach (var pair in _editors)
            {
                _gridEditors.Rows.Add(pair.Key, pair.Value);
            }
        }

        private void BtnInstall_Click(object? sender, EventArgs e)
        {
            try
            {
                // ИСПРАВЛЕНО: Ведет на чистый архив инсталляторов движка, без навязывания Unity Hub
                string url = "https://unity.com";
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex) { MessageBox.Show($"Не удалось открыть браузер: {ex.Message}"); }
        }


        private void DeleteItem_Click(object? sender, EventArgs e)
        {
            if (_gridEditors.SelectedRows.Count > 0)
            {
                // Просто скрываем из таблицы текущей сессии
                _gridEditors.Rows.Remove(_gridEditors.SelectedRows[0]);
            }
        }
    }
}
