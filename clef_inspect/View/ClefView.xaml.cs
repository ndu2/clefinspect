using clef_inspect.ViewModel.ClefView;
using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using clef_inspect.ViewModel;

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

        public double PinWidth
        {
            get
            {
                FormattedText formattedText = new FormattedText(Settings.PinWidthText, CultureInfo.CurrentCulture,
                    FlowDirection, new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                    FontSize, Brushes.Black,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                return formattedText.Width;
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

        private List<ClefLineView> GetSelectedInOrder()
        {
            System.Collections.IList list = ListViewLogEntries.SelectedItems;
            List<ClefLineView> selected = new List<ClefLineView>(list.Count);
            foreach (object item in list)
            {
                if (item is ClefLineView line)
                {
                    selected.Add(line);
                }
            }
            selected.Sort((x, y) => (int)(x.Sort - y.Sort));
            return selected;
        }

        private void CopySelected_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            IList<ClefLineView> sel = GetSelectedInOrder();
            foreach (ClefLineView line in sel)
            {
                sb.AppendLine(line.ToString());
            }
            if(sb.Length > 0)
            {
                Clipboard.SetText(sb.ToString());
            }
        }

        private void CopySelectedClef_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            IList<ClefLineView> sel = GetSelectedInOrder();
            foreach (ClefLineView line in sel)
            {
                sb.AppendLine(line.Json);
            }
            if (sb.Length > 0)
            {
                Clipboard.SetText(sb.ToString());
            }
        }

        private void PinSelected_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (var item in ListViewLogEntries.SelectedItems)
            {
                if (item is ClefLineView line)
                {
                    line.Pin = true;
                }
            }
        }
        private void UnpinSelected_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (var item in ListViewLogEntries.SelectedItems)
            {
                if (item is ClefLineView line)
                {
                    line.Pin = false;
                }
            }
        }
        
    }
}
