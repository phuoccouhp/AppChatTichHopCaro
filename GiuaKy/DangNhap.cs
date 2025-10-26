using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GiuaKy
{
    public partial class DangNhap : Form
    {
        public DangNhap()
        {
            InitializeComponent();
        }

        private void DangNhap_Load(object sender, EventArgs e)
        {

        }
        private void btn_dangnhap_click(object sender, EventArgs e)
        {
            GiaoDien GiaoDien = new GiaoDien();
            GiaoDien.Show();
            this.Hide(); 
        }
        private void link_dangky_linkclicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DangKy DangKy = new DangKy();
            DangKy.Show();
            this.Hide();
        }
        private void link_quenmk_linkclicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            QuenMatKhau QuenMatKhau = new QuenMatKhau();
            QuenMatKhau.Show();
            this.Hide();
        }
    }
}
