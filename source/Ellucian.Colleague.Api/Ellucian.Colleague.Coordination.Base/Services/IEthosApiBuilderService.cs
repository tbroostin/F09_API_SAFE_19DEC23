// Copyright 2019-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Http.EthosExtend;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for EthosApiBuildersService
    /// </summary>
    public interface IEthosApiBuilderService : IBaseService
    {
        Task<Tuple<IEnumerable<Dtos.EthosApiBuilder>, int>> GetEthosApiBuilderAsync(int offset, int limit, string resourceName, Dictionary<string, EthosExtensibleDataFilter> filterDictionary, bool bypassCache);
        Task<Dtos.EthosApiBuilder> GetEthosApiBuilderByIdAsync(string id, string resourceName);
        Task<Dtos.EthosApiBuilder> PutEthosApiBuilderAsync(string id, Dtos.EthosApiBuilder extendedData, EthosResourceRouteInfo ethosResourceRouteInfo, Dictionary<string, EthosExtensibleDataFilter> filterDictionary);
        Task<Dtos.EthosApiBuilder> PostEthosApiBuilderAsync(Dtos.EthosApiBuilder extendedData, EthosResourceRouteInfo ethosResourceRouteInfo, Dictionary<string, EthosExtensibleDataFilter> filterDictionary);
        Task DeleteEthosApiBuilderAsync(string id, string resourceName);
        Task<Ellucian.Colleague.Domain.Base.Entities.EthosApiConfiguration> GetEthosApiConfigurationByResourceAsync(string resourceName);
        Task<string> GetRecordIdFromGuidAsync(string guid, string entityName, string secondaryKey = "");
        Task<string> GetRecordIdFromTranslationAsync(string sourceData, string entityName, string sourceColumn = "", string tableName = "");
        string EncodePrimaryKey(string id);
        string UnEncodePrimaryKey(string id);
        Task<string> GetEthosApiBuilderPermissionsAsync(string resourceName, string method);
        /// <summary>
        /// Gets  extended configurations by resource, returns null if there are none
        /// </summary>
        /// <param name="bypassCache">bypassCache </param>
        /// <returns>List with all of the extended configurations if available. Returns an null if none available or none configured</returns>
        Task<List<Domain.Base.Entities.EthosExtensibleData>> GetExtendedEthosVersionsConfigurationsByResource(EthosResourceRouteInfo ethosResourceRouteInfo, bool bypassCache = false, bool customOnly = true);
        /// <summary>
        /// returns true if an api has any data privacy set up
        /// </summary>
        /// <param name="apiName">name of the api (eedm schema name)</param>
        /// <param name="bypassCache"></param>
        /// <returns>true if there is data privacy</returns>
        Task<bool> CheckDataPrivacyByApi(string apiName, bool bypassCache = false);
    }
}