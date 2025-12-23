using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace ChatAppServer
{
    /// <summary>
    /// Helper class ?? m? port tr�n Windows Firewall v� ki?m tra k?t n?i m?ng
    /// </summary>
    public static class FirewallHelper
    {
        /// <summary>
        /// M? port tr�n Windows Firewall cho c? Inbound v� Outbound
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

                Logger.Success($"?� m? port {port} tr�n Windows Firewall");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"L?i khi m? port {port} tr�n Firewall", ex);
                return false;
            }
        }

        /// <summary>
        /// Ki?m tra xem rule ?� t?n t?i ch?a
        /// </summary>
        public static bool IsPortOpen(int port, string ruleName = "ChatAppServer", int retryCount = 1, int delayMs = 0)
        {
            for (int attempt = 0; attempt < retryCount; attempt++)
            {
<<<<<<< HEAD
                // Ki?m tra inbound rule
                string inboundResult = RunNetshCommand($"advfirewall firewall show rule name=\"{ruleName}\" dir=in");
                
                // Ki?m tra outbound rule
                string outboundResult = RunNetshCommand($"advfirewall firewall show rule name=\"{ruleName} (Out)\" dir=out");
                
                // Inbound rule ph?i t?n t?i, enabled, action=allow
                bool inboundExists = !string.IsNullOrEmpty(inboundResult) && 
                    (inboundResult.Contains("Rule Name") || inboundResult.Contains("T�n quy t?c")) &&
                    (inboundResult.Contains("Enabled") && (inboundResult.Contains("Yes") || inboundResult.Contains("C�") || inboundResult.Contains("?� b?t")));
                
                // Outbound rule ph?i t?n t?i, enabled, action=allow
                bool outboundExists = !string.IsNullOrEmpty(outboundResult) && 
                    (outboundResult.Contains("Rule Name") || outboundResult.Contains("T�n quy t?c")) &&
                    (outboundResult.Contains("Enabled") && (outboundResult.Contains("Yes") || outboundResult.Contains("C�") || outboundResult.Contains("?� b?t")));
                
                Logger.Info($"[IsPortOpen] Inbound={inboundExists}, Outbound={outboundExists}");
                
                return inboundExists && outboundExists;
            }
            catch (Exception ex)
            {
                Logger.Warning($"L?i khi ki?m tra firewall rule: {ex.Message}");
                return false;
=======
                try
                {
                    // Kiểm tra inbound rule
                    string inboundResult = RunNetshCommand($"advfirewall firewall show rule name=\"{ruleName}\" dir=in");

                    // Kiểm tra outbound rule
                    string outboundRuleName = $"{ruleName} (Out)";
                    // Thử cả với và không có dấu ngoặc kép
                    string outboundResult = RunNetshCommand($"advfirewall firewall show rule name=\"{outboundRuleName}\" dir=out");
                    if (string.IsNullOrEmpty(outboundResult) || (!outboundResult.Contains("Rule Name") && !outboundResult.Contains("Tên quy tắc")))
                    {
                        // Thử lại không có dấu ngoặc kép
                        outboundResult = RunNetshCommand($"advfirewall firewall show rule name={outboundRuleName} dir=out");
                    }

                    // Inbound rule phải tồn tại (có Rule Name) - kiểm tra Enabled nếu có
                    bool inboundExists = !string.IsNullOrEmpty(inboundResult) &&
                        (inboundResult.Contains("Rule Name") || inboundResult.Contains("Tên quy tắc"));
                    
                    // Nếu rule tồn tại, kiểm tra Enabled (nhưng không bắt buộc)
                    if (inboundExists)
                    {
                        bool enabled = inboundResult.Contains("Enabled") && 
                            (inboundResult.Contains("Yes") || inboundResult.Contains("Có") || inboundResult.Contains("Đã bật"));
                        if (!enabled && inboundResult.Contains("Enabled"))
                        {
                            Logger.Warning($"[IsPortOpen] Inbound rule exists but may be disabled");
                        }
                    }

                    // Outbound rule phải tồn tại (có Rule Name) - kiểm tra Enabled nếu có
                    bool outboundExists = !string.IsNullOrEmpty(outboundResult) &&
                        (outboundResult.Contains("Rule Name") || outboundResult.Contains("Tên quy tắc"));
                    
                    // Nếu rule tồn tại, kiểm tra Enabled (nhưng không bắt buộc)
                    if (outboundExists)
                    {
                        bool enabled = outboundResult.Contains("Enabled") && 
                            (outboundResult.Contains("Yes") || outboundResult.Contains("Có") || outboundResult.Contains("Đã bật"));
                        if (!enabled && outboundResult.Contains("Enabled"))
                        {
                            Logger.Warning($"[IsPortOpen] Outbound rule exists but may be disabled");
                        }
                    }
                    else
                    {
                        // Debug: log một phần output để xem tại sao không tìm thấy
                        if (!string.IsNullOrEmpty(outboundResult))
                        {
                            string preview = outboundResult.Length > 200 ? outboundResult.Substring(0, 200) : outboundResult;
                            Logger.Info($"[IsPortOpen] Outbound result preview: {preview.Replace("\r\n", " | ")}");
                        }
                        else
                        {
                            Logger.Info($"[IsPortOpen] Outbound result is empty - rule may not exist");
                        }
                    }

                    Logger.Info($"[IsPortOpen] Attempt {attempt + 1}/{retryCount}: Inbound={inboundExists}, Outbound={outboundExists}");

                    // Đối với server, Inbound rule là quan trọng nhất (cho phép clients kết nối đến)
                    // Outbound rule cũng tốt nhưng không bắt buộc (chủ yếu cho traffic đi ra)
                    if (inboundExists)
                    {
                        if (!outboundExists)
                        {
                            Logger.Warning($"[IsPortOpen] Inbound rule tồn tại nhưng Outbound rule không tìm thấy. Inbound rule là đủ cho server.");
                        }
                        return true; // Inbound rule đủ để server hoạt động
                    }

                    // Nếu chưa tìm thấy và còn lần thử, đợi rồi thử lại
                    if (attempt < retryCount - 1 && delayMs > 0)
                    {
                        System.Threading.Thread.Sleep(delayMs);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Lỗi khi kiểm tra firewall rule (attempt {attempt + 1}/{retryCount}): {ex.Message}");
                    if (attempt < retryCount - 1 && delayMs > 0)
                    {
                        System.Threading.Thread.Sleep(delayMs);
                    }
                }
>>>>>>> 2306d4ff8607175fe37f458d62cda6a086df55c5
            }

            return false;
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
<<<<<<< HEAD
                    
                    bool finished = process.WaitForExit(5000); // Timeout 5 gi�y
                    
=======

                    bool finished = process.WaitForExit(5000); // Timeout 5 giây

>>>>>>> 2306d4ff8607175fe37f458d62cda6a086df55c5
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
        /// M? port v?i quy?n Administrator b?ng c�ch ch?y file batch
        /// </summary>
        public static bool OpenPortAsAdmin(int port, string ruleName = "ChatAppServer")
        {
            string tempBatchFile = null;
            try
            {
<<<<<<< HEAD
                // T?o file batch t?m th?i - M? CHO T?T C? PROFILE
                string batchContent = $@"@echo off
setlocal enabledelayedexpansion

echo Opening Firewall Port {port}...
echo.

REM X�a rule c? n?u t?n t?i
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
=======
                // Tạo file batch tạm thời - MỞ CHO TẤT CẢ PROFILE
                string batchContent = $@"@echo off
setlocal enabledelayedexpansion

echo ========================================
echo Opening Firewall Port {port}...
echo ========================================
echo.

REM Xóa rule cũ nếu tồn tại
echo [1/4] Deleting old rules (if any)...
netsh advfirewall firewall delete rule name=""{ruleName}"" >nul 2>&1
netsh advfirewall firewall delete rule name=""{ruleName} (Out)"" >nul 2>&1
echo    Done
echo.

REM Tạo Inbound rule
echo [2/4] Adding inbound rule...
netsh advfirewall firewall add rule name=""{ruleName}"" dir=in action=allow protocol=TCP localport={port} profile=any enable=yes
if !errorlevel! neq 0 (
    echo    ERROR: Failed to add inbound rule (errorlevel=!errorlevel!)
    exit /b !errorlevel!
)
echo    Success
echo.

REM Tạo Outbound rule
echo [3/4] Adding outbound rule...
netsh advfirewall firewall add rule name=""{ruleName} (Out)"" dir=out action=allow protocol=TCP localport={port} profile=any enable=yes
if !errorlevel! neq 0 (
    echo    ERROR: Failed to add outbound rule (errorlevel=!errorlevel!)
    exit /b !errorlevel!
)
echo    Success
echo.

REM Verify rules - đợi một chút để rule được commit
echo [4/4] Verifying rules...
timeout /t 1 /nobreak >nul 2>&1
netsh advfirewall firewall show rule name=""{ruleName}"" dir=in | findstr /C:""Rule Name"" >nul 2>&1
if !errorlevel! neq 0 (
    echo    WARNING: Inbound rule not found after creation
    exit /b 2
)
netsh advfirewall firewall show rule name=""{ruleName} (Out)"" dir=out | findstr /C:""Rule Name"" >nul 2>&1
if !errorlevel! neq 0 (
    echo    WARNING: Outbound rule not found after creation
    exit /b 2
)
echo    Success - Both rules verified
echo.
echo ========================================
echo SUCCESS: Firewall port {port} opened
echo ========================================
exit /b 0
";
                tempBatchFile = Path.Combine(Path.GetTempPath(), $"open_firewall_{Guid.NewGuid().ToString("N").Substring(0, 8)}.bat");
                // Dùng ASCII encoding để đảm bảo batch file hoạt động đúng
                File.WriteAllText(tempBatchFile, batchContent, System.Text.Encoding.ASCII);

                Logger.Info($"[OpenPortAsAdmin] Batch file: {tempBatchFile}");
                Logger.Info("[OpenPortAsAdmin] Requesting Administrator privileges (UAC will appear)...");
>>>>>>> 2306d4ff8607175fe37f458d62cda6a086df55c5

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = tempBatchFile,
                    UseShellExecute = true,
                    Verb = "runas",
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal,
                    WorkingDirectory = Path.GetTempPath()
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

<<<<<<< HEAD
                    bool finished = process.WaitForExit(30000);
                    
                    if (!finished)
                    {
                        Logger.Warning("Process timeout after 30 seconds");
                        try { process.Kill(); } catch { }
=======
                    // Đợi process hoàn thành với timeout 30 giây
                    bool finished = process.WaitForExit(30000);

                    if (!finished)
                    {
                        Logger.Warning("Process mở firewall timeout (quá 30 giây). Có thể đang chờ UAC hoặc có vấn đề.");
                        try 
                        { 
                            if (!process.HasExited)
                            {
                                process.Kill(); 
                            }
                        } 
                        catch (Exception killEx)
                        {
                            Logger.Warning($"Không thể kill process: {killEx.Message}");
                        }
>>>>>>> 2306d4ff8607175fe37f458d62cda6a086df55c5
                        return false;
                    }

                    // Đảm bảo process đã thực sự kết thúc
                    process.WaitForExit();
                    int exitCode = process.ExitCode;
                    Logger.Info($"[OpenPortAsAdmin] Exit code: {exitCode}");
<<<<<<< HEAD
                    
                    try { File.Delete(tempBatchFile); } catch { }
                    
                    if (exitCode == 0)
                    {
                        Logger.Success($"Firewall rule created successfully");
                        System.Threading.Thread.Sleep(2000);
                        return true;
=======

                    // Xóa file batch tạm
                    try 
                    { 
                        System.Threading.Thread.Sleep(200);
                        File.Delete(tempBatchFile);
                        tempBatchFile = null;
                    } 
                    catch (Exception delEx)
                    {
                        Logger.Warning($"Không thể xóa file batch tạm: {delEx.Message}");
                    }

                    if (exitCode == 0)
                    {
                        Logger.Success($"Firewall rule được tạo thành công (ExitCode: {exitCode})");
                        
                        // Đợi một chút để rule được commit vào firewall
                        System.Threading.Thread.Sleep(1000);
                        
                        // Verify lại rule đã tồn tại chưa (với retry)
                        bool verified = IsPortOpen(port, ruleName, retryCount: 5, delayMs: 500);
                        if (verified)
                        {
                            Logger.Success($"Đã xác nhận rule tồn tại trong firewall!");
                            return true;
                        }
                        else
                        {
                            Logger.Warning($"Process trả về ExitCode=0 nhưng không tìm thấy rule sau khi tạo. Có thể do delay hoặc quyền truy cập.");
                            // Vẫn return true vì process đã thành công, rule có thể chưa được commit ngay
                            return true;
                        }
                    }
                    else if (exitCode == 2)
                    {
                        Logger.Warning($"Process mở firewall: Rule không được verify ngay sau khi tạo (ExitCode: {exitCode}). Đang thử verify lại...");
                        // Đợi thêm một chút rồi verify lại
                        System.Threading.Thread.Sleep(2000);
                        bool verified = IsPortOpen(port, ruleName, retryCount: 5, delayMs: 1000);
                        if (verified)
                        {
                            Logger.Success($"Đã xác nhận rule tồn tại sau khi verify lại!");
                            return true;
                        }
                        Logger.Error($"Vẫn không tìm thấy rule sau khi verify lại.");
                        return false;
>>>>>>> 2306d4ff8607175fe37f458d62cda6a086df55c5
                    }
                    else
                    {
                        Logger.Error($"Process returned error code: {exitCode}");
                        return false;
                    }
                }
                finally
                {
<<<<<<< HEAD
                    process?.Dispose();
=======
                    if (process != null)
                    {
                        try
                        {
                            if (!process.HasExited)
                            {
                                process.Kill();
                            }
                            process.Dispose();
                        }
                        catch { }
                    }
>>>>>>> 2306d4ff8607175fe37f458d62cda6a086df55c5
                    if (tempBatchFile != null)
                    {
                        try 
                        { 
                            System.Threading.Thread.Sleep(200);
                            File.Delete(tempBatchFile); 
                        } 
                        catch { }
                    }
                }
            }
            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
<<<<<<< HEAD
                Logger.Warning("User denied Administrator request (UAC cancelled)");
=======
                Logger.Warning("Người dùng đã từ chối yêu cầu quyền Administrator (UAC bị hủy)");
>>>>>>> 2306d4ff8607175fe37f458d62cda6a086df55c5
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
<<<<<<< HEAD
                    SocketError.ConnectionRefused => "Port closed or no service listening",
                    SocketError.TimedOut => "Connection timeout - firewall may be blocking",
                    SocketError.NetworkUnreachable => "Network unreachable",
                    SocketError.HostUnreachable => "Host unreachable - check IP",
                    SocketError.HostNotFound => "Host not found",
                    _ => $"Socket error: {ex.SocketErrorCode}"
=======
                    SocketError.ConnectionRefused => "Không thể kết nối - Server đích chưa chạy hoặc port chưa mở",
                    SocketError.TimedOut => "Kết nối timeout - Firewall có thể đang chặn hoặc mạng chậm",
                    SocketError.NetworkUnreachable => "Không thể đến mạng đích - Kiểm tra kết nối mạng",
                    SocketError.HostUnreachable => "Không thể đến host - Kiểm tra IP có đúng không",
                    SocketError.HostNotFound => "Không tìm thấy host - IP không hợp lệ",
                    _ => $"Lỗi socket: {ex.SocketErrorCode}"
>>>>>>> 2306d4ff8607175fe37f458d62cda6a086df55c5
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
        /// L?y t?t c? ??a ch? IP c?a m�y
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
        /// Ki?m tra xem port c� ?ang ???c s? d?ng kh�ng
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
<<<<<<< HEAD
        /// Ki?m tra xem port c� ?ang l?ng nghe (LISTEN) kh�ng
=======
        /// Kiểm tra xem port có đang lắng nghe (LISTEN) không
>>>>>>> 2306d4ff8607175fe37f458d62cda6a086df55c5
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
<<<<<<< HEAD
                        
=======

>>>>>>> 2306d4ff8607175fe37f458d62cda6a086df55c5
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
<<<<<<< HEAD
                
=======

>>>>>>> 2306d4ff8607175fe37f458d62cda6a086df55c5
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
