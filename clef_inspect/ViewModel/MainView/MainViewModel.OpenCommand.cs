using System.Windows.Input;

namespace clef_inspect.ViewModel.MainView
{
    public partial class MainViewModel
    {
        public class OpenCommand : ICommand
        {
            private readonly MainViewModel mainViewModel;

            public OpenCommand(MainViewModel mainViewModel)
            {
                this.mainViewModel = mainViewModel;
            }

            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                return true;
            }

            public void Execute(object? parameter)
            {
                var dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.Filter = @"all supported logs|*.json;*.clef|Json only (.json)|*.json|Clef only (.clef)|*.clef|All|*.*";
                bool? result = dialog.ShowDialog();
                // Process open file dialog box results
                if (result == true)
                {
                    this.mainViewModel.OpenFile(dialog.FileName);
                }
            }
        }

    }
}
