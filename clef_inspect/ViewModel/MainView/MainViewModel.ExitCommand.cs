using System.Windows.Input;

namespace clef_inspect.ViewModel.MainView
{
    public partial class MainViewModel
    {
        public class ExitCommand : ICommand
        {
            // no CS0067 though the event is left unused
            public event EventHandler? CanExecuteChanged { add { } remove { } }

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
