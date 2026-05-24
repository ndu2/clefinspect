using System.Windows.Input;

namespace ndu.ClefInspect.ViewModel.ClefView
{
    public partial class ClefViewModel
    {
        public class ColumnsMenuCommand : ICommand
        {
            private readonly ClefViewModel _vm;

            public ColumnsMenuCommand(ClefViewModel mainViewModel)
            {
                _vm = mainViewModel;
                _vm.DataColumns.CollectionChanged += (s, e) => { CanExecuteChanged?.Invoke(this, EventArgs.Empty); };
            }

            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                return _vm.DataColumns.Count > 0;
            }

            public void Execute(object? parameter)
            {
                return;
            }
        }
    }
}
