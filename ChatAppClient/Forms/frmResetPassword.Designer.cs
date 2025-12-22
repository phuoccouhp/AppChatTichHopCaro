namespace ChatAppClient.Forms
{
    partial class frmResetPassword
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmResetPassword));
            lblTitle = new Label();
            lblInstruction = new Label();
            txtNewPassword = new RoundedTextBox();
            txtConfirmPassword = new RoundedTextBox();
            btnReset = new RoundedButton();
            lnkBack = new LinkLabel();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.BackColor = Color.Transparent;
            lblTitle.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(0, 240, 255);
            lblTitle.Location = new Point(120, 67);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(355, 39);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "RESET PASSWORD";
            // 
            // lblInstruction
            // 
            lblInstruction.AutoSize = true;
            lblInstruction.BackColor = Color.Transparent;
            lblInstruction.Font = new Font("Segoe UI", 11F);
            lblInstruction.ForeColor = Color.White;
            lblInstruction.Location = new Point(119, 134);
            lblInstruction.Name = "lblInstruction";
            lblInstruction.Size = new Size(322, 24);
            lblInstruction.TabIndex = 1;
            lblInstruction.Text = "Enter your new password";
            lblInstruction.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtNewPassword
            // 
            txtNewPassword.BackColor = Color.Transparent;
            txtNewPassword.Font = new Font("Segoe UI", 11F);
            txtNewPassword.Icon = (Image)resources.GetObject("txtPass.Icon");
            txtNewPassword.IconEnd = (Image)resources.GetObject("txtPass.IconEnd");
            txtNewPassword.IsPassword = true;
            txtNewPassword.Location = new Point(109, 200);
            txtNewPassword.Margin = new Padding(3, 4, 3, 4);
            txtNewPassword.Name = "txtNewPassword";
            txtNewPassword.Padding = new Padding(11, 13, 11, 13);
            txtNewPassword.PlaceholderText = "New Password";
            txtNewPassword.Size = new Size(343, 60);
            txtNewPassword.TabIndex = 2;
            txtNewPassword.Text = "New Password";
            // 
            // txtConfirmPassword
            // 
            txtConfirmPassword.BackColor = Color.Transparent;
            txtConfirmPassword.Font = new Font("Segoe UI", 11F);
            txtConfirmPassword.Icon = (Image)resources.GetObject("txtPass.Icon");
            txtConfirmPassword.IconEnd = (Image)resources.GetObject("txtPass.IconEnd");
            txtConfirmPassword.IsPassword = true;
            txtConfirmPassword.Location = new Point(109, 279);
            txtConfirmPassword.Margin = new Padding(3, 4, 3, 4);
            txtConfirmPassword.Name = "txtConfirmPassword";
            txtConfirmPassword.Padding = new Padding(11, 13, 11, 13);
            txtConfirmPassword.PlaceholderText = "Confirm Password";
            txtConfirmPassword.Size = new Size(343, 60);
            txtConfirmPassword.TabIndex = 3;
            txtConfirmPassword.Text = "Confirm Password";
            // 
            // btnReset
            // 
            btnReset.BackColor = Color.FromArgb(12, 42, 82);
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.FlatStyle = FlatStyle.Flat;
            btnReset.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            btnReset.ForeColor = Color.White;
            btnReset.Location = new Point(120, 364);
            btnReset.Margin = new Padding(3, 4, 3, 4);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(343, 67);
            btnReset.TabIndex = 4;
            btnReset.Text = "Reset Password";
            btnReset.UseVisualStyleBackColor = false;
            // 
            // lnkBack
            // 
            lnkBack.AutoSize = true;
            lnkBack.BackColor = Color.Transparent;
            lnkBack.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            lnkBack.LinkColor = Color.FromArgb(0, 132, 255);
            lnkBack.Location = new Point(223, 462);
            lnkBack.Name = "lnkBack";
            lnkBack.Size = new Size(125, 20);
            lnkBack.TabIndex = 5;
            lnkBack.TabStop = true;
            lnkBack.Text = "Back to Login";
            // 
            // frmResetPassword
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(571, 533);
            Controls.Add(lnkBack);
            Controls.Add(btnReset);
            Controls.Add(txtConfirmPassword);
            Controls.Add(txtNewPassword);
            Controls.Add(lblInstruction);
            Controls.Add(lblTitle);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            Name = "frmResetPassword";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Reset Password";
            Load += frmResetPassword_Load;
            FormClosing += frmResetPassword_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblInstruction;
        public ChatAppClient.RoundedTextBox txtNewPassword;
        public ChatAppClient.RoundedTextBox txtConfirmPassword;
        public ChatAppClient.RoundedButton btnReset;
        public System.Windows.Forms.LinkLabel lnkBack;
    }
}

