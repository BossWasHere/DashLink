using DashLink.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DashLink
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private static readonly Properties.Settings CurrentSettings = DashLink.Properties.Settings.Default;
        public static DashLinkCore Service => new DashLinkCore();

        public static string Version { get; private set; }

        /* Start Setting Properties */

        internal static bool SetupDone
        {
            get => CurrentSettings.SetupDone;
            set => CurrentSettings.SetupDone = value;
        }
        internal static string Theme
        {
            get => CurrentSettings.Theme;
            set => CurrentSettings.Theme = value;
        }
        internal static bool UseProxy
        {
            get => CurrentSettings.UseProxy;
            set => CurrentSettings.UseProxy = value;
        }
        internal static string ProxyAddress
        {
            get => CurrentSettings.ProxyAddress;
            set => CurrentSettings.ProxyAddress = value;
        }
        internal static int ProxyPort
        {
            get => CurrentSettings.ProxyPort;
            set => CurrentSettings.ProxyPort = value;
        }
        internal static string LinkUID
        {
            get => CurrentSettings.LinkUID;
            set => CurrentSettings.LinkUID = value;
        }
        internal static string LinkKey
        {
            get => CurrentSettings.LinkKey;
            set => CurrentSettings.LinkKey = value;
        }
        internal static bool DoUpdateCheck
        {
            get => CurrentSettings.DoUpdateCheck;
            set => CurrentSettings.DoUpdateCheck = value;
        }
        internal static bool AdminSettingLock
        {
            get => CurrentSettings.AdminSettingLock;
            set => CurrentSettings.AdminSettingLock = value;
        }

        /* End Setting Properties */

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var updates = DoUpdateCheck;
            for (int i = 0; i < e.Args.Length; i++)
            {
                string arg = e.Args[i].ToLower();
                if (arg.Equals("--skipupdate"))
                {
                    updates = false;
                }
                else if (arg.Equals("--forceupdate"))
                {
                    updates = true;
                }
            }
            if (updates)
            {
                Updater.RunUpdateChecker();
            }
        }

        internal static void SafeShutdown()
        {
            Updater.WaitUntilFinished();
            Service.Dispose();
            Current.Shutdown();
        }
    }
}
