using compact_log_browser.Model;
using System.Collections.Specialized;
using System.Text.Json.Nodes;

namespace compact_log_browser.ViewModel.ClefView
{
    public partial class ClefViewModel
    {
        /// <summary>
        /// list of the filtered items. ObservableCollection cannot bulk add triggering
        /// a collection changed event for every item. in the most usecases (filtering a lot)
        /// changing the whole list with NotifyCollectionChangedAction.Reset seems to be faster
        /// </summary>
        public class FilteredClef : List<ClefLine>, INotifyCollectionChanged
        {
            private ClefViewSettings _settings;

            public FilteredClef(ClefViewSettings settings)
            {
                _settings = settings;
            }

            public event NotifyCollectionChangedEventHandler? CollectionChanged;

            public void Reload(Clef clef, List<Filter> filters, Func<JsonObject?, bool> textFilterOk, ref int selectedIndex)
            {
                ClefLine? selectedLine = this.ElementAtOrDefault(selectedIndex);
                DateTime? date = selectedLine?.GetTime();
                Clear();
                selectedIndex = -1;
                int selectedIndexExact = -1;
                foreach (var line in clef.Lines)
                {
                    bool ok = filters.All(f => f.Accept(line)) && textFilterOk(line);
                    if (ok)
                    {
                        ClefLine item = new ClefLine(line, _settings);
                        Add(item);
                        if (item.GetTime() <= date)
                        {
                            selectedIndex++;
                        }
                        if (item.JsonObject == selectedLine?.JsonObject)
                        {
                            selectedIndexExact = selectedIndex;
                        }
                    }
                }
                if (selectedIndexExact > 0)
                {
                    selectedIndex = selectedIndexExact;
                }
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

    }
}
