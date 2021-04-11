using DashLink.Net.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Net.Data
{
    public class PacketEventArgs : EventArgs
    {
        public bool IsIncoming { get; set; }
        public IPacket Packet { get; set; }
        public bool Cancel { get; set; }

        public PacketEventArgs(bool isIncoming, IPacket packet)
        {
            IsIncoming = isIncoming;
            Packet = packet;
            Cancel = false;
        }
    }
}
