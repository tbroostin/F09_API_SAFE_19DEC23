// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Data.Colleague;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Interface for PersonVisaRepository
    /// </summary>
    public interface IEthosApiBuilderRepository : IEthosExtended
    {
        Tuple<bool, List<string>, List<string>> GetSecureDataDefinition();

        Task<Tuple<IEnumerable<Entities.EthosApiBuilder>, int>> GetEthosApiBuilderAsync(int offset, int limit, EthosApiConfiguration configuration, Dictionary<string, EthosExtensibleDataFilter> filterDictionary, bool bypassCache);

        Task<Entities.EthosApiBuilder> GetEthosApiBuilderByIdAsync(string id, EthosApiConfiguration configuration);

        Task<Entities.EthosApiBuilder> UpdateEthosApiBuilderAsync(EthosApiBuilder ethisApiBuilderEntity, EthosApiConfiguration configuration);

        Task<Dictionary<string, Dictionary<string, string>>> UpdateEthosBusinessProcessApiAsync(EthosApiBuilder ethisApiBuilderEntity, EthosApiConfiguration configuration, Dictionary<string, EthosExtensibleDataFilter> filterDictionary, bool returnRestrictedFields, EthosExtensibleData extendedEthosVersion);

        Task DeleteEthosApiBuilderAsync(string id, EthosApiConfiguration configuration);

        Task<GuidLookupResult> GetRecordInfoFromGuidAsync(string id);

        Task<string> GetRecordIdFromTranslationAsync(string sourceData, string entityName, string sourceColumn = "", string tableName = "");
    }
}