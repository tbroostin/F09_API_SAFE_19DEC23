// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IAcademicCatalogService : IBaseService
    {
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets all academic catalogs
        /// </summary>
        /// <returns>Collection of AcademicCatalog2 DTO objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.AcademicCatalog2>> GetAcademicCatalogs2Async(bool bypassCache = false);

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Get an academic catalog from its GUID
        /// </summary>
        /// <returns>AcademicCatalog2 DTO object</returns>
        Task<Ellucian.Colleague.Dtos.AcademicCatalog2> GetAcademicCatalogByGuid2Async(string guid);

        /// <remarks>FOR USE WITH ELLUCIAN SS</remarks>
        /// <summary>
        /// Gets all academic catalogs
        /// </summary>
        /// <returns>Collection of Catalog DTO objects</returns>
        Task<IEnumerable<Dtos.Student.Catalog>> GetAllAcademicCatalogsAsync(bool bypassCache = false);
    }
}
