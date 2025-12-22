using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ChatAppClient.UserControls
{
    public partial class ImageBubble : UserControl
    {
        private MessageType _type;
        private int _bubbleWidth = 0;
        private byte[] _imageData;

        public event EventHandler<byte[]>? OnForwardRequested;

        public ImageBubble()
        {
            InitializeComponent();
            btnDownload.Click += BtnDownload_Click;
            btnForward.Click += BtnForward_Click;
            
            // Hover events
            this.MouseEnter += ImageBubble_MouseEnter;
            this.MouseLeave += ImageBubble_MouseLeave;
            pbImage.MouseEnter += ImageBubble_MouseEnter;
            pbImage.MouseLeave += ImageBubble_MouseLeave;
        }

        private void ImageBubble_MouseEnter(object? sender, EventArgs e)
        {
            btnDownload.Visible = true;
            btnForward.Visible = true;
            btnDownload.BringToFront();
            btnForward.BringToFront();
        }

        private void ImageBubble_MouseLeave(object? sender, EventArgs e)
        {
            if (!btnDownload.ClientRectangle.Contains(btnDownload.PointToClient(Control.MousePosition)) &&
                !btnForward.ClientRectangle.Contains(btnForward.PointToClient(Control.MousePosition)))
            {
                btnDownload.Visible = false;
                btnForward.Visible = false;
            }
        }

        private void BtnDownload_Click(object? sender, EventArgs e)
        {
            if (_imageData == null || pbImage.Image == null)
            {
                MessageBox.Show("Không có dữ liệu ảnh để lưu.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = "image.png";
            sfd.Title = "Lưu ảnh";
            sfd.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.gif;*.bmp";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    pbImage.Image.Save(sfd.FileName);
                    MessageBox.Show("Tải ảnh thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi lưu ảnh: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnForward_Click(object? sender, EventArgs e)
        {
            if (_imageData != null)
            {
                OnForwardRequested?.Invoke(this, _imageData);
            }
        }

        public void SetMessage(byte[] imageData, MessageType type, int parentUsableWidth)
        {
            _type = type;
            _imageData = imageData;

            try
            {
                using (var ms = new MemoryStream(imageData))
                {
                    Image img = Image.FromStream(ms);
                    int maxWidth = 300;
                    int newHeight = (int)((double)img.Height / img.Width * maxWidth);

                    pbImage.Size = new Size(maxWidth, newHeight);
                    pbImage.Image = img;
                }
            }
            catch (Exception ex)
            {
                pbImage.Image = pbImage.ErrorImage;
            }

            this.Size = new Size(pbImage.Width + 6, pbImage.Height + 6);
            _bubbleWidth = this.Width;

            UpdateMargins(parentUsableWidth);
        }

        public void UpdateMargins(int parentUsableWidth)
        {
            if (_bubbleWidth == 0) _bubbleWidth = this.Width;
            int remainingSpace = parentUsableWidth - _bubbleWidth;
            if (remainingSpace < 0) remainingSpace = 0;

            if (_type == MessageType.Outgoing)
            {
                this.Margin = new Padding(remainingSpace, 5, 0, 5);
            }
            else
            {
                this.Margin = new Padding(0, 5, remainingSpace, 5);
            }
        }
    }
}