using LeanWebServer.Enums;
using LeanWebServer.Extentions;
using System.Text;
using System.Xml.XPath;

namespace LeanWebServer
{
    public class ResponsePacket
    {
        public string Redirect { get; set; }

        public ServerErrors Error { get; set; }

        public byte[] Data { get; set; }
        public string ContentType { get; set; }
        public Encoding Encoding { get; set; }
    }

    internal class ExtensionInfo
    {
        public string ContentType { get; set; }
        public Func<Session, string, string, ExtensionInfo, ResponsePacket> Loader { get; set; }
    }

    public class Router
    {
        public string WebsitePath { get; set; }

        public const string POST = "post";
        public const string GET = "get";
        public const string PUT = "put";
        public const string DELETE = "delete";
        private List<Route> routes;

        private Dictionary<string, ExtensionInfo> extFolderMap;

        public Router()
        {
            routes = new List<Route>();
            extFolderMap = new Dictionary<string, ExtensionInfo>()
            {
                {"ico", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/ico"}},
                {"png", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/png"}},
                {"jpg", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/jpg"}},
                {"gif", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/gif"}},
                {"bmp", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/bmp"}},
                {"html", new ExtensionInfo() {Loader=PageLoader, ContentType="text/html"}},
                {"css", new ExtensionInfo() {Loader=FileLoader, ContentType="text/css"}},
                {"js", new ExtensionInfo() {Loader=FileLoader, ContentType="text/javascript"}},
                {"", new ExtensionInfo() {Loader=PageLoader, ContentType="text/html"}},
            };
        }

        public ResponsePacket Route(Session session, string verb, string path, Dictionary<string, object> kvParams)
        {
            string ext = path.RightOfRightmostOf(".");
            ExtensionInfo extinfo;
            ResponsePacket ret = null;
            verb = verb.ToLower();

            if (extFolderMap.TryGetValue(ext, out extinfo))
            {
                string wpath = path.Substring(1).Replace('/', '\\');
                string fullPath = Path.Combine(WebsitePath, wpath);

                Route handler = routes.SingleOrDefault(r => r.Verb.ToLower() == verb && path == r.Path);

                if (handler != null)
                {
                    ResponsePacket handlerResponse = handler.Handler.Handle(session, kvParams, path);

                    if (handlerResponse == null)
                    {
                        if (!session.IsAuthorized && path != "/User/login.html")
                        {
                            ret = new ResponsePacket() { Error = ServerErrors.NotAuthorized, Redirect = "/User/login.html" };
                            ret.Data = Encoding.UTF8.GetBytes("Unauthenticated User");
                            return ret;
                        }
                        else
                        {
                            ret = extinfo.Loader(session, fullPath, ext, extinfo);
                        }
                    }
                    else
                    {
                        ret = handlerResponse;
                    }

                }
                else
                {
                    if (!session.IsAuthorized && path != "/User/login.html")
                    {
                        ret = new ResponsePacket() { Error = ServerErrors.NotAuthorized, Redirect = "/User/login.html" };
                        ret.Data = Encoding.UTF8.GetBytes("Unauthenticated User");
                        return ret;
                    }
                    else
                    {
                        ret = extinfo.Loader(session, fullPath, ext, extinfo);
                    }
                    //ret = new ResponsePacket() { Error = ServerErrors.HandlerNotRegistered, Redirect = "handlerNotRegistered.html" };
                }
            }
            else
            {
                ret = new ResponsePacket() { Error = ServerErrors.UnknownType, Redirect = "unknownType.html" };
            }

            return ret;
        }

        private ResponsePacket ImageLoader(Session session, string fullPath, string ext, ExtensionInfo extInfo)
        {
            if (File.Exists(fullPath))
            {
                FileStream fStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fStream);
                ResponsePacket response = new ResponsePacket() { Data = br.ReadBytes((int)fStream.Length), ContentType = extInfo.ContentType };
                br.Close();
                fStream.Close();

                return response;
            }
            else
            {
                ResponsePacket response = new ResponsePacket() { Error = ServerErrors.FileNotFound };
                return response;
            }
        }

        private ResponsePacket FileLoader(Session session, string fullPath, string ext, ExtensionInfo extensionInfo)
        {
            if (File.Exists(fullPath))
            {
                string text = File.ReadAllText(fullPath);
                text = Server.DefaultPostProcessing(session, text);
                ResponsePacket response = new ResponsePacket() { Data = Encoding.UTF8.GetBytes(text), ContentType = extensionInfo.ContentType, Encoding = Encoding.UTF8 };
                return response;

            }
            else
            {
                ResponsePacket response = new ResponsePacket() { Error = ServerErrors.FileNotFound };
                return response;
            }
        }

        private ResponsePacket PageLoader(Session session, string fullPath, string ext, ExtensionInfo extInfo)
        {
            ResponsePacket ret = new ResponsePacket();

            if (fullPath == WebsitePath)
            {
                ret = Route(session, GET, "/index.html", null);
            }
            else
            {
                if (string.IsNullOrEmpty(ext))
                    fullPath = fullPath + ".html";

                fullPath = WebsitePath + "\\Pages" + fullPath.RightOfSubstring(WebsitePath);
                ret = FileLoader(session, fullPath, ext, extInfo);
            }
            return ret;
        }

        public void AddRoute(Route route)
        {
            routes.Add(route);
        }

    }

}
