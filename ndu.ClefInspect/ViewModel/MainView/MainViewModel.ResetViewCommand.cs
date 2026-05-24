using ndu.ClefInspect.ViewModel.ClefView;

namespace ndu.ClefInspect.ViewModel.MainView
{
    public partial class MainViewModel
    {
        private class ResetViewCommand(MainViewModel mainViewModel) : AbstractRunExecuteWhenActiveTabSetCommand(mainViewModel)
        {
            public override void Execute(object? parameter)
            {
                _mainViewModel.ActiveTab?.ClefViewModel.Settings.ResetView();
            }
        }
    }
}
