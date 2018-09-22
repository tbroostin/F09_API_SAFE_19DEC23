/*Copyright 2015-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
    /// Exposes ChecklistItems data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FinancialAidChecklistItemsController : BaseCompressedApiController
    {
        private readonly IFinancialAidReferenceDataRepository _financialAidReferenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// ChecklistItems constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="financialAidReferenceDataRepository">Financial Aid Reference Data Repository of type <see cref="IFinancialAidReferenceDataRepository">IFinancialAidReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public FinancialAidChecklistItemsController(IAdapterRegistry adapterRegistry, IFinancialAidReferenceDataRepository financialAidReferenceDataRepository, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _financialAidReferenceDataRepository = financialAidReferenceDataRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all ChecklistItems objects that could potentially comprise a student's Financial Aid Checklist
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A List of all Checklist Items</returns>
        [HttpGet]
        public IEnumerable<ChecklistItem> GetChecklistItems()
        {
            try
            {
                var checklistItems = _financialAidReferenceDataRepository.ChecklistItems;

                var checklistItemDtoAdapter = _adapterRegistry.GetAdapter<Colleague.Domain.FinancialAid.Entities.ChecklistItem, Colleague.Dtos.FinancialAid.ChecklistItem>();

                return checklistItems.Select(budget =>
                    checklistItemDtoAdapter.MapToType(budget));
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting Checklist Items resource. See log for details");
            }
        }

    }
}