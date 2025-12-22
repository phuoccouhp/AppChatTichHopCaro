namespace ChatAppClient.UserControls
{
    partial class FileBubble
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        } // Giữ code mặc định

        #region Component Designer generated code
        private void InitializeComponent()
        {
            this.pnlContainer = new System.Windows.Forms.Panel();
            this.pbIcon = new System.Windows.Forms.PictureBox();
            this.lblFileName = new System.Windows.Forms.Label();
            this.btnDownload = new System.Windows.Forms.Button();
            this.btnForward = new System.Windows.Forms.Button();
            this.pnlContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlContainer
            // 
            this.pnlContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.pnlContainer.Controls.Add(this.btnForward);
            this.pnlContainer.Controls.Add(this.btnDownload);
            this.pnlContainer.Controls.Add(this.lblFileName);
            this.pnlContainer.Controls.Add(this.pbIcon);
            this.pnlContainer.Location = new System.Drawing.Point(0, 0);
            this.pnlContainer.Name = "pnlContainer";
            this.pnlContainer.Size = new System.Drawing.Size(250, 60);
            this.pnlContainer.TabIndex = 0;
            // 
            // pbIcon
            // 
            // Lấy icon file từ hệ thống
            this.pbIcon.Image = System.Drawing.SystemIcons.Shield.ToBitmap();
            this.pbIcon.Size = new System.Drawing.Size(40, 40);
            this.pbIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbIcon.TabIndex = 0;
            this.pbIcon.TabStop = false;
            // 
            // lblFileName
            // 
            this.lblFileName.AutoEllipsis = true;
            this.lblFileName.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileName.Location = new System.Drawing.Point(56, 10);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(128, 40);
            this.lblFileName.TabIndex = 1;
            this.lblFileName.Text = "ten_file_rat_dai_de_test.pdf";
            this.lblFileName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnDownload
            // 
            this.btnDownload.Font = new System.Drawing.Font("Segoe UI Emoji", 10.2F);
            this.btnDownload.Location = new System.Drawing.Point(190, 15);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(28, 30);
            this.btnDownload.TabIndex = 2;
            this.btnDownload.Text = "💾"; // Save icon
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Visible = false;
            this.btnDownload.Cursor = System.Windows.Forms.Cursors.Hand;
            // 
            // btnForward
            // 
            this.btnForward.BackColor = System.Drawing.Color.FromArgb(100, 100, 100);
            this.btnForward.FlatAppearance.BorderSize = 0;
            this.btnForward.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnForward.Font = new System.Drawing.Font("Segoe UI Emoji", 10F);
            this.btnForward.ForeColor = System.Drawing.Color.White;
            this.btnForward.Location = new System.Drawing.Point(145, 15);
            this.btnForward.Name = "btnForward";
            this.btnForward.Size = new System.Drawing.Size(28, 30);
            this.btnForward.TabIndex = 3;
            this.btnForward.Text = "➡️";
            this.btnForward.UseVisualStyleBackColor = false;
            this.btnForward.Visible = false;
            this.btnForward.Cursor = System.Windows.Forms.Cursors.Hand;
            // 
            // FileBubble
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.pnlContainer);
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "FileBubble";
            this.Size = new System.Drawing.Size(253, 63);
            this.pnlContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon)).EndInit();
            this.ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.Panel pnlContainer;
        private System.Windows.Forms.PictureBox pbIcon;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.Button btnForward;
    }
}