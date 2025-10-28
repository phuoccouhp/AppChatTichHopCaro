using ChatAppClient.UserControls;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ChatAppClient.Forms
{
    public partial class frmHome : Form
    {
        // Danh sách để quản lý các Form Chat đang mở
        private Dictionary<string, frmChat> openChatForms;

        public frmHome()
        {
            InitializeComponent();
            openChatForms = new Dictionary<string, frmChat>();
        }

        private void frmHome_Load(object sender, EventArgs e)
        {
            // TODO: Lấy tên user từ GlobalState hoặc NetworkManager
            lblWelcome.Text = "Chào mừng, user1!";

            // TODO: Lấy danh sách bạn bè từ Server
            // NetworkManager.Instance.RequestFriendList();

            // Khi Server trả về danh sách, gọi: LoadFriendList(danhSach);

            // Giả lập danh sách bạn bè để test
            LoadFakeFriendList();
        }

        private void LoadFakeFriendList()
        {
            AddFriendToList("friend1", "Bạn Bè A", "Online", true);
            AddFriendToList("friend2", "Bạn Bè B", "Đang chơi game...", true);
            AddFriendToList("friend3", "Người Lạ C", "Offline 2 giờ trước", false);
            AddFriendToList("friend4", "Test User 1", "Online", true);
            AddFriendToList("friend5", "Test User 2", "Offline", false);
        }

        // Hàm này có thể được gọi khi Server trả về danh sách bạn
        public void AddFriendToList(string id, string name, string status, bool isOnline)
        {
            FriendListItem item = new FriendListItem();
            item.SetData(id, name, status, isOnline);

            // Gán sự kiện Click cho item (và các control con của nó)
            item.Click += (sender, e) => FriendItem_Click(item);
            foreach (Control control in item.Controls)
            {
                control.Click += (sender, e) => FriendItem_Click(item);
            }

            flpFriendsList.Controls.Add(item);
        }

        // Khi click vào một người bạn
        private void FriendItem_Click(FriendListItem item)
        {
            string friendId = item.FriendID;
            string friendName = item.FriendName;

            // Kiểm tra xem form chat với người này đã mở chưa
            if (openChatForms.ContainsKey(friendId))
            {
                // Nếu đã mở, chỉ cần kích hoạt (focus) nó
                openChatForms[friendId].Activate();
            }
            else
            {
                // Nếu chưa mở, tạo form mới
                frmChat chatForm = new frmChat(friendId, friendName);
                chatForm.FormClosed += (s, e) => {
                    // Xóa form khỏi danh sách khi nó bị đóng
                    openChatForms.Remove(friendId);
                };

                openChatForms.Add(friendId, chatForm);
                chatForm.Show();
            }
        }

        private void frmHome_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Khi form Home bị tắt, tắt toàn bộ ứng dụng
            // TODO: Gửi tin nhắn "Disconnect" lên Server
            Application.Exit();
        }
    }
}