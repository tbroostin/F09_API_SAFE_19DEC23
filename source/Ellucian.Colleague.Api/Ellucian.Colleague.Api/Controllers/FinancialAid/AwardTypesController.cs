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
    /// Exposes access to Financial Aid Award Types data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AwardTypesController : BaseCompressedApiController
    {
        private readonly IFinancialAidReferenceDataRepository FinancialAidReferenceDataRepository;
        private readonly IAdapterRegistry AdapterRegistry;
        private readonly ILogger Logger;

        /// <summary>
        /// AwardTypesController constructor
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="financialAidReferenceDataRepository">FinancialAidReferenceDataRepository</param>
        /// <param name="logger">Logger</param>
        public AwardTypesController(IAdapterRegistry adapterRegistry, IFinancialAidReferenceDataRepository financialAidReferenceDataRepository, ILogger logger)
        {
            AdapterRegistry = adapterRegistry;
            FinancialAidReferenceDataRepository = financialAidReferenceDataRepository;
            Logger = logger;
        }

        /// <summary>
        /// Get a list of all Financial Aid Award Types
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A collection of AwardType data objects</returns>
        public IEnumerable<AwardType> GetAwardTypes()
        {
            try
            {
                var AwardTypesCollection = FinancialAidReferenceDataRepository.AwardTypes;

                //Get the adapter for the type mapping
                var awardTypesDtoAdapter = AdapterRegistry.GetAdapter<Domain.FinancialAid.Entities.AwardType, AwardType>();

                //Map the awardyear entity to the awardyear dto
                var awardTypesDtoCollection = new List<AwardType>();
                foreach (var awardType in AwardTypesCollection)
                {
                    awardTypesDtoCollection.Add(awardTypesDtoAdapter.MapToType(awardType));
                }

                return awardTypesDtoCollection;
            }
            catch (KeyNotFoundException knfe)
            {
                Logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("AwardTypes", "");
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting AwardTypes resource. See log for details");
            }
        }
    }
}