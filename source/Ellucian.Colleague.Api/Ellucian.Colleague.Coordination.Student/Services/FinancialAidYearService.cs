//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.EnumProperties;
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
    public class FinancialAidYearService : BaseCoordinationService, IFinancialAidYearService
    {
        private IStudentReferenceDataRepository financialAidReferenceDataRepository;
        private readonly IConfigurationRepository configurationRepository;

        public FinancialAidYearService(IAdapterRegistry adapterRegistry,
            IStudentReferenceDataRepository financialAidReferenceDataRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            this.financialAidReferenceDataRepository = financialAidReferenceDataRepository;
            this.configurationRepository = configurationRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all financial aid years
        /// </summary>
        /// <returns>Collection of FinancialAidYear DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidYear>> GetFinancialAidYearsAsync(bool bypassCache = false)
        {
            var financialAidYearCollection = new List<Ellucian.Colleague.Dtos.FinancialAidYear>();

            var financialAidYearEntities = await financialAidReferenceDataRepository.GetFinancialAidYearsAsync(bypassCache);
            if (financialAidYearEntities != null && financialAidYearEntities.Count() > 0)
            {
                foreach (var financialAidYear in financialAidYearEntities)
                {
                    financialAidYearCollection.Add(ConvertFinancialAidYearEntityToDto(financialAidYear));
                }
            }
            return financialAidYearCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get an financial aid Year from its GUID
        /// </summary>
        /// <returns>FinancialAidYear DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.FinancialAidYear> GetFinancialAidYearByGuidAsync(string guid)
        {
            try
            {
                var finYears = await financialAidReferenceDataRepository.GetFinancialAidYearsAsync(true);
                return ConvertFinancialAidYearEntityToDto(finYears.Where(fa => fa.Guid.ToString().Equals( guid, StringComparison.OrdinalIgnoreCase)).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Financial aid year not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts an Financial Aid Year domain entity to its corresponding FinancialAidYear DTO
        /// </summary>
        /// <param name="source">FinancialAidYear domain entity</param>
        /// <returns>FinancialAidYear DTO</returns>
        private Ellucian.Colleague.Dtos.FinancialAidYear ConvertFinancialAidYearEntityToDto(Domain.Student.Entities.FinancialAidYear source)
        {
            var financialAidYear = new Ellucian.Colleague.Dtos.FinancialAidYear();

            financialAidYear.Id = source.Guid;
            financialAidYear.Code = source.Code;
            financialAidYear.Title = source.Description;
            if (!string.IsNullOrEmpty(source.Code))
            {
                try
                {
                    var hostCountry = source.HostCountry;
                    switch (hostCountry.ToString())
                    {
                        case "USA":
                            financialAidYear.Start = new DateTime(Int32.Parse(source.Code), 07, 01);
                            financialAidYear.End = new DateTime(Int32.Parse(source.Code) + 1, 06, 30);
                            break;
                        default:
                            financialAidYear.Start = null;
                            financialAidYear.End = null;
                            break;
                    }
                }
                catch (Exception e)
                {
                    throw new ArgumentException("Code not defined for financial aid year for guid " + source.Guid + " with title " + source.Description);
                }
            }
            financialAidYear.Description = null;
            switch (source.status)
            {
                case "D":
                    financialAidYear.Status = FinancialAidYearStatus.Inactive;
                    break;
                default:
                    financialAidYear.Status = FinancialAidYearStatus.Active;
                    break;
            }

            return financialAidYear;
        }
    }
}