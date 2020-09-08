//Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for EducationalGoals services
    /// </summary>
    public interface IEducationalGoalsService : IBaseService
    {
          
        /// <summary>
        /// Gets all educational-goals
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="EducationalGoals">educationalGoals</see> objects</returns>
         Task<IEnumerable<Ellucian.Colleague.Dtos.EducationalGoals>> GetEducationalGoalsAsync(bool bypassCache = false);
               
        /// <summary>
        /// Get a educationalGoals by guid.
        /// </summary>
        /// <param name="guid">Guid of the educationalGoals in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="EducationalGoals">educationalGoals</see></returns>
        Task<Ellucian.Colleague.Dtos.EducationalGoals> GetEducationalGoalsByGuidAsync(string guid, bool bypassCache = true);
    }
}