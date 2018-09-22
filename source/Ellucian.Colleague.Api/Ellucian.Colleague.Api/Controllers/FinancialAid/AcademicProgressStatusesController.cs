// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Dtos.FinancialAid;
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

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Exposes methods to interact with AcademicProgressStatus resources
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AcademicProgressStatusesController: BaseCompressedApiController
    {
        private readonly IFinancialAidReferenceDataRepository financialAidReferenceDataRepository;

        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        /// <summary>
        /// Constructor to AcademicProgressStatusesController
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        /// <param name="financialAidReferenceDataRepository"></param>
        public AcademicProgressStatusesController(IAdapterRegistry adapterRegistry, ILogger logger, IFinancialAidReferenceDataRepository financialAidReferenceDataRepository)
        {
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
            this.financialAidReferenceDataRepository = financialAidReferenceDataRepository;
        }
        /// <summary>
        /// Get all AcademicProgressStatus objects. An AcademicProgressStatus indicates the status of an academic progress evaluation.
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns></returns>
        public async Task<IEnumerable<AcademicProgressStatus>> GetAcademicProgressStatusesAsync()
        {
            try
            {
                var statusEntities = await financialAidReferenceDataRepository.GetAcademicProgressStatusesAsync();
                var statusDtoAdapter = adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.AcademicProgressStatus, AcademicProgressStatus>();
                return statusEntities.Select(s => statusDtoAdapter.MapToType(s));
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error occurred getting Academic Progress Statuses");
                throw CreateHttpResponseException(e.Message);
            }
        }
    }
}