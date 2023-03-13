using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TMPClient
{
    public partial class Form1 : Form
    {
        Client client;
        public Form1()
        {
            InitializeComponent();
        }

        private string getHost
        {
            get
            {
                return textBox_host.Text;
            }
        }
        private int getPort
        {
            get
            {
                int rez;
                if (int.TryParse(textBox_port.Text, out rez)) return rez;
                return 0;
            }
        }
        private void button_connect_Click(object sender, EventArgs e)
        {
            button_connect.Enabled = false;
            client = new Client(getHost, getPort);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (client == null) {
                button_connect.Enabled = true;
                label_status.Text = string.Empty;
            }
            if (client != null)
            {
                button_connect.Enabled = false;
                label_status.Text = client.getStatus;
            }
        }
    }
}
