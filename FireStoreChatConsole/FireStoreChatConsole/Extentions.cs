using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireStoreChatConsole
{
    public static class Extentions
    {

        public static List<string> GetSplittedString(this string str)
        {
            if (str.Length <= 100) return new List<string>() { str };
            List<string> tmp = new List<string>();
            
            string tmpStr = "";
            
            for(int i = 0; i < str.Length;i++)
            {
                tmpStr += str[i];
                if (tmpStr.Length % 100 == 0)
                {
                    tmp.Add(tmpStr);
                    tmpStr = "";
                }
            }

            if(!string.IsNullOrEmpty(tmpStr))
                tmp.Add(tmpStr);
            return tmp;
        }

        


    }
}
