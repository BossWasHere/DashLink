using DashLink.Net.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Net.Packet
{
    public class PacketButtonPress : IPacket
    {
        public PacketType PacketId => PacketType.ButtonPress;
        public int ButtonId { get; set; }

        public void Receive(IConnectionHandle src)
        {
            ButtonId = src.ReadByte();
        }

        public void HandleReceived(ConnectionInterface ci, IConnectionHandle src)
        {
            ci.ButtonEvent(new ButtonEventArgs(ButtonId, true));
        }

        public void Send(IConnectionHandle dest)
        { }
    }

    public class PacketButtonRelease : IPacket
    {
        public PacketType PacketId => PacketType.ButtonRelease;
        public int ButtonId { get; set; }

        public void Receive(IConnectionHandle src)
        {
            ButtonId = src.ReadByte();
        }

        public void HandleReceived(ConnectionInterface ci, IConnectionHandle src)
        {
            ci.ButtonEvent(new ButtonEventArgs(ButtonId, false));
        }

        public void Send(IConnectionHandle dest)
        { }
    }
}
