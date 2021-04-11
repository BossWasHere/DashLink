using DashLink.Net.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Net.Packet
{
    public class PacketHello : IPacket
    {
        public PacketType PacketId => PacketType.Hello;

        public void Receive(IConnectionHandle src)
        { }

        public void HandleReceived(ConnectionInterface ci, IConnectionHandle src)
        {
            src.SendPacket(new PacketHelloResponse() { Payload = ci.Uid });
        }

        public void Send(IConnectionHandle dest)
        {
            dest.BufferWriteType(PacketId);
        }
    }

    public class PacketHelloResponse : IPacket
    {
        public PacketType PacketId => PacketType.HelloResponse;

        public string Payload { get; set; }

        public void Receive(IConnectionHandle src)
        {
            Payload = src.ReadString();
        }

        public void HandleReceived(ConnectionInterface ci, IConnectionHandle src)
        {
            ci.RemoteUid = Payload;
        }

        public void Send(IConnectionHandle dest)
        {
            dest.BufferWriteType(PacketId);
            dest.BufferWriteString(Payload);
        }
    }
}
