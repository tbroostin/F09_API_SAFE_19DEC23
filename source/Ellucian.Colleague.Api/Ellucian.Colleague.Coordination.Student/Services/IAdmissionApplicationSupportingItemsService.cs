//Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for AdmissionApplicationSupportingItems services
    /// </summary>
    public interface IAdmissionApplicationSupportingItemsService : IBaseService
    {
        /// <summary>
        /// Gets all admission-application-supporting-items
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="Dtos.AdmissionApplicationSupportingItems">admissionApplicationSupportingItems</see> objects</returns>          
        Task<Tuple<IEnumerable<Dtos.AdmissionApplicationSupportingItems>, int>> GetAdmissionApplicationSupportingItemsAsync(int offset, int limit, bool bypassCache = false);

        /// <summary>
        /// Get a admissionApplicationSupportingItems by guid.
        /// </summary>
        /// <param name="guid">Guid of the admissionApplicationSupportingItems in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="Dtos.AdmissionApplicationSupportingItems">admissionApplicationSupportingItems</see></returns>
        Task<Dtos.AdmissionApplicationSupportingItems> GetAdmissionApplicationSupportingItemsByGuidAsync(string guid, bool bypassCache = false);

        /// <summary>
        /// Update a admissionApplicationSupportingItems.
        /// </summary>
        /// <param name="admissionApplicationSupportingItems">The <see cref="Dtos.AdmissionApplicationSupportingItems">admissionApplicationSupportingItems</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="Dtos.AdmissionApplicationSupportingItems">admissionApplicationSupportingItems</see></returns>
        Task<Dtos.AdmissionApplicationSupportingItems> UpdateAdmissionApplicationSupportingItemsAsync(Ellucian.Colleague.Dtos.AdmissionApplicationSupportingItems admissionApplicationSupportingItems);

        /// <summary>
        /// Create a admissionApplicationSupportingItems.
        /// </summary>
        /// <param name="admissionApplicationSupportingItems">The <see cref="Dtos.AdmissionApplicationSupportingItems">admissionApplicationSupportingItems</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="Dtos.AdmissionApplicationSupportingItems">admissionApplicationSupportingItems</see></returns>
        Task<Dtos.AdmissionApplicationSupportingItems> CreateAdmissionApplicationSupportingItemsAsync(Ellucian.Colleague.Dtos.AdmissionApplicationSupportingItems admissionApplicationSupportingItems);

    }
}
