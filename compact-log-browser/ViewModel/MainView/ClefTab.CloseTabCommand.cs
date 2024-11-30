using System.Windows.Input;

namespace compact_log_browser.ViewModel.MainView
{
    public partial class ClefTab
    {
        public class CloseTabCommand : ICommand
        {
            private ClefTab clefTab;

            public CloseTabCommand(ClefTab clefTab)
            {
                this.clefTab = clefTab;
            }

            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                return true;
            }

            public void Execute(object? parameter)
            {
                clefTab.Closing?.Invoke();
            }
        }
    }
}
