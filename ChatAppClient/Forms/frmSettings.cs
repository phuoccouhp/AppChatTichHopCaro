using ChatApp.Shared;
using ChatAppClient.Helpers;
using ChatAppClient.Forms;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ChatAppClient.Forms
{
    public partial class frmSettings : Form
    {
        private byte[] _newAvatarBytes = null;

        public frmSettings()
        {
            InitializeComponent();
        }

        private void frmSettings_Load(object sender, EventArgs e)
        {
            string? myId = NetworkManager.Instance.UserID ?? "";
            string? myName = NetworkManager.Instance.UserName ?? "";

            lblUserID.Text = $"ID: {myId}";
            txtDisplayName.Text = myName;

            string avatarPath = Path.Combine("Images", $"{myId}.png");
            string defaultPath = Path.Combine("Images", "default_avatar.png");

            try
            {
                if (File.Exists(avatarPath))
                {
                    using (var temp = new Bitmap(avatarPath)) pbAvatar.Image = new Bitmap(temp);
                }
                else if (File.Exists(defaultPath))
                {
                    using (var temp = new Bitmap(defaultPath)) pbAvatar.Image = new Bitmap(temp);
                }
            }
            catch { }
        }

        private void btnChangeAvatar_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _newAvatarBytes = File.ReadAllBytes(dialog.FileName);
                    using (var ms = new MemoryStream(_newAvatarBytes))
                    {
                        pbAvatar.Image = Image.FromStream(ms);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi chọn ảnh: " + ex.Message);
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string newName = txtDisplayName.Text.Trim();
            if (string.IsNullOrEmpty(newName))
            {
                MessageBox.Show("Tên hiển thị không được để trống.");
                return;
            }

            string? myId = NetworkManager.Instance.UserID;
            if (myId == null)
            {
                MessageBox.Show("Không thể xác định UserID. Vui lòng đăng nhập lại.");
                return;
            }

            var packet = new UpdateProfilePacket
            {
                UserID = myId,
                NewDisplayName = newName,
                HasNewAvatar = (_newAvatarBytes != null),
                NewAvatarData = _newAvatarBytes
            };

            NetworkManager.Instance.SendPacket(packet);
            MessageBox.Show("Đã gửi yêu cầu cập nhật!");
            this.Close();
        }
    }
}