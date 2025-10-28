using ChatAppClient.Forms;
using System;
using System.Windows.Forms;

namespace ChatAppClient
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Bắt đầu ứng dụng với Form Đăng nhập
            Application.Run(new frmLogin());
        }
    }
}