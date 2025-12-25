#pragma warning disable SYSLIB0011
using ChatApp.Shared;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatAppServer
{
    public class ClientHandler
    {
        private TcpClient _client;
        private Server _server;
        private NetworkStream _stream;
        private JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public string UserID { get; private set; }
        public string UserName { get; private set; }
        public string ClientIP { get; private set; } = "Unknown";
        public DateTime LoginTime { get; private set; } = DateTime.Now;

        private string _currentOtp = null;
        private string _currentResetEmail = null;
        private DateTime? _otpCreatedTime = null;
        private const int OTP_VALID_MINUTES = 5;

        // OTP retry tracking
        private int _otpFailedAttempts = 0;
        private DateTime? _otpLockoutTime = null;
        private const int MAX_OTP_ATTEMPTS = 5;
        private const int LOCKOUT_MINUTES = 5;

        public ClientHandler(TcpClient client, Server server)
        {
            _client = client;
            _server = server;
            try
            {
                ClientIP = _client.Client.RemoteEndPoint?.ToString() ?? "Unknown";
                _stream = _client.GetStream();

                _client.ReceiveBufferSize = 131072;
                _client.SendBufferSize = 131072;
                _client.NoDelay = true;
                
                _stream.ReadTimeout = 120000;
                _stream.WriteTimeout = 60000;

                try
                {
                    _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    
                    byte[] keepAliveValues = new byte[12];
                    BitConverter.GetBytes((uint)1).CopyTo(keepAliveValues, 0);
                    BitConverter.GetBytes((uint)30000).CopyTo(keepAliveValues, 4);
                    BitConverter.GetBytes((uint)5000).CopyTo(keepAliveValues, 8);
                    _client.Client.IOControl(IOControlCode.KeepAliveValues, keepAliveValues, null);
                }
                catch (Exception kex)
                {
                    Logger.Warning($"[ClientHandler] Could not set keepalive options: {kex.Message}");
                }

                Logger.Info($"[CONNECT] New client connected: {ClientIP}");
            }
            catch (Exception ex)
            {
                Logger.Error("Error init handler", ex);
                Close();
            }
        }

        public async Task StartHandlingAsync()
        {
            if (_stream == null) return;
            byte[] lenBuf = new byte[4];
            int consecutiveTimeouts = 0;
            const int maxConsecutiveTimeouts = 5;

            while (_client != null && _client.Connected)
            {
                try
                {
                    int read = 0;
                    while (read < 4)
                    {
                        if (_stream == null || _client == null || !_client.Connected)
                            return;
                            
                        try
                        {
                            int r = await _stream.ReadAsync(lenBuf, read, 4 - read);
                            if (r == 0) return;
                            read += r;
                            consecutiveTimeouts = 0;
                        }
                        catch (IOException ioEx) when (ioEx.InnerException is SocketException sockEx && 
                            sockEx.SocketErrorCode == SocketError.TimedOut)
                        {
                            consecutiveTimeouts++;
                            Logger.Warning($"[{ClientIP}] Read timeout ({consecutiveTimeouts}/{maxConsecutiveTimeouts})");
                            
                            if (consecutiveTimeouts >= maxConsecutiveTimeouts)
                            {
                                Logger.Warning($"[{ClientIP}] Too many timeouts, disconnecting");
                                return;
                            }
                            continue;
                        }
                    }
                    
                    int payloadLen = BitConverter.ToInt32(lenBuf, 0);

                    if (payloadLen <= 0 || payloadLen > 10 * 1024 * 1024)
                    {
                        Logger.Error($"[{ClientIP}] Invalid packet size: {payloadLen}");
                        return;
                    }

                    byte[] payload = new byte[payloadLen];
                    int offset = 0;
                    while (offset < payloadLen)
                    {
                        if (_stream == null || _client == null || !_client.Connected)
                            return;
                            
                        int r = await _stream.ReadAsync(payload, offset, payloadLen - offset);
                        if (r == 0) return;
                        offset += r;
                    }

                    string jsonString = Encoding.UTF8.GetString(payload);

                    try
                    {
                        string snippet = jsonString.Length > 1000 ? jsonString.Substring(0, 1000) + "..." : jsonString;
                        Logger.Info($"[RECV RAW] {ClientIP}: {snippet}");
                    }
                    catch { }

                    PacketWrapper wrapper = null;
                    try
                    {
                        wrapper = JsonSerializer.Deserialize<PacketWrapper>(jsonString, _jsonOptions);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"[DESERIALIZE ERROR] Failed to deserialize PacketWrapper from client {ClientIP}", ex);
                    }

                    if (wrapper == null)
                    {
                        Logger.Warning($"[WARNING] Received null or invalid PacketWrapper from {ClientIP}");
                        continue;
                    }

                    if (string.IsNullOrEmpty(wrapper.Type))
                    {
                        Logger.Warning($"[WARNING] PacketWrapper.Type empty from {ClientIP}");
                        continue;
                    }

                    Type type = PacketMapper.GetPacketType(wrapper.Type);

                    if (type == null)
                    {
                        Logger.Warning($"[UNKNOWN PACKET] From {ClientIP} Type='{wrapper.Type}' - payload size={wrapper.Payload?.Length}");
                        continue;
                    }

                    object packet = null;
                    try
                    {
                        packet = JsonSerializer.Deserialize(wrapper.Payload, type, _jsonOptions);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"[DESERIALIZE ERROR] Failed to deserialize payload to {type.Name} from {ClientIP}", ex);
                        continue;
                    }

                    if (packet == null)
                    {
                        Logger.Warning($"[WARNING] Deserialized packet is null for type {type.Name} from {ClientIP}");
                        continue;
                    }

                    HandlePacket(packet);
                }
                catch (IOException) { break; }
                catch (Exception ex)
                {
                    Logger.Warning($"Error processing packet: {ex.Message}");
                    break;
                }
            }
            Close();
        }

        public void SendPacket(object packet)
        {
            if (_stream == null || !_stream.CanWrite) return;
            try
            {
                string simpleTypeName = packet.GetType().Name;
                string payloadJson = JsonSerializer.Serialize(packet, packet.GetType(), _jsonOptions);

                var wrapper = new PacketWrapper { Type = simpleTypeName, Payload = payloadJson };
                string wrapperJson = JsonSerializer.Serialize(wrapper, _jsonOptions);

                byte[] data = Encoding.UTF8.GetBytes(wrapperJson);
                byte[] lenBuf = BitConverter.GetBytes(data.Length);

                lock (_stream)
                {
                    _stream.Write(lenBuf, 0, 4);
                    _stream.Write(data, 0, data.Length);
                    _stream.Flush();
                }
            }
            catch { Close(); }
        }

        private void HandlePacket(object packet)
        {
            switch (packet)
            {
                case LoginPacket p: HandleLogin(p); break;
                case RegisterPacket p: HandleRegister(p); break;
                case ForgotPasswordPacket p: HandleForgotPasswordRequest(p); break;
                case ResetPasswordPacket p: HandleResetPassword(p); break;
                case UpdateProfilePacket p: HandleUpdateProfile(p); break;
                case ChatHistoryRequestPacket p: HandleChatHistoryRequest(p); break;
                case RequestOnlineListPacket p: HandleRequestOnlineList(p); break;

                case TextPacket p:
                    Logger.Info($"[Chat] {p.SenderID} -> {p.ReceiverID}: {p.MessageContent}");
                    _server.RelayPrivatePacket(p.ReceiverID, p);
                    _ = Task.Run(() => DatabaseManager.Instance.SaveMessage(p.SenderID, p.ReceiverID, p.MessageContent));
                    break;

                case FilePacket p:
                    Logger.Info($"[File] {p.SenderID} -> {p.ReceiverID}: {p.FileName}");
                    _server.RelayPrivatePacket(p.ReceiverID, p);
                    _ = Task.Run(() => DatabaseManager.Instance.SaveMessage(p.SenderID, p.ReceiverID, $"Sent file: {p.FileName}", p.IsImage ? "Image" : "File", p.FileName));
                    break;

                case GameInvitePacket p:
                    Logger.Info($"[GameInvite] {p.SenderID} -> {p.ReceiverID}");
                    _server.RelayPrivatePacket(p.ReceiverID, p);
                    _ = Task.Run(() =>
                    {
                        int id = DatabaseManager.Instance.SaveMessage(p.SenderID, p.ReceiverID, "Invited to play Caro", "GameInvite", "Caro");
                        _server.StoreGameInviteMessageId(p.SenderID, p.ReceiverID, id);
                    });
                    break;

                case TankInvitePacket p:
                    Logger.Info($"[TankInvite] {p.SenderID} -> {p.ReceiverID}");
                    _server.RelayPrivatePacket(p.ReceiverID, p);
                    _ = Task.Run(() =>
                    {
                        int id = DatabaseManager.Instance.SaveMessage(p.SenderID, p.ReceiverID, "Invited to play Tank", "GameInvite", "Tank");
                        _server.StoreGameInviteMessageId(p.SenderID, p.ReceiverID, id);
                    });
                    break;

                case GameResponsePacket p: _server.ProcessGameResponse(p); break;
                case GameMovePacket p: _server.ProcessGameMove(p); break;
                case RematchRequestPacket p: _server.ProcessRematchRequest(p); break;
                case RematchResponsePacket p: _server.ProcessRematchResponse(p); break;
                case TankResponsePacket p: _server.ProcessTankResponse(p); break;
                case TankActionPacket p: _server.ProcessTankAction(p); break;
                case TankHitPacket p: _server.TankGameManager.ProcessHit(p.GameID, p.HitPlayerID, p.Damage, _server); break;

                case CreateGroupPacket p: HandleCreateGroup(p); break;
                case GroupTextPacket p: HandleGroupText(p); break;
                case GroupFilePacket p: HandleGroupFile(p); break;
                case GroupInvitePacket p: HandleGroupInvite(p); break;
                case LeaveGroupPacket p: HandleLeaveGroup(p); break;
                case RequestGroupListPacket p: HandleRequestGroupList(p); break;
                case GroupHistoryRequestPacket p: HandleGroupHistoryRequest(p); break;
            }
        }

        private void HandleLogin(LoginPacket p)
        {
            string loginUser = p.Username ?? p.Email;
            bool useEmail = p.UseEmailLogin;
            string password = p.Password;
            
            var user = DatabaseManager.Instance.Login(loginUser, password, useEmail);
            
            if (user != null)
            {
                UserID = user.Username;
                UserName = user.DisplayName;

                Logger.Success($"[LOGIN] User '{UserName}' ({UserID}) logged in from IP: {ClientIP}");

                var onlineUsers = _server.GetOnlineUsers(UserID);
                SendPacket(new LoginResultPacket { Success = true, UserID = UserID, UserName = UserName, OnlineUsers = onlineUsers });
                
                _server.RegisterClient(UserID, this);
            }
            else
            {
                Logger.Warning($"[LOGIN FAIL] Login failed from IP {ClientIP} with user: {loginUser}");
                SendPacket(new LoginResultPacket { Success = false, Message = "Wrong username or password" });
                
                Task.Delay(300).ContinueWith(_ => Close());
            }
        }

        private void HandleRegister(RegisterPacket p)
        {
            bool ok = DatabaseManager.Instance.RegisterUser(p.Username, p.Password, p.Email);
            if (ok) Logger.Success($"[REGISTER] New user registered: {p.Username}");
            else Logger.Warning($"[REGISTER FAIL] Registration failed for user: {p.Username}");

            SendPacket(new RegisterResultPacket { Success = ok, Message = ok ? "Success" : "User exists" });
        }

        private void HandleForgotPasswordRequest(ForgotPasswordPacket p)
        {
            // Reset lockout when requesting new OTP
            _otpFailedAttempts = 0;
            _otpLockoutTime = null;

            if (DatabaseManager.Instance.CheckEmailExists(p.Email))
            {
                _currentOtp = new Random().Next(100000, 999999).ToString();
                _currentResetEmail = p.Email;
                _otpCreatedTime = DateTime.Now;
                bool mailSent = EmailHelper.SendOTP(p.Email, _currentOtp);
                
                if (mailSent)
                {
                    Logger.Info($"[ForgotPass] OTP sent to email: {p.Email} - OTP: {_currentOtp}");
                    SendPacket(new ForgotPasswordResultPacket { Success = true, IsStep1Success = true, Message = "OTP has been sent. Please check your email." });
                }
                else
                {
                    Logger.Error($"[ForgotPass] Could not send OTP to email: {p.Email}");
                    SendPacket(new ForgotPasswordResultPacket { Success = false, Message = "Error sending email. Please check email configuration or try again later." });
                    _currentOtp = null;
                    _currentResetEmail = null;
                    _otpCreatedTime = null;
                }
            }
            else
            {
                SendPacket(new ForgotPasswordResultPacket { Success = false, Message = "Email does not exist in the system." });
            }
        }

        private void HandleResetPassword(ResetPasswordPacket p)
        {
            // Check input
            if (p == null)
            {
                Logger.Error("[ResetPass] ResetPasswordPacket is null");
                SendPacket(new ForgotPasswordResultPacket { Success = false, Message = "Invalid data." });
                return;
            }

            if (string.IsNullOrWhiteSpace(p.Email))
            {
                Logger.Warning("[ResetPass] Email cannot be empty");
                SendPacket(new ForgotPasswordResultPacket { Success = false, Message = "Email cannot be empty." });
                return;
            }

            if (string.IsNullOrWhiteSpace(p.OtpCode))
            {
                Logger.Warning($"[ResetPass] OTP cannot be empty for email: {p.Email}");
                SendPacket(new ForgotPasswordResultPacket { Success = false, Message = "OTP code cannot be empty." });
                return;
            }

            // Check lockout
            if (_otpLockoutTime != null)
            {
                double minutesSinceLockout = (DateTime.Now - _otpLockoutTime.Value).TotalMinutes;
                if (minutesSinceLockout < LOCKOUT_MINUTES)
                {
                    int remainingMinutes = (int)Math.Ceiling(LOCKOUT_MINUTES - minutesSinceLockout);
                    Logger.Warning($"[ResetPass] User is locked out for email: {p.Email}. Remaining: {remainingMinutes} minutes");
                    SendPacket(new ForgotPasswordResultPacket { Success = false, Message = $"Too many failed attempts. Please wait {remainingMinutes} minutes before trying again." });
                    return;
                }
                else
                {
                    // Lockout expired, reset
                    _otpLockoutTime = null;
                    _otpFailedAttempts = 0;
                }
            }

            bool isVerifyOnly = string.IsNullOrWhiteSpace(p.NewPassword);

            // Check if OTP was created
            if (string.IsNullOrEmpty(_currentOtp) || string.IsNullOrEmpty(_currentResetEmail))
            {
                Logger.Warning($"[ResetPass] No OTP was created for email: {p.Email}");
                SendPacket(new ForgotPasswordResultPacket { Success = false, Message = "Please request an OTP first." });
                return;
            }

            // Check email match
            if (!string.Equals(_currentResetEmail, p.Email, StringComparison.OrdinalIgnoreCase))
            {
                Logger.Warning($"[ResetPass] Email mismatch. Requested: {p.Email}, OTP belongs to: {_currentResetEmail}");
                SendPacket(new ForgotPasswordResultPacket { Success = false, Message = "Email does not match the email that requested OTP." });
                return;
            }

            // Check OTP expiry
            if (_otpCreatedTime == null)
            {
                Logger.Warning($"[ResetPass] OTP creation time is invalid for email: {p.Email}");
                _currentOtp = null;
                _currentResetEmail = null;
                _otpCreatedTime = null;
                SendPacket(new ForgotPasswordResultPacket { Success = false, Message = "OTP is invalid. Please request a new code." });
                return;
            }

            double minutesSinceCreation = (DateTime.Now - _otpCreatedTime.Value).TotalMinutes;
            if (minutesSinceCreation > OTP_VALID_MINUTES)
            {
                Logger.Warning($"[ResetPass] OTP expired for email: {p.Email} (elapsed {minutesSinceCreation:F1} minutes)");
                _currentOtp = null;
                _currentResetEmail = null;
                _otpCreatedTime = null;
                SendPacket(new ForgotPasswordResultPacket { Success = false, Message = "OTP has expired. Please request a new code." });
                return;
            }

            // Compare OTP
            string expectedOtp = _currentOtp?.Trim() ?? string.Empty;
            string providedOtp = p.OtpCode?.Trim() ?? string.Empty;
            
            if (string.IsNullOrEmpty(expectedOtp))
            {
                Logger.Error($"[ResetPass] CRITICAL ERROR: _currentOtp is null or empty after initial check for email: {p.Email}");
                SendPacket(new ForgotPasswordResultPacket { Success = false, Message = "System error. Please try again." });
                return;
            }
            
            if (string.IsNullOrEmpty(providedOtp))
            {
                Logger.Warning($"[ResetPass] Provided OTP is empty for email: {p.Email}");
                SendPacket(new ForgotPasswordResultPacket { Success = false, Message = "OTP code cannot be empty." });
                return;
            }
            
            bool isOtpValid = string.Equals(expectedOtp, providedOtp, StringComparison.Ordinal);
            
            if (!isOtpValid)
            {
                // OTP is wrong - increment failed attempts
                _otpFailedAttempts++;
                int remainingAttempts = MAX_OTP_ATTEMPTS - _otpFailedAttempts;
                
                Logger.Warning($"[ResetPass] WRONG OTP for email: {p.Email}. Entered: '{providedOtp}', Correct: '{expectedOtp}'. Attempt {_otpFailedAttempts}/{MAX_OTP_ATTEMPTS}");
                
                if (_otpFailedAttempts >= MAX_OTP_ATTEMPTS)
                {
                    // Lock out the user
                    _otpLockoutTime = DateTime.Now;
                    Logger.Warning($"[ResetPass] User locked out for {LOCKOUT_MINUTES} minutes after {MAX_OTP_ATTEMPTS} failed attempts for email: {p.Email}");
                    SendPacket(new ForgotPasswordResultPacket { Success = false, Message = $"Too many failed attempts ({MAX_OTP_ATTEMPTS}). Please wait {LOCKOUT_MINUTES} minutes before trying again." });
                }
                else
                {
                    SendPacket(new ForgotPasswordResultPacket { Success = false, Message = $"Incorrect OTP. You have {remainingAttempts} attempts remaining." });
                }
                return;
            }

            // OTP is valid - reset failed attempts
            _otpFailedAttempts = 0;
            _otpLockoutTime = null;
            
            if (isVerifyOnly)
            {
                Logger.Info($"[ResetPass] OTP verified successfully for email: {p.Email} (verify only, not changing password)");
                SendPacket(new ForgotPasswordResultPacket { Success = true, IsStep1Success = false, Message = "OTP verified successfully" });
            }
            else
            {
                Logger.Info($"[ResetPass] OTP verified successfully for email: {p.Email}. Proceeding to change password...");
                
                try
                {
                    DatabaseManager.Instance.UpdatePassword(p.Email, p.NewPassword);
                    Logger.Success($"[ResetPass] Password changed successfully for email: {p.Email}");
                    
                    _currentOtp = null;
                    _currentResetEmail = null;
                    _otpCreatedTime = null;
                    
                    SendPacket(new ForgotPasswordResultPacket { Success = true, IsStep1Success = false, Message = "Password Changed" });
                }
                catch (Exception ex)
                {
                    Logger.Error($"[ResetPass] Error changing password for email: {p.Email}", ex);
                    SendPacket(new ForgotPasswordResultPacket { Success = false, Message = "System error while changing password. Please try again later." });
                }
            }
        }

        private void HandleUpdateProfile(UpdateProfilePacket p)
        {
            DatabaseManager.Instance.UpdateDisplayName(p.UserID, p.NewDisplayName);
            this.UserName = p.NewDisplayName;
            Logger.Info($"[Profile] User {UserID} changed name to: {p.NewDisplayName}");
            _server.BroadcastPacket(p, null);
        }

        private void HandleChatHistoryRequest(ChatHistoryRequestPacket p)
        {
            var dbMessages = DatabaseManager.Instance.GetChatHistory(p.UserID, p.FriendID, p.Limit);
            var sharedMessages = dbMessages.Select(m => new ChatHistoryMessage
            {
                MessageID = m.MessageID,
                SenderID = m.SenderID ?? "",
                ReceiverID = m.ReceiverID ?? "",
                MessageContent = m.MessageContent ?? "",
                MessageType = m.MessageType,
                FileName = m.FileName,
                CreatedAt = m.CreatedAt
            }).ToList();

            var response = new ChatHistoryResponsePacket { Success = true, Message = "OK", Messages = sharedMessages };
            SendPacket(response);
        }

        private void HandleRequestOnlineList(RequestOnlineListPacket p)
        {
            SendPacket(new OnlineListPacket { OnlineUsers = _server.GetOnlineUsers(UserID) });
        }

        private void HandleCreateGroup(CreateGroupPacket p)
        {
            Logger.Info($"[Group] {p.CreatorID} creating group '{p.GroupName}' with {p.MemberIDs.Count} members");
            
            string groupId = DatabaseManager.Instance.CreateGroup(p.GroupName, p.CreatorID, p.MemberIDs);
            
            if (string.IsNullOrEmpty(groupId))
            {
                SendPacket(new CreateGroupResultPacket { Success = false, Message = "Could not create group" });
                return;
            }
            
            var members = DatabaseManager.Instance.GetGroupMembers(groupId);
            
            SendPacket(new CreateGroupResultPacket 
            { 
                Success = true, 
                GroupID = groupId, 
                GroupName = p.GroupName,
                Members = members
            });
            
            var notification = new GroupInviteNotificationPacket
            {
                GroupID = groupId,
                GroupName = p.GroupName,
                InviterName = UserName,
                Members = members
            };
            
            foreach (var memberId in p.MemberIDs)
            {
                if (memberId != p.CreatorID)
                {
                    _server.RelayPrivatePacket(memberId, notification);
                }
            }
        }

        private void HandleGroupText(GroupTextPacket p)
        {
            Logger.Info($"[Group] {p.SenderID} -> Group {p.GroupID}: {p.MessageContent}");
            
            _ = Task.Run(() => DatabaseManager.Instance.SaveGroupMessage(p.GroupID, p.SenderID, p.MessageContent, "Text"));
            
            var members = DatabaseManager.Instance.GetGroupMembers(p.GroupID);
            p.SenderName = UserName;
            p.SentAt = DateTime.Now;
            
            foreach (var member in members)
            {
                if (member.UserID != p.SenderID)
                {
                    _server.RelayPrivatePacket(member.UserID, p);
                }
            }
        }

        private void HandleGroupFile(GroupFilePacket p)
        {
            Logger.Info($"[Group] File from {p.SenderID} -> Group {p.GroupID}: {p.FileName}");
            
            string type = p.IsImage ? "Image" : "File";
            _ = Task.Run(() => DatabaseManager.Instance.SaveGroupMessage(p.GroupID, p.SenderID, $"Sent {type}: {p.FileName}", type, p.FileName));
            
            var members = DatabaseManager.Instance.GetGroupMembers(p.GroupID);
            p.SenderName = UserName;
            
            foreach (var member in members)
            {
                if (member.UserID != p.SenderID)
                {
                    _server.RelayPrivatePacket(member.UserID, p);
                }
            }
        }

        private void HandleGroupInvite(GroupInvitePacket p)
        {
            Logger.Info($"[Group] {p.InviterID} inviting {p.InviteeIDs.Count} people to group {p.GroupID}");
            
            var members = DatabaseManager.Instance.GetGroupMembers(p.GroupID);
            
            foreach (var inviteeId in p.InviteeIDs)
            {
                if (DatabaseManager.Instance.AddGroupMember(p.GroupID, inviteeId))
                {
                    var notification = new GroupInviteNotificationPacket
                    {
                        GroupID = p.GroupID,
                        GroupName = p.GroupName,
                        InviterName = UserName,
                        Members = DatabaseManager.Instance.GetGroupMembers(p.GroupID)
                    };
                    _server.RelayPrivatePacket(inviteeId, notification);
                    
                    var memberUpdate = new GroupMemberUpdatePacket
                    {
                        GroupID = p.GroupID,
                        UserID = inviteeId,
                        UserName = DatabaseManager.Instance.GetDisplayName(inviteeId),
                        Joined = true
                    };
                    
                    foreach (var member in members)
                    {
                        _server.RelayPrivatePacket(member.UserID, memberUpdate);
                    }
                }
            }
        }

        private void HandleLeaveGroup(LeaveGroupPacket p)
        {
            Logger.Info($"[Group] {p.UserID} leaving group {p.GroupID}");
            
            var members = DatabaseManager.Instance.GetGroupMembers(p.GroupID);
            
            if (DatabaseManager.Instance.RemoveGroupMember(p.GroupID, p.UserID))
            {
                var memberUpdate = new GroupMemberUpdatePacket
                {
                    GroupID = p.GroupID,
                    UserID = p.UserID,
                    UserName = UserName,
                    Joined = false
                };
                
                foreach (var member in members)
                {
                    if (member.UserID != p.UserID)
                    {
                        _server.RelayPrivatePacket(member.UserID, memberUpdate);
                    }
                }
            }
        }

        private void HandleRequestGroupList(RequestGroupListPacket p)
        {
            var groups = DatabaseManager.Instance.GetUserGroups(p.UserID);
            SendPacket(new GroupListPacket { Groups = groups });
        }

        private void HandleGroupHistoryRequest(GroupHistoryRequestPacket p)
        {
            if (!DatabaseManager.Instance.IsGroupMember(p.GroupID, p.UserID))
            {
                SendPacket(new GroupHistoryResponsePacket { Success = false, GroupID = p.GroupID });
                return;
            }
            
            var messages = DatabaseManager.Instance.GetGroupHistory(p.GroupID, p.Limit);
            SendPacket(new GroupHistoryResponsePacket 
            { 
                Success = true, 
                GroupID = p.GroupID,
                Messages = messages 
            });
        }

        public void Close()
        {
            if (UserID != null)
            {
                Logger.Warning($"[LOGOUT] User '{UserName}' ({UserID}) disconnected.");
                _server.RemoveClient(UserID);
            }
            _client?.Close();
        }

        public void CloseConnectionOnly()
        {
            _client?.Close();
        }
    }
}
