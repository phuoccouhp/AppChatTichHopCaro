using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace ChatAppServer
{
    /// <summary>
    /// Helper class để mở port trên Windows Firewall và kiểm tra kết nối mạng
    /// </summary>
    public static class FirewallHelper
    {
        /// <summary>
        /// Mở port trên Windows Firewall cho cả Inbound và Outbound
        /// </summary>
        public static bool OpenPort(int port, string ruleName = "ChatAppServer")
        {
            try
            {
                // Xóa rule cũ trước
                RunNetshCommand($"advfirewall firewall delete rule name=\"{ruleName}\"");
                RunNetshCommand($"advfirewall firewall delete rule name=\"{ruleName} (Out)\"");

                // Tạo Inbound Rule (Cho phép người khác kết nối vào mình)
                // Thay profile=any bằng profile=private,domain,public để tránh lỗi cú pháp
                string inboundResult = RunNetshCommand(
                    $"advfirewall firewall add rule name=\"{ruleName}\" " +
                    $"dir=in action=allow protocol=TCP localport={port} " +
                    $"profile=private,domain,public enable=yes");

                // Tạo Outbound Rule (Cho phép mình trả lời lại)
                string outboundResult = RunNetshCommand(
                    $"advfirewall firewall add rule name=\"{ruleName} (Out)\" " +
                    $"dir=out action=allow protocol=TCP localport={port} " +
                    $"profile=private,domain,public enable=yes");

                Logger.Success($"Đã gửi lệnh mở port {port} tới Firewall.");
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
        public static bool IsPortOpen(int port, string ruleName = "ChatAppServer", int retryCount = 1, int delayMs = 0)
        {
            for (int attempt = 0; attempt < retryCount; attempt++)
            {
                try
                {
                    // Kiểm tra inbound rule
                    string inboundResult = RunNetshCommand($"advfirewall firewall show rule name=\"{ruleName}\" dir=in");

                    // Kiểm tra outbound rule
                    string outboundRuleName = $"{ruleName} (Out)";
                    string outboundResult = RunNetshCommand($"advfirewall firewall show rule name=\"{outboundRuleName}\" dir=out");

                    if (string.IsNullOrEmpty(outboundResult) || (!outboundResult.Contains("Rule Name") && !outboundResult.Contains("Tên quy tắc")))
                    {
                        outboundResult = RunNetshCommand($"advfirewall firewall show rule name={outboundRuleName} dir=out");
                    }

                    // Inbound check
                    bool inboundExists = !string.IsNullOrEmpty(inboundResult) &&
                        (inboundResult.Contains("Rule Name") || inboundResult.Contains("Tên quy tắc"));

                    // Outbound check
                    bool outboundExists = !string.IsNullOrEmpty(outboundResult) &&
                        (outboundResult.Contains("Rule Name") || outboundResult.Contains("Tên quy tắc"));

                    // Debug log chi tiết hơn để biết tại sao fail
                    if (!inboundExists || !outboundExists)
                    {
                        Logger.Info($"[IsPortOpen] Attempt {attempt + 1}/{retryCount}: Inbound={inboundExists}, Outbound={outboundExists}");
                    }

                    if (inboundExists && outboundExists)
                    {
                        return true;
                    }
                    else if (inboundExists)
                    {
                        Logger.Warning($"[IsPortOpen] Inbound OK, Outbound MISSING. Client gửi tin đến được nhưng Server không trả lời được.");
                    }

                    if (attempt < retryCount - 1 && delayMs > 0)
                    {
                        System.Threading.Thread.Sleep(delayMs);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Lỗi kiểm tra firewall (lần {attempt + 1}): {ex.Message}");
                }
            }

            return false;
        }

        private static string RunNetshCommand(string arguments)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (Process process = new Process())
                {
                    process.StartInfo = psi;
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    bool finished = process.WaitForExit(5000);

                    if (!finished)
                    {
                        try { process.Kill(); } catch { }
                        return "";
                    }

                    // Log lỗi nếu netsh báo lỗi (để debug vấn đề profile=any)
                    if (!string.IsNullOrEmpty(output) && (output.Contains("not valid") || output.Contains("Error")))
                    {
                        Logger.Info($"[Netsh Output] {output.Trim()}");
                    }

                    return output;
                }
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Mở port với quyền Admin bằng file batch (Fix lỗi cú pháp profile=any)
        /// </summary>
        public static bool OpenPortAsAdmin(int port, string ruleName = "ChatAppServer")
        {
            string tempBatchFile = null;
            try
            {
                // SỬA LỖI: Thay profile=any bằng profile=private,domain,public
                string batchContent = $@"@echo off
setlocal enabledelayedexpansion

echo ========================================
echo Opening Firewall Port {port}...
echo ========================================

echo [1/4] Deleting old rules...
netsh advfirewall firewall delete rule name=""{ruleName}"" >nul 2>&1
netsh advfirewall firewall delete rule name=""{ruleName} (Out)"" >nul 2>&1

echo [2/4] Adding inbound rule...
netsh advfirewall firewall add rule name=""{ruleName}"" dir=in action=allow protocol=TCP localport={port} profile=private,domain,public enable=yes
if !errorlevel! neq 0 echo ERROR Inbound: !errorlevel!

echo [3/4] Adding outbound rule...
netsh advfirewall firewall add rule name=""{ruleName} (Out)"" dir=out action=allow protocol=TCP localport={port} profile=private,domain,public enable=yes
if !errorlevel! neq 0 echo ERROR Outbound: !errorlevel!

echo [4/4] Verifying...
timeout /t 2 /nobreak >nul
exit /b 0
";
                tempBatchFile = Path.Combine(Path.GetTempPath(), $"open_firewall_{Guid.NewGuid().ToString("N").Substring(0, 8)}.bat");
                File.WriteAllText(tempBatchFile, batchContent, System.Text.Encoding.ASCII);

                Logger.Info($"[OpenPortAsAdmin] Running batch script...");

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = tempBatchFile,
                    UseShellExecute = true,
                    Verb = "runas", // Yêu cầu quyền Admin
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                Process? process = Process.Start(psi);
                if (process == null) return false;

                process.WaitForExit();

                // Xóa file tạm
                try { System.Threading.Thread.Sleep(500); File.Delete(tempBatchFile); } catch { }

                // Verify lại
                return IsPortOpen(port, ruleName, retryCount: 3, delayMs: 1000);
            }
            catch (Exception ex)
            {
                Logger.Error($"OpenPortAsAdmin Failed: {ex.Message}");
                return false;
            }
        }

        // ... (Giữ nguyên các hàm TestConnection, Ping, GetAllLocalIPs, IsPortInUse, IsPortListening như cũ) ...
        // Bạn có thể copy lại phần dưới của file cũ vào đây nếu cần, nhưng quan trọng nhất là phần trên đã sửa.

        public static (bool success, string message, int latencyMs) TestConnection(string ipAddress, int port, int timeoutMs = 5000)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    var result = client.BeginConnect(ipAddress, port, null, null);
                    bool success = result.AsyncWaitHandle.WaitOne(timeoutMs);
                    stopwatch.Stop();

                    if (success && client.Connected)
                    {
                        client.EndConnect(result);
                        return (true, $"Connection successful to {ipAddress}:{port}", (int)stopwatch.ElapsedMilliseconds);
                    }
                    else
                    {
                        return (false, $"Cannot connect to {ipAddress}:{port} (Timeout)", (int)stopwatch.ElapsedMilliseconds);
                    }
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return (false, $"Error: {ex.Message}", (int)stopwatch.ElapsedMilliseconds);
            }
        }

        public static (bool success, string message, int latencyMs) Ping(string ipAddress, int timeoutMs = 3000)
        {
            try
            {
                using (var ping = new System.Net.NetworkInformation.Ping())
                {
                    var reply = ping.Send(ipAddress, timeoutMs);
                    if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                        return (true, $"Ping successful ({reply.RoundtripTime}ms)", (int)reply.RoundtripTime);
                    else
                        return (false, $"Ping failed: {reply.Status}", 0);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Ping error: {ex.Message}", 0);
            }
        }

        public static List<string> GetAllLocalIPs()
        {
            var ips = new List<string>();
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        ips.Add(ip.ToString());
                }
            }
            catch { }
            return ips;
        }

        public static bool IsPortInUse(int port)
        {
            try
            {
                using (TcpListener listener = new TcpListener(IPAddress.Loopback, port))
                {
                    listener.Start();
                    listener.Stop();
                    return false;
                }
            }
            catch (SocketException) { return true; }
        }

        public static bool IsPortListening(int port)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    var result = client.BeginConnect(IPAddress.Loopback, port, null, null);
                    if (result.AsyncWaitHandle.WaitOne(2000) && client.Connected) return true;
                }
                return false;
            }
            catch { return false; }
        }
    }
}