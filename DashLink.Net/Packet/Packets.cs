using DashLink.Net.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Net.Packet
{
    public static class Packets
    {
        private static readonly Dictionary<PacketType, IPacketType<IPacket>> types;
        static Packets()
        {
            types = typeof(Packets).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => (IPacketType<IPacket>)f.GetValue(null)).ToDictionary(x => x.PacketId);
        }

        public static IPacketType<IPacket> GetPacket(int packetId)
        {
            try
            {
                return GetPacket((PacketType)packetId);
            }
            catch
            {
                return null;
            }
        }

        public static IPacketType<IPacket> GetPacket(PacketType packetId)
        {
            if (types.TryGetValue(packetId, out IPacketType<IPacket> value))
            {
                return value;
            }
            return null;
        }

        public static readonly PacketType<PacketButtonCount> ButtonCount = new PacketType<PacketButtonCount>(PacketType.ButtonCount);
        public static readonly PacketType<PacketButtonPress> ButtonPress = new PacketType<PacketButtonPress>(PacketType.ButtonPress);
        public static readonly PacketType<PacketButtonRelease> ButtonRelease = new PacketType<PacketButtonRelease>(PacketType.ButtonRelease);
        public static readonly PacketType<PacketRotary> Rotary = new PacketType<PacketRotary>(PacketType.Rotary);
        public static readonly PacketType<PacketLcdLineLength> LcdLineLength = new PacketType<PacketLcdLineLength>(PacketType.LcdLineLength);
        public static readonly PacketType<PacketLcdTopLine> LcdTopLine = new PacketType<PacketLcdTopLine>(PacketType.LcdTopLine);
        public static readonly PacketType<PacketLcdBottomLine> LcdBottomLine = new PacketType<PacketLcdBottomLine>(PacketType.LcdBottomLine);
        public static readonly PacketType<PacketHello> Hello = new PacketType<PacketHello>(PacketType.Hello);
        public static readonly PacketType<PacketHelloResponse> HelloResponse = new PacketType<PacketHelloResponse>(PacketType.HelloResponse);
        public static readonly PacketType<PacketCapabilities> Capabilities = new PacketType<PacketCapabilities>(PacketType.Capabilities);
        public static readonly PacketType<PacketCapabilitiesResponse> CapabilitiesResponse = new PacketType<PacketCapabilitiesResponse>(PacketType.CapabilitiesResponse);
    }

    public interface IPacketType<out T> where T : IPacket
    {
        PacketType PacketId { get; }
        T GetBase();
    }

    public class PacketType<T> : IPacketType<T> where T : IPacket, new()
    {
        public PacketType PacketId { get; }

        public PacketType(PacketType packetId)
        {
            PacketId = packetId;
        }

        public T GetBase() => new T();

    }
}
