using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;

namespace telnet_sample
{


    public partial class loader : Form
    {
        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();

        public List<siteitem> siteslists = new List<siteitem>();
     
        public loader()
        {
            InitializeComponent();
            init();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AllocConsole();
            Console.WriteLine("連入 " + textBox1.Text.Replace(" ", "") + " 中..");
            Thread.Sleep(1000);
            this.Visible = false;
            win32api.set_console_desktop_xy(0, 0);
            minimal_telnet mytenlet = new minimal_telnet(textBox1.Text.Replace(" ", ""), int.Parse(textBox2.Text.Replace(" ", "")), comboBox1.SelectedIndex);
            mytenlet.start();
            Close();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Text = siteslists[comboBox2.SelectedIndex].ip;
            textBox2.Text = siteslists[comboBox2.SelectedIndex].port;

        }

        public void init()
        {
            string siteslist = Application.StartupPath + @"\sites.list";
            string sites_c = File.OpenText(siteslist).ReadToEnd();
            List<string> sites_l = sites_c.Split(new char[] { '\n' }).ToList();

            foreach (string i in sites_l)
            {
                string tmp = i;
                tmp = i.Replace(" ", "").Replace("\n", "").Replace("\r", "");
                if (tmp == "")
                    continue;
                List<string> site_inf = i.Split(new char[] { ',' }).ToList();
                if (site_inf.Count != 3)
                    continue;

                siteitem st = new siteitem();
                st.name = site_inf[0];
                st.ip = site_inf[1];
                st.port = site_inf[2];

                siteslists.Add(st);
            }

            comboBox1.SelectedIndex = 3;
            comboBox2.SelectedIndex = 0;
        }
    }

    public class siteitem
    {
        public string name = "";
        public string ip = "";
        public string port = "";
    }

}
