using ChatAppClient.Helpers;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace ChatAppClient.UserControls
{
    public partial class ImageBubble : UserControl
    {
        private byte[] _rawData;
        private string _fileName;

        // Sự kiện Forward
        public event EventHandler<(string fileName, byte[] fileData)>? OnForwardRequested;

        public ImageBubble()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            // Sự kiện Click nút
            btnDownload.Click += BtnDownload_Click;
            btnForward.Click += BtnForward_Click;

            // Sự kiện Hover để hiện nút
            this.MouseEnter += (s, e) => ToggleButtons(true);
            this.MouseLeave += (s, e) => CheckMouseLeave();
            pbImage.MouseEnter += (s, e) => ToggleButtons(true);
            pbImage.MouseLeave += (s, e) => CheckMouseLeave();

            // Click vào ảnh để xem (hoặc lưu)
            pbImage.Click += BtnDownload_Click;
        }

        // Logic ẩn/hiện nút khi di chuột
        private void ToggleButtons(bool show)
        {
            btnDownload.Visible = show;
            btnForward.Visible = show;
            if (show)
            {
                btnDownload.BringToFront();
                btnForward.BringToFront();
            }
        }

        private void CheckMouseLeave()
        {
            Point pt = this.PointToClient(Control.MousePosition);
            if (!this.ClientRectangle.Contains(pt))
            {
                ToggleButtons(false);
            }
        }

        public void SetImage(Image img, string fileName, byte[] rawData, MessageType type)
        {
            _rawData = rawData;
            _fileName = fileName;

            // 1. Tính toán kích thước hiển thị (Max 300x400)
            int maxWidth = 300;
            int maxHeight = 400;

            double ratioX = (double)maxWidth / img.Width;
            double ratioY = (double)maxHeight / img.Height;
            double ratio = Math.Min(ratioX, ratioY);

            // Nếu ảnh nhỏ hơn max thì giữ nguyên, lớn hơn thì thu nhỏ
            int newWidth = (ratio < 1) ? (int)(img.Width * ratio) : img.Width;
            int newHeight = (ratio < 1) ? (int)(img.Height * ratio) : img.Height;

            // Đảm bảo không quá nhỏ để chứa nút
            if (newWidth < 80) newWidth = 80;
            if (newHeight < 80) newHeight = 80;

            this.Size = new Size(newWidth, newHeight);
            pbImage.Size = this.Size;
            pbImage.Image = img;

            // Bo góc ảnh
            SetRoundedRegion(pbImage, 15);
        }

        private void SetRoundedRegion(Control c, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(c.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(c.Width - radius, c.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, c.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            c.Region = new Region(path);
        }

        private void BtnDownload_Click(object sender, EventArgs e)
        {
            if (_rawData == null) return;
            SaveFileDialog sfd = new SaveFileDialog
            {
                FileName = _fileName,
                Filter = "Images|*.jpg;*.png;*.bmp;*.gif"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllBytes(sfd.FileName, _rawData);
                    MessageBox.Show("Lưu ảnh thành công!", "Thông báo");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi lưu ảnh: " + ex.Message);
                }
            }
        }

        private void BtnForward_Click(object sender, EventArgs e)
        {
            if (_rawData != null)
            {
                OnForwardRequested?.Invoke(this, (_fileName, _rawData));
            }
        }
    }
}