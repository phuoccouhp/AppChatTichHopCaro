using ChatAppClient.Helpers;
using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

namespace ChatAppClient.UserControls
{
    public partial class FileBubble : UserControl
    {
        private MessageType _type;
        private int _bubbleWidth = 0;
        private byte[] _fileData;
        private string _fileName;
        public event EventHandler<(string fileName, byte[] fileData)>? OnForwardRequested;

        public FileBubble()
        {
            InitializeComponent();
            btnDownload.Click += BtnDownload_Click;
            btnForward.Click += BtnForward_Click;

            this.MouseEnter += (s, e) => ToggleBtns(true);
            this.MouseLeave += (s, e) => CheckMouseLeave();
            pnlContainer.MouseEnter += (s, e) => ToggleBtns(true);
            pnlContainer.MouseLeave += (s, e) => CheckMouseLeave();
        }

        private void ToggleBtns(bool show) { btnDownload.Visible = show; btnForward.Visible = show; if (show) { btnDownload.BringToFront(); btnForward.BringToFront(); } }
        private void CheckMouseLeave() { if (!this.ClientRectangle.Contains(this.PointToClient(Control.MousePosition))) ToggleBtns(false); }

        private void BtnForward_Click(object? sender, EventArgs e) { if (_fileData != null) OnForwardRequested?.Invoke(this, (_fileName, _fileData)); }

        public void SetMessage(string fileName, byte[] fileData, MessageType type, int parentUsableWidth)
        {
            _type = type;
            _fileName = fileName;
            _fileData = fileData;
            lblFileName.Text = fileName;

            if (type == MessageType.Outgoing)
            {
                pnlContainer.BackColor = AppColors.Primary;
                lblFileName.ForeColor = Color.White;
            }
            else
            {
                pnlContainer.BackColor = Color.LightGray;
                lblFileName.ForeColor = Color.Black;
            }

            _bubbleWidth = pnlContainer.Width;
            UpdateMargins(parentUsableWidth);
        }

        private void BtnDownload_Click(object sender, EventArgs e)
        {
            if (_fileData == null) return;
            SaveFileDialog sfd = new SaveFileDialog { FileName = _fileName, Filter = "All Files|*.*" };
            if (sfd.ShowDialog() == DialogResult.OK) try { File.WriteAllBytes(sfd.FileName, _fileData); MessageBox.Show("Xong!"); } catch { }
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
    }
}