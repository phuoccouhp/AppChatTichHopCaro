using System.Drawing;
using System.Windows.Forms;

namespace ChatAppClient.UserControls
{
    partial class ChatViewControl
    {
        private System.ComponentModel.IContainer components = null;

        private Panel pnlHeader;
        private Button btnStartGame;
        private Button btnStartTank;
        private Label lblFriendName;

        private Panel pnlInput;
        private CustomControls.RoundedButton btnSend;
        private TextBox txtMessage;
        private Button btnAttach;
        private Button btnEmoji;

        private FlowLayoutPanel flpMessages;
        private FlowLayoutPanel pnlEmojiPicker;

        private ContextMenuStrip ctxAttachMenu;
        private ToolStripMenuItem btnSendImage;
        private ToolStripMenuItem btnSendFile;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            pnlHeader = new Panel();
            btnStartTank = new Button();
            btnStartGame = new Button();
            lblFriendName = new Label();

            pnlInput = new Panel();
            btnEmoji = new Button();
            btnAttach = new Button();
            ctxAttachMenu = new ContextMenuStrip(components);
            btnSendImage = new ToolStripMenuItem();
            btnSendFile = new ToolStripMenuItem();
            btnSend = new CustomControls.RoundedButton();
            txtMessage = new TextBox();

            flpMessages = new FlowLayoutPanel();
            pnlEmojiPicker = new FlowLayoutPanel();

            pnlHeader.SuspendLayout();
            pnlInput.SuspendLayout();
            ctxAttachMenu.SuspendLayout();
            SuspendLayout();

            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(0, 145, 255);
            pnlHeader.Controls.Add(btnStartTank);
            pnlHeader.Controls.Add(btnStartGame);
            pnlHeader.Controls.Add(lblFriendName);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Size = new Size(615, 55);

            // 
            // btnStartTank
            // 
            btnStartTank.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnStartTank.BackColor = Color.FromArgb(0, 145, 255);
            btnStartTank.FlatAppearance.BorderSize = 0;
            btnStartTank.FlatStyle = FlatStyle.Flat;
            btnStartTank.Font = new Font("Segoe UI Emoji", 14F);
            btnStartTank.ForeColor = Color.White;
            btnStartTank.Size = new Size(40, 40);
            btnStartTank.Location = new Point(515, 7);
            btnStartTank.Text = "🚜";
            btnStartTank.UseVisualStyleBackColor = false;

            // 
            // btnStartGame
            // 
            btnStartGame.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnStartGame.BackColor = Color.FromArgb(0, 145, 255);
            btnStartGame.FlatAppearance.BorderSize = 0;
            btnStartGame.FlatStyle = FlatStyle.Flat;
            btnStartGame.Font = new Font("Segoe UI Emoji", 14F);
            btnStartGame.ForeColor = Color.White;
            btnStartGame.Size = new Size(40, 40);
            btnStartGame.Location = new Point(460, 7);
            btnStartGame.Text = "🎲";
            btnStartGame.UseVisualStyleBackColor = false;

            // 
            // lblFriendName
            // 
            lblFriendName.AutoSize = true;
            lblFriendName.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblFriendName.ForeColor = Color.White;
            lblFriendName.Location = new Point(12, 13);
            lblFriendName.Text = "Bạn Bè A";

            // 
            // pnlInput
            // 
            pnlInput.BackColor = Color.White;
            pnlInput.Controls.Add(btnEmoji);
            pnlInput.Controls.Add(btnAttach);
            pnlInput.Controls.Add(btnSend);
            pnlInput.Controls.Add(txtMessage);
            pnlInput.Dock = DockStyle.Bottom;
            pnlInput.Padding = new Padding(10);
            pnlInput.Size = new Size(615, 60);

            // 
            // btnEmoji
            // 
            btnEmoji.BackColor = Color.Transparent;
            btnEmoji.Size = new Size(40, 40);
            btnEmoji.Location = new Point(50, 10);
            btnEmoji.FlatAppearance.BorderSize = 0;
            btnEmoji.FlatStyle = FlatStyle.Flat;
            btnEmoji.Font = new Font("Segoe UI Emoji", 12F);
            btnEmoji.Text = "😊";

            // 
            // btnAttach
            // 
            btnAttach.BackColor = Color.Transparent;
            btnAttach.ContextMenuStrip = ctxAttachMenu;
            btnAttach.Size = new Size(40, 40);
            btnAttach.Location = new Point(10, 10);
            btnAttach.FlatAppearance.BorderSize = 0;
            btnAttach.FlatStyle = FlatStyle.Flat;
            btnAttach.Font = new Font("Segoe UI Emoji", 12F);
            btnAttach.Text = "📎";

            // 
            // ctxAttachMenu
            // 
            ctxAttachMenu.Items.AddRange(new ToolStripItem[] { btnSendImage, btnSendFile });

            btnSendImage.Text = "Gửi Hình Ảnh";
            btnSendFile.Text = "Gửi File";

            // 
            // btnSend
            // 
            btnSend.BackColor = Color.Transparent;
            btnSend.BorderRadius = 20;
            btnSend.ButtonColor = Color.FromArgb(0, 145, 255);
            btnSend.Size = new Size(75, 40);
            btnSend.Dock = DockStyle.Right;
            btnSend.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSend.ForeColor = Color.White;
            btnSend.Text = "Gửi";

            // 
            // txtMessage
            // 
            txtMessage.Location = new Point(96, 14);
            txtMessage.Multiline = true;
            txtMessage.Size = new Size(358, 32);
            txtMessage.Font = new Font("Segoe UI", 10.8F);

            // 
            // flpMessages
            // 
            flpMessages.AutoScroll = true;
            flpMessages.BackColor = Color.FromArgb(229, 221, 213);
            flpMessages.Dock = DockStyle.Fill;
            flpMessages.FlowDirection = FlowDirection.TopDown;
            flpMessages.WrapContents = false;
            flpMessages.Padding = new Padding(5);

            // 
            // pnlEmojiPicker
            // 
            pnlEmojiPicker.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            pnlEmojiPicker.BackColor = Color.White;
            pnlEmojiPicker.BorderStyle = BorderStyle.FixedSingle;
            pnlEmojiPicker.Size = new Size(250, 150);
            pnlEmojiPicker.Visible = false;

            // 
            // ChatViewControl
            // 
            Controls.Add(pnlEmojiPicker);
            Controls.Add(flpMessages);
            Controls.Add(pnlInput);
            Controls.Add(pnlHeader);
            Size = new Size(615, 540);
            Load += ChatViewControl_Load;

            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlInput.ResumeLayout(false);
            pnlInput.PerformLayout();
            ctxAttachMenu.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
    }
}
