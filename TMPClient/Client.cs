using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TMPClient
{
    public class Client
    {
        public Socket client;
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        public Client(string adress, int port) {
            IPHostEntry ipHost = Dns.GetHostEntry(adress);
            IPAddress ip = ipHost.AddressList.FirstOrDefault(x=>x.AddressFamily == AddressFamily.InterNetwork);
            IPEndPoint remoteIP = new IPEndPoint(ip, port);

            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.BeginConnect(remoteIP, new AsyncCallback(connectCallback), client);
            connectDone.WaitOne();
            
        }

        private void connectCallback(IAsyncResult ar)
        {
            Socket handle = ar.AsyncState as Socket;
            client.EndConnect(ar);
            connectDone.Set();
        }

        public string getStatus
        {
            get
            {
                if (client != null)
                {
                    return client.Connected ? "connect" : "disconnect";
                }
                return "off";
            }
        }
    }
}
