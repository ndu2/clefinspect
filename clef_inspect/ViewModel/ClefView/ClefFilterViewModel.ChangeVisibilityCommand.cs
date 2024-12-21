using clef_inspect.Model;
using System.Windows.Input;

namespace clef_inspect.ViewModel.ClefView
{
    public partial class ClefFilterViewModel
    {
        private class ChangeVisibilityCommand : ICommand
        {
            private readonly ClefFilterViewModel _vm;
            private Filter _filter;

            public ChangeVisibilityCommand(ClefFilterViewModel vm, Filter filter)
            {
                _vm = vm;
                _vm.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == nameof(vm.Visible))
                    {
                        NotifyCanExecuteChanged();
                    }
                };
                _filter = filter;
                _filter.FilterChanged += NotifyCanExecuteChanged;
            }

            private void NotifyCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }

            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? visible)
            {
                if (visible == null)
                {
                    return false;
                }
                if (visible is bool b)
                {
                    if (b != _vm.Visible)
                    {
                        return b == true || _filter.Values.All(v => v.Enabled);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            public void Execute(object? hidden)
            {
                if (!CanExecute(hidden))
                {
                    throw new InvalidOperationException("cannot execute command");
                }
                if (hidden is bool b)
                {
                    _vm.Visible = b;
                }
            }
        }
    }
}
