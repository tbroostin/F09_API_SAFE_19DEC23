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
    public class FinancialAidAwardPeriodService : BaseCoordinationService, IFinancialAidAwardPeriodService
    {
        private IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository configurationRepository;

        public FinancialAidAwardPeriodService(IAdapterRegistry adapterRegistry,
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
        /// Gets all financial aid award periods
        /// </summary>
        /// <returns>Collection of FinancialAidAwardPeriod DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidAwardPeriod>> GetFinancialAidAwardPeriodsAsync(bool bypassCache = false)
        {
            var financialAidAwardPeriodCollection = new List<Ellucian.Colleague.Dtos.FinancialAidAwardPeriod>();

            var financialAidAwardPeriodEntities = await _referenceDataRepository.GetFinancialAidAwardPeriodsAsync(bypassCache);
            if (financialAidAwardPeriodEntities != null && financialAidAwardPeriodEntities.Count() > 0)
            {
                foreach (var financialAidAwardPeriod in financialAidAwardPeriodEntities)
                {
                    financialAidAwardPeriodCollection.Add(ConvertFinancialAidAwardPeriodEntityToDto(financialAidAwardPeriod));
                }
            }
            return financialAidAwardPeriodCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get an financial aid award period from its GUID
        /// </summary>
        /// <returns>FinancialAidAwardPeriod DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.FinancialAidAwardPeriod> GetFinancialAidAwardPeriodByGuidAsync(string guid)
        {
            try
            {
                return ConvertFinancialAidAwardPeriodEntityToDto((await _referenceDataRepository.GetFinancialAidAwardPeriodsAsync(true)).Where(fa => fa.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Financial aid award period not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts an Financial Aid Award Period domain entity to its corresponding FinancialAidAwardPeriod DTO
        /// </summary>
        /// <param name="source">FinancialAidAwardPeriod domain entity</param>
        /// <returns>FinancialAidAwardPeriod DTO</returns>
        private Dtos.FinancialAidAwardPeriod ConvertFinancialAidAwardPeriodEntityToDto(Domain.Student.Entities.FinancialAidAwardPeriod source)
        {
            var financialAidAwardPeriod = new Dtos.FinancialAidAwardPeriod();

            financialAidAwardPeriod.Id = source.Guid;
            financialAidAwardPeriod.Code = source.Code;
            financialAidAwardPeriod.Title = source.Description;
            financialAidAwardPeriod.Description = null;
            financialAidAwardPeriod.Start = source.StartDate;
            financialAidAwardPeriod.End = source.EndDate;
            financialAidAwardPeriod.Status = source.status;

            return financialAidAwardPeriod;
        }
    }
}
