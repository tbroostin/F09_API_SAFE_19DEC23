/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    public class StudentChecklistService : AwardYearCoordinationService, IStudentChecklistService
    {
        private readonly IStudentChecklistRepository studentChecklistRepository;
        private readonly IFinancialAidReferenceDataRepository financialAidReferenceDataRepository;
        private readonly IConfigurationRepository configurationRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly IRelationshipRepository _relationshipRepository;
        private readonly IProxyRepository _proxyRepository;

        /// <summary>
        /// Constructor for StudentChecklistService
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="studentChecklistRepository">StudentChecklistRepository</param>
        /// <param name="studentAwardYearRepository">StudentAwardYearRepository</param>
        /// <param name="financialAidOfficeRepository">FinancialAidOfficeRepository</param>
        /// <param name="financialAidReferenceDataRepository">FinancialAidReferenceDataRepository</param>
        /// <param name="profileRepository">ProfileRepository</param>
        /// <param name="proxyRepository">ProxyRepository</param>
        /// <param name="relationshipRepository">RelationshipRepository</param>
        /// <param name="configurationRepository">ConfigurationRepository</param>
        /// <param name="currentUserFactory">CurrentUserFactory</param>
        /// <param name="roleRepository">RoleRepository</param>
        /// <param name="logger">Logger</param>
        public StudentChecklistService(
            IAdapterRegistry adapterRegistry,
            IStudentChecklistRepository studentChecklistRepository,
            IStudentAwardYearRepository studentAwardYearRepository,
            IFinancialAidOfficeRepository financialAidOfficeRepository,
            IFinancialAidReferenceDataRepository financialAidReferenceDataRepository,
            IProfileRepository profileRepository,
            IProxyRepository proxyRepository,
            IRelationshipRepository relationshipRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, financialAidOfficeRepository, studentAwardYearRepository, configurationRepository, currentUserFactory, roleRepository, logger)
        {
            this.studentChecklistRepository = studentChecklistRepository;
            this.financialAidReferenceDataRepository = financialAidReferenceDataRepository;
            this.configurationRepository = configurationRepository;
            _profileRepository = profileRepository;
            _proxyRepository = proxyRepository;
            _relationshipRepository = relationshipRepository;

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

        ///<summary>
        ///Get a parent's profile for a PLUS MPN
        /// </summary>
        /// <param name="parentId">The ID for the parent whose profile is being retrieved</param>
        /// <param name="studentId">The ID for the student whose parent's profile is being retrieved</param>
        /// <param name="useCache">True/false flag to retrieve cached data</param>
        /// <returns>A parent's demographic profile</returns>
        public async Task<Dtos.Base.Profile> GetMpnProfileAsync(string parentId, string studentId, bool useCache = true)
        {
            if (string.IsNullOrEmpty(parentId))
            {
                throw new ArgumentNullException("parentId", "A person's id is required to retrieve profile information");
            }

            if (!await FAUserCanViewProfileForPerson(studentId, parentId))
            {
                string message = CurrentUser.PersonId + " cannot view profile for person " + parentId + " check PREL and proxy access. If this is an admin user, check for the VIEW.FINANCIAL.AID.INFORMATION permission code.";
                logger.Info(message);
                throw new PermissionsException(message);
            }

            Profile profileEntity = await _profileRepository.GetProfileAsync(parentId, useCache);
            if (profileEntity == null)
            {
                throw new Exception("Profile information could not be retrieved for person " + parentId);
            }

            var profileDtoAdapter = _adapterRegistry.GetAdapter<Profile, Dtos.Base.Profile>();
            Dtos.Base.Profile profileDto = profileDtoAdapter.MapToType(profileEntity);

            return profileDto;
        }

        /// <summary>
        /// Verifies if the user is permitted to view another person's profile
        /// Profile information for the parentId can only be viewed when:
        ///  The student (studentId) has an established relationship on PREL with the parentId
        ///  AND
        ///  (
        ///  1. The CurrentUser is the studentID
        ///  OR  
        ///  2. The CurrentUser has the VIEW.FINANCIAL.AID.INFORMATION permission code on one of their roles
        ///  OR
        ///  3. The CurrentUser exists as a proxy for the studentId
        ///  )
        /// </summary>
        /// <param name="studentId">The student ID to retrieve PREL relationships for </param>
        /// <param name="parentId">The parent ID to attempt to evaluate if the current user can view and return a profile for </param>
        private async Task<bool> FAUserCanViewProfileForPerson(string studentId, string parentId)
        {
            bool isFaAdmin = false;
            bool parentIsLinkedToStudent = false;
            var relationshipIds = await _relationshipRepository.GetRelatedPersonIdsAsync(studentId);
            if (relationshipIds.Contains(parentId))
            {
                parentIsLinkedToStudent = true;
            }
            else return false;

            if (CurrentUser.IsPerson(studentId) && parentIsLinkedToStudent)
            {
                return true;
            }

            if (!CurrentUser.IsPerson(studentId) && !CurrentUser.IsPerson(parentId))
            {
                //If the current user isn't the student or the parent, check if they have the VIEW.FINANCIAL.AID.INFORMATION permission code
                isFaAdmin = IsUserFaAdmin();
                if (isFaAdmin)
                {
                    return true;
                }
                
                //If the current user is not the student, parent, or an FA admin, check if they have proxy access to the student
                var proxyUsers = await _proxyRepository.GetUserProxyPermissionsAsync(studentId);
                if (proxyUsers.Select(pu => pu.Id).Contains(CurrentUser.PersonId))
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if the current user ID has the VIEW.FINANCIAL.AID.INFORMATION permissions code
        /// If so, they are an admin and are able to view the parent ID's profile 
        /// </summary>
        /// <returns>TRUE if the ID has the permission code - otherwise returns FALSE</returns>
        private bool IsUserFaAdmin()
        {
            if (HasPermission(Domain.Student.StudentPermissionCodes.ViewFinancialAidInformation))
            {
                return true;
            }
            else return false;
        }
    }
}
