using ndu.ClefInspect.ViewModel.ClefView;

namespace ndu.ClefInspect.ViewModel.MainView
{
    public partial class MainViewModel
    {
        private class ApplyViewDefaultsCommand(MainViewModel mainViewModel) : AbstractRunExecuteWhenActiveTabSetCommand(mainViewModel)
        {
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
                    tab.ClefViewModel.Settings.ShowFiltered = false;
                    tab.ClefViewModel.Settings.ShowHiddenEvents = false;
                    tab.ClefViewModel.Settings.ShowPinned = true;
                    tab.ClefViewModel.Settings.FilterAll = false;
                }
            }
        }
    }
}
