using DashLink.View;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace DashLink.Core
{
    public class DashLinkCore : IDisposable
    {
        public DashLinkCore()
        {

        }

        internal void OpenSpotifySettingsWindow(Window owner)
        {
            SpotifySettingsWindow ssw = new SpotifySettingsWindow
            {
                Owner = owner,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            ssw.ShowDialog();


        }

        public void Dispose()
        {
            
        }
    }
}
