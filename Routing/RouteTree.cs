using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanWebServer.Routing
{
    public class RouteV2
    {
        public string PageName { get; set; }

        public string FileRoute { get; set; }
    }

    public static class RouteTree
    {
        private readonly static Dictionary<string, string> RouteDictionary = new Dictionary<string, string>();

        public static void AddRoute(string pageName, string fileRoute)
        {
            RouteDictionary.Add(pageName, fileRoute);
        }
    }
}
