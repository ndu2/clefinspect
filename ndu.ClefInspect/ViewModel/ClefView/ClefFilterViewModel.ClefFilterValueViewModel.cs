using ndu.ClefInspect.Model;

namespace ndu.ClefInspect.ViewModel.ClefView
{
    public partial class ClefFilterViewModel
    {
        public class ClefFilterValueViewModel(ClefFilterViewModel vm, FilterValue filterValue)
        {
            public FilterValue FilterValue { get; } = filterValue;
            public bool Visible
            {
                get => FilterValue.Value.Contains(vm.SearchFilter, StringComparison.InvariantCultureIgnoreCase);
            }
        }
    }
}
