using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Net.Data
{
    public enum PacketType : byte
    {
        ButtonCount = 0x10,
        ButtonPress = 0x11,
        ButtonRelease = 0x12,
        Rotary = 0x13,
        LcdLineLength = 0x20,
        LcdTopLine = 0x21,
        LcdBottomLine = 0x22,

        // Net
        NetForward = 0xB0,
        NetForwardError = 0xB1,

        // Shared
        Hello = 0xF0,
        HelloResponse = 0xF1,
        Capabilities = 0xF2,
        CapabilitiesResponse = 0xF3,
        Unknown = 0xFF,
    }
}
