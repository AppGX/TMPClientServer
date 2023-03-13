using System;
using System.Net.Sockets;

namespace SocketLib
{
    public class StateObject
    {
        public string Id = Guid.NewGuid().ToString();
        public Socket handler = null;
        public const int sizeBuffer = 1024;
        public byte[] buffer = new byte[sizeBuffer];
    }
}