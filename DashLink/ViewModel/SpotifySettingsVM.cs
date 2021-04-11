using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;

namespace DashLink.ViewModel
{
    class SpotifySettingsVM : IHostViewModel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public Window HostWindow { get; }

        internal bool EnableIntegration { get; set; }
        internal bool AutoProfile { get; set; }
        internal bool LegacyMode { get; set; }
        internal bool FallbackLegacy { get; set; }
        internal bool FetchSongDetails { get; set; }

        internal string SpotifyAPIToken { get; set; }

        internal SimpleCommand SaveChangesCommand { get; }
        internal SimpleCommand DiscardChangesCommand { get; }
        internal SimpleCommand GetTokenCommand { get; }
        internal SimpleCommand PasteTokenCommand { get; }

        public SpotifySettingsVM(Window host)
        {
            HostWindow = host;
            SaveChangesCommand = new SimpleCommand();
            DiscardChangesCommand = new SimpleCommand();
            GetTokenCommand = new SimpleCommand();
            PasteTokenCommand = new SimpleCommand();
        }

        internal void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
