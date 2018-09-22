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
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class MealPlansService : StudentCoordinationService, IMealPlansService
    {

        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public MealPlansService(

            IStudentReferenceDataRepository studentReferenceDataRepository,
            IReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IStudentRepository studentRepository,
            IStaffRepository staffRepository,
            IRoomRepository roomRepository,
            ILogger logger,
            IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository, staffRepository)
        {
            _configurationRepository = configurationRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _referenceDataRepository = referenceDataRepository;
            _roomRepository = roomRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all meal-plans
        /// </summary>
        /// <returns>Collection of MealPlans DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.MealPlans>> GetMealPlansAsync(bool bypassCache = false)
        {
            var mealPlansCollection = new List<Ellucian.Colleague.Dtos.MealPlans>();

            var mealPlansEntities = await _studentReferenceDataRepository.GetMealPlansAsync(bypassCache);
            if (mealPlansEntities != null && mealPlansEntities.Any())
            {
                foreach (var mealPlans in mealPlansEntities)
                {
                    var test = mealPlansCollection.Where(mp => mp.Id.Equals(mealPlans.Guid));
                    if (!test.Any())
                    {
                        mealPlansCollection.Add(await ConvertMealPlansEntityToDto(mealPlans, bypassCache));
                    }
                }
            }
            return mealPlansCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a MealPlans from its GUID
        /// </summary>
        /// <returns>MealPlans DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.MealPlans> GetMealPlansByGuidAsync(string guid)
        {
            try
            {
                return await ConvertMealPlansEntityToDto((await _studentReferenceDataRepository.GetMealPlansAsync(true)).Where(r => r.Guid == guid).First(), true);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("meal-plans not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("meal-plans not found for GUID " + guid, ex);
            }
        }

        private IEnumerable<Domain.Student.Entities.StudentResidentialCategories> _srcs = null;
        private async Task<IEnumerable<Domain.Student.Entities.StudentResidentialCategories>> GetAllStudentResidentialCategoriesAsync(bool bypassCache)
        {
            if (_srcs == null)
            {
                _srcs = await _studentReferenceDataRepository.GetStudentResidentialCategoriesAsync(bypassCache);
            }
            return _srcs;
        }

        private IEnumerable<Domain.Student.Entities.MealType> _mealTypes = null;
        private async Task<IEnumerable<Domain.Student.Entities.MealType>> GetAllMealTypesAsync(bool bypassCache)
        {
            if (_mealTypes == null)
            {
                _mealTypes = await _studentReferenceDataRepository.GetMealTypesAsync(bypassCache);
            }
            return _mealTypes;
        }

        private IEnumerable<Domain.Base.Entities.Room> _rooms = null;
        private async Task<IEnumerable<Domain.Base.Entities.Room>> GetAllRoomsAsync(bool bypassCache)
        {
            if (_rooms == null)
            {
                _rooms = await _roomRepository.GetRoomsAsync(bypassCache);
            }
            return _rooms;
        }

        private IEnumerable<Domain.Base.Entities.Location> _sites = null;
        private async Task<IEnumerable<Domain.Base.Entities.Location>> GetAllLocationsAsync(bool bypassCache)
        {
            if (_sites == null)
            {
                _sites = await _referenceDataRepository.GetLocationsAsync(bypassCache);
            }
            return _sites;
        }

        private IEnumerable<Domain.Base.Entities.Building> _buildings = null;
        private async Task<IEnumerable<Domain.Base.Entities.Building>> GetAllBuildingsAsync(bool bypassCache)
        {
            if (_buildings == null)
            {
                _buildings = await _referenceDataRepository.GetBuildingsAsync(bypassCache);
            }
            return _buildings;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a MealPlans domain entity to its corresponding MealPlans DTO
        /// </summary>
        /// <param name="source">MealPlans domain entity</param>
        /// <returns>MealPlans DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.MealPlans> ConvertMealPlansEntityToDto(MealPlan source, bool bypassCache)
        {
            var mealPlans = new Ellucian.Colleague.Dtos.MealPlans();

            mealPlans.Id = source.Guid;
            mealPlans.Code = source.Code;
            mealPlans.Title = source.Description;
            mealPlans.Description = null;
            if (!string.IsNullOrEmpty(source.Classification))
            {
                mealPlans.StudentResidentialCategories = await ConvertClassificationtoStudentResidentialCategories(source.Classification, bypassCache);
            }
            var components = await ConvertMealPlansComponentEntitytoDto(source, bypassCache);
            if (components.Any() && components.Count() > 0)
            {
                mealPlans.Components = components;
            }
            mealPlans.StartOn = source.StartDate;
            mealPlans.EndOn = source.EndDate;
            return mealPlans;
        }

        private async Task<List<GuidObject2>> ConvertClassificationtoStudentResidentialCategories(String classification, bool bypassCache)
        {
            //var categoriesEntities = await _studentReferenceDataRepository.GetStudentResidentialCategoriesAsync(bypassCache);
            var categoryEntities = await GetAllStudentResidentialCategoriesAsync(bypassCache);

            if (categoryEntities != null)
            {
                var categories = categoryEntities.Where(src => src.Code == classification).Select(ay => new GuidObject2(ay.Guid)).Distinct().ToList();

                return categories;
            }

            return null;
        }

        private async Task<List<MealPlansComponents>> ConvertMealPlansComponentEntitytoDto(MealPlan source, bool bypassCache)
        {
            List<MealPlansComponents> mealPlanComponents = new List<MealPlansComponents>();
            MealPlansComponents mpc = new MealPlansComponents();

            List<MealPlansRestrictions> mealPlansRestrictions = new List<MealPlansRestrictions>();
            MealPlansRestrictions mpr = new MealPlansRestrictions();

            if (!string.IsNullOrEmpty(source.ComponentTimePeriod))
            {
                mpc.NumberOfUnits = source.ComponentNumberOfUnits.Equals(null) ? 0 : source.ComponentNumberOfUnits;
                mpc.UnitType = MealPlansUnitType.Meal;
                mpc.TimePeriod = ConvertTimePeriodEntityToDto(source.ComponentTimePeriod);
                
                ////

                //var mealTypeEntities = await _studentReferenceDataRepository.GetMealTypesAsync(bypassCache);
                
                //var mealTypeEntities = GetAllMealTypesAsync(bypassCache).Result;
                var mealTypesCollection = new List<GuidObject2>();
                if (source.MealTypes != null && source.MealTypes.Any())
                {
                    foreach (var mealType in source.MealTypes)
                    {
                        var mealTypeEntities = await GetAllMealTypesAsync(bypassCache);

                        if (mealTypeEntities != null)
                        {
                            mealTypesCollection.Add(new GuidObject2(mealTypeEntities.Where(m => m.Code == mealType).Select(m => m.Guid).Distinct().FirstOrDefault()));
                    
                        }
                        //mealTypesCollection.Add(new GuidObject2(mealTypeEntities.Where(m => m.Code == mealType).Select(m => m.Guid).Distinct().FirstOrDefault()));
                    }
                    mpr.MealTypes = mealTypesCollection;
                }

                //var diningFacilityEntities = GetAllRoomsAsync(bypassCache).Result;
                var diningFacilitiesCollection = new List<GuidObject2>();
                if (source.DiningFacilities != null && source.DiningFacilities.Any())
                {
                    foreach (var diningFacility in source.DiningFacilities)
                    {
                        var diningFacilityEntities = await GetAllRoomsAsync(bypassCache);

                        if (diningFacilityEntities != null)
                        {
                            diningFacilitiesCollection.Add(new GuidObject2(diningFacilityEntities.Where(m => m.Code == diningFacility).Select(m => m.Guid).Distinct().FirstOrDefault()));

                        }
                        //diningFacilitiesCollection.Add(new GuidObject2(diningFacilityEntities.Where(df => df.Code == diningFacility).Select(df => df.Guid).Distinct().FirstOrDefault()));
                    }
                    mpr.DiningFacilities = diningFacilitiesCollection;
                }

                //var buildingEntities = GetAllBuildingsAsync(bypassCache).Result;
                var buildingsCollection = new List<GuidObject2>();
                if (source.Buildings != null && source.Buildings.Any())
                {
                    foreach (var building in source.Buildings)
                    {
                        var buildingEntities = await GetAllBuildingsAsync(bypassCache);

                        if (buildingEntities != null)
                        {
                            buildingsCollection.Add(new GuidObject2(buildingEntities.Where(m => m.Code == building).Select(m => m.Guid).Distinct().FirstOrDefault()));

                        }
                        //buildingsCollection.Add(new GuidObject2(buildingEntities.Where(b => b.Code == building).Select(df => df.Guid).Distinct().FirstOrDefault()));
                    }
                    mpr.Buildings = buildingsCollection;
                }

                //var siteEntities = GetAllLocationsAsync(bypassCache).Result;
                var sitesCollection = new List<GuidObject2>();
                //var sites = siteEntities.Where(m => m.Code == source).Select(m => m.Guid).Distinct().ToList();
                if (source.Sites != null && source.Sites.Any())
                {
                    foreach (var site in source.Sites)
                    {
                        var siteEntities = await GetAllLocationsAsync(bypassCache);

                        if (siteEntities != null)
                        {
                            sitesCollection.Add(new GuidObject2(siteEntities.Where(m => m.Code == site).Select(m => m.Guid).Distinct().FirstOrDefault()));

                        }
                        //sitesCollection.Add(new GuidObject2(siteEntities.Where(s => s.Code == site).Select(m => m.Guid).Distinct().FirstOrDefault()));
                    }
                    mpr.Sites = sitesCollection;
                }

                var daysOfWeek = new List<DaysOfWeek>();
                List<string> days = new List<string>() { "SU", "M", "T", "W", "TH", "F", "S" };

                if (!string.IsNullOrEmpty(source.StartDay) && !string.IsNullOrEmpty(source.EndDay))
                {
                    int start = days.IndexOf(source.StartDay);
                    int end = days.IndexOf(source.EndDay);

                    for (int i = start; i <= end; i++)
                    {
                        var day = ConvertDaysOfWeekEntityToDto(days[i]);
                        daysOfWeek.Add(day);
                    }

                    mpr.DaysOfWeek = daysOfWeek;

                }
            }

            if (mpr.MealTypes != null || mpr.DiningFacilities != null || mpr.Buildings != null || mpr.Sites != null)
            {
                mealPlansRestrictions.Add(mpr);
            }
            if (mealPlansRestrictions.Any())
            {
                mpc.Restrictions = mealPlansRestrictions;
                mealPlanComponents.Add(mpc);
            }
            else if (mpc.UnitType != null)
            {
                mealPlanComponents.Add(mpc);
            }

            return mealPlanComponents;
        }

        private MealPlansTimePeriod ConvertTimePeriodEntityToDto(string source)
        {
            switch (source)
            {
                case "D":
                    return MealPlansTimePeriod.Day;
                case "W":
                    return MealPlansTimePeriod.Week;
                case "Y":
                    return MealPlansTimePeriod.Year;
                case "T":
                    return MealPlansTimePeriod.Term;
                default:
                    return MealPlansTimePeriod.Other;
            }
        }

        private DaysOfWeek ConvertDaysOfWeekEntityToDto(string source)
        {
            switch (source)
            {
                case "SU":
                    return DaysOfWeek.Sunday;
                case "M":
                    return DaysOfWeek.Monday;
                case "T":
                    return DaysOfWeek.Tuesday;
                case "W":
                    return DaysOfWeek.Wednesday;
                case "TH":
                    return DaysOfWeek.Thursday;
                case "F":
                    return DaysOfWeek.Friday;
                case "S":
                    return DaysOfWeek.Saturday;
                default:
                    return DaysOfWeek.Other;
            }
        }

    }
}
