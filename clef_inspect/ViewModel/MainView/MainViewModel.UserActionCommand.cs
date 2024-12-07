using clef_inspect.ViewModel.ClefView;
using System.Windows.Input;

namespace clef_inspect.ViewModel.MainView
{
    public partial class MainViewModel
    {
        public class UserActionCommand : ICommand
        {
            private readonly MainViewModel _vm;
            private readonly ClefViewModel.UserAction _action;

            public UserActionCommand(MainViewModel vm, ClefViewModel.UserAction action)
            {
                _vm = vm;
                _action = action;
            }

            // no CS0067 though the event is left unused
            public event EventHandler? CanExecuteChanged { add { } remove { } }

            public bool CanExecute(object? parameter) => true;

            public void Execute(object? parameter) => _vm.ActiveTab?.ClefViewModel?.DoUserAction(_action);
        }

    }
}
