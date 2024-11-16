using System.Net;

namespace LeanWebServer
{
    public class Session
    {
        public DateTime LastConnection { get; set; }
        public bool IsAuthorized { get; set; }

        public Dictionary<string, object> Objects { get; set; }

        public Session()
        {
            Objects = new Dictionary<string, object>();
            UpdateLastConnection();
        }

        public void UpdateLastConnection()
        {
            LastConnection = DateTime.Now;
        }

        public object this[string objectKey]
        {
            get
            {
                object val = null;
                Objects.TryGetValue(objectKey, out val);

                return val;
            }

            set { Objects[objectKey] = value; }
        }
        public bool IsExpired(int expirationInSeconds)
        {
            return (DateTime.Now - LastConnection).Seconds > expirationInSeconds;
        }
    }

    public class SessionManager
    {
        protected Dictionary<IPAddress, Session> sessionMap = new Dictionary<IPAddress, Session>();

        public SessionManager()
        {
            sessionMap = new Dictionary<IPAddress, Session>();
        }

        public Session GetSession(IPEndPoint remoteEndpoint)
        {
            Session session;

            if (!sessionMap.TryGetValue(remoteEndpoint.Address, out session))
            {
                session = new Session();

                session[Server.validationTokenName] = Guid.NewGuid().ToString();
                sessionMap[remoteEndpoint.Address] = session;
            }
           
            return session;
        }
    }
}
