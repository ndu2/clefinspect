using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ndu.ClefInspect.ViewModel.MainView
{
    public partial class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            Settings = new MainViewSettings();

            var args = Environment.GetCommandLineArgs();
            ClefTabs = [];

            for (int i = 1; i < args.Length; i++)
            {
                OpenFile(new FileInfo(args[i]));
            }
            Exit = new ExitCommand();
            Open = new OpenCommand(this);
            OpenFilesTabbed = new OpenFilesTabbedCommand(this);
            OpenFilesConcat = new OpenFilesConcatCommand(this);
            OpenFilesCancel = new OpenFilesCancelCommand(this);
            OpenFromClipboard = new OpenFromClipboardCommand(this);
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
        public ICommand OpenFilesTabbed { get; set; }
        public ICommand OpenFilesConcat { get; set; }
        public ICommand OpenFilesCancel { get; set; }
        public ICommand OpenFromClipboard { get; set; }
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

        public void OpenFile(FileInfo fileName)
        {
            ClefTab tab = new(fileName, Settings);
            ClefTabs.Add(tab);
            tab.Closing += () =>
            {
                ClefTabs.Remove(tab);
            };
            ActiveTab = tab;
        }
        public void OpenFile(List<FileInfo> fileNames)
        {
            ClefTab tab = new(fileNames, Settings);
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
                OpenFile(new FileInfo(file));
            }
        }

        public void OpenText(string text)
        {
            ClefTab tab = new(text, Settings);
            ClefTabs.Add(tab);
            tab.Closing += () =>
            {
                ClefTabs.Remove(tab);
            };
            ActiveTab = tab;
        }

        public void OpenSelectedFilesTabbed()
        {
            if (SelectedFiles == null || SelectedFiles.Length == 0)
            {
                return;
            }
            foreach (string file in SelectedFiles)
            {
                OpenFile(new FileInfo(file));
            }
            SelectedFiles = null;
        }
        public void OpenSelectedFilesConcat()
        {
            if (SelectedFiles == null || SelectedFiles.Length == 0) {
                return;
            }
            List<FileInfo> files = new List<FileInfo>();
            foreach(string file in SelectedFiles)
            {
                files.Add(new FileInfo(file));
            }
            OpenFile(files);
            SelectedFiles = null;
        }

        private string[]? _selectedFiles = null;
        public void SetSelectedFilesSorted(string[] fileNames)
        {
            string[] sorted = new string[fileNames.Length];
            Array.Copy(fileNames, sorted, sorted.Length);
            Array.Sort(sorted);
            SelectedFiles = sorted;
        }
        public string[]? SelectedFiles
        {
            get => _selectedFiles;
            set
            {
                _selectedFiles = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFiles)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowOpenFileOptions)));
            }
        }
        public bool ShowOpenFileOptions => _selectedFiles != null && _selectedFiles.Length > 0;
        public static Brush ShowOpenFileForeground => SystemColors.WindowTextBrush;
        public static Brush ShowOpenFileBackground => SystemColors.WindowBrush;
    }
}
