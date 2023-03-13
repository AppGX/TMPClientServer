using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TMPServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Server server;

        protected string getHost
        {
            get
            {
                return textBox_host.Text;
            }
        }
        protected int getPort
        {
            get
            {
                int port;
                if (int.TryParse(textBox_port.Text, out port)) return port;
                return 11000;
            }
        }
        private void button_start_Click(object sender, EventArgs e)
        {
            server = new Server(getHost, getPort);
            
        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            server.Stop();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label_status.Text = server?.getStatus ?? "";
        }
    }
}
