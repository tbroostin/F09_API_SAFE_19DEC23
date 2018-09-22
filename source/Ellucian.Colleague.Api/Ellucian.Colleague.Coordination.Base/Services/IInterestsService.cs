// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IInterestsService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.Interest>> GetHedmInterestsAsync(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.Interest> GetHedmInterestByIdAsync(string id);

        Task<IEnumerable<Ellucian.Colleague.Dtos.InterestArea>> GetInterestAreasAsync(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.InterestArea> GetInterestAreasByIdAsync(string id);
    }
}