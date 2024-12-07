using System.Windows.Input;

namespace clef_inspect.ViewModel.MainView
{
    public partial class MainViewModel
    {
        public class CloseTabCommand : ICommand
        {
            private readonly MainViewModel mainViewModel;

            public CloseTabCommand(MainViewModel mainViewModel)
            {
                this.mainViewModel = mainViewModel;
            }

            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                return this.mainViewModel?.ActiveTab?.Close.CanExecute(parameter) ?? false;
            }

            public void Execute(object? parameter)
            {

                this.mainViewModel?.ActiveTab?.Close.Execute(parameter);
            }
        }

    }
}
