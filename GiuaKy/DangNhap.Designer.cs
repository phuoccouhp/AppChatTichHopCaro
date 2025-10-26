namespace GiuaKy
{
    partial class DangNhap
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
            this.lbl_dangnhap = new System.Windows.Forms.Label();
            this.link_quenmk = new System.Windows.Forms.LinkLabel();
            this.link_dangky = new System.Windows.Forms.LinkLabel();
            this.btn_dangnhap = new RoundedButton();
            this.txt_matkhau = new UnderlinedTextBox();
            this.lbl_matkhau = new System.Windows.Forms.Label();
            this.txt_nhap = new UnderlinedTextBox();
            this.lbl_nhap = new System.Windows.Forms.Label();
            this.check_luu = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // lbl_dangnhap
            // 
            this.lbl_dangnhap.AutoSize = true;
            this.lbl_dangnhap.Font = new System.Drawing.Font("Microsoft Sans Serif", 17F);
            this.lbl_dangnhap.Location = new System.Drawing.Point(207, 26);
            this.lbl_dangnhap.Name = "lbl_dangnhap";
            this.lbl_dangnhap.Size = new System.Drawing.Size(296, 53);
            this.lbl_dangnhap.TabIndex = 0;
            this.lbl_dangnhap.Text = "DANG NHAP";
            // 
            // link_quenmk
            // 
            this.link_quenmk.AutoSize = true;
            this.link_quenmk.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.link_quenmk.Location = new System.Drawing.Point(114, 692);
            this.link_quenmk.Name = "link_quenmk";
            this.link_quenmk.Size = new System.Drawing.Size(179, 26);
            this.link_quenmk.TabIndex = 3;
            this.link_quenmk.TabStop = true;
            this.link_quenmk.Text = "Quen mat khau ?";
            this.link_quenmk.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.link_quenmk_linkclicked);
            // 
            // link_dangky
            // 
            this.link_dangky.AutoSize = true;
            this.link_dangky.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.link_dangky.Location = new System.Drawing.Point(363, 692);
            this.link_dangky.Name = "link_dangky";
            this.link_dangky.Size = new System.Drawing.Size(186, 26);
            this.link_dangky.TabIndex = 4;
            this.link_dangky.TabStop = true;
            this.link_dangky.Text = "Dang ky tai khoan";
            this.link_dangky.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.link_dangky_linkclicked);
            // 
            // btn_dangnhap
            // 
            this.btn_dangnhap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btn_dangnhap.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_dangnhap.FlatAppearance.BorderSize = 0;
            this.btn_dangnhap.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_dangnhap.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btn_dangnhap.ForeColor = System.Drawing.Color.White;
            this.btn_dangnhap.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(255)))));
            this.btn_dangnhap.Location = new System.Drawing.Point(183, 570);
            this.btn_dangnhap.Name = "btn_dangnhap";
            this.btn_dangnhap.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btn_dangnhap.Size = new System.Drawing.Size(308, 92);
            this.btn_dangnhap.TabIndex = 8;
            this.btn_dangnhap.Text = "DANG NHAP";
            this.btn_dangnhap.UseVisualStyleBackColor = false;
            this.btn_dangnhap.Click += new System.EventHandler(this.btn_dangnhap_click);
            // 
            // txt_matkhau
            // 
            this.txt_matkhau.BackColor = System.Drawing.SystemColors.Control;
            this.txt_matkhau.FocusLineColor = System.Drawing.Color.DeepSkyBlue;
            this.txt_matkhau.Gap = 1;
            this.txt_matkhau.LineColor = System.Drawing.Color.Gray;
            this.txt_matkhau.Location = new System.Drawing.Point(121, 338);
            this.txt_matkhau.Name = "txt_matkhau";
            this.txt_matkhau.Size = new System.Drawing.Size(430, 86);
            this.txt_matkhau.TabIndex = 7;
            // 
            // lbl_matkhau
            // 
            this.lbl_matkhau.AutoSize = true;
            this.lbl_matkhau.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            this.lbl_matkhau.Location = new System.Drawing.Point(117, 312);
            this.lbl_matkhau.Name = "lbl_matkhau";
            this.lbl_matkhau.Size = new System.Drawing.Size(108, 24);
            this.lbl_matkhau.TabIndex = 2;
            this.lbl_matkhau.Text = "MAT KHAU";
            // 
            // txt_nhap
            // 
            this.txt_nhap.BackColor = System.Drawing.SystemColors.Control;
            this.txt_nhap.FocusLineColor = System.Drawing.Color.DeepSkyBlue;
            this.txt_nhap.Gap = 1;
            this.txt_nhap.LineColor = System.Drawing.Color.Gray;
            this.txt_nhap.Location = new System.Drawing.Point(121, 165);
            this.txt_nhap.Name = "txt_nhap";
            this.txt_nhap.Size = new System.Drawing.Size(430, 86);
            this.txt_nhap.TabIndex = 5;
            // 
            // lbl_nhap
            // 
            this.lbl_nhap.AutoSize = true;
            this.lbl_nhap.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            this.lbl_nhap.Location = new System.Drawing.Point(117, 138);
            this.lbl_nhap.Name = "lbl_nhap";
            this.lbl_nhap.Size = new System.Drawing.Size(240, 24);
            this.lbl_nhap.TabIndex = 1;
            this.lbl_nhap.Text = "USERNAME / EMAIL / SDT";
            // 
            // check_luu
            // 
            this.check_luu.AutoSize = true;
            this.check_luu.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.check_luu.Location = new System.Drawing.Point(227, 472);
            this.check_luu.Name = "check_luu";
            this.check_luu.Size = new System.Drawing.Size(226, 30);
            this.check_luu.TabIndex = 9;
            this.check_luu.Text = "LUU DANG NHAP";
            this.check_luu.UseVisualStyleBackColor = true;
            // 
            // DangNhap
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(678, 798);
            this.Controls.Add(this.check_luu);
            this.Controls.Add(this.btn_dangnhap);
            this.Controls.Add(this.txt_matkhau);
            this.Controls.Add(this.txt_nhap);
            this.Controls.Add(this.link_dangky);
            this.Controls.Add(this.link_quenmk);
            this.Controls.Add(this.lbl_matkhau);
            this.Controls.Add(this.lbl_nhap);
            this.Controls.Add(this.lbl_dangnhap);
            this.Name = "DangNhap";
            this.Text = "DangNhap";
            this.Load += new System.EventHandler(this.DangNhap_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_dangnhap;
        private System.Windows.Forms.LinkLabel link_quenmk;
        private System.Windows.Forms.LinkLabel link_dangky;
        private RoundedButton btn_dangnhap;
        private UnderlinedTextBox txt_matkhau;
        private System.Windows.Forms.Label lbl_matkhau;
        private UnderlinedTextBox txt_nhap;
        private System.Windows.Forms.Label lbl_nhap;
        private System.Windows.Forms.CheckBox check_luu;
    }
}