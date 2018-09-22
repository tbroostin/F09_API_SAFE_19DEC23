// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Exposes Academic Progress Appeal Codes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AcademicProgressAppealCodesController : BaseCompressedApiController
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
        public AcademicProgressAppealCodesController(IAdapterRegistry adapterRegistry, ILogger logger, IFinancialAidReferenceDataRepository financialAidReferenceDataRepository)
        {
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
            this.financialAidReferenceDataRepository = financialAidReferenceDataRepository;
        }

        /// <summary>
        /// Get all Academic Progress Appeal Codes objects.
        /// An Academic Progress Appeal Code indicates an appeal of an academic progress evaluation.
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns></returns>
        public async Task<IEnumerable<Dtos.FinancialAid.AcademicProgressAppealCode>> GetAcademicProgressAppealCodesAsync()
        {
            try
            {
                var appealEntities = await financialAidReferenceDataRepository.GetAcademicProgressAppealCodesAsync();
                var appealDtoAdapter = adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.AcademicProgressAppealCode, Dtos.FinancialAid.AcademicProgressAppealCode>();
                return appealEntities.Select(s => appealDtoAdapter.MapToType(s));
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error occurred getting Academic Progress Appeal Codes");
                throw CreateHttpResponseException(e.Message);
            }
        }

    }
}