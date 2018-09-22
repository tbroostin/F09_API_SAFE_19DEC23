//Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Web.Adapters;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.FinancialAid;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Provides access to award letter configurations data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AwardLetterConfigurationsController : BaseCompressedApiController
    {
        private readonly IFinancialAidReferenceDataRepository financialAidReferenceDataRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// AwardLetterConfigurationsController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="financialAidReferenceDataRepository">Financial Aid Reference Data Repository of type <see cref="IFinancialAidReferenceDataRepository">IFinancialAidReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public AwardLetterConfigurationsController(IAdapterRegistry adapterRegistry, IFinancialAidReferenceDataRepository financialAidReferenceDataRepository, ILogger logger)
        {
            this.adapterRegistry = adapterRegistry;
            this.financialAidReferenceDataRepository = financialAidReferenceDataRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Gets a list of AwardLetterConfiguration DTOs build from Colleague award letter parameter records
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>List of AwardLetterConfiguration DTOs</returns>
        public async Task<IEnumerable<AwardLetterConfiguration>> GetAwardLetterConfigurationsAsync()
        {
            try
            {
                var awardLetterConfigurations = await financialAidReferenceDataRepository.GetAwardLetterConfigurationsAsync();

                var awardLetterConfigurationDtoAdapter = adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.AwardLetterConfiguration, AwardLetterConfiguration>();

                var awardLetterConfigurationDtos = new List<AwardLetterConfiguration>();
                foreach(var configuration in awardLetterConfigurations){
                    awardLetterConfigurationDtos.Add(awardLetterConfigurationDtoAdapter.MapToType(configuration));
                }
                return awardLetterConfigurationDtos;
            }            
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting Award Letter Configurations resource. See log for details");
            }
        }
    }
}