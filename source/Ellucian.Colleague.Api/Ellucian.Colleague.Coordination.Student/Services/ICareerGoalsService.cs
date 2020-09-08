//Copyright 2020 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for CareerGoals services
    /// </summary>
    public interface ICareerGoalsService : IBaseService
    {

        /// <summary>
        /// Gets all career-goals
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="CareerGoals">careerGoals</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.CareerGoals>> GetCareerGoalsAsync(bool bypassCache = false);

        /// <summary>
        /// Get a careerGoals by guid.
        /// </summary>
        /// <param name="guid">Guid of the careerGoals in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="CareerGoals">careerGoals</see></returns>
        Task<Ellucian.Colleague.Dtos.CareerGoals> GetCareerGoalsByGuidAsync(string guid, bool bypassCache = true);


    }
}
