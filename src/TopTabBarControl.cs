#nullable enable
using System;
using System.Drawing;
using System.Windows.Forms;

namespace UnityMegaHub
{
    [System.ComponentModel.DesignerCategory("")]
    public class TopTabBarControl : UserControl
    {
        public event Action<string>? OnTabChanged;

        public TopTabBarControl()
        {
            this.Height = 40;
            this.BackColor = Color.FromArgb(30, 30, 30);
            CreateTabs();
        }

        private void CreateTabs()
        {
            //string[] tabs = { "Projects", "Templates", "Editors", "Updates", "Tools", "Settings" };
            string[] tabs = { "Projects", "Templates", "Editors", "Settings" };
            int leftPos = 5;

            foreach (var tabName in tabs)
            {
                Button btn = new Button
                {
                    Text = tabName,
                    Width = 90,
                    Height = 30,
                    Top = 5,
                    Left = leftPos,
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(45, 45, 45)
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += (s, e) => OnTabChanged?.Invoke(tabName);
                this.Controls.Add(btn);
                leftPos += 95;
            }
        }
    }
}
