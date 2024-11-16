using LeanWebServer.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanWebServer
{
    public abstract class RouteHandler
    {
        protected readonly Server server;
        protected Func<Session, Dictionary<string, object>, ResponsePacket> handler;

        public RouteHandler(Server server, Func<Session, Dictionary<string, object>, ResponsePacket> handler) 
        {
            this.server = server;
            this.handler = handler;
        }

        public virtual ResponsePacket Handle(Session session, Dictionary<string, object> parms) 
        {
            return InvokeHandler(session, parms);
        }

        protected ResponsePacket InvokeHandler(Session session, Dictionary<string, object> parms)
        {
            ResponsePacket packet = null;
            handler.IfNotNull((h) => packet = h(session, parms));

            return packet;
        }
    }

    public class AnonymousRouteHandler : RouteHandler
    {

        public AnonymousRouteHandler(Server server, Func<Session, Dictionary<string, object>, ResponsePacket> handler = null) : base(server, handler) 
        {
            
        }
    }

    public class AuthorizedRouteHandler : RouteHandler
    {

        public AuthorizedRouteHandler(Server server, Func<Session, Dictionary<string, object>, ResponsePacket> handler = null) : base(server, handler)
        {
        }

        public override ResponsePacket Handle(Session session, Dictionary<string, object> parms)
        {
            ResponsePacket result;
            if (session.IsAuthorized)
            {
                result = InvokeHandler(session, parms);
            }
            else 
            {
                result =  server.Redirect(server.OnError(Enums.ServerErrors.NotAuthorized));
            }

            return result;
        }
    }

    public class AuthorizedExpirableRouteHandler : AuthorizedRouteHandler
    {
        public AuthorizedExpirableRouteHandler(Server server, Func<Session, Dictionary<string, object>, ResponsePacket> handler = null) : base(server, handler) 
        {
        }
        public override ResponsePacket Handle(Session session, Dictionary<string, object> parms)
        {
            ResponsePacket result = null;
            if (session.IsExpired(Server.ExpirationTimeInSeconds))
            {
                session.IsAuthorized = false;
                result = server.Redirect(server.OnError(Enums.ServerErrors.ExpiredSession));
            }
            else 
            {
                result = InvokeHandler(session, parms);
            }

            return result;
        }
    }
}
