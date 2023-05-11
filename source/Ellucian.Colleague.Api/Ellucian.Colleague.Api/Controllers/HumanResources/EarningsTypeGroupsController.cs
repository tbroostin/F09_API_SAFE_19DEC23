/* Copyright 2021-2022 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Controller exposes actions to interact with EarningsTypeGroups
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EarningsTypeGroupsController : BaseCompressedApiController
    {
        private ILogger logger;
        private IAdapterRegistry adapterRegistry;
        private IHumanResourcesReferenceDataRepository referenceDataRepository;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";

        /// <summary>
        /// Constructor
        /// </summary>
        public EarningsTypeGroupsController(ILogger logger, IAdapterRegistry adapterRegistry, IHumanResourcesReferenceDataRepository referenceDataRepository)
        {
            this.logger = logger;
            this.adapterRegistry = adapterRegistry;
            this.referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Get all EarningsTypeGroups. This endpoint is used in SelfService
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can view EarningsTypeGroups
        /// </accessComments>
        /// <returns>A list of all EarningsTypeGroups</returns>
        public async Task<IEnumerable<EarningsTypeGroup>> GetEarningsTypeGroupsAsync()
        {
            logger.Debug("********* Start - Process to get Earnings Type Groups- Start*********");
            try
            {
                var earningsTypeGroupDictionary = await referenceDataRepository.GetEarningsTypesGroupsAsync();
                if (earningsTypeGroupDictionary == null || !earningsTypeGroupDictionary.Any())
                {
                    return new List<EarningsTypeGroup>();
                }

                var adapter = adapterRegistry.GetAdapter<Domain.HumanResources.Entities.EarningsTypeGroup, EarningsTypeGroup>();
                var dtos = earningsTypeGroupDictionary.Values.Select(etg => adapter.MapToType(etg));
                logger.Debug("********* End - Process to get Earnings Type Groups- End*********");
                return dtos;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

    }
}