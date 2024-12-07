﻿using System.Windows.Input;

namespace clef_inspect.ViewModel.MainView
{
    public partial class ClefTab
    {

        public class CloseTabCommand : ICommand
        {
            private readonly ClefTab clefTab;

            public CloseTabCommand(ClefTab clefTab)
            {
                this.clefTab = clefTab;
            }

            // no CS0067 though the event is left unused
            public event EventHandler? CanExecuteChanged { add { } remove { } }

            public bool CanExecute(object? parameter)
            {
                return true;
            }

            public void Execute(object? parameter)
            {
                clefTab.DoClose();
            }
        }
    }
}
