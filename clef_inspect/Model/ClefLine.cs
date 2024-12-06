using clef_inspect.ViewModel.ClefView;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Nodes;

namespace clef_inspect.Model
{
    public class ClefLine
    {
        private JsonObject? _line;
        private bool _pin;

        public ClefLine(long sort, JsonObject? line)
        {
            Sort = sort;
            _line = line;
            _pin = false;
            Message = Render(line);
            Time = GetTime(line);
        }

        private static string? Render(JsonObject? line)
        {
            return line?["@m"]?.ToString() ?? Render(line?["@mt"]?.ToString(), line);
        }

        private enum ParseState
        {
            Text,
            Token,
            TokenEaten,
        }

        private static string? Render(string? mt, JsonObject? line)
        {
            if (mt == null || line == null) return null;
            ParseState state = ParseState.Text;
            StringBuilder outString = new StringBuilder();
            int i1 = 0;
            for (int i = 0; i < mt.Length; ++i)
            {
                switch (state)
                {
                    case ParseState.Text:
                        if (mt[i] == '{')
                        {
                            outString.Append(mt.Substring(i1, i - i1));
                            state = ParseState.Token;
                            i1 = i;
                        }
                        break;
                    case ParseState.Token:
                        if (mt[i] == '}')
                        {
                            state = ParseState.TokenEaten;
                            string t1 = mt.Substring(i1 + 1, i - i1 - 1);
                            outString.Append(line[t1] ?? $"{{{t1}}}");

                        }
                        break;
                    case ParseState.TokenEaten:
                        state = ParseState.Text;
                        i1 = i;
                        break;
                }
            }
            outString.Append(mt.Substring(i1, mt.Length - i1));
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


        public long Sort { get; }

        public string? Level
        {
            get
            {
                return _line?["@l"]?.ToString() ?? "Info";
            }
        }
        public DateTime? Time { get; }

        public string? SourceContext
        {
            get
            {
                return _line?["SourceContext"]?.ToString();
            }
        }
        public string? Message { get; } 

        public JsonObject? JsonObject
        {
            get => _line;
        }

        public string? Json
        {
            get
            {
                return _line?.ToString();
            }
        }

        public bool Pin { get; set; } = false;

        public override string ToString()
        {
            return $"{Time};{Level};{Message}";
        }
    }
}
