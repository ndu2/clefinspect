
namespace ndu.ClefInspect.ViewModel.MainView
{
    public partial class MainViewModel
    {
        public class OpenCommand(MainViewModel mainViewModel) : AbstractCanAlwaysExecuteCommand
        {
            private readonly MainViewModel mainViewModel = mainViewModel;

            public override void Execute(object? parameter)
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = @"all supported logs|*.json;*.clef|Json only (.json)|*.json|Clef only (.clef)|*.clef|All|*.*",
                    Multiselect = true
                };
                bool? result = dialog.ShowDialog();
                // Process open file dialog box results
                if (result == true)
                {
                    if (dialog.FileNames.Length == 1)
                    {
                        this.mainViewModel.OpenFiles(dialog.FileNames);
                    }
                    else
                    {
                        this.mainViewModel.SelectedFiles = dialog.FileNames;
                    }
                }
            }
        }
    }
}
