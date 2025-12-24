using ChatAppClient.Helpers;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
// using ChatAppClient; // Đã nằm cùng namespace root nên tự nhận

namespace ChatAppClient.UserControls
{
    public partial class ImageBubble : UserControl
    {
        private MessageType _type; // Đã nhận diện được MessageType
        private int _bubbleWidth = 0;
        public event EventHandler<byte[]>? OnForwardRequested;
        private byte[] _imageData;

        public ImageBubble()
        {
            InitializeComponent();
            pbImage.Click += (s, e) => SaveImage();

            ContextMenuStrip ctx = new ContextMenuStrip();
            ctx.Items.Add("Lưu ảnh", null, (s, e) => SaveImage());
            ctx.Items.Add("Chuyển tiếp", null, (s, e) => OnForwardRequested?.Invoke(this, _imageData));
            pbImage.ContextMenuStrip = ctx;
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
                    int maxWidth = 250;
                    int w = (img.Width > maxWidth) ? maxWidth : img.Width;
                    int h = (int)((double)img.Height / img.Width * w);
                    this.Size = new Size(w, h);
                    pbImage.Image = new Bitmap(img);
                }
            }
            catch { }

            _bubbleWidth = this.Width;
            UpdateMargins(parentUsableWidth);
        }

        public void UpdateMargins(int parentUsableWidth)
        {
            if (_bubbleWidth == 0) _bubbleWidth = this.Width;
            int remainingSpace = parentUsableWidth - _bubbleWidth;
            if (remainingSpace < 0) remainingSpace = 0;

            if (_type == MessageType.Outgoing)
                this.Margin = new Padding(remainingSpace, 5, 0, 5);
            else
                this.Margin = new Padding(0, 5, remainingSpace, 5);
        }

        private void SaveImage()
        {
            if (_imageData == null) return;
            SaveFileDialog sfd = new SaveFileDialog { Filter = "Images|*.jpg;*.png" };
            if (sfd.ShowDialog() == DialogResult.OK) File.WriteAllBytes(sfd.FileName, _imageData);
        }
    }
}