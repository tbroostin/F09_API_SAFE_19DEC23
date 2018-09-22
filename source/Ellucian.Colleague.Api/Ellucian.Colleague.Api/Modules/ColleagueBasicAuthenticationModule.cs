// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Security;
using slf4net;

namespace Ellucian.Colleague.Api.Modules
{
    /// <summary>
    /// Provides an HTTP module that enables basic authentication  with Colleague API.
    /// </summary>
    public class ColleagueBasicAuthenticationModule : IHttpModule
    {
        private const string AuthorizationHeaderKey = "authorization";
        private const string AuthorizationScheme = "basic";
        private const string ProductNameHeaderKey = "X-ProductName";
        private const string ProductVersionHeaderKey = "X-ProductVersion";

        /// <summary>
        /// Disposes of all resources used by this module.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Initializes this module.
        /// </summary>
        /// <param name="context"></param>
        public void Init(HttpApplication context)
        {
            var wrapper = new EventHandlerTaskAsyncHelper(OnAuthenticateRequestAsync);
            context.AddOnBeginRequestAsync(wrapper.BeginEventHandler, wrapper.EndEventHandler);
        }

        /// <summary>
        /// Callback that asynchronously attempts basic authentication by first checking for
        /// an existing session based on the credentials and if not found, attempts to perform
        /// a standard login.
        /// </summary>
        /// <param name="sender">sending object</param>
        /// <param name="e">event arguments</param>
        private async Task OnAuthenticateRequestAsync(object sender, EventArgs e)
        {
            // read header
            var request = HttpContext.Current.Request;
            var authHeader = request.Headers[AuthorizationHeaderKey];
            if (!string.IsNullOrEmpty(authHeader))
            {
                // read header value
                AuthenticationHeaderValue authHeaderValue = null;
                if (AuthenticationHeaderValue.TryParse(authHeader, out authHeaderValue))
                {
                    // value present, proceed if basic auth scheme
                    if (authHeaderValue != null &&
                        authHeaderValue.Scheme.Equals(AuthorizationScheme, StringComparison.OrdinalIgnoreCase) &&
                        !string.IsNullOrEmpty(authHeaderValue.Parameter))
                    {
                        IPrincipal principal = null;

                        // check for an existing token
                        Tuple<bool, IPrincipal> existingSessionTuple = await TryGetExistingSessionAsync(authHeaderValue.Parameter);
                        bool tokenExists = existingSessionTuple.Item1;
                        principal = existingSessionTuple.Item2;

                        if (tokenExists)
                        {
                            SetPrincipal(principal);
                        }
                        else
                        {
                            //login if no token
                            try
                            {
                                Tuple<bool, IPrincipal> authenticationTuple = await AuthenticateUserAsync(authHeaderValue.Parameter);
                                bool validated = authenticationTuple.Item1;
                                principal = authenticationTuple.Item2;

                                if (validated)
                                {
                                    SetPrincipal(principal);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (ex.Message.Contains("password has expired"))
                                {
                                    WriteResponse(HttpStatusCode.Forbidden, ex.Message);
                                }
                                else
                                {
                                    WriteResponse(HttpStatusCode.Unauthorized, ex.Message);
                                }
                                // we're done... this will end the request in the pipeline!
                                HttpContext.Current.Response.End();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to perform a standard login against Colleague using the provided basic credentials as
        /// an asynchronous transaction.
        /// </summary>
        /// <param name="credentials">The base64 encoded basic authentication credentials.</param>
        /// <returns>
        /// Tuple result:
        /// Item1: boolean: Validation, True if successful, False if not.
        /// Item2: IPrincipal: Principal, populated upon successful authentication.
        /// </returns>
        /// <exception cref="System.FormatException">Invalid basic credentials format</exception>
        private async static Task<Tuple<bool, IPrincipal>> AuthenticateUserAsync(string credentials)
        {
            IPrincipal principal = null;
            bool validated = false;
            string username = null;
            string password = null;

            // parse username/password
            try
            {
                var encoding = Encoding.GetEncoding("iso-8859-1");
                credentials = encoding.GetString(Convert.FromBase64String(credentials));
                int separator = credentials.IndexOf(':'); // use the first occrance of a colon as the separator
                username = credentials.Substring(0, separator);
                password = credentials.Substring(separator + 1);
            }
            catch
            {
                throw new FormatException("Invalid basic credentials format");
            }

            var logger = DependencyResolver.Current.GetService<ILogger>();

            // execute login
            Stopwatch sw = null;
            string jwt = null;
            try
            {
                if (logger.IsDebugEnabled)
                {
                    sw = new Stopwatch();
                    sw.Start();
                }
                var sessionRepository = DependencyResolver.Current.GetService<ISessionRepository>();
                string product = null;
                string version = null;
                GetProductNameAndVersion(out product, out version);
                sessionRepository.ProductName = product;
                sessionRepository.ProductVersion = version;
                jwt = await sessionRepository.LoginAsync(username, password);
            }
            catch (Exception e)
            {
                // need to let this bubble up so calling code can return proper HTTP response
                logger.Error(e, "Basic AuthenticateUser error");
                throw;
            }
            finally
            {
                if (logger.IsDebugEnabled && sw != null)
                {
                    sw.Stop();
                    logger.Debug(string.Format("Basic AuthenticateUser: {0} seconds", sw.Elapsed.ToString()));
                }
            }

            // get principal
            try
            {
                if (!string.IsNullOrEmpty(jwt))
                {
                    principal = JwtHelper.CreatePrincipal(jwt);
                    if (principal != null)
                    {
                        validated = true;
                    }
                }
            }
            catch
            {
                validated = false;
            }

            return new Tuple<bool, IPrincipal>(validated, principal);
        }

        /// <summary>
        /// Attempts to retrieve an existing Colleague session as an asynchronous transaction
        /// using the provided basic credentials.
        /// </summary>
        /// <param name="credentials">The base64 encoded basic authentication credentials.</param>
        /// <returns>
        /// Tuple:
        /// Item1: boolean: Validation, True if successful, False if not.
        /// Item2: IPrincipal: Principal, populated upon successful retrieval of an existing session.
        /// </returns>
        private async static Task<Tuple<bool, IPrincipal>> TryGetExistingSessionAsync(string credentials)
        {
            IPrincipal principal = null;
            bool success = false;

            Stopwatch sw = null;
            var logger = DependencyResolver.Current.GetService<ILogger>();
            try
            {
                var sessionRepository = DependencyResolver.Current.GetService<ISessionRepository>();
                if (logger.IsDebugEnabled)
                {
                    sw = new Stopwatch();
                    sw.Start();
                }
                string jwt = await sessionRepository.GetTokenAsync(credentials);
                principal = JwtHelper.CreatePrincipal(jwt);
                if (principal != null)
                {
                    success = true;
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Basic TryGetExistingSession error");
                success = false;
            }
            finally
            {
                if (logger.IsDebugEnabled && sw != null)
                {
                    sw.Stop();
                    logger.Debug(string.Format("Basic TryGetExistingSession: {0} seconds", sw.Elapsed.ToString()));
                }
            }
            return new Tuple<bool, IPrincipal>(success, principal);
        }

        /// <summary>
        /// Sets the current principal.
        /// </summary>
        /// <param name="principal">The principal to set.</param>
        private static void SetPrincipal(IPrincipal principal)
        {
            Thread.CurrentPrincipal = principal;
            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }
        }

        /// <summary>
        /// Writes the provided status code and string content to the HTTP response stream.
        /// </summary>
        /// <param name="statusCode"><see cref="HttpStatusCode"/> to return.</param>
        /// <param name="content">String content to return.</param>
        private static void WriteResponse(HttpStatusCode statusCode, string content)
        {
            HttpContext.Current.Response.StatusCode = (int)statusCode;
            if (!string.IsNullOrEmpty(content))
            {
                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.ContentType = "text/plain";
                HttpContext.Current.Response.Write(content);
            }
        }

        /// <summary>
        /// Gets the values of the product name and product version headers. Both must be present otherwise
        /// default values are returned.
        /// </summary>
        /// <param name="productName">The value of the product name header.</param>
        /// <param name="productVersion">The value of the product version header.</param>
        private static void GetProductNameAndVersion(out string productName, out string productVersion)
        {
            productName = null;
            productVersion = null;

            string productNameHeaderValue = HttpContext.Current.Request.Headers[ProductNameHeaderKey];
            string productVersionHeaderValue = HttpContext.Current.Request.Headers[ProductVersionHeaderKey];

            if (!string.IsNullOrEmpty(productNameHeaderValue))
            {
                productName = productNameHeaderValue;
            }

            if (!string.IsNullOrEmpty(productVersionHeaderValue))
            {
                productVersion = productVersionHeaderValue;
            }

            // both need to be supplied, if not, use defaults
            if (string.IsNullOrEmpty(productName) || string.IsNullOrEmpty(productVersion))
            {
                productName = "WebApi";
                var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                productVersion = string.Format("{0}.{1}", assemblyVersion.Major, assemblyVersion.Minor);
            }
        }

    }
}
