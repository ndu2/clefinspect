using ndu.ClefInspect.Model;
using ndu.ClefInspect.ViewModel.ClefView;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace ndu.ClefInspect.ViewModel.MainView
{
    public partial class ClefTab : INotifyPropertyChanged
    {

        public ClefTab(string fileName, MainViewSettings settings)
        {
            ClefViewModel = new ClefViewModel(fileName, settings);
            Close = new CloseTabCommand(this);
            ClefViewModel.PropertyChanged += OnClefViewModelPropertyChanged;
            ClefViewModel.Clef.PropertyChanged += OnClefPropertyChanged;
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnClefViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ClefViewModel.CalculationRunning))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CalculationRunning)));
            }
        }
        private void OnClefPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Clef.AutoUpdate))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoUpdate)));
            }
            if (e.PropertyName == nameof(Clef.FileOk))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileError)));
            }
        }

        private void DoClose()
        {
            ClefViewModel.PropertyChanged -= OnClefViewModelPropertyChanged;
            ClefViewModel.Clef.PropertyChanged -= OnClefPropertyChanged;
            Closing?.Invoke();
            ClefViewModel.DoClose();
        }

        public event Action? Closing;

        public ICommand Close { get; set; }

        public string Name => ClefViewModel.Clef.Title;

        public ClefViewModel ClefViewModel { get; }

        public bool AutoUpdate
        {
            get => ClefViewModel.Clef.AutoUpdate;
            set => ClefViewModel.Clef.AutoUpdate = value;
        }
        public Visibility FileError
        {
            get => ClefViewModel.Clef.FileOk ? Visibility.Hidden : Visibility.Visible;
        }
        public Visibility CalculationRunning
        {
            get => ClefViewModel.CalculationRunning ? Visibility.Visible : Visibility.Hidden;
        }

    }
}