//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for CourseTitleType services
    /// </summary>
    public interface ICourseTitleTypesService : IBaseService
    {

        /// <summary>
        /// Gets all CourseTitleTypes
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="CourseTitleType">courseTitleTypes</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.CourseTitleType>> GetCourseTitleTypesAsync(bool bypassCache = false);

        /// <summary>
        /// Get a CourseTitleType by guid.
        /// </summary>
        /// <param name="guid">Guid of the CourseTitleType in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="CourseTitleType">courseTitleTypes</see></returns>
        Task<Ellucian.Colleague.Dtos.CourseTitleType> GetCourseTitleTypeByGuidAsync(string guid, bool bypassCache = true);


    }
}
