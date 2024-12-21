using System.Windows.Input;

namespace ndu.ClefInspect.ViewModel.MainView
{
    public partial class MainViewModel
    {
        public abstract class AbstractRunExecuteWhenActiveTabSetCommand : ICommand
        {
            protected MainViewModel _mainViewModel;

            public AbstractRunExecuteWhenActiveTabSetCommand(MainViewModel mainViewModel)
            {
                _mainViewModel = mainViewModel;
                _mainViewModel.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(MainViewModel.ActiveTab))
                    {
                        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                    }
                };
            }

            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                return _mainViewModel.ActiveTab != null;
            }

            public abstract void Execute(object? parameter);
        }
    }
}
