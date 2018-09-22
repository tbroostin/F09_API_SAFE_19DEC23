/*Copyright 2014-2018 Ellucian Company L.P. and its affiliates.*/
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
    /// Exposes access to Financial Aid Award Years data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AwardYearsController : BaseCompressedApiController
    {
        private readonly IFinancialAidReferenceDataRepository FinancialAidReferenceDataRepository;
        private readonly IAdapterRegistry AdapterRegistry;
        private readonly ILogger Logger;

        /// <summary>
        /// AwardYearsController constructor
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="financialAidReferenceDataRepository">FinancialAidReferenceDataRepository</param>
        /// <param name="logger">Logger</param>
        public AwardYearsController(IAdapterRegistry adapterRegistry, IFinancialAidReferenceDataRepository financialAidReferenceDataRepository, ILogger logger)
        {
            AdapterRegistry = adapterRegistry;
            FinancialAidReferenceDataRepository = financialAidReferenceDataRepository;
            Logger = logger;
        }

        /// <summary>
        /// Get a list of all Financial Aid Years from Colleague
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A collection of AwardYear data objects</returns>
        public IEnumerable<AwardYear> GetAwardYears()
        {
            try
            {
                var AwardYearCollection = FinancialAidReferenceDataRepository.AwardYears;

                //Get the adapter for the type mapping
                var awardYearDtoAdapter = AdapterRegistry.GetAdapter<Domain.FinancialAid.Entities.AwardYear, AwardYear>();

                //Map the awardyear entity to the awardyear dto
                var awardYearDtoCollection = new List<AwardYear>();
                foreach (var year in AwardYearCollection)
                {
                    awardYearDtoCollection.Add(awardYearDtoAdapter.MapToType(year));
                }

                return awardYearDtoCollection;
            }
            catch (KeyNotFoundException knfe)
            {
                Logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("AwardYears", "");
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting AwardYears resource. See log for details");
            }
        }
    }
}
