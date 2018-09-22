// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
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
    /// Provides access to the hold request types primarily used to hold student requests.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class HoldRequestTypesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository referenceDataRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the HoldRequestTypesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">Logger</see></param>
        public HoldRequestTypesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ILogger logger)
        {
            this.referenceDataRepository = referenceDataRepository;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves all Hold Request Types.
        /// </summary>
        /// <returns>All <see cref="HoldRequestType">Hold Request type</see> codes and descriptions.</returns>
        /// <accessComments>Any authenticated user can retrieve hold request types.</accessComments>
        public async Task<IEnumerable<HoldRequestType>> GetHoldRequestTypesAsync()
        {
            try
            {
                var holdRequestCollection = await referenceDataRepository.GetHoldRequestTypesAsync();

                // Get the right adapter for the type mapping
                var holdRequestDtoAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.HoldRequestType, HoldRequestType>();

                // Map the HoldRequestType entity to the DTO
                var holdRequestDtoCollection = new List<HoldRequestType>();
                if (holdRequestCollection != null && holdRequestCollection.Any())
                {
                    foreach (var yc in holdRequestCollection)
                    {
                        holdRequestDtoCollection.Add(holdRequestDtoAdapter.MapToType(yc));
                    }
                }
                return holdRequestDtoCollection;
            }
            catch (System.Exception e)
            {
                this.logger.Error(e, "Unable to retrieve the hold request type information");
                throw CreateHttpResponseException("Unable to retrieve data");
            }
        }
    }
}