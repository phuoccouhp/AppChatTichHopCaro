namespace ChatAppClient.UserControls
{
    partial class ImageBubble
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        private void InitializeComponent()
        {
            this.pbImage = new System.Windows.Forms.PictureBox();
            this.btnDownload = new System.Windows.Forms.Button();
            this.btnForward = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbImage)).BeginInit();
            this.SuspendLayout();
            // 
            // pbImage
            // 
            this.pbImage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbImage.Location = new System.Drawing.Point(3, 3);
            this.pbImage.Name = "pbImage";
            this.pbImage.Size = new System.Drawing.Size(200, 150); // Kích thước mặc định
            this.pbImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbImage.TabIndex = 0;
            this.pbImage.TabStop = false;
            // 
            // btnDownload
            // 
            this.btnDownload.BackColor = System.Drawing.Color.FromArgb(100, 100, 100);
            this.btnDownload.FlatAppearance.BorderSize = 0;
            this.btnDownload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDownload.Font = new System.Drawing.Font("Segoe UI Emoji", 10F);
            this.btnDownload.ForeColor = System.Drawing.Color.White;
            this.btnDownload.Location = new System.Drawing.Point(5, 5);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(28, 28);
            this.btnDownload.TabIndex = 1;
            this.btnDownload.Text = "💾";
            this.btnDownload.UseVisualStyleBackColor = false;
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
            this.btnForward.Location = new System.Drawing.Point(38, 5);
            this.btnForward.Name = "btnForward";
            this.btnForward.Size = new System.Drawing.Size(28, 28);
            this.btnForward.TabIndex = 2;
            this.btnForward.Text = "➡️";
            this.btnForward.UseVisualStyleBackColor = false;
            this.btnForward.Visible = false;
            this.btnForward.Cursor = System.Windows.Forms.Cursors.Hand;
            // 
            // ImageBubble
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.Transparent; // Nền trong suốt
            this.Controls.Add(this.btnForward);
            this.Controls.Add(this.btnDownload);
            this.Controls.Add(this.pbImage);
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "ImageBubble";
            this.Size = new System.Drawing.Size(206, 156);
            ((System.ComponentModel.ISupportInitialize)(this.pbImage)).EndInit();
            this.ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.PictureBox pbImage;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.Button btnForward;
    }
}