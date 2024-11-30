using System.ComponentModel;
using System.Windows.Input;
using compact_log_browser.ViewModel.ClefView;
using compact_log_browser.Model;

namespace compact_log_browser.ViewModel.MainView
{
    public partial class ClefTab : INotifyPropertyChanged
    {

        public ClefTab(Clef clef, Settings settings)
        {
            ClefViewModel = new ClefViewModel(clef, settings);
            Close = new CloseTabCommand(this);
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        public event Action? Closing;

        public ICommand Close { get; set; }

        public string Name => ClefViewModel.Clef.FileName;

        public ClefViewModel ClefViewModel { get; }

    }
}