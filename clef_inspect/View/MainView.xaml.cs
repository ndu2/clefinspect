using clef_inspect.ViewModel.MainView;
using System.Reflection;
using System.Windows;

namespace clef_inspect.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
            this.Title += " "+ Assembly.GetExecutingAssembly().GetName().Version;
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
                ((MainViewModel)this.DataContext).OpenFiles(files);
            }
        }
    }
}