using Ellucian.Colleague.Coordination.Base.Services;
//Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    public interface IDeductionTypesService : IBaseService
    {
        Task<IEnumerable<Dtos.DeductionType>> GetDeductionTypesAsync(bool bypassCache = false);
        Task<Dtos.DeductionType> GetDeductionTypeByIdAsync(string id);

        Task<IEnumerable<Dtos.DeductionType2>> GetDeductionTypes2Async(bool bypassCache = false);
        Task<Dtos.DeductionType2> GetDeductionTypeById2Async(string id);
    }
}
