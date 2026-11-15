#nullable enable
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace UnityMegaHub
{
    [System.ComponentModel.DesignerCategory("")]
    public class UpdatesControl : Panel
    {
        private readonly HubSettings _settings;
        private ListView _lvEngines = null!;
        private Button _btnRefresh = null!;
        private Label _lblStatus = null!;

        private class UnityEngineInfo
        {
            public string Name { get; set; } = string.Empty;
            public string Version { get; set; } = string.Empty;
            public string YandexUrl { get; set; } = string.Empty;
        }

        // Каталог доступных дистрибутивов на Яндекс.Диске
        private readonly UnityEngineInfo[] _hardcodedEngines = new UnityEngineInfo[]
        {
            new UnityEngineInfo
            {
                Name = "Unity 6 (LTS)",
                Version = "6000.0.51f1",
                YandexUrl = "https://yandex.ru"
            },
            new UnityEngineInfo
            {
                Name = "Unity 2022 (LTS)",
                Version = "2022.3.50f1",
                YandexUrl = "https://yandex.ru"
            }
        };

        public UpdatesControl(HubSettings settings)
        {
            _settings = settings;
            //InitializeComponent();
            //RefreshEnginesList();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(40, 40, 40);

            var topPanel = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = Color.FromArgb(35, 35, 35) };
            var lblTitle = new Label
            {
                Text = "Редакции Unity в системе и доступные для загрузки в РФ (Яндекс.Диск):",
                ForeColor = Color.White,
                Location = new Point(10, 14),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            _btnRefresh = new Button
            {
                Text = "Обновить статус 🔄",
                Location = new Point(750, 10),
                Size = new Size(160, 25),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnRefresh.Click += (s, e) => RefreshEnginesList();

            topPanel.Controls.AddRange(new Control[] { lblTitle, _btnRefresh });

            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Color.FromArgb(30, 30, 30) };
            _lblStatus = new Label { Text = "Инициализация...", ForeColor = Color.LightGray, Location = new Point(10, 12), AutoSize = true };
            bottomPanel.Controls.Add(_lblStatus);

            _lvEngines = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                HeaderStyle = ColumnHeaderStyle.Nonclickable
            };

            _lvEngines.Columns.Add("Название движка", 200);
            _lvEngines.Columns.Add("Версия", 120);
            _lvEngines.Columns.Add("Статус в системе", 220);
            _lvEngines.Columns.Add("Действие", 300);

            _lvEngines.MouseDoubleClick += LvEngines_MouseDoubleClick;

            this.Controls.Add(_lvEngines);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(topPanel);

            this.VisibleChanged += (s, e) => { if (this.Visible) RefreshEnginesList(); };
        }

        private void RefreshEnginesList()
        {
            _lvEngines.BeginUpdate();
            _lvEngines.Items.Clear();

            _settings.LoadSettings();

            int installedCount = 0;
            var uniqueInstalledVersions = new System.Collections.Generic.HashSet<string>();

            // 1. ПРОСТО ВЫВОДИМ ВСЕ ДВИЖКИ, КОТОРЫЕ НАШЛИ НА ДИСКЕ
            if (_settings.EditorDirectories != null)
            {
                foreach (string rawDir in _settings.EditorDirectories)
                {
                    if (string.IsNullOrEmpty(rawDir)) continue;

                    string dir = rawDir.Replace("\r", "").Replace("\n", "").Trim().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    if (!Directory.Exists(dir)) continue;

                    try
                    {
                        // Рекурсивно находим каждый Unity.exe
                        string[] exeFiles = Directory.GetFiles(dir, "Unity.exe", SearchOption.AllDirectories);

                        foreach (string exePath in exeFiles)
                        {
                            // Находим папку версии (выходим из папки \Editor\ на уровень выше)
                            string? versionFolder = Path.GetDirectoryName(Path.GetDirectoryName(exePath));
                            if (string.IsNullOrEmpty(versionFolder)) continue;

                            // Имя этой папки и есть наша версия (например, 6000.0.51f1 или 2022.3.50f1)
                            string folderName = Path.GetFileName(versionFolder);

                            if (uniqueInstalledVersions.Add(folderName.ToLower()))
                            {
                                var item = new ListViewItem(folderName.StartsWith("6000") ? "Unity 6" : "Unity Editor");
                                item.SubItems.Add(folderName);
                                item.SubItems.Add("Установлен");
                                item.SubItems.Add("Уже установлен (Double-Click: открыть папку)");
                                item.ForeColor = Color.LightGreen;
                                item.Tag = versionFolder; // Запоминаем путь к папке для открытия

                                _lvEngines.Items.Add(item);
                                installedCount++;
                            }
                        }
                    }
                    catch { }
                }
            }

            // 2. ДОБАВЛЯЕМ В ТАБЛИЦУ ССЫЛКИ НА ТЕ ДВИЖКИ ИЗ КАТАЛОГА, КОТОРЫХ НЕТ НА ДИСКЕ
            foreach (var engine in _hardcodedEngines)
            {
                if (!uniqueInstalledVersions.Contains(engine.Version.ToLower().Trim()))
                {
                    var item = new ListViewItem(engine.Name);
                    item.SubItems.Add(engine.Version);
                    item.SubItems.Add("Не найден на ПК");
                    item.SubItems.Add("Надо качать (Double-Click: открыть Яндекс.Диск)");
                    item.ForeColor = Color.LightSalmon;
                    item.Tag = engine.YandexUrl; // Запоминаем ссылку для скачивания

                    _lvEngines.Items.Add(item);
                }
            }

            _lvEngines.EndUpdate();
            _lblStatus.Text = $"Проверка завершена. Найдено на ПК: {installedCount} редакторов.";
        }

        private void LvEngines_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (_lvEngines.SelectedItems.Count == 0) return;
            var selectedItem = _lvEngines.SelectedItems[0];
            if (selectedItem.Tag == null) return;

            string target = selectedItem.Tag.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(target)) return;

            if (target.StartsWith("http"))
            {
                Process.Start(new ProcessStartInfo { FileName = target, UseShellExecute = true });
            }
            else
            {
                if (Directory.Exists(target))
                {
                    Process.Start("explorer.exe", target);
                }
                else
                {
                    MessageBox.Show($"Не удалось открыть директорию: {target}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
