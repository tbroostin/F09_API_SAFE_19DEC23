//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for SectionTitleType services
    /// </summary>
    public interface ISectionTitleTypesService : IBaseService
    {

        /// <summary>
        /// Gets all section-title-types
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="SectionTitleType">sectionTitleTypes</see> objects</returns>
        Task<IEnumerable<Dtos.SectionTitleType>> GetSectionTitleTypesAsync(bool bypassCache = false);

        /// <summary>
        /// Get a sectionTitleType by guid.
        /// </summary>
        /// <param name="guid">Guid of the sectionTitleTypes in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="SectionTitleType">sectionTitleTypes</see></returns>
        Task<Dtos.SectionTitleType> GetSectionTitleTypeByGuidAsync(string guid, bool bypassCache = true);


    }
}
