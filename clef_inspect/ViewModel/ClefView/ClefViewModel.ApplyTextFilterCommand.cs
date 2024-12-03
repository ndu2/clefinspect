using System.Windows.Input;

namespace clef_inspect.ViewModel.ClefView
{
    public partial class ClefViewModel
    {
        private class ApplyTextFilterCommand : ICommand
        {
            private ClefViewModel clefViewModel;

            public ApplyTextFilterCommand(ClefViewModel clefViewModel)
            {
                this.clefViewModel = clefViewModel;
            }

            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                return true;
            }

            public void Execute(object? parameter)
            {
                clefViewModel.Reload();
            }
        }
    }
}
