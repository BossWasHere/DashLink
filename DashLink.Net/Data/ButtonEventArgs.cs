using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Net.Data
{
    public class ButtonEventArgs : EventArgs
    {
        public int ButtonId { get; set; }
        public bool IsPressed { get; set; }

        public ButtonEventArgs(int buttonId, bool isPressed)
        {
            ButtonId = buttonId;
            IsPressed = isPressed;
        }
    }
}
