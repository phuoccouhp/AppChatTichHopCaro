namespace ChatAppClient.UserControls
{
    partial class ChatMessageBubble
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblMessage = new Label();
            btnForward = new Button();
            SuspendLayout();
            // 
            // lblMessage
            // 
            lblMessage.AutoSize = true;
            lblMessage.BackColor = Color.Transparent;
            lblMessage.Font = new Font("Segoe UI", 11F);
            lblMessage.Location = new Point(12, 8);
            lblMessage.Margin = new Padding(0);
            lblMessage.MaximumSize = new Size(400, 0);
            lblMessage.Name = "lblMessage";
            lblMessage.Size = new Size(93, 25);
            lblMessage.TabIndex = 0;
            lblMessage.Text = "";
            lblMessage.TextAlign = ContentAlignment.TopLeft;
            lblMessage.Padding = new Padding(0);
            lblMessage.Visible = false; // ✅ [FIX] Ẩn hoàn toàn vì dùng custom painting
            // 
            // btnForward
            // 
            btnForward.BackColor = Color.FromArgb(100, 100, 100);
            btnForward.FlatAppearance.BorderSize = 0;
            btnForward.FlatStyle = FlatStyle.Flat;
            btnForward.Font = new Font("Segoe UI Emoji", 10F);
            btnForward.ForeColor = Color.White;
            btnForward.Location = new Point(0, 0);
            btnForward.Name = "btnForward";
            btnForward.Size = new Size(28, 28);
            btnForward.TabIndex = 1;
            btnForward.Text = "➡️";
            btnForward.UseVisualStyleBackColor = false;
            btnForward.Visible = false;
            btnForward.Cursor = Cursors.Hand;
            // 
            // ChatMessageBubble
            // 
            AutoScaleMode = AutoScaleMode.None;
            AutoSize = false; // ✅ [FIX] Tắt AutoSize để control size được tính toán chính xác
            BackColor = Color.Transparent;
            Controls.Add(btnForward);
            Controls.Add(lblMessage);
            Margin = new Padding(5, 5, 5, 5);
            MinimumSize = new Size(50, 30);
            Name = "ChatMessageBubble";
            Size = new Size(100, 50);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Button btnForward;
    }
}