//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student;
using System.Linq;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class StudentMealPlansService : BaseCoordinationService, IStudentMealPlansService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IMealPlanAssignmentRepository _studentMealPlanRepository;
        private readonly IPersonRepository _personRepository;
        private readonly ITermRepository _termRepository;
        private readonly IConfigurationRepository _configurationRepository;

        private IEnumerable<Domain.Student.Entities.BillingOverrideReasons> _billingOverrideReasons;
        private IEnumerable<Domain.Student.Entities.AcademicPeriod> _academicPeriods;
        private IEnumerable<Domain.Student.Entities.AccountingCode> _accountingCodes;
        private IEnumerable<Domain.Student.Entities.MealPlan> _mealPlans;
        private IEnumerable<Domain.Student.Entities.MealPlanRates> _mealPlanRates;
        private string _hostCountry;

        public StudentMealPlansService(

            IStudentReferenceDataRepository referenceDataRepository,
            IMealPlanAssignmentRepository studentMealPlanRepository,
            ITermRepository termRepository,
            IPersonRepository personRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _configurationRepository = configurationRepository;
            _referenceDataRepository = referenceDataRepository;
            _studentMealPlanRepository = studentMealPlanRepository;
            _termRepository = termRepository;
            _personRepository = personRepository;

        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all student-meal-plans
        /// </summary>
        /// <param name="offset">Paging offset</param>
        /// <param name="limit">Paging limit</param>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will re-query cached items</param>
        /// <returns>List of <see cref="Dtos.StudentMealPlans">Dtos.StudentMealPlans</see></returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentMealPlans>, int>> GetStudentMealPlansAsync(int offset, int limit, bool bypassCache = false)
        {
            CheckViewMealPlanAssignmentPermission();

            var studentMealPlansCollection = new List<Ellucian.Colleague.Dtos.StudentMealPlans>();

            var mealPlanAssignmentEntities = await _studentMealPlanRepository.GetAsync(offset, limit);
            var totalRecords = mealPlanAssignmentEntities.Item2;

            foreach (var mealPlanAssignmentEntity in mealPlanAssignmentEntities.Item1)
            {
                if (mealPlanAssignmentEntity.Guid != null)
                {
                    var studentMealPlansDto = await ConvertMealPlanAssignmentEntityToDtoAsync(mealPlanAssignmentEntity, bypassCache);
                    studentMealPlansCollection.Add(studentMealPlansDto);
                }
            }
            return new Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentMealPlans>, int>(studentMealPlansCollection, totalRecords);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a StudentMealPlans from its GUID
        /// </summary>
        /// <param name="guid">Guid</param>
        /// <returns><see cref="Dtos.StudentMealPlans">Dtos.StudentMealPlans object</see></returns>
        public async Task<Ellucian.Colleague.Dtos.StudentMealPlans> GetStudentMealPlansByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "A GUID is required to obtain an student-meal-plan.");
            }

            CheckViewMealPlanAssignmentPermission();

            try
            {
                return await ConvertMealPlanAssignmentEntityToDtoAsync(await _studentMealPlanRepository.GetByIdAsync(guid));
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("meal-plan-assignments not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("meal-plan-assignments not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Put (Update) an Student Meal Plan domain entity
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="StudentMealPlansDto"><see cref="Dtos.StudentMealPlans">StudentMealPlans</see></param>
        /// <returns><see cref="Dtos.StudentMealPlans">StudentMealPlans</see></returns>
        public async Task<StudentMealPlans> PutStudentMealPlansAsync(string guid, Dtos.StudentMealPlans StudentMealPlansDto)
        {
            try
            {
                if (StudentMealPlansDto == null)
                    throw new ArgumentNullException("MealPlanAssignments", "Must provide a meal plan assignment request body for update");
                if (string.IsNullOrEmpty(StudentMealPlansDto.Id))
                    throw new ArgumentNullException("MealPlanAssignments", "Must provide a guid for meal plan assignment update");

                // verify the user has the permission to update a StudentMealPlans
                CheckCreateMealPlanAssignmentPermission();

                _studentMealPlanRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                // map the DTO to entities
                var StudentMealPlansEntity
                    = await ConvertStudentMealPlansDtoToEntityAsync(StudentMealPlansDto);

                // update the entity in the database
                var updatedStudentMealPlansEntity =
                    await _studentMealPlanRepository.UpdateMealPlanAssignmentAsync(StudentMealPlansEntity);
                var dtoStudentMealPlans = await ConvertMealPlanAssignmentEntityToDtoAsync(updatedStudentMealPlansEntity, true);


                // return the newly updated DTO
                return dtoStudentMealPlans;

            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }


        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Post (Create) an Student meal plan doamin entity
        /// </summary>
        /// <param name="StudentMealPlansDto"><see cref="Dtos.StudentMealPlans">StudentMealPlans</see></param>
        /// <returns><see cref="Dtos.StudentMealPlans">StudentMealPlans</see></returns>
        public async Task<StudentMealPlans> PostStudentMealPlansAsync(Dtos.StudentMealPlans StudentMealPlansDto)
        {
            try
            {
                if (StudentMealPlansDto == null)
                    throw new ArgumentNullException("MealPlanAssignments", "Must provide a meal plan assignemnts request body to create a meal plan assignment. ");
                if (string.IsNullOrEmpty(StudentMealPlansDto.Id) || !string.Equals(StudentMealPlansDto.Id, Guid.Empty.ToString()))
                    throw new ArgumentNullException("MealPlanAssignments", "Must provide a nil guid to create a meal plan assignment. ");
                Ellucian.Colleague.Domain.Student.Entities.MealPlanAssignment createdStudentMealPlans = null;


                // verify the user has the permission to update a StudentMealPlans
                CheckCreateMealPlanAssignmentPermission();
                
                _studentMealPlanRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                // map the DTO to entities
                var StudentMealPlansEntity
                    = await ConvertStudentMealPlansDtoToEntityAsync(StudentMealPlansDto);

                // create the entity in the database
                createdStudentMealPlans = await _studentMealPlanRepository.CreateMealPlanAssignmentAsync(StudentMealPlansEntity);
                var dtoStudentMealPlans = await ConvertMealPlanAssignmentEntityToDtoAsync(createdStudentMealPlans, true);

                // return the newly updated DTO
                return dtoStudentMealPlans;

            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }


        }
        
        /// <summary>
        /// Get all AcademicPeriod Entity Objects
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will re-query cached items</param>
        /// <returns>List of <see cref="AcademicPeriod">AcademicPeriod objects</see></returns>
        private async Task<IEnumerable<Domain.Student.Entities.AcademicPeriod>> GetAcademicPeriodsAsync(bool bypassCache)
        {
            if (_academicPeriods == null)
            {
                var termEntities = await _termRepository.GetAsync(bypassCache);
                _academicPeriods = _termRepository.GetAcademicPeriods(termEntities);
            }
            return _academicPeriods;
        }

        /// <summary>
        /// Get all MealPlan Entity Objects
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will re-query cached items</param>
        /// <returns>List of <see cref="MealPlan">Entities.MealPlan objects</see></returns>
        private async Task<IEnumerable<Domain.Student.Entities.MealPlan>> GetMealPlansAsync(bool bypassCache)
        {
            if (_mealPlans == null)
            {
                _mealPlans = await _referenceDataRepository.GetMealPlansAsync(bypassCache);
            }
            return _mealPlans;
        }

        /// <summary>
        /// Get all MealPlanRates Entity Objects
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will re-query cached items</param>
        /// <returns>List of <see cref="MealPlanRates">Entities.MealPlanRates objects</see></returns>
        private async Task<IEnumerable<Domain.Student.Entities.MealPlanRates>> GetMealPlanRatesAsync(bool bypassCache)
        {
            if (_mealPlanRates == null)
            {
                _mealPlanRates = await _referenceDataRepository.GetMealPlanRatesAsync(bypassCache);
            }
            return _mealPlanRates;
        }

        /// <summary>
        /// Get all AccountingCodes Entity Objects
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will re-query cached items</param>
        /// <returns>List of <see cref="AccountingCode">Entities.AccountingCode objects</see></returns>
        private async Task<IEnumerable<Domain.Student.Entities.AccountingCode>> GetAccountingCodesAsync(bool bypassCache)
        {
            if (_accountingCodes == null)
            {
                _accountingCodes = await _referenceDataRepository.GetAccountingCodesAsync(bypassCache);
            }
            return _accountingCodes;
        }

        /// <summary>
        /// Get all BillingOverrideReasons Entity Objects
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will re-query cached items</param>
        /// <returns>List of <see cref="BillingOverrideReasons">Entities.BillingOverrideReasons objects</see></returns>
        private async Task<IEnumerable<Domain.Student.Entities.BillingOverrideReasons>> GetBillingOverrideReasonsAsync(bool bypassCache)
        {
            if (_billingOverrideReasons == null)
            {
                _billingOverrideReasons = await _referenceDataRepository.GetBillingOverrideReasonsAsync(bypassCache);
            }
            return _billingOverrideReasons;
        }

        /// <summary>
        /// Get Host Country
        /// </summary>
        /// <returns>string representng the host country</returns>
        private async Task<string> GetHostCountryAsync()
        {
            if (_hostCountry == null)
            {
                _hostCountry = await _referenceDataRepository.GetHostCountryAsync();
            }
            return _hostCountry;
        }
               
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a MealPlanAssignment domain entity to its corresponding StudentMealPlans DTO
        /// </summary>
        /// <param name="source">MealPlanAssignment domain entity</param>
        /// <returns>StudentMealPlans DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.StudentMealPlans> ConvertMealPlanAssignmentEntityToDtoAsync(MealPlanAssignment source, bool bypassCache = false)
        {
            var studentMealPlans = new Ellucian.Colleague.Dtos.StudentMealPlans();

            if (!string.IsNullOrEmpty(source.Term))
            {
                var academicPeriodEntities = await GetAcademicPeriodsAsync(bypassCache);

                if (academicPeriodEntities == null)
                {
                    throw new Exception("Unable to retrieve academic periods");
                }
                var term = academicPeriodEntities.FirstOrDefault(mp => mp.Code == source.Term);
                if (term == null)
                {
                    throw new Exception(string.Concat("Unable to locate guid for term: ", source.Term, ", Entity: 'MEAL.PLAN.ASSIGNMENTS', Record ID: '", source.Id, "' "));
                }
                studentMealPlans.AcademicPeriod = new Dtos.GuidObject2(term.Guid);

            }

            if ((source.OverrideRate != null) && (source.OverrideRate.HasValue) && (source.OverrideRate > 0))
            {
                studentMealPlans.OverrideRate = await BuildAssignedOverrideAsync(source, bypassCache);
            }

            if (source.NoRatePeriods != null) studentMealPlans.NumberOfPeriods = source.NoRatePeriods;

            if (!string.IsNullOrEmpty(source.MealPlan))
            {
                var startDate = Convert.ToDateTime(source.StartDate);
                var mealPlanRates = await GetMealPlanRatesAsync(bypassCache);

                if (mealPlanRates == null)
                {
                    throw new Exception("Unable to retrieve meal plan rates");
                }
                var mealPlanRate = mealPlanRates.Where(mpr => mpr.Code == source.MealPlan && mpr.MealPlansMealPlanRates != null)
                    .OrderByDescending(mpr => mpr.MealPlansMealPlanRates.EffectiveDates);

                if (mealPlanRate != null)
                {
                    var mealPlanGuid = string.Empty;
                    foreach (var mealPlan in mealPlanRate)
                    {
                        if ((mealPlan.MealPlansMealPlanRates != null) && (DateTime.Compare(startDate, Convert.ToDateTime(mealPlan.MealPlansMealPlanRates.EffectiveDates)) >= 0))
                        {
                            mealPlanGuid = mealPlan.Guid;
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(mealPlanGuid))
                    {
                        studentMealPlans.AssignedRate = new Dtos.GuidObject2(mealPlanGuid);
                    }
                }
            }



            if (source.EndDate != null)
            {
                studentMealPlans.EndOn = Convert.ToDateTime(source.EndDate);
            }
            studentMealPlans.Id = source.Guid;

            if (!string.IsNullOrEmpty(source.MealPlan))
            {
                var mealPlans = await GetMealPlansAsync(bypassCache);
                if (mealPlans == null)
                {
                    throw new Exception("Unable to retrieve meal plans");
                }
                var mealPlan = mealPlans.FirstOrDefault(mp => mp.Code == source.MealPlan);
                if (mealPlan == null)
                {
                    throw new Exception(string.Concat("Unable to locate guid for MealPlan: ", source.MealPlan, ", Entity: 'MEAL.PLAN.ASSIGNMENTS', Record ID: '", source.Id, "' "));
                }
                studentMealPlans.MealPlan = new Dtos.GuidObject2(mealPlan.Guid);
            }

            if (!string.IsNullOrEmpty(source.PersonId))
            {
                var personGuid = await _personRepository.GetPersonGuidFromIdAsync(source.PersonId);
                if (string.IsNullOrEmpty(personGuid))
                {
                    throw new ArgumentException(string.Concat("Unable to find a GUID for Person ", source.PersonId, " person.id.   Entity: 'MEAL.PLAN.ASSIGNMENTS', Record ID: '", source.Id, "' "));
                }
                studentMealPlans.Person = new Dtos.GuidObject2(personGuid);
            }

            studentMealPlans.StartOn = Convert.ToDateTime(source.StartDate);

            if (!string.IsNullOrEmpty(source.Status))
            {
                studentMealPlans.Status = ConvertStatusToStudentMealPlansStatusDtoEnum(source.Status);
            }

            if (source.StatusDate != null)
            {
                studentMealPlans.StatusDate = Convert.ToDateTime(source.StatusDate);
            }

            if (!string.IsNullOrEmpty(source.MealCard)) studentMealPlans.MealCard = source.MealCard;

            if (!string.IsNullOrEmpty(source.MealComments)) studentMealPlans.Comment = source.MealComments;

            if (source.UsedRatePeriods >= 0 || source.PercentUsed >= 0)
            {
                var studentMealPlansConsumption = new StudentMealPlansConsumption();
                if (source.PercentUsed >= 0)
                {
                    studentMealPlansConsumption.Percent = source.PercentUsed;
                }
                else
                {
                    studentMealPlansConsumption.Units = source.UsedRatePeriods;
                }                
                studentMealPlans.Consumption = studentMealPlansConsumption;
            }
            

            return studentMealPlans;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a StudentMealPlans DTO to its corresponding MealPlanAssignment domain entity  
        /// </summary>
        /// <param name="source">StudentMealPlans DTO</param>
        /// <returns>MealPlanAssignment</returns>
        private async Task<MealPlanAssignment> ConvertStudentMealPlansDtoToEntityAsync(Ellucian.Colleague.Dtos.StudentMealPlans source, bool bypassCache = false)
        {
            //check for required data
            //handle empty guid
            var guid = string.Empty;
            var id = string.Empty;
            var personId = string.Empty;
            var mealplanId = string.Empty;
            int? NoPeriods = 0;
            var status = string.Empty;
            DateTime? statusDate = new DateTime();
            DateTime? startDate = new DateTime();
            if (!string.Equals(source.Id, Guid.Empty.ToString()))
            {
                guid = source.Id;
            }
            //get person ID
            if (source.Person != null && !string.IsNullOrEmpty(source.Person.Id))
            {
                personId = await _personRepository.GetPersonIdFromGuidAsync(source.Person.Id);
                if (string.IsNullOrEmpty(personId))
                {
                    throw new ArgumentException(string.Concat(" Person ID '", source.Person.Id.ToString(), "' was not found. Valid Person is required."));
                }
            }
            else
            {
                throw new ArgumentException(string.Concat("Person is required."));
            }
            //get mealplan ID
            if (source.MealPlan != null && !string.IsNullOrEmpty(source.MealPlan.Id))
            {
                var mealPlans = await GetMealPlansAsync(bypassCache);
                if (mealPlans == null)
                {
                    throw new Exception("Unable to retrieve meal plans");
                }
                var mealPlan = mealPlans.FirstOrDefault(mp => mp.Guid == source.MealPlan.Id);
                if (mealPlan == null)
                {
                    throw new ArgumentException(string.Concat(" Meal Plan ID '", source.MealPlan.Id.ToString(), "' was not found. Valid Meal Plan is required."));
                }
                else
                {
                    mealplanId = mealPlan.Code;
                }
                
            }
            else
            {
                throw new ArgumentException(string.Concat("MealPlan is required."));
            }
            //get status code
            if (source.Status == null || source.Status == Dtos.EnumProperties.StudentMealPlansStatus.NotSet)
            {
                throw new ArgumentException(string.Concat("Invalid Status. Status is required."));
            }
            else
            {
                var statusCode = ConvertStudentMealPlansStatusDtoEnumToStatus(source.Status);
                if (string.IsNullOrEmpty(statusCode))
                {
                    throw new ArgumentException(string.Concat(" Status '", source.Status.ToString(), "' was not found. Valid Status is required."));
                }
                else
                {
                    status = statusCode;
                }
            }

            // get numger of periods
            NoPeriods = source.NumberOfPeriods;

            //get start Date

            if (source.StartOn == null || source.StartOn == DateTime.MinValue)
            {
                throw new ArgumentException(string.Concat("Start Date is required."));
            }
            else
            {
                startDate = Convert.ToDateTime(source.StartOn.Value.ToString("yyyy-MM-dd"));
            }
            //get status date
             if (source.StatusDate == null || source.StatusDate == DateTime.MinValue)
            {
                throw new ArgumentException(string.Concat("Status Date is required."));
            }
            else
            {
                statusDate = Convert.ToDateTime(source.StatusDate.Value.ToString("yyyy-MM-dd"));
            }

            //now we can create the entity
             var studentMealPlans = new MealPlanAssignment(guid, id, personId, mealplanId, startDate, NoPeriods, status, statusDate);

            //get the term
            if (source.AcademicPeriod != null && !string.IsNullOrEmpty(source.AcademicPeriod.Id))
            {
                var academicPeriodEntities = await GetAcademicPeriodsAsync(bypassCache);

                if (academicPeriodEntities == null)
                {
                    throw new Exception("Unable to retrieve academic periods");
                }
                var term = academicPeriodEntities.FirstOrDefault(mp => mp.Guid == source.AcademicPeriod.Id);
                if (term == null)
                {
                    throw new Exception(string.Concat(" Academic Period '", source.AcademicPeriod.Id, "' was not found."));
                }
                studentMealPlans.Term = term.Code;
            }

            //get end on
            if (source.EndOn != null && source.EndOn.HasValue)
            {
                studentMealPlans.EndDate = Convert.ToDateTime(source.EndOn.Value.ToString("yyyy-MM-dd"));
            }

            //get comments
            if (source.Comment != null && !string.IsNullOrEmpty(source.Comment))
            {
                studentMealPlans.MealComments = source.Comment;
            }

            //get mealcard
            if (source.MealCard != null && !string.IsNullOrEmpty(source.MealCard))
            {
                studentMealPlans.MealCard = source.MealCard;
            }        

            //get used rate periods and percentage
            if (source.Consumption != null && source.Consumption.Units != null && source.Consumption.Percent != null)
            {
                studentMealPlans.UsedRatePeriods = source.Consumption.Units;
                studentMealPlans.PercentUsed = source.Consumption.Percent;
            }
            
            //The rate period associated with the assignment cannot be overridden from the rate period set on the meal plan rate. On a PUT/POST we will ignore and respond with the ratePeriod of the meal plan.
            if (source.AssignedRate != null)
            {
                var defaultRate = source.AssignedRate;                
                DateTime? effectiveDate = new DateTime();
                if (defaultRate != null)
                {
                    var mealPlanRates = await GetMealPlanRatesAsync(bypassCache);
                    if (mealPlanRates == null)
                    {
                        throw new Exception("Unable to retrieve meal plan rates");
                    }
                    var mealPlanRate = mealPlanRates.FirstOrDefault(mpr => mpr.Guid == defaultRate.Id);
                    if (mealPlanRate == null)
                    {
                        throw new Exception(string.Concat(" Meal Plan rate '", defaultRate.Id.ToString(), "' was not found."));
                    }
                    else
                    {
                        if (mealPlanRate.Code != mealplanId)
                        {
                            throw new Exception(string.Concat("Meal plan '", mealPlanRate.Code, "' from meal plan rate does not match assignment's meal plan '", mealplanId, "'"));
                        }
                        var correctMealPlanRate = string.Empty;
                        //
                        // Find the appropriate meal plan rate based on the meal plan rate effective dates and the start date
                        // of the meal plan assignment. Issue error if it does not match the incoming meal plan rate.
                        //
                        if (mealPlanRate.MealPlansMealPlanRates != null)
                        {
                            effectiveDate = mealPlanRate.MealPlansMealPlanRates.EffectiveDates;
                        }
                        var mealPlanRatesSorted = mealPlanRates.Where(mpr => mpr.Code == studentMealPlans.MealPlan && mpr.MealPlansMealPlanRates != null)
                            .OrderByDescending(mpr => mpr.MealPlansMealPlanRates.EffectiveDates);                        
                        if (mealPlanRatesSorted != null || mealPlanRatesSorted.Count() >= 0)
                        {                   
                            foreach (var tempMealPlanRate in mealPlanRatesSorted)
                            {
                                DateTime tempStartDate = (DateTime)studentMealPlans.StartDate;
                                if ((tempMealPlanRate.MealPlansMealPlanRates != null) && (DateTime.Compare(tempStartDate, Convert.ToDateTime(tempMealPlanRate.MealPlansMealPlanRates.EffectiveDates)) >= 0))
                                {
                                    correctMealPlanRate = tempMealPlanRate.Guid;
                                    break;
                                }
                            }
                        }
                        if (correctMealPlanRate != defaultRate.Id)
                        {
                            throw new Exception(string.Concat(" Invalid meal plan rate '", defaultRate.Id.ToString(), "' with effective date '", effectiveDate, "' for start date of '", startDate, "'"));
                        }
                    }                    
                }
            }

            if (source.OverrideRate != null)
            {
                //we need to try casting it and try catch the error to see which one.
                try
                {
                    var overrideRate = source.OverrideRate.Rate;                 
                    //get accounting code
                    if (source.OverrideRate.AccountingCode != null)
                    {
                        var accountingCodeEntities = await GetAccountingCodesAsync(bypassCache);
                        if (accountingCodeEntities == null)
                        {
                            throw new Exception("Unable to retrieve accounting codes");
                        }
                        var accountingCode = accountingCodeEntities.FirstOrDefault(mpr => mpr.Guid == source.OverrideRate.AccountingCode.Id);
                        if (accountingCode == null)
                        {
                            throw new Exception(string.Concat(" Accounting Code '", source.OverrideRate.AccountingCode.Id.ToString(), "' was not found."));
                        }
                        studentMealPlans.OverrideArCode = accountingCode.Code;
                    }
                    //get override reason
                    if (source.OverrideRate.OverrideReason != null)
                    {
                        var billingOverrideReasonsEntities = await GetBillingOverrideReasonsAsync(bypassCache);
                        if (billingOverrideReasonsEntities == null)
                        {
                            throw new Exception("Unable to retrieve billing override reasons");
                        }
                        var billingOverrideReason = billingOverrideReasonsEntities.FirstOrDefault(mpr => mpr.Guid == source.OverrideRate.OverrideReason.Id);
                        if (billingOverrideReason == null)
                        {
                            throw new Exception(string.Concat("Override Reason '", source.OverrideRate.OverrideReason.Id.ToString(), "' was not found."));
                        }
                        studentMealPlans.RateOverrideReason = billingOverrideReason.Code;
                    }
                    //get rate value
                    if (source.OverrideRate.Rate != null)
                    {
                        if (source.OverrideRate.Rate.Value != null && source.OverrideRate.Rate.Currency != Dtos.EnumProperties.CurrencyIsoCode.USD && source.OverrideRate.Rate.Currency != Dtos.EnumProperties.CurrencyIsoCode.CAD)
                        {
                            throw new ArgumentException("The override rate currency must be set to either 'USD' or 'CAD'. ");
                        }
                        if (source.OverrideRate.Rate.Value < 0)
                        {
                            throw new ArgumentException("The override rate amount must be set greater than zero. ");
                        }
                        studentMealPlans.OverrideRate = source.OverrideRate.Rate.Value;
                    }                    
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message, ex.InnerException);
                }
            }

            return studentMealPlans;
        }

        /// <summary>
        ///  Build StudentMealPlanAssignedRatesDtoProperty
        /// </summary>
        /// <param name="source">MealPlanAssignment domain entity</param>
        /// <param name="bypassCache"></param>
        /// <returns><see cref="StudentMealPlanAssignedRatesDtoProperty">Dtos.StudentMealPlanAssignedRatesDtoProperty object</see></returns>
        private async Task<GuidObject2> BuildAssignedDefaultAsync(MealPlanAssignment source, bool bypassCache = false)        
        {

            GuidObject2 assignedRate = new GuidObject2();
            
            if (!string.IsNullOrEmpty(source.MealPlan))
            {
                var startDate = Convert.ToDateTime(source.StartDate);
                var mealPlanRates = await GetMealPlanRatesAsync(bypassCache);
                
                if (mealPlanRates == null)
                {
                    throw new Exception("Unable to retrieve meal plan rates");
                }
                var mealPlanRate = mealPlanRates.Where(mpr => mpr.Code == source.MealPlan && mpr.MealPlansMealPlanRates != null)
                    .OrderByDescending(mpr => mpr.MealPlansMealPlanRates.EffectiveDates);

                if (mealPlanRate != null)
                {
                    var mealPlanGuid = string.Empty;
                    foreach (var mealPlan in mealPlanRate)
                    {
                        if ((mealPlan.MealPlansMealPlanRates != null) && (DateTime.Compare(startDate, Convert.ToDateTime(mealPlan.MealPlansMealPlanRates.EffectiveDates)) >= 0))
                        {
                            mealPlanGuid = mealPlan.Guid;
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(mealPlanGuid))
                    {
                        assignedRate = new Dtos.GuidObject2(mealPlanGuid);
                    }
                }
            }
            return assignedRate;
        }
        
        /// <summary>
        ///  Build StudentMealPlanAssignedOverrideDtoProperty
        /// </summary>
        /// <param name="source">MealPlanAssignment domain entity</param>
        /// <param name="bypassCache"></param>
        /// <returns><see cref="StudentMealPlanAssignedOverrideDtoProperty">Dtos.StudentMealPlanAssignedOverrideDtoProperty object</see></returns>
        private async Task<StudentMealPlansOverrideRateDtoProperty> BuildAssignedOverrideAsync(MealPlanAssignment source, bool bypassCache = false)
        {
            var studentMealPlansOverrideRate = new StudentMealPlansOverrideRateDtoProperty();
            if (!(string.IsNullOrEmpty(source.OverrideArCode)))
            {
                var accountingCodeEntities = await GetAccountingCodesAsync(bypassCache);
                if (accountingCodeEntities == null)
                {
                    throw new Exception("Unable to retrieve accounting codes");
                }
                var accountingCode = accountingCodeEntities.FirstOrDefault(mpr => mpr.Code == source.OverrideArCode);
                if (accountingCode == null)
                {
                    throw new Exception(string.Concat("Unable to locate guid for override AR code: ", source.OverrideArCode, ", Entity: 'MEAL.PLAN.ASSIGNMENTS', Record ID: '", source.Id, "'",
                        " Student: '", source.PersonId, "'", " Term: '", source.Term, '"'));

                }
                studentMealPlansOverrideRate.AccountingCode = new Dtos.GuidObject2(accountingCode.Guid);
            }

            if (!(string.IsNullOrEmpty(source.RateOverrideReason)))
            {
                var billingOverrideReasonsEntities = await GetBillingOverrideReasonsAsync(bypassCache);
                if (billingOverrideReasonsEntities == null)
                {
                    throw new Exception("Unable to retrieve billing override reasons");
                }
                var billingOverrideReason = billingOverrideReasonsEntities.FirstOrDefault(mpr => mpr.Code == source.RateOverrideReason);
                if (billingOverrideReason == null)
                {
                    throw new Exception(string.Concat("Unable to locate guid for rate override reason: ", source.RateOverrideReason, ", Entity: 'MEAL.PLAN.ASSIGNMENTS', Record ID: '", source.Id, "' ",
                        " Student: '", source.PersonId, "'", " Term: '", source.Term, '"'));

                }
                studentMealPlansOverrideRate.OverrideReason = new Dtos.GuidObject2(billingOverrideReason.Guid);
            }

            if ((source.OverrideRate != null) && (source.OverrideRate.HasValue))
            {
                studentMealPlansOverrideRate.Rate = new Amount2DtoProperty()
                {
                    Currency = (await GetHostCountryAsync()).ToUpper() == "USA" ? CurrencyIsoCode.USD : CurrencyIsoCode.CAD,
                    Value = source.OverrideRate
                };
            }

            if (!string.IsNullOrEmpty(source.MealPlan))
            {
                var mealPlans = await GetMealPlansAsync(bypassCache);

                if (mealPlans == null)
                {
                    throw new Exception("Unable to retrieve meal plans");
                }
                var mealPlan = mealPlans.FirstOrDefault(mpr => mpr.Code == source.MealPlan);
                if (mealPlan == null)
                {
                    throw new Exception(string.Concat("Unable to locate guid for MealPlan: ", source.MealPlan, ", Entity: 'MEAL.PLAN.ASSIGNMENTS', Record ID: '", source.Id, "' ",
                        " Student: '", source.PersonId, "'", " Term: '", source.Term, '"'));

                }
                studentMealPlansOverrideRate.RatePeriod = ConvertMealPlanRateToMealPlanRatesRatePeriodDtoEnum(mealPlan.RatePeriod);
            }
            
            return studentMealPlansOverrideRate;
        }

        /// <summary>
        /// Permissions code that allows an external system to do a READ operation. This API will integrate information related to meal plan assignments that 
        /// could be deemed personal.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewMealPlanAssignmentPermission()
        {
            var hasPermission = HasPermission(StudentPermissionCodes.ViewMealPlanAssignment);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view MEAL.PLAN.ASSIGNMENT.");
            }
        }

        /// <summary>
        /// Permissions code that allows an external system to do a UPDATE operation. This API will integrate information related to meal plan assignments that 
        /// could be deemed personal.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckCreateMealPlanAssignmentPermission()
        {
            var hasPermission = HasPermission(StudentPermissionCodes.CreateMealPlanAssignment);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to update MEAL.PLAN.ASSIGNMENT.");
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a meal assignment status to its corresponding StudentMealPlansStatus DTO enumeration value
        /// </summary>
        /// <param name="source">meal assignment status</param>
        /// <returns>StudentMealPlansStatus DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.EnumProperties.StudentMealPlansStatus ConvertStatusToStudentMealPlansStatusDtoEnum(string source)
        {
            switch (source)
            {

                case "A":
                    return Dtos.EnumProperties.StudentMealPlansStatus.Assigned;
                case "C":
                    return Dtos.EnumProperties.StudentMealPlansStatus.Cancelled;
                case "T":
                    return Dtos.EnumProperties.StudentMealPlansStatus.Terminated;
                case "L":
                    return Dtos.EnumProperties.StudentMealPlansStatus.Prorated;
                default:
                    return Dtos.EnumProperties.StudentMealPlansStatus.NotSet;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a StudentMealPlansStatus DTO enumeration value to its corresponding meal assignment status 
        /// </summary>
        /// <param name="source">StudentMealPlansStatus DTO enumeration value</param>
        /// <returns> meal assignment status</returns>
        private string ConvertStudentMealPlansStatusDtoEnumToStatus(Dtos.EnumProperties.StudentMealPlansStatus? source)
        {
            switch (source)
            {

                case Dtos.EnumProperties.StudentMealPlansStatus.Assigned :
                    return "A";
                case Dtos.EnumProperties.StudentMealPlansStatus.Cancelled:
                    return "C";
                case Dtos.EnumProperties.StudentMealPlansStatus.Terminated:
                    return "T";
                case Dtos.EnumProperties.StudentMealPlansStatus.Prorated:
                    return "L";
                default:
                    return string.Empty;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a meal plan rate to its corresponding MealPlanRatesRatePeriod DTO enumeration value
        /// </summary>
        /// <param name="source">meal plan rate value</param>
        /// <returns>MealPlanRatesRatePeriod DTO enumeration value</returns>
        private MealPlanRatesRatePeriod? ConvertMealPlanRateToMealPlanRatesRatePeriodDtoEnum(string source)
        {
            switch (source)
            {
                case "D":
                    return MealPlanRatesRatePeriod.Day;
                case "W":
                    return MealPlanRatesRatePeriod.Week;
                case "Y":
                    return MealPlanRatesRatePeriod.Year;
                case "T":
                    return MealPlanRatesRatePeriod.Term;
                case "B":
                    return MealPlanRatesRatePeriod.Meal;
                case "M":
                    return MealPlanRatesRatePeriod.Month;
                default:
                    return MealPlanRatesRatePeriod.NotSet;
            }
        }


    }
}