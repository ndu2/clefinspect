using System.Windows.Input;

namespace clef_inspect.ViewModel.ClefView
{
    public partial class ClefViewModel
    {
        private class ClearTextFilterCommand : ICommand
        {
            private readonly ClefViewModel clefViewModel;

            public ClearTextFilterCommand(ClefViewModel clefViewModel)
            {
                this.clefViewModel = clefViewModel;
                this.clefViewModel.PropertyChanged += (sender, args) =>
                {
                    if (sender == clefViewModel && args.PropertyName == nameof(clefViewModel.TextFilter))
                    {
                        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                    }
                };
            }

            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                return clefViewModel?.TextFilter?.Length > 0;
            }

            public void Execute(object? parameter)
            {
                clefViewModel.TextFilter = "";
                clefViewModel.ApplyTextFilter.Execute(this);
            }
        }
    }
}
