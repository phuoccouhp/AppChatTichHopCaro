namespace ChatAppClient.Forms
{
    partial class frmCaroGame
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
            pnlHeader = new Panel();
            lblTurn = new Label();
            pnlBoard = new Panel();
            pnlControls = new Panel();
            btnNewGame = new CustomControls.RoundedButton();
            pnlHeader.SuspendLayout();
            pnlControls.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.White;
            pnlHeader.Controls.Add(lblTurn);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Margin = new Padding(3, 4, 3, 4);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(602, 62);
            pnlHeader.TabIndex = 0;
            // 
            // lblTurn
            // 
            lblTurn.AutoSize = true;
            lblTurn.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTurn.ForeColor = Color.FromArgb(0, 145, 255);
            lblTurn.Location = new Point(12, 14);
            lblTurn.Name = "lblTurn";
            lblTurn.Size = new Size(170, 28);
            lblTurn.TabIndex = 0;
            lblTurn.Text = "Đến lượt bạn (X)";
            // 
            // pnlBoard
            // 
            pnlBoard.BackColor = Color.White;
            pnlBoard.Location = new Point(0, 62);
            pnlBoard.Margin = new Padding(3, 4, 3, 4);
            pnlBoard.Name = "pnlBoard";
            pnlBoard.Size = new Size(601, 751);
            pnlBoard.TabIndex = 1;
            // 
            // pnlControls
            // 
            pnlControls.BackColor = Color.FromArgb(240, 240, 240);
            pnlControls.Controls.Add(btnNewGame);
            pnlControls.Dock = DockStyle.Bottom;
            pnlControls.Location = new Point(0, 814);
            pnlControls.Margin = new Padding(3, 4, 3, 4);
            pnlControls.Name = "pnlControls";
            pnlControls.Size = new Size(602, 75);
            pnlControls.TabIndex = 2;
            // 
            // btnNewGame
            // 
            btnNewGame.BackColor = Color.Transparent;
            btnNewGame.BorderRadius = 20;
            btnNewGame.ButtonColor = Color.Gray;
            btnNewGame.FlatStyle = FlatStyle.Flat;
            btnNewGame.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNewGame.ForeColor = Color.Transparent;
            btnNewGame.Location = new Point(478, 12);
            btnNewGame.Margin = new Padding(3, 4, 3, 4);
            btnNewGame.Name = "btnNewGame";
            btnNewGame.Size = new Size(112, 50);
            btnNewGame.TabIndex = 0;
            btnNewGame.Text = "Chơi Lại";
            btnNewGame.TextColor = Color.White;
            btnNewGame.UseVisualStyleBackColor = false;
            btnNewGame.Click += BtnNewGame_Click;
            // 
            // frmCaroGame
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(602, 889);
            Controls.Add(pnlControls);
            Controls.Add(pnlBoard);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(3, 4, 3, 4);
            Name = "frmCaroGame";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Game Cờ Caro";
            Load += frmCaroGame_Load;
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlControls.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTurn;
        private System.Windows.Forms.Panel pnlBoard;
        private System.Windows.Forms.Panel pnlControls;
        private CustomControls.RoundedButton btnNewGame;
    }
}