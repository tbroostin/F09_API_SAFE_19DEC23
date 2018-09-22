// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
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
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to the cap size data for graduation.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class CapSizesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository referenceDataRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the CapSizesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">Logger</see></param>
        public CapSizesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ILogger logger)
        {
            this.referenceDataRepository = referenceDataRepository;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves all Cap Sizes.
        /// </summary>
        /// <returns>All <see cref="CapSize">Cap Size</see> codes and descriptions.</returns>
        /// <accessComments>Any authenticated user can retrieve cap sizes.</accessComments>
        public async Task<IEnumerable<CapSize>> GetAsync()
        {
            try
            {
                var capSizeCollection = await referenceDataRepository.GetCapSizesAsync();

                // Get the right adapter for the type mapping
                var capSizeDtoAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.CapSize, CapSize>();

                // Map the CapSize entity to the program DTO
                var capSizeDtoCollection = new List<CapSize>();
                if (capSizeCollection != null && capSizeCollection.Any())
                {
                    foreach (var applicationStatusCategory in capSizeCollection)
                    {
                        capSizeDtoCollection.Add(capSizeDtoAdapter.MapToType(applicationStatusCategory));
                    }
                }
                return capSizeDtoCollection;
            }
            catch (System.Exception e)
            {
                this.logger.Error(e, "Unable to retrieve the Cap Size information");
                throw CreateHttpResponseException("Unable to retrieve data");
            }
        }
    }
}