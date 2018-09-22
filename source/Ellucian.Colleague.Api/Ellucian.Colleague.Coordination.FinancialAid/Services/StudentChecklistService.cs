/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    public class StudentChecklistService : AwardYearCoordinationService, IStudentChecklistService
    {
        private readonly IStudentChecklistRepository studentChecklistRepository;
        private readonly IFinancialAidReferenceDataRepository financialAidReferenceDataRepository;
        private readonly IConfigurationRepository configurationRepository;

        /// <summary>
        /// Constructor for StudentChecklistService
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="studentChecklistRepository">StudentChecklistRepository</param>
        /// <param name="studentAwardYearRepository">StudentAwardYearRepository</param>
        /// <param name="financialAidOfficeRepository">FinancialAidOfficeRepository</param>
        /// <param name="financialAidReferenceDataRepository">FinancialAidReferenceDataRepository</param>
        /// <param name="currentUserFactory">CurrentUserFactory</param>
        /// <param name="roleRepository">RoleRepository</param>
        /// <param name="logger">Logger</param>
        public StudentChecklistService(
            IAdapterRegistry adapterRegistry,
            IStudentChecklistRepository studentChecklistRepository,
            IStudentAwardYearRepository studentAwardYearRepository,
            IFinancialAidOfficeRepository financialAidOfficeRepository,
            IFinancialAidReferenceDataRepository financialAidReferenceDataRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, financialAidOfficeRepository, studentAwardYearRepository, configurationRepository, currentUserFactory, roleRepository, logger)
        {
            this.studentChecklistRepository = studentChecklistRepository;
            this.financialAidReferenceDataRepository = financialAidReferenceDataRepository;
            this.configurationRepository = configurationRepository;
        }

        /// <summary>
        /// Create a new checklist for a student for a year.
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to create a checklist</param>
        /// <param name="year">The award year for which to create a checklist</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active years data only</param>
        /// <returns>List of student financial aid checklist objects per year</returns>
        /// <exception cref="ApplicationException">Thrown if there is an error creating the StudentChecklist</exception>
        /// <exception cref="ExistingResourceException">Thrown if a checklist already exists for this year for this student</exception>
        public async Task<StudentFinancialAidChecklist> CreateStudentChecklistAsync(string studentId, string year, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (string.IsNullOrEmpty(year))
            {
                throw new ArgumentNullException("year");
            }

            if (!UserIsSelf(studentId))
            {
                var message = string.Format("User {0} does not have permission to create StudentChecklist objects for student {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            Domain.FinancialAid.Entities.StudentFinancialAidChecklist existingChecklist = null;
            try
            {
                existingChecklist = await studentChecklistRepository.GetStudentChecklistAsync(studentId, year);
            }
            catch (Exception e)
            {
                logger.Info(e, string.Format("Student {0} has no checklist for {1}. In create method so this is good!", studentId, year));
            }
            if (existingChecklist != null)
            {
                var message = string.Format("Student {0} is already assigned a checklist for {1}", studentId, year);
                logger.Error(message);
                throw new ExistingResourceException(message, year);
            }

            //Get all student award years and the current student award year
            IEnumerable<Colleague.Domain.FinancialAid.Entities.StudentAwardYear> studentAwardYearEntities = null;
            try
            {
                studentAwardYearEntities = await GetStudentAwardYearEntitiesAsync(studentId, getActiveYearsOnly);
            }
            catch (Exception e)
            {
                logger.Info(e.Message);
            }
            var studentAwardYearEntity = studentAwardYearEntities != null ? studentAwardYearEntities.FirstOrDefault(y => y.Code == year) : null;

            //Get current office and configuration - may return default office if office with specified id does not exist
            var currentOfficeService = new CurrentOfficeService(await financialAidOfficeRepository.GetFinancialAidOfficesAsync());
            var currentOffice = currentOfficeService.GetCurrentOfficeByOfficeId(studentAwardYearEntity != null ? studentAwardYearEntity.FinancialAidOfficeId : string.Empty);
            Colleague.Domain.FinancialAid.Entities.FinancialAidConfiguration currentConfig = null;
            
            //If the student is assigned the passed in award year, get configuration for that year; otherwise, get most recent Configuration
            if (currentOffice != null && currentOffice.Configurations != null && currentOffice.Configurations.Any())
            {
                if(studentAwardYearEntity != null){
                    currentConfig = currentOffice.Configurations.FirstOrDefault(c => c.AwardYear == year);
                }
                else{
                    currentConfig = currentOffice.Configurations.OrderByDescending(c => c.AwardYear).FirstOrDefault();
                }
            }

            var defaultCreateFlag = currentConfig != null ? currentConfig.CreateChecklistItemsForNewStudent : false;

            //If "Create checklist if no FinAid record Exists" flag is set to "N", we will throw an exception if no student award year(s) exist(s)
            if (defaultCreateFlag == false)
            {
                if (studentAwardYearEntities == null || !studentAwardYearEntities.Any())
                {
                    var message = string.Format("Student {0} has no award years for which to create StudentChecklist objects", studentId);
                    logger.Error(message);
                    throw new ApplicationException(message);
                    //eventually, will want to create this StudentAwardYear for the student, running location rules to determine location
                }
                
                if (studentAwardYearEntity == null)
                {
                    var message = string.Format("Student {0} has no financial aid data for award year {1}", studentId, year);
                    logger.Error(message);
                    throw new ApplicationException(message);
                    //eventually, will want to create this StudentAwardYear for the student, running location rules to determine location
                }
            }

            var referenceChecklistItems = financialAidReferenceDataRepository.ChecklistItems;
            if (referenceChecklistItems == null || !referenceChecklistItems.Any())
            {
                var message = string.Format("Error getting checklist items. No Checklist Items appear to be defined");
                logger.Error(message);
                throw new ApplicationException(message);
            }

            Colleague.Domain.FinancialAid.Entities.StudentFinancialAidChecklist inputChecklistEntity = null;
            Colleague.Domain.FinancialAid.Entities.StudentFinancialAidChecklist newChecklistEntity = null;

            //Create a dictionary of checklist items defined on the year/office level (key - item code, value - item control status)
            Dictionary<string, string> officeChecklistItems = new Dictionary<string, string>();
            if (currentConfig != null && currentConfig.ChecklistItemCodes.Any() && currentConfig.ChecklistItemControlStatuses.Any() && currentConfig.ChecklistItemDefaultFlags.Any())
            {
                for (var i = 0; i < currentConfig.ChecklistItemCodes.Count; i++)
                {
                    if(!string.IsNullOrEmpty(currentConfig.ChecklistItemDefaultFlags[i]) && currentConfig.ChecklistItemDefaultFlags[i].ToUpper() == "Y")
                    {
                        officeChecklistItems.Add(currentConfig.ChecklistItemCodes[i], currentConfig.ChecklistItemControlStatuses[i]);
                    }
                }

                inputChecklistEntity = StudentChecklistDomainService.BuildStudentFinancialAidChecklist(studentId, currentConfig.AwardYear, referenceChecklistItems, officeChecklistItems);
                newChecklistEntity = await studentChecklistRepository.CreateStudentChecklistAsync(inputChecklistEntity);
            }
            
            if (newChecklistEntity == null)
            {
                var message = string.Format("Could not create a student checklist for {0} year", year);
                logger.Error(message);
                throw new ApplicationException(message);
            }

            var studentChecklistDtoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.StudentFinancialAidChecklist, Dtos.FinancialAid.StudentFinancialAidChecklist>();
            var inputStudentChecklistEntity = studentChecklistDtoAdapter.MapToType(newChecklistEntity);

            return inputStudentChecklistEntity;

        }

        /// <summary>
        /// Get all checklists for a student
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get checklists</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active years data only</param>
        /// <returns>List of student checklists</returns>
        public async Task<IEnumerable<StudentFinancialAidChecklist>> GetAllStudentChecklistsAsync(string studentId, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("User {0} does not have permission to get StudentChecklists for student {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var studentAwardYearEntities = await GetStudentAwardYearEntitiesAsync(studentId, getActiveYearsOnly);
            if (studentAwardYearEntities == null || !studentAwardYearEntities.Any())
            {
                var message = string.Format("Student {0} has no award years for which to get StudentChecklist objects", studentId);
                logger.Info(message);
                return new List<StudentFinancialAidChecklist>();
            }
            
            var studentChecklistEntities = await studentChecklistRepository.GetStudentChecklistsAsync(studentId, studentAwardYearEntities.Select(y => y.Code).ToList());
            if (studentChecklistEntities == null || !studentChecklistEntities.Any())
            {
                //  empty list instead of an error
                var message = string.Format("Student {0} has no checklist items for any award years", studentId);
                logger.Info(message);
                return new List<StudentFinancialAidChecklist>();
            }

            var studentChecklistDtoAdapter = _adapterRegistry.GetAdapter<Colleague.Domain.FinancialAid.Entities.StudentFinancialAidChecklist, StudentFinancialAidChecklist>();
            return studentChecklistEntities.Select(item => studentChecklistDtoAdapter.MapToType(item));
        }

        /// <summary>
        /// Get a student checklist for a given year
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get the checklist</param>
        /// <param name="year">The award year for which to get a checklist</param>
        /// <returns>A single student checklist</returns>
        public async Task<StudentFinancialAidChecklist> GetStudentChecklistAsync(string studentId, string year, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (string.IsNullOrEmpty(year))
            {
                throw new ArgumentNullException("year");
            }

            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("User {0} does not have permission to get StudentChecklist for student {1} and awardYear {2}", CurrentUser.PersonId, studentId, year);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var studentAwardYearEntities = await GetStudentAwardYearEntitiesAsync(studentId, getActiveYearsOnly);
            if (studentAwardYearEntities == null || !studentAwardYearEntities.Any())
            {
                var message = string.Format("Student {0} has no award years for which to get StudentChecklist objects", studentId);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            var studentAwardYearEntity = studentAwardYearEntities.FirstOrDefault(y => y.Code == year);
            if (studentAwardYearEntity == null)
            {
                var message = string.Format("Student {0} has no financial aid data for award year {1}", studentId, year);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            var studentChecklistEntity = await studentChecklistRepository.GetStudentChecklistAsync(studentId, year);
            if (studentChecklistEntity == null)
            {
                var message = string.Format("Student {0} has no checklist items for award year {1}", studentId, year);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            var studentChecklistDtoAdapter = _adapterRegistry.GetAdapter<Colleague.Domain.FinancialAid.Entities.StudentFinancialAidChecklist, Ellucian.Colleague.Dtos.FinancialAid.StudentFinancialAidChecklist>();
            var inputStudentChecklistEntity = studentChecklistDtoAdapter.MapToType(studentChecklistEntity);

            return inputStudentChecklistEntity;

        }

    }
}
