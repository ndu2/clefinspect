using clef_inspect.Model;
using System.Collections.Specialized;

namespace clef_inspect.ViewModel.ClefView
{
    public partial class ClefViewModel
    {
        /// <summary>
        /// list of the filtered items. ObservableCollection cannot bulk add triggering
        /// a collection changed event for every item. in the most usecases (filtering a lot)
        /// changing the whole list with NotifyCollectionChangedAction.Reset seems to be faster
        /// </summary>
        public class FilteredClef : List<ClefLineView>, INotifyCollectionChanged
        {
            private ClefViewSettings _settings;

            public FilteredClef(ClefViewSettings settings)
                :base(settings.DefaultCapacity)
            {
                _settings = settings;
            }

            public event NotifyCollectionChangedEventHandler? CollectionChanged;

            public void Reload(Clef clef, IEnumerable<Filter> filters2, Func<ClefLine, bool> textFilterOk, ref int selectedIndex, LinesChangedEventArgs.LinesChangedEventArgsAction action)
            {
                if(action == LinesChangedEventArgs.LinesChangedEventArgsAction.None)
                {
                    return;
                }
                Filter[] filters = filters2.ToArray();
                ClefLineView? selectedLine = this.ElementAtOrDefault(selectedIndex);
                DateTime? date = selectedLine?.GetTime();
                selectedIndex = -1;
                int selectedIndexExact = -1;

                int sourceIdx = (action == LinesChangedEventArgs.LinesChangedEventArgsAction.Add && Count > 0) ? this[Count - 1].SourceIdx + 1 : 0;
                int idx = (action == LinesChangedEventArgs.LinesChangedEventArgsAction.Add) ? Count : 0;
                    (object?, int) added = (null, 0);
                bool changedAlot = false;
                IList<ClefLine> lines = clef.ViewFrom(sourceIdx);

                for(int i = 0; i < lines.Count; ++i)
                {
                    ClefLine line = lines[i];
                    bool ok = line.Pin || (filters.Length == 0 || filters.All(f => f.Accept(line))) && textFilterOk(line);
                    if (ok)
                    {
                        ClefLineView item;
                        if (this.Count > idx)
                        {
                            if (line == this[idx].ClefLine)
                            {
                                item = this[idx];
                            }
                            else
                            {
                                item = new ClefLineView(sourceIdx + i, line, _settings);
                                this[idx] = item;
                                changedAlot = true;
                            }
                        }
                        else
                        {
                            item = new ClefLineView(sourceIdx + i, line, _settings);
                            this.Add(item);
                            if (added.Item1 == null)
                            {
                                added = (this[idx], idx);
                            }
                            else
                            {
                                // RangeActionsNotSupported
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
                    if (added.Item1 != null)
                    {
                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, added.Item1, added.Item2));
                    }
                }
            }
        }
    }
}
