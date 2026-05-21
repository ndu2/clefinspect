using ndu.ClefInspect.Model;
using System.Collections.Specialized;
using System.Windows;
using static ndu.ClefInspect.Model.LinesChangedEventArgs;

namespace ndu.ClefInspect.ViewModel.ClefView
{
    public partial class ClefViewModel
    {
        public class FilterTaskManager(ClefViewModel clefViewModel, ClefViewSettings settings)
        {
            private Task? _filterTask;
            private CancellationTokenSource? _cancelFilterTask;
            private readonly ClefViewModel _clefViewModel = clefViewModel;
            private LinesChangedEventArgsAction _filterAction = LinesChangedEventArgsAction.None;
            private bool _pinPresetChanged = false;
            private readonly ClefViewSettings _settings = settings;
            private int _nextItemFromSource = 0;

            public void Filter(List<IMatcher> matchers, ISet<string> ignoredEventIds, LinesChangedEventArgsAction action, bool pinPresetChanged,
                bool ignoreFilter, bool filterAll, bool showPinned, bool showHidden, Action<int> onChanged)
            {
                Task? previousFilterTask = _filterTask;
                LinesChangedEventArgsAction previousAction = LinesChangedEventArgsAction.None;
                bool previousPinPresetChanged = false;
                CancellationTokenSource? previousCancellationToken = _cancelFilterTask;
                if (previousFilterTask != null && previousCancellationToken != null && previousFilterTask.Status != TaskStatus.RanToCompletion)
                {
                    previousCancellationToken.Cancel();
                    previousAction = _filterAction;
                    previousPinPresetChanged = _pinPresetChanged;
                }
                else
                {
                    previousFilterTask = null;
                    previousCancellationToken = null;
                }
                _cancelFilterTask = new CancellationTokenSource();
                _filterAction = Union(action, previousAction);
                _pinPresetChanged = previousPinPresetChanged | pinPresetChanged;
                _filterTask = Reload(
                    clefLines: _clefViewModel.ClefLines,
                    clef: _clefViewModel.Clef,
                    filters: matchers,
                    ignoredEvents: ignoredEventIds,
                    selectedIndex: _clefViewModel.SelectedIndex,
                    action: _filterAction,
                    pinPresetChanged: _pinPresetChanged,
                    ignoreFilter: ignoreFilter,
                    filterAll: filterAll,
                    showPinned: showPinned,
                    showHidden: showHidden,
                    onRun: () =>
                    {
                        if (previousFilterTask != null && previousCancellationToken != null)
                        {
                            try
                            {
                                previousFilterTask.Wait(previousCancellationToken.Token);
                            }
                            catch (OperationCanceledException)
                            {
                                // task was cancelled, nothing to do
                            }
                        }
                    },
                   onChanged: onChanged,
                   cancellationToken: _cancelFilterTask.Token);
            }


            public Task Reload(FilteredClef clefLines,
                IClef clef, List<IMatcher> filters, ISet<string> ignoredEvents,
                int selectedIndex, LinesChangedEventArgsAction action,
                bool pinPresetChanged,
                bool ignoreFilter, bool filterAll, bool showPinned, bool showHidden,
                Action onRun, Action<int> onChanged,
                CancellationToken cancellationToken)
            {
                if (action == LinesChangedEventArgsAction.None)
                {
                    return Task.CompletedTask;
                }

                ClefLineViewModel? selectedLine = clefLines.ElementAtOrDefault(selectedIndex);
                DateTime? date = selectedLine?.GetTime();
                int selectedIndexExact = (action == LinesChangedEventArgsAction.Add && selectedIndex < clefLines.Count) ? selectedIndex : -1;
                selectedIndex = -1;

                int sourceIdx = (action == LinesChangedEventArgsAction.Add && clefLines.Count > 0) ? _nextItemFromSource : 0;
                int idx = (action == LinesChangedEventArgsAction.Add) ? clefLines.Count : 0;
                List<ClefLineViewModel> result = new(clefLines);
                return Task.Run(() =>
                {
                    onRun();
                    (object?, int) added = (null, 0);
                    bool changedAlot = false;
                    IList<ClefLine> lines = clef.ViewFrom(sourceIdx);
                    List<int> notifyHiddenChangedOn = new List<int>();
                    for (int i = 0; i < lines.Count; ++i)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        ClefLine line = lines[i];
                        _nextItemFromSource = sourceIdx + i + 1;
                        bool ignoreChanged = false;

                        string? eventId = line.EventId;
                        bool ignore = eventId != null && ignoredEvents.Contains(eventId);
                        ignoreChanged = ignore != line.Ignore;
                        if (ignoreChanged)
                        {
                            line.Ignore = ignore;
                        }
                        bool ok = (showPinned && line.HasPin()) ||
                        (showHidden && (line.Hide || line.Ignore)) ||
                        !filterAll && !line.Hide && !line.Ignore && (ignoreFilter || filters.Count == 0 || filters.All(f => f.Accept(line)));
                        if (ok)
                        {
                            ClefLineViewModel item;
                            if (result.Count > idx)
                            {
                                if (line == result[idx].ClefLine)
                                {
                                    item = result[idx];
                                }
                                else
                                {
                                    item = new ClefLineViewModel(line, _settings);
                                    result[idx] = item;
                                    changedAlot = true;
                                }
                            }
                            else
                            {
                                item = new ClefLineViewModel(line, _settings);
                                result.Add(item);
                                if (added.Item1 == null)
                                {
                                    added = (result[idx], idx);
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
                            if(ignoreChanged)
                            {
                                notifyHiddenChangedOn.Add(idx);
                            }
                            idx++;
                        }
                    }
                    if (result.Count - idx != 0)
                    {
                        result.RemoveRange(idx, result.Count - idx);
                        changedAlot = true;
                    }
                    if (selectedIndexExact > 0)
                    {
                        selectedIndex = selectedIndexExact;
                    }

                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        foreach(int idx in notifyHiddenChangedOn)
                        {
                            if (idx >= 0 && idx < result.Count)
                            {
                                result[idx].NotifyHiddenChanged();
                            }
                        }
                        clefLines.Clear();
                        clefLines.AddRange(result);
                        if (changedAlot || pinPresetChanged)
                        {
                            clefLines.NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                        }
                        else
                        {
                            if (added.Item1 != null)
                            {
                                clefLines.NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, added.Item1, added.Item2));
                            }
                        }
                        onChanged(selectedIndex);
                    });
                }, cancellationToken);
            }

        }
    }
}
