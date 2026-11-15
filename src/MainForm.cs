#nullable enable
using System;
using System.Drawing;
using System.Windows.Forms;

namespace UnityMegaHub
{
    public class MainForm : Form
    {
        private readonly HubSettings _settings;
        private TopTabBarControl _topTabBar = null!;
        private SettingsControl _settingsControl = null!;
        private ProjectsControl _projectsControl = null!;
        private TemplatesControl _templatesControl = null!;
        private EditorsControl _editorsControl = null!; // Добавили поле экрана редакторов
        private Panel _containerPanel = null!;
        private UpdatesControl _updatesControl = null!;

        public MainForm()
        {
            _settings = new HubSettings();
            SetupMainLayout();
        }

        private void SetupMainLayout()
        {
            this.Text = "UnityMegaHub";
            this.Size = new Size(950, 650);
            this.MinimumSize = new Size(950, 650); // ИСПРАВЛЕНО: Окно больше нельзя сжать меньше базового UI
            this.BackColor = Color.FromArgb(40, 40, 40);
            this.StartPosition = FormStartPosition.CenterScreen;

            _topTabBar = new TopTabBarControl { Dock = DockStyle.Top };
            _topTabBar.OnTabChanged += HandleTabChange;

            _containerPanel = new Panel { Dock = DockStyle.Fill };
            _updatesControl = new UpdatesControl(_settings) { Dock = DockStyle.Fill };

            _settingsControl = new SettingsControl(_settings) { Dock = DockStyle.Fill };
            _projectsControl = new ProjectsControl(_settings) { Dock = DockStyle.Fill };
            _editorsControl = new EditorsControl(_settings) { Dock = DockStyle.Fill }; // Инициализируем экран редакторов

            _templatesControl = new TemplatesControl(_settings, () =>
            {
                _projectsControl.UpdateProjectsData();
                HandleTabChange("Projects");
            })
            { Dock = DockStyle.Fill };

            this.Controls.Add(_containerPanel);
            this.Controls.Add(_topTabBar);

            if (_settings.ProjectDirectories.Count > 0)
                HandleTabChange("Projects");
            else
                HandleTabChange("Settings");
        }

        private void HandleTabChange(string tabName)
        {
            _containerPanel.Controls.Clear();

            if (tabName == "Settings") _containerPanel.Controls.Add(_settingsControl);
            else if (tabName == "Projects") _containerPanel.Controls.Add(_projectsControl);
            else if (tabName == "Templates") _containerPanel.Controls.Add(_templatesControl);
            else if (tabName == "Editors") _containerPanel.Controls.Add(_editorsControl); // Обрабатываем переключение
            else if (tabName == "Updates") _containerPanel.Controls.Add(_updatesControl);
            else
            {
                Label lbl = new Label { Text = $"Экран: {tabName}", ForeColor = Color.White, Location = new Point(40, 40), AutoSize = true };
                _containerPanel.Controls.Add(lbl);
            }
        }

        public void RefreshTemplatesTab()
        {
            _templatesControl.LoadTemplates();
        }
    }
}
