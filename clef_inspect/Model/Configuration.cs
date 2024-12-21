using Microsoft.Extensions.Configuration;
using System;
using System.Configuration;
using System.IO;
using System.IO.Packaging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace clef_inspect.Model
{
    public class Configuration
    {
        private const string DEFAULT_JSON = "clef_inspect.defaults.json";

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
            public HashSet<string> DefaultFilterVisibility { get; set; } = new();
            public HashSet<string> DefaultColumnVisibility { get; set; } = new();
        }

        public class SessionOptions
        {
            public const string Session = "session";
            public List<string> Files { get; set; } = new();
        }

        public Configuration()
        {
            ClefFeatures = new ClefFeaturesOptions();
            ViewSettings = new ViewSettingsOptions();
            Session = new SessionOptions();
            try
            {
                IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile(DEFAULT_JSON, optional: true)
                .Build();
                config.GetSection(ClefFeaturesOptions.ClefFeatures).Bind(ClefFeatures);
                config.GetSection(ViewSettingsOptions.ViewSettings).Bind(ViewSettings);
                config.GetSection(SessionOptions.Session).Bind(Session);
            }
            catch (Exception)
            {
                ClefFeatures.WriteableConfig = false;
            }

        }

        public ClefFeaturesOptions ClefFeatures { get; }
        public ViewSettingsOptions ViewSettings { get; }
        public SessionOptions Session { get; }
        public void Write()
        {
            var options = new JsonWriterOptions
            {
                Indented = true
            };
            using var stream = new FileStream(DEFAULT_JSON, FileMode.Create, FileAccess.Write);
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
