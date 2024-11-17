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
        protected Func<Session, Dictionary<string, object>, string, ResponsePacket> handler;

        public RouteHandler(Server server, Func<Session, Dictionary<string, object>, string, ResponsePacket> handler) 
        {
            this.server = server;
            this.handler = handler;
        }

        public virtual ResponsePacket Handle(Session session, Dictionary<string, object> parms, string path) 
        {
            return InvokeHandler(session, parms, path);
        }

        protected ResponsePacket InvokeHandler(Session session, Dictionary<string, object> parms, string path)
        {
            ResponsePacket packet = null;
            handler.IfNotNull((h) => packet = h(session, parms, path));

            return packet;
        }
    }

    public class AnonymousRouteHandler : RouteHandler
    {

        public AnonymousRouteHandler(Server server, Func<Session, Dictionary<string, object>, string, ResponsePacket> handler = null) : base(server, handler) 
        {
            
        }
    }

    public class AuthorizedRouteHandler : RouteHandler
    {

        public AuthorizedRouteHandler(Server server, Func<Session, Dictionary<string, object>, string, ResponsePacket> handler = null) : base(server, handler)
        {
        }

        public override ResponsePacket Handle(Session session, Dictionary<string, object> parms, string path)
        {
            ResponsePacket result;
            if (session.IsAuthorized)
            {
                result = InvokeHandler(session, parms, path);
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
        public AuthorizedExpirableRouteHandler(Server server, Func<Session, Dictionary<string, object>, string, ResponsePacket> handler = null) : base(server, handler) 
        {
        }
        public override ResponsePacket Handle(Session session, Dictionary<string, object> parms, string path)
        {
            ResponsePacket result = null;
            if (session.IsExpired(Server.ExpirationTimeInSeconds))
            {
                session.IsAuthorized = false;
                result = server.Redirect(server.OnError(Enums.ServerErrors.ExpiredSession));
            }
            else 
            {
                result = InvokeHandler(session, parms, path);
            }

            return result;
        }
    }
}
