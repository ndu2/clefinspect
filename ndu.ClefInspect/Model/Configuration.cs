using Microsoft.Extensions.Configuration;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
namespace ndu.ClefInspect.Model
{
    public class Configuration
    {
        private const string DEFAULT_JSON = "ClefInspect.defaults.json";
        private readonly string DEFAULT_JSON_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DEFAULT_JSON);

        public class ClefFeaturesOptions
        {
            public const string ClefFeatures = "clefFeatures";
            public bool WriteableConfig { get; set; } = false;
        }
        public class ViewSettingsOptions
        {
            public const string ViewSettings = "viewSettings";
            public bool LocalTime { get; set; } = true;
            public bool OneLineOnly { get; set; }
            public HashSet<string> DefaultFilterVisibility { get; set; } = ["Level"];
            public HashSet<string> DefaultColumnVisibility { get; set; } = [];
            public double DetailViewFraction { get; set; } = 0.33;
            public bool DetailView { get; set; } = false;
        }

        public class SessionOptions
        {
            public const string Session = "session";
            public ObservableCollection<string> Files { get; set; } = [];
        }
        public class PinPresetOptions
        {
            public const string PinPresets = "pinPresets";
            public string Name { get; set; } = string.Empty;
            public Brush Color { get; set; } = SystemColors.GrayTextBrush;
            public bool Enabled { get; set; } = false;
            public List<string> SearchText { get; set; } = [];
        }

        public Configuration()
        {
            ClefFeatures = new ClefFeaturesOptions();
            ViewSettings = new ViewSettingsOptions();
            Session = new SessionOptions();
            PinPresets = [];
            try
            {
                IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile(DEFAULT_JSON_PATH, optional: true)
                .Build();
                config.GetSection(ClefFeaturesOptions.ClefFeatures).Bind(ClefFeatures);
                config.GetSection(ViewSettingsOptions.ViewSettings).Bind(ViewSettings);
                config.GetSection(SessionOptions.Session).Bind(Session);
                config.GetSection(PinPresetOptions.PinPresets).Bind(PinPresets);
            }
            catch (Exception e)
            {
                string err = e.Message;
                if(e.InnerException != null)
                {
                    err += "\n\n";
                    err += e.InnerException.Message;
                }
                if (e.InnerException?.InnerException != null)
                {
                    err += "\n\n";
                    err += e.InnerException.InnerException.Message;
                }
                MessageBox.Show(err, "Invalid ClefInspect.defaults.json", MessageBoxButton.OK);
                ClefFeatures.WriteableConfig = false;
            }

        }

        public ClefFeaturesOptions ClefFeatures { get; }
        public ViewSettingsOptions ViewSettings { get; }
        public SessionOptions Session { get; }
        public ObservableCollection<PinPresetOptions> PinPresets { get; }

        public void Write()
        {
            var options = new JsonWriterOptions
            {
                Indented = true
            };
            using var stream = new FileStream(DEFAULT_JSON_PATH, FileMode.Create, FileAccess.Write);
            using var writer = new Utf8JsonWriter(stream, options);
            writer.WriteStartObject();
            writer.WritePropertyName(ClefFeaturesOptions.ClefFeatures);
            JsonSerializer.Serialize(writer, ClefFeatures);
            writer.WritePropertyName(ViewSettingsOptions.ViewSettings);
            JsonSerializer.Serialize(writer, ViewSettings);
            writer.WritePropertyName(SessionOptions.Session);
            JsonSerializer.Serialize(writer, Session);
            writer.WriteEndObject();
            writer.Flush();
        }
    }
}
