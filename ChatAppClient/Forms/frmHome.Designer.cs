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
            pbUserAvatar = new PictureBox();
            pnlSidebar = new Panel();
            flpFriendsList = new FlowLayoutPanel();
            lblFriendsTitle = new Label();
            pnlSearchBox = new Panel();
            lblSearchIcon = new Label();
            txtSearch = new TextBox();
            pnlMain = new Panel();
            lblMainWelcome = new Label();
            btnSettings = new Button();
            lblWelcome = new Label();
            pnlHeader = new Panel();
            ((System.ComponentModel.ISupportInitialize)pbUserAvatar).BeginInit();
            pnlSidebar.SuspendLayout();
            pnlSearchBox.SuspendLayout();
            pnlMain.SuspendLayout();
            pnlHeader.SuspendLayout();
            SuspendLayout();
            // 
            // pbUserAvatar
            // 
            pbUserAvatar.BackColor = Color.WhiteSmoke;
            pbUserAvatar.Location = new Point(19, 12);
            pbUserAvatar.Name = "pbUserAvatar";
            pbUserAvatar.Size = new Size(50, 50);
            pbUserAvatar.SizeMode = PictureBoxSizeMode.StretchImage;
            pbUserAvatar.TabIndex = 1;
            pbUserAvatar.TabStop = false;
            // 
            // pnlSidebar
            // 
            pnlSidebar.BackColor = Color.FromArgb(30, 33, 45);
            pnlSidebar.Controls.Add(flpFriendsList);
            pnlSidebar.Controls.Add(lblFriendsTitle);
            pnlSidebar.Controls.Add(pnlSearchBox);
            pnlSidebar.Dock = DockStyle.Left;
            pnlSidebar.Location = new Point(0, 75);
            pnlSidebar.Name = "pnlSidebar";
            pnlSidebar.Size = new Size(280, 525);
            pnlSidebar.TabIndex = 1;
            // 
            // flpFriendsList
            // 
            flpFriendsList.AutoScroll = true;
            flpFriendsList.BackColor = Color.FromArgb(30, 33, 45);
            flpFriendsList.Dock = DockStyle.Fill;
            flpFriendsList.FlowDirection = FlowDirection.TopDown;
            flpFriendsList.Location = new Point(0, 90);
            flpFriendsList.Name = "flpFriendsList";
            flpFriendsList.Size = new Size(280, 435);
            flpFriendsList.TabIndex = 1;
            flpFriendsList.WrapContents = false;
            // 
            // lblFriendsTitle
            // 
            lblFriendsTitle.AutoSize = true;
            lblFriendsTitle.Dock = DockStyle.Top;
            lblFriendsTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblFriendsTitle.ForeColor = Color.Gainsboro;
            lblFriendsTitle.Location = new Point(0, 40);
            lblFriendsTitle.Name = "lblFriendsTitle";
            lblFriendsTitle.Padding = new Padding(15, 15, 0, 10);
            lblFriendsTitle.Size = new Size(145, 50);
            lblFriendsTitle.TabIndex = 0;
            lblFriendsTitle.Text = "DANH SÁCH ";
            // 
            // pnlSearchBox
            // 
            pnlSearchBox.BackColor = Color.FromArgb(40, 43, 55);
            pnlSearchBox.Controls.Add(lblSearchIcon);
            pnlSearchBox.Controls.Add(txtSearch);
            pnlSearchBox.Dock = DockStyle.Top;
            pnlSearchBox.Location = new Point(0, 0);
            pnlSearchBox.Name = "pnlSearchBox";
            pnlSearchBox.Padding = new Padding(10, 5, 10, 5);
            pnlSearchBox.Size = new Size(280, 40);
            pnlSearchBox.TabIndex = 2;
            // 
            // lblSearchIcon
            // 
            lblSearchIcon.AutoSize = true;
            lblSearchIcon.Font = new Font("Segoe UI Emoji", 10F);
            lblSearchIcon.ForeColor = Color.Gray;
            lblSearchIcon.Location = new Point(8, 10);
            lblSearchIcon.Name = "lblSearchIcon";
            lblSearchIcon.Size = new Size(33, 22);
            lblSearchIcon.TabIndex = 1;
            lblSearchIcon.Text = "🔍";
            // 
            // txtSearch
            // 
            txtSearch.BackColor = Color.FromArgb(40, 43, 55);
            txtSearch.BorderStyle = BorderStyle.None;
            txtSearch.Font = new Font("Segoe UI", 11F);
            txtSearch.ForeColor = Color.White;
            txtSearch.Location = new Point(47, 8);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(220, 25);
            txtSearch.TabIndex = 0;
            txtSearch.Text = "Tìm kiếm...";
            txtSearch.TextChanged += txtSearch_TextChanged;
            txtSearch.Enter += txtSearch_Enter;
            txtSearch.Leave += txtSearch_Leave;
            // 
            // pnlMain
            // 
            pnlMain.BackColor = Color.FromArgb(54, 57, 63);
            pnlMain.Controls.Add(lblMainWelcome);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(280, 75);
            pnlMain.Name = "pnlMain";
            pnlMain.Size = new Size(520, 525);
            pnlMain.TabIndex = 2;
            // 
            // lblMainWelcome
            // 
            lblMainWelcome.Anchor = AnchorStyles.None;
            lblMainWelcome.AutoSize = true;
            lblMainWelcome.Font = new Font("Segoe UI", 16F, FontStyle.Italic);
            lblMainWelcome.ForeColor = Color.Gray;
            lblMainWelcome.Location = new Point(48, 241);
            lblMainWelcome.Name = "lblMainWelcome";
            lblMainWelcome.Size = new Size(449, 37);
            lblMainWelcome.TabIndex = 0;
            lblMainWelcome.Text = "Chọn một người bạn để bắt đầu chat";
            // 
            // btnSettings
            // 
            btnSettings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSettings.BackColor = Color.FromArgb(55, 58, 70);
            btnSettings.Cursor = Cursors.Hand;
            btnSettings.FlatAppearance.BorderSize = 0;
            btnSettings.FlatStyle = FlatStyle.Flat;
            btnSettings.Font = new Font("Segoe UI", 15F);
            btnSettings.ForeColor = Color.White;
            btnSettings.Location = new Point(738, 16);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(45, 45);
            btnSettings.TabIndex = 1;
            btnSettings.Text = "⚙️";
            btnSettings.UseVisualStyleBackColor = false;
            btnSettings.Click += btnSettings_Click;
            // 
            // lblWelcome
            // 
            lblWelcome.AutoSize = true;
            lblWelcome.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblWelcome.ForeColor = Color.White;
            lblWelcome.Location = new Point(75, 22);
            lblWelcome.Name = "lblWelcome";
            lblWelcome.Size = new Size(237, 32);
            lblWelcome.TabIndex = 0;
            lblWelcome.Text = "Chào mừng, [User]!";
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(40, 43, 55);
            pnlHeader.Controls.Add(lblWelcome);
            pnlHeader.Controls.Add(btnSettings);
            pnlHeader.Controls.Add(pbUserAvatar);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(800, 75);
            pnlHeader.TabIndex = 0;
            // 
            // frmHome
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(800, 600);
            Controls.Add(pnlMain);
            Controls.Add(pnlSidebar);
            Controls.Add(pnlHeader);
            Name = "frmHome";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Chat & Caro - Trang Chủ";
            FormClosing += frmHome_FormClosing;
            Load += frmHome_Load;
            ((System.ComponentModel.ISupportInitialize)pbUserAvatar).EndInit();
            pnlSidebar.ResumeLayout(false);
            pnlSidebar.PerformLayout();
            pnlSearchBox.ResumeLayout(false);
            pnlSearchBox.PerformLayout();
            pnlMain.ResumeLayout(false);
            pnlMain.PerformLayout();
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.Label lblFriendsTitle;
        private System.Windows.Forms.FlowLayoutPanel flpFriendsList;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Label lblMainWelcome;
        private System.Windows.Forms.PictureBox pbUserAvatar;
        private Button btnSettings;
        private Label lblWelcome;
        private Panel pnlHeader;
        private System.Windows.Forms.Panel pnlSearchBox;
        private System.Windows.Forms.TextBox txtSearch;
        private Label lblSearchIcon;
    }
}