using System;
namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    public interface IRehireTypeService
    {
        System.Threading.Tasks.Task<Ellucian.Colleague.Dtos.RehireType> GetRehireTypeByGuidAsync(string guid);
        System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Ellucian.Colleague.Dtos.RehireType>> GetRehireTypesAsync(bool bypassCache = false);
    }
}
