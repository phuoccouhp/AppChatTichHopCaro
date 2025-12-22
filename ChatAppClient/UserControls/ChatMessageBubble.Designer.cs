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
            SuspendLayout();
            // 
            // lblMessage
            // 
            lblMessage.AutoSize = true;
            lblMessage.BackColor = Color.Transparent;
            lblMessage.Font = new Font("Microsoft Sans Serif", 12F);
            lblMessage.Location = new Point(0, 7);
            lblMessage.Margin = new Padding(0);
            lblMessage.MaximumSize = new Size(400, 0);
            lblMessage.Name = "lblMessage";
            lblMessage.Size = new Size(93, 25);
            lblMessage.TabIndex = 0;
            lblMessage.Text = "Message";
            lblMessage.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // ChatMessageBubble
            // 
            AutoScaleMode = AutoScaleMode.None;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BackColor = Color.Transparent;
            Controls.Add(lblMessage);
            Margin = new Padding(5, 5, 100, 5);
            MinimumSize = new Size(50, 30);
            Name = "ChatMessageBubble";
            Size = new Size(93, 32);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblMessage;
    }
}