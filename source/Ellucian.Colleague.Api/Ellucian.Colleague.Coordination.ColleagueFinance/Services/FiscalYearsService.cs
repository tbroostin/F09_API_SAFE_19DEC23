//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.ColleagueFinance.Adapters;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
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

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    [RegisterType]
    public class FiscalYearsService : BaseCoordinationService, IFiscalYearsService
    {

        private readonly IColleagueFinanceReferenceDataRepository _referenceDataRepository;

        public FiscalYearsService(

            IColleagueFinanceReferenceDataRepository referenceDataRepository,
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
        /// Gets all fiscal-years
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <param name="reportingSegment">reportingSegment filter</param>
        /// <returns>Collection of <see cref="FiscalYears">fiscalYears</see> objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.FiscalYears>> GetFiscalYearsAsync(bool bypassCache = false, string reportingSegment = "")
        {
            var fiscalYearsCollection = new List<Ellucian.Colleague.Dtos.FiscalYears>();

            var fiscalYearsEntities = await _referenceDataRepository.GetFiscalYearsAsync(bypassCache);

            if (!(string.IsNullOrEmpty(reportingSegment)))
            {
                fiscalYearsEntities = fiscalYearsEntities.Where(x => (!string.IsNullOrEmpty(x.InstitutionName)) 
                    && x.InstitutionName.ToLower() == reportingSegment.ToLower());
            }

            if (fiscalYearsEntities != null && fiscalYearsEntities.Any())
            {
                foreach (var FiscalYears in fiscalYearsEntities)
                {
                    fiscalYearsCollection.Add(ConvertFiscalYearsEntityToDto(FiscalYears));
                }
            }
            return fiscalYearsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a FiscalYears from its GUID
        /// </summary>
        /// <returns>FiscalYears DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.FiscalYears> GetFiscalYearsByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertFiscalYearsEntityToDto((await _referenceDataRepository.GetFiscalYearsAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("fiscal-years not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("fiscal-years not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a FiscalYear domain entity to its corresponding FiscalYears DTO
        /// </summary>
        /// <param name="source">FiscalYear domain entity</param>
        /// <returns>FiscalYears DTO</returns>
        private Ellucian.Colleague.Dtos.FiscalYears ConvertFiscalYearsEntityToDto(FiscalYear source)
        {
            var fiscalYears = new Ellucian.Colleague.Dtos.FiscalYears();

            fiscalYears.Id = source.Guid;
            fiscalYears.Title = source.Title;
            fiscalYears.NumberOfPeriods = 12;

            if (source.FiscalStartMonth.HasValue)
            {
                if (source.FiscalStartMonth.Value == 1)
                {
                    fiscalYears.StartOn = new DateTime(Convert.ToInt32(source.Id), 1, 1);
                    fiscalYears.EndOn = new DateTime(Convert.ToInt32(source.Id), 12, 31);
                }
                else
                {
                    var month = Convert.ToInt16(source.FiscalStartMonth);
                    var year = Convert.ToInt16(source.Id);
                    var daysInMonth = DateTime.DaysInMonth(year, month -1);

                    fiscalYears.StartOn = new DateTime(year - 1, month, 1);
                    fiscalYears.EndOn = new DateTime(year, month - 1, daysInMonth);
                }
            }
            if (!(string.IsNullOrEmpty(source.Status)))
            {
                switch (source.Status)
                {
                    case "O":
                        fiscalYears.Status = FiscalPeriodsStatus.Open;
                        fiscalYears.YearEndAdjustment = FiscalYearsYearEndAdjustment.Inactive;
                        break;
                    case "C":
                        fiscalYears.Status = FiscalPeriodsStatus.Closed;
                        fiscalYears.YearEndAdjustment = FiscalYearsYearEndAdjustment.Inactive;
                        break;
                    case "Y":
                        fiscalYears.Status = FiscalPeriodsStatus.Closed;
                        fiscalYears.YearEndAdjustment = FiscalYearsYearEndAdjustment.Active;
                        break;
                    default:
                        break;
                }
                    }
            fiscalYears.ReportingSegment = source.InstitutionName;
            return fiscalYears;
        }
    }
}