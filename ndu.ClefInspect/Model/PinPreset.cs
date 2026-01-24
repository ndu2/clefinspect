using System.ComponentModel;
using System.Windows.Media;
using static ndu.ClefInspect.Model.Configuration;

namespace ndu.ClefInspect.Model
{
    public class PinPreset : INotifyPropertyChanged
    {
        private bool _enabled = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        public PinPreset(PinPresetOptions pinPresetOption)
        {
            PinPresetOptions = pinPresetOption;
            _enabled = PinPresetOptions.Enabled;
        }
        public string Name => PinPresetOptions.Name;
        public List<string> SearchText => PinPresetOptions.SearchText;
        public Brush Color => PinPresetOptions.Color;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Enabled)));
                }
            }
        }

        public PinPresetOptions PinPresetOptions { get; }
    }
}
