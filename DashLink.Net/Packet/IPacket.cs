using DashLink.Net.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Net.Packet
{
    public interface IPacket
    {
        PacketType PacketId { get; }
        void Send(IConnectionHandle dest);
        void Receive(IConnectionHandle src);
        void HandleReceived(ConnectionInterface ci, IConnectionHandle src);
    }
}
