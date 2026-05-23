using System.Windows.Input;

namespace ndu.ClefInspect.ViewModel.ClefView
{
    public partial class ClefViewModel
    {
        public enum SearchDirection { Undefined, Up, Down };
        private class SearchCommand(ClefViewModel clefViewModel) : ICommand
        {
            private readonly ClefViewModel clefViewModel = clefViewModel;

            // no CS0067 though the event is left unused
            public event EventHandler? CanExecuteChanged { add { } remove { } }

            public bool CanExecute(object? parameter)
            {
                return parameter is SearchDirection;
            }

            public void Execute(object? parameter)
            {
                if(parameter is SearchDirection dir)
                {
                    clefViewModel.BrowseTo(dir, clefViewModel.TextSearch);
                }
            }
        }
    }
}