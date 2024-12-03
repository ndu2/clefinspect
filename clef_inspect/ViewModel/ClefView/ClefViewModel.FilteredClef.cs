using clef_inspect.Model;
using System.Collections.Specialized;
using System.Text.Json.Nodes;

namespace clef_inspect.ViewModel.ClefView
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

            public void Reload(Clef clef, ICollection<Filter> filters, Func<JsonObject?, bool> textFilterOk, ref int selectedIndex)
            {
                ClefLine? selectedLine = this.ElementAtOrDefault(selectedIndex);
                DateTime? date = selectedLine?.GetTime();
                selectedIndex = -1;
                int selectedIndexExact = -1;
                int idx = 0;
                (object?, int) removed = (null, 0);
                List<(object, int)> adds = new List<(object, int)>();
                bool changedAlot = false;
                foreach (var line in clef.Lines.Reverse())
                {
                    bool ok = filters.All(f => f.Accept(line)) && textFilterOk(line);
                    if (ok)
                    {
                        ClefLine item = new ClefLine(line, _settings);
                        if (this.Count > idx)
                        {
                            if (this[idx].JsonObject != item.JsonObject)
                            {
                                if (removed.Item1 == null)
                                {
                                    removed = (this[idx], idx);
                                }
                                else
                                {
                                    changedAlot = true;
                                }
                                this[idx] = item;
                            }
                        }
                        else
                        {
                            this.Add(item);
                            if (adds.Count < 50)
                            {
                                adds.Add((item, idx));
                            }
                            else
                            {
                                changedAlot = true;
                            }
                        }
                        if (item.GetTime() <= date)
                        {
                            selectedIndex++;
                        }
                        if (item.JsonObject == selectedLine?.JsonObject)
                        {
                            selectedIndexExact = selectedIndex;
                        }
                        idx++;
                    }
                }
                if (Count - idx != 0)
                {
                    this.RemoveRange(idx, Count - idx);
                    changedAlot = true;
                }
                if (selectedIndexExact > 0)
                {
                    selectedIndex = selectedIndexExact;
                }
                if (changedAlot)
                {
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
                else
                {
                    if (removed.Item1 != null)
                    {
                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed.Item1, removed.Item2));
                    }
                    foreach ((object, int) add in adds)
                    {
                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, add.Item1, add.Item2));
                    }
                }
            }
        }
    }
}
