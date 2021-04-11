using DashLink.Net.Data;
using DashLink.Net.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Net
{
    public abstract class ConnectionInterface : IConnectionSender, IDisposable
    {
        public delegate void PacketEventHandler(object sender, PacketEventArgs e);
        public event PacketEventHandler OnPacketEvent;

        public delegate void ButtonEventHandler(object sender, ButtonEventArgs e);
        public event ButtonEventHandler OnButtonEvent;

        public delegate void RotaryEventHandler(object sender, RotaryEventArgs e);
        public event RotaryEventHandler OnRotaryEvent;

        ~ConnectionInterface()
        {
            Dispose(false);
        }

        protected bool isDisposed = false;

        public Capabilities InterfaceCapabilities { get; set; }
        public string Uid { get; set; }
        public string RemoteUid { get; set; }

        public int LcdLineLength { get; set; }
        public int Buttons { get; set; }

        public virtual bool IsConnected { get; protected set; }
        public abstract void Connect();
        public void Dispose()
        {
            Dispose(true);
        }
        protected abstract void Dispose(bool disposing);

        protected virtual void PostConnection(IConnectionHandle handle)
        {
            handle.SendPacket(new PacketHello());
            handle.SendPacket(new PacketCapabilities());
            handle.SendPacket(new PacketLcdLineLength());
        }

        public abstract void SendPacket(IPacket packet);

        protected void HandleRead(int packetId, IConnectionHandle source)
        {
            if (packetId == -1) return;

            IPacketType<IPacket> pt = Packets.GetPacket(packetId);
            if (pt == null) return;
            var packet = pt.GetBase();

            packet.Receive(source);
            var pea = new PacketEventArgs(true, packet);
            OnPacketEvent?.Invoke(this, pea);

            if (pea.Cancel) return;
            packet.HandleReceived(this, source);
        }

        protected internal void ButtonEvent(ButtonEventArgs args)
        {
            OnButtonEvent?.Invoke(this, args);
        }

        protected internal void RotaryEvent(RotaryEventArgs args)
        {
            OnRotaryEvent?.Invoke(this, args);
        }
    }
}
