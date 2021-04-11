using DashLink.Net.Data;
using DashLink.Net.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Net
{
    public interface IConnectionHandle : IConnectionSender
    {
        int ReadByte(bool allowEOF = false);
        int Read(byte[] buffer, int offset, int count);
        string ReadString();
        void BufferWriteByte(byte b);
        void BufferWriteType(PacketType type);
        void BufferWrite(byte[] buffer, int offset, int count);
        void BufferWriteString(string str);
        void SendBuffer();
        void ClearBuffer();
    }
}
