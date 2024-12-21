using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace clef_inspect.ViewModel.MainView
{
    public partial class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            Settings = new MainViewSettings();

            var args = Environment.GetCommandLineArgs();
            ClefTabs = new ObservableCollection<ClefTab>();

            for (int i = 1; i < args.Length; i++)
            {
                OpenFile(args[i]);
            }
            Exit = new ExitCommand();
            Open = new OpenCommand(this);
            SaveViewDefaults = new SaveViewDefaultsCommand(this);
            ApplyViewDefaults = new ApplyViewDefaultsCommand(this);
            SaveSession = new SaveSessionCommand(this);
            LoadSession = new LoadSessionCommand(this);
            CloseTab = new CloseTabCommand(this);
            CopySelected = new UserActionCommand(this, ClefView.ClefViewModel.UserAction.Copy);
            CopyClefSelected = new UserActionCommand(this, ClefView.ClefViewModel.UserAction.CopyClef);
            PinSelected = new UserActionCommand(this, ClefView.ClefViewModel.UserAction.Pin);
            UnpinSelected = new UserActionCommand(this, ClefView.ClefViewModel.UserAction.Unpin);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewSettings Settings { get; }
        public ICommand Open { get; set; }
        public ICommand SaveViewDefaults { get; set; }
        public ICommand ApplyViewDefaults { get; set; }
        public ICommand SaveSession { get; set; }
        public ICommand LoadSession { get; set; }
        public CloseTabCommand CloseTab { get; }
        public UserActionCommand CopySelected { get; }
        public UserActionCommand CopyClefSelected { get; }
        public UserActionCommand PinSelected { get; }
        public UserActionCommand UnpinSelected { get; }
        public ICommand Exit { get; set; }

        private ClefTab? _activeTab;
        public ObservableCollection<ClefTab> ClefTabs { get; set; }

        public ClefTab? ActiveTab
        {
            get => _activeTab;
            set
            {
                if (_activeTab != value)
                {
                    _activeTab = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveTab)));
                }
            }
        }

        public void OpenFile(string fileName)
        {
            ClefTab tab = new(fileName, Settings);
            ClefTabs.Add(tab);
            tab.Closing += () =>
            {
                ClefTabs.Remove(tab);
            };
            ActiveTab = tab;
        }

        public void OpenFiles(string[] files)
        {
            foreach (string file in files)
            {
                OpenFile(file);
            }
        }
    }
}
