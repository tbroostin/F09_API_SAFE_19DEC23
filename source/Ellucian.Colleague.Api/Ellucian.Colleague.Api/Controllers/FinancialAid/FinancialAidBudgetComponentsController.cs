/*Copyright 2014-2018 Ellucian Company L.P. and its affiliates.*/
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
    /// Exposes BudgetComponents data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FinancialAidBudgetComponentsController : BaseCompressedApiController
    {
        private readonly IFinancialAidReferenceDataRepository _FinancialAidReferenceDataRepository;
        private readonly IAdapterRegistry _AdapterRegistry;
        private readonly ILogger _Logger;

        /// <summary>
        /// BudgetComponents constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="financialAidReferenceDataRepository">Financial Aid Reference Data Repository of type <see cref="IFinancialAidReferenceDataRepository">IFinancialAidReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public FinancialAidBudgetComponentsController(IAdapterRegistry adapterRegistry, IFinancialAidReferenceDataRepository financialAidReferenceDataRepository, ILogger logger)
        {
            _AdapterRegistry = adapterRegistry;
            _FinancialAidReferenceDataRepository = financialAidReferenceDataRepository;
            _Logger = logger;
        }

        /// <summary>
        /// Get all BudgetComponent objects for all Financial Aid award years
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A List of all Budget Components</returns>
        [HttpGet]
        public IEnumerable<BudgetComponent> GetBudgetComponents()
        {
            try
            {
                var budgetComponents = _FinancialAidReferenceDataRepository.BudgetComponents;

                var budgetComponentDtoAdapter = _AdapterRegistry.GetAdapter<Colleague.Domain.FinancialAid.Entities.BudgetComponent, Colleague.Dtos.FinancialAid.BudgetComponent>();

                return budgetComponents.Select(budget =>
                    budgetComponentDtoAdapter.MapToType(budget));
            }
            catch (Exception e)
            {
                _Logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting BudgetComponents resource. See log for details");
            }
        }
    }
}