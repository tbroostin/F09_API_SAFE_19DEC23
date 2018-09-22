// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Web.Http;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to Gown Sizes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class GownSizesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository referenceDataRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// AdmittedStatusesController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Repository of type <see cref="ILogger">IStudentReferenceDataRepository</see></param>
        public GownSizesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ILogger logger)
        {
            this.adapterRegistry = adapterRegistry;
            this.referenceDataRepository = referenceDataRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves all Gown Sizes with PilotFlag set to Yes or True.
        /// </summary>
        /// <returns>All <see cref="GownSize">GownSize</see> codes and descriptions.</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.GownSize>> GetAsync()
        {
            try
            {
                var gownSizeCollection = await referenceDataRepository.GetGownSizesAsync();
                // Get the right adapter for the type mapping
                var gownSizeDtoAdapter = adapterRegistry.GetAdapter<Domain.Student.Entities.GownSize, GownSize>();

                // Map the gown size entity to the program DTO
                var gownSizeDtoCollection = new List<Ellucian.Colleague.Dtos.Student.GownSize>();
                if (gownSizeCollection != null && gownSizeCollection.Any())
                {
                    foreach (var gownSize in gownSizeCollection)
                    {
                        gownSizeDtoCollection.Add(gownSizeDtoAdapter.MapToType(gownSize));
                    }
                }
                return gownSizeDtoCollection;
            }
            catch (System.Exception ex)
            {
                logger.Error(ex, "Unable to retrieve GownSize data");
                throw CreateHttpResponseException("Unable to retrieve GownSize data");
            }

        }
    }
}