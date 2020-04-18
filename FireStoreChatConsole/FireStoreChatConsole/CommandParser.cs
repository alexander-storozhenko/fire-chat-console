using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireStoreChatConsole
{
    struct ParseObject
    {
        public string Action { get; set; }
        public List<string> Parameters { get; set; }
        public List<string> Values { get; set; }

        public ParseObject(string action = "")
        {
            Parameters = new List<string>();
            Values = new List<string>();
            Action = action;
        }

    }
    class CommandParser
    {
        public ParseObject Parse(string text)
        {
            ParseObject parseObject = new ParseObject("");

            if (text[0] == '/')
            {
                int j = 1;
                while (text[j] != ' ')
                {
                    parseObject.Action += text[j];
                    j++;
                }

            }
            for (int i = 0; i < text.Length-1; i++)
            {
                if (text[i] == ' ' && text[i+1] == '-')
                {
                    int j = i + 1;
                    var tmpP = "";

                    while (text[j] != ' ')
                    {
                        tmpP += text[j];
                        j++;
                    }
                    parseObject.Parameters.Add(tmpP);

                }
                else if(text[i] == ' ' && text[i + 1] != '-' && text[i + 1] != ' ')
                {
                    int j = i + 1;
                    var tmpV = "";

                    while (text[j] != ' ')
                    {
                        tmpV += text[j];
                        j++;
                    }
                    parseObject.Values.Add(tmpV);
                }
            }
            return parseObject;
        }

    }
}
