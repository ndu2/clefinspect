using compact_log_browser.ViewModel.MainView;
using System;
using System.Windows;

namespace compact_log_browser.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
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