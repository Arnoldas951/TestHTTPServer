using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanWebServer.Extentions
{
    public static class BoolExtensions
    {
        public static bool If<T>(this T v, Func<T, bool> predicate, Action<T> action)
        {
            bool ret = predicate(v);

            if (ret)
            {
                action(v);
            }

            return ret;
        }

        public static bool IfNull<T>(this T obj)
        {
            return obj == null;
        }

        public static bool IfNull<T>(this T obj, Action action)
        {
            bool ret = obj == null;

            if (ret) { action(); }

            return ret;
        }

        public static bool IfNotNull<T>(this T obj, Action<T> action)
        {
           bool result = obj != null;

            if (result)
                action(obj);

            return result;
        }
    }
}
