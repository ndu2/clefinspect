using System.Windows.Input;
using compact_log_browser.Model;

namespace compact_log_browser.ViewModel.ClefView
{
    public partial class ClefFilterViewModel
    {
        private class CheckAllCommand : ICommand
        {
            private Filter _filter;

            public CheckAllCommand(Filter filter)
            {
                _filter = filter;
                _filter.PropertyChanged += (src, e) => { CanExecuteChanged?.Invoke(this, EventArgs.Empty); };
            }
            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                return _filter.Values.Any(v => !v.Enabled);
            }

            public void Execute(object? parameter)
            {
                foreach (var item in _filter.Values)
                {
                    item.Enabled = true;
                }
            }
        }
    }
}
