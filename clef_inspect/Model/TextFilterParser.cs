using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clef_inspect.Model
{
    public class TextFilterParser
    {
        private const char QUOTE = '\"';
        private const char DELIM = ',';
        private const char SPACE = ' ';
        enum ParseState
        {
            Init, Text, Quote, Endquote
        }
        public static List<string> Parse(string? text)
        {
            // parse
            List<string> textFilter = new List<string>();
            StringBuilder outs = new StringBuilder();
            ParseState ps = ParseState.Init;
            int quotepos = -1;
            for (int i = 0; i < text?.Length; ++i)
            {
                if (text[i] == QUOTE && ps == ParseState.Init)
                {
                    ps = ParseState.Quote;
                    quotepos = i;
                }
                else if (text[i] == QUOTE && ps == ParseState.Quote)
                {
                    if (quotepos + 1 == i)
                    {
                        ps = ParseState.Text;
                        outs.Append(QUOTE);
                    }
                    else
                    {
                        quotepos = i;
                        ps = ParseState.Endquote;
                    }
                }
                else if (text[i] == QUOTE && ps == ParseState.Endquote)
                {
                    if (quotepos + 1 == i)
                    {
                        ps = ParseState.Quote;
                        outs.Append(QUOTE);
                    }
                    // continue
                }
                else if (text[i] == SPACE && ps == ParseState.Endquote)
                {
                    // continue
                }
                else if (text[i] == DELIM && ps == ParseState.Endquote)
                {
                    textFilter.Add(outs.ToString());
                    outs.Clear();
                    ps = ParseState.Init;
                }
                else if (text[i] == DELIM && ps == ParseState.Text)
                {
                    textFilter.Add(outs.ToString().Trim());
                    outs.Clear();
                    ps = ParseState.Init;
                }
                else if (ps == ParseState.Quote || ps == ParseState.Text)
                {
                    outs.Append(text[i]);
                }
                else if (ps == ParseState.Init)
                {
                    ps = ParseState.Text;
                    outs.Append(text[i]);
                }
            }
            if (ps == ParseState.Endquote)
            {
                textFilter.Add(outs.ToString());
            }
            else if (ps == ParseState.Text)
            {
                textFilter.Add(outs.ToString().Trim());
            }
            else if(ps == ParseState.Init)
            {
                // OK
            }
            else if (ps == ParseState.Endquote)
            {
                // OK
            }
            else if (ps == ParseState.Quote)
            {
                throw new ArgumentException("missing closing quote");
            }
            return textFilter;
        }
    }
}
