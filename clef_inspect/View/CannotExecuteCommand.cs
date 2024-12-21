using System.Windows.Input;

namespace clef_inspect.View
{
    public class CannotExecuteCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

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
