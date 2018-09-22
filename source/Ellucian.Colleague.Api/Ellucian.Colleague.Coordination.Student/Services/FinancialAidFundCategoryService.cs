//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class FinancialAidFundCategoryService : BaseCoordinationService, IFinancialAidFundCategoryService
    {
        private IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository configurationRepository;

        public FinancialAidFundCategoryService(IAdapterRegistry adapterRegistry,
            IStudentReferenceDataRepository referenceDataRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            this._referenceDataRepository = referenceDataRepository;
            this.configurationRepository = configurationRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all financial aid fund categories
        /// </summary>
        /// <returns>Collection of FinancialAidFundCategory DTO objects</returns>
        public async Task<IEnumerable<Dtos.FinancialAidFundCategory>> GetFinancialAidFundCategoriesAsync(bool bypassCache = false)
        {
            var financialAidFundCategoryCollection = new List<Dtos.FinancialAidFundCategory>();

            var financialAidFundCategoryEntities = await _referenceDataRepository.GetFinancialAidFundCategoriesAsync(bypassCache);
            if (financialAidFundCategoryEntities != null && financialAidFundCategoryEntities.Any())
            {
                foreach (var financialAidFundCategory in financialAidFundCategoryEntities)
                {
                    financialAidFundCategoryCollection.Add(ConvertFinancialAidFundCategoryEntityToDto(financialAidFundCategory));
                }
            }
            return financialAidFundCategoryCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get an financial aid Fund Category from its GUID
        /// </summary>
        /// <returns>FinancialAidFundCategory DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.FinancialAidFundCategory> GetFinancialAidFundCategoryByGuidAsync(string guid)
        {
            try
            {
                return ConvertFinancialAidFundCategoryEntityToDto((await _referenceDataRepository.GetFinancialAidFundCategoriesAsync(true)).Where(fa => fa.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Financial aid fund category not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts an Financial Aid Fund Category domain entity to its corresponding FinancialAidFundCategory DTO
        /// </summary>
        /// <param name="source">FinancialAidFundCategory domain entity</param>
        /// <returns>FinancialAidFundCategory DTO</returns>
        private Dtos.FinancialAidFundCategory ConvertFinancialAidFundCategoryEntityToDto(Domain.Student.Entities.FinancialAidFundCategory source)
        {
            var financialAidFundCategory = new Dtos.FinancialAidFundCategory();

            financialAidFundCategory.Id = source.Guid;
            financialAidFundCategory.Code = source.Code;
            financialAidFundCategory.Title = source.Description;
            financialAidFundCategory.Description = null;

            return financialAidFundCategory;
        }
    }
}
