#nullable enable
using System;
using System.Drawing;
using System.Windows.Forms;

namespace UnityMegaHub
{
    public class ImagePopupForm : Form
    {
        private Image? _currentImage;

        public ImagePopupForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.ShowInTaskbar = false;
            this.Size = new Size(600, 455);
            this.BackColor = Color.FromArgb(35, 35, 35);

            // ИСПРАВЛЕНО: Форма теперь физически всегда находится поверх лаунчера
            this.TopMost = true;
            this.SetStyle(ControlStyles.Selectable, false);
        }

        // ИСПРАВЛЕНО: Заставляем Windows игнорировать форму при кликах и наведениях,
        // чтобы она работала как честный прозрачный оверлей (актуально для Win10/Win11)
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE (Окно не перехватывает фокус)
                return cp;
            }
        }

        protected override bool ShowWithoutActivation => true;

        public void ShowImage(Image img, Point position)
        {
            _currentImage = img;
            this.Location = position;
            if (!this.Visible) this.Show();
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_currentImage != null)
            {
                e.Graphics.DrawImage(_currentImage, 5, 5, this.Width - 10, this.Height - 10);
            }
            using (Pen pen = new Pen(Color.FromArgb(70, 70, 70), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            }
        }
    }
}
