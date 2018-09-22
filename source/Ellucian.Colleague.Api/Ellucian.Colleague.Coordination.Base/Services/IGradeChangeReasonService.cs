// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IGradeChangeReasonService
    {
        /// <summary>
        /// Gets all grade change reason
        /// </summary>
        /// <returns>Collection of grade change reasons DTO objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.GradeChangeReason>> GetAsync(bool bypassCache);
        
        /// <summary>
        /// Get grade change reason by id
        /// </summary>
        /// <returns>a single grade change reason DTO objects</returns>
        Task<Dtos.GradeChangeReason> GetGradeChangeReasonByIdAsync(string id);        
    }
}
