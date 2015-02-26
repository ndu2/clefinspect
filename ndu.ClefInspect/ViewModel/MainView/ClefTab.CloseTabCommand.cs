using System.Windows.Input;

namespace ndu.ClefInspect.ViewModel.MainView
{
    public partial class ClefTab
    {

        public class CloseTabCommand(ClefTab clefTab) : ICommand
        {
            private readonly ClefTab clefTab = clefTab;

            // no CS0067 though the event is left unused
            public event EventHandler? CanExecuteChanged { add { } remove { } }

            public bool CanExecute(object? parameter)
            {
                return true;
            }

            public void Execute(object? parameter)
            {
                clefTab.DoClose();
            }
        }
    }
}
