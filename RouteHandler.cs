using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanWebServer
{
    public abstract class RouteHandler
    {
        protected Func<Session, Dictionary<string, object>, string> handler;

        public RouteHandler(Func<Session, Dictionary<string, object>, string> handler) 
        {
            this.handler = handler;
        }

        public abstract string Handle(Session session, Dictionary<string, object> parms);
    }

    public class AnonymousRouteHandler : RouteHandler
    {
        public AnonymousRouteHandler(Func<Session, Dictionary<string, object>, string> handler) : base(handler) 
        {

        }

        public override string Handle(Session session, Dictionary<string, object> parms)
        {
            return handler(session, parms);
        }
    }

    public class AuthorizedRouteHandler : RouteHandler
    {
        private readonly Server server;
        public AuthorizedRouteHandler(Func<Session, Dictionary<string, object>, string> handler, Server server) : base(handler)
        {
            this.server = server;
        }

        public override string Handle(Session session, Dictionary<string, object> parms)
        {
            string result;
            if (session.IsAuthorized)
            {
                result = handler(session, parms);
            }
            else 
            {
                result = server.OnError(Enums.ServerErrors.NotAuthorized);
            }

            return result;
        }
    }

    public class AuthorizedExpirableRouteHandler : AuthorizedRouteHandler
    {
        private readonly Server server;
        public AuthorizedExpirableRouteHandler(Func<Session, Dictionary<string, object>, string> handler, Server server) : base(handler, server) 
        {
            this.server = server;
        }
        public override string Handle(Session session, Dictionary<string, object> parms)
        {
            string result = string.Empty;
            if (session.IsExpired(Server.ExpirationTimeInSeconds))
            {
                session.IsAuthorized = false;
                result = server.OnError(Enums.ServerErrors.ExpiredSession);
            }
            else 
            {
                result = handler(session, parms);
            }

            return result;
        }
    }
}
