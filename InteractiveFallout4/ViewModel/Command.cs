using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace InteractiveFallout4.ViewModel
{
    public class Command : ICommand
    {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public Command(Action<object> execute, Func<object, bool> canExecutet=null)
        {
            this.execute = execute;
            this.canExecute = canExecutet;
        }

        //Возможно ли выполнить функцию
        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute(parameter);
        }

        //Выполнить функцию
        public void Execute(object parameter)
        {
            this.execute(parameter);
        }
    }

    class CommandWithMultParam : ICommand
    {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public CommandWithMultParam(Action<object> execute, Func<object, bool> canExecutet = null)
        {
            this.execute = execute;
            this.canExecute = canExecutet;
        }

        //Возможно ли выполнить функцию
        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute(parameter);
        }

        //Выполнить функцию
        public void Execute(object parameter)
        {

            var values = (object[])parameter;

            this.execute(parameter);

        }
    }
}
