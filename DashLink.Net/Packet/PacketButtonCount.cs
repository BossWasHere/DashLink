using DashLink.Net.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Net.Packet
{
    public class PacketButtonCount : IPacket
    {
        public PacketType PacketId => PacketType.ButtonCount;

        public int ButtonCount { get; private set; }

        public void Receive(IConnectionHandle src)
        {
            ButtonCount = src.ReadByte();
        }

        public void HandleReceived(ConnectionInterface ci, IConnectionHandle src)
        {
            ci.Buttons = ButtonCount;
        }

        public void Send(IConnectionHandle dest)
        {
            dest.BufferWriteType(PacketId);
        }
    }
}
