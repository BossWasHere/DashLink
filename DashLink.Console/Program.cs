using DashLink.ConsoleTest.Properties;
using DashLink.Core;
using DashLink.Core.Config;
using DashLink.Net;
using DashLink.Net.Data;
using DashLink.Net.Serial;
using DashLink.Spotify;
using DashLink.Spotify.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DashLink.ConsoleTest
{
    class Program
    {
        private const string Sep = "====================================";
        private const string SpotifyClientId = "12e60f57f4524b75955c6a7c2ca892c7";
        private const string SpotifyCallback = null;
        private const string SpotifyScope = "streaming user-modify-playback-state user-read-playback-state user-read-currently-playing";

        static void Main(string[] args)
        {
            Console.WriteLine(Sep);
            Console.WriteLine(".oO DashLink Console Environment Oo.");
            Console.WriteLine(Sep);
            Console.WriteLine();

            Console.WriteLine("Select connection method:");
            Console.WriteLine("1. Serial");
            Console.WriteLine("2. WebSocket");
            Console.WriteLine("3. Emulate");
            var sel = IntSelection(1, 3);
            Console.WriteLine();

            Console.WriteLine("Enter configuration path:");
            DashLinkConfig config = null;
            do
            {
                var path = Console.ReadLine();
                if (!File.Exists(path))
                {
                    Console.WriteLine("The file specified does not exist. Please provide a valid file path:");
                    continue;
                }
                try
                {
                    config = JsonSerializer.Deserialize<DashLinkConfig>(File.ReadAllText(path));
                }
                catch (Exception e)
                {
                    Console.Write("An error occured while reading the configuration file: ");
                    Console.WriteLine(e);
                    Console.WriteLine("Please provide a valid file path:");
                }
                
            } while (config == null);
            Console.WriteLine();

            Console.WriteLine("Enable Spotify?");
            Console.WriteLine("1. Yes");
            Console.WriteLine("2. Legacy Only");
            Console.WriteLine("3. Yes & Force Setup");
            Console.WriteLine("4. No");

            SpotifyModule spotify = null;
            var sel2 = IntSelection(1, 4);
            if (sel2 != 4)
            {
                Console.WriteLine("Enabling Spotify!");
                spotify = new SpotifyModule
                {
                    SpotifyClient = sel2 == 2 ? null : GetSpotify(sel2 == 3)
                };
            }
            Console.WriteLine();

            ConnectionInterface ci = null;
            switch (sel)
            {
                case 1:
                    ci = GetSerial();
                    break;
                case 2:
                    ci = GetWebSocket();
                    break;
                case 3:
                    ci = GetEmulate();
                    break;
                default:
                    break;
            }

            if (ci != null)
            {
                Run(ci, config, sel != 3, spotify);
            }
        }

        static ConnectionInterface GetSerial()
        {
            SerialConnectionData scd = new SerialConnectionData();

            var ports = SerialInterface.GetPortNames();
            if (ports.Length > 0)
            {
                if (ports.Length > 1)
                {
                    while (scd.SerialPort == null)
                    {
                        Console.WriteLine("Select Serial Port:");
                        int i = 0;
                        for (; i < ports.Length; i++)
                        {
                            Console.WriteLine((i + 1) + ". " + ports[i]);
                        }
                        i += 2;
                        Console.WriteLine((i) + ". Rescan");
                        var sel = IntSelection(1, i);
                        if (sel == i)
                        {
                            ports = SerialInterface.GetPortNames();
                        }
                        else
                        {
                            scd.SerialPort = ports[i - 1];
                        }
                    }
                }
                else
                {
                    scd.SerialPort = ports[0];
                }
            }
            else
            {
                Console.WriteLine("No serial ports detected! Shutting down...");
                Console.ReadLine();
                return null;
            }
            Console.WriteLine("Select Baud Rate:");
            scd.BaudRate = IntSelection(9600, 115200);

            var si = new SerialInterface(scd);

            si.OnPacketEvent += OnPacketEvent;

            Console.WriteLine("==========================\n          Serial\n==========================");

            return si;
        }

        static void OnPacketEvent(object sender, PacketEventArgs e)
        {
            Console.WriteLine("Packet in: " + e.Packet.GetType().Name);
        }

        static ConnectionInterface GetWebSocket()
        {
            //TODO implement
            return null;
        }

        static ConnectionInterface GetEmulate()
        {
            return new ConnectionEmulator();
        }

        static void Run(ConnectionInterface connection, DashLinkConfig config, bool acceptCommands, params IModule[] modules)
        {
            DashLinkHost host = new DashLinkHost(connection, config);

            foreach (var module in modules)
            {
                if (module != null)
                {
                    host.LoadModule(module);
                }
            }
            foreach (var module in host.LoadedModules)
            {
                module.ModuleMessageEvent += Module_ModuleMessageEvent;
            }

            host.Interface.Uid = Environment.MachineName;

            try
            {
                host.Interface.Connect();
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception occured with the connection:");
                Console.WriteLine(e);
            }
            if (acceptCommands)
            {
                Console.WriteLine("To manually set LCD text, type 'lcd1 <text>' or 'lcd2 <text>'");
                Console.WriteLine("To exit, type 'exit'");
            }
            while (host.Interface.IsConnected)
            {
                if (acceptCommands)
                {
                    var x = Console.ReadLine().ToLower();
                    if (x.Equals("stop") || x.Equals("exit") || x.Equals("quit"))
                    {
                        host.Interface.Dispose();
                    }
                    else if (x.StartsWith("lcd1 "))
                    {
                        
                    }
                    else if (x.StartsWith("lcd2 "))
                    {

                    }
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
            Console.WriteLine("Connection Closed. Press any key to continue");
            Console.ReadLine();
        }

        static BaseOAuthClient GetSpotify(bool forceSetup = false)
        {
            var spotifyCode = Settings.Default.SpotifyCode;
            var spotifyRefresh = Settings.Default.SpotifyRefresh;
            var pkce = Settings.Default.SpotifyPKCE;

            BaseOAuthClient client;

            if (string.IsNullOrEmpty(spotifyCode) || forceSetup)
            {

                Console.WriteLine("Spotify not set up. Set up now?");
                Console.WriteLine("1. Set up\n2. Skip");
                if (IntSelection(1, 2) == 1)
                {

                    if (string.IsNullOrEmpty(SpotifyCallback))
                    {
                        Console.WriteLine("Cannot setup Spotify at this time: The redirect link is not available for public use, and you should use your own Spotify account to create a Spotify Application instead.");
                        Console.WriteLine();
                        return null;
                    }

                    Console.WriteLine();
                    Console.WriteLine("Select an authorization flow for Spotify:");
                    Console.WriteLine("1. Authorization Code Flow\n2. Authorization Code Flow with Proof Key for Code Exchange (PKCE)");
                    pkce = IntSelection(1, 2) == 2;

                    if (pkce)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Generating Code Validation pair...");
                        var wClient = new PKCEOAuthClient();

                        Console.WriteLine("You will need to verify the token. A link will open!");
                        string uri = EndpointBuilder.PKCEAuthorizeEndpoint(SpotifyClientId, SpotifyCallback, wClient.GetCodeChallenge(), SpotifyScope, wClient.NewState(), true);
                        Process.Start(uri);

                        Console.WriteLine();
                        Console.WriteLine("Follow the authorization procedure. Copy the text from the browser into your clipboard. (Press enter for next instruction)");
                        Console.ReadLine();
                        Console.WriteLine("The text will be read from your clipboard when you next press enter.");
                        Console.ReadLine();

                        var returnData = Clipboard.GetText();
                        Console.WriteLine("Loaded the following data:");
                        Console.WriteLine(returnData);


                        var split = returnData.Split(':');

                        if (!wClient.CompareState(split[0]))
                        {
                            Console.WriteLine("Error: State mismatch");
                            return null;
                        }
                        Console.WriteLine("States match!");

                        Console.WriteLine();
                        Console.WriteLine("Retrieving access token, please wait...");

                        var t = wClient.ExchangeCode(EndpointBuilder.TokenEndpoint(), split[1], SpotifyClientId, SpotifyCallback);
                        var task = Task.Run(async () => await t);

                        if (task.Result == null)
                        {
                            Console.WriteLine("Could not get token :(");
                            return null;
                        }

                        spotifyCode = task.Result.AccessToken;
                        spotifyRefresh = task.Result.RefreshToken;

                        Console.WriteLine("Done!");

                        client = wClient;
                    }
                    else
                    {
                        Console.WriteLine("Enter OAuth Token:");
                        spotifyCode = Console.ReadLine();

                        client = new OAuthClient(new ClientInformation()
                        {
                            AccessToken = spotifyCode
                        });
                    }

                    Settings.Default.SpotifyCode = spotifyCode;
                    Settings.Default.SpotifyRefresh = spotifyRefresh;
                    Settings.Default.SpotifyPKCE = pkce;

                    Settings.Default.Save();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (pkce)
                {
                    client = new PKCEOAuthClient(new ClientInformation()
                    {
                        AccessToken = spotifyCode,
                        RefreshToken = spotifyRefresh,
                        ClientId = SpotifyClientId,
                        RequestUri = SpotifyCallback,
                        RefreshUri = EndpointBuilder.TokenEndpoint()
                    });
                }
                else
                {
                    client = new OAuthClient(new ClientInformation()
                    {
                        AccessToken = spotifyCode
                    });
                }
            }

            client.OnAuthenticationRefreshEvent += Client_OnAuthenticationRefreshEvent;

            return client;
        }

        private static void Client_OnAuthenticationRefreshEvent(object sender, AuthenticationRefreshEventArgs args)
        {
            if (args.Successful)
            {
                Console.WriteLine("[ Spotify Refresh Success ]");

                Settings.Default.SpotifyCode = args.NewToken.AccessToken;
                Settings.Default.SpotifyRefresh = args.NewToken.RefreshToken;
                Settings.Default.SpotifyPKCE = sender is PKCEOAuthClient;

                Console.Write(Properties.Settings.Default.SpotifyPKCE ? " - Using PKCE " : " - Using Standard");
                Console.WriteLine("Valid for: " + args.NewToken.ExpiresIn + "s");
                Console.WriteLine();

                Properties.Settings.Default.Save();
            }
        }

        private static void Module_ModuleMessageEvent(object sender, ModuleMessageEventArgs args)
        {
            Console.WriteLine("Module Message Event: " + sender.GetType().Name);
            Console.WriteLine(" - " + args.Level + ": " + args.Message);
        }

        static int IntSelection(int min, int max)
        {
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out int v))
                {
                    if (v <= max && v >= min) return v;
                    Console.WriteLine("Value not in range");
                }
                else
                {
                    Console.WriteLine("Value not an integer");
                }
            }
        }
    }
}
