using ndu.ClefInspect.ViewModel.MainView;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
                    ((MainViewModel)this.DataContext).SetSelectedFilesSorted(files);
                }
                else
                {
                    ((MainViewModel)this.DataContext).OpenFiles(files);
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            base.OnDragOver(e);
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        public double ButtonSize
        {
            get
            {
                FormattedText formattedText = new("WWOpen in individual TabsWW", CultureInfo.CurrentCulture,
                        FlowDirection, new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                        FontSize, Brushes.Black,
                        VisualTreeHelper.GetDpi(this).PixelsPerDip);
                return formattedText.Width;
            }
        }
        private void FileListOnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed &&
                sender is TextBlock textBlockFile &&
                DataContext is MainViewModel mainViewModel &&
                mainViewModel.StartReorderSelectedFilesStart())
            {
                DragDropEffects result = DragDrop.DoDragDrop(textBlockFile, textBlockFile.Text, DragDropEffects.Move);
                if(result.HasFlag(DragDropEffects.Move))
                {
                    mainViewModel.ReorderSelectedFilesConfirm();
                }
                else
                {
                    mainViewModel.CancelReorderSelectedFiles();
                }
            }
        }
        private bool FileListDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(String).FullName) is string draggedFileName &&
                sender is TextBlock textBlockFile &&
                DataContext is MainViewModel mainViewModel &&
                mainViewModel.SelectedFiles != null &&
                mainViewModel.SelectedFiles.Length >= 1)
            {
                bool after =  e.GetPosition(textBlockFile).Y > 0.5 * textBlockFile.ActualHeight;
                string target = textBlockFile.Text;
                mainViewModel.ReorderSelectedFiles(draggedFileName, target, after);
                return true;
            }
            return false;
        }

        private void FileListOnDragEnter(object sender, DragEventArgs e)
        {
            e.Handled = FileListDrop(sender, e);
        }

        private void FileListOnDragOver(object sender, DragEventArgs e)
        {
            e.Handled = FileListDrop(sender, e);
        }

        private void FileListOnDrop(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }
    }
}