using DashLink.Net.Data;
using DashLink.Net.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Net.WebSocket
{
    public class WebSocketInterface : ConnectionInterface, IConnectionHandle
    {

        public override void SendPacket(IPacket packet)
        {
            throw new NotImplementedException();
        }

        public void BufferWrite(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public void BufferWriteByte(byte b)
        {
            throw new NotImplementedException();
        }

        public void BufferWriteString(string str)
        {
            throw new NotImplementedException();
        }

        public void BufferWriteType(PacketType type)
        {
            throw new NotImplementedException();
        }

        public void ClearBuffer()
        {
            throw new NotImplementedException();
        }

        public override void Connect()
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            throw new NotImplementedException();
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public int ReadByte(bool allowEOF = false)
        {
            throw new NotImplementedException();
        }

        public string ReadString()
        {
            throw new NotImplementedException();
        }

        public void SendBuffer()
        {
            throw new NotImplementedException();
        }
    }
}
