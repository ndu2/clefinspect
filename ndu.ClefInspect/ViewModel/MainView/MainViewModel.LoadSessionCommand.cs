namespace ndu.ClefInspect.ViewModel.MainView
{
    public partial class MainViewModel
    {
        public class LoadSessionCommand(MainViewModel mainViewModel) : AbstractCanAlwaysExecuteCommand
        {
            private readonly MainViewModel _mainViewModel = mainViewModel;

            public override void Execute(object? parameter)
            {
                List<ClefTab> tabs = new(_mainViewModel.ClefTabs);
                foreach (ClefTab c in tabs)
                {
                    c.Close.Execute(null);
                }
                IList<string> files = _mainViewModel.Settings.GetSessionFiles();
                _mainViewModel.OpenFiles([.. files]);
            }
        }
    }
}
