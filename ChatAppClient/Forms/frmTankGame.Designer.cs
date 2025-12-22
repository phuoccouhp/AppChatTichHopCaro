namespace ChatAppClient.Forms
{
    partial class frmTankGame
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblScore;
        private System.Windows.Forms.Label lblHits;
        private System.Windows.Forms.Button btnRematch;
        private System.Windows.Forms.Button btnExit;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblScore = new System.Windows.Forms.Label();
            this.lblHits = new System.Windows.Forms.Label();
            this.btnRematch = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblScore
            // 
            this.lblScore.AutoSize = true;
            this.lblScore.BackColor = System.Drawing.Color.Transparent;
            this.lblScore.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblScore.ForeColor = System.Drawing.Color.White;
            this.lblScore.Location = new System.Drawing.Point(12, 9);
            this.lblScore.Name = "lblScore";
            this.lblScore.Size = new System.Drawing.Size(111, 21);
            this.lblScore.TabIndex = 0;
            this.lblScore.Text = "Score: 0";
            // 
            // lblHits
            // 
            this.lblHits.AutoSize = true;
            this.lblHits.BackColor = System.Drawing.Color.Transparent;
            this.lblHits.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblHits.ForeColor = System.Drawing.Color.White;
            this.lblHits.Location = new System.Drawing.Point(12, 40);
            this.lblHits.Name = "lblHits";
            this.lblHits.Size = new System.Drawing.Size(106, 21);
            this.lblHits.TabIndex = 1;
            this.lblHits.Text = "Hits: 0";
            // 
            // btnRematch
            // 
            this.btnRematch.BackColor = System.Drawing.Color.FromArgb(0, 145, 255);
            this.btnRematch.FlatAppearance.BorderSize = 0;
            this.btnRematch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRematch.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnRematch.ForeColor = System.Drawing.Color.White;
            this.btnRematch.Location = new System.Drawing.Point(300, 280);
            this.btnRematch.Name = "btnRematch";
            this.btnRematch.Size = new System.Drawing.Size(120, 40);
            this.btnRematch.TabIndex = 2;
            this.btnRematch.Text = "Chơi Lại";
            this.btnRematch.UseVisualStyleBackColor = false;
            this.btnRematch.Visible = false;
            // 
            // btnExit
            // 
            this.btnExit.BackColor = System.Drawing.Color.FromArgb(220, 53, 69);
            this.btnExit.FlatAppearance.BorderSize = 0;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnExit.ForeColor = System.Drawing.Color.White;
            this.btnExit.Location = new System.Drawing.Point(440, 280);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(120, 40);
            this.btnExit.TabIndex = 3;
            this.btnExit.Text = "Thoát";
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Visible = false;
            // 
            // frmTankGame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkGreen;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnRematch);
            this.Controls.Add(this.lblHits);
            this.Controls.Add(this.lblScore);
            this.Name = "frmTankGame";
            this.Text = "Tank Game";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}