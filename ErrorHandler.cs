using LeanWebServer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanWebServer
{
    public static class ErrorHandler
    {
        public static string ErrorHandling(ServerErrors error) 
        {
            return error switch
            {
                ServerErrors.ExpiredSession => "/ErrorPages/expiredSession.html",
                ServerErrors.ServerError => "/ErrorPages/serverError.html",
                ServerErrors.UnknownType => "/ErrorPages/unknownType.html",
                ServerErrors.NotAuthorized => "/ErrorPages/notAuthorized.html",
                ServerErrors.FileNotFound => "/ErrorPages/fileNotFound.html",
                ServerErrors.PageNotFound=> "/ErrorPages/pageNotFound.html",
                _=> "/ErrorPages/fileNotFound.html"
            };
        }

    }
}
