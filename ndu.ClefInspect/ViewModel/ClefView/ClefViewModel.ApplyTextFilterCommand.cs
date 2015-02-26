using System.Windows.Input;

namespace ndu.ClefInspect.ViewModel.ClefView
{
    public partial class ClefViewModel
    {
        private class ApplyTextFilterCommand(ClefViewModel clefViewModel) : ICommand
        {
            private readonly ClefViewModel clefViewModel = clefViewModel;

            // no CS0067 though the event is left unused
            public event EventHandler? CanExecuteChanged { add { } remove { } }

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
