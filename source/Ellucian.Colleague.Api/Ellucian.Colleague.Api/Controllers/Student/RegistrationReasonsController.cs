// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to the registration reason data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class RegistrationReasonsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository referenceDataRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the RegistrationReasonsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">Logger</see></param>
        public RegistrationReasonsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ILogger logger)
        {
            this.referenceDataRepository = referenceDataRepository;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Returns all registration reasons
        /// </summary>
        /// <returns>Collection of <see cref="RegistrationReason">registration reasons</see></returns>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        public async Task<IEnumerable<RegistrationReason>> GetRegistrationReasonsAsync()
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var registrationReasonDtoCollection = new List<Ellucian.Colleague.Dtos.Student.RegistrationReason>();
                var registrationReasonCollection = await referenceDataRepository.GetRegistrationReasonsAsync(bypassCache);
                var registrationReasonDtoAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationReason, RegistrationReason>();
                if (registrationReasonCollection != null && registrationReasonCollection.Count() > 0)
                {
                    foreach (var country in registrationReasonCollection)
                    {
                        try
                        {
                            registrationReasonDtoCollection.Add(registrationReasonDtoAdapter.MapToType(country));
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, "Error encountered converting registration reason entity to DTO.");
                        }
                    }
                }
                return registrationReasonDtoCollection;
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unable to retrieve registration reasons.");
            }
        }
    }
}