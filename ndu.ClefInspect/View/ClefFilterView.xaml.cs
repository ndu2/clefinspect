using System.Windows.Controls;
using ndu.ClefInspect.ViewModel.ClefView;
using System.Windows.Data;
using static ndu.ClefInspect.ViewModel.ClefView.ClefFilterViewModel;

namespace ndu.ClefInspect.View
{
    /// <summary>
    /// Interaction logic for ClefFilterView.xaml
    /// </summary>
    public partial class ClefFilterView : UserControl
    {
        public ClefFilterView()
        {
            InitializeComponent();

        }

        private void FilterCollectionViewFilter(object sender, FilterEventArgs e)
        {
            if(e.Item is ClefFilterValueViewModel f)
            {
                e.Accepted = f.Visible;
            }
            else
            {
                e.Accepted = false;
            }
        }
        private void RefreshFilterValuesSorted(object sender, TextChangedEventArgs e)
        {
            if(this.Resources["FilterValuesSorted"] is CollectionViewSource cs)
            {
                cs.View.Refresh();
            }
        }
    }
}
