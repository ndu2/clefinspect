namespace clef_inspect.ViewModel.MainView
{
    public partial class MainViewModel
    {
        public class CloseTabCommand : AbstractRunExecuteWhenActiveTabSetCommand
        {
            public CloseTabCommand(MainViewModel mainViewModel)
                :base(mainViewModel)
            {
            }

            public override void Execute(object? parameter)
            {
                _mainViewModel?.ActiveTab?.Close.Execute(parameter);
            }
        }

    }
}
