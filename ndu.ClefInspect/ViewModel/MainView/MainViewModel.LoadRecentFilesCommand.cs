using System.IO;

namespace ndu.ClefInspect.ViewModel.MainView
{
    public partial class MainViewModel
    {
        public class LoadRecentFilesCommand(MainViewModel mainViewModel) : AbstractCanAlwaysExecuteCommand
        {
            private readonly MainViewModel _mainViewModel = mainViewModel;

            public override void Execute(object? parameter)
            {
                IList<string> files = _mainViewModel.Settings.RecentFiles;
                foreach (string file in files)
                {
                    List<FileInfo> fileInfo = file.Split(MainViewSettings.Fsep).Select(s => new FileInfo(s)).ToList();
                    _mainViewModel.OpenFile(fileInfo);
                }
            }
        }
    }
}
