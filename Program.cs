using LeanWebServer;
using LeanWebServer.Extentions;
using System.ComponentModel.Design;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        Server server = new Server();

        server.OnError = ErrorHandler.ErrorHandling;
        server.Start(GetWebsitePath());
        server.router.AddRoute(new Route() { Verb = Router.POST, Path = "/demo/redirect", Action = RedirectAction });
        Console.ReadLine();
    }

    public static string GetWebsitePath()
    {
        string webSitePath = Assembly.GetExecutingAssembly().Location;
        webSitePath = webSitePath.LeftOfRightmostOf("\\").LeftOfRightmostOf("\\").LeftOfRightmostOf("\\").LeftOfRightmostOf("\\")  + "\\Website";

        return webSitePath;
    }

    public static string RedirectAction(Dictionary<string, object> parms)
    {
        return "/demo/clicked";
    }
}