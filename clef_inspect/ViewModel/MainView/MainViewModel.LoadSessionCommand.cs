namespace clef_inspect.ViewModel.MainView
{
    public partial class MainViewModel
    {
        public class LoadSessionCommand : AbstractCanAlwaysExecuteCommand
        {
            private MainViewModel _mainViewModel;

            public LoadSessionCommand(MainViewModel mainViewModel)
            {
                _mainViewModel = mainViewModel;
            }

            public override void Execute(object? parameter)
            {
                List<ClefTab> tabs = new(_mainViewModel.ClefTabs);
                foreach (ClefTab c in tabs)
                {
                    c.Close.Execute(null);
                }
                List<string> files = _mainViewModel.Settings.GetSessionFiles();
                _mainViewModel.OpenFiles(files.ToArray());
            }
        }
    }
}
