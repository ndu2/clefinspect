using System.Windows;

namespace ndu.ClefInspect.ViewModel.MainView
{
    public partial class MainViewModel
    {
        private class OpenFromClipboardCommand(MainViewModel mainViewModel) : AbstractCanAlwaysExecuteCommand
        {
            public override void Execute(object? parameter)
            {
                if (Clipboard.ContainsText(TextDataFormat.Text))
                {
                    string clipboardText = Clipboard.GetText(TextDataFormat.Text);
                    // Do whatever you need to do with clipboardText
                    mainViewModel.OpenText(clipboardText);
                }
            }
        }
    }
}
