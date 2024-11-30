using System.Windows.Input;

namespace compact_log_browser.ViewModel.MainView
{
    public partial class MainViewModel
    {
        public class ExitCommand : ICommand
        {
            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                return true;
            }

            public void Execute(object? parameter)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }

    }
}
