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
            this.btnOpenFirewall = new System.Windows.Forms.Button();
            this.btnTestConnection = new System.Windows.Forms.Button();
            this.btnShowHelp = new System.Windows.Forms.Button();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            this.lstUsers = new System.Windows.Forms.ListBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblServerIP = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnStart.Location = new System.Drawing.Point(12, 12);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(100, 50);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "▶ Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnStop.Location = new System.Drawing.Point(118, 12);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(100, 50);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "⏹ Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnOpenFirewall
            // 
            this.btnOpenFirewall.BackColor = System.Drawing.Color.Orange;
            this.btnOpenFirewall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpenFirewall.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnOpenFirewall.ForeColor = System.Drawing.Color.White;
            this.btnOpenFirewall.Location = new System.Drawing.Point(235, 12);
            this.btnOpenFirewall.Name = "btnOpenFirewall";
            this.btnOpenFirewall.Size = new System.Drawing.Size(130, 50);
            this.btnOpenFirewall.TabIndex = 2;
            this.btnOpenFirewall.Text = "🔓 Mở Firewall";
            this.btnOpenFirewall.UseVisualStyleBackColor = false;
            this.btnOpenFirewall.Click += new System.EventHandler(this.btnOpenFirewall_Click);
            // 
            // btnTestConnection
            // 
            this.btnTestConnection.BackColor = System.Drawing.Color.RoyalBlue;
            this.btnTestConnection.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTestConnection.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnTestConnection.ForeColor = System.Drawing.Color.White;
            this.btnTestConnection.Location = new System.Drawing.Point(371, 12);
            this.btnTestConnection.Name = "btnTestConnection";
            this.btnTestConnection.Size = new System.Drawing.Size(130, 50);
            this.btnTestConnection.TabIndex = 3;
            this.btnTestConnection.Text = "🔍 Check Port";
            this.btnTestConnection.UseVisualStyleBackColor = false;
            // 
            // btnShowHelp
            // 
            this.btnShowHelp.BackColor = System.Drawing.Color.Gray;
            this.btnShowHelp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnShowHelp.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnShowHelp.ForeColor = System.Drawing.Color.White;
            this.btnShowHelp.Location = new System.Drawing.Point(507, 12);
            this.btnShowHelp.Name = "btnShowHelp";
            this.btnShowHelp.Size = new System.Drawing.Size(100, 50);
            this.btnShowHelp.TabIndex = 4;
            this.btnShowHelp.Text = "❓ Hỗ trợ";
            this.btnShowHelp.UseVisualStyleBackColor = false;
            this.btnShowHelp.Click += new System.EventHandler(this.btnShowHelp_Click);
            // 
            // rtbLog
            // 
            this.rtbLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbLog.Location = new System.Drawing.Point(12, 107);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.Size = new System.Drawing.Size(650, 381);
            this.rtbLog.TabIndex = 5;
            this.rtbLog.Text = "";
            this.rtbLog.WordWrap = true;
            // 
            // lstUsers
            // 
            this.lstUsers.Dock = System.Windows.Forms.DockStyle.Right;
            this.lstUsers.FormattingEnabled = true;
            this.lstUsers.ItemHeight = 20;
            this.lstUsers.Location = new System.Drawing.Point(676, 0);
            this.lstUsers.Name = "lstUsers";
            this.lstUsers.Size = new System.Drawing.Size(206, 500);
            this.lstUsers.TabIndex = 6;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblStatus.Location = new System.Drawing.Point(623, 26);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(132, 23);
            this.lblStatus.TabIndex = 7;
            this.lblStatus.Text = "Status: Stopped";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblServerIP
            // 
            this.lblServerIP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblServerIP.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblServerIP.ForeColor = System.Drawing.Color.MidnightBlue;
            this.lblServerIP.Location = new System.Drawing.Point(12, 75);
            this.lblServerIP.Name = "lblServerIP";
            this.lblServerIP.Size = new System.Drawing.Size(650, 25);
            this.lblServerIP.TabIndex = 8;
            this.lblServerIP.Text = "IP Address: ...";
            // 
            // frmServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(882, 500);
            this.Controls.Add(this.lblServerIP);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.rtbLog);
            this.Controls.Add(this.btnShowHelp);
            this.Controls.Add(this.btnTestConnection);
            this.Controls.Add(this.btnOpenFirewall);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.lstUsers);
            this.Name = "frmServer";
            this.Text = "Chat App Server Monitor";
            this.Load += new System.EventHandler(this.frmServer_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnOpenFirewall;
        private System.Windows.Forms.Button btnTestConnection; // Thêm nút check
        private System.Windows.Forms.Button btnShowHelp;       // Thêm nút Help
        private System.Windows.Forms.RichTextBox rtbLog;
        private System.Windows.Forms.ListBox lstUsers;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblServerIP;
    }
}