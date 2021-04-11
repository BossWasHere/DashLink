using DashLink.Net.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Net
{
    public interface IConnectionSender
    {
        void SendPacket(IPacket packet);
    }
}
