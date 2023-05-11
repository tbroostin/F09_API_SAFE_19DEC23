// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using slf4net;
using Ellucian.Web.Security;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Exposure of User information
    /// </summary>
    [Authorize]
    public class UsersController : BaseCompressedApiController
    {
        private readonly IUserRepository userRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private readonly IProxyService proxyService;
        private readonly ISelfservicePreferencesService selfservicePreferencesService;
        private const string permissionExceptionMessage = "User does not have permission to access the requested information";
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string invalidPermissionsErrorMessage = "The current user does not have the permissions to perform the requested operation.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";
        /// <summary>
        /// UsersController constructor
        /// </summary>
        /// <param name="userRepository">Repository of type <see cref="IUserRepository">IUserRepository</see></param>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="logger">Interface to logger of type <see cref="ILogger"/>ILogger</param>
        /// <param name="proxyService">Interface to the proxy coordination service of type <see cref="IProxyService"/>IProxyService</param>
        /// <param name="selfservicePreferencesService">Interface to the SelfservicePreferences coordination service of type <see cref="ISelfservicePreferencesService"/></param>
        public UsersController(IUserRepository userRepository, IAdapterRegistry adapterRegistry, ILogger logger, IProxyService proxyService, ISelfservicePreferencesService selfservicePreferencesService)
        {
            if (userRepository == null)
            {
                throw new ArgumentNullException("userRepository");
            }
            this.userRepository = userRepository;

            if (adapterRegistry == null)
            {
                throw new ArgumentNullException("adapterRegistry");
            }
            this.adapterRegistry = adapterRegistry;

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            this.logger = logger;

            if (proxyService == null)
            {
                throw new ArgumentNullException("proxyService");
            }
            this.proxyService = proxyService;

            if (selfservicePreferencesService == null)
            {
                throw new ArgumentNullException("selfservicePreferencesService");
            }
            this.selfservicePreferencesService = selfservicePreferencesService;
        }

        /// <summary>
        /// Returns the users with login IDs that start with the specified query string.
        /// </summary>
        /// <param name="q">The query string</param>
        /// <returns>All <see cref="User">user names that matched the query.</see></returns>
        public IEnumerable<User> GetUsers([FromUri]string q)
        {
            var domainUsers = userRepository.GetMatchingUsers(q);
            var dtoUsers = new List<Dtos.Base.User>();
            var adapter = adapterRegistry.GetAdapter<Domain.Entities.User, Dtos.Base.User>();
            foreach (var domainUser in domainUsers)
            {
                dtoUsers.Add(adapter.MapToType(domainUser));
            }
            return dtoUsers;
        }

        /// <summary>
        /// Post changes to a user's proxy permissions
        /// </summary>
        /// <param name="assignment">The proxy permissions being changed</param>
        /// <param name="useEmployeeGroups">Optional parameter used to differentiate between employee proxy and person proxy</param>
        /// <returns>A collection of <see cref="ProxyAccessPermission">proxy access permissions</see>.</returns>
        /// <accessComments>
        /// Any logged in user can update their own proxy permissions.       
        /// </accessComments>
        [Obsolete("Obsolete as of API verson 1.36; use version 2 of this endpoint")]
        public async Task<IEnumerable<ProxyAccessPermission>> PostUserProxyPermissionsAsync(ProxyPermissionAssignment assignment, string useEmployeeGroups = "False")
        {
            try
            {
                bool isEmployeeProxy;
                if (bool.TryParse(useEmployeeGroups, out isEmployeeProxy))
                {
                    // Fix for backward compatability with SS 2.35.1 - Bypassing the ADD.ALL.HR.PROXY permission check.
                    return await proxyService.PostUserProxyPermissionsAsync(assignment, false);
                }
                else
                {
                    return await proxyService.PostUserProxyPermissionsAsync(assignment);
                }
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException(permissionExceptionMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Post changes to a user's proxy permissions
        /// </summary>
        /// <param name="assignment">The proxy permissions being changed</param>
        /// <param name="useEmployeeGroups">Optional parameter used to differentiate between employee proxy and person proxy</param>
        /// <returns>A collection of <see cref="ProxyAccessPermission">proxy access permissions</see>.</returns>
        /// <accessComments>
        /// Any logged in user can update their own proxy permissions.
        /// A user with the permission ADD.ALL.HR.PROXY is considered as an admin and can update proxy permissions of any employee.
        /// </accessComments>
        public async Task<IEnumerable<ProxyAccessPermission>> PostUserProxyPermissionsV2Async(ProxyPermissionAssignment assignment, string useEmployeeGroups = "False")
        {
            try
            {
                bool isEmployeeProxy;
                if (bool.TryParse(useEmployeeGroups, out isEmployeeProxy))
                {
                    return await proxyService.PostUserProxyPermissionsAsync(assignment, isEmployeeProxy);
                }
                else
                {
                    return await proxyService.PostUserProxyPermissionsAsync(assignment);
                }
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException(permissionExceptionMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets a collection of proxy access permissions, by user, for the supplied person
        /// </summary>
        /// <param name="userId">The identifier of the entity of interest</param>
        /// <returns>A collection of proxy access permissions for the supplied person</returns>
        /// <param name="useEmployeeGroups">Optional parameter used to differentiate between employee proxy and person proxy</param>
        /// <accessComments>
        /// Any logged in user can get their own proxy access permissions. 
        /// A user with the permission ADD.ALL.HR.PROXY is considered as an admin and can access the proxy access permissions of any employee.
        /// </accessComments>
        public async Task<IEnumerable<ProxyUser>> GetUserProxyPermissionsAsync(string userId, string useEmployeeGroups = "False")
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw CreateHttpResponseException("ID must be supplied.", HttpStatusCode.BadRequest);
            }
            try
            {
                bool isEmployeeProxy;
                if (bool.TryParse(useEmployeeGroups, out isEmployeeProxy))
                {                  
                    return await proxyService.GetUserProxyPermissionsAsync(userId, isEmployeeProxy);
                }
                else
                {
                    return await proxyService.GetUserProxyPermissionsAsync(userId);
                }
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                logger.Error(pex.ToString());
                throw CreateHttpResponseException(permissionExceptionMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets the proxy user's proxy subjects. Note: "proxy subject" means the user that has granted
        /// proxy access to the proxy user. The proxy user may act on behalf of the proxy subject.
        /// </summary>
        /// <param name="userId">The proxy user's person ID.</param>
        /// <returns></returns>
        /// <accessComments>
        /// Only the current user can get their own proxy subjects.
        /// </accessComments>
        public async Task<IEnumerable<Dtos.Base.ProxySubject>> GetUserProxySubjectsAsync(string userId)
        {
            try
            {
                return await proxyService.GetUserProxySubjectsAsync(userId);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Ellucian.Web.Security.PermissionsException ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Create a Proxy Candidate
        /// </summary>
        /// <param name="candidate">A <see cref="ProxyCandidate"/> object containing the values to store</param>
        /// <returns>The created <see cref="ProxyCandidate"/></returns>
        /// <accessComments>
        /// Only the current user can create their own proxy candidate. 
        /// </accessComments>
        public async Task<ProxyCandidate> PostProxyCandidateAsync(ProxyCandidate candidate)
        {
            try
            {
                return await proxyService.PostProxyCandidateAsync(candidate);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                logger.Error(pex.Message);
                throw CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets a collection of proxy candidates that the proxy user has submitted for evaluation.
        /// </summary>
        /// <param name="userId">ID of the user granting access</param>
        /// <returns>A collection of proxy candidates</returns>
        /// <accessComments>
        ///  Any logged in user can get their proxy candidates.
        ///  A user with the permission ADD.ALL.HR.PROXY is considered as an admin and can access the proxy candidates of any employee.
        /// </accessComments>
        public async Task<IEnumerable<ProxyCandidate>> GetUserProxyCandidatesAsync(string userId)
        {
            try
            {
                return await proxyService.GetUserProxyCandidatesAsync(userId);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                logger.Error(pex.Message);
                throw CreateHttpResponseException(permissionExceptionMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Creates a Person record for the purposes of becoming a proxy user.  The Proxy Parameters (PRXP) form Allow Addition of New Users field must be turned on.
        /// </summary>
        /// <param name="user">Information about the <see cref="PersonProxyUser">proxy user</see> to create</param>
        /// <returns>The created <see cref="PersonProxyUser">proxy user</see></returns>
        public async Task<PersonProxyUser> PostProxyUserAsync(PersonProxyUser user)
        {
            try
            {
                return await proxyService.PostPersonProxyUserAsync(user);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the user's preferences for the given self service module.
        /// </summary>
        /// <param name="personId">Person for which to retrieve preferences</param>
        /// <param name="preferenceType">The key for the self service module</param>
        /// <returns>SelfservicePreference for the module for the user</returns>
        /// <accessComments>
        /// Only the current user can get their own preferences.
        /// </accessComments>
        public async Task<Dtos.Base.SelfservicePreference> GetSelfservicePreferenceAsync(string personId, string preferenceType)
        {
            if (string.IsNullOrEmpty(personId))
            {
                logger.Error("Error retrieving preference due to invalid arguments.");
                throw CreateHttpResponseException("Could not retrieve preference.");
            }
            if (string.IsNullOrEmpty(preferenceType))
            {
                logger.Error("Error retrieving preference due to invalid arguments.");
                throw CreateHttpResponseException("Could not retrieve preference.");
            }

            try
            {
                var preference = await selfservicePreferencesService.GetPreferenceAsync(personId, preferenceType);
                if (preference == null)
                {
                    throw CreateHttpResponseException("No preference exists for user of given type.", System.Net.HttpStatusCode.NotFound);
                }
                return preference;
            }
            catch (HttpResponseException)
            {
                throw;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error retrieving preference for person " + personId + " of type " + preferenceType + ".");
                throw CreateHttpResponseException("Could not retrieve preference.");
            }
        }

        /// <summary>
        /// Updates the user's preferences with the given parameters.
        /// </summary>
        /// <param name="selfservicePreference">The user preference to be updated</param>
        /// <returns>The updated user preference</returns>
        /// <accessComments>
        /// Only the current user can update their preferences.
        /// </accessComments>
        [HttpPut]
        public async Task<Dtos.Base.SelfservicePreference> UpdateSelfservicePreferenceAsync([FromBody]SelfservicePreference selfservicePreference)
        {
            if (selfservicePreference == null)
            {
                throw CreateHttpResponseException("Could not update preference.");
            }
            try
            {
                var updatedPreference = await selfservicePreferencesService.UpdatePreferenceAsync(selfservicePreference.Id, selfservicePreference.PersonId, selfservicePreference.PreferenceType, selfservicePreference.Preferences);
                return updatedPreference;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error updating preference for person " + selfservicePreference.PersonId + " of type " + selfservicePreference.PreferenceType + ".");
                throw CreateHttpResponseException("Could not update preference.");
            }
        }

        /// <summary>
        /// Delete a user preference
        /// </summary>
        /// <param name="personId">The person ID belonging to the preference to delete</param>
        /// <param name="preferenceType">The type of preference to delete</param>
        /// <returns>nothing</returns>
        [HttpDelete]
        public async Task DeleteSelfServicePreferenceAsync(string personId, string preferenceType)
        {
            if (string.IsNullOrEmpty(personId))
            {
                logger.Error("Error deleting preference due to invalid arguments.");
                throw CreateHttpResponseException("Could not delete preference.");
            }
            if (string.IsNullOrEmpty(preferenceType))
            {
                logger.Error("Error deleting preference due to invalid arguments.");
                throw CreateHttpResponseException("Could not delete preference.");
            }
            try
            {
                await selfservicePreferencesService.DeletePreferenceAsync(personId, preferenceType);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error deleting preference for person " + personId + " of type " + preferenceType + ".");
                throw CreateHttpResponseException("Could not delete preference.");
            }
        }
    }
}
