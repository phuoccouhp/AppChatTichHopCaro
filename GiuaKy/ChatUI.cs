using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GiuaKy
{
    public partial class ChatUI : UserControl
    {
        public ChatUI()
        {
            Initialize();
            LoadOnlineUsers();
            LoadChatList();
        }

        private void panel_chat_Paint(object sender, PaintEventArgs e)
        {

        }
        private void LoadOnlineUsers()
        {
            panel_online.Controls.Clear();

            // Tạo flow panel chứa avatar
            FlowLayoutPanel flowOnline = new FlowLayoutPanel();
            flowOnline.Dock = DockStyle.Fill;
            flowOnline.WrapContents = false; // không xuống dòng
            flowOnline.FlowDirection = FlowDirection.LeftToRight; // xếp ngang
            flowOnline.AutoScroll = true; // bật cuộn, mặc định là ngang

            // 🔥 ép tắt thanh cuộn dọc bằng event Layout
            flowOnline.Layout += (s, e) =>
            {
                flowOnline.VerticalScroll.Maximum = 0;
                flowOnline.VerticalScroll.Visible = false;
                flowOnline.HorizontalScroll.Visible = true;
            };

            panel_online.Controls.Add(flowOnline);

            // Giả lập người online
            for (int i = 1; i <= 12; i++)
            {
                PictureBox avatar = new PictureBox();
                avatar.Width = 60;
                avatar.Height = 60;
                avatar.SizeMode = PictureBoxSizeMode.Zoom;
                avatar.Margin = new Padding(10, 10, 0, 10);
                avatar.BackColor = Color.LightGray;
                avatar.Cursor = Cursors.Hand;

                int index = i;
                avatar.Click += (s, e) => OpenConversation("User " + index);

                flowOnline.Controls.Add(avatar);
            }
        }

        private void LoadChatList()
        {
            // Giả lập 10 đoạn hội thoại
            for (int i = 1; i <= 10; i++)
            {
                Button btn = new Button();
                btn.Text = "Người dùng " + i;
                btn.Dock = DockStyle.Top;
                btn.Height = 50;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.BackColor = Color.WhiteSmoke;
                btn.TextAlign = ContentAlignment.MiddleLeft;
                btn.Font = new Font("Segoe UI", 10, FontStyle.Regular);

                btn.Click += (s, e) => OpenConversation("Người dùng " + i);
                panel_list.Controls.Add(btn);
            }
        }
        private void OpenConversation(string userName)
        {
            panel_chat.Controls.Clear();

            Label lbl = new Label();
            lbl.Text = $"Đang chat với {userName}";
            lbl.Dock = DockStyle.Top;
            lbl.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lbl.Height = 40;
            lbl.TextAlign = ContentAlignment.MiddleCenter;

            TextBox txtChat = new TextBox();
            txtChat.Multiline = true;
            txtChat.Dock = DockStyle.Fill;
            txtChat.ScrollBars = ScrollBars.Vertical;

            panel_chat.Controls.Add(txtChat);
            panel_chat.Controls.Add(lbl);
        }

        private void panel_online_Paint(object sender, PaintEventArgs e)
        {

        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ChatUI
            // 
            this.Name = "ChatUI";
            this.Load += new System.EventHandler(this.ChatUI_Load);
            this.ResumeLayout(false);

        }

        private void ChatUI_Load(object sender, EventArgs e)
        {

        }
    }
}
