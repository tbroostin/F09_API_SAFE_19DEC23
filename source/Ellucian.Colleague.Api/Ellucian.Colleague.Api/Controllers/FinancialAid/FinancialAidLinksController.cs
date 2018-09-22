//Copyright 2014 Ellucian Company L.P. and its affiliates.
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
    /// Links Controller is used to get links for the Financial Aid Homepage
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FinancialAidLinksController : BaseCompressedApiController
    {
        private readonly IFinancialAidReferenceDataRepository FinancialAidReferenceDataRepository;
        private readonly IAdapterRegistry AdapterRegistry;
        private readonly ILogger Logger;

        /// <summary>
        /// Links Controller constructor
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="financialAidReferenceDataRepository">FinancialAidReferenceDataRepository</param>
        /// <param name="logger">Logger</param>
        public FinancialAidLinksController(IAdapterRegistry adapterRegistry, IFinancialAidReferenceDataRepository financialAidReferenceDataRepository, ILogger logger)
        {
            AdapterRegistry = adapterRegistry;
            FinancialAidReferenceDataRepository = financialAidReferenceDataRepository;
            Logger = logger;
        }

        /// <summary>
        /// Get a list of all Financial Aid Links from Colleague
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A collection of Links</returns>
        public IEnumerable<Link> GetLinks()
        {
            try
            {
                var LinksCollection = FinancialAidReferenceDataRepository.Links;

                //Get the adapter for the type mapping
                var linkDtoAdapter = AdapterRegistry.GetAdapter<Domain.FinancialAid.Entities.Link, Link>();

                //Map the Link entity to the Link dto
                var LinkDtoCollection = new List<Link>();

                foreach (var link in LinksCollection)
                {
                    LinkDtoCollection.Add(linkDtoAdapter.MapToType(link));
                }

                return LinkDtoCollection;
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting Links resource");
            }
        }
    }
}