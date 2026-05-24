using System.IO;

namespace ndu.ClefInspect.ViewModel.MainView
{
    public partial class MainViewModel
    {
        public class ClearRecentFilesCommand(MainViewModel mainViewModel) : AbstractCanAlwaysExecuteCommand
        {
            private readonly MainViewModel _mainViewModel = mainViewModel;

            public override void Execute(object? parameter)
            {
                _mainViewModel.Settings.ClearRecentFiles();
            }
        }
    }
}
