// Copyright 2019 Ellucian Company L.P. and its affiliates.
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
        Task<Dtos.EthosApiBuilder> PutEthosApiBuilderAsync(string id, Dtos.EthosApiBuilder extendedData, string resourceName);
        Task<Dtos.EthosApiBuilder> PostEthosApiBuilderAsync(Dtos.EthosApiBuilder extendedData, string resourceName);
        Task DeleteEthosApiBuilderAsync(string id, string resourceName);
        Task<string> GetRecordIdFromGuidAsync(string guid, string entityName, string secondaryKey = "");
        Task<string> GetRecordIdFromTranslationAsync(string sourceData, string entityName, string sourceColumn = "", string tableName = "");
    }
}