using ndu.ClefInspect.ViewModel;
using ndu.ClefInspect.ViewModel.ClefView;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace ndu.ClefInspect.View
{
    /// <summary>
    /// Interaction logic for ClefView.xaml
    /// </summary>
    public partial class ClefView : UserControl
    {
        private readonly Dictionary<string, GridViewColumn> _customColumns = [];
        private ScrollViewer? _listViewLogEntriesScrollViewer = null;

        public ClefView()
        {
            InitializeComponent();
            this.DataContextChanged += ClefView_DataContextChanged;
        }

        private void OnDataColumnSetChanged()
        {

            if (DataContext is ClefViewModel viewModel)
            {
                // remove all columns not available in viewModel
                List<string> surplusCols = [];
                foreach (var cc in _customColumns)
                {
                    if (!viewModel.DataColumns.Any(col => col.Header == cc.Key))
                    {
                        surplusCols.Add(cc.Key);
                    }
                }
                foreach (string s in surplusCols)
                {
                    RemoveColumn(s);
                }
            }
            OnDataColumnEnabledChanged();
        }

        private void OnDataColumnEnabledChanged()
        {
            if (DataContext is ClefViewModel viewModel)
            {
                foreach (ClefViewModel.DataColumnView dataColumn in viewModel.DataColumns)
                {
                    if (dataColumn.Enabled)
                    {
                        AddColumn(dataColumn.Header);
                    }
                    else
                    {
                        RemoveColumn(dataColumn.Header);
                    }
                }
            }
        }
        public void AddColumn(string key)
        {
            if (_customColumns.ContainsKey(key))
            {
                return;
            }
            if (ListViewLogEntries.View is GridView gv)
            {
                GridViewColumn gvc = new()
                {
                    Header = key,
                    DisplayMemberBinding = new Binding($"[{key}]")
                };
                gv.Columns.Add(gvc);
                _customColumns.Add(key, gvc);
            }
        }

        public void RemoveColumn(string key)
        {
            if (_customColumns.TryGetValue(key, out GridViewColumn? gvc))
            {
                if (ListViewLogEntries.View is GridView gv)
                {
                    _customColumns.Remove(key);
                    gv.Columns.Remove(gvc);
                }
            }
        }

        private void ClefView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ClefViewModel oldViewModel)
            {
                oldViewModel.Reloaded -= OnReloaded;
                oldViewModel.UserActionHandler -= OnUserAction;
                oldViewModel.DataColumnEnabledChanged -= OnDataColumnEnabledChanged;
                oldViewModel.Settings.SessionSettings.PropertyChanged -= ListViewLogEntries_Update;
                oldViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
            if (DataContext is ClefViewModel viewModel)
            {
                viewModel.Reloaded += OnReloaded;
                viewModel.UserActionHandler += OnUserAction;
                viewModel.DataColumnEnabledChanged += OnDataColumnEnabledChanged;
                OnDataColumnSetChanged();
                if (_listViewLogEntriesScrollViewer != null)
                {
                    _listViewLogEntriesScrollViewer.ScrollToVerticalOffset(viewModel.VerticalOffset);
                    _listViewLogEntriesScrollViewer.ScrollToHorizontalOffset(viewModel.HorizontalOffset);
                }
                viewModel.Settings.SessionSettings.PropertyChanged += ListViewLogEntries_Update;
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                Layout(viewModel);
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(sender is ClefViewModel viewModel && e.PropertyName == nameof(ClefViewModel.ColWidthDetails))
            {
                Layout(viewModel);
            }
        }

        private void Layout(ClefViewModel viewModel)
        {
            ColDetails.Width = viewModel.ColWidthDetails;
            ColEvtList.Width = viewModel.ColWidthEvtList;
        }

        private void GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (DataContext is ClefViewModel viewModel)
            {
                viewModel.DetailViewUpdate(1.0 / (ColDetails.Width.Value + ColEvtList.Width.Value) * ColDetails.Width.Value);
            }
        }

        public double PinWidth
        {
            get
            {
                FormattedText formattedText = new(MainViewSettings.PinWidthText, CultureInfo.CurrentCulture,
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
                FormattedText formattedText = new(MainViewSettings.DateWidthText, CultureInfo.CurrentCulture,
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
                FormattedText formattedText = new(MainViewSettings.LevelWidthText, CultureInfo.CurrentCulture,
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
                FormattedText formattedText = new(MainViewSettings.DeltaWidthText, CultureInfo.CurrentCulture,
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
        private void TextDatePosition_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextDatePosition.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            }
        }
        private void ButtonApplyTextFilter_Click(object sender, RoutedEventArgs e)
        {
            TextFilter.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            (DataContext as ClefViewModel)?.ApplyTextFilter.Execute(this);
        }

        private List<ClefLineViewModel> GetSelectedInOrder()
        {
            System.Collections.IList list = ListViewLogEntries.SelectedItems;
            List<ClefLineViewModel> selected = new(list.Count);
            foreach (object item in list)
            {
                if (item is ClefLineViewModel line)
                {
                    selected.Add(line);
                }
            }
            selected.Sort((x, y) => (int)(x.Sort - y.Sort));
            return selected;
        }


        private void OnUserAction(ClefViewModel.UserAction userAction)
        {
            switch (userAction)
            {
                case ClefViewModel.UserAction.Copy: CopySelected(); break;
                case ClefViewModel.UserAction.CopyClef: CopySelectedClef(); break;
                case ClefViewModel.UserAction.Pin: PinSelected(); break;
                case ClefViewModel.UserAction.Unpin: UnpinSelected(); break;
            }
        }

        private void CopySelected()
        {
            List<string> columns = [.. _customColumns.Keys];
            StringBuilder sb = new();
            IList<ClefLineViewModel> sel = GetSelectedInOrder();
            foreach (ClefLineViewModel line in sel)
            {
                sb.AppendLine(line.ToString(columns));
            }
            if (sb.Length > 0)
            {
                Clipboard.SetText(sb.ToString());
            }
        }
        private void CopySelected_Click(object sender, System.Windows.RoutedEventArgs e) => CopySelected();

        private void CopySelectedClef()
        {
            StringBuilder sb = new();
            IList<ClefLineViewModel> sel = GetSelectedInOrder();
            foreach (ClefLineViewModel line in sel)
            {
                sb.AppendLine(line.Json);
            }
            if (sb.Length > 0)
            {
                Clipboard.SetText(sb.ToString());
            }
        }
        private void CopySelectedClef_Click(object sender, System.Windows.RoutedEventArgs e) => CopySelectedClef();

        private void PinSelected()
        {
            foreach (var item in ListViewLogEntries.SelectedItems)
            {
                if (item is ClefLineViewModel line)
                {
                    line.Pin = true;
                }
            }
        }
        private void PinSelected_Click(object sender, System.Windows.RoutedEventArgs e) => PinSelected();
        private void UnpinSelected()
        {
            foreach (var item in ListViewLogEntries.SelectedItems)
            {
                if (item is ClefLineViewModel line)
                {
                    line.Pin = false;
                }
            }
        }
        private void UnpinSelected_Click(object sender, System.Windows.RoutedEventArgs e) => UnpinSelected();


        private void OnReloaded()
        {
            if (DataContext is ClefViewModel viewModel)
            {
                ListViewLogEntries.ScrollIntoView(viewModel.SelectedItem);
            }
        }

        public static ScrollViewer? GetScrollViewer(DependencyObject o)
        {
            //https://stackoverflow.com/questions/1009036/how-can-i-programmatically-scroll-a-wpf-listview
            // Return the DependencyObject if it is a ScrollViewer
            if (o is ScrollViewer oo)
            { return oo; }
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);
                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }
            return null;
        }

        private void ListViewLogEntries_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (DataContext is ClefViewModel viewModel)
            {
                _listViewLogEntriesScrollViewer ??= GetScrollViewer(ListViewLogEntries);
                if (_listViewLogEntriesScrollViewer != null)
                {
                    viewModel.VerticalOffset = _listViewLogEntriesScrollViewer.VerticalOffset;
                    viewModel.HorizontalOffset = _listViewLogEntriesScrollViewer.HorizontalOffset;
                }
            }
        }
        private void ListViewLogEntries_Update(object? sender, PropertyChangedEventArgs e)
        {
            ListViewLogEntries_Update(ListViewLogEntries);
        }

        private static void ListViewLogEntries_Update(DependencyObject d)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(d, i);
                if (child is TextBlock o)
                {
                    var x = o.GetBindingExpression(TextBlock.TextProperty);
                    x?.UpdateTarget();
                }
                ListViewLogEntries_Update(child);
            }
        }
    }
}
