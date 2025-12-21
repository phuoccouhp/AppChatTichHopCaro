namespace ChatAppClient.UserControls
{
    partial class ChatViewControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            pnlHeader = new Panel();
            btnStartGame = new Button();
            lblFriendName = new Label();
            pnlSeparator = new Panel();
            pnlInput = new Panel();
            btnEmoji = new Button();
            btnAttach = new Button();
            ctxAttachMenu = new ContextMenuStrip(components);
            btnSendImage = new ToolStripMenuItem();
            btnSendFile = new ToolStripMenuItem();
            btnSend = new ChatAppClient.CustomControls.RoundedButton();
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
            pnlHeader.BackColor = Color.FromArgb(50, 49, 54);
            pnlHeader.Controls.Add(btnStartGame);
            pnlHeader.Controls.Add(lblFriendName);
            pnlHeader.Controls.Add(pnlSeparator);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(520, 60);
            pnlHeader.TabIndex = 1;
            // 
            // btnStartGame
            // 
            btnStartGame.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnStartGame.BackColor = Color.Transparent;
            btnStartGame.FlatAppearance.BorderSize = 0;
            btnStartGame.FlatAppearance.MouseDownBackColor = Color.FromArgb(40, 42, 48);
            btnStartGame.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 63, 75);
            btnStartGame.FlatStyle = FlatStyle.Flat;
            btnStartGame.Font = new Font("Segoe UI Emoji", 16F);
            btnStartGame.ForeColor = Color.White;
            btnStartGame.Location = new Point(468, 8);
            btnStartGame.Name = "btnStartGame";
            btnStartGame.Size = new Size(45, 45);
            btnStartGame.TabIndex = 1;
            btnStartGame.Text = "🎲";
            btnStartGame.UseVisualStyleBackColor = false;
            // 
            // lblFriendName
            // 
            lblFriendName.AutoSize = true;
            lblFriendName.Font = new Font("Montserrat", 13F, FontStyle.Bold);
            lblFriendName.ForeColor = Color.White;
            lblFriendName.Location = new Point(15, 15);
            lblFriendName.Name = "lblFriendName";
            lblFriendName.Size = new Size(121, 34);
            lblFriendName.TabIndex = 0;
            lblFriendName.Text = "Bạn Bè A";
            // 
            // pnlSeparator
            // 
            pnlSeparator.BackColor = Color.FromArgb(32, 34, 37);
            pnlSeparator.Dock = DockStyle.Bottom;
            pnlSeparator.Location = new Point(0, 59);
            pnlSeparator.Name = "pnlSeparator";
            pnlSeparator.Size = new Size(520, 1);
            pnlSeparator.TabIndex = 10;
            // 
            // pnlInput
            // 
            pnlInput.BackColor = Color.FromArgb(40, 43, 55);
            pnlInput.Controls.Add(btnEmoji);
            pnlInput.Controls.Add(btnAttach);
            pnlInput.Controls.Add(btnSend);
            pnlInput.Controls.Add(txtMessage);
            pnlInput.Dock = DockStyle.Bottom;
            pnlInput.Location = new Point(0, 470);
            pnlInput.Name = "pnlInput";
            pnlInput.Padding = new Padding(10);
            pnlInput.Size = new Size(520, 70);
            pnlInput.TabIndex = 2;
            // 
            // btnEmoji
            // 
            btnEmoji.BackColor = Color.Transparent;
            btnEmoji.Dock = DockStyle.Left;
            btnEmoji.FlatAppearance.BorderSize = 0;
            btnEmoji.FlatAppearance.MouseDownBackColor = Color.FromArgb(40, 42, 48);
            btnEmoji.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 63, 75);
            btnEmoji.FlatStyle = FlatStyle.Flat;
            btnEmoji.Font = new Font("Segoe UI Emoji", 14F);
            btnEmoji.ForeColor = Color.White;
            btnEmoji.Location = new Point(50, 10);
            btnEmoji.Name = "btnEmoji";
            btnEmoji.Size = new Size(40, 50);
            btnEmoji.TabIndex = 3;
            btnEmoji.Text = "😊";
            btnEmoji.UseVisualStyleBackColor = false;
            // 
            // btnAttach
            // 
            btnAttach.BackColor = Color.Transparent;
            btnAttach.ContextMenuStrip = ctxAttachMenu;
            btnAttach.Dock = DockStyle.Left;
            btnAttach.FlatAppearance.BorderSize = 0;
            btnAttach.FlatAppearance.MouseDownBackColor = Color.FromArgb(40, 42, 48);
            btnAttach.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 63, 75);
            btnAttach.FlatStyle = FlatStyle.Flat;
            btnAttach.Font = new Font("Segoe UI Emoji", 14F);
            btnAttach.ForeColor = Color.White;
            btnAttach.Location = new Point(10, 10);
            btnAttach.Name = "btnAttach";
            btnAttach.Size = new Size(40, 50);
            btnAttach.TabIndex = 2;
            btnAttach.Text = "📎";
            btnAttach.UseVisualStyleBackColor = false;
            // 
            // ctxAttachMenu
            // 
            ctxAttachMenu.ImageScalingSize = new Size(20, 20);
            ctxAttachMenu.Items.AddRange(new ToolStripItem[] { btnSendImage, btnSendFile });
            ctxAttachMenu.Name = "ctxAttachMenu";
            ctxAttachMenu.Size = new Size(167, 52);
            // 
            // btnSendImage
            // 
            btnSendImage.Name = "btnSendImage";
            btnSendImage.Size = new Size(166, 24);
            btnSendImage.Text = "Gửi Hình Ảnh";
            // 
            // btnSendFile
            // 
            btnSendFile.Name = "btnSendFile";
            btnSendFile.Size = new Size(166, 24);
            btnSendFile.Text = "Gửi File";
            // 
            // btnSend
            // 
            btnSend.Anchor = AnchorStyles.Right;
            btnSend.BackColor = Color.FromArgb(47, 49, 54);
            btnSend.BorderRadius = 20;
            btnSend.ButtonColor = Color.FromArgb(88, 101, 242);
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.FlatStyle = FlatStyle.Flat;
            btnSend.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSend.ForeColor = Color.White;
            btnSend.Location = new Point(435, 10);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(82, 50);
            btnSend.TabIndex = 1;
            btnSend.Text = "Gửi";
            btnSend.TextColor = Color.White;
            btnSend.UseVisualStyleBackColor = false;
            // 
            // txtMessage
            // 
            txtMessage.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtMessage.BackColor = Color.FromArgb(64, 68, 75);
            txtMessage.BorderStyle = BorderStyle.None;
            txtMessage.Font = new Font("Microsoft Sans Serif", 14F);
            txtMessage.ForeColor = Color.White;
            txtMessage.Location = new Point(96, 22);
            txtMessage.Multiline = true;
            txtMessage.Name = "txtMessage";
            txtMessage.Size = new Size(333, 30);
            txtMessage.TabIndex = 0;
            // 
            // flpMessages
            // 
            flpMessages.AutoScroll = true;
            flpMessages.BackColor = Color.FromArgb(54, 57, 63);
            flpMessages.Dock = DockStyle.Fill;
            flpMessages.FlowDirection = FlowDirection.TopDown;
            flpMessages.Location = new Point(0, 60);
            flpMessages.Name = "flpMessages";
            flpMessages.Padding = new Padding(10);
            flpMessages.Size = new Size(520, 410);
            flpMessages.TabIndex = 3;
            flpMessages.WrapContents = false;
            // 
            // pnlEmojiPicker
            // 
            pnlEmojiPicker.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            pnlEmojiPicker.BackColor = Color.FromArgb(47, 49, 54);
            pnlEmojiPicker.BorderStyle = BorderStyle.FixedSingle;
            pnlEmojiPicker.Location = new Point(96, 314);
            pnlEmojiPicker.Name = "pnlEmojiPicker";
            pnlEmojiPicker.Size = new Size(250, 150);
            pnlEmojiPicker.TabIndex = 4;
            pnlEmojiPicker.Visible = false;
            // 
            // ChatViewControl
            // 
            AutoScaleMode = AutoScaleMode.None;
            Controls.Add(pnlEmojiPicker);
            Controls.Add(flpMessages);
            Controls.Add(pnlInput);
            Controls.Add(pnlHeader);
            Name = "ChatViewControl";
            Size = new Size(520, 540);
            Load += ChatViewControl_Load;
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlInput.ResumeLayout(false);
            pnlInput.PerformLayout();
            ctxAttachMenu.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Button btnStartGame;
        private System.Windows.Forms.Label lblFriendName;
        private System.Windows.Forms.Panel pnlInput;
        private System.Windows.Forms.Button btnEmoji;
        private System.Windows.Forms.Button btnAttach;
        private System.Windows.Forms.ContextMenuStrip ctxAttachMenu;
        private System.Windows.Forms.ToolStripMenuItem btnSendImage;
        private System.Windows.Forms.ToolStripMenuItem btnSendFile;
        private ChatAppClient.CustomControls.RoundedButton btnSend;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.FlowLayoutPanel flpMessages;
        private System.Windows.Forms.FlowLayoutPanel pnlEmojiPicker;
        private System.Windows.Forms.Panel pnlSeparator;
    }
}