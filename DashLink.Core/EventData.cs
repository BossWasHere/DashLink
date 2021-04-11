using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Core
{
    public static class EventData
    {
        public const string ButtonPressEventStr = "button.press.";
        public const string ButtonReleaseEventStr = "button.release.";

        public const string RotaryTurnAnticlockwiseEventStr = "rotary.left";
        public const string RotaryTurnClockwiseEventStr = "rotary.right";

        public const string RotaryPressEventStr = "rotary.press";
        public const string RotaryDoublePressEventStr = "rotary.doublepress";
    }
}
