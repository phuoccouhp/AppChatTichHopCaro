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
            pnlHeader.Controls.Add(btnStartGame);
            pnlHeader.Controls.Add(lblFriendName);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(520, 65);
            pnlHeader.TabIndex = 1;
            pnlHeader.Paint += PnlHeader_Paint;
            // 
            // btnStartGame
            // 
            btnStartGame.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnStartGame.BackColor = Color.Transparent;
            btnStartGame.FlatAppearance.BorderSize = 0;
            btnStartGame.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 120, 220);
            btnStartGame.FlatStyle = FlatStyle.Flat;
            btnStartGame.Font = new Font("Segoe UI Emoji", 14F);
            btnStartGame.ForeColor = Color.White;
            btnStartGame.Location = new Point(465, 12);
            btnStartGame.Name = "btnStartGame";
            btnStartGame.Size = new Size(45, 45);
            btnStartGame.TabIndex = 1;
            btnStartGame.Text = "🎲";
            btnStartGame.UseVisualStyleBackColor = false;
            btnStartGame.Cursor = Cursors.Hand;
            // 
            // lblFriendName
            // 
            lblFriendName.AutoSize = true;
            lblFriendName.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblFriendName.ForeColor = Color.White;
            lblFriendName.Location = new Point(15, 20);
            lblFriendName.Name = "lblFriendName";
            lblFriendName.Size = new Size(98, 30);
            lblFriendName.TabIndex = 0;
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
            pnlInput.Location = new Point(0, 480);
            pnlInput.Name = "pnlInput";
            pnlInput.Padding = new Padding(12, 12, 12, 12);
            pnlInput.Size = new Size(520, 70);
            pnlInput.TabIndex = 2;
            pnlInput.Paint += PnlInput_Paint;
            // 
            // btnEmoji
            // 
            btnEmoji.BackColor = Color.Transparent;
            btnEmoji.FlatAppearance.BorderSize = 0;
            btnEmoji.FlatAppearance.MouseOverBackColor = Color.FromArgb(245, 245, 245);
            btnEmoji.FlatStyle = FlatStyle.Flat;
            btnEmoji.Font = new Font("Segoe UI Emoji", 14F);
            btnEmoji.Location = new Point(55, 18);
            btnEmoji.Name = "btnEmoji";
            btnEmoji.Size = new Size(42, 42);
            btnEmoji.TabIndex = 3;
            btnEmoji.Text = "😊";
            btnEmoji.UseVisualStyleBackColor = false;
            btnEmoji.Cursor = Cursors.Hand;
            // 
            // btnAttach
            // 
            btnAttach.BackColor = Color.Transparent;
            btnAttach.ContextMenuStrip = ctxAttachMenu;
            btnAttach.FlatAppearance.BorderSize = 0;
            btnAttach.FlatAppearance.MouseOverBackColor = Color.FromArgb(245, 245, 245);
            btnAttach.FlatStyle = FlatStyle.Flat;
            btnAttach.Font = new Font("Segoe UI Emoji", 14F);
            btnAttach.Location = new Point(12, 18);
            btnAttach.Name = "btnAttach";
            btnAttach.Size = new Size(42, 42);
            btnAttach.TabIndex = 2;
            btnAttach.Text = "📎";
            btnAttach.UseVisualStyleBackColor = false;
            btnAttach.Cursor = Cursors.Hand;
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
            btnSend.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSend.BackColor = Color.Transparent;
            btnSend.BorderRadius = 22;
            btnSend.ButtonColor = Color.FromArgb(0, 145, 255);
            btnSend.FlatStyle = FlatStyle.Flat;
            btnSend.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnSend.ForeColor = Color.Transparent;
            btnSend.Location = new Point(430, 18);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(78, 42);
            btnSend.TabIndex = 1;
            btnSend.Text = "Gửi";
            btnSend.TextColor = Color.White;
            btnSend.UseVisualStyleBackColor = false;
            btnSend.Cursor = Cursors.Hand;
            // 
            // txtMessage
            // 
            txtMessage.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtMessage.BorderStyle = BorderStyle.None;
            txtMessage.Font = new Font("Segoe UI", 11F);
            txtMessage.Location = new Point(107, 20);
            txtMessage.Multiline = true;
            txtMessage.Name = "txtMessage";
            txtMessage.Size = new Size(315, 38);
            txtMessage.TabIndex = 0;
            txtMessage.BackColor = Color.FromArgb(245, 247, 250);
            // 
            // flpMessages
            // 
            flpMessages.AutoScroll = true;
            flpMessages.BackColor = Color.FromArgb(240, 242, 245);
            flpMessages.Dock = DockStyle.Fill;
            flpMessages.FlowDirection = FlowDirection.TopDown;
            flpMessages.Location = new Point(0, 65);
            flpMessages.Name = "flpMessages";
            flpMessages.Padding = new Padding(8, 10, 8, 10);
            flpMessages.Size = new Size(520, 415);
            flpMessages.TabIndex = 3;
            flpMessages.WrapContents = false;
            // 
            // pnlEmojiPicker
            // 
            pnlEmojiPicker.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            pnlEmojiPicker.BackColor = Color.White;
            pnlEmojiPicker.BorderStyle = BorderStyle.None;
            pnlEmojiPicker.Location = new Point(102, 330);
            pnlEmojiPicker.Name = "pnlEmojiPicker";
            pnlEmojiPicker.Padding = new Padding(8);
            pnlEmojiPicker.Size = new Size(260, 160);
            pnlEmojiPicker.TabIndex = 4;
            pnlEmojiPicker.Visible = false;
            pnlEmojiPicker.Paint += PnlEmojiPicker_Paint;
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
        // Thêm các control mới vào
        private System.Windows.Forms.ContextMenuStrip ctxAttachMenu;
        private System.Windows.Forms.ToolStripMenuItem btnSendImage;
        private System.Windows.Forms.ToolStripMenuItem btnSendFile;
        private System.Windows.Forms.Button btnEmoji;
        private System.Windows.Forms.FlowLayoutPanel pnlEmojiPicker;
        //... (giữ các control cũ)

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Button btnStartGame;
        private System.Windows.Forms.Label lblFriendName;
        private System.Windows.Forms.Panel pnlInput;
        private CustomControls.RoundedButton btnSend;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.FlowLayoutPanel flpMessages;
        private System.Windows.Forms.Button btnAttach;
    }
}