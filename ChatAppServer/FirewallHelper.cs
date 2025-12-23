using System;
using System.Diagnostics;

namespace ChatAppServer
{
    /// <summary>
    /// Helper class để mở port trên Windows Firewall
    /// </summary>
    public static class FirewallHelper
    {
        /// <summary>
        /// Mở port trên Windows Firewall cho cả Inbound và Outbound
        /// </summary>
        /// <param name="port">Port cần mở</param>
        /// <param name="ruleName">Tên rule trong Firewall</param>
        /// <returns>True nếu thành công</returns>
        public static bool OpenPort(int port, string ruleName = "ChatAppServer")
        {
            try
            {
                // Xóa rule cũ nếu có (để tránh duplicate)
                RunNetshCommand($"advfirewall firewall delete rule name=\"{ruleName}\"");

                // Thêm rule Inbound (cho phép kết nối đến)
                string inboundResult = RunNetshCommand(
                    $"advfirewall firewall add rule name=\"{ruleName}\" " +
                    $"dir=in action=allow protocol=TCP localport={port} " +
                    $"profile=any enable=yes");

                // Thêm rule Outbound (cho phép kết nối đi)
                string outboundResult = RunNetshCommand(
                    $"advfirewall firewall add rule name=\"{ruleName} (Out)\" " +
                    $"dir=out action=allow protocol=TCP localport={port} " +
                    $"profile=any enable=yes");

                Logger.Success($"Đã mở port {port} trên Windows Firewall");
                Logger.Info($"Inbound: {inboundResult}");
                Logger.Info($"Outbound: {outboundResult}");
                
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Lỗi khi mở port {port} trên Firewall", ex);
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra xem rule đã tồn tại chưa
        /// </summary>
        public static bool IsPortOpen(int port, string ruleName = "ChatAppServer")
        {
            try
            {
                string result = RunNetshCommand($"advfirewall firewall show rule name=\"{ruleName}\"");
                return result.Contains("Rule Name") || result.Contains("Tên quy tắc");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Chạy lệnh netsh với quyền admin
        /// </summary>
        private static string RunNetshCommand(string arguments)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                Verb = "runas" // Yêu cầu quyền admin
            };

            using (Process process = new Process())
            {
                process.StartInfo = psi;
                process.Start();
                
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    Logger.Warning($"Netsh warning: {error}");
                }

                return output;
            }
        }

        /// <summary>
        /// Mở port với quyền Administrator (hiện UAC prompt)
        /// </summary>
        public static bool OpenPortAsAdmin(int port, string ruleName = "ChatAppServer")
        {
            try
            {
                // Tạo script PowerShell để mở port
                string script = $@"
                    $ruleName = '{ruleName}'
                    $port = {port}
                    
                    # Xóa rule cũ nếu có
                    netsh advfirewall firewall delete rule name=""$ruleName"" 2>$null
                    netsh advfirewall firewall delete rule name=""$ruleName (Out)"" 2>$null
                    
                    # Thêm rule mới
                    netsh advfirewall firewall add rule name=""$ruleName"" dir=in action=allow protocol=TCP localport=$port profile=any enable=yes
                    netsh advfirewall firewall add rule name=""$ruleName (Out)"" dir=out action=allow protocol=TCP localport=$port profile=any enable=yes
                    
                    Write-Host 'Da mo port thanh cong!'
                ";

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script.Replace("\"", "\\\"")}\"",
                    UseShellExecute = true,
                    Verb = "runas", // Yêu cầu quyền admin (hiện UAC)
                    CreateNoWindow = false
                };

                using (Process process = Process.Start(psi))
                {
                    process?.WaitForExit();
                    return process?.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi khi mở port với quyền Admin", ex);
                return false;
            }
        }
    }
}

