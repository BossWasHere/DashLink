using DashLink.Core.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace DashLink.Data
{
    public class InputCommandBinding
    {
        public string InputName { get; }
        public ObservableCollection<Command> Commands { get; }

        public InputCommandBinding(string name)
        {
            InputName = name;
            Commands = new ObservableCollection<Command>();
        }

        public InputCommandBinding(string name, ICollection<Command> commands)
        {
            InputName = name;
            Commands = new ObservableCollection<Command>(commands);
        }
    }
}
