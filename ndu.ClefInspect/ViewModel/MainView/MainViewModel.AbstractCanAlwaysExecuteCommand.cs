using System.Windows.Input;

namespace ndu.ClefInspect.ViewModel.MainView
{
    public partial class MainViewModel
    {
        public abstract class AbstractCanAlwaysExecuteCommand : ICommand
        {
            // no CS0067 though the event is left unused
            public event EventHandler? CanExecuteChanged { add { } remove { } }

            public bool CanExecute(object? parameter)
            {
                return true;
            }
            public abstract void Execute(object? parameter);
        }
    }
}
