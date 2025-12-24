using System;
using System.Windows.Forms;

namespace ChatAppServer
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // --- XÓA HOẶC COMMENT DÒNG NÀY ---
            // AppConfig.Initialize(); 
            // ----------------------------------

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Chạy Form Server
            Application.Run(new frmServer());
        }
    }
}