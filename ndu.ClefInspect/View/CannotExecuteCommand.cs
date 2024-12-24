using System.Windows.Input;

namespace ndu.ClefInspect.View
{
    public class CannotExecuteCommand : ICommand
    {
        // no CS0067 though the event is left unused
        public event EventHandler? CanExecuteChanged { add { } remove { } }

        public bool CanExecute(object? parameter)
        {
            return false;
        }

        public void Execute(object? parameter)
        {
            return;
        }
    }
}
