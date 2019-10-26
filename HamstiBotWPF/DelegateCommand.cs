using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HamstiBotWPF
{
    class DelegateCommand : ICommand
    {
        //These delegates store methods to be called that contains the body of the Execute and CanExecue methods
        //for each particular instance of DelegateCommand.
        private readonly Predicate<object> canExecute;
        private readonly Action<object> execute;

        //CanExecute and Execute come from ICommand
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        //Two Constructors, for convenience and clean code - often you won't need CanExecute
        public DelegateCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            this.canExecute = canExecute;
            this.execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null ? true : canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            if (!CanExecute(parameter))
                return;
            execute(parameter);
        }

        /// <summary>
        /// Not a part of ICommand, but commonly added so you can trigger a manual refresh on the result of CanExecute.
        /// </summary>
        //public void RaiseCanExecuteChanged()
        //{
        //    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        //}
    }
}

