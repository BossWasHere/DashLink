using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace DashLink.ViewModel
{
    interface IHostViewModel
    {
        Window HostWindow { get; }
    }
}
