using System.Windows.Input;
using clef_inspect.Model;

namespace clef_inspect.ViewModel.ClefView
{
    public partial class ClefFilterViewModel
    {
        private class CheckAllCommand : ICommand
        {
            private readonly Filter _filter;

            public CheckAllCommand(Filter filter)
            {
                _filter = filter;
                _filter.FilterChanged += () => { CanExecuteChanged?.Invoke(this, EventArgs.Empty); };
            }
            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                return _filter.Values.Any(v => !v.Enabled);
            }

            public void Execute(object? parameter)
            {
                _filter.CheckAll();
            }
        }
    }
}
