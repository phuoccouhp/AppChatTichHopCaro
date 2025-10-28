using ChatAppClient.Helpers;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ChatAppClient.UserControls
{
    public partial class FileBubble : UserControl
    {
        private MessageType _type;
        private int _bubbleWidth = 0;

        // Biến để lưu dữ liệu file
        private byte[] _fileData;
        private string _fileName;

        public FileBubble()
        {
            InitializeComponent();
            btnDownload.Click += BtnDownload_Click;
        }

        public void SetMessage(string fileName, byte[] fileData, MessageType type, int parentUsableWidth)
        {
            _type = type;
            _fileName = fileName;
            _fileData = fileData; // Lưu lại

            lblFileName.Text = fileName;

            // Cập nhật màu nền dựa trên người gửi/nhận
            if (type == MessageType.Outgoing)
            {
                pnlContainer.BackColor = AppColors.Primary; // Màu xanh
                lblFileName.ForeColor = Color.White;
            }
            else
            {
                pnlContainer.BackColor = AppColors.LightGray; // Màu xám
                lblFileName.ForeColor = Color.Black;
            }

            // --- ĐÃ XÓA KHỐI LỖI ---
            // Không cần làm gì ở đây cả,
            // vì pbIcon.Image đã được gán icon Shield
            // trong file .designer.cs

            _bubbleWidth = pnlContainer.Width;
            UpdateMargins(parentUsableWidth);
        }

        // Sự kiện khi nhấn nút Tải về
        private void BtnDownload_Click(object sender, EventArgs e)
        {
            if (_fileData == null)
            {
                MessageBox.Show("Không có dữ liệu file để lưu.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = _fileName; // Tên file mặc định
            sfd.Title = "Lưu file";
            sfd.Filter = "All Files|*.*";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllBytes(sfd.FileName, _fileData);
                    MessageBox.Show("Tải file thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi lưu file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Copy hàm này từ ChatMessageBubble
        public void UpdateMargins(int parentUsableWidth)
        {
            if (_bubbleWidth == 0) _bubbleWidth = this.Width;
            int remainingSpace = parentUsableWidth - _bubbleWidth;
            if (remainingSpace < 0) remainingSpace = 0;

            if (_type == MessageType.Outgoing)
            {
                // Căn Phải
                this.Margin = new Padding(remainingSpace, 5, 0, 5);
            }
            else
            {
                // Căn Trái
                this.Margin = new Padding(0, 5, remainingSpace, 5);
            }
        }
    }
}