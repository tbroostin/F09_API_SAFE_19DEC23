// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for PersonVisasService
    /// </summary>
    public interface IPersonVisasService : IBaseService
    {
        Task<Tuple<IEnumerable<Dtos.PersonVisa>, int>> GetAllAsync(int offset, int limit, string person, bool bypassCache);
        Task<Tuple<IEnumerable<Dtos.PersonVisa>, int>> GetAll2Async(int offset, int limit, string person, string visaTypeCategory, string visaTypeDetail, bool bypassCache);
        Task<Ellucian.Colleague.Dtos.PersonVisa> GetPersonVisaByIdAsync(string id);
        Task<Dtos.PersonVisa> PostPersonVisaAsync(Dtos.PersonVisa PersonVisa);
        Task<Dtos.PersonVisa> PutPersonVisaAsync(string id, Dtos.PersonVisa PersonVisa);
        Task DeletePersonVisaAsync(string id);
    }
}