namespace ndu.ClefInspect.ViewModel.MainView
{
    public partial class MainViewModel
    {
        public class CloseTabCommand(MainViewModel mainViewModel) : AbstractRunExecuteWhenActiveTabSetCommand(mainViewModel)
        {
            public override void Execute(object? parameter)
            {
                _mainViewModel?.ActiveTab?.Close.Execute(parameter);
            }
        }

    }
}
