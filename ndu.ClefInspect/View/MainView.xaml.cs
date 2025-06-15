using ndu.ClefInspect.ViewModel.MainView;
using System.Reflection;
using System.Windows;
using static ndu.ClefInspect.SingleInstanceManager;

namespace ndu.ClefInspect.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window, IMainView
    {
        public MainView()
        {
            InitializeComponent();
            Version v = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 1);
            this.Title += $" {v.Major}.{v.Minor}.{v.Build}";
        }
        public void OpenFiles(string[] files)
        {
            ((MainViewModel)this.DataContext).OpenFiles(files);
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 1)
                {
                    ((MainViewModel)this.DataContext).SelectedFiles = files;
                }
                else
                {
                    ((MainViewModel)this.DataContext).OpenFiles(files);
                }
            }
        }
    }
}