using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Net.Data
{
    public struct SerialConnectionData
    {
        public string SerialPort { get; set; }

        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }
        public Parity Parity { get; set; }
        public bool UseCustomSerialSettings { get; set; }

        public static SerialConnectionData ArduinoDefaults()
        {
            return new SerialConnectionData()
            {
                BaudRate = 9600,
                DataBits = 8,
                StopBits = StopBits.One,
                Parity = Parity.None,
                UseCustomSerialSettings = true
            };
        }

        public int GetActualBaudRate()
        {
            return !UseCustomSerialSettings && BaudRate < 1 ? 9600 : BaudRate;
        }

        public int GetActualDataBits()
        {
            return !UseCustomSerialSettings && DataBits < 1 ? 8 : DataBits;
        }
    }
}
