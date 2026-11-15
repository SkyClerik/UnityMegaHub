#nullable enable
using System;
using System.Windows.Forms;

namespace UnityMegaHub
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Запускаем наше главное окно, которое мы создали ранее
            Application.Run(new MainForm());
        }
    }
}
