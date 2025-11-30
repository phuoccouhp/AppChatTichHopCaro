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
            rtbLog = new RichTextBox();
            lstUsers = new ListBox();
            lblStatus = new Label();
            SuspendLayout();
            // 
            // btnStart
            // 
            btnStart.Location = new Point(12, 12);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(103, 55);
            btnStart.TabIndex = 0;
            btnStart.Text = "Start Server";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // btnStop
            // 
            btnStop.Enabled = false;
            btnStop.Location = new Point(151, 12);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(103, 60);
            btnStop.TabIndex = 1;
            btnStop.Text = "Stop Server";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            // 
            // rtbLog
            // 
            rtbLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtbLog.BackColor = SystemColors.HighlightText;
            rtbLog.Location = new Point(12, 82);
            rtbLog.Name = "rtbLog";
            rtbLog.Size = new Size(554, 347);
            rtbLog.TabIndex = 2;
            rtbLog.Text = "";
            // 
            // lstUsers
            // 
            lstUsers.Dock = DockStyle.Right;
            lstUsers.FormattingEnabled = true;
            lstUsers.Location = new Point(602, 0);
            lstUsers.Name = "lstUsers";
            lstUsers.Size = new Size(198, 450);
            lstUsers.TabIndex = 3;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(317, 38);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(113, 20);
            lblStatus.TabIndex = 4;
            lblStatus.Text = "Status: Stopped";
            // 
            // frmServer
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(lblStatus);
            Controls.Add(lstUsers);
            Controls.Add(rtbLog);
            Controls.Add(btnStop);
            Controls.Add(btnStart);
            Name = "frmServer";
            Text = "Chat App Server Monitor";
            Load += frmServer_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.RichTextBox rtbLog;
        private System.Windows.Forms.ListBox lstUsers;
        private System.Windows.Forms.Label lblStatus;
    }
}