using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Net.Serial
{
    public class SerialException : Exception
    {
        public SerialException() : base()
        { }

        public SerialException(string message) : base(message)
        { }
    }
}
