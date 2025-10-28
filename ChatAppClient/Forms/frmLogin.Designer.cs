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
            lblTitle = new Label();
            txtUsername = new TextBox();
            txtPassword = new TextBox();
            btnRegister = new CustomControls.RoundedButton();
            btnLogin = new CustomControls.RoundedButton();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 22.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.ForeColor = Color.FromArgb(0, 145, 255);
            lblTitle.Location = new Point(58, 40);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(202, 50);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Chat && Caro";
            // 
            // txtUsername
            // 
            txtUsername.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtUsername.Location = new Point(69, 130);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(266, 34);
            txtUsername.TabIndex = 1;
            txtUsername.Text = "user1";
            // 
            // txtPassword
            // 
            txtPassword.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtPassword.Location = new Point(69, 180);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '●';
            txtPassword.Size = new Size(266, 34);
            txtPassword.TabIndex = 2;
            txtPassword.Text = "123";
            // 
            // btnRegister
            // 
            btnRegister.BackColor = Color.Transparent;
            btnRegister.BorderRadius = 20;
            btnRegister.ButtonColor = Color.Gray;
            btnRegister.FlatStyle = FlatStyle.Flat;
            btnRegister.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnRegister.ForeColor = Color.Transparent;
            btnRegister.Location = new Point(69, 300);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new Size(266, 45);
            btnRegister.TabIndex = 4;
            btnRegister.Text = "Đăng Ký";
            btnRegister.TextColor = Color.White;
            btnRegister.UseVisualStyleBackColor = false;
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.Transparent;
            btnLogin.BorderRadius = 20;
            btnLogin.ButtonColor = Color.FromArgb(0, 145, 255);
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnLogin.ForeColor = Color.Transparent;
            btnLogin.Location = new Point(69, 240);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(266, 45);
            btnLogin.TabIndex = 3;
            btnLogin.Text = "Đăng Nhập";
            btnLogin.TextColor = Color.White;
            btnLogin.UseVisualStyleBackColor = false;
            // 
            // frmLogin
            // 
            AcceptButton = btnLogin;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(400, 400);
            Controls.Add(btnRegister);
            Controls.Add(btnLogin);
            Controls.Add(txtPassword);
            Controls.Add(txtUsername);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "frmLogin";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Đăng Nhập";
            FormClosing += frmLogin_FormClosing;
            Load += frmLogin_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.TextBox txtPassword;
        private CustomControls.RoundedButton btnLogin;
        private CustomControls.RoundedButton btnRegister;
    }
}