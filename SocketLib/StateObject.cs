using System;
using System.Net.Sockets;
using System.Text;

namespace SocketLib
{
    public class StateObject
    {
        public string Id = Guid.NewGuid().ToString();
        public Socket handler = null;
        // public const int sizeBuffer = 1024;
        public byte[] buffer;
        public StringBuilder sb = new StringBuilder();

        public StateObject(Socket socket, int sizeBuffer = 1024)
        {
            handler = socket;
            buffer = new byte[sizeBuffer];
        }

        public int SizeBuffer
        {
            get { return buffer.Length; }
        }

        public void Disconnect()
        {
            if (handler != null)
            {
                try
                {
                    if (handler.Connected) { handler.Disconnect(false); }
                }
                finally
                {
                    handler.Close();

                }
                handler.Dispose();
                handler = null;
            }
        }
    }
}