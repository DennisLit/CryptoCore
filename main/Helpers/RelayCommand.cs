using System;
using System.Windows.Input;

namespace CryptoCore.Helpers
{ 
        public class RelayCommand : ICommand
        {
            private Action methodToExecute;

            //func that defines if action should be executed right now

            private bool canExecute;


            public RelayCommand(Action action, bool canExecute)
            {
                this.methodToExecute = action;
                this.canExecute = canExecute;
            }

            // if second parameter was not present then command can be executed at any time

            public RelayCommand(Action action) : this(action, true)
            {

            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                methodToExecute();
            }

        }

        public class RelayCommandWithParam : ICommand
        {
            private Action<Object> methodToExecute;


            private bool canExecute;


            public RelayCommandWithParam(Action<Object> action, bool canExecute)
            {
                this.methodToExecute = action;
                this.canExecute = canExecute;
            }

            // if second parameter was not present than command can be executed at any time

            public RelayCommandWithParam(Action<Object> action) : this(action, true)
            {

            }

            public event EventHandler CanExecuteChanged;

            public void RaiseCanExecuteChanged()
            {
                var handler = CanExecuteChanged;
                if (CanExecuteChanged != null)
                    handler(this, EventArgs.Empty);
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                methodToExecute(parameter);
            }


        }
}
