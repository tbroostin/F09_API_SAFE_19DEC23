// Copyright 2021 Ellucian Company L.P. and its affiliates.
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
    /// Provides access to intent to withdraw code data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class IntentToWithdrawCodesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository referenceDataRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the IntentToWithdrawCodesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">Logger</see></param>
        public IntentToWithdrawCodesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ILogger logger)
        {
            this.referenceDataRepository = referenceDataRepository;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Returns all intent to withdraw codes
        /// </summary>
        /// <returns>Collection of <see cref="IntentToWithdrawCode">intent to withdraw codes</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if there was a Colleage data or configuration error.</exception>
        /// <accessComments>Any authenticated user can retrieve intent to withdraw codes.</accessComments>
        public async Task<IEnumerable<IntentToWithdrawCode>> GetIntentToWithdrawCodesAsync()
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
                var dtoCollection = new List<Ellucian.Colleague.Dtos.Student.IntentToWithdrawCode>();
                var entityCollection = await referenceDataRepository.GetIntentToWithdrawCodesAsync(bypassCache);
                var dtoAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.IntentToWithdrawCode, IntentToWithdrawCode>();
                if (entityCollection != null && entityCollection.Count() > 0)
                {
                    foreach (var entity in entityCollection)
                    {
                        try
                        {
                            dtoCollection.Add(dtoAdapter.MapToType(entity));
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, "Error encountered converting intent to withdraw code entity to DTO.");
                        }
                    }
                }
                return dtoCollection;
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unable to retrieve intent to withdraw codes.");
            }
        }

    }
}