using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiuaKy
{
    partial class ChatUI
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void Initialize()
        {
            this.panel_online = new System.Windows.Forms.Panel();
            this.panel_list = new System.Windows.Forms.Panel();
            this.panel_chat = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // panel_online
            // 
            this.panel_online.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_online.Height = 100;
            this.panel_online.BackColor = System.Drawing.Color.WhiteSmoke;
            // 
            // panel_list
            // 
            this.panel_list.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel_list.Width = 200;
            this.panel_list.BackColor = System.Drawing.Color.Gainsboro;
            // 
            // panel_chat
            // 
            this.panel_chat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_chat.BackColor = System.Drawing.Color.White;
            // 
            // ChatUI
            // 
            this.Controls.Add(this.panel_chat);
            this.Controls.Add(this.panel_list);
            this.Controls.Add(this.panel_online);
            this.Name = "ChatUI";
            this.Size = new System.Drawing.Size(1200, 800);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panel_online;
        private System.Windows.Forms.Panel panel_list;
        private System.Windows.Forms.Panel panel_chat;
    }
}

