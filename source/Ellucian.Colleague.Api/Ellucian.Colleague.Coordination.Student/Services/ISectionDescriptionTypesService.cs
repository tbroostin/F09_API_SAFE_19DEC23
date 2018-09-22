//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for SectionDescriptionType services
    /// </summary>
    public interface ISectionDescriptionTypesService : IBaseService
    {

        /// <summary>
        /// Gets all section-description-types
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="SectionDescriptionType">SectionDescriptionTypes</see> objects</returns>
        Task<IEnumerable<Dtos.SectionDescriptionTypes>> GetSectionDescriptionTypesAsync(bool bypassCache = false);

        /// <summary>
        /// Get a SectionDescriptionTypes by guid.
        /// </summary>
        /// <param name="guid">Guid of the SectionDescriptionTypes in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="SectionDescriptionTypes">SectionDescriptionTypes</see></returns>
        Task<Dtos.SectionDescriptionTypes> GetSectionDescriptionTypeByGuidAsync(string guid, bool bypassCache = true);


    }
}
