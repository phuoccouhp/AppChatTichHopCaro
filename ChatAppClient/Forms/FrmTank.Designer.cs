namespace ChatAppClient.Forms
{
    partial class FrmTank
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.PictureBox tank;
        private System.Windows.Forms.Timer gameTimer;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tank = new System.Windows.Forms.PictureBox();
            this.gameTimer = new System.Windows.Forms.Timer(this.components);

            ((System.ComponentModel.ISupportInitialize)(this.tank)).BeginInit();
            this.SuspendLayout();

            // Tank
            this.tank.BackColor = System.Drawing.Color.LimeGreen;
            this.tank.Size = new System.Drawing.Size(50, 50);
            this.tank.Location = new System.Drawing.Point(375, 275);
            this.tank.Tag = "tank";

            // Timer
            this.gameTimer.Interval = 20;
            this.gameTimer.Enabled = true;

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.tank);
            this.BackColor = System.Drawing.Color.Black;
            this.DoubleBuffered = true;
            this.KeyPreview = true;   // ⭐ FIX: để form nhận Keyboard
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FrmTank";
            this.Text = "Simple Tank Game";

            ((System.ComponentModel.ISupportInitialize)(this.tank)).EndInit();
            this.ResumeLayout(false);
        }
        #endregion
    }
}
