using clef_inspect.ViewModel.ClefView;

namespace clef_inspect.ViewModel.MainView
{
    public partial class MainViewModel
    {
        private class SaveViewDefaultsCommand : AbstractRunExecuteWhenActiveTabSetCommand
        {
            public SaveViewDefaultsCommand(MainViewModel mainViewModel)
                : base(mainViewModel)
            {
            }

            public override void Execute(object? parameter)
            {
                ClefTab? tab = _mainViewModel.ActiveTab;
                if (tab != null)
                {
                    List<string> colsEnabled = new();
                    foreach (ClefViewModel.DataColumnView dcv in tab.ClefViewModel.DataColumns.Where(c => c.Enabled))
                    {
                        colsEnabled.Add(dcv.Header);
                    }
                    _mainViewModel.Settings.SetVisibleColumnDefaults(colsEnabled);
                    List<string> filters = new();
                    foreach (ClefFilterViewModel fvm in tab.ClefViewModel.VisibleFilters)
                    {
                        filters.Add(fvm.Name);
                    }
                    _mainViewModel.Settings.SetVisibleFilterDefaults(filters);
                }
                _mainViewModel.Settings.UserSettings.Write();
            }
        }
    }
}
