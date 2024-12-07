using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using clef_inspect.ViewModel.ClefView;
using clef_inspect.Model;

namespace clef_inspect.ViewModel.MainView
{
    public partial class ClefTab : INotifyPropertyChanged
    {

        public ClefTab(string fileName, Settings settings)
        {
            ClefViewModel = new ClefViewModel(fileName, settings);
            Close = new CloseTabCommand(this);

            ClefViewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(ClefViewModel.CalculationRunning))
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CalculationRunning)));
                }
            };
            ClefViewModel.Clef.PropertyChanged += (sender, e) =>
            {
                if(e.PropertyName == nameof(Clef.AutoUpdate))
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoUpdate)));
                }
                if (e.PropertyName == nameof(Clef.FileOk))
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileError)));
                }
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void DoClose()
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
        public Visibility FileError
        {
            get => ClefViewModel.Clef.FileOk ? Visibility.Hidden: Visibility.Visible;
        }
        public Visibility CalculationRunning
        {
            get => ClefViewModel.CalculationRunning ? Visibility.Visible: Visibility.Hidden;
        }

    }
}