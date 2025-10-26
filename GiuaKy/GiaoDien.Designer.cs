namespace GiuaKy
{
    partial class GiaoDien
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
            this.panel_menu = new System.Windows.Forms.Panel();
            this.panel_giaodien = new System.Windows.Forms.Panel();
            this.btn_caidat = new System.Windows.Forms.PictureBox();
            this.btn_caro = new System.Windows.Forms.PictureBox();
            this.btn_profile = new System.Windows.Forms.PictureBox();
            this.btn_chat = new System.Windows.Forms.PictureBox();
            this.panel_menu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btn_caidat)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btn_caro)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btn_profile)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btn_chat)).BeginInit();
            this.SuspendLayout();
            // 
            // panel_menu
            // 
            this.panel_menu.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_menu.Controls.Add(this.btn_caidat);
            this.panel_menu.Controls.Add(this.btn_caro);
            this.panel_menu.Controls.Add(this.btn_profile);
            this.panel_menu.Controls.Add(this.btn_chat);
            this.panel_menu.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel_menu.Location = new System.Drawing.Point(0, 0);
            this.panel_menu.Name = "panel_menu";
            this.panel_menu.Size = new System.Drawing.Size(112, 1100);
            this.panel_menu.TabIndex = 0;
            // 
            // panel_giaodien
            // 
            this.panel_giaodien.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_giaodien.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_giaodien.Location = new System.Drawing.Point(112, 0);
            this.panel_giaodien.Name = "panel_giaodien";
            this.panel_giaodien.Size = new System.Drawing.Size(1691, 1100);
            this.panel_giaodien.TabIndex = 1;
            this.panel_giaodien.Paint += new System.Windows.Forms.PaintEventHandler(this.panel_giaodien_Paint);
            // 
            // btn_caidat
            // 
            this.btn_caidat.Image = global::GiuaKy.Properties.Resources.Ảnh_màn_hình_2025_10_22_lúc_22_02_04;
            this.btn_caidat.Location = new System.Drawing.Point(3, 349);
            this.btn_caidat.Name = "btn_caidat";
            this.btn_caidat.Size = new System.Drawing.Size(100, 96);
            this.btn_caidat.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.btn_caidat.TabIndex = 3;
            this.btn_caidat.TabStop = false;
            // 
            // btn_caro
            // 
            this.btn_caro.Image = global::GiuaKy.Properties.Resources.images;
            this.btn_caro.Location = new System.Drawing.Point(3, 249);
            this.btn_caro.Name = "btn_caro";
            this.btn_caro.Size = new System.Drawing.Size(100, 94);
            this.btn_caro.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.btn_caro.TabIndex = 2;
            this.btn_caro.TabStop = false;
            // 
            // btn_profile
            // 
            this.btn_profile.Location = new System.Drawing.Point(5, 147);
            this.btn_profile.Name = "btn_profile";
            this.btn_profile.Size = new System.Drawing.Size(100, 96);
            this.btn_profile.TabIndex = 1;
            this.btn_profile.TabStop = false;
            this.btn_profile.Click += new System.EventHandler(this.Profile_Click);
            // 
            // btn_chat
            // 
            this.btn_chat.Image = global::GiuaKy.Properties.Resources.images1;
            this.btn_chat.Location = new System.Drawing.Point(5, 43);
            this.btn_chat.Name = "btn_chat";
            this.btn_chat.Size = new System.Drawing.Size(100, 98);
            this.btn_chat.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.btn_chat.TabIndex = 0;
            this.btn_chat.TabStop = false;
            // 
            // GiaoDien
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1803, 1100);
            this.Controls.Add(this.panel_giaodien);
            this.Controls.Add(this.panel_menu);
            this.Name = "GiaoDien";
            this.Text = "GiaoDien";
            this.panel_menu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.btn_caidat)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btn_caro)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btn_profile)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btn_chat)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel_menu;
        private System.Windows.Forms.Panel panel_giaodien;
        private System.Windows.Forms.PictureBox btn_caidat;
        private System.Windows.Forms.PictureBox btn_caro;
        private System.Windows.Forms.PictureBox btn_profile;
        private System.Windows.Forms.PictureBox btn_chat;
    }
}