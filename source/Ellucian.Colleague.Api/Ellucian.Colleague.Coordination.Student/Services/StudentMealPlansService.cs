//Copyright 2017-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        #region 16.0.0 

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all student-meal-plans
        /// </summary>
        /// <param name="offset">Paging offset</param>
        /// <param name="limit">Paging limit</param>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will re-query cached items</param>
        /// <returns>List of <see cref="Dtos.StudentMealPlans2">Dtos.StudentMealPlans</see></returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentMealPlans2>, int>> GetStudentMealPlans2Async(int offset, int limit, StudentMealPlans2 criteriaFilter, bool bypassCache = false)
        {
          
            string person = string.Empty, term = string.Empty, mealplan = string.Empty, status = string.Empty, startDate = string.Empty, endDate = string.Empty;

            if (criteriaFilter != null)
            {
                //process person guid filter
                var personGuid = criteriaFilter.Person != null ? criteriaFilter.Person.Id : string.Empty;
                if (!string.IsNullOrEmpty(personGuid))
                {
                    try
                    {
                        person = await _personRepository.GetPersonIdFromGuidAsync(personGuid);
                        if(string.IsNullOrEmpty(person))
                        {
                            return new Tuple<IEnumerable<Dtos.StudentMealPlans2>, int>( new List<Dtos.StudentMealPlans2>(), 0 );
                        }
                    }
                    catch (Exception)
                    {
                        return new Tuple<IEnumerable<Dtos.StudentMealPlans2>, int>(new List<Dtos.StudentMealPlans2>(), 0);
                    }
                }
                //process mealplan guid filter
                var mealplanGuid = criteriaFilter.MealPlan != null ? criteriaFilter.MealPlan.Id : string.Empty;
                if (!string.IsNullOrEmpty(mealplanGuid))
                {
                    try
                    {
                        var mealPlans = await GetMealPlansAsync(false);
                        if (mealPlans == null)
                        {
                            return new Tuple<IEnumerable<Dtos.StudentMealPlans2>, int>(new List<Dtos.StudentMealPlans2>(), 0);
                        }
                        var mealPlanEntity = mealPlans.FirstOrDefault( mp => mp.Guid.Equals( mealplanGuid, StringComparison.InvariantCultureIgnoreCase ) );
                        if (mealPlanEntity == null)
                        {
                            return new Tuple<IEnumerable<Dtos.StudentMealPlans2>, int>(new List<Dtos.StudentMealPlans2>(), 0);
                        }
                        mealplan = mealPlanEntity.Code;
                    }
                    catch (Exception)
                    {
                        return new Tuple<IEnumerable<Dtos.StudentMealPlans2>, int>(new List<Dtos.StudentMealPlans2>(), 0);
                    }
                }
                //process academicPeriod guid filter
                var academicPeriodGuid = criteriaFilter.AcademicPeriod != null ? criteriaFilter.AcademicPeriod.Id : string.Empty;
                if (!string.IsNullOrEmpty(academicPeriodGuid))
                {
                    try
                    {
                        var academicPeriods = await GetAcademicPeriodsAsync(false);
                        if (academicPeriods == null)
                        {
                            return new Tuple<IEnumerable<Dtos.StudentMealPlans2>, int>(new List<Dtos.StudentMealPlans2>(), 0);
                        }
                        var academicPeriodEntity = academicPeriods.FirstOrDefault( mp => mp.Guid.Equals( academicPeriodGuid, StringComparison.InvariantCultureIgnoreCase ) );
                        if (academicPeriodEntity == null || string.IsNullOrEmpty( academicPeriodEntity.Code ))
                        {
                            return new Tuple<IEnumerable<Dtos.StudentMealPlans2>, int>(new List<Dtos.StudentMealPlans2>(), 0);
                        }
                        term = academicPeriodEntity.Code;
                    }
                    catch (Exception)
                    {
                        return new Tuple<IEnumerable<Dtos.StudentMealPlans2>, int>(new List<Dtos.StudentMealPlans2>(), 0);
                    }
                }
                //process status filter
                status = ConvertStudentMealPlansStatusDtoEnumToStatus(criteriaFilter.Status);
                //process startOn and endOn filter
                try
                {
                    startDate = criteriaFilter.StartOn.HasValue ? await ConvertDateArgument(criteriaFilter.StartOn.ToString()) : string.Empty;
                    endDate = criteriaFilter.EndOn.HasValue ? await ConvertDateArgument(criteriaFilter.EndOn.ToString()) : string.Empty;
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<Dtos.StudentMealPlans2>, int>(new List<Dtos.StudentMealPlans2>(), 0);
                }
            }
            try
            {
                var mealPlanAssignmentEntitiesTuple = await _studentMealPlanRepository.GetAsync(offset, limit, person, term, mealplan, status, startDate, endDate);
                if (mealPlanAssignmentEntitiesTuple != null)
                {
                    var mealPlanAssignmentEntities = mealPlanAssignmentEntitiesTuple.Item1;
                    var totalRecords = mealPlanAssignmentEntitiesTuple.Item2;
                    if (mealPlanAssignmentEntities != null && mealPlanAssignmentEntities.Any())
                    {
                        return new Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentMealPlans2>, int>(await ConvertMealPlanAssignmentEntityToDto2Async(mealPlanAssignmentEntities.ToList(), bypassCache), totalRecords);
                    }
                    else
                    {
                        return new Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentMealPlans2>, int>(new List<Ellucian.Colleague.Dtos.StudentMealPlans2>(), 0);
                    }
                }
                else
                    return new Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentMealPlans2>, int>(new List<Ellucian.Colleague.Dtos.StudentMealPlans2>(), 0);
            }
            catch(IntegrationApiException)
            {
                throw;
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(e.Message);
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a StudentMealPlans from its GUID
        /// </summary>
        /// <param name="guid">Guid</param>
        /// <returns><see cref="Dtos.StudentMealPlans2">Dtos.StudentMealPlans object</see></returns>
        public async Task<Ellucian.Colleague.Dtos.StudentMealPlans2> GetStudentMealPlansByGuid2Async(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "A GUID is required to obtain an student-meal-plan.");
            }
           
            try
            {
                var stuMealPlan = new List<Dtos.StudentMealPlans2>();
                var stuMealPlanEntity = await _studentMealPlanRepository.GetByIdAsync(guid);
                if ( stuMealPlanEntity == null )
                    throw new KeyNotFoundException( string.Format( "No meal plan assignment was found for guid '{0}'.", guid ) );
                var stuMealPlanDto = await ConvertMealPlanAssignmentEntityToDto2Async(new List<MealPlanAssignment>() { stuMealPlanEntity });
                if ( stuMealPlanDto == null || !stuMealPlanDto.Any() )
                    throw new KeyNotFoundException( string.Format( "No meal plan assignment was found for guid '{0}'.", guid ) );
                return stuMealPlanDto.FirstOrDefault();
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException( string.Format( "No meal plan assignment was found for guid '{0}'.", guid, ex ) );
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Put (Update) an Student Meal Plan domain entity
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="studentMealPlansDto2"><see cref="Dtos.StudentMealPlans2">StudentMealPlans</see></param>
        /// <returns><see cref="Dtos.StudentMealPlans2">StudentMealPlans</see></returns>
        public async Task<StudentMealPlans2> PutStudentMealPlans2Async(string guid, Dtos.StudentMealPlans2 studentMealPlansDto2)
        {
            string recordKey = string.Empty;
            try
            {
                if( studentMealPlansDto2 == null )
                    throw new ArgumentNullException( "MealPlanAssignments", "Must provide a meal plan assignment request body for update" );
                if( string.IsNullOrEmpty( studentMealPlansDto2.Id ) )
                    throw new ArgumentNullException( "MealPlanAssignments", "Must provide a guid for meal plan assignment update" );

              
                _studentMealPlanRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
                try
                {
                    recordKey = await _studentMealPlanRepository.GetMealPlanAssignmentIdFromGuidAsync( guid );
                }
                catch( Exception e )
                {
                    IntegrationApiExceptionAddError( e.Message, "Validation.Exception", guid, recordKey );
                    throw IntegrationApiException;
                }

                // map the DTO to entities
                var StudentMealPlansEntity
                    = await ConvertStudentMealPlansDtoToEntity2Async( studentMealPlansDto2, recordKey );

                if( IntegrationApiException != null )
                {
                    throw IntegrationApiException;
                }

                // update the entity in the database
                var updatedStudentMealPlansEntity =
                    await _studentMealPlanRepository.UpdateMealPlanAssignmentAsync( StudentMealPlansEntity );
                var dtoStudentMealPlans = await ConvertMealPlanAssignmentEntityToDto2Async( new List<MealPlanAssignment>() { updatedStudentMealPlansEntity }, true );


                // return the newly updated DTO
                return dtoStudentMealPlans.FirstOrDefault();

            }
            catch( IntegrationApiException )
            {
                throw;
            }
            catch( RepositoryException )
            {
                throw;
            }
            catch( Exception e )
            {
                IntegrationApiExceptionAddError( e.Message, "Validation.Exception", guid, recordKey );
                throw IntegrationApiException;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Post (Create) an Student meal plan doamin entity
        /// </summary>
        /// <param name="studentMealPlansDto2"><see cref="Dtos.StudentMealPlans2">StudentMealPlans</see></param>
        /// <returns><see cref="Dtos.StudentMealPlans2">StudentMealPlans</see></returns>
        public async Task<StudentMealPlans2> PostStudentMealPlans2Async(Dtos.StudentMealPlans2 studentMealPlansDto2)
        {
            try
            {
                if (studentMealPlansDto2 == null)
                    throw new ArgumentNullException("MealPlanAssignments", "Must provide a meal plan assignemnts request body to create a meal plan assignment. ");
                if (string.IsNullOrEmpty(studentMealPlansDto2.Id) || !string.Equals(studentMealPlansDto2.Id, Guid.Empty.ToString()))
                    throw new ArgumentNullException("MealPlanAssignments", "Must provide a nil guid to create a meal plan assignment. ");
                Ellucian.Colleague.Domain.Student.Entities.MealPlanAssignment createdStudentMealPlans = null;

                _studentMealPlanRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                // map the DTO to entities
                var StudentMealPlansEntity
                    = await ConvertStudentMealPlansDtoToEntity2Async(studentMealPlansDto2);
                
                if( IntegrationApiException != null )
                {
                    throw IntegrationApiException;
                }

                // create the entity in the database
                createdStudentMealPlans = await _studentMealPlanRepository.CreateMealPlanAssignmentAsync(StudentMealPlansEntity);
                var dtoStudentMealPlans = await ConvertMealPlanAssignmentEntityToDto2Async(new List<MealPlanAssignment>() { createdStudentMealPlans }, true);

                // return the newly updated DTO
                return dtoStudentMealPlans.FirstOrDefault();

            }

            catch( IntegrationApiException )
            {
                throw;
            }
            catch( RepositoryException )
            {
                throw;
            }
            catch( Exception e )
            {
                IntegrationApiExceptionAddError( e.Message, "Validation.Exception", string.Empty, string.Empty );
                throw IntegrationApiException;
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a MealPlanAssignment domain entity to its corresponding StudentMealPlans DTO
        /// </summary>
        /// <param name="source">MealPlanAssignment domain entity</param>
        /// <returns>StudentMealPlans DTO</returns>
        private async Task<IEnumerable<Ellucian.Colleague.Dtos.StudentMealPlans2>> ConvertMealPlanAssignmentEntityToDto2Async(List<MealPlanAssignment> sources, bool bypassCache = false)
        {
            var studentMealPlans = new List<Ellucian.Colleague.Dtos.StudentMealPlans2>();
            //get person ids and get the associated Guids
            var ids = new List<string>();
            Dictionary<string, string> personGuidCollection = null;
            List<Domain.Student.Entities.MealPlanRates> mealPlanRates = new List<Domain.Student.Entities.MealPlanRates>();
            try
            {
                ids.AddRange( sources.Where( p => p != null && !string.IsNullOrEmpty( p.PersonId ) )
                    .Select( p => p.PersonId ).Distinct().ToList() );
                personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync( ids );
            }
            catch( ColleagueDataReaderException e )
            {
                IntegrationApiExceptionAddError( e.Message, "Bad.Data" );
            }

            foreach (var source in sources)
            {
                var studentMealPlan = new Ellucian.Colleague.Dtos.StudentMealPlans2();
                //get term guid
                if (!string.IsNullOrEmpty(source.Term))
                {
                    string term = string.Empty;
                    try
                    {
                        term = await _termRepository.GetAcademicPeriodsGuidAsync( source.Term );
                        if( string.IsNullOrEmpty( term ) )
                        {
                            IntegrationApiExceptionAddError( string.Format( "Academic period guid not found for meal plan assignment Id: '{0}'.", source.Term ), "GUID.Not.Found", source.Guid, source.Id );
                        }
                        else
                        {
                            studentMealPlan.AcademicPeriod = new Dtos.GuidObject2( term );
                        }
                    }
                    catch( RepositoryException )
                    {
                        IntegrationApiExceptionAddError( string.Format( "Academic period guid not found for term Id: '{0}'.", source.Term ), "GUID.Not.Found", source.Guid, source.Id );
                    }
                }

                if ((source.OverrideRate != null) && (source.OverrideRate.HasValue) && (source.OverrideRate > 0))
                {
                    studentMealPlan.OverrideRate = await BuildAssignedOverride2Async(source, bypassCache);
                }

                if (source.NoRatePeriods != null) studentMealPlan.NumberOfPeriods = source.NoRatePeriods;

                if (!string.IsNullOrEmpty(source.MealPlan))
                {
                    var startDate = Convert.ToDateTime(source.StartDate);
                    
                    try
                    {
                        mealPlanRates = (await GetMealPlanRatesAsync( bypassCache )).ToList();

                        if( mealPlanRates == null || !mealPlanRates.Any() )
                        {
                            IntegrationApiExceptionAddError( "Meal plan rates not found.", "Validation.Exception" );

                        }
                        else
                        {
                            var mealPlanRate = mealPlanRates.Where( mpr => mpr != null && !string.IsNullOrEmpty( mpr.Code ) && mpr.Code.Equals( source.MealPlan, StringComparison.InvariantCultureIgnoreCase )
                                                && mpr.MealPlansMealPlanRates != null )
                                .OrderByDescending( mpr => mpr.MealPlansMealPlanRates.EffectiveDates );

                            if( mealPlanRate != null )
                            {
                                var mealPlanGuid = string.Empty;
                                foreach( var mealPlan in mealPlanRate )
                                {
                                    if( ( mealPlan.MealPlansMealPlanRates != null ) && ( DateTime.Compare( startDate, Convert.ToDateTime( mealPlan.MealPlansMealPlanRates.EffectiveDates ) ) >= 0 ) )
                                    {
                                        mealPlanGuid = mealPlan.Guid;
                                        break;
                                    }
                                }

                                if( !string.IsNullOrEmpty( mealPlanGuid ) )
                                {
                                    studentMealPlan.AssignedRates = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2( mealPlanGuid ) };
                                }
                            }
                        }
                    }
                    catch(ArgumentNullException e)
                    {
                        IntegrationApiExceptionAddError( e.Message, "Bad.Data", source.Guid, source.Id );
                    }
                    catch(RepositoryException e)
                    {
                        IntegrationApiExceptionAddError( e, "Bad.Data", source.Guid, source.Id );
                    }
                }
                if (source.EndDate != null)
                {
                    studentMealPlan.EndOn = Convert.ToDateTime(source.EndDate);
                }

                if(string.IsNullOrEmpty(source.Guid))
                {
                    IntegrationApiExceptionAddError( string.Format( "Meal plan assignment guid is missing Id: '{0}'.", source.Id ), "GUID.Not.Found", source.Guid, source.Id );
                }
                studentMealPlan.Id = source.Guid;

                //get mealplan guid
                if (!string.IsNullOrEmpty(source.MealPlan))
                {
                    try
                    {
                        var mealplanGuid = await _referenceDataRepository.GetMealPlanGuidAsync( source.MealPlan );
                        if( !string.IsNullOrEmpty( mealplanGuid ) )
                            studentMealPlan.MealPlan = new Dtos.GuidObject2( mealplanGuid );
                    }
                    catch( RepositoryException e )
                    {
                        IntegrationApiExceptionAddError( e, "Bad.Data", source.Guid, source.Id );
                    }
                }
                //get person guid
                if (!string.IsNullOrEmpty(source.PersonId))
                {
                    var studentGuid = string.Empty;
                    if (!personGuidCollection.TryGetValue(source.PersonId, out studentGuid))
                    {
                        IntegrationApiExceptionAddError( string.Format( "Person guid not found for person Id: '{0}'.", source.PersonId ), "GUID.Not.Found", source.Guid, source.Id );
                    }
                    else
                    {
                        studentMealPlan.Person = new Dtos.GuidObject2(studentGuid);
                    }
                }

                studentMealPlan.StartOn = Convert.ToDateTime(source.StartDate);

                if (!string.IsNullOrEmpty(source.Status))
                {
                    studentMealPlan.Status = ConvertStatusToStudentMealPlansStatusDtoEnum(source.Status);
                }

                if (source.StatusDate.HasValue)
                {
                    studentMealPlan.StatusDate = Convert.ToDateTime(source.StatusDate);
                }

                if (!string.IsNullOrEmpty(source.MealCard)) studentMealPlan.MealCard = source.MealCard;

                if (!string.IsNullOrEmpty(source.MealComments)) studentMealPlan.Comment = source.MealComments;

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
                    studentMealPlan.Consumption = studentMealPlansConsumption;
                }
                studentMealPlans.Add(studentMealPlan);
            }

            if( IntegrationApiException != null )
            {
                throw IntegrationApiException;
            }

            return studentMealPlans;
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a StudentMealPlans DTO to its corresponding MealPlanAssignment domain entity  
        /// </summary>
        /// <param name="source">StudentMealPlans DTO</param>
        /// <returns>MealPlanAssignment</returns>
        private async Task<MealPlanAssignment> ConvertStudentMealPlansDtoToEntity2Async(Ellucian.Colleague.Dtos.StudentMealPlans2 source, string recordKey = "", bool bypassCache = false)
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
            MealPlanAssignment studentMealPlans = null;
            
            if (!string.Equals(source.Id, Guid.Empty.ToString()))
            {
                guid = source.Id;
            }
            //get person ID
            if (source.Person != null && !string.IsNullOrEmpty(source.Person.Id))
            {
                try
                {
                    personId = await _personRepository.GetPersonIdFromGuidAsync( source.Person.Id );
                    if( string.IsNullOrEmpty( personId ) )
                    {
                        IntegrationApiExceptionAddError( string.Format( "Person ID '{0}' was not found. Valid Person is required.", source.Person.Id ), "Validation.Exception", guid, recordKey );
                    }
                }
                catch(Exception e)
                {
                    IntegrationApiExceptionAddError( e.Message, "Validation.Exception", guid, recordKey );
                }
            }
            else
            {
                IntegrationApiExceptionAddError( "Person is required.", "Missing.Request.Property", guid, recordKey );
            }
            //get mealplan ID
            if (source.MealPlan != null && !string.IsNullOrEmpty(source.MealPlan.Id))
            {
                var mealPlans = await GetMealPlansAsync(bypassCache);
                if (mealPlans == null)
                {
                    IntegrationApiExceptionAddError("Unable to retrieve meal plans.", "Validation.Exception", guid, recordKey);
                }
                else
                {
                    var mealPlan = mealPlans.FirstOrDefault(mp => mp != null && mp.Guid.Equals(source.MealPlan.Id, StringComparison.InvariantCultureIgnoreCase));
                    if (mealPlan == null)
                    {
                        IntegrationApiExceptionAddError(string.Format("Meal Plan ID '{0}' was not found. Valid Meal Plan is required.", source.MealPlan.Id), "Missing.Request.ID", guid, recordKey);
                    }
                    else
                    {
                        mealplanId = mealPlan.Code;
                    }
                }

            }
            else
            {
                IntegrationApiExceptionAddError( "MealPlan is required.", "Missing.Request.Property", guid, recordKey );
            }
            //get status code
            if (source.Status == null || source.Status == Dtos.EnumProperties.StudentMealPlansStatus.NotSet)
            {
                IntegrationApiExceptionAddError( "Invalid Status. Status is required.", "Missing.Request.Property", guid, recordKey );
            }
            else
            {
                var statusCode = ConvertStudentMealPlansStatusDtoEnumToStatus(source.Status);
                if (string.IsNullOrEmpty(statusCode))
                {
                    IntegrationApiExceptionAddError( string.Format( "Status '{0}' was not found. Valid Status is required.", source.Status ), "Validation.Exception", guid, recordKey );
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
                IntegrationApiExceptionAddError( "Start Date is required.", "Missing.Request.Property", guid, recordKey );
            }
            else
            {
                startDate = Convert.ToDateTime(source.StartOn.Value.ToString("yyyy-MM-dd"));
            }
            //get status date
            if (source.StatusDate == null || source.StatusDate == DateTime.MinValue)
            {
                IntegrationApiExceptionAddError( "Status Date is required.", "Missing.Request.Property", guid, recordKey );
            }
            else
            {
                statusDate = Convert.ToDateTime(source.StatusDate.Value.ToString("yyyy-MM-dd"));
            }

            //now we can create the entity
            try
            {
                studentMealPlans = new MealPlanAssignment( guid, id, personId, mealplanId, startDate, NoPeriods, status, statusDate );
            }
            catch(Exception e)
            {
                IntegrationApiExceptionAddError( e.Message, "Missing.Required.Property", guid, recordKey );
                throw IntegrationApiException;
            }

            //get the term
            if (source.AcademicPeriod != null && !string.IsNullOrEmpty(source.AcademicPeriod.Id))
            {
                var academicPeriodEntities = await GetAcademicPeriodsAsync(bypassCache);

                if (academicPeriodEntities == null)
                {
                    IntegrationApiExceptionAddError("Unable to retrieve academic periods.", "Validation.Exception", guid, recordKey);
                }
                else
                {
                    var term = academicPeriodEntities.FirstOrDefault(mp => mp.Guid == source.AcademicPeriod.Id);
                    if (term == null)
                    {
                        IntegrationApiExceptionAddError(string.Format("Academic Period '{0}' was not found.", source.AcademicPeriod.Id), "Missing.Request.ID", guid, recordKey);
                    }
                    else
                    {
                        studentMealPlans.Term = term.Code;
                    }
                }
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
            if (source.Consumption != null)
            {
                if (source.Consumption.Units != null)
                {
                    studentMealPlans.UsedRatePeriods = source.Consumption.Units;
                }

                if (source.Consumption.Percent != null)
                {
                    studentMealPlans.PercentUsed = source.Consumption.Percent;
                }
            }
            //The rate period associated with the assignment cannot be overridden from the rate period set on the meal plan rate. On a PUT/POST we will ignore and respond with the ratePeriod of the meal plan.
                if (source.AssignedRates != null && source.AssignedRates.Any())
            {
                if(source.AssignedRates.Count() > 1)
                {
                    IntegrationApiExceptionAddError( "Only a single assigned rate is permitted.", "Validation.Exception", guid, recordKey );
                }
                var defaultRate = source.AssignedRates.FirstOrDefault();
                DateTime? effectiveDate = new DateTime();
                if (defaultRate != null)
                {
                    var mealPlanRates = await GetMealPlanRatesAsync(bypassCache);
                    if (mealPlanRates == null || !mealPlanRates.Any())
                    {
                        IntegrationApiExceptionAddError( "Unable to retrieve meal plan rates.", "Validation.Exception", guid, recordKey );
                    }
                    var mealPlanRate = mealPlanRates.FirstOrDefault( mpr => mpr != null && mpr.Guid.Equals( defaultRate.Id, StringComparison.InvariantCultureIgnoreCase ) );
                    if (mealPlanRate == null)
                    {
                        IntegrationApiExceptionAddError( string.Format( "Meal Plan rate '{0}' was not found.", defaultRate.Id ), "Missing.Request.ID", guid, recordKey );
                    }
                    else
                    {
                        if (mealPlanRate.Code != mealplanId)
                        {
                            IntegrationApiExceptionAddError( string.Format( "Meal plan '{0}' from meal plan rate does not match assignment's meal plan '{1}'.", mealPlanRate.Code, mealplanId ), 
                                "Missing.Request.ID", guid, recordKey );
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
                            IntegrationApiExceptionAddError( string.Format( "Invalid meal plan rate '{0}' with effective date '{1}' for start date of '{2}'.", defaultRate.Id, effectiveDate, startDate ),
                                "Validation.Exception", guid, recordKey );
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
                            IntegrationApiExceptionAddError("Unable to retrieve accounting codes.", "Validation.Exception", guid, recordKey);
                        }
                        else
                        {
                            var accountingCode = accountingCodeEntities.FirstOrDefault(mpr => mpr != null && mpr.Guid.Equals(source.OverrideRate.AccountingCode.Id, StringComparison.InvariantCultureIgnoreCase));
                            if (accountingCode == null)
                            {
                                IntegrationApiExceptionAddError(string.Format("Accounting Code '{0}' was not found.", source.OverrideRate.AccountingCode.Id), "Missing.Request.ID", guid, recordKey);
                            }
                            else
                            {
                                studentMealPlans.OverrideArCode = accountingCode.Code;
                            }
                        }
                    }
                    //get override reason
                    if (source.OverrideRate.OverrideReason != null)
                    {
                        var billingOverrideReasonsEntities = await GetBillingOverrideReasonsAsync(bypassCache);
                        if (billingOverrideReasonsEntities == null)
                        {
                            IntegrationApiExceptionAddError("Unable to retrieve billing override reasons.", "Validation.Exception", guid, recordKey);
                        }
                        else
                        {
                            var billingOverrideReason = billingOverrideReasonsEntities.FirstOrDefault(mpr => mpr != null && mpr.Guid.Equals(source.OverrideRate.OverrideReason.Id, StringComparison.InvariantCultureIgnoreCase));
                            if (billingOverrideReason == null)
                            {
                                IntegrationApiExceptionAddError(string.Format("Override Reason '{0}' was not found.", source.OverrideRate.OverrideReason.Id), "Missing.Request.ID", guid, recordKey);
                            }
                            else
                            {
                                studentMealPlans.RateOverrideReason = billingOverrideReason.Code;
                            }
                        }
                    }
                    //get rate value
                    if (source.OverrideRate.Rate != null)
                    {
                        if (source.OverrideRate.Rate.Value != null && source.OverrideRate.Rate.Currency != Dtos.EnumProperties.CurrencyIsoCode.USD && source.OverrideRate.Rate.Currency != Dtos.EnumProperties.CurrencyIsoCode.CAD)
                        {
                            IntegrationApiExceptionAddError( "The override rate currency must be set to either 'USD' or 'CAD'.", "Validation.Exception", guid, recordKey );
                        }
                        if (source.OverrideRate.Rate.Value < 0)
                        {
                            IntegrationApiExceptionAddError( "The override rate amount must be set greater than zero.", "Validation.Exception", guid, recordKey );
                        }
                        studentMealPlans.OverrideRate = source.OverrideRate.Rate.Value;
                    }
                }
                catch (Exception e)
                {
                    IntegrationApiExceptionAddError( e.Message, "Global.Internal.Error", guid, recordKey );
                }
            }

            return studentMealPlans;
        }

        #endregion


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all student-meal-plans
        /// </summary>
        /// <param name="offset">Paging offset</param>
        /// <param name="limit">Paging limit</param>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will re-query cached items</param>
        /// <returns>List of <see cref="Dtos.StudentMealPlans2">Dtos.StudentMealPlans</see></returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentMealPlans>, int>> GetStudentMealPlansAsync(int offset, int limit, StudentMealPlans criteriaFilter, bool bypassCache = false)
        {
            
            string person = string.Empty, term = string.Empty, mealplan = string.Empty, status = string.Empty, startDate = string.Empty, endDate = string.Empty;

            if (criteriaFilter != null)
            {
                //process person guid filter
                var personGuid = criteriaFilter.Person != null ? criteriaFilter.Person.Id : string.Empty;
                if (!string.IsNullOrEmpty(personGuid))
                {
                    try
                    {
                        person = await _personRepository.GetPersonIdFromGuidAsync(personGuid);
                    }
                    catch (Exception)
                    {
                        return new Tuple<IEnumerable<Dtos.StudentMealPlans>, int>(new List<Dtos.StudentMealPlans>(), 0);
                    }
                }
                //process mealplan guid filter
                var mealplanGuid = criteriaFilter.MealPlan != null ? criteriaFilter.MealPlan.Id : string.Empty;
                if (!string.IsNullOrEmpty(mealplanGuid))
                {
                    try
                    {
                        var mealPlans = await GetMealPlansAsync(false);
                        if (mealPlans == null)
                        {
                            return new Tuple<IEnumerable<Dtos.StudentMealPlans>, int>(new List<Dtos.StudentMealPlans>(), 0);
                        }
                        var mealPlanEntity = mealPlans.FirstOrDefault(mp => mp.Guid == mealplanGuid);
                        if (mealPlanEntity == null)
                        {
                            return new Tuple<IEnumerable<Dtos.StudentMealPlans>, int>(new List<Dtos.StudentMealPlans>(), 0);
                        }
                        mealplan = mealPlanEntity.Code;
                    }
                    catch (Exception)
                    {
                        return new Tuple<IEnumerable<Dtos.StudentMealPlans>, int>(new List<Dtos.StudentMealPlans>(), 0);
                    }
                }
                //process academicPeriod guid filter
                var academicPeriodGuid = criteriaFilter.AcademicPeriod != null ? criteriaFilter.AcademicPeriod.Id : string.Empty;
                if (!string.IsNullOrEmpty(academicPeriodGuid))
                {
                    try
                    {
                        var academicPeriods = await GetAcademicPeriodsAsync(false);
                        if (academicPeriods == null)
                        {
                            return new Tuple<IEnumerable<Dtos.StudentMealPlans>, int>(new List<Dtos.StudentMealPlans>(), 0);
                        }
                        var academicPeriodEntity = academicPeriods.FirstOrDefault(mp => mp.Guid == academicPeriodGuid);
                        if (academicPeriodEntity == null)
                        {
                            return new Tuple<IEnumerable<Dtos.StudentMealPlans>, int>(new List<Dtos.StudentMealPlans>(), 0);
                        }
                        term = academicPeriodEntity.Code;
                    }
                    catch (Exception)
                    {
                        return new Tuple<IEnumerable<Dtos.StudentMealPlans>, int>(new List<Dtos.StudentMealPlans>(), 0);
                    }
                }
                //process status filter
                status = ConvertStudentMealPlansStatusDtoEnumToStatus(criteriaFilter.Status);
                //process startOn and endOn filter
                try
                {
                    startDate = criteriaFilter.StartOn.HasValue ? await ConvertDateArgument(criteriaFilter.StartOn.ToString()) : string.Empty;
                    endDate = criteriaFilter.EndOn.HasValue ? await ConvertDateArgument(criteriaFilter.EndOn.ToString()) : string.Empty;
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<Dtos.StudentMealPlans>, int>(new List<Dtos.StudentMealPlans>(), 0);
                }
            }
            try
            {
                var mealPlanAssignmentEntitiesTuple = await _studentMealPlanRepository.GetAsync(offset, limit, person, term, mealplan, status, startDate, endDate);
                if (mealPlanAssignmentEntitiesTuple != null)
                {
                    var mealPlanAssignmentEntities = mealPlanAssignmentEntitiesTuple.Item1;
                    var totalRecords = mealPlanAssignmentEntitiesTuple.Item2;
                    if (mealPlanAssignmentEntities != null && mealPlanAssignmentEntities.Any())
                        return new Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentMealPlans>, int>(await ConvertMealPlanAssignmentEntityToDtoAsync(mealPlanAssignmentEntities.ToList(), bypassCache), totalRecords);
                    else
                        return new Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentMealPlans>, int>(new List<Ellucian.Colleague.Dtos.StudentMealPlans>(), 0);
                }
                else
                    return new Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentMealPlans>, int>(new List<Ellucian.Colleague.Dtos.StudentMealPlans>(), 0);
            }
            catch (RepositoryException e)
            {
                throw e;
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(e.Message);
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a StudentMealPlans from its GUID
        /// </summary>
        /// <param name="guid">Guid</param>
        /// <returns><see cref="Dtos.StudentMealPlans2">Dtos.StudentMealPlans object</see></returns>
        public async Task<Ellucian.Colleague.Dtos.StudentMealPlans> GetStudentMealPlansByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "A GUID is required to obtain an student-meal-plan.");
            }
            
            try
            {
                var stuMealPlan = new List<Dtos.StudentMealPlans>();
                var stuMealPlanEntity = await _studentMealPlanRepository.GetByIdAsync(guid);
                if (stuMealPlanEntity == null )
                    throw new KeyNotFoundException("meal-plan-assignments not found for GUID " + guid);
                var stuMealPlanDto = await ConvertMealPlanAssignmentEntityToDtoAsync(new List<MealPlanAssignment>(){ stuMealPlanEntity});
                if (stuMealPlanDto == null)
                    throw new KeyNotFoundException("meal-plan-assignments not found for GUID " + guid);
                return stuMealPlanDto.FirstOrDefault();
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
        /// <param name="StudentMealPlansDto"><see cref="Dtos.StudentMealPlans2">StudentMealPlans</see></param>
        /// <returns><see cref="Dtos.StudentMealPlans2">StudentMealPlans</see></returns>
        public async Task<StudentMealPlans> PutStudentMealPlansAsync(string guid, Dtos.StudentMealPlans StudentMealPlansDto)
        {
            try
            {
                if (StudentMealPlansDto == null)
                    throw new ArgumentNullException("MealPlanAssignments", "Must provide a meal plan assignment request body for update");
                if (string.IsNullOrEmpty(StudentMealPlansDto.Id))
                    throw new ArgumentNullException("MealPlanAssignments", "Must provide a guid for meal plan assignment update");

                _studentMealPlanRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                // map the DTO to entities
                var StudentMealPlansEntity
                    = await ConvertStudentMealPlansDtoToEntityAsync(StudentMealPlansDto);

                // update the entity in the database
                var updatedStudentMealPlansEntity =
                    await _studentMealPlanRepository.UpdateMealPlanAssignmentAsync(StudentMealPlansEntity);
                var dtoStudentMealPlans = await ConvertMealPlanAssignmentEntityToDtoAsync(new List<MealPlanAssignment>() { updatedStudentMealPlansEntity }, true);


                // return the newly updated DTO
                return dtoStudentMealPlans.FirstOrDefault();

            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException(ex.Message, ex.InnerException);
            }


        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Post (Create) an Student meal plan doamin entity
        /// </summary>
        /// <param name="StudentMealPlansDto"><see cref="Dtos.StudentMealPlans2">StudentMealPlans</see></param>
        /// <returns><see cref="Dtos.StudentMealPlans2">StudentMealPlans</see></returns>
        public async Task<StudentMealPlans> PostStudentMealPlansAsync(Dtos.StudentMealPlans StudentMealPlansDto)
        {
            try
            {
                if (StudentMealPlansDto == null)
                    throw new ArgumentNullException("MealPlanAssignments", "Must provide a meal plan assignemnts request body to create a meal plan assignment. ");
                if (string.IsNullOrEmpty(StudentMealPlansDto.Id) || !string.Equals(StudentMealPlansDto.Id, Guid.Empty.ToString()))
                    throw new ArgumentNullException("MealPlanAssignments", "Must provide a nil guid to create a meal plan assignment. ");
                Ellucian.Colleague.Domain.Student.Entities.MealPlanAssignment createdStudentMealPlans = null;

                _studentMealPlanRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                // map the DTO to entities
                var StudentMealPlansEntity
                    = await ConvertStudentMealPlansDtoToEntityAsync(StudentMealPlansDto);

                // create the entity in the database
                createdStudentMealPlans = await _studentMealPlanRepository.CreateMealPlanAssignmentAsync(StudentMealPlansEntity);
                var dtoStudentMealPlans = await ConvertMealPlanAssignmentEntityToDtoAsync(new List<MealPlanAssignment>() { createdStudentMealPlans }, true);

                // return the newly updated DTO
                return dtoStudentMealPlans.FirstOrDefault();

            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException(ex.Message, ex.InnerException);
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
        private async Task<IEnumerable<Ellucian.Colleague.Dtos.StudentMealPlans>> ConvertMealPlanAssignmentEntityToDtoAsync(List<MealPlanAssignment> sources, bool bypassCache = false)
        {
            var studentMealPlans = new List<Ellucian.Colleague.Dtos.StudentMealPlans>();
            //get person ids and get the associated Guids
            var ids = new List<string>();

            ids.AddRange(sources.Where(p => (!string.IsNullOrEmpty(p.PersonId)))
                .Select(p => p.PersonId).Distinct().ToList());
            var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(ids);

            foreach (var source in sources)
            {
                var studentMealPlan = new Ellucian.Colleague.Dtos.StudentMealPlans();
                //get term guid
                if (!string.IsNullOrEmpty(source.Term))
                {
                    try
                    {
                        var term = await _termRepository.GetAcademicPeriodsGuidAsync(source.Term);
                        if (!string.IsNullOrEmpty(term))
                            studentMealPlan.AcademicPeriod = new Dtos.GuidObject2(term);
                    }
                    catch (RepositoryException ex)
                    {
                        throw new ColleagueWebApiException(ex.Message);
                    }
                }

                if ((source.OverrideRate != null) && (source.OverrideRate.HasValue) && (source.OverrideRate > 0))
                {
                    studentMealPlan.OverrideRate = await BuildAssignedOverrideAsync(source, bypassCache);
                }

                if (source.NoRatePeriods != null) studentMealPlan.NumberOfPeriods = source.NoRatePeriods;

                if (!string.IsNullOrEmpty(source.MealPlan))
                {
                    var startDate = Convert.ToDateTime(source.StartDate);
                    var mealPlanRates = await GetMealPlanRatesAsync(bypassCache);

                    if (mealPlanRates == null)
                    {
                        throw new ColleagueWebApiException("Unable to retrieve meal plan rates");
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
                            studentMealPlan.AssignedRate = new Dtos.GuidObject2(mealPlanGuid);
                        }
                    }
                }
                if (source.EndDate != null)
                {
                    studentMealPlan.EndOn = Convert.ToDateTime(source.EndDate);
                }
                studentMealPlan.Id = source.Guid;

                //get mealplan guid
                if (!string.IsNullOrEmpty(source.MealPlan))
                {
                    try
                    {
                        var mealplanGuid = await _referenceDataRepository.GetMealPlanGuidAsync(source.MealPlan);
                        if (!string.IsNullOrEmpty(mealplanGuid))
                            studentMealPlan.MealPlan = new Dtos.GuidObject2(mealplanGuid);
                    }
                    catch (RepositoryException ex)
                    {
                        throw new ColleagueWebApiException(ex.Message);
                    }
                }                  
                //get person guid
                if (!string.IsNullOrEmpty(source.PersonId))
                {
                    var studentGuid = string.Empty;
                    personGuidCollection.TryGetValue(source.PersonId, out studentGuid);
                    if (string.IsNullOrEmpty(studentGuid))
                    {
                        //throw new KeyNotFoundException(string.Concat("Student guid not found, StudentId: '", StuProgram.StudentId, "', Record ID: '", StuProgram.Guid, "'"));
                        throw new ColleagueWebApiException(string.Concat("Person guid not found for person Id: '", source.PersonId));
                    }
                    else
                    {
                        studentMealPlan.Person = new Dtos.GuidObject2(studentGuid);
                    }
                }

                studentMealPlan.StartOn = Convert.ToDateTime(source.StartDate);

                if (!string.IsNullOrEmpty(source.Status))
                {
                    studentMealPlan.Status = ConvertStatusToStudentMealPlansStatusDtoEnum(source.Status);
                }

                if (source.StatusDate != null)
                {
                    studentMealPlan.StatusDate = Convert.ToDateTime(source.StatusDate);
                }

                if (!string.IsNullOrEmpty(source.MealCard)) studentMealPlan.MealCard = source.MealCard;

                if (!string.IsNullOrEmpty(source.MealComments)) studentMealPlan.Comment = source.MealComments;

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
                    studentMealPlan.Consumption = studentMealPlansConsumption;
                }
                studentMealPlans.Add(studentMealPlan);
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
                    throw new ColleagueWebApiException("Unable to retrieve meal plans");
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
                    throw new ColleagueWebApiException("Unable to retrieve academic periods");
                }
                var term = academicPeriodEntities.FirstOrDefault(mp => mp.Guid == source.AcademicPeriod.Id);
                if (term == null)
                {
                    throw new ColleagueWebApiException(string.Concat(" Academic Period '", source.AcademicPeriod.Id, "' was not found."));
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
            if (source.Consumption != null)
            {
                if (source.Consumption.Units != null)
                {
                    studentMealPlans.UsedRatePeriods = source.Consumption.Units;
                }

                if (source.Consumption.Percent != null)
                {
                    studentMealPlans.PercentUsed = source.Consumption.Percent;
                }
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
                        throw new ColleagueWebApiException("Unable to retrieve meal plan rates");
                    }
                    var mealPlanRate = mealPlanRates.FirstOrDefault(mpr => mpr.Guid == defaultRate.Id);
                    if (mealPlanRate == null)
                    {
                        throw new ColleagueWebApiException(string.Concat(" Meal Plan rate '", defaultRate.Id.ToString(), "' was not found."));
                    }
                    else
                    {
                        if (mealPlanRate.Code != mealplanId)
                        {
                            throw new ColleagueWebApiException(string.Concat("Meal plan '", mealPlanRate.Code, "' from meal plan rate does not match assignment's meal plan '", mealplanId, "'"));
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
                            throw new ColleagueWebApiException(string.Concat(" Invalid meal plan rate '", defaultRate.Id.ToString(), "' with effective date '", effectiveDate, "' for start date of '", startDate, "'"));
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
                            throw new ColleagueWebApiException("Unable to retrieve accounting codes");
                        }
                        var accountingCode = accountingCodeEntities.FirstOrDefault(mpr => mpr.Guid == source.OverrideRate.AccountingCode.Id);
                        if (accountingCode == null)
                        {
                            throw new ColleagueWebApiException(string.Concat(" Accounting Code '", source.OverrideRate.AccountingCode.Id.ToString(), "' was not found."));
                        }
                        studentMealPlans.OverrideArCode = accountingCode.Code;
                    }
                    //get override reason
                    if (source.OverrideRate.OverrideReason != null)
                    {
                        var billingOverrideReasonsEntities = await GetBillingOverrideReasonsAsync(bypassCache);
                        if (billingOverrideReasonsEntities == null)
                        {
                            throw new ColleagueWebApiException("Unable to retrieve billing override reasons");
                        }
                        var billingOverrideReason = billingOverrideReasonsEntities.FirstOrDefault(mpr => mpr.Guid == source.OverrideRate.OverrideReason.Id);
                        if (billingOverrideReason == null)
                        {
                            throw new ColleagueWebApiException(string.Concat("Override Reason '", source.OverrideRate.OverrideReason.Id.ToString(), "' was not found."));
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
                    throw new ColleagueWebApiException(ex.Message, ex.InnerException);
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
                    throw new ColleagueWebApiException("Unable to retrieve meal plan rates");
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
                    throw new ColleagueWebApiException("Unable to retrieve accounting codes");
                }
                var accountingCode = accountingCodeEntities.FirstOrDefault(mpr => mpr.Code == source.OverrideArCode);
                if (accountingCode == null)
                {
                    throw new ColleagueWebApiException(string.Concat("Unable to locate guid for override AR code: ", source.OverrideArCode, ", Entity: 'MEAL.PLAN.ASSIGNMENTS', Record ID: '", source.Id, "'",
                        " Student: '", source.PersonId, "'", " Term: '", source.Term, '"'));

                }
                studentMealPlansOverrideRate.AccountingCode = new Dtos.GuidObject2(accountingCode.Guid);
            }

            if (!(string.IsNullOrEmpty(source.RateOverrideReason)))
            {
                var billingOverrideReasonsEntities = await GetBillingOverrideReasonsAsync(bypassCache);
                if (billingOverrideReasonsEntities == null)
                {
                    throw new ColleagueWebApiException("Unable to retrieve billing override reasons");
                }
                var billingOverrideReason = billingOverrideReasonsEntities.FirstOrDefault(mpr => mpr.Code == source.RateOverrideReason);
                if (billingOverrideReason == null)
                {
                    throw new ColleagueWebApiException(string.Concat("Unable to locate guid for rate override reason: ", source.RateOverrideReason, ", Entity: 'MEAL.PLAN.ASSIGNMENTS', Record ID: '", source.Id, "' ",
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
                    throw new ColleagueWebApiException("Unable to retrieve meal plans");
                }
                var mealPlan = mealPlans.FirstOrDefault(mpr => mpr.Code == source.MealPlan);
                if (mealPlan == null)
                {
                    throw new ColleagueWebApiException(string.Concat("Unable to locate guid for MealPlan: ", source.MealPlan, ", Entity: 'MEAL.PLAN.ASSIGNMENTS', Record ID: '", source.Id, "' ",
                        " Student: '", source.PersonId, "'", " Term: '", source.Term, '"'));

                }
                studentMealPlansOverrideRate.RatePeriod = ConvertMealPlanRateToMealPlanRatesRatePeriodDtoEnum(mealPlan.RatePeriod);
            }

            return studentMealPlansOverrideRate;
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

        /// <summary>
        /// Converts date to unidata Date
        /// </summary>
        /// <param name="date">UTC datetime</param>
        /// <returns>Unidata Date</returns>
        private async Task<string> ConvertDateArgument(string date)
        {
            try
            {
                return await _referenceDataRepository.GetUnidataFormattedDate(date);
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Invalid Date format in arguments");
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

        /// <summary>
        ///  Build StudentMealPlanAssignedOverrideDtoProperty
        /// </summary>
        /// <param name="source">MealPlanAssignment domain entity</param>
        /// <param name="bypassCache"></param>
        /// <returns><see cref="StudentMealPlanAssignedOverrideDtoProperty">Dtos.StudentMealPlanAssignedOverrideDtoProperty object</see></returns>
        private async Task<StudentMealPlansOverrideRateDtoProperty> BuildAssignedOverride2Async( MealPlanAssignment source, bool bypassCache = false )
        {
            var studentMealPlansOverrideRate = new StudentMealPlansOverrideRateDtoProperty();
            if( !( string.IsNullOrEmpty( source.OverrideArCode ) ) )
            {
                var accountingCodeEntities = await GetAccountingCodesAsync( bypassCache );
                if( accountingCodeEntities == null )
                {
                    //throw new ColleagueWebApiException("Unable to retrieve accounting codes");
                    IntegrationApiExceptionAddError( "Unable to retrieve accounting codes.", "Validation.Exception" );
                }
                else
                {
                    var accountingCode = accountingCodeEntities.FirstOrDefault( mpr => mpr != null && !string.IsNullOrEmpty( mpr.Code ) && mpr.Code.Equals( source.OverrideArCode, StringComparison.InvariantCultureIgnoreCase ) );
                    if( accountingCode == null )
                    {
                        string message = string.Concat( "Unable to locate guid for override AR code: ", source.OverrideArCode, ", Entity: 'MEAL.PLAN.ASSIGNMENTS', Record ID: '", source.Id, "'",
                            " Student: '", source.PersonId, "'", " Term: '", source.Term, '"' );
                        IntegrationApiExceptionAddError( message, "GUID.Not.Found", source.Guid, source.Id );
                    }
                    else
                    {
                        studentMealPlansOverrideRate.AccountingCode = new Dtos.GuidObject2( accountingCode.Guid );
                    }
                }
            }

            if( !( string.IsNullOrEmpty( source.RateOverrideReason ) ) )
            {
                var billingOverrideReasonsEntities = await GetBillingOverrideReasonsAsync( bypassCache );
                if( billingOverrideReasonsEntities == null )
                {
                    IntegrationApiExceptionAddError( "Unable to retrieve billing override reasons.", "Validation.Exception" );
                }
                else
                {
                    var billingOverrideReason = billingOverrideReasonsEntities.FirstOrDefault( mpr => mpr != null && !string.IsNullOrEmpty( mpr.Code ) && mpr.Code.Equals( source.RateOverrideReason, StringComparison.InvariantCultureIgnoreCase ) );
                    if( billingOverrideReason == null )
                    {
                        string message = string.Concat( "Unable to locate guid for rate override reason: ", source.RateOverrideReason, ", Entity: 'MEAL.PLAN.ASSIGNMENTS', Record ID: '", source.Id, "' ",
                            " Student: '", source.PersonId, "'", " Term: '", source.Term, '"' );
                        IntegrationApiExceptionAddError( message, "GUID.Not.Found", source.Guid, source.Id );

                    }
                    else
                    {
                        studentMealPlansOverrideRate.OverrideReason = new Dtos.GuidObject2( billingOverrideReason.Guid );
                    }
                }
            }

            if( ( source.OverrideRate != null ) && ( source.OverrideRate.HasValue ) )
            {
                studentMealPlansOverrideRate.Rate = new Amount2DtoProperty()
                {
                    Currency = ( await GetHostCountryAsync() ).ToUpper() == "USA" ? CurrencyIsoCode.USD : CurrencyIsoCode.CAD,
                    Value = source.OverrideRate
                };
            }

            if( !string.IsNullOrEmpty( source.MealPlan ) )
            {
                var mealPlans = await GetMealPlansAsync( bypassCache );

                if( mealPlans == null )
                {
                    IntegrationApiExceptionAddError( "Unable to retrieve meal plans.", "Validation.Exception" );

                }
                else
                {
                    var mealPlan = mealPlans.FirstOrDefault( mpr => mpr != null && !string.IsNullOrEmpty( mpr.Code ) && mpr.Code.Equals( source.MealPlan, StringComparison.InvariantCultureIgnoreCase ) );
                    if( mealPlan == null )
                    {
                        string message = string.Concat( "Unable to locate guid for MealPlan: ", source.MealPlan, ", Entity: 'MEAL.PLAN.ASSIGNMENTS', Record ID: '", source.Id, "' ",
                            " Student: '", source.PersonId, "'", " Term: '", source.Term, '"' );
                        IntegrationApiExceptionAddError( message, "GUID.Not.Found", source.Guid, source.Id );
                    }
                    else
                    {
                        studentMealPlansOverrideRate.RatePeriod = ConvertMealPlanRateToMealPlanRatesRatePeriodDtoEnum( mealPlan.RatePeriod );
                    }
                }
            }

            return studentMealPlansOverrideRate;
        }

    }
}