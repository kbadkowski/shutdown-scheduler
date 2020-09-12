using System;
using System.Windows.Input;

namespace ShutdownScheduler.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _executeAction;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> executeAction, Func<object, bool> canExecute)
        {
            _canExecute = canExecute;
            _executeAction = executeAction;
        }

        public RelayCommand(Action<object> executeAction)
        {
            _canExecute = x => true;
            _executeAction = executeAction;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
            {
                return true;
            }
            else
            {
                return _canExecute(parameter);
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public void Execute(object parameter)
        {
            _executeAction(parameter);
        }
    }
}
