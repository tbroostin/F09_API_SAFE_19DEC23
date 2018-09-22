//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
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
    public class FiscalPeriodsService : BaseCoordinationService, IFiscalPeriodsService
    {
        private readonly IColleagueFinanceReferenceDataRepository _referenceDataRepository;

        public FiscalPeriodsService(

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
        /// Gets all fiscal-periods
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <param name="fiscalYear">fiscalYear filter</param>
        /// <returns>Collection of FiscalPeriods DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.FiscalPeriods>> GetFiscalPeriodsAsync(bool bypassCache = false, string fiscalYear = "")
        {
            var fiscalPeriodsCollection = new List<Ellucian.Colleague.Dtos.FiscalPeriods>();
        
            int? fiscalYearCode = null;

            if (!(string.IsNullOrEmpty(fiscalYear)))
            {
                var fiscalYears = await _referenceDataRepository.GetFiscalYearsAsync(bypassCache);
                if (fiscalYears == null)
                {
                    throw new ArgumentException("Unable to retrieve fiscal years.");
                }
                var fiscalYearLookup = fiscalYears.FirstOrDefault(x => x.Guid == fiscalYear);
                if (fiscalYearLookup == null)
                {
                    return fiscalPeriodsCollection;
                }
                fiscalYearCode = Convert.ToInt16(fiscalYearLookup.Id);
            }   
           
            var fiscalPeriodsEntities = await _referenceDataRepository.GetFiscalPeriodsIntgAsync(bypassCache);

            if ((fiscalYearCode != null && fiscalYearCode.HasValue))
            {
                fiscalPeriodsEntities = fiscalPeriodsEntities.Where(x => (x.FiscalYear != null)
                    && (x.FiscalYear.HasValue) && (x.FiscalYear == fiscalYearCode));
            }

            if (fiscalPeriodsEntities != null && fiscalPeriodsEntities.Any())
            {
                
                foreach (var fiscalPeriods in fiscalPeriodsEntities)
                {
                    fiscalPeriodsCollection.Add(await ConvertFiscalPeriodsEntityToDtoAsync(fiscalPeriods, bypassCache));
                }
            }
            return fiscalPeriodsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a FiscalPeriods from its GUID
        /// </summary>
        /// <returns>FiscalPeriods DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.FiscalPeriods> GetFiscalPeriodsByGuidAsync(string guid, bool bypassCache = true)
        {
                 
            try
            {
                return await ConvertFiscalPeriodsEntityToDtoAsync((await _referenceDataRepository.GetFiscalPeriodsIntgAsync(bypassCache)).Where(r => r.Guid == guid).First(),  bypassCache);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("fiscal-periods not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("fiscal-periods not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a FiscalPeriodsIntg domain entity to its corresponding FiscalPeriods DTO
        /// </summary>
        /// <param name="source">FiscalPeriodsIntg domain entity</param>
        /// <returns>FiscalPeriods DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.FiscalPeriods> ConvertFiscalPeriodsEntityToDtoAsync(FiscalPeriodsIntg source,   bool bypassCache = false)
        {
            var fiscalPeriods = new Ellucian.Colleague.Dtos.FiscalPeriods();

            fiscalPeriods.Id = source.Guid;
            fiscalPeriods.Title = source.Title;

            if (source.FiscalYear.HasValue)
            {
                
                var fiscalYears = await _referenceDataRepository.GetFiscalYearsAsync(bypassCache);
                if (fiscalYears == null)
                {
                    throw new ArgumentException("Unable to retrieve fiscal years.");
                }
                var fiscalYear = fiscalYears.FirstOrDefault(x => x.Id == Convert.ToString(source.FiscalYear));
                if (fiscalYear == null)
                {
                    throw new ArgumentException(string.Concat("Unable to retrieve fiscal year for: ", Convert.ToString(source.FiscalYear)));
                }
                fiscalPeriods.FiscalYear = new GuidObject2(fiscalYear.Guid);

                if ((source.Month.HasValue) && (source.FiscalYear.HasValue)
                    && (fiscalYear.FiscalStartMonth.HasValue))
                {
                    var month = Convert.ToInt16(source.Month);
                    var year = Convert.ToInt16(source.Year);
                    var daysInMonth = DateTime.DaysInMonth(year, month);

                    fiscalPeriods.StartOn = new DateTime(year, month, 1);
                    fiscalPeriods.EndOn = new DateTime(year, month, daysInMonth);

                    fiscalPeriods.Status = FiscalPeriodsStatus.Closed;

                    if (source.Status == "O")
                        fiscalPeriods.Status = FiscalPeriodsStatus.Open;       
                }             
            }                                     
            return fiscalPeriods;
        }
    }
}