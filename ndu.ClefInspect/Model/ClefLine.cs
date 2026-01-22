using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Unicode;

namespace ndu.ClefInspect.Model
{
    public class ClefLine(long sort, JsonObject? line)
    {

        private static readonly JsonSerializerOptions _showUnicode = new()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            WriteIndented = false
        };
        private static readonly JsonSerializerOptions _escapeUnicode = new()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin),
            WriteIndented = false
        };

        private readonly JsonObject? _line = line;

        private static string? Render(JsonObject? line)
        {
            return line?["@m"]?.ToString() ?? Render(line?["@mt"]?.ToString(), line);
        }

        private enum ParseState
        {
            Text,
            Token,
        }

        private static string? Render(string? mt, JsonObject? line)
        {
            if (mt == null || line == null) return null;
            ParseState state = ParseState.Text;
            StringBuilder outString = new();
            int i1 = 0;
            for (int i = 0; i < mt.Length; ++i)
            {
                switch (state)
                {
                    case ParseState.Text:
                        if (mt[i] == '{')
                        {
                            outString.Append(mt[i1..i]);
                            state = ParseState.Token;
                            i1 = i;
                        }
                        break;
                    case ParseState.Token:
                        if (mt[i] == '}')
                        {
                            state = ParseState.Text;
                            string t1 = mt.Substring(i1 + 1, i - i1 - 1);
                            outString.Append(line[t1]?.ToJsonString(_showUnicode) ?? $"{{{t1}}}");
                            i1 = i + 1;
                        }
                        break;
                }
            }
            outString.Append(mt[i1..]);
            return outString.ToString();
        }
        private static DateTime? GetTime(JsonObject? line)
        {
            string? l = line?["@t"]?.ToString();
            if (l != null)
            {
                try
                {
                    DateTime dt = DateTime.Parse(l);
                    return dt;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return null;
                }
            }
            return null;
        }


        public long Sort { get; } = sort;

        public string? Level
        {
            get
            {
                return _line?[Clef.LEVEL_KEY]?.ToString() ?? Clef.LEVEL_EMPTY;
            }
        }
        public DateTime? Time { get; } = GetTime(line);

        public string? Message { get; } = Render(line);
        public string? Exception
        {
            get
            {
                return _line?[Clef.EXCEPTION_KEY]?.ToString();
            }
        }

        public JsonObject? JsonObject
        {
            get => _line;
        }

        public string? Json
        {
            get
            {
                return _line?.ToJsonString(_escapeUnicode);
            }
        }

        public bool Pin { get; set; } = false;

        public override string ToString()
        {
            return $"{Time};{Level};{Message}";
        }
    }
}
