// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using Ellucian.Web.Http.EthosExtend;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using System.Text;
using Ellucian.Dmi.Runtime;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Abstract class for coordination services
    /// </summary>
    public abstract class BaseCoordinationService
    {
        private readonly ICurrentUserFactory currentUserFactory;
        private readonly IRoleRepository roleRepository;
        private readonly IStaffRepository staffRepository;
        private readonly IConfigurationRepository configurationRepository;

        char _VM = Convert.ToChar(DynamicArray.VM);
        char _SM = Convert.ToChar(DynamicArray.SM);
        char _XM = Convert.ToChar(250);

        /// <summary>
        /// The logger
        /// </summary>
        protected readonly ILogger logger;

        /// <summary>
        /// The _adapter registry
        /// </summary>
        protected readonly IAdapterRegistry _adapterRegistry;

        private ICurrentUser currentUser;

        /// <summary>
        /// Gets the current user.
        /// </summary>
        protected ICurrentUser CurrentUser
        {
            get
            {
                if (currentUser == null)
                {
                    currentUser = currentUserFactory.CurrentUser;
                }
                return currentUser;
            }
        }

        /// <summary>
        /// Dictionary of string, string that contains the Ethos Extended Data to send into the CTX
        /// key is column name
        /// value is value to save in, if empty string then this means it is meant to remove the data from colleague
        /// </summary>
        public Dictionary<string, string> EthosExtendedDataDictionary { get; set; }
        
        /// <summary>
        /// Contains a Tuple where Item1 is a bool set to true if any fields are denied or secured, 
        /// Item2 is a list of DeniedAccess Fields and Item3 is a list of Restricted fields.
        /// </summary>
        public Tuple<bool, List<string>, List<string>> SecureDataDefinition { get; set; }

        /// <summary>
        /// Get the Denied and Secured Data Properties from the CTX call.
        /// </summary>
        /// <returns>Returns a Tuple with the secure flag and list of denied data fields and list of secure data fields.</returns>
        public Tuple<bool, List<string>, List<string>> GetSecureDataDefinition()
        {
            if (SecureDataDefinition == null)
            {
                SecureDataDefinition = new Tuple<bool, List<string>, List<string>>(false, new List<string>(), new List<string>());
            }
            return SecureDataDefinition;
        }

        /// <summary>
        /// IntegrationApiException object for error collection
        /// </summary>
        public IntegrationApiException IntegrationApiException { get; set; }

        /// <summary>
        /// Used to decide if the EthosApiBuilder is running and then report errors as an exception.
        /// </summary>
        public bool ReportEthosApiErrors { get; set; }

        /// <summary>
        /// Get all the data privacy and store it in the global variable for performance. 
        /// </summary>
        private IEnumerable<Domain.Base.Entities.EthosSecurity> _ethosDataPrivacyConfigs = null;
        private async Task<IEnumerable<Domain.Base.Entities.EthosSecurity>> GetEthosDataPrivacyConfiguration(bool bypassCache)
        {
            if (_ethosDataPrivacyConfigs == null)
            {
                _ethosDataPrivacyConfigs = await configurationRepository.GetEthosDataPrivacyConfiguration(bypassCache);
            }
            return _ethosDataPrivacyConfigs;

        }

        /// <summary>
        /// Initializes a new instance of the BaseCoordinationService class.
        /// </summary>
        /// <param name="adapterRegistry">adapter registry, must not be null</param>
        /// <param name="currentUserFactory">the current user factory, must not be null</param>
        /// <param name="roleRepository">a role repository, must not be null</param>
        /// <param name="logger">a logger, must not be null</param>
        /// <param name="staffRepository"></param>
        /// <param name="configurationRepository"></param>
        protected BaseCoordinationService(IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, IStaffRepository staffRepository = null, IConfigurationRepository configurationRepository = null)
        {
            if (adapterRegistry == null)
            {
                throw new ArgumentNullException("adapterRegistry");
            }
            this._adapterRegistry = adapterRegistry;
            if (currentUserFactory == null)
            {
                throw new ArgumentNullException("currentUserFactory");
            }
            this.currentUserFactory = currentUserFactory;

            if (roleRepository == null)
            {
                throw new ArgumentNullException("roleRepository");
            }
            this.roleRepository = roleRepository;

            this.staffRepository = staffRepository;

            this.configurationRepository = configurationRepository;

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            this.logger = logger;
        }

        /// <summary>
        /// Populate IntegrationApiException object for error collection
        /// </summary>
        /// <param name="message">The detailed actionable error message.</param>
        /// <param name="code">The error message code used to describe the error details</param>
        /// <param name="guid">The global identifier of the resource in error.</param>
        /// <param name="id">The source applications data reference identifier for the primary data entity used to create the resource.</param>
        /// <param name="httpStatusCode">HTTP Status Code.  Default 400 Bad Request</param>
        protected void IntegrationApiExceptionAddError(string message, string code = null, string guid = null,
            string id = null, System.Net.HttpStatusCode httpStatusCode = System.Net.HttpStatusCode.BadRequest)
        {
            if (IntegrationApiException == null)
                IntegrationApiException = new IntegrationApiException();
       
            IntegrationApiException.AddError(ConvertToIntegrationApiError(message, code, guid, id, httpStatusCode));
        }

        /// <summary>
        /// Populate IntegrationApiException object for error collection. Extracts all errors
        /// from a Repository Exception
        /// </summary>
        /// <param name="ex">The RepositoryException.</param>
        /// <param name="code">The error message code used to describe the error details</param>
        /// <param name="guid">The global identifier of the resource in error.</param>
        /// <param name="id">The source applications data reference identifier for the primary data entity used to create the resource.</param>
        /// <param name="httpStatusCode">HTTP Status Code.  Default 400 Bad Request</param>
        protected void IntegrationApiExceptionAddError(RepositoryException ex, string code = null, string guid = null,
            string id = null, System.Net.HttpStatusCode httpStatusCode = System.Net.HttpStatusCode.BadRequest)
        {
            if (ex == null)
                return;

            if (IntegrationApiException == null)
                IntegrationApiException = new IntegrationApiException();

            IntegrationApiException.AddErrors(ex.Errors.ToList().ConvertAll(
                x => ConvertToIntegrationApiError(x.Message, x.Code, 
                !string.IsNullOrEmpty(x.Id) || !string.IsNullOrEmpty(x.SourceId) ? x.Id : guid,
                !string.IsNullOrEmpty(x.Id) || !string.IsNullOrEmpty(x.SourceId) ? x.SourceId : id,
                httpStatusCode)));

            if ((!string.IsNullOrEmpty(ex.Message)) 
                && (!IntegrationApiException.Errors.Any(e => !string.IsNullOrEmpty(e.Message)
                    && e.Message.Equals(ex.Message, StringComparison.OrdinalIgnoreCase))))
            {
                IntegrationApiException.AddError(ConvertToIntegrationApiError(ex.Message, string.IsNullOrEmpty(code) ? "Data.Access" : code, guid, id, httpStatusCode));
            }           
        }

        /// <summary>
        /// Answers if the current user has the specified permission.
        /// </summary>
        /// <param name="permissionCode"></param>
        /// <returns></returns>
        protected bool HasPermission(string permissionCode)
        {
            if (string.IsNullOrEmpty(permissionCode))
            {
                throw new ArgumentNullException("permissionCode");
            }
            var userRoles = CurrentUser.Roles;
            var matchingRolesFromDb = roleRepository.Roles.Where(r => userRoles.Contains(r.Title));
            foreach (var r in matchingRolesFromDb)
            {
                if (r.HasPermission(permissionCode))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns a unique list of permission codes for the current user for all their relevant roles.
        /// If the user has no permission codes an empty list is returned.
        /// </summary>
        /// <returns>The permission codes for the user</returns>
        protected IEnumerable<string> GetUserPermissionCodes()
        {
            List<string> permissions = new List<string>();
            var userRoles = CurrentUser.Roles;
            var matchingRolesFromDb = roleRepository.Roles.Where(r => userRoles.Contains(r.Title));

            foreach (var role in matchingRolesFromDb)
            {
                foreach (var perm in role.Permissions)
                {
                    if (!permissions.Contains(perm.Code))
                    {
                        permissions.Add(perm.Code);
                    }
                }
            }
            return permissions;
        }

        /// <summary>
        /// Returns a unique list of role ids for the current user
        /// If the user has no roles an empty list is returned.
        /// </summary>
        /// <returns>The role ids for the user</returns>
        protected async Task<IEnumerable<string>> GetUserRoleIdsAsync()
        {
            return
                (await roleRepository.GetRolesAsync()).Where(r => CurrentUser.Roles.Contains(r.Title))
                    .Select(r => r.Id.ToString())
                    .ToList();
        }

        /// <summary>
        /// Returns a unique list of permission codes for the current user for all their relevant roles.
        /// If the user has no permission codes an empty list is returned.
        /// </summary>
        /// <param name="permissionCodes"></param>
        /// <returns>The permission codes for the user</returns>
        protected async Task<IEnumerable<string>> GetUserPermissionCodesAsync()
        {
            List<string> permissions = new List<string>();
            var userRoles = CurrentUser.Roles;
            var matchingRolesFromDb = (await roleRepository.GetRolesAsync()).Where(r => userRoles.Contains(r.Title));

            foreach (var role in matchingRolesFromDb)
            {
                foreach (var perm in role.Permissions)
                {
                    if (!permissions.Contains(perm.Code))
                    {
                        permissions.Add(perm.Code);
                    }
                }
            }
            return permissions;
        }
        /// <summary>
        /// Convert a code in a code file to a GUID
        /// </summary>
        /// <param name="codeList">Source list of codes, must inherit GuidCodeItem</param>
        /// <param name="code">Specific code in code list</param>
        /// <returns>GUID corresponding to the code</returns>
        protected static string ConvertCodeToGuid(IEnumerable<Domain.Entities.GuidCodeItem> codeList, string code)
        {
            if (codeList == null || codeList.Count() == 0)
            {
                throw new ArgumentNullException("codeList");
            }
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }

            var entity = codeList.FirstOrDefault(c => c.Code == code);
            return entity == null ? null : entity.Guid;
        }

        /// <summary>
        /// Convert a GUID to a code in a code file
        /// </summary>
        /// <param name="codeList">Source list of codes, must inherit GuidCodeItem</param>
        /// <param name="guid">GUID corresponding to a code</param>
        /// <returns>The code corresponding to the GUID</returns>
        protected static string ConvertGuidToCode(IEnumerable<Domain.Entities.GuidCodeItem> codeList, string guid)
        {
            if (codeList == null || codeList.Count() == 0)
            {
                throw new ArgumentNullException("codeList");
            }
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            var entity = codeList.FirstOrDefault(c => c.Guid.Equals(guid, StringComparison.OrdinalIgnoreCase));
            return entity == null ? null : entity.Code;
        }

        /// <summary>
        /// Convert a GUID to a code in a code file, throw an exception on any failure.  For HEDM use.
        /// </summary>
        /// <param name="codeList">Source list of codes, must inherit GuidCodeItem</param>
        /// <param name="guid">GUID corresponding to a code</param>
        /// <returns>The code corresponding to the GUID</returns>
        protected static string ConvertGuidToCodeNoFail(IEnumerable<Domain.Entities.GuidCodeItem> codeList, string guid)
        {
            if (codeList == null || codeList.Count() == 0)
            {
                throw new ArgumentNullException("codeList");
            }
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentException("GUID: " + guid + " is not valid.");
            }
            var entity = codeList.FirstOrDefault(c => c.Guid.Equals(guid, StringComparison.OrdinalIgnoreCase));

            if (entity == null)
            {
                throw new ArgumentException("GUID: " + guid + " is not valid.");
            }

            return entity.Code;
        }

        /// <summary>
        /// Confirms that the user is proxying on behalf of the specified person
        /// and, optionally, has one or more of the specified proxy permissions.
        /// </summary>
        /// <param name="personId">Person ID to check for proxy access</param>
        /// <param name="proxyPermission">(Optional) Proxy permissions list with which to check the current user has been granted at least one, if present.</param>
        /// <returns>True if the user has proxy access to the specified person, otherwise false.</returns>
        protected bool HasProxyAccessForPerson(string personId, params Domain.Base.Entities.ProxyWorkflowConstants[] proxyPermissions)
        {
            var proxySubject = CurrentUser.ProxySubjects.FirstOrDefault();
            var hasProxySubject = proxySubject != null && proxySubject.PersonId == personId;
            if (hasProxySubject && proxyPermissions != null && proxyPermissions.Any())
            {
                var hasProxyPermission = proxySubject.Permissions.Intersect(proxyPermissions.Select(perm => perm.Value)).Any();
                return hasProxyPermission;
            }
            else
            {
                return hasProxySubject;
            }
        }

        /// <summary>
        /// Confirms that the user is allowed to access records w
        /// </summary>
        /// <param name="privacyCode">The privacy code to check</param>
        /// <returns>True, if the current user can access the record.</returns>
        protected bool HasPrivacyCodeAccess(string privacyCode)
        {
            try
            {
                if (staffRepository != null)
                {
                    var staff = staffRepository.Get(CurrentUser.PersonId);

                    return staff.PrivacyCodes.Contains(privacyCode);
                }
                else
                {
                    return false;
                }
            }
            catch // There was some sort of error getting the staff record (or there is no staff record)
            {
                return false;
            }
        }

        /// <summary>
        /// Determine if the logged in user is the person whose data is being accessed.
        /// </summary>
        /// <param name="personId">ID of person from data</param>
        protected void CheckIfUserIsSelf(string personId)
        {
            if (!CurrentUser.IsPerson(personId))
            {
                logger.Error(CurrentUser + " is not person " + personId);
                throw new PermissionsException();
            }
            return;
        }
        
        /// <summary>
        /// Check for permissions and build error message.
        /// If multiple permissions are assigned, then only one is required for access.
        /// Used to provide consistent error message when permission denied
        /// </summary>
        /// <param name="permissionsTuple">Tuple consisting of:
        /// 1. string[]: array of valid permissions
        /// 2. string: http method (ex: "GET")
        /// 3. string: resource name (ex: 'person-holds')</param>
        /// <returns>bool</returns>
        public bool ValidatePermissions(Tuple<string[], string,string> permissionsTuple)
        {
            bool hasPermission = false; 
            
            if (permissionsTuple == null)
            {
                return hasPermission;
            }

            if (permissionsTuple.Item1 != null && permissionsTuple.Item1.Any())
            {
                // if multiple permissions are assigned, then only one is required for access.
                foreach (var p in permissionsTuple.Item1)
                {
                    hasPermission = HasPermission(p);
                    if (hasPermission)
                        break;
                }
            }
             if (!hasPermission)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("User '" + CurrentUser.UserId + "' does not have permission");
               
                switch (permissionsTuple.Item2)
                {
                    case ("GET"):
                        sb.Append(" to view"); break;
                    case ("PUT"):
                        sb.Append(" to update"); break;
                    case ("POST"):
                        sb.Append(" to create"); break;
                    case ("DELETE"):
                        sb.Append(" to delete"); break;
                    default:
                        break;
                }
                if (!string.IsNullOrEmpty(permissionsTuple.Item3))
                {
                    sb.Append(" " + permissionsTuple.Item3.ToLower());
                }
                sb.Append(".");
                
                logger.Error(sb.ToString());
                throw new PermissionsException(sb.ToString());
            }
            return hasPermission;
        }

        /// <summary>
        /// returns true if an api has any data privacy set up
        /// </summary>
        /// <param name="apiName">name of the api (eedm schema name)</param>
        /// <param name="bypassCache"></param>
        /// <returns>true if there is data privacy</returns>
        public async Task<bool> CheckDataPrivacyByApi(string apiName, bool bypassCache = false)
        {
            bool hasPrivacy = false;

            if (configurationRepository != null)
            {
                var ethosDataPrivacyListForApi =
                    (await GetEthosDataPrivacyConfiguration(bypassCache)).Where(
                        e => e.ApiName.Equals(apiName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                if (ethosDataPrivacyListForApi != null && ethosDataPrivacyListForApi.PropertyDefinitions.Any())
                {
                    hasPrivacy = true;
                }
            }
            return hasPrivacy;
        }

        /// <summary>
        /// Gets the list of EEDM data privacy settings by user based on user, roles and permissions
        /// </summary>
        /// <param name="apiName">name of the api (eedm schema name)</param>
        /// <param name="bypassCache"></param>
        /// <returns>list of data privacy strings to apply</returns>
        public async Task<IEnumerable<string>> GetDataPrivacyListByApi(string apiName, bool bypassCache = false)
        {
            var userPermissionList = (await GetUserPermissionCodesAsync()).ToList();
            var userRoleIdList = (await GetUserRoleIdsAsync()).ToList();

            if (configurationRepository != null)
            {
                //since this CINC Huib record data is static, even though the api is called with no-cache, we are always going to read the HUB record from cache as this record is quite static. 
                var isEmaUser = await configurationRepository.IsThisTheEmaUser(CurrentUser.UserId, false);
                var returnList = new List<string>();

                var ethosDataPrivacyListForApi =
                    (await GetEthosDataPrivacyConfiguration(bypassCache)).Where(
                        e => e.ApiName.Equals(apiName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                if (ethosDataPrivacyListForApi != null && ethosDataPrivacyListForApi.PropertyDefinitions.Any())
                {
                    //if this is the EMA user, apply all data privacy settings.
                    if (isEmaUser)
                    {
                        ethosDataPrivacyListForApi.PropertyDefinitions.ForEach(p =>
                        {
                            returnList.Add(p.PropertyInformation);
                        });
                        return returnList;
                    }

                    //it isn't the EMA user so we apply by the permissions settings
                    ethosDataPrivacyListForApi.PropertyDefinitions.ForEach(p =>
                    {
                        //if not allowed to view set, if any of username, role or permission match, they are not allowed to see it.
                        if (p.NotAllowedToViewData)
                        {
                            if (!string.IsNullOrEmpty(p.UserName) && currentUser.UserId.Equals(p.UserName, StringComparison.OrdinalIgnoreCase))
                            {
                                returnList.Add(p.PropertyInformation);
                                return;
                            }

                            if (!string.IsNullOrEmpty(p.UserRole) && userRoleIdList.Contains(p.UserRole))
                            {
                                returnList.Add(p.PropertyInformation);
                                return;
                            }

                            if (!string.IsNullOrEmpty(p.UserPermission) && (userPermissionList.Contains(p.UserPermission)))
                            {
                                returnList.Add(p.PropertyInformation);
                                return;
                            }
                        }
                        else if (p.RequiredToViewData)
                        {
                            //if required to view data is set then they must match either username, role or permission, if any of those match they are allowed to see it
                            bool match = false;

                            if (!string.IsNullOrEmpty(p.UserName) && currentUser.UserId.Equals(p.UserName, StringComparison.OrdinalIgnoreCase))
                            {
                                match = true;
                            }

                            if (!string.IsNullOrEmpty(p.UserRole) && userRoleIdList.Contains(p.UserRole))
                            {
                                match = true;
                            }

                            if (!string.IsNullOrEmpty(p.UserPermission) && (userPermissionList.Contains(p.UserPermission)))
                            {
                                match = true;
                            }

                            if (!match)
                            {
                                returnList.Add(p.PropertyInformation);
                            }
                        }
                    });

                    return returnList;
                }
                else
                {
                    return returnList;
                }
            }
            else
            {
                var noConfigError = new ArgumentNullException(string.Concat("Configuration Repository is not intialized for API ", apiName, "."));
                logger.Error(noConfigError, string.Concat("Configuration Repository is not intialized for API ", apiName, "."));
                throw noConfigError;
            }
        }

        /// <summary>
        /// Gets the list of EEDM data privacy settings by user based on user, roles and permissions
        /// </summary>
        /// <param name="ethosResourceRouteInfo">ethos resource info from route</param>
        /// <param name="bypassCache"></param>
        /// <returns>list of data privacy strings to apply</returns>
        public async Task<IEnumerable<string>> GetDataPrivacyListByApi(EthosResourceRouteInfo ethosResourceRouteInfo, bool bypassCache = false)
        {
            var userPermissionList = (await GetUserPermissionCodesAsync()).ToList();
            var userRoleIdList = (await GetUserRoleIdsAsync()).ToList();
            if (configurationRepository != null)
            {
                var isEmaUser = await configurationRepository.IsThisTheEmaUser(CurrentUser.UserId, bypassCache);
                var returnList = new List<string>();

                var ethosDataPrivacyListForApi =
                    (await GetEthosDataPrivacyConfiguration(bypassCache)).FirstOrDefault(e => e.ApiName.Equals(ethosResourceRouteInfo.ResourceName, StringComparison.OrdinalIgnoreCase));

                if (ethosDataPrivacyListForApi == null || !ethosDataPrivacyListForApi.PropertyDefinitions.Any())
                    return returnList;

                //if this is the EMA user, apply all data privacy settings.
                if (isEmaUser)
                {
                    ethosDataPrivacyListForApi.PropertyDefinitions.ForEach(p =>
                    {
                        returnList.Add(p.PropertyInformation);
                    });
                    return returnList;
                }

                //it isn't the EMA user so we apply by the permissions settings
                ethosDataPrivacyListForApi.PropertyDefinitions.ForEach(p =>
                {
                    //if not allowed to view set, if any of username, role or permission match, they are not allowed to see it.
                    if (p.NotAllowedToViewData)
                    {
                        if (!string.IsNullOrEmpty(p.UserName) && currentUser.UserId.Equals(p.UserName, StringComparison.OrdinalIgnoreCase))
                        {
                            returnList.Add(p.PropertyInformation);
                            return;
                        }

                        if (!string.IsNullOrEmpty(p.UserRole) && userRoleIdList.Contains(p.UserRole))
                        {
                            returnList.Add(p.PropertyInformation);
                            return;
                        }

                        if (!string.IsNullOrEmpty(p.UserPermission) && (userPermissionList.Contains(p.UserPermission)))
                        {
                            returnList.Add(p.PropertyInformation);
                        }
                    }
                    else if (p.RequiredToViewData)
                    {
                        //if required to view data is set then they must match either username, role or permission, if any of those match they are allowed to see it
                        bool match = !string.IsNullOrEmpty(p.UserName) && currentUser.UserId.Equals(p.UserName, StringComparison.OrdinalIgnoreCase)
                                     || !string.IsNullOrEmpty(p.UserRole) && userRoleIdList.Contains(p.UserRole)
                                     || !string.IsNullOrEmpty(p.UserPermission) && (userPermissionList.Contains(p.UserPermission));

                        if (!match)
                        {
                            returnList.Add(p.PropertyInformation);
                        }
                    }
                });

                return returnList;
            }

            var noConfigError = new ArgumentNullException(string.Concat("Configuration Repository is not intialized for API ", ethosResourceRouteInfo.ResourceName, "."));
            logger.Error(noConfigError, string.Concat("Configuration Repository is not intialized for API ", ethosResourceRouteInfo.ResourceName, "."));
            throw noConfigError;

        }

        /// <summary>
        /// Gets the extended data available on a resource, returns an empty list if there are none
        /// </summary>
        /// <param name="ethosResourceRouteInfo">Ethos Resource Route Info </param>
        /// <param name="resourceIds">IEnumerable of the ids for the resources in guid form</param>
        /// <returns>List with all of the extended data if aavailable. Returns an empty list if none available or none configured</returns>
        public async Task<IList<EthosExtensibleData>> GetExtendedEthosDataByResource(EthosResourceRouteInfo ethosResourceRouteInfo, IEnumerable<string> resourceIds, bool bypassCache = false, bool useRecordKey = false)
        {
            if (!bypassCache)
            {
                bypassCache = ethosResourceRouteInfo.BypassCache;
            }
            ReportEthosApiErrors = ethosResourceRouteInfo.ReportEthosExtendedErrors;

            var resourceName = ethosResourceRouteInfo.ResourceName;
            if (!string.IsNullOrEmpty(ethosResourceRouteInfo.ExtendedSchemaResourceId) && !ethosResourceRouteInfo.ExtendedSchemaResourceId.Equals(ethosResourceRouteInfo.ResourceName, StringComparison.OrdinalIgnoreCase))
            {
                resourceName = ethosResourceRouteInfo.ExtendedSchemaResourceId.ToLowerInvariant();
            }

            if (configurationRepository != null)
            {
                Dictionary<string, Dictionary<string, string>> allColumnData = null;
                if (EthosExtendedDataDictionary != null && EthosExtendedDataDictionary.Any())
                {
                    allColumnData = new Dictionary<string, Dictionary<string, string>>
                    {
                        { resourceIds.FirstOrDefault(), EthosExtendedDataDictionary }
                    };
                }

                var extendDataFromRepo = await configurationRepository.GetExtendedEthosDataByResource(resourceName,
                    ethosResourceRouteInfo.ResourceVersionNumber, ethosResourceRouteInfo.ExtendedSchemaResourceId,
                    resourceIds, allColumnData, ReportEthosApiErrors, bypassCache, useRecordKey, ethosResourceRouteInfo.ReturnRestrictedFields);

                // If the extended data CTX has restricted or denied fields, pass this back to the controller.
                // Only get the list for GET and not when PUT or POST has already set this property.
                if (allColumnData == null || !allColumnData.Any())
                {
                    SecureDataDefinition = configurationRepository.GetSecureDataDefinition();
                }

                // Check for self-service style security for access to "my" records only and filter out any
                // responses that don't don't belong to "me".
                List<Domain.Base.Entities.EthosExtensibleData> newExtendDataFromRepo = new List<Domain.Base.Entities.EthosExtensibleData>();
                foreach (var dataItem in extendDataFromRepo)
                {
                    if (!string.IsNullOrEmpty(dataItem.CurrentUserIdPath))
                    {
                        var matchingData = extendDataFromRepo.Select(ed => ed.ExtendedDataList.Where(edl => edl.FullJsonPath.Equals(dataItem.CurrentUserIdPath, StringComparison.OrdinalIgnoreCase)).FirstOrDefault()).FirstOrDefault();
                        if (matchingData != null)
                        {
                            bool matchFound = false;
                            var matchingValue = matchingData.ExtendedDataValue.Split(_VM);
                            foreach (var matchValue in matchingValue)
                            {
                                if (matchValue.Equals(CurrentUser.PersonId, StringComparison.OrdinalIgnoreCase))
                                {
                                    matchFound = true;
                                }
                            }
                            if (matchFound)
                            {
                                newExtendDataFromRepo.Add(dataItem);
                            }
                        }
                        else
                        {
                            if (dataItem.CurrentUserIdPath.Equals("/id", StringComparison.OrdinalIgnoreCase) || dataItem.CurrentUserIdPath.Equals("/_id", StringComparison.OrdinalIgnoreCase))
                            {
                                var primaryKey = dataItem.ResourceId.Replace("-", "%").ToUpper();
                                var matchingValue = Uri.UnescapeDataString(primaryKey);
                                if (matchingValue.Contains("+"))
                                {
                                    var idSplit = matchingValue.Split('+');
                                    matchingValue = idSplit[1];
                                }
                                if (matchingValue.Equals(CurrentUser.PersonId, StringComparison.OrdinalIgnoreCase))
                                {
                                    newExtendDataFromRepo.Add(dataItem);
                                }
                            }
                        }
                    }
                    else
                    {
                        newExtendDataFromRepo.Add(dataItem);
                    }
                }
                extendDataFromRepo = newExtendDataFromRepo;

                return ConvertExtendedDataFromDomainToPlatform(extendDataFromRepo);
            }

            var noConfigError = new ArgumentNullException(string.Concat("Configuration Repository is not intialized for API ", ethosResourceRouteInfo.ResourceName, "."));
            logger.Error(noConfigError, string.Concat("Configuration Repository is not intialized for API ", ethosResourceRouteInfo.ResourceName, "."));
            throw noConfigError;
        }

        /// <summary>
        /// Returns the default API version when API builder is called without an accept header.
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<string> GetEthosExtensibilityResourceDefaultVersion(string resourceName, bool bypassCache = false, string requestedVersion = "")
        {
            return await configurationRepository.GetEthosExtensibilityResourceDefaultVersion(resourceName, bypassCache, requestedVersion);
        }

        /// <summary>
        /// Gets the extended configuration available on a resource, returns null if there are none
        /// </summary>
        /// <param name="ethosResourceRouteInfo">Ethos Resource Route Info </param>
        /// <returns>List with all of the extended configurations if aavailable. Returns an null if none available or none configured</returns>
        public async Task<EthosExtensibleData> GetExtendedEthosConfigurationByResource(EthosResourceRouteInfo ethosResourceRouteInfo, bool bypassCache = false)
        {
            if (!bypassCache)
            {
                bypassCache = ethosResourceRouteInfo.BypassCache;
            }

            if (configurationRepository != null)
            {
                var resourceName = ethosResourceRouteInfo.ResourceName;
                if (!string.IsNullOrEmpty(ethosResourceRouteInfo.ExtendedSchemaResourceId) && !ethosResourceRouteInfo.ExtendedSchemaResourceId.Equals(ethosResourceRouteInfo.ResourceName, StringComparison.OrdinalIgnoreCase))
                {
                    resourceName = ethosResourceRouteInfo.ExtendedSchemaResourceId.ToLowerInvariant();
                }
                var extendDataFromRepo = await configurationRepository.GetExtendedEthosConfigurationByResource(resourceName,
                    ethosResourceRouteInfo.ResourceVersionNumber, ethosResourceRouteInfo.ExtendedSchemaResourceId, bypassCache);

                if (extendDataFromRepo == null)
                {
                    return null;
                }

                extendDataFromRepo.CurrentUserId = CurrentUser.PersonId;
                var extendedConfigList = ConvertExtendedDataFromDomainToPlatform(new List<Domain.Base.Entities.EthosExtensibleData> { extendDataFromRepo });

                return extendedConfigList.Any() ? extendedConfigList.First() : null;
            }

            var noConfigError = new ArgumentNullException(string.Concat("Configuration Repository is not intialized for API ", ethosResourceRouteInfo.ResourceName, "."));
            logger.Error(noConfigError, string.Concat("Configuration Repository is not intialized for API ", ethosResourceRouteInfo.ResourceName, "."));
            throw noConfigError;
        }

        /// <summary>
        /// Gets the extended configuration available on a resource, returns null if there are none
        /// </summary>
        /// <param name="ethosResourceRouteInfo">Ethos Resource Route Info </param>
        /// <returns>List with all of the extended configurations if available. Returns an null if none available or none configured</returns>
        public async Task<EthosExtensibleData> GetBulkExtendedEthosConfigurationByResource(EthosResourceRouteInfo ethosResourceRouteInfo, bool bypassCache = false)
        {
            if (!bypassCache)
            {
                bypassCache = ethosResourceRouteInfo.BypassCache;
            }

            if (configurationRepository != null)
            {
                var extendDataFromRepo = await configurationRepository.GetExtendedEthosConfigurationByResource(ethosResourceRouteInfo.ResourceName,
                    ethosResourceRouteInfo.BulkRepresentation, ethosResourceRouteInfo.ExtendedSchemaResourceId, bypassCache);

                if (extendDataFromRepo == null)
                {
                    return null;
                }

                var extendedConfigList = ConvertExtendedDataFromDomainToPlatform(new List<Domain.Base.Entities.EthosExtensibleData> { extendDataFromRepo });

                return extendedConfigList.Any() ? extendedConfigList.First() : null;
            }

            var noConfigError = new ArgumentNullException(string.Concat("Configuration Repository is not intialized for API ", ethosResourceRouteInfo.ResourceName, "."));
            logger.Error(noConfigError, string.Concat("Configuration Repository is not intialized for API ", ethosResourceRouteInfo.ResourceName, "."));
            throw noConfigError;
        }


        /// <summary>
        /// Gets  all extended configurations, returns null if there are none
        /// </summary>
        /// <param name="ethosResourceRouteInfo">Ethos Resource Route Info </param>
        /// <returns>List with all of the extended configurations if available. Returns an null if none available, or none configured</returns>
        public async Task<List<Domain.Base.Entities.EthosExtensibleData>> GetAllExtendedEthosConfigurations(bool bypassCache = false, bool customOnly = true)
        {
            if (configurationRepository != null)
            {
                return (await configurationRepository.GetEthosExtensibilityConfigurationEntities(customOnly, bypassCache)).ToList();

            }
            else
                return null;
        }

        /// <summary>
        /// Import Extended Ethos Data
        /// </summary>
        /// <param name="importedDataList">Extended Ethos Data to send into the service</param>
        public async Task ImportExtendedEthosData(Dictionary<string, string> importedDataList)
        {
            EthosExtendedDataDictionary = importedDataList;
        }


        public IList<EthosExtensibleData> ConvertExtendedDataFromDomainToPlatform(IEnumerable<Domain.Base.Entities.EthosExtensibleData> domainExtensibleData)
        {

            var platformExtensibleDatas = new List<EthosExtensibleData>();

            foreach (var extDataItem in domainExtensibleData)
            {
                var newEthosDataItem = new EthosExtensibleData
                {
                    ApiResourceName = extDataItem.ApiResourceName,
                    ApiVersionNumber = extDataItem.ApiVersionNumber,
                    VersionReleaseStatus = extDataItem.VersionReleaseStatus,
                    ApiType = extDataItem.ApiType,
                    ExtendedSchemaType = extDataItem.ExtendedSchemaType,
                    ResourceId = extDataItem.ResourceId,
                    ColleagueTimeZone = extDataItem.ColleagueTimeZone,
                    CurrentUserIdPath = extDataItem.CurrentUserIdPath,
                    CurrentUserId = extDataItem.CurrentUserId,
                    ColleagueFileNames = extDataItem.ColleagueFileNames,
                    ColleagueKeyNames = extDataItem.ColleagueKeyNames,
                    ExtendedDataList = new List<EthosExtensibleDataRow>(),
                    ExtendedDataFilterList = new List<EthosExtensibleDataFilter>()
                };

                foreach (var dataRow in extDataItem.ExtendedDataList)
                {
                    var row = new EthosExtensibleDataRow
                    {
                        ColleagueColumnName = dataRow.ColleagueColumnName,
                        ColleagueFileName = dataRow.ColleagueFileName,
                        ColleaguePropertyPosition = dataRow.ColleaguePropertyPosition,
                        Required = dataRow.Required,
                        ColleaguePropertyLength = dataRow.ColleaguePropertyLength,
                        FullJsonPath = dataRow.FullJsonPath,
                        JsonPath = dataRow.JsonPath,
                        JsonTitle = dataRow.JsonTitle,
                        ExtendedDataValue = dataRow.ExtendedDataValue,
                        AssociationController = dataRow.AssociationController,
                        UsageType = dataRow.DatabaseUsageType,
                        TransType = dataRow.TransType
                        
                    };

                    switch (dataRow.JsonPropertyType.ToLower())
                    {
                        case "string":
                            row.JsonPropertyType = JsonPropertyTypeExtensions.String;
                            break;
                        case "number":
                            row.JsonPropertyType = JsonPropertyTypeExtensions.Number;
                            break;
                        case "date":
                            row.JsonPropertyType = JsonPropertyTypeExtensions.Date;
                            break;
                        case "time":
                            row.JsonPropertyType = JsonPropertyTypeExtensions.Time;
                            break;
                        case "datetime":
                            row.JsonPropertyType = JsonPropertyTypeExtensions.DateTime;
                            break;
                        default:
                            row.JsonPropertyType = JsonPropertyTypeExtensions.String;
                            break;
                    }

                    newEthosDataItem.ExtendedDataList.Add(row);
                }

                foreach (var filterRow in extDataItem.ExtendedDataFilterList)
                {
                    var row = new EthosExtensibleDataFilter
                    {
                        ColleagueColumnName = filterRow.ColleagueColumnName,
                        DatabaseUsageType = filterRow.DatabaseUsageType,
                        ColleagueFileName = filterRow.ColleagueFileName,
                        ColleaguePropertyLength = filterRow.ColleaguePropertyLength,
                        ColleaguePropertyPosition = filterRow.ColleagueFieldPosition,
                        Required = filterRow.Required,
                        FullJsonPath = filterRow.FullJsonPath,
                        JsonPath = filterRow.JsonPath,
                        JsonTitle = filterRow.JsonTitle,
                        GuidColumnName = filterRow.GuidColumnName,
                        GuidDatabaseUsageType = filterRow.GuidDatabaseUsageType,
                        GuidFileName = filterRow.GuidFileName,
                        SavingField = filterRow.SavingField,
                        SavingOption = filterRow.SavingOption,
                        SelectColumnName = filterRow.SelectColumnName,
                        SelectFileName = filterRow.SelectFileName,
                        SelectSubroutineName = filterRow.SelectSubroutineName,
                        SelectParagraph = filterRow.SelectParagraph,
                        TransColumnName = filterRow.TransColumnName,
                        TransFileName = filterRow.TransFileName,
                        TransTableName = filterRow.TransTableName,
                        FilterValue = filterRow.FilterValue,
                        FilterOper = filterRow.FilterOper,
                        ValidFilterOpers = filterRow.ValidFilterOpers,
                        SelectRules = filterRow.SelectRules,
                        QueryName = filterRow.NamedQuery && !filterRow.KeyQuery ? filterRow.JsonTitle : "criteria",
                        KeyQuery = filterRow.KeyQuery
                    };

                    switch (filterRow.JsonPropertyType.ToLower())
                    {
                        case "string":
                            row.JsonPropertyType = JsonPropertyTypeExtensions.String;
                            break;
                        case "number":
                            row.JsonPropertyType = JsonPropertyTypeExtensions.Number;
                            break;
                        case "date":
                            row.JsonPropertyType = JsonPropertyTypeExtensions.Date;
                            break;
                        case "time":
                            row.JsonPropertyType = JsonPropertyTypeExtensions.Time;
                            break;
                        case "datetime":
                            row.JsonPropertyType = JsonPropertyTypeExtensions.DateTime;
                            break;
                        default:
                            row.JsonPropertyType = JsonPropertyTypeExtensions.String;
                            break;
                    }

                    row.SelectionCriteria = new List<EthosApiSelectCriteria>();
                    foreach (var selectionCriteria in filterRow.SelectionCriteria)
                    {
                        row.SelectionCriteria.Add(new EthosApiSelectCriteria(selectionCriteria.SelectConnector,
                            selectionCriteria.SelectColumn,
                            selectionCriteria.SelectOper,
                            selectionCriteria.SelectValue)
                        );
                    }

                    row.SortColumns = new List<EthosApiSortCriteria>();
                    foreach (var sortCriteria in filterRow.SortColumns)
                    {
                        row.SortColumns.Add(new EthosApiSortCriteria(sortCriteria.SortColumn, sortCriteria.SortSequence));
                    }

                    row.Enumerations = new List<EthosApiEnumerations>();
                    foreach (var enums in filterRow.Enumerations)
                    {
                        row.Enumerations.Add(new EthosApiEnumerations(enums.EnumerationValue, enums.ColleagueValue));
                    }

                    newEthosDataItem.ExtendedDataFilterList.Add(row);
                }

                platformExtensibleDatas.Add(newEthosDataItem);
            }

            return platformExtensibleDatas;
        }

        /// <summary>
        /// Gets the extended configuration available on a resource, returns null if there are none
        /// </summary>
        /// <param name="ethosResourceRouteInfo">Ethos Resource Route Info </param>
        /// <returns>List with all of the extended configurations if aavailable. Returns an null if none available or none configured</returns>
        public async Task<EthosApiConfiguration> GetEthosApiConfigurationByResource(EthosResourceRouteInfo ethosResourceRouteInfo, bool bypassCache = false)
        {
            if (!bypassCache)
            {
                bypassCache = ethosResourceRouteInfo.BypassCache;
            }

            if (configurationRepository != null)
            {
                var resourceName = ethosResourceRouteInfo.ResourceName;
                if (!string.IsNullOrEmpty(ethosResourceRouteInfo.ExtendedSchemaResourceId) && !ethosResourceRouteInfo.ExtendedSchemaResourceId.Equals(ethosResourceRouteInfo.ResourceName, StringComparison.OrdinalIgnoreCase))
                {
                    resourceName = ethosResourceRouteInfo.ExtendedSchemaResourceId.ToLowerInvariant();
                }
                var extendDataFromRepo = await configurationRepository.GetEthosApiConfigurationByResource(resourceName, bypassCache);

                if (extendDataFromRepo == null)
                {
                    return null;
                }

                var extendedConfigList = ConvertEthosApiFromDomainToPlatform(new List<Domain.Base.Entities.EthosApiConfiguration> { extendDataFromRepo });

                return extendedConfigList.Any() ? extendedConfigList.First() : null;
            }

            var noConfigError = new ArgumentNullException(string.Concat("Configuration Repository is not intialized for API ", ethosResourceRouteInfo.ResourceName, "."));
            logger.Error(noConfigError, string.Concat("Configuration Repository is not intialized for API ", ethosResourceRouteInfo.ResourceName, "."));
            throw noConfigError;
        }


        private static IList<EthosApiConfiguration> ConvertEthosApiFromDomainToPlatform(IEnumerable<Domain.Base.Entities.EthosApiConfiguration> domainExtensibleData)
        {
            var platformExtensibleDatas = new List<EthosApiConfiguration>();

            foreach (var extDataItem in domainExtensibleData)
            {
                var newEthosDataItem = new EthosApiConfiguration()
                {
                    ResourceName = extDataItem.ResourceName,
                    ApiType = extDataItem.ApiType,
                    ProcessId = extDataItem.ProcessId,
                    ColleagueFileNames = extDataItem.ColleagueFileNames,
                    ColleagueKeyNames = extDataItem.ColleagueKeyNames,
                    ParentResourceName = extDataItem.ParentResourceName,
                    PrimaryEntity = extDataItem.PrimaryEntity,
                    PrimaryApplication = extDataItem.PrimaryApplication,
                    PrimaryTableName = extDataItem.PrimaryTableName,
                    PrimaryKeyName = extDataItem.PrimaryKeyName,
                    SecondaryKeyName = extDataItem.SecondaryKeyName,
                    SecondaryKeyPosition = extDataItem.SecondaryKeyPosition,
                    PrimaryGuidSource = extDataItem.PrimaryGuidSource,
                    PrimaryGuidDbType = extDataItem.PrimaryGuidDbType,
                    PrimaryGuidFileName = extDataItem.PrimaryGuidFileName,
                    PageLimit = extDataItem.PageLimit,
                    SelectFileName = extDataItem.SelectFileName,
                    SelectSubroutineName = extDataItem.SelectSubroutineName,
                    SavingField = extDataItem.SavingField,
                    SavingOption = extDataItem.SavingOption,
                    SelectColumnName = extDataItem.SelectColumnName,
                    SelectRules = extDataItem.SelectRules,
                    SelectParagraph = extDataItem.SelectParagraph,
                    HttpMethods = new List<EthosApiSupportedMethods>(),
                    SelectionCriteria = new List<EthosApiSelectCriteria>(),
                    SortColumns = new List<EthosApiSortCriteria>(),
                    CurrentUserIdPath = extDataItem.CurrentUserIdPath,
                    CurrentUserId = extDataItem.CurrentUserId,
                    Description = extDataItem.Description,
                    ProcessDesc = extDataItem.ProcessDesc,
                    ApiDomain = extDataItem.ApiDomain,
                    ReleaseStatus = extDataItem.ReleaseStatus
                };
                foreach (var method in extDataItem.HttpMethods)
                {
                    newEthosDataItem.HttpMethods.Add(new EthosApiSupportedMethods(method.Method, method.Permission, method.Description, method.Summary));
                }
                foreach (var response in extDataItem.PreparedResponses)
                {
                    newEthosDataItem.PreparedResponses.Add(new EthosApiPreparedResponse(response.Text, response.JsonTitle, response.Options, response.DefaultOption));
                }
                foreach (var select in extDataItem.SelectionCriteria)
                {
                    newEthosDataItem.SelectionCriteria.Add(
                        new EthosApiSelectCriteria(select.SelectConnector,
                        select.SelectColumn,
                        select.SelectOper,
                        select.SelectValue)
                    );
                }
                foreach (var sort in extDataItem.SortColumns)
                {
                    newEthosDataItem.SortColumns.Add(new EthosApiSortCriteria(sort.SortColumn, sort.SortSequence));
                }

                platformExtensibleDatas.Add(newEthosDataItem);
            }

            return platformExtensibleDatas;
        }

        /// <summary>
        /// Static helper method to convert a repository error into an integration API error
        /// </summary>
        /// <param name="error">A repository error</param>
        /// <returns>An integration API error</returns>
        public static IntegrationApiError ConvertToIntegrationApiError(string message, string code = null, string guid = null,
            string id = null, System.Net.HttpStatusCode httpStatusCode = System.Net.HttpStatusCode.BadRequest)
        {
            if (string.IsNullOrEmpty(code))
                code = "Global.Internal.Error";

            return new IntegrationApiError()
            {
                Code = code,
                Message = message,
                Guid = !string.IsNullOrEmpty(guid) ? guid : null,
                Id = !string.IsNullOrEmpty(id) ? id : null,
                StatusCode = httpStatusCode
            };
        }

        /// <summary>
        /// Checks whether the user has the departmental oversight permission to access section information.
        /// </summary>
        /// <param name="section">Section information</param>
        /// <param name="departments">Departments information</param>
        /// <returns></returns>
        protected bool CheckDepartmentalOversightAccessForSection(Domain.Student.Entities.Section section, IEnumerable<Domain.Base.Entities.Department> departments)
        {
            if (section == null)
            {
                throw new ArgumentNullException("section", "Must provide a section to check departmental oversight permission.");
            }
            if (departments == null)
            {
                throw new ArgumentNullException("departments", "Must provide departments to check departmental oversight permission.");
            }

            // Filter the assigned departments for the requestor
            var assignedDepartments = departments.Where(d => d.DepartmentalOversightIds != null && d.DepartmentalOversightIds.Contains(CurrentUser.PersonId)).Select(d => d.Code);
            if (assignedDepartments != null && assignedDepartments.Any())
            {
                var offeringDepartments = section.Departments.Where(d => assignedDepartments.Any(dc => dc == d.AcademicDepartmentCode)).ToList();
                if (offeringDepartments != null && offeringDepartments.Any())
                {
                    return true;
                }
            }
            return false;
        }
    }
}
