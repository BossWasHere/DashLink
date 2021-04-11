using DashLink.Net.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Net.Packet
{
    public class PacketCapabilities : IPacket
    {
        public PacketType PacketId => PacketType.Capabilities;

        public void Receive(IConnectionHandle src)
        { }

        public void HandleReceived(ConnectionInterface ci, IConnectionHandle src)
        {
            new PacketCapabilitiesResponse() { Capabilities = Capabilities.Host }.Send(src);
        }

        public void Send(IConnectionHandle dest)
        {
            dest.BufferWriteType(PacketId);
        }
    }

    public class PacketCapabilitiesResponse : IPacket
    {
        public PacketType PacketId => PacketType.CapabilitiesResponse;

        public Capabilities Capabilities { get; set; }

        public void Receive(IConnectionHandle src)
        {
            Capabilities = (Capabilities)src.ReadByte();
        }

        public void HandleReceived(ConnectionInterface ci, IConnectionHandle src)
        {
            ci.InterfaceCapabilities = Capabilities;
        }

        public void Send(IConnectionHandle dest)
        {
            dest.BufferWriteType(PacketId);
            dest.BufferWriteByte((byte)Capabilities);
        }
    }
}
