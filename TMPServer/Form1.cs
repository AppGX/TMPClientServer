using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SocketLib;
using SocketLib.Server;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TMPServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private SServer server;

        public List<object> Logs = new List<object>();

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
            server = new SServer(getHost, getPort);
            
            server.Start();

            server.OnDisconnect += (object _sender, StateObject state) => { 
                // Disconnect client
            };
            server.OnInconnect += (object _sender, StateObject state) =>
            {
                // Client connect
            };
            server.OnRead += (object _sender, StateObject state) =>
            {
                // Read msg client
            };
            server.OnError += (object _sender, string msg) =>
            {
                // Error server
                var item = new
                {
                    Client = (_sender as StateObject).Id,
                    Type = "Error",
                    Date = DateTime.Now.ToShortTimeString(),
                    Text = msg,
                };
                Logs.Add(item);
            };


        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            server.Stop();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            // label_status.Text = (bool)server?.isRun ? "Runing" : "Stop";
            button_start.Enabled = true;
            button_stop.Enabled = false;
            var server_Status = "Not start";
            var server_ClientCount = "0";

            if (server != null)
            {
                if (server.isRun)
                {
                    server_Status = "Runing";
                    server_ClientCount = server.getClients.ToString();
                    button_start.Enabled = !server.isRun;
                    button_stop.Enabled = server.isRun;
                }
            }
            label_status.Text = server_Status;
            clients_count.Text = server_ClientCount;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox_port.Text = getPort.ToString();
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            // client_list.Items.Clear();
            comboBox1.Items.Clear();
            comboBox1.Items.Add("Any");
            textBox_host.Text = ipHost.AddressList.Where(w => !w.IsIPv6LinkLocal).First().ToString();
            foreach (var item in ipHost.AddressList)
            {
                if (!(item.IsIPv6LinkLocal | item.IsIPv6Multicast)) { comboBox1.Items.Add(item.ToString()); }
            }
            comboBox1.SelectedIndex = comboBox1.Items.Count > 0 ? 1 : 0;
        }

        private void button_stop_Click_1(object sender, EventArgs e)
        {
            // Stop server
            server.Stop();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem == "") return;
            switch (comboBox2.SelectedItem.ToString().ToLower())
            {
                case "logs": break;
                case "print": break;
            }
        }
    }
}
