using System.Windows.Input;

namespace ndu.ClefInspect.ViewModel.ClefView
{
    public partial class ClefViewModel
    {
        private class HideAllEventIdsCommand(ClefViewModel clefViewModel) : ICommand
        {
            private readonly ClefViewModel clefViewModel = clefViewModel;

            // no CS0067 though the event is left unused
            public event EventHandler? CanExecuteChanged { add { } remove { } }

            public bool CanExecute(object? parameter)
            {
                return parameter is string || parameter is ISet<string>;
            }

            public void Execute(object? parameter)
            {
                if(parameter is string evtId)
                {
                    clefViewModel.Settings.IgnoredEventId.Add(evtId);
                }
                if (parameter is ISet<string> set)
                {
                    HashSet<string> ids = new HashSet<string>(clefViewModel.Settings.IgnoredEventId);
                    List<string> newIds = new List<string>();
                    foreach (string s in set)
                    {
                        if (!ids.Contains(s))
                        {
                            clefViewModel.Settings.IgnoredEventId.Add(s);
                        }
                    }
                }
            }
        }
    }
}
