using clef_inspect.ViewModel.ClefView;
using clef_inspect.Model;
using clef_inspect.ViewModel.ClefView;
using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace clef_inspect.View
{
    /// <summary>
    /// Interaction logic for ClefView.xaml
    /// </summary>
    public partial class ClefView : UserControl
    {
        public ClefView()
        {
            InitializeComponent();
            this.DataContextChanged += ClefView_DataContextChanged;
        }

        private void ClefView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is ClefViewModel viewModel)
            {
                viewModel.Reloaded += () =>
                {
                    ListViewLogEntries.ScrollIntoView(viewModel.SelectedItem);
                };
            }
        }

        public double DateWidth
        {
            get
            {
                FormattedText formattedText = new FormattedText(Settings.DateWidthText, CultureInfo.CurrentCulture,
                    FlowDirection, new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                    FontSize, Brushes.Black,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                return formattedText.Width;
            }
        }

        public double LevelWidth
        {
            get
            {
                FormattedText formattedText = new FormattedText(Settings.LevelWidthText, CultureInfo.CurrentCulture,
                        FlowDirection, new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                        FontSize, Brushes.Black,
                        VisualTreeHelper.GetDpi(this).PixelsPerDip);
                return formattedText.Width;
            }
        }

        public double SourceContextWidth
        {
            get
            {
                FormattedText formattedText = new FormattedText(Settings.SourceContextWidthText, CultureInfo.CurrentCulture,
                        FlowDirection, new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                        FontSize, Brushes.Black,
                        VisualTreeHelper.GetDpi(this).PixelsPerDip);
                return formattedText.Width;
            }
        }

        public double DeltaWidth
        {
            get
            {
                FormattedText formattedText = new FormattedText(Settings.DeltaWidthText, CultureInfo.CurrentCulture,
                        FlowDirection, new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                        FontSize, Brushes.Black,
                        VisualTreeHelper.GetDpi(this).PixelsPerDip);
                return formattedText.Width;
            }
        }


        private void TextFilter_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextFilter.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                (DataContext as ClefViewModel)?.ApplyTextFilter.Execute(this);
            }
        }

        private void CopySelected_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            var sel = ListViewLogEntries.SelectedItems;
            foreach (var item in sel)
            {
                if (item is ClefLine line)
                {
                    sb.AppendLine(line.ToString());
                }
            }
            if(sb.Length > 0)
            {
                Clipboard.SetText(sb.ToString());
            }
        }

        private void CopySelectedClef_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            var sel = ListViewLogEntries.SelectedItems;
            foreach (var item in sel)
            {
                if (item is ClefLine line)
                {
                    sb.AppendLine(line.Json);
                }
            }
            if (sb.Length > 0)
            {
                Clipboard.SetText(sb.ToString());
            }
        }
        private void CopyAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in ListViewLogEntries.Items)
            {
                if (item is ClefLine line)
                {
                    sb.AppendLine(line.ToString());
                }
            }
            if (sb.Length > 0)
            {
                Clipboard.SetText(sb.ToString());
            }
        }
    }
}
