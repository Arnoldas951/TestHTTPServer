using LeanWebServer;
using LeanWebServer.Extentions;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        Server server = new Server();
        server.Start(GetWebsitePath());
        Console.ReadLine();
    }

    public static string GetWebsitePath()
    {
        string webSitePath = Assembly.GetExecutingAssembly().Location;
        webSitePath = webSitePath.LeftOfRightmostOf("\\").LeftOfRightmostOf("\\").LeftOfRightmostOf("\\").LeftOfRightmostOf("\\")  + "\\Website";

        return webSitePath;
    }
}