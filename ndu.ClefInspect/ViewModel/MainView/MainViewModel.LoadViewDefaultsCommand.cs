using ndu.ClefInspect.ViewModel.ClefView;

namespace ndu.ClefInspect.ViewModel.MainView
{
    public partial class MainViewModel
    {
        private class ApplyViewDefaultsCommand : AbstractRunExecuteWhenActiveTabSetCommand
        {
            public ApplyViewDefaultsCommand(MainViewModel mainViewModel)
                : base(mainViewModel)
            {
            }

            public override void Execute(object? parameter)
            {
                ClefTab? tab = _mainViewModel.ActiveTab;
                if (tab != null)
                {
                    foreach (ClefViewModel.DataColumnView dcv in tab.ClefViewModel.DataColumns)
                    {
                        dcv.Enabled = _mainViewModel.Settings.IsVisibleColumnByDefault(dcv.Header);
                    }
                    foreach (ClefFilterViewModel fvm in tab.ClefViewModel.Filters)
                    {
                        fvm.Visible = _mainViewModel.Settings.IsVisibleFilterByDefault(fvm.Name);
                    }
                }
            }
        }
    }
}
