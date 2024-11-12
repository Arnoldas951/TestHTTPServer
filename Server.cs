using LeanWebServer.Extentions;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace LeanWebServer
{
    public class Server
    {
        private static HttpListener _listener;
        public static int maxSimultaneousConnections = 20;
        private static Semaphore sem = new Semaphore(maxSimultaneousConnections, maxSimultaneousConnections);
        private static string logLocation = @"D:\Logs\";
        private static Router router;

        public Server()
        {
            router = new Router();
        }

        public void Start(string webSitePath)
        {
            router.WebsitePath = webSitePath;
            List<IPAddress> localHostIPs = GetLocalHostIPs();
            HttpListener listener = InitializeListener(localHostIPs);
            Start(listener);
        }

        private static List<IPAddress> GetLocalHostIPs()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            List<IPAddress> ret = host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToList();

            return ret;

        }

        private static HttpListener InitializeListener(List<IPAddress> localhostIPs)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost/");

            localhostIPs.ForEach(ip =>
            {
                if (!ip.ToString().Contains("192.168.1.106"))
                {
                    Console.WriteLine($"Listening on IP http://{ip.ToString()}");
                    listener.Prefixes.Add("http://" + ip.ToString() + "/");
                }
            });
            return listener;
        }

        private async void StartConnectionListener(HttpListener listener)
        {
            ResponsePacket resp = null;
            HttpListenerContext context = await listener.GetContextAsync();

            string path = context.Request.RawUrl.LeftOf("?");
            string verb = context.Request.HttpMethod;
            string parms = context.Request.RawUrl.RightOf("?");
            Dictionary<string, object> kvParams = GetKeyValues(parms);
            string data = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding).ReadToEnd();
            GetKeyValues(data, kvParams);
            Log(parms);
            resp = router.Route(verb, path, kvParams);

            Respond(context.Response, resp);

            Console.WriteLine("Connected: " + context.Request.UserAgent);
            sem.Release();
            Log(context.Request);

            //string response = "<html><head><meta http-equiv='content-type' content='text/html; charset=utf-8'/>\r\n      </head>>Look at you lmao!</html";

            //byte[] encoded = Encoding.UTF8.GetBytes(response);
            //context.Response.ContentLength64 = encoded.Length;
            //context.Response.OutputStream.Write(encoded, 0, encoded.Length);
            //context.Response.OutputStream.Close();
        }

        private void RunServer(HttpListener listener)
        {
            while (true)
            {
                sem.WaitOne();
                StartConnectionListener(listener);
            }
        }

        private void Start(HttpListener listener)
        {
            listener.Start();
            Task.Run(() => RunServer(listener));
        }

        public static void Log(HttpListenerRequest request)
        {
            string pathLocation = logLocation + DateTime.Now.ToShortDateString() + ".txt";
            if (!File.Exists(pathLocation))
                File.Create(pathLocation);

            Console.WriteLine(request.RemoteEndPoint +
                                " " + request.HttpMethod + " /" +
                                request.Url?.AbsoluteUri);

            try
            {
                TextWriter writer = File.AppendText(pathLocation);
                writer.WriteLine(DateTime.Now + " " + request.RemoteEndPoint +
                    " " + request.HttpMethod + " /" +
                    request.Url?.AbsoluteUri);
                writer.Close();
            }
            catch (Exception c)
            {
                Console.WriteLine(c.ToString());
            }
        }

        public static void Log(string LogItem)
        {
            string pathLocation = logLocation + DateTime.Now.ToShortDateString() + ".txt";
            if (!File.Exists(pathLocation))
                File.Create(pathLocation);
            try
            {

                TextWriter writer = File.AppendText(pathLocation);
                writer.WriteLine(DateTime.Now + " " + LogItem);
                writer.Close();
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.ToString());
            }
        }


        

        private static void Respond(HttpListenerResponse response, ResponsePacket resp)
        {
            response.ContentType = resp.ContentType;
            response.ContentLength64 = resp.Data.Length;
            response.OutputStream.Write(resp.Data, 0, resp.Data.Length);
            response.ContentEncoding = resp.Encoding;
            response.StatusCode = (int)HttpStatusCode.OK;
            response.OutputStream.Close();
        }

        private Dictionary<string, object> GetKeyValues(string data, Dictionary<string, object> kv = null)
        {
            kv.IfNull(() => kv = new Dictionary<string, object>());
            data.If(d => d.Length > 0, (d) => d.Split('&').ForEach(keyValue => kv[keyValue.LeftOf("=")] = System.Uri.UnescapeDataString(keyValue.RightOf("="))));

            return kv;
        }


    }
}
