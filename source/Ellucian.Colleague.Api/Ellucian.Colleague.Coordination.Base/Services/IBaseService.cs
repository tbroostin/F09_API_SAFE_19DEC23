// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Web.Http.EthosExtend;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IBaseService
    {
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
        Task<IList<EthosExtensibleData>> GetExtendedEthosDataByResource(EthosResourceRouteInfo ethosResourceRouteInfo, IEnumerable<string> resournceIds);

        /// <summary>
        /// Import Extended Ethos Data
        /// </summary>
        /// <param name="importedDataList">Extended Ethos Data to send into the service</param>
        Task ImportExtendedEthosData(Dictionary<string, string> importedDataList);

        /// <summary>
        /// Gets the extended configuration available on a resource, returns an empty list if there are none
        /// </summary>
        /// <param name="ethosResourceRouteInfo">Ethos Resource Route Info </param>
        /// <returns>List with all of the extended configurations if aavailable. Returns an empty list if none available or none configured</returns>
        Task<EthosExtensibleData> GetExtendedEthosConfigurationByResource(EthosResourceRouteInfo ethosResourceRouteInfo);
    }
}
