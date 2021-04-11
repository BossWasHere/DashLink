using System;
using System.Windows.Input;

namespace DashLink
{
    public class SimpleCommand : ICommand
    {
#pragma warning disable 67
        public event EventHandler CanExecuteChanged;
#pragma warning restore 67

        public SimpleCommand()
        {}

        public SimpleCommand(ExecuteCommand command)
        {
            ExecuteDelegate = command;
        }

        public SimpleCommand(ParameterizedExecuteCommand command)
        {
            ParameterizedExecuteDelegate = command;
        }

        public virtual bool CanExecute(object parameter)
        {
            return true;
        }

        public delegate void ExecuteCommand();
        public delegate void ParameterizedExecuteCommand(object parameter);

        public void Execute(object parameter)
        {
            if (ParameterizedExecuteDelegate != null)
            {
                ParameterizedExecuteDelegate(parameter);
            }
            else
            {
                ExecuteDelegate?.Invoke();
            }
        }

        public ExecuteCommand ExecuteDelegate { get; set; }
        public ParameterizedExecuteCommand ParameterizedExecuteDelegate { get; set; }
    }
}
