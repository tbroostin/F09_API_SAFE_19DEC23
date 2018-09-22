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
    public class MealPlanRequestsService : BaseCoordinationService, IMealPlanRequestsService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IMealPlanReqsIntgRepository _mealPlanRequestRepository;
        private readonly IPersonRepository _personRepository;
        private readonly ITermRepository _termRepository;
        private readonly IConfigurationRepository _configurationRepository;

        private IEnumerable<Domain.Student.Entities.AcademicPeriod> _academicPeriods;
        private IEnumerable<Domain.Student.Entities.MealPlan> _mealPlans;

        public MealPlanRequestsService(

            IStudentReferenceDataRepository referenceDataRepository,
            IMealPlanReqsIntgRepository MealPlanRequestRepository,
            ITermRepository termRepository,
            IPersonRepository personRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
            _mealPlanRequestRepository = MealPlanRequestRepository;
            _termRepository = termRepository;
            _personRepository = personRepository;
            _configurationRepository = configurationRepository;

        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all meal plan requests
        /// </summary>
        /// <param name="offset">Paging offset</param>
        /// <param name="limit">Paging limit</param>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will re-query cached items</param>
        /// <returns>List of <see cref="Dtos.MealPlanRequests">Dtos.MealPlanRequests</see></returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.MealPlanRequests>, int>> GetMealPlanRequestsAsync(int offset, int limit, bool bypassCache = false)
        {
            CheckViewMealPlanRequestPermission();

            var MealPlanRequestsCollection = new List<Ellucian.Colleague.Dtos.MealPlanRequests>();

            var MealPlanRequestEntities = await _mealPlanRequestRepository.GetAsync(offset, limit, bypassCache);
            var totalRecords = MealPlanRequestEntities.Item2;

            foreach (var MealPlanRequestEntity in MealPlanRequestEntities.Item1)
            {
                var MealPlanRequestsDto = await ConvertMealPlanRequestEntityToDtoAsync(MealPlanRequestEntity, bypassCache);
                MealPlanRequestsCollection.Add(MealPlanRequestsDto);
            }
            return new Tuple<IEnumerable<Ellucian.Colleague.Dtos.MealPlanRequests>, int>(MealPlanRequestsCollection, totalRecords);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a MealPlanRequests from its GUID
        /// </summary>
        /// <param name="guid">Guid</param>
        /// <returns><see cref="Dtos.MealPlanRequests">Dtos.MealPlanRequests object</see></returns>
        public async Task<Ellucian.Colleague.Dtos.MealPlanRequests> GetMealPlanRequestsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "A GUID is required to obtain an meal-plan-request.");
            }

            CheckViewMealPlanRequestPermission();

            try
            {
                return await ConvertMealPlanRequestEntityToDtoAsync(await _mealPlanRequestRepository.GetByIdAsync(guid));
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("meal plan requests not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("meal plan requests not found for GUID " + guid, ex);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Put (Update) an Student Meal Request domain entity
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="MealPlanRequestsDto"><see cref="Dtos.MealPlanRequests">MealPlanRequests</see></param>
        /// <returns><see cref="Dtos.MealPlanRequests">MealPlanRequests</see></returns>
        public async Task<MealPlanRequests> PutMealPlanRequestsAsync(string guid, Dtos.MealPlanRequests MealPlanRequestsDto)
        {
            try
            {

                if (MealPlanRequestsDto == null)
                    throw new ArgumentNullException("MealPlanRequests", "Must provide a MealPlanRequests for update");
                if (string.IsNullOrEmpty(MealPlanRequestsDto.Id))
                    throw new ArgumentNullException("MealPlanRequests", "Must provide a guid for MealPlanRequests update");



                // verify the user has the permission to update a MealPlanRequests
                CheckCreateMealPlanRequestPermission();

                _mealPlanRequestRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                // map the DTO to entities
                var MealPlanRequestsEntity
                    = await ConvertMealPlanRequestsDtoToEntityAsync(MealPlanRequestsDto);

                // update the entity in the database
                var updatedMealPlanRequestsEntity =
                    await _mealPlanRequestRepository.UpdateMealPlanReqsIntgAsync(MealPlanRequestsEntity);
                var dtoMealPlanRequests = await ConvertMealPlanRequestEntityToDtoAsync(updatedMealPlanRequestsEntity, true);


                // return the newly updated DTO
                return dtoMealPlanRequests;

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
        /// Post (Create) an Student meal plan request doamin entity
        /// </summary>
        /// <param name="MealPlanRequestsDto"><see cref="Dtos.MealPlanRequests">MealPlanRequests</see></param>
        /// <returns><see cref="Dtos.MealPlanRequests">MealPlanRequests</see></returns>
        public async Task<MealPlanRequests> PostMealPlanRequestsAsync(Dtos.MealPlanRequests MealPlanRequestsDto)
        {
            try
            {
                if (MealPlanRequestsDto == null)
                    throw new ArgumentNullException("MealPlanRequests", "Must provide a MealPlanRequests for update");
                if (string.IsNullOrEmpty(MealPlanRequestsDto.Id))
                    throw new ArgumentNullException("MealPlanRequests", "Must provide a guid for MealPlanRequests update");
                Ellucian.Colleague.Domain.Student.Entities.MealPlanReqsIntg createdMealPlanRequests = null;


                // verify the user has the permission to update a MealPlanRequests
                CheckCreateMealPlanRequestPermission();

                _mealPlanRequestRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                // map the DTO to entities
                var MealPlanRequestsEntity
                    = await ConvertMealPlanRequestsDtoToEntityAsync(MealPlanRequestsDto);

                // create the entity in the database
                createdMealPlanRequests = await _mealPlanRequestRepository.CreateMealPlanReqsIntgAsync(MealPlanRequestsEntity);
                var dtoMealPlanRequests = await ConvertMealPlanRequestEntityToDtoAsync(createdMealPlanRequests, true);


                // return the newly updated DTO
                return dtoMealPlanRequests;

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

         /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a MealPlanRequest domain entity to its corresponding MealPlanRequests DTO
        /// </summary>
        /// <param name="source">MealPlanRequest domain entity</param>
        /// <returns>MealPlanRequests DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.MealPlanRequests> ConvertMealPlanRequestEntityToDtoAsync(MealPlanReqsIntg source, bool bypassCache = false)
        {
            var MealPlanRequests = new Ellucian.Colleague.Dtos.MealPlanRequests();

           //convert term
            if (!string.IsNullOrEmpty(source.Term))
            {
                _academicPeriods = null;

                var academicPeriodEntities = await GetAcademicPeriodsAsync(bypassCache);

                if (academicPeriodEntities == null)
                {
                    throw new Exception("Unable to retrieve academic periods");
                }
                var term = academicPeriodEntities.FirstOrDefault(mp => mp.Code == source.Term);
                if (term == null)
                {
                    throw new KeyNotFoundException(string.Concat("Unable to locate guid for term: ", source.Term, ", Entity: 'MEAL.PLAN.ASSIGNMENTS', Record ID: '", source.Id, "' "));
                }
                MealPlanRequests.AcademicPeriod = new Dtos.GuidObject2(term.Guid);

            }

           //convert end date
            if (source.EndDate != null)
            {
                MealPlanRequests.EndOn = Convert.ToDateTime(source.EndDate);
            }

            //convert submitted 
            if (source.SubmittedDate != null)
            {
                MealPlanRequests.SubmittedOn = Convert.ToDateTime(source.SubmittedDate);
            }
            //assign guid
            MealPlanRequests.Id = source.Guid;
            //convert mealplan
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
                    throw new KeyNotFoundException(string.Concat("Unable to locate guid for MealPlan: ", source.MealPlan, ", Entity: 'MEAL.PLAN.ASSIGNMENTS', Record ID: '", source.Id, "' "));
                }
                MealPlanRequests.MealPlan = new Dtos.GuidObject2(mealPlan.Guid);
            }
            //convert person
            if (!string.IsNullOrEmpty(source.PersonId))
            {
                var personGuid = await _personRepository.GetPersonGuidFromIdAsync(source.PersonId);
                if (string.IsNullOrEmpty(personGuid))
                {
                    throw new KeyNotFoundException(string.Concat("Unable to find a GUID for Person ", source.PersonId, " person.id.   Entity: 'MEAL.PLAN.ASSIGNMENTS', Record ID: '", source.Id, "' "));
                }
                MealPlanRequests.Person = new Dtos.GuidObject2(personGuid);
            }
            //convert start date
            if (source.StartDate != null)
            {
            MealPlanRequests.StartOn = Convert.ToDateTime(source.StartDate);
            }
            //convert status
            
            if (!string.IsNullOrEmpty(source.Status))
            {
                var status = new Ellucian.Colleague.Dtos.EnumProperties.MealPlanRequestsStatus();
                status =  ConvertStatusToMealPlanRequestsStatusDtoEnum(source.Status);
                MealPlanRequests.Status = status;

            }

            return MealPlanRequests;
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a MealPlanRequests DTO to its corresponding MealPlanRequest domain entity  
        /// </summary>
        /// <param name="source">MealPlanRequest DTO</param>
        /// <returns>MealPlanRequests DTO</returns>
        private async Task<MealPlanReqsIntg> ConvertMealPlanRequestsDtoToEntityAsync(Ellucian.Colleague.Dtos.MealPlanRequests source, bool bypassCache = false)
        {
            //check for required data
            //handle empty guid
            var guid = string.Empty;
            var id = string.Empty;
            var personId = string.Empty;
            var mealplanId = string.Empty;
            var status = string.Empty;
            if (!string.Equals(source.Id, Guid.Empty.ToString()))
            {
                guid = source.Id;
            }
            //get person ID
            if (source.Person != null)
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
            if (source.MealPlan != null)
            {
                var mealPlans = await GetMealPlansAsync(bypassCache);
                if (mealPlans == null || !mealPlans.Any())
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
           
            ////get status date
            //if (source.StatusDate == null || source.StatusDate == DateTime.MinValue)
            //{
            //    throw new ArgumentException(string.Concat("Status Date is required."));
            //}
            //else
            //{
            //    statusDate = Convert.ToDateTime(source.StatusDate.Value.ToString("yyyy-MM-dd"));
            //}

            //now we can create the entity
             var mealPlanRequests = new MealPlanReqsIntg(guid, personId, mealplanId);

            //get the term
             var termStartDate = new DateTime();
             var termEndDate = new DateTime();
            if (source.AcademicPeriod != null)
            {
                var academicPeriodEntities = await GetAcademicPeriodsAsync(bypassCache);

                if (academicPeriodEntities == null || !academicPeriodEntities.Any())
                {
                    throw new Exception("Unable to retrieve academic periods");
                }
                var term = academicPeriodEntities.FirstOrDefault(mp => mp.Guid == source.AcademicPeriod.Id);
                if (term == null)
                {
                    throw new Exception(string.Concat(" Academic Period '", source.AcademicPeriod.Id.ToString(), "' was not found."));
                }
                mealPlanRequests.Term = term.Code;
                termStartDate = term.StartDate;
                termEndDate = term.EndDate;
            }

             //get start Date

            if (source.StartOn != null && source.StartOn != DateTime.MinValue)
            {
                mealPlanRequests.StartDate= Convert.ToDateTime(source.StartOn.Value.ToString("yyyy-MM-dd"));
            }
            else if (termStartDate != DateTime.MinValue)
            {
                mealPlanRequests.StartDate = termStartDate;
            }


            //get end on
            if (source.EndOn != null)
            {
                mealPlanRequests.EndDate = Convert.ToDateTime(source.EndOn.Value.ToString("yyyy-MM-dd"));
            }
            else if (termEndDate != DateTime.MinValue)
            {
                mealPlanRequests.EndDate = termEndDate;
            }

            //get submitted on
            if (source.SubmittedOn != null)
            {
                mealPlanRequests.SubmittedDate = Convert.ToDateTime(source.SubmittedOn.Value.ToString("yyyy-MM-dd"));
            }

            //get status code
            if (source.Status == null || source.Status == Dtos.EnumProperties.MealPlanRequestsStatus.NotSet)
            {
                throw new ArgumentException(string.Concat("Invalid Status. Status is required."));
            }
            else
            {
                var statusCode = ConvertMealPlanRequestsStatusDtoEnumToStatus(source.Status);
                if (string.IsNullOrEmpty(statusCode))
                {
                    throw new ArgumentException(string.Concat(" Status '", source.Status.ToString(), "' was not found. Valid Status is required."));
                }
                else
                {
                    mealPlanRequests.Status = statusCode;
                }
            }
           
            return mealPlanRequests;
        }

        
        /// <summary>
        /// Permissions code that allows an external system to do a READ operation. This API will integrate information related to meal plan assignments that 
        /// could be deemed personal.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewMealPlanRequestPermission()
        {
            var hasPermission = HasPermission(StudentPermissionCodes.ViewMealPlanRequest);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view MEAL.PLAN.REQS.INTG.");
            }
        }

        /// <summary>
        /// Permissions code that allows an external system to do a UPDATE operation. This API will integrate information related to meal plan assignments that 
        /// could be deemed personal.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckCreateMealPlanRequestPermission()
        {
            var hasPermission = HasPermission(StudentPermissionCodes.CreateMealPlanRequest);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to update MEAL.PLAN.REQS.INTG.");
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a meal assignment status to its corresponding MealPlanRequestsStatus DTO enumeration value
        /// </summary>
        /// <param name="source">meal assignment status</param>
        /// <returns>MealPlanRequestsStatus DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.EnumProperties.MealPlanRequestsStatus ConvertStatusToMealPlanRequestsStatusDtoEnum(string source)
        {
            switch (source)
            {

                case "A":
                    return Dtos.EnumProperties.MealPlanRequestsStatus.Approved;
                case "R":
                    return Dtos.EnumProperties.MealPlanRequestsStatus.Rejected;
                case "S":
                    return Dtos.EnumProperties.MealPlanRequestsStatus.Submitted;
                case "W":
                    return Dtos.EnumProperties.MealPlanRequestsStatus.Withdrawn;
                default:
                    return Dtos.EnumProperties.MealPlanRequestsStatus.NotSet;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a MealPlanRequestsStatus DTO enumeration value to its corresponding meal assignment status 
        /// </summary>
        /// <param name="source">MealPlanRequestsStatus DTO enumeration value</param>
        /// <returns> meal assignment status</returns>
        private string ConvertMealPlanRequestsStatusDtoEnumToStatus(Dtos.EnumProperties.MealPlanRequestsStatus? source)
        {
            switch (source)
            {

                case Dtos.EnumProperties.MealPlanRequestsStatus.Approved:
                    return "A";
                case Dtos.EnumProperties.MealPlanRequestsStatus.Rejected:
                    return "R";
                case Dtos.EnumProperties.MealPlanRequestsStatus.Submitted:
                    return "S";
                case Dtos.EnumProperties.MealPlanRequestsStatus.Withdrawn:
                    return "W";
                default:
                    return string.Empty;
            }
        }
    }
}