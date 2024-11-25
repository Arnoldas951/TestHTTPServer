using LeanWebServer;
using LeanWebServer.Extentions;
using LeanWebServer.Routing;
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
        //Server.onRequest = (session, context) =>
        //{
        //    session.IsAuthorized = true;
        //    session.UpdateLastConnection();
        //};

        //RouteTree.AddRoute("/User/login.html", "");

        server.router.AddRoute(new Route() { Verb = Router.POST, Path = "/demo/redirect", Handler = new AuthorizedRouteHandler(server, RedirectTo) });
        //server.router.AddRoute(new Route() { Verb = Router.GET, Path = "/demo/redirect.html", Handler = new AuthorizedRouteHandler(server, RedirectTo) });
        //server.router.AddRoute(new Route() { Verb = Router.GET, Path = "/demo/ajax", Handler = new AuthorizedRouteHandler(server, AjaxResponder) });
        //server.router.AddRoute(new Route() { Verb = Router.GET, Path = "/User/login.html", Handler = new AnonymousRouteHandler(server, RedirectTo) });
        //server.router.AddRoute(new Route() { Verb = Router.GET, Path = "/User/register.html", Handler = new AnonymousRouteHandler(server, RedirectTo) });
        server.router.AddRoute(new Route() { Verb = Router.POST, Path = "/User/register", Handler = new AnonymousRouteHandler(server, Register) });
        //server.router.AddRoute(new Route() { Verb = Router.GET, Path = "/demo/clicked.html", Handler = new AuthorizedRouteHandler(server, RedirectTo) });

        Console.ReadLine();
    }

    public static string GetWebsitePath()
    {
        string webSitePath = Assembly.GetExecutingAssembly().Location;
        webSitePath = webSitePath.LeftOfRightmostOf("\\").LeftOfRightmostOf("\\").LeftOfRightmostOf("\\").LeftOfRightmostOf("\\") + "\\Website";

        return webSitePath;
    }

    public static ResponsePacket RedirectTo(Session session, Dictionary<string, object> parms, string path) 
    {
        return server.Redirect(path);
    }

    public static ResponsePacket Register(Session session, Dictionary<string, object> parms, string path)
    {
        string data = "test";
        ResponsePacket packet = new ResponsePacket() { Data = Encoding.UTF8.GetBytes(data), ContentType = "text" };
        return packet;
    }

    public static ResponsePacket RedirectAction(Session session, Dictionary<string, object> parms, string path)
    {
        return server.Redirect("/demo/clicked");
    }

    public static ResponsePacket RedirectToLogin(Session session, Dictionary<string, object> parms, string path) 
    {
        return server.Redirect("/User/login.html");
    }

    public static ResponsePacket AjaxResponder(Session session, Dictionary<string, object> parms, string path) 
    {
        string data = "EL OU EL " + parms["number"].ToString();
        
        ResponsePacket packet = new ResponsePacket() { Data = Encoding.UTF8.GetBytes(data), ContentType = "text"};
        return packet;
    }
}