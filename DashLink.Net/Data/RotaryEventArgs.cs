using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Net.Data
{
    public class RotaryEventArgs : EventArgs
    {
        public bool IsButtonPress { get; set; }
        public bool IsButtonDoublePress { get; set; }
        public bool IsTurn { get; set; }
        public bool IsClockwiseTurn { get; set; }

        public RotaryEventArgs(bool isButtonPress, bool isDoublePress, bool isTurn, bool isClockwiseTurn)
        {
            IsButtonPress = isButtonPress;
            IsButtonDoublePress = isDoublePress;
            IsTurn = isTurn;
            IsClockwiseTurn = isClockwiseTurn;
        }
    }
}
