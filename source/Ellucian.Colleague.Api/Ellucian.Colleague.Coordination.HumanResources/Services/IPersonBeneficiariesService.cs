//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for PersonBeneficiaries services
    /// </summary>
    public interface IPersonBeneficiariesService : IBaseService
    {
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.PersonBeneficiaries>, int>> GetPersonBeneficiariesAsync(int offset, int limit, bool bypassCache = false);

        //Task<IEnumerable<Ellucian.Colleague.Dtos.PersonBeneficiaries>> GetPersonBeneficiariesAsync(bool bypassCache = false);

        Task<Ellucian.Colleague.Dtos.PersonBeneficiaries> GetPersonBeneficiariesByGuidAsync(string id);
    }
}
