using System.Net;

namespace LeanWebServer
{
    public class Session
    {
        public DateTime LastConnection { get; set; }
        public bool IsAuthorized { get; set; }
        
        public Dictionary<string, Session> Objects { get; set; }

        public Session() 
        {
            Objects = new Dictionary<string, Session>();
            UpdateLastConnection();
        }

        public void UpdateLastConnection() 
        {
            LastConnection = DateTime.Now;
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
            if (sessionMap.ContainsKey(remoteEndpoint.Address))
                return sessionMap[remoteEndpoint.Address];
            else
                sessionMap[remoteEndpoint.Address] = new Session();
            return sessionMap[remoteEndpoint.Address];
        }
    }
}
