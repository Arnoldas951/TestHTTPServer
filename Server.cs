﻿using LeanWebServer.Enums;
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

        /// <summary>
        /// For testing, never expire, always authorized
        /// </summary>
        public static Action<Session, HttpListenerContext> onRequest;


        private static HttpListener _listener;
        public static int maxSimultaneousConnections = 20;
        private static Semaphore sem = new Semaphore(maxSimultaneousConnections, maxSimultaneousConnections);
        private static string logLocation = @"D:\Logs\";
        public Router router;
        private SessionManager sessionManager;
        public const int ExpirationTimeInSeconds = 120;
        public Func<ServerErrors, string> OnError { get; set; }

        public static string validationTokenScript = "<%AntiForgeryToken%>";
        public static string validationTokenName = "__CSRFToken__";

        public Server()
        {
            router = new Router();
            sessionManager = new SessionManager();
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
            try
            {
                Session session = sessionManager.GetSession(context.Request.RemoteEndPoint);
                string path = context.Request.RawUrl.LeftOf("?");
                string verb = context.Request.HttpMethod;
                string parms = context.Request.RawUrl.RightOf("?");
                Dictionary<string, object> kvParams = GetKeyValues(parms);
                string data = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding).ReadToEnd();
                GetKeyValues(data, kvParams);
                Log(kvParams);
                Log(parms);
                if (verb == Router.POST && !verifyCSRF(session, kvParams))
                {
                    //context.Response.OutputStream.Close();
                    resp = router.Route(session, "get", OnError(ServerErrors.NotAuthorized), null);
                    resp.Redirect = OnError(ServerErrors.NotAuthorized);
                }
                else
                {
                    resp = router.Route(session, verb, path, kvParams);
                    session.UpdateLastConnection();

                    if (resp.Error != Enums.ServerErrors.OK)
                    {
                        resp.Redirect = OnError(resp.Error);
                    }

                }
                Respond(context.Request, context.Response, resp);

                Console.WriteLine("Connected: " + context.Request.UserAgent);
                sem.Release();
                Log(context.Request);

            }
            catch (Exception ex)
            {
                Log(ex.Message);
                Log(ex.StackTrace);
                resp = new ResponsePacket() { Redirect = OnError(ServerErrors.ServerError) };
            }

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

        public static void Log(Dictionary<string, object> kvparamas)
        {
            kvparamas.ForEach(f => Console.WriteLine(f.Key + " " + f.Value));
        }




        private static void Respond(HttpListenerRequest request, HttpListenerResponse response, ResponsePacket resp)
        {
            if (string.IsNullOrEmpty(resp.Redirect))
            {
                response.ContentType = resp.ContentType;
                response.ContentLength64 = resp.Data.Length;
                response.OutputStream.Write(resp.Data, 0, resp.Data.Length);
                response.ContentEncoding = resp.Encoding;
                response.StatusCode = (int)HttpStatusCode.OK;
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.Redirect;
                response.Redirect("http://" + request.UserHostAddress + resp.Redirect);
            }

            response.OutputStream.Close();
        }

        private Dictionary<string, object> GetKeyValues(string data, Dictionary<string, object> kv = null)
        {
            kv.IfNull(() => kv = new Dictionary<string, object>());
            data.If(d => d.Length > 0, (d) => d.Split('&').ForEach(keyValue => kv[keyValue.LeftOf("=")] = System.Uri.UnescapeDataString(keyValue.RightOf("="))));

            return kv;
        }

        public ResponsePacket Redirect(string url, string parm = null)
        {
            ResponsePacket packet = new ResponsePacket() { Redirect = url };

            parm.IfNotNull((p) => packet.Redirect += "?" + p);

            return packet;
        }

        public static string DefaultPostProcessing(Session session, string html)
        {
            string result = html.Replace(validationTokenScript, "<input name='" + validationTokenName + "'type='hidden' value='" + session.Objects[validationTokenName].ToString()
                + " id='#__csrf__'" + "/>");
            return result;
        }

        private bool verifyCSRF(Session session, Dictionary<string, object> kvParams)
        {
            bool result = true;
            object token;
            if (kvParams.TryGetValue(validationTokenName, out token))
            {
                result = session.Objects[validationTokenName].ToString() == token.ToString();
            }
            else
            {
                Log("CSRF didnt pass");
                result = false;
            }
            return result;
        }


    }
}
