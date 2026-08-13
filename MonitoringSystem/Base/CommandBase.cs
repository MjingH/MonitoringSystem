using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonitoringSystem.Base
{
    public class CommandBase : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
           this.DoExcute?.Invoke(parameter);
        }
        public Action<object> DoExcute { get; set; }

        public CommandBase(){}
        public CommandBase(Action<object> action)
        {
            DoExcute = action;
        }
    }
}
