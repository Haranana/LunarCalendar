using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfClient.MVVM
{
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;
        private bool _isRunning;

        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => !_isRunning && (_canExecute?.Invoke() ?? true);

        public async void Execute(object parameter)
        {
            _isRunning = true;
            RaiseCanExecuteChanged();

            try
            {
                await _execute().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                // tu możesz np. Debug.WriteLine(ex) albo event do VM
                System.Diagnostics.Debug.WriteLine(ex);
            }
            finally
            {
                _isRunning = false;
                RaiseCanExecuteChanged();
            }
        }

        public event EventHandler CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
