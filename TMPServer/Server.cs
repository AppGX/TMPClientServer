using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SocketLib;

namespace TMPServer
{
    internal class Server
    {
        public Socket server;
        protected Task listen;
        public List<StateObject> clients = new List<StateObject>();
        protected ManualResetEvent allDone = new ManualResetEvent(false);
        public Server(string ip, int port = 11000)
        {
            isStop = false;
            IPAddress _ip = IPAddress.Any;
            if (ip == "")
            {
                IPAddress.TryParse(ip, out _ip);
            }
            if (_ip == null)
            {
                _ip = IPAddress.Any;
            }
            IPEndPoint iPEndPoint = new IPEndPoint(_ip, port);

            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(iPEndPoint);
            server.Listen(100);

            listen  = Task.Run(Listen);
        
        }

        private bool isStop = false;
        protected Task Listen()
        {
            while (!isStop)
            {
                if (isStop) {
                    return Task.CompletedTask;
                }
                try
                {
                    allDone.Reset();

                    server.BeginAccept(new AsyncCallback(acceptCallback), server);

                    allDone.WaitOne();

                }
                catch (Exception ex)
                {

                }
            }
            return Task.CompletedTask;

        }

        public void Stop()
        {
            
            foreach (var client in clients)
            {
                client.handler.Shutdown(SocketShutdown.Both);
                client.handler.Close();
                
            }
            clients.Clear();
            allDone.Set();
            // server.Close();
            isStop = true;
            listen.Wait();
            
            server.Close();
            server = null;

            
        }

        private void acceptCallback(IAsyncResult ar)
        {
            allDone.Set();
            Socket server = ar.AsyncState as Socket;
            if (server == null) return;
            try
            {
                Socket client = server.EndAccept(ar);

                StateObject state = new StateObject();
                state.handler = client;

                clients.Add(state);
                client.BeginReceive(state.buffer, 0, StateObject.sizeBuffer, 0, new AsyncCallback(readCallback), state);
            } catch (Exception) { }
        }

        private void readCallback(IAsyncResult ar)
        {
            StateObject state = ar.AsyncState as StateObject;
            Socket client = state.handler;
            try
            {
                int bytesRead = client.EndReceive(ar);

                if (bytesRead >= 0)
                {
                }
            }
            catch (Exception)
            {

            }
        }


        public string getStatus
        {
            get
            {
                if (server != null)
                {
                    return server != null ? "On" : "Off" + " A:" + server.Available.ToString(); 
                }
                return "Off";
            }
        }
    }
}
