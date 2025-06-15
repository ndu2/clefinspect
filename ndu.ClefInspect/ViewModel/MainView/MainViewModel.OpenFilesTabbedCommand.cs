
namespace ndu.ClefInspect.ViewModel.MainView
{
    public partial class MainViewModel
    {
        public class OpenFilesTabbedCommand(MainViewModel mainViewModel) : AbstractCanAlwaysExecuteCommand
        {
            private readonly MainViewModel mainViewModel = mainViewModel;

            public override void Execute(object? parameter)
            {
                this.mainViewModel.OpenSelectedFilesTabbed();
            }
        }

    }
}
