namespace ndu.ClefInspect.ViewModel.MainView
{
    public partial class MainViewModel
    {
        public class OpenCommand : AbstractCanAlwaysExecuteCommand
        {
            private readonly MainViewModel mainViewModel;

            public OpenCommand(MainViewModel mainViewModel)
            {
                this.mainViewModel = mainViewModel;
            }

            public override void Execute(object? parameter)
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = @"all supported logs|*.json;*.clef|Json only (.json)|*.json|Clef only (.clef)|*.clef|All|*.*"
                };
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
