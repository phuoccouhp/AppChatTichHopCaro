using ChatAppClient;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLogin));
            lblTitle = new Label();
            txtUser = new RoundedTextBox();
            txtPass = new RoundedTextBox();
            chkRemember = new CheckBox();
            lnkForgot = new LinkLabel();
            btnLogin = new RoundedButton();
            lblNoAccount = new Label();
            lnkSignup = new LinkLabel();
            txtServerIP = new RoundedTextBox();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.BackColor = Color.Transparent;
            lblTitle.Font = new Font("Microsoft Sans Serif", 24F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.ForeColor = Color.FromArgb(0, 240, 255);
            lblTitle.Location = new Point(150, 49);
            lblTitle.Margin = new Padding(4, 0, 4, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(361, 46);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "CHAT APP CARO";
            // 
            // txtUser
            // 
            txtUser.BackColor = Color.Transparent;
            txtUser.Font = new Font("Microsoft Sans Serif", 9F);
            txtUser.Icon = (Image)resources.GetObject("txtUser.Icon");
            txtUser.IconEnd = null;
            txtUser.IsPassword = false;
            txtUser.Location = new Point(101, 247);
            txtUser.Margin = new Padding(4, 5, 4, 5);
            txtUser.Name = "txtUser";
            txtUser.Padding = new Padding(13, 8, 13, 8);
            txtUser.PlaceholderText = "Username";
            txtUser.Size = new Size(467, 69);
            txtUser.TabIndex = 4;
            txtUser.Text = "Username";
            // 
            // txtPass
            // 
            txtPass.BackColor = Color.Transparent;
            txtPass.Font = new Font("Microsoft Sans Serif", 9F);
            txtPass.Icon = (Image)resources.GetObject("txtPass.Icon");
            txtPass.IconEnd = (Image)resources.GetObject("txtPass.IconEnd");
            txtPass.IsPassword = true;
            txtPass.Location = new Point(101, 339);
            txtPass.Margin = new Padding(4, 5, 4, 5);
            txtPass.Name = "txtPass";
            txtPass.Padding = new Padding(13, 8, 13, 8);
            txtPass.PlaceholderText = "Password";
            txtPass.Size = new Size(467, 69);
            txtPass.TabIndex = 5;
            txtPass.Text = "Password";
            // 
            // chkRemember
            // 
            chkRemember.AutoSize = true;
            chkRemember.BackColor = Color.Transparent;
            chkRemember.Font = new Font("Microsoft Sans Serif", 10F);
            chkRemember.ForeColor = Color.White;
            chkRemember.Location = new Point(101, 431);
            chkRemember.Margin = new Padding(4, 5, 4, 5);
            chkRemember.Name = "chkRemember";
            chkRemember.Size = new Size(141, 24);
            chkRemember.TabIndex = 6;
            chkRemember.Text = "Remember me";
            chkRemember.UseVisualStyleBackColor = false;
            // 
            // lnkForgot
            // 
            lnkForgot.AutoSize = true;
            lnkForgot.BackColor = Color.Transparent;
            lnkForgot.Font = new Font("Microsoft Sans Serif", 10F);
            lnkForgot.LinkColor = Color.White;
            lnkForgot.Location = new Point(358, 432);
            lnkForgot.Margin = new Padding(4, 0, 4, 0);
            lnkForgot.Name = "lnkForgot";
            lnkForgot.Size = new Size(180, 20);
            lnkForgot.TabIndex = 7;
            lnkForgot.TabStop = true;
            lnkForgot.Text = "Forgot your password?";
            lnkForgot.LinkClicked += lnkForgot_LinkClicked;
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.FromArgb(12, 42, 82);
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Bold);
            btnLogin.ForeColor = Color.White;
            btnLogin.Location = new Point(101, 493);
            btnLogin.Margin = new Padding(4, 5, 4, 5);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(467, 77);
            btnLogin.TabIndex = 8;
            btnLogin.Text = "Log in";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += BtnLogin_Click;
            // 
            // lblNoAccount
            // 
            lblNoAccount.AutoSize = true;
            lblNoAccount.BackColor = Color.Transparent;
            lblNoAccount.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblNoAccount.ForeColor = Color.White;
            lblNoAccount.Location = new Point(201, 601);
            lblNoAccount.Margin = new Padding(4, 0, 4, 0);
            lblNoAccount.Name = "lblNoAccount";
            lblNoAccount.Size = new Size(111, 18);
            lblNoAccount.TabIndex = 9;
            lblNoAccount.Text = "Not a member?";
            // 
            // lnkSignup
            // 
            lnkSignup.AutoSize = true;
            lnkSignup.BackColor = Color.Transparent;
            lnkSignup.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            lnkSignup.LinkColor = Color.FromArgb(0, 132, 255);
            lnkSignup.Location = new Point(338, 601);
            lnkSignup.Margin = new Padding(4, 0, 4, 0);
            lnkSignup.Name = "lnkSignup";
            lnkSignup.Size = new Size(104, 18);
            lnkSignup.TabIndex = 10;
            lnkSignup.TabStop = true;
            lnkSignup.Text = "Sign up now!";
            lnkSignup.LinkClicked += lnkSignup_LinkClicked;
            // 
            // txtServerIP
            // 
            txtServerIP.BackColor = Color.Transparent;
            txtServerIP.Font = new Font("Microsoft Sans Serif", 9F);
            txtServerIP.Icon = (Image)resources.GetObject("txtServerIP.Icon");
            txtServerIP.IconEnd = null;
            txtServerIP.IsPassword = false;
            txtServerIP.Location = new Point(101, 154);
            txtServerIP.Margin = new Padding(4, 5, 4, 5);
            txtServerIP.Name = "txtServerIP";
            txtServerIP.Padding = new Padding(13, 8, 13, 8);
            txtServerIP.PlaceholderText = "127.0.0.1";
            txtServerIP.Size = new Size(467, 69);
            txtServerIP.TabIndex = 4;
            txtServerIP.Text = "10.45.100.45";
            // 
            // frmLogin
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(667, 660);
            Controls.Add(lnkSignup);
            Controls.Add(lblNoAccount);
            Controls.Add(btnLogin);
            Controls.Add(lnkForgot);
            Controls.Add(chkRemember);
            Controls.Add(txtPass);
            Controls.Add(txtServerIP);
            Controls.Add(txtUser);
            Controls.Add(lblTitle);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            Name = "frmLogin";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Login";
            Load += frmLogin_Load;
            ResumeLayout(false);
            PerformLayout();
        }


        #endregion

        private System.Windows.Forms.Label lblTitle;
        public ChatAppClient.RoundedTextBox txtUser;
        public ChatAppClient.RoundedTextBox txtPass;
        public System.Windows.Forms.CheckBox chkRemember;
        public System.Windows.Forms.LinkLabel lnkForgot;
        public ChatAppClient.RoundedButton btnLogin;
        private System.Windows.Forms.Label lblNoAccount;
        public System.Windows.Forms.LinkLabel lnkSignup;
        public RoundedTextBox txtServerIP;
    }
}