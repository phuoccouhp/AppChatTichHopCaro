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

        public ImageBubble()
        {
            InitializeComponent();
        }

        public void SetMessage(byte[] imageData, MessageType type, int parentUsableWidth)
        {
            _type = type;

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