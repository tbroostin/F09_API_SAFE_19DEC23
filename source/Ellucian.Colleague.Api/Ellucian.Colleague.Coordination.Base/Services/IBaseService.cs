// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Web.Http.EthosExtend;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IBaseService
    {

        /// <summary>
        /// Check for permissions and build error message.
        /// If multiple permissions are assigned, then only one is required for access.
        /// </summary>
        /// <param name="permissionsTuple">Tuple consisting of:
        /// 1. string[]: array of valid permissions
        /// 2. string: http method (ex: 'GET')
        /// 3. string: resource name (ex: 'person-holds')</param>
        /// <returns>bool</returns>
        bool ValidatePermissions(Tuple<string[], string, string> permissions);


        /// <summary>
        /// Gets the list of EEDM data privacy settings by user based on user, roles and permissions
        /// </summary>
        /// <param name="apiName">name of the api (eedm schema name)</param>
        /// <param name="bypassCache"></param>
        /// <returns>list of data privacy strings to apply</returns>
        Task<IEnumerable<string>> GetDataPrivacyListByApi(string apiName, bool bypassCache = false);

        /// <summary>
        /// Gets the list of EEDM data privacy settings by user based on user, roles and permissions
        /// </summary>
        /// <param name="ethosResourceRouteInfo">ethos resource info from route</param>
        /// <param name="bypassCache"></param>
        /// <returns>list of data privacy strings to apply</returns>
        Task<IEnumerable<string>> GetDataPrivacyListByApi(EthosResourceRouteInfo ethosResourceRouteInfo, bool bypassCache = false);

        /// <summary>
        /// Gets the extended data available on a resource, returns an empty list if there are no 
        /// </summary>
        /// <param name="ethosResourceRouteInfo">Ethos Resource Route Info </param>
        /// <param name="resournceIds">IEnumerable of the ids for the resources in guid form</param>
        /// <returns>List with all of the extended data if aavailable. Returns an empty list if none available or none configured</returns>
        Task<IList<EthosExtensibleData>> GetExtendedEthosDataByResource(EthosResourceRouteInfo ethosResourceRouteInfo, IEnumerable<string> resournceIds, bool bypassCache = false, bool useRecordKey = false);

      
        /// <summary>
        /// Import Extended Ethos Data
        /// </summary>
        /// <param name="importedDataList">Extended Ethos Data to send into the service</param>
        Task ImportExtendedEthosData(Dictionary<string, string> importedDataList);

        /// <summary>
        /// Returns the default API version when API builder is called without an accept header.
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<string> GetEthosExtensibilityResourceDefaultVersion(string resourceName, bool bypassCache = false);

        /// <summary>
        /// Gets the extended configuration available on a resource, returns an empty list if there are none
        /// </summary>
        /// <param name="ethosResourceRouteInfo">Ethos Resource Route Info </param>
        /// <returns>List with all of the extended configurations if aavailable. Returns an empty list if none available or none configured</returns>
        Task<EthosExtensibleData> GetExtendedEthosConfigurationByResource(EthosResourceRouteInfo ethosResourceRouteInfo, bool bypassCache = false);

        /// <summary>
        /// Gets the extended configuration available on a resource, returns an empty list if there are none
        /// </summary>
        /// <param name="ethosResourceRouteInfo">Ethos Resource Route Info </param>
        /// <returns>List with all of the extended configurations if available. Returns an empty list if none available or none configured</returns>
        Task<EthosExtensibleData> GetBulkExtendedEthosConfigurationByResource(EthosResourceRouteInfo ethosResourceRouteInfo, bool bypassCache = false);

        /// <summary>
        /// Gets  all extended configurations, returns null if there are none
        /// </summary>
        /// <param name="bypassCache">bypassCache </param>
        /// <returns>List with all of the extended configurations if available. Returns an null if none available or none configured</returns>
        Task<List<Domain.Base.Entities.EthosExtensibleData>> GetAllExtendedEthosConfigurations(bool bypassCache = false);

        /// <summary>
        /// Gets the extended configuration available on a resource, returns an empty list if there are none
        /// </summary>
        /// <param name="ethosResourceRouteInfo">Ethos Resource Route Info </param>
        /// <returns>List with all of the extended configurations if aavailable. Returns an empty list if none available or none configured</returns>
        Task<EthosApiConfiguration> GetEthosApiConfigurationByResource(EthosResourceRouteInfo ethosResourceRouteInfo, bool bypassCache = false);
    }
}
