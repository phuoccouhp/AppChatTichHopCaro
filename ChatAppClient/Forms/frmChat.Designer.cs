namespace ChatAppClient.Forms
{
    partial class frmChat
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pnlHeader = new Panel();
            btnStartGame = new Button();
            lblFriendName = new Label();
            pnlInput = new Panel();
            btnSend = new CustomControls.RoundedButton();
            txtMessage = new TextBox();
            flpMessages = new FlowLayoutPanel();
            pnlHeader.SuspendLayout();
            pnlInput.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(0, 145, 255);
            pnlHeader.Controls.Add(btnStartGame);
            pnlHeader.Controls.Add(lblFriendName);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Margin = new Padding(3, 4, 3, 4);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(432, 69);
            pnlHeader.TabIndex = 0;
            // 
            // btnStartGame
            // 
            btnStartGame.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnStartGame.BackColor = Color.FromArgb(0, 145, 255);
            btnStartGame.FlatAppearance.BorderSize = 0;
            btnStartGame.FlatStyle = FlatStyle.Flat;
            btnStartGame.Font = new Font("Segoe UI Emoji", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnStartGame.ForeColor = Color.White;
            btnStartGame.Location = new Point(380, 9);
            btnStartGame.Margin = new Padding(3, 4, 3, 4);
            btnStartGame.Name = "btnStartGame";
            btnStartGame.Size = new Size(40, 50);
            btnStartGame.TabIndex = 1;
            btnStartGame.Text = "🎲";
            btnStartGame.UseVisualStyleBackColor = false;
            // 
            // lblFriendName
            // 
            lblFriendName.AutoSize = true;
            lblFriendName.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblFriendName.ForeColor = Color.White;
            lblFriendName.Location = new Point(12, 16);
            lblFriendName.Name = "lblFriendName";
            lblFriendName.Size = new Size(98, 28);
            lblFriendName.TabIndex = 0;
            lblFriendName.Text = "Bạn Bè A";
            // 
            // pnlInput
            // 
            pnlInput.BackColor = Color.White;
            pnlInput.Controls.Add(btnSend);
            pnlInput.Controls.Add(txtMessage);
            pnlInput.Dock = DockStyle.Bottom;
            pnlInput.Location = new Point(0, 810);
            pnlInput.Margin = new Padding(3, 4, 3, 4);
            pnlInput.Name = "pnlInput";
            pnlInput.Padding = new Padding(10, 12, 10, 12);
            pnlInput.Size = new Size(432, 75);
            pnlInput.TabIndex = 1;
            // 
            // btnSend
            // 
            btnSend.BackColor = Color.Transparent;
            btnSend.BorderRadius = 20;
            btnSend.ButtonColor = Color.FromArgb(0, 145, 255);
            btnSend.Dock = DockStyle.Right;
            btnSend.FlatStyle = FlatStyle.Flat;
            btnSend.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSend.ForeColor = Color.Transparent;
            btnSend.Location = new Point(347, 12);
            btnSend.Margin = new Padding(3, 4, 3, 4);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(75, 51);
            btnSend.TabIndex = 1;
            btnSend.Text = "Gửi";
            btnSend.TextColor = Color.White;
            btnSend.UseVisualStyleBackColor = false;
            btnSend.Click += BtnSend_Click;
            // 
            // txtMessage
            // 
            txtMessage.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtMessage.Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtMessage.Location = new Point(10, 18);
            txtMessage.Margin = new Padding(3, 4, 3, 4);
            txtMessage.Multiline = true;
            txtMessage.Name = "txtMessage";
            txtMessage.Size = new Size(331, 39);
            txtMessage.TabIndex = 0;
            // 
            // flpMessages
            // 
            flpMessages.AutoScroll = true;
            flpMessages.BackColor = Color.FromArgb(229, 221, 213);
            flpMessages.Dock = DockStyle.Fill;
            flpMessages.FlowDirection = FlowDirection.TopDown;
            flpMessages.Location = new Point(0, 69);
            flpMessages.Margin = new Padding(3, 4, 3, 4);
            flpMessages.Name = "flpMessages";
            flpMessages.Padding = new Padding(5, 6, 5, 6);
            flpMessages.Size = new Size(432, 741);
            flpMessages.TabIndex = 2;
            flpMessages.WrapContents = false;
            // 
            // frmChat
            // 
            AcceptButton = btnSend;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(432, 885);
            Controls.Add(flpMessages);
            Controls.Add(pnlInput);
            Controls.Add(pnlHeader);
            Margin = new Padding(3, 4, 3, 4);
            Name = "frmChat";
            Text = "Chat Đồ Án";
            Load += frmChat_Load;
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlInput.ResumeLayout(false);
            pnlInput.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblFriendName;
        private System.Windows.Forms.Panel pnlInput;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.FlowLayoutPanel flpMessages;
        private System.Windows.Forms.Button btnStartGame;
        private CustomControls.RoundedButton btnSend;
    }
}