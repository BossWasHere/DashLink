using DashLink.Net.Data;
using DashLink.Net.Packet;
using System;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace DashLink.Net.Serial
{
    /// <summary>
    /// Subclass of <see cref="DashLink.Net.ConnectionInterface"/> wrapping a <see cref="System.IO.Ports.SerialPort"/> to communicate.
    /// </summary>
    public class SerialInterface : ConnectionInterface, IConnectionHandle
    {
        /// <summary>
        /// Gets the data associated with the current serial connection
        /// </summary>
        public SerialConnectionData Data { get; }
        /// <summary>
        /// Gets the address of the port currently in use
        /// </summary>
        public string CurrentPort { get; private set; }
        /// <summary>
        /// Gets the number of bytes of data in the receive buffer. This wraps the property <see cref="System.IO.Ports.SerialPort.BytesToRead"/>.
        /// </summary>
        public int BytesToRead => port != null ? port.BytesToRead : 0;

        private SerialPort port;
        private readonly MemoryStream outputStream = new MemoryStream();

        /// <summary>
        /// Gets the state of the connection
        /// </summary>
        public override bool IsConnected => port != null && port.IsOpen;

        /// <summary>
        /// Create a new <see cref="DashLink.Net.Serial.SerialInterface"/> using the default options for most Arduino platforms.
        /// <seealso cref="DashLink.Net.Data.SerialConnectionData"/>
        /// </summary>
        public SerialInterface()
        {
            Data = SerialConnectionData.ArduinoDefaults();
        }

        /// <summary>
        /// Create a new <see cref="DashLink.Net.Serial.SerialInterface"/> using the specified options.
        /// </summary>
        /// <param name="data">The connection options to use.</param>
        public SerialInterface(SerialConnectionData data)
        {
            Data = data;
        }

        /// <summary>
        /// Gets an array of serial port names for the current computer. This is an alias for <see cref="System.IO.Ports.SerialPort.GetPortNames"/>.
        /// </summary>
        /// <returns>An array of serial port names for the current computer.</returns>
        public static string[] GetPortNames()
        {
            return SerialPort.GetPortNames();
        }

        /// <summary>
        /// Connects to the serial port and begins communication using the DashLink protocol
        /// </summary>
        /// <exception cref="DashLink.Net.Serial.SerialException">The port is already open or no ports are available.</exception>
        public override void Connect()
        {
            if (IsConnected) throw new SerialException("Port already open on this instance");

            var ports = SerialPort.GetPortNames();
            if (ports.Length < 1)
            {
                throw new SerialException("No serial ports found");
            }

            if (Data.SerialPort != null && ports.Contains(Data.SerialPort))
            {
                CurrentPort = Data.SerialPort;
            }

            if (CurrentPort == null) throw new SerialException("Could not find a DashLink serial port");

            port = Data.UseCustomSerialSettings ? new SerialPort(CurrentPort, Data.GetActualBaudRate(), Data.Parity, Data.GetActualDataBits(), Data.StopBits)
                : new SerialPort(CurrentPort, 9600, Parity.None, 8, StopBits.One);

            port.DataReceived += Port_DataReceived;
            port.Open();

            PostConnection(this);
        }

        /// <summary>
        /// Sends a packet to the serial device.
        /// </summary>
        /// <param name="packet">The packet to send.</param>
        /// <remarks>Always use this instead of <see cref="DashLink.Net.Packet.IPacket.Send(IConnectionHandle)"/>, which is used per implementation of <see cref="DashLink.Net.ConnectionInterface"/>.</remarks>
        public override void SendPacket(IPacket packet)
        {
            packet.Send(this);
            SendBuffer();
        }


        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var id = port.ReadByte();
            HandleRead(id, this);
        }

        protected override void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (IsConnected)
                {
                    port.Close();
                }

                if (disposing)
                {
                    port.Dispose();
                    outputStream.Dispose();
                }

                isDisposed = true;
            }
        }

        /// <summary>
        /// Synchronously reads one byte from the input buffer. This is a wrapper method for <see cref="System.IO.Ports.SerialPort.ReadByte"/>.
        /// </summary>
        /// <param name="allowEOF">When set to false, reaching the end of the stream will yield an exception.</param>
        /// <returns>The byte, cast to an <see cref="int"/>, or -1 if the end of the stream has been read.</returns>
        /// <exception cref="System.InvalidOperationException">The port is not open.</exception>
        /// <exception cref="System.TimeoutException">The operation did not complete before the time-out period ended or no byte was read.</exception>
        /// <exception cref="System.IO.EndOfStreamException">The stream has no more data to read, when it is forbidden to read past the end.</exception>
        public int ReadByte(bool allowEOF = false)
        {
            if (!IsConnected) throw new InvalidOperationException("Cannot read from closed serial port");
            var i = port.ReadByte();
            return allowEOF || i != -1 ? i : throw new EndOfStreamException("Tried to read a byte past end of stream");
        }

        /// <summary>
        /// Reads a number of bytes from the input buffer and writes those bytes into a byte array at the specified offset. This is a wrapper method for <see cref="System.IO.Ports.SerialPort.Read(byte[], int, int)"/>.
        /// </summary>
        /// <param name="buffer">The byte array to write the input to.</param>
        /// <param name="offset">The offset in buffer at which to write the bytes.</param>
        /// <param name="count">The maximum number of bytes to read. Fewer bytes are read if count is greater than the number of bytes in the input buffer.</param>
        /// <returns>The number of bytes read.</returns>
        /// <exception cref="System.ArgumentNullException">The buffer passed is null.</exception>
        /// <exception cref="System.InvalidOperationException">The port is not open.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The offset or count parameters are outside a valid region of the buffer being passed. Either offset or count is less than zero.</exception>
        /// <exception cref="System.ArgumentException">The offset plus count is greater than the length of the buffer.</exception>
        /// <exception cref="System.TimeoutException">No bytes were available to read.</exception>
        public int Read(byte[] buffer, int offset, int count)
        {
            if (!IsConnected) throw new InvalidOperationException("Cannot read from closed serial port");
            return port.Read(buffer, offset, count);
        }

        /// <summary>
        /// Reads the next available bytes from the input buffer and parses it as an ASCII-encoded string.
        /// </summary>
        /// <returns>The string read.</returns>
        /// <exception cref="System.InvalidOperationException">The port is not open.</exception>
        /// <exception cref="System.IO.EndOfStreamException">The first byte (length byte) was not available to be read.</exception>
        /// <exception cref="System.TimeoutException">No bytes were available to read.</exception>
        public string ReadString()
        {
            if (!IsConnected) throw new InvalidOperationException("Cannot read from closed serial port");
            var length = port.ReadByte();
            if (length == -1)
            {
                throw new EndOfStreamException("Tried to read a byte past end of stream");
            }
            else if (length == 0)
            {
                return string.Empty;
            }
            byte[] buffer = new byte[length];
            port.Read(buffer, 0, length);
            return Encoding.ASCII.GetString(buffer);
            
        }

        /// <summary>
        /// Writes a byte to the serial output buffer stream.
        /// </summary>
        /// <param name="b">The byte to write.</param>
        /// <exception cref="System.ObjectDisposedException">The underlying buffer stream is disposed.</exception>
        public void BufferWriteByte(byte b)
        {
            outputStream.WriteByte(b);
        }

        /// <summary>
        /// Writes the packet type byte to the serial output buffer stream.
        /// </summary>
        /// <param name="type">The <see cref="DashLink.Net.Data.PacketType"/> to write.</param>
        /// <exception cref="System.ObjectDisposedException">The underlying buffer stream is disposed.</exception>
        public void BufferWriteType(PacketType type)
        {
            BufferWriteByte((byte)type);
        }

        /// <summary>
        /// Writes a block of bytes to the serial output buffer stream using data read from a buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <exception cref="System.ArgumentNullException">The buffer is null.</exception>
        /// <exception cref="System.ArgumentException">The offset subtracted from the buffer length is less than count.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The offset or count are negative.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="System.ObjectDisposedException">The underlying buffer stream is disposed.</exception>
        public void BufferWrite(byte[] buffer, int offset, int count)
        {
            if (buffer != null) outputStream.Write(buffer, offset, count);
        }

        /// <summary>
        /// Writes a string to the serial output buffer stream using ASCII encoding.
        /// </summary>
        /// <param name="str">The string to write.</param>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="System.ObjectDisposedException">The underlying buffer stream is disposed.</exception>
        public void BufferWriteString(string str)
        {
            if (str != null)
            {
                byte[] data = Encoding.ASCII.GetBytes(str);
                int length = Math.Min(data.Length, 255);
                outputStream.WriteByte((byte)length);
                outputStream.Write(data, 0, length);
            }
        }

        /// <summary>
        /// Collects the bytes in the serial output buffer and sends it to the remote device. This is a wrapper method for <see cref="System.IO.Ports.SerialPort.Write(byte[], int, int)"/>.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">The port is not open.</exception>
        /// <exception cref="System.TimeoutException">The operation did not complete before the time-out period ended.</exception>
        public void SendBuffer()
        {
            if (port == null || !port.IsOpen) throw new InvalidOperationException("Cannot write to closed serial port");
            var data = outputStream.ToArray();
            if (data.Length > 0)
            {
                port.Write(data, 0, data.Length);
            }

            ClearBuffer();
        }

        /// <summary>
        /// Clears the underlying serial output buffer.
        /// </summary>
        public void ClearBuffer()
        {
            outputStream.SetLength(0);
        }
    }
}
