// Copyright 2019 - 2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to the registration marketing source data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class RegistrationMarketingSourcesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository referenceDataRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the RegistrationMarketingSourcesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">Logger</see></param>
        public RegistrationMarketingSourcesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ILogger logger)
        {
            this.referenceDataRepository = referenceDataRepository;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Returns all registration marketing sources
        /// </summary>
        /// <returns>Collection of <see cref="RegistrationMarketingSource">registration marketing sources</see></returns>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        public async Task<IEnumerable<RegistrationMarketingSource>> GetRegistrationMarketingSourcesAsync()
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
                var registrationMarketingSourceDtoCollection = new List<Ellucian.Colleague.Dtos.Student.RegistrationMarketingSource>();
                var registrationMarketingSourceCollection = await referenceDataRepository.GetRegistrationMarketingSourcesAsync(bypassCache);
                var registrationMarketingSourceDtoAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationMarketingSource, RegistrationMarketingSource>();
                if (registrationMarketingSourceCollection != null && registrationMarketingSourceCollection.Count() > 0)
                {
                    foreach (var country in registrationMarketingSourceCollection)
                    {
                        try
                        {
                            registrationMarketingSourceDtoCollection.Add(registrationMarketingSourceDtoAdapter.MapToType(country));
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, "Error encountered converting registration marketing source entity to DTO.");
                        }
                    }
                }
                return registrationMarketingSourceDtoCollection;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Session has expired while retrieving registration marketing sources data";
                logger.Error(csse, message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unable to retrieve registration marketing sources.");
            }
        }
    }
}