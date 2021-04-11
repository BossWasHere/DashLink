using DashLink.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;

namespace DashLink.ViewModel
{
    class MainWindowVM : IHostViewModel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public Window HostWindow { get; }

        public SimpleCommand ExitCommand { get; }
        public SimpleCommand SpotifySettingsCommand { get; }

        public ObservableCollection<InputCommandBinding> InputData { get; }

        public MainWindowVM(Window host)
        {
            HostWindow = host;
            ExitCommand = new SimpleCommand(() => { App.SafeShutdown(); });
            SpotifySettingsCommand = new SimpleCommand(() => { App.SafeShutdown(); });

            InputData = new ObservableCollection<InputCommandBinding>()
            {
                new InputCommandBinding("Button 1 Press")
            };
        }

        internal void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
