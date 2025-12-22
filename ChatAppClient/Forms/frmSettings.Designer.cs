namespace ChatAppClient.Forms
{
    partial class frmSettings
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblUserID = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDisplayName = new System.Windows.Forms.TextBox();
            this.pbAvatar = new System.Windows.Forms.PictureBox();
            this.btnChangeAvatar = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbAvatar)).BeginInit();
            this.SuspendLayout();
            // 
            // lblUserID
            // 
            this.lblUserID.AutoSize = true;
            this.lblUserID.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblUserID.Location = new System.Drawing.Point(30, 20);
            this.lblUserID.Name = "lblUserID";
            this.lblUserID.Size = new System.Drawing.Size(120, 23);
            this.lblUserID.TabIndex = 0;
            this.lblUserID.Text = "Username: ...";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.label2.Location = new System.Drawing.Point(30, 220);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 23);
            this.label2.TabIndex = 1;
            this.label2.Text = "Tên hiển thị:";
            // 
            // txtDisplayName
            // 
            this.txtDisplayName.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtDisplayName.Location = new System.Drawing.Point(33, 240);
            this.txtDisplayName.Name = "txtDisplayName";
            this.txtDisplayName.Size = new System.Drawing.Size(250, 30);
            this.txtDisplayName.TabIndex = 2;
            // 
            // pbAvatar
            // 
            this.pbAvatar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbAvatar.Location = new System.Drawing.Point(80, 60);
            this.pbAvatar.Name = "pbAvatar";
            this.pbAvatar.Size = new System.Drawing.Size(150, 150);
            this.pbAvatar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbAvatar.TabIndex = 3;
            this.pbAvatar.TabStop = false;
            // 
            // btnChangeAvatar
            // 
            this.btnChangeAvatar.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnChangeAvatar.Location = new System.Drawing.Point(240, 120);
            this.btnChangeAvatar.Name = "btnChangeAvatar";
            this.btnChangeAvatar.Size = new System.Drawing.Size(90, 30);
            this.btnChangeAvatar.TabIndex = 4;
            this.btnChangeAvatar.Text = "Chọn Ảnh";
            this.btnChangeAvatar.UseVisualStyleBackColor = true;
            this.btnChangeAvatar.Click += new System.EventHandler(this.btnChangeAvatar_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.SystemColors.Highlight;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(33, 300);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(250, 45);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "Lưu Thay Đổi";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // frmSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 380);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnChangeAvatar);
            this.Controls.Add(this.pbAvatar);
            this.Controls.Add(this.txtDisplayName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblUserID);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cài Đặt Tài Khoản";
            this.Load += new System.EventHandler(this.frmSettings_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pbAvatar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblUserID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtDisplayName;
        private System.Windows.Forms.PictureBox pbAvatar;
        private System.Windows.Forms.Button btnChangeAvatar;
        private System.Windows.Forms.Button btnSave;
    }
}