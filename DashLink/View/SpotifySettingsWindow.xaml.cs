using DashLink.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DashLink.View
{
    /// <summary>
    /// Interaction logic for SpotifySettingsWindow.xaml
    /// </summary>
    public partial class SpotifySettingsWindow : Window
    {
        public SpotifySettingsWindow()
        {
            DataContext = new SpotifySettingsVM(this);
            InitializeComponent();
        }
    }
}
