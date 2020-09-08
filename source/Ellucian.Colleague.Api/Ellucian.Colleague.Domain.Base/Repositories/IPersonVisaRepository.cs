// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Data.Colleague;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Interface for PersonVisaRepository
    /// </summary>
    public interface IPersonVisaRepository : IEthosExtended
    {
        Task<Tuple<IEnumerable<Ellucian.Colleague.Domain.Base.Entities.PersonVisa>, int>> GetAllPersonVisasAsync(int offset, int limit, string person, bool bypassCache);
        Task<Tuple<IEnumerable<Ellucian.Colleague.Domain.Base.Entities.PersonVisa>, int>> GetAllPersonVisas2Async(int offset, int limit, string person, List<string> visaTypeCategory, string visaTypeDetail, bool bypassCache);
        Task<Ellucian.Colleague.Domain.Base.Entities.PersonVisa> GetPersonVisaByIdAsync(string id);
        Task<GuidLookupResult> GetRecordInfoFromGuidAsync(string id);
        Task<Ellucian.Colleague.Domain.Base.Entities.PersonVisa> UpdatePersonVisaAsync(Ellucian.Colleague.Domain.Base.Entities.PersonVisaRequest personVisaRequest);
        Task DeletePersonVisaAsync(string id, string personId);
    }
}