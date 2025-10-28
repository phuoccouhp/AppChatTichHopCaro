using ChatAppClient.UserControls;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ChatApp.Shared;
namespace ChatAppClient.Forms
{
    public partial class frmHome : Form
    {
        // FIX 1: Đổi Dictionary từ frmChat sang ChatViewControl
        private Dictionary<string, ChatViewControl> openChatControls;
        private ChatViewControl _currentChatControl = null; // Control chat đang hiển thị

        public frmHome()
        {
            InitializeComponent();
            // FIX 2: Khởi tạo Dictionary mới
            openChatControls = new Dictionary<string, ChatViewControl>();
        }

        private void frmHome_Load(object sender, EventArgs e)
        {
            lblWelcome.Text = "Chào mừng, user1!";
            LoadFakeFriendList();

            // Xóa text "Chọn một người bạn..."
            lblMainWelcome.Visible = true;
        }

        private void LoadFakeFriendList()
        {
            AddFriendToList("friend1", "Bạn Bè A", "Online", true);
            AddFriendToList("friend2", "Bạn Bè B", "Đang chơi game...", true);
            AddFriendToList("friend3", "Người Lạ C", "Offline 2 giờ trước", false);
            // ...
        }

        public void AddFriendToList(string id, string name, string status, bool isOnline)
        {
            FriendListItem item = new FriendListItem();
            item.SetData(id, name, status, isOnline);

            item.Click += (sender, e) => FriendItem_Click(item);
            foreach (Control control in item.Controls)
            {
                control.Click += (sender, e) => FriendItem_Click(item);
            }

            flpFriendsList.Controls.Add(item);
        }

        // FIX 3: (QUAN TRỌNG) Đây là logic chính
        private void FriendItem_Click(FriendListItem item)
        {
            string friendId = item.FriendID;
            string friendName = item.FriendName;

            // Ẩn lời chào
            lblMainWelcome.Visible = false;

            // Nếu đang click vào chat hiện tại, không làm gì cả
            if (_currentChatControl != null && _currentChatControl.Name == friendId)
            {
                return;
            }

            // Ẩn control chat cũ đi (nếu có)
            if (_currentChatControl != null)
            {
                _currentChatControl.Visible = false;
            }

            ChatViewControl chatControl;
            // Kiểm tra xem đã mở control chat này bao giờ chưa (cache)
            if (openChatControls.ContainsKey(friendId))
            {
                // Nếu rồi, lấy nó từ cache
                chatControl = openChatControls[friendId];
                chatControl.Visible = true;
            }
            else
            {
                // Nếu chưa, tạo mới
                // Truyền 'this' (form Home) vào để xử lý Invoke
                chatControl = new ChatViewControl(friendId, friendName, this);
                chatControl.Name = friendId; // Đặt tên để quản lý
                chatControl.Dock = DockStyle.Fill; // Cho nó lấp đầy pnlMain

                openChatControls.Add(friendId, chatControl); // Thêm vào cache
                pnlMain.Controls.Add(chatControl); // Thêm vào pnlMain
            }

            // Đặt nó làm control hiện tại và mang lên trên
            _currentChatControl = chatControl;
            chatControl.BringToFront();
        }
        // (Trong public partial class frmHome : Form)

        // Hàm này được gọi bởi NetworkManager khi có tin nhắn TEXT
        public void HandleIncomingTextMessage(TextPacket packet)
        {
            // Tìm control chat tương ứng với người gửi
            if (openChatControls.ContainsKey(packet.SenderID))
            {
                var chatControl = openChatControls[packet.SenderID];
                // Gọi hàm của control đó
                chatControl.ReceiveMessage(packet.MessageContent);
            }
            else
            {
                // TODO: Hiển thị thông báo tin nhắn mới
            }
        }

        // Hàm này được gọi bởi NetworkManager khi có tin nhắn FILE/ẢNH
        public void HandleIncomingFileMessage(FilePacket packet)
        {
            // Tìm control chat tương ứng
            if (openChatControls.ContainsKey(packet.SenderID))
            {
                var chatControl = openChatControls[packet.SenderID];
                // Gọi hàm nhận file của control đó
                chatControl.ReceiveFileMessage(packet, MessageType.Incoming);
            }
            else
            {
                // TODO: Hiển thị thông báo file mới
            }
        }

        // ... các hàm xử lý game...
        private void frmHome_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}