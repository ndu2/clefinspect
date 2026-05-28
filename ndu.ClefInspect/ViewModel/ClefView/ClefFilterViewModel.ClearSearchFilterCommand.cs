using System.ComponentModel;
using System.Windows.Input;

namespace ndu.ClefInspect.ViewModel.ClefView
{
    public partial class ClefFilterViewModel
    {
        public class ClearSearchFilterCommand : ICommand
        {
            private readonly ClefFilterViewModel _vm;

            public ClearSearchFilterCommand(ClefFilterViewModel vm)
            {
                _vm = vm;
                PropertyChangedEventManager.AddHandler(_vm, (s, e) => { CanExecuteChanged?.Invoke(this, EventArgs.Empty); }, nameof(vm.SearchFilter));
            }

            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                return _vm.SearchFilter != string.Empty;
            }

            public void Execute(object? parameter)
            {
                _vm.SearchFilter = string.Empty;
            }
        }

    }
}
