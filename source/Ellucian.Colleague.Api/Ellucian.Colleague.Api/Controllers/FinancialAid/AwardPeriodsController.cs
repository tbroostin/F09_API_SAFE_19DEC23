/*Copyright 2014-2018 Ellucian Company L.P. and its affiliates*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Controller exposes Colleague Financial Aid AwardPeriods resources
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AwardPeriodsController : BaseCompressedApiController
    {
        private readonly IFinancialAidReferenceDataRepository FinancialAidReferenceDataRepository;
        private readonly IAdapterRegistry AdapterRegistry;
        private readonly ILogger Logger;

        /// <summary>
        /// Initializes a new instance of the AwardPeriodController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IFinancialAidReferenceDataRepository">IFinancialAidReferenceDataRepository</see></param>
        /// <param name="logger">Transaction logger of type <see cref="ILogger">ILogger</see></param>
        public AwardPeriodsController(IAdapterRegistry adapterRegistry, IFinancialAidReferenceDataRepository referenceDataRepository, ILogger logger)
        {
            AdapterRegistry = adapterRegistry;
            FinancialAidReferenceDataRepository = referenceDataRepository;
            Logger = logger;
        }

        /// <summary>
        /// Get all the AwardPeriods
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A set of AwardPeriod DTOs</returns> 
        public IEnumerable<AwardPeriod> GetAwardPeriods()
        {
            try
            {
                var AwardPeriodCollection = FinancialAidReferenceDataRepository.AwardPeriods;

                // Get the right adapter for the type mapping
                var awardPeriodDtoAdapter = AdapterRegistry.GetAdapter<Ellucian.Colleague.Domain.FinancialAid.Entities.AwardPeriod, AwardPeriod>();

                // Map the award periods entity to the award periods DTO
                var awardPeriodDtoCollection = new List<AwardPeriod>();
                foreach (var awardPeriod in AwardPeriodCollection)
                {
                    awardPeriodDtoCollection.Add(awardPeriodDtoAdapter.MapToType(awardPeriod));
                }

                return awardPeriodDtoCollection;
            }
            catch (KeyNotFoundException knfe)
            {
                Logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("AwardPeriods", "");
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting AwardPeriods resource. See log for details");
            }
        }

    }
}
