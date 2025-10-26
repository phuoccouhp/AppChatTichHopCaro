using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace GiuaKy
{
    public partial class DangKy : Form
    {
        public DangKy()
        {
            InitializeComponent();
        }

        private void DangKy_Load(object sender, EventArgs e)
        {
        }
        private void link_dangnhap_linkclicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DangNhap DangNhap = new DangNhap();
            DangNhap.Show();
            this.Hide();
        }
    }
}
