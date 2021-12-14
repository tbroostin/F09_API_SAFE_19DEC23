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
using Ellucian.Colleague.Dtos.Student;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Exposes access to Financial Aid Awards data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AwardsController : BaseCompressedApiController
    {
        private readonly IFinancialAidReferenceDataRepository _FinancialAidReferenceDataRepository;
        private readonly IAdapterRegistry _AdapterRegistry;
        private readonly ILogger _Logger;

        /// <summary>
        /// AwardsController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="financialAidReferenceDataRepository">Financial Aid Reference Data Repository of type <see cref="IFinancialAidReferenceDataRepository">IFinancialAidReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public AwardsController(IAdapterRegistry adapterRegistry, IFinancialAidReferenceDataRepository financialAidReferenceDataRepository, ILogger logger)
        {
            _AdapterRegistry = adapterRegistry;
            _FinancialAidReferenceDataRepository = financialAidReferenceDataRepository;
            _Logger = logger;
        }

        /// <summary>
        /// Get a list of all Financial Aid Awards from Colleague
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A collection of Award data objects</returns>
        [Obsolete("Obsolete as of Api version 1.8, use version 2 of this API")]
        public IEnumerable<Award> GetAwards()
        {
            try
            {
                var AwardCollection = _FinancialAidReferenceDataRepository.Awards;

                //Get the adapter for the type mapping
                var awardDtoAdapter = _AdapterRegistry.GetAdapter<Domain.FinancialAid.Entities.Award, Award>();

                //Map the award entity to the award DTO
                var awardDtoCollection = new List<Award>();
                foreach (var award in AwardCollection)
                {
                    awardDtoCollection.Add(awardDtoAdapter.MapToType(award));
                }

                return awardDtoCollection;
            }
            catch (KeyNotFoundException knfe)
            {
                _Logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("Awards", "");
            }
            catch (Exception e)
            {
                _Logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting Awards resource. See log for details");
            }
        }

        /// <summary>
        /// Get a list of all Financial Aid Award2 DTOs from Colleague
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A collection of Award data objects</returns>   
        public IEnumerable<Award2> GetAwards2()
        {
            try
            {
                var AwardCollection = _FinancialAidReferenceDataRepository.Awards;

                //Get the adapter for the type mapping
                var awardDtoAdapter = _AdapterRegistry.GetAdapter<Domain.FinancialAid.Entities.Award, Award2>();

                //Map the award entity to the award2 DTO
                var awardDtoCollection = new List<Award2>();
                foreach (var award in AwardCollection)
                {
                    awardDtoCollection.Add(awardDtoAdapter.MapToType(award));
                }

                return awardDtoCollection;
            }
            catch (KeyNotFoundException knfe)
            {
                _Logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("Awards", "");
            }
            catch (Exception e)
            {
                _Logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting Awards resource. See log for details");
            }
        }

        /// <summary>
        /// Get a list of all Financial Aid Award3 DTOs from Colleague
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A collection of Award data objects</returns>   
        public IEnumerable<Award3> GetAwards3()
        {
            try
            {
                var AwardCollection = _FinancialAidReferenceDataRepository.Awards;

                //Get the adapter for the type mapping
                var awardDtoAdapter = _AdapterRegistry.GetAdapter<Domain.FinancialAid.Entities.Award, Award3>();

                //Map the award entity to the award2 DTO
                var awardDtoCollection = new List<Award3>();
                foreach (var award in AwardCollection)
                {
                    awardDtoCollection.Add(awardDtoAdapter.MapToType(award));
                }

                return awardDtoCollection;
            }
            catch (KeyNotFoundException knfe)
            {
                _Logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("Awards", "");
            }
            catch (Exception e)
            {
                _Logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting Awards resource. See log for details");
            }
        }
    }
}
