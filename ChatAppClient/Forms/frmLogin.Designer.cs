namespace ChatAppClient.Forms
{
    partial class frmLogin
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.btnRegister = new ChatAppClient.CustomControls.RoundedButton();
            this.btnLogin = new ChatAppClient.CustomControls.RoundedButton();
            // THÊM 2 KHAI BÁO NÀY
            this.lblServerIp = new System.Windows.Forms.Label();
            this.txtServerIp = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 22.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = ChatAppClient.Helpers.AppColors.Primary;
            this.lblTitle.Location = new System.Drawing.Point(58, 20); // Dịch lên một chút
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(284, 51);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Chat && Caro"; // Đã sửa '&'
            // 
            // lblServerIp (NHÃN MỚI)
            // 
            this.lblServerIp.AutoSize = true;
            this.lblServerIp.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblServerIp.ForeColor = ChatAppClient.Helpers.AppColors.TextSecondary;
            this.lblServerIp.Location = new System.Drawing.Point(63, 85); // Vị trí
            this.lblServerIp.Name = "lblServerIp";
            this.lblServerIp.Size = new System.Drawing.Size(71, 20);
            this.lblServerIp.TabIndex = 5;
            this.lblServerIp.Text = "Server IP:";
            // 
            // txtServerIp (Ô NHẬP IP MỚI)
            // 
            this.txtServerIp.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtServerIp.Location = new System.Drawing.Point(67, 108); // Vị trí
            this.txtServerIp.Name = "txtServerIp";
            this.txtServerIp.Size = new System.Drawing.Size(266, 34);
            this.txtServerIp.TabIndex = 6; // TabIndex quan trọng
            this.txtServerIp.Text = "127.0.0.1"; // Giá trị mặc định
            // 
            // txtUsername
            // 
            this.txtUsername.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUsername.Location = new System.Drawing.Point(67, 155); // Dịch xuống
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(266, 34);
            this.txtUsername.TabIndex = 1; // Sửa TabIndex
            // 
            // txtPassword
            // 
            this.txtPassword.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPassword.Location = new System.Drawing.Point(67, 200); // Dịch xuống
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '●';
            this.txtPassword.Size = new System.Drawing.Size(266, 34);
            this.txtPassword.TabIndex = 2; // Sửa TabIndex
            // 
            // btnRegister
            // 
            this.btnRegister.BorderRadius = 20;
            this.btnRegister.ButtonColor = System.Drawing.Color.Gray;
            this.btnRegister.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnRegister.Location = new System.Drawing.Point(67, 310); // Dịch xuống
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(266, 45);
            this.btnRegister.TabIndex = 4; // Sửa TabIndex
            this.btnRegister.Text = "Đăng Ký";
            this.btnRegister.TextColor = System.Drawing.Color.White;
            // 
            // btnLogin
            // 
            this.btnLogin.BorderRadius = 20;
            this.btnLogin.ButtonColor = ChatAppClient.Helpers.AppColors.Primary;
            this.btnLogin.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnLogin.Location = new System.Drawing.Point(67, 255); // Dịch xuống
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(266, 45);
            this.btnLogin.TabIndex = 3; // Sửa TabIndex
            this.btnLogin.Text = "Đăng Nhập";
            this.btnLogin.TextColor = System.Drawing.Color.White;
            // 
            // frmLogin
            // 
            this.AcceptButton = this.btnLogin;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = ChatAppClient.Helpers.AppColors.FormBackground;
            this.ClientSize = new System.Drawing.Size(400, 400); // Giữ nguyên Size
            this.Controls.Add(this.txtServerIp); // Thêm control mới
            this.Controls.Add(this.lblServerIp); // Thêm control mới
            this.Controls.Add(this.btnRegister);
            this.Controls.Add(this.btnLogin);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "frmLogin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Đăng Nhập";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmLogin_FormClosing);
            this.Load += new System.EventHandler(this.frmLogin_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.TextBox txtPassword;
        private CustomControls.RoundedButton btnLogin;
        private CustomControls.RoundedButton btnRegister;
        // THÊM 2 KHAI BÁO BIẾN NÀY
        private System.Windows.Forms.Label lblServerIp;
        private System.Windows.Forms.TextBox txtServerIp;
    }
}