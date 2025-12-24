namespace ChatAppClient.UserControls
{
    partial class ChatViewControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblFriendName = new System.Windows.Forms.Label();
            this.btnStartGame = new System.Windows.Forms.Button();
            this.flpMessages = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlInput = new System.Windows.Forms.Panel();
            this.btnSend = new System.Windows.Forms.Button();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.btnAttach = new System.Windows.Forms.Button();
            this.btnEmoji = new System.Windows.Forms.Button();
            this.pnlEmojiPicker = new System.Windows.Forms.FlowLayoutPanel();
            this.ctxAttachMenu = new System.Windows.Forms.ContextMenuStrip();
            this.btnSendImage = new System.Windows.Forms.ToolStripMenuItem();
            this.btnSendFile = new System.Windows.Forms.ToolStripMenuItem();

            this.pnlHeader.SuspendLayout();
            this.pnlInput.SuspendLayout();
            this.ctxAttachMenu.SuspendLayout();
            this.SuspendLayout();

            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.pnlHeader.Controls.Add(this.btnStartGame);
            this.pnlHeader.Controls.Add(this.lblFriendName);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(600, 50);
            this.pnlHeader.TabIndex = 0;
            this.pnlHeader.Paint += new System.Windows.Forms.PaintEventHandler(this.PnlHeader_Paint);

            // 
            // lblFriendName
            // 
            this.lblFriendName.AutoSize = true;
            this.lblFriendName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFriendName.ForeColor = System.Drawing.Color.White;
            this.lblFriendName.Location = new System.Drawing.Point(15, 13);
            this.lblFriendName.Name = "lblFriendName";
            this.lblFriendName.Size = new System.Drawing.Size(106, 21);
            this.lblFriendName.TabIndex = 0;
            this.lblFriendName.Text = "Friend Name";

            // 
            // btnStartGame
            // 
            this.btnStartGame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStartGame.BackColor = System.Drawing.Color.Transparent;
            this.btnStartGame.FlatAppearance.BorderSize = 0;
            this.btnStartGame.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartGame.Font = new System.Drawing.Font("Segoe UI Emoji", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStartGame.ForeColor = System.Drawing.Color.White;
            this.btnStartGame.Location = new System.Drawing.Point(550, 5);
            this.btnStartGame.Name = "btnStartGame";
            this.btnStartGame.Size = new System.Drawing.Size(40, 40);
            this.btnStartGame.TabIndex = 1;
            this.btnStartGame.Text = "🎲";
            this.btnStartGame.UseVisualStyleBackColor = false;
            this.btnStartGame.Cursor = System.Windows.Forms.Cursors.Hand;

            // 
            // flpMessages
            // 
            this.flpMessages.AutoScroll = true;
            this.flpMessages.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(237)))), ((int)(((byte)(241)))));
            this.flpMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpMessages.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpMessages.Location = new System.Drawing.Point(0, 50);
            this.flpMessages.Name = "flpMessages";
            this.flpMessages.Padding = new System.Windows.Forms.Padding(10);
            this.flpMessages.Size = new System.Drawing.Size(600, 400);
            this.flpMessages.TabIndex = 1;
            this.flpMessages.WrapContents = false;

            // 
            // pnlInput
            // 
            this.pnlInput.BackColor = System.Drawing.Color.White;
            this.pnlInput.Controls.Add(this.btnAttach);
            this.pnlInput.Controls.Add(this.btnEmoji);
            this.pnlInput.Controls.Add(this.btnSend);
            this.pnlInput.Controls.Add(this.txtMessage);
            this.pnlInput.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlInput.Location = new System.Drawing.Point(0, 450);
            this.pnlInput.Name = "pnlInput";
            this.pnlInput.Size = new System.Drawing.Size(600, 50);
            this.pnlInput.TabIndex = 2;
            this.pnlInput.Paint += new System.Windows.Forms.PaintEventHandler(this.PnlInput_Paint);

            // 
            // txtMessage
            // 
            this.txtMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMessage.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtMessage.Location = new System.Drawing.Point(90, 15);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(440, 20);
            this.txtMessage.TabIndex = 0;

            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(132)))), ((int)(((byte)(255)))));
            this.btnSend.FlatAppearance.BorderSize = 0;
            this.btnSend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSend.ForeColor = System.Drawing.Color.White;
            this.btnSend.Location = new System.Drawing.Point(540, 8);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(50, 34);
            this.btnSend.TabIndex = 1;
            this.btnSend.Text = "Gửi";
            this.btnSend.UseVisualStyleBackColor = false;
            this.btnSend.Cursor = System.Windows.Forms.Cursors.Hand;

            // 
            // btnAttach
            // 
            this.btnAttach.BackColor = System.Drawing.Color.Transparent;
            this.btnAttach.FlatAppearance.BorderSize = 0;
            this.btnAttach.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAttach.Font = new System.Drawing.Font("Segoe UI Emoji", 12F);
            this.btnAttach.Location = new System.Drawing.Point(5, 8);
            this.btnAttach.Name = "btnAttach";
            this.btnAttach.Size = new System.Drawing.Size(35, 35);
            this.btnAttach.TabIndex = 2;
            this.btnAttach.Text = "📎";
            this.btnAttach.UseVisualStyleBackColor = false;
            this.btnAttach.Cursor = System.Windows.Forms.Cursors.Hand;

            // 
            // btnEmoji
            // 
            this.btnEmoji.BackColor = System.Drawing.Color.Transparent;
            this.btnEmoji.FlatAppearance.BorderSize = 0;
            this.btnEmoji.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEmoji.Font = new System.Drawing.Font("Segoe UI Emoji", 12F);
            this.btnEmoji.Location = new System.Drawing.Point(45, 8);
            this.btnEmoji.Name = "btnEmoji";
            this.btnEmoji.Size = new System.Drawing.Size(35, 35);
            this.btnEmoji.TabIndex = 3;
            this.btnEmoji.Text = "😊";
            this.btnEmoji.UseVisualStyleBackColor = false;
            this.btnEmoji.Cursor = System.Windows.Forms.Cursors.Hand;

            // 
            // pnlEmojiPicker
            // 
            this.pnlEmojiPicker.BackColor = System.Drawing.Color.White;
            this.pnlEmojiPicker.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlEmojiPicker.Location = new System.Drawing.Point(45, 250);
            this.pnlEmojiPicker.Name = "pnlEmojiPicker";
            this.pnlEmojiPicker.Size = new System.Drawing.Size(200, 200);
            this.pnlEmojiPicker.TabIndex = 3;
            this.pnlEmojiPicker.Visible = false;
            this.pnlEmojiPicker.Paint += new System.Windows.Forms.PaintEventHandler(this.PnlEmojiPicker_Paint);

            // 
            // ctxAttachMenu
            // 
            this.ctxAttachMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnSendImage,
            this.btnSendFile});
            this.ctxAttachMenu.Name = "ctxAttachMenu";
            this.ctxAttachMenu.Size = new System.Drawing.Size(181, 48);

            // 
            // btnSendImage
            // 
            this.btnSendImage.Name = "btnSendImage";
            this.btnSendImage.Size = new System.Drawing.Size(180, 22);
            this.btnSendImage.Text = "Gửi Ảnh";
            // 
            // btnSendFile
            // 
            this.btnSendFile.Name = "btnSendFile";
            this.btnSendFile.Size = new System.Drawing.Size(180, 22);
            this.btnSendFile.Text = "Gửi File";

            // 
            // ChatViewControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlEmojiPicker);
            this.Controls.Add(this.flpMessages);
            this.Controls.Add(this.pnlInput);
            this.Controls.Add(this.pnlHeader);
            this.Name = "ChatViewControl";
            this.Size = new System.Drawing.Size(600, 500);
            this.Load += new System.EventHandler(this.ChatViewControl_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlInput.ResumeLayout(false);
            this.pnlInput.PerformLayout();
            this.ctxAttachMenu.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblFriendName;
        private System.Windows.Forms.Button btnStartGame;
        private System.Windows.Forms.FlowLayoutPanel flpMessages;
        private System.Windows.Forms.Panel pnlInput;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnAttach;
        private System.Windows.Forms.Button btnEmoji;
        private System.Windows.Forms.FlowLayoutPanel pnlEmojiPicker;
        private System.Windows.Forms.ContextMenuStrip ctxAttachMenu;
        private System.Windows.Forms.ToolStripMenuItem btnSendImage;
        private System.Windows.Forms.ToolStripMenuItem btnSendFile;
    }
}