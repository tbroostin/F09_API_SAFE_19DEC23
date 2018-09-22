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
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Exposes access to Financial Aid Award Categories data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AwardCategoriesController : BaseCompressedApiController
    {
        private readonly IFinancialAidReferenceDataRepository FinancialAidReferenceDataRepository;
        private readonly IAdapterRegistry AdapterRegistry;
        private readonly ILogger Logger;

        /// <summary>
        /// AwardCategoriesController constructor
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="financialAidReferenceDataRepository">FinancialAidReferenceDataRepository</param>
        /// <param name="logger">Logger</param>
        public AwardCategoriesController(IAdapterRegistry adapterRegistry, IFinancialAidReferenceDataRepository financialAidReferenceDataRepository, ILogger logger)
        {
            AdapterRegistry = adapterRegistry;
            FinancialAidReferenceDataRepository = financialAidReferenceDataRepository;
            Logger = logger;
        }

        /// <summary>
        /// Get a list of all Financial Aid Award Categories
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A collection of AwardCategories</returns>
        [Obsolete("Obsolete as of Api version 1.8, use version 2 of this API")]
        public IEnumerable<AwardCategory> GetAwardCategories()
        {
            try
            {
                var AwardCategoryCollection = FinancialAidReferenceDataRepository.AwardCategories;

                //Get the adapter for the type mapping
                var awardCategoryDtoAdapter = AdapterRegistry.GetAdapter<Domain.FinancialAid.Entities.AwardCategory, AwardCategory>();

                //Map the awardyear entity to the awardyear dto
                var awardCategoryDtoCollection = new List<AwardCategory>();
                foreach (var category in AwardCategoryCollection)
                {
                    awardCategoryDtoCollection.Add(awardCategoryDtoAdapter.MapToType(category));
                }

                return awardCategoryDtoCollection;
            }
            catch (KeyNotFoundException knfe)
            {
                Logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("AwardCategories", "");
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting AwardCategories resource. See log for details");
            }
        }

        /// <summary>
        /// Get a list of all Financial Aid Award Category2 DTOs
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A collection of AwardCategory2 DTOs</returns>
        public async Task<IEnumerable<AwardCategory2>> GetAwardCategories2Async()
        {
            try
            {
                //var AwardCategoryCollection = FinancialAidReferenceDataRepository.AwardCategories;
                var AwardCategoryCollection = await FinancialAidReferenceDataRepository.GetAwardCategoriesAsync();

                //Get the adapter for the type mapping
                var awardCategoryDtoAdapter = AdapterRegistry.GetAdapter<Domain.FinancialAid.Entities.AwardCategory, AwardCategory2>();

                //Map the awardyear entity to the awardyear2 dto
                var awardCategoryDtoCollection = new List<AwardCategory2>();
                foreach (var category in AwardCategoryCollection)
                {
                    awardCategoryDtoCollection.Add(awardCategoryDtoAdapter.MapToType(category));
                }

                return awardCategoryDtoCollection;
            }
            catch (KeyNotFoundException knfe)
            {
                Logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("AwardCategories", "");
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting AwardCategories resource. See log for details");
            }
        }
    }
}