using DashLink.Net;
using DashLink.Net.Data;
using DashLink.Net.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.ConsoleTest
{
    public class ConnectionEmulator : ConnectionInterface
    {

        public override void Connect()
        {
            IsConnected = true;

            Console.WriteLine("==========================\n   Connection Emulator\n==========================");
            Console.WriteLine("Enter commands in standard event form i.e. 'button.press.1'");
            Console.WriteLine("To exit, type 'exit'");

            bool u = false;
            while (IsConnected)
            {
                Console.Write("> ");
                var x = Console.ReadLine().ToLower();

                if (!IsConnected || x.Equals("stop") || x.Equals("exit") || x.Equals("quit"))
                {
                    IsConnected = false;
                    return;
                }

                var split = x.Split('.');
                switch (split[0])
                {
                    case "button":
                        {
                            if (split.Length < 3)
                            {
                                u = true;
                                break;
                            }
                            switch (split[1])
                            {
                                case "press":
                                    {
                                        if (int.TryParse(split[2], out int t)) ButtonEvent(new ButtonEventArgs(t, true));
                                        break;
                                    }
                                case "release":
                                    {
                                        if (int.TryParse(split[2], out int t)) ButtonEvent(new ButtonEventArgs(t, false));
                                        break;
                                    }
                                default:
                                    u = true;
                                    break;
                            }
                            break;
                        }
                    case "rotary":
                        {
                            if (split.Length == 1)
                            {
                                u = true;
                                break;
                            }
                            switch (split[1])
                            {
                                case "left":
                                    {
                                        RotaryEvent(new RotaryEventArgs(false, false, true, false));
                                        break;
                                    }
                                case "right":
                                    {
                                        RotaryEvent(new RotaryEventArgs(false, false, true, true));
                                        break;
                                    }
                                case "press":
                                    {
                                        RotaryEvent(new RotaryEventArgs(true, false, false, false));
                                        break;
                                    }
                                case "doublepress":
                                    {
                                        RotaryEvent(new RotaryEventArgs(true, true, false, false));
                                        break;
                                    }
                                default:
                                    u = true;
                                    break;
                            }
                            break;
                        }
                    default:
                        u = true;
                        break;
                }

                Console.WriteLine(u ? "< unknown" : "< ok");
                u = false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            IsConnected = false;
        }

        public override void SendPacket(IPacket packet)
        {
            Console.WriteLine("< Send Packet " + packet.GetType().Name);
        }
    }
}
