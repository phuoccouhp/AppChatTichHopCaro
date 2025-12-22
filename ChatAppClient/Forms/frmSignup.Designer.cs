namespace ChatAppClient.Forms
{
    partial class frmSignup
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSignup));
            lblTitle = new Label();
            txtUser = new RoundedTextBox();
            txtPass = new RoundedTextBox();
            txtConfirm = new RoundedTextBox();
            btnRegister = new RoundedButton();
            lnkLogin = new LinkLabel();
            lblNoAccount = new Label();
            txtEmail = new RoundedTextBox();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.BackColor = Color.Transparent;
            lblTitle.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(0, 240, 255);
            lblTitle.Location = new Point(191, 53);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(201, 62);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "SIGN UP";
            // 
            // txtUser
            // 
            txtUser.BackColor = Color.Transparent;
            txtUser.Icon = (Image)resources.GetObject("txtUser.Icon");
            txtUser.IconEnd = null;
            txtUser.IsPassword = false;
            txtUser.Location = new Point(114, 173);
            txtUser.Margin = new Padding(3, 4, 3, 4);
            txtUser.Name = "txtUser";
            txtUser.Padding = new Padding(11, 13, 11, 13);
            txtUser.PlaceholderText = "Username";
            txtUser.Size = new Size(343, 60);
            txtUser.TabIndex = 1;
            // 
            // txtPass
            // 
            txtPass.BackColor = Color.Transparent;
            txtPass.Icon = (Image)resources.GetObject("txtPass.Icon");
            txtPass.IconEnd = (Image)resources.GetObject("txtPass.IconEnd");
            txtPass.IsPassword = true;
            txtPass.Location = new Point(114, 267);
            txtPass.Margin = new Padding(3, 4, 3, 4);
            txtPass.Name = "txtPass";
            txtPass.Padding = new Padding(11, 13, 11, 13);
            txtPass.PlaceholderText = "Password";
            txtPass.Size = new Size(343, 60);
            txtPass.TabIndex = 2;
            // 
            // txtConfirm
            // 
            txtConfirm.BackColor = Color.Transparent;
            txtConfirm.Icon = (Image)resources.GetObject("txtConfirm.Icon");
            txtConfirm.IconEnd = (Image)resources.GetObject("txtConfirm.IconEnd");
            txtConfirm.IsPassword = true;
            txtConfirm.Location = new Point(114, 360);
            txtConfirm.Margin = new Padding(3, 4, 3, 4);
            txtConfirm.Name = "txtConfirm";
            txtConfirm.Padding = new Padding(11, 13, 11, 13);
            txtConfirm.PlaceholderText = "Confirm Password";
            txtConfirm.Size = new Size(343, 60);
            txtConfirm.TabIndex = 3;
            // 
            // btnRegister
            // 
            btnRegister.BackColor = Color.FromArgb(12, 42, 82);
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.FlatStyle = FlatStyle.Flat;
            btnRegister.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnRegister.ForeColor = Color.White;
            btnRegister.Location = new Point(114, 551);
            btnRegister.Margin = new Padding(3, 4, 3, 4);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new Size(343, 67);
            btnRegister.TabIndex = 4;
            btnRegister.Text = "Create Account";
            btnRegister.UseVisualStyleBackColor = false;
            // 
            // lnkLogin
            // 
            lnkLogin.AutoSize = true;
            lnkLogin.BackColor = Color.Transparent;
            lnkLogin.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lnkLogin.LinkColor = Color.FromArgb(0, 132, 255);
            lnkLogin.Location = new Point(345, 646);
            lnkLogin.Name = "lnkLogin";
            lnkLogin.Size = new Size(96, 24);
            lnkLogin.TabIndex = 5;
            lnkLogin.TabStop = true;
            lnkLogin.Text = "Login now!";
            lnkLogin.LinkClicked += lnkLogin_LinkClicked;
            // 
            // lblNoAccount
            // 
            lblNoAccount.AutoSize = true;
            lblNoAccount.BackColor = Color.Transparent;
            lblNoAccount.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblNoAccount.ForeColor = Color.White;
            lblNoAccount.Location = new Point(135, 646);
            lblNoAccount.Margin = new Padding(4, 0, 4, 0);
            lblNoAccount.Name = "lblNoAccount";
            lblNoAccount.Size = new Size(203, 24);
            lblNoAccount.TabIndex = 10;
            lblNoAccount.Text = "Already have an account?";
            // 
            // txtEmail
            // 
            txtEmail.BackColor = Color.Transparent;
            txtEmail.Icon = (Image)resources.GetObject("txtEmail.Icon");
            txtEmail.IconEnd = null;
            txtEmail.IsPassword = false;
            txtEmail.Location = new Point(114, 452);
            txtEmail.Margin = new Padding(3, 4, 3, 4);
            txtEmail.Name = "txtEmail";
            txtEmail.Padding = new Padding(11, 13, 11, 13);
            txtEmail.PlaceholderText = "E-mail address";
            txtEmail.Size = new Size(343, 60);
            txtEmail.TabIndex = 1;
            // 
            // frmSignup
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(571, 733);
            Controls.Add(lblNoAccount);
            Controls.Add(lnkLogin);
            Controls.Add(btnRegister);
            Controls.Add(txtConfirm);
            Controls.Add(txtPass);
            Controls.Add(txtEmail);
            Controls.Add(txtUser);
            Controls.Add(lblTitle);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            Name = "frmSignup";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Sign Up";
            Load += frmSignup_Load;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        public ChatAppClient.RoundedTextBox txtUser;
        public ChatAppClient.RoundedTextBox txtPass;
        public ChatAppClient.RoundedTextBox txtConfirm;
        public ChatAppClient.RoundedButton btnRegister;
        public System.Windows.Forms.LinkLabel lnkLogin;
        private Label lblNoAccount;
        public RoundedTextBox txtEmail;
    }
}