using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanWebServer.Extentions
{
    public static class StringExtention
    {
        public static string LeftOf(this string s, string c)
        {
            string result = s;

            int index = s.IndexOf(c);

            if (index != -1)
                result = s.Substring(0, index);

            return result;
        }
        public static string RightOf(this string s, string c)
        {
            string result = s;

            int index = s.IndexOf(c);

            if (index != -1)
                result = s.Substring(index + 1);

            return result;
        }

        public static string RightOfSubstring(this string s, string substring)
        {
            string ret = string.Empty;
            int idx = s.IndexOf(substring);

            if (idx != -1)
            {
                ret = s.Substring(idx + substring.Length);
            }

            return ret;
        }

        public static string LeftOfRightmostOf(this string src, string c)
        {
            string ret = src;
            int idx = src.LastIndexOf(c);

            if (idx != -1)
            {
                ret = src.Substring(0, idx);
            }

            return ret;
        }
    }

}
