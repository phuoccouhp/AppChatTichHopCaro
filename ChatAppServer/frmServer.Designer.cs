namespace ChatAppServer
{
    partial class frmServer
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnStart = new Button();
            btnStop = new Button();
            btnOpenFirewall = new Button();
            rtbLog = new RichTextBox();
            lstUsers = new ListBox();
            lblStatus = new Label();
            lblServerIP = new Label();
            SuspendLayout();
            // 
            // btnStart
            // 
            btnStart.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnStart.Location = new Point(12, 12);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(100, 50);
            btnStart.TabIndex = 0;
            btnStart.Text = "▶ Start";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // btnStop
            // 
            btnStop.Enabled = false;
            btnStop.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnStop.Location = new Point(118, 12);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(100, 50);
            btnStop.TabIndex = 1;
            btnStop.Text = "⏹ Stop";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            // 
            // btnOpenFirewall
            // 
            btnOpenFirewall.BackColor = Color.Orange;
            btnOpenFirewall.FlatStyle = FlatStyle.Flat;
            btnOpenFirewall.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnOpenFirewall.ForeColor = Color.White;
            btnOpenFirewall.Location = new Point(235, 12);
            btnOpenFirewall.Name = "btnOpenFirewall";
            btnOpenFirewall.Size = new Size(130, 50);
            btnOpenFirewall.TabIndex = 2;
            btnOpenFirewall.Text = "🔓 Mở Firewall";
            btnOpenFirewall.UseVisualStyleBackColor = false;
            btnOpenFirewall.Click += btnOpenFirewall_Click;
            // 
            // rtbLog
            // 
            rtbLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtbLog.Location = new Point(12, 107);
            rtbLog.Name = "rtbLog";
            rtbLog.Size = new Size(650, 381);
            rtbLog.TabIndex = 5;
            rtbLog.Text = "";
            // 
            // lstUsers
            // 
            lstUsers.Dock = DockStyle.Right;
            lstUsers.FormattingEnabled = true;
            lstUsers.Location = new Point(676, 0);
            lstUsers.Name = "lstUsers";
            lstUsers.Size = new Size(206, 500);
            lstUsers.TabIndex = 6;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 10F);
            lblStatus.Location = new Point(425, 25);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(128, 23);
            lblStatus.TabIndex = 7;
            lblStatus.Text = "Status: Stopped";
            lblStatus.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblServerIP
            // 
            lblServerIP.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblServerIP.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblServerIP.ForeColor = Color.MidnightBlue;
            lblServerIP.Location = new Point(12, 75);
            lblServerIP.Name = "lblServerIP";
            lblServerIP.Size = new Size(650, 25);
            lblServerIP.TabIndex = 8;
            lblServerIP.Text = "IP Address: ...";
            // 
            // frmServer
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(882, 500);
            Controls.Add(lblServerIP);
            Controls.Add(lblStatus);
            Controls.Add(rtbLog);
            Controls.Add(btnOpenFirewall);
            Controls.Add(btnStop);
            Controls.Add(btnStart);
            Controls.Add(lstUsers);
            Name = "frmServer";
            Text = "Chat App Server Monitor";
            Load += frmServer_Load;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnOpenFirewall;
        private System.Windows.Forms.RichTextBox rtbLog;
        private System.Windows.Forms.ListBox lstUsers;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblServerIP;
    }
}