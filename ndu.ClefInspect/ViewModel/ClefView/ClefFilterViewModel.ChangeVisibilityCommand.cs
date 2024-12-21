using ndu.ClefInspect.Model;
using System.ComponentModel;
using System.Windows.Input;

namespace ndu.ClefInspect.ViewModel.ClefView
{
    public partial class ClefFilterViewModel
    {
        private class ChangeVisibilityCommand : ICommand
        {
            private readonly ClefFilterViewModel _vm;
            private readonly Filter _filter;

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
                PropertyChangedEventManager.AddHandler(filter, (s, e) => { NotifyCanExecuteChanged(); }, nameof(filter.AcceptsAll));
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
                        return b == true || _filter.AcceptsAll;
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
