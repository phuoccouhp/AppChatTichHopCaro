namespace ChatAppClient.Forms
{
    partial class frmHome
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
            lblWelcome = new Label();
            pnlSidebar = new Panel();
            flpFriendsList = new FlowLayoutPanel();
            lblFriendsTitle = new Label();
            pnlMain = new Panel();
            lblMainWelcome = new Label();
            pnlHeader.SuspendLayout();
            pnlSidebar.SuspendLayout();
            pnlMain.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(0, 145, 255);
            pnlHeader.Controls.Add(lblWelcome);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Margin = new Padding(3, 4, 3, 4);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(800, 75);
            pnlHeader.TabIndex = 0;
            // 
            // lblWelcome
            // 
            lblWelcome.AutoSize = true;
            lblWelcome.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblWelcome.ForeColor = Color.White;
            lblWelcome.Location = new Point(12, 20);
            lblWelcome.Name = "lblWelcome";
            lblWelcome.Size = new Size(194, 28);
            lblWelcome.TabIndex = 0;
            lblWelcome.Text = "Chào mừng, [User]!";
            // 
            // pnlSidebar
            // 
            pnlSidebar.BackColor = Color.White;
            pnlSidebar.Controls.Add(flpFriendsList);
            pnlSidebar.Controls.Add(lblFriendsTitle);
            pnlSidebar.Dock = DockStyle.Left;
            pnlSidebar.Location = new Point(0, 75);
            pnlSidebar.Margin = new Padding(3, 4, 3, 4);
            pnlSidebar.Name = "pnlSidebar";
            pnlSidebar.Size = new Size(280, 675);
            pnlSidebar.TabIndex = 1;
            // 
            // flpFriendsList
            // 
            flpFriendsList.AutoScroll = true;
            flpFriendsList.Dock = DockStyle.Fill;
            flpFriendsList.FlowDirection = FlowDirection.TopDown;
            flpFriendsList.Location = new Point(0, 44);
            flpFriendsList.Margin = new Padding(3, 4, 3, 4);
            flpFriendsList.Name = "flpFriendsList";
            flpFriendsList.Size = new Size(280, 631);
            flpFriendsList.TabIndex = 1;
            flpFriendsList.WrapContents = false;
            // 
            // lblFriendsTitle
            // 
            lblFriendsTitle.AutoSize = true;
            lblFriendsTitle.Dock = DockStyle.Top;
            lblFriendsTitle.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblFriendsTitle.ForeColor = Color.Gray;
            lblFriendsTitle.Location = new Point(0, 0);
            lblFriendsTitle.Name = "lblFriendsTitle";
            lblFriendsTitle.Padding = new Padding(10, 19, 0, 0);
            lblFriendsTitle.Size = new Size(177, 44);
            lblFriendsTitle.TabIndex = 0;
            lblFriendsTitle.Text = "Danh Sách Bạn Bè";
            // 
            // pnlMain
            // 
            pnlMain.BackColor = Color.FromArgb(229, 221, 213);
            pnlMain.Controls.Add(lblMainWelcome);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(280, 75);
            pnlMain.Margin = new Padding(3, 4, 3, 4);
            pnlMain.Name = "pnlMain";
            pnlMain.Size = new Size(520, 675);
            pnlMain.TabIndex = 2;
            // 
            // lblMainWelcome
            // 
            lblMainWelcome.Anchor = AnchorStyles.None;
            lblMainWelcome.AutoSize = true;
            lblMainWelcome.Font = new Font("Segoe UI", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblMainWelcome.ForeColor = Color.Gray;
            lblMainWelcome.Location = new Point(60, 318);
            lblMainWelcome.Name = "lblMainWelcome";
            lblMainWelcome.Size = new Size(393, 31);
            lblMainWelcome.TabIndex = 0;
            lblMainWelcome.Text = "Chọn một người bạn để bắt đầu chat";
            // 
            // frmHome
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 750);
            Controls.Add(pnlMain);
            Controls.Add(pnlSidebar);
            Controls.Add(pnlHeader);
            Margin = new Padding(3, 4, 3, 4);
            MinimumSize = new Size(700, 613);
            Name = "frmHome";
            Text = "Chat & Caro - Trang Chủ";
            FormClosing += frmHome_FormClosing;
            Load += frmHome_Load;
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlSidebar.ResumeLayout(false);
            pnlSidebar.PerformLayout();
            pnlMain.ResumeLayout(false);
            pnlMain.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblWelcome;
        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.Label lblFriendsTitle;
        private System.Windows.Forms.FlowLayoutPanel flpFriendsList;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Label lblMainWelcome;
    }
}