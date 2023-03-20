using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketLib.Server
{
    public class SServer
    {
        public EventHandler<StateObject> OnInconnect;
        public EventHandler<StateObject> OnDisconnect;
        public EventHandler<StateObject> OnRead;
        public EventHandler<int> OnSendComplied;
        public EventHandler<string> OnError;

        protected Socket listener;
        protected ManualResetEvent allComplied = new ManualResetEvent(false);

        List<StateObject> clients = new List<StateObject>();
        protected bool _runing = false;
        public bool isRun { get { return _runing; } }

        public int getClients
        {
            get { return clients.Count; }
        }

        protected IPEndPoint localEnIp;
        public SServer(string ip, int port)
        {
            IPAddress _ip;
            if (!IPAddress.TryParse(ip, out _ip))
            {
                _ip = IPAddress.Any;
            }
            localEnIp = new IPEndPoint(_ip, port);
            try
            {
                listener = new Socket(localEnIp.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEnIp);
                //listener.Listen(100);
                // Task.Run(ListenAsync);
            } catch(Exception ex)
            {
                OnError?.Invoke(null, "Error run server: " + ex);
            }
        }

        public void Start(int CountConnection = 100)
        {
            try
            {
                //listener.Bind(localEnIp);
                listener.Listen(CountConnection);
                Task.Run(ListenAsync);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null, "Error run server: " + ex);
            }
        }

        public void Stop()
        {
            _runing = false;
            foreach (var client in clients)
            {
                client.Disconnect();
            }
            clients.Clear();

            if (listener != null)
            {
                allComplied.Set();
                try
                {
                    listener.Shutdown(SocketShutdown.Both);
                    listener.Disconnect(false);
                }
                catch (SocketException) { }
                finally
                {
                    listener.Close();
                }
                listener.Dispose();
                listener = null;
            }
        }

        protected async Task ListenAsync()
        {
            _runing = true;
            while (_runing && listener != null)
            {
                allComplied.Reset();
                listener?.BeginAccept(acceptCallback, listener);
                allComplied.WaitOne();
            }
        }

        public void Send(byte[] data, string id = null, bool ReverseId = false)
        {
            if (id == null)
            {
                foreach(var cl in clients)
                {
                    cl.handler.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(sendCallBack), cl);
                }
                return;
            } 
            if (ReverseId)
            {
                foreach (var cl in clients.Where(w=>w.Id != id).ToList())
                {
                    cl.handler.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(sendCallBack), cl);
                }
                return;
            } else
            {
                foreach (var cl in clients.Where(w => w.Id == id).ToList())
                {
                    cl.handler.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(sendCallBack), cl);
                }
                return;
            }
        }

        protected internal void RemoveConnection(string id)
        {
            var client = clients.FirstOrDefault(c => c.Id == id);
            if (client != null)
            {
                clients.Remove(client);
                client.Disconnect();
            }
        }

        private void acceptCallback(IAsyncResult ar)
        {
            allComplied.Set();
            Socket _server = ar.AsyncState as Socket;
            if (_server != null)
            {
                Socket client;
                try
                {
                    client = _server.EndAccept(ar);
                } catch (SocketException) { return; }
                catch (Exception ex) { OnError?.Invoke(null, "Error client connect: " + ex); return; }

                StateObject state = new StateObject(client);
                client.BeginReceive(state.buffer, 0, state.SizeBuffer, SocketFlags.None, new AsyncCallback(receiveCallback), state);
                OnDisconnect?.Invoke(null, state);
                clients.Add(state);
            }
        }

        private void receiveCallback(IAsyncResult ar)
        {
            StateObject state = ar.AsyncState as StateObject;
            Socket client = state.handler;
            try
            {
                int byteRead = client.EndReceive(ar);
                var isAlive = !(client.Poll(1, SelectMode.SelectRead) && client.Available == 0);

                if ((client.Available == 0 || !client.Connected) && byteRead == 0)
                {
                    OnDisconnect?.Invoke(null, state);
                    RemoveConnection(state.Id);
                    return;
                }

                if (byteRead > 0)
                {
                    // state.buffer.ToString();
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, byteRead));
                    OnRead?.Invoke(null, state);
                    state.sb.Clear();
                    client.BeginReceive(state.buffer, 0, state.SizeBuffer, SocketFlags.None, new AsyncCallback(receiveCallback), state);
                }
                else
                {
                    OnDisconnect?.Invoke(null, state);
                }
            }
            catch
            {
                OnError?.Invoke(null, String.Format("Error recieve client {0}", state.Id.ToString()));
            }
        }

        private void sendCallBack(IAsyncResult ar)
        {
            StateObject state = ar.AsyncState as StateObject;
            Socket client = state.handler;
            try
            {
                int byteRead = client.EndReceive(ar);
                OnSendComplied?.Invoke(state, byteRead);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(state, $"Error send msg: {ex}");
            }
        }
    }
}
