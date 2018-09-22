//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.HumanResources.Adapters;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class DeductionCategoriesService : BaseCoordinationService, IDeductionCategoriesService
    {

        private readonly IHumanResourcesReferenceDataRepository _referenceDataRepository;

        public DeductionCategoriesService(

            IHumanResourcesReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all deduction-categories
        /// </summary>
        /// <returns>Collection of DeductionCategories DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.DeductionCategories>> GetDeductionCategoriesAsync(bool bypassCache = false)
        {
            var deductionCategoriesCollection = new List<Ellucian.Colleague.Dtos.DeductionCategories>();

            var deductionCategoriesEntities = await _referenceDataRepository.GetDeductionCategoriesAsync(bypassCache);
            if (deductionCategoriesEntities != null && deductionCategoriesEntities.Any())
            {
                foreach (var deductionCategories in deductionCategoriesEntities)
                {
                    deductionCategoriesCollection.Add(ConvertDeductionCategoriesEntityToDto(deductionCategories));
                }
            }
            return deductionCategoriesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a DeductionCategories from its GUID
        /// </summary>
        /// <returns>DeductionCategories DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.DeductionCategories> GetDeductionCategoriesByGuidAsync(string guid)
        {
            try
            {
                return ConvertDeductionCategoriesEntityToDto((await _referenceDataRepository.GetDeductionCategoriesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("deduction-categories not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("deduction-categories not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a DeductionCategory domain entity to its corresponding DeductionCategories DTO
        /// </summary>
        /// <param name="source">DeductionCategory domain entity</param>
        /// <returns>DeductionCategories DTO</returns>
        private Ellucian.Colleague.Dtos.DeductionCategories ConvertDeductionCategoriesEntityToDto(DeductionCategory source)
        {
            var deductionCategories = new Ellucian.Colleague.Dtos.DeductionCategories();

            deductionCategories.Id = source.Guid;
            deductionCategories.Code = source.Code;
            deductionCategories.Title = source.Description;
            deductionCategories.Description = null;           
                                                                        
            return deductionCategories;
        }

    }
   
}