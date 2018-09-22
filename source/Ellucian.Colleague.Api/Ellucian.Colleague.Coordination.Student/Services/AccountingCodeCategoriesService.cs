//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
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

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class AccountingCodeCategoriesService : BaseCoordinationService, IAccountingCodeCategoriesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;

        public AccountingCodeCategoriesService(

            IStudentReferenceDataRepository referenceDataRepository,
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
        /// Gets all accounting-code-categories
        /// </summary>
        /// <returns>Collection of AccountingCodeCategories DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AccountingCodeCategory>> GetAccountingCodeCategoriesAsync(bool bypassCache = false)
        {
            var accountingCodeCategoriesCollection = new List<Ellucian.Colleague.Dtos.AccountingCodeCategory>();

            var accountingCodeCategoriesEntities = await _referenceDataRepository.GetArCategoriesAsync(bypassCache);
            if (accountingCodeCategoriesEntities != null && accountingCodeCategoriesEntities.Any())
            {
                foreach (var accountingCodeCategories in accountingCodeCategoriesEntities)
                {
                    accountingCodeCategoriesCollection.Add(ConvertAccountingCodeCategoriesEntityToDto(accountingCodeCategories));
                }
            }
            return accountingCodeCategoriesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a AccountingCodeCategories from its GUID
        /// </summary>
        /// <returns>AccountingCodeCategories DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AccountingCodeCategory> GetAccountingCodeCategoryByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertAccountingCodeCategoriesEntityToDto((await _referenceDataRepository.GetArCategoriesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("accounting-code-categories not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("accounting-code-categories not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a ArCategories domain entity to its corresponding AccountingCodeCategories DTO
        /// </summary>
        /// <param name="source">ArCategories domain entity</param>
        /// <returns>AccountingCodeCategories DTO</returns>
        private Ellucian.Colleague.Dtos.AccountingCodeCategory ConvertAccountingCodeCategoriesEntityToDto(ArCategory source)
        {
            var accountingCodeCategories = new Ellucian.Colleague.Dtos.AccountingCodeCategory();

            accountingCodeCategories.Id = source.Guid;
            accountingCodeCategories.Code = source.Code;
            accountingCodeCategories.Title = source.Description;
            accountingCodeCategories.Description = null;

            return accountingCodeCategories;
        }


    }
}