
namespace ndu.ClefInspect.ViewModel.MainView
{
    public partial class MainViewModel
    {
        public class OpenFilesCancelCommand(MainViewModel mainViewModel) : AbstractCanAlwaysExecuteCommand
        {
            private readonly MainViewModel mainViewModel = mainViewModel;

            public override void Execute(object? parameter)
            {
                this.mainViewModel.SelectedFiles = null;
            }
        }

    }
}
