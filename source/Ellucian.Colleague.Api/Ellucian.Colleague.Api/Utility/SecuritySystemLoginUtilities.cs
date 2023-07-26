
using Ellucian.Colleague.Api.Client;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System.Web;
using System.Web.Mvc;

namespace Ellucian.Web.Http.Utilities
{
    /// <summary>
    /// SystemSystemLoginUtilities that contains static properties useful for interacting with the API as a security system login.
    /// </summary>
    public static class SecuritySystemLoginUtilities
    {
        private static ILogger Logger
        {
            get
            {
                var logger = DependencyResolver.Current.GetService<ILogger>();
                return logger;
            }
        }

        /// <summary>
        /// Gets a service client for use in security system applications that require elevated privileges the user
        /// themselves will not have.
        /// </summary>
        /// <returns>An API client authenticated with security system login credentials</returns>
        public static ColleagueApiClient GetSecuritySystemLoginApiClient(HttpCookie cookie)
        {
            LocalUserUtilities.ParseCookie(cookie, out string baseUrl, out string token);
            var client = new ColleagueApiClient(baseUrl, 2, Logger)
            {
                Credentials = token
            };
            return client;
        }
    }

}
