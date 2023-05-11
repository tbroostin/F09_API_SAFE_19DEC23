//Copyright 2017-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class MealPlanRatesService : BaseCoordinationService, IMealPlanRatesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;

        public MealPlanRatesService(

            IStudentReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger,
            IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, null, configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all meal-plan-rates
        /// </summary>
        /// <returns>Collection of MealPlanRates DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.MealPlanRates>> GetMealPlanRatesAsync(bool bypassCache = false)
        {
            var mealPlanRatesCollection = new List<Ellucian.Colleague.Dtos.MealPlanRates>();

            var mealPlanRatesEntities = await _referenceDataRepository.GetMealPlanRatesAsync(bypassCache);
            if (mealPlanRatesEntities != null && mealPlanRatesEntities.Any())
            {
                foreach (var mealPlanRates in mealPlanRatesEntities)
                {
                    mealPlanRatesCollection.Add(await ConvertMealPlanRatesEntityToDtoAsync(mealPlanRates, bypassCache));
                }
            }
            return mealPlanRatesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a MealPlanRates from its GUID
        /// </summary>
        /// <returns>MealPlanRates DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.MealPlanRates> GetMealPlanRatesByGuidAsync(string guid)
        {
            try
            {
                return await ConvertMealPlanRatesEntityToDtoAsync((await _referenceDataRepository.GetMealPlanRatesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("meal-plan-rates not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("meal-plan-rates not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a MealPlanRates domain entity to its corresponding MealPlanRates DTO
        /// </summary>
        /// <param name="source">MealPlanRates domain entity</param>
        /// <returns>MealPlanRates DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.MealPlanRates> ConvertMealPlanRatesEntityToDtoAsync(Ellucian.Colleague.Domain.Student.Entities.MealPlanRates source, bool bypassCache = false)
        {
            var mealPlanRates = new Ellucian.Colleague.Dtos.MealPlanRates();

            var mealPlansMealPlanRates = source.MealPlansMealPlanRates;
            if (mealPlansMealPlanRates == null)
            {
                throw new ArgumentNullException(string.Concat("Meal plan rate not found, Record title: ", source.Code, ", Record Guid: ", source.Guid));
            }

            if (mealPlansMealPlanRates.EffectiveDates == null && mealPlansMealPlanRates.EffectiveDates != DateTime.MinValue)
            {
                throw new ArgumentNullException(string.Concat("Meal plan rate effective date not found, Record title: ", source.Code, ", Record Guid: ", source.Guid));
            }

            mealPlanRates.Id = source.Guid;

            mealPlanRates.Title = string.Concat(source.Description, " ", Convert.ToDateTime(mealPlansMealPlanRates.EffectiveDates).ToShortDateString());

            if (!(string.IsNullOrEmpty(source.Code)))
            {
                var mealPlans = await _referenceDataRepository.GetMealPlansAsync(bypassCache);
                if (mealPlans != null)
                {
                    var mealPlan = mealPlans.FirstOrDefault(mp => mp.Code == source.Code);
                    if (mealPlan != null)
                    {
                        mealPlanRates.MealPlan = new Dtos.GuidObject2(mealPlan.Guid);
                    }
                    else
                    {
                        throw new ColleagueWebApiException(string.Concat("Unable to locate guid for MealPlan: ", source.Code, ", Record Guid: ", source.Guid));
                    }
                }
                else
                {
                    throw new ColleagueWebApiException(string.Concat("Unable to locate guid for MealPlan: ", source.Code, ", Record Guid: ", source.Guid));
                }
            }

            mealPlanRates.Rate = new Dtos.DtoProperties.Amount2DtoProperty()
            {
                Currency = (await _referenceDataRepository.GetHostCountryAsync()).ToUpper() == "USA" ? CurrencyIsoCode.USD : CurrencyIsoCode.CAD,
                Value = source.MealPlansMealPlanRates.MealRates
            };

            var ratePeriod = ConvertMealPlanRatesRatePeriodDomainEnumToDtoEnum(source.MealRatePeriod);
            if (ratePeriod == MealPlanRatesRatePeriod.NotSet)
            {
                throw new ColleagueWebApiException(string.Concat("Unable to determine rate period for MealPlan: ", source.Code, ", Record Guid: ", source.Guid));
            }
            mealPlanRates.RatePeriod = ratePeriod;

            mealPlanRates.StartOn = new DateTime(source.MealPlansMealPlanRates.EffectiveDates.Value.Date.Ticks, DateTimeKind.Utc);;

            if (!(string.IsNullOrEmpty(source.MealArCode)))
            {
                var accountingCodes = await _referenceDataRepository.GetAccountingCodesAsync(bypassCache);
                if (accountingCodes != null)
                {
                    var accountingCode = accountingCodes.FirstOrDefault(ac => ac.Code == source.MealArCode);
                    if (accountingCode != null)
                    {
                        mealPlanRates.AccountingCode = new Dtos.GuidObject2(accountingCode.Guid);
                    }
                }
                else {
                    throw new ColleagueWebApiException("Unable to get accounting codes.");
                }
            }
            return mealPlanRates;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a MealPlanRatePeriod domain enumeration value to its corresponding MealPlanRatePeriods DTO enumeration value
        /// </summary>
        /// <param name="source">MealPlanRatePeriods domain enumeration value</param>
        /// <returns>MealPlanRatesRatePeriod DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.EnumProperties.MealPlanRatesRatePeriod ConvertMealPlanRatesRatePeriodDomainEnumToDtoEnum(MealPlanRatePeriods source)
        {
            switch (source)
            {

                case MealPlanRatePeriods.Day:
                    return Dtos.EnumProperties.MealPlanRatesRatePeriod.Day;
                case MealPlanRatePeriods.Week:
                    return Dtos.EnumProperties.MealPlanRatesRatePeriod.Week;
                case MealPlanRatePeriods.Term:
                    return Dtos.EnumProperties.MealPlanRatesRatePeriod.Term;
                case MealPlanRatePeriods.Meal:
                    return Dtos.EnumProperties.MealPlanRatesRatePeriod.Meal;
                case MealPlanRatePeriods.Month:
                    return Dtos.EnumProperties.MealPlanRatesRatePeriod.Month;
                case MealPlanRatePeriods.Year:
                    return Dtos.EnumProperties.MealPlanRatesRatePeriod.Year;
                default:
                    return Dtos.EnumProperties.MealPlanRatesRatePeriod.NotSet;
            }
        }
    }
}