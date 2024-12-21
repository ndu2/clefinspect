using System.Windows.Input;

namespace clef_inspect.ViewModel.ClefView
{
    public partial class ClefViewModel
    {
        public class FiltersMenuCommand : ICommand
        {
            private readonly ClefViewModel _vm;

            public FiltersMenuCommand(ClefViewModel mainViewModel)
            {
                _vm = mainViewModel;
                _vm.Filters.CollectionChanged += (s, e) => { CanExecuteChanged?.Invoke(this, EventArgs.Empty); };
            }

            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                return _vm.Filters.Count > 0;
            }

            public void Execute(object? parameter)
            {
                return;
            }
        }
    }
}
