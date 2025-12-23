using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace ChatAppServer
{
    /// <summary>
    /// Helper class ?? m? port trên Windows Firewall và ki?m tra k?t n?i m?ng
    /// </summary>
    public static class FirewallHelper
    {
        /// <summary>
        /// M? port trên Windows Firewall cho c? Inbound và Outbound
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

                Logger.Success($"?ã m? port {port} trên Windows Firewall");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"L?i khi m? port {port} trên Firewall", ex);
                return false;
            }
        }

        /// <summary>
        /// Ki?m tra xem rule ?ã t?n t?i ch?a
        /// </summary>
        public static bool IsPortOpen(int port, string ruleName = "ChatAppServer")
        {
            try
            {
                // Ki?m tra inbound rule
                string inboundResult = RunNetshCommand($"advfirewall firewall show rule name=\"{ruleName}\" dir=in");
                
                // Ki?m tra outbound rule
                string outboundResult = RunNetshCommand($"advfirewall firewall show rule name=\"{ruleName} (Out)\" dir=out");
                
                // Inbound rule ph?i t?n t?i, enabled, action=allow
                bool inboundExists = !string.IsNullOrEmpty(inboundResult) && 
                    (inboundResult.Contains("Rule Name") || inboundResult.Contains("Tên quy t?c")) &&
                    (inboundResult.Contains("Enabled") && (inboundResult.Contains("Yes") || inboundResult.Contains("Có") || inboundResult.Contains("?ã b?t")));
                
                // Outbound rule ph?i t?n t?i, enabled, action=allow
                bool outboundExists = !string.IsNullOrEmpty(outboundResult) && 
                    (outboundResult.Contains("Rule Name") || outboundResult.Contains("Tên quy t?c")) &&
                    (outboundResult.Contains("Enabled") && (outboundResult.Contains("Yes") || outboundResult.Contains("Có") || outboundResult.Contains("?ã b?t")));
                
                Logger.Info($"[IsPortOpen] Inbound={inboundExists}, Outbound={outboundExists}");
                
                return inboundExists && outboundExists;
            }
            catch (Exception ex)
            {
                Logger.Warning($"L?i khi ki?m tra firewall rule: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Ch?y l?nh netsh
        /// </summary>
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
                    
                    bool finished = process.WaitForExit(5000); // Timeout 5 giây
                    
                    if (!finished)
                    {
                        try { process.Kill(); } catch { }
                        Logger.Warning($"Netsh command timeout: {arguments}");
                        return "";
                    }
                    
                    if (process.ExitCode != 0 && !string.IsNullOrEmpty(error))
                    {
                        Logger.Warning($"Netsh command error: {error}");
                    }
                    
                    return output;
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"L?i khi ch?y netsh command: {ex.Message}");
                return "";
            }
        }

        /// <summary>
        /// M? port v?i quy?n Administrator b?ng cách ch?y file batch
        /// </summary>
        public static bool OpenPortAsAdmin(int port, string ruleName = "ChatAppServer")
        {
            string tempBatchFile = null;
            try
            {
                // T?o file batch t?m th?i - M? CHO T?T C? PROFILE
                string batchContent = $@"@echo off
setlocal enabledelayedexpansion

echo Opening Firewall Port {port}...
echo.

REM Xóa rule c? n?u t?n t?i
netsh advfirewall firewall delete rule name=""{ruleName}"" >nul 2>&1
netsh advfirewall firewall delete rule name=""{ruleName} (Out)"" >nul 2>&1
echo Removing old rules...
echo.

REM T?o Inbound rule
echo Creating Inbound rule...
netsh advfirewall firewall add rule name=""{ruleName}"" dir=in action=allow protocol=TCP localport={port} profile=any enable=yes

if !errorlevel! neq 0 (
    echo ERROR: Failed to create inbound rule
    pause
    exit /b 1
)

REM T?o Outbound rule
echo Creating Outbound rule...
netsh advfirewall firewall add rule name=""{ruleName} (Out)"" dir=out action=allow protocol=TCP localport={port} profile=any enable=yes

if !errorlevel! neq 0 (
    echo ERROR: Failed to create outbound rule
    pause
    exit /b 1
)

echo.
echo SUCCESS: Port {port} is now open!
echo.
pause
exit /b 0
";
                tempBatchFile = Path.Combine(Path.GetTempPath(), $"open_firewall_{Guid.NewGuid().ToString("N").Substring(0, 8)}.bat");
                File.WriteAllText(tempBatchFile, batchContent);

                Logger.Info($"[OpenPortAsAdmin] Batch file: {tempBatchFile}");
                Logger.Info("[OpenPortAsAdmin] Requesting Administrator privileges...");

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = tempBatchFile,
                    UseShellExecute = true,
                    Verb = "runas",
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                Process? process = null;
                try
                {
                    process = Process.Start(psi);
                    if (process == null)
                    {
                        Logger.Error("Failed to start process. Possible UAC denial.");
                        return false;
                    }

                    bool finished = process.WaitForExit(30000);
                    
                    if (!finished)
                    {
                        Logger.Warning("Process timeout after 30 seconds");
                        try { process.Kill(); } catch { }
                        return false;
                    }

                    int exitCode = process.ExitCode;
                    Logger.Info($"[OpenPortAsAdmin] Exit code: {exitCode}");
                    
                    try { File.Delete(tempBatchFile); } catch { }
                    
                    if (exitCode == 0)
                    {
                        Logger.Success($"Firewall rule created successfully");
                        System.Threading.Thread.Sleep(2000);
                        return true;
                    }
                    else
                    {
                        Logger.Error($"Process returned error code: {exitCode}");
                        return false;
                    }
                }
                finally
                {
                    process?.Dispose();
                    if (tempBatchFile != null)
                    {
                        try { File.Delete(tempBatchFile); } catch { }
                    }
                }
            }
            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                Logger.Warning("User denied Administrator request (UAC cancelled)");
                try { if (tempBatchFile != null) File.Delete(tempBatchFile); } catch { }
                return false;
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                Logger.Error($"Win32 Error: {ex.Message} (Code: {ex.NativeErrorCode})");
                try { if (tempBatchFile != null) File.Delete(tempBatchFile); } catch { }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error opening port: {ex.GetType().Name} - {ex.Message}", ex);
                try { if (tempBatchFile != null) File.Delete(tempBatchFile); } catch { }
                return false;
            }
        }

        /// <summary>
        /// Test k?t n?i ??n m?t ??a ch? IP:Port
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
                        return (true, $"Connection successful to {ipAddress}:{port}", (int)stopwatch.ElapsedMilliseconds);
                    }
                    else
                    {
                        return (false, $"Cannot connect to {ipAddress}:{port} (Timeout)", (int)stopwatch.ElapsedMilliseconds);
                    }
                }
            }
            catch (SocketException ex)
            {
                stopwatch.Stop();
                string errorMsg = ex.SocketErrorCode switch
                {
                    SocketError.ConnectionRefused => "Port closed or no service listening",
                    SocketError.TimedOut => "Connection timeout - firewall may be blocking",
                    SocketError.NetworkUnreachable => "Network unreachable",
                    SocketError.HostUnreachable => "Host unreachable - check IP",
                    SocketError.HostNotFound => "Host not found",
                    _ => $"Socket error: {ex.SocketErrorCode}"
                };
                return (false, errorMsg, (int)stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return (false, $"Error: {ex.Message}", (int)stopwatch.ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// Ping ??n m?t ??a ch? IP
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
                        return (true, $"Ping successful ({reply.RoundtripTime}ms)", (int)reply.RoundtripTime);
                    }
                    else
                    {
                        return (false, $"Ping failed: {reply.Status}", 0);
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, $"Ping error: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// L?y t?t c? ??a ch? IP c?a máy
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
        /// Ki?m tra xem port có ?ang ???c s? d?ng không
        /// </summary>
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
            catch (SocketException)
            {
                return true;
            }
        }

        /// <summary>
        /// Ki?m tra xem port có ?ang l?ng nghe (LISTEN) không
        /// </summary>
        public static bool IsPortListening(int port)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    try
                    {
                        var result = client.BeginConnect(IPAddress.Loopback, port, null, null);
                        bool success = result.AsyncWaitHandle.WaitOne(2000);
                        
                        if (success && client.Connected)
                        {
                            client.EndConnect(result);
                            return true;
                        }
                    }
                    catch (SocketException sockEx)
                    {
                        if (sockEx.SocketErrorCode == SocketError.ConnectionRefused)
                        {
                            return false;
                        }
                        Logger.Warning($"Socket exception checking port {port}: {sockEx.SocketErrorCode}");
                        return false;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Logger.Warning($"Error checking port listening: {ex.Message}");
                return false;
            }
        }
    }
}
