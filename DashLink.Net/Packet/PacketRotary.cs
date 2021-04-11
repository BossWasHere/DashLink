using DashLink.Net.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Net.Packet
{
    public class PacketRotary : IPacket
    {
        public PacketType PacketId => PacketType.Rotary;
        public bool IsTurning => (data & 0b00000001) == 1;
        public bool IsRightTurn => (data & 0b00000011) == 3;
        public bool IsPressed => (data & 0b00000100) == 4;
        public bool IsDoublePressed => (data & 0b00001100) == 12;

        private int data;

        public virtual void Receive(IConnectionHandle src)
        {
            data = src.ReadByte();
        }

        public virtual void HandleReceived(ConnectionInterface ci, IConnectionHandle src)
        {
            ci.RotaryEvent(new RotaryEventArgs(IsPressed, IsDoublePressed, IsTurning, IsRightTurn));
        }

        public virtual void Send(IConnectionHandle dest)
        { }
    }

}
