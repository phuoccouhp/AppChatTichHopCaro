using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Threading;

namespace ChatAppServer
{
    public static class FirewallHelper
    {
        /// <summary>
        /// Hàm chính để gọi mở Port. Sẽ tự động hiện bảng xin quyền Admin nếu chưa có.
        /// </summary>
        public static void OpenPortAsAdmin(int port, string ruleName = "ChatAppServer")
        {
            try
            {
                // Kiểm tra xem đã có quyền Admin chưa
                if (!IsAdministrator())
                {
                    Logger.Warning($"Đang yêu cầu quyền Admin để mở Port {port}...");
                    RunPowershellWithAdmin(port, ruleName);
                }
                else
                {
                    // Đã là Admin rồi thì chạy trực tiếp không cần popup
                    Logger.Info($"Đang cấu hình Firewall cho Port {port} (Quyền Admin)...");
                    RunPowershellDirect(port, ruleName);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Lỗi khi mở Firewall: {ex.Message}");
                Logger.Warning("HÃY THỬ CHẠY VISUAL STUDIO HOẶC FILE EXE DƯỚI QUYỀN 'RUN AS ADMINISTRATOR'");
            }
        }

        /// <summary>
        /// Kiểm tra xem ứng dụng hiện tại có đang chạy với quyền Admin không
        /// </summary>
        public static bool IsAdministrator()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        /// <summary>
        /// Chỉ kiểm tra đơn giản xem Rule đã tồn tại chưa (Không check sâu enabled/disabled để tránh lỗi ngôn ngữ)
        /// </summary>
        public static bool IsPortOpen(int port, string ruleName = "ChatAppServer")
        {
            try
            {
                // Lệnh Powershell kiểm tra xem có rule nào tên như vậy không
                string script = $"Get-NetFirewallRule -DisplayName '{ruleName}' -ErrorAction SilentlyContinue";

                var psi = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-NoProfile -Command \"{script}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    // Nếu có output trả về tức là Rule có tồn tại
                    return !string.IsNullOrWhiteSpace(output);
                }
            }
            catch
            {
                // Nếu lỗi kiểm tra, cứ trả về false để code thực hiện mở lại cho chắc
                return false;
            }
        }

        /// <summary>
        /// Chạy PowerShell với yêu cầu nâng quyền (Hiện bảng Yes/No)
        /// </summary>
        private static void RunPowershellWithAdmin(int port, string ruleName)
        {
            string psScript = BuildPowershellScript(port, ruleName);

            // Tạo file tạm chứa script để chạy cho ổn định
            string tempFile = Path.Combine(Path.GetTempPath(), "OpenPort.ps1");
            File.WriteAllText(tempFile, psScript);

            var psi = new ProcessStartInfo
            {
                FileName = "powershell",
                // ExecutionPolicy Bypass: Để cho phép chạy script không cần ký số
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{tempFile}\"",
                Verb = "runas", // <--- CÂU LỆNH QUAN TRỌNG: YÊU CẦU QUYỀN ADMIN
                UseShellExecute = true, // Bắt buộc true để dùng Verb runas
                WindowStyle = ProcessWindowStyle.Hidden
            };

            try
            {
                var process = Process.Start(psi);
                process?.WaitForExit(); // Chờ user bấm Yes và chạy xong
                Logger.Success($"Đã gửi lệnh mở Port {port} vào Windows Firewall.");
            }
            catch (System.ComponentModel.Win32Exception)
            {
                Logger.Error("Người dùng đã từ chối cấp quyền Admin (Bấm No). Không thể mở Port.");
            }
            finally
            {
                // Dọn dẹp file tạm
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        /// <summary>
        /// Chạy PowerShell trực tiếp (khi đã có quyền Admin)
        /// </summary>
        private static void RunPowershellDirect(int port, string ruleName)
        {
            string psScript = BuildPowershellScript(port, ruleName);

            var psi = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-NoProfile -Command \"{psScript}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var process = Process.Start(psi);
            process.WaitForExit();

            // Check lỗi nếu cần
            string error = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(error))
            {
                // Logger.Warning($"PS Warning: {error}"); // Uncomment nếu muốn debug kỹ
            }
            Logger.Success($"Đã cập nhật Firewall rule cho Port {port}.");
        }

        /// <summary>
        /// Tạo nội dung script PowerShell để xóa cũ -> tạo mới
        /// </summary>
        private static string BuildPowershellScript(int port, string ruleName)
        {
            // Script này làm 3 việc:
            // 1. Xóa rule cũ (Inbound & Outbound) để tránh trùng lặp
            // 2. Tạo Rule Inbound (Cho phép người khác nối vào mình)
            // 3. Tạo Rule Outbound (Cho phép mình trả lời lại) - Quan trọng!
            return $@"
                $name = '{ruleName}';
                $port = {port};
                
                # Xóa rule cũ nếu có
                Remove-NetFirewallRule -DisplayName $name -ErrorAction SilentlyContinue;
                Remove-NetFirewallRule -DisplayName ($name + ' Out') -ErrorAction SilentlyContinue;

                # Tạo Inbound Rule (Cho phép kết nối đến) cho mọi Profile (Domain, Private, Public)
                New-NetFirewallRule -DisplayName $name -Direction Inbound -LocalPort $port -Protocol TCP -Action Allow -Profile Any -Enabled True;

                # Tạo Outbound Rule (Cho phép gửi dữ liệu đi)
                New-NetFirewallRule -DisplayName ($name + ' Out') -Direction Outbound -LocalPort $port -Protocol TCP -Action Allow -Profile Any -Enabled True;
            ";
        }
    }
}