using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for InstructorTenureTypes services
    /// </summary>
    public interface IInstructorTenureTypesService : IBaseService
    {

        Task<IEnumerable<Ellucian.Colleague.Dtos.InstructorTenureTypes>> GetInstructorTenureTypesAsync(bool bypassCache = false);

        Task<Ellucian.Colleague.Dtos.InstructorTenureTypes> GetInstructorTenureTypesByGuidAsync(string id);
    }
}
