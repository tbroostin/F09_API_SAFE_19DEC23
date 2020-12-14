//Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for CourseTopics services
    /// </summary>
    public interface ICipCodeService : IBaseService
    {
          
         Task<IEnumerable<Dtos.CipCode>> GetCipCodesAsync(bool bypassCache = false);
               
        Task<Dtos.CipCode> GetCipCodeByGuidAsync(string id, bool bypassCache = true);
    }
}
