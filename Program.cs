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
        Server.onRequest = (session, context) =>
        {
            session.IsAuthorized = true;
            session.UpdateLastConnection();
        };
        server.router.AddRoute(new Route() { Verb = Router.POST, Path = "/demo/redirect", Handler = new AuthorizedExpirableRouteHandler(RedirectAction, server) });
        Console.ReadLine();
    }

    public static string GetWebsitePath()
    {
        string webSitePath = Assembly.GetExecutingAssembly().Location;
        webSitePath = webSitePath.LeftOfRightmostOf("\\").LeftOfRightmostOf("\\").LeftOfRightmostOf("\\").LeftOfRightmostOf("\\") + "\\Website";

        return webSitePath;
    }

    public static string RedirectAction(Session session, Dictionary<string, object> parms)
    {
        return "/demo/clicked";
    }
}