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
            string result = string.Empty;
            int index = s.IndexOf(substring);

            if (index != -1)
            {
                result = s.Substring(index + substring.Length);
            }

            return result;
        }

        public static string RightOfRightmostOf(this string s, string c)
        {
            string result = string.Empty;
            int index = s.LastIndexOf(c);

            if (index != -1)
                result = s.Substring(index + 1);

            return result;
        }

        public static string LeftOfRightmostOf(this string s, string c)
        {
            string result = s;
            int index = s.LastIndexOf(c);

            if (index != -1)
            {
                result = result.Substring(0, index);
            }

            return result;
        }
    }

}
