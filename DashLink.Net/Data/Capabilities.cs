using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Net.Data
{
    [Flags]
    public enum Capabilities : byte
    {
        None = 0,
        ButtonInput = 1,
        RotaryTurn = 2,
        RotaryButton = 4,
        LiquidCrystal = 8,
        Host = 64,
        Proxy = 128
    }
}
