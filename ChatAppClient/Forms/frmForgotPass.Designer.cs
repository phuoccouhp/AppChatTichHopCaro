namespace ChatAppClient.Forms
{
    partial class frmForgotPass
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmForgotPass));
            lblTitle = new Label();
            lblInstruction = new Label();
            txtEmail = new RoundedTextBox();
            btnSend = new RoundedButton();
            lnkBack = new LinkLabel();
            roundedButton1 = new RoundedButton();
            txtOTP = new RoundedTextBox();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.BackColor = Color.Transparent;
            lblTitle.Font = new Font("Montserrat", 20F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(0, 240, 255);
            lblTitle.Location = new Point(120, 67);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(361, 53);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "PASSWORD RESET";
            // 
            // lblInstruction
            // 
            lblInstruction.AutoSize = true;
            lblInstruction.BackColor = Color.Transparent;
            lblInstruction.Font = new Font("Montserrat", 11F);
            lblInstruction.ForeColor = Color.White;
            lblInstruction.Location = new Point(119, 134);
            lblInstruction.Name = "lblInstruction";
            lblInstruction.Size = new Size(361, 30);
            lblInstruction.TabIndex = 1;
            lblInstruction.Text = "Enter your email to receive OTP pass";
            lblInstruction.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtEmail
            // 
            txtEmail.BackColor = Color.Transparent;
            txtEmail.Icon = null;
            txtEmail.IconEnd = null;
            txtEmail.IsPassword = false;
            txtEmail.Location = new Point(109, 200);
            txtEmail.Margin = new Padding(3, 4, 3, 4);
            txtEmail.Name = "txtEmail";
            txtEmail.Padding = new Padding(11, 13, 11, 13);
            txtEmail.PlaceholderText = "Enter your E-mail address";
            txtEmail.Size = new Size(343, 60);
            txtEmail.TabIndex = 2;
            // 
            // btnSend
            // 
            btnSend.BackColor = Color.FromArgb(12, 42, 82);
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.FlatStyle = FlatStyle.Flat;
            btnSend.Font = new Font("Montserrat", 14F, FontStyle.Bold);
            btnSend.ForeColor = Color.White;
            btnSend.Location = new Point(120, 364);
            btnSend.Margin = new Padding(3, 4, 3, 4);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(343, 67);
            btnSend.TabIndex = 3;
            btnSend.Text = "Reset My Password";
            btnSend.UseVisualStyleBackColor = false;
            // 
            // lnkBack
            // 
            lnkBack.AutoSize = true;
            lnkBack.BackColor = Color.Transparent;
            lnkBack.Font = new Font("Montserrat", 10F, FontStyle.Bold);
            lnkBack.LinkColor = Color.FromArgb(0, 132, 255);
            lnkBack.Location = new Point(223, 462);
            lnkBack.Name = "lnkBack";
            lnkBack.Size = new Size(134, 27);
            lnkBack.TabIndex = 4;
            lnkBack.TabStop = true;
            lnkBack.Text = "Back to Login";
            lnkBack.LinkClicked += lnkBack_LinkClicked;
            // 
            // roundedButton1
            // 
            roundedButton1.BackColor = Color.FromArgb(153, 0, 17);
            roundedButton1.FlatAppearance.BorderSize = 0;
            roundedButton1.FlatStyle = FlatStyle.Flat;
            roundedButton1.Font = new Font("Montserrat ExtraBold", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            roundedButton1.ForeColor = Color.FromArgb(247, 197, 204);
            roundedButton1.Location = new Point(337, 279);
            roundedButton1.Margin = new Padding(3, 4, 3, 4);
            roundedButton1.Name = "roundedButton1";
            roundedButton1.Size = new Size(115, 60);
            roundedButton1.TabIndex = 3;
            roundedButton1.Text = "Send OTP";
            roundedButton1.UseVisualStyleBackColor = false;
            // 
            // txtOTP
            // 
            txtOTP.BackColor = Color.Transparent;
            txtOTP.Icon = null;
            txtOTP.IconEnd = null;
            txtOTP.IsPassword = false;
            txtOTP.Location = new Point(109, 279);
            txtOTP.Margin = new Padding(3, 4, 3, 4);
            txtOTP.Name = "txtOTP";
            txtOTP.Padding = new Padding(11, 13, 11, 13);
            txtOTP.PlaceholderText = "Enter your OTP";
            txtOTP.Size = new Size(222, 60);
            txtOTP.TabIndex = 2;
            // 
            // frmForgotPass
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(571, 533);
            Controls.Add(lnkBack);
            Controls.Add(roundedButton1);
            Controls.Add(btnSend);
            Controls.Add(txtOTP);
            Controls.Add(txtEmail);
            Controls.Add(lblInstruction);
            Controls.Add(lblTitle);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            Name = "frmForgotPass";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Forgot Password";
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblInstruction;
        public ChatAppClient.RoundedTextBox txtEmail;
        public ChatAppClient.RoundedButton btnSend;
        public System.Windows.Forms.LinkLabel lnkBack;
        public RoundedButton roundedButton1;
        public RoundedTextBox txtOTP;
    }
}