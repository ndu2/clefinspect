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
            ResetView = new ResetViewCommand(this);
            LoadRecentFiles = new LoadRecentFilesCommand(this);
            ClearRecentFiles = new ClearRecentFilesCommand(this);
            CloseTab = new CloseTabCommand(this);
            CopySelected = new UserActionCommand(this, ClefView.ClefViewModel.UserAction.Copy);
            CopyClefSelected = new UserActionCommand(this, ClefView.ClefViewModel.UserAction.CopyClef);
            PinSelected = new UserActionCommand(this, ClefView.ClefViewModel.UserAction.Pin);
            UnpinSelected = new UserActionCommand(this, ClefView.ClefViewModel.UserAction.Unpin);
            HideSelected = new UserActionCommand(this, ClefView.ClefViewModel.UserAction.Hide);
            HideAllFromSelected = new UserActionCommand(this, ClefView.ClefViewModel.UserAction.HideAllFromSelected);
            ShowSelected = new UserActionCommand(this, ClefView.ClefViewModel.UserAction.Show);
            ToggleIgnoreFilter = new UserActionCommand(this, ClefView.ClefViewModel.UserAction.ToggleIgnoreFilter);
            ToggleIgnorePinned = new UserActionCommand(this, ClefView.ClefViewModel.UserAction.ToggleIgnorePinned);
            ToggleShowHiddenEvents = new UserActionCommand(this, ClefView.ClefViewModel.UserAction.ToggleShowHiddenEvents);
            ToggleFilterAll = new UserActionCommand(this, ClefView.ClefViewModel.UserAction.ToggleFilterAll);
            FocusFilter = new UserActionCommand(this, ClefView.ClefViewModel.UserAction.FocusFilter);
            FocusSearch = new UserActionCommand(this, ClefView.ClefViewModel.UserAction.FocusSearch);
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
        public ICommand ResetView { get; }
        public ICommand LoadRecentFiles { get; set; }
        public ICommand ClearRecentFiles { get; set; }
        public CloseTabCommand CloseTab { get; }
        public UserActionCommand CopySelected { get; }
        public UserActionCommand CopyClefSelected { get; }
        public UserActionCommand PinSelected { get; }
        public UserActionCommand UnpinSelected { get; }
        public UserActionCommand HideSelected { get; }
        public UserActionCommand HideAllFromSelected { get; }
        public UserActionCommand ShowSelected { get; }
        public UserActionCommand ToggleIgnoreFilter { get; }
        public UserActionCommand ToggleIgnorePinned { get; }
        public UserActionCommand ToggleShowHiddenEvents { get; }
        public UserActionCommand ToggleFilterAll { get; }
        public UserActionCommand FocusFilter { get; }
        public UserActionCommand FocusSearch { get; }
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
            Settings.AddRecentFile([fileName]);
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
            Settings.AddRecentFile(fileNames);
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
            if (SelectedFiles == null || SelectedFiles.Length == 0)
            {
                return;
            }
            List<FileInfo> files = new List<FileInfo>();
            foreach (string file in SelectedFiles)
            {
                files.Add(new FileInfo(file));
            }
            OpenFile(files);
            SelectedFiles = null;
        }

        private string[]? _selectedFiles = null;
        private string[]? _selectedFilesBackup = null;
        public void SetSelectedFilesSorted(string[] fileNames)
        {
            string[] sorted = new string[fileNames.Length];
            Array.Copy(fileNames, sorted, sorted.Length);
            Array.Sort(sorted);
            SelectedFiles = sorted;
        }
        public void CancelReorderSelectedFiles()
        {
            SelectedFiles = _selectedFilesBackup;
            _selectedFilesBackup = null;
        }

        public void ReorderSelectedFilesConfirm()
        {
            _selectedFilesBackup = null;
        }
        public bool StartReorderSelectedFilesStart()
        {
            _selectedFilesBackup = null;
            if (SelectedFiles != null)
            {
                _selectedFilesBackup = new string[SelectedFiles.Length];
                Array.Copy(SelectedFiles, _selectedFilesBackup, SelectedFiles.Length);
            }
            return _selectedFilesBackup?.Length > 0;
        }


        public void ReorderSelectedFiles(string file, string toPosition, bool after)
        {
            if(SelectedFiles == null || _selectedFilesBackup?.Length != SelectedFiles.Length)
            {
                return;
            }
            List<string> reordered = new(SelectedFiles.Length);
            for (int i = 0; i < SelectedFiles.Length; i++)
            {
                if (!after && SelectedFiles[i] == toPosition)
                {
                    reordered.Add(file);
                }
                if (SelectedFiles[i] != file)
                {
                    reordered.Add(SelectedFiles[i]);
                }
                if (after && SelectedFiles[i] == toPosition)
                {
                    reordered.Add(file);
                }
            }
            SelectedFiles = reordered.ToArray();
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
