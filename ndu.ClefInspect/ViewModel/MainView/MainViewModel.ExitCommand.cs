namespace ndu.ClefInspect.ViewModel.MainView
{
    public partial class MainViewModel
    {
        public class ExitCommand : AbstractCanAlwaysExecuteCommand
        {

            public override void Execute(object? parameter)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }

    }
}
