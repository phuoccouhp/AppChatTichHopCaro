namespace ChatAppClient.UserControls
{
    partial class FriendListItem
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
            this.pbAvatar = new System.Windows.Forms.PictureBox();
            this.lblFriendName = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.pnlStatusDot = new System.Windows.Forms.Panel();
            this.lblNewMessageBadge = new System.Windows.Forms.Label(); // Khai báo dòng này trước
            ((System.ComponentModel.ISupportInitialize)(this.pbAvatar)).BeginInit();
            this.SuspendLayout();
            // 
            // pbAvatar
            // 
            try
            {
                // Tải ảnh từ thư mục "Images" (đảm bảo file tồn tại và đã set "Copy if newer")
                this.pbAvatar.Image = System.Drawing.Image.FromFile("Images/default_avatar.png");
            }
            catch (System.IO.FileNotFoundException)
            {
                // Nếu không tìm thấy file, hiển thị ảnh lỗi (hoặc để trống)
                this.pbAvatar.Image = this.pbAvatar.ErrorImage;
            }
            this.pbAvatar.Location = new System.Drawing.Point(10, 10);
            this.pbAvatar.Name = "pbAvatar";
            this.pbAvatar.Size = new System.Drawing.Size(50, 50);
            this.pbAvatar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbAvatar.TabIndex = 0;
            this.pbAvatar.TabStop = false;
            // 
            // lblFriendName
            // 
            this.lblFriendName.AutoSize = true;
            this.lblFriendName.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFriendName.Location = new System.Drawing.Point(70, 10);
            this.lblFriendName.Name = "lblFriendName";
            this.lblFriendName.Size = new System.Drawing.Size(123, 25);
            this.lblFriendName.TabIndex = 1;
            this.lblFriendName.Text = "Friend Name";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblStatus.Location = new System.Drawing.Point(88, 38);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(54, 20);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "Offline";
            // 
            // pnlStatusDot
            // 
            this.pnlStatusDot.BackColor = System.Drawing.Color.Gray;
            this.pnlStatusDot.Location = new System.Drawing.Point(73, 43);
            this.pnlStatusDot.Name = "pnlStatusDot";
            this.pnlStatusDot.Size = new System.Drawing.Size(10, 10);
            this.pnlStatusDot.TabIndex = 3;
            // 
            // lblNewMessageBadge
            // 
            this.lblNewMessageBadge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNewMessageBadge.BackColor = System.Drawing.Color.Red;
            this.lblNewMessageBadge.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNewMessageBadge.ForeColor = System.Drawing.Color.White;
            this.lblNewMessageBadge.Location = new System.Drawing.Point(240, 22);
            this.lblNewMessageBadge.Name = "lblNewMessageBadge";
            this.lblNewMessageBadge.Size = new System.Drawing.Size(25, 25);
            this.lblNewMessageBadge.TabIndex = 4;
            this.lblNewMessageBadge.Text = "N";
            this.lblNewMessageBadge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblNewMessageBadge.Visible = false;
            // 
            // FriendListItem
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.lblNewMessageBadge); // Thêm chấm đỏ
            this.Controls.Add(this.pnlStatusDot);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblFriendName);
            this.Controls.Add(this.pbAvatar);
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "FriendListItem";
            this.Size = new System.Drawing.Size(280, 70);
            ((System.ComponentModel.ISupportInitialize)(this.pbAvatar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pbAvatar;
        private System.Windows.Forms.Label lblFriendName;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Panel pnlStatusDot;
        private System.Windows.Forms.Label lblNewMessageBadge; // Khai báo biến
    }
}