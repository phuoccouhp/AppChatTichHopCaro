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
    public partial class GiaoDien : Form
    {
        private ChatUI chat;
        public GiaoDien()
        {
            InitializeComponent();
            chat = new ChatUI();
            chat.Dock = DockStyle.Fill;
            panel_giaodien.Controls.Clear();
            panel_giaodien.Controls.Add(chat);
        }
        private void panel_giaodien_Paint(object sender, PaintEventArgs e)
        {

        }
        private void Profile_Click(object sender, EventArgs e)
        {
            Profile Profile = new Profile();
            Profile.Show();
            this.Hide(); // Ẩn form hiện tại (nếu m muốn chuyển hẳn sang form Profile)
        }
    }
}
