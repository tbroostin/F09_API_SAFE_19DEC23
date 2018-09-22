//Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
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
    /// Controller exposes read-only access to Financial Aid AwardStatus data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AwardStatusesController : BaseCompressedApiController
    {
        private readonly IFinancialAidReferenceDataRepository FinancialAidReferenceDataRepository;
        private readonly IAdapterRegistry AdapterRegistry;
        private readonly ILogger Logger;

        /// <summary>
        /// AwardStatuses Controller constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter Registry</param>
        /// <param name="financialAidReferenceDataRepository">FinancialAid Reference Data Repository</param>
        /// <param name="logger">Logger</param>
        public AwardStatusesController(IAdapterRegistry adapterRegistry, IFinancialAidReferenceDataRepository financialAidReferenceDataRepository, ILogger logger)
        {
            AdapterRegistry = adapterRegistry;
            FinancialAidReferenceDataRepository = financialAidReferenceDataRepository;
            Logger = logger;
        }

        /// <summary>
        /// Get a list of all Financial Aid Award Status codes from Colleague
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A collection of AwardStatus data objects</returns>
        public IEnumerable<AwardStatus> GetAwardStatuses()
        {
            try
            {
                var AwardStatusCollection = FinancialAidReferenceDataRepository.AwardStatuses;

                //Get the adapter for the type mapping
                var awardStatusDtoAdapter = AdapterRegistry.GetAdapter<Domain.FinancialAid.Entities.AwardStatus, AwardStatus>();

                var awardStatusDtoCollection = new List<AwardStatus>();
                foreach (var status in AwardStatusCollection)
                {
                    awardStatusDtoCollection.Add(awardStatusDtoAdapter.MapToType(status));
                }

                return awardStatusDtoCollection;
            }
            catch (KeyNotFoundException knfe)
            {
                Logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("AwardStatuses", "");
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting AwardStatuses resource. See log for details");
            }
        }
    }
}
