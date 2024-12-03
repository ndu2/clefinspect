using System.ComponentModel;
using System.Windows.Input;
using compact_log_browser.ViewModel.ClefView;
using compact_log_browser.Model;

namespace compact_log_browser.ViewModel.MainView
{
    public partial class ClefTab : INotifyPropertyChanged
    {

        public ClefTab(string fileName, Settings settings)
        {
            ClefViewModel = new ClefViewModel(fileName, settings);
            Close = new CloseTabCommand(this);

            ClefViewModel.Clef.PropertyChanged += (sender, e) =>
            {
                if(e.PropertyName == nameof(Clef.AutoUpdate))
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoUpdate)));
                }
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void DoClose()
        {
            Closing?.Invoke();
            ClefViewModel.DoClose();
        }

        public event Action? Closing;

        public ICommand Close { get; set; }

        public string Name => ClefViewModel.Clef.FileName;

        public ClefViewModel ClefViewModel { get; }

        public bool AutoUpdate
        {
            get => ClefViewModel.Clef.AutoUpdate;
            set => ClefViewModel.Clef.AutoUpdate = value;
        }

    }
}