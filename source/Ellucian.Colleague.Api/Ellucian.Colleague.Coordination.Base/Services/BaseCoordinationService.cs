// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.

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
        /// IntegrationApiException object for error collection
        /// </summary>
        public IntegrationApiException IntegrationApiException { get; set; }

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
                IntegrationApiException.AddError(ConvertToIntegrationApiError(ex.Message, "Data.Access", guid, id, httpStatusCode));
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
                logger.Info(CurrentUser + " is not person " + personId);
                throw new PermissionsException();
            }
            return;
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
                var isEmaUser = await configurationRepository.IsThisTheEmaUser(CurrentUser.UserId, bypassCache);
                var returnList = new List<string>();

                var ethosDataPrivacyListForApi =
                    (await configurationRepository.GetEthosDataPrivacyConfiguration(bypassCache)).Where(
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
                    (await configurationRepository.GetEthosDataPrivacyConfiguration(bypassCache)).FirstOrDefault(e => e.ApiName.Equals(ethosResourceRouteInfo.ResourceName, StringComparison.OrdinalIgnoreCase));

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
        /// Gets the extended data available on a resource, returns an empty list if there are no 
        /// </summary>
        /// <param name="ethosResourceRouteInfo">Ethos Resource Route Info </param>
        /// <param name="resourceIds">IEnumerable of the ids for the resources in guid form</param>
        /// <returns>List with all of the extended data if aavailable. Returns an empty list if none available or none configured</returns>
        public async Task<IList<EthosExtensibleData>> GetExtendedEthosDataByResource(EthosResourceRouteInfo ethosResourceRouteInfo, IEnumerable<string> resourceIds)
        {
            if (configurationRepository != null)
            {
                var extendDataFromRepo = await configurationRepository.GetExtendedEthosDataByResource(ethosResourceRouteInfo.ResourceName,
                    ethosResourceRouteInfo.ResourceVersionNumber, ethosResourceRouteInfo.ExtendedSchemaResourceId, resourceIds);

                return ConvertExtendedDataFromDomainToPlatform(extendDataFromRepo);
            }

            var noConfigError = new ArgumentNullException(string.Concat("Configuration Repository is not intialized for API ", ethosResourceRouteInfo.ResourceName, "."));
            logger.Error(noConfigError, string.Concat("Configuration Repository is not intialized for API ", ethosResourceRouteInfo.ResourceName, "."));
            throw noConfigError;
        }

        /// <summary>
        /// Gets the extended configuration available on a resource, returns null if there are none
        /// </summary>
        /// <param name="ethosResourceRouteInfo">Ethos Resource Route Info </param>
        /// <returns>List with all of the extended configurations if aavailable. Returns an null if none available or none configured</returns>
        public async Task<EthosExtensibleData> GetExtendedEthosConfigurationByResource(EthosResourceRouteInfo ethosResourceRouteInfo)
        {
            if (configurationRepository != null)
            {
                var extendDataFromRepo = await configurationRepository.GetExtendedEthosConfigurationByResource(ethosResourceRouteInfo.ResourceName,
                    ethosResourceRouteInfo.ResourceVersionNumber, ethosResourceRouteInfo.ExtendedSchemaResourceId);

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
        /// Import Extended Ethos Data
        /// </summary>
        /// <param name="importedDataList">Extended Ethos Data to send into the service</param>
        public async Task ImportExtendedEthosData(Dictionary<string, string> importedDataList)
        {
            EthosExtendedDataDictionary = importedDataList;
        }


        private static IList<EthosExtensibleData> ConvertExtendedDataFromDomainToPlatform(IEnumerable<Domain.Base.Entities.EthosExtensibleData> domainExtensibleData)
        {

            var platformExtensibleDatas = new List<EthosExtensibleData>();

            foreach (var extDataItem in domainExtensibleData)
            {
                var newEthosDataItem = new EthosExtensibleData
                {
                    ApiResourceName = extDataItem.ApiResourceName,
                    ApiVersionNumber = extDataItem.ApiVersionNumber,
                    ExtendedSchemaType = extDataItem.ExtendedSchemaType,
                    ResourceId = extDataItem.ResourceId,
                    ColleagueTimeZone = extDataItem.ColleagueTimeZone,
                    ExtendedDataList = new List<EthosExtensibleDataRow>()
                };

                foreach (var dataRow in extDataItem.ExtendedDataList)
                {
                    var row = new EthosExtensibleDataRow
                    {
                        ColleagueColumnName = dataRow.ColleagueColumnName,
                        ColleagueFileName = dataRow.ColleagueFileName,
                        ColleaguePropertyLength = dataRow.ColleaguePropertyLength,
                        FullJsonPath = dataRow.FullJsonPath,
                        JsonPath = dataRow.JsonPath,
                        JsonTitle = dataRow.JsonTitle,
                        ExtendedDataValue = dataRow.ExtendedDataValue
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
    }
}
