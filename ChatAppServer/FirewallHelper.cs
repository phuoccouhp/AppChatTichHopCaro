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
                RunNetshCommand($"advfirewall firewall delete rule name=\"{ruleName}\"");
                
                string inboundResult = RunNetshCommand(
                    $"advfirewall firewall add rule name=\"{ruleName}\" " +
                    $"dir=in action=allow protocol=TCP localport={port} " +
                    $"profile=any enable=yes");

                string outboundResult = RunNetshCommand(
                    $"advfirewall firewall add rule name=\"{ruleName} (Out)\" " +
                    $"dir=out action=allow protocol=TCP localport={port} " +
                    $"profile=any enable=yes");

                Logger.Success($"Đã mở port {port} trên Windows Firewall");
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
                return result.Contains("Rule Name") || result.Contains("Tên quy tắc") || result.Contains("Enabled");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Chạy lệnh netsh
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
                CreateNoWindow = true
            };

            using (Process process = new Process())
            {
                process.StartInfo = psi;
                process.Start();
                
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                
                process.WaitForExit();
                return output;
            }
        }

        /// <summary>
        /// Mở port với quyền Administrator bằng cách chạy file batch
        /// </summary>
        public static bool OpenPortAsAdmin(int port, string ruleName = "ChatAppServer")
        {
            try
            {
                // Tạo file batch tạm thời - chỉ cho Private network (WiFi)
                string batchContent = $@"@echo off
netsh advfirewall firewall delete rule name=""{ruleName}"" >nul 2>&1
netsh advfirewall firewall delete rule name=""{ruleName} (Out)"" >nul 2>&1
netsh advfirewall firewall add rule name=""{ruleName}"" dir=in action=allow protocol=TCP localport={port} profile=private,domain enable=yes
netsh advfirewall firewall add rule name=""{ruleName} (Out)"" dir=out action=allow protocol=TCP localport={port} profile=private,domain enable=yes
echo Done
";
                string tempBatchFile = Path.Combine(Path.GetTempPath(), "open_firewall_chatapp.bat");
                File.WriteAllText(tempBatchFile, batchContent);

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = tempBatchFile,
                    UseShellExecute = true,
                    Verb = "runas", // Yêu cầu quyền admin (hiện UAC)
                    CreateNoWindow = false
                };

                using (Process process = Process.Start(psi))
                {
                    process?.WaitForExit(10000); // Timeout 10 giây
                    
                    // Xóa file batch tạm
                    try { File.Delete(tempBatchFile); } catch { }
                    
                    return process?.ExitCode == 0;
                }
            }
            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                // User đã từ chối UAC
                Logger.Warning("Người dùng đã từ chối yêu cầu quyền Administrator");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi khi mở port với quyền Admin", ex);
                return false;
            }
        }

        /// <summary>
        /// Test kết nối đến một địa chỉ IP:Port
        /// </summary>
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
                        return (true, $"Kết nối thành công đến {ipAddress}:{port}", (int)stopwatch.ElapsedMilliseconds);
                    }
                    else
                    {
                        return (false, $"Không thể kết nối đến {ipAddress}:{port} (Timeout)", (int)stopwatch.ElapsedMilliseconds);
                    }
                }
            }
            catch (SocketException ex)
            {
                stopwatch.Stop();
                string errorMsg = ex.SocketErrorCode switch
                {
                    SocketError.ConnectionRefused => "Port đang đóng hoặc không có service lắng nghe",
                    SocketError.TimedOut => "Kết nối timeout - có thể do firewall chặn",
                    SocketError.NetworkUnreachable => "Không thể truy cập mạng",
                    SocketError.HostUnreachable => "Không thể truy cập host - kiểm tra IP",
                    SocketError.HostNotFound => "Không tìm thấy host",
                    _ => $"Lỗi socket: {ex.SocketErrorCode}"
                };
                return (false, errorMsg, (int)stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return (false, $"Lỗi: {ex.Message}", (int)stopwatch.ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// Ping đến một địa chỉ IP
        /// </summary>
        public static (bool success, string message, int latencyMs) Ping(string ipAddress, int timeoutMs = 3000)
        {
            try
            {
                using (var ping = new System.Net.NetworkInformation.Ping())
                {
                    var reply = ping.Send(ipAddress, timeoutMs);
                    if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                    {
                        return (true, $"Ping thành công ({reply.RoundtripTime}ms)", (int)reply.RoundtripTime);
                    }
                    else
                    {
                        return (false, $"Ping thất bại: {reply.Status}", 0);
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi ping: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// Lấy tất cả địa chỉ IP của máy
        /// </summary>
        public static List<string> GetAllLocalIPs()
        {
            var ips = new List<string>();
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ips.Add(ip.ToString());
                    }
                }
            }
            catch { }
            return ips;
        }

        /// <summary>
        /// Kiểm tra xem port có đang được sử dụng không
        /// </summary>
        public static bool IsPortInUse(int port)
        {
            try
            {
                using (TcpListener listener = new TcpListener(IPAddress.Loopback, port))
                {
                    listener.Start();
                    listener.Stop();
                    return false; // Port không được sử dụng
                }
            }
            catch (SocketException)
            {
                return true; // Port đang được sử dụng
            }
        }
    }
}

