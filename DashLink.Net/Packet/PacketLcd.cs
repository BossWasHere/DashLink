using DashLink.Net.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Net.Packet
{
    public abstract class PacketLcdLine : IPacket
    {
        public abstract PacketType PacketId { get; }

        public string Text { get; set; }
        public abstract bool IsTopLine { get; }

        public virtual void Receive(IConnectionHandle src)
        { }

        public virtual void HandleReceived(ConnectionInterface ci, IConnectionHandle src)
        { }

        public virtual void Send(IConnectionHandle dest)
        {
            dest.BufferWriteType(PacketId);
            dest.BufferWriteString(Text);
        }
    }

    public class PacketLcdTopLine : PacketLcdLine
    {
        public override PacketType PacketId => PacketType.LcdTopLine;
        public override bool IsTopLine => true;
    }

    public class PacketLcdBottomLine : PacketLcdLine
    {
        public override PacketType PacketId => PacketType.LcdBottomLine;
        public override bool IsTopLine => false;
    }

    public class PacketLcdLineLength : IPacket
    {
        public PacketType PacketId => PacketType.LcdLineLength;

        public int Length { get; set; }

        public void Receive(IConnectionHandle src)
        {
            Length = src.ReadByte();
        }

        public void HandleReceived(ConnectionInterface ci, IConnectionHandle src)
        {
            ci.LcdLineLength = Length;
        }

        public void Send(IConnectionHandle dest)
        {
            dest.BufferWriteType(PacketId);
        }
    }
}
