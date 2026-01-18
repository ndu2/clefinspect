using ndu.ClefInspect.ViewModel;
using ndu.ClefInspect.ViewModel.MainView;
using System.Globalization;
using System.Reflection;
using System.Windows;
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
    }
}