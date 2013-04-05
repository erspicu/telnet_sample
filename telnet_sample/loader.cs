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
        const int SWP_NOSIZE = 0x0001;

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetConsoleWindow();


        public List<siteitem> siteslists = new List<siteitem>();
     
        public loader()
        {
            InitializeComponent();
            init();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Rectangle resolution = Screen.PrimaryScreen.Bounds;
            int height=resolution.Size.Height;
            int width=resolution.Size.Width;
            
            //MessageBox.Show(  width.ToString() );

            int x=0, y=0;

            if (comboBox1.SelectedIndex == 4)
            {
                x = (width - 1317) / 2;
                y = (height - 1009) / 2;
            }
            if (comboBox1.SelectedIndex == 3)
            {
                x = (width - 1157) / 2;
                y = (height - 909) / 2;
            }
            if (comboBox1.SelectedIndex == 2)
            {
                x = (width - 997) / 2;
                y = (height - 784) / 2;
            }
            if (comboBox1.SelectedIndex == 1)
            {
                x = (width - 837) / 2;
                y = (height - 659) / 2;
            }
            if (comboBox1.SelectedIndex == 0)
            {
                x = (width - 667) / 2;
                y = (height - 534) / 2;
            }



            this.Visible = false;

            AllocConsole();
            IntPtr MyConsole = GetConsoleWindow();
            SetWindowPos(MyConsole, 0, x, y, 0, 0, SWP_NOSIZE);

            Console.WriteLine("連入 " + textBox1.Text.Replace(" ", "") + " 中..");
            Thread.Sleep(1000);
            Console.WindowLeft = 0;
                        
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

            comboBox1.SelectedIndex = 2;
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
