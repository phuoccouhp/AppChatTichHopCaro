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
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            this.lstUsers = new System.Windows.Forms.ListBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblServerIP = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(12, 12);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(103, 55);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start Server";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(151, 12);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(103, 60);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "Stop Server";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // rtbLog
            // 
            this.rtbLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.rtbLog.Location = new System.Drawing.Point(12, 100);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.Size = new System.Drawing.Size(584, 329);
            this.rtbLog.TabIndex = 2;
            this.rtbLog.Text = "";
            this.rtbLog.WordWrap = true;
            this.rtbLog.DetectUrls = false;
            // 
            // lstUsers
            // 
            this.lstUsers.Dock = System.Windows.Forms.DockStyle.Right;
            this.lstUsers.FormattingEnabled = true;
            this.lstUsers.ItemHeight = 20; // Có thể thay đổi tùy font
            this.lstUsers.Location = new System.Drawing.Point(602, 0);
            this.lstUsers.Name = "lstUsers";
            this.lstUsers.Size = new System.Drawing.Size(198, 450);
            this.lstUsers.TabIndex = 3;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(317, 38);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(113, 20);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "Status: Stopped";
            // 
            // lblServerIP
            // 
            this.lblServerIP.AutoSize = false;
            this.lblServerIP.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblServerIP.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblServerIP.Location = new System.Drawing.Point(12, 70);
            this.lblServerIP.Name = "lblServerIP";
            this.lblServerIP.Size = new System.Drawing.Size(580, 25);
            this.lblServerIP.TabIndex = 5;
            // 
            // frmServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblServerIP);
            this.Controls.Add(this.rtbLog);
            this.Controls.Add(this.lstUsers); // Add ListBox after rtbLog (Dock Right)
            this.Name = "frmServer";
            this.Text = "Chat App Server Monitor";
            this.Load += new System.EventHandler(this.frmServer_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.RichTextBox rtbLog;
        private System.Windows.Forms.ListBox lstUsers;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblServerIP;
    }
}