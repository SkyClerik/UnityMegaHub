#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace UnityMegaHub
{
    [System.ComponentModel.DesignerCategory("")]
    public class ProjectsControl : UserControl
    {
        private readonly HubSettings _settings;
        private readonly ProjectScanner _scanner;
        private readonly EditorScanner _editorScanner;
        private TextBox _searchBox = null!;
        private DataGridView _gridProjects = null!;
        private List<ProjectItem> _allProjects = new List<ProjectItem>();
        private Dictionary<string, string> _cachedEditors = new Dictionary<string, string>();
        private Bitmap _defaultProjectIcon = null!;
        private ImagePopupForm _popupForm = null!;
        private ContextMenuStrip _bottomEditorsMenu = null!;
        private ContextMenuStrip _rowContextMenu = null!;

        public ProjectsControl(HubSettings settings)
        {
            _settings = settings;
            _scanner = new ProjectScanner(_settings);
            _editorScanner = new EditorScanner(_settings);
            this.BackColor = Color.FromArgb(40, 40, 40);
            CreateDefaultIcon();
            InitializeUI();
            _popupForm = new ImagePopupForm();
            LoadProjectsFromCache();
            RefreshEditorsCache();
        }

        private void CreateDefaultIcon()
        {
            _defaultProjectIcon = new Bitmap(32, 32);
            using (Graphics g = Graphics.FromImage(_defaultProjectIcon))
            {
                g.Clear(Color.FromArgb(65, 65, 65));
                using (Pen pen = new Pen(Color.DarkGray, 2)) g.DrawRectangle(pen, 4, 4, 24, 24);
            }
        }

        private void InitializeUI()
        {
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = Color.FromArgb(35, 35, 35) };

            // 1. Поиск (X = 10, Ширина = 250 -> заканчивается на 260px)
            _searchBox = new TextBox { Location = new Point(10, 12), Width = 250, BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            _searchBox.TextChanged += (s, e) => ApplyFilter();

            // 2. Кнопка "Add Project.." (X = 275, ставим чуть правее поиска)
            Button btnAddProject = new Button { Text = "Add Project..", Location = new Point(275, 10), Size = new Size(100, 25), FlatStyle = FlatStyle.Flat, ForeColor = Color.White };
            // Убираем Anchor.Right, по умолчанию останется надежная привязка к левому краю
            btnAddProject.Click += BtnAddProject_Click;

            // 3. Кнопка "Обновить" (X = 385, ставим сразу после Add Project)
            Button btnRefresh = new Button { Text = "🔄", Location = new Point(385, 10), Size = new Size(35, 25), FlatStyle = FlatStyle.Flat, ForeColor = Color.White };
            btnRefresh.Click += (s, e) => RefreshProjectList();

            topPanel.Controls.AddRange(new Control[] { _searchBox, btnAddProject, btnRefresh });
            this.Controls.Add(topPanel);

            Panel bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = Color.FromArgb(30, 30, 30) };
            Button btnRunUnity = new Button { Text = "Run Unity Editor ▼", Location = new Point(10, 10), Size = new Size(150, 30), FlatStyle = FlatStyle.Flat, ForeColor = Color.White };

            _bottomEditorsMenu = new ContextMenuStrip();
            btnRunUnity.Click += (s, e) =>
            {
                var menu = ProjectContextMenu.CreateEditorsOnlyMenu(_cachedEditors);
                menu.Show(btnRunUnity, new Point(0, btnRunUnity.Height));
            };

            bottomPanel.Controls.Add(btnRunUnity);
            this.Controls.Add(bottomPanel);

            _gridProjects = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(40, 40, 40),
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(50, 50, 50),
                RowTemplate = { Height = 40 }
            };

            SetupGridDarkTheme();
            this.Controls.Add(_gridProjects);
            _gridProjects.BringToFront();
        }

        private void SetupGridDarkTheme()
        {
            _gridProjects.EnableHeadersVisualStyles = false;
            _gridProjects.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            _gridProjects.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _gridProjects.DefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45);
            _gridProjects.DefaultCellStyle.ForeColor = Color.White;
            _gridProjects.DefaultCellStyle.SelectionBackColor = Color.FromArgb(70, 70, 70);

            _gridProjects.Columns.Add(new DataGridViewImageColumn { Name = "Icon", HeaderText = "", Image = _defaultProjectIcon, Width = 40, ImageLayout = DataGridViewImageCellLayout.Zoom, ReadOnly = true });
            var btnCol = new DataGridViewButtonColumn { Name = "NextImg", HeaderText = "", Width = 40, FlatStyle = FlatStyle.Flat };
            btnCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            btnCol.DefaultCellStyle.BackColor = Color.FromArgb(55, 55, 55);
            _gridProjects.Columns.Add(btnCol);

            _gridProjects.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Project Name", ReadOnly = true });
            _gridProjects.Columns.Add(new DataGridViewTextBoxColumn { Name = "Version", HeaderText = "Unity Version", ReadOnly = true });
            _gridProjects.Columns.Add(new DataGridViewTextBoxColumn { Name = "Path", HeaderText = "Path", ReadOnly = true });
            _gridProjects.Columns.Add(new DataGridViewTextBoxColumn { Name = "Modified", HeaderText = "Last Modified", ReadOnly = true });

            _gridProjects.CellMouseEnter += GridProjects_CellMouseEnter;
            _gridProjects.CellMouseLeave += GridProjects_CellMouseLeave;
            _gridProjects.CellContentClick += GridProjects_CellContentClick;
            _gridProjects.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) { var p = GetSelectedProject(); if (p != null) ProjectLauncherService.Launch(p, _cachedEditors, () => { }); } };

            _rowContextMenu = ProjectContextMenu.Create(GetSelectedProject, DeleteSelectedProject, p => ProjectLauncherService.Launch(p, _cachedEditors, () => { }), () => _cachedEditors, _settings, () => { if (this.ParentForm is MainForm mainForm) mainForm.RefreshTemplatesTab(); });

            _gridProjects.CellMouseDown += (s, e) => { if (e.Button == MouseButtons.Right && e.RowIndex >= 0) { _gridProjects.ClearSelection(); _gridProjects.Rows[e.RowIndex].Selected = true; _rowContextMenu.Show(Cursor.Position); } };
        }

        private ProjectItem? GetSelectedProject() => _gridProjects.SelectedRows.Count == 0 ? null : _allProjects.Find(p => p.Path == _gridProjects.SelectedRows[0].Cells["Path"].Value?.ToString());
        private void RefreshEditorsCache() => _cachedEditors = _editorScanner.ScanForEditors();
        private void LoadProjectsFromCache() { _allProjects = _scanner.LoadCachedProjects(); ApplyFilter(); }
        private void RefreshProjectList() { _allProjects = _scanner.ScanDirectories(); RefreshEditorsCache(); ApplyFilter(); }

        private void DeleteSelectedProject() { var proj = GetSelectedProject(); if (proj != null) { _allProjects.Remove(proj); _scanner.SaveProjectsToCache(_allProjects); ApplyFilter(); } }

        private void BtnAddProject_Click(object? sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                // Принудительно отключаем кэширование старого пути в Windows
                fbd.AddToRecent = false;

                // Проверяем первый сохраненный путь проектов из нашей таблицы параметров
                if (_settings.ProjectDirectories.Count > 0)
                {
                    string targetPath = _settings.ProjectDirectories[0];
                    if (Directory.Exists(targetPath))
                    {
                        // ИСПРАВЛЕНО: Гарантируем наличие слэша '\' на конце пути, чтобы диалог заходил внутрь папки
                        if (!targetPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                        {
                            targetPath += Path.DirectorySeparatorChar;
                        }
                        fbd.SelectedPath = targetPath;
                    }
                }

                else if (Directory.Exists(@"D:\Developing"))
                {
                    fbd.SelectedPath = @"D:\Developing";
                }

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    _settings.AddPath(fbd.SelectedPath, "project");
                    RefreshProjectList();
                }
            }
        }

        private void GridProjects_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && _gridProjects.Columns[e.ColumnIndex].Name == "NextImg")
            {
                var proj = _allProjects.Find(p => p.Path == _gridProjects.Rows[e.RowIndex].Cells["Path"].Value?.ToString());
                if (proj != null && proj.AvailableImages.Count > 1)
                {
                    proj.ImagePath = proj.AvailableImages[(proj.AvailableImages.IndexOf(proj.ImagePath ?? "") + 1) % proj.AvailableImages.Count];
                    _scanner.SaveProjectsToCache(_allProjects);
                    try { _gridProjects.Rows[e.RowIndex].Cells["Icon"].Value = Image.FromFile(proj.ImagePath); } catch { }
                }
            }
        }

        private void GridProjects_CellMouseEnter(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 0)
            {
                var proj = _allProjects.Find(p => p.Path == _gridProjects.Rows[e.RowIndex].Cells["Path"].Value?.ToString());
                if (proj != null)
                {
                    Image targetImg = _defaultProjectIcon;
                    if (!string.IsNullOrEmpty(proj.ImagePath) && File.Exists(proj.ImagePath)) { try { targetImg = Image.FromFile(proj.ImagePath); } catch { } }
                    _popupForm.ShowImage(targetImg, new Point(Cursor.Position.X + 20, Cursor.Position.Y + 10));
                }
            }
        }

        private void GridProjects_CellMouseLeave(object? sender, DataGridViewCellEventArgs e) => _popupForm.Hide();

        private void ApplyFilter()
        {
            _gridProjects.Rows.Clear();
            string filter = _searchBox.Text.ToLower();
            foreach (var proj in _allProjects)
            {
                if (string.IsNullOrEmpty(filter) || proj.Name.ToLower().Contains(filter) || proj.Path.ToLower().Contains(filter))
                {
                    Image rowIcon = _defaultProjectIcon;
                    if (!string.IsNullOrEmpty(proj.ImagePath) && File.Exists(proj.ImagePath)) { try { rowIcon = Image.FromFile(proj.ImagePath); } catch { } }
                    _gridProjects.Rows.Add(rowIcon, ">", proj.Name, proj.Version, proj.Path, proj.ModifiedDisplay);
                }
            }
        }

        public void UpdateProjectsData() => RefreshProjectList();
    }
}
