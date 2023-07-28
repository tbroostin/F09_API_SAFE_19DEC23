// Copyright 2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Security;
using slf4net;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Ellucian.Web.Http.Configuration;
using System.Linq;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Ellucian.Colleague.Api.Modules
{
    /// <summary>
    /// Provides an HTTP module that enables bearer (OAuth) authentication  with Colleague API.
    /// </summary>
    public class ColleagueBearerAuthenticationModule : IHttpModule
    {
        private const string AuthorizationHeaderKey = "authorization";
        private const string AuthorizationScheme = "bearer";
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
            var wrapper = new EventHandlerTaskAsyncHelper(OnBearerAuthenticateRequestAsync);
            context.AddOnBeginRequestAsync(wrapper.BeginEventHandler, wrapper.EndEventHandler);
        }

        /// <summary>
        /// Callback that asynchronously attempts Bearer Token authentication by first checking for
        /// an existing session based on the credentials and if not found, attempts to perform
        /// a standard login.
        /// </summary>
        /// <param name="sender">sending object</param>
        /// <param name="e">event arguments</param>
        private async Task OnBearerAuthenticateRequestAsync(object sender, EventArgs e)
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
                        var logger = DependencyResolver.Current.GetService<ILogger>();
                        var settings = DependencyResolver.Current.GetService<XmlSettingsRepository>().Get();

                        // Validate the Token
                        var validToken = await ValidateCurrentToken(authHeaderValue.Parameter, settings, logger);

                        if (validToken)
                        {
                            // Get Person Login ID from GUID
                            var handler = new JwtSecurityTokenHandler();
                            var jsonToken = handler.ReadJwtToken(authHeaderValue.Parameter);
                            var personGuid = jsonToken.Subject;

                            // Get Oauth Proxy User Name and Password
                            string proxyId = string.Empty;
                            string password = string.Empty;
                            if (settings != null && settings.OauthSettings != null)
                            {
                                proxyId = settings.OauthSettings.OauthProxyLogin;
                                password = settings.OauthSettings.OauthProxyPassword;
                            }

                            // check for an existing token
                            Tuple<bool, string,  IPrincipal> existingSessionTuple = await TryGetExistingSessionAsync(proxyId, password, personGuid);
                            bool tokenExists = existingSessionTuple.Item1;
                            var username = existingSessionTuple.Item2;
                            var principal = existingSessionTuple.Item3;

                            if (tokenExists & principal != null)
                            {
                                SetPrincipal(principal);
                            }
                            else
                            {
                                //login if no token
                                try
                                {
                                    if (string.IsNullOrEmpty(proxyId) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(personGuid))
                                    {
                                        throw new Exception("Login failed: The Oauth Token represents an invalid username or password. Please try again.");
                                    }
                                    Tuple<bool, IPrincipal> authenticationTuple = await AuthenticateUserAsync(proxyId, password, username, personGuid, logger);
                                    bool validated = authenticationTuple.Item1;
                                    principal = authenticationTuple.Item2;

                                    if (validated && principal != null)
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
        }

        /// <summary>
        /// Attempts to perform a standard login against Colleague using the provided basic credentials as
        /// an asynchronous transaction.
        /// </summary>
        /// <param name="proxyId">The Oauth Proxy UserName.</param>
        /// <param name="password">The Oauth Proxy password.</param>
        /// <param name="personGuid">The person guid for context extracted from the token.</param>
        /// <param name="username">The person userID for logging into Colleague.</param>
        /// <param name="logger">The system logger interface.</param>
        /// <returns>
        /// Tuple result:
        /// Item1: boolean: Validation, True if successful, False if not.
        /// Item2: IPrincipal: Principal, populated upon successful authentication.
        /// </returns>
        /// <exception cref="System.FormatException">Invalid basic credentials format</exception>
        private async static Task<Tuple<bool, IPrincipal>> AuthenticateUserAsync(string proxyId, string password, string username, string personGuid, ILogger logger)
        {
            IPrincipal principal = null;
            bool validated = false;

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
                jwt = await sessionRepository.ProxyLoginAsync(proxyId, password, username, personGuid);
            }
            catch (Exception e)
            {
                // need to let this bubble up so calling code can return proper HTTP response
                logger.Error(e, "Bearer AuthenticateUser error");
                throw;
            }
            finally
            {
                if (logger.IsDebugEnabled && sw != null)
                {
                    sw.Stop();
                    logger.Debug(string.Format("Bearer AuthenticateUser: {0} seconds", sw.Elapsed.ToString()));
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
        /// <param name="proxyId">The Oauth Proxy ID from settings.config.</param>
        /// <param name="password">The Oauth Password from settings.config.</param>
        /// <param name="personGuid">The GUID for the person from the credentials subject used for context.</param>
        /// <returns>
        /// Tuple:
        /// Item1: boolean: Validation, True if successful, False if not.
        /// Item2: IPrincipal: Principal, populated upon successful retrieval of an existing session.
        /// </returns>
        private async static Task<Tuple<bool, string, IPrincipal>> TryGetExistingSessionAsync(string proxyId, string password, string personGuid)
        {
            IPrincipal principal = null;
            bool success = false;
            string username = "";

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

                var jwtTuple = await sessionRepository.GetOauthProxyTokenAsync(proxyId, password, personGuid);
                string jwt = jwtTuple.Item1;
                username = jwtTuple.Item2;

                if (!string.IsNullOrEmpty(jwt))
                {
                    principal = JwtHelper.CreatePrincipal(jwt);
                    if (principal != null)
                    {
                        success = true;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Bearer TryGetExistingSession error");
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
            return new Tuple<bool, string, IPrincipal>(success, username, principal);
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

        /// <summary>
        /// Validate the input security token
        /// </summary>
        /// <param name="token">Jwt Bearer Security Token</param>
        /// <param name="settings"></param>
        /// <param name="logger"></param>
        /// <returns>
        /// boolean: Validation, True if successful, False if not.
        /// </returns>
        private async static Task<bool> ValidateCurrentToken(string token, Settings settings, ILogger logger)
        {
            Stopwatch sw = null;

            var myIssuer = "https://oauth.prod.10005.elluciancloud.com";
            var myAudience = "https://elluciancloud.com";

            if (settings != null && settings.OauthSettings != null)
            {
                myIssuer = settings.OauthSettings.OauthIssuerUrl;
            }

            try
            {
                IConfigurationManager<OpenIdConnectConfiguration> configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>($"{myIssuer}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
                OpenIdConnectConfiguration openIdConfig = await configurationManager.GetConfigurationAsync(CancellationToken.None);

                var tokenHandler = new JwtSecurityTokenHandler();
                if (logger.IsDebugEnabled)
                {
                    sw = new Stopwatch();
                    sw.Start();
                }
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = myIssuer,
                    ValidAudience = myAudience,
                    IssuerSigningKeys = openIdConfig != null ? openIdConfig.SigningKeys : null
                };
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                if (ex.Message.Contains("expired"))
                {
                    WriteResponse(HttpStatusCode.Unauthorized, "Login failed: The oauth token has expired.");
                }
                else
                {
                    WriteResponse(HttpStatusCode.Unauthorized, "Login failed: The oauth token validation failed.");
                }
                // we're done... this will end the request in the pipeline!
                HttpContext.Current.Response.End();
                return false;
            }
            finally
            {
                if (logger.IsDebugEnabled && sw != null)
                {
                    sw.Stop();
                    logger.Debug(string.Format("Oauth Token Validation: {0} seconds", sw.Elapsed.ToString()));
                }
            }

            return true;
        }

        /// <summary>
        /// Get a requested claim type from the bearer security token
        /// </summary>
        /// <param name="token"></param>
        /// <param name="claimType"></param>
        /// <returns></returns>
        private static string GetClaim(string token, string claimType)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.ReadJwtToken(token);

            var stringClaimValue = securityToken.Claims.First(claim => claim.Type == claimType).Value;
            return stringClaimValue;
        }
    }
}
