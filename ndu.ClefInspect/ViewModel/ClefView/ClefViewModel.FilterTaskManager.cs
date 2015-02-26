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
            private readonly ClefViewSettings _settings = settings;
            private int _nextItemFromSource = 0;

            public void Filter(List<IMatcher> matchers, LinesChangedEventArgsAction action, Action<int> onChanged)
            {
                Task? previousFilterTask = _filterTask;
                LinesChangedEventArgsAction previousAction = LinesChangedEventArgsAction.None;
                CancellationTokenSource? previousCancellationToken = _cancelFilterTask;
                if (previousFilterTask != null && previousCancellationToken != null && previousFilterTask.Status != TaskStatus.RanToCompletion)
                {
                    previousCancellationToken.Cancel();
                    previousAction = _filterAction;
                }
                else
                {
                    previousFilterTask = null;
                    previousCancellationToken = null;

                }
                _cancelFilterTask = new CancellationTokenSource();
                _filterAction = Union(action, previousAction);
                _filterTask = Reload(_clefViewModel.ClefLines, _clefViewModel.Clef, matchers, _clefViewModel.SelectedIndex, _filterAction,
                    () =>
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
                   onChanged,
                   _cancelFilterTask.Token);
            }


            public Task Reload(FilteredClef clefLines,
                Clef clef, List<IMatcher> filters, int selectedIndex, LinesChangedEventArgsAction action,
                Action onRun, Action<int> onChanged,
                CancellationToken cancellationToken)
            {
                if (action == LinesChangedEventArgsAction.None)
                {
                    return Task.CompletedTask;
                }

                ClefLineViewModel? selectedLine = clefLines.ElementAtOrDefault(selectedIndex);
                DateTime? date = selectedLine?.GetTime();
                selectedIndex = -1;
                int selectedIndexExact = -1;

                int sourceIdx = (action == LinesChangedEventArgsAction.Add && clefLines.Count > 0) ? _nextItemFromSource : 0;
                int idx = (action == LinesChangedEventArgsAction.Add) ? clefLines.Count : 0;
                List<ClefLineViewModel> result = new(clefLines);

                return Task.Run(() =>
                {
                    onRun();
                    (object?, int) added = (null, 0);
                    bool changedAlot = false;
                    IList<ClefLine> lines = clef.ViewFrom(sourceIdx);

                    for (int i = 0; i < lines.Count; ++i)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        ClefLine line = lines[i];
                        _nextItemFromSource = sourceIdx + i + 1;
                        bool ok = line.Pin || (filters.Count == 0 || filters.All(f => f.Accept(line)));
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
                        clefLines.Clear();
                        clefLines.AddRange(result);
                        if (changedAlot)
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
