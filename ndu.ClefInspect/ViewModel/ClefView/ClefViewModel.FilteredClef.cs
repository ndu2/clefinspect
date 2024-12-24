using ndu.ClefInspect.ViewModel.ClefView;
using System.Collections.Specialized;

namespace ndu.ClefInspect.ViewModel.ClefView
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
            public FilteredClef(ClefViewSettings settings)
                :base(settings.DefaultCapacity)
            {
            }

            public event NotifyCollectionChangedEventHandler? CollectionChanged;

            public void NotifyCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                CollectionChanged?.Invoke(this, e);
            }
        }
    }
}
