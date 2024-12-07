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
                this.mainViewModel.PropertyChanged += (o, e) =>
                {
                    if(e.PropertyName == nameof(mainViewModel.ActiveTab))
                    {
                        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                    }
                };
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
