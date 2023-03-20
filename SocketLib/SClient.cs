using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketLib
{
    public class SClient
    {
        Socket client;
        public event EventHandler onConnected;
        public event EventHandler onDisconnected;
        public event EventHandler<string> onError;
        public event EventHandler<string> onReceive;
        public event EventHandler<int> onSend;

        protected ManualResetEvent conntentComplited = new ManualResetEvent(false);
        protected ManualResetEvent receiveComplited = new ManualResetEvent(false);
        protected ManualResetEvent sendComplited = new ManualResetEvent(false);

        IPEndPoint iPEndPoint;

        public SClient(string ipHost, int port)
        {
            try
            {
                IPHostEntry iPHost = Dns.GetHostEntry(ipHost);
                IPAddress ip = iPHost.AddressList.FirstOrDefault();
                iPEndPoint = new IPEndPoint(ip, port);

                client = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            } catch
            {
                onError?.Invoke(this, $"Dont connect to server {ipHost}:{port}");
            }
        }

        Thread process;
        public void Run()
        {
            // await RunAsync();

            process = new Thread(RunAsync);
            process.Start();
        }

        public void Stop()
        {
            if (client != null)
            {
                if (client.Connected)
                {
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }
            }
            // process.Join();
            process.Abort();
        }

        private async void RunAsync()
        {
            if (client == null) return;
            try
            {
                conntentComplited.Reset();
                client.BeginConnect(iPEndPoint, new AsyncCallback(connectionCallback), client);
                conntentComplited.WaitOne();
            }
            catch
            {
                onError?.Invoke(this, "begin connect");
            }

            await ProcessAsync(client);

        }

        public bool IsConnected
        {
            get
            {
                if (client == null) return false;
                if (!client.Connected) return false;
                try
                {
                    return !(client.Poll(1, SelectMode.SelectRead) && client.Available == 0);
                }
                catch
                {
                    return false;
                }
            }
        }

        private async Task ProcessAsync(Socket handler)
        {
            if (!IsConnected) return;
            StateObject state = new StateObject(handler);
            try
            {
                while (IsConnected)
                {
                    receiveComplited.Reset();
                    handler.BeginReceive(state.buffer, 0, state.SizeBuffer, SocketFlags.None, new AsyncCallback(receiveCallback), state);
                    receiveComplited.WaitOne();
                }
            }
            catch
            {
                if (handler.Connected)
                {
                    handler.Close();
                }
                onError?.Invoke(null, "Error process client");
            }

        }

        public void Send(string msg)
        {
            if (IsConnected)
            {
                byte[] byteData = Encoding.ASCII.GetBytes(msg);
                client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(sendCallback), client);
                // client.BeginSend(state.buffer, 0, )
            }
        }

        private void sendCallback(IAsyncResult ar)
        {
            Socket client = ar.AsyncState as Socket;
            try
            {
                int byteSend = client.EndSend(ar);
                onSend?.Invoke(null, byteSend);
            }
            catch
            {
                onError?.Invoke(null, "Error send");
            }
        }
        private void connectionCallback(IAsyncResult ar)
        {
            conntentComplited.Set();
            Socket client = ar.AsyncState as Socket;
            if (!client.Connected)
            {
                onError?.Invoke(this, "not Connect");
                onDisconnected?.Invoke(this, EventArgs.Empty);

                return;
            }
            client.EndConnect(ar);
            onConnected?.Invoke(this, EventArgs.Empty);
            // throw new NotImplementedException();
        }
        private void receiveCallback(IAsyncResult ar)
        {
            StateObject state = ar.AsyncState as StateObject;
            var client = state.handler as Socket;
            try
            {
                int byteRead = client.EndReceive(ar);
                bool isAlive = !(client.Poll(1, SelectMode.SelectRead) && client.Available == 0);
                if (!isAlive)
                {
                    client.Close();
                    onDisconnected?.Invoke(this, null);
                    receiveComplited.Set();
                    return;
                }
                if (byteRead > 0)
                {
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, byteRead));
                    var responce = state.sb.ToString();
                    onReceive?.Invoke(this, responce);
                    state.sb.Clear();
                }
                receiveComplited.Set();
            }
            catch (Exception e)
            {
                client.Close();
                onError?.Invoke(null, e.Message);
            }
        }
    }
}
