using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using compact_log_browser.Model;

namespace compact_log_browser.ViewModel.MainView
{
    public partial class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            Settings = new Settings();

            var args = Environment.GetCommandLineArgs();
            ClefTabs = new ObservableCollection<ClefTab>();

            for (int i = 1; i < args.Length; i++)
            {
                OpenFile(args[i]);
            }
            Exit = new ExitCommand();
            Open = new OpenCommand(this);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public Settings Settings { get; }
        public ICommand Open { get; set; }
        public ICommand Exit { get; set; }

        private ClefTab? _activeTab;
        public ObservableCollection<ClefTab> ClefTabs { get; set; }

        public ClefTab? ActiveTab
        {
            get => _activeTab;
            set
            {
                if(_activeTab != value)
                {
                    _activeTab = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveTab)));
                }
            }
        }

        public void OpenFile(string fileName)
        {
            Clef clef = new Clef(new FileInfo(fileName));

            ClefTab tab = new ClefTab(clef, Settings);
            ClefTabs.Add(tab);
            tab.Closing += () =>
            {
                ClefTabs.Remove(tab);
                clef.Dispose();
            };
            ActiveTab = tab;
        }

        public void OpenFiles(string[] files)
        {
            foreach(string file in files)
            {
                OpenFile(file);
            }
        }

    }
}
