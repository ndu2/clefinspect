using ndu.ClefInspect.ViewModel.ClefView;
using System.Windows;

namespace ndu.ClefInspect.ViewModel.MainView
{
    public partial class MainViewModel
    {
        private class SaveViewDefaultsCommand(MainViewModel mainViewModel) : AbstractRunExecuteWhenActiveTabSetCommand(mainViewModel)
        {
            public override void Execute(object? parameter)
            {
                ClefTab? tab = _mainViewModel.ActiveTab;
                if (tab != null)
                {
                    List<string> colsEnabled = [];
                    foreach (ClefViewModel.DataColumnView dcv in tab.ClefViewModel.DataColumns.Where(c => c.Enabled))
                    {
                        colsEnabled.Add(dcv.Header);
                    }
                    _mainViewModel.Settings.SetVisibleColumnDefaults(colsEnabled);
                    List<string> filters = [];
                    foreach (ClefFilterViewModel fvm in tab.ClefViewModel.VisibleFilters)
                    {
                        filters.Add(fvm.Name);
                    }
                    _mainViewModel.Settings.SetVisibleFilterDefaults(filters);
                }
                try
                {
                    _mainViewModel.Settings.UserSettings.Write();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Error saving settings", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
