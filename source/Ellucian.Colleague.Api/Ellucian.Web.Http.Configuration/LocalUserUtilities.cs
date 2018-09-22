// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using System.Web;
using Ellucian.Web.Security;

namespace Ellucian.Web.Http.Configuration
{
    public class LocalUserUtilities
    {
        private static readonly string cookieRootName = "ColleagueWebApi";

        public static string CookieId
        {
            get 
            {
                string websiteName = System.Web.Hosting.HostingEnvironment.ApplicationHost.GetSiteName();
                if (string.IsNullOrEmpty(websiteName))
                {
                    websiteName = "unknown";
                }
                websiteName = websiteName.Replace(" ", "_");
                return string.Join("_", cookieRootName, websiteName); 
            }
        }

        public static IPrincipal GetCurrentUser(HttpRequestBase request)
        {
            var cookie = request.Cookies[CookieId];
            var cookieValue = cookie == null ? null : cookie.Value;
            if (!string.IsNullOrEmpty(cookieValue))
            {
                var token = cookieValue.Split('*')[1];
                var principal = JwtHelper.CreatePrincipal(token);
                return principal;
            }

            return null;
        }

        public static HttpCookie GetCookie(HttpRequestBase request)
        {
            return request.Cookies[CookieId];
        }

        public static void ParseCookie(HttpCookie cookie, out string baseUrl, out string token)
        {
            baseUrl = null;
            token = null;

            if (cookie != null)
            {
                var cookieValue = cookie == null ? null : cookie.Value;
                if (!string.IsNullOrEmpty(cookieValue))
                {
                    string[] data = cookieValue.Split('*');
                    if (data != null)
                    {
                        if (data.Length > 0)
                        {
                            baseUrl = data[0];
                        }
                        if (data.Length > 1)
                        {
                            token = data[1];
                        }
                    }
                }
            }
        }

        public static HttpCookie CreateCookie(string baseUrl, string token)
        {
            return new HttpCookie(CookieId, baseUrl + "*" + token);
        }

        public static HttpCookie CreateExpiredCookie()
        {
            return new HttpCookie(CookieId) { Expires = DateTime.Now.AddYears(-1) };
        }

        public static bool IsLoggedIn(HttpRequestBase request)
        {
            return GetCookie(request) != null;
        }
    }
}
