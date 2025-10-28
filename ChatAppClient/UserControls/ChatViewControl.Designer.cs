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
            this.components = new System.ComponentModel.Container();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.btnStartGame = new System.Windows.Forms.Button();
            this.lblFriendName = new System.Windows.Forms.Label();
            this.pnlInput = new System.Windows.Forms.Panel();
            this.btnEmoji = new System.Windows.Forms.Button();
            this.btnAttach = new System.Windows.Forms.Button();
            this.btnSend = new ChatAppClient.CustomControls.RoundedButton();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.flpMessages = new System.Windows.Forms.FlowLayoutPanel();
            this.ctxAttachMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.btnSendImage = new System.Windows.Forms.ToolStripMenuItem();
            this.btnSendFile = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlEmojiPicker = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlHeader.SuspendLayout();
            this.pnlInput.SuspendLayout();
            this.ctxAttachMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = ChatAppClient.Helpers.AppColors.Primary;
            this.pnlHeader.Controls.Add(this.btnStartGame);
            this.pnlHeader.Controls.Add(this.lblFriendName);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(520, 55);
            this.pnlHeader.TabIndex = 1;
            // 
            // btnStartGame
            // 
            this.btnStartGame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStartGame.BackColor = ChatAppClient.Helpers.AppColors.Primary;
            this.btnStartGame.FlatAppearance.BorderSize = 0;
            this.btnStartGame.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartGame.Font = new System.Drawing.Font("Segoe UI Emoji", 13.8F);
            this.btnStartGame.ForeColor = System.Drawing.Color.White;
            this.btnStartGame.Location = new System.Drawing.Point(468, 7);
            this.btnStartGame.Name = "btnStartGame";
            this.btnStartGame.Size = new System.Drawing.Size(40, 40);
            this.btnStartGame.TabIndex = 1;
            this.btnStartGame.Text = "🎲";
            this.btnStartGame.UseVisualStyleBackColor = false;
            // 
            // lblFriendName
            // 
            this.lblFriendName.AutoSize = true;
            this.lblFriendName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblFriendName.ForeColor = System.Drawing.Color.White;
            this.lblFriendName.Location = new System.Drawing.Point(12, 13);
            this.lblFriendName.Name = "lblFriendName";
            this.lblFriendName.Size = new System.Drawing.Size(107, 28);
            this.lblFriendName.TabIndex = 0;
            this.lblFriendName.Text = "Bạn Bè A";
            // 
            // pnlInput
            // 
            this.pnlInput.BackColor = ChatAppClient.Helpers.AppColors.FormBackground;
            this.pnlInput.Controls.Add(this.btnEmoji);
            this.pnlInput.Controls.Add(this.btnAttach);
            this.pnlInput.Controls.Add(this.btnSend);
            this.pnlInput.Controls.Add(this.txtMessage);
            this.pnlInput.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlInput.Location = new System.Drawing.Point(0, 480);
            this.pnlInput.Name = "pnlInput";
            this.pnlInput.Padding = new System.Windows.Forms.Padding(10);
            this.pnlInput.Size = new System.Drawing.Size(520, 60);
            this.pnlInput.TabIndex = 2;
            // 
            // btnEmoji
            // 
            this.btnEmoji.BackColor = System.Drawing.Color.Transparent;
            this.btnEmoji.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnEmoji.FlatAppearance.BorderSize = 0;
            this.btnEmoji.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEmoji.Font = new System.Drawing.Font("Segoe UI Emoji", 12F);
            this.btnEmoji.Location = new System.Drawing.Point(50, 10);
            this.btnEmoji.Name = "btnEmoji";
            this.btnEmoji.Size = new System.Drawing.Size(40, 40);
            this.btnEmoji.TabIndex = 3;
            this.btnEmoji.Text = "😊";
            this.btnEmoji.UseVisualStyleBackColor = false;
            // 
            // btnAttach
            // 
            this.btnAttach.BackColor = System.Drawing.Color.Transparent;
            this.btnAttach.ContextMenuStrip = this.ctxAttachMenu; // Gán context menu
            this.btnAttach.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnAttach.FlatAppearance.BorderSize = 0;
            this.btnAttach.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAttach.Font = new System.Drawing.Font("Segoe UI Emoji", 12F);
            this.btnAttach.Location = new System.Drawing.Point(10, 10);
            this.btnAttach.Name = "btnAttach";
            this.btnAttach.Size = new System.Drawing.Size(40, 40);
            this.btnAttach.TabIndex = 2;
            this.btnAttach.Text = "📎";
            this.btnAttach.UseVisualStyleBackColor = false;
            // 
            // btnSend
            // 
            this.btnSend.BorderRadius = 20;
            this.btnSend.ButtonColor = ChatAppClient.Helpers.AppColors.Primary;
            this.btnSend.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSend.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnSend.Location = new System.Drawing.Point(435, 10);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(75, 40);
            this.btnSend.TabIndex = 1;
            this.btnSend.Text = "Gửi";
            this.btnSend.TextColor = System.Drawing.Color.White;
            // 
            // txtMessage
            // 
            this.txtMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMessage.Font = new System.Drawing.Font("Segoe UI", 10.8F);
            this.txtMessage.Location = new System.Drawing.Point(96, 14);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(333, 32);
            this.txtMessage.TabIndex = 0;
            // 
            // flpMessages
            // 
            this.flpMessages.AutoScroll = true;
            this.flpMessages.BackColor = ChatAppClient.Helpers.AppColors.Background;
            this.flpMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpMessages.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpMessages.Location = new System.Drawing.Point(0, 55);
            this.flpMessages.Name = "flpMessages";
            this.flpMessages.Padding = new System.Windows.Forms.Padding(5);
            this.flpMessages.Size = new System.Drawing.Size(520, 425);
            this.flpMessages.TabIndex = 3;
            this.flpMessages.WrapContents = false;
            // 
            // ctxAttachMenu
            // 
            this.ctxAttachMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ctxAttachMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
    this.btnSendImage,
    this.btnSendFile});
            this.ctxAttachMenu.Name = "ctxAttachMenu";
            this.ctxAttachMenu.Size = new System.Drawing.Size(167, 52);
            // 
            // btnSendImage
            // 
            this.btnSendImage.Name = "btnSendImage";
            this.btnSendImage.Size = new System.Drawing.Size(166, 24);
            this.btnSendImage.Text = "Gửi Hình Ảnh";
            // 
            // btnSendFile
            // 
            this.btnSendFile.Name = "btnSendFile";
            this.btnSendFile.Size = new System.Drawing.Size(166, 24);
            this.btnSendFile.Text = "Gửi File";
            // 
            // pnlEmojiPicker
            // 
            this.pnlEmojiPicker.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pnlEmojiPicker.BackColor = System.Drawing.Color.White;
            this.pnlEmojiPicker.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlEmojiPicker.Location = new System.Drawing.Point(96, 324);
            this.pnlEmojiPicker.Name = "pnlEmojiPicker";
            this.pnlEmojiPicker.Size = new System.Drawing.Size(250, 150);
            this.pnlEmojiPicker.TabIndex = 4;
            this.pnlEmojiPicker.Visible = false; // Ẩn ban đầu
                                                 // 
                                                 // ChatViewControl
                                                 // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.pnlEmojiPicker);
            this.Controls.Add(this.flpMessages);
            this.Controls.Add(this.pnlInput);
            this.Controls.Add(this.pnlHeader);
            this.Name = "ChatViewControl";
            this.Size = new System.Drawing.Size(520, 540);
            this.Load += new System.EventHandler(this.ChatViewControl_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlInput.ResumeLayout(false);
            this.pnlInput.PerformLayout();
            this.ctxAttachMenu.ResumeLayout(false);
            this.ResumeLayout(false);

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