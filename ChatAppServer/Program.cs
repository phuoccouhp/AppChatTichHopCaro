using System;
using System.Windows.Forms;

namespace ChatAppServer
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Khởi tạo cấu hình từ file appsettings.json
            AppConfig.Initialize();
            
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Chạy Form Server thay vì Console
            Application.Run(new frmServer());
        }
    }
}