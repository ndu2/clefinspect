using System.Windows.Media;

namespace ndu.ClefInspect.ViewModel.ClefView
{
    public class ClefLineViewDetailModel(string header, string? text)
    {
        public string Header { get; } = header;
        public string? Text { get; } = text;
    }
}
