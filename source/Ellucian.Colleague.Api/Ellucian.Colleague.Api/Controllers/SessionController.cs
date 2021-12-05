﻿// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using slf4net;
using Ellucian.Dmi.Client;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Offers login, logout, and change password services.
    /// </summary>
    public class SessionController : BaseCompressedApiController
    {
        private readonly ISessionRepository sessionRepository;
        private readonly ILogger logger;
        private readonly ISessionRecoveryService sessionRecoveryService;

        /// <summary>
        /// SessionsController constructor
        /// </summary>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="sessionRepository">Repository of type <see cref="ISessionRepository">ISessionRepository</see></param>
        /// <param name="sessionRecoveryService">Session recovery service</param>
        public SessionController(ILogger logger, ISessionRepository sessionRepository, ISessionRecoveryService sessionRecoveryService)
        {
            this.logger = logger;
            this.sessionRepository = sessionRepository;
            this.sessionRecoveryService = sessionRecoveryService;
        }

        /// <summary>
        /// Log in to Colleague.
        /// </summary>
        /// <param name="credentials">From Body, Login <see cref="Credentials">Credentials</see></param>
        /// <returns><see cref="HttpResponseMessage">HttpResponseMessage</see> with JSON Web Token</returns>
        [Obsolete("Obsolete as of API version 1.12, use PostLogin2Async instead")]
        public async Task<HttpResponseMessage> PostLoginAsync([FromBody]Credentials credentials)
        {
            IEnumerable<string> nameHeaderValues = null;
            IEnumerable<string> versionHeaderValues = null;
            bool hasName = Request.Headers.TryGetValues("X-ProductName", out nameHeaderValues);
            bool hasVersion = Request.Headers.TryGetValues("X-ProductVersion", out versionHeaderValues);
            if (hasName)
            {
                string productName = nameHeaderValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(productName))
                {
                    this.sessionRepository.ProductName = productName;
                }
            }
            if (hasVersion)
            {
                string productVersion = versionHeaderValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(productVersion))
                {
                    this.sessionRepository.ProductVersion = productVersion;
                }
            }

            if (string.IsNullOrEmpty(sessionRepository.ProductName))
            {
                var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                this.sessionRepository.ProductName = "WebApi";
                this.sessionRepository.ProductVersion = string.Format("{0}.{1}", assemblyVersion.Major, assemblyVersion.Minor);
            }

            try
            {
                return new HttpResponseMessage() { Content = new StringContent(await sessionRepository.LoginAsync(credentials.UserId, credentials.Password)) };
            }
            catch (LoginException lex)
            {
                // Check if login failure is from a force change or password expired error (DMI error code 10017 or 10016)
                if (lex.ErrorCode == "10017" || lex.ErrorCode == "10016")
                {
                    if (lex.ErrorCode == "10017")
                        return new HttpResponseMessage(HttpStatusCode.Forbidden) { Content = new StringContent("You must change your password. Please choose a new password.") };
                    else
                        return new HttpResponseMessage(HttpStatusCode.Forbidden) { Content = new StringContent("Your password has expired.  Please choose a new password.") };
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent(lex.Message) };
                }
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent(ex.Message) };
            }
        }

        /// <summary>
        /// Log in to Colleague.
        /// </summary>
        /// <param name="credentials">From Body, Login <see cref="Credentials">Credentials</see></param>
        /// <returns><see cref="HttpResponseMessage">HttpResponseMessage</see> A string representing the Colleague Web API session token to be used with all requests requiring authorization, 
        /// or one of the following failure responses: 
        /// HttpStatusCode.Forbidden : Password has expired, HttpStatusCode.Unauthorized : Invalid credentials provided, or HttpStatusCode.NotFound : Listener not found or not responding
        /// </returns>
        public async Task<HttpResponseMessage> PostLogin2Async([FromBody]Credentials credentials)
        {
            IEnumerable<string> nameHeaderValues = null;
            IEnumerable<string> versionHeaderValues = null;
            bool hasName = Request.Headers.TryGetValues("X-ProductName", out nameHeaderValues);
            bool hasVersion = Request.Headers.TryGetValues("X-ProductVersion", out versionHeaderValues);
            if (hasName)
            {
                string productName = nameHeaderValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(productName))
                {
                    this.sessionRepository.ProductName = productName;
                }
            }
            if (hasVersion)
            {
                string productVersion = versionHeaderValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(productVersion))
                {
                    this.sessionRepository.ProductVersion = productVersion;
                }
            }

            if (string.IsNullOrEmpty(sessionRepository.ProductName))
            {
                var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                this.sessionRepository.ProductName = "WebApi";
                this.sessionRepository.ProductVersion = string.Format("{0}.{1}", assemblyVersion.Major, assemblyVersion.Minor);
            }

            try
            {
                return new HttpResponseMessage() { Content = new StringContent(await sessionRepository.LoginAsync(credentials.UserId, credentials.Password)) };
            }
            catch (LoginException lex)
            {
                // Check if login failure is from a force change or password expired error (DMI error code 10017 or 10016)
                if (lex.ErrorCode == "10017" || lex.ErrorCode == "10016")
                {
                    logger.Info(lex, "Login attempt failed due to expired password.");
                    return new HttpResponseMessage(HttpStatusCode.Forbidden) { Content = new StringContent(lex.Message + "Error: " + lex.ErrorCode) };
                }
                // Check if login failure is due to reaching the maximum number of login attempts.
                else if (lex.ErrorCode == "10014")
                {
                    logger.Info(lex, "Login attempt failed due to too many incorrect login attempts for User: " + credentials.UserId);
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent(lex.Message + "Error: " + lex.ErrorCode) };
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent(lex.Message) };
                }
            }
            catch (ColleagueDmiConnectionException cdce)
            {
                logger.Error("Login attempt failed with ColleagueDmiConnectionException: " + cdce.Message);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("Listener was not found or was unresponsive.") });
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent(ex.Message) };
            }
        }

        /// <summary>
        /// Log in to Colleague using proxy authentication.
        /// </summary>
        /// <param name="proxyCredentials">From Body, <see cref="ProxyCredentials">ProxyCredentials</see></param>
        /// <returns><see cref="HttpResponseMessage">HttpResponseMessage</see> with JSON Web Token</returns>
        [Obsolete("Obsolete as of API version 1.12, use PostProxyLogin2Async instead")]
        public async Task<HttpResponseMessage> PostProxyLoginAsync([FromBody]ProxyCredentials proxyCredentials)
        {
            IEnumerable<string> nameHeaderValues = null;
            IEnumerable<string> versionHeaderValues = null;
            bool hasName = Request.Headers.TryGetValues("X-ProductName", out nameHeaderValues);
            bool hasVersion = Request.Headers.TryGetValues("X-ProductVersion", out versionHeaderValues);
            if (hasName)
            {
                string productName = nameHeaderValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(productName))
                {
                    this.sessionRepository.ProductName = productName;
                }
            }
            if (hasVersion)
            {
                string productVersion = versionHeaderValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(productVersion))
                {
                    this.sessionRepository.ProductVersion = productVersion;
                }
            }

            if (string.IsNullOrEmpty(sessionRepository.ProductName))
            {
                var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                this.sessionRepository.ProductName = "WebApi";
                this.sessionRepository.ProductVersion = string.Format("{0}.{1}", assemblyVersion.Major, assemblyVersion.Minor);
            }

            try
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(await sessionRepository.ProxyLoginAsync(
                        proxyCredentials.ProxyId, proxyCredentials.ProxyPassword, proxyCredentials.UserId))
                };
            }
            catch (LoginException lex)
            {
                // Check if login failure is from a force change or password expired error (DMI error code 10017 or 10016)
                if (lex.ErrorCode == "10017" || lex.ErrorCode == "10016")
                {
                    return new HttpResponseMessage(HttpStatusCode.Forbidden) { Content = new StringContent(lex.Message) };
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent(lex.Message) };
                }
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent(ex.Message) };
            }
        }

        /// <summary>
        /// Log in to Colleague using proxy authentication.
        /// </summary>
        /// <param name="proxyCredentials">From Body, <see cref="ProxyCredentials">ProxyCredentials</see></param>
        /// <returns><see cref="HttpResponseMessage">HttpResponseMessage</see> A string representing the Colleague Web API session token to be used with all requests requiring authorization, 
        /// or one of the following failure responses: 
        /// HttpStatusCode.Forbidden : Password has expired, HttpStatusCode.Unauthorized : Invalid credentials provided, or HttpStatusCode.NotFound : Listener not found or not responding
        /// </returns>
        public async Task<HttpResponseMessage> PostProxyLogin2Async([FromBody]ProxyCredentials proxyCredentials)
        {
            IEnumerable<string> nameHeaderValues = null;
            IEnumerable<string> versionHeaderValues = null;
            bool hasName = Request.Headers.TryGetValues("X-ProductName", out nameHeaderValues);
            bool hasVersion = Request.Headers.TryGetValues("X-ProductVersion", out versionHeaderValues);
            if (hasName)
            {
                string productName = nameHeaderValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(productName))
                {
                    this.sessionRepository.ProductName = productName;
                }
            }
            if (hasVersion)
            {
                string productVersion = versionHeaderValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(productVersion))
                {
                    this.sessionRepository.ProductVersion = productVersion;
                }
            }

            if (string.IsNullOrEmpty(sessionRepository.ProductName))
            {
                var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                this.sessionRepository.ProductName = "WebApi";
                this.sessionRepository.ProductVersion = string.Format("{0}.{1}", assemblyVersion.Major, assemblyVersion.Minor);
            }

            try
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(await sessionRepository.ProxyLoginAsync(
                        proxyCredentials.ProxyId, proxyCredentials.ProxyPassword, proxyCredentials.UserId))
                };
            }
            catch (LoginException lex)
            {
                // Check if login failure is from a force change or password expired error (DMI error code 10017 or 10016)
                if (lex.ErrorCode == "10017" || lex.ErrorCode == "10016")
                {
                    logger.Info(lex, lex.Message);
                    return new HttpResponseMessage(HttpStatusCode.Forbidden) { Content = new StringContent(lex.Message) };
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent(lex.Message) };
                }
            }
            catch (ColleagueDmiConnectionException cdce)
            {
                logger.Error("Login attempt failed with ColleagueDmiConnectionException: " + cdce.Message);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("Listener was not found or was unresponsive.") });
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent(ex.Message) };
            }
        }

        /// <summary>
        /// Log out of Colleague
        /// </summary>
        public async Task PostLogoutAsync()
        {
            /*
             * API 1.2 changes:
             * 1) Token should no longer be sent to this action using a query string parameter.
             *    The standard X-CustomCredentials header must be used.
             * 2) Token argument has been removed so that the API self-doc does not show it with
             *    the endpoint URI.
             * 3) To ensure logout if the query string parameter is used, this action will parse
             *    the query string from the raw request in order to still process the logout, 
             *    BUT will result in a bad request response and a log entry stating that the
             *    token query string parameter was supplied.
             */

            string token = null;
            string tokenParameter = "token";

            // Fetch the token from the credentials header
            IEnumerable<string> xCustomCredentialsHeader = null;
            Request.Headers.TryGetValues(Ellucian.Colleague.Api.Client.ColleagueApiClient.CredentialsHeaderKey, out xCustomCredentialsHeader);
            if (xCustomCredentialsHeader != null && xCustomCredentialsHeader.Count() > 0)
            {
                token = xCustomCredentialsHeader.ToList()[0];
            }

            // Check to see if the token was passed via the query string and if present, use it, but also log as an error!
            var tokenParameterUsed = false;
            var queryParameters = Request.RequestUri.ParseQueryString();
            if (queryParameters != null && queryParameters.Count > 0 && queryParameters.AllKeys.Contains(tokenParameter))
            {
                if (string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(queryParameters[tokenParameter]))
                {
                    token = queryParameters[tokenParameter];
                }
                tokenParameterUsed = true;
                logger.Error("session/logout: token parameter passed via the query string from {0}! The token must be supplied using the credentials header.", Request.RequestUri.Host);
            }

            // logout
            try
            {
                await sessionRepository.LogoutAsync(token);
            }
            catch (Exception ex)
            {
                // Log it but don't re-throw
                logger.Error(ex.ToString());
            }

            // if the token query parameter was used, issue an error
            if (tokenParameterUsed)
            {
                throw CreateHttpResponseException("Logout called using an deprecated request format. The logout request was processed", HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Gets token of user
        /// </summary>
        /// <param name="session"></param>
        /// <returns>User's token</returns>
        public async Task<string> PostTokenAsync([FromBody]LegacyColleagueSession session)
        {
            return await sessionRepository.GetTokenAsync(session.SecurityToken, session.ControlId);
        }

        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="request"><see cref="ChangePassword">ChangePassword</see> request</param>
        /// <returns>Empty <see cref="HttpResponseMessage">HttpResponseMessage</see> if successful</returns>
        /// <exception> <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadResponse when passed data is not acceptable</exception>
        public async Task<HttpResponseMessage> PostNewPasswordAsync([FromBody]ChangePassword request)
        {
            try
            {
                await sessionRepository.ChangePasswordAsync(request.UserId, request.OldPassword, request.NewPassword);
            }
            catch (Exception e)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent(e.Message) };
            }

            // no exceptions? no problem?
            return new HttpResponseMessage();
        }

        /// <summary>
        /// Obtains new JWT that includes new proxy access granted by the specified proxy subject to the current user. This also updates
        /// the web session token in Colleague for the current user. Note: "proxy subject" means the user that has granted
        /// proxy access to the current user.
        /// </summary>
        /// <param name="proxySubject">The proxy subject. Only the ID is required. If this ID is empty, or is 
        /// the same as the Current user ID, then any previously assigned proxy subject claims will be removed.</param>
        /// <returns></returns>
        [Authorize]
        public async Task<HttpResponseMessage> PutSessionProxySubjectsAsync([FromBody]ProxySubject proxySubject)
        {
            try
            {
                string proxySubjectID = (proxySubject == null) ?
                                            string.Empty :
                                            (proxySubject.Id ?? string.Empty);

                string currentUserId = "";
                var currentUserPrincipal = Microsoft.IdentityModel.Claims.ClaimsPrincipal.CreateFromPrincipal(User);
                var currentUserIdClaim = currentUserPrincipal.Identities.First().Claims.FirstOrDefault(
                    c => c.ClaimType == Ellucian.Web.Security.ClaimConstants.PersonId);
                if (currentUserIdClaim != null)
                {
                    currentUserId = currentUserIdClaim.Value;
                }

                if (currentUserId == proxySubjectID)
                {
                    // If proxy subject ID is the current user's ID (proxying oneself), then the proxy subject claims  
                    // are to beremoved from the token. To achieve this, an empty string must be passed in to the repo method.
                    proxySubjectID = "";
                }

                return new HttpResponseMessage()
                {
                    Content =
                        new StringContent(await sessionRepository.SetProxySubjectAsync(proxySubjectID))
                };
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(
                        string.Format("Problem occurred obtaining proxy access for proxy subject {0}, please contact the system administrator.", proxySubject.Id))
                };
            }
        }

        /// <summary>
        /// Executes a request for a reset password token to be emailed to the user to enable them to complete a password reset.
        /// </summary>
        /// <param name="tokenRequest">The reset password token request</param>
        /// <returns>A successful 202 response. For security reasons, failures will not be reported.</returns>
        /// <accessComments>User must have ADMIN.RESET.ALL.PASSWORDS permission to reset passwords.</accessComments>
        [HttpPost]
        [Authorize]
        public async Task<HttpResponseMessage> PostResetPasswordTokenRequestAsync([FromBody]PasswordResetTokenRequest tokenRequest)
        {
            try
            {
                await sessionRecoveryService.RequestPasswordResetTokenAsync(tokenRequest.UserId, tokenRequest.EmailAddress);
                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to request password reset token request.");
                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
        }

        /// <summary>
        /// Request recovery of a user ID (user name) given supporting information and email.
        /// </summary>
        /// <param name="userIdRecoveryRequest">User ID Recovery Request</param>
        /// <returns>A successful 202 response. For security reasons, failures will not be reported.</returns>
        /// <accessComments>User must have ADMIN.RESET.ALL.PASSWORDS permission to reset passwords.</accessComments>
        [HttpPost]
        [Authorize]
        public async Task<HttpResponseMessage> PostUserIdRecoveryRequestAsync([FromBody]UserIdRecoveryRequest userIdRecoveryRequest)
        {
            try
            {
                await sessionRecoveryService.RequestUserIdRecoveryAsync(userIdRecoveryRequest.FirstName, userIdRecoveryRequest.LastName, userIdRecoveryRequest.EmailAddress);
                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to recover user ID.");
                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
        }

        /// <summary>
        /// Resets a password using a password reset token
        /// </summary>
        /// <param name="resetPassword">Reset password</param>
        /// <returns></returns>
        /// <accessComments>User must have ADMIN.RESET.ALL.PASSWORDS permission to reset passwords.</accessComments>
        [HttpPost]
        [Authorize]
        public async Task<HttpResponseMessage> PostResetPasswordAsync([FromBody]ResetPassword resetPassword)
        {
            try
            {
                if (resetPassword == null || string.IsNullOrEmpty(resetPassword.UserId) || string.IsNullOrEmpty(resetPassword.ResetToken) || string.IsNullOrEmpty(resetPassword.NewPassword))
                {
                    throw new ArgumentNullException("resetPassword");
                }
                await sessionRecoveryService.ResetPasswordAsync(resetPassword.UserId, resetPassword.ResetToken, resetPassword.NewPassword);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (PasswordComplexityException pce)
            {
                throw CreateHttpResponseException("Password complexity failure");
            }
            catch (PasswordUsedException pue)
            {
                throw CreateHttpResponseException("Password used recently failure");
            }
            catch (PasswordResetTokenExpiredException prtee)
            {
                throw CreateHttpResponseException("Password reset token expired failure");
            }
            catch (Web.Security.PermissionsException pe)
            {
                throw CreateHttpResponseException("Unable to reset password");
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException("Unable to reset password");
            }
        }

        /// <summary>
        /// Sync user's Colleague web token's timeout on app server side.
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Authorize]
        public async Task<HttpResponseMessage> PutSyncAsync()
        {
            try
            {
                var currentUserPrincipal = Microsoft.IdentityModel.Claims.ClaimsPrincipal.CreateFromPrincipal(User);
                var currentUserTokenClaim = currentUserPrincipal.Identities.First().Claims.FirstOrDefault(
                    c => c.ClaimType == Ellucian.Web.Security.ClaimConstants.SecurityTokenControlId);

                if (currentUserTokenClaim == null 
                    || string.IsNullOrWhiteSpace(currentUserTokenClaim.Value)
                    || currentUserTokenClaim.Value.Split('*').Length != 2)
                {
                    logger.Error("Unable to sync session due to invalid token claim.");
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
                string secToken = currentUserTokenClaim.Value.Split('*')[0];
                string controlId = currentUserTokenClaim.Value.Split('*')[1];
                await sessionRepository.SyncSessionAsync(secToken, controlId);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                logger.Error(e, "Unexpected error occurred syncing web session.");
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }
    }
}
