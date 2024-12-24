using ndu.ClefInspect.ViewModel.ClefView;

namespace ndu.ClefInspect.ViewModel.MainView
{
    public partial class MainViewModel
    {
        public class UserActionCommand : AbstractRunExecuteWhenActiveTabSetCommand
        {
            private readonly ClefViewModel.UserAction _action;

            public UserActionCommand(MainViewModel vm, ClefViewModel.UserAction action)
                : base(vm)
            {
                _action = action;
            }

            public override void Execute(object? parameter) => _mainViewModel.ActiveTab?.ClefViewModel?.DoUserAction(_action);
        }

    }
}
