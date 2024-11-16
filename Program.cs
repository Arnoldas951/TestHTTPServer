using LeanWebServer;
using LeanWebServer.Extentions;
using System.ComponentModel.Design;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

class Program
{
    public static Server server;
    static void Main(string[] args)
    {
        server = new Server();
        server.OnError = ErrorHandler.ErrorHandling;
        server.Start(GetWebsitePath());
        Server.onRequest = (session, context) =>
        {
            session.IsAuthorized = true;
            session.UpdateLastConnection();
        };

        server.router.AddRoute(new Route() { Verb = Router.POST, Path = "/demo/redirect", Handler = new AuthorizedExpirableRouteHandler(server, RedirectAction) });
        server.router.AddRoute(new Route() { Verb = Router.GET, Path = "/demo/ajax", Handler = new AnonymousRouteHandler(server, AjaxResponder) });
        Console.ReadLine();
    }

    public static string GetWebsitePath()
    {
        string webSitePath = Assembly.GetExecutingAssembly().Location;
        webSitePath = webSitePath.LeftOfRightmostOf("\\").LeftOfRightmostOf("\\").LeftOfRightmostOf("\\").LeftOfRightmostOf("\\") + "\\Website";

        return webSitePath;
    }

    public static ResponsePacket RedirectAction(Session session, Dictionary<string, object> parms)
    {
        return server.Redirect("/demo/clicked");
    }

    public static ResponsePacket AjaxResponder(Session session, Dictionary<string, object> parms) 
    {
        string data = "EL OU EL " + parms["number"].ToString();
        
        ResponsePacket packet = new ResponsePacket() { Data = Encoding.UTF8.GetBytes(data), ContentType = "text"};
        return packet;
    }
}