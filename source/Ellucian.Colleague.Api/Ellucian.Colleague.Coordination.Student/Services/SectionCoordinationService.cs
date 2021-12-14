// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Services;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Coordination.Base;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ellucian.Colleague.Coordination.Student.Adapters;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Coordination service for sections
    /// </summary>
    [RegisterType]
    public class SectionCoordinationService : StudentCoordinationService, ISectionCoordinationService, IBaseService
    {
        private readonly ISectionRepository _sectionRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ITermRepository _termRepository;
        private readonly IStudentConfigurationRepository _studentConfigRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IBookRepository _bookRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="sectionRepository"></param>
        /// <param name="courseRepository"></param>
        /// <param name="studentRepository"></param>
        /// <param name="studentReferenceDataRepository"></param>
        /// <param name="referenceDataRepository"></param>
        /// <param name="termRepository"></param>
        /// <param name="studentConfigurationRepository"></param>
        /// <param name="personRepository"></param>
        /// <param name="roomRepository"></param>
        /// <param name="configurationRepository"></param>
        /// <param name="eventRepository"></param>
        /// <param name="academicPeriodRepository"></param>
        /// <param name="bookRepository"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        public SectionCoordinationService(IAdapterRegistry adapterRegistry, ISectionRepository sectionRepository, ICourseRepository courseRepository,
            IStudentRepository studentRepository, IStudentReferenceDataRepository studentReferenceDataRepository, IReferenceDataRepository referenceDataRepository,
            ITermRepository termRepository, IStudentConfigurationRepository studentConfigurationRepository, IConfigurationRepository configurationRepository,
            IPersonRepository personRepository, IRoomRepository roomRepository, IEventRepository eventRepository,
            IBookRepository bookRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _sectionRepository = sectionRepository;
            _studentRepository = studentRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _referenceDataRepository = referenceDataRepository;
            _courseRepository = courseRepository;
            _termRepository = termRepository;
            _studentConfigRepository = studentConfigurationRepository;
            _configurationRepository = configurationRepository;
            _personRepository = personRepository;
            _roomRepository = roomRepository;
            _eventRepository = eventRepository;
            _bookRepository = bookRepository;
        }

        /// <summary>
        /// Get all person pins
        /// </summary>
        public IEnumerable<Domain.Base.Entities.PersonPin> _personPins { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.PersonPin>> GetPersonPinsAsync(string[] guids)
        {
            if (_personPins == null)
            {
                _personPins = await _personRepository.GetPersonPinsAsync(guids);
            }
            return _personPins;
        }

        /// <summary>
        /// Get the guids for an array of faculty IDs
        /// </summary>
        private Dictionary<string, string> _facultyGuids = null;
        private async Task SetFacultyGuidsAsync(string[] facultyIds)
        {
            _facultyGuids = await _personRepository.GetPersonGuidsCollectionAsync(facultyIds);
        }

        /// <summary>
        /// Get minimal person data for the instructors (faculty)
        /// </summary>
        private Dictionary<string, Domain.Base.Entities.PersonIntegration> _personFacultyData = null;
        private async Task SetPersonDataForFacultyAsync(string[] guids)
        {
            _personFacultyData = new Dictionary<string, Domain.Base.Entities.PersonIntegration>();
            IEnumerable<Domain.Base.Entities.PersonIntegration> personIntegrations = await _personRepository.GetPersonNamesAndCredsByGuidAsync(guids);
            if ((personIntegrations != null) && (personIntegrations.Any()))
            {
                foreach (var pi in personIntegrations)
                {
                    if (pi != null)
                    {
                        _personFacultyData.Add(pi.Guid, pi);
                    }
                }
            }
        }
        private async Task<Domain.Base.Entities.PersonIntegration> GetPersonDataForFacultyByGuidAsync(string personGuid)
        {
            Domain.Base.Entities.PersonIntegration person = null;
            if (_personFacultyData != null && _personFacultyData.Any())
            {
                _personFacultyData.TryGetValue(personGuid, out person);
            }
            else
            {
                person = await _personRepository.GetPersonNamesAndCredsByGuidAsync(personGuid);
            }
            return person;
        }

        /// <summary>
        /// Get all section guids
        /// </summary>
        private Dictionary<string, string> _sectionGuids = null;
        private void SetSectionGuids(IEnumerable<Domain.Student.Entities.Section> sections)
        {
            _sectionGuids = new Dictionary<string, string>();
            if ((sections != null) && (sections.Any()))
            {
                foreach (var sec in sections)
                {
                    if (!string.IsNullOrEmpty(sec.Guid))
                    {
                        _sectionGuids.Add(sec.Id, sec.Guid);
                    }
                }
            }
        }
        private void SetSectionGuids(Domain.Student.Entities.Section section)
        {
            _sectionGuids = new Dictionary<string, string>();
            if ((section != null) && (!string.IsNullOrEmpty(section.Guid)))
            {
                _sectionGuids.Add(section.Id, section.Guid);
            }
        }
        private async Task<string> ConvertSectionIdToSectionGuidAsync(string sectionId)
        {
            string sectionGuid = null;
            if (_sectionGuids != null && _sectionGuids.Any())
            {
                _sectionGuids.TryGetValue(sectionId, out sectionGuid);
            }
            else
            {
                sectionGuid = await _sectionRepository.GetSectionGuidFromIdAsync(sectionId);
            }
            return sectionGuid;
        }

        /// <summary>
        /// Get all course info for a list of course IDs
        /// </summary>
        private Dictionary<string, Course> _courses = null;
        private async Task SetCourseCollectionAsync(IEnumerable<string> courseIds)
        {
            _courses = new Dictionary<string, Course>();
            if ((courseIds != null) && (courseIds.Any()))
            {
                IEnumerable<Course> _courseData = await _courseRepository.GetCoursesByIdAsync(courseIds);
                if ((_courseData != null) && (_courseData.Any()))
                {
                    foreach (var crs in _courseData)
                    {
                        if (crs != null)
                        {
                            _courses.Add(crs.Id, crs);
                        }
                    }
                }
            }
        }
        private async Task<Course> GetCourseByIdAsync(string courseId)
        {
            Course course = null;
            string courseGuid = null;

            if (_courses != null)
            {
                if (!string.IsNullOrEmpty(courseId))
                {
                    _courses.TryGetValue(courseId, out course);
                    courseGuid = (course == null) ? null : course.Guid;
                }
            }
            else
            {
                courseGuid = await _courseRepository.GetCourseGuidFromIdAsync(courseId);
                if (!string.IsNullOrEmpty(courseGuid))
                {
                    course = await _courseRepository.GetCourseByGuidAsync(courseGuid);
                }
            }
            if (string.IsNullOrEmpty(courseGuid))
            {
                throw new ArgumentException(string.Concat("Course guid not found for Colleague Course Id : ", courseId));
            }
            return course;
        }

        private IEnumerable<Domain.Base.Entities.Department> _departments = null;
        private async Task<IEnumerable<Domain.Base.Entities.Department>> DepartmentsAsync()
        {
            if (_departments == null)
            {
                _departments = await _referenceDataRepository.DepartmentsAsync();
            }
            return _departments;
        }

        private IEnumerable<Domain.Base.Entities.Location> _locations = null;
        private IEnumerable<Domain.Base.Entities.Location> locations
        {
            get
            {
                if (_locations == null)
                {
                    _locations = _referenceDataRepository.Locations;
                }
                return _locations;
            }
        }

        private IEnumerable<Domain.Base.Entities.Room> _rooms = null;
        private async Task<IEnumerable<Domain.Base.Entities.Room>> RoomsAsync()
        {
            if (_rooms == null)
            {
                _rooms = await _roomRepository.RoomsAsync();
            }

            return _rooms;
        }

        private IEnumerable<Domain.Base.Entities.Building> _buildings = null;
        private async Task<IEnumerable<Domain.Base.Entities.Building>> BuildingsAsync()
        {
            if (_buildings == null)
            {
                _buildings = await _referenceDataRepository.BuildingsAsync();
            }

            return _buildings;
        }

        private IEnumerable<Domain.Student.Entities.AcademicLevel> _academicLevels = null;
        private async Task<IEnumerable<Domain.Student.Entities.AcademicLevel>> AcademicLevelsAsync()
        {
            if (_academicLevels == null)
            {
                _academicLevels = await _studentReferenceDataRepository.GetAcademicLevelsAsync();
            }
            return _academicLevels;
        }

        private IEnumerable<Domain.Student.Entities.GradeScheme> _gradeSchemes = null;
        private async Task<IEnumerable<Domain.Student.Entities.GradeScheme>> GradeSchemesAsync()
        {
            if (_gradeSchemes == null)
            {
                _gradeSchemes = await _studentReferenceDataRepository.GetGradeSchemesAsync();
            }
            return _gradeSchemes;
        }

        private IEnumerable<Domain.Student.Entities.CourseLevel> _courseLevels = null;
        private async Task<IEnumerable<Domain.Student.Entities.CourseLevel>> CourseLevelsAsync()
        {
            if (_courseLevels == null)
            {
                _courseLevels = await _studentReferenceDataRepository.GetCourseLevelsAsync();
            }
            return _courseLevels;
        }

        private IEnumerable<Domain.Student.Entities.CreditCategory> _creditTypes = null;
        private async Task<IEnumerable<Domain.Student.Entities.CreditCategory>> CreditTypesAsync()
        {
            if (_creditTypes == null)
            {
                _creditTypes = await _studentReferenceDataRepository.GetCreditCategoriesAsync();
            }
            return _creditTypes;
        }

        private IEnumerable<Domain.Student.Entities.InstructionalMethod> _instructionalMethods = null;
        private async Task<IEnumerable<Domain.Student.Entities.InstructionalMethod>> InstructionalMethodsAsync(bool bypassCache = false)
        {
            if (_instructionalMethods == null)
            {
                _instructionalMethods = await _studentReferenceDataRepository.GetInstructionalMethodsAsync(bypassCache);
            }
            return _instructionalMethods;
        }

        private IEnumerable<Domain.Student.Entities.AdministrativeInstructionalMethod> _administrativeInstructionalMethods = null;
        private async Task<IEnumerable<Domain.Student.Entities.AdministrativeInstructionalMethod>> AdministrativeInstructionalMethodsAsync(bool bypassCache = false)
        {
            if (_administrativeInstructionalMethods == null)
            {
                _administrativeInstructionalMethods = await _studentReferenceDataRepository.GetAdministrativeInstructionalMethodsAsync(bypassCache);
            }
            return _administrativeInstructionalMethods;
        }

        private IEnumerable<Domain.Student.Entities.SectionStatusCode> _sectionStatuses = null;
        private async Task<IEnumerable<Domain.Student.Entities.SectionStatusCode>> SectionStatusesAsync()
        {
            if (_sectionStatuses == null)
            {
                _sectionStatuses = await _studentReferenceDataRepository.GetSectionStatusCodesAsync();
            }
            return _sectionStatuses;
        }

        private IEnumerable<Domain.Base.Entities.ScheduleRepeat> _scheduleRepeats = null;
        private IEnumerable<Domain.Base.Entities.ScheduleRepeat> scheduleRepeats
        {
            get
            {
                if (_scheduleRepeats == null)
                {
                    _scheduleRepeats = _referenceDataRepository.ScheduleRepeats;
                }
                return _scheduleRepeats;
            }
        }

        private IEnumerable<Domain.Base.Entities.InstructionalPlatform> _instructionalPlatforms = null;
        private async Task<IEnumerable<Domain.Base.Entities.InstructionalPlatform>> InstructionalPlatformsAsync()
        {
            if (_instructionalPlatforms == null)
            {
                _instructionalPlatforms = await _referenceDataRepository.GetInstructionalPlatformsAsync(false);
            }
            return _instructionalPlatforms;
        }

        private IEnumerable<Domain.Student.Entities.AcademicPeriod> _academicPeriods = null;
        private IEnumerable<Domain.Student.Entities.AcademicPeriod> academicPeriods
        {
            get
            {
                if (_academicPeriods == null)
                {
                    _academicPeriods = _termRepository.GetAcademicPeriods(_termRepository.Get());
                }
                return _academicPeriods;
            }
        }

        private IEnumerable<Domain.Student.Entities.Term> _terms = null;
        private async Task<IEnumerable<Domain.Student.Entities.Term>> AllTermsAsync()
        {
            if (_terms == null)
            {
                _terms = await _termRepository.GetAsync();
            }
            return _terms;
        }

        private IEnumerable<SectionStatusCodeGuid> _allStatusesWithGuids = null;
        private async Task<IEnumerable<SectionStatusCodeGuid>> AllStatusesWithGuidsAsync()
        {
            if (_allStatusesWithGuids == null)
            {
                _allStatusesWithGuids = await _sectionRepository.GetStatusCodesWithGuidsAsync();
            }
            return _allStatusesWithGuids;
        }

        private IEnumerable<Domain.Student.Entities.CourseType> _allCourseTypes = null;
        private async Task<IEnumerable<Domain.Student.Entities.CourseType>> AllCourseTypes()
        {
            if (_allCourseTypes == null)
            {
                _allCourseTypes = await _studentReferenceDataRepository.GetCourseTypesAsync(false);
            }
            return _allCourseTypes;
        }

        private IEnumerable<Domain.Base.Entities.Department> _allDepartments = null;
        private async Task<IEnumerable<Domain.Base.Entities.Department>> AllDepartments()
        {
            if (_allDepartments == null)
            {
                _allDepartments = await _referenceDataRepository.GetDepartmentsAsync(false);
            }
            return _allDepartments;
        }

        private Domain.Base.Entities.DefaultsConfiguration _defaultConfiguration = null;
        private Domain.Base.Entities.DefaultsConfiguration defaultConfiguration
        {
            get
            {
                if (_defaultConfiguration == null)
                {
                    _defaultConfiguration = _configurationRepository.GetDefaultsConfiguration();
                }
                return _defaultConfiguration;
            }
        }

        private IEnumerable<Domain.Student.Entities.ContactMeasure> _allContactMeasures = null;
        private async Task<IEnumerable<Domain.Student.Entities.ContactMeasure>> AllContactMeasuresAsync(bool bypassCache = false)
        {
            if (_allContactMeasures == null)
            {
                _allContactMeasures = await _studentReferenceDataRepository.GetContactMeasuresAsync(bypassCache);
            }
            return _allContactMeasures;
        }

        private IEnumerable<Domain.Student.Entities.ChargeAssessmentMethod> _allChargeAssessmentMethods = null;
        private async Task<IEnumerable<Domain.Student.Entities.ChargeAssessmentMethod>> AllChargeAssessmentMethodAsync(bool bypassCache = false)
        {
            if (_allChargeAssessmentMethods == null)
            {
                _allChargeAssessmentMethods = await _studentReferenceDataRepository.GetChargeAssessmentMethodsAsync(bypassCache);
            }
            return _allChargeAssessmentMethods;
        }

        private Domain.Student.Entities.CurriculumConfiguration _curriculumConfiguration = null;
        private async Task<Domain.Student.Entities.CurriculumConfiguration> GetCurriculumConfigurationAsync()
        {
            if (_curriculumConfiguration == null)
            {
                _curriculumConfiguration = await _studentConfigRepository.GetCurriculumConfigurationAsync();
                VerifyCurriculumConfiguration(_curriculumConfiguration);
            }
            return _curriculumConfiguration;
        }

        private Domain.Student.Entities.CurriculumConfiguration _curriculumConfiguration2 = null;
        private async Task<Domain.Student.Entities.CurriculumConfiguration> GetCurriculumConfiguration2Async()
        {
            if (_curriculumConfiguration2 == null)
            {
                _curriculumConfiguration2 = await _studentConfigRepository.GetCurriculumConfigurationAsync();
                VerifyCurriculumConfiguration2(_curriculumConfiguration2);
            }
            return _curriculumConfiguration2;
        }

        private IEnumerable<Domain.Student.Entities.CourseTitleType> _courseTitleTypes = null;
        private async Task<IEnumerable<Domain.Student.Entities.CourseTitleType>> CourseTitleTypesAsync(bool bypassCache = false)
        {
            if (_courseTitleTypes == null)
            {
                _courseTitleTypes = await _studentReferenceDataRepository.GetCourseTitleTypesAsync(bypassCache);
            }
            return _courseTitleTypes;
        }

        private IEnumerable<Domain.Student.Entities.Subject> _subjects = null;
        private async Task<IEnumerable<Domain.Student.Entities.Subject>> SubjectsAsync(bool bypassCache = false)
        {
            if (_subjects == null)
            {
                _subjects = await _studentReferenceDataRepository.GetSubjectsAsync(bypassCache);
            }
            return _subjects;
        }

        private IEnumerable<Domain.Student.Entities.SectionTitleType> _sectionTitleTypes = null;
        private async Task<IEnumerable<Domain.Student.Entities.SectionTitleType>> SectionTitleTypesAsync(bool bypassCache = false)
        {
            if (_sectionTitleTypes == null)
            {
                _sectionTitleTypes = await _studentReferenceDataRepository.GetSectionTitleTypesAsync(bypassCache);
            }
            return _sectionTitleTypes;
        }

        private IEnumerable<Domain.Student.Entities.SectionDescriptionType> _sectionDescriptionTypes = null;
        private async Task<IEnumerable<Domain.Student.Entities.SectionDescriptionType>> SectionDescriptionTypesAsync(bool bypassCache = false)
        {
            if (_sectionDescriptionTypes == null)
            {
                _sectionDescriptionTypes = await _studentReferenceDataRepository.GetSectionDescriptionTypesAsync(bypassCache);
            }
            return _sectionDescriptionTypes;
        }
        public IEnumerable<Domain.Base.Entities.PersonNameTypeItem> _personNameTypes { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.PersonNameTypeItem>> GetPersonNameTypesAsync(bool bypassCache)
        {
            if (_personNameTypes == null)
            {
                _personNameTypes = await _referenceDataRepository.GetPersonNameTypesAsync(bypassCache);
            }
            return _personNameTypes;
        }


        /// <summary>
        /// Retrieves roster information for a course section.
        /// </summary>
        /// <param name="sectionId">ID of the course section for which roster students will be retrieved</param>
        /// <returns>All <see cref="RosterStudent">students</see> in the course section</returns>
        public async Task<IEnumerable<Dtos.Student.RosterStudent>> GetSectionRosterAsync(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId");
            }
            List<Dtos.Student.RosterStudent> results = new List<Dtos.Student.RosterStudent>();
            List<string> id = new List<string>() { sectionId };
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections;
            sections = await _sectionRepository.GetCachedSectionsAsync(id);

            if (sections.Count() > 0)
            {
                Domain.Student.Entities.Section section = sections.ElementAt(0);
                // determine the ID of the logged in entity
                string entity = CurrentUser.PersonId;
                if (section.ActiveStudentIds.Contains(entity) || section.FacultyIds.Contains(entity))
                {
                    var studentRostersEntities = await _studentRepository.GetRosterStudentsAsync(section.ActiveStudentIds);
                    foreach (var studentRosterEntity in studentRostersEntities)
                    {
                        results.Add(_adapterRegistry.GetAdapter<Domain.Student.Entities.RosterStudent, Dtos.Student.RosterStudent>().MapToType(studentRosterEntity));
                    }
                }
                else
                {
                    throw new PermissionsException("Requestor not authorized to view section roster.");
                }
            }
            else
            {
                throw new ApplicationException("Section not found in repository");
            }
            return results;
        }

        /// <summary>
        /// Get a <see cref="Dtos.Student.SectionRoster"/> for a given course section ID
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        /// <returns>A <see cref="Dtos.Student.SectionRoster"/></returns>
        public async Task<Dtos.Student.SectionRoster> GetSectionRoster2Async(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "Cannot build a section roster without a course section ID.");
            }

            SectionRoster entity = await _sectionRepository.GetSectionRosterAsync(sectionId);

            // Verify user is a student or faculty assigned to the course section
            if (!entity.StudentIds.Contains(CurrentUser.PersonId) && !entity.FacultyIds.Contains(CurrentUser.PersonId))
            {
                throw new PermissionsException(string.Format("Requestor is not authorized to view course section roster for section {0}.", sectionId));
            }

            Dtos.Student.SectionRoster dto = _adapterRegistry.GetAdapter<Domain.Student.Entities.SectionRoster, Dtos.Student.SectionRoster>().MapToType(entity);
            return dto;
        }

        /// <summary>
        /// Get a<see cref="Dtos.Student.SectionWaitlist"/> for a given course section ID
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        /// <returns>A<see cref="Dtos.Student.SectionWaitlist"/></returns>
        public async Task<Dtos.Student.SectionWaitlist> GetSectionWaitlistAsync(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "Cannot build a section waitlist without a course section ID.");
            }

            // Permission check
            var section = await _sectionRepository.GetSectionAsync(sectionId);
            CanViewSectionWaitlists(section, sectionId);
            SectionWaitlist entity = await _sectionRepository.GetSectionWaitlistAsync(sectionId);

            Dtos.Student.SectionWaitlist dto = _adapterRegistry.GetAdapter<Domain.Student.Entities.SectionWaitlist, Dtos.Student.SectionWaitlist>().MapToType(entity);
            return dto;
        }

        /// <summary>
        /// Get a list of<see cref="Dtos.Student.SectionWaitlistStudent"/> for a given course section ID
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        /// <returns>A list of<see cref="Dtos.Student.SectionWaitlistStudent"/></returns>
        public async Task<IEnumerable<Dtos.Student.SectionWaitlistStudent>> GetSectionWaitlist2Async(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "Cannot build a section waitlist without a course section ID.");
            }

            // Permission check
            var section = await _sectionRepository.GetSectionAsync(sectionId);
            CanViewSectionWaitlists(section, sectionId);
            IEnumerable<SectionWaitlistStudent> entity = await _sectionRepository.GetSectionWaitlist2Async(new List<string> { sectionId });
            List<Dtos.Student.SectionWaitlistStudent> waitlistDetails = new List<Dtos.Student.SectionWaitlistStudent>();
            foreach (var item in entity)
            {
                Dtos.Student.SectionWaitlistStudent dto = _adapterRegistry.GetAdapter<Domain.Student.Entities.SectionWaitlistStudent, Dtos.Student.SectionWaitlistStudent>().MapToType(item);
                waitlistDetails.Add(dto);
            }
            return waitlistDetails;
        }
        /// <summary>
        /// Gets a list of SectionWaitlistStudent for given sections or includes the cross listed sections data based on the query criteria
        /// </summary>
        /// <param name="criteria">SectionWaitlistQueryCriteria object</param>
        /// <returns>A list of Dtos.Student.SectionWaitlistStudent </returns>
        public async Task<IEnumerable<Dtos.Student.SectionWaitlistStudent>> GetSectionWaitlist3Async(Dtos.Student.SectionWaitlistQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Must supply a criteria to query waitlist data.");
            }
            if (criteria.SectionIds == null || !criteria.SectionIds.Any())
            {
                throw new ArgumentException("Must supply atleast one section to query waitlist data.");
            }
            List<Domain.Student.Entities.Section> sections = new List<Domain.Student.Entities.Section>();
            // Permission check
            foreach (var sectionId in criteria.SectionIds)
            {
                var section = await _sectionRepository.GetSectionAsync(sectionId);
                CanViewSectionWaitlists(section, sectionId);
                sections.Add(section);              
            }
            List<string> querySectionIds =  criteria.SectionIds;
            
            if(criteria.IncludeCrossListedSections && sections != null && sections.Any())
            {
                foreach (var sec in sections)
                {
                    if (sec.CrossListedSections != null && sec.CrossListedSections.Any())
                    {
                        querySectionIds.AddRange(sec.CrossListedSections.Select(x => x.Id));
                    }
                }

            }
            IEnumerable<SectionWaitlistStudent> entity = await _sectionRepository.GetSectionWaitlist2Async(querySectionIds);
            List<Dtos.Student.SectionWaitlistStudent> waitlistDetails = new List<Dtos.Student.SectionWaitlistStudent>();
            foreach (var item in entity)
            {
                Dtos.Student.SectionWaitlistStudent dto = _adapterRegistry.GetAdapter<Domain.Student.Entities.SectionWaitlistStudent, Dtos.Student.SectionWaitlistStudent>().MapToType(item);
                waitlistDetails.Add(dto);
            }
            return waitlistDetails;
        }

        /// <summary>
        /// Get a list of <see cref="Dtos.Student.StudentWaitlistStatus"/>
        /// </summary>
        /// <returns>A list of<see cref="Dtos.Student.StudentWaitlistStatus"/> </returns>
        public async Task<IEnumerable<Dtos.Student.StudentWaitlistStatus>> GetStudentWaitlistStatusesAsync()
        {
            List<Dtos.Student.StudentWaitlistStatus> studentWaitlistStatuses = new List<Dtos.Student.StudentWaitlistStatus>();
            IEnumerable<StudentWaitlistStatus> waitlistStatuses = await _sectionRepository.GetStudentWaitlistStatusesAsync();
            if (waitlistStatuses != null)
            {
                foreach (var item in waitlistStatuses)
                {
                    if (item != null)
                    {
                        Dtos.Student.StudentWaitlistStatus dto = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentWaitlistStatus, Dtos.Student.StudentWaitlistStatus>().MapToType(item);
                        studentWaitlistStatuses.Add(dto);
                    }
                }
            }
            return studentWaitlistStatuses;
        }

        /// <summary>
        /// Get a <see cref="Dtos.Student.SectionWaitlistConfig"/> for a given course section ID
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        /// <returns>A <see cref="Dtos.Student.SectionWaitlistConfig"/></returns>
        public async Task<Dtos.Student.SectionWaitlistConfig> GetSectionWaitlistConfigAsync(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "Cannot get a section waitlist config without a course section ID.");
            }

            // Permission check
            var section = await _sectionRepository.GetSectionAsync(sectionId);
            CanViewSectionWaitlists(section, sectionId);
            SectionWaitlistConfig entity = await _sectionRepository.GetSectionWaitlistConfigAsync(sectionId);

            Dtos.Student.SectionWaitlistConfig dto = _adapterRegistry.GetAdapter<Domain.Student.Entities.SectionWaitlistConfig, Dtos.Student.SectionWaitlistConfig>().MapToType(entity);
            return dto;
        }

        /// <summary>
        /// Get a StudentSectionWaitlistInfo based on section id and student id
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        /// <param name="studentId">student ID</param>
        /// <returns>StudentSectionWaitlistInfo DTO object</returns>
        public async Task<Dtos.Student.StudentSectionWaitlistInfo> GetStudentSectionWaitlistsByStudentAndSectionIdAsync(string sectionId, string studentId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentException("section Id argument is required");
            }
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentException("student Id argument is required");
            }
            // Make sure the person requesting the section waitlist information is the student.
            if (studentId != CurrentUser.PersonId)
            {
                var canAccess = await UserIsAdvisorAsync(studentId);
                if (!canAccess)
                {
                    var message = "Current user is not the student for the requested section waitlist information and therefore cannot access it.";
                    logger.Info(message);
                    throw new PermissionsException(message);
                }
            }
            try
            {
                var waitEntity = await _sectionRepository.GetStudentSectionWaitlistsByStudentAndSectionIdAsync(sectionId, studentId);
                if (waitEntity == null)
                {
                    throw new Exception("Failed to retrieve student section waitlist information");
                }
                Dtos.Student.StudentSectionWaitlistInfo dto = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentSectionWaitlistInfo, Dtos.Student.StudentSectionWaitlistInfo>().MapToType(waitEntity);
                return dto;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("student-section-waitlists not found for section and student id " + sectionId + " and " + studentId, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("student-section-waitlists not found for section and student id " + sectionId + " and " + studentId, ex);
            }
            catch (Exception ex)
            {
                var message = string.Format("Exception occurred while trying to fetch the student section waitlists using  student Id {0} and section Id {1}  Exception message: ", studentId, sectionId, ex.Message);
                logger.Error(ex, message);
                throw;
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to manage a section's textbook assignments
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CanViewSectionWaitlists(Domain.Student.Entities.Section section, string sectionId)
        {
            if (section != null && section.FacultyIds != null && section.FacultyIds.Contains(CurrentUser.PersonId)) return;
            string error = "Current user is not authorized to view waitlist students for the section: " + sectionId;
            logger.Error(error);
            throw new PermissionsException("Current user is not authorized to view waitlist students in the current section");
        }

        /// <summary>
        /// Imports students grades for a section. Calling user must have permission to update grades.
        /// </summary>
        /// <returns><see cref="Grade">StudentSectionGradeResponse</see></returns>
        public async Task<IEnumerable<Dtos.Student.SectionGradeResponse>> ImportGradesAsync(Dtos.Student.SectionGrades sectionGrades)
        {
            if (sectionGrades == null)
                throw new ArgumentNullException("sectionGrades");

            if (!await UserCanUpdateGradesAsync())
                throw new PermissionsException();

            // Convert to domain objects
            var gradeDomainAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Dtos.Student.SectionGrades, Ellucian.Colleague.Domain.Student.Entities.SectionGrades>();
            var domainSectionGrades = gradeDomainAdapter.MapToType(sectionGrades);

            // Import the grades
            // forceNoVerifyFlag false for default immediate verification, and false for check for locks. This maintains the behavior of the repository 
            // prior to the addition of the the arguments in v3 of the section/{sectionId}/grades endpoint. 
            // callerType ILP because only ILP functionality was supported in v1.
            // New argument with the v5 endpoint. Default to false for all prior versions.
            bool sendGradingCompleteEmail = false;
            var domainResponse = await _sectionRepository.ImportGradesAsync(domainSectionGrades, false, false, GradesPutCallerTypes.ILP, sendGradingCompleteEmail);

            // Convert domain response to DTO
            var gradeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionGradeResponse, Ellucian.Colleague.Dtos.Student.SectionGradeResponse>();
            List<Dtos.Student.SectionGradeResponse> dtoResponseCollection = new List<Dtos.Student.SectionGradeResponse>();
            if ((domainResponse != null) && (domainResponse.StudentResponses != null))
            {
                foreach (var response in domainResponse.StudentResponses)
                {
                    dtoResponseCollection.Add(gradeDtoAdapter.MapToType(response));
                }
            }

            return dtoResponseCollection;
        }

        /// <summary>
        /// Imports students grades for a section. Calling user must have permission to update grades.
        /// </summary>
        /// <returns><see cref="Grade">StudentSectionGradeResponse</see></returns>
        public async Task<IEnumerable<Dtos.Student.SectionGradeResponse>> ImportGrades2Async(Dtos.Student.SectionGrades2 sectionGrades)
        {
            if (sectionGrades == null)
                throw new ArgumentNullException("sectionGrades");

            if (!await UserCanUpdateGradesAsync())
                throw new PermissionsException();

            // Convert to domain objects
            var gradeDomainAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Dtos.Student.SectionGrades2, Ellucian.Colleague.Domain.Student.Entities.SectionGrades>();
            var domainSectionGrades = gradeDomainAdapter.MapToType(sectionGrades);

            // Import the grades
            // forceNoVerifyFlag false for default immediate verification, and false for check for locks. This maintains the behavior of the repository 
            // prior to the addition of the the arguments in v3 of the section/{sectionId}/grades endpoint. 
            // callerType ILP because only ILP functionality was supported in v2.
            // New argument with the v5 endpoint. Default to false for all prior versions.
            bool sendGradingCompleteEmail = false;
            var domainResponse = await _sectionRepository.ImportGradesAsync(domainSectionGrades, false, false, GradesPutCallerTypes.ILP, sendGradingCompleteEmail);

            // Convert domain response to DTO
            var gradeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionGradeResponse, Ellucian.Colleague.Dtos.Student.SectionGradeResponse>();
            List<Dtos.Student.SectionGradeResponse> dtoResponseCollection = new List<Dtos.Student.SectionGradeResponse>();
            if ((domainResponse != null) && (domainResponse.StudentResponses != null))
            {
                foreach (var response in domainResponse.StudentResponses)
                {
                    dtoResponseCollection.Add(gradeDtoAdapter.MapToType(response));
                }
            }

            return dtoResponseCollection;
        }

        /// <summary>
        /// Imports students grades for a section. Calling user must have permission to update grades.
        /// </summary>
        /// <param name="sectionGrades">DTO of section grade information</param>
        /// <returns><see cref="Grade">StudentSectionGradeResponse</see></returns>
        public async Task<IEnumerable<Dtos.Student.SectionGradeResponse>> ImportGrades3Async(Dtos.Student.SectionGrades3 sectionGrades)
        {
            if (sectionGrades == null)
                throw new ArgumentNullException("sectionGrades");

            // Can the user update grades for this section?
            if (!await UserCanUpdateGradesOfSectionAsync(sectionGrades.SectionId))
            {
                throw new PermissionsException();
            }

            // Convert to domain objects
            var gradeDomainAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Dtos.Student.SectionGrades3, Ellucian.Colleague.Domain.Student.Entities.SectionGrades>();
            var domainSectionGrades = gradeDomainAdapter.MapToType(sectionGrades);

            // Import the grades
            // forceNoVerifyFlag defaults to false if it was not supplied. False causes the configured immediate verification behavior to occur.
            bool forceNoVerifyFlag = (sectionGrades.ForceNoVerifyFlag.HasValue) ? sectionGrades.ForceNoVerifyFlag.Value : false;
            // The checkForLocksFlag is added and passed true starting with version 3 of the section/{sectionId}/grades endpoint.
            // callerType ILP because only ILP functionality was supported in v3.
            // New argument with the v5 endpoint. Default to false for all prior versions.
            bool sendGradingCompleteEmail = false;
            var domainResponse = await _sectionRepository.ImportGradesAsync(domainSectionGrades, forceNoVerifyFlag, true, GradesPutCallerTypes.ILP, sendGradingCompleteEmail);

            // Convert domain response to DTO
            var gradeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionGradeResponse, Ellucian.Colleague.Dtos.Student.SectionGradeResponse>();
            List<Dtos.Student.SectionGradeResponse> dtoResponseCollection = new List<Dtos.Student.SectionGradeResponse>();
            if ((domainResponse != null) && (domainResponse.StudentResponses != null))
            {
                foreach (var response in domainResponse.StudentResponses)
                {
                    dtoResponseCollection.Add(gradeDtoAdapter.MapToType(response));
                }
            }

            return dtoResponseCollection;
        }

        /// <summary>
        /// Imports students grades for a section from a standard non-ILP caller.
        /// </summary>
        /// <param name="sectionGrades">DTO of section grade information</param>
        /// <returns><see cref="Grade">StudentSectionGradeResponse</see></returns>
        public async Task<IEnumerable<Dtos.Student.SectionGradeResponse>> ImportGrades4Async(Dtos.Student.SectionGrades3 sectionGrades)
        {
            Dtos.Student.SectionGrades4 sectionGrades4 = new Dtos.Student.SectionGrades4();
            sectionGrades4.SectionId = sectionGrades.SectionId;
            sectionGrades4.ForceNoVerifyFlag = sectionGrades.ForceNoVerifyFlag;
            sectionGrades4.StudentGrades = sectionGrades.StudentGrades;
            // Pass property for newer API version as null. Appropriate default value will be used.
            sectionGrades4.SendGradingCompleteEmail = null;

            var sectionResponse = await this.ImportGradesFromIlpOrStandard(sectionGrades4, GradesPutCallerTypes.Standard);
            // This older endpoint version returns only StudentResponses
            return sectionResponse.StudentResponses;
        }

        /// <summary>
        /// Imports students grades for a section from a standard non-ILP caller.
        /// </summary>
        /// <param name="sectionGrades">DTO of section grade information</param>
        /// <returns><see cref="Grade">StudentSectionGradeResponse</see></returns>
        public async Task<Dtos.Student.SectionGradeSectionResponse> ImportGrades5Async(Dtos.Student.SectionGrades4 sectionGrades)
        {
            return await this.ImportGradesFromIlpOrStandard(sectionGrades, GradesPutCallerTypes.Standard);
        }

        /// <summary>
        /// Imports students grades for a section from an ILP caller.
        /// </summary>
        /// <param name="sectionGrades">DTO of section grade information</param>
        /// <returns><see cref="Grade">StudentSectionGradeResponse</see></returns>
        public async Task<IEnumerable<Dtos.Student.SectionGradeResponse>> ImportIlpGrades1Async(Dtos.Student.SectionGrades3 sectionGrades)
        {
            Dtos.Student.SectionGrades4 sectionGrades4 = new Dtos.Student.SectionGrades4();
            sectionGrades4.SectionId = sectionGrades.SectionId;
            sectionGrades4.ForceNoVerifyFlag = sectionGrades.ForceNoVerifyFlag;
            sectionGrades4.StudentGrades = sectionGrades.StudentGrades;
            // Pass property for newer API version as null. Appropriate default value will be used.
            sectionGrades4.SendGradingCompleteEmail = null;

            var sectionResponse = await this.ImportGradesFromIlpOrStandard(sectionGrades4, GradesPutCallerTypes.ILP);
            // This older endpoint version returns only StudentResponses
            return sectionResponse.StudentResponses;

        }

        /// <summary>
        /// Imports students grades for a section.
        /// Contains the common code shared by public method ImportGrades4Async and ImportIlpGrades1Async.
        /// </summary>
        /// <param name="sectionGrades">DTO of section grade information</param>
        /// <param name="callerType">Indicate the caller type</param>
        /// <returns><see cref="Grade">StudentSectionGradeResponse</see></returns>
        private async Task<Dtos.Student.SectionGradeSectionResponse> ImportGradesFromIlpOrStandard(Dtos.Student.SectionGrades4 sectionGrades, GradesPutCallerTypes callerType)
        {
            Dtos.Student.SectionGradeSectionResponse responseDto = new Dtos.Student.SectionGradeSectionResponse();

            if (sectionGrades == null)
                throw new ArgumentNullException("sectionGrades");

            // Can the user update grades for this section?
            if (!await UserCanUpdateGradesOfSectionAsync(sectionGrades.SectionId))
            {
                throw new PermissionsException();
            }

            // Convert to domain objects
            var gradeDomainAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Dtos.Student.SectionGrades4, Ellucian.Colleague.Domain.Student.Entities.SectionGrades>();
            var domainSectionGrades = gradeDomainAdapter.MapToType(sectionGrades);

            // Import the grades
            // forceNoVerifyFlag defaults to false if it was not supplied. False causes the configured immediate verification behavior to occur.
            bool forceNoVerifyFlag = (sectionGrades.ForceNoVerifyFlag.HasValue) ? sectionGrades.ForceNoVerifyFlag.Value : false;
            // The checkForLocksFlag is added and passed true starting with version 3 of the section/{sectionId}/grades endpoint.
            bool checkForLocksFlag = true;
            // sendGradingCompleteEmail defaults to false if it was not supplied. 
            bool sendGradingCompleteFlag = (sectionGrades.SendGradingCompleteEmail.HasValue) ? sectionGrades.SendGradingCompleteEmail.Value : false;

            var domainResponse = await _sectionRepository.ImportGradesAsync(domainSectionGrades, forceNoVerifyFlag, checkForLocksFlag, callerType, sendGradingCompleteFlag);

            // Add each student response from the entity to the return dto
            var gradeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionGradeResponse, Ellucian.Colleague.Dtos.Student.SectionGradeResponse>();
            if (domainResponse != null)
            {
                if (domainResponse.StudentResponses != null)
                {
                    foreach (var studentResponse in domainResponse.StudentResponses)
                    {
                        responseDto.StudentResponses.Add(gradeDtoAdapter.MapToType(studentResponse));
                    }
                }

                if (domainResponse.InformationalMessages != null)
                {
                    // Add each informational message from the entity to the return dto
                    foreach (string infoMsg in domainResponse.InformationalMessages)
                    {
                        responseDto.InformationalMessages.Add(infoMsg);
                    }
                }
            }

            return responseDto;
        }

        /// <summary>
        /// Checks whether the user has the "super" permission to update the grades of any section.
        /// 
        /// This was the only permission available when the v1 and v2 versions of the 
        /// PUT sections/{sectionId}/grades endpoint were created.
        /// As of v3 of the endpoint, UserCanUpdateGradesOfSectionAsync should be
        /// used. It also lets the users update grades of sections they teach regardless
        /// of permissions.
        /// </summary>
        /// <returns>true if the user has permission to update the grades of any section</returns>
        private async Task<bool> UserCanUpdateGradesAsync()
        {
            IEnumerable<string> userPermissions = await GetUserPermissionCodesAsync();

            if (userPermissions.Contains(SectionPermissionCodes.UpdateGrades))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check whether the current user has permissions to update grades of the specified section.
        /// </summary>
        /// <param name="sectionId">The section ID</param>
        /// <returns>true if the user can update grades of the section, else false</returns>
        private async Task<bool> UserCanUpdateGradesOfSectionAsync(string sectionId)
        {
            IEnumerable<string> userPermissions = await GetUserPermissionCodesAsync();

            // Does the user have the "super" permission to update grades of all sections?
            if (userPermissions.Contains(SectionPermissionCodes.UpdateGrades))
            {
                return true;
            }

            // Is the user an instructor of this section?
            List<string> id = new List<string>() { sectionId };
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections = await _sectionRepository.GetCachedSectionsAsync(id);
            if (sections != null && sections.Count() > 0)
            {
                Domain.Student.Entities.Section section = sections.ElementAt(0);
                string entity = CurrentUser.PersonId; // determine the ID of the logged in entity
                if (section != null && section.FacultyIds.Contains(entity))
                {
                    return true;
                }
            }

            return false;
        }

        #region HeDM Version 1-3

        /// <summary>
        /// Create a new section
        /// </summary>
        /// <param name="section">DTO containing the section to create</param>
        /// <returns>DTO containing the created section</returns>
        public async Task<Dtos.Section> PostSectionAsync(Dtos.Section section)
        {
            if (section == null)
            {
                throw new ArgumentNullException("section", "Section DTO is required for POST.");
            }
            // We must have a GUID
            if (string.IsNullOrEmpty(section.Guid))
            {
                throw new KeyNotFoundException("Section must provide a GUID.");
            }

        
            // Convert the CDM section into a domain entity and create it
            var entity = await ConvertSectionDtoToEntityAsync(section);
            var newEntity = await _sectionRepository.PostSectionAsync(entity);
            var newDto = await ConvertSectionEntityToDtoAsync(newEntity);

            return newDto;
        }

        /// <summary>
        /// Update a section
        /// </summary>
        /// <param name="section">DTO containing the section to update</param>
        /// <returns>DTO containing the updated section</returns>
        public async Task<Dtos.Section> PutSectionAsync(Dtos.Section section)
        {
            if (section == null)
            {
                throw new ArgumentNullException("section", "Section DTO is required for PUT.");
            }
            // We must have a GUID so we can get the existing data
            if (string.IsNullOrEmpty(section.Guid))
            {
                throw new KeyNotFoundException("Section must provide a GUID.");
            }

            // Convert the CDM section into a domain entity and update it
            var entity = await ConvertSectionDtoToEntityAsync(section);
            var updatedEntity = await _sectionRepository.PutSectionAsync(entity);
            var updatedDto = await ConvertSectionEntityToDtoAsync(updatedEntity);

            return updatedDto;

            //// Do this if we have to support partial updates
            //var entity = _sectionRepository.Get(section.Guid);
            //var existingDto = ConvertSectionEntityToDto(entity);
            //var updatedDto = UpdateDtoProperties<Dtos.Section>(existingDto, section);
            //var updatedEntity = ConvertSectionDtoToEntity(updatedDto);
            //var returnEntity = _sectionRepository.Update(updatedEntity);
            //var returnDto = ConvertSectionEntityToDto(returnEntity);
            //return returnDto;
        }

        /// <summary>
        /// Update a book assignment for a section.
        /// </summary>
        /// <param name="textbook">The textbook whose assignment to a specific section is being updated.</param>
        /// <returns>An updated <see cref="Section3"/> object.</returns>
        public async Task<Dtos.Student.Section3> UpdateSectionBookAsync(Dtos.Student.SectionTextbook textbook)
        {
            if (textbook == null)
            {
                throw new ArgumentNullException("textbook", "Textbook may not be null");
            }
            if (string.IsNullOrEmpty(textbook.SectionId))
            {
                throw new ArgumentNullException("textbook.SectionId", "Section Id may not be null or empty");
            }

            var textbookDtoAdapter = _adapterRegistry.GetAdapter<Dtos.Student.SectionTextbook, Domain.Student.Entities.SectionTextbook>();
            var textbookEntity = textbookDtoAdapter.MapToType(textbook);
            var bookAction = ConvertSectionBookActionDtoToDomainEntityAsync(textbook.Action);
            var section = await _sectionRepository.GetSectionAsync(textbook.SectionId);
            CanManageSectionTextbookAssignments(section);

            if (string.IsNullOrEmpty(textbook.Textbook.Id))
            {
                try
                {
                    var book = await _bookRepository.CreateBookAsync(textbookEntity);
                    textbookEntity = new Domain.Student.Entities.SectionTextbook(book, textbook.SectionId, textbook.RequirementStatusCode, bookAction);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to get the list of book options.");
                    throw;
                }
            }

            var sectionEntity = await _sectionRepository.UpdateSectionBookAsync(textbookEntity);

            // Get the right adapter for the type mapping
            var sectionDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Student.Section3>();

            // Map the section entity to the section Dto then set up response and cache for the single item.
            var sectionDto = sectionDtoAdapter.MapToType(sectionEntity);

            return sectionDto;
        }


        /// <summary>
        /// Convert the SectionBookAction DTO into a SectionBookAction Domain entity
        /// </summary>
        /// <param name="action">A SectionBookAction DTO</param>
        /// <returns>A SectionBookAction Domain entity entity</returns>
        private Domain.Student.Entities.SectionBookAction ConvertSectionBookActionDtoToDomainEntityAsync(Dtos.Student.SectionBookAction action)
        {
            switch (action)
            {
                case Dtos.Student.SectionBookAction.Remove:
                    return Domain.Student.Entities.SectionBookAction.Remove;
                case Dtos.Student.SectionBookAction.Add:
                    return Domain.Student.Entities.SectionBookAction.Add;
                default:
                    return Domain.Student.Entities.SectionBookAction.Update;
            }
        }

        //private T UpdateDtoProperties<T>(T source, T update)
        //    where T: class, new()
        //{
        //    if (source == null)
        //    {
        //        throw new ArgumentNullException("source", "Source DTO must be provided.");
        //    }

        //    T target = new T();
        //    Type targetType = target.GetType();
        //    PropertyInfo[] properties = targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        //    foreach (var property in properties)
        //    {
        //        // Only deal with properties that we can both read and write
        //        if (!property.CanRead || !property.CanWrite)
        //        {
        //            continue;
        //        }

        //        // Get the value of each property in the source DTO and the update DTO
        //        var sourceValue = property.GetValue(source, null);
        //        var updateValue = property.GetValue(update, null);

        //        // If the update value is null, then the source value is the update value
        //        if (updateValue == null)
        //        {
        //            updateValue = sourceValue;
        //        }
        //        // Update the target property
        //        property.SetValue(target, updateValue, null);

        //        // TODO JTM: Once we've figured out how to clear data values for other types
        //        // The above code can handle clearing of strings, but not other types - handle clearing
        //        // of other data types, indicated by ???
        //        //if (property.PropertyType == typeof(string))
        //        //{

        //        //}
        //    }

        //    return target;
        //}

        #endregion

        #region Convert HeDM Version 1-3

        /// <summary>
        /// Convert a Section entity into the CDM sections format DTO
        /// </summary>
        /// <param name="entity">A Section entity</param>
        /// <returns>A CDM-version Section DTO</returns>
        private async Task<Dtos.Section> ConvertSectionEntityToDtoAsync(Domain.Student.Entities.Section entity)
        {
            if (entity == null)
            {
                return null;
            }

            var sectionDto = new Dtos.Section();

            sectionDto.Guid = entity.Guid.ToLowerInvariant();
            sectionDto.Number = entity.Number;
            sectionDto.Title = entity.Title;
            sectionDto.StartDate = entity.StartDate;
            sectionDto.EndDate = entity.EndDate;
            sectionDto.Course = new Dtos.GuidObject(await _courseRepository.GetCourseGuidFromIdAsync(entity.CourseId));

            var credit = new Dtos.Credit();
            credit.CreditCategory = new Dtos.GuidObject();
            credit.CreditCategory.Guid = ConvertCodeToGuid(await CreditTypesAsync(), entity.CreditTypeCode);
            if (entity.Ceus.HasValue)
            {
                credit.Measure = Dtos.CreditMeasure.CEU;
                credit.Minimum = entity.Ceus.Value;
            }
            else
            {
                credit.Measure = Dtos.CreditMeasure.Credit;
                credit.Minimum = entity.MinimumCredits.GetValueOrDefault();
                credit.Maximum = entity.MaximumCredits;
                credit.Increment = entity.VariableCreditIncrement;
            }
            sectionDto.Credits.Add(credit);

            sectionDto.Site = new Dtos.GuidObject(ConvertCodeToGuid(locations, entity.Location));
            sectionDto.AcademicLevels.Add(new Dtos.GuidObject(ConvertCodeToGuid((await AcademicLevelsAsync()), entity.AcademicLevelCode)));
            var gradeScheme = (await GradeSchemesAsync()).Where(gs => gs != null && gs.Code == entity.GradeSchemeCode).FirstOrDefault();
            var gradeSchemeGuid = gradeScheme != null ? gradeScheme.Guid : null;
            sectionDto.GradeSchemes.Add(new Dtos.GuidObject(gradeSchemeGuid));
            foreach (var code in entity.CourseLevelCodes)
            {
                sectionDto.CourseLevels.Add(new Dtos.GuidObject(ConvertCodeToGuid(await CourseLevelsAsync(), code)));
            }
            string status;
            sectionDto.Status = ConvertSectionStatusToDto(entity.CurrentStatus, out status);
            sectionDto.MaximumEnrollment = entity.GlobalCapacity ?? entity.SectionCapacity;
            sectionDto.InstructionalEvents = entity.Meetings.Select(x => new Dtos.GuidObject(x.Guid)).ToList();
            foreach (var code in entity.Departments)
            {
                sectionDto.OwningOrganizations.Add(new Dtos.OfferingOrganization() { Guid = ConvertCodeToGuid(await DepartmentsAsync(), code.AcademicDepartmentCode), Share = code.ResponsibilityPercentage });
            }

            return sectionDto;
        }

        /// <summary>
        /// Convert the CDM sections format DTO into a Section entity
        /// </summary>
        /// <param name="entity">A CDM-version Section DTO</param>
        /// <returns>A Section entity</returns>
        private async Task<Domain.Student.Entities.Section> ConvertSectionDtoToEntityAsync(Dtos.Section sectionDto)
        {
            if (sectionDto == null)
            {
                throw new ArgumentNullException("sectionDto", "Section DTO must be provided.");
            }
            if (string.IsNullOrEmpty(sectionDto.Guid))
            {
                throw new ArgumentException("Section GUID not specified.");
            }

            Domain.Student.Entities.Section section = null;

            try
            {
                section = await _sectionRepository.GetSectionByGuidAsync(sectionDto.Guid);
            }
            catch (Exception) { }

            string id = section == null ? null : section.Id;
            var currentStatuses = section == null || section.Statuses == null ? new List<SectionStatusItem>() : section.Statuses.ToList();
            var curriculumConfiguration = await GetCurriculumConfigurationAsync();
            var statuses = UpdateSectionStatus(currentStatuses, sectionDto.Status, curriculumConfiguration.SectionActiveStatusCode, curriculumConfiguration.SectionInactiveStatusCode);

            var course = await _courseRepository.GetCourseByGuidAsync(sectionDto.Course.Guid);
            string creditType = null;
            decimal? minCredits = null;
            decimal? maxCredits = null;
            decimal? creditIncr = null;
            decimal? ceus = null;
            if (sectionDto.Credits != null && sectionDto.Credits.Any())
            {
                if (sectionDto.Credits[0] == null || sectionDto.Credits[0].CreditCategory == null || !sectionDto.Credits[0].Measure.HasValue)
                {
                    throw new ArgumentException("Credits data is required.");
                }
                creditType = ConvertGuidToCode((await CreditTypesAsync()), sectionDto.Credits[0].CreditCategory.Guid);
                if (sectionDto.Credits[0].Measure == Dtos.CreditMeasure.CEU)
                {
                    ceus = sectionDto.Credits[0].Minimum;
                }
                else
                {
                    minCredits = sectionDto.Credits[0].Minimum;
                    maxCredits = sectionDto.Credits[0].Maximum;
                    creditIncr = sectionDto.Credits[0].Increment;
                }
            }

            var departmentsToConvert = await DepartmentsAsync();

            // Use the department values on the course; if null, use the default from the DTO
            List<OfferingDepartment> offeringDepartments = (course.Departments != null && course.Departments.Any()) ? course.Departments.ToList() :
                sectionDto.OwningOrganizations.Select(x => new OfferingDepartment(ConvertGuidToCode(departmentsToConvert, x.Guid), x.Share)).ToList();
            var courseLevels = await CourseLevelsAsync();
            // Convert various codes to their Colleague values or get them from the course
            List<string> courseLevelCodes = (sectionDto.CourseLevels == null || sectionDto.CourseLevels.Count == 0) ? course.CourseLevelCodes :
                sectionDto.CourseLevels.Select(x => ConvertGuidToCode(courseLevels, x.Guid)).ToList();
            string academicLevel = (sectionDto.AcademicLevels == null || sectionDto.AcademicLevels.Count == 0) ?
                course.AcademicLevelCode : ConvertGuidToCode((await AcademicLevelsAsync()), sectionDto.AcademicLevels[0].Guid);

            var gradeSchemeEntity = (await GradeSchemesAsync()).Where(gs => gs != null && gs.Guid == sectionDto.GradeSchemes[0].Guid).FirstOrDefault();
            var gradeSchemeCodeFromEntity = gradeSchemeEntity != null ? gradeSchemeEntity.Code : null;
            string gradeScheme = (sectionDto.GradeSchemes == null || sectionDto.GradeSchemes.Count == 0) ? course.GradeSchemeCode :
                gradeSchemeCodeFromEntity;
            string site = sectionDto.Site == null ? null : ConvertGuidToCode(locations, sectionDto.Site.Guid);

            // Create the section entity
            var entity = new Domain.Student.Entities.Section(id, course.Id, sectionDto.Number, sectionDto.StartDate.GetValueOrDefault(),
                minCredits, ceus, sectionDto.Title, creditType, offeringDepartments, courseLevelCodes, academicLevel, statuses,
                course.AllowPassNoPass, course.AllowAudit, course.OnlyPassNoPass, course.AllowWaitlist, false)
            {
                Guid = sectionDto.Guid.ToLowerInvariant(),
                EndDate = sectionDto.EndDate,
                MaximumCredits = maxCredits,
                VariableCreditIncrement = creditIncr,
                GradeSchemeCode = gradeScheme,
                TopicCode = course.TopicCode,
                GlobalCapacity = sectionDto.MaximumEnrollment,
                SectionCapacity = sectionDto.MaximumEnrollment,
                Location = site
            };
            foreach (var item in course.Types)
            {
                entity.AddCourseType(item);
            }
            entity.TermId = SectionProcessor.DetermineTerm(entity, await AllTermsAsync());
            if (entity.EndDate.HasValue)
            {
                entity.NumberOfWeeks = (int)Math.Ceiling((entity.EndDate.Value - entity.StartDate).Days / 7m);
            }

            return entity;
        }

        private List<SectionStatusItem> UpdateSectionStatus(List<SectionStatusItem> currentStatuses, Dtos.SectionStatus? dtoStatus,
            string activeStatusCode, string inactiveStatusCode)
        {
            var statuses = new List<SectionStatusItem>();
            string code;
            var newStatus = ConvertSectionStatusFromDto(dtoStatus, out code);
            if (currentStatuses == null || currentStatuses.Count == 0)
            {
                // No existing statuses
                if (newStatus.HasValue)
                {
                    // We have a new status - add it with today's date
                    statuses.Add(new SectionStatusItem(newStatus.Value, activeStatusCode, DateTime.Today));
                }
            }
            else
            {
                var currentStatus = currentStatuses[0].Status;
                if (newStatus.HasValue && newStatus.Value != currentStatus)
                {
                    // New status found and it's different - add it to the etop of the list
                    var status = currentStatus == Domain.Student.Entities.SectionStatus.Active ? inactiveStatusCode : activeStatusCode;
                    statuses.Add(new SectionStatusItem(newStatus.Value, status, DateTime.Today));
                    statuses.AddRange(currentStatuses);
                }
                else
                {
                    // No new value - the current statuses haven't changed
                    statuses = currentStatuses;
                }
            }

            return statuses;
        }

        private Dtos.SectionStatus? ConvertSectionStatusToDto(Domain.Student.Entities.SectionStatus status, out string statusCode)
        {

            statusCode = status.ToString();
            switch (status)
            {
                case Domain.Student.Entities.SectionStatus.Active:
                    return Dtos.SectionStatus.Open;
                case Domain.Student.Entities.SectionStatus.Cancelled:
                    return Dtos.SectionStatus.Cancelled;
                case Domain.Student.Entities.SectionStatus.Inactive:
                default:
                    return Dtos.SectionStatus.Closed;
            }
        }

        private Domain.Student.Entities.SectionStatus? ConvertSectionStatusFromDto(Dtos.SectionStatus? status, out string statusCode)
        {
            statusCode = null;
            if (!status.HasValue)
            {
                return null;
            }
            switch (status)
            {
                case Dtos.SectionStatus.Open:
                    return Domain.Student.Entities.SectionStatus.Active;
                case Dtos.SectionStatus.Cancelled:
                    return Domain.Student.Entities.SectionStatus.Cancelled;
                default:
                    return Domain.Student.Entities.SectionStatus.Inactive;
            }
        }

        #endregion

        #region HeDM Version 4

        /// <summary>
        /// Get selection criteria, convert it to codes as required and select 
        /// multiple sections from Colleague.
        /// </summary>
        /// <param name="title">Section Title Contains ...title...</param>
        /// <param name="startOn">Section starts on or after this date</param>
        /// <param name="endOn">Section ends on or before this date</param>
        /// <param name="code">Section Name Contains ...code...</param>
        /// <param name="number">Section Number equal to</param>
        /// <param name="instructionalPlatform">Learning Platform equal to (guid)</param>
        /// <param name="academicPeriod">Section Term equal to (guid)</param>
        /// <param name="academicLevel">Section Academic Level equal to (guid)</param>
        /// <param name="course">Section Course equal to (guid)</param>
        /// <param name="site">Section Location equal to (guid)</param>
        /// <param name="status">Section Status matches closed, open, pending, or cancelled</param>
        /// <param name="owningOrganization">Section Department equal to (guid)</param>
        /// <returns>List of Section2 <see cref="Dtos.Section2"/> objects representing matching sections</returns>
        public async Task<Tuple<IEnumerable<Dtos.Section2>, int>> GetSections2Async(int offset, int limit, string title = "", string startOn = "", string endOn = "",
            string code = "", string number = "", string instructionalPlatform = "", string academicPeriod = "",
            string academicLevel = "", string course = "", string site = "", string status = "", string owningOrganization = "")
        {
            // Convert and validate all input parameters
            var newStartOn = (startOn == string.Empty ? string.Empty : await ConvertDateArgument(startOn));
            var newEndOn = (endOn == string.Empty ? string.Empty : await ConvertDateArgument(endOn));
            var newInstructionalPlatform = (instructionalPlatform == string.Empty ? string.Empty : ConvertGuidToCode((await InstructionalPlatformsAsync()), instructionalPlatform));
            var newAcademicPeriod = (academicPeriod == string.Empty ? string.Empty : ConvertGuidToCode(academicPeriods, academicPeriod));
            var newAcademicLevel = (academicLevel == string.Empty ? string.Empty : ConvertGuidToCode((await AcademicLevelsAsync()), academicLevel));
            var newCourse = (course == string.Empty ? string.Empty : await ConvertCourseArgument(course));
            var newSite = (site == string.Empty ? string.Empty : ConvertGuidToCode(locations, site));
            var newStatus = (status == string.Empty ? string.Empty : await ConvertStatusArgument(status));
            var newOwningOrganization = (owningOrganization == string.Empty ? string.Empty : ConvertGuidToCode((await DepartmentsAsync()), owningOrganization));

            var sectionDtos = new List<Dtos.Section2>();
            var sectionEntities = await _sectionRepository.GetSectionsAsync(offset, limit, title, newStartOn, newEndOn,
                code, number, newInstructionalPlatform, newAcademicPeriod,
                newAcademicLevel, newCourse, newSite, newStatus, newOwningOrganization);

            foreach (var sectionEntity in sectionEntities.Item1)
            {
                if (sectionEntity.Guid != null)
                {
                    var sectionDto = await ConvertSectionEntityToDto2Async(sectionEntity);
                    sectionDtos.Add(sectionDto);
                }
            }
            return new Tuple<IEnumerable<Dtos.Section2>, int>(sectionDtos, sectionEntities.Item2);
        }

        #endregion

        #region Convert HeDM Version 4

        /// <summary>
        /// Convert a Section entity into the HeDM SectionMaximum format DTO
        /// </summary>
        /// <param name="entity">A Section entity</param>
        /// <returns>A HeDM-version 4 SectionMaximum</returns>
        private async Task<Dtos.SectionMaximum> ConvertSectionEntityToSectionMaximumAsync(Domain.Student.Entities.Section entity)
        {
            if (entity == null)
            {
                return null;
            }

            var sectionDto = new Dtos.SectionMaximum();

            sectionDto.Id = entity.Guid.ToLowerInvariant();
            sectionDto.Code = entity.Name;
            sectionDto.Number = entity.Number;
            sectionDto.Title = entity.Title;
            sectionDto.StartOn = entity.StartDate;
            sectionDto.EndOn = entity.EndDate;

            if (!string.IsNullOrEmpty(entity.LearningProvider))
            {
                var instructionalPlatform = (await InstructionalPlatformsAsync()).FirstOrDefault(i => i.Code == entity.LearningProvider);

                sectionDto.InstructionalPlatform = (instructionalPlatform != null)
                    ? new InstructionalPlatformDtoProperty()
                    {
                        Code = instructionalPlatform.Code,
                        Title = instructionalPlatform.Description,
                        Detail = new GuidObject2(instructionalPlatform.Guid)
                    }
                    : null;
            }

            if (!string.IsNullOrEmpty(entity.TermId))
            {
                var academicPeriod = academicPeriods.FirstOrDefault(a => a.Code == entity.TermId);
                sectionDto.AcademicPeriod = (academicPeriod != null)
                    ? new AcademicPeriodDtoProperty()
                    {
                        Detail = new GuidObject2(academicPeriod.Guid),
                        Code = academicPeriod.Code,
                        Start = academicPeriod.StartDate,
                        End = academicPeriod.EndDate,
                        Title = academicPeriod.Description
                    }
                    : null;
            }


            var courseGuid = await _courseRepository.GetCourseGuidFromIdAsync(entity.CourseId);
            if (string.IsNullOrEmpty(courseGuid))
            {
                throw new RepositoryException(string.Concat("Course guid not found for Colleague Course Id : ", entity.CourseId));
            }

            var course = await _courseRepository.GetCourseByGuidAsync(courseGuid);
            if (course != null)
            {
                var courseReturn = new CourseDtoProperty()
                {
                    Detail = new GuidObject2(course.Guid),
                    Number = course.Number,
                    Title = course.Title
                };
                var subject = (await _studentReferenceDataRepository.GetSubjectsAsync()).Where(s => s.Code == course.SubjectCode).FirstOrDefault();
                if (subject != null)
                {
                    courseReturn.Subject = new SubjectDtoProperty()
                    {
                        Abbreviation = subject.Code,
                        Title = subject.Description,
                        Detail = new GuidObject2(subject.Guid)
                    };
                }
                sectionDto.Course = courseReturn;
            }
            else
            {
                throw new RepositoryException(string.Concat("Course not found for Id : ", courseGuid));
            }

            var credit = new CreditDtoProperty();
            var creditTypeItems = await CreditTypesAsync();
            if (creditTypeItems.Any(ct => ct.Code == entity.CreditTypeCode))
            {
                var creditTypeItem = creditTypeItems.Where(ct => ct.Code == entity.CreditTypeCode).First();
                credit.CreditCategory.Detail.Id = creditTypeItem.Guid;
                credit.CreditCategory.Code = creditTypeItem.Code;
                credit.CreditCategory.Title = creditTypeItem.Description;
                switch (creditTypeItem.CreditType)
                {
                    case CreditType.ContinuingEducation:
                        credit.CreditCategory.CreditType = CreditCategoryType2.ContinuingEducation;
                        break;
                    case CreditType.Institutional:
                        credit.CreditCategory.CreditType = CreditCategoryType2.Institutional;
                        break;
                    case CreditType.Transfer:
                        credit.CreditCategory.CreditType = CreditCategoryType2.Transfer;
                        break;
                    default:
                        credit.CreditCategory.CreditType = CreditCategoryType2.ContinuingEducation;
                        break;
                }
            }
            else
            {
                //default to CE
                credit.CreditCategory.CreditType = CreditCategoryType2.ContinuingEducation;
            }

            if (entity.Ceus.HasValue)
            {
                credit.Measure = Dtos.CreditMeasure2.CEU;
                credit.Minimum = entity.Ceus.Value;
            }
            else
            {
                credit.Measure = Dtos.CreditMeasure2.Credit;
                credit.Minimum = entity.MinimumCredits.GetValueOrDefault();
                credit.Maximum = entity.MaximumCredits;
                credit.Increment = entity.VariableCreditIncrement;
            }
            var creditList = new List<CreditDtoProperty>();
            creditList.Add(credit);
            sectionDto.Credits = creditList;

            var site = new SiteDtoProperty();

            var location = locations.Where(l => l.Code == entity.Location).FirstOrDefault();
            if (location != null)
            {
                site.Code = location.Code;
                site.Title = location.Description;
                site.Detail = new GuidObject2(location.Guid);

                sectionDto.Site = site;
            }

            var acadLevel = new AcademicLevelDtoProperty();

            var level = (await AcademicLevelsAsync()).Where(l => l.Code == entity.AcademicLevelCode).FirstOrDefault();
            if (level != null)
            {
                acadLevel.Code = level.Code;
                acadLevel.Title = level.Description;
                acadLevel.Detail = new GuidObject2(level.Guid);

                sectionDto.AcademicLevels = new List<AcademicLevelDtoProperty>()
                {
                   acadLevel
                };
            }

            var awardScheme = new GradeSchemeDtoProperty();

            var gradeScheme = (await GradeSchemesAsync()).Where(g => g.Code == entity.GradeSchemeCode).FirstOrDefault();
            if (gradeScheme != null)
            {
                awardScheme.Code = gradeScheme.Code;
                awardScheme.Title = gradeScheme.Description;
                awardScheme.Start = gradeScheme.EffectiveStartDate;
                awardScheme.End = gradeScheme.EffectiveEndDate;
                awardScheme.Detail = new GuidObject2(gradeScheme.Guid);
                awardScheme.AcademicLevel = acadLevel;

                sectionDto.AwardGradeSchemes = new List<GradeSchemeDtoProperty>()
                {
                   awardScheme
                };

                sectionDto.TranscriptGradeSchemes = new List<GradeSchemeDtoProperty>()
                {
                   awardScheme
                };
            }

            var courseLevels = await CourseLevelsAsync();
            if (courseLevels.Any())
            {
                var courseLevelList = new List<CourseLevelDtoProperty>();
                foreach (var code in entity.CourseLevelCodes)
                {
                    var cLevel = courseLevels.Where(l => l.Code == code).FirstOrDefault();
                    if (cLevel != null)
                    {
                        var levelReturn = new CourseLevelDtoProperty();
                        levelReturn.Code = cLevel.Code;
                        levelReturn.Title = cLevel.Description;
                        levelReturn.Detail = new GuidObject2(cLevel.Guid);
                        courseLevelList.Add(levelReturn);
                    }
                }
                sectionDto.CourseLevels = courseLevelList;
            }

            string status;
            sectionDto.Status = ConvertSectionStatusToDto2(entity.Statuses.ElementAt(0).IntegrationStatus, out status);
            sectionDto.MaximumEnrollment = entity.GlobalCapacity ?? entity.SectionCapacity;

            if (entity.NumberOfWeeks.HasValue && entity.NumberOfWeeks > 0)
            {
                sectionDto.Duration = new SectionDuration2()
                {
                    Length = entity.NumberOfWeeks.Value,
                    Unit = DurationUnit2.Weeks
                };
            }

            var orgs = await DepartmentsAsync();
            if (orgs.Any())
            {
                var orgsList = new List<OwningOrganizationDtoProperty>();
                foreach (var code in entity.Departments)
                {
                    var owningOrg = orgs.Where(o => o.Code == code.AcademicDepartmentCode).FirstOrDefault();
                    if (owningOrg != null)
                    {
                        var orgReturn = new OwningOrganizationDtoProperty();
                        orgReturn.Code = owningOrg.Code;
                        orgReturn.Title = owningOrg.Description;
                        orgReturn.OwnershipPercentage = code.ResponsibilityPercentage;
                        orgReturn.Detail = new GuidObject2(owningOrg.Guid);
                        orgsList.Add(orgReturn);
                    }
                }
                sectionDto.OwningOrganizations = orgsList;
            }

            var pageOfItems = await _sectionRepository.GetSectionMeetingAsync(0, 0, entity.Id, string.Empty, string.Empty, string.Empty, string.Empty, new List<string>(), new List<string>(), new List<string>(), string.Empty);
            var eventEntities = pageOfItems.Item1;

            if (eventEntities.Any())
            {
                var instructionalEventsList = new List<InstructionalEventDtoProperty>();

                foreach (var eventInstructional in eventEntities)
                {
                    if (eventInstructional.Guid != null)
                    {
                        var returnInstructionalEventDto = new InstructionalEventDtoProperty();
                        var instEventDto = await ConvertSectionMeetingToInstructionalEvent2Async(eventInstructional);

                        returnInstructionalEventDto.Detail = new GuidObject2(instEventDto.Id);
                        returnInstructionalEventDto.Title = instEventDto.Title;
                        var instMethod = (await this.InstructionalMethodsAsync())
                            .FirstOrDefault(im => im.Guid == instEventDto.InstructionalMethod.Id);

                        if (instMethod != null)
                        {
                            returnInstructionalEventDto.InstructionalMethod = new InstructionalMethodDtoProperty()
                            {
                                Abbreviation = instMethod.Code,
                                Title = instMethod.Description,
                                Detail = new GuidObject2(instMethod.Guid)
                            };
                        }

                        if (instEventDto.Recurrence != null)
                        {
                            returnInstructionalEventDto.Recurrence = instEventDto.Recurrence;
                        }

                        if (instEventDto.Locations.Any())
                        {
                            var locationsList = new List<LocationDtoProperty>();
                            var locationDtoProperty = new LocationDtoProperty();
                            var locationRoom = new LocationRoomDtoProperty();
                            var room = (await RoomsAsync()).Where(r => r.Code == eventInstructional.Room).FirstOrDefault();

                            if (room != null)
                            {
                                locationRoom.LocationType = InstructionalLocationType.InstructionalRoom;
                                locationRoom.Title = room.Name;
                                locationRoom.Number = room.Number;
                                locationRoom.Floor = room.Floor;
                                locationRoom.Detail = new GuidObject2(room.Guid);

                                var building = (await _referenceDataRepository.BuildingsAsync())
                                    .Where(b => b.Code == room.BuildingCode).FirstOrDefault();
                                if (building != null)
                                {
                                    locationRoom.Building = new BuildingDtoProperty()
                                    {
                                        Code = building.Code,
                                        Title = building.Description,
                                        Detail = new GuidObject2(building.Guid)
                                    };
                                }

                                locationDtoProperty.Location = locationRoom;
                                locationsList.Add(locationDtoProperty);
                            }
                            returnInstructionalEventDto.Locations = locationsList;

                        }

                        if (instEventDto.Instructors.Any())
                        {
                            var instructorList = new List<InstructorRosterDtoProperty>();

                            foreach (var eventInstructor in instEventDto.Instructors)
                            {
                                var returnInstructorRoster = new InstructorRosterDtoProperty();
                                returnInstructorRoster.WorkLoadPercentage = eventInstructor.WorkLoadPercentage;
                                returnInstructorRoster.ResponsibilityPercentage = eventInstructor.ResponsibilityPercentage;
                                returnInstructorRoster.WorkStartDate = eventInstructor.WorkStartDate;
                                returnInstructorRoster.WorkEndDate = eventInstructor.WorkEndDate;

                                var returnInstructor = new InstructorDtoProperty();
                                returnInstructor.Detail = new GuidObject2(eventInstructor.Instructor.Id);
                                var namesList = new List<InstructorNameDtoProperty>();
                                var instructorName = new InstructorNameDtoProperty();

                                var person = await _personRepository.GetPersonByGuidNonCachedAsync(eventInstructor.Instructor.Id);
                                instructorName.NameType = InstructorNameType.Primary;

                                if (!string.IsNullOrEmpty(person.Prefix))
                                {
                                    var prefixEntity = _referenceDataRepository.Prefixes.FirstOrDefault(p => p.Abbreviation == person.Prefix);
                                    if (prefixEntity != null && !string.IsNullOrEmpty(prefixEntity.Code))
                                    {
                                        instructorName.Title = prefixEntity.Code;
                                    }
                                }

                                instructorName.FirstName = string.IsNullOrEmpty(person.FirstName) ? "" : person.FirstName;
                                instructorName.MiddleName = string.IsNullOrEmpty(person.MiddleName) ? null : person.MiddleName;
                                instructorName.LastNamePrefix = null;
                                instructorName.LastName = person.LastName;
                                instructorName.PreferredName = string.IsNullOrEmpty(person.Nickname) ? null : person.Nickname;

                                if (!string.IsNullOrEmpty(person.Suffix))
                                {
                                    var suffixEntity = _referenceDataRepository.Prefixes.FirstOrDefault(p => p.Abbreviation == person.Suffix);
                                    if (suffixEntity != null && !string.IsNullOrEmpty(suffixEntity.Code))
                                    {
                                        instructorName.Pedigree = suffixEntity.Code;
                                    }
                                }
                                namesList.Add(instructorName);
                                returnInstructor.Names = namesList;

                                var credentialList = new List<CredentialDtoProperty>();

                                credentialList.Add(new CredentialDtoProperty()
                                {
                                    Type = Dtos.EnumProperties.CredentialType.ColleaguePersonId,
                                    Value = person.Id
                                });

                                if (person.PersonAltIds != null && person.PersonAltIds.Any())
                                {
                                    var elevPersonAltId = person.PersonAltIds.FirstOrDefault(a => a.Type == Domain.Base.Entities.PersonAlt.ElevatePersonAltType);
                                    if (elevPersonAltId != null && !string.IsNullOrEmpty(elevPersonAltId.Id))
                                    {
                                        credentialList.Add(new CredentialDtoProperty()
                                        {
                                            Type = Dtos.EnumProperties.CredentialType.ElevateID,
                                            Value = elevPersonAltId.Id
                                        });
                                    }
                                }

                                if (!string.IsNullOrEmpty(person.GovernmentId))
                                {
                                    credentialList.Add(new CredentialDtoProperty()
                                    {
                                        Type = Dtos.EnumProperties.CredentialType.Ssn,
                                        Value = person.GovernmentId
                                    });
                                }

                                returnInstructor.Credentials = credentialList;
                            }
                            returnInstructionalEventDto.InstructorRoster = instructorList;

                        }


                        instructionalEventsList.Add(returnInstructionalEventDto);
                    }
                }

                if (instructionalEventsList.Any())
                {
                    sectionDto.InstructionalEvents = instructionalEventsList;
                }
            }

            return sectionDto;
        }

        /// <summary>
        /// Convert a Section entity into the HeDM sections format DTO
        /// </summary>
        /// <param name="entity">A Section entity</param>
        /// <returns>A HeDM-version 4 Section DTO</returns>
        private async Task<Dtos.Section2> ConvertSectionEntityToDto2Async(Domain.Student.Entities.Section entity)
        {
            if (entity == null)
            {
                return null;
            }

            var sectionDto = new Dtos.Section2();

            sectionDto.Id = entity.Guid.ToLowerInvariant();
            sectionDto.Code = entity.Name;
            sectionDto.Number = entity.Number;
            sectionDto.Title = entity.Title;
            sectionDto.StartOn = entity.StartDate;
            sectionDto.EndOn = entity.EndDate;

            if (!string.IsNullOrEmpty(entity.TermId))
            {
                sectionDto.AcademicPeriod = new Dtos.GuidObject2();
                var academicPeriod = academicPeriods.FirstOrDefault(a => a.Code == entity.TermId);
                sectionDto.AcademicPeriod.Id = (academicPeriod != null) ? academicPeriod.Guid : null;
            }

            if (!string.IsNullOrEmpty(entity.LearningProvider))
            {
                var instructionalPlatform = (await InstructionalPlatformsAsync()).FirstOrDefault(i => i.Code == entity.LearningProvider);
                if (instructionalPlatform != null && !string.IsNullOrEmpty(instructionalPlatform.Guid))
                {
                    sectionDto.InstructionalPlatform = new Dtos.GuidObject2(instructionalPlatform.Guid);
                }
            }
            sectionDto.Course = new Dtos.GuidObject2(await _courseRepository.GetCourseGuidFromIdAsync(entity.CourseId));

            var credit = new Dtos.Credit2();
            credit.CreditCategory = new CreditIdAndTypeProperty();
            var creditTypeItems = await CreditTypesAsync();
            if (creditTypeItems.Any(ct => ct.Code == entity.CreditTypeCode))
            {
                var creditTypeItem = creditTypeItems.Where(ct => ct.Code == entity.CreditTypeCode).First();
                credit.CreditCategory.Detail = new GuidObject2(creditTypeItem.Guid);
                switch (creditTypeItem.CreditType)
                {
                    case CreditType.ContinuingEducation:
                        credit.CreditCategory.CreditType = CreditCategoryType2.ContinuingEducation;
                        break;
                    case CreditType.Institutional:
                        credit.CreditCategory.CreditType = CreditCategoryType2.Institutional;
                        break;
                    case CreditType.Transfer:
                        credit.CreditCategory.CreditType = CreditCategoryType2.Transfer;
                        break;
                    default:
                        credit.CreditCategory.CreditType = CreditCategoryType2.ContinuingEducation;
                        break;
                }
            }
            else
            {
                //default to CE
                credit.CreditCategory.CreditType = CreditCategoryType2.ContinuingEducation;
            }

            if (entity.Ceus.HasValue)
            {
                credit.Measure = Dtos.CreditMeasure2.CEU;
                credit.Minimum = entity.Ceus.Value;
            }
            else
            {
                credit.Measure = Dtos.CreditMeasure2.Credit;
                credit.Minimum = entity.MinimumCredits.GetValueOrDefault();
                credit.Maximum = entity.MaximumCredits;
                credit.Increment = entity.VariableCreditIncrement;
            }
            sectionDto.Credits.Add(credit);

            if (!string.IsNullOrEmpty(entity.Location))
            {
                sectionDto.Site = new Dtos.GuidObject2(ConvertCodeToGuid(locations, entity.Location));
            }
            sectionDto.AcademicLevels.Add(new Dtos.GuidObject2(ConvertCodeToGuid((await AcademicLevelsAsync()), entity.AcademicLevelCode)));
            var gradeScheme = (await GradeSchemesAsync()).Where(gs => gs != null && gs.Code == entity.GradeSchemeCode).FirstOrDefault();
            var gradeSchemeGuid = gradeScheme != null ? gradeScheme.Guid : null;
            sectionDto.GradeSchemes.Add(new Dtos.GuidObject2(gradeSchemeGuid));
            foreach (var code in entity.CourseLevelCodes)
            {
                sectionDto.CourseLevels.Add(new Dtos.GuidObject2(ConvertCodeToGuid(await CourseLevelsAsync(), code)));
            }
            string status;
            sectionDto.Status = ConvertSectionStatusToDto2(entity.Statuses.ElementAt(0).IntegrationStatus, out status);
            sectionDto.MaximumEnrollment = entity.GlobalCapacity ?? entity.SectionCapacity;

            foreach (var code in entity.Departments)
            {
                sectionDto.OwningOrganizations.Add(new Dtos.OfferingOrganization2() { Organization = new GuidObject2(ConvertCodeToGuid(await DepartmentsAsync(), code.AcademicDepartmentCode)), Share = code.ResponsibilityPercentage });
            }

            return sectionDto;
        }

        private async Task<List<SectionStatusItem>> UpdateSectionStatus2(List<SectionStatusItem> currentStatuses, Dtos.SectionStatus2? dtoStatus)
        //string activeStatusCode, string inactiveStatusCode)
        {
            var statuses = new List<SectionStatusItem>();
            string code;
            var newStatus = ConvertSectionStatusFromDto2(dtoStatus, out code);
            if (currentStatuses == null || currentStatuses.Count == 0)
            {
                // No existing statuses
                if (newStatus.HasValue)
                {
                    // We have a new status - add it with today's date
                    var status = (await SectionStatusesAsync()).FirstOrDefault(i => i.IntegrationStatusType.Value.ToString().Equals(newStatus.Value.ToString()));
                    if (status == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Section status code found for: ", newStatus.Value.ToString()));
                    }

                    statuses.Add(new SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, newStatus.Value, status.Code, DateTime.Today));
                }
            }
            else
            {
                var currentStatus = currentStatuses[0];
                if (newStatus.HasValue && newStatus.Value != currentStatus.IntegrationStatus)
                {
                    // New status found and it's different - add it to the etop of the list
                    var status = (await SectionStatusesAsync()).FirstOrDefault(i => i.IntegrationStatusType.Value.ToString().Equals(newStatus.Value.ToString()));
                    if (status == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Section status code found for: ", newStatus.Value.ToString()));
                    }

                    statuses.Add(new SectionStatusItem(currentStatus.Status, newStatus.Value, status.Code, DateTime.Today));
                    statuses.AddRange(currentStatuses);
                }
                else
                {
                    // No new value - the current statuses haven't changed
                    statuses = currentStatuses;
                }
            }

            return statuses;
        }

        private Dtos.SectionStatus2 ConvertSectionStatusToDto2(Domain.Student.Entities.SectionStatusIntegration? status, out string statusCode)
        {
            statusCode = null;
            if (!status.HasValue || status == SectionStatusIntegration.NotSet)
            {
                return SectionStatus2.NotSet;
            }
            switch (status)
            {
                case Domain.Student.Entities.SectionStatusIntegration.Open:
                    return Dtos.SectionStatus2.Open;
                case Domain.Student.Entities.SectionStatusIntegration.Cancelled:
                    return Dtos.SectionStatus2.Cancelled;
                case Domain.Student.Entities.SectionStatusIntegration.Pending:
                    return Dtos.SectionStatus2.Pending;
                case Domain.Student.Entities.SectionStatusIntegration.NotSet:
                    return Dtos.SectionStatus2.NotSet;
                default:
                    return Dtos.SectionStatus2.Closed;
            }
        }

        private Domain.Student.Entities.SectionStatusIntegration? ConvertSectionStatusFromDto2(Dtos.SectionStatus2? status, out string statusCode)
        {
            statusCode = null;
            if (!status.HasValue)
            {
                return null;
            }
            switch (status)
            {
                case Dtos.SectionStatus2.Open:
                    return Domain.Student.Entities.SectionStatusIntegration.Open;
                case Dtos.SectionStatus2.Cancelled:
                    return Domain.Student.Entities.SectionStatusIntegration.Cancelled;
                case Dtos.SectionStatus2.Pending:
                    return Domain.Student.Entities.SectionStatusIntegration.Pending;
                case Dtos.SectionStatus2.NotSet:
                    return Domain.Student.Entities.SectionStatusIntegration.NotSet;
                default:
                    return Domain.Student.Entities.SectionStatusIntegration.Closed;
            }
        }

        private async Task<string> ConvertDateArgument(string date)
        {
            try
            {
                return await _sectionRepository.GetUnidataFormattedDate(date);
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid Date format in arguments");
            }
        }

        private async Task<string> ConvertCourseArgument(string course)
        {
            return await _sectionRepository.GetCourseIdFromGuidAsync(course);
        }

        private async Task<string> ConvertStatusArgument(string status)
        {
            return await _sectionRepository.ConvertStatusToStatusCodeAsync(status);
        }
        private async Task<string> ConvertStatusArgumentNoDefault(string status)
        {
            return await _sectionRepository.ConvertStatusToStatusCodeNoDefaultAsync(status);
        }

        #endregion

        #region HeDM Version 6

        /// <summary>
        /// Get selection criteria, convert it to codes as required and select 
        /// multiple sections from Colleague.
        /// </summary>
        /// <param name="title">Section Title Contains ...title...</param>
        /// <param name="startOn">Section starts on or after this date</param>
        /// <param name="endOn">Section ends on or before this date</param>
        /// <param name="code">Section Name Contains ...code...</param>
        /// <param name="number">Section Number equal to</param>
        /// <param name="instructionalPlatform">Learning Platform equal to (guid)</param>
        /// <param name="academicPeriod">Section Term equal to (guid)</param>
        /// <param name="academicLevel">Section Academic Level equal to (guid)</param>
        /// <param name="course">Section Course equal to (guid)</param>
        /// <param name="site">Section Location equal to (guid)</param>
        /// <param name="status">Section Status matches closed, open, pending, or cancelled</param>
        /// <param name="owningOrganization">Section Department equal to (guid)</param>
        /// <returns>List of Section3 <see cref="Dtos.Section3"/> objects representing matching sections</returns>
        public async Task<Tuple<IEnumerable<Dtos.Section3>, int>> GetSections3Async(int offset, int limit, string title = "", string startOn = "", string endOn = "",
            string code = "", string number = "", string instructionalPlatform = "", string academicPeriod = "",
            string academicLevel = "", string course = "", string site = "", string status = "", string owningOrganization = "")
        {
            string newStartOn = "", newEndOn = "", newInstructionalPlatform = "", newAcademicPeriod = "",
                newAcademicLevel = "", newCourse = "", newSite = "", newStatus = "", newOwningOrganization = "";
            try
            {
                // Convert and validate all input parameters
                newStartOn = (startOn == string.Empty ? string.Empty : await ConvertDateArgument(startOn));
                if (!string.IsNullOrEmpty(startOn) && string.IsNullOrEmpty(newStartOn))
                    return new Tuple<IEnumerable<Dtos.Section3>, int>(new List<Dtos.Section3>(), 0);
                newEndOn = (endOn == string.Empty ? string.Empty : await ConvertDateArgument(endOn));
                if (!string.IsNullOrEmpty(endOn) && string.IsNullOrEmpty(newEndOn))
                    return new Tuple<IEnumerable<Dtos.Section3>, int>(new List<Dtos.Section3>(), 0);
                newInstructionalPlatform = (instructionalPlatform == string.Empty ? string.Empty : ConvertGuidToCode((await InstructionalPlatformsAsync()), instructionalPlatform));
                if (!string.IsNullOrEmpty(instructionalPlatform) && string.IsNullOrEmpty(newInstructionalPlatform))
                    // throw new ArgumentException(string.Format("instructional platform guid {0} is invalid.", instructionalPlatform));
                    return new Tuple<IEnumerable<Dtos.Section3>, int>(new List<Dtos.Section3>(), 0);
                newAcademicPeriod = (academicPeriod == string.Empty ? string.Empty : ConvertGuidToCode(academicPeriods, academicPeriod));
                if (!string.IsNullOrEmpty(academicPeriod) && string.IsNullOrEmpty(newAcademicPeriod))
                    // throw new ArgumentException(string.Format("academic period guid {0} is invalid.", academicPeriod));
                    return new Tuple<IEnumerable<Dtos.Section3>, int>(new List<Dtos.Section3>(), 0);
                newAcademicLevel = (academicLevel == string.Empty ? string.Empty : ConvertGuidToCode((await AcademicLevelsAsync()), academicLevel));
                if (!string.IsNullOrEmpty(academicLevel) && string.IsNullOrEmpty(newAcademicLevel))
                    // throw new ArgumentException(string.Format("academic level guid {0} is invalid.", academicLevel));
                    return new Tuple<IEnumerable<Dtos.Section3>, int>(new List<Dtos.Section3>(), 0);
                newCourse = (course == string.Empty ? string.Empty : await ConvertCourseArgument(course));
                if (!string.IsNullOrEmpty(course) && string.IsNullOrEmpty(newCourse))
                    return new Tuple<IEnumerable<Dtos.Section3>, int>(new List<Dtos.Section3>(), 0);
                newSite = (site == string.Empty ? string.Empty : ConvertGuidToCode(locations, site));
                if (!string.IsNullOrEmpty(site) && string.IsNullOrEmpty(newSite))
                    throw new ArgumentException(string.Format("site guid {0} is invalid.", site));
                newStatus = (status == string.Empty ? string.Empty : await ConvertStatusArgument(status));
                if (!string.IsNullOrEmpty(status) && string.IsNullOrEmpty(newStatus))
                    return new Tuple<IEnumerable<Dtos.Section3>, int>(new List<Dtos.Section3>(), 0);
                newOwningOrganization = (owningOrganization == string.Empty ? string.Empty : ConvertGuidToCode((await DepartmentsAsync()), owningOrganization));
                if (!string.IsNullOrEmpty(owningOrganization) && string.IsNullOrEmpty(newOwningOrganization))
                    return new Tuple<IEnumerable<Dtos.Section3>, int>(new List<Dtos.Section3>(), 0);
            }
            catch
            {
                return new Tuple<IEnumerable<Dtos.Section3>, int>(new List<Dtos.Section3>(), 0);
            }

            var sectionDtos = new List<Dtos.Section3>();
            var sectionEntities = await _sectionRepository.GetSectionsAsync(offset, limit, title, newStartOn, newEndOn,
                code, number, newInstructionalPlatform, newAcademicPeriod,
                newAcademicLevel, newCourse, newSite, newStatus, newOwningOrganization);

            foreach (var sectionEntity in sectionEntities.Item1)
            {
                if (sectionEntity.Guid != null)
                {
                    var sectionDto = await ConvertSectionEntityToDto3Async(sectionEntity);
                    sectionDtos.Add(sectionDto);
                }
            }
            return new Tuple<IEnumerable<Dtos.Section3>, int>(sectionDtos, sectionEntities.Item2);
        }

        /// <summary>
        /// Get a Data Model section version 6 by its GUID
        /// </summary>
        /// <param name="guid">GUID of section</param>
        /// <returns>The Data Model Section version 6 DTO</returns>
        public async Task<Dtos.Section3> GetSection3ByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get a section.");
            }
            var sectionEntity = await _sectionRepository.GetSectionByGuidAsync(guid);
            if (sectionEntity == null)
            {
                throw new KeyNotFoundException(string.Concat("Section not found or invalid for id '", guid, "'.  See Log for more information."));
            }
            var sectionDto = await ConvertSectionEntityToDto3Async(sectionEntity);
            return sectionDto;
        }

        /// <summary>
        /// Create a new section
        /// </summary>
        /// <param name="section">DTO containing the Data Model version 6 section to create</param>
        /// <returns>DTO containing the created Data Model version 6 section</returns>
        public async Task<Dtos.Section3> PostSection3Async(Dtos.Section3 section)
        {
         
            if ((section != null) && (section.Status != null))
                await ValidateSectionStatusConfigurationAsync(section.Status);

            _sectionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            // Make sure dept percentage is from 0-100%.  This is also checked in the contructor for the domain entity OfferingDepartment,
            // but we want to customize the error message for Ethos APIs to refernece "Ownership percentage".
            if (section != null)
            {
                if (section.OwningInstitutionUnits != null && section.OwningInstitutionUnits.Any())
                {
                    foreach (var owningInstitutionUnit in section.OwningInstitutionUnits)
                    {
                        if (owningInstitutionUnit != null)
                        {
                            if (owningInstitutionUnit.OwnershipPercentage < 0 || owningInstitutionUnit.OwnershipPercentage > 100)
                            {
                                throw new ArgumentOutOfRangeException("ownershipPercentage", "Ownership percentage must be between 0 and 100.");
                            }
                        }
                    }
                }
            }
            // Convert the CDM section into a domain entity and create it
            var entity = await ConvertSectionDto3ToEntityAsync(section);
            if (entity == null)
            {
                throw new KeyNotFoundException("Failed to create a new section. ");
            }
            var newEntity = await _sectionRepository.PostSectionAsync(entity);
            if (newEntity == null)
            {
                throw new KeyNotFoundException("Failed to return a valid section. ");
            }
            var newDto = await ConvertSectionEntityToDto3Async(newEntity);

            return newDto;
        }

        /// <summary>
        /// Update a section
        /// </summary>
        /// <param name="section">DTO containing the Data Model version 6 section to update</param>
        /// <returns>DTO containing the updated Data Model version 6 section</returns>
        public async Task<Dtos.Section3> PutSection3Async(Dtos.Section3 section)
        {
          
            if ((section != null) && (section.Status != null))
                await ValidateSectionStatusConfigurationAsync(section.Status);

            _sectionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            // Make sure dept percentage is from 0-100%.  This is also checked in the contructor for the domain entity OfferingDepartment,
            // but we want to customize the error message for Ethos APIs to refernece "Ownership percentage".
            if (section != null)
            {
                if (section.OwningInstitutionUnits != null && section.OwningInstitutionUnits.Any())
                {
                    foreach (var owningInstitutionUnit in section.OwningInstitutionUnits)
                    {
                        if (owningInstitutionUnit != null)
                        {
                            if (owningInstitutionUnit.OwnershipPercentage < 0 || owningInstitutionUnit.OwnershipPercentage > 100)
                            {
                                throw new ArgumentOutOfRangeException("ownershipPercentage", "Ownership percentage must be between 0 and 100.");
                            }
                        }
                    }
                }
            }
            // Convert the CDM section into a domain entity and update it
            var entity = await ConvertSectionDto3ToEntityAsync(section);
            if (entity == null)
            {
                throw new KeyNotFoundException(string.Format("Could not update section '{0}'. ", section.Id));
            }
            var updatedEntity = await _sectionRepository.PutSectionAsync(entity);
            if (updatedEntity == null)
            {
                throw new KeyNotFoundException(string.Format("GUID '{0}' failed to return a valid section. ", section.Id));
            }
            var updatedDto = await ConvertSectionEntityToDto3Async(updatedEntity);

            return updatedDto;
        }


        /// <summary>
        /// Get selection criteria, convert it to codes as required and select 
        /// multiple sections from Colleague.
        /// </summary>
        /// <param name="title">Section Title Contains ...title...</param>
        /// <param name="startOn">Section starts on or after this date</param>
        /// <param name="endOn">Section ends on or before this date</param>
        /// <param name="code">Section Name Contains ...code...</param>
        /// <param name="number">Section Number equal to</param>
        /// <param name="instructionalPlatform">Learning Platform equal to (guid)</param>
        /// <param name="academicPeriod">Section Term equal to (guid)</param>
        /// <param name="academicLevel">Section Academic Level equal to (guid)</param>
        /// <param name="course">Section Course equal to (guid)</param>
        /// <param name="site">Section Location equal to (guid)</param>
        /// <param name="status">Section Status matches closed, open, pending, or cancelled</param>
        /// <param name="owningOrganization">Section Department equal to (guid)</param>
        /// <returns>List of Section2 <see cref="Dtos.SectionMaximum"/> objects representing matching sections</returns>
        public async Task<Tuple<IEnumerable<Dtos.SectionMaximum2>, int>> GetSectionsMaximum2Async(int offset, int limit, string title = "", string startOn = "", string endOn = "",
            string code = "", string number = "", string instructionalPlatform = "", string academicPeriod = "",
            string academicLevel = "", string course = "", string site = "", string status = "", string owningOrganization = "")
        {
            //Convert and validate all input parameters
            IDictionary<string, string> args = new Dictionary<string, string>();
            try
            {
                args = await ValidateAndConvertFilterArguments(startOn, endOn, instructionalPlatform, academicPeriod, course, site, status, "", academicLevel);
            }
            catch (ArgumentException)
            {
                // One or more of the arguments failed to match up to a guid.  Return empty set.
                return new Tuple<IEnumerable<Dtos.SectionMaximum2>, int>(new List<Dtos.SectionMaximum2>(), 0);
            }

            var newStartOn = args["newStartOn"];
            var newEndOn = args["newEndOn"];
            var newInstructionalPlatform = args["newInstructionalPlatform"];
            var newAcademicPeriod = args["newAcademicPeriod"];
            var newCourse = args["newCourse"];
            var newSite = args["newSite"];
            var newStatus = args["newStatus"];
            var newOwningOrganization = args["newOwningOrganization"];
            var newAcademicLevel = args["newAcademicLevel"];

            var sectionDtos = new List<Dtos.SectionMaximum2>();
            var sectionEntities = await _sectionRepository.GetSectionsAsync(offset, limit, title, newStartOn, newEndOn,
                code, number, newInstructionalPlatform, newAcademicPeriod,
                newAcademicLevel, newCourse, newSite, newStatus, newOwningOrganization);

            foreach (var sectionEntity in sectionEntities.Item1)
            {
                if (sectionEntity.Guid != null)
                {
                    var sectionDto = await ConvertSectionEntityToSectionMaximum2Async(sectionEntity);
                    sectionDtos.Add(sectionDto);
                }
            }
            return new Tuple<IEnumerable<Dtos.SectionMaximum2>, int>(sectionDtos, sectionEntities.Item2);
        }

        /// <summary>
        /// Get selection criteria, convert it to codes as required and select 
        /// multiple sections from Colleague.
        /// </summary>
        /// <param name="title">Section Title Contains ...title...</param>
        /// <param name="startOn">Section starts on or after this date</param>
        /// <param name="endOn">Section ends on or before this date</param>
        /// <param name="code">Section Name Contains ...code...</param>
        /// <param name="number">Section Number equal to</param>
        /// <param name="instructionalPlatform">Learning Platform equal to (guid)</param>
        /// <param name="academicPeriod">Section Term equal to (guid)</param>
        /// <param name="academicLevel">Section Academic Level equal to (guid)</param>
        /// <param name="course">Section Course equal to (guid)</param>
        /// <param name="site">Section Location equal to (guid)</param>
        /// <param name="status">Section Status matches closed, open, pending, or cancelled</param>
        /// <param name="owningOrganization">Section Department equal to (guid)</param>
        /// <returns>List of SectionMaximum3 <see cref="Dtos.SectionMaximum3"/> objects representing matching sections</returns>
        public async Task<Tuple<IEnumerable<Dtos.SectionMaximum3>, int>> GetSectionsMaximum3Async(int offset, int limit, string title = "", string startOn = "", string endOn = "",
            string code = "", string number = "", string instructionalPlatform = "", string academicPeriod = "",
            List<string> academicLevel = null, string course = "", string site = "", string status = "", List<string> owningOrganization = null)
        {
            //Convert and validate all input parameters
            IDictionary<string, string> args = new Dictionary<string, string>();
            try
            {
                args = await ValidateAndConvertFilterArguments(startOn, endOn, instructionalPlatform, academicPeriod, course, site, status, "");
            }
            catch (ArgumentException)
            {
                // One or more of the arguments failed to match up to a guid.  Return empty set.
                return new Tuple<IEnumerable<Dtos.SectionMaximum3>, int>(new List<Dtos.SectionMaximum3>(), 0);

            }

            var arrayArgs = new Dictionary<string, List<string>>();
            try
            {
                arrayArgs = await ValidateAndConvertFilterArrayArguments(academicLevel, owningOrganization);
            }
            catch (ArgumentException)
            {
                // One or more of the arguments failed to match up to a guid.  Return empty set.
                return new Tuple<IEnumerable<Dtos.SectionMaximum3>, int>(new List<Dtos.SectionMaximum3>(), 0);

            }
            var newStartOn = args["newStartOn"];
            var newEndOn = args["newEndOn"];
            var newInstructionalPlatform = args["newInstructionalPlatform"];
            var newAcademicPeriod = args["newAcademicPeriod"];
            var instructorId = args["instructorId"];
            var newCourse = args["newCourse"];
            var newSite = args["newSite"];
            var newStatus = args["newStatus"];
            var newOwningOrganization = arrayArgs["newOwningOrganizations"];
            var newAcademicLevel = arrayArgs["newAcademicLevels"];
            var newSubject = args["newSubject"];

            var sectionDtos = new List<Dtos.SectionMaximum3>();
            var sectionEntities = await _sectionRepository.GetSectionsAsync(offset, limit, title, newStartOn, newEndOn,
                code, number, newInstructionalPlatform, newAcademicPeriod,
                newAcademicLevel, newCourse, newSite, newStatus, newOwningOrganization);

            foreach (var sectionEntity in sectionEntities.Item1)
            {
                if (sectionEntity.Guid != null)
                {
                    var sectionDto = await ConvertSectionEntityToSectionMaximum3Async(sectionEntity);
                    sectionDtos.Add(sectionDto);
                }
            }
            return new Tuple<IEnumerable<Dtos.SectionMaximum3>, int>(sectionDtos, sectionEntities.Item2);
        }

        /// <summary>
        /// Get a HeDM SectionMaximum version 6 by its GUID
        /// </summary>
        /// <param name="guid">GUID of section</param>
        /// <returns>The HeDM SectionMaximum version 4 DTO</returns>
        public async Task<Dtos.SectionMaximum2> GetSectionMaximumByGuid2Async(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get a section.");
            }
            var sectionEntity = await _sectionRepository.GetSectionByGuidAsync(guid);
            if (sectionEntity == null)
            {
                throw new KeyNotFoundException(string.Concat("Section not found or invalid for id '", guid, "'.  See Log for more information."));
            }
            var sectionDto = await ConvertSectionEntityToSectionMaximum2Async(sectionEntity);
            return sectionDto;
        }

        /// <summary>
        /// Get a HeDM SectionMaximum version 8 by its GUID
        /// </summary>
        /// <param name="guid">GUID of section</param>
        /// <returns>The EEDM SectionMaximum version 8 DTO</returns>
        public async Task<Dtos.SectionMaximum3> GetSectionMaximumByGuid3Async(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get a section.");
            }
            var sectionEntity = await _sectionRepository.GetSectionByGuidAsync(guid);
            if (sectionEntity == null)
            {
                throw new KeyNotFoundException(string.Concat("Section not found or invalid for id '", guid, "'.  See Log for more information."));
            }
            var sectionDto = await ConvertSectionEntityToSectionMaximum3Async(sectionEntity);
            return sectionDto;
        }

        #endregion

        #region Convert HeDM Version 6

        /// <summary>
        /// Convert a Section entity into the HeDM SectionMaximum format DTO
        /// </summary>
        /// <param name="entity">A Section entity</param>
        /// <returns>A HeDM-version 4 SectionMaximum</returns>
        private async Task<Dtos.SectionMaximum2> ConvertSectionEntityToSectionMaximum2Async(Domain.Student.Entities.Section entity)
        {
            if (entity == null)
            {
                return null;
            }

            var sectionDto = new Dtos.SectionMaximum2();

            sectionDto.Id = entity.Guid.ToLowerInvariant();
            sectionDto.Code = entity.Name;
            sectionDto.Number = entity.Number;
            sectionDto.Title = entity.Title;
            sectionDto.StartOn = entity.StartDate;
            sectionDto.EndOn = entity.EndDate;

            if (!string.IsNullOrEmpty(entity.LearningProvider))
            {
                var instructionalPlatform = (await InstructionalPlatformsAsync()).FirstOrDefault(i => i.Code == entity.LearningProvider);

                sectionDto.InstructionalPlatform = (instructionalPlatform != null)
                    ? new InstructionalPlatformDtoProperty()
                    {
                        Code = instructionalPlatform.Code,
                        Title = instructionalPlatform.Description,
                        Detail = new GuidObject2(instructionalPlatform.Guid)
                    }
                    : null;
            }

            if (!string.IsNullOrEmpty(entity.TermId))
            {
                var academicPeriod = academicPeriods.FirstOrDefault(a => a.Code == entity.TermId);
                sectionDto.AcademicPeriod = (academicPeriod != null)
                    ? new AcademicPeriodDtoProperty()
                    {
                        Detail = new GuidObject2(academicPeriod.Guid),
                        Code = academicPeriod.Code,
                        Start = academicPeriod.StartDate,
                        End = academicPeriod.EndDate,
                        Title = academicPeriod.Description
                    }
                    : null;
            }


            var courseGuid = await _courseRepository.GetCourseGuidFromIdAsync(entity.CourseId);
            if (string.IsNullOrEmpty(courseGuid))
            {
                throw new RepositoryException(string.Concat("Course guid not found for Colleague Course Id : ", entity.CourseId));
            }

            var course = await _courseRepository.GetCourseByGuidAsync(courseGuid);
            if (course != null)
            {
                var courseReturn = new CourseDtoProperty()
                {
                    Detail = new GuidObject2(course.Guid),
                    Number = course.Number,
                    Title = course.Title
                };
                var subject = (await _studentReferenceDataRepository.GetSubjectsAsync()).FirstOrDefault(s => s.Code == course.SubjectCode);
                if (subject != null)
                {
                    courseReturn.Subject = new SubjectDtoProperty()
                    {
                        Abbreviation = subject.Code,
                        Title = subject.Description,
                        Detail = new GuidObject2(subject.Guid)
                    };
                }
                sectionDto.Course = courseReturn;
            }
            else
            {
                throw new RepositoryException(string.Concat("Course not found for Id : ", courseGuid));
            }

            var credit = new Dtos.DtoProperties.Credit2DtoProperty();
            var creditTypeItems = await CreditTypesAsync();
            if (creditTypeItems.Any(ct => ct.Code == entity.CreditTypeCode))
            {
                var creditTypeItem = creditTypeItems.First(ct => ct.Code == entity.CreditTypeCode);
                credit.CreditCategory.Detail.Id = creditTypeItem.Guid;
                credit.CreditCategory.Code = creditTypeItem.Code;
                credit.CreditCategory.Title = creditTypeItem.Description;
                switch (creditTypeItem.CreditType)
                {

                    case CreditType.ContinuingEducation:
                        credit.CreditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
                        break;
                    case CreditType.Institutional:
                        credit.CreditCategory.CreditType = CreditCategoryType3.Institutional;
                        break;
                    case CreditType.Transfer:
                        credit.CreditCategory.CreditType = CreditCategoryType3.Transfer;
                        break;
                    case CreditType.Exchange:
                        credit.CreditCategory.CreditType = CreditCategoryType3.Exchange;
                        break;
                    case CreditType.Other:
                        credit.CreditCategory.CreditType = CreditCategoryType3.Other;
                        break;
                    case CreditType.None:
                        credit.CreditCategory.CreditType = CreditCategoryType3.NoCredit;
                        break;
                    default:
                        credit.CreditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
                        break;

                }
            }
            else
            {
                //default to CE
                credit.CreditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
            }

            if (entity.Ceus.HasValue)
            {
                credit.Measure = Dtos.CreditMeasure2.CEU;
                credit.Minimum = entity.Ceus.Value;
            }
            else
            {
                credit.Measure = Dtos.CreditMeasure2.Credit;
                credit.Minimum = entity.MinimumCredits.GetValueOrDefault();
                credit.Maximum = entity.MaximumCredits;
                credit.Increment = entity.VariableCreditIncrement;
            }
            var creditList = new List<Dtos.DtoProperties.Credit2DtoProperty>();
            creditList.Add(credit);
            sectionDto.Credits = creditList;

            var site = new SiteDtoProperty();

            var location = locations.FirstOrDefault(l => l.Code == entity.Location);
            if (location != null)
            {
                site.Code = location.Code;
                site.Title = location.Description;
                site.Detail = new GuidObject2(location.Guid);

                sectionDto.Site = site;
            }

            var acadLevel = new AcademicLevelDtoProperty();

            var level = (await AcademicLevelsAsync()).FirstOrDefault(l => l.Code == entity.AcademicLevelCode);
            if (level != null)
            {
                acadLevel.Code = level.Code;
                acadLevel.Title = level.Description;
                acadLevel.Detail = new GuidObject2(level.Guid);

                sectionDto.AcademicLevels = new List<AcademicLevelDtoProperty>()
                {
                   acadLevel
                };
            }

            var awardScheme = new GradeSchemeDtoProperty();

            var gradeScheme = (await GradeSchemesAsync()).FirstOrDefault(g => g.Code == entity.GradeSchemeCode);
            if (gradeScheme != null)
            {
                awardScheme.Code = gradeScheme.Code;
                awardScheme.Title = gradeScheme.Description;
                awardScheme.Start = gradeScheme.EffectiveStartDate;
                awardScheme.End = gradeScheme.EffectiveEndDate;
                awardScheme.Detail = new GuidObject2(gradeScheme.Guid);
                awardScheme.AcademicLevel = acadLevel;

                sectionDto.AwardGradeSchemes = new List<GradeSchemeDtoProperty>()
                {
                   awardScheme
                };

                sectionDto.TranscriptGradeSchemes = new List<GradeSchemeDtoProperty>()
                {
                   awardScheme
                };
            }

            var courseLevels = await CourseLevelsAsync();
            if (courseLevels.Any())
            {
                var courseLevelList = new List<CourseLevelDtoProperty>();
                foreach (var code in entity.CourseLevelCodes)
                {
                    var cLevel = courseLevels.FirstOrDefault(l => l.Code == code);
                    if (cLevel != null)
                    {
                        var levelReturn = new CourseLevelDtoProperty
                        {
                            Code = cLevel.Code,
                            Title = cLevel.Description,
                            Detail = new GuidObject2(cLevel.Guid)
                        };
                        courseLevelList.Add(levelReturn);
                    }
                }
                sectionDto.CourseLevels = courseLevelList;
            }

            string status;
            sectionDto.Status = ConvertSectionStatusToDto2(entity.Statuses.ElementAt(0).IntegrationStatus, out status);
            sectionDto.MaximumEnrollment = entity.GlobalCapacity ?? entity.SectionCapacity;

            if (entity.NumberOfWeeks.HasValue && entity.NumberOfWeeks > 0)
            {
                sectionDto.Duration = new SectionDuration2()
                {
                    Length = entity.NumberOfWeeks.Value,
                    Unit = DurationUnit2.Weeks
                };
            }

            var orgs = await DepartmentsAsync();
            if (orgs.Any())
            {
                var orgsList = new List<OwningOrganizationDtoProperty>();
                foreach (var code in entity.Departments)
                {
                    var owningOrg = orgs.FirstOrDefault(o => o.Code == code.AcademicDepartmentCode);
                    if (owningOrg != null)
                    {
                        var orgReturn = new OwningOrganizationDtoProperty();
                        orgReturn.Code = owningOrg.Code;
                        orgReturn.Title = owningOrg.Description;
                        orgReturn.OwnershipPercentage = code.ResponsibilityPercentage;
                        orgReturn.Detail = new GuidObject2(owningOrg.Guid);
                        orgsList.Add(orgReturn);
                    }
                }
                sectionDto.OwningOrganizations = orgsList;
            }

            var pageOfItems = await _sectionRepository.GetSectionMeetingAsync(0, 0, entity.Id, string.Empty, string.Empty, string.Empty, string.Empty, new List<string>(), new List<string>(), new List<string>(), string.Empty);
            var eventEntities = pageOfItems.Item1;

            if (eventEntities.Any())
            {
                var instructionalEventsList = new List<InstructionalEventDtoProperty>();

                foreach (var eventInstructional in eventEntities)
                {
                    if (eventInstructional.Guid != null)
                    {
                        var returnInstructionalEventDto = new InstructionalEventDtoProperty();
                        var instEventDto = await ConvertSectionMeetingToInstructionalEvent2Async(eventInstructional);

                        returnInstructionalEventDto.Detail = new GuidObject2(instEventDto.Id);
                        returnInstructionalEventDto.Title = instEventDto.Title;
                        var instMethod = (await _studentReferenceDataRepository.GetInstructionalMethodsAsync()).FirstOrDefault(im => im.Guid == instEventDto.InstructionalMethod.Id);

                        if (instMethod != null)
                        {
                            returnInstructionalEventDto.InstructionalMethod = new InstructionalMethodDtoProperty()
                            {
                                Abbreviation = instMethod.Code,
                                Title = instMethod.Description,
                                Detail = new GuidObject2(instMethod.Guid)
                            };
                        }

                        if (instEventDto.Recurrence != null)
                        {
                            returnInstructionalEventDto.Recurrence = instEventDto.Recurrence;
                        }

                        if ((instEventDto.Locations != null) && (instEventDto.Locations.Any()))
                        {
                            var locationsList = new List<LocationDtoProperty>();
                            var locationDtoProperty = new LocationDtoProperty();
                            var locationRoom = new LocationRoomDtoProperty();
                            var room = (await RoomsAsync()).FirstOrDefault(r => r.Id == eventInstructional.Room);

                            if (room != null)
                            {
                                locationRoom.LocationType = InstructionalLocationType.InstructionalRoom;
                                locationRoom.Title = !string.IsNullOrEmpty(room.Name) ? room.Name : null;
                                locationRoom.Number = !string.IsNullOrEmpty(room.Number) ? room.Number : null;
                                locationRoom.Floor = !string.IsNullOrEmpty(room.Floor) ? room.Floor : null;
                                locationRoom.Detail = new GuidObject2(room.Guid);

                                var building = (await _referenceDataRepository.BuildingsAsync()).FirstOrDefault(b => b.Code == room.BuildingCode);
                                if (building != null)
                                {
                                    locationRoom.Building = new BuildingDtoProperty()
                                    {
                                        Code = building.Code,
                                        Title = building.Description,
                                        Detail = new GuidObject2(building.Guid)
                                    };
                                }

                                locationDtoProperty.Location = locationRoom;
                                locationsList.Add(locationDtoProperty);
                            }
                            returnInstructionalEventDto.Locations = locationsList;

                        }

                        if ((instEventDto.Instructors != null) && (instEventDto.Instructors.Any()))
                        {
                            var instructorList = new List<InstructorRosterDtoProperty>();

                            foreach (var eventInstructor in instEventDto.Instructors)
                            {
                                var returnInstructorRoster = new InstructorRosterDtoProperty
                                {
                                    WorkLoadPercentage = eventInstructor.WorkLoadPercentage,
                                    ResponsibilityPercentage = eventInstructor.ResponsibilityPercentage,
                                    WorkStartDate = eventInstructor.WorkStartDate,
                                    WorkEndDate = eventInstructor.WorkEndDate
                                };

                                var returnInstructor = new InstructorDtoProperty { Detail = new GuidObject2(eventInstructor.Instructor.Id) };
                                var namesList = new List<InstructorNameDtoProperty>();
                                var instructorName = new InstructorNameDtoProperty();

                                var person = await _personRepository.GetPersonByGuidNonCachedAsync(eventInstructor.Instructor.Id);
                                instructorName.NameType = InstructorNameType.Primary;

                                instructorName.Title = string.IsNullOrEmpty(person.Prefix) ? "" : person.Prefix;
                                instructorName.Pedigree = string.IsNullOrEmpty(person.Suffix) ? "" : person.Suffix;
                                instructorName.FirstName = string.IsNullOrEmpty(person.FirstName) ? "" : person.FirstName;
                                instructorName.MiddleName = string.IsNullOrEmpty(person.MiddleName) ? null : person.MiddleName;
                                instructorName.LastNamePrefix = null;
                                instructorName.LastName = person.LastName;
                                instructorName.PreferredName = string.IsNullOrEmpty(person.Nickname) ? null : person.Nickname;

                                namesList.Add(instructorName);
                                returnInstructor.Names = namesList;

                                var credentialList = new List<CredentialDtoProperty>();

                                credentialList.Add(new CredentialDtoProperty()
                                {
                                    Type = Dtos.EnumProperties.CredentialType.ColleaguePersonId,
                                    Value = person.Id
                                });

                                if (person.PersonAltIds != null && person.PersonAltIds.Any())
                                {
                                    var elevPersonAltId = person.PersonAltIds.FirstOrDefault(a => a.Type == Domain.Base.Entities.PersonAlt.ElevatePersonAltType);
                                    if (elevPersonAltId != null && !string.IsNullOrEmpty(elevPersonAltId.Id))
                                    {
                                        credentialList.Add(new CredentialDtoProperty()
                                        {
                                            Type = Dtos.EnumProperties.CredentialType.ElevateID,
                                            Value = elevPersonAltId.Id
                                        });
                                    }
                                }

                                returnInstructor.Credentials = credentialList;
                                returnInstructorRoster.Instructor = returnInstructor;
                                instructorList.Add(returnInstructorRoster);
                            }
                            returnInstructionalEventDto.InstructorRoster = instructorList;

                        }


                        instructionalEventsList.Add(returnInstructionalEventDto);
                    }
                }

                if (instructionalEventsList.Any())
                {
                    sectionDto.InstructionalEvents = instructionalEventsList;
                }
            }

            return sectionDto;
        }

        #endregion

        #region Convert EEDM Version 8
        /// <summary>
        /// Convert a Section entity into the HeDM SectionMaximum format DTO
        /// </summary>
        /// <param name="entity">A Section entity</param>
        /// <returns>A HeDM-version 4 SectionMaximum</returns>
        private async Task<Dtos.SectionMaximum3> ConvertSectionEntityToSectionMaximum3Async(Domain.Student.Entities.Section entity)
        {
            if (entity == null)
            {
                return null;
            }

            var sectionDto = new Dtos.SectionMaximum3();

            sectionDto.Id = entity.Guid.ToLowerInvariant();
            sectionDto.Code = entity.Name;
            sectionDto.Number = entity.Number;
            sectionDto.Title = entity.Title;
            sectionDto.StartOn = entity.StartDate;
            sectionDto.EndOn = entity.EndDate;

            if (!string.IsNullOrEmpty(entity.LearningProvider))
            {
                var instructionalPlatform = (await InstructionalPlatformsAsync()).FirstOrDefault(i => i.Code == entity.LearningProvider);

                sectionDto.InstructionalPlatform = (instructionalPlatform != null)
                    ? new InstructionalPlatformDtoProperty()
                    {
                        Code = instructionalPlatform.Code,
                        Title = instructionalPlatform.Description,
                        Detail = new GuidObject2(instructionalPlatform.Guid)
                    }
                    : null;
            }

            if (!string.IsNullOrEmpty(entity.TermId))
            {
                var academicPeriod = academicPeriods.FirstOrDefault(a => a.Code == entity.TermId);
                if (academicPeriod != null)
                {
                    var returnAcadPeriod = new AcademicPeriodDtoProperty2()
                    {
                        Detail = new GuidObject2(academicPeriod.Guid),
                        Code = academicPeriod.Code,
                        Start = academicPeriod.StartDate,
                        End = academicPeriod.EndDate,
                        Title = academicPeriod.Description
                    };

                    if (academicPeriod.RegistrationDates != null)
                    {
                        if (academicPeriod.RegistrationDates.FirstOrDefault().CensusDates.Any())
                        {
                            returnAcadPeriod.CensusDates = academicPeriod.RegistrationDates.FirstOrDefault().CensusDates;
                        }
                    }

                    var category = new Dtos.AcademicPeriodCategory2();
                    if (IsReportingTermEqualCode(academicPeriod))
                    {
                        category.Type = Dtos.AcademicTimePeriod2.Term;
                        category.ParentGuid = (academicPeriod.ParentId != null) ? new GuidObject2(academicPeriod.ParentId) : null;
                        category.PrecedingGuid = (academicPeriod.PrecedingId != null) ? new GuidObject2(academicPeriod.PrecedingId) : null;
                    }
                    else
                    {
                        category.Type = Dtos.AcademicTimePeriod2.Subterm;
                        category.ParentGuid = (academicPeriod.ParentId != null) ? new GuidObject2(academicPeriod.ParentId) : null;
                    }

                    returnAcadPeriod.Category = category;

                    sectionDto.AcademicPeriod = returnAcadPeriod;
                }

            }


            var courseGuid = await _courseRepository.GetCourseGuidFromIdAsync(entity.CourseId);
            if (string.IsNullOrEmpty(courseGuid))
            {
                throw new RepositoryException(string.Concat("Course guid not found for Colleague Course Id : ", entity.CourseId));
            }

            var course = await _courseRepository.GetCourseByGuidAsync(courseGuid);
            if (course != null)
            {
                var courseReturn = new CourseDtoProperty()
                {
                    Detail = new GuidObject2(course.Guid),
                    Number = course.Number,
                    Title = string.IsNullOrEmpty(course.LongTitle) ? course.Title : course.LongTitle
                };
                var subject = (await _studentReferenceDataRepository.GetSubjectsAsync()).FirstOrDefault(s => s.Code == course.SubjectCode);
                if (subject != null)
                {
                    courseReturn.Subject = new SubjectDtoProperty()
                    {
                        Abbreviation = subject.Code,
                        Title = subject.Description,
                        Detail = new GuidObject2(subject.Guid)
                    };
                }
                sectionDto.Course = courseReturn;
            }
            else
            {
                throw new RepositoryException(string.Concat("Course not found for Id : ", courseGuid));
            }

            var credit = new Dtos.DtoProperties.Credit2DtoProperty();
            var creditTypeItems = await CreditTypesAsync();
            if (creditTypeItems.Any(ct => ct.Code == entity.CreditTypeCode))
            {
                var creditTypeItem = creditTypeItems.First(ct => ct.Code == entity.CreditTypeCode);
                credit.CreditCategory.Detail.Id = creditTypeItem.Guid;
                credit.CreditCategory.Code = creditTypeItem.Code;
                credit.CreditCategory.Title = creditTypeItem.Description;
                switch (creditTypeItem.CreditType)
                {

                    case CreditType.ContinuingEducation:
                        credit.CreditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
                        break;
                    case CreditType.Institutional:
                        credit.CreditCategory.CreditType = CreditCategoryType3.Institutional;
                        break;
                    case CreditType.Transfer:
                        credit.CreditCategory.CreditType = CreditCategoryType3.Transfer;
                        break;
                    case CreditType.Exchange:
                        credit.CreditCategory.CreditType = CreditCategoryType3.Exchange;
                        break;
                    case CreditType.Other:
                        credit.CreditCategory.CreditType = CreditCategoryType3.Other;
                        break;
                    case CreditType.None:
                        credit.CreditCategory.CreditType = CreditCategoryType3.NoCredit;
                        break;
                    default:
                        credit.CreditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
                        break;

                }
            }
            else
            {
                //default to CE
                credit.CreditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
            }

            if (entity.Ceus.HasValue)
            {
                credit.Measure = Dtos.CreditMeasure2.CEU;
                credit.Minimum = entity.Ceus.Value;
            }
            else
            {
                credit.Measure = Dtos.CreditMeasure2.Credit;
                credit.Minimum = entity.MinimumCredits.GetValueOrDefault();
                credit.Maximum = entity.MaximumCredits;
                credit.Increment = entity.VariableCreditIncrement;
            }
            var creditList = new List<Dtos.DtoProperties.Credit2DtoProperty>();
            creditList.Add(credit);
            sectionDto.Credits = creditList;

            var site = new SiteDtoProperty();

            var location = locations.FirstOrDefault(l => l.Code == entity.Location);
            if (location != null)
            {
                site.Code = location.Code;
                site.Title = location.Description;
                site.Detail = new GuidObject2(location.Guid);

                sectionDto.Site = site;
            }

            var acadLevel = new AcademicLevelDtoProperty();
            var acadLevel2 = new AcademicLevel2DtoProperty();
            var level = (await AcademicLevelsAsync()).FirstOrDefault(l => l.Code == entity.AcademicLevelCode);
            if (level != null)
            {
                acadLevel.Code = level.Code;
                acadLevel.Title = level.Description;
                acadLevel.Detail = new GuidObject2(level.Guid);

                acadLevel2.Code = level.Code;
                acadLevel2.Title = level.Description;
                acadLevel2.Detail = new GuidObject2(level.Guid);

                sectionDto.AcademicLevels = new List<AcademicLevelDtoProperty>()
                {
                   acadLevel
                };
            }



            var gradeScheme = (await GradeSchemesAsync()).FirstOrDefault(g => g.Code == entity.GradeSchemeCode);
            if (gradeScheme != null)
            {
                var returnGradeScheme = new GradeSchemeDtoProperty2();

                returnGradeScheme.Code = gradeScheme.Code;
                returnGradeScheme.Title = gradeScheme.Description;
                returnGradeScheme.Start = gradeScheme.EffectiveStartDate;
                returnGradeScheme.End = gradeScheme.EffectiveEndDate;
                returnGradeScheme.Detail = new GuidObject2(gradeScheme.Guid);
                returnGradeScheme.AcademicLevel = acadLevel2;

                sectionDto.GradeSchemes = new List<GradeSchemeDtoProperty2>()
                {
                   returnGradeScheme
                };

            }

            var courseLevels = await CourseLevelsAsync();
            if (courseLevels.Any())
            {
                var courseLevelList = new List<CourseLevelDtoProperty>();
                foreach (var code in entity.CourseLevelCodes)
                {
                    var cLevel = courseLevels.FirstOrDefault(l => l.Code == code);
                    if (cLevel != null)
                    {
                        var levelReturn = new CourseLevelDtoProperty
                        {
                            Code = cLevel.Code,
                            Title = cLevel.Description,
                            Detail = new GuidObject2(cLevel.Guid)
                        };
                        courseLevelList.Add(levelReturn);
                    }
                }
                sectionDto.CourseLevels = courseLevelList;
            }

            string status;
            sectionDto.Status = ConvertSectionStatusToDto2(entity.Statuses.ElementAt(0).IntegrationStatus, out status);
            sectionDto.MaximumEnrollment = entity.SectionCapacity;

            if (entity.NumberOfWeeks.HasValue && entity.NumberOfWeeks > 0)
            {
                sectionDto.Duration = new SectionDuration2()
                {
                    Length = entity.NumberOfWeeks.Value,
                    Unit = DurationUnit2.Weeks
                };
            }

            var orgs = await DepartmentsAsync();
            if (orgs.Any())
            {
                var orgsList = new List<OwningOrganizationDtoProperty>();
                foreach (var code in entity.Departments)
                {
                    var owningOrg = orgs.FirstOrDefault(o => o.Code == code.AcademicDepartmentCode);
                    if (owningOrg != null)
                    {
                        var orgReturn = new OwningOrganizationDtoProperty();
                        orgReturn.Code = owningOrg.Code;
                        orgReturn.Title = owningOrg.Description;
                        orgReturn.OwnershipPercentage = code.ResponsibilityPercentage;
                        orgReturn.Detail = new GuidObject2(owningOrg.Guid);
                        orgsList.Add(orgReturn);
                    }
                }
                sectionDto.OwningOrganizations = orgsList;
            }

            var pageOfItems = await _sectionRepository.GetSectionMeetingAsync(0, 0, entity.Id, string.Empty, string.Empty, string.Empty, string.Empty, new List<string>(), new List<string>(), new List<string>(), string.Empty);
            var eventEntities = pageOfItems.Item1;

            if (eventEntities.Any())
            {
                var instructionalEventsList = new List<InstructionalEventDtoProperty2>();

                foreach (var eventInstructional in eventEntities)
                {
                    if (eventInstructional.Guid != null)
                    {
                        var returnInstructionalEventDto = new InstructionalEventDtoProperty2();
                        var instEventDto = await ConvertSectionMeetingToInstructionalEvent2Async(eventInstructional);

                        returnInstructionalEventDto.Detail = new GuidObject2(instEventDto.Id);
                        returnInstructionalEventDto.Title = instEventDto.Title;
                        var instMethod = (await _studentReferenceDataRepository.GetInstructionalMethodsAsync()).FirstOrDefault(im => im.Guid == instEventDto.InstructionalMethod.Id);

                        if (instMethod != null)
                        {
                            returnInstructionalEventDto.InstructionalMethod = new InstructionalMethodDtoProperty()
                            {
                                Abbreviation = instMethod.Code,
                                Title = instMethod.Description,
                                Detail = new GuidObject2(instMethod.Guid)
                            };
                        }

                        if (instEventDto.Recurrence != null)
                        {
                            returnInstructionalEventDto.Recurrence = instEventDto.Recurrence;
                        }

                        if ((instEventDto.Locations != null) && (instEventDto.Locations.Any()))
                        {
                            var locationsList = new List<LocationDtoProperty>();
                            var locationDtoProperty = new LocationDtoProperty();
                            var locationRoom = new LocationRoomDtoProperty();
                            var room = (await RoomsAsync()).FirstOrDefault(r => r.Id == eventInstructional.Room);

                            if (room != null)
                            {
                                locationRoom.LocationType = InstructionalLocationType.InstructionalRoom;
                                locationRoom.Title = !string.IsNullOrEmpty(room.Name) ? room.Name : null;
                                locationRoom.Number = !string.IsNullOrEmpty(room.Number) ? room.Number : null;
                                locationRoom.Floor = !string.IsNullOrEmpty(room.Floor) ? room.Floor : null;
                                locationRoom.Detail = new GuidObject2(room.Guid);

                                var building = (await _referenceDataRepository.BuildingsAsync()).FirstOrDefault(b => b.Code == room.BuildingCode);
                                if (building != null)
                                {
                                    locationRoom.Building = new BuildingDtoProperty()
                                    {
                                        Code = building.Code,
                                        Title = building.Description,
                                        Detail = new GuidObject2(building.Guid)
                                    };
                                }

                                locationDtoProperty.Location = locationRoom;
                                locationsList.Add(locationDtoProperty);
                            }
                            returnInstructionalEventDto.Locations = locationsList;

                        }

                        if ((instEventDto.Instructors != null) && (instEventDto.Instructors.Any()))
                        {
                            var instructorList = new List<InstructorRosterDtoProperty2>();

                            foreach (var eventInstructor in instEventDto.Instructors)
                            {
                                var returnInstructorRoster = new InstructorRosterDtoProperty2
                                {
                                    WorkLoadPercentage = eventInstructor.WorkLoadPercentage,
                                    ResponsibilityPercentage = eventInstructor.ResponsibilityPercentage,
                                    WorkStartDate = eventInstructor.WorkStartDate,
                                    WorkEndDate = eventInstructor.WorkEndDate
                                };

                                var returnInstructor = new InstructorDtoProperty2 { Detail = new GuidObject2(eventInstructor.Instructor.Id) };
                                var namesList = new List<InstructorNameDtoProperty2>();
                                var instructorName = new InstructorNameDtoProperty2();

                                var person = await _personRepository.GetPersonByGuidNonCachedAsync(eventInstructor.Instructor.Id);

                                var facultyList = await _sectionRepository.GetSectionFacultyAsync(0, 100, entity.Id, "", new List<string>());

                                if (facultyList != null && facultyList.Item1.Any())
                                {
                                    var faculty = facultyList.Item1.FirstOrDefault(fac => fac.FacultyId == person.Id);
                                    if (faculty != null && faculty.PrimaryIndicator)
                                    {
                                        returnInstructor.InstructorRole = SectionInstructorsInstructorRole.Primary;
                                    }
                                }


                                instructorName.NameType = new InstructorNameTypeDtoProperty()
                                {
                                    Category = InstructorNameType2.Personal
                                };

                                instructorName.Title = string.IsNullOrEmpty(person.Prefix) ? "" : person.Prefix;
                                instructorName.Pedigree = string.IsNullOrEmpty(person.Suffix) ? "" : person.Suffix;
                                instructorName.FirstName = string.IsNullOrEmpty(person.FirstName) ? "" : person.FirstName;
                                instructorName.MiddleName = string.IsNullOrEmpty(person.MiddleName) ? null : person.MiddleName;
                                instructorName.LastNamePrefix = null;
                                instructorName.LastName = person.LastName;
                                instructorName.PreferredName = string.IsNullOrEmpty(person.Nickname) ? null : person.Nickname;
                                instructorName.FullName = BuildFullName(instructorName.FirstName,
                                    instructorName.MiddleName, instructorName.LastName);

                                namesList.Add(instructorName);
                                returnInstructor.Names = namesList;

                                var credentialList = new List<CredentialDtoProperty>();

                                credentialList.Add(new CredentialDtoProperty()
                                {
                                    Type = Dtos.EnumProperties.CredentialType.ColleaguePersonId,
                                    Value = person.Id
                                });

                                if (person.PersonAltIds != null && person.PersonAltIds.Any())
                                {
                                    var elevPersonAltId = person.PersonAltIds.FirstOrDefault(a => a.Type == Domain.Base.Entities.PersonAlt.ElevatePersonAltType);
                                    if (elevPersonAltId != null && !string.IsNullOrEmpty(elevPersonAltId.Id))
                                    {
                                        credentialList.Add(new CredentialDtoProperty()
                                        {
                                            Type = Dtos.EnumProperties.CredentialType.ElevateID,
                                            Value = elevPersonAltId.Id
                                        });
                                    }
                                }

                                returnInstructor.Credentials = credentialList;
                                returnInstructorRoster.Instructor = returnInstructor;
                                instructorList.Add(returnInstructorRoster);
                            }
                            returnInstructionalEventDto.InstructorRoster = instructorList;

                        }


                        instructionalEventsList.Add(returnInstructionalEventDto);
                    }
                }

                if (instructionalEventsList.Any())
                {
                    sectionDto.InstructionalEvents = instructionalEventsList;
                }
            }

            return sectionDto;
        }


        /// <summary>
        /// Convert a Section entity into the HeDM sections format DTO
        /// </summary>
        /// <param name="entity">A Section entity</param>
        /// <returns>A HeDM-version 4 Section DTO</returns>
        private async Task<Dtos.Section3> ConvertSectionEntityToDto3Async(Domain.Student.Entities.Section entity)
        {
            if (entity == null)
            {
                return null;
            }

            var sectionDto = new Dtos.Section3();

            sectionDto.Id = entity.Guid.ToLowerInvariant();
            sectionDto.Code = entity.Name;
            sectionDto.Number = entity.Number;
            sectionDto.Title = entity.Title;
            sectionDto.StartOn = entity.StartDate;
            sectionDto.EndOn = entity.EndDate;

            if (!string.IsNullOrEmpty(entity.TermId))
            {
                sectionDto.AcademicPeriod = new Dtos.GuidObject2();
                var academicPeriod = academicPeriods.FirstOrDefault(a => a.Code == entity.TermId);
                sectionDto.AcademicPeriod.Id = (academicPeriod != null) ? academicPeriod.Guid : null;
            }
            if (string.IsNullOrEmpty(entity.LearningProvider))
            {
                if (!string.IsNullOrEmpty(entity.DefaultLearningProvider))
                {
                    entity.LearningProvider = entity.DefaultLearningProvider;
                }
            }
            if (!string.IsNullOrEmpty(entity.LearningProvider))
            {
                var instructionalPlatform = (await InstructionalPlatformsAsync()).FirstOrDefault(i => i.Code == entity.LearningProvider);
                if (instructionalPlatform != null && !string.IsNullOrEmpty(instructionalPlatform.Guid))
                {
                    sectionDto.InstructionalPlatform = new Dtos.GuidObject2(instructionalPlatform.Guid);
                }
            }
            sectionDto.Course = new Dtos.GuidObject2(await _courseRepository.GetCourseGuidFromIdAsync(entity.CourseId));

            var credit = new Dtos.DtoProperties.SectionCreditDtoProperty();
            var ceuCredit = new Dtos.DtoProperties.SectionCreditDtoProperty();
            credit.CreditCategory = new CreditIdAndTypeProperty2();
            ceuCredit.CreditCategory = new CreditIdAndTypeProperty2();

            var creditTypeItems = await CreditTypesAsync();

            if (creditTypeItems.Any(ct => ct.Code == entity.CreditTypeCode))
            {
                var creditTypeItem = creditTypeItems.Where(ct => ct.Code == entity.CreditTypeCode).First();
                if (creditTypeItem != null)
                {
                    if (!string.IsNullOrEmpty(creditTypeItem.Guid))
                    {
                        credit.CreditCategory.Detail = new GuidObject2() { Id = creditTypeItem.Guid };
                        ceuCredit.CreditCategory.Detail = new GuidObject2() { Id = creditTypeItem.Guid };
                    }
                    switch (creditTypeItem.CreditType)
                    {
                        case CreditType.ContinuingEducation:
                            credit.CreditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
                            break;
                        case CreditType.Institutional:
                            credit.CreditCategory.CreditType = CreditCategoryType3.Institutional;
                            break;
                        case CreditType.Transfer:
                            credit.CreditCategory.CreditType = CreditCategoryType3.Transfer;
                            break;
                        case CreditType.Exchange:
                            credit.CreditCategory.CreditType = CreditCategoryType3.Exchange;
                            break;
                        case CreditType.Other:
                            credit.CreditCategory.CreditType = CreditCategoryType3.Other;
                            break;
                        case CreditType.None:
                            credit.CreditCategory.CreditType = CreditCategoryType3.NoCredit;
                            break;
                        default:
                            credit.CreditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
                            break;
                    }
                }
            }
            else
            {
                //default to CE
                credit.CreditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
            }

            ceuCredit.CreditCategory.CreditType = credit.CreditCategory.CreditType;


            if (entity.Ceus.HasValue)
            {
                ceuCredit.Measure = Dtos.CreditMeasure2.CEU;
                ceuCredit.Minimum = entity.Ceus.Value;
                sectionDto.Credits.Add(ceuCredit);
            }
            if (entity.MinimumCredits.HasValue)
            {
                credit.Measure = Dtos.CreditMeasure2.Credit;
                credit.Minimum = entity.MinimumCredits.GetValueOrDefault();
                credit.Maximum = entity.MaximumCredits;
                credit.Increment = entity.VariableCreditIncrement;
                sectionDto.Credits.Add(credit);
            }


            if (!string.IsNullOrEmpty(entity.Location))
            {
                sectionDto.Site = new Dtos.GuidObject2(ConvertCodeToGuid(locations, entity.Location));
            }
            sectionDto.AcademicLevels.Add(new Dtos.GuidObject2(ConvertCodeToGuid((await AcademicLevelsAsync()), entity.AcademicLevelCode)));
            var gradeScheme = (await GradeSchemesAsync()).Where(gs => gs != null && gs.Code == entity.GradeSchemeCode).FirstOrDefault();
            var gradeSchemeGuid = gradeScheme != null ? gradeScheme.Guid : null;
            sectionDto.GradeSchemes.Add(new Dtos.GuidObject2(gradeSchemeGuid));
            foreach (var code in entity.CourseLevelCodes)
            {
                sectionDto.CourseLevels.Add(new Dtos.GuidObject2(ConvertCodeToGuid(await CourseLevelsAsync(), code)));
            }
            string status;
            sectionDto.Status = ConvertSectionStatusToDto2(entity.Statuses.ElementAt(0).IntegrationStatus, out status);
            sectionDto.MaximumEnrollment = entity.GlobalCapacity ?? entity.SectionCapacity;

            // Determine the Department information for the course
            sectionDto.OwningInstitutionUnits = new List<Ellucian.Colleague.Dtos.OwningInstitutionUnit>();
            var departments = new List<Ellucian.Colleague.Dtos.OwningInstitutionUnit>();

            if (entity.Departments != null && entity.Departments.Any())
            {
                foreach (var offeringDept in entity.Departments)
                {
                    var academicDepartment = (await AllDepartments()).FirstOrDefault(d => d.Code == offeringDept.AcademicDepartmentCode);
                    if (academicDepartment != null)
                    {
                        var department = new Ellucian.Colleague.Dtos.OwningInstitutionUnit();
                        department.InstitutionUnit.Id = academicDepartment.Guid;
                        department.OwnershipPercentage = offeringDept.ResponsibilityPercentage;
                        departments.Add(department);
                    }
                }
                sectionDto.OwningInstitutionUnits = departments;
            }

            if (entity.NumberOfWeeks.HasValue)
            {
                sectionDto.Duration = new SectionDuration2()
                {
                    Length = entity.NumberOfWeeks.Value,
                    Unit = DurationUnit2.Weeks
                };
            }

            return sectionDto;
        }

        /// <summary>
        /// Convert the HeDM sections format DTO into a Section entity
        /// </summary>
        /// <param name="entity">A Data Model version 6 Section DTO</param>
        /// <returns>A Section entity</returns>
        private async Task<Domain.Student.Entities.Section> ConvertSectionDto3ToEntityAsync(Dtos.Section3 sectionDto)
        {
            if (sectionDto == null)
            {
                throw new ArgumentNullException("sectionDto", "Section DTO must be provided.");
            }
            if (string.IsNullOrEmpty(sectionDto.Id))
            {
                throw new ArgumentException("Section GUID not specified.");
            }

            if (sectionDto.Course == null)
            {
                throw new ArgumentNullException("course", "course is required.");
            }

            if (string.IsNullOrEmpty(sectionDto.Title))
            {
                throw new ArgumentNullException("title", "title is required.");
            }

            if (sectionDto.StartOn == null)
            {
                throw new ArgumentNullException("startOn", "startOn is required.");
            }

            if (sectionDto.StartOn.HasValue && sectionDto.EndOn.HasValue)
            {
                if (sectionDto.StartOn.Value > sectionDto.EndOn.Value)
                {
                    throw new ArgumentException("endOn can not occur earlier than startOn.", "endOn");
                }
            }

            if (sectionDto.Credits.Any())
            {
                foreach (var credit in sectionDto.Credits)
                {
                    if (credit.CreditCategory == null)
                    {
                        throw new ArgumentException(
                            "Section provided is not valid, Credits must contain a Credit Category.", "credit category");
                    }

                    if (credit.Measure == null)
                    {
                        throw new ArgumentException("Section provided is not valid, Credits must contain a Measure.",
                            "credit measure");
                    }

                    if (credit.Minimum == null)
                    {
                        throw new ArgumentException("Section provided is not valid, Credits must contain a Minimum.",
                            "credit minimum");
                    }

                    if (credit.CreditCategory.CreditType == null)
                    {
                        throw new ArgumentException("Credit Type is required if for Credit Categories if Credits are in the message body.");
                    }

                    if (credit.CreditCategory.Detail != null && string.IsNullOrEmpty(credit.CreditCategory.Detail.Id))
                    {
                        throw new ArgumentException("Credit Category id is required within the detail section of Credit Category if it is in the message body.");
                    }
                    if (credit.Increment == null && credit.Maximum != null)
                    {
                        throw new ArgumentException("Credit Increment is required when Credit Maximum exists.");
                    }
                    if (credit.Maximum == null && credit.Increment != null)
                    {
                        throw new ArgumentException("Credit Maximum is required when Credit Increment exists.");
                    }
                    if (credit.Maximum != null && credit.Measure == CreditMeasure2.CEU)
                    {
                        throw new ArgumentException("Credit Maximum cannot exist when Credit Measure is 'ceu'.");
                    }
                    if (credit.Increment != null && credit.Measure == CreditMeasure2.CEU)
                    {
                        throw new ArgumentException("Credit Increment cannot exist when Credit Measure is 'ceu'.");
                    }
                }

            }

            if (sectionDto.CourseLevels != null && sectionDto.CourseLevels.Any())
            {
                foreach (var level in sectionDto.CourseLevels)
                {
                    if (string.IsNullOrEmpty(level.Id))
                    {
                        throw new ArgumentException(
                            "Course Level id is a required field when Course Levels are in the message body.");
                    }
                }
            }

            if (sectionDto.InstructionalPlatform != null)
            {
                if (string.IsNullOrEmpty(sectionDto.InstructionalPlatform.Id))
                {
                    throw new ArgumentException(
                        "Instructional Platform id is a required field when Instructional Methods are in the message body.");
                }
            }

            if (sectionDto.AcademicLevels != null && sectionDto.AcademicLevels.Any())
            {
                foreach (var level in sectionDto.AcademicLevels)
                {
                    if (string.IsNullOrEmpty(level.Id))
                    {
                        throw new ArgumentException(
                            "Academic Level id is a required field when Academic Levels are in the message body.");
                    }
                }
            }

            if (sectionDto.GradeSchemes != null && sectionDto.GradeSchemes.Any())
            {
                foreach (var scheme in sectionDto.GradeSchemes)
                {
                    if (string.IsNullOrEmpty(scheme.Id))
                    {
                        throw new ArgumentException(
                            "Grade Scheme id is a required field when Grade Schemes are in the message body.");
                    }
                }
            }

            if (sectionDto.OwningInstitutionUnits != null && sectionDto.OwningInstitutionUnits.Any())
            {
                foreach (var org in sectionDto.OwningInstitutionUnits)
                {
                    if (org.InstitutionUnit == null || string.IsNullOrEmpty(org.InstitutionUnit.Id))
                    {
                        throw new ArgumentException("Institution Unit id is a required field when Owning Organizations are in the message body.");
                    }
                }
            }

            if (sectionDto.Credits != null && sectionDto.Credits.Any())
            {
                foreach (var credit in sectionDto.Credits)
                {
                    if (credit.CreditCategory == null)
                    {
                        throw new ArgumentException("Credit Category is required if Credits are in the message body.");
                    }

                    if (credit.CreditCategory.CreditType == null)
                    {
                        throw new ArgumentException("Credit Type is required for Credit Categories if Credits are in the message body.");
                    }
                }
            }

            if (sectionDto.Duration != null && (sectionDto.Duration.Unit == DurationUnit2.Months || sectionDto.Duration.Unit == DurationUnit2.Years || sectionDto.Duration.Unit == DurationUnit2.Days))
            {
                throw new ArgumentException("Section Duration Unit is not allowed to be set to Days, Months or Years");
            }

            if (sectionDto.Duration != null && sectionDto.Duration.Unit == DurationUnit2.Weeks && sectionDto.Duration.Length < 0)
            {
                throw new ArgumentException("Section Duration Length must be a positive number.");
            }

            if (sectionDto.Duration != null && sectionDto.Duration.Unit == null)
            {
                throw new ArgumentException("Section Duration Unit must contain a value if the unit property is specified.");
            }

            if (sectionDto.Credits.Count() > 2)
            {
                throw new ArgumentException("A maximum of 2 entries are allowed in the Credits array.");
            }
            if (sectionDto.Credits.Count() == 2)
            {
                if (sectionDto.Credits.ElementAt(0).CreditCategory.CreditType != sectionDto.Credits.ElementAt(1).CreditCategory.CreditType)
                {
                    throw new ArgumentException("The same Credit Type must be used for each entry in the Credits array.");
                }
                if (!(sectionDto.Credits.ElementAt(0).Measure == CreditMeasure2.CEU && sectionDto.Credits.ElementAt(1).Measure == CreditMeasure2.Credit)
                    && !(sectionDto.Credits.ElementAt(0).Measure == CreditMeasure2.Credit && sectionDto.Credits.ElementAt(1).Measure == CreditMeasure2.CEU)
                    && !(sectionDto.Credits.ElementAt(0).Measure == CreditMeasure2.CEU && sectionDto.Credits.ElementAt(1).Measure == CreditMeasure2.Hours)
                    && !(sectionDto.Credits.ElementAt(0).Measure == CreditMeasure2.Hours && sectionDto.Credits.ElementAt(1).Measure == CreditMeasure2.CEU))
                {
                    throw new ArgumentException("Invalid combination of measures '" + sectionDto.Credits.ElementAt(0).Measure
                        + "' and '" + sectionDto.Credits.ElementAt(1).Measure + "'");
                }
            }


            Domain.Student.Entities.Section section = null;

            try
            {
                section = await _sectionRepository.GetSectionByGuidAsync(sectionDto.Id);
            }
            catch (Exception)
            {
            }

            string id = section == null ? null : section.Id;
            var currentStatuses = section == null || section.Statuses == null
                ? new List<SectionStatusItem>()
                : section.Statuses.ToList();
            var curriculumConfiguration = await GetCurriculumConfiguration2Async();
            var statuses = await UpdateSectionStatus3(currentStatuses, sectionDto.Status);

            var course = await _courseRepository.GetCourseByGuidAsync(sectionDto.Course.Id);

            // Set the credit type and credits/CEUs for the course based on the supplied GUID, or using the ERP default 
            Ellucian.Colleague.Domain.Student.Entities.CreditCategory creditCategory = null;
            if (sectionDto.Credits != null &&
                sectionDto.Credits.Any() &&
                sectionDto.Credits.ToList()[0].CreditCategory != null &&
                sectionDto.Credits.ToList()[0].CreditCategory.Detail != null &&
                !string.IsNullOrEmpty(sectionDto.Credits.ToList()[0].CreditCategory.Detail.Id))
            {
                creditCategory = (await CreditTypesAsync()).FirstOrDefault(ct => ct.Guid == sectionDto.Credits.ToList()[0].CreditCategory.Detail.Id);
                if (creditCategory == null)
                {
                    throw new ArgumentException("Invalid Id '" + sectionDto.Credits.ToList()[0].CreditCategory.Detail.Id + "' supplied for creditCategory");
                }
            }
            // If we don't have a GUID then check for a CreditType enumeration value
            if (creditCategory == null &&
                sectionDto.Credits != null &&
                sectionDto.Credits.Any() &&
                sectionDto.Credits.ToList()[0].CreditCategory != null &&
                sectionDto.Credits.ToList()[0].CreditCategory.CreditType != null)
            {
                if (sectionDto.Credits.ToList()[0].CreditCategory.CreditType == CreditCategoryType3.Exam ||
                    sectionDto.Credits.ToList()[0].CreditCategory.CreditType == CreditCategoryType3.WorkLifeExperience)
                {
                    throw new InvalidOperationException("Credit category type 'exam' or 'workLifeExperience' are not supported.");
                }

                // Find the credit category that matches the enumeration
                switch (sectionDto.Credits.ToList()[0].CreditCategory.CreditType)
                {
                    case (CreditCategoryType3.ContinuingEducation):
                        creditCategory = (await CreditTypesAsync()).FirstOrDefault(cc => cc.CreditType == CreditType.ContinuingEducation);
                        break;
                    case (CreditCategoryType3.Institutional):
                        creditCategory = (await CreditTypesAsync()).FirstOrDefault(cc => cc.CreditType == CreditType.Institutional);
                        break;
                    case (CreditCategoryType3.Transfer):
                        creditCategory = (await CreditTypesAsync()).FirstOrDefault(cc => cc.CreditType == CreditType.Transfer);
                        break;
                    case (CreditCategoryType3.Exchange):
                        creditCategory = (await CreditTypesAsync()).FirstOrDefault(cc => cc.CreditType == CreditType.Exchange);
                        break;
                    case (CreditCategoryType3.Other):
                        creditCategory = (await CreditTypesAsync()).FirstOrDefault(cc => cc.CreditType == CreditType.Other);
                        break;
                    case (CreditCategoryType3.NoCredit):
                        creditCategory = (await CreditTypesAsync()).FirstOrDefault(cc => cc.CreditType == CreditType.None);
                        break;
                }
            }

            //If creditCategory is null then throw an exception
            if (creditCategory == null)
            {
                throw new ArgumentException("Credits data requires Credit Category Type or Id");
            }

            var creditTypeCode = creditCategory == null ? string.Empty : creditCategory.Code;

            decimal? minCredits;
            decimal? ceus;
            decimal? maxCredits;
            decimal? varIncrCredits;
            minCredits = null;
            maxCredits = null;
            varIncrCredits = null;
            ceus = null;
            foreach (var credits in sectionDto.Credits)
            {
                var creditInfo = (sectionDto.Credits == null || sectionDto.Credits.Count == 0) ? null : credits;
                var measure = creditInfo == null ? null : creditInfo.Measure;
                if (measure == Dtos.CreditMeasure2.CEU)
                {
                    ceus = creditInfo == null ? 0 : creditInfo.Minimum;
                }
                else
                {
                    minCredits = creditInfo == null ? 0 : creditInfo.Minimum;
                    maxCredits = creditInfo == null ? null : creditInfo.Maximum;
                    varIncrCredits = creditInfo == null ? null : creditInfo.Increment;
                }
            }

            if (sectionDto.Credits == null || !sectionDto.Credits.Any())
            {
                if (course != null)
                {
                    minCredits = course.MinimumCredits;
                    ceus = course.Ceus;
                    creditTypeCode = course.LocalCreditType;
                }
            }

            // Set the list of departments/shares
            List<OfferingDepartment> offeringDepartments = new List<OfferingDepartment>();

            // If we have supplied owning organization, then use that first.
            if (sectionDto.OwningInstitutionUnits != null && sectionDto.OwningInstitutionUnits.Any())
            {
                foreach (var owningInstitutionUnit in sectionDto.OwningInstitutionUnits)
                {

                    var division = (await _referenceDataRepository.GetDivisionsAsync(true))
                                            .FirstOrDefault(div => div.Guid.Equals(owningInstitutionUnit.InstitutionUnit.Id, StringComparison.OrdinalIgnoreCase));
                    if (division != null)
                    {
                        throw new InvalidOperationException("Owning institution unit of type 'division' is not supported.");
                    }

                    var school = (await _referenceDataRepository.GetSchoolsAsync(true))
                                            .FirstOrDefault(div => div.Guid.Equals(owningInstitutionUnit.InstitutionUnit.Id, StringComparison.OrdinalIgnoreCase));
                    if (school != null)
                    {
                        throw new InvalidOperationException("Owning institution unit of type 'school' is not supported.");
                    }

                    var department = (await DepartmentsAsync()).Where(d => d.Guid == owningInstitutionUnit.InstitutionUnit.Id).FirstOrDefault();
                    var academicDepartment = department != null ? (await _studentReferenceDataRepository.GetAcademicDepartmentsAsync()).FirstOrDefault(ad => ad.Code == department.Code) : null;
                    if (academicDepartment != null)
                    {
                        offeringDepartments.Add(new OfferingDepartment(academicDepartment.Code, owningInstitutionUnit.OwnershipPercentage));
                    }
                }
            }
            // If we don't have offering departments in the payload and didn't find
            // any departments, we will default from the course.  This has to be done
            // here instead of the Colleague transaction because the Sections Entity
            // requires at least one department.
            if (offeringDepartments.Count == 0)
            {
                offeringDepartments.AddRange(course.Departments);
            }

            var courseLevels = await CourseLevelsAsync();
            // Convert various codes to their Colleague values or get them from the course
            List<string> courseLevelCodes = new List<string>();
            if (sectionDto.CourseLevels == null || sectionDto.CourseLevels.Count == 0)
            {
                courseLevelCodes = course.CourseLevelCodes;
            }
            else
            {
                foreach (var level in sectionDto.CourseLevels)
                {
                    var courseLevelCode = ConvertGuidToCode(courseLevels, level.Id);
                    if (string.IsNullOrEmpty(courseLevelCode))
                    {
                        throw new ArgumentException(string.Concat("Invalid Id '", level.Id, "' supplied for courseLevels"));
                    }
                    else
                    {
                        courseLevelCodes.Add(courseLevelCode);
                    }
                }
            }

            string academicLevel = course.AcademicLevelCode;
            if (sectionDto.AcademicLevels != null && sectionDto.AcademicLevels.Any())
            {
                var tempAcadCode = ConvertGuidToCode((await AcademicLevelsAsync()), sectionDto.AcademicLevels[0].Id);
                if (!string.IsNullOrEmpty(tempAcadCode))
                {
                    academicLevel = tempAcadCode;
                }
            }

            string gradeScheme = null;
            if (sectionDto.GradeSchemes == null || sectionDto.GradeSchemes.Count == 0)
            {
                gradeScheme = course.GradeSchemeCode;
            }
            else
            {
                var gradeSchemeEntity = (await GradeSchemesAsync()).Where(gs => gs != null && gs.Guid == sectionDto.GradeSchemes[0].Id).FirstOrDefault();
                var schemeCode = gradeSchemeEntity != null ? gradeSchemeEntity.Code : null;
                if (string.IsNullOrEmpty(schemeCode))
                {
                    throw new ArgumentException(string.Concat("Invalid Id '", sectionDto.GradeSchemes[0].Id, "' supplied for gradeSchemes"));
                }
                else
                {
                    gradeScheme = schemeCode;
                }
            }

            string site = sectionDto.Site == null ? null : ConvertGuidToCode(locations, sectionDto.Site.Id);

            string learningProvider = null;
            if (sectionDto.InstructionalPlatform != null)
            {
                var providerCode = ConvertGuidToCode((await InstructionalPlatformsAsync()), sectionDto.InstructionalPlatform.Id);
                if (string.IsNullOrEmpty(providerCode))
                {
                    throw new ArgumentException(string.Concat("Invalid Id '", sectionDto.InstructionalPlatform.Id, "' supplied for instructionalMethods"));
                }
                else
                {
                    learningProvider = providerCode;
                }
            }

            // string term = sectionDto.AcademicPeriod == null ? null : ConvertGuidToCode(academicPeriods, sectionDto.AcademicPeriod.Id);
            string term = string.Empty;
            if (sectionDto.AcademicPeriod != null && !string.IsNullOrEmpty(sectionDto.AcademicPeriod.Id))
            {
                term = ConvertGuidToCode(academicPeriods, sectionDto.AcademicPeriod.Id);
                if (string.IsNullOrEmpty(term))
                {
                    throw new ArgumentException(string.Format("Invalid Id '{0}' supplied for academicPeriod.", sectionDto.AcademicPeriod.Id));
                }
            }

            // Create the section entity
            var entity = new Domain.Student.Entities.Section(id, course.Id, sectionDto.Number, sectionDto.StartOn.GetValueOrDefault().DateTime,
                minCredits, ceus, sectionDto.Title, creditTypeCode, offeringDepartments, courseLevelCodes, academicLevel, statuses,
                course.AllowPassNoPass, course.AllowAudit, course.OnlyPassNoPass, course.AllowWaitlist, false)
            {
                Guid = (sectionDto.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase)) ? string.Empty : sectionDto.Id.ToLowerInvariant(),
                EndDate = sectionDto.EndOn == null ? default(DateTime?) : sectionDto.EndOn.Value.Date,
                MaximumCredits = maxCredits,
                VariableCreditIncrement = varIncrCredits,
                GradeSchemeCode = gradeScheme,
                TopicCode = course.TopicCode,
                GlobalCapacity = sectionDto.MaximumEnrollment,
                SectionCapacity = sectionDto.MaximumEnrollment,
                Name = sectionDto.Code,
                LearningProvider = learningProvider,
                TermId = term,
                Location = site
            };
            foreach (var item in course.Types)
            {
                entity.AddCourseType(item);
            }

            if (string.IsNullOrEmpty(entity.TermId))
            {
                entity.TermId = SectionProcessor.DetermineTerm(entity, await AllTermsAsync());
            }

            //check if duration is set on the incoming DTO
            if (sectionDto.Duration != null)
            {
                entity.NumberOfWeeks = sectionDto.Duration.Length;
            }
            else
            {
                if (entity.EndDate.HasValue)
                {
                    entity.NumberOfWeeks = (int)Math.Ceiling((entity.EndDate.Value - entity.StartDate).Days / 7m);
                }
            }

            return entity;
        }

        private async Task<List<SectionStatusItem>> UpdateSectionStatus3(List<SectionStatusItem> currentStatuses, Dtos.SectionStatus2? dtoStatus)
        //string activeStatusCode, string inactiveStatusCode)
        {
            var statuses = new List<SectionStatusItem>();
            string code;
            var newStatus = ConvertSectionStatusFromDto3(dtoStatus, out code);
            if (currentStatuses == null || currentStatuses.Count == 0)
            {
                // No existing statuses
                if (newStatus.HasValue)
                {
                    // We have a new status - add it with today's date
                    var allStatuses = await SectionStatusesAsync();
                    Domain.Student.Entities.SectionStatusCode status = null;
                    foreach (var stat in allStatuses)
                    {
                        if (stat.IntegrationStatusType.HasValue && stat.IntegrationStatusType.Value.ToString() == newStatus.Value.ToString())
                        {
                            status = stat;
                        }
                    }
                    if (status == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Section status code found for: ", newStatus.Value.ToString()));
                    }

                    statuses.Add(new SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, newStatus.Value, status.Code, DateTime.Today));
                }
            }
            else
            {
                var currentStatus = currentStatuses[0];
                if (newStatus.HasValue && newStatus.Value != currentStatus.IntegrationStatus)
                {
                    // New status found and it's different - add it to the etop of the list
                    var status = (await SectionStatusesAsync()).FirstOrDefault(i => i.IntegrationStatusType.Value.ToString().Equals(newStatus.Value.ToString()));
                    if (status == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Section status code found for: ", newStatus.Value.ToString()));
                    }

                    statuses.Add(new SectionStatusItem(currentStatus.Status, newStatus.Value, status.Code, DateTime.Today));
                    statuses.AddRange(currentStatuses);
                }
                else
                {
                    // No new value - the current statuses haven't changed
                    statuses = currentStatuses;
                }
            }

            return statuses;
        }
        private async Task<List<SectionStatusItem>> UpdateSectionStatus4(List<SectionStatusItem> currentStatuses, SectionStatusDtoProperty status)
        //string activeStatusCode, string inactiveStatusCode)
        {

            var statuses = new List<SectionStatusItem>();
            string guid = status.Detail.Id;

            try
            {
                var newStatusCodeGuid = (await AllStatusesWithGuidsAsync()).Single(swg => swg.Guid == status.Detail.Id);
                var allSectionStatusCodes = await SectionStatusesAsync();
                var newStatusCode = allSectionStatusCodes.Single(ss => ss.Code == newStatusCodeGuid.Code);

                if (currentStatuses == null || currentStatuses.Count == 0)
                {
                    statuses.Add(new SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, newStatusCode.IntegrationStatusType.Value, newStatusCodeGuid.Code, DateTime.Today));
                }
                else
                {
                    var currentStatus = currentStatuses[0];
                    if (newStatusCode.Code != currentStatus.StatusCode)
                    {
                        statuses.Add(new SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, newStatusCode.IntegrationStatusType.Value, newStatusCodeGuid.Code, DateTime.Today));
                        statuses.AddRange(currentStatuses);
                    }
                    else
                    {
                        // No new value - the current statuses haven't changed
                        statuses = currentStatuses;
                    }
                }
            }
            catch
            {
                throw new ApplicationException("Section status guid '" + status.Detail.Id + "' was not found");
            }

            return statuses;
        }
        private Dtos.SectionStatus2? ConvertSectionStatusToDto3(Domain.Student.Entities.SectionStatusIntegration? status, out string statusCode)
        {
            statusCode = null;
            if (!status.HasValue)
            {
                return null;
            }
            switch (status)
            {
                case Domain.Student.Entities.SectionStatusIntegration.Open:
                    return Dtos.SectionStatus2.Open;
                case Domain.Student.Entities.SectionStatusIntegration.Cancelled:
                    return Dtos.SectionStatus2.Cancelled;
                case Domain.Student.Entities.SectionStatusIntegration.Pending:
                    return Dtos.SectionStatus2.Pending;
                case Domain.Student.Entities.SectionStatusIntegration.NotSet:
                    return Dtos.SectionStatus2.NotSet;
                default:
                    return Dtos.SectionStatus2.Closed;
            }
        }

        private Domain.Student.Entities.SectionStatusIntegration? ConvertSectionStatusFromDto3(Dtos.SectionStatus2? status, out string statusCode)
        {
            statusCode = null;
            if (!status.HasValue)
            {
                return null;
            }
            switch (status)
            {
                case Dtos.SectionStatus2.Open:
                    return Domain.Student.Entities.SectionStatusIntegration.Open;
                case Dtos.SectionStatus2.Cancelled:
                    return Domain.Student.Entities.SectionStatusIntegration.Cancelled;
                case Dtos.SectionStatus2.Pending:
                    return Domain.Student.Entities.SectionStatusIntegration.Pending;
                case Dtos.SectionStatus2.NotSet:
                    return Domain.Student.Entities.SectionStatusIntegration.NotSet;
                default:
                    return Domain.Student.Entities.SectionStatusIntegration.Closed;
            }
        }

        #endregion

        #region HeDM Version 8

        /// <summary>
        /// Get selection criteria, convert it to codes as required and select 
        /// multiple sections from Colleague.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="title">Section Title Contains ...title...</param>
        /// <param name="startOn">Section starts on or after this date</param>
        /// <param name="endOn">Section ends on or before this date</param>
        /// <param name="code">Section Name Contains ...code...</param>
        /// <param name="number">Section Number equal to</param>
        /// <param name="instructionalPlatform">Learning Platform equal to (guid)</param>
        /// <param name="academicPeriod">Section Term equal to (guid)</param>
        /// <param name="academicLevel">Section Academic Level equal to (guid)</param>
        /// <param name="course">Section Course equal to (guid)</param>
        /// <param name="site">Section Location equal to (guid)</param>
        /// <param name="status">Section Status matches closed, open, pending, or cancelled</param>
        /// <param name="owningOrganization">Section Department equal to (guid)</param>
        /// <param name="subject">Section Title Contains ...subject...</param>
        /// <param name="instructor">Section Instructor equal to (guid)</param>
        /// <param name="search">Check if a section is searchable or hidden</param>
        /// <param name="keyword">Perform a keyword search</param>
        /// <returns>List of Section4 <see cref="Dtos.Section4"/> objects representing matching sections</returns>
        public async Task<Tuple<IEnumerable<Dtos.Section4>, int>> GetSections4Async(int offset, int limit, string title = "", string startOn = "", string endOn = "",
            string code = "", string number = "", string instructionalPlatform = "", string academicPeriod = "",
            List<string> academicLevel = null, string course = "", string site = "", string status = "", List<string> owningOrganization = null,
            string subject = "", string instructor = "", SectionsSearchable search = SectionsSearchable.NotSet, string keyword = "")
        {
            // Convert and validate all input parameters

            var args = new Dictionary<string, string>();
            try
            {
                args = await ValidateAndConvertFilterArguments(startOn, endOn, instructionalPlatform, academicPeriod, course, site, status, instructor, subject: subject);
            }
            catch (ArgumentException)
            {
                // One or more of the arguments failed to match up to a guid.  Return empty set.
                return new Tuple<IEnumerable<Dtos.Section4>, int>(new List<Dtos.Section4>(), 0);

            }

            var arrayArgs = new Dictionary<string, List<string>>();
            try
            {
                arrayArgs = await ValidateAndConvertFilterArrayArguments(academicLevel, owningOrganization);
            }
            catch (ArgumentException)
            {
                // One or more of the arguments failed to match up to a guid.  Return empty set.
                return new Tuple<IEnumerable<Dtos.Section4>, int>(new List<Dtos.Section4>(), 0);

            }
            var newStartOn = args["newStartOn"];
            var newEndOn = args["newEndOn"];
            var newInstructionalPlatform = args["newInstructionalPlatform"];
            var newAcademicPeriod = args["newAcademicPeriod"];
            var instructorId = args["instructorId"];
            var newCourse = args["newCourse"];
            var newSite = args["newSite"];
            var newStatus = args["newStatus"];
            var newOwningOrganization = arrayArgs["newOwningOrganizations"];
            var newAcademicLevel = arrayArgs["newAcademicLevels"];
            var newSubject = args["newSubject"];

            var sectionDtos = new List<Dtos.Section4>();

            Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section>, int> sectionEntities = null;

            if (search != SectionsSearchable.NotSet)
            {
                sectionEntities = await _sectionRepository.GetSectionsSearchableAsync(offset, limit, search.ToString());
            }
            else if (!(string.IsNullOrEmpty(keyword)))
            {
                sectionEntities = await _sectionRepository.GetSectionsKeywordAsync(offset, limit, keyword);
            }
            else
            {
                sectionEntities = await _sectionRepository.GetSectionsAsync(offset, limit, title, newStartOn, newEndOn,
                    code, number, newInstructionalPlatform, newAcademicPeriod,
                    newAcademicLevel, newCourse, newSite, newStatus, newOwningOrganization, newSubject, instructorId);
            }
            foreach (var sectionEntity in sectionEntities.Item1)
            {
                if (sectionEntity.Guid != null)
                {
                    var sectionDto = await ConvertSectionEntityToDto4Async(sectionEntity);
                    sectionDtos.Add(sectionDto);
                }
            }
            return new Tuple<IEnumerable<Dtos.Section4>, int>(sectionDtos, sectionEntities.Item2);
        }

        /// <summary>
        /// Get a Data Model section version 8 by its GUID
        /// </summary>
        /// <param name="guid">GUID of section</param>
        /// <returns>The Data Model Section version 6 DTO</returns>
        public async Task<Dtos.Section4> GetSection4ByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get a section.");
            }
            var sectionEntity = await _sectionRepository.GetSectionByGuidAsync(guid);
            if (sectionEntity == null)
            {
                throw new KeyNotFoundException(string.Concat("Section not found or invalid for id '", guid, "'.  See Log for more information."));
            }
            var sectionDto = await ConvertSectionEntityToDto4Async(sectionEntity);
            return sectionDto;
        }

        /// <summary>
        /// Create a new section
        /// </summary>
        /// <param name="section">DTO containing the Data Model version 6 section to create</param>
        /// <returns>DTO containing the created Data Model version 6 section</returns>
        public async Task<Dtos.Section4> PostSection4Async(Dtos.Section4 section)
        {
          
            if ((section != null) && (section.Status != null))
                await ValidateSectionStatusConfigurationAsync(section.Status);

            _sectionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            // Make sure dept percentage is from 0-100%.  This is also checked in the contructor for the domain entity OfferingDepartment,
            // but we want to customize the error message for Ethos APIs to refernece "Ownership percentage".
            if (section != null)
            {
                if (section.OwningInstitutionUnits != null && section.OwningInstitutionUnits.Any())
                {
                    foreach (var owningInstitutionUnit in section.OwningInstitutionUnits)
                    {
                        if (owningInstitutionUnit != null)
                        {
                            if (owningInstitutionUnit.OwnershipPercentage < 0 || owningInstitutionUnit.OwnershipPercentage > 100)
                            {
                                throw new ArgumentOutOfRangeException("ownershipPercentage", "Ownership percentage must be between 0 and 100.");
                            }
                        }
                    }
                }
            }
            // Convert the CDM section into a domain entity and create it
            var entity = await ConvertSectionDto4ToEntityAsync(section);
            if (entity == null)
            {
                throw new KeyNotFoundException("Failed to create a new section. ");
            }
            var newEntity = await _sectionRepository.PostSectionAsync(entity);
            if (entity == null)
            {
                throw new KeyNotFoundException("Failed to return a valid section. ");
            }
            var newDto = await ConvertSectionEntityToDto4Async(newEntity);

            return newDto;
        }

        /// <summary>
        /// Update a section
        /// </summary>
        /// <param name="section">DTO containing the Data Model version 8 section to update</param>
        /// <returns>DTO containing the updated Data Model version 8 section</returns>
        public async Task<Dtos.Section4> PutSection4Async(Dtos.Section4 section)
        {
            
            if ((section != null) && (section.Status != null))
                await ValidateSectionStatusConfigurationAsync(section.Status);

            _sectionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            // Make sure dept percentage is from 0-100%.  This is also checked in the contructor for the domain entity OfferingDepartment,
            // but we want to customize the error message for Ethos APIs to refernece "Ownership percentage".
            if (section != null)
            {
                if (section.OwningInstitutionUnits != null && section.OwningInstitutionUnits.Any())
                {
                    foreach (var owningInstitutionUnit in section.OwningInstitutionUnits)
                    {
                        if (owningInstitutionUnit != null)
                        {
                            if (owningInstitutionUnit.OwnershipPercentage < 0 || owningInstitutionUnit.OwnershipPercentage > 100)
                            {
                                throw new ArgumentOutOfRangeException("ownershipPercentage", "Ownership percentage must be between 0 and 100.");
                            }
                        }
                    }
                }
            }
            // Convert the CDM section into a domain entity and update it
            var entity = await ConvertSectionDto4ToEntityAsync(section);
            if (entity == null)
            {
                throw new KeyNotFoundException(string.Format("Could not update section '{0}'. ", section.Id));
            }
            var updatedEntity = await _sectionRepository.PutSectionAsync(entity);
            if (updatedEntity == null)
            {
                throw new KeyNotFoundException(string.Format("GUID '{0}' failed to return a valid section. ", section.Id));
            }
            var updatedDto = await ConvertSectionEntityToDto4Async(updatedEntity);

            return updatedDto;
        }

        #endregion

        #region Convert HeDM Version 8



        /// <summary>
        /// Convert a Section entity into the HeDM sections format DTO
        /// </summary>
        /// <param name="entity">A Section entity</param>
        /// <returns>A HeDM-version 4 Section DTO</returns>
        private async Task<Dtos.Section4> ConvertSectionEntityToDto4Async(Domain.Student.Entities.Section entity)
        {
            if (entity == null)
            {
                return null;
            }

            var sectionDto = new Dtos.Section4();

            sectionDto.Id = entity.Guid.ToLowerInvariant();
            sectionDto.Code = entity.Name;
            sectionDto.Number = entity.Number;
            sectionDto.Title = entity.Title;
            sectionDto.StartOn = entity.StartDate;
            sectionDto.EndOn = entity.EndDate;

            if (!string.IsNullOrEmpty(entity.TermId))
            {
                sectionDto.AcademicPeriod = new Dtos.GuidObject2();
                var academicPeriod = academicPeriods.FirstOrDefault(a => a.Code == entity.TermId);
                sectionDto.AcademicPeriod.Id = (academicPeriod != null) ? academicPeriod.Guid : null;
            }
            if (string.IsNullOrEmpty(entity.LearningProvider))
            {
                if (!string.IsNullOrEmpty(entity.DefaultLearningProvider))
                {
                    entity.LearningProvider = entity.DefaultLearningProvider;
                }
            }
            if (!string.IsNullOrEmpty(entity.LearningProvider))
            {
                var instructionalPlatform = (await InstructionalPlatformsAsync()).FirstOrDefault(i => i.Code == entity.LearningProvider);
                if (instructionalPlatform != null && !string.IsNullOrEmpty(instructionalPlatform.Guid))
                {
                    sectionDto.InstructionalPlatform = new Dtos.GuidObject2(instructionalPlatform.Guid);
                }
            }
            sectionDto.Course = new Dtos.GuidObject2(await _courseRepository.GetCourseGuidFromIdAsync(entity.CourseId));

            var credit = new Dtos.DtoProperties.SectionCreditDtoProperty();
            var ceuCredit = new Dtos.DtoProperties.SectionCreditDtoProperty();
            credit.CreditCategory = new CreditIdAndTypeProperty2();
            ceuCredit.CreditCategory = new CreditIdAndTypeProperty2();

            var creditTypeItems = await CreditTypesAsync();

            if (creditTypeItems.Any(ct => ct.Code == entity.CreditTypeCode))
            {
                var creditTypeItem = creditTypeItems.Where(ct => ct.Code == entity.CreditTypeCode).First();
                if (creditTypeItem != null)
                {
                    if (!string.IsNullOrEmpty(creditTypeItem.Guid))
                    {
                        credit.CreditCategory.Detail = new GuidObject2() { Id = creditTypeItem.Guid };
                        ceuCredit.CreditCategory.Detail = new GuidObject2() { Id = creditTypeItem.Guid };
                    }
                    switch (creditTypeItem.CreditType)
                    {
                        case CreditType.ContinuingEducation:
                            credit.CreditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
                            break;
                        case CreditType.Institutional:
                            credit.CreditCategory.CreditType = CreditCategoryType3.Institutional;
                            break;
                        case CreditType.Transfer:
                            credit.CreditCategory.CreditType = CreditCategoryType3.Transfer;
                            break;
                        case CreditType.Exchange:
                            credit.CreditCategory.CreditType = CreditCategoryType3.Exchange;
                            break;
                        case CreditType.Other:
                            credit.CreditCategory.CreditType = CreditCategoryType3.Other;
                            break;
                        case CreditType.None:
                            credit.CreditCategory.CreditType = CreditCategoryType3.NoCredit;
                            break;
                        default:
                            credit.CreditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
                            break;
                    }
                }
            }
            else
            {
                //default to CE
                credit.CreditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
            }

            ceuCredit.CreditCategory.CreditType = credit.CreditCategory.CreditType;


            if (entity.Ceus.HasValue)
            {
                ceuCredit.Measure = Dtos.CreditMeasure2.CEU;
                ceuCredit.Minimum = entity.Ceus.Value;
                sectionDto.Credits.Add(ceuCredit);
            }
            if (entity.MinimumCredits.HasValue)
            {
                credit.Measure = Dtos.CreditMeasure2.Credit;
                credit.Minimum = entity.MinimumCredits.GetValueOrDefault();
                credit.Maximum = entity.MaximumCredits;
                credit.Increment = entity.VariableCreditIncrement;
                sectionDto.Credits.Add(credit);
            }


            if (!string.IsNullOrEmpty(entity.Location))
            {
                sectionDto.Site = new Dtos.GuidObject2(ConvertCodeToGuid(locations, entity.Location));
            }
            sectionDto.AcademicLevels.Add(new Dtos.GuidObject2(ConvertCodeToGuid((await AcademicLevelsAsync()), entity.AcademicLevelCode)));
            var gradeScheme = (await GradeSchemesAsync()).Where(gs => gs != null && gs.Code == entity.GradeSchemeCode).FirstOrDefault();
            var gradeSchemeGuid = gradeScheme != null ? gradeScheme.Guid : null;
            sectionDto.GradeSchemes.Add(new Dtos.GuidObject2(gradeSchemeGuid));
            foreach (var code in entity.CourseLevelCodes)
            {
                sectionDto.CourseLevels.Add(new Dtos.GuidObject2(ConvertCodeToGuid(await CourseLevelsAsync(), code)));
            }
            string status;
            sectionDto.Status = ConvertSectionStatusToDto2(entity.Statuses.ElementAt(0).IntegrationStatus, out status);

            // Use section capacity here.  Use the section-crosslists endpoint to find global capacity
            sectionDto.MaximumEnrollment = entity.SectionCapacity;

            // Determine the Department information for the course
            sectionDto.OwningInstitutionUnits = new List<Ellucian.Colleague.Dtos.OwningInstitutionUnit>();
            var departments = new List<Ellucian.Colleague.Dtos.OwningInstitutionUnit>();

            if (entity.Departments != null && entity.Departments.Any())
            {
                foreach (var offeringDept in entity.Departments)
                {
                    var academicDepartment = (await AllDepartments()).FirstOrDefault(d => d.Code == offeringDept.AcademicDepartmentCode);
                    if (academicDepartment != null)
                    {
                        var department = new Ellucian.Colleague.Dtos.OwningInstitutionUnit();
                        department.InstitutionUnit.Id = academicDepartment.Guid;
                        department.OwnershipPercentage = offeringDept.ResponsibilityPercentage;
                        departments.Add(department);
                    }
                }
                sectionDto.OwningInstitutionUnits = departments;

            }

            if (entity.NumberOfWeeks.HasValue)
            {
                sectionDto.Duration = new SectionDuration2()
                {
                    Length = entity.NumberOfWeeks.Value,
                    Unit = DurationUnit2.Weeks
                };
            }


            if (entity.CensusDates != null && entity.CensusDates.Count() > 0)
            {
                sectionDto.CensusDates = entity.CensusDates;
            }
            else if (!String.IsNullOrEmpty(entity.TermId))
            {
                // If the section has a term, and if there are no census dates in the section, for EEDM output we will check
                // TERMS.LOCATIONS for census dates.  If none there, we will check TERMS.  Most of this logic is already in the
                // Terms repository, and the results are in the regstration date overrides for the term and keyed by location.


                var secTerm = entity.TermId;
                var secLoc = entity.Location;
                sectionDto.CensusDates = new List<DateTime?>(); //default

                Domain.Student.Entities.Term term = (await AllTermsAsync()).FirstOrDefault(t => t.Code == secTerm);
                var termOverrides = term.RegistrationDates.Where(trd => trd.Location == "");
                if (!string.IsNullOrWhiteSpace(secLoc))
                {
                    var termLocOverrides = term.RegistrationDates.Where(trd => trd.Location == secLoc);
                    if (termLocOverrides != null && termLocOverrides.Count() > 0)
                    {
                        if (termLocOverrides.First().CensusDates != null && termLocOverrides.First().CensusDates.Count() > 0)
                        {
                            sectionDto.CensusDates = termLocOverrides.First().CensusDates;
                        }
                    }
                }
                if (sectionDto.CensusDates == null || sectionDto.CensusDates.Count() <= 0)
                {
                    if (termOverrides != null && termOverrides.Count() > 0 && termOverrides.First().CensusDates.Count() > 0)
                    {
                        if (termOverrides.First().CensusDates != null)
                        {
                            sectionDto.CensusDates = termOverrides.First().CensusDates;
                        }
                    }
                }

            }

            sectionDto.Billing = entity.BillingCred;
            return sectionDto;
        }

        /// <summary>
        /// Convert the HeDM sections format DTO into a Section entity
        /// </summary>
        /// <param name="sectionDto">A Data Model version 8 Section DTO</param>
        /// <returns>A Section entity</returns>
        private async Task<Domain.Student.Entities.Section> ConvertSectionDto4ToEntityAsync(Dtos.Section4 sectionDto)
        {

            if (sectionDto == null)
            {
                throw new ArgumentNullException("sectionDto", "Section DTO must be provided.");
            }
            if (string.IsNullOrEmpty(sectionDto.Id))
            {
                throw new ArgumentException("Section GUID not specified.");
            }

            if (sectionDto.Course == null)
            {
                throw new ArgumentNullException("course", "course is required.");
            }

            if (string.IsNullOrEmpty(sectionDto.Title))
            {
                throw new ArgumentNullException("title", "title is required.");
            }

            if (sectionDto.StartOn == null)
            {
                throw new ArgumentNullException("startOn", "startOn is required.");
            }

            if (sectionDto.StartOn.HasValue && sectionDto.EndOn.HasValue)
            {
                if (sectionDto.StartOn.Value > sectionDto.EndOn.Value)
                {
                    throw new ArgumentException("endOn can not occur earlier than startOn.", "endOn");
                }
            }

            if (sectionDto.Billing.HasValue)
            {
                if (sectionDto.Billing < 0)
                {
                    throw new ArgumentException("section.Billing", "Billing can not be a negative number");
                }
            }
            if (sectionDto.Credits.Any())
            {
                foreach (var credit in sectionDto.Credits)
                {
                    if (credit.CreditCategory == null)
                    {
                        throw new ArgumentException(
                            "Section provided is not valid, Credits must contain a Credit Category.", "credit category");
                    }

                    if (credit.Measure == null)
                    {
                        throw new ArgumentException("Section provided is not valid, Credits must contain a Measure.",
                            "credit measure");
                    }

                    if (credit.Minimum == null)
                    {
                        throw new ArgumentException("Section provided is not valid, Credits must contain a Minimum.",
                            "credit minimum");
                    }

                    if (credit.CreditCategory.CreditType == null)
                    {
                        throw new ArgumentException("Credit Type is required if for Credit Categories if Credits are in the message body.");
                    }

                    if (credit.CreditCategory.Detail != null && string.IsNullOrEmpty(credit.CreditCategory.Detail.Id))
                    {
                        throw new ArgumentException("Credit Category id is required within the detail section of Credit Category if it is in the message body.");
                    }
                    if (credit.Increment == null && credit.Maximum != null)
                    {
                        throw new ArgumentException("Credit Increment is required when Credit Maximum exists.");
                    }
                    if (credit.Maximum == null && credit.Increment != null)
                    {
                        throw new ArgumentException("Credit Maximum is required when Credit Increment exists.");
                    }
                    if (credit.Maximum != null && credit.Measure == CreditMeasure2.CEU)
                    {
                        throw new ArgumentException("Credit Maximum cannot exist when Credit Measure is 'ceu'.");
                    }
                    if (credit.Increment != null && credit.Measure == CreditMeasure2.CEU)
                    {
                        throw new ArgumentException("Credit Increment cannot exist when Credit Measure is 'ceu'.");
                    }
                }

            }

            if (sectionDto.CourseLevels != null && sectionDto.CourseLevels.Any())
            {
                foreach (var level in sectionDto.CourseLevels)
                {
                    if (string.IsNullOrEmpty(level.Id))
                    {
                        throw new ArgumentException(
                            "Course Level id is a required field when Course Levels are in the message body.");
                    }
                }
            }

            if (sectionDto.InstructionalPlatform != null)
            {
                if (string.IsNullOrEmpty(sectionDto.InstructionalPlatform.Id))
                {
                    throw new ArgumentException(
                        "Instructional Platform id is a required field when Instructional Methods are in the message body.");
                }
            }

            if (sectionDto.AcademicLevels != null && sectionDto.AcademicLevels.Any())
            {
                foreach (var level in sectionDto.AcademicLevels)
                {
                    if (string.IsNullOrEmpty(level.Id))
                    {
                        throw new ArgumentException(
                            "Academic Level id is a required field when Academic Levels are in the message body.");
                    }
                }
            }

            if (sectionDto.GradeSchemes != null && sectionDto.GradeSchemes.Any())
            {
                foreach (var scheme in sectionDto.GradeSchemes)
                {
                    if (string.IsNullOrEmpty(scheme.Id))
                    {
                        throw new ArgumentException(
                            "Grade Scheme id is a required field when Grade Schemes are in the message body.");
                    }
                }
            }

            if (sectionDto.OwningInstitutionUnits != null && sectionDto.OwningInstitutionUnits.Any())
            {
                foreach (var org in sectionDto.OwningInstitutionUnits)
                {
                    if (org.InstitutionUnit == null || string.IsNullOrEmpty(org.InstitutionUnit.Id))
                    {
                        throw new ArgumentException("Institution Unit id is a required field when Owning Organizations are in the message body.");
                    }
                }
            }

            if (sectionDto.Credits != null && sectionDto.Credits.Any())
            {
                foreach (var credit in sectionDto.Credits)
                {
                    if (credit.CreditCategory == null)
                    {
                        throw new ArgumentException("Credit Category is required if Credits are in the message body.");
                    }

                    if (credit.CreditCategory.CreditType == null)
                    {
                        throw new ArgumentException("Credit Type is required for Credit Categories if Credits are in the message body.");
                    }
                }
            }

            if (sectionDto.Duration != null && (sectionDto.Duration.Unit == DurationUnit2.Months || sectionDto.Duration.Unit == DurationUnit2.Years || sectionDto.Duration.Unit == DurationUnit2.Days))
            {
                throw new ArgumentException("Section Duration Unit is not allowed to be set to Days, Months or Years");
            }

            if (sectionDto.Duration != null && sectionDto.Duration.Unit == DurationUnit2.Weeks && sectionDto.Duration.Length < 0)
            {
                throw new ArgumentException("Section Duration Length must be a positive number.");
            }

            if (sectionDto.Duration != null && sectionDto.Duration.Unit == null)
            {
                throw new ArgumentException("Section Duration Unit must contain a value if the unit property is specified.");
            }

            if (sectionDto.Credits.Count() > 2)
            {
                throw new ArgumentException("A maximum of 2 entries are allowed in the Credits array.");
            }
            if (sectionDto.Credits.Count() == 2)
            {
                if (sectionDto.Credits.ElementAt(0).CreditCategory.CreditType != sectionDto.Credits.ElementAt(1).CreditCategory.CreditType)
                {
                    throw new ArgumentException("The same Credit Type must be used for each entry in the Credits array.");
                }
                if (!(sectionDto.Credits.ElementAt(0).Measure == CreditMeasure2.CEU && sectionDto.Credits.ElementAt(1).Measure == CreditMeasure2.Credit)
                    && !(sectionDto.Credits.ElementAt(0).Measure == CreditMeasure2.Credit && sectionDto.Credits.ElementAt(1).Measure == CreditMeasure2.CEU)
                    && !(sectionDto.Credits.ElementAt(0).Measure == CreditMeasure2.CEU && sectionDto.Credits.ElementAt(1).Measure == CreditMeasure2.Hours)
                    && !(sectionDto.Credits.ElementAt(0).Measure == CreditMeasure2.Hours && sectionDto.Credits.ElementAt(1).Measure == CreditMeasure2.CEU))
                {
                    throw new ArgumentException("Invalid combination of measures '" + sectionDto.Credits.ElementAt(0).Measure
                        + "' and '" + sectionDto.Credits.ElementAt(1).Measure + "'");
                }
            }


            Domain.Student.Entities.Section section = null;

            try
            {
                section = await _sectionRepository.GetSectionByGuidAsync(sectionDto.Id);
            }
            catch (Exception)
            {
            }

            string id = section == null ? null : section.Id;
            var currentStatuses = section == null || section.Statuses == null
                ? new List<SectionStatusItem>()
                : section.Statuses.ToList();
            var curriculumConfiguration = await GetCurriculumConfiguration2Async();
            var statuses = await UpdateSectionStatus3(currentStatuses, sectionDto.Status);
            var course = await _courseRepository.GetCourseByGuidAsync(sectionDto.Course.Id);

            // Set the credit type and credits/CEUs for the course based on the supplied GUID, or using the ERP default 
            Ellucian.Colleague.Domain.Student.Entities.CreditCategory creditCategory = null;
            if (sectionDto.Credits != null &&
                sectionDto.Credits.Any() &&
                sectionDto.Credits.ToList()[0].CreditCategory != null &&
                sectionDto.Credits.ToList()[0].CreditCategory.Detail != null &&
                !string.IsNullOrEmpty(sectionDto.Credits.ToList()[0].CreditCategory.Detail.Id))
            {
                creditCategory = (await CreditTypesAsync()).FirstOrDefault(ct => ct.Guid == sectionDto.Credits.ToList()[0].CreditCategory.Detail.Id);
                if (creditCategory == null)
                {
                    throw new ArgumentException("Invalid Id '" + sectionDto.Credits.ToList()[0].CreditCategory.Detail.Id + "' supplied for creditCategory");
                }
            }
            // If we don't have a GUID then check for a CreditType enumeration value
            if (creditCategory == null &&
                sectionDto.Credits != null &&
                sectionDto.Credits.Any() &&
                sectionDto.Credits.ToList()[0].CreditCategory != null &&
                sectionDto.Credits.ToList()[0].CreditCategory.CreditType != null)
            {
                if (sectionDto.Credits.ToList()[0].CreditCategory.CreditType == CreditCategoryType3.Exam ||
                    sectionDto.Credits.ToList()[0].CreditCategory.CreditType == CreditCategoryType3.WorkLifeExperience)
                {
                    throw new InvalidOperationException("Credit category type 'exam' or 'workLifeExperience' are not supported.");
                }

                // Find the credit category that matches the enumeration
                switch (sectionDto.Credits.ToList()[0].CreditCategory.CreditType)
                {
                    case (CreditCategoryType3.ContinuingEducation):
                        creditCategory = (await CreditTypesAsync()).FirstOrDefault(cc => cc.CreditType == CreditType.ContinuingEducation);
                        break;
                    case (CreditCategoryType3.Institutional):
                        creditCategory = (await CreditTypesAsync()).FirstOrDefault(cc => cc.CreditType == CreditType.Institutional);
                        break;
                    case (CreditCategoryType3.Transfer):
                        creditCategory = (await CreditTypesAsync()).FirstOrDefault(cc => cc.CreditType == CreditType.Transfer);
                        break;
                    case (CreditCategoryType3.Exchange):
                        creditCategory = (await CreditTypesAsync()).FirstOrDefault(cc => cc.CreditType == CreditType.Exchange);
                        break;
                    case (CreditCategoryType3.Other):
                        creditCategory = (await CreditTypesAsync()).FirstOrDefault(cc => cc.CreditType == CreditType.Other);
                        break;
                    case (CreditCategoryType3.NoCredit):
                        creditCategory = (await CreditTypesAsync()).FirstOrDefault(cc => cc.CreditType == CreditType.None);
                        break;
                }
            }

            //If creditCategory is null then throw an exception
            if (creditCategory == null)
            {
                throw new ArgumentException("Credits data requires Credit Category Type or Id");
            }

            var creditTypeCode = creditCategory == null ? string.Empty : creditCategory.Code;

            decimal? minCredits;
            decimal? ceus;
            decimal? maxCredits;
            decimal? varIncrCredits;
            minCredits = null;
            maxCredits = null;
            varIncrCredits = null;
            ceus = null;
            foreach (var credits in sectionDto.Credits)
            {
                var creditInfo = (sectionDto.Credits == null || sectionDto.Credits.Count == 0) ? null : credits;
                var measure = creditInfo == null ? null : creditInfo.Measure;
                if (measure == Dtos.CreditMeasure2.CEU)
                {
                    ceus = creditInfo == null ? 0 : creditInfo.Minimum;
                }
                else
                {
                    minCredits = creditInfo == null ? 0 : creditInfo.Minimum;
                    maxCredits = creditInfo == null ? null : creditInfo.Maximum;
                    varIncrCredits = creditInfo == null ? null : creditInfo.Increment;
                }
            }

            if (sectionDto.Credits == null || !sectionDto.Credits.Any())
            {
                if (course != null)
                {
                    minCredits = course.MinimumCredits;
                    ceus = course.Ceus;
                    creditTypeCode = course.LocalCreditType;
                }
            }

            // Set the list of departments/shares
            List<OfferingDepartment> offeringDepartments = new List<OfferingDepartment>();

            // If we have supplied owning organization, then use that first.
            if (sectionDto.OwningInstitutionUnits != null && sectionDto.OwningInstitutionUnits.Any())
            {
                foreach (var owningInstitutionUnit in sectionDto.OwningInstitutionUnits)
                {

                    var division = (await _referenceDataRepository.GetDivisionsAsync(true))
                                            .FirstOrDefault(div => div.Guid.Equals(owningInstitutionUnit.InstitutionUnit.Id, StringComparison.OrdinalIgnoreCase));
                    if (division != null)
                    {
                        throw new InvalidOperationException("Owning institution unit of type 'division' is not supported.");
                    }

                    var school = (await _referenceDataRepository.GetSchoolsAsync(true))
                                            .FirstOrDefault(div => div.Guid.Equals(owningInstitutionUnit.InstitutionUnit.Id, StringComparison.OrdinalIgnoreCase));
                    if (school != null)
                    {
                        throw new InvalidOperationException("Owning institution unit of type 'school' is not supported.");
                    }

                    var department = (await DepartmentsAsync()).Where(d => d.Guid == owningInstitutionUnit.InstitutionUnit.Id).FirstOrDefault();
                    var academicDepartment = department != null ? (await _studentReferenceDataRepository.GetAcademicDepartmentsAsync()).FirstOrDefault(ad => ad.Code == department.Code) : null;
                    if (academicDepartment != null)
                    {
                        offeringDepartments.Add(new OfferingDepartment(academicDepartment.Code, owningInstitutionUnit.OwnershipPercentage));
                    }
                }
            }
            // If we don't have offering departments in the payload and didn't find
            // any departments, we will default from the course.  This has to be done
            // here instead of the Colleague transaction because the Sections Entity
            // requires at least one department.
            if (offeringDepartments.Count == 0)
            {
                offeringDepartments.AddRange(course.Departments);
            }

            var courseLevels = await CourseLevelsAsync();
            // Convert various codes to their Colleague values or get them from the course
            List<string> courseLevelCodes = new List<string>();
            if (sectionDto.CourseLevels == null || sectionDto.CourseLevels.Count == 0)
            {
                courseLevelCodes = course.CourseLevelCodes;
            }
            else
            {
                foreach (var level in sectionDto.CourseLevels)
                {
                    var courseLevelCode = ConvertGuidToCode(courseLevels, level.Id);
                    if (string.IsNullOrEmpty(courseLevelCode))
                    {
                        throw new ArgumentException(string.Concat("Invalid Id '", level.Id, "' supplied for courseLevels"));
                    }
                    else
                    {
                        courseLevelCodes.Add(courseLevelCode);
                    }
                }
            }

            string academicLevel = course.AcademicLevelCode;
            if (sectionDto.AcademicLevels != null && sectionDto.AcademicLevels.Any())
            {
                var tempAcadCode = ConvertGuidToCode((await AcademicLevelsAsync()), sectionDto.AcademicLevels[0].Id);
                if (!string.IsNullOrEmpty(tempAcadCode))
                {
                    academicLevel = tempAcadCode;
                }
            }

            string gradeScheme = null;
            if (sectionDto.GradeSchemes == null || sectionDto.GradeSchemes.Count == 0)
            {
                gradeScheme = course.GradeSchemeCode;
            }
            else
            {
                var gradeSchemeEntity = (await GradeSchemesAsync()).Where(gs => gs != null && gs.Guid == sectionDto.GradeSchemes[0].Id).FirstOrDefault();
                var schemeCode = gradeSchemeEntity != null ? gradeSchemeEntity.Code : null;
                if (string.IsNullOrEmpty(schemeCode))
                {
                    throw new ArgumentException(string.Concat("Invalid Id '", sectionDto.GradeSchemes[0].Id, "' supplied for gradeSchemes"));
                }
                else
                {
                    gradeScheme = schemeCode;
                }
            }

            string site = sectionDto.Site == null ? null : ConvertGuidToCode(locations, sectionDto.Site.Id);

            string learningProvider = null;
            if (sectionDto.InstructionalPlatform != null)
            {
                var providerCode = ConvertGuidToCode((await InstructionalPlatformsAsync()), sectionDto.InstructionalPlatform.Id);
                if (string.IsNullOrEmpty(providerCode))
                {
                    throw new ArgumentException(string.Concat("Invalid Id '", sectionDto.InstructionalPlatform.Id, "' supplied for instructionalMethods"));
                }
                else
                {
                    learningProvider = providerCode;
                }
            }

            // string term = sectionDto.AcademicPeriod == null ? null : ConvertGuidToCode(academicPeriods, sectionDto.AcademicPeriod.Id);
            string term = string.Empty;
            if (sectionDto.AcademicPeriod != null && !string.IsNullOrEmpty(sectionDto.AcademicPeriod.Id))
            {
                term = ConvertGuidToCode(academicPeriods, sectionDto.AcademicPeriod.Id);
                if (string.IsNullOrEmpty(term))
                {
                    throw new ArgumentException(string.Format("Invalid Id '{0}' supplied for academicPeriod.", sectionDto.AcademicPeriod.Id));
                }
            }


            // Because census dates can be defaulted in from TERMS or TERMS.LOCATIONS when the consumer GETS them,
            // if the consumer PUTs or POSTs a section with census dates, we have to make sure they are intended to
            // be overridden at the section level, rather than blindly writing inherited defaults back to the section.

            if (sectionDto.CensusDates != null && sectionDto.CensusDates.Count() > 0)
            {

                Domain.Student.Entities.Term secTerm = null;
                List<DateTime?> secTermOverrides = null;
                List<DateTime?> secTermLocOverrides = null;

                // Check for term Census dates overrides

                if (term != null)
                {
                    var secTerms = (await AllTermsAsync()).Where(t => t.Code == term);
                    if (secTerms != null && secTerms.Count() == 1)
                    {
                        secTerm = secTerms.First();
                        if (secTerm.RegistrationDates != null && secTerm.RegistrationDates.Where(srd => srd.Location == null).Count() == 1)
                        {
                            secTermOverrides = secTerm.RegistrationDates.First(srd => string.IsNullOrEmpty(srd.Location)).CensusDates;
                        }
                    }

                }

                // If we have a term, now check for a location to see if it has its own term-location overrides

                String secLocCode = null;

                if (term != null && site != null && secTerm != null)
                {
                    var key = term + "*" + site;
                    var termLocRegDates = secTerm.RegistrationDates.Where(trd => trd.Location == secLocCode);
                    if (termLocRegDates != null && termLocRegDates.Count() > 0)
                    {
                        secTermLocOverrides = termLocRegDates.First().CensusDates;
                    }
                }

                // Now compare the lists of dates (if any) and decide if the incoming dates match
                // existing overrides - which indicates they probably were defaulted in on the GET, 
                // and should not be written to the section record on the PUT.

                if (secTermLocOverrides != null)
                {
                    var secOverCount = sectionDto.CensusDates.Count();
                    var secTermLocOverCount = secTermLocOverrides.Count();
                    if (secOverCount == secTermLocOverCount && sectionDto.CensusDates.Intersect(secTermLocOverrides).Count() == secOverCount)
                    {
                        // incoming census dates match the term-location overrides, do not update the section overrides.
                        sectionDto.CensusDates = null;
                    }
                }
                if (sectionDto.CensusDates != null && secTermOverrides != null)
                {
                    var secOverCount = sectionDto.CensusDates.Count();
                    var secTermOverCount = secTermOverrides.Count();
                    if (secOverCount == secTermOverCount && sectionDto.CensusDates.Intersect(secTermOverrides).Count() == secOverCount)
                    {
                        // incoming census dates match the term overrides, do not update the section overrides.
                        sectionDto.CensusDates = null;
                    }
                }
            }

            // Create the section entity
            var entity = new Domain.Student.Entities.Section(id, course.Id, sectionDto.Number, sectionDto.StartOn.GetValueOrDefault().DateTime,
                minCredits, ceus, sectionDto.Title, creditTypeCode, offeringDepartments, courseLevelCodes, academicLevel, statuses,
                course.AllowPassNoPass, course.AllowAudit, course.OnlyPassNoPass, course.AllowWaitlist, false)
            {
                Guid = (sectionDto.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase)) ? string.Empty : sectionDto.Id.ToLowerInvariant(),
                EndDate = sectionDto.EndOn == null ? default(DateTime?) : sectionDto.EndOn.Value.Date,
                MaximumCredits = maxCredits,
                VariableCreditIncrement = varIncrCredits,
                GradeSchemeCode = gradeScheme,
                TopicCode = course.TopicCode,
                GlobalCapacity = sectionDto.MaximumEnrollment,
                SectionCapacity = sectionDto.MaximumEnrollment,
                Name = sectionDto.Code,
                LearningProvider = learningProvider,
                TermId = term,
                Location = site,
                CensusDates = sectionDto.CensusDates
            };
            entity.BillingCred = sectionDto.Billing;

            foreach (var item in course.Types)
            {
                entity.AddCourseType(item);
            }

            if (string.IsNullOrEmpty(entity.TermId))
            {
                entity.TermId = SectionProcessor.DetermineTerm(entity, await AllTermsAsync());
            }

            //check if duration is set on the incoming DTO
            if (sectionDto.Duration != null)
            {
                entity.NumberOfWeeks = sectionDto.Duration.Length;
            }
            else
            {
                if (entity.EndDate.HasValue)
                {
                    entity.NumberOfWeeks = (int)Math.Ceiling((entity.EndDate.Value - entity.StartDate).Days / 7m);
                }
            }

            return entity;
        }

        private async Task<Dictionary<string, string>> ValidateAndConvertFilterArguments(string startOn,
            string endOn, string instructionalPlatform, string academicPeriod, string course,
            string site, string status, string instructor, string academicLevel = "", string owningOrganization = "", string subject = "", string reportingAcademicPeriod = "", string scheduleAcademicPeriod = "")
        {
            // Convert and validate all input parameters
            var newStartOn = (startOn == string.Empty ? string.Empty : await ConvertDateArgument(startOn));
            var newEndOn = (endOn == string.Empty ? string.Empty : await ConvertDateArgument(endOn));
            var newInstructionalPlatform = (instructionalPlatform == string.Empty ? string.Empty : ConvertGuidToCodeNoFail((await InstructionalPlatformsAsync()), instructionalPlatform));
            if (!string.IsNullOrEmpty(instructionalPlatform) && string.IsNullOrEmpty(newInstructionalPlatform))
            {
                string errorMessage = string.Format("instructional platform guid {0} is invalid.", instructionalPlatform);
                logger.Error(errorMessage);
                throw new ArgumentException();
            }
            var newAcademicPeriod = (academicPeriod == string.Empty ? string.Empty : ConvertGuidToCodeNoFail(academicPeriods, academicPeriod));
            if (!string.IsNullOrEmpty(academicPeriod) && string.IsNullOrEmpty(newAcademicPeriod))
            {
                string errorMessage = string.Format("academic period guid {0} is invalid.", academicPeriod);
                logger.Error(errorMessage);
                throw new ArgumentException();
            }
            var newReportingAcademicPeriod = (reportingAcademicPeriod == string.Empty ? string.Empty : ConvertGuidToCodeNoFail(academicPeriods, reportingAcademicPeriod));
            if (!string.IsNullOrEmpty(reportingAcademicPeriod) && string.IsNullOrEmpty(newReportingAcademicPeriod))
            {
                string errorMessage = string.Format("reporting academic period guid {0} is invalid.", reportingAcademicPeriod);
                logger.Error(errorMessage);
                throw new ArgumentException();
            }
            var newScheduleAcademicPeriod = (scheduleAcademicPeriod == string.Empty ? string.Empty : ConvertGuidToCodeNoFail(academicPeriods, scheduleAcademicPeriod));
            if (!string.IsNullOrEmpty(scheduleAcademicPeriod) && string.IsNullOrEmpty(newScheduleAcademicPeriod))
            {
                string errorMessage = string.Format("schedule academic period guid {0} is invalid.", scheduleAcademicPeriod);
                logger.Error(errorMessage);
                throw new ArgumentException();
            }

            var newCourse = string.Empty;
            try
            {
                newCourse = (course == string.Empty ? string.Empty : await ConvertCourseArgument(course));
                if (!string.IsNullOrEmpty(course) && string.IsNullOrEmpty(newCourse))
                {
                    string errorMessage = string.Format("course guid {0} is invalid.", course);
                    logger.Error(errorMessage);
                    throw new ArgumentException();
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw new ArgumentException(e.Message);
            }

            var newSite = (site == string.Empty ? string.Empty : ConvertGuidToCodeNoFail(locations, site));
            if (!string.IsNullOrEmpty(site) && string.IsNullOrEmpty(newSite))
                throw new ArgumentException(string.Format("site guid {0} is invalid.", site));

            var newStatus = string.Empty;
            try
            {
                newStatus = (status == string.Empty ? string.Empty : await ConvertStatusArgumentNoDefault(status));
                if (!string.IsNullOrEmpty(status) && string.IsNullOrEmpty(newStatus))
                {
                    string errorMessage = string.Format("status {0} is invalid.", status);
                    logger.Error(errorMessage);
                    throw new ArgumentException();
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }

            var instructorId = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(instructor))
                {
                    instructorId = await _personRepository.GetPersonIdFromGuidAsync(instructor);
                    if (string.IsNullOrEmpty(instructorId))
                    {
                        string errorMessage = string.Format("Instructor guid {0} is invalid.", instructor);
                        logger.Error(errorMessage);
                        throw new ArgumentException();
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw new ArgumentException(e.Message);
            }

            var newAcademicLevel = string.Empty;
            if (!(string.IsNullOrEmpty(academicLevel)))
            {
                newAcademicLevel = (academicLevel == string.Empty ? string.Empty : ConvertGuidToCodeNoFail((await AcademicLevelsAsync()), academicLevel));
                if (!string.IsNullOrEmpty(academicLevel) && string.IsNullOrEmpty(newAcademicLevel))
                {
                    string errorMessage = string.Format("academic level guid {0} is invalid.", academicLevel);
                    logger.Error(errorMessage);
                    throw new ArgumentException();
                }
            }


            var newOwningOrganization = string.Empty;
            if (!(string.IsNullOrEmpty(owningOrganization)))
            {
                newOwningOrganization = ConvertGuidToCodeNoFail((await DepartmentsAsync()), owningOrganization);
                if (string.IsNullOrEmpty(newOwningOrganization))
                {
                    string errorMessage = string.Format("owningOrganization guid {0} is invalid.", owningOrganization);
                    logger.Error(errorMessage);
                    throw new ArgumentException();
                }
            }

            var newSubject = subject;
            Guid newGuid;
            // if the subject is a guid, then lookup the code, otherwise return the original string
            if (Guid.TryParse(subject, out newGuid))
            {

                var subjectCode = ConvertGuidToCodeNoFail(await _studentReferenceDataRepository.GetSubjectsAsync(), subject); ;
                if (!string.IsNullOrEmpty(course) && string.IsNullOrEmpty(newCourse))
                {
                    string errorMessage = string.Format("subject guid {0} is invalid.", subject);
                    logger.Error(errorMessage);
                    throw new ArgumentException();
                }
                newSubject = subjectCode;
            }

            return new Dictionary<string, string>()
            {
                {"newStartOn",newStartOn},
                {"newEndOn", newEndOn},
                {"newInstructionalPlatform",newInstructionalPlatform},
                {"newAcademicPeriod",newAcademicPeriod},
                {"newScheduleAcademicPeriod",newScheduleAcademicPeriod},
                {"newReportingAcademicPeriod",newReportingAcademicPeriod},
                {"newCourse",newCourse},
                {"newSite",newSite},
                {"newStatus",newStatus},
                {"instructorId", instructorId},
                {"newOwningOrganization", newOwningOrganization },
                {"newAcademicLevel", newAcademicLevel },
                {"newSubject", newSubject }
            };
        }

        private async Task<Dictionary<string, List<string>>> ValidateAndConvertFilterArrayArguments(List<string> academicLevels, List<string> owningOrganizations, List<string> instructors = null)
        {
            var retval = new Dictionary<string, List<string>>();

            var newAcademicLevels = new List<string>();
            if (academicLevels != null && academicLevels.Any())
            {
                foreach (var academicLevel in academicLevels)
                {
                    var newAcademicLevel = (academicLevel == string.Empty ? string.Empty : ConvertGuidToCodeNoFail((await AcademicLevelsAsync()), academicLevel));
                    if (!string.IsNullOrEmpty(academicLevel) && string.IsNullOrEmpty(newAcademicLevel))
                    {
                        string errorMessage = string.Format("academic level guid {0} is invalid.", academicLevel);
                        logger.Error(errorMessage);
                        throw new ArgumentException();
                    }
                    if (!newAcademicLevels.Contains(newAcademicLevel))
                        newAcademicLevels.Add(newAcademicLevel);
                }
            }
            var newInstructors = new List<string>();
            if (instructors != null && instructors.Any())
            {
                foreach (var instructor in instructors)
                {
                    var newInstructor = (instructor == string.Empty ? string.Empty : await _personRepository.GetPersonIdFromGuidAsync(instructor));
                    if (!string.IsNullOrEmpty(instructor) && string.IsNullOrEmpty(newInstructor))
                    {
                        string errorMessage = string.Format("instructor guid {0} is invalid.", instructor);
                        logger.Error(errorMessage);
                        throw new ArgumentException();
                    }
                    if (!newInstructors.Contains(newInstructor))
                        newInstructors.Add(newInstructor);
                }
            }

            var newOwningOrganizations = new List<string>();
            if (owningOrganizations != null && owningOrganizations.Any())
            {
                foreach (var owningOrganization in owningOrganizations)
                {
                    var newOwningOrganization = ConvertGuidToCodeNoFail((await DepartmentsAsync()), owningOrganization);
                    if (string.IsNullOrEmpty(newOwningOrganization))
                    {
                        string errorMessage = string.Format("owningOrganization guid {0} is invalid.", owningOrganization);
                        logger.Error(errorMessage);
                        throw new ArgumentException();
                    }
                    if (!newOwningOrganizations.Contains(newOwningOrganization))
                        newOwningOrganizations.Add(newOwningOrganization);
                }
            }

            retval.Add("newAcademicLevels", newAcademicLevels);
            retval.Add("newOwningOrganizations", newOwningOrganizations);
            retval.Add("newInstructors", newInstructors);

            return retval;
        }

        #endregion

        #region EEDM Version 11

        /// <summary>
        /// Get selection criteria, convert it to codes as required and select 
        /// multiple sections from Colleague.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="title">Section Title Contains ...title...</param>
        /// <param name="startOn">Section starts on or after this date</param>
        /// <param name="endOn">Section ends on or before this date</param>
        /// <param name="code">Section Name Contains ...code...</param>
        /// <param name="number">Section Number equal to</param>
        /// <param name="instructionalPlatform">Learning Platform equal to (guid)</param>
        /// <param name="academicPeriod">Section Term equal to (guid)</param>
        /// <param name="academicLevel">Section Academic Level equal to (guid)</param>
        /// <param name="course">Section Course equal to (guid)</param>
        /// <param name="site">Section Location equal to (guid)</param>
        /// <param name="status">Section Status matches closed, open, pending, or cancelled</param>
        /// <param name="owningOrganization">Section Department equal to (guid)</param>
        /// <param name="subject">Section Title Contains ...subject...</param>
        /// <param name="instructor">Section Instructor equal to (guid)</param>
        /// <param name="search">Check if a section is searchable or hidden</param>
        /// <param name="keyword">Perform a keyword search</param>
        /// <returns>List of Section5 <see cref="Dtos.Section5"/> objects representing matching sections</returns>
        public async Task<Tuple<IEnumerable<Dtos.Section5>, int>> GetSections5Async(int offset, int limit, string title = "", string startOn = "", string endOn = "",
            string code = "", string number = "", string instructionalPlatform = "", string academicPeriod = "",
            List<string> academicLevel = null, string course = "", string site = "", string status = "", List<string> owningOrganization = null,
            string subject = "", string instructor = "", SectionsSearchable search = SectionsSearchable.NotSet, string keyword = "")
        {
            // Convert and validate all input parameters

            IDictionary<string, string> args = new Dictionary<string, string>();
            try
            {
                args = await ValidateAndConvertFilterArguments(startOn, endOn, instructionalPlatform, academicPeriod, course, site, status, instructor, subject: subject);
            }
            catch (ArgumentException)
            {
                // One or more of the arguments failed to match up to a guid.  Return empty set.
                return new Tuple<IEnumerable<Dtos.Section5>, int>(new List<Dtos.Section5>(), 0);

            }
            var arrayArgs = new Dictionary<string, List<string>>();
            try
            {
                arrayArgs = await ValidateAndConvertFilterArrayArguments(academicLevel, owningOrganization);
            }
            catch (ArgumentException)
            {
                // One or more of the arguments failed to match up to a guid.  Return empty set.
                return new Tuple<IEnumerable<Dtos.Section5>, int>(new List<Dtos.Section5>(), 0);

            }
            var newStartOn = args["newStartOn"];
            var newEndOn = args["newEndOn"];
            var newInstructionalPlatform = args["newInstructionalPlatform"];
            var newAcademicPeriod = args["newAcademicPeriod"];
            var instructorId = args["instructorId"];
            var newCourse = args["newCourse"];
            var newSite = args["newSite"];
            var newStatus = args["newStatus"];
            var newOwningOrganization = arrayArgs["newOwningOrganizations"];
            var newAcademicLevel = arrayArgs["newAcademicLevels"];
            var newSubject = args["newSubject"];

            var sectionDtos = new List<Dtos.Section5>();

            Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section>, int> sectionEntities = null;

            if (search != SectionsSearchable.NotSet)
            {
                sectionEntities = await _sectionRepository.GetSectionsSearchableAsync(offset, limit, search.ToString());
            }
            else if (!(string.IsNullOrEmpty(keyword)))
            {
                sectionEntities = await _sectionRepository.GetSectionsKeywordAsync(offset, limit, keyword);
            }
            else
            {
                sectionEntities = await _sectionRepository.GetSectionsAsync(offset, limit, title, newStartOn, newEndOn,
                    code, number, newInstructionalPlatform, newAcademicPeriod,
                    newAcademicLevel, newCourse, newSite, newStatus, newOwningOrganization, newSubject, instructorId);
            }
            foreach (var sectionEntity in sectionEntities.Item1)
            {
                if (sectionEntity.Guid != null)
                {
                    var sectionDto = await ConvertSectionEntityToDto5Async(sectionEntity, false);
                    sectionDtos.Add(sectionDto);
                }
            }
            return new Tuple<IEnumerable<Dtos.Section5>, int>(sectionDtos, sectionEntities.Item2);
        }

        /// <summary>
        /// Get a DTO for section5 for v11 sections by its GUID
        /// </summary>
        /// <param name="guid">GUID of section</param>
        /// <returns>The Data Model Section version 5 DTO</returns>
        public async Task<Dtos.Section5> GetSection5ByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get a section.");
            }
            var sectionEntity = await _sectionRepository.GetSectionByGuidAsync(guid);
            if (sectionEntity == null)
            {
                throw new KeyNotFoundException(string.Concat("Section not found or invalid for id '", guid, "'.  See Log for more information."));
            }
            var sectionDto = await ConvertSectionEntityToDto5Async(sectionEntity, false);
            return sectionDto;
        }

        /// <summary>
        /// Get a DTO for section5 for v11 sections by its GUID
        ///but exclude defaulted census dates
        /// </summary>
        /// <param name="guid">GUID of section</param>
        /// <returns>The Data Model Section version 5 DTO</returns>
        public async Task<Dtos.Section5> GetSection5ByGuidFilterCensusDatesAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get a section.");
            }
            var sectionEntity = await _sectionRepository.GetSectionByGuidAsync(guid);
            if (sectionEntity == null)
            {
                throw new KeyNotFoundException(string.Concat("Section not found or invalid for id '", guid, "'.  See Log for more information."));
            }
            var sectionDto = await ConvertSectionEntityToDto5Async(sectionEntity, true);
            return sectionDto;
        }

        /// <summary>
        /// Create a new section
        /// </summary>
        /// <param name="section">DTO containing the Data Model version 6 section to create</param>
        /// <returns>DTO containing the created Data Model version 6 section</returns>
        public async Task<Dtos.Section5> PostSection5Async(Dtos.Section5 section)
        {         
            if ((section != null) && (section.Status != null))
                await ValidateSectionStatusConfigurationAsync(section.Status.Category);

            _sectionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            // Make sure dept percentage is from 0-100%.  This is also checked in the contructor for the domain entity OfferingDepartment,
            // but we want to customize the error message for Ethos APIs to refernece "Ownership percentage".
            if (section != null)
            {
                if (section.OwningInstitutionUnits != null && section.OwningInstitutionUnits.Any())
                {
                    foreach (var owningInstitutionUnit in section.OwningInstitutionUnits)
                    {
                        if (owningInstitutionUnit != null)
                        {
                            if (owningInstitutionUnit.OwnershipPercentage < 0 || owningInstitutionUnit.OwnershipPercentage > 100)
                            {
                                throw new ArgumentOutOfRangeException("ownershipPercentage", "Ownership percentage must be between 0 and 100.");
                            }
                        }
                    }
                }
            }
            // Convert the CDM section into a domain entity and create it
            var entity = await ConvertSectionDto5ToEntityAsync(section);
            if (entity == null)
            {
                throw new KeyNotFoundException("Failed to create a new section. ");
            }
            var newEntity = await _sectionRepository.PostSectionAsync(entity);
            if (newEntity == null)
            {
                throw new KeyNotFoundException("Failed to return a valid section. ");
            }
            var newDto = await ConvertSectionEntityToDto5Async(newEntity, false);

            return newDto;
        }

        private async Task<Domain.Student.Entities.Section> ConvertSectionDto5ToEntityAsync(Section5 sectionDto)
        {

            if (sectionDto == null)
            {
                throw new ArgumentNullException("sectionDto", "Section DTO must be provided.");
            }
            if (string.IsNullOrEmpty(sectionDto.Id))
            {
                throw new ArgumentException("Section GUID not specified.");
            }

            if (sectionDto.Course == null)
            {
                throw new ArgumentNullException("course", "course is required.");
            }

            if (string.IsNullOrEmpty(sectionDto.Title))
            {
                throw new ArgumentNullException("title", "title is required.");
            }

            if (sectionDto.StartOn == null)
            {
                throw new ArgumentNullException("startOn", "startOn is required.");
            }

            if (sectionDto.StartOn.HasValue && sectionDto.EndOn.HasValue)
            {
                if (sectionDto.StartOn.Value > sectionDto.EndOn.Value)
                {
                    throw new ArgumentException("endOn can not occur earlier than startOn.", "endOn");
                }
            }

            if (sectionDto.Billing.HasValue)
            {
                if (sectionDto.Billing < 0)
                {
                    throw new ArgumentException("section.Billing", "Billing can not be a negative number");
                }
            }
            if (sectionDto.Credits.Any())
            {
                foreach (var credit in sectionDto.Credits)
                {
                    if (credit.CreditCategory == null)
                    {
                        throw new ArgumentException(
                            "Section provided is not valid, Credits must contain a Credit Category.", "credit category");
                    }

                    if (credit.Measure == null)
                    {
                        throw new ArgumentException("Section provided is not valid, Credits must contain a Measure.",
                            "credit measure");
                    }

                    if (credit.Minimum == null)
                    {
                        throw new ArgumentException("Section provided is not valid, Credits must contain a Minimum.",
                            "credit minimum");
                    }

                    if (credit.CreditCategory.CreditType == null)
                    {
                        throw new ArgumentException("Credit Type is required if for Credit Categories if Credits are in the message body.");
                    }

                    if (credit.CreditCategory.Detail != null && string.IsNullOrEmpty(credit.CreditCategory.Detail.Id))
                    {
                        throw new ArgumentException("Credit Category id is required within the detail section of Credit Category if it is in the message body.");
                    }
                    if (credit.Increment == null && credit.Maximum != null)
                    {
                        throw new ArgumentException("Credit Increment is required when Credit Maximum exists.");
                    }
                    if (credit.Maximum == null && credit.Increment != null)
                    {
                        throw new ArgumentException("Credit Maximum is required when Credit Increment exists.");
                    }
                    if (credit.Maximum != null && credit.Measure == CreditMeasure2.CEU)
                    {
                        throw new ArgumentException("Credit Maximum cannot exist when Credit Measure is 'ceu'.");
                    }
                    if (credit.Increment != null && credit.Measure == CreditMeasure2.CEU)
                    {
                        throw new ArgumentException("Credit Increment cannot exist when Credit Measure is 'ceu'.");
                    }
                }

            }

            if (sectionDto.CourseLevels != null && sectionDto.CourseLevels.Any())
            {
                foreach (var level in sectionDto.CourseLevels)
                {
                    if (string.IsNullOrEmpty(level.Id))
                    {
                        throw new ArgumentException(
                            "Course Level id is a required field when Course Levels are in the message body.");
                    }
                }
            }

            if (sectionDto.InstructionalPlatform != null)
            {
                if (string.IsNullOrEmpty(sectionDto.InstructionalPlatform.Id))
                {
                    throw new ArgumentException(
                        "Instructional Platform id is a required field when Instructional Methods are in the message body.");
                }
            }

            if (sectionDto.AcademicLevels != null && sectionDto.AcademicLevels.Any())
            {
                foreach (var level in sectionDto.AcademicLevels)
                {
                    if (string.IsNullOrEmpty(level.Id))
                    {
                        throw new ArgumentException(
                            "Academic Level id is a required field when Academic Levels are in the message body.");
                    }
                }
            }

            if (sectionDto.GradeSchemes != null && sectionDto.GradeSchemes.Any())
            {
                foreach (var scheme in sectionDto.GradeSchemes)
                {
                    if (string.IsNullOrEmpty(scheme.Id))
                    {
                        throw new ArgumentException(
                            "Grade Scheme id is a required field when Grade Schemes are in the message body.");
                    }
                }
            }

            if (sectionDto.OwningInstitutionUnits != null && sectionDto.OwningInstitutionUnits.Any())
            {
                foreach (var org in sectionDto.OwningInstitutionUnits)
                {
                    if (org.InstitutionUnit == null || string.IsNullOrEmpty(org.InstitutionUnit.Id))
                    {
                        throw new ArgumentException("Institution Unit id is a required field when Owning Organizations are in the message body.");
                    }
                }
            }

            if (sectionDto.Credits != null && sectionDto.Credits.Any())
            {
                foreach (var credit in sectionDto.Credits)
                {
                    if (credit.CreditCategory == null)
                    {
                        throw new ArgumentException("Credit Category is required if Credits are in the message body.");
                    }

                    if (credit.CreditCategory.CreditType == null)
                    {
                        throw new ArgumentException("Credit Type is required for Credit Categories if Credits are in the message body.");
                    }
                }
            }

            if (sectionDto.Duration != null && (sectionDto.Duration.Unit == DurationUnit2.Months || sectionDto.Duration.Unit == DurationUnit2.Years || sectionDto.Duration.Unit == DurationUnit2.Days))
            {
                throw new ArgumentException("Section Duration Unit is not allowed to be set to Days, Months or Years");
            }

            if (sectionDto.Duration != null && sectionDto.Duration.Unit == DurationUnit2.Weeks && sectionDto.Duration.Length < 0)
            {
                throw new ArgumentException("Section Duration Length must be a positive number.");
            }

            if (sectionDto.Duration != null && sectionDto.Duration.Unit == null)
            {
                throw new ArgumentException("Section Duration Unit must contain a value if the unit property is specified.");
            }

            if (sectionDto.Credits.Count() > 2)
            {
                throw new ArgumentException("A maximum of 2 entries are allowed in the Credits array.");
            }
            if (sectionDto.Credits.Count() == 2)
            {
                if (sectionDto.Credits.ElementAt(0).CreditCategory.CreditType != sectionDto.Credits.ElementAt(1).CreditCategory.CreditType)
                {
                    throw new ArgumentException("The same Credit Type must be used for each entry in the Credits array.");
                }
                if (!(sectionDto.Credits.ElementAt(0).Measure == CreditMeasure2.CEU && sectionDto.Credits.ElementAt(1).Measure == CreditMeasure2.Credit)
                    && !(sectionDto.Credits.ElementAt(0).Measure == CreditMeasure2.Credit && sectionDto.Credits.ElementAt(1).Measure == CreditMeasure2.CEU)
                    && !(sectionDto.Credits.ElementAt(0).Measure == CreditMeasure2.CEU && sectionDto.Credits.ElementAt(1).Measure == CreditMeasure2.Hours)
                    && !(sectionDto.Credits.ElementAt(0).Measure == CreditMeasure2.Hours && sectionDto.Credits.ElementAt(1).Measure == CreditMeasure2.CEU))
                {
                    throw new ArgumentException("Invalid combination of measures '" + sectionDto.Credits.ElementAt(0).Measure
                        + "' and '" + sectionDto.Credits.ElementAt(1).Measure + "'");
                }
            }

            if (sectionDto.ReservedSeatsMaximum != null)
            {
                throw new ArgumentException("Maximum reserved seats cannot be set through the sections endpoint.  Use section-crosslists endpoint instead.");
            }

            // Waitlist not implemented
            //if (sectionDto.Waitlist != null && sectionDto.Waitlist.RegistrationInterval != null)
            //{
            //    if (sectionDto.Waitlist.RegistrationInterval.Unit == SectionWaitlistRegistrationIntervalUnit.Hour)
            //        throw new ArgumentException("Colleague only allows registration intervals in days.  Hours cannot be accepted.");
            //}

            Domain.Student.Entities.Section section = null;

            try
            {
                section = await _sectionRepository.GetSectionByGuidAsync(sectionDto.Id);
            }
            catch (Exception)
            {
            }


            SectionStatusIntegration? newStatusFromDetail = null;

            if (sectionDto.Status != null && sectionDto.Status.Detail != null)
            {
                var allStatuses = await _studentReferenceDataRepository.GetSectionStatusesAsync(false);
                try
                {
                    newStatusFromDetail = allStatuses.Single(st => st.Guid == sectionDto.Status.Detail.Id).SectionStatusIntg;
                }
                catch
                {
                    throw new ArgumentException("Section status detail guid " + sectionDto.Status.Detail.Id + " is not valid.");
                }

            }



            string id = section == null ? null : section.Id;
            var currentStatuses = section == null || section.Statuses == null
                ? new List<SectionStatusItem>()
                : section.Statuses.ToList();
            var curriculumConfiguration = await GetCurriculumConfiguration2Async();

            List<SectionStatusItem> statuses = new List<SectionStatusItem>();
            if (sectionDto.Status != null)
            {
                if (newStatusFromDetail == null)
                {
                    statuses = await UpdateSectionStatus3(currentStatuses, sectionDto.Status.Category);
                }
                else
                {
                    statuses = await UpdateSectionStatus4(currentStatuses, sectionDto.Status);
                }
            }
            var course = await _courseRepository.GetCourseByGuidAsync(sectionDto.Course.Id);

            // Waitlist no longer implemented due to perfomance problems
            // Check to make sure user is not trying to change the section waitlist status
            //if (section != null && sectionDto.Waitlist != null)
            //{
            //    if ( section.WaitlistStatus != null)
            //    {
            //        if (sectionDto.Waitlist.Status == SectionWaitlistStatus.Open)
            //        {
            //            if (section.WaitlistStatus != "Wlst" && section.WaitlistStatus != "Open")
            //            {
            //                throw new ArgumentException("Waitlist status of " + sectionDto.Waitlist.Status.ToString() + " does not match existing" +
            //                    " waitlist status.  Status cannot be changed through the API.");
            //            }
            //        }
            //        else
            //        {
            //            if (section.WaitlistStatus == "Wlst" || section.WaitlistStatus == "Open")
            //            {
            //                throw new ArgumentException("Waitlist status of " + sectionDto.Waitlist.Status.ToString() + " does not match existing" +
            //                    " waitlist status.  Status cannot be changed through the API.");
            //            }
            //        }

            //    }
            //}


            // Set the credit type and credits/CEUs for the course based on the supplied GUID, or using the ERP default 
            Ellucian.Colleague.Domain.Student.Entities.CreditCategory creditCategory = null;
            if (sectionDto.Credits != null &&
                sectionDto.Credits.Any() &&
                sectionDto.Credits.ToList()[0].CreditCategory != null &&
                sectionDto.Credits.ToList()[0].CreditCategory.Detail != null &&
                !string.IsNullOrEmpty(sectionDto.Credits.ToList()[0].CreditCategory.Detail.Id))
            {
                creditCategory = (await CreditTypesAsync()).FirstOrDefault(ct => ct.Guid == sectionDto.Credits.ToList()[0].CreditCategory.Detail.Id);
                if (creditCategory == null)
                {
                    throw new ArgumentException("Invalid Id '" + sectionDto.Credits.ToList()[0].CreditCategory.Detail.Id + "' supplied for creditCategory");
                }
            }
            // If we don't have a GUID then check for a CreditType enumeration value
            if (creditCategory == null &&
                sectionDto.Credits != null &&
                sectionDto.Credits.Any() &&
                sectionDto.Credits.ToList()[0].CreditCategory != null &&
                sectionDto.Credits.ToList()[0].CreditCategory.CreditType != null)
            {
                if (sectionDto.Credits.ToList()[0].CreditCategory.CreditType == CreditCategoryType3.Exam ||
                    sectionDto.Credits.ToList()[0].CreditCategory.CreditType == CreditCategoryType3.WorkLifeExperience)
                {
                    throw new InvalidOperationException("Credit category type 'exam' or 'workLifeExperience' are not supported.");
                }

                // Find the credit category that matches the enumeration
                switch (sectionDto.Credits.ToList()[0].CreditCategory.CreditType)
                {
                    case (CreditCategoryType3.ContinuingEducation):
                        creditCategory = (await CreditTypesAsync()).FirstOrDefault(cc => cc.CreditType == CreditType.ContinuingEducation);
                        break;
                    case (CreditCategoryType3.Institutional):
                        creditCategory = (await CreditTypesAsync()).FirstOrDefault(cc => cc.CreditType == CreditType.Institutional);
                        break;
                    case (CreditCategoryType3.Transfer):
                        creditCategory = (await CreditTypesAsync()).FirstOrDefault(cc => cc.CreditType == CreditType.Transfer);
                        break;
                    case (CreditCategoryType3.Exchange):
                        creditCategory = (await CreditTypesAsync()).FirstOrDefault(cc => cc.CreditType == CreditType.Exchange);
                        break;
                    case (CreditCategoryType3.Other):
                        creditCategory = (await CreditTypesAsync()).FirstOrDefault(cc => cc.CreditType == CreditType.Other);
                        break;
                    case (CreditCategoryType3.NoCredit):
                        creditCategory = (await CreditTypesAsync()).FirstOrDefault(cc => cc.CreditType == CreditType.None);
                        break;
                }
            }

            //If creditCategory is null then throw an exception
            if (creditCategory == null)
            {
                throw new ArgumentException("Credits data requires Credit Category Type or Id");
            }

            var creditTypeCode = creditCategory == null ? string.Empty : creditCategory.Code;

            decimal? minCredits;
            decimal? ceus;
            decimal? maxCredits;
            decimal? varIncrCredits;
            minCredits = null;
            maxCredits = null;
            varIncrCredits = null;
            ceus = null;
            foreach (var credits in sectionDto.Credits)
            {
                var creditInfo = (sectionDto.Credits == null || sectionDto.Credits.Count == 0) ? null : credits;
                var measure = creditInfo == null ? null : creditInfo.Measure;
                if (measure == Dtos.CreditMeasure2.CEU)
                {
                    ceus = creditInfo == null ? 0 : creditInfo.Minimum;
                }
                else
                {
                    minCredits = creditInfo == null ? 0 : creditInfo.Minimum;
                    maxCredits = creditInfo == null ? null : creditInfo.Maximum;
                    varIncrCredits = creditInfo == null ? null : creditInfo.Increment;
                }
            }

            if (sectionDto.Credits == null || !sectionDto.Credits.Any())
            {
                if (course != null)
                {
                    minCredits = course.MinimumCredits;
                    ceus = course.Ceus;
                    creditTypeCode = course.LocalCreditType;
                }
            }

            // Set the list of departments/shares
            List<OfferingDepartment> offeringDepartments = new List<OfferingDepartment>();

            // If we have supplied owning organization, then use that first.
            if (sectionDto.OwningInstitutionUnits != null && sectionDto.OwningInstitutionUnits.Any())
            {
                foreach (var owningInstitutionUnit in sectionDto.OwningInstitutionUnits)
                {

                    var division = (await _referenceDataRepository.GetDivisionsAsync(true))
                                            .FirstOrDefault(div => div.Guid.Equals(owningInstitutionUnit.InstitutionUnit.Id, StringComparison.OrdinalIgnoreCase));
                    if (division != null)
                    {
                        throw new InvalidOperationException("Owning institution unit of type 'division' is not supported.");
                    }

                    var school = (await _referenceDataRepository.GetSchoolsAsync(true))
                                            .FirstOrDefault(div => div.Guid.Equals(owningInstitutionUnit.InstitutionUnit.Id, StringComparison.OrdinalIgnoreCase));
                    if (school != null)
                    {
                        throw new InvalidOperationException("Owning institution unit of type 'school' is not supported.");
                    }

                    var department = (await DepartmentsAsync()).Where(d => d.Guid == owningInstitutionUnit.InstitutionUnit.Id).FirstOrDefault();
                    var academicDepartment = department != null ? (await _studentReferenceDataRepository.GetAcademicDepartmentsAsync()).FirstOrDefault(ad => ad.Code == department.Code) : null;
                    if (academicDepartment != null)
                    {
                        offeringDepartments.Add(new OfferingDepartment(academicDepartment.Code, owningInstitutionUnit.OwnershipPercentage));
                    }
                }
            }
            // If we don't have offering departments in the payload and didn't find
            // any departments, we will default from the course.  This has to be done
            // here instead of the Colleague transaction because the Sections Entity
            // requires at least one department.
            if (offeringDepartments.Count == 0)
            {
                offeringDepartments.AddRange(course.Departments);
            }

            var courseLevels = await CourseLevelsAsync();
            // Convert various codes to their Colleague values or get them from the course
            List<string> courseLevelCodes = new List<string>();
            if (sectionDto.CourseLevels == null || sectionDto.CourseLevels.Count == 0)
            {
                courseLevelCodes = course.CourseLevelCodes;
            }
            else
            {
                foreach (var level in sectionDto.CourseLevels)
                {
                    var courseLevelCode = ConvertGuidToCode(courseLevels, level.Id);
                    if (string.IsNullOrEmpty(courseLevelCode))
                    {
                        throw new ArgumentException(string.Concat("Invalid Id '", level.Id, "' supplied for courseLevels"));
                    }
                    else
                    {
                        courseLevelCodes.Add(courseLevelCode);
                    }
                }
            }

            string academicLevel = course.AcademicLevelCode;
            if (sectionDto.AcademicLevels != null && sectionDto.AcademicLevels.Any())
            {
                var tempAcadCode = ConvertGuidToCode((await AcademicLevelsAsync()), sectionDto.AcademicLevels[0].Id);
                if (!string.IsNullOrEmpty(tempAcadCode))
                {
                    academicLevel = tempAcadCode;
                }
            }

            string gradeScheme = null;
            if (sectionDto.GradeSchemes == null || sectionDto.GradeSchemes.Count == 0)
            {
                gradeScheme = course.GradeSchemeCode;
            }
            else
            {
                var gradeSchemeEntity = (await GradeSchemesAsync()).Where(gs => gs != null && gs.Guid == sectionDto.GradeSchemes[0].Id).FirstOrDefault();
                var schemeCode = gradeSchemeEntity != null ? gradeSchemeEntity.Code : null;
                if (string.IsNullOrEmpty(schemeCode))
                {
                    throw new ArgumentException(string.Concat("Invalid Id '", sectionDto.GradeSchemes[0].Id, "' supplied for gradeSchemes"));
                }
                else
                {
                    gradeScheme = schemeCode;
                }
            }

            string site = sectionDto.Site == null ? null : ConvertGuidToCode(locations, sectionDto.Site.Id);

            string learningProvider = null;
            if (sectionDto.InstructionalPlatform != null)
            {
                var providerCode = ConvertGuidToCode((await InstructionalPlatformsAsync()), sectionDto.InstructionalPlatform.Id);
                if (string.IsNullOrEmpty(providerCode))
                {
                    throw new ArgumentException(string.Concat("Invalid Id '", sectionDto.InstructionalPlatform.Id, "' supplied for instructionalPlatforms"));
                }
                else
                {
                    learningProvider = providerCode;
                }
            }

            //string term = sectionDto.AcademicPeriod == null ? null : ConvertGuidToCode(academicPeriods, sectionDto.AcademicPeriod.Id);
            string term = string.Empty;
            if (sectionDto.AcademicPeriod != null && !string.IsNullOrEmpty(sectionDto.AcademicPeriod.Id))
            {
                term = ConvertGuidToCode(academicPeriods, sectionDto.AcademicPeriod.Id);
                if (string.IsNullOrEmpty(term))
                {
                    throw new ArgumentException(string.Format("Invalid Id '{0}' supplied for academicPeriod.", sectionDto.AcademicPeriod.Id));
                }
            }


            // Because census dates can be defaulted in from TERMS or TERMS.LOCATIONS when the consumer GETS them,
            // if the consumer PUTs or POSTs a section with census dates, we have to make sure they are intended to
            // be overridden at the section level, rather than blindly writing inherited defaults back to the section.

            if (sectionDto.CensusDates != null && sectionDto.CensusDates.Count() > 0)
            {

                Domain.Student.Entities.Term secTerm = null;
                List<DateTime?> secTermOverrides = null;
                List<DateTime?> secTermLocOverrides = null;

                // Check for term Census dates overrides

                if (term != null)
                {
                    var secTerms = (await AllTermsAsync()).Where(t => t.Code == term);
                    if (secTerms != null && secTerms.Count() == 1)
                    {
                        secTerm = secTerms.First();                        
                        if (secTerm.RegistrationDates != null && secTerm.RegistrationDates.Where(srd => string.IsNullOrEmpty(srd.Location)).Count() == 1)
                        {
                            secTermOverrides = secTerm.RegistrationDates.First(srd => string.IsNullOrEmpty(srd.Location)).CensusDates;
                        }
                    }

                }

                // If we have a term, now check for a location to see if it has its own term-location overrides

                String secLocCode = null;

                if (term != null && site != null && secTerm != null)
                {
                    var key = term + "*" + site;
                    var termLocRegDates = secTerm.RegistrationDates.Where(trd => trd.Location == secLocCode);
                    //var termLocRegDates = secTerm.RegistrationDates.Where(trd => trd.Location == site);
                    if (termLocRegDates != null && termLocRegDates.Count() > 0)
                    {
                        secTermLocOverrides = termLocRegDates.First().CensusDates;
                    }
                }

                // Now compare the lists of dates (if any) and decide if the incoming dates match
                // existing overrides - which indicates they probably were defaulted in on the GET, 
                // and should not be written to the section record on the PUT.

                if (secTermLocOverrides != null && sectionDto.CensusDates != null && sectionDto.CensusDates.Any())
                {
                    if (sectionDto.CensusDates.SequenceEqual(secTermLocOverrides))
                    {
                        // incoming census dates match the term-location overrides, do not update the section overrides.
                        sectionDto.CensusDates = null;
                    }
                }
                else
                {
                    if (sectionDto.CensusDates != null && secTermOverrides != null)
                    {
                        if (sectionDto.CensusDates.SequenceEqual(secTermOverrides))
                        {
                            // incoming census dates match the term overrides, do not update the section overrides.
                            sectionDto.CensusDates = null;
                        }
                    }
                }
            }

            // Create the section entity
            var entity = new Domain.Student.Entities.Section(id, course.Id, sectionDto.Number, sectionDto.StartOn.GetValueOrDefault().DateTime,
                minCredits, ceus, sectionDto.Title, creditTypeCode, offeringDepartments, courseLevelCodes, academicLevel, statuses,
                course.AllowPassNoPass, course.AllowAudit, course.OnlyPassNoPass, course.AllowWaitlist, false)
            {
                Guid = (sectionDto.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase)) ? string.Empty : sectionDto.Id.ToLowerInvariant(),
                EndDate = sectionDto.EndOn == null ? default(DateTime?) : sectionDto.EndOn.Value.Date,
                MaximumCredits = maxCredits,
                VariableCreditIncrement = varIncrCredits,
                GradeSchemeCode = gradeScheme,
                TopicCode = course.TopicCode,
                GlobalCapacity = sectionDto.MaximumEnrollment,
                SectionCapacity = sectionDto.MaximumEnrollment,
                Name = sectionDto.Code,
                LearningProvider = learningProvider,
                TermId = term,
                Location = site,
                CensusDates = sectionDto.CensusDates,
            };
            entity.BillingCred = sectionDto.Billing;


            //Instructional methods
            if (sectionDto.InstructionalMethods.Count() > 0)
            {
                var allInstructionalMethods = await _studentReferenceDataRepository.GetInstructionalMethodsAsync();
                foreach (var method in sectionDto.InstructionalMethods)
                {
                    var instrMethodEntity = allInstructionalMethods.SingleOrDefault(im => im.Guid == method.Id);
                    if (instrMethodEntity != null)
                    {
                        entity.AddInstructionalMethod(instrMethodEntity.Code);
                    }
                    else
                    {
                        throw new ArgumentException(string.Concat("Invalid Id '", method.Id, "' supplied for instructionalMethods"));
                    }
                }
            }

            // Course Categories/types
            if (sectionDto.CourseCategories != null && sectionDto.CourseCategories.Count() > 0)
            {
                foreach (var cat in sectionDto.CourseCategories)
                {
                    var typeEntity = (await AllCourseTypes()).FirstOrDefault(cc => cc.Guid == cat.Id);
                    if (typeEntity != null)
                    {
                        entity.AddCourseType(typeEntity.Code);
                    }
                    else
                    {
                        throw new ArgumentException(string.Concat("Invalid Id '", cat.Id, "' supplied for courseCategories"));
                    }

                }
            }

            if (string.IsNullOrEmpty(entity.TermId))
            {
                entity.TermId = SectionProcessor.DetermineTerm(entity, await AllTermsAsync());
            }

            //check if duration is set on the incoming DTO
            if (sectionDto.Duration != null)
            {
                entity.NumberOfWeeks = sectionDto.Duration.Length;
            }
            else
            {
                if (entity.EndDate.HasValue)
                {
                    entity.NumberOfWeeks = (int)Math.Ceiling((entity.EndDate.Value - entity.StartDate).Days / 7m);
                }
            }
            return entity;
        }



        /// <summary>
        /// Update a section
        /// </summary>
        /// <param name="section">DTO containing the Data Model version 8 section to update</param>
        /// <returns>DTO containing the updated Data Model version 8 section</returns>
        public async Task<Dtos.Section5> PutSection5Async(Dtos.Section5 section)
        {
         
            if ((section != null) && (section.Status != null))
                await ValidateSectionStatusConfigurationAsync(section.Status.Category);

            _sectionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            // Make sure dept percentage is from 0-100%.  This is also checked in the contructor for the domain entity OfferingDepartment,
            // but we want to customize the error message for Ethos APIs to refernece "Ownership percentage".
            if (section != null)
            {
                if (section.OwningInstitutionUnits != null && section.OwningInstitutionUnits.Any())
                {
                    foreach (var owningInstitutionUnit in section.OwningInstitutionUnits)
                    {
                        if (owningInstitutionUnit != null)
                        {
                            if (owningInstitutionUnit.OwnershipPercentage < 0 || owningInstitutionUnit.OwnershipPercentage > 100)
                            {
                                throw new ArgumentOutOfRangeException("ownershipPercentage", "Ownership percentage must be between 0 and 100.");
                            }
                        }
                    }
                }
            }
            // Convert the CDM section into a domain entity and update it
            var entity = await ConvertSectionDto5ToEntityAsync(section);
            if (entity == null)
            {
                throw new KeyNotFoundException(string.Format("Could not update section '{0}'. ", section.Id));
            }
            var updatedEntity = await _sectionRepository.PutSectionAsync(entity);
            if (updatedEntity == null)
            {
                throw new KeyNotFoundException(string.Format("GUID '{0}' failed to return a valid section. ", section.Id));
            }
            var updatedDto = await ConvertSectionEntityToDto5Async(updatedEntity, false);

            return updatedDto;
        }


        /// <summary>
        /// Converts entity to dto.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="filterDefaultCensusDates"</param>
        /// <returns></returns>
        private async Task<Dtos.Section5> ConvertSectionEntityToDto5Async(Domain.Student.Entities.Section entity, bool filterDefaultCensusDates = false)
        {
            if (entity == null)
            {
                return null;
            }

            var sectionDto = new Dtos.Section5();

            sectionDto.Id = entity.Guid.ToLowerInvariant();
            sectionDto.Code = entity.Name;
            sectionDto.Number = entity.Number;
            sectionDto.Title = entity.Title;
            sectionDto.StartOn = entity.StartDate;
            sectionDto.EndOn = entity.EndDate;

            if (!string.IsNullOrEmpty(entity.TermId))
            {
                sectionDto.AcademicPeriod = new Dtos.GuidObject2();
                var academicPeriod = academicPeriods.FirstOrDefault(a => a.Code == entity.TermId);
                sectionDto.AcademicPeriod.Id = (academicPeriod != null) ? academicPeriod.Guid : null;
            }
            if (string.IsNullOrEmpty(entity.LearningProvider))
            {
                if (!string.IsNullOrEmpty(entity.DefaultLearningProvider))
                {
                    entity.LearningProvider = entity.DefaultLearningProvider;
                }
            }
            if (!string.IsNullOrEmpty(entity.LearningProvider))
            {
                var instructionalPlatform = (await InstructionalPlatformsAsync()).FirstOrDefault(i => i.Code == entity.LearningProvider);
                if (instructionalPlatform != null && !string.IsNullOrEmpty(instructionalPlatform.Guid))
                {
                    sectionDto.InstructionalPlatform = new Dtos.GuidObject2(instructionalPlatform.Guid);
                }
            }
            sectionDto.Course = new Dtos.GuidObject2(await _courseRepository.GetCourseGuidFromIdAsync(entity.CourseId));

            var credit = new Dtos.DtoProperties.SectionCreditDtoProperty();
            var ceuCredit = new Dtos.DtoProperties.SectionCreditDtoProperty();
            credit.CreditCategory = new CreditIdAndTypeProperty2();
            ceuCredit.CreditCategory = new CreditIdAndTypeProperty2();

            var creditTypeItems = await CreditTypesAsync();

            if (creditTypeItems.Any(ct => ct.Code == entity.CreditTypeCode))
            {
                var creditTypeItem = creditTypeItems.Where(ct => ct.Code == entity.CreditTypeCode).First();
                if (creditTypeItem != null)
                {
                    if (!string.IsNullOrEmpty(creditTypeItem.Guid))
                    {
                        credit.CreditCategory.Detail = new GuidObject2() { Id = creditTypeItem.Guid };
                        ceuCredit.CreditCategory.Detail = new GuidObject2() { Id = creditTypeItem.Guid };
                    }
                    switch (creditTypeItem.CreditType)
                    {
                        case CreditType.ContinuingEducation:
                            credit.CreditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
                            break;
                        case CreditType.Institutional:
                            credit.CreditCategory.CreditType = CreditCategoryType3.Institutional;
                            break;
                        case CreditType.Transfer:
                            credit.CreditCategory.CreditType = CreditCategoryType3.Transfer;
                            break;
                        case CreditType.Exchange:
                            credit.CreditCategory.CreditType = CreditCategoryType3.Exchange;
                            break;
                        case CreditType.Other:
                            credit.CreditCategory.CreditType = CreditCategoryType3.Other;
                            break;
                        case CreditType.None:
                            credit.CreditCategory.CreditType = CreditCategoryType3.NoCredit;
                            break;
                        default:
                            credit.CreditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
                            break;
                    }
                }
            }
            else
            {
                //default to CE
                credit.CreditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
            }

            ceuCredit.CreditCategory.CreditType = credit.CreditCategory.CreditType;


            if (entity.Ceus.HasValue)
            {
                ceuCredit.Measure = Dtos.CreditMeasure2.CEU;
                ceuCredit.Minimum = entity.Ceus.Value;
                sectionDto.Credits.Add(ceuCredit);
            }
            if (entity.MinimumCredits.HasValue)
            {
                credit.Measure = Dtos.CreditMeasure2.Credit;
                credit.Minimum = entity.MinimumCredits.GetValueOrDefault();
                credit.Maximum = entity.MaximumCredits;
                credit.Increment = entity.VariableCreditIncrement;
                sectionDto.Credits.Add(credit);
            }


            if (!string.IsNullOrEmpty(entity.Location))
            {
                sectionDto.Site = new Dtos.GuidObject2(ConvertCodeToGuid(locations, entity.Location));
            }
            sectionDto.AcademicLevels.Add(new Dtos.GuidObject2(ConvertCodeToGuid((await AcademicLevelsAsync()), entity.AcademicLevelCode)));
            var gradeScheme = (await GradeSchemesAsync()).Where(gs => gs != null && gs.Code == entity.GradeSchemeCode).FirstOrDefault();
            var gradeSchemeGuid = gradeScheme != null ? gradeScheme.Guid : null;
            sectionDto.GradeSchemes.Add(new Dtos.GuidObject2(gradeSchemeGuid));
            foreach (var code in entity.CourseLevelCodes)
            {
                sectionDto.CourseLevels.Add(new Dtos.GuidObject2(ConvertCodeToGuid(await CourseLevelsAsync(), code)));
            }
            string status;
            var currentStatus = ConvertSectionStatusToDto2(entity.Statuses.ElementAt(0).IntegrationStatus, out status);
            status = entity.Statuses.ElementAt(0).StatusCode;
            if (currentStatus != SectionStatus2.NotSet)
            {
                sectionDto.Status = new SectionStatusDtoProperty();
                sectionDto.Status.Category = currentStatus;
                sectionDto.Status.Detail = new GuidObject2(ConvertCodeToGuid((await AllStatusesWithGuidsAsync()), status));
            }

            // Use section capacity here.  Use the section-crosslists endpoint to find global capacity
            sectionDto.MaximumEnrollment = entity.SectionCapacity;

            // Determine the Department information for the course
            sectionDto.OwningInstitutionUnits = new List<Ellucian.Colleague.Dtos.OwningInstitutionUnit>();
            var departments = new List<Ellucian.Colleague.Dtos.OwningInstitutionUnit>();

            if (entity.Departments != null && entity.Departments.Any())
            {
                foreach (var offeringDept in entity.Departments)
                {
                    var academicDepartment = (await AllDepartments()).FirstOrDefault(d => d.Code == offeringDept.AcademicDepartmentCode);
                    if (academicDepartment != null)
                    {
                        var department = new Ellucian.Colleague.Dtos.OwningInstitutionUnit();
                        department.InstitutionUnit.Id = academicDepartment.Guid;
                        department.OwnershipPercentage = offeringDept.ResponsibilityPercentage;
                        departments.Add(department);
                    }
                }
                sectionDto.OwningInstitutionUnits = departments;

            }

            if (entity.NumberOfWeeks.HasValue)
            {
                sectionDto.Duration = new SectionDuration2()
                {
                    Length = entity.NumberOfWeeks.Value,
                    Unit = DurationUnit2.Weeks
                };
            }


            if (entity.CensusDates != null && entity.CensusDates.Count() > 0)
            {
                sectionDto.CensusDates = entity.CensusDates;
            }
            else if (filterDefaultCensusDates == false && !String.IsNullOrEmpty(entity.TermId))
            {
                // If the section has a term, and if there are no census dates in the section, for EEDM output we will check
                // TERMS.LOCATIONS for census dates.  If none there, we will check TERMS.  Most of this logic is already in the
                // Terms repository, and the results are in the regstration date overrides for the term and keyed by location.


                var secTerm = entity.TermId;
                var secLoc = entity.Location;

                Domain.Student.Entities.Term term = (await AllTermsAsync()).FirstOrDefault(t => t.Code == secTerm);
                if (term != null)
                {
                    var termOverrides = term.RegistrationDates.Where(trd => trd.Location == "");
                    if (!string.IsNullOrWhiteSpace(secLoc))
                    {
                        var termLocOverrides = term.RegistrationDates.Where(trd => trd.Location == secLoc);
                        if (termLocOverrides != null && termLocOverrides.Count() > 0)
                        {
                            if (termLocOverrides.First().CensusDates != null && termLocOverrides.First().CensusDates.Count() > 0)
                            {
                                sectionDto.CensusDates = termLocOverrides.First().CensusDates;
                            }
                        }
                    }
                    if (sectionDto.CensusDates == null || sectionDto.CensusDates.Count() <= 0)
                    {
                        if (termOverrides != null && termOverrides.Count() > 0 && termOverrides.First().CensusDates.Count() > 0)
                        {
                            if (termOverrides.First().CensusDates != null)
                            {
                                sectionDto.CensusDates = termOverrides.First().CensusDates;
                            }
                        }
                    }
                }

            }

            sectionDto.Billing = entity.BillingCred;



            if (entity.CourseTypeCodes != null && entity.CourseTypeCodes.Any())
            {
                sectionDto.CourseCategories = new List<GuidObject2>();
                foreach (var cat in entity.CourseTypeCodes)
                {
                    var catGuid = (await AllCourseTypes()).FirstOrDefault(ct => ct.Code == cat);
                    if (catGuid != null)
                    {
                        sectionDto.CourseCategories.Add(new GuidObject2() { Id = catGuid.Guid });
                    }
                    else
                    {
                        throw new KeyNotFoundException("Course type code " + cat + " not found");
                    }
                }
            }

            if (entity.InstructionalMethods != null && entity.InstructionalMethods.Any())
            {
                var instructionalMethods = new List<GuidObject2>();
                var allInstructionalMethods = await this.InstructionalMethodsAsync();
                foreach (var method in entity.InstructionalMethods)
                {
                    var instructionalMethod = allInstructionalMethods.FirstOrDefault(x => x.Code == method);
                    if (instructionalMethod == null)
                    {
                        throw new ArgumentException(string.Concat("Instructional Method not found for :", method));
                    }
                    instructionalMethods.Add(new GuidObject2(instructionalMethod.Guid));

                }
                if (instructionalMethods != null && instructionalMethods.Any())
                    sectionDto.InstructionalMethods = instructionalMethods;
            }

            // Causing perfomance issue, temporarily removing
            //sectionDto.Waitlist = new SectionWaitlistDtoProperty();
            //sectionDto.Waitlist.Status = SectionWaitlistStatus.Closed;

            //if (entity.AllowWaitlist)
            //{
            //    sectionDto.Waitlist.WaitlistMaximum = entity.WaitlistMaximum;
            //    if (entity.WaitListNumberOfDays != null && entity.WaitListNumberOfDays > 0)
            //    {
            //        sectionDto.Waitlist.RegistrationInterval = new SectionRegistrationIntervalDtoProperty();
            //        sectionDto.Waitlist.RegistrationInterval.Unit = SectionWaitlistRegistrationIntervalUnit.Day;
            //        sectionDto.Waitlist.RegistrationInterval.Value = entity.WaitListNumberOfDays;
            //    }
            //    if (entity.WaitlistStatus == "Wlst" || entity.WaitlistStatus == "Open")
            //    {
            //        sectionDto.Waitlist.Status = SectionWaitlistStatus.Open;
            //    }
            //}

            return sectionDto;
        }

        #endregion EEDM Version 11

        #region EEDM Version 16

        /// <summary>
        /// Get selection criteria, convert it to codes as required and select 
        /// multiple sections from Colleague.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="title">Section Title Contains ...title...</param>
        /// <param name="startOn">Section starts on or after this date</param>
        /// <param name="endOn">Section ends on or before this date</param>
        /// <param name="code">Section Name Contains ...code...</param>
        /// <param name="number">Section Number equal to</param>
        /// <param name="instructionalPlatform">Learning Platform equal to (guid)</param>
        /// <param name="academicPeriod">Section Term equal to (guid)</param>
        /// <param name="academicLevel">Section Academic Level equal to (guid)</param>
        /// <param name="course">Section Course equal to (guid)</param>
        /// <param name="site">Section Location equal to (guid)</param>
        /// <param name="status">Section Status matches closed, open, pending, or cancelled</param>
        /// <param name="owningOrganization">Section Department equal to (guid)</param>
        /// <param name="subject">Section Title Contains ...subject...</param>
        /// <param name="instructor">Section Instructor equal to (guid)</param>
        /// <param name="search">Check if a section is searchable or hidden</param>
        /// <param name="keyword">Perform a keyword search</param>
        /// <returns>List of Section5 <see cref="Dtos.Section6"/> objects representing matching sections</returns>
        public async Task<Tuple<IEnumerable<Dtos.Section6>, int>> GetSections6Async(int offset, int limit, string title = "", string startOn = "", string endOn = "",
            string code = "", string number = "", string instructionalPlatform = "", string academicPeriod = "", string reportingAcademicPeriod = "",
            List<string> academicLevel = null, string course = "", string site = "", string status = "", List<string> owningOrganization = null,
            string subject = "", string instructor = "", SectionsSearchable search = SectionsSearchable.NotSet, string keyword = "", bool bypassCache = false)
        {
            // Convert and validate all input parameters

            IDictionary<string, string> args = new Dictionary<string, string>();
            try
            {
                args = await ValidateAndConvertFilterArguments(startOn, endOn, instructionalPlatform, academicPeriod, course, site, status, instructor, "", "", subject, reportingAcademicPeriod);
            }
            catch (ArgumentException)
            {
                // One or more of the arguments failed to match up to a guid.  Return empty set.
                return new Tuple<IEnumerable<Dtos.Section6>, int>(new List<Dtos.Section6>(), 0);

            }
            var arrayArgs = new Dictionary<string, List<string>>();
            try
            {
                arrayArgs = await ValidateAndConvertFilterArrayArguments(academicLevel, owningOrganization);
            }
            catch (ArgumentException)
            {
                // One or more of the arguments failed to match up to a guid.  Return empty set.
                return new Tuple<IEnumerable<Dtos.Section6>, int>(new List<Dtos.Section6>(), 0);

            }
            var newStartOn = args["newStartOn"];
            var newEndOn = args["newEndOn"];
            var newInstructionalPlatform = args["newInstructionalPlatform"];
            var newAcademicPeriod = args["newAcademicPeriod"];
            var newReportingAcademicPeriod = args["newReportingAcademicPeriod"];
            var instructorId = args["instructorId"];
            var newCourse = args["newCourse"];
            var newSite = args["newSite"];
            var newStatus = args["newStatus"];
            var newOwningOrganization = arrayArgs["newOwningOrganizations"];
            var newAcademicLevel = arrayArgs["newAcademicLevels"];
            var newSubject = args["newSubject"];

            var sectionDtos = new List<Dtos.Section6>();

            Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section>, int> sectionEntities = null;
            try
            {
                if (search != SectionsSearchable.NotSet)
                {
                    sectionEntities = await _sectionRepository.GetSectionsSearchable1Async(offset, limit, search.ToString(), true);
                }
                else if (!(string.IsNullOrEmpty(keyword)))
                {
                    sectionEntities = await _sectionRepository.GetSectionsKeyword1Async(offset, limit, keyword, true, addToCollection: true);
                }
                else
                {
                    var instructors = new List<string>();
                    if (!string.IsNullOrEmpty(instructorId)) instructors.Add(instructorId);

                    sectionEntities = await _sectionRepository.GetSections2Async(offset, limit, title, newStartOn, newEndOn,
                        code, number, newInstructionalPlatform, newAcademicPeriod, newReportingAcademicPeriod,
                        newAcademicLevel, newCourse, newSite, newStatus, newOwningOrganization, newSubject, instructors, "", true);
                }
                foreach (var sectionEntity in sectionEntities.Item1)
                {
                    if (sectionEntity.Guid != null)
                    {
                        var sectionDto = await ConvertSectionEntityToDto6Async(sectionEntity, bypassCache, false);
                        sectionDtos.Add(sectionDto);
                    }
                }

                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }

                return new Tuple<IEnumerable<Dtos.Section6>, int>(sectionDtos, sectionEntities.Item2);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (IntegrationApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get a DTO for section6 for v16.0.0 sections by its GUID
        /// </summary>
        /// <param name="guid">GUID of section</param>
        /// <returns>The Data Model Section version 6 DTO</returns>
        public async Task<Dtos.Section6> GetSection6ByGuidAsync(string guid, bool bypassCache = false)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get a section.");
            }

            try
            {
                var sectionEntity = await _sectionRepository.GetSectionByGuid2Async(guid, true);
                if (sectionEntity == null)
                {
                    throw new KeyNotFoundException(string.Concat("Section not found or invalid for id '", guid, "'.  See Log for more information."));
                }

                var dto = await ConvertSectionEntityToDto6Async(sectionEntity, bypassCache, false);

                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }

                return dto;
            }
            catch (IntegrationApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        ///Get a DTO for section6 for v16.0.0 sections by its GUID
        ///but exclude defaulted census dates
        /// </summary>
        /// <param name="guid">GUID of section</param>
        /// <returns>The Data Model Section version 6 DTO</returns>
        public async Task<Dtos.Section6> GetSection6ByGuidFilterCensusDatesAsync(string guid, bool bypassCache = false)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get a section.");
            }

            try
            {
                var sectionEntity = await _sectionRepository.GetSectionByGuid2Async(guid, true);
                if (sectionEntity == null)
                {
                    throw new KeyNotFoundException(string.Concat("Section not found or invalid for id '", guid, "'.  See Log for more information."));
                }

                var dto = await ConvertSectionEntityToDto6Async(sectionEntity, bypassCache, true);

                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }

                return dto;
            }
            catch (IntegrationApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Create a new section
        /// </summary>
        /// <param name="section">DTO containing the Data Model version 6 section to create</param>
        /// <returns>DTO containing the created Data Model version 6 section</returns>
        public async Task<Dtos.Section6> PostSection6Async(Dtos.Section6 section)
        {
            if ((section != null) && (section.Status != null))
            {
                try
                {
                    await ValidateSectionStatusConfigurationAsync(section.Status.Category);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            _sectionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            // Make sure dept percentage is from 0-100%.  This is also checked in the contructor for the domain entity OfferingDepartment,
            // but we want to customize the error message for Ethos APIs to refernece "Ownership percentage".
            if (section != null)
            {
                if (section.OwningInstitutionUnits != null && section.OwningInstitutionUnits.Any())
                {
                    foreach (var owningInstitutionUnit in section.OwningInstitutionUnits)
                    {
                        if (owningInstitutionUnit != null)
                        {
                            if (owningInstitutionUnit.OwnershipPercentage < 0 || owningInstitutionUnit.OwnershipPercentage > 100)
                            {
                                throw new ArgumentOutOfRangeException("ownershipPercentage", "Ownership percentage must be between 0 and 100.");
                            }
                        }
                    }
                }
            }
            // Convert the CDM section into a domain entity and create it
            var entity = await ConvertSectionDto6ToEntityAsync(section);
            if (entity == null)
            {
                throw new KeyNotFoundException("Failed to create a valid section. ");
            }
            var newEntity = await _sectionRepository.PostSection2Async(entity);
            if (newEntity == null)
            {
                throw new KeyNotFoundException("Failed to return a valid section. ");
            }
            return await ConvertSectionEntityToDto6Async(newEntity, false);

        }

        /// <summary>
        /// Update a section
        /// </summary>
        /// <param name="section">DTO containing the Data Model version 6 section to update</param>
        /// <returns>DTO containing the updated Data Model version 6 section</returns>
        public async Task<Dtos.Section6> PutSection6Async(Dtos.Section6 section)
        {
            if ((section != null) && (section.Status != null))
            {
                try
                {
                    await ValidateSectionStatusConfigurationAsync(section.Status.Category);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            _sectionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            // Make sure dept percentage is from 0-100%.  This is also checked in the contructor for the domain entity OfferingDepartment,
            // but we want to customize the error message for Ethos APIs to refernece "Ownership percentage".
            if (section != null)
            {
                if (section.OwningInstitutionUnits != null && section.OwningInstitutionUnits.Any())
                {
                    foreach (var owningInstitutionUnit in section.OwningInstitutionUnits)
                    {
                        if (owningInstitutionUnit != null)
                        {
                            if (owningInstitutionUnit.OwnershipPercentage < 0 || owningInstitutionUnit.OwnershipPercentage > 100)
                            {
                                throw new ArgumentOutOfRangeException("ownershipPercentage", "Ownership percentage must be between 0 and 100.");
                            }
                        }
                    }
                }
            }
            // Convert the CDM section into a domain entity and update it
            var entity = await ConvertSectionDto6ToEntityAsync(section);
            if (entity == null)
            {
                throw new KeyNotFoundException(string.Format("Could not update section '{0}'. ", section.Id));
            }
            var updatedEntity = await _sectionRepository.PutSection2Async(entity);
            if (updatedEntity == null)
            {
                throw new KeyNotFoundException(string.Format("GUID '{0}' failed to return a valid section. ", section.Id));
            }
            var updatedDto = await ConvertSectionEntityToDto6Async(updatedEntity, false);

            return updatedDto;
        }

        /// <summary>
        /// Converts entity to dto.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="bypassCache"></param>
        /// <param name="filterDefaultCensusDates"</param>
        /// <returns></returns>
        private async Task<Dtos.Section6> ConvertSectionEntityToDto6Async(Domain.Student.Entities.Section entity, bool bypassCache = false, bool filterDefaultCensusDates = false)
        {
            if (entity == null)
            {
                return null;
            }

            var sectionDto = new Dtos.Section6();

            sectionDto.Id = entity.Guid.ToLowerInvariant();
            sectionDto.Code = entity.Name;
            sectionDto.Number = entity.Number;
            sectionDto.Titles = new List<CoursesTitlesDtoProperty>();

            try
            {
                var shortEntity = await _studentReferenceDataRepository.GetSectionTitleTypesGuidAsync("short");
                var shortTitle = new CoursesTitlesDtoProperty() { Type = new GuidObject2(shortEntity), Value = entity.Title };
                sectionDto.Titles.Add(shortTitle);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
            }

            // Printed Comments from Section record.
            if (!string.IsNullOrEmpty(entity.Comments))
            {
                sectionDto.Descriptions = new List<SectionDescriptionDtoProperty>();
                try
                {
                    var printedCommentsEntity = await _studentReferenceDataRepository.GetSectionDescriptionTypesGuidAsync("printed");
                    var sectionDescription = new SectionDescriptionDtoProperty() { Type = new GuidObject2(printedCommentsEntity), Value = entity.Comments };
                    sectionDto.Descriptions.Add(sectionDescription);
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                }
            }

            sectionDto.StartOn = entity.StartDate;
            sectionDto.EndOn = entity.EndDate;

            if (!string.IsNullOrEmpty(entity.TermId))
            {
                try
                {
                    var academicPeriod = await _termRepository.GetAcademicPeriodsGuidAsync(entity.TermId);
                    sectionDto.AcademicPeriod = new Dtos.GuidObject2(academicPeriod);
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                }
            }

            // ReportingAcademicPeriod
            if (!string.IsNullOrEmpty(entity.TermId))
            {
                var acadPeriods = this.academicPeriods;
                if (acadPeriods != null && acadPeriods.Any())
                {
                    var academicPeriod = acadPeriods.FirstOrDefault(a => a.Code == entity.TermId);
                    if (academicPeriod != null && !string.IsNullOrEmpty(academicPeriod.ReportingTerm))
                    {
                        try
                        {
                            var reportingAcademicPeriod = await _termRepository.GetAcademicPeriodsGuidAsync(academicPeriod.ReportingTerm);
                            sectionDto.ReportingAcademicPeriod = new Dtos.GuidObject2(reportingAcademicPeriod);
                        }
                        catch (RepositoryException ex)
                        {
                            IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                        }
                    }
                }
            }

            //Learning Providor
            if (string.IsNullOrEmpty(entity.LearningProvider))
            {
                if (!string.IsNullOrEmpty(entity.DefaultLearningProvider))
                {
                    entity.LearningProvider = entity.DefaultLearningProvider;
                }                    
            }
            if (!string.IsNullOrEmpty(entity.LearningProvider))
            {
                try
                {
                    var instructionalPlatform = await _referenceDataRepository.GetInstructionalPlatformsGuidAsync(entity.LearningProvider);
                    sectionDto.InstructionalPlatform = new Dtos.GuidObject2(instructionalPlatform);
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                }
            }

            try
            {
                var courseGuid = await _courseRepository.GetCourseGuidFromIdAsync(entity.CourseId);
                sectionDto.Course = new Dtos.GuidObject2(courseGuid);
            }
            catch (ArgumentException ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
            }

            var credit = new Dtos.DtoProperties.SectionCreditDtoProperty();
            var ceuCredit = new Dtos.DtoProperties.SectionCreditDtoProperty();
            credit.CreditCategory = new CreditIdAndTypeProperty2();
            ceuCredit.CreditCategory = new CreditIdAndTypeProperty2();

            var creditTypeItems = await CreditTypesAsync();

            if (creditTypeItems != null && creditTypeItems.Any(ct => ct.Code == entity.CreditTypeCode))
            {
                var creditTypeItem = creditTypeItems.Where(ct => ct.Code == entity.CreditTypeCode).FirstOrDefault();
                if (creditTypeItem != null)
                {
                    if (!string.IsNullOrEmpty(creditTypeItem.Guid))
                    {
                        credit.CreditCategory.Detail = new GuidObject2() { Id = creditTypeItem.Guid };
                        ceuCredit.CreditCategory.Detail = new GuidObject2() { Id = creditTypeItem.Guid };
                    }
                    switch (creditTypeItem.CreditType)
                    {
                        case CreditType.ContinuingEducation:
                            credit.CreditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
                            break;
                        case CreditType.Institutional:
                            credit.CreditCategory.CreditType = CreditCategoryType3.Institutional;
                            break;
                        case CreditType.Transfer:
                            credit.CreditCategory.CreditType = CreditCategoryType3.Transfer;
                            break;
                        case CreditType.Exchange:
                            credit.CreditCategory.CreditType = CreditCategoryType3.Exchange;
                            break;
                        case CreditType.Other:
                            credit.CreditCategory.CreditType = CreditCategoryType3.Other;
                            break;
                        case CreditType.None:
                            credit.CreditCategory.CreditType = CreditCategoryType3.NoCredit;
                            break;
                        default:
                            credit.CreditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
                            break;
                    }
                }
            }
            else
            {
                //default to CE
                credit.CreditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
            }

            ceuCredit.CreditCategory.CreditType = credit.CreditCategory.CreditType;

            if (entity.Ceus.HasValue)
            {
                ceuCredit.Measure = Dtos.CreditMeasure2.CEU;
                ceuCredit.Minimum = entity.Ceus.Value;

                if (sectionDto.Credits == null) sectionDto.Credits = new List<SectionCreditDtoProperty>();

                sectionDto.Credits.Add(ceuCredit);
            }
            if (entity.MinimumCredits.HasValue)
            {
                credit.Measure = Dtos.CreditMeasure2.Credit;
                credit.Minimum = entity.MinimumCredits.GetValueOrDefault();
                credit.Maximum = entity.MaximumCredits;
                credit.Increment = entity.VariableCreditIncrement;

                if (sectionDto.Credits == null) sectionDto.Credits = new List<SectionCreditDtoProperty>();

                sectionDto.Credits.Add(credit);
            }

            //site.id
            if (!string.IsNullOrEmpty(entity.Location))
            {
                try
                {
                    var location = await _referenceDataRepository.GetLocationsGuidAsync(entity.Location);
                    sectionDto.Site = new Dtos.GuidObject2(location);
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                }
            }

            //academicLevel.id
            if (!string.IsNullOrEmpty(entity.AcademicLevelCode))
            {
                try
                {
                    if (sectionDto.AcademicLevels == null) sectionDto.AcademicLevels = new List<GuidObject2>();

                    var acadLevelGuid = await _studentReferenceDataRepository.GetAcademicLevelsGuidAsync(entity.AcademicLevelCode);
                    sectionDto.AcademicLevels.Add(new Dtos.GuidObject2(acadLevelGuid));
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                }
            }

            //gradeSchemes.id
            if (!string.IsNullOrEmpty(entity.GradeSchemeCode))
            {
                try
                {
                    var gradeSchemeGuid = await _studentReferenceDataRepository.GetGradeSchemeGuidAsync(entity.GradeSchemeCode);

                    if (sectionDto.GradeSchemes == null) sectionDto.GradeSchemes = new List<GuidObject2>();

                    sectionDto.GradeSchemes.Add(new Dtos.GuidObject2(gradeSchemeGuid));
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                }
            }

            //courseLevels.id
            if (sectionDto.CourseLevels == null) sectionDto.CourseLevels = new List<GuidObject2>();
            foreach (var code in entity.CourseLevelCodes)
            {
                try
                {
                    if (!string.IsNullOrEmpty(code))
                    {
                        var courseLevelCodeGuid = await _studentReferenceDataRepository.GetCourseLevelGuidAsync(code);
                        sectionDto.CourseLevels.Add(new Dtos.GuidObject2(courseLevelCodeGuid));
                    }
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                }
            }
            string status;
            if (entity.Statuses == null || !entity.Statuses.Any() || entity.Statuses.ElementAt(0).IntegrationStatus == SectionStatusIntegration.NotSet)
            {
                IntegrationApiExceptionAddError(string.Format("Section '{0}' is missing status.", entity.Guid), "Validation.Exception", entity.Guid, entity.Id);
            }
            else
            {
                var currentStatus = ConvertSectionStatusToDto2(entity.Statuses.ElementAt(0).IntegrationStatus, out status);
                status = entity.Statuses.ElementAt(0).StatusCode;
                if (currentStatus != SectionStatus2.NotSet)
                {
                    sectionDto.Status = new SectionStatusDtoProperty();
                    sectionDto.Status.Category = currentStatus;
                    try
                    {
                        var statuses = await AllStatusesWithGuidsAsync();
                        if (statuses != null && statuses.Any())
                        {
                            var sts = statuses.FirstOrDefault(i => i.Code.Equals(status, StringComparison.OrdinalIgnoreCase));
                            if (sts == null)
                            {
                                IntegrationApiExceptionAddError(string.Format("No Guid found, Entity:'SECTION.STATUSES', Record ID:'{0}'", status), "GUID.Not.Found", entity.Guid, entity.Id);
                            }
                            else
                            {
                                sectionDto.Status.Detail = new GuidObject2(sts.Guid);
                            }
                        }
                    }
                    catch (ArgumentNullException)
                    {
                        IntegrationApiExceptionAddError(string.Format("No Guid found, Entity:'SECTION.STATUSES', Record ID:'{0}'", status), "GUID.Not.Found", entity.Guid, entity.Id);
                    }
                }
            }
            // Use section capacity here.  Use the section-crosslists endpoint to find global capacity
            sectionDto.MaximumEnrollment = entity.SectionCapacity;

            // Determine the Department information for the course
            sectionDto.OwningInstitutionUnits = new List<Ellucian.Colleague.Dtos.OwningInstitutionUnit>();
            var departments = new List<Ellucian.Colleague.Dtos.OwningInstitutionUnit>();

            if (entity.Departments != null && entity.Departments.Any())
            {
                foreach (var offeringDept in entity.Departments)
                {
                    try
                    {
                        var academicDepartmentGuid = await _referenceDataRepository.GetDepartments2GuidAsync(offeringDept.AcademicDepartmentCode);
                        if (!string.IsNullOrEmpty(academicDepartmentGuid))
                        {
                            var department = new Ellucian.Colleague.Dtos.OwningInstitutionUnit();
                            department.InstitutionUnit.Id = academicDepartmentGuid;
                            department.OwnershipPercentage = offeringDept.ResponsibilityPercentage;
                            departments.Add(department);
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                    }

                }
                sectionDto.OwningInstitutionUnits = departments;
            }

            if (entity.NumberOfWeeks.HasValue)
            {
                sectionDto.Duration = new SectionDuration2()
                {
                    Length = entity.NumberOfWeeks.Value,
                    Unit = DurationUnit2.Weeks
                };
            }

            if (entity.CensusDates != null && entity.CensusDates.Count() > 0)
            {
                sectionDto.CensusDates = entity.CensusDates;
            }
            else if (filterDefaultCensusDates == false && !String.IsNullOrEmpty(entity.TermId))
            {
                // If the section has a term, and if there are no census dates in the section, for EEDM output we will check
                // TERMS.LOCATIONS for census dates.  If none there, we will check TERMS.  Most of this logic is already in the
                // Terms repository, and the results are in the regstration date overrides for the term and keyed by location.


                var secTerm = entity.TermId;
                var secLoc = entity.Location;

                try
                {
                    var terms = await AllTermsAsync();
                    Domain.Student.Entities.Term term = terms.FirstOrDefault(t => t.Code == secTerm);
                    if (term != null)
                    {
                        var termOverrides = term.RegistrationDates.Where(trd => trd.Location == "");
                        if (!string.IsNullOrWhiteSpace(secLoc))
                        {
                            var termLocOverrides = term.RegistrationDates.Where(trd => trd.Location == secLoc);
                            if (termLocOverrides != null && termLocOverrides.Count() > 0)
                            {
                                if (termLocOverrides.FirstOrDefault() != null && termLocOverrides.FirstOrDefault().CensusDates != null && termLocOverrides.FirstOrDefault().CensusDates.Count() > 0)
                                {
                                    if (sectionDto.CensusDates == null)
                                    {
                                        sectionDto.CensusDates = new List<DateTime?>(); //default
                                    }
                                    var item = termLocOverrides.FirstOrDefault();
                                    sectionDto.CensusDates = item != null && item.CensusDates != null && item.CensusDates.Any() ? item.CensusDates : null;
                                }
                            }
                        }
                        if (sectionDto.CensusDates == null || sectionDto.CensusDates.Count() <= 0)
                        {
                            if (termOverrides != null && termOverrides.Count() > 0 && termOverrides.FirstOrDefault() != null && termOverrides.FirstOrDefault().CensusDates != null &&
                                termOverrides.FirstOrDefault().CensusDates.Count() > 0)
                            {
                                if (termOverrides.FirstOrDefault().CensusDates != null)
                                {
                                    if (sectionDto.CensusDates == null)
                                    {
                                        sectionDto.CensusDates = new List<DateTime?>(); //default
                                    }
                                    var item = termOverrides.FirstOrDefault();
                                    sectionDto.CensusDates = item != null && item.CensusDates != null && item.CensusDates.Any() ? termOverrides.FirstOrDefault().CensusDates : null;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError("Validation table terms is missing from the database.", "Record.Not.Found", entity.Guid, entity.Id);
                }
            }

            sectionDto.Billing = entity.BillingCred;

            if (entity.CourseTypeCodes != null && entity.CourseTypeCodes.Any())
            {
                sectionDto.CourseCategories = new List<GuidObject2>();
                foreach (var cat in entity.CourseTypeCodes)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(cat))
                        {
                            var catGuid = await _studentReferenceDataRepository.GetCourseTypeGuidAsync(cat);
                            sectionDto.CourseCategories.Add(new GuidObject2(catGuid));
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                    }
                }
            }

            // New object InstructionalMethods with v16

            if (entity.InstructionalContacts != null && entity.InstructionalContacts.Any())
            {
                sectionDto.InstructionalMethods = new List<GuidObject2>();
                var instructionalMethodDetails = new List<InstructionalMethodDetailsDtoProperty>();
                var allInstructionalMethods = await InstructionalMethodsAsync(bypassCache);
                var allContactMeasures = await AllContactMeasuresAsync(bypassCache);
                foreach (var instructionalContact in entity.InstructionalContacts)
                {
                    ContactHoursPeriod? instrMethodPeriod = ContactHoursPeriod.NotSet;
                    decimal? instrMethodHours = null;

                    var instructionalMethod = allInstructionalMethods.FirstOrDefault(x => x.Code == instructionalContact.InstructionalMethodCode);
                    if (instructionalMethod == null || string.IsNullOrEmpty(instructionalMethod.Guid))
                    {
                        IntegrationApiExceptionAddError(string.Format("No Guid found, Entity:'INSTR.METHODS', Record ID:'{0}'", instructionalContact.InstructionalMethodCode), "GUID.Not.Found", entity.Guid, entity.Id);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(instructionalContact.ContactMeasure))
                        {
                            var interval = allContactMeasures.FirstOrDefault(x => x.Code == instructionalContact.ContactMeasure);
                            if (interval == null)
                            {
                                IntegrationApiExceptionAddError(string.Format("Contact Measure not found for '{0}'.", instructionalContact.ContactMeasure), "Validation.Exception", entity.Guid, entity.Id);
                            }
                            else
                            {
                                instrMethodPeriod = ConvertContactPeriodToInterval(interval.ContactPeriod);
                            }
                        }
                        if ((instructionalContact.ContactHours.HasValue) || ((instrMethodPeriod != null) || (instrMethodPeriod != ContactHoursPeriod.NotSet)))
                        {
                            instrMethodHours = instructionalContact.ContactHours;
                        }
                        if (instrMethodHours != null && instrMethodHours.HasValue)
                        {
                            try
                            {
                                var adminInstructionalMethod = await _studentReferenceDataRepository.GetAdministrativeInstructionalMethodGuidAsync(instructionalContact.InstructionalMethodCode);
                                var instrMethodHoursDetail = new CoursesHoursDtoProperty()
                                {
                                    AdministrativeInstructionalMethod = new GuidObject2(adminInstructionalMethod),
                                    Minimum = instrMethodHours
                                };
                                if (instrMethodPeriod != ContactHoursPeriod.NotSet)
                                    instrMethodHoursDetail.Interval = instrMethodPeriod;
                                if (sectionDto.Hours == null)
                                {
                                    sectionDto.Hours = new List<CoursesHoursDtoProperty>();
                                }
                                sectionDto.Hours.Add(instrMethodHoursDetail);
                            }
                            catch (RepositoryException ex)
                            {
                                IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                            }
                        }
                    }
                }
            }
            if (entity.InstructionalMethods != null && entity.InstructionalMethods.Any())
            {
                //var allInstructionalMethods = await this.InstructionalMethodsAsync();
                foreach (var method in entity.InstructionalMethods)
                {
                    //var instructionalMethod = allInstructionalMethods.FirstOrDefault(x => x.Code == method); 
                    try
                    {
                        var instructionalMethod = await _studentReferenceDataRepository.GetInstructionalMethodGuidAsync(method);
                        var item = new GuidObject2(instructionalMethod);
                        if (!sectionDto.InstructionalMethods.Contains(item))
                            sectionDto.InstructionalMethods.Add(new GuidObject2(instructionalMethod));
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                    }
                }
            }

            //ChargeAssessmentMethod

            if (!string.IsNullOrEmpty(entity.BillingMethod))
            {
                var allChargeAssessmentMethods = await this.AllChargeAssessmentMethodAsync(bypassCache);
                if (allChargeAssessmentMethods != null)
                {
                    var chargeAssessmentMethod = allChargeAssessmentMethods.FirstOrDefault(x => x.Code == entity.BillingMethod);
                    if (chargeAssessmentMethod != null)
                    {
                        sectionDto.ChargeAssessmentMethod = new GuidObject2(chargeAssessmentMethod.Guid);
                    }
                }
            }
            sectionDto.CrossListed = ((entity.CrossListedSections != null) && (entity.CrossListedSections.Any()))
               ? CrossListed.CrossListed : CrossListed.NotCrossListed;

            sectionDto.Waitlist = new SectionWaitlistDtoProperty()
            {
                Eligible = SectionWaitlistEligible.NotEligible
            };

            if (entity.AllowWaitlist)
            {
                sectionDto.Waitlist.Eligible = SectionWaitlistEligible.Eligible;
                sectionDto.Waitlist.WaitlistMaximum = entity.WaitlistMaximum;
                if (entity.WaitListNumberOfDays != null && entity.WaitListNumberOfDays > 0)
                {
                    sectionDto.Waitlist.RegistrationInterval = new SectionRegistrationIntervalDtoProperty();
                    sectionDto.Waitlist.RegistrationInterval.Unit = SectionWaitlistRegistrationIntervalUnit.Day;
                    sectionDto.Waitlist.RegistrationInterval.Value = entity.WaitListNumberOfDays;
                }
            }

            // New property for alternate Ids
            sectionDto.AlternateIds = new List<SectionsAlternateIdsDtoProperty>()
            {
                new SectionsAlternateIdsDtoProperty()
                {
                    Title = "Source Key",
                    Value = entity.Id
                }
            };

            return sectionDto;
        }

        /// <summary>
        /// Converts a Contact period Entity enum to a ContactHoursPeriod DTO enum
        /// </summary>
        /// <returns></returns>
        private ContactHoursPeriod ConvertContactPeriodToInterval(ContactPeriod period)
        {
            switch (period)
            {
                case ContactPeriod.day:
                    return ContactHoursPeriod.Day;
                case ContactPeriod.month:
                    return ContactHoursPeriod.Month;
                case ContactPeriod.week:
                    return ContactHoursPeriod.Week;
                case ContactPeriod.term:
                    return ContactHoursPeriod.Term;
                default:
                    return ContactHoursPeriod.NotSet;
            }
        }

        /// <summary>
        /// Converts a IntervalTypes DTO enum to a ContactPeriod entity enum
        /// </summary>
        /// <returns></returns>
        private ContactPeriod ConvertIntervalToContactHours(IntervalTypes? interval) //(ContactPeriod period)
        {
            if (interval == null)
                return ContactPeriod.notSet;
            switch (interval)
            {
                case IntervalTypes.Day:
                    return ContactPeriod.day;
                case IntervalTypes.Month:
                    return ContactPeriod.month;
                case IntervalTypes.Term:
                    return ContactPeriod.term;
                case IntervalTypes.Week:
                    return ContactPeriod.week;
                default:
                    return ContactPeriod.notSet;
            }
        }

        private async Task<Domain.Student.Entities.Section> ConvertSectionDto6ToEntityAsync(Section6 sectionDto, bool bypassCache = false)
        {

            if (sectionDto == null)
            {
                IntegrationApiExceptionAddError("Must provide a section representation for update.", "Missing.Request.Body");
                throw IntegrationApiException;
            }
            if (string.IsNullOrEmpty(sectionDto.Id))
            {
                IntegrationApiExceptionAddError("Must provide a section ID for update.", "Missing.Request.ID");
            }

            Domain.Student.Entities.Section section = null;
            string sectionId = string.Empty;
            try
            {
                section = await _sectionRepository.GetSectionByGuidAsync(sectionDto.Id);
                sectionId = section != null && string.IsNullOrEmpty(section.Id) ? section.Id : string.Empty;
            }
            catch (Exception)
            {
            }

            //First, we need to check all required properties are present at the root level
            if (sectionDto.Course == null || string.IsNullOrEmpty(sectionDto.Course.Id))
            {
                IntegrationApiExceptionAddError("Course is required.", "Missing.Required.Property", sectionDto.Id, sectionId);
            }

            if (sectionDto.Titles == null || !sectionDto.Titles.Any())
            {
                IntegrationApiExceptionAddError("At least one section title must be provided.", "Validation.Exception", sectionDto.Id, sectionId);
            }

            if (sectionDto.StartOn == null || !sectionDto.StartOn.HasValue)
            {
                IntegrationApiExceptionAddError("The startOn is required.", "Missing.Required.Property", sectionDto.Id, sectionId);
            }

            if (sectionDto.InstructionalMethods == null || !sectionDto.InstructionalMethods.Any())
            {
                IntegrationApiExceptionAddError("Instructional method id is a required property.", "Missing.Required.Property", sectionDto.Id, sectionId);
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
            //Moved course up here and throw error since course is required and many of the course fields are needed to finish PUT/POST.
            Course course = null;
            try
            {
                course = await _courseRepository.GetCourseByGuidAsync(sectionDto.Course.Id);
            }
            catch (Exception)
            {
                IntegrationApiExceptionAddError("Course record is not found for the requested GUID.", "Record.Not.Found", sectionDto.Id, sectionId);
                throw IntegrationApiException;
            }

            if (sectionDto.Titles != null && sectionDto.Titles.Any())
            {
                foreach (var title in sectionDto.Titles)
                {
                    if (string.IsNullOrEmpty(title.Value))
                    {
                        IntegrationApiExceptionAddError("The title value must be provided for the title object.", "Missing.Required.Property", sectionDto.Id, sectionId);
                    }
                }
            }

            if (sectionDto.StartOn.HasValue && sectionDto.EndOn.HasValue)
            {
                if (sectionDto.StartOn.Value > sectionDto.EndOn.Value)
                {
                    IntegrationApiExceptionAddError("The endOn can not occur earlier than startOn.", "Validation.Exception", sectionDto.Id, sectionId);
                }
            }

            if (sectionDto.Billing.HasValue)
            {
                if (sectionDto.Billing < 0)
                {
                    IntegrationApiExceptionAddError("Billing can not be a negative number.", "Validation.Exception", sectionDto.Id, sectionId);
                }
            }
            if (sectionDto.Credits != null && sectionDto.Credits.Any())
            {
                foreach (var credit in sectionDto.Credits)
                {
                    if (credit.CreditCategory == null)
                    {
                        IntegrationApiExceptionAddError("Credit Category is required if Credits are in the message body.", "Missing.Required.Property", sectionDto.Id, sectionId);
                    }

                    if (credit.Measure == null)
                    {
                        IntegrationApiExceptionAddError("Section provided is not valid, Credits must contain a Measure.", "Missing.Required.Property", sectionDto.Id, sectionId);
                    }

                    if (credit.Minimum == null)
                    {
                        IntegrationApiExceptionAddError("Section provided is not valid, Credits must contain a Minimum.", "Missing.Required.Property", sectionDto.Id, sectionId);
                    }

                    if (credit.CreditCategory != null && credit.CreditCategory.CreditType == null)
                    {
                        IntegrationApiExceptionAddError("Credit Type is required for Credit Categories if Credits are in the message body.", "Missing.Required.Property", sectionDto.Id, sectionId);
                    }

                    if (credit.CreditCategory != null && credit.CreditCategory.Detail != null && string.IsNullOrEmpty(credit.CreditCategory.Detail.Id))
                    {
                        IntegrationApiExceptionAddError("Credit Category id is required within the detail section of Credit Category if it is in the message body.", "Missing.Required.Property", sectionDto.Id, sectionId);
                    }

                    if (!credit.Maximum.HasValue && credit.Increment.HasValue)
                    {
                        IntegrationApiExceptionAddError("Credit Maximum is required when Credit Increment exists.", "Validation.Exception", sectionDto.Id, sectionId);
                    }
                    if (credit.Maximum.HasValue && credit.Measure == CreditMeasure2.CEU)
                    {
                        IntegrationApiExceptionAddError("Credit Maximum cannot exist when Credit Measure is 'ceu'.", "Validation.Exception", sectionDto.Id, sectionId);
                    }
                    if (credit.Increment.HasValue && credit.Measure == CreditMeasure2.CEU)
                    {
                        IntegrationApiExceptionAddError("Credit Increment cannot exist when Credit Measure is 'ceu'.", "Validation.Exception", sectionDto.Id, sectionId);
                    }
                }
            }

            if (sectionDto.CourseLevels != null && sectionDto.CourseLevels.Any())
            {
                foreach (var level in sectionDto.CourseLevels)
                {
                    if (string.IsNullOrEmpty(level.Id))
                    {
                        IntegrationApiExceptionAddError("Course Level id is a required field when Course Levels are in the message body.", "Missing.Required.Property", sectionDto.Id, sectionId);
                    }
                }
            }

            if (sectionDto.InstructionalPlatform != null)
            {
                if (string.IsNullOrEmpty(sectionDto.InstructionalPlatform.Id))
                {
                    IntegrationApiExceptionAddError("Instructional Platform id is a required field when Instructional Methods are in the message body.", "Missing.Required.Property", sectionDto.Id, sectionId);
                }
            }

            if (sectionDto.AcademicLevels != null && sectionDto.AcademicLevels.Any())
            {
                foreach (var level in sectionDto.AcademicLevels)
                {
                    if (string.IsNullOrEmpty(level.Id))
                    {
                        IntegrationApiExceptionAddError("Academic Level id is a required field when Academic Levels are in the message body.", "Missing.Required.Property", sectionDto.Id, sectionId);
                    }
                }
            }

            if (sectionDto.GradeSchemes != null && sectionDto.GradeSchemes.Any())
            {
                foreach (var scheme in sectionDto.GradeSchemes)
                {
                    if (string.IsNullOrEmpty(scheme.Id))
                    {
                        IntegrationApiExceptionAddError("Grade Scheme id is a required field when Grade Schemes are in the message body.", "Missing.Required.Property", sectionDto.Id, sectionId);
                    }
                }
            }

            if (sectionDto.OwningInstitutionUnits != null && sectionDto.OwningInstitutionUnits.Any())
            {
                foreach (var org in sectionDto.OwningInstitutionUnits)
                {
                    if (org.InstitutionUnit == null || string.IsNullOrEmpty(org.InstitutionUnit.Id))
                    {
                        IntegrationApiExceptionAddError("Institution Unit id is a required field when Owning Organizations are in the message body.", "Missing.Required.Property", sectionDto.Id, sectionId);
                    }
                }
            }

            if (sectionDto.Duration != null && (sectionDto.Duration.Unit == DurationUnit2.Months || sectionDto.Duration.Unit == DurationUnit2.Years || sectionDto.Duration.Unit == DurationUnit2.Days))
            {
                IntegrationApiExceptionAddError("Section Duration Unit is not allowed to be set to Days, Months or Years.", "Validation.Exception", sectionDto.Id, sectionId);
            }

            if (sectionDto.Duration != null && sectionDto.Duration.Unit == DurationUnit2.Weeks && sectionDto.Duration.Length < 0)
            {
                IntegrationApiExceptionAddError("Section Duration Length must be a positive number.", "Validation.Exception", sectionDto.Id, sectionId);
            }

            if (sectionDto.Duration != null && sectionDto.Duration.Unit == null)
            {
                IntegrationApiExceptionAddError("Section Duration Unit must contain a value if the unit property is specified.", "Validation.Exception", sectionDto.Id, sectionId);
            }

            if (sectionDto.Credits != null && sectionDto.Credits.Count() > 2)
            {
                IntegrationApiExceptionAddError("A maximum of 2 entries are allowed in the Credits array.", "Validation.Exception", sectionDto.Id, sectionId);
            }
            if (sectionDto.Credits != null && sectionDto.Credits.Count() == 2)
            {
                if (sectionDto.Credits.ElementAt(0).CreditCategory != null && sectionDto.Credits.ElementAt(0).CreditCategory.CreditType.HasValue &&
                    sectionDto.Credits.ElementAt(1).CreditCategory != null && sectionDto.Credits.ElementAt(1).CreditCategory.CreditType.HasValue &&
                    sectionDto.Credits.ElementAt(0).CreditCategory.CreditType != sectionDto.Credits.ElementAt(1).CreditCategory.CreditType)
                {
                    IntegrationApiExceptionAddError("The same Credit Type must be used for each entry in the Credits array.", "Validation.Exception", sectionDto.Id, sectionId);
                }
                if (!(sectionDto.Credits.ElementAt(0).Measure == CreditMeasure2.CEU && sectionDto.Credits.ElementAt(1).Measure == CreditMeasure2.Credit)
                    && !(sectionDto.Credits.ElementAt(0).Measure == CreditMeasure2.Credit && sectionDto.Credits.ElementAt(1).Measure == CreditMeasure2.CEU)
                    && !(sectionDto.Credits.ElementAt(0).Measure == CreditMeasure2.CEU && sectionDto.Credits.ElementAt(1).Measure == CreditMeasure2.Hours)
                    && !(sectionDto.Credits.ElementAt(0).Measure == CreditMeasure2.Hours && sectionDto.Credits.ElementAt(1).Measure == CreditMeasure2.CEU))
                {
                    var errMsg = string.Format("Invalid combination of measures '{0}' and '{1}'", sectionDto.Credits.ElementAt(0).Measure, sectionDto.Credits.ElementAt(1).Measure);
                    IntegrationApiExceptionAddError(errMsg, "Validation.Exception", sectionDto.Id, sectionId);
                }
            }

            if (sectionDto.ReservedSeatsMaximum != null)
            {
                IntegrationApiExceptionAddError("Maximum reserved seats cannot be set through the sections endpoint. Use section-crosslists endpoint instead.", "Validation.Exception", sectionDto.Id, sectionId);
            }

            // Waitlist
            if (sectionDto.Waitlist != null)
            {
                if (sectionDto.Waitlist.Eligible == SectionWaitlistEligible.Eligible)
                {
                    if (sectionDto.Waitlist.RegistrationInterval != null)
                    {
                        if (sectionDto.Waitlist.RegistrationInterval.Unit == SectionWaitlistRegistrationIntervalUnit.Hour)
                        {
                            IntegrationApiExceptionAddError("Colleague only allows registration intervals in days. Hours cannot be accepted.", "Validation.Exception", sectionDto.Id, sectionId);
                        }
                        if (sectionDto.Waitlist.RegistrationInterval.Unit == null)
                        {
                            IntegrationApiExceptionAddError("The units must be specified when definining waitlist registrationInterval.", "Validation.Exception", sectionDto.Id, sectionId);
                        }
                        if (sectionDto.Waitlist.RegistrationInterval.Value == null)
                        {
                            IntegrationApiExceptionAddError("The value must be specified when defining waitlist registrationInterval.", "Validation.Exception", sectionDto.Id, sectionId);
                        }
                    }
                }
                else
                {
                    if (sectionDto.Waitlist.RegistrationInterval != null)
                    {
                        IntegrationApiExceptionAddError("Waitlist registrationInterval cannot be updated because the section is not eligible for waitlisting.", "Validation.Exception", sectionDto.Id, sectionId);
                    }
                    if (sectionDto.Waitlist.WaitlistMaximum != null)
                    {
                        IntegrationApiExceptionAddError("WaitlistMaximum cannot be updated because the section is not eligible for waitlisting.", "Validation.Exception", sectionDto.Id, sectionId);
                    }
                }
            }

            SectionStatusIntegration? newStatusFromDetail = null;

            if (sectionDto.Status != null && sectionDto.Status.Detail != null)
            {
                var allStatuses = await _studentReferenceDataRepository.GetSectionStatusesAsync(bypassCache);
                try
                {
                    newStatusFromDetail = allStatuses.Single(st => st.Guid == sectionDto.Status.Detail.Id).SectionStatusIntg;
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError(string.Format("Section status detail guid '{0}' is not valid.", sectionDto.Status.Detail.Id), "Validation.Exception", sectionDto.Id, sectionId);
                }
            }

            string id = section == null ? null : section.Id;
            var currentStatuses = section == null || section.Statuses == null
                ? new List<SectionStatusItem>()
                : section.Statuses.ToList();
            //var curriculumConfiguration = await GetCurriculumConfiguration2Async();

            List<SectionStatusItem> statuses = new List<SectionStatusItem>();
            if (sectionDto.Status != null)
            {
                try
                {
                    if (newStatusFromDetail == null)
                    {
                        statuses = await UpdateSectionStatus3(currentStatuses, sectionDto.Status.Category);
                    }
                    else
                    {
                        statuses = await UpdateSectionStatus4(currentStatuses, sectionDto.Status);
                    }
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "Validation.Exception", sectionDto.Id, sectionId);
                }
            }

            // Set the credit type and credits/CEUs for the course based on the supplied GUID, or using the ERP default 
            Ellucian.Colleague.Domain.Student.Entities.CreditCategory creditCategory = null;
            if (sectionDto.Credits != null &&
                sectionDto.Credits.Any() &&
                sectionDto.Credits.ToList()[0].CreditCategory != null &&
                sectionDto.Credits.ToList()[0].CreditCategory.Detail != null &&
                !string.IsNullOrEmpty(sectionDto.Credits.ToList()[0].CreditCategory.Detail.Id))
            {
                var creditTypes = await CreditTypesAsync();
                if (creditTypes != null && creditTypes.Any())
                {
                    creditCategory = creditTypes.FirstOrDefault(ct => ct.Guid == sectionDto.Credits.ToList()[0].CreditCategory.Detail.Id);
                    if (creditCategory == null)
                    {
                        IntegrationApiExceptionAddError(string.Format("Invalid Id '{0}' supplied for creditCategory", sectionDto.Credits.ToList()[0].CreditCategory.Detail.Id), "Validation.Exception", sectionDto.Id, sectionId);
                    }
                }
            }
            // If we don't have a GUID then check for a CreditType enumeration value
            if (creditCategory == null &&
                sectionDto.Credits != null &&
                sectionDto.Credits.Any() &&
                sectionDto.Credits.ToList()[0].CreditCategory != null &&
                sectionDto.Credits.ToList()[0].CreditCategory.CreditType != null)
            {
                if (sectionDto.Credits.ToList()[0].CreditCategory.CreditType == CreditCategoryType3.Exam ||
                    sectionDto.Credits.ToList()[0].CreditCategory.CreditType == CreditCategoryType3.WorkLifeExperience)
                {
                    IntegrationApiExceptionAddError("Credit category type 'exam' or 'workLifeExperience' are not supported.", "Validation.Exception", sectionDto.Id, sectionId);
                }

                // Find the credit category that matches the enumeration
                switch (sectionDto.Credits.ToList()[0].CreditCategory.CreditType)
                {
                    case (CreditCategoryType3.ContinuingEducation):
                        creditCategory = (await CreditTypesAsync()).FirstOrDefault(cc => cc.CreditType == CreditType.ContinuingEducation);
                        break;
                    case (CreditCategoryType3.Institutional):
                        creditCategory = (await CreditTypesAsync()).FirstOrDefault(cc => cc.CreditType == CreditType.Institutional);
                        break;
                    case (CreditCategoryType3.Transfer):
                        creditCategory = (await CreditTypesAsync()).FirstOrDefault(cc => cc.CreditType == CreditType.Transfer);
                        break;
                    case (CreditCategoryType3.Exchange):
                        creditCategory = (await CreditTypesAsync()).FirstOrDefault(cc => cc.CreditType == CreditType.Exchange);
                        break;
                    case (CreditCategoryType3.Other):
                        creditCategory = (await CreditTypesAsync()).FirstOrDefault(cc => cc.CreditType == CreditType.Other);
                        break;
                    case (CreditCategoryType3.NoCredit):
                        creditCategory = (await CreditTypesAsync()).FirstOrDefault(cc => cc.CreditType == CreditType.None);
                        break;
                }
            }

            var creditTypeCode = creditCategory == null ? string.Empty : creditCategory.Code;

            decimal? minCredits;
            decimal? ceus;
            decimal? maxCredits;
            decimal? varIncrCredits;
            minCredits = null;
            maxCredits = null;
            varIncrCredits = null;
            ceus = null;
            if (sectionDto.Credits != null && sectionDto.Credits.Any())
            {
                foreach (var credits in sectionDto.Credits)
                {
                    var creditInfo = (sectionDto.Credits == null || sectionDto.Credits.Count == 0) ? null : credits;
                    var measure = creditInfo == null ? null : creditInfo.Measure;
                    if (measure == Dtos.CreditMeasure2.CEU)
                    {
                        ceus = creditInfo == null ? 0 : creditInfo.Minimum;
                    }
                    else
                    {
                        minCredits = creditInfo == null ? 0 : creditInfo.Minimum;
                        maxCredits = creditInfo == null ? null : creditInfo.Maximum;
                        varIncrCredits = creditInfo == null ? null : creditInfo.Increment;
                    }
                }
            }

            if (sectionDto.Credits == null || !sectionDto.Credits.Any())
            {
                if (course != null)
                {
                    minCredits = course.MinimumCredits;
                    ceus = course.Ceus;
                    creditTypeCode = course.LocalCreditType;
                }
            }

            // Set the list of departments/shares
            List<OfferingDepartment> offeringDepartments = new List<OfferingDepartment>();

            // If we have supplied owning organization, then use that first.
            if (sectionDto.OwningInstitutionUnits != null && sectionDto.OwningInstitutionUnits.Any())
            {
                foreach (var owningInstitutionUnit in sectionDto.OwningInstitutionUnits)
                {
                    var divisions = await _referenceDataRepository.GetDivisionsAsync(bypassCache);
                    if (divisions != null && divisions.Any())
                    {
                        var division = divisions.FirstOrDefault(div => div.Guid.Equals(owningInstitutionUnit.InstitutionUnit.Id, StringComparison.OrdinalIgnoreCase));
                        if (division != null)
                        {
                            IntegrationApiExceptionAddError("Owning institution unit of type 'division' is not supported.", "Validation.Exception", sectionDto.Id, sectionId);
                        }
                    }
                    var schools = await _referenceDataRepository.GetSchools2Async(bypassCache);
                    if (schools != null && schools.Any())
                    {

                        var school = schools.FirstOrDefault(div => div.Guid.Equals(owningInstitutionUnit.InstitutionUnit.Id, StringComparison.OrdinalIgnoreCase));
                        if (school != null)
                        {
                            IntegrationApiExceptionAddError("Owning institution unit of type 'school' is not supported.", "Validation.Exception", sectionDto.Id, sectionId);
                        }
                    }

                    var departments = await DepartmentsAsync();
                    if (departments != null && departments.Any())
                    {
                        var department = departments.Where(d => d.Guid == owningInstitutionUnit.InstitutionUnit.Id).FirstOrDefault();

                        if (department != null)
                        {
                            var acadDepartments = await _studentReferenceDataRepository.GetAcademicDepartmentsAsync();
                            var academicDepartment = acadDepartments.FirstOrDefault(ad => ad.Code == department.Code);
                            if (academicDepartment != null)
                            {
                                try
                                {
                                    offeringDepartments.Add(new OfferingDepartment(academicDepartment.Code, owningInstitutionUnit.OwnershipPercentage));
                                }
                                catch (Exception ex)
                                {
                                    IntegrationApiExceptionAddError(ex.Message, "Missing.Required.Property", sectionDto.Id, sectionId);
                                }
                            }
                        }
                    }
                }
            }
            // If we don't have offering departments in the payload and didn't find
            // any departments, we will default from the course.  This has to be done
            // here instead of the Colleague transaction because the Sections Entity
            // requires at least one department.
            if (offeringDepartments.Count == 0)
            {
                offeringDepartments.AddRange(course.Departments);
            }

            var courseLevels = await CourseLevelsAsync();
            // Convert various codes to their Colleague values or get them from the course
            List<string> courseLevelCodes = new List<string>();
            if (sectionDto.CourseLevels == null || sectionDto.CourseLevels.Count == 0)
            {
                courseLevelCodes = course.CourseLevelCodes;
            }
            else
            {
                foreach (var level in sectionDto.CourseLevels)
                {
                    try
                    {
                        var courseLevelCode = ConvertGuidToCode(courseLevels, level.Id);
                        if (string.IsNullOrEmpty(courseLevelCode))
                        {
                            IntegrationApiExceptionAddError(string.Format("Invalid Id '{0}' supplied for courseLevels.", level.Id), "Validation.Exception", sectionDto.Id, sectionId);
                        }
                        else
                        {
                            courseLevelCodes.Add(courseLevelCode);
                        }
                    }
                    catch (Exception)
                    {
                        IntegrationApiExceptionAddError("Validation table course levels is missing from the database.", "Validation.Exception", sectionDto.Id, sectionId);
                    }
                }
            }

            string academicLevel = course.AcademicLevelCode;
            if (sectionDto.AcademicLevels != null && sectionDto.AcademicLevels.Any())
            {
                try
                {
                    var acadLevels = await AcademicLevelsAsync();
                    var tempAcadCode = ConvertGuidToCode(acadLevels, sectionDto.AcademicLevels[0].Id);
                    if (!string.IsNullOrEmpty(tempAcadCode))
                    {
                        academicLevel = tempAcadCode;
                    }
                }
                catch (ArgumentNullException)
                {
                    IntegrationApiExceptionAddError("Validation table academic levels is missing from the database.", "Validation.Exception", sectionDto.Id, sectionId);
                }
            }

            string gradeScheme = null;
            if (sectionDto.GradeSchemes == null || sectionDto.GradeSchemes.Count == 0)
            {
                gradeScheme = course.GradeSchemeCode;
            }
            else
            {
                var gradeSchemes = await GradeSchemesAsync();
                if (gradeSchemes != null && gradeSchemes.Any())
                {
                    var gradeSchemeEntity = gradeSchemes.Where(gs => gs != null && gs.Guid == sectionDto.GradeSchemes[0].Id).FirstOrDefault();
                    var schemeCode = gradeSchemeEntity != null ? gradeSchemeEntity.Code : null;
                    if (string.IsNullOrEmpty(schemeCode))
                    {
                        IntegrationApiExceptionAddError(string.Format("Invalid id '{0}' supplied for gradeSchemes.", sectionDto.GradeSchemes[0].Id), "Validation.Exception", sectionDto.Id, sectionId);
                    }
                    else
                    {
                        gradeScheme = schemeCode;
                    }
                }
            }

            string learningProvider = null;
            if (sectionDto.InstructionalPlatform != null)
            {
                try
                {
                    var instrPlatforms = await InstructionalPlatformsAsync();
                    var providerCode = ConvertGuidToCode(instrPlatforms, sectionDto.InstructionalPlatform.Id);
                    if (string.IsNullOrEmpty(providerCode))
                    {                        
                        IntegrationApiExceptionAddError(string.Format("Invalid GUID '{0}' supplied for instructionalPlatform.", sectionDto.InstructionalPlatform.Id), "Record.Not.Found", sectionDto.Id, sectionId);
                    }
                    else
                    {
                        learningProvider = providerCode;
                    }
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError("Validation table instructional platforms is missing from the database.", "Record.Not.Found", sectionDto.Id, sectionId);
                }
            }

            string term = string.Empty;
            try
            {
                if (sectionDto.AcademicPeriod != null && !string.IsNullOrEmpty(sectionDto.AcademicPeriod.Id))
                {
                    term = ConvertGuidToCode(academicPeriods, sectionDto.AcademicPeriod.Id);
                    if (string.IsNullOrEmpty(term))
                    {
                        IntegrationApiExceptionAddError(string.Format("Invalid guid '{0}' supplied for academicPeriod.", sectionDto.AcademicPeriod.Id), "Validation.Exception", sectionDto.Id, sectionId);
                    }
                }
            }
            catch (Exception)
            {
                IntegrationApiExceptionAddError("Validation table academic periods is missing from the database.", "Validation.Exception", sectionDto.Id, sectionId);
            }

            if (sectionDto.AcademicPeriod != null && !string.IsNullOrEmpty(sectionDto.AcademicPeriod.Id))
            {
                var academicPeriod = academicPeriods.FirstOrDefault(a => a.Guid == sectionDto.AcademicPeriod.Id);
                // Reporting Term Academic Period
                try
                {
                    string reportingTerm = (sectionDto.ReportingAcademicPeriod == null || string.IsNullOrEmpty(sectionDto.ReportingAcademicPeriod.Id)) ? null :
                        ConvertGuidToCode(academicPeriods, sectionDto.ReportingAcademicPeriod.Id);
                    if (academicPeriod != null && !string.IsNullOrEmpty(reportingTerm) && reportingTerm != academicPeriod.ReportingTerm)
                    {
                        IntegrationApiExceptionAddError(string.Format("The reportingAcademicPeriod.id '{0}' does not match the reportingAcademicPeriod for academicPeriod.id '{1}'", sectionDto.ReportingAcademicPeriod.Id, sectionDto.AcademicPeriod.Id),
                            "Validation.Exception", sectionDto.Id, sectionId);
                    }
                    if (string.IsNullOrEmpty(reportingTerm) && sectionDto.ReportingAcademicPeriod != null && !string.IsNullOrEmpty(sectionDto.ReportingAcademicPeriod.Id))
                    {
                        IntegrationApiExceptionAddError(string.Format("The reportingAcademicPeriod.id '{0}' is invalid.", sectionDto.ReportingAcademicPeriod.Id), "Validation.Exception", sectionDto.Id, sectionId);
                    }
                }
                catch (ArgumentNullException)
                {
                    IntegrationApiExceptionAddError("Validation table academic periods is missing from the database.", "Validation.Exception", sectionDto.Id, sectionId);
                }
            }
            else
            {
                if (sectionDto.ReportingAcademicPeriod != null && !string.IsNullOrEmpty(sectionDto.ReportingAcademicPeriod.Id))
                {
                    IntegrationApiExceptionAddError("The reportingAcademicPeriod can only be defined through the academicPeriod.id.", "Validation.Exception", sectionDto.Id, sectionId);
                }
            }

            string site = null;
            if (sectionDto.Site != null)
            {
                try
                {
                    var tempLocationCode = ConvertGuidToCode(locations, sectionDto.Site.Id);
                    if (string.IsNullOrEmpty(tempLocationCode))
                    {
                        IntegrationApiExceptionAddError(string.Format("Invalid GUID '{0}' supplied for site.", sectionDto.Site.Id), "Record.Not.Found", sectionDto.Id, sectionId);
                    }
                    else
                    {
                        site = tempLocationCode;
                    }                    
                }            
                catch (ArgumentNullException)
                {
                    IntegrationApiExceptionAddError("Validation table locations is missing from the database.", "Validation.Exception", sectionDto.Id, sectionId);
                }
            }

            // Because census dates can be defaulted in from TERMS or TERMS.LOCATIONS when the consumer GETS them,
            // if the consumer PUTs or POSTs a section with census dates, we have to make sure they are intended to
            // be overridden at the section level, rather than blindly writing inherited defaults back to the section.

            if (sectionDto.CensusDates != null && sectionDto.CensusDates.Count() > 0)
            {

                Domain.Student.Entities.Term secTerm = null;
                List<DateTime?> secTermOverrides = null;
                List<DateTime?> secTermLocOverrides = null;

                // Check for term Census dates overrides

                if (term != null)
                {
                    var terms = await AllTermsAsync();
                    if (terms != null && terms.Any())
                    {
                        var secTerms = terms.Where(t => t.Code == term);
                        if (secTerms != null && secTerms.Count() == 1)
                        {
                            secTerm = secTerms.FirstOrDefault();
                            if (secTerm != null && secTerm.RegistrationDates != null && secTerm.RegistrationDates.Where(srd => string.IsNullOrEmpty(srd.Location)).Count() == 1)
                            {
                                secTermOverrides = secTerm.RegistrationDates.FirstOrDefault(srd => string.IsNullOrEmpty(srd.Location)).CensusDates;
                            }
                        }
                    }
                }

                // If we have a term, now check for a location to see if it has its own term-location overrides
                try
                {
                    if (term != null && site != null && secTerm != null)
                    {
                        var key = term + "*" + site;
                        var termLocRegDates = secTerm.RegistrationDates.Where(trd => trd.Location == site);
                        if (termLocRegDates != null && termLocRegDates.Count() > 0)
                        {
                            secTermLocOverrides = termLocRegDates.FirstOrDefault().CensusDates;
                        }
                    }
                }
                catch (ArgumentNullException)
                {
                    IntegrationApiExceptionAddError("Validation table locations is missing from the database.", "Validation.Exception", sectionDto.Id, sectionId);
                }

                // Now compare the lists of dates (if any) and decide if the incoming dates match
                // existing overrides - which indicates they probably were defaulted in on the GET, 
                // and should not be written to the section record on the PUT.

                if (secTermLocOverrides != null && sectionDto.CensusDates != null && sectionDto.CensusDates.Any())
                {
                    if (sectionDto.CensusDates.SequenceEqual(secTermLocOverrides))
                    {
                        // incoming census dates match the term-location overrides, do not update the section overrides.
                        sectionDto.CensusDates = null;
                    }
                }
                else
                {
                    if (sectionDto.CensusDates != null && secTermOverrides != null)
                    {
                        if (sectionDto.CensusDates.SequenceEqual(secTermOverrides))
                        {
                            // incoming census dates match the term overrides, do not update the section overrides.
                            sectionDto.CensusDates = null;
                        }
                    }
                }
            }
            // Determine both short and long titles
            string shortTitle = "";
            if (sectionDto.Titles != null && sectionDto.Titles.Any())
            {
                var sectTitleTypes = await SectionTitleTypesAsync(bypassCache);
                if (sectTitleTypes != null && sectTitleTypes.Any())
                {
                    var shortEntity = sectTitleTypes.FirstOrDefault(ctt => ctt.Code.ToLower() == "short");
                    if (shortEntity == null || string.IsNullOrEmpty(shortEntity.Guid))
                    {
                        IntegrationApiExceptionAddError("The record 'SHORT' or it's GUID is missing from section title types.", "Validation.Exception", sectionDto.Id, sectionId);
                    }
                    foreach (var title in sectionDto.Titles)
                    {
                        if (title.Type != null && !string.IsNullOrEmpty(title.Type.Id) && !string.IsNullOrEmpty(title.Value))
                        {
                            if (title.Type.Id.Equals(shortEntity.Guid, StringComparison.OrdinalIgnoreCase)) shortTitle = title.Value;
                        }
                        if (!string.IsNullOrEmpty(title.Value))
                        {
                            shortTitle = title.Value;
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(shortTitle))
            {
                IntegrationApiExceptionAddError("Section titles must have a short title defined.", "Validation.Exception", sectionDto.Id, sectionId);
            }
            bool allowWaitlist = course.AllowWaitlist;
            if (sectionDto.Waitlist != null)
            {
                switch (sectionDto.Waitlist.Eligible)
                {
                    case SectionWaitlistEligible.Eligible:
                        allowWaitlist = true;
                        break;
                    case SectionWaitlistEligible.NotEligible:
                        allowWaitlist = false;
                        break;
                    default:
                        allowWaitlist = course.AllowWaitlist;
                        break;
                }
            }

            // Create the section entity
            Domain.Student.Entities.Section entity = null;
            try
            {
                entity = new Domain.Student.Entities.Section(id, course.Id, sectionDto.Number, sectionDto.StartOn.GetValueOrDefault(),
                    minCredits, ceus, shortTitle, creditTypeCode, offeringDepartments, courseLevelCodes, academicLevel, statuses,
                    course.AllowPassNoPass, course.AllowAudit, course.OnlyPassNoPass, allowWaitlist, false)
                {
                    Guid = (sectionDto.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase)) ? string.Empty : sectionDto.Id.ToLowerInvariant(),
                    EndDate = sectionDto.EndOn == null ? default(DateTime?) : sectionDto.EndOn.GetValueOrDefault(),
                    MaximumCredits = maxCredits,
                    VariableCreditIncrement = varIncrCredits,
                    GradeSchemeCode = gradeScheme,
                    TopicCode = course.TopicCode,
                    GlobalCapacity = sectionDto.MaximumEnrollment,
                    SectionCapacity = sectionDto.MaximumEnrollment,
                    Name = sectionDto.Code,
                    LearningProvider = learningProvider,
                    TermId = term,
                    Location = site,
                    CensusDates = sectionDto.CensusDates,
                };
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Missing.Required.Property", sectionDto.Id, sectionId);
                throw IntegrationApiException;
            }
            entity.BillingCred = sectionDto.Billing;

            // Contact methods, period, hours

            List<string> contactMethodCodes = null;
            List<string> contactPeriods = null;
            List<decimal?> contactHours = null;
            List<string> DuplicateContactMethodCodes = new List<string>();

            var allContactMeasures = await this.AllContactMeasuresAsync(bypassCache);
            var allInstructionalMethods = await this.InstructionalMethodsAsync(bypassCache);
            var allAdministrativeInstructionalMethods = await AdministrativeInstructionalMethodsAsync(bypassCache);

            if (sectionDto.Hours != null && sectionDto.Hours.Any())
            {
                if (contactMethodCodes == null)
                {
                    contactMethodCodes = new List<string>();
                    contactPeriods = new List<string>();
                    contactHours = new List<decimal?>();
                }
                foreach (var mthd in sectionDto.Hours)
                {
                    if (mthd.AdministrativeInstructionalMethod == null || string.IsNullOrEmpty(mthd.AdministrativeInstructionalMethod.Id))
                    {
                        IntegrationApiExceptionAddError("Hours object requires an administrative instructional method Id.", "Validation.Exception", sectionDto.Id, sectionId);
                    }
                    else
                    {
                        var method = allAdministrativeInstructionalMethods.FirstOrDefault(im => im.Guid.Equals(mthd.AdministrativeInstructionalMethod.Id, StringComparison.OrdinalIgnoreCase));
                        if (method != null)
                        {
                            contactMethodCodes.Add(method.Code);
                            if (DuplicateContactMethodCodes.Contains(method.Code))
                            {
                                IntegrationApiExceptionAddError("Duplicate administrative instructional methods are not permitted.", "Validation.Exception", sectionDto.Id, sectionId);
                            }
                            DuplicateContactMethodCodes.Add(method.Code);
                        }
                        else
                        {
                            IntegrationApiExceptionAddError(string.Format("Administrative Instructional method GUID '{0}' could not be found.", mthd.AdministrativeInstructionalMethod.Id),
                                "Validation.Exception", sectionDto.Id, sectionId);
                        }
                    }
                    if (mthd.Minimum.HasValue)
                    {
                        contactHours.Add(mthd.Minimum);

                        ContactMeasure contactPeriod = null;
                        if ((mthd.Interval != null) && (mthd.Interval != ContactHoursPeriod.NotSet))
                        {

                            contactPeriod = allContactMeasures.FirstOrDefault(cm => cm.ContactPeriod.ToString() == mthd.Interval.ToString().ToLowerInvariant());

                            if (contactPeriod == null)
                            {
                                IntegrationApiExceptionAddError(string.Format("Hours interval value '{0}' could not be matched to a contact measure.", mthd.Interval.ToString()), "Validation.Exception",
                                    sectionDto.Id, sectionId);
                            }
                            contactPeriods.Add(contactPeriod.Code);
                        }
                        else
                        {
                            contactPeriods.Add(null);
                        }
                        // Maximum and Increment are not published or consumed by Colleague per spec
                        // but if they were they'd go here.
                    }
                    else
                    {
                        IntegrationApiExceptionAddError("Hours object requires a minimum value.", "Validation.Exception", sectionDto.Id, sectionId);
                    }

                }
            }
            // Add in any missing instructional methods from instructional method details
            if (sectionDto.InstructionalMethods != null && sectionDto.InstructionalMethods.Any())
            {
                DuplicateContactMethodCodes = new List<string>();
                foreach (var mthd in sectionDto.InstructionalMethods)
                {
                    if (mthd != null && !string.IsNullOrEmpty(mthd.Id))
                    {
                        var method = allInstructionalMethods.FirstOrDefault(im => im.Guid.Equals(mthd.Id, StringComparison.OrdinalIgnoreCase));
                        if (method != null)
                        {
                            if (contactMethodCodes == null)
                            {
                                contactMethodCodes = new List<string>();
                            }
                            if (!contactMethodCodes.Contains(method.Code))
                            {
                                contactMethodCodes.Add(method.Code);
                            }
                            if (DuplicateContactMethodCodes.Contains(method.Code))
                            {
                                IntegrationApiExceptionAddError("Duplicate instructional methods are not permitted.", "Validation.Exception", sectionDto.Id, sectionId);
                            }
                            DuplicateContactMethodCodes.Add(method.Code);
                        }
                        else
                        {
                            IntegrationApiExceptionAddError(string.Format("Instructional method GUID '{0}' could not be found.", mthd.Id), "Validation.Exception", sectionDto.Id, sectionId);
                        }
                    }
                    else
                    {
                        IntegrationApiExceptionAddError("Instructional method id is a required property.", "Missing.Required.Property", sectionDto.Id, sectionId);
                    }
                }
            }

            // Add instructional method data if supplied
            if (contactMethodCodes != null)
            {
                int index = 0;
                foreach (var method in contactMethodCodes)
                {
                    string period = null;
                    decimal? hours = null;
                    if (contactPeriods != null && contactPeriods.Count > index)
                    {
                        period = contactPeriods.ElementAt(index);
                    }
                    if (contactHours != null && contactHours.Count > index)
                    {
                        hours = contactHours.ElementAt(index);
                    }

                    var instructionalContact = new Domain.Student.Entities.InstructionalContact(method)
                    {
                        ContactHours = hours,
                        ContactMeasure = period
                    };
                    entity.AddInstructionalContact(instructionalContact);
                    index++;
                }
            }

            if ((sectionDto.ChargeAssessmentMethod != null) && (!string.IsNullOrEmpty(sectionDto.ChargeAssessmentMethod.Id)))
            {
                var allChargeAssessmentMethods = await this.AllChargeAssessmentMethodAsync(bypassCache);
                if (allChargeAssessmentMethods != null)
                {
                    var chargeAssessmentMethod = allChargeAssessmentMethods.FirstOrDefault(x => x.Guid == sectionDto.ChargeAssessmentMethod.Id);
                    if (chargeAssessmentMethod == null)
                    {
                        IntegrationApiExceptionAddError(string.Format("Invalid guid '{0}' supplied for chargeAssessmentMethod.", sectionDto.ChargeAssessmentMethod.Id), "Validation.Exception", sectionDto.Id, sectionId);
                    }
                    entity.BillingMethod = chargeAssessmentMethod.Code;
                }
            }
            if (sectionDto.CrossListed != CrossListed.NotSet)
            {
                entity.IsCrossListedSection = (sectionDto.CrossListed == CrossListed.CrossListed)
                    ? true : false;
            }

            // Course Categories/types
            if (sectionDto.CourseCategories != null && sectionDto.CourseCategories.Count() > 0)
            {
                foreach (var cat in sectionDto.CourseCategories)
                {
                    var typeEntity = (await AllCourseTypes()).FirstOrDefault(cc => cc.Guid == cat.Id);
                    if (typeEntity != null)
                    {
                        entity.AddCourseType(typeEntity.Code);
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Format("Invalid Id '{0}' supplied for courseCategories.", cat.Id), "Validation.Exception", sectionDto.Id, sectionId);
                    }

                }
            }

            if (string.IsNullOrEmpty(entity.TermId))
            {
                entity.TermId = SectionProcessor.DetermineTerm(entity, await AllTermsAsync());
            }

            //check if duration is set on the incoming DTO
            if (sectionDto.Duration != null)
            {
                entity.NumberOfWeeks = sectionDto.Duration.Length;
            }
            else
            {
                if (entity.EndDate.HasValue)
                {
                    entity.NumberOfWeeks = (int)Math.Ceiling((entity.EndDate.Value - entity.StartDate).Days / 7m);
                }
            }

            entity.WaitlistMaximum = null;
            entity.WaitListNumberOfDays = null;
            if (sectionDto.Waitlist != null && allowWaitlist)
            {
                if (sectionDto.Waitlist.WaitlistMaximum != null)
                {
                    entity.WaitlistMaximum = sectionDto.Waitlist.WaitlistMaximum;
                }
                if (sectionDto.Waitlist.RegistrationInterval != null && sectionDto.Waitlist.RegistrationInterval.Value != null)
                {
                    entity.WaitListNumberOfDays = sectionDto.Waitlist.RegistrationInterval.Value;
                }
            }

            // Determine printed comments from section descriptions
            string printedComments = "";
            if (sectionDto.Descriptions != null && sectionDto.Descriptions.Any())
            {
                var printedCommentsEntity = (await SectionDescriptionTypesAsync(bypassCache)).FirstOrDefault(ctt => ctt.Code.ToLower() == "printed");
                if (printedCommentsEntity == null || string.IsNullOrEmpty(printedCommentsEntity.Guid))
                {
                    IntegrationApiExceptionAddError("The record 'PRINTED' or it's GUID is missing from section description types.", "Validation.Exception", sectionDto.Id, sectionId);
                }
                foreach (var description in sectionDto.Descriptions)
                {
                    if (description.Type != null && !string.IsNullOrEmpty(description.Type.Id) && !string.IsNullOrEmpty(description.Value))
                    {
                        if (description.Type.Id.Equals(printedCommentsEntity.Guid, StringComparison.OrdinalIgnoreCase))
                        {
                            printedComments = description.Value;
                        }
                        else
                        {
                            IntegrationApiExceptionAddError(string.Format("The GUID '{0}' is invalid for section description types. ", description.Type.Id), "Validation.Exception", sectionDto.Id, sectionId);
                        }
                    }
                    if (!string.IsNullOrEmpty(description.Value))
                    {
                        printedComments = description.Value;
                    }
                }
            }
            if (!string.IsNullOrEmpty(printedComments))
            {
                entity.Comments = printedComments;
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return entity;
        }
        #endregion EEDM Version 16

        #region EEDM sections-maximum Version 11


        /// <summary>
        /// Get selection criteria, convert it to codes as required and select 
        /// multiple sections from Colleague.
        /// </summary>
        /// <param name="title">Section Title Contains ...title...</param>
        /// <param name="startOn">Section starts on or after this date</param>
        /// <param name="endOn">Section ends on or before this date</param>
        /// <param name="code">Section Name Contains ...code...</param>
        /// <param name="number">Section Number equal to</param>
        /// <param name="instructionalPlatform">Learning Platform equal to (guid)</param>
        /// <param name="academicPeriod">Section Term equal to (guid)</param>
        /// <param name="academicLevel">Section Academic Level equal to (guid)</param>
        /// <param name="course">Section Course equal to (guid)</param>
        /// <param name="site">Section Location equal to (guid)</param>
        /// <param name="status">Section Status matches closed, open, pending, or cancelled</param>
        /// <param name="owningOrganization">Section Department equal to (guid)</param>
        /// <returns>List of SectionMaximum4 <see cref="Dtos.SectionMaximum4"/> objects representing matching sections</returns>
        public async Task<Tuple<IEnumerable<Dtos.SectionMaximum4>, int>> GetSectionsMaximum4Async(int offset, int limit, string title = "", string startOn = "", string endOn = "",
            string code = "", string number = "", string instructionalPlatform = "", string academicPeriod = "",
            List<string> academicLevel = null, string course = "", string site = "", string status = "", List<string> owningOrganization = null)
        {
            //Convert and validate all input parameters
            IDictionary<string, string> args = new Dictionary<string, string>();
            try
            {
                args = await ValidateAndConvertFilterArguments(startOn, endOn, instructionalPlatform, academicPeriod, course, site, status, "");
            }
            catch (ArgumentException)
            {
                // One or more of the arguments failed to match up to a guid.  Return empty set.
                return new Tuple<IEnumerable<Dtos.SectionMaximum4>, int>(new List<Dtos.SectionMaximum4>(), 0);

            }
            var arrayArgs = new Dictionary<string, List<string>>();
            try
            {
                arrayArgs = await ValidateAndConvertFilterArrayArguments(academicLevel, owningOrganization);
            }
            catch (ArgumentException)
            {
                // One or more of the arguments failed to match up to a guid.  Return empty set.
                return new Tuple<IEnumerable<Dtos.SectionMaximum4>, int>(new List<Dtos.SectionMaximum4>(), 0);

            }
            var newStartOn = args["newStartOn"];
            var newEndOn = args["newEndOn"];
            var newInstructionalPlatform = args["newInstructionalPlatform"];
            var newAcademicPeriod = args["newAcademicPeriod"];
            var instructorId = args["instructorId"];
            var newCourse = args["newCourse"];
            var newSite = args["newSite"];
            var newStatus = args["newStatus"];
            var newOwningOrganization = arrayArgs["newOwningOrganizations"];
            var newAcademicLevel = arrayArgs["newAcademicLevels"];
            var newSubject = args["newSubject"];



            var sectionDtos = new List<Dtos.SectionMaximum4>();
            var sectionEntities = await _sectionRepository.GetSectionsAsync(offset, limit, title, newStartOn, newEndOn,
                code, number, newInstructionalPlatform, newAcademicPeriod,
                newAcademicLevel, newCourse, newSite, newStatus, newOwningOrganization);

            foreach (var sectionEntity in sectionEntities.Item1)
            {
                if (sectionEntity.Guid != null)
                {
                    var sectionDto = await ConvertSectionEntityToSectionMaximum4Async(sectionEntity);
                    sectionDtos.Add(sectionDto);
                }
            }
            return new Tuple<IEnumerable<Dtos.SectionMaximum4>, int>(sectionDtos, sectionEntities.Item2);
        }

        /// <summary>
        /// Get a HeDM SectionMaximum version 8 by its GUID
        /// </summary>
        /// <param name="guid">GUID of section</param>
        /// <returns>The EEDM SectionMaximum version 11 DTO</returns>
        public async Task<Dtos.SectionMaximum4> GetSectionMaximumByGuid4Async(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get a section.");
            }
            var sectionEntity = await _sectionRepository.GetSectionByGuidAsync(guid);
            if (sectionEntity == null)
            {
                throw new KeyNotFoundException(string.Concat("Section not found or invalid for id '", guid, "'.  See Log for more information."));
            }
            var sectionDto = await ConvertSectionEntityToSectionMaximum4Async(sectionEntity);
            return sectionDto;
        }


        /// <summary>
        /// Convert a Section entity into the HeDM SectionMaximum format DTO
        /// </summary>
        /// <param name="entity">A Section entity</param>
        /// <returns>A HeDM-version 4 SectionMaximum</returns>
        private async Task<Dtos.SectionMaximum4> ConvertSectionEntityToSectionMaximum4Async(Domain.Student.Entities.Section entity)
        {
            if (entity == null)
            {
                return null;
            }

            var sectionDto = new Dtos.SectionMaximum4();

            sectionDto.Id = entity.Guid.ToLowerInvariant();
            sectionDto.Code = entity.Name;
            sectionDto.Number = entity.Number;
            sectionDto.Title = entity.Title;
            sectionDto.StartOn = entity.StartDate;
            sectionDto.EndOn = entity.EndDate;

            if (!string.IsNullOrEmpty(entity.LearningProvider))
            {
                var instructionalPlatform = (await InstructionalPlatformsAsync()).FirstOrDefault(i => i.Code == entity.LearningProvider);

                sectionDto.InstructionalPlatform = (instructionalPlatform != null)
                    ? new InstructionalPlatformDtoProperty()
                    {
                        Code = instructionalPlatform.Code,
                        Title = instructionalPlatform.Description,
                        Detail = new GuidObject2(instructionalPlatform.Guid)
                    }
                    : null;
            }

            if (!string.IsNullOrEmpty(entity.TermId))
            {
                var academicPeriod = academicPeriods.FirstOrDefault(a => a.Code == entity.TermId);
                if (academicPeriod != null)
                {
                    var returnAcadPeriod = new AcademicPeriodDtoProperty2()
                    {
                        Detail = new GuidObject2(academicPeriod.Guid),
                        Code = academicPeriod.Code,
                        Start = academicPeriod.StartDate,
                        End = academicPeriod.EndDate,
                        Title = academicPeriod.Description
                    };

                    if (academicPeriod.RegistrationDates != null)
                    {
                        if (academicPeriod.RegistrationDates.FirstOrDefault().CensusDates.Any())
                        {
                            returnAcadPeriod.CensusDates = academicPeriod.RegistrationDates.FirstOrDefault().CensusDates;
                        }
                    }

                    var category = new Dtos.AcademicPeriodCategory2();
                    if (IsReportingTermEqualCode(academicPeriod))
                    {
                        category.Type = Dtos.AcademicTimePeriod2.Term;
                        category.ParentGuid = (academicPeriod.ParentId != null) ? new GuidObject2(academicPeriod.ParentId) : null;
                        category.PrecedingGuid = (academicPeriod.PrecedingId != null) ? new GuidObject2(academicPeriod.PrecedingId) : null;
                    }
                    else
                    {
                        category.Type = Dtos.AcademicTimePeriod2.Subterm;
                        category.ParentGuid = (academicPeriod.ParentId != null) ? new GuidObject2(academicPeriod.ParentId) : null;
                    }

                    returnAcadPeriod.Category = category;

                    sectionDto.AcademicPeriod = returnAcadPeriod;
                }

            }


            var courseGuid = await _courseRepository.GetCourseGuidFromIdAsync(entity.CourseId);
            if (string.IsNullOrEmpty(courseGuid))
            {
                throw new ArgumentException(string.Concat("Course guid not found for Colleague Course Id : ", entity.CourseId));
            }

            var course = await _courseRepository.GetCourseByGuidAsync(courseGuid);
            if (course != null)
            {
                var courseReturn = new CourseDtoProperty()
                {
                    Detail = new GuidObject2(course.Guid),
                    Number = course.Number,
                    Title = string.IsNullOrEmpty(course.LongTitle) ? course.Title : course.LongTitle

                };
                var subject = (await _studentReferenceDataRepository.GetSubjectsAsync()).FirstOrDefault(s => s.Code == course.SubjectCode);
                if (subject != null)
                {
                    courseReturn.Subject = new SubjectDtoProperty()
                    {
                        Abbreviation = subject.Code,
                        Title = subject.Description,
                        Detail = new GuidObject2(subject.Guid)
                    };
                }
                sectionDto.Course = courseReturn;
            }
            else
            {
                throw new ArgumentException(string.Concat("Course not found for Id : ", courseGuid));
            }

            var credit = new Dtos.DtoProperties.Credit2DtoProperty();
            var creditTypeItems = await CreditTypesAsync();
            if (creditTypeItems.Any(ct => ct.Code == entity.CreditTypeCode))
            {
                var creditTypeItem = creditTypeItems.First(ct => ct.Code == entity.CreditTypeCode);
                credit.CreditCategory.Detail.Id = creditTypeItem.Guid;
                credit.CreditCategory.Code = creditTypeItem.Code;
                credit.CreditCategory.Title = creditTypeItem.Description;
                switch (creditTypeItem.CreditType)
                {

                    case CreditType.ContinuingEducation:
                        credit.CreditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
                        break;
                    case CreditType.Institutional:
                        credit.CreditCategory.CreditType = CreditCategoryType3.Institutional;
                        break;
                    case CreditType.Transfer:
                        credit.CreditCategory.CreditType = CreditCategoryType3.Transfer;
                        break;
                    case CreditType.Exchange:
                        credit.CreditCategory.CreditType = CreditCategoryType3.Exchange;
                        break;
                    case CreditType.Other:
                        credit.CreditCategory.CreditType = CreditCategoryType3.Other;
                        break;
                    case CreditType.None:
                        credit.CreditCategory.CreditType = CreditCategoryType3.NoCredit;
                        break;
                    default:
                        credit.CreditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
                        break;

                }
            }
            else
            {
                //default to CE
                credit.CreditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
            }

            if (entity.Ceus.HasValue)
            {
                credit.Measure = Dtos.CreditMeasure2.CEU;
                credit.Minimum = entity.Ceus.Value;
            }
            else
            {
                credit.Measure = Dtos.CreditMeasure2.Credit;
                credit.Minimum = entity.MinimumCredits.GetValueOrDefault();
                credit.Maximum = entity.MaximumCredits;
                credit.Increment = entity.VariableCreditIncrement;
            }
            var creditList = new List<Dtos.DtoProperties.Credit2DtoProperty>();
            creditList.Add(credit);
            sectionDto.Credits = creditList;

            var site = new SiteDtoProperty();

            var location = locations.FirstOrDefault(l => l.Code == entity.Location);
            if (location != null)
            {
                site.Code = location.Code;
                site.Title = location.Description;
                site.Detail = new GuidObject2(location.Guid);

                sectionDto.Site = site;
            }

            var acadLevel = new AcademicLevelDtoProperty();
            var acadLevel2 = new AcademicLevel2DtoProperty();
            var level = (await AcademicLevelsAsync()).FirstOrDefault(l => l.Code == entity.AcademicLevelCode);
            if (level != null)
            {
                acadLevel.Code = level.Code;
                acadLevel.Title = level.Description;
                acadLevel.Detail = new GuidObject2(level.Guid);

                acadLevel2.Code = level.Code;
                acadLevel2.Title = level.Description;
                acadLevel2.Detail = new GuidObject2(level.Guid);

                sectionDto.AcademicLevels = new List<AcademicLevelDtoProperty>()
                {
                   acadLevel
                };
            }



            var gradeScheme = (await GradeSchemesAsync()).FirstOrDefault(g => g.Code == entity.GradeSchemeCode);
            if (gradeScheme != null)
            {
                var returnGradeScheme = new GradeSchemeDtoProperty2();

                returnGradeScheme.Code = gradeScheme.Code;
                returnGradeScheme.Title = gradeScheme.Description;
                returnGradeScheme.Start = gradeScheme.EffectiveStartDate;
                returnGradeScheme.End = gradeScheme.EffectiveEndDate;
                returnGradeScheme.Detail = new GuidObject2(gradeScheme.Guid);
                returnGradeScheme.AcademicLevel = acadLevel2;

                sectionDto.GradeSchemes = new List<GradeSchemeDtoProperty2>()
                {
                   returnGradeScheme
                };

            }

            var courseLevels = await CourseLevelsAsync();
            if (courseLevels.Any())
            {
                var courseLevelList = new List<CourseLevelDtoProperty>();
                foreach (var code in entity.CourseLevelCodes)
                {
                    var cLevel = courseLevels.FirstOrDefault(l => l.Code == code);
                    if (cLevel != null)
                    {
                        var levelReturn = new CourseLevelDtoProperty
                        {
                            Code = cLevel.Code,
                            Title = cLevel.Description,
                            Detail = new GuidObject2(cLevel.Guid)
                        };
                        courseLevelList.Add(levelReturn);
                    }
                }
                sectionDto.CourseLevels = courseLevelList;
            }

            string status;
            var currentStatus = ConvertSectionStatusToDto2(entity.Statuses.ElementAt(0).IntegrationStatus, out status);
            status = entity.Statuses.ElementAt(0).StatusCode;
            if (currentStatus != SectionStatus2.NotSet)
            {
                sectionDto.Status = new SectionStatusDtoProperty();
                sectionDto.Status.Category = currentStatus;
                sectionDto.Status.Detail = new GuidObject2(ConvertCodeToGuid((await AllStatusesWithGuidsAsync()), status));
            }

            sectionDto.MaximumEnrollment = entity.SectionCapacity;

            if (entity.NumberOfWeeks.HasValue && entity.NumberOfWeeks > 0)
            {
                sectionDto.Duration = new SectionDuration2()
                {
                    Length = entity.NumberOfWeeks.Value,
                    Unit = DurationUnit2.Weeks
                };
            }

            var orgs = await DepartmentsAsync();
            if (orgs.Any())
            {
                var orgsList = new List<OwningOrganizationDtoProperty>();
                foreach (var code in entity.Departments)
                {
                    var owningOrg = orgs.FirstOrDefault(o => o.Code == code.AcademicDepartmentCode);
                    if (owningOrg != null)
                    {
                        var orgReturn = new OwningOrganizationDtoProperty();
                        orgReturn.Code = owningOrg.Code;
                        orgReturn.Title = owningOrg.Description;
                        orgReturn.OwnershipPercentage = code.ResponsibilityPercentage;
                        orgReturn.Detail = new GuidObject2(owningOrg.Guid);
                        orgsList.Add(orgReturn);
                    }
                }
                sectionDto.OwningOrganizations = orgsList;
            }

            var pageOfItems = await _sectionRepository.GetSectionMeetingAsync(0, 0, entity.Id, string.Empty, string.Empty, string.Empty, string.Empty, new List<string>(), new List<string>(), new List<string>(), string.Empty);
            var eventEntities = pageOfItems.Item1;

            if (eventEntities.Any())
            {
                var instructionalEventsList = new List<InstructionalEventDtoProperty2>();

                foreach (var eventInstructional in eventEntities)
                {
                    if (eventInstructional.Guid != null)
                    {
                        var returnInstructionalEventDto = new InstructionalEventDtoProperty2();
                        var instEventDto = await ConvertSectionMeetingToInstructionalEvent2Async(eventInstructional);

                        returnInstructionalEventDto.Detail = new GuidObject2(instEventDto.Id);
                        returnInstructionalEventDto.Title = instEventDto.Title;
                        var instMethod = (await _studentReferenceDataRepository.GetInstructionalMethodsAsync()).FirstOrDefault(im => im.Guid == instEventDto.InstructionalMethod.Id);

                        if (instMethod != null)
                        {
                            returnInstructionalEventDto.InstructionalMethod = new InstructionalMethodDtoProperty()
                            {
                                Abbreviation = instMethod.Code,
                                Title = instMethod.Description,
                                Detail = new GuidObject2(instMethod.Guid)
                            };
                        }

                        if (instEventDto.Recurrence != null)
                        {
                            returnInstructionalEventDto.Recurrence = instEventDto.Recurrence;
                        }

                        if ((instEventDto.Locations != null) && (instEventDto.Locations.Any()))
                        {
                            var locationsList = new List<LocationDtoProperty>();
                            var locationDtoProperty = new LocationDtoProperty();
                            var locationRoom = new LocationRoomDtoProperty();
                            var room = (await RoomsAsync()).FirstOrDefault(r => r.Id == eventInstructional.Room);

                            if (room != null)
                            {
                                locationRoom.LocationType = InstructionalLocationType.InstructionalRoom;
                                locationRoom.Title = !string.IsNullOrEmpty(room.Name) ? room.Name : null;
                                locationRoom.Number = !string.IsNullOrEmpty(room.Number) ? room.Number : null;
                                locationRoom.Floor = !string.IsNullOrEmpty(room.Floor) ? room.Floor : null;
                                locationRoom.Detail = new GuidObject2(room.Guid);

                                var building = (await _referenceDataRepository.BuildingsAsync()).FirstOrDefault(b => b.Code == room.BuildingCode);
                                if (building != null)
                                {
                                    locationRoom.Building = new BuildingDtoProperty()
                                    {
                                        Code = building.Code,
                                        Title = building.Description,
                                        Detail = new GuidObject2(building.Guid)
                                    };
                                }

                                locationDtoProperty.Location = locationRoom;
                                locationsList.Add(locationDtoProperty);
                            }
                            returnInstructionalEventDto.Locations = locationsList;

                        }

                        if ((instEventDto.Instructors != null) && (instEventDto.Instructors.Any()))
                        {
                            var instructorList = new List<InstructorRosterDtoProperty2>();


                            foreach (var eventInstructor in instEventDto.Instructors)
                            {
                                var returnInstructorRoster = new InstructorRosterDtoProperty2
                                {
                                    WorkLoadPercentage = eventInstructor.WorkLoadPercentage,
                                    ResponsibilityPercentage = eventInstructor.ResponsibilityPercentage,
                                    WorkStartDate = eventInstructor.WorkStartDate,
                                    WorkEndDate = eventInstructor.WorkEndDate
                                };

                                var returnInstructor = new InstructorDtoProperty2 { Detail = new GuidObject2(eventInstructor.Instructor.Id) };
                                var namesList = new List<InstructorNameDtoProperty2>();
                                var instructorName = new InstructorNameDtoProperty2();

                                var person = await _personRepository.GetPersonByGuidNonCachedAsync(eventInstructor.Instructor.Id);
                                var facultyList = await _sectionRepository.GetSectionFacultyAsync(0, 100, entity.Id, "", new List<string>());

                                if (facultyList != null && facultyList.Item1.Any())
                                {
                                    var faculty = facultyList.Item1.FirstOrDefault(fac => fac.FacultyId == person.Id);
                                    if (faculty != null && faculty.PrimaryIndicator)
                                    {
                                        returnInstructor.InstructorRole = SectionInstructorsInstructorRole.Primary;
                                    }
                                }


                                instructorName.NameType = new InstructorNameTypeDtoProperty()
                                {
                                    Category = InstructorNameType2.Personal
                                };

                                instructorName.Title = string.IsNullOrEmpty(person.Prefix) ? "" : person.Prefix;
                                instructorName.Pedigree = string.IsNullOrEmpty(person.Suffix) ? "" : person.Suffix;
                                instructorName.FirstName = string.IsNullOrEmpty(person.FirstName) ? "" : person.FirstName;
                                instructorName.MiddleName = string.IsNullOrEmpty(person.MiddleName) ? null : person.MiddleName;
                                instructorName.LastNamePrefix = null;
                                instructorName.LastName = person.LastName;
                                instructorName.PreferredName = string.IsNullOrEmpty(person.Nickname) ? null : person.Nickname;
                                instructorName.FullName = BuildFullName(instructorName.FirstName,
                                    instructorName.MiddleName, instructorName.LastName);

                                namesList.Add(instructorName);
                                returnInstructor.Names = namesList;

                                var credentialList = new List<CredentialDtoProperty>();

                                credentialList.Add(new CredentialDtoProperty()
                                {
                                    Type = Dtos.EnumProperties.CredentialType.ColleaguePersonId,
                                    Value = person.Id
                                });

                                if (person.PersonAltIds != null && person.PersonAltIds.Any())
                                {
                                    var elevPersonAltId = person.PersonAltIds.FirstOrDefault(a => a.Type == Domain.Base.Entities.PersonAlt.ElevatePersonAltType);
                                    if (elevPersonAltId != null && !string.IsNullOrEmpty(elevPersonAltId.Id))
                                    {
                                        credentialList.Add(new CredentialDtoProperty()
                                        {
                                            Type = Dtos.EnumProperties.CredentialType.ElevateID,
                                            Value = elevPersonAltId.Id
                                        });
                                    }
                                }

                                returnInstructor.Credentials = credentialList;
                                returnInstructorRoster.Instructor = returnInstructor;
                                instructorList.Add(returnInstructorRoster);
                            }
                            returnInstructionalEventDto.InstructorRoster = instructorList;

                        }


                        instructionalEventsList.Add(returnInstructionalEventDto);
                    }
                }

                if (instructionalEventsList.Any())
                {
                    sectionDto.InstructionalEvents = instructionalEventsList;
                }
            }

            return sectionDto;
        }

        #endregion EEDM sections-maximum Version 11

        #region EEDM sections-maximum Version 16.0.0


        /// <summary>
        /// Get selection criteria, convert it to codes as required and select 
        /// multiple sections from Colleague.
        /// </summary>
        /// <param name="title">Section Title Contains ...title...</param>
        /// <param name="startOn">Section starts on or after this date</param>
        /// <param name="endOn">Section ends on or before this date</param>
        /// <param name="code">Section Name Contains ...code...</param>
        /// <param name="number">Section Number equal to</param>
        /// <param name="instructionalPlatform">Learning Platform equal to (guid)</param>
        /// <param name="academicPeriod">Section Term equal to (guid)</param>
        /// <param name="academicLevel">Section Academic Level equal to (guid)</param>
        /// <param name="course">Section Course equal to (guid)</param>
        /// <param name="site">Section Location equal to (guid)</param>
        /// <param name="status">Section Status matches closed, open, pending, or cancelled</param>
        /// <param name="owningOrganization">Section Department equal to (guid)</param>
        /// <returns>List of SectionMaximum4 <see cref="Dtos.SectionMaximum5"/> objects representing matching sections</returns>
        public async Task<Tuple<IEnumerable<Dtos.SectionMaximum5>, int>> GetSectionsMaximum5Async(int offset, int limit, string title = "", string startOn = "", string endOn = "",
            string code = "", string number = "", string instructionalPlatform = "", string academicPeriod = "", string reportingAcademicPeriod = "",
            List<string> academicLevel = null, string course = "", string site = "", string status = "", List<string> owningOrganization = null, List<string> instructors = null,
            string scheduleAcademicPeriod = "", bool bypassCache = false)
        {
            //Convert and validate all input parameters
            IDictionary<string, string> args = new Dictionary<string, string>();
            try
            {
                args = await ValidateAndConvertFilterArguments(startOn, endOn, instructionalPlatform, academicPeriod, course, site, status, "", reportingAcademicPeriod: reportingAcademicPeriod, scheduleAcademicPeriod: scheduleAcademicPeriod);
            }
            catch (ArgumentException)
            {
                // One or more of the arguments failed to match up to a guid.  Return empty set.
                return new Tuple<IEnumerable<Dtos.SectionMaximum5>, int>(new List<Dtos.SectionMaximum5>(), 0);

            }
            var arrayArgs = new Dictionary<string, List<string>>();
            try
            {
                arrayArgs = await ValidateAndConvertFilterArrayArguments(academicLevel, owningOrganization, instructors);
            }
            catch (ArgumentException)
            {
                // One or more of the arguments failed to match up to a guid.  Return empty set.
                return new Tuple<IEnumerable<Dtos.SectionMaximum5>, int>(new List<Dtos.SectionMaximum5>(), 0);

            }
            var newStartOn = args["newStartOn"];
            var newEndOn = args["newEndOn"];
            var newInstructionalPlatform = args["newInstructionalPlatform"];
            var newAcademicPeriod = args["newAcademicPeriod"];
            var newScheduleAcademicPeriod = args["newScheduleAcademicPeriod"];
            var newReportingAcademicPeriod = args["newReportingAcademicPeriod"];
            var instructorId = args["instructorId"];
            var newCourse = args["newCourse"];
            var newSite = args["newSite"];
            var newStatus = args["newStatus"];
            var newOwningOrganization = arrayArgs["newOwningOrganizations"];
            var newAcademicLevel = arrayArgs["newAcademicLevels"];
            var newInstructors = arrayArgs["newInstructors"];
            var newSubject = args["newSubject"];

            var sectionDtos = new List<Dtos.SectionMaximum5>();
            Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section>, int> sectionEntities = null;
            try
            {                
                sectionEntities = await _sectionRepository.GetSectionsAsync(offset, limit, title, newStartOn, newEndOn,
                    code, number, newInstructionalPlatform, newAcademicPeriod, newReportingAcademicPeriod,
                    newAcademicLevel, newCourse, newSite, newStatus, newOwningOrganization, "", newInstructors, newScheduleAcademicPeriod);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message);
                throw IntegrationApiException;
            }

            var personIds = sectionEntities.Item1
                            .SelectMany(f => f.Faculty)
                            .Where(i => !string.IsNullOrWhiteSpace(i.Id))
                            .Select(fc => fc.FacultyId).Distinct().ToArray();

            if (personIds != null && personIds.Any())
            {
                await SetFacultyGuidsAsync(personIds);
            }

            if (_facultyGuids != null && _facultyGuids.Any())
            {
                var facultyGuids = _facultyGuids.Where(v => !string.IsNullOrWhiteSpace(v.Value)).Select(i => i.Value).ToArray();
                _personPins = await GetPersonPinsAsync(facultyGuids);
                await SetPersonDataForFacultyAsync(facultyGuids);
            }

            var courseIds = sectionEntities.Item1
                            .Where(i => !string.IsNullOrWhiteSpace(i.CourseId))
                            .Select(c => c.CourseId).Distinct().ToList();
            await SetCourseCollectionAsync(courseIds);

            foreach (var sectionEntity in sectionEntities.Item1)
            {
                if (sectionEntity.Guid != null)
                {
                    var sectionDto = await ConvertSectionEntityToSectionMaximum5Async(sectionEntity);
                    sectionDtos.Add(sectionDto);
                }
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            return new Tuple<IEnumerable<Dtos.SectionMaximum5>, int>(sectionDtos, sectionEntities.Item2);
        }

        /// <summary>
        /// Get a HeDM SectionMaximum version 8 by its GUID
        /// </summary>
        /// <param name="guid">GUID of section</param>
        /// <returns>The EEDM SectionMaximum version 16.0.0 DTO</returns>
        public async Task<Dtos.SectionMaximum5> GetSectionMaximumByGuid5Async(string guid, bool bypassCache = false)
        {
            if (string.IsNullOrEmpty(guid))
            {
                IntegrationApiExceptionAddError("A GUID is required to obtain a section", "Missing.Required.Property");
                throw IntegrationApiException;
            }
            Domain.Student.Entities.Section sectionEntity = null;
            try
            {
                sectionEntity = await _sectionRepository.GetSectionByGuidAsync(guid);
                if (sectionEntity == null)
                {
                    IntegrationApiExceptionAddError("Section not found or invalid for id '", guid, "'.  See Log for more information.", "Bad.Data");
                    throw IntegrationApiException;
                }
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex, guid: guid);
                throw IntegrationApiException;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message);
                throw IntegrationApiException;
            }

            var personIds = sectionEntity.Faculty
                           .Where(i => !string.IsNullOrWhiteSpace(i.Id))
                           .Select(fc => fc.FacultyId).ToArray();

            await SetFacultyGuidsAsync(personIds);

            if (_facultyGuids != null && _facultyGuids.Any())
            {
                var facultyGuids = _facultyGuids.Where(v => !string.IsNullOrWhiteSpace(v.Value)).Select(i => i.Value).ToArray();
                _personPins = await GetPersonPinsAsync(facultyGuids);
                await SetPersonDataForFacultyAsync(facultyGuids);
            }

            var courseIds = new List<string>() { sectionEntity.CourseId };
            await SetCourseCollectionAsync(courseIds);

            SetSectionGuids(sectionEntity);

            var sectionDto = await ConvertSectionEntityToSectionMaximum5Async(sectionEntity, bypassCache);
            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
            return sectionDto;
        }


        /// <summary>
        /// Convert a Section entity into the HeDM SectionMaximum format DTO
        /// </summary>
        /// <param name="entity">A Section entity</param>
        /// <returns>A HeDM-version 4 SectionMaximum</returns>
        private async Task<Dtos.SectionMaximum5> ConvertSectionEntityToSectionMaximum5Async(Domain.Student.Entities.Section entity, bool bypassCache = false)
        {
            if (entity == null)
            {
                return null;
            }

            var sectionDto = new Dtos.SectionMaximum5();

            sectionDto.Id = entity.Guid.ToLowerInvariant();
            sectionDto.Code = entity.Name;
            sectionDto.Number = entity.Number;
            sectionDto.Title = entity.Title;
            sectionDto.StartOn = entity.StartDate;
            sectionDto.EndOn = entity.EndDate;
            if (!string.IsNullOrEmpty(entity.Comments))
                sectionDto.Description = entity.Comments;
            if (entity.CensusDates != null && entity.CensusDates.Any())
                sectionDto.CensusDates = entity.CensusDates;

            if (!string.IsNullOrEmpty(entity.LearningProvider))
            {
                Domain.Base.Entities.InstructionalPlatform instructionalPlatform = null;
                try
                {
                    var instructionalPlatforms = await InstructionalPlatformsAsync();
                    if (instructionalPlatforms.Any())
                    {
                        instructionalPlatform = (instructionalPlatforms.FirstOrDefault(i => i.Code == entity.LearningProvider));
                        if (instructionalPlatform != null && !string.IsNullOrEmpty(instructionalPlatform.Guid))
                        {
                            if (!string.IsNullOrEmpty(instructionalPlatform.Description))
                            {
                                sectionDto.InstructionalPlatform = (instructionalPlatform != null)
                                        ? new InstructionalPlatformDtoProperty()
                                        {
                                            Code = instructionalPlatform.Code,
                                            Title = instructionalPlatform.Description,
                                            Detail = new GuidObject2(instructionalPlatform.Guid)
                                        }
                                        : null;
                            }
                            else
                            {
                                IntegrationApiExceptionAddError(string.Concat("Missing description for instructional platform ", entity.LearningProvider, "."),
                                "GUID.Not.Found", entity.Guid, entity.Id);
                            }
                        }
                        else
                        {
                            IntegrationApiExceptionAddError(string.Concat("Missing instructional platform GUID or invalid code of ", entity.LearningProvider, "."),
                                "GUID.Not.Found", entity.Guid, entity.Id);
                        }
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Concat("No instructional platforms found."),
                            "GUID.Not.Found", entity.Guid, entity.Id);
                    }
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                }
            }

            if (!string.IsNullOrEmpty(entity.TermId))
            {
                Domain.Student.Entities.AcademicPeriod academicPeriod = null;
                try
                {
                    var acadPeriods = academicPeriods;
                    if (acadPeriods.Any())
                    {
                        academicPeriod = acadPeriods.FirstOrDefault(a => a.Code == entity.TermId);
                        if (academicPeriod != null && !string.IsNullOrEmpty(academicPeriod.Guid))
                        {
                            var validAcadPeriod = true;
                            if (string.IsNullOrEmpty(academicPeriod.Description))
                            {
                                IntegrationApiExceptionAddError(string.Concat("Missing description for academic period ", entity.TermId, "."),
                                "GUID.Not.Found", entity.Guid, entity.Id);
                                validAcadPeriod = false;
                            }
                            if (academicPeriod.StartDate == null)
                            {
                                IntegrationApiExceptionAddError(string.Concat("Missing start date for academic period ", entity.TermId, "."),
                                "GUID.Not.Found", entity.Guid, entity.Id);
                                validAcadPeriod = false;
                            }
                            if (academicPeriod.EndDate == null)
                            {
                                IntegrationApiExceptionAddError(string.Concat("Missing end date for academic period ", entity.TermId, "."),
                                "GUID.Not.Found", entity.Guid, entity.Id);
                                validAcadPeriod = false;
                            }
                            if (validAcadPeriod == true)
                            {
                                var returnAcadPeriod = new AcademicPeriodDtoProperty3()
                                {
                                    Detail = new GuidObject2(academicPeriod.Guid),
                                    Code = academicPeriod.Code,
                                    Start = academicPeriod.StartDate,
                                    End = academicPeriod.EndDate,
                                    Title = academicPeriod.Description
                                };

                                if (academicPeriod.RegistrationDates != null)
                                {
                                    if (academicPeriod.RegistrationDates.FirstOrDefault().CensusDates.Any())
                                    {
                                        returnAcadPeriod.CensusDates = academicPeriod.RegistrationDates.FirstOrDefault().CensusDates;
                                    }
                                }

                                var category = new Dtos.AcademicPeriodCategory3();
                                switch (academicPeriod.Category)
                                {
                                    case ("year"):
                                        {
                                            category.Type = Dtos.AcademicTimePeriod2.Year;
                                            break;
                                        }
                                    case ("term"):
                                        {
                                            category.Type = Dtos.AcademicTimePeriod2.Term;
                                            if (!string.IsNullOrEmpty(academicPeriod.ParentId))
                                            {
                                                category.Parent = new AcademicPeriodCategoryParent()
                                                {
                                                    Id = academicPeriod.ParentId
                                                };
                                            }
                                            else
                                            {
                                                if (academicPeriod.ReportingYear > 0)
                                                {
                                                    category.Parent = new AcademicPeriodCategoryParent()
                                                    {
                                                        Year = academicPeriod.ReportingYear
                                                    };
                                                }
                                            }

                                            if (!string.IsNullOrEmpty(academicPeriod.PrecedingId))
                                            {
                                                category.PrecedingGuid = new GuidObject2(academicPeriod.PrecedingId);
                                            }
                                            break;
                                        }
                                    case ("subterm"):
                                        {
                                            category.Type = Dtos.AcademicTimePeriod2.Subterm;
                                            if (academicPeriod.ParentId != null)
                                            {
                                                category.Parent = new AcademicPeriodCategoryParent()
                                                {
                                                    Id = academicPeriod.ParentId
                                                };
                                            }

                                            if (!string.IsNullOrEmpty(academicPeriod.PrecedingId))
                                            {
                                                category.PrecedingGuid = new GuidObject2(academicPeriod.PrecedingId);
                                            }
                                            break;
                                        }
                                }

                                returnAcadPeriod.Category = category;

                                sectionDto.AcademicPeriod = returnAcadPeriod;

                                var returnScheduleAcadPeriod = new AcademicPeriodDtoProperty3()
                                {
                                    Detail = new GuidObject2(academicPeriod.Guid),
                                    Code = academicPeriod.Code,
                                    Title = academicPeriod.Description
                                };
                                sectionDto.ScheduleAcademicPeriod = returnScheduleAcadPeriod;

                                // Update Reporting Academic Period Object
                                if (!string.IsNullOrEmpty(academicPeriod.ReportingTerm))
                                {
                                    var reportingTerm = academicPeriods.FirstOrDefault(a => a.Code == academicPeriod.ReportingTerm);
                                    if (reportingTerm != null)
                                    {
                                        sectionDto.ReportingAcademicPeriod = new GuidObject2(reportingTerm.Guid);
                                    }
                                }
                            }
                        }
                        else
                        {
                            IntegrationApiExceptionAddError(string.Concat("Missing academic period GUID or invalid code of ", entity.TermId, "."),
                                "GUID.Not.Found", entity.Guid, entity.Id);
                        }
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Concat("No academic periods found."),
                            "GUID.Not.Found", entity.Guid, entity.Id);
                    }
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                }
            }

            // get course from previously bulk loaded courses
            if (!string.IsNullOrEmpty(entity.CourseId))
            {
                try
                {
                    var course = await GetCourseByIdAsync(entity.CourseId);
                    if (course != null)
                    {
                        var courseReturn = new CourseDtoProperty2()
                        {
                            Detail = new GuidObject2(course.Guid),
                            Number = course.Number

                        };
                        courseReturn.Titles = new List<CoursesTitlesDtoProperty2>();
                        try
                        {
                            var courseTitleTypes = await CourseTitleTypesAsync(bypassCache);
                            if (courseTitleTypes.Any())
                            {
                                if (!string.IsNullOrEmpty(course.Title))
                                {
                                    var shortCourseEntity = courseTitleTypes.FirstOrDefault(ctt => ctt.Code.ToLower() == "short");
                                    if (shortCourseEntity != null && !string.IsNullOrEmpty(shortCourseEntity.Guid))
                                    {
                                        var shortCourseTitle = new CoursesTitlesDtoProperty2() { Type = new GuidObject2(shortCourseEntity.Guid), Value = course.Title };
                                        courseReturn.Titles.Add(shortCourseTitle);
                                    }
                                    else
                                    {
                                        IntegrationApiExceptionAddError("Missing course title type GUID for 'SHORT'.",
                                        "GUID.Not.Found", entity.Guid, entity.Id);
                                    }
                                }
                                else
                                {
                                    IntegrationApiExceptionAddError(string.Concat("Missing short course title for course ", course.Id, "."),
                                "GUID.Not.Found", entity.Guid, entity.Id);
                                }

                                if (!string.IsNullOrEmpty(course.LongTitle))
                                {
                                    var longCourseEntity = courseTitleTypes.FirstOrDefault(ctt => ctt.Code.ToLower() == "long");
                                    if (longCourseEntity != null && !string.IsNullOrEmpty(longCourseEntity.Guid))
                                    {
                                        var longCourseTitle = new CoursesTitlesDtoProperty2() { Type = new GuidObject2(longCourseEntity.Guid), Value = course.LongTitle };
                                        courseReturn.Titles.Add(longCourseTitle);
                                    }
                                    else
                                    {
                                        IntegrationApiExceptionAddError("Missing course title type GUID for 'LONG'.",
                                        "GUID.Not.Found", entity.Guid, entity.Id);
                                    }
                                }
                                else
                                {
                                    IntegrationApiExceptionAddError(string.Concat("Missing long course title for course ", course.Id, "."),
                                "GUID.Not.Found", entity.Guid, entity.Id);
                                }
                            }
                            else
                            {
                                {
                                    IntegrationApiExceptionAddError(string.Concat("No course title types found."),
                                        "GUID.Not.Found", entity.Guid, entity.Id);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                        }

                        
                        if (!string.IsNullOrEmpty(course.SubjectCode))
                        {
                            var subjects = await SubjectsAsync(bypassCache);
                            if (subjects.Any())
                            {
                                var subject = subjects.FirstOrDefault(s => s.Code == course.SubjectCode);
                                if (subject != null)
                                {
                                    courseReturn.Subject = new SubjectDtoProperty()
                                    {
                                        Abbreviation = subject.Code,
                                        Title = subject.Description,
                                        Detail = new GuidObject2(subject.Guid)
                                    };
                                    sectionDto.Course = courseReturn;
                                }
                                else
                                {
                                    IntegrationApiExceptionAddError(string.Concat("Missing subject GUID or invalid code of ", course.SubjectCode, "."),
                                "GUID.Not.Found", entity.Guid, entity.Id);
                                }
                            }
                            else
                            {
                                {
                                    IntegrationApiExceptionAddError(string.Concat("No subjects found."),
                                        "GUID.Not.Found", entity.Guid, entity.Id);
                                }
                            }
                        }     
                        else
                        {
                            IntegrationApiExceptionAddError(string.Concat("Missing subject for course ", course.Id, "."),
                               "GUID.Not.Found", entity.Guid, entity.Id);
                        }
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Concat("Missing course GUID for id '", entity.CourseId, "."),
                                "GUID.Not.Found", entity.Guid, entity.Id);
                    }
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                }
            }
            else
            {
                IntegrationApiExceptionAddError("Missing course.", "GUID.Not.Found", entity.Guid, entity.Id);
            }
                        
            var credit = new Dtos.DtoProperties.Credit2DtoProperty();
            if (!string.IsNullOrEmpty(entity.CreditTypeCode))
            {
                try
                {
                    var creditTypeItems = await CreditTypesAsync();
                    if (creditTypeItems.Any())
                    {
                        if (creditTypeItems.Any(ct => ct.Code == entity.CreditTypeCode))
                        {
                            var creditTypeItem = creditTypeItems.First(ct => ct.Code == entity.CreditTypeCode);
                            credit.CreditCategory.Detail.Id = creditTypeItem.Guid;
                            credit.CreditCategory.Code = creditTypeItem.Code;
                            credit.CreditCategory.Title = creditTypeItem.Description;
                            switch (creditTypeItem.CreditType)
                            {

                                case CreditType.ContinuingEducation:
                                    credit.CreditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
                                    break;
                                case CreditType.Institutional:
                                    credit.CreditCategory.CreditType = CreditCategoryType3.Institutional;
                                    break;
                                case CreditType.Transfer:
                                    credit.CreditCategory.CreditType = CreditCategoryType3.Transfer;
                                    break;
                                case CreditType.Exchange:
                                    credit.CreditCategory.CreditType = CreditCategoryType3.Exchange;
                                    break;
                                case CreditType.Other:
                                    credit.CreditCategory.CreditType = CreditCategoryType3.Other;
                                    break;
                                case CreditType.None:
                                    credit.CreditCategory.CreditType = CreditCategoryType3.NoCredit;
                                    break;
                                default:
                                    credit.CreditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
                                    break;

                            }
                        }
                        else
                        {
                            //default to CE
                            credit.CreditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
                        }
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Concat("No credit types found."),
                            "GUID.Not.Found", entity.Guid, entity.Id);
                    }
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                }
            }

            if (entity.Ceus.HasValue)
            {
                credit.Measure = Dtos.CreditMeasure2.CEU;
                credit.Minimum = entity.Ceus.Value;
            }
            else
            {
                credit.Measure = Dtos.CreditMeasure2.Credit;
                credit.Minimum = entity.MinimumCredits.GetValueOrDefault();
                credit.Maximum = entity.MaximumCredits;
                credit.Increment = entity.VariableCreditIncrement;
            }
            var creditList = new List<Dtos.DtoProperties.Credit2DtoProperty>();
            creditList.Add(credit);
            sectionDto.Credits = creditList;

            if (!string.IsNullOrEmpty(entity.Location))
            {                
                var site = new SiteDtoProperty();
                try
                {
                    var sites = locations;
                    if (sites.Any())
                    {
                        var location = sites.FirstOrDefault(l => l.Code == entity.Location);
                        if (location != null)
                        {
                            site.Code = location.Code;
                            site.Title = location.Description;
                            site.Detail = new GuidObject2(location.Guid);

                            sectionDto.Site = site;
                        }
                        else
                        {
                            IntegrationApiExceptionAddError(string.Concat("Missing site GUID or invalid code of ", entity.Location, "."),
                                "GUID.Not.Found", entity.Guid, entity.Id);
                        }
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Concat("No sites found."),
                            "GUID.Not.Found", entity.Guid, entity.Id);
                    }
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                }
            }

            var acadLevel = new AcademicLevelDtoProperty();
            var acadLevel2 = new AcademicLevel2DtoProperty();
            if (!string.IsNullOrEmpty(entity.AcademicLevelCode))
            {
                try
                {
                    var levels = await AcademicLevelsAsync();
                    if (levels.Any())
                    {                        
                        var level = (levels.FirstOrDefault(l => l.Code == entity.AcademicLevelCode));
                        if (level != null)
                        {
                            if (!string.IsNullOrEmpty(level.Description))
                            {
                                acadLevel.Code = level.Code;
                                acadLevel.Title = level.Description;
                                acadLevel.Detail = new GuidObject2(level.Guid);

                                acadLevel2.Code = level.Code;
                                acadLevel2.Title = level.Description;
                                acadLevel2.Detail = new GuidObject2(level.Guid);

                                sectionDto.AcademicLevels = new List<AcademicLevelDtoProperty>()
                                {
                                    acadLevel
                                };
                            }
                            else
                            {
                                IntegrationApiExceptionAddError(string.Concat("Missing description for academic level ", entity.AcademicLevelCode, "."),
                                "GUID.Not.Found", entity.Guid, entity.Id);
                            }

                        }
                        else
                        {
                            IntegrationApiExceptionAddError(string.Concat("Missing academic level GUID or invalid code of ", entity.Location, "."),
                                "GUID.Not.Found", entity.Guid, entity.Id);
                        }
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Concat("No academic levels found."),
                            "GUID.Not.Found", entity.Guid, entity.Id);
                    }
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                }
            }

            if (!string.IsNullOrEmpty(entity.GradeSchemeCode))
            {
                try
                {
                    var gradeSchemes = await GradeSchemesAsync();
                    if (gradeSchemes.Any())
                    {
                        var gradeScheme = (await GradeSchemesAsync()).FirstOrDefault(g => g.Code == entity.GradeSchemeCode);
                        if (gradeScheme != null)
                        {
                            if (!string.IsNullOrEmpty(gradeScheme.Description))
                            {
                                var returnGradeScheme = new GradeSchemeDtoProperty2();

                                returnGradeScheme.Code = gradeScheme.Code;
                                returnGradeScheme.Title = gradeScheme.Description;
                                returnGradeScheme.Start = gradeScheme.EffectiveStartDate;
                                returnGradeScheme.End = gradeScheme.EffectiveEndDate;
                                returnGradeScheme.Detail = new GuidObject2(gradeScheme.Guid);
                                returnGradeScheme.AcademicLevel = acadLevel2;

                                sectionDto.GradeSchemes = new List<GradeSchemeDtoProperty2>()
                                {
                                    returnGradeScheme
                                };
                            }
                            else
                            {
                                IntegrationApiExceptionAddError(string.Concat("Missing description for grade scheme ", entity.GradeSchemeCode, "."),
                                "GUID.Not.Found", entity.Guid, entity.Id);
                            }
                        }
                        else
                        {
                            IntegrationApiExceptionAddError(string.Concat("Missing grade scheme GUID or invalid code of ", entity.GradeSchemeCode, "."),
                                "GUID.Not.Found", entity.Guid, entity.Id);
                        }
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Concat("No grade schemes found."),
                            "GUID.Not.Found", entity.Guid, entity.Id);
                    }
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                }
            }

            if (entity.CourseLevelCodes.Any())
            {
                try
                {
                    var courseLevels = await CourseLevelsAsync();
                    if (courseLevels.Any())
                    {
                        var courseLevelList = new List<CourseLevelDtoProperty>();
                        foreach (var code in entity.CourseLevelCodes)
                        {
                            if (!string.IsNullOrEmpty(code))
                            {
                                var cLevel = courseLevels.FirstOrDefault(l => l.Code == code);
                                if (cLevel != null)
                                {
                                    if (!string.IsNullOrEmpty(cLevel.Description))
                                    {
                                        var levelReturn = new CourseLevelDtoProperty
                                        {
                                            Code = cLevel.Code,

                                            Title = cLevel.Description,
                                            Detail = new GuidObject2(cLevel.Guid)
                                        };
                                        courseLevelList.Add(levelReturn);
                                    }
                                    else
                                    {
                                        IntegrationApiExceptionAddError(string.Concat("Missing description for course level ", code, "."),
                                        "GUID.Not.Found", entity.Guid, entity.Id);
                                    }
                                }
                                else
                                {
                                    IntegrationApiExceptionAddError(string.Concat("Missing course level GUID or invalid code of ", code, "."),
                                        "GUID.Not.Found", entity.Guid, entity.Id);
                                }
                            }
                        }
                        sectionDto.CourseLevels = courseLevelList;
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Concat("No course levels found."),
                            "GUID.Not.Found", entity.Guid, entity.Id);
                    }
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                }
            }
            string status;
            var currentStatus = ConvertSectionStatusToDto2(entity.Statuses.ElementAt(0).IntegrationStatus, out status);
            status = entity.Statuses.ElementAt(0).StatusCode;
            if (currentStatus != SectionStatus2.NotSet)
            {
                sectionDto.Status = new SectionStatusDtoProperty();
                sectionDto.Status.Category = currentStatus;
                sectionDto.Status.Detail = new GuidObject2(ConvertCodeToGuid((await AllStatusesWithGuidsAsync()), status));
            }

            sectionDto.MaximumEnrollment = entity.SectionCapacity;

            if (entity.NumberOfWeeks.HasValue && entity.NumberOfWeeks > 0)
            {
                sectionDto.Duration = new SectionDuration2()
                {
                    Length = entity.NumberOfWeeks.Value,
                    Unit = DurationUnit2.Weeks
                };
            }


            //if (entity.Departments != null && entity.Departments.Any() && orgs != null && orgs.Any())
            if (entity.Departments != null && entity.Departments.Any())
            {
                try
                {
                    var orgs = await DepartmentsAsync();
                    if (orgs.Any())
                    {
                        var orgsList = new List<OwningOrganizationDtoProperty>();
                        foreach (var code in entity.Departments)
                        {
                            if (code != null)
                            {
                                var owningOrg = orgs.FirstOrDefault(o => o.Code == code.AcademicDepartmentCode);
                                if (owningOrg != null)
                                {
                                    var orgReturn = new OwningOrganizationDtoProperty();
                                    orgReturn.Code = owningOrg.Code;
                                    orgReturn.Title = owningOrg.Description;
                                    orgReturn.OwnershipPercentage = code.ResponsibilityPercentage;
                                    orgReturn.Detail = new GuidObject2(owningOrg.Guid);
                                    orgReturn.Type = EducationalInstitutionUnitType.Department;
                                    orgsList.Add(orgReturn);
                                }
                                else
                                {
                                    IntegrationApiExceptionAddError(string.Concat("Missing owning institution unit GUID or invalid code of ", code, "."),
                                        "GUID.Not.Found", entity.Guid, entity.Id);
                                }
                            }
                        }
                        sectionDto.OwningOrganizations = orgsList;
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Concat("No owning institution units found."),
                            "GUID.Not.Found", entity.Guid, entity.Id);
                    }
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                }
            }

            // Get instructional methods if it will be needed for any of multiple DTO properties
            IEnumerable<Domain.Student.Entities.InstructionalMethod> allInstructionalMethods = null;            
            if ((entity.InstructionalContacts != null && entity.InstructionalContacts.Any()) ||
                (entity.InstructionalMethods != null && entity.InstructionalMethods.Any()) ||
                (entity.Meetings != null) && (entity.Meetings.Any()) ||
                (entity.Faculty != null) && (entity.Faculty.Any()))           
            {
                try
                {
                    allInstructionalMethods = await InstructionalMethodsAsync(bypassCache);
                    if (!allInstructionalMethods.Any())
                    {  
                        IntegrationApiExceptionAddError(string.Concat("No instructional methods found."),
                            "GUID.Not.Found", entity.Guid, entity.Id);
                    }
                }
                catch (Exception ex) // could not get all instructional methods
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                }
            }

            // New object InstructionalMethods with v16
            if (entity.InstructionalContacts != null && entity.InstructionalContacts.Any())
            {
                sectionDto.InstructionalMethods = new List<GuidObject2>();
                var instructionalMethodDetails = new List<InstructionalMethodDetailsDtoProperty>();
                if (allInstructionalMethods != null && allInstructionalMethods.Any())
                {
                    try
                    {
                        var allContactMeasures = await AllContactMeasuresAsync(bypassCache);
                        if (allContactMeasures.Any())
                        {
                            foreach (var method in entity.InstructionalContacts)
                            {
                                if (method != null)
                                {
                                    ContactHoursPeriod? instrMethodPeriod = ContactHoursPeriod.NotSet;
                                    decimal? instrMethodHours = null;

                                    var instructionalMethod = allInstructionalMethods.FirstOrDefault(x => x.Code == method.InstructionalMethodCode);
                                    if (instructionalMethod == null || string.IsNullOrEmpty(instructionalMethod.Guid))
                                    {
                                        IntegrationApiExceptionAddError(string.Concat("Missing instructional method GUID or invalid code of ",
                                            method.InstructionalMethodCode, "."), "GUID.Not.Found", entity.Guid, entity.Id);
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(method.ContactMeasure))
                                        {
                                            var interval = allContactMeasures.FirstOrDefault(x => x.Code == method.ContactMeasure);
                                            if (interval == null)
                                            {
                                                IntegrationApiExceptionAddError(string.Concat("Missing contact measures GUID or invalid code of ",
                                            method.ContactMeasure, "."), "GUID.Not.Found", entity.Guid, entity.Id);
                                            }
                                            else
                                            {
                                                instrMethodPeriod = ConvertContactPeriodToInterval(interval.ContactPeriod);


                                                if ((method.ContactHours.HasValue) || ((instrMethodPeriod != null) || (instrMethodPeriod != ContactHoursPeriod.NotSet)))
                                                {
                                                    instrMethodHours = method.ContactHours;
                                                }
                                                if (instrMethodHours != null && instrMethodHours.HasValue)
                                                {
                                                    try
                                                    {
                                                        var adminInstructionalMethods = await AdministrativeInstructionalMethodsAsync(bypassCache);
                                                        if (adminInstructionalMethods.Any())
                                                        {
                                                            var adminInstructionalMethod = adminInstructionalMethods.FirstOrDefault(x => x.Code == method.InstructionalMethodCode);
                                                            if (adminInstructionalMethod == null || string.IsNullOrEmpty(adminInstructionalMethod.Guid))
                                                            {
                                                                IntegrationApiExceptionAddError(string.Concat("Missing administrative instructional method GUID or invalid code of ",
                                            method.InstructionalMethodCode, "."), "GUID.Not.Found", entity.Guid, entity.Id);
                                                            }
                                                            else
                                                            {
                                                                var instrMethodHoursDetail = new CoursesHoursDtoProperty()
                                                                {
                                                                    AdministrativeInstructionalMethod = new GuidObject2(adminInstructionalMethod.Guid),
                                                                    Minimum = instrMethodHours
                                                                };
                                                                if (instrMethodPeriod != ContactHoursPeriod.NotSet)
                                                                    instrMethodHoursDetail.Interval = instrMethodPeriod;
                                                                if (sectionDto.Hours == null)
                                                                {
                                                                    sectionDto.Hours = new List<CoursesHoursDtoProperty>();
                                                                }
                                                                sectionDto.Hours.Add(instrMethodHoursDetail);
                                                            }

                                                        }
                                                        else
                                                        {
                                                            IntegrationApiExceptionAddError(string.Concat("No administrative instructional methods found."),
                                                                "GUID.Not.Found", entity.Guid, entity.Id);
                                                        }
                                                    }
                                                    catch (Exception ex) // could not get all contact measures
                                                    {
                                                        IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            IntegrationApiExceptionAddError(string.Concat("No contact measures found."),
                                "GUID.Not.Found", entity.Guid, entity.Id);
                        }
                    }
                    catch (Exception ex) // could not get all contact measures
                    {
                        IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                    }

                }
            }
            if (entity.InstructionalMethods != null && entity.InstructionalMethods.Any())
            {
                if (allInstructionalMethods != null && allInstructionalMethods.Any())
                {
                    foreach (var method in entity.InstructionalMethods)
                    {
                        if (method != null)
                        {
                            var instructionalMethod = allInstructionalMethods.FirstOrDefault(x => x.Code == method);
                            if (instructionalMethod != null && !string.IsNullOrEmpty(instructionalMethod.Guid))
                            {
                                var item = new GuidObject2(instructionalMethod.Guid);
                                if (!sectionDto.InstructionalMethods.Contains(item))
                                    sectionDto.InstructionalMethods.Add(new GuidObject2(instructionalMethod.Guid));
                            }
                            else
                            {
                                IntegrationApiExceptionAddError(string.Concat("Missing instructional method GUID or invalid code of ",
                                                instructionalMethod.Code, "."), "GUID.Not.Found", entity.Guid, entity.Id);
                            }
                        }
                    }
                }
            }

            //ChargeAssessmentMethod

            if (!string.IsNullOrEmpty(entity.BillingMethod))
            {
                try
                {
                    var allChargeAssessmentMethods = await this.AllChargeAssessmentMethodAsync(bypassCache);
                    if (allChargeAssessmentMethods.Any())
                    {
                        var chargeAssessmentMethod = allChargeAssessmentMethods.FirstOrDefault(x => x.Code == entity.BillingMethod);
                        if (chargeAssessmentMethod != null && !string.IsNullOrEmpty(chargeAssessmentMethod.Guid))
                        {
                            sectionDto.ChargeAssessmentMethod = new GuidObject2(chargeAssessmentMethod.Guid);
                        }
                        else
                        {
                            IntegrationApiExceptionAddError(string.Concat("Missing charge assessment GUID or invalid code of ",
                                            chargeAssessmentMethod.Code, "."), "GUID.Not.Found", entity.Guid, entity.Id);
                        }
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Concat("No charge assessments found."),
                            "GUID.Not.Found", entity.Guid, entity.Id);
                    }
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                }               
            }

            // New property for alternate Ids
            sectionDto.AlternateIds = new List<SectionsAlternateIdsDtoProperty>()
            {
                new SectionsAlternateIdsDtoProperty()
                {
                    Title = "Source Key",
                    Value = entity.Id
                }
            };

            if ((entity.Meetings != null) && (entity.Meetings.Any()))
            {
                if (allInstructionalMethods != null && allInstructionalMethods.Any())
                {
                    var instructionalEventsList = new List<InstructionalEventDtoProperty3>();
                    var instructorRosterList = new List<InstructorRosterDtoProperty2>();

                    foreach (var eventInstructional in entity.Meetings)
                    {
                        if (eventInstructional.Guid != null)
                        {
                            var returnInstructionalEventDto = new InstructionalEventDtoProperty3();

                            var instEventDto = await ConvertSectionMeetingToInstructionalEvent2_5Async(eventInstructional, entity);

                            returnInstructionalEventDto.Detail = new GuidObject2(instEventDto.Id);
                            returnInstructionalEventDto.Title = instEventDto.Title;
                            var instMethod = (allInstructionalMethods.FirstOrDefault(im => im.Guid == instEventDto.InstructionalMethod.Id));

                            if (instMethod != null)
                            {
                                returnInstructionalEventDto.InstructionalMethod = new InstructionalMethodDtoProperty()
                                {
                                    Abbreviation = instMethod.Code,
                                    Title = instMethod.Description,
                                    Detail = new GuidObject2(instMethod.Guid)
                                };
                            }
                            else
                            {
                                IntegrationApiExceptionAddError(string.Concat("Missing instructional method GUID or invalid code of ",
                                                instMethod.Code, "."), "GUID.Not.Found", entity.Guid, entity.Id);
                            }

                            if (instEventDto.Recurrence != null)
                            {
                                returnInstructionalEventDto.Recurrence = instEventDto.Recurrence;
                            }

                            if ((instEventDto.Locations != null) && (instEventDto.Locations.Any()))
                            {
                                var locationsList = new List<LocationDtoProperty>();
                                var locationDtoProperty = new LocationDtoProperty();
                                var locationRoom = new LocationRoomDtoProperty();
                                try
                                {
                                    var rooms = await RoomsAsync();
                                    if (rooms.Any())
                                    {
                                        var room = (await RoomsAsync()).FirstOrDefault(r => r.Id == eventInstructional.Room);
                                        if (room != null)
                                        {
                                            locationRoom.LocationType = InstructionalLocationType.InstructionalRoom;
                                            locationRoom.Title = !string.IsNullOrEmpty(room.Name) ? room.Name : null;
                                            locationRoom.Number = !string.IsNullOrEmpty(room.Number) ? room.Number : null;
                                            locationRoom.Floor = !string.IsNullOrEmpty(room.Floor) ? room.Floor : null;
                                            locationRoom.Detail = new GuidObject2(room.Guid);

                                            try
                                            {

                                                var buildings = await BuildingsAsync();
                                                if (buildings.Any())
                                                {
                                                    var building = buildings.FirstOrDefault(b => b.Code == room.BuildingCode);
                                                    if (building != null)
                                                    {
                                                        locationRoom.Building = new BuildingDtoProperty()
                                                        {
                                                            Code = building.Code,
                                                            Title = building.Description,
                                                            Detail = new GuidObject2(building.Guid)
                                                        };
                                                    }
                                                    else
                                                    {
                                                        IntegrationApiExceptionAddError(string.Concat("Missing building GUID or invalid code of ",
                                                                        room.BuildingCode, "."), "GUID.Not.Found", entity.Guid, entity.Id);
                                                    }
                                                }
                                                else
                                                {
                                                    IntegrationApiExceptionAddError(string.Concat("No buildings found."),
                                                        "GUID.Not.Found", entity.Guid, entity.Id);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                                            }

                                            locationDtoProperty.Location = locationRoom;
                                            locationsList.Add(locationDtoProperty);
                                        }
                                        else
                                        {
                                            IntegrationApiExceptionAddError(string.Concat("Missing room  GUID or invalid code of ",
                                                            eventInstructional.Room, "."), "GUID.Not.Found", entity.Guid, entity.Id);
                                        }
                                        returnInstructionalEventDto.Locations = locationsList;
                                    }
                                    else
                                    {
                                        IntegrationApiExceptionAddError(string.Concat("No rooms found."),
                                            "GUID.Not.Found", entity.Guid, entity.Id);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", entity.Guid, entity.Id);
                                }
                            }
                            instructionalEventsList.Add(returnInstructionalEventDto);
                        }
                    }
                    if (instructionalEventsList.Any())
                    {
                        sectionDto.InstructionalEvents = instructionalEventsList;
                    }
                }
            }

            var instructorList = new List<InstructorRosterDtoProperty3>();
            if ((entity.Faculty != null) && (entity.Faculty.Any()))
            {
                if (allInstructionalMethods != null && allInstructionalMethods.Any())
                {
                    foreach (var sectionFaculty in entity.Faculty)
                    {
                        var returnInstructorRoster = new InstructorRosterDtoProperty3
                        {
                            WorkLoadPercentage = sectionFaculty.LoadFactor,
                            ResponsibilityPercentage = sectionFaculty.ResponsibilityPercentage,
                            WorkStartDate = sectionFaculty.StartDate,
                            WorkEndDate = sectionFaculty.EndDate
                        };
                        if (sectionFaculty.PrimaryIndicator)
                        {
                            returnInstructorRoster.InstructorRole = SectionInstructorsInstructorRole.Primary;
                        }
                        // Update instructional method for instructor and any instructional events
                        // that match the instructional method.
                        if (!string.IsNullOrEmpty(sectionFaculty.InstructionalMethodCode))
                        {
                            returnInstructorRoster.InstructionalMethods = new List<GuidObject2>();
                            var instructionalMethod = allInstructionalMethods.FirstOrDefault(x => x.Code == sectionFaculty.InstructionalMethodCode);
                            if (instructionalMethod == null)
                            {
                                IntegrationApiExceptionAddError(string.Concat("Instructional Method not found for :", sectionFaculty.InstructionalMethodCode), "Bad.Data", entity.Guid, entity.Id);
                            }
                            var item = new GuidObject2(instructionalMethod.Guid);
                            if (!returnInstructorRoster.InstructionalMethods.Contains(item))
                                returnInstructorRoster.InstructionalMethods.Add(new GuidObject2(instructionalMethod.Guid));
                            if (sectionDto.InstructionalEvents != null && sectionDto.InstructionalEvents.Any())
                            {
                                var instrEvents = sectionDto.InstructionalEvents.Where(ie => ie.InstructionalMethod != null &&
                                    ie.InstructionalMethod.Detail != null && !string.IsNullOrEmpty(ie.InstructionalMethod.Detail.Id) &&
                                    ie.InstructionalMethod.Detail.Id == instructionalMethod.Guid);
                                if (instrEvents != null && instrEvents.Any())
                                {
                                    returnInstructorRoster.InstructionalEvents = new List<GuidObject2>();
                                    returnInstructorRoster.InstructionalEvents = instrEvents.Select(ie => ie.Detail).ToList();
                                }
                            }
                        }
                        else
                        {
                            if (sectionDto.InstructionalEvents != null && sectionDto.InstructionalEvents.Any())
                            {
                                var instrEvents = sectionDto.InstructionalEvents.Where(ie => ie.InstructionalMethod != null &&
                                ie.InstructionalMethod.Detail != null && !string.IsNullOrEmpty(ie.InstructionalMethod.Detail.Id));
                                if (instrEvents != null && instrEvents.Any())
                                {
                                    returnInstructorRoster.InstructionalMethods = new List<GuidObject2>();
                                    returnInstructorRoster.InstructionalEvents = new List<GuidObject2>();
                                    returnInstructorRoster.InstructionalMethods = instrEvents.Select(ie => ie.InstructionalMethod.Detail).ToList();
                                    returnInstructorRoster.InstructionalEvents = instrEvents.Select(ie => ie.Detail).ToList();
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(sectionFaculty.FacultyId))
                        {
                            IntegrationApiExceptionAddError("Faculty ID is a required arguyment", "Bad.Data", entity.Guid, entity.Id);
                        }

                        string personGuid = null;
                        if (_facultyGuids != null && _facultyGuids.Any())
                        {
                            _facultyGuids.TryGetValue(sectionFaculty.FacultyId, out personGuid);
                        }

                        if (personGuid == null)
                        {
                            IntegrationApiExceptionAddError("No GUID found for person ID " + sectionFaculty.FacultyId, "GUID.Not.Found", entity.Guid, entity.Id);
                        }
                        Domain.Base.Entities.PersonIntegration person = await GetPersonDataForFacultyByGuidAsync(personGuid);

                        if (person == null)
                        {
                            IntegrationApiExceptionAddError("Unable to retrieve person record, or duplicate GUID, found for person ID " + sectionFaculty.FacultyId, "GUID.Not.Found", entity.Guid, entity.Id);
                        }


                        var returnInstructor = new InstructorDtoProperty3 { Detail = new GuidObject2(personGuid) };

                        returnInstructor.Names = await GetInstructorNames(person, bypassCache);

                        var credentialList = new List<InstructorCredential3DtoProperty>();

                        credentialList.Add(new InstructorCredential3DtoProperty()
                        {
                            Type = Dtos.EnumProperties.Credential3Type.ColleaguePersonId,
                            Value = person.Id
                        });

                        // Elevate ID
                        if (person.PersonAltIds != null && person.PersonAltIds.Any())
                        {
                            // Allow more than one Elevate ID as of v12.1.0
                            var elevPersonAltIdList =
                                person.PersonAltIds.Where(
                                    a => a.Type == Domain.Base.Entities.PersonAlt.ElevatePersonAltType);
                            if (elevPersonAltIdList != null && elevPersonAltIdList.Any())
                            {
                                foreach (var elevPersonAltId in elevPersonAltIdList)
                                {
                                    if (elevPersonAltId != null && !string.IsNullOrEmpty(elevPersonAltId.Id))
                                    {
                                        credentialList.Add(new Dtos.DtoProperties.InstructorCredential3DtoProperty()
                                        {
                                            Type = Dtos.EnumProperties.Credential3Type.ElevateID,
                                            Value = elevPersonAltId.Id
                                        });
                                    }
                                }
                            }
                        }
                        // SSN
                        if (!string.IsNullOrEmpty(person.GovernmentId))
                        {
                            var type = Dtos.EnumProperties.Credential3Type.TaxIdentificationNumber;
                            credentialList.Add(new InstructorCredential3DtoProperty()
                            {
                                Type = type,
                                Value = person.GovernmentId
                            });
                        }
                        //PERSON.PIN
                        if (_personPins != null)
                        {
                            var personPinEntity = _personPins.FirstOrDefault(i => i.PersonId.Equals(person.Id, StringComparison.OrdinalIgnoreCase));
                            if ((personPinEntity != null) && (!string.IsNullOrEmpty(personPinEntity.PersonPinUserId)))
                            {
                                credentialList.Add(new InstructorCredential3DtoProperty()
                                {
                                    Type = Dtos.EnumProperties.Credential3Type.ColleagueUserName,
                                    Value = personPinEntity.PersonPinUserId
                                });
                            }
                        }

                        returnInstructor.Credentials = credentialList;
                        returnInstructorRoster.Instructor = returnInstructor;
                        instructorList.Add(returnInstructorRoster);
                    }
                }
            }
            if (instructorList.Any())
            {
                sectionDto.InstructorRoster = instructorList;
            }

            return sectionDto;
        }

        private async Task<List<InstructorNameDtoProperty2>> GetInstructorNames(Domain.Base.Entities.PersonIntegration person, bool bypassCache = false)
        {
            List<Dtos.DtoProperties.InstructorNameDtoProperty2> personNames =
                new List<Dtos.DtoProperties.InstructorNameDtoProperty2>();

            var preferredNameType = person.PreferredNameType;

            // Legal Name for a person
            var personNameTypeItem = (await GetPersonNameTypesAsync(bypassCache)).FirstOrDefault(
                pn => pn.Code == "LEGAL");
            if (personNameTypeItem != null)
            {
                var personName = new Dtos.DtoProperties.InstructorNameDtoProperty2()
                {
                    NameType = new Dtos.DtoProperties.InstructorNameTypeDtoProperty()
                    {
                        Category = Dtos.EnumProperties.InstructorNameType2.Legal,
                        Detail =
                            new Dtos.GuidObject2(
                                    personNameTypeItem.Guid)
                    },
                    FullName =
                        !string.IsNullOrEmpty(preferredNameType) &&
                            preferredNameType.Equals("LEGAL", StringComparison.CurrentCultureIgnoreCase) &&
                            !string.IsNullOrEmpty(person.PreferredName) ?
                            person.PreferredName :
                        BuildFullName("FM", person.Prefix, person.FirstName, person.MiddleName,
                            person.LastName, person.Suffix),
                    Preference = (string.IsNullOrEmpty(preferredNameType) || preferredNameType.Equals("LEGAL", StringComparison.CurrentCultureIgnoreCase)) ?
                            Dtos.EnumProperties.PersonNamePreference.Preferred : (PersonNamePreference?)null,
                    Title = string.IsNullOrEmpty(person.Prefix) ? null : person.Prefix,
                    FirstName = string.IsNullOrEmpty(person.FirstName) ? null : person.FirstName,
                    MiddleName = string.IsNullOrEmpty(person.MiddleName) ? null : person.MiddleName,
                    LastName = string.IsNullOrEmpty(person.LastName) ? null : person.LastName,
                    LastNamePrefix =
                        person.LastName.Contains(" ")
                            ? person.LastName.Split(" ".ToCharArray())[0].ToString()
                            : null,
                    Pedigree = string.IsNullOrEmpty(person.Suffix) ? null : person.Suffix,
                    ProfessionalAbbreviation =
                        person.ProfessionalAbbreviations.Any() ? person.ProfessionalAbbreviations : null
                };
                personNames.Add(personName);
            }

            // Birth Name
            if (!string.IsNullOrEmpty(person.BirthNameLast) || !string.IsNullOrEmpty(person.BirthNameFirst) ||
                !string.IsNullOrEmpty(person.BirthNameMiddle))
            {
                var birthNameTypeItem = (await GetPersonNameTypesAsync(bypassCache))
                    .FirstOrDefault(pn => pn.Code == "BIRTH");
                if (birthNameTypeItem != null)
                {
                    var birthName = new Dtos.DtoProperties.InstructorNameDtoProperty2()
                    {
                        NameType = new Dtos.DtoProperties.InstructorNameTypeDtoProperty()
                        {
                            Category = Dtos.EnumProperties.InstructorNameType2.Birth,
                            Detail =
                                new Dtos.GuidObject2(
                                        birthNameTypeItem.Guid)
                        },
                        FullName =
                            !string.IsNullOrEmpty(preferredNameType) &&
                            preferredNameType.Equals("BIRTH", StringComparison.CurrentCultureIgnoreCase) &&
                            !string.IsNullOrEmpty(person.PreferredName) ?
                            person.PreferredName :
                            BuildFullName("FM", "", person.BirthNameFirst, person.BirthNameMiddle, person.BirthNameLast, ""),
                        FirstName = string.IsNullOrEmpty(person.BirthNameFirst) ? null : person.BirthNameFirst,
                        MiddleName = string.IsNullOrEmpty(person.BirthNameMiddle) ? null : person.BirthNameMiddle,
                        LastName = string.IsNullOrEmpty(person.BirthNameLast) ? null : person.BirthNameLast,
                        LastNamePrefix =
                            person.BirthNameLast.Contains(" ")
                                ? person.BirthNameLast.Split(" ".ToCharArray())[0].ToString()
                                    : null,
                        Preference = !string.IsNullOrEmpty(preferredNameType) && preferredNameType.Equals("BIRTH", StringComparison.CurrentCultureIgnoreCase) ?
                            Dtos.EnumProperties.PersonNamePreference.Preferred : (PersonNamePreference?)null,
                    };
                    personNames.Add(birthName);
                }
            }

            // Chosen Name
            if (!string.IsNullOrEmpty(person.ChosenLastName) || !string.IsNullOrEmpty(person.ChosenFirstName) ||
                !string.IsNullOrEmpty(person.ChosenMiddleName))
            {
                var chosenNameTypeItem = (await GetPersonNameTypesAsync(bypassCache))
                    .FirstOrDefault(pn => pn.Code == "CHOSEN");
                if (chosenNameTypeItem != null)
                {
                    var chosenName = new Dtos.DtoProperties.InstructorNameDtoProperty2()
                    {
                        NameType = new Dtos.DtoProperties.InstructorNameTypeDtoProperty()
                        {
                            Category = Dtos.EnumProperties.InstructorNameType2.Favored,
                            Detail =
                                new Dtos.GuidObject2(
                                        chosenNameTypeItem.Guid)
                        },
                        FullName =
                            !string.IsNullOrEmpty(preferredNameType) &&
                            preferredNameType.Equals("CHOSEN", StringComparison.CurrentCultureIgnoreCase) &&
                            !string.IsNullOrEmpty(person.PreferredName) ?
                            person.PreferredName :
                            BuildFullName("FM", "", person.ChosenFirstName, person.ChosenMiddleName, person.ChosenLastName, ""),
                        FirstName = string.IsNullOrEmpty(person.ChosenFirstName) ? null : person.ChosenFirstName,
                        MiddleName = string.IsNullOrEmpty(person.ChosenMiddleName) ? null : person.ChosenMiddleName,
                        LastName = string.IsNullOrEmpty(person.ChosenLastName) ? null : person.ChosenLastName,
                        LastNamePrefix =
                            person.ChosenLastName.Contains(" ")
                                ? person.ChosenLastName.Split(" ".ToCharArray())[0].ToString()
                                    : null,
                        Preference = !string.IsNullOrEmpty(preferredNameType) && preferredNameType.Equals("CHOSEN", StringComparison.CurrentCultureIgnoreCase) ?
                            Dtos.EnumProperties.PersonNamePreference.Preferred : (PersonNamePreference?)null,
                    };
                    personNames.Add(chosenName);
                }
            }

            // Nickname
            if (!string.IsNullOrEmpty(person.Nickname))
            {
                var nickNameTypeItem = (await GetPersonNameTypesAsync(false))
                    .FirstOrDefault(pn => pn.Code == "NICKNAME");
                if (nickNameTypeItem != null)
                {
                    var nickName = new Dtos.DtoProperties.InstructorNameDtoProperty2()
                    {
                        NameType = new Dtos.DtoProperties.InstructorNameTypeDtoProperty()
                        {
                            Category = Dtos.EnumProperties.InstructorNameType2.Personal,
                            Detail =
                                new Dtos.GuidObject2(
                                        nickNameTypeItem.Guid)
                        },
                        FullName = person.Nickname,
                        Preference = !string.IsNullOrEmpty(preferredNameType) && preferredNameType.Equals("NICKNAME", StringComparison.CurrentCultureIgnoreCase) ?
                           Dtos.EnumProperties.PersonNamePreference.Preferred : (PersonNamePreference?)null,
                    };

                    personNames.Add(nickName);
                }
            }

            // Name History
            if ((person.FormerNames != null) && (person.FormerNames.Any()))
            {
                var historyNameTypeItem = (await GetPersonNameTypesAsync(false))
                    .FirstOrDefault(pn => pn.Code == "HISTORY");
                if (historyNameTypeItem != null)
                {
                    foreach (var name in person.FormerNames)
                    {
                        var formerName = new Dtos.DtoProperties.InstructorNameDtoProperty2()
                        {
                            NameType = new Dtos.DtoProperties.InstructorNameTypeDtoProperty()
                            {
                                Category = Dtos.EnumProperties.InstructorNameType2.Personal,
                                Detail =
                                    new Dtos.GuidObject2(
                                            historyNameTypeItem.Guid)
                            },
                            FullName = BuildFullName("FM", "", name.GivenName, name.MiddleName, name.FamilyName, ""),
                            FirstName = string.IsNullOrEmpty(name.GivenName) ? null : name.GivenName,
                            MiddleName = string.IsNullOrEmpty(name.MiddleName) ? null : name.MiddleName,
                            LastName = string.IsNullOrEmpty(name.FamilyName) ? null : name.FamilyName,
                            LastNamePrefix =
                                name.FamilyName.Contains(" ")
                                    ? name.FamilyName.Split(" ".ToCharArray())[0].ToString()
                                    : null
                        };
                        personNames.Add(formerName);
                    }
                }
            }
            return personNames;
        }

        #endregion EEDM sections-maximum Version 11

        /// <summary>
        /// Helper method to determine if integration statuses have been configured
        /// </summary>
        private async Task ValidateSectionStatusConfigurationAsync(SectionStatus2? status = null)
        {
            if (status != null)
            {
                var sectionStatusCodes = (await SectionStatusesAsync()).ToList();
                SectionStatusIntegration? sectionStatusIntegration;

                switch (status)
                {
                    case SectionStatus2.Open:
                        sectionStatusIntegration = SectionStatusIntegration.Open;
                        break;
                    case SectionStatus2.Closed:
                        sectionStatusIntegration = SectionStatusIntegration.Closed;
                        break;
                    case SectionStatus2.Cancelled:
                        sectionStatusIntegration = SectionStatusIntegration.Cancelled;
                        break;
                    case SectionStatus2.Pending:
                        sectionStatusIntegration = SectionStatusIntegration.Pending;
                        break;
                    default:
                        sectionStatusIntegration = null;
                        break;
                }

                if (!(sectionStatusCodes.Any(x => x.IntegrationStatusType == sectionStatusIntegration)))
                {
                    const string errorMessage = "SECTION.STATUSES must be configured for use in integration";
                    logger.Info(errorMessage);
                    throw new Exception(errorMessage);
                }
            }
        }   
        #region Instructional Events Version 6

        /// <summary>
        /// Get an instructional event
        /// </summary>
        /// <param name="id">GUID of the event</param>
        /// <returns>InstructionalEvent DTO</returns>
        public async Task<Dtos.InstructionalEvent2> GetInstructionalEvent2Async(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            var entity = await _sectionRepository.GetSectionMeetingByGuidAsync(id);
            if (entity == null)
            {
                throw new KeyNotFoundException(string.Format("Record not found, invalid id provided: {0}", id));
            }
            var dto = await ConvertSectionMeetingToInstructionalEvent2Async(entity);
            return dto;
        }

        /// <summary>
        /// Get all instructional events matching on filters passed into this method
        /// </summary>
        /// <param name="section">Section Id</param>
        /// <param name="startOn">Start Date and Time</param>
        /// <param name="endOn">End Date and Time</param>
        /// <param name="room">Room Id</param>
        /// <param name="instructor">Instructor Id</param>
        /// <returns>List of InstructionalEvent objects</returns>
        public async Task<Tuple<IEnumerable<Dtos.InstructionalEvent2>, int>> GetInstructionalEvent2Async(int offset, int limit, string section, string startOn, string endOn, string room, string instructor)
        {
            // Convert and validate all input parameters
            string startDate = (startOn == string.Empty ? string.Empty : await ConvertDateArgument(startOn));
            string endDate = (endOn == string.Empty ? string.Empty : await ConvertDateArgument(endOn));
            string startTime = string.Empty;
            string endTime = string.Empty;
            string time = string.Empty;
            string newSection = string.Empty;
            List<string> newBuildings = new List<string>();
            List<string> newRooms = new List<string>();
            List<string> newInstructors = new List<string>();
            try
            {
                time = (startOn == string.Empty ? string.Empty : DateTimeOffset.Parse(startOn).ToLocalTime().TimeOfDay.ToString());
                startTime = (time == "00:00:00" ? string.Empty : time);
                time = (endOn == string.Empty ? string.Empty : DateTimeOffset.Parse(endOn).ToLocalTime().TimeOfDay.ToString());
                endTime = (time == "00:00:00" ? string.Empty : time);
            }
            catch
            {
                // throw new ArgumentException("Invalid time format in date arguments");
                return new Tuple<IEnumerable<InstructionalEvent2>, int>(new List<InstructionalEvent2>(), 0);
            }
            try
            {
                newSection = (section == string.Empty ? string.Empty : await _sectionRepository.GetSectionIdFromGuidAsync(section));
            }
            catch
            {
                // throw new ArgumentException("Invalid section Id argument");
                return new Tuple<IEnumerable<InstructionalEvent2>, int>(new List<InstructionalEvent2>(), 0);
            }
            try
            {
                if (!string.IsNullOrEmpty(room))
                {
                    var newBuilding = (room == string.Empty ? string.Empty : (await _roomRepository.GetRoomsAsync(false)).Where(r => r.Guid == room).First().BuildingCode);
                    var newRoom = (room == string.Empty ? string.Empty : (await _roomRepository.GetRoomsAsync(false)).Where(r => r.Guid == room).First().Code);
                    if (string.IsNullOrEmpty(newBuilding) || string.IsNullOrEmpty(newRoom))
                    {
                        // throw new ArgumentException("Invalid instructor Id argument");
                        return new Tuple<IEnumerable<InstructionalEvent2>, int>(new List<InstructionalEvent2>(), 0);
                    }
                    newBuildings.Add(newBuilding);
                    newRooms.Add(newRoom);
                }
            }
            catch
            {
                // throw new ArgumentException("Invalid room Id argument");
                return new Tuple<IEnumerable<InstructionalEvent2>, int>(new List<InstructionalEvent2>(), 0);
            }
            try
            {
                if (!string.IsNullOrEmpty(instructor))
                {
                    var newInstructor = await _personRepository.GetPersonIdFromGuidAsync(instructor);
                    if (string.IsNullOrEmpty(newInstructor))
                    {
                        // throw new ArgumentException("Invalid instructor Id argument");
                        return new Tuple<IEnumerable<InstructionalEvent2>, int>(new List<InstructionalEvent2>(), 0);
                    }
                    newInstructors.Add(newInstructor);
                }
            }
            catch
            {
                // throw new ArgumentException("Invalid instructor Id argument");
                return new Tuple<IEnumerable<InstructionalEvent2>, int>(new List<InstructionalEvent2>(), 0);
            }

            var instructionalEventDtos = new List<Dtos.InstructionalEvent2>();
            var pageOfItems = await _sectionRepository.GetSectionMeetingAsync(offset, limit, newSection, startDate, endDate, startTime, endTime, newBuildings, newRooms, newInstructors, "");
            var entities = pageOfItems.Item1;

            foreach (var entity in entities)
            {
                if (entity.Guid != null)
                {
                    var sectionDto = await ConvertSectionMeetingToInstructionalEvent2Async(entity);
                    instructionalEventDtos.Add(sectionDto);
                }
                else
                {
                    throw new ArgumentException(string.Format("GUID is missing for COURSE.SEC.MEETING record '{0}'. ", entity.Id), "GUID");
                }
            }
            return new Tuple<IEnumerable<InstructionalEvent2>, int>(instructionalEventDtos, pageOfItems.Item2);
        }

        /// <summary>
        /// Create an instructional event
        /// </summary>
        /// <param name="meeting">The event to create</param>
        /// <returns>InstructionalEvent DTO</returns>
        public async Task<Dtos.InstructionalEvent2> CreateInstructionalEvent2Async(Dtos.InstructionalEvent2 meeting)
        {
            if (meeting == null)
            {
                throw new ArgumentNullException("meeting");
            }

            CheckInstructionalEvent2Permissions(meeting);

            _sectionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            var section = await ConvertInstructionalEvent2ToSectionMeetingAsync(meeting);
            var entity = await _sectionRepository.PostSectionMeetingAsync(section, meeting.Id);
            var dto = await ConvertSectionMeetingToInstructionalEvent2Async(entity);

            return dto;
        }

        /// <summary>
        /// Update an instructional event
        /// </summary>
        /// <param name="meeting">The event to update</param>
        /// <returns>InstructionalEvent DTO</returns>
        public async Task<Dtos.InstructionalEvent2> UpdateInstructionalEvent2Async(Dtos.InstructionalEvent2 meeting)
        {
            if (meeting == null)
            {
                throw new ArgumentNullException("meeting");
            }

            CheckInstructionalEvent2Permissions(meeting);

            _sectionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            var section = await ConvertInstructionalEvent2ToSectionMeetingAsync(meeting);
            var updatedEntity = await _sectionRepository.PutSectionMeetingAsync(section, meeting.Id);
            var dto = await ConvertSectionMeetingToInstructionalEvent2Async(updatedEntity);

            return dto;
        }

        #endregion

        #region Instructional Events Version 8

        /// <summary>
        /// Get an instructional event
        /// </summary>
        /// <param name="id">GUID of the event</param>
        /// <returns>InstructionalEvent DTO</returns>
        public async Task<Dtos.InstructionalEvent3> GetInstructionalEvent3Async(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            var entity = await _sectionRepository.GetSectionMeetingByGuidAsync(id);
            if (entity == null)
            {
                throw new KeyNotFoundException(string.Format("Record not found, invalid id provided: {0}", id));
            }
            var dto = await ConvertSectionMeetingToInstructionalEvent3Async(entity);
            return dto;
        }

        /// <summary>
        /// Get all instructional events matching on filters passed into this method
        /// </summary>
        /// <param name="section">Section Id</param>
        /// <param name="startOn">Start Date and Time</param>
        /// <param name="endOn">End Date and Time</param>
        /// <param name="roomList">Room Id</param>
        /// <param name="instructorList">Instructor Id</param>
        /// <returns>List of InstructionalEvent objects</returns>
        public async Task<Tuple<IEnumerable<Dtos.InstructionalEvent3>, int>> GetInstructionalEvent3Async(int offset, int limit, string section, string startOn, string endOn, List<string> roomList, List<string> instructorList, string academicPeriod)
        {
            // Convert and validate all input parameters
            string startDate = (startOn == string.Empty ? string.Empty : await ConvertDateArgument(startOn));
            string endDate = (endOn == string.Empty ? string.Empty : await ConvertDateArgument(endOn));
            string startTime = string.Empty;
            string endTime = string.Empty;
            string time = string.Empty;
            string newSection = string.Empty;
            string newTerm = string.Empty;
            List<string> newBuildings = new List<string>();
            List<string> newRooms = new List<string>();
            List<string> newInstructors = new List<string>();
            try
            {
                time = (startOn == string.Empty ? string.Empty : DateTimeOffset.Parse(startOn).ToLocalTime().TimeOfDay.ToString());
                startTime = (time == "00:00:00" ? string.Empty : time);
                time = (endOn == string.Empty ? string.Empty : DateTimeOffset.Parse(endOn).ToLocalTime().TimeOfDay.ToString());
                endTime = (time == "00:00:00" ? string.Empty : time);
            }
            catch
            {
                //no results
                return new Tuple<IEnumerable<Dtos.InstructionalEvent3>, int>(new List<Dtos.InstructionalEvent3>(), 0);
                //throw new ArgumentException("Invalid time format in date arguments");
            }
            try
            {
                newSection = (section == string.Empty ? string.Empty : await _sectionRepository.GetSectionIdFromGuidAsync(section));
            }
            catch
            {
                //no results
                return new Tuple<IEnumerable<Dtos.InstructionalEvent3>, int>(new List<Dtos.InstructionalEvent3>(), 0);
                //throw new ArgumentException("Invalid section Id argument");
            }
            try
            {
                foreach (var room in roomList)
                {
                    var newBuilding = (room == string.Empty ? string.Empty : (await _roomRepository.GetRoomsAsync(false)).Where(r => r.Guid == room).First().BuildingCode);
                    var newRoom = (room == string.Empty ? string.Empty : (await _roomRepository.GetRoomsAsync(false)).Where(r => r.Guid == room).First().Code);
                    if (!string.IsNullOrEmpty(newBuilding) && !string.IsNullOrEmpty(newRoom))
                    {
                        newBuildings.Add(newBuilding);
                        newRooms.Add(newRoom);
                    }
                }
            }
            catch
            {
                //no results
                return new Tuple<IEnumerable<Dtos.InstructionalEvent3>, int>(new List<Dtos.InstructionalEvent3>(), 0);
                //throw new ArgumentException("Invalid room Id argument");
            }
            // If any room failed validation then return an empty set (try/catch should have caught this one).
            if (roomList.Count != newRooms.Count)
            {
                //no results
                return new Tuple<IEnumerable<Dtos.InstructionalEvent3>, int>(new List<Dtos.InstructionalEvent3>(), 0);
                //throw new ArgumentException("Invalid room Id argument");
            }
            try
            {
                foreach (var instructor in instructorList)
                {
                    var newInstructor = (instructor == string.Empty ? string.Empty : await _personRepository.GetPersonIdFromGuidAsync(instructor));
                    if (!string.IsNullOrEmpty(newInstructor))
                    {
                        newInstructors.Add(newInstructor);
                    }
                }
            }
            catch
            {
                //no results
                return new Tuple<IEnumerable<Dtos.InstructionalEvent3>, int>(new List<Dtos.InstructionalEvent3>(), 0);
                //throw new ArgumentException("Invalid instructor Id argument");
            }
            // If any of the instructors failed validation, then return empty set (AND is implied)
            if (instructorList.Count != newInstructors.Count)
            {
                //no results
                return new Tuple<IEnumerable<Dtos.InstructionalEvent3>, int>(new List<Dtos.InstructionalEvent3>(), 0);
                //throw new ArgumentException("Invalid instructor Id argument");
            }

            if (!string.IsNullOrEmpty(academicPeriod))
            {
                try
                {
                    var acadPeriod = academicPeriods.FirstOrDefault(ap => ap.Guid.Equals(academicPeriod, StringComparison.OrdinalIgnoreCase));
                    if (acadPeriod == null)
                    {
                        //no results
                        return new Tuple<IEnumerable<Dtos.InstructionalEvent3>, int>(new List<Dtos.InstructionalEvent3>(), 0);
                        //throw new KeyNotFoundException(string.Format("Academic period not found, invalid id provided: {0}", academicPeriod));
                    }

                    newTerm = acadPeriod.Code;
                }
                catch
                {
                    //no results
                    return new Tuple<IEnumerable<Dtos.InstructionalEvent3>, int>(new List<Dtos.InstructionalEvent3>(), 0);
                }
            }

            var instructionalEventDtos = new List<Dtos.InstructionalEvent3>();
            var pageOfItems = await _sectionRepository.GetSectionMeetingAsync(offset, limit, newSection, startDate, endDate, startTime, endTime, newBuildings, newRooms, newInstructors, newTerm);
            var entities = pageOfItems.Item1;

            foreach (var entity in entities)
            {
                if (entity.Guid != null)
                {
                    var sectionDto = await ConvertSectionMeetingToInstructionalEvent3Async(entity);
                    instructionalEventDtos.Add(sectionDto);
                }
                else
                {
                    throw new ArgumentException(string.Format("GUID is missing for COURSE.SEC.MEETING record '{0}'. ", entity.Id), "GUID");
                }
            }
            return new Tuple<IEnumerable<InstructionalEvent3>, int>(instructionalEventDtos, pageOfItems.Item2);
        }


        /// <summary>
        /// Create an instructional event
        /// </summary>
        /// <param name="meeting">The event to create</param>
        /// <returns>InstructionalEvent DTO</returns>
        public async Task<Dtos.InstructionalEvent3> CreateInstructionalEvent3Async(Dtos.InstructionalEvent3 meeting)
        {
            if (meeting == null)
            {
                throw new ArgumentNullException("meeting");
            }

            CheckInstructionalEvent3Permissions(meeting);

            _sectionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            var section = await ConvertInstructionalEvent3ToSectionMeetingAsync(meeting);
            var entity = await _sectionRepository.PostSectionMeetingAsync(section, meeting.Id);
            var dto = await ConvertSectionMeetingToInstructionalEvent3Async(entity);

            return dto;
        }

        /// <summary>
        /// Update an instructional event
        /// </summary>
        /// <param name="meeting">The event to update</param>
        /// <returns>InstructionalEvent DTO</returns>
        public async Task<Dtos.InstructionalEvent3> UpdateInstructionalEvent3Async(Dtos.InstructionalEvent3 meeting)
        {
            if (meeting == null)
            {
                throw new ArgumentNullException("meeting");
            }

            CheckInstructionalEvent3Permissions(meeting);

            _sectionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            var section = await ConvertInstructionalEvent3ToSectionMeetingAsync(meeting);
            var updatedEntity = await _sectionRepository.PutSectionMeetingAsync(section, meeting.Id);
            var dto = await ConvertSectionMeetingToInstructionalEvent3Async(updatedEntity);

            return dto;
        }

        #endregion

        #region Instructional Events Version 11

        /// <summary>
        /// Get all instructional events matching on filters passed into this method V11
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="section"></param>
        /// <param name="startOn"></param>
        /// <param name="endOn"></param>
        /// <param name="academicPeriod"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Dtos.InstructionalEvent4>, int>> GetInstructionalEvent4Async(int offset, int limit, string section, string startOn, string endOn, string academicPeriod)
        {
            // Convert and validate all input parameters
            string startDate = (string.IsNullOrEmpty(startOn) ? string.Empty : await ConvertDateArgument(startOn));
            string endDate = (string.IsNullOrEmpty(endOn) ? string.Empty : await ConvertDateArgument(endOn));
            string startTime = string.Empty;
            string endTime = string.Empty;
            string time = string.Empty;
            string newSection = string.Empty;
            string newAcademicPeriod = string.Empty;
            string newTerm = string.Empty;
            try
            {
                time = (string.IsNullOrEmpty(startOn) ? string.Empty : DateTimeOffset.Parse(startOn).ToLocalTime().TimeOfDay.ToString());
                startTime = (time == "00:00:00" ? string.Empty : time);
                time = (string.IsNullOrEmpty(endOn) ? string.Empty : DateTimeOffset.Parse(endOn).ToLocalTime().TimeOfDay.ToString());
                endTime = (time == "00:00:00" ? string.Empty : time);
            }
            catch
            {
                //no results
                return new Tuple<IEnumerable<Dtos.InstructionalEvent4>, int>(new List<Dtos.InstructionalEvent4>(), 0);
            }

            if (!string.IsNullOrEmpty(section))
            {
                try
                {
                    newSection = await _sectionRepository.GetSectionIdFromGuidAsync(section);
                    if (string.IsNullOrEmpty(newSection))
                    {
                        //no results
                        return new Tuple<IEnumerable<Dtos.InstructionalEvent4>, int>(new List<Dtos.InstructionalEvent4>(), 0);
                    }
                }
                catch
                {
                    //no results
                    return new Tuple<IEnumerable<Dtos.InstructionalEvent4>, int>(new List<Dtos.InstructionalEvent4>(), 0);
                }
            }

            if (!string.IsNullOrEmpty(academicPeriod))
            {
                try
                {
                    newTerm = await _termRepository.GetAcademicPeriodsCodeFromGuidAsync(academicPeriod);
                    if (string.IsNullOrEmpty(newTerm))
                    {
                        //no results
                        return new Tuple<IEnumerable<Dtos.InstructionalEvent4>, int>(new List<Dtos.InstructionalEvent4>(), 0);
                    }
                }
                catch
                {
                    //no results
                    return new Tuple<IEnumerable<Dtos.InstructionalEvent4>, int>(new List<Dtos.InstructionalEvent4>(), 0);
                }
            }

            var instructionalEventDtos = new List<Dtos.InstructionalEvent4>();
            Tuple<IEnumerable<SectionMeeting>, int> pageOfItems = null;

            try
            {
                pageOfItems = await _sectionRepository.GetSectionMeeting2Async(offset, limit, newSection, startDate, endDate, startTime, endTime,
                    new List<string>(), new List<string>(), new List<string>(), newTerm);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("An unexpected exception occurred retrieving instructional-events from the database" + ex.Message);
                throw IntegrationApiException;
            }

            var entities = pageOfItems.Item1;

            if (entities == null || !entities.Any())
            {
                return new Tuple<IEnumerable<Dtos.InstructionalEvent4>, int>(new List<Dtos.InstructionalEvent4>(), 0);
            }


            var sectionIds = entities.Where(p => !string.IsNullOrEmpty(p.SectionId)).Select(s => s.SectionId);
            var sectionIdDict = await _sectionRepository.GetSectionGuidsCollectionAsync(sectionIds.Distinct().ToArray());

            foreach (var entity in entities)
            {
                try
                {
                    instructionalEventDtos.Add(await ConvertSectionMeetingToInstructionalEvent4Async(entity, sectionIdDict));
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError("An unexpected error occurred extracting instructional-event." + ex.Message, "Global.Internal.Error", entity.Guid, entity.Id);
                }
            }
            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            return new Tuple<IEnumerable<InstructionalEvent4>, int>(instructionalEventDtos, pageOfItems.Item2);
        }

        /// <summary>
        /// Get an instructional event V11
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<InstructionalEvent4> GetInstructionalEvent4Async(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                IntegrationApiExceptionAddError("Must provide a instructional-events GUID for retrieval.", "Missing.GUID",
                   "", "", System.Net.HttpStatusCode.NotFound);
                throw IntegrationApiException;
            }

            SectionMeeting entity = null;
            try
            {
                entity = await _sectionRepository.GetSectionMeetingByGuidAsync(id);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(string.Format("No instructional-events was found for GUID '{0}'", id), "GUID.Not.Found", id, "",
                   System.Net.HttpStatusCode.NotFound);
                throw IntegrationApiException;
            }

            if (entity == null)
            {
                IntegrationApiExceptionAddError(string.Format("No instructional-events was found for GUID '{0}'", id), "GUID.Not.Found", id, "",
                   System.Net.HttpStatusCode.NotFound);
                throw IntegrationApiException;
            }
            InstructionalEvent4 dto = null;
            try
            {
                var sectionIdDict = await _sectionRepository.GetSectionGuidsCollectionAsync(new List<string>() { entity.SectionId });

                dto = await ConvertSectionMeetingToInstructionalEvent4Async(entity, sectionIdDict);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("An unexpected exception occurred extracting response. " + ex.Message, "Global.Internal.Error", id);
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
            return dto;
        }

        /// <summary>
        /// Create an instructional event V11
        /// </summary>
        /// <param name="meeting">The instructional event to create</param>
        /// <returns>InstructionalEvent DTO</returns>
        public async Task<Dtos.InstructionalEvent4> CreateInstructionalEvent4Async(Dtos.InstructionalEvent4 meeting)
        {
            if (meeting == null)
            {
                IntegrationApiExceptionAddError("Missing request body.", "Validation.Exception");
                throw IntegrationApiException;
            }

            CheckInstructionalEvent4Permissions(meeting);

            #region create domain entity from request
            Domain.Student.Entities.Section section = null;
            _sectionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
            try
            {
                section = await ConvertInstructionalEvent4ToSectionMeetingAsync(meeting);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("Record not created.  Error extracting request." + ex.Message, "Global.Internal.Error",
                   !string.IsNullOrEmpty(meeting.Id) ? meeting.Id : null
                  );
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            #endregion

            #region Create record from domain entity
            SectionMeeting entity = null;
            try
            {
                entity = await _sectionRepository.PostSectionMeeting2Async(section, meeting.Id);

                if (entity == null)
                {
                    IntegrationApiExceptionAddError("An unexpected error occurred attempting to create the instructional-event record. "
                        , "Global.Internal.Error", !string.IsNullOrEmpty(meeting.Id) ? meeting.Id : null);
                }
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("An unexpected error occurred attempting to create the instructional-event record. " + ex.Message, "Global.Internal.Error",
                     !string.IsNullOrEmpty(meeting.Id) ? meeting.Id : null);
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            #endregion

            #region Build DTO response

            InstructionalEvent4 dto = null;

            try
            {
                var sectionIdDict = await _sectionRepository.GetSectionGuidsCollectionAsync(new List<string>() { entity.SectionId });

                dto = await ConvertSectionMeetingToInstructionalEvent4Async(entity, sectionIdDict);

                if (dto == null)
                {
                    IntegrationApiExceptionAddError("Record created. Error building response. ", "Global.Internal.Error",
                       !string.IsNullOrEmpty(meeting.Id) ? meeting.Id : null);
                }
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("Record created. Error building response. " + ex.Message, "Global.Internal.Error",
                       !string.IsNullOrEmpty(meeting.Id) ? meeting.Id : null);
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            #endregion

            return dto;
        }

        /// <summary>
        /// Update an instructional event V11
        /// </summary>
        /// <param name="meeting">The event to update</param>
        /// <returns>InstructionalEvent DTO</returns>
        public async Task<Dtos.InstructionalEvent4> UpdateInstructionalEvent4Async(Dtos.InstructionalEvent4 meeting)
        {
            if (meeting == null)
            {
                IntegrationApiExceptionAddError("Missing request body.", "Validation.Exception");
                throw IntegrationApiException;
            }

            CheckInstructionalEvent4Permissions(meeting);

            #region update domain entity from request

            Domain.Student.Entities.Section section = null;
            _sectionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
            try
            {
                section = await ConvertInstructionalEvent4ToSectionMeetingAsync(meeting);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("Record not updated.  Error extracting request." + ex.Message, "Global.Internal.Error",
                   !string.IsNullOrEmpty(meeting.Id) ? meeting.Id : null
                  );
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            #endregion

            #region Update record from domain entity

            SectionMeeting entity = null;
            try
            {
                entity = await _sectionRepository.PutSectionMeeting2Async(section, meeting.Id);

                if (entity == null)
                {
                    IntegrationApiExceptionAddError("An unexpected error occurred attempting to update the instructional-event record. "
                        , "Global.Internal.Error", !string.IsNullOrEmpty(meeting.Id) ? meeting.Id : null);
                }
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("An unexpected error occurred attempting to update the instructional-event record. " + ex.Message, "Global.Internal.Error",
                     !string.IsNullOrEmpty(meeting.Id) ? meeting.Id : null);
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            #endregion

            #region Build DTO response

            InstructionalEvent4 dto = null;

            try
            {
                var sectionIdDict = await _sectionRepository.GetSectionGuidsCollectionAsync(new List<string>() { entity.SectionId });

                dto = await ConvertSectionMeetingToInstructionalEvent4Async(entity, sectionIdDict);

            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("Record updated. Error building response. " + ex.Message, "Global.Internal.Error",
                       !string.IsNullOrEmpty(meeting.Id) ? meeting.Id : null);
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            #endregion

            return dto;
        }

        /// <summary>
        /// Helper method to determine which permissions need to be checked V11
        /// </summary>
        /// <param name="meeting">The instructional event</param>
        private void CheckInstructionalEvent4Permissions(Dtos.InstructionalEvent4 meeting)
        {
            if (meeting.Locations != null && meeting.Locations.Any())
            {
                CheckRoomBookingPermission();
            }
        }

        #endregion

        /// <summary>
        /// Delete an instructional event
        /// </summary>
        /// <param name="guid">The GUID of the event</param>
        public async Task DeleteInstructionalEventAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            try
            {
                var meeting = await _sectionRepository.GetSectionMeetingByGuidAsync(guid);

                if (meeting == null)
                {
                    throw new KeyNotFoundException();
                }
                var section = await _sectionRepository.GetSectionAsync(meeting.SectionId);
                // Remove from section any faculty that do not have the same instructional method
                // as the meeting we are removing.  Then, section.Faculty.ToList() only contains
                // the faculty objects we wish to delete.
                SectionProcessor.DeleteSectionFacultyFromSectionMeetings(section, meeting.Id);

                await _sectionRepository.DeleteSectionMeetingAsync(meeting.Id, section.Faculty.ToList());
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException(string.Format("Instructional-events not found for guid: '{0}'.", guid));
            }
        }

        /// <summary>
        /// Gets the section events for a single section Id 
        /// The section event describes a specific class meeting time (period) for the section - related to the calendar.
        /// </summary>
        /// <param name="sectionId">The section id.</param>
        /// <returns>The section events</returns>
        public async Task<IEnumerable<Dtos.Student.SectionMeetingInstance>> GetSectionMeetingInstancesAsync(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("Section ID is required for retrieving section meeting instances.", "sectionId");
            }
            var eventEntities = await _sectionRepository.GetSectionMeetingInstancesAsync(sectionId);
            List<Dtos.Student.SectionMeetingInstance> eventDtos = new List<Dtos.Student.SectionMeetingInstance>();
            var eventDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.SectionMeetingInstance, Dtos.Student.SectionMeetingInstance>();
            if (eventEntities == null)
            {
                throw new ApplicationException("Could not retrieve section meeting instances for section " + sectionId);
            }
            foreach (var eventEntity in eventEntities)
            {
                Dtos.Student.SectionMeetingInstance eventDto = eventDtoAdapter.MapToType(eventEntity);
                eventDtos.Add(eventDto);
            }
            return eventDtos;
        }

        /// <summary>
        /// Helper method to determine which permissions need to be checked
        /// </summary>
        /// <param name="meeting">The instructional event</param>
        private void CheckInstructionalEvent2Permissions(Dtos.InstructionalEvent2 meeting)
        {
            if (meeting.Locations != null && meeting.Locations.Any())
            {
                CheckRoomBookingPermission();
            }
            if (meeting.Instructors != null && meeting.Instructors.Any())
            {
                CheckFacultyBookingPermission();
            }
        }

        /// <summary>
        /// Helper method to determine which permissions need to be checked
        /// </summary>
        /// <param name="meeting">The instructional event</param>
        private void CheckInstructionalEvent3Permissions(Dtos.InstructionalEvent3 meeting)
        {
            if (meeting.Locations != null && meeting.Locations.Any())
            {
                CheckRoomBookingPermission();
            }
            if (meeting.Instructors != null && meeting.Instructors.Any())
            {
                CheckFacultyBookingPermission();
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to update room bookings
        /// </summary>
        /// <permission cref="StudentPermissionCodes.CreateAndUpdateRoomBooking"></permission>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckRoomBookingPermission()
        {
            bool hasRoomBookingPermission = HasPermission(StudentPermissionCodes.CreateAndUpdateRoomBooking);

            // User is not allowed to create or update room bookings without the appropriate permissions
            if (!hasRoomBookingPermission)
            {
                var message = "User " + CurrentUser.UserId + " does not have permission to create or update room bookings.";
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to update faculty bookings
        /// </summary>
        /// <permission cref="StudentPermissionCodes.CreateAndUpdateFacultyBooking"></permission>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckFacultyBookingPermission()
        {
            bool hasFacultyBookingPermission = HasPermission(StudentPermissionCodes.CreateAndUpdateFacultyBooking);

            // User is not allowed to create or update faculty bookings without the appropriate permissions
            if (!hasFacultyBookingPermission)
            {
                var message = "User " + CurrentUser.UserId + " does not have permission to create or update faculty bookings.";
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }

        #region Convert Section Meeting Version 6

        private async Task<Dtos.InstructionalEvent2> ConvertSectionMeetingToInstructionalEvent2Async(Domain.Student.Entities.SectionMeeting meeting)
        {
            var outputDto = new Dtos.InstructionalEvent2();
            outputDto.Id = meeting.Guid;
            try
            {
                outputDto.Section = new Dtos.GuidObject2(await ConvertSectionIdToSectionGuidAsync(meeting.SectionId));
            }
            catch
            {
                if (string.IsNullOrEmpty(meeting.SectionId))
                {
                    throw new ArgumentException(string.Format("Section ID is missing for the instructional event '{0}'. ", meeting.Guid), "meeting.SectionId");
                }
                throw new ArgumentException(string.Format("Section ID '{0}' is invalid for the instructional event '{1}'. ", meeting.SectionId, meeting.Guid), "meeting.SectionId");
            }
            try
            {
                outputDto.InstructionalMethod = new Dtos.GuidObject2(ConvertCodeToGuid(await InstructionalMethodsAsync(), meeting.InstructionalMethodCode));
            }
            catch
            {
                if (string.IsNullOrEmpty(meeting.InstructionalMethodCode))
                {
                    throw new ArgumentException(string.Format("Instructional Method is missing for the instructional event '{0}'. ", meeting.Guid), "meeting.InstructionalMethodCode");
                }
                throw new ArgumentException(string.Format("Instructional Method '{0}' is invalid for the instructional event '{1}'. ", meeting.InstructionalMethodCode, meeting.Guid), "meeting.InstructionalMethodCode");
            }
            outputDto.Recurrence = new Dtos.Recurrence3();
            outputDto.Recurrence.TimePeriod = new Dtos.RepeatTimePeriod2()
            {
                StartOn = meeting.StartDate.GetValueOrDefault().Date + meeting.StartTime.GetValueOrDefault().TimeOfDay,
                EndOn = meeting.EndDate.GetValueOrDefault().Date + meeting.EndTime.GetValueOrDefault().TimeOfDay
            };
                if (!string.IsNullOrEmpty(meeting.Frequency))
                {
                    var repeatCode = scheduleRepeats.FirstOrDefault(x => x.Code == meeting.Frequency);
                    if (repeatCode == null)
                    {
                        throw new ArgumentException("Invalid meeting frequency code: " + meeting.Frequency, "meeting.Frequency");
                    }

                    switch ((Dtos.FrequencyType2)repeatCode.FrequencyType)
                    {
                        case Dtos.FrequencyType2.Daily:
                            var repeatRuleDaily = new Dtos.RepeatRuleDaily()
                            {
                                Interval = repeatCode.Interval.GetValueOrDefault(),
                                Ends = new RepeatRuleEnds() { Date = meeting.EndDate.GetValueOrDefault().Date }
                            };
                            outputDto.Recurrence.RepeatRule = repeatRuleDaily;
                            break;
                        case Dtos.FrequencyType2.Weekly:
                            var repeatRuleWeekly = new Dtos.RepeatRuleWeekly()
                            {
                                Interval = repeatCode.Interval.GetValueOrDefault(),
                                Ends = new RepeatRuleEnds() { Date = meeting.EndDate.GetValueOrDefault().Date },
                                DayOfWeek = ConvertDaysToHedmDays(meeting.Days)
                            };

                            outputDto.Recurrence.RepeatRule = repeatRuleWeekly;
                            break;
                        case Dtos.FrequencyType2.Monthly:
                            int? dayOfMonth = null;
                            RepeatRuleDayOfWeek dayOfWeek = null;
                            if (meeting.Days.Any())
                            {
                                dayOfWeek = new RepeatRuleDayOfWeek()
                                {
                                    Day = ConvertDaysToHedmDays(meeting.Days).ElementAt(0)
                                };
                            }
                            else
                            {
                                dayOfMonth = meeting.StartDate.GetValueOrDefault().Day;
                            }
                            var repeatRuleMonthly = new Dtos.RepeatRuleMonthly()
                            {
                                Interval = repeatCode.Interval.GetValueOrDefault(),
                                Ends = new RepeatRuleEnds() { Date = meeting.EndDate.GetValueOrDefault().Date },
                                RepeatBy = new RepeatRuleRepeatBy()
                                {
                                    DayOfWeek = dayOfWeek,
                                    DayOfMonth = dayOfMonth
                                }
                            };
                            outputDto.Recurrence.RepeatRule = repeatRuleMonthly;
                            break;
                        case Dtos.FrequencyType2.Yearly:
                            var repeatRuleYearly = new Dtos.RepeatRuleYearly()
                            {
                                Interval = repeatCode.Interval.GetValueOrDefault(),
                                Ends = new RepeatRuleEnds() { Date = meeting.EndDate.GetValueOrDefault().Date }
                            };
                            outputDto.Recurrence.RepeatRule = repeatRuleYearly;
                            break;
                        default: break;
                    }
                
            }

            if (!string.IsNullOrEmpty(meeting.Room) && meeting.Room != "*")
            {
                /*
                    In some cases, it's possible that Colleague customers may no longer have records in the ROOMS file that correspond to the 
                    CSM.BLDG and CSM.ROOM combination that is exposed in this reference (e.g. some rooms may have been removed when remodeling was done, 
                    but the reference to the room itself was not removed from COURSE.SEC.MEETING file). If this is the case, we should omit the locations 
                    array object from the response altogether. This will avoid the issue of surfacing an error for an old room that is no longer used by 
                    a client yet is referenced in older COURSE.SEC.MEETING records. See http://corpappprod.ellucian.com/crmview/Main/CRDetail.asp?crno=CR-000173681 
                */

                try
                {
                    //In above scenario following may throw RepositoryException. Per spec just don't add locations to the payload.
                    var locations = new List<Location>();
                    string roomGuid = string.Empty;
                    roomGuid = await _roomRepository.GetRoomsGuidAsync(meeting.Room);
                    if ((string.IsNullOrEmpty(roomGuid)) || (roomGuid.Equals(Guid.Empty.ToString())))
                    {
                        throw new ArgumentException(string.Format("GUID is missing, or not found, for room '{0}'. ", meeting.Room),
                            "GUID.Not.Found");
                    }
                    else
                    {
                        var roomid = new GuidObject2(roomGuid);
                        var room = new Dtos.InstructionalRoom2() { Room = roomid, LocationType = InstructionalLocationType.InstructionalRoom };

                        var location = new Location() { Locations = room };
                        locations.Add(location);
                        outputDto.Locations = locations;
                    }
                }
                catch (Exception e)
                {
                    logger.Error(string.Concat("Ethos API instructional-events V6, Method: ConvertSectionMeetingToInstructionalEvent2Async, Line: 10653, ", e.Message));
                }
            }

            outputDto.Workload = meeting.Load;

            if (meeting.FacultyRoster != null && meeting.FacultyRoster.Any())
            {
                var instructors = new List<Dtos.InstructionalEventInstructor2>();
                foreach (var sectionfaculty in meeting.FacultyRoster)
                {
                    var instructor = new Dtos.InstructionalEventInstructor2();
                    try
                    {
                        instructor.Instructor = new Dtos.GuidObject2(await _personRepository.GetPersonGuidFromIdAsync(sectionfaculty.FacultyId));
                    }
                    catch
                    {
                        if (string.IsNullOrEmpty(sectionfaculty.FacultyId))
                        {
                            throw new ArgumentException(string.Format("Instructor is missing for the instructional event '{0}'. ", meeting.Guid), "meeting.FacultyRoster.FacultyId");
                        }
                        throw new ArgumentException(string.Format("Instructor '{0}' is invalid for the instructional event '{1}'. ", sectionfaculty.FacultyId, meeting.Guid), "meeting.FacultyRoster.FacultyId");
                    }
                    instructor.WorkLoadPercentage = sectionfaculty.LoadFactor;
                    instructor.ResponsibilityPercentage = sectionfaculty.ResponsibilityPercentage;
                    instructor.WorkStartDate = sectionfaculty.StartDate;
                    instructor.WorkEndDate = sectionfaculty.EndDate;
                    instructors.Add(instructor);
                }
                outputDto.Instructors = instructors;
            }

            return outputDto;
        }

        #endregion

        // upgraded from version "2" of this method for v2 errors for sections-maximum
        private async Task<Dtos.InstructionalEvent2> ConvertSectionMeetingToInstructionalEvent2_5Async(Domain.Student.Entities.SectionMeeting meeting, Domain.Student.Entities.Section entity)
        {
            var outputDto = new Dtos.InstructionalEvent2();
            outputDto.Id = meeting.Guid;
            try
            {
                outputDto.Section = new Dtos.GuidObject2(await ConvertSectionIdToSectionGuidAsync(meeting.SectionId));
            }
            catch
            {
                if (string.IsNullOrEmpty(meeting.SectionId))
                {
                    IntegrationApiExceptionAddError(string.Format("Section ID is missing for the instructional event '{0}'. ", meeting.Guid), "Bad.Data", entity.Guid, entity.Id);
                }
                IntegrationApiExceptionAddError(string.Format("Section ID is missing for the instructional event '{0}'. ", meeting.Guid), "Bad.Data", entity.Guid, entity.Id);
            }
            try
            {
                outputDto.InstructionalMethod = new Dtos.GuidObject2(ConvertCodeToGuid(await InstructionalMethodsAsync(), meeting.InstructionalMethodCode));
            }
            catch
            {
                if (string.IsNullOrEmpty(meeting.InstructionalMethodCode))
                {
                    IntegrationApiExceptionAddError(string.Format("Instructional Method is missing for the instructional event '{0}'. ", meeting.Guid), "Bad.Data", entity.Guid, entity.Id);
                }
                IntegrationApiExceptionAddError(string.Format("Instructional Method '{0}' is invalid for the instructional event '{1}'. ", meeting.InstructionalMethodCode, meeting.Guid),
                    "Bad.Data", entity.Guid, entity.Id);
            }
            outputDto.Recurrence = new Dtos.Recurrence3();
            outputDto.Recurrence.TimePeriod = new Dtos.RepeatTimePeriod2()
            {
                StartOn = meeting.StartDate.GetValueOrDefault().Date + meeting.StartTime.GetValueOrDefault().TimeOfDay,
                EndOn = meeting.EndDate.GetValueOrDefault().Date + meeting.EndTime.GetValueOrDefault().TimeOfDay
            };
            if (!string.IsNullOrEmpty(meeting.Frequency))
            {
                var repeatCode = scheduleRepeats.FirstOrDefault(x => x.Code == meeting.Frequency);
                if (repeatCode == null)
                {
                    IntegrationApiExceptionAddError("Invalid meeting frequency code: " + meeting.Frequency,
                    "Bad.Data", entity.Guid, entity.Id);
                }

                switch ((Dtos.FrequencyType2)repeatCode.FrequencyType)
                {
                    case Dtos.FrequencyType2.Daily:
                        var repeatRuleDaily = new Dtos.RepeatRuleDaily()
                        {
                            Interval = repeatCode.Interval.GetValueOrDefault(),
                            Ends = new RepeatRuleEnds() { Date = meeting.EndDate.GetValueOrDefault().Date }
                        };
                        outputDto.Recurrence.RepeatRule = repeatRuleDaily;
                        break;
                    case Dtos.FrequencyType2.Weekly:
                        var repeatRuleWeekly = new Dtos.RepeatRuleWeekly()
                        {
                            Interval = repeatCode.Interval.GetValueOrDefault(),
                            Ends = new RepeatRuleEnds() { Date = meeting.EndDate.GetValueOrDefault().Date },
                            DayOfWeek = ConvertDaysToHedmDays(meeting.Days)
                        };

                        outputDto.Recurrence.RepeatRule = repeatRuleWeekly;
                        break;
                    case Dtos.FrequencyType2.Monthly:
                        int? dayOfMonth = null;
                        RepeatRuleDayOfWeek dayOfWeek = null;
                        if (meeting.Days.Any())
                        {
                            dayOfWeek = new RepeatRuleDayOfWeek()
                            {
                                Day = ConvertDaysToHedmDays(meeting.Days).ElementAt(0)
                            };
                        }
                        else
                        {
                            dayOfMonth = meeting.StartDate.GetValueOrDefault().Day;
                        }
                        var repeatRuleMonthly = new Dtos.RepeatRuleMonthly()
                        {
                            Interval = repeatCode.Interval.GetValueOrDefault(),
                            Ends = new RepeatRuleEnds() { Date = meeting.EndDate.GetValueOrDefault().Date },
                            RepeatBy = new RepeatRuleRepeatBy()
                            {
                                DayOfWeek = dayOfWeek,
                                DayOfMonth = dayOfMonth
                            }
                        };
                        outputDto.Recurrence.RepeatRule = repeatRuleMonthly;
                        break;
                    case Dtos.FrequencyType2.Yearly:
                        var repeatRuleYearly = new Dtos.RepeatRuleYearly()
                        {
                            Interval = repeatCode.Interval.GetValueOrDefault(),
                            Ends = new RepeatRuleEnds() { Date = meeting.EndDate.GetValueOrDefault().Date }
                        };
                        outputDto.Recurrence.RepeatRule = repeatRuleYearly;
                        break;
                    default: break;
                }

            }

            if (!string.IsNullOrEmpty(meeting.Room) && meeting.Room != "*")
            {
                /*
                    In some cases, it's possible that Colleague customers may no longer have records in the ROOMS file that correspond to the 
                    CSM.BLDG and CSM.ROOM combination that is exposed in this reference (e.g. some rooms may have been removed when remodeling was done, 
                    but the reference to the room itself was not removed from COURSE.SEC.MEETING file). If this is the case, we should omit the locations 
                    array object from the response altogether. This will avoid the issue of surfacing an error for an old room that is no longer used by 
                    a client yet is referenced in older COURSE.SEC.MEETING records. See http://corpappprod.ellucian.com/crmview/Main/CRDetail.asp?crno=CR-000173681 
                */

                try
                {
                    //In above scenario following may throw RepositoryException. Per spec just don't add locations to the payload.
                    var locations = new List<Location>();
                    string roomGuid = string.Empty;
                    roomGuid = await _roomRepository.GetRoomsGuidAsync(meeting.Room);
                    if ((string.IsNullOrEmpty(roomGuid)) || (roomGuid.Equals(Guid.Empty.ToString())))
                    {
                        IntegrationApiExceptionAddError(string.Format("GUID is missing, or not found, for room '{0}'. ", meeting.Room),
                            "GUID.Not.Found", entity.Guid, entity.Id);
                    }
                    else
                    {
                        var roomid = new GuidObject2(roomGuid);
                        var room = new Dtos.InstructionalRoom2() { Room = roomid, LocationType = InstructionalLocationType.InstructionalRoom };

                        var location = new Location() { Locations = room };
                        locations.Add(location);
                        outputDto.Locations = locations;
                    }
                }
                catch (Exception e)
                {
                    logger.Error(string.Concat("Ethos API instructional-events V6, Method: ConvertSectionMeetingToInstructionalEvent2Async, Line: 10653, ", e.Message));
                }
            }

            outputDto.Workload = meeting.Load;

            if (meeting.FacultyRoster != null && meeting.FacultyRoster.Any())
            {
                var instructors = new List<Dtos.InstructionalEventInstructor2>();
                foreach (var sectionfaculty in meeting.FacultyRoster)
                {
                    var instructor = new Dtos.InstructionalEventInstructor2();
                    try
                    {
                        instructor.Instructor = new Dtos.GuidObject2(await _personRepository.GetPersonGuidFromIdAsync(sectionfaculty.FacultyId));
                    }
                    catch
                    {
                        if (string.IsNullOrEmpty(sectionfaculty.FacultyId))
                        {
                            IntegrationApiExceptionAddError(string.Format("Instructor is missing for the instructional event '{0}'. ", meeting.Guid),
                    "Bad.Data", entity.Guid, entity.Id);
                        }
                        throw new ArgumentException(string.Format("Instructor '{0}' is invalid for the instructional event '{1}'. ", sectionfaculty.FacultyId, meeting.Guid), "meeting.FacultyRoster.FacultyId");
                        IntegrationApiExceptionAddError(string.Format("Instructor '{0}' is invalid for the instructional event '{1}'. ", sectionfaculty.FacultyId, meeting.Guid),
                    "Bad.Data", entity.Guid, entity.Id);
                    }
                    instructor.WorkLoadPercentage = sectionfaculty.LoadFactor;
                    instructor.ResponsibilityPercentage = sectionfaculty.ResponsibilityPercentage;
                    instructor.WorkStartDate = sectionfaculty.StartDate;
                    instructor.WorkEndDate = sectionfaculty.EndDate;
                    instructors.Add(instructor);
                }
                outputDto.Instructors = instructors;
            }

            return outputDto;
        }

        #region Convert Section Meeting Version 8

        private async Task<Dtos.InstructionalEvent3> ConvertSectionMeetingToInstructionalEvent3Async(Domain.Student.Entities.SectionMeeting meeting)
        {
            var outputDto = new Dtos.InstructionalEvent3();
            outputDto.Id = meeting.Guid;
            try
            {
                outputDto.Section = new Dtos.GuidObject2(await _sectionRepository.GetSectionGuidFromIdAsync(meeting.SectionId));
            }
            catch
            {
                if (string.IsNullOrEmpty(meeting.SectionId))
                {
                    throw new ArgumentException(string.Format("Section ID is missing for the instructional event '{0}'. ", meeting.Guid), "meeting.SectionId");
                }
                throw new ArgumentException(string.Format("Section ID '{0}' is invalid for the instructional event '{1}'. ", meeting.SectionId, meeting.Guid), "meeting.SectionId");
            }
            try
            {
                outputDto.InstructionalMethod = new Dtos.GuidObject2(ConvertCodeToGuid(await InstructionalMethodsAsync(), meeting.InstructionalMethodCode));
            }
            catch
            {
                if (string.IsNullOrEmpty(meeting.InstructionalMethodCode))
                {
                    throw new ArgumentException(string.Format("Instructional Method is missing for the instructional event '{0}'. ", meeting.Guid), "meeting.InstructionalMethodCode");
                }
                throw new ArgumentException(string.Format("Instructional Method '{0}' is invalid for the instructional event '{1}'. ", meeting.InstructionalMethodCode, meeting.Guid), "meeting.InstructionalMethodCode");
            }            
            outputDto.Recurrence = new Dtos.Recurrence3();
            outputDto.Recurrence.TimePeriod = new Dtos.RepeatTimePeriod2()
            {
                StartOn = meeting.StartDate.GetValueOrDefault().Date + meeting.StartTime.GetValueOrDefault().TimeOfDay,
                EndOn = meeting.EndDate.GetValueOrDefault().Date + meeting.EndTime.GetValueOrDefault().TimeOfDay
            };
            if (!string.IsNullOrEmpty(meeting.Frequency))
            {
                var repeatCode = scheduleRepeats.FirstOrDefault(x => x.Code == meeting.Frequency);
                if (repeatCode == null)
                {
                    throw new ArgumentException("Invalid meeting frequency code: " + meeting.Frequency, "meeting.Frequency");
                }

                switch ((Dtos.FrequencyType2)repeatCode.FrequencyType)
                {
                    case Dtos.FrequencyType2.Daily:
                        var repeatRuleDaily = new Dtos.RepeatRuleDaily()
                        {
                            Interval = repeatCode.Interval.GetValueOrDefault(),
                            Ends = new RepeatRuleEnds() { Date = meeting.EndDate.GetValueOrDefault().Date }
                        };
                        outputDto.Recurrence.RepeatRule = repeatRuleDaily;
                        break;
                    case Dtos.FrequencyType2.Weekly:
                        var repeatRuleWeekly = new Dtos.RepeatRuleWeekly()
                        {
                            Interval = repeatCode.Interval.GetValueOrDefault(),
                            Ends = new RepeatRuleEnds() { Date = meeting.EndDate.GetValueOrDefault().Date },
                            DayOfWeek = ConvertDaysToHedmDays(meeting.Days)
                        };

                        outputDto.Recurrence.RepeatRule = repeatRuleWeekly;
                        break;
                    case Dtos.FrequencyType2.Monthly:
                        int? dayOfMonth = null;
                        RepeatRuleDayOfWeek dayOfWeek = null;
                        if (meeting.Days.Any())
                        {
                            dayOfWeek = new RepeatRuleDayOfWeek()
                            {
                                Day = ConvertDaysToHedmDays(meeting.Days).ElementAt(0)
                            };
                        }
                        else
                        {
                            dayOfMonth = meeting.StartDate.GetValueOrDefault().Day;
                        }
                        var repeatRuleMonthly = new Dtos.RepeatRuleMonthly()
                        {
                            Interval = repeatCode.Interval.GetValueOrDefault(),
                            Ends = new RepeatRuleEnds() { Date = meeting.EndDate.GetValueOrDefault().Date },
                            RepeatBy = new RepeatRuleRepeatBy()
                            {
                                DayOfWeek = dayOfWeek,
                                DayOfMonth = dayOfMonth
                            }
                        };
                        outputDto.Recurrence.RepeatRule = repeatRuleMonthly;
                        break;
                    case Dtos.FrequencyType2.Yearly:
                        var repeatRuleYearly = new Dtos.RepeatRuleYearly()
                        {
                            Interval = repeatCode.Interval.GetValueOrDefault(),
                            Ends = new RepeatRuleEnds() { Date = meeting.EndDate.GetValueOrDefault().Date }
                        };
                        outputDto.Recurrence.RepeatRule = repeatRuleYearly;
                        break;
                    default: break;
                }

            }

            if (!string.IsNullOrEmpty(meeting.Room) && meeting.Room != "*")
            {
                /*
                    In some cases, it's possible that Colleague customers may no longer have records in the ROOMS file that correspond to the 
                    CSM.BLDG and CSM.ROOM combination that is exposed in this reference (e.g. some rooms may have been removed when remodeling was done, 
                    but the reference to the room itself was not removed from COURSE.SEC.MEETING file). If this is the case, we should omit the locations 
                    array object from the response altogether. This will avoid the issue of surfacing an error for an old room that is no longer used by 
                    a client yet is referenced in older COURSE.SEC.MEETING records. See http://corpappprod.ellucian.com/crmview/Main/CRDetail.asp?crno=CR-000173681 
                */

                try
                {
                    //In above scenario following may throw RepositoryException. Per spec just don't add locations to the payload.
                    var locations = new List<Location>();
                    string roomGuid = string.Empty;
                    roomGuid = await _roomRepository.GetRoomsGuidAsync(meeting.Room);
                    if ((string.IsNullOrEmpty(roomGuid)) || (roomGuid.Equals(Guid.Empty.ToString())))
                    {
                        throw new ArgumentException(string.Format("GUID is missing, or not found, for room '{0}'. ", meeting.Room),
                            "GUID.Not.Found");
                    }
                    else
                    {
                        var roomid = new GuidObject2(roomGuid);
                        var room = new Dtos.InstructionalRoom2() { Room = roomid, LocationType = InstructionalLocationType.InstructionalRoom };

                        var location = new Location() { Locations = room };
                        locations.Add(location);
                        outputDto.Locations = locations;
                    }
                }
                catch (Exception e)
                {
                    logger.Error(string.Concat("Ethos API instructional-events V8, Method: ConvertSectionMeetingToInstructionalEvent3Async, Line: 10827, ", e.Message));
                }
            }

            outputDto.Workload = meeting.Load;

            if (meeting.FacultyRoster != null && meeting.FacultyRoster.Any())
            {
                var instructors = new List<Dtos.InstructionalEventInstructor3>();
                bool firstInstructor = true;
                foreach (var sectionfaculty in meeting.FacultyRoster)
                {
                    var instructor = new Dtos.InstructionalEventInstructor3();
                    try
                    {
                        instructor.Instructor = new Dtos.GuidObject2(await _personRepository.GetPersonGuidFromIdAsync(sectionfaculty.FacultyId));
                    }
                    catch
                    {
                        if (string.IsNullOrEmpty(sectionfaculty.FacultyId))
                        {
                            throw new ArgumentException(string.Format("Instructor is missing for the instructional event '{0}'. ", meeting.Guid), "meeting.FacultyRoster.FacultyId");
                        }
                        throw new ArgumentException(string.Format("Instructor '{0}' is invalid for the instructional event '{1}'. ", sectionfaculty.FacultyId, meeting.Guid), "meeting.FacultyRoster.FacultyId");
                    }
                    if (firstInstructor)
                    {
                        instructor.instructorRole = Dtos.EnumProperties.InstructorRoleType.Primary;
                        firstInstructor = false;
                    }
                    instructor.WorkLoadPercentage = sectionfaculty.LoadFactor;
                    instructor.ResponsibilityPercentage = sectionfaculty.ResponsibilityPercentage;
                    instructor.WorkStartDate = sectionfaculty.StartDate;
                    instructor.WorkEndDate = sectionfaculty.EndDate;
                    instructors.Add(instructor);
                }
                outputDto.Instructors = instructors;
            }

            return outputDto;
        }

        /// <summary>
        /// Convert entity to dto
        /// </summary>
        /// <param name="meeting"></param>
        /// <returns>Task<Dtos.InstructionalEvent4></returns>
        private async Task<Dtos.InstructionalEvent4> ConvertSectionMeetingToInstructionalEvent4Async(Domain.Student.Entities.SectionMeeting meeting, Dictionary<string, string> sectionIdDict)
        {
            var outputDto = new Dtos.InstructionalEvent4();
            if (meeting == null)
            {
                IntegrationApiExceptionAddError("SectionMeeting is required.", "Global.Internal.Error");
                return outputDto;
            }
            if (string.IsNullOrEmpty(meeting.Guid) || (meeting.Guid.Equals(Guid.Empty.ToString())))
            {
                IntegrationApiExceptionAddError("GUID is missing for the instructional event.", "Missing.GUID");
                return outputDto;

            }
            outputDto.Id = meeting.Guid;

            if (string.IsNullOrEmpty(meeting.SectionId))
            {
                IntegrationApiExceptionAddError("Section ID is missing for instructional event.", "Bad.Data", meeting.Guid, meeting.Id);
            }
            else if (sectionIdDict == null)
            {
                IntegrationApiExceptionAddError(string.Format("GUID not found for the section id '{0}'. ", meeting.SectionId), "Bad.Data", meeting.Guid, meeting.Id);
            }
            else
            {
                try
                {
                    //var sectionGuid = await _sectionRepository.GetSectionGuidFromIdAsync(meeting.SectionId);
                    string sectionGuid = null;
                    if (sectionIdDict.TryGetValue(meeting.SectionId, out sectionGuid))

                        if (string.IsNullOrEmpty(sectionGuid))
                        {
                            IntegrationApiExceptionAddError(string.Format("GUID not found for the section id '{0}'. ", meeting.SectionId),
                                     "GUID.Not.Found", meeting.Guid, meeting.Id);
                        }
                        else
                        {
                            outputDto.Section = new Dtos.GuidObject2(sectionGuid);
                        }
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, "GUID.Not.Found", meeting.Guid, meeting.Id);
                }
            }

            if (string.IsNullOrEmpty(meeting.InstructionalMethodCode))
            {
                IntegrationApiExceptionAddError(string.Format("Instructional Method is missing for the instructional event '{0}'.", meeting.Guid), "Bad.Data", meeting.Guid, meeting.Id);
            }
            else
            {
                try
                {
                    var instMethodGuid = await _studentReferenceDataRepository.GetInstructionalMethodGuidAsync(meeting.InstructionalMethodCode);
                    if (string.IsNullOrEmpty(instMethodGuid))
                    {
                        IntegrationApiExceptionAddError(string.Format("GUID not found for the instructional method '{0}'. ", meeting.InstructionalMethodCode),
                            "GUID.Not.Found", meeting.Guid, meeting.Id);
                    }
                    else
                    {
                        outputDto.InstructionalMethod = new Dtos.GuidObject2(instMethodGuid);
                    }

                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, "GUID.Not.Found", meeting.Guid, meeting.Id);
                }
            }
            // only set Recurrence if we have start/end times
            if (meeting.StartTime != null && meeting.EndTime != null)
            {
                outputDto.Recurrence = new Dtos.Recurrence3();
                outputDto.Recurrence.TimePeriod = new Dtos.RepeatTimePeriod2()
                {
                    StartOn = meeting.StartDate.GetValueOrDefault().Date + meeting.StartTime.GetValueOrDefault().TimeOfDay,
                    EndOn = meeting.EndDate.GetValueOrDefault().Date + meeting.EndTime.GetValueOrDefault().TimeOfDay
                };
                if (!string.IsNullOrEmpty(meeting.Frequency))
                {
                    var repeatCode = scheduleRepeats.FirstOrDefault(x => x.Code == meeting.Frequency);
                    if (repeatCode == null)
                    {
                        IntegrationApiExceptionAddError("Invalid meeting frequency code: " + meeting.Frequency, "Bad.Data", meeting.Guid, meeting.Id);
                    }
                    else
                    {
                        switch ((Dtos.FrequencyType2)repeatCode.FrequencyType)
                        {
                            case Dtos.FrequencyType2.Daily:
                                var repeatRuleDaily = new Dtos.RepeatRuleDaily()
                                {
                                    Interval = repeatCode.Interval.GetValueOrDefault(),
                                    Ends = new RepeatRuleEnds() { Date = meeting.EndDate.GetValueOrDefault().Date }
                                };
                                outputDto.Recurrence.RepeatRule = repeatRuleDaily;
                                break;
                            case Dtos.FrequencyType2.Weekly:
                                var repeatRuleWeekly = new Dtos.RepeatRuleWeekly()
                                {
                                    Interval = repeatCode.Interval.GetValueOrDefault(),
                                    Ends = new RepeatRuleEnds() { Date = meeting.EndDate.GetValueOrDefault().Date },
                                    DayOfWeek = ConvertDaysToHedmDays(meeting.Days)
                                };

                                outputDto.Recurrence.RepeatRule = repeatRuleWeekly;
                                break;
                            case Dtos.FrequencyType2.Monthly:
                                int? dayOfMonth = null;
                                RepeatRuleDayOfWeek dayOfWeek = null;
                                if (meeting.Days.Any())
                                {
                                    dayOfWeek = new RepeatRuleDayOfWeek()
                                    {
                                        Day = ConvertDaysToHedmDays(meeting.Days).ElementAt(0)
                                    };
                                }
                                else
                                {
                                    dayOfMonth = meeting.StartDate.GetValueOrDefault().Day;
                                }
                                var repeatRuleMonthly = new Dtos.RepeatRuleMonthly()
                                {
                                    Interval = repeatCode.Interval.GetValueOrDefault(),
                                    Ends = new RepeatRuleEnds() { Date = meeting.EndDate.GetValueOrDefault().Date },
                                    RepeatBy = new RepeatRuleRepeatBy()
                                    {
                                        DayOfWeek = dayOfWeek,
                                        DayOfMonth = dayOfMonth
                                    }
                                };
                                outputDto.Recurrence.RepeatRule = repeatRuleMonthly;
                                break;
                            case Dtos.FrequencyType2.Yearly:
                                var repeatRuleYearly = new Dtos.RepeatRuleYearly()
                                {
                                    Interval = repeatCode.Interval.GetValueOrDefault(),
                                    Ends = new RepeatRuleEnds() { Date = meeting.EndDate.GetValueOrDefault().Date }
                                };
                                outputDto.Recurrence.RepeatRule = repeatRuleYearly;
                                break;
                            default: break;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(meeting.Room) && meeting.Room != "*")
            {

                /*
                    In some cases, it's possible that Colleague customers may no longer have records in the ROOMS file that correspond to the 
                    CSM.BLDG and CSM.ROOM combination that is exposed in this reference (e.g. some rooms may have been removed when remodeling was done, 
                    but the reference to the room itself was not removed from COURSE.SEC.MEETING file). If this is the case, we should omit the locations 
                    array object from the response altogether. This will avoid the issue of surfacing an error for an old room that is no longer used by 
                    a client yet is referenced in older COURSE.SEC.MEETING records. See http://corpappprod.ellucian.com/crmview/Main/CRDetail.asp?crno=CR-000173681 
                */

                try
                {
                    //In above scenario following may throw RepositoryException. Per spec just don't add locations to the payload.
                    var locations = new List<Location>();
                    string roomGuid = string.Empty;
                    roomGuid = await _roomRepository.GetRoomsGuidAsync(meeting.Room);
                    if ((string.IsNullOrEmpty(roomGuid)) || (roomGuid.Equals(Guid.Empty.ToString())))
                    {
                        IntegrationApiExceptionAddError(string.Format("GUID is missing, or not found, for room '{0}'. ", meeting.Room),
                            "GUID.Not.Found", meeting.Guid, meeting.Id);
                    }
                    else
                    {
                        var roomid = new GuidObject2(roomGuid);
                        var room = new Dtos.InstructionalRoom2() { Room = roomid, LocationType = InstructionalLocationType.InstructionalRoom };

                        var location = new Location() { Locations = room };
                        locations.Add(location);
                        outputDto.Locations = locations;
                    }
                }
                catch (Exception e)
                {
                    logger.Error(string.Concat("Ethos API instructional-events V11, Method: ConvertSectionMeetingToInstructionalEvent4Async, Line: 10976, ", e.Message));
                }
            }

            outputDto.Workload = meeting.Load;

            return outputDto;
        }
        #endregion

        private List<HedmDayOfWeek?> ConvertDaysToHedmDays(List<DayOfWeek> daysOfWeek)
        {
            if (daysOfWeek != null && daysOfWeek.Any())
            {
                var newDaysOfWeek = new List<HedmDayOfWeek?>();
                foreach (var day in daysOfWeek)
                {
                    newDaysOfWeek.Add((Dtos.EnumProperties.HedmDayOfWeek)day);
                }
                return newDaysOfWeek;
            }
            return null;
        }

        private string ConvertRoomCodeToGuid(IEnumerable<Domain.Base.Entities.Room> roomsList, string room)
        {
            var entry = roomsList.FirstOrDefault(x => x.Id == room);
            return entry == null ? null : entry.Guid;
        }

        #region Convert Instructional Events Version 6

        private async Task<Domain.Student.Entities.Section> ConvertInstructionalEvent2ToSectionMeetingAsync(Dtos.InstructionalEvent2 meeting)
        {
            // Initialize the logger for the section processor service
            SectionProcessor.InitializeLogger(logger);

            // Check for required data - A GUID must be supplied, but it may not correspond to an ID if this is a new record
            if (string.IsNullOrEmpty(meeting.Id))
            {
                // Integration API error InstructionalEvent.NotFound
                var ex = new IntegrationApiException("Validation exception");
                ex.AddError(new IntegrationApiError() { Code = "InstructionalEvent.NotFound", Message = "The instructional event was not supplied." });
                throw ex;
            }
            if (meeting.Section == null || string.IsNullOrEmpty(meeting.Section.Id))
            {
                // Integration API error InstructionalEvent.Section.NotFound
                var ex = new IntegrationApiException("Validation exception");
                ex.AddError(new IntegrationApiError()
                {
                    Code = "InstructionalEvent.Section.NotFound",
                    Message = "Section id is required in order to schedule meeting times."
                });
                throw ex;
            }
            string meetingId = await _sectionRepository.GetSectionMeetingIdFromGuidAsync(meeting.Id);

            // Section ID is required
            Domain.Student.Entities.Section section = null;
            try
            {
                section = meeting.Section != null && !string.IsNullOrEmpty(meeting.Section.Id) ? await _sectionRepository.GetSectionByGuidAsync(meeting.Section.Id) : null;
            }
            catch (KeyNotFoundException)
            {
                // Fall through - we want this condition to do the same as if the section is null
            }
            if (section == null || string.IsNullOrEmpty(section.Id))
            {
                // Integration API error InstructionalEvent.Section.NotFound
                var ex = new IntegrationApiException("Validation exception");
                ex.AddError(new IntegrationApiError()
                {
                    Code = "InstructionalEvent.Section.NotFound",
                    Message = "Section id is required in order to schedule meeting times."
                });
                throw ex;
            }

            if (meeting.Recurrence != null)
            {
                if (meeting.Recurrence.RepeatRule == null)
                {
                    // Integration API error InstructionalEvent.InvalidInstructionalMethod.NotFound
                    var ex = new IntegrationApiException("Validation exception");
                    ex.AddError(new IntegrationApiError()
                    {
                        Code = "InstructionalEvent.Recurrence.RepeatRule",
                        Message = "Repeat rule is required when recurrence is not null. "
                    });
                    throw ex;
                }
                if (meeting.Recurrence.RepeatRule.Type == null)
                {
                    // Integration API error InstructionalEvent.InvalidInstructionalMethod.NotFound
                    var ex = new IntegrationApiException("Validation exception");
                    ex.AddError(new IntegrationApiError()
                    {
                        Code = "InstructionalEvent.Recurrence.RepeatRule.Type",
                        Message = "Repeat rule type was not specified and is required. "
                    });
                    throw ex;
                }
                if (meeting.Recurrence.TimePeriod == null || (meeting.Recurrence.TimePeriod.StartOn == null && meeting.Recurrence.TimePeriod.EndOn == null))
                {
                    // Integration API error InstructionalEvent.Recurrence.TimePeriod
                    var ex = new IntegrationApiException("Validation exception");
                    ex.AddError(new IntegrationApiError()
                    {
                        Code = "InstructionalEvent.Recurrence.TimePeriod",
                        Message = "At least one timePeriod must be specified on the recurrence pattern. "
                    });
                    throw ex;
                }
            }

            int interval = new int();
            List<HedmDayOfWeek?> daysOfWeek = new List<HedmDayOfWeek?>();

            if (meeting.Recurrence != null && meeting.Recurrence.RepeatRule != null && meeting.Recurrence.RepeatRule.Interval.ToString() != null)
            {
                var frequencyType = (Dtos.FrequencyType2?)meeting.Recurrence.RepeatRule.Type;
                if (frequencyType == null)
                {
                    // Integration API error InstructionalEvent.Section.NotFound
                    var ex = new IntegrationApiException("Validation exception");
                    ex.AddError(new IntegrationApiError()
                    {
                        Code = "InstructionalEvent.Recurrence.RepeatRule.Type",
                        Message = "RepeatRule type is required in order to schedule meeting times."
                    });
                    throw ex;
                }

                interval = meeting.Recurrence.RepeatRule.Interval;
                switch ((Dtos.FrequencyType2)meeting.Recurrence.RepeatRule.Type)
                {
                    case Dtos.FrequencyType2.Daily:
                        var repeatRuleDaily = (RepeatRuleDaily)meeting.Recurrence.RepeatRule;
                        if (repeatRuleDaily != null)
                        {
                            daysOfWeek = Enum.GetValues(typeof(HedmDayOfWeek)).Cast<HedmDayOfWeek?>().ToList();
                        }
                        break;
                    case Dtos.FrequencyType2.Weekly:
                        var repeatRuleWeekly = (RepeatRuleWeekly)meeting.Recurrence.RepeatRule;
                        if (repeatRuleWeekly != null)
                        {
                            daysOfWeek = repeatRuleWeekly.DayOfWeek;
                        }
                        break;
                    case Dtos.FrequencyType2.Monthly:
                        var repeatRuleMonthly = (RepeatRuleMonthly)meeting.Recurrence.RepeatRule;
                        if (repeatRuleMonthly != null)
                        {
                            if ((repeatRuleMonthly.RepeatBy != null) && (repeatRuleMonthly.RepeatBy.DayOfWeek != null))
                                daysOfWeek = new List<HedmDayOfWeek?>() { repeatRuleMonthly.RepeatBy.DayOfWeek.Day };
                        }
                        break;
                    case Dtos.FrequencyType2.Yearly:

                        break;
                    default: break;
                }
            }

            string instructionalMethod = meeting.InstructionalMethod == null ? null : ConvertGuidToCode(await InstructionalMethodsAsync(), meeting.InstructionalMethod.Id);
            // Instructional method is required - use the (first) instructional method from the section
            if (string.IsNullOrEmpty(instructionalMethod))
            {
                var firstContact = section.InstructionalContacts.FirstOrDefault();
                instructionalMethod = firstContact == null ? null : firstContact.InstructionalMethodCode;
                if (string.IsNullOrEmpty(instructionalMethod))
                {
                    instructionalMethod = (await GetCurriculumConfigurationAsync()).DefaultInstructionalMethodCode;
                }
            }

            if (string.IsNullOrEmpty(instructionalMethod))
            {
                // Integration API error InstructionalEvent.InvalidInstructionalMethod.NotFound
                var ex = new IntegrationApiException("Validation exception");
                ex.AddError(new IntegrationApiError()
                {
                    Code = "InstructionalEvent.InvalidInstructionalMethod.NotFound",
                    Message = "No valid instructional method found."
                });
                throw ex;
            }

            // The frequency code in Colleague represents both frequency and the interval
            string frequency = null;
            if (meeting.Recurrence != null && meeting.Recurrence.RepeatRule != null)
            {
                if (meeting.Recurrence.RepeatRule != null && interval.ToString() != null && meeting.Recurrence.RepeatRule.Type.ToString() != null)
                {
                    var repeatCode = scheduleRepeats.FirstOrDefault(x => meeting.Recurrence.RepeatRule.Type == (Dtos.FrequencyType2)x.FrequencyType && interval == x.Interval);
                    frequency = repeatCode == null ? null : repeatCode.Code;
                }
            }

            // Determine any overrides
            bool overrideRoomCapacity = false;
            bool overrideRoomAvailability = false;
            bool overrideFacultyAvailability = false;
            bool overrideFacultyCapacity = false;
            if (meeting.Approvals != null && meeting.Approvals.Any())
            {
                overrideRoomCapacity = meeting.Approvals.Any(x => x.Type == Dtos.InstructionalEventApprovalType2.RoomCapacity && x.ApprovingEntity == Dtos.InstructionalEventApprovalEntity2.User);
                overrideRoomAvailability = meeting.Approvals.Any(x => x.Type == Dtos.InstructionalEventApprovalType2.RoomAvailability && x.ApprovingEntity == Dtos.InstructionalEventApprovalEntity2.User);
                overrideFacultyAvailability = meeting.Approvals.Any(x => x.Type == Dtos.InstructionalEventApprovalType2.InstructorAvailability && x.ApprovingEntity == Dtos.InstructionalEventApprovalEntity2.User);
                overrideFacultyCapacity = meeting.Approvals.Any(x => x.Type == Dtos.InstructionalEventApprovalType2.InstructorCapacity && x.ApprovingEntity == Dtos.InstructionalEventApprovalEntity2.User);
            }

            // The room must exist, if specified
            string roomId = null;
            if (meeting.Locations != null && meeting.Locations.Any())
            {

                // Get the first entry where the room is not null and the GUID exists
                InstructionalRoom2 classroom = null;
                foreach (var location in meeting.Locations)
                {
                    if (location.Locations != null && location.Locations.LocationType == null)
                    {
                        // Integration API error InstructionalEvent.Section.NotFound
                        var ex = new IntegrationApiException("Validation exception");
                        ex.AddError(new IntegrationApiError()
                        {
                            Code = "InstructionalEvent.Locations.Location.Type",
                            Message = "Location type is required in order to assign a classroom."
                        });
                        throw ex;
                    }

                    if (location.Locations != null && location.Locations.LocationType == InstructionalLocationType.InstructionalRoom)
                    {
                        var instructionalRoom = location.Locations as Dtos.InstructionalRoom2;
                        if ((instructionalRoom != null) && (instructionalRoom.Room != null) && (instructionalRoom.Room.Id != null))
                        {
                            classroom = instructionalRoom;
                            break;
                        }
                    }
                }

                if (classroom != null && classroom.Room != null)
                {
                    var room = (await RoomsAsync()).FirstOrDefault(x => x.Guid == classroom.Room.Id);
                    if (room == null)
                    {
                        // Integration API error InstructionalEvent.Location.RoomNotFound
                        var ex = new IntegrationApiException("Validation exception");
                        ex.AddError(new IntegrationApiError()
                        {
                            Code = "InstructionalEvent.Location.RoomNotFound",
                            Message = "The room (" + classroom.Room.Id + ") was not found for the room assignment."
                        });
                        throw ex;
                    }
                    roomId = room.Id;
                    if (section.Capacity.HasValue && room.Capacity < section.Capacity)
                    {
                        if (overrideRoomCapacity)
                        {
                            logger.Info("Overriding room capacity for section " + section.Name + " and room " + room.Id);
                        }
                        else
                        {
                            // Integration API error InstructionalEvent.Location.InsufficientRoomCapacity
                            var ex = new IntegrationApiException("Validation exception");
                            ex.AddError(new IntegrationApiError()
                            {
                                Code = "InstructionalEvent.Location.InsufficientRoomCapacity",
                                Message = string.Format("Room {0} has capacity of {1}; section {2} has capacity of {3}", room.Id, room.Capacity, section.Name, section.Capacity)
                            });
                            throw ex;
                        }
                    }
                }
            }

            if (meeting.Recurrence != null)
            {
                if (meeting.Recurrence.TimePeriod == null)
                {
                    // Must have start and end date in the recurrence pattern
                    var ex = new IntegrationApiException("Validation exception");
                    ex.AddError(new IntegrationApiError() { Code = "InstructionalEvent.Recurrence.TimePeriod", Message = "Time period is required. " });
                    throw ex;
                }
                if (meeting.Recurrence.TimePeriod.StartOn == null)
                {
                    // Must have start and end date in the recurrence pattern
                    var ex = new IntegrationApiException("Validation exception");
                    ex.AddError(new IntegrationApiError() { Code = "InstructionalEvent.Recurrence.TimePeriod.StartOn", Message = "start date is required field when time period is not null. " });
                    throw ex;
                }
                if (meeting.Recurrence.TimePeriod.EndOn == null)
                {
                    // Must have start and end date in the recurrence pattern
                    var ex = new IntegrationApiException("Validation exception");
                    ex.AddError(new IntegrationApiError() { Code = "InstructionalEvent.Recurrence.TimePeriod.EndOn", Message = "end date is required field when time period is not null. " });
                    throw ex;
                }
            }
            if (meeting.Recurrence != null && meeting.Recurrence.TimePeriod != null)
            {
                // Check the date and time ranges
                if (meeting.Recurrence.TimePeriod.StartOn != null && meeting.Recurrence.TimePeriod.EndOn != null && meeting.Recurrence.TimePeriod.EndOn.Value.Date < meeting.Recurrence.TimePeriod.StartOn.Value.Date)
                {
                    // Integration API error InstructionalEvent.EndDate.OutOfRange
                    var ex = new IntegrationApiException("Validation exception");
                    ex.AddError(new IntegrationApiError() { Code = "InstructionalEvent.EndDate.OutOfRange", Message = "End date must be after start date." });
                    throw ex;
                }
                if (meeting.Recurrence.TimePeriod.StartOn != null && meeting.Recurrence.TimePeriod.EndOn != null && meeting.Recurrence.TimePeriod.EndOn.Value.ToLocalTime().TimeOfDay < meeting.Recurrence.TimePeriod.StartOn.Value.ToLocalTime().TimeOfDay)
                {
                    // Integration API error InstructionalEvent.EndTime.OutOfRange
                    var ex = new IntegrationApiException("Validation exception");
                    ex.AddError(new IntegrationApiError() { Code = "InstructionalEvent.EndTime.OutOfRange", Message = "End time must be after start time." });
                    throw ex;
                }
            }

            // Check any instructors assigned to the section
            var facultyLookup = new Dictionary<string, string>();
            if (meeting.Instructors != null && meeting.Instructors.Any())
            {
                if (meeting.Instructors.Any(x => x.ResponsibilityPercentage.HasValue) && meeting.Instructors.Sum(x => x.ResponsibilityPercentage) != 100)
                {
                    // Integration API error InstructionalEvent.InstructorRoster.InstructorResponsibilityOutOfRange
                    var ex = new IntegrationApiException("Validation exception");
                    ex.AddError(new IntegrationApiError()
                    {
                        Code = "InstructionalEvent.InstructorRoster.InstructorResponsibilityOutOfRange",
                        Message = "Instructor responsibility percentage does not total to 100%"
                    });
                    throw ex;
                }

                foreach (var instructor in meeting.Instructors)
                {
                    if (!string.IsNullOrEmpty(instructor.Instructor.Id))
                    {
                        string facultyId = await _personRepository.GetPersonIdFromGuidAsync(instructor.Instructor.Id);
                        if (string.IsNullOrEmpty(facultyId))
                        {
                            // Integration API error InstructionalEvent.InstructorRoster.Instructor.NotFound
                            var ex = new IntegrationApiException("Validation exception");
                            ex.AddError(new IntegrationApiError()
                            {
                                Code = "InstructionalEvent.InstructorRoster.Instructor.NotFound",
                                Message = "The instructor cannot be found in Colleague."
                            });
                            throw ex;
                        }
                        if (!await _personRepository.IsFacultyAsync(facultyId))
                        {
                            // Integration API error InstructionalEvent.InstructorRoster.Instructor.InvalidRole
                            var ex = new IntegrationApiException("Validation exception");
                            ex.AddError(new IntegrationApiError()
                            {
                                Code = "InstructionalEvent.InstructorRoster.Instructor.InvalidRole",
                                Message = "Instructor is not a faculty member."
                            });
                            throw ex;
                        }
                        // Build a lookup table to get the ID for an instructor's GUID
                        facultyLookup.Add(instructor.Instructor.Id, facultyId);
                    }
                }
            }

            //// If we have a room or instructors, check their availability
            //if (!string.IsNullOrEmpty(roomId) || instructors.Any() && meeting.StartDate.HasValue && meeting.EndDate.HasValue)
            //{
            //    // Calculate all the meeting dates of the section
            //    var campusCalendarId = _configurationRepository.GetDefaultsConfiguration().CampusCalendarId;
            //    var campusCalendar = _eventRepository.GetCalendar(campusCalendarId);
            //    var frequencyType = ConvertFrequencyTypeEnumDtoToFrequencyTypeDomainEnum(meeting.Recurrence.Frequency);
            //    var meetingDates = RoomAvailabilityService.BuildDateList(meeting.StartDate.Value, meeting.EndDate.Value, frequencyType, meeting.Recurrence.Interval, meeting.Recurrence.Days, campusCalendar.SpecialDays, campusCalendar.BookPastNumberOfDays);
            //}

            // Everything looks good - create a SectionMeeting entity
            var entity = new Domain.Student.Entities.SectionMeeting(meetingId, section.Id, instructionalMethod,
                (meeting.Recurrence != null && meeting.Recurrence.TimePeriod != null && meeting.Recurrence.TimePeriod.StartOn != null ? meeting.Recurrence.TimePeriod.StartOn.Value.Date : section.StartDate),
                (meeting.Recurrence != null && meeting.Recurrence.TimePeriod != null && meeting.Recurrence.TimePeriod.EndOn != null ? meeting.Recurrence.TimePeriod.EndOn.Value.Date : section.EndDate),
                frequency)
            {
                Guid = meeting.Id,
                StartTime = (meeting.Recurrence != null && meeting.Recurrence.TimePeriod != null ? meeting.Recurrence.TimePeriod.StartOn.Value.ToLocalTime() : new DateTimeOffset?()),
                EndTime = (meeting.Recurrence != null && meeting.Recurrence.TimePeriod != null ? meeting.Recurrence.TimePeriod.EndOn.Value.ToLocalTime() : new DateTimeOffset?()),
                Days = meeting.Recurrence == null || daysOfWeek == null ? new List<DayOfWeek>() : ConvertHedmDaysToDays(daysOfWeek),
                Room = roomId,
                Load = (meeting.Workload != null ? meeting.Workload : null),
                TotalMeetingMinutes = (meeting.Recurrence != null && daysOfWeek != null && daysOfWeek.Any() && meeting.Recurrence.RepeatRule != null && meeting.Recurrence.TimePeriod.StartOn != null ? CalculateMeetingMinutes2(meeting) : new int()),
                OverrideRoomCapacity = overrideRoomCapacity,
                OverrideRoomAvailability = overrideRoomAvailability,
                OverrideFacultyAvailability = overrideFacultyAvailability,
                OverrideFacultyCapacity = overrideFacultyCapacity
            };

            // Get the contact info for the meeting's instruction method
            // var contact = section.InstructionalContacts.FirstOrDefault(x => x.InstructionalMethodCode == entity.InstructionalMethodCode);
            // decimal loadAmount = contact == null ? 0 : contact.Load.GetValueOrDefault();
            decimal loadAmount = (meeting.Workload != null ? meeting.Workload.GetValueOrDefault() : new decimal());
            // Make sure the meeting we're working on is included
            if (string.IsNullOrEmpty(entity.Id) || !section.Meetings.Any(m => m.Id == entity.Id))
            {
                section.AddSectionMeeting(entity);
            }
            else
            {
                bool validChange = true;
                if (entity.Room == null && meeting.Locations != null && meeting.Locations.Any())
                {
                    // this is caused by data security on locations.location.room.id
                    validChange = false;

                }

                if (validChange)
                {
                    if (!string.IsNullOrEmpty(entity.Id) && section.Meetings.Any(m => m.Id == entity.Id))
                    {
                        // Add Faculty ID to list of Faculty from original roster
                        // so that we can remove the Ids of faculty no longer in the roster
                        foreach (var roster in section.Meetings.Where(m => m.Id == entity.Id).First().FacultyRoster)
                        {
                            if (!string.IsNullOrEmpty(roster.FacultyId))
                            {
                                entity.AddFacultyId(roster.FacultyId);
                            }
                        }
                        // Removed the existing item since it's being replaced.
                        section.RemoveSectionMeeting(entity);
                    }
                    // Add the new item with the updated information.
                    section.AddSectionMeeting(entity);
                }
            }
            if (loadAmount > 0)
            {
                // SectionProcessor.AdjustMeetingLoads(section.Meetings.ToList(), loadAmount);
            }

            int entityPos = 0;
            try
            {
                entityPos = section.Meetings.IndexOf(section.Meetings.Where(m => m.Guid == entity.Guid).First());
            }
            catch (Exception)
            {
                // Integration API error InstructionalEvent.SectionMeeting.Guid.NotFound
                var ex = new IntegrationApiException("Validation exception");
                ex.AddError(new IntegrationApiError()
                {
                    Code = "InstructionalEvent.SectionMeeting.Id.NotFound",
                    Message = "The section meeting GUID is not valid for this meeting instance."
                });
                throw ex;
            }

            if (meeting.Instructors != null && meeting.Instructors.Any())
            {
                foreach (var instructor in meeting.Instructors)
                {
                    if (!string.IsNullOrEmpty(instructor.Instructor.Id))
                    {
                        DateTime startDate = instructor.WorkStartDate.HasValue ? instructor.WorkStartDate.GetValueOrDefault().DateTime : entity.StartDate.GetValueOrDefault();
                        DateTime endDate = instructor.WorkEndDate.HasValue ? instructor.WorkEndDate.GetValueOrDefault().DateTime : entity.EndDate.GetValueOrDefault();
                        var sectionFaculty = new Domain.Student.Entities.SectionFaculty(null, section.Id, facultyLookup[instructor.Instructor.Id],
                            instructionalMethod, startDate, endDate, instructor.ResponsibilityPercentage.GetValueOrDefault())
                        {
                            LoadFactor = instructor.WorkLoadPercentage
                        };
                        section.Meetings[entityPos].AddSectionFaculty(sectionFaculty);
                    }
                }

                // Now, check the responsibility percentage and load for the faculty on the new meeting
                SectionProcessor.AdjustFacultyPercentages(section.Meetings[entityPos].FacultyRoster, section.Meetings[entityPos].Load);
                if (section.Meetings[entityPos].Load.HasValue)
                {
                    SectionProcessor.AdjustFacultyLoads(section.Meetings[entityPos].FacultyRoster, section.Meetings[entityPos].Load.Value);
                }

                // Now verify that the faculty associated with each meeting are correct: they should total to 100% responsibility,
                // and match the load for this meeting time/section offering
                foreach (var sectionMeeting in section.Meetings)
                {
                    SectionProcessor.AdjustFacultyPercentages(sectionMeeting.FacultyRoster, sectionMeeting.Load);
                    if (sectionMeeting.Load.HasValue && sectionMeeting.Load.Value > 0)
                    {
                        //var faculty = sectionMeeting.FacultyRoster.ToList();
                        SectionProcessor.AdjustFacultyLoads(sectionMeeting.FacultyRoster, sectionMeeting.Load.Value);
                    }
                }

                // Now, build/update the section faculty on the section from those on the section meetings
                SectionProcessor.UpdateSectionFacultyFromSectionMeetings(section, entity.Guid);
            }

            return section;
        }

        private int CalculateMeetingMinutes2(Dtos.InstructionalEvent2 meeting)
        {
            if (meeting.Recurrence.TimePeriod.StartOn == null || meeting.Recurrence.TimePeriod.EndOn == null)
            {
                return 0;
            }

            // Calculate all the meeting dates of the section
            var campusCalendarId = defaultConfiguration.CampusCalendarId;
            var campusCalendar = _eventRepository.GetCalendar(campusCalendarId);
            var frequencyType = ConvertFrequencyType2EnumDtoToFrequencyTypeDomainEnum(meeting.Recurrence.RepeatRule.Type);

            var meetingDates = Ellucian.Colleague.Coordination.Base.RecurrenceUtility.GetRecurrenceDates(meeting.Recurrence, frequencyType, campusCalendar);
            //var meetingDates = RoomAvailabilityService.BuildDateList(meeting.Recurrence.TimePeriod.StartOn.Date, meeting.Recurrence.TimePeriod.EndOn.Date, frequencyType, meeting.Recurrence.RepeatRule.Interval, meeting.Recurrence.RepeatRule.Days, campusCalendar.SpecialDays, campusCalendar.BookPastNumberOfDays);

            var meetingTime = meeting.Recurrence.TimePeriod.EndOn.Value.ToLocalTime().TimeOfDay - meeting.Recurrence.TimePeriod.StartOn.Value.ToLocalTime().TimeOfDay;

            return ((meetingTime.Hours * 60) + meetingTime.Minutes) * meetingDates.Count();
        }

        #endregion

        #region Convert Instructional Events Version 8

        private async Task<Domain.Student.Entities.Section> ConvertInstructionalEvent3ToSectionMeetingAsync(Dtos.InstructionalEvent3 meeting)
        {
            // Initialize the logger for the section processor service
            SectionProcessor.InitializeLogger(logger);

            // Check for required data - A GUID must be supplied, but it may not correspond to an ID if this is a new record
            if (string.IsNullOrEmpty(meeting.Id))
            {
                // Integration API error InstructionalEvent.NotFound
                var ex = new IntegrationApiException("Validation exception");
                ex.AddError(new IntegrationApiError() { Code = "InstructionalEvent.NotFound", Message = "The instructional event was not supplied." });
                throw ex;
            }
            if (meeting.Section == null || string.IsNullOrEmpty(meeting.Section.Id))
            {
                // Integration API error InstructionalEvent.Section.NotFound
                var ex = new IntegrationApiException("Validation exception");
                ex.AddError(new IntegrationApiError()
                {
                    Code = "InstructionalEvent.Section.NotFound",
                    Message = "Section id is required in order to schedule meeting times."
                });
                throw ex;
            }
            string meetingId = await _sectionRepository.GetSectionMeetingIdFromGuidAsync(meeting.Id);

            // Section ID is required
            Domain.Student.Entities.Section section = null;
            try
            {
                section = meeting.Section != null && !string.IsNullOrEmpty(meeting.Section.Id) ? await _sectionRepository.GetSectionByGuidAsync(meeting.Section.Id) : null;
            }
            catch (KeyNotFoundException)
            {
                // Fall through - we want this condition to do the same as if the section is null
            }
            if (section == null || string.IsNullOrEmpty(section.Id))
            {
                // Integration API error InstructionalEvent.Section.NotFound
                var ex = new IntegrationApiException("Validation exception");
                ex.AddError(new IntegrationApiError()
                {
                    Code = "InstructionalEvent.Section.NotFound",
                    Message = "Section id is required in order to schedule meeting times."
                });
                throw ex;
            }

            if (meeting.Recurrence != null)
            {
                if (meeting.Recurrence.RepeatRule == null)
                {
                    // Integration API error InstructionalEvent.InvalidInstructionalMethod.NotFound
                    var ex = new IntegrationApiException("Validation exception");
                    ex.AddError(new IntegrationApiError()
                    {
                        Code = "InstructionalEvent.Recurrence.RepeatRule",
                        Message = "Repeat rule is required when recurrence is not null. "
                    });
                    throw ex;
                }
                if (meeting.Recurrence.RepeatRule.Type == null)
                {
                    // Integration API error InstructionalEvent.InvalidInstructionalMethod.NotFound
                    var ex = new IntegrationApiException("Validation exception");
                    ex.AddError(new IntegrationApiError()
                    {
                        Code = "InstructionalEvent.Recurrence.RepeatRule.Type",
                        Message = "Repeat rule type was not specified and is required. "
                    });
                    throw ex;
                }
                if (meeting.Recurrence.TimePeriod == null || (meeting.Recurrence.TimePeriod.StartOn == null && meeting.Recurrence.TimePeriod.EndOn == null))
                {
                    // Integration API error InstructionalEvent.Recurrence.TimePeriod
                    var ex = new IntegrationApiException("Validation exception");
                    ex.AddError(new IntegrationApiError()
                    {
                        Code = "InstructionalEvent.Recurrence.TimePeriod",
                        Message = "At least one timePeriod must be specified on the recurrence pattern. "
                    });
                    throw ex;
                }
            }

            int interval = new int();
            List<HedmDayOfWeek?> daysOfWeek = new List<HedmDayOfWeek?>();

            if (meeting.Recurrence != null && meeting.Recurrence.RepeatRule != null && meeting.Recurrence.RepeatRule.Interval.ToString() != null)
            {
                var frequencyType = (Dtos.FrequencyType2?)meeting.Recurrence.RepeatRule.Type;
                if (frequencyType == null)
                {
                    // Integration API error InstructionalEvent.Section.NotFound
                    var ex = new IntegrationApiException("Validation exception");
                    ex.AddError(new IntegrationApiError()
                    {
                        Code = "InstructionalEvent.Recurrence.RepeatRule.Type",
                        Message = "RepeatRule type is required in order to schedule meeting times."
                    });
                    throw ex;
                }

                interval = meeting.Recurrence.RepeatRule.Interval;
                switch ((Dtos.FrequencyType2)meeting.Recurrence.RepeatRule.Type)
                {
                    case Dtos.FrequencyType2.Daily:
                        var repeatRuleDaily = (RepeatRuleDaily)meeting.Recurrence.RepeatRule;
                        if (repeatRuleDaily != null)
                        {
                            daysOfWeek = Enum.GetValues(typeof(HedmDayOfWeek)).Cast<HedmDayOfWeek?>().ToList();
                        }
                        break;
                    case Dtos.FrequencyType2.Weekly:
                        var repeatRuleWeekly = (RepeatRuleWeekly)meeting.Recurrence.RepeatRule;
                        if (repeatRuleWeekly != null)
                        {
                            daysOfWeek = repeatRuleWeekly.DayOfWeek;
                        }
                        break;
                    case Dtos.FrequencyType2.Monthly:
                        var repeatRuleMonthly = (RepeatRuleMonthly)meeting.Recurrence.RepeatRule;
                        if (repeatRuleMonthly != null)
                        {
                            if ((repeatRuleMonthly.RepeatBy != null) && (repeatRuleMonthly.RepeatBy.DayOfWeek != null))
                                daysOfWeek = new List<HedmDayOfWeek?>() { repeatRuleMonthly.RepeatBy.DayOfWeek.Day };
                        }
                        break;
                    case Dtos.FrequencyType2.Yearly:

                        break;
                    default: break;
                }
            }

            string instructionalMethod = meeting.InstructionalMethod == null ? null : ConvertGuidToCode(await InstructionalMethodsAsync(), meeting.InstructionalMethod.Id);
            // Instructional method is required - use the (first) instructional method from the section
            if (string.IsNullOrEmpty(instructionalMethod))
            {
                var firstContact = section.InstructionalContacts.FirstOrDefault();
                instructionalMethod = firstContact == null ? null : firstContact.InstructionalMethodCode;
                if (string.IsNullOrEmpty(instructionalMethod))
                {
                    instructionalMethod = (await GetCurriculumConfigurationAsync()).DefaultInstructionalMethodCode;
                }
            }

            if (string.IsNullOrEmpty(instructionalMethod))
            {
                // Integration API error InstructionalEvent.InvalidInstructionalMethod.NotFound
                var ex = new IntegrationApiException("Validation exception");
                ex.AddError(new IntegrationApiError()
                {
                    Code = "InstructionalEvent.InvalidInstructionalMethod.NotFound",
                    Message = "No valid instructional method found."
                });
                throw ex;
            }

            // The frequency code in Colleague represents both frequency and the interval
            string frequency = null;
            if (meeting.Recurrence != null && meeting.Recurrence.RepeatRule != null)
            {
                if (meeting.Recurrence.RepeatRule != null && interval.ToString() != null && meeting.Recurrence.RepeatRule.Type.ToString() != null)
                {
                    var repeatCode = scheduleRepeats.FirstOrDefault(x => meeting.Recurrence.RepeatRule.Type == (Dtos.FrequencyType2)x.FrequencyType && interval == x.Interval);
                    frequency = repeatCode == null ? null : repeatCode.Code;
                }
            }

            // Determine any overrides
            bool overrideRoomCapacity = false;
            bool overrideRoomAvailability = false;
            bool overrideFacultyAvailability = false;
            bool overrideFacultyCapacity = false;
            if (meeting.Approvals != null && meeting.Approvals.Any())
            {
                overrideRoomCapacity = meeting.Approvals.Any(x => x.Type == Dtos.InstructionalEventApprovalType2.RoomCapacity && x.ApprovingEntity == Dtos.InstructionalEventApprovalEntity2.User);
                overrideRoomAvailability = meeting.Approvals.Any(x => x.Type == Dtos.InstructionalEventApprovalType2.RoomAvailability && x.ApprovingEntity == Dtos.InstructionalEventApprovalEntity2.User);
                overrideFacultyAvailability = meeting.Approvals.Any(x => x.Type == Dtos.InstructionalEventApprovalType2.InstructorAvailability && x.ApprovingEntity == Dtos.InstructionalEventApprovalEntity2.User);
                overrideFacultyCapacity = meeting.Approvals.Any(x => x.Type == Dtos.InstructionalEventApprovalType2.InstructorCapacity && x.ApprovingEntity == Dtos.InstructionalEventApprovalEntity2.User);
            }

            // The room must exist, if specified
            string roomId = null;
            if (meeting.Locations != null && meeting.Locations.Any())
            {

                // Get the first entry where the room is not null and the GUID exists
                InstructionalRoom2 classroom = null;
                foreach (var location in meeting.Locations)
                {
                    if (location.Locations != null && location.Locations.LocationType == null)
                    {
                        // Integration API error InstructionalEvent.Section.NotFound
                        var ex = new IntegrationApiException("Validation exception");
                        ex.AddError(new IntegrationApiError()
                        {
                            Code = "InstructionalEvent.Locations.Location.Type",
                            Message = "Location type is required in order to assign a classroom."
                        });
                        throw ex;
                    }

                    if (location.Locations != null && location.Locations.LocationType == InstructionalLocationType.InstructionalRoom)
                    {
                        var instructionalRoom = location.Locations as Dtos.InstructionalRoom2;
                        if ((instructionalRoom != null) && (instructionalRoom.Room != null) && (instructionalRoom.Room.Id != null))
                        {
                            classroom = instructionalRoom;
                            break;
                        }
                    }
                }
                if (classroom != null && classroom.Room != null)
                {
                    var room = (await RoomsAsync()).FirstOrDefault(x => x.Guid == classroom.Room.Id);
                    if (room == null)
                    {
                        // Integration API error InstructionalEvent.Location.RoomNotFound
                        var ex = new IntegrationApiException("Validation exception");
                        ex.AddError(new IntegrationApiError()
                        {
                            Code = "InstructionalEvent.Location.RoomNotFound",
                            Message = "The room (" + classroom.Room.Id + ") was not found for the room assignment."
                        });
                        throw ex;
                    }
                    roomId = room.Id;
                    if (section.Capacity.HasValue && room.Capacity < section.Capacity)
                    {
                        if (overrideRoomCapacity)
                        {
                            logger.Info("Overriding room capacity for section " + section.Name + " and room " + room.Id);
                        }
                        else
                        {
                            // Integration API error InstructionalEvent.Location.InsufficientRoomCapacity
                            var ex = new IntegrationApiException("Validation exception");
                            ex.AddError(new IntegrationApiError()
                            {
                                Code = "InstructionalEvent.Location.InsufficientRoomCapacity",
                                Message = string.Format("Room {0} has capacity of {1}; section {2} has capacity of {3}", room.Id, room.Capacity, section.Name, section.Capacity)
                            });
                            throw ex;
                        }
                    }
                }
            }

            if (meeting.Recurrence != null)
            {
                if (meeting.Recurrence.TimePeriod == null)
                {
                    // Must have start and end date in the recurrence pattern
                    var ex = new IntegrationApiException("Validation exception");
                    ex.AddError(new IntegrationApiError() { Code = "InstructionalEvent.Recurrence.TimePeriod", Message = "Time period is required. " });
                    throw ex;
                }
                if (meeting.Recurrence.TimePeriod.StartOn == null)
                {
                    // Must have start and end date in the recurrence pattern
                    var ex = new IntegrationApiException("Validation exception");
                    ex.AddError(new IntegrationApiError() { Code = "InstructionalEvent.Recurrence.TimePeriod.StartOn", Message = "start date is required field when time period is not null. " });
                    throw ex;
                }
                if (meeting.Recurrence.TimePeriod.EndOn == null)
                {
                    // Must have start and end date in the recurrence pattern
                    var ex = new IntegrationApiException("Validation exception");
                    ex.AddError(new IntegrationApiError() { Code = "InstructionalEvent.Recurrence.TimePeriod.EndOn", Message = "end date is required field when time period is not null. " });
                    throw ex;
                }
            }
            if (meeting.Recurrence != null && meeting.Recurrence.TimePeriod != null)
            {
                // Check the date and time ranges
                if (meeting.Recurrence.TimePeriod.StartOn != null && meeting.Recurrence.TimePeriod.EndOn != null && meeting.Recurrence.TimePeriod.EndOn.Value.Date < meeting.Recurrence.TimePeriod.StartOn.Value.Date)
                {
                    // Integration API error InstructionalEvent.EndDate.OutOfRange
                    var ex = new IntegrationApiException("Validation exception");
                    ex.AddError(new IntegrationApiError() { Code = "InstructionalEvent.EndDate.OutOfRange", Message = "End date must be after start date." });
                    throw ex;
                }
                if (meeting.Recurrence.TimePeriod.StartOn != null && meeting.Recurrence.TimePeriod.EndOn != null && meeting.Recurrence.TimePeriod.EndOn.Value.ToLocalTime().TimeOfDay < meeting.Recurrence.TimePeriod.StartOn.Value.ToLocalTime().TimeOfDay)
                {
                    // Integration API error InstructionalEvent.EndTime.OutOfRange
                    var ex = new IntegrationApiException("Validation exception");
                    ex.AddError(new IntegrationApiError() { Code = "InstructionalEvent.EndTime.OutOfRange", Message = "End time must be after start time." });
                    throw ex;
                }
            }

            // Check any instructors assigned to the section
            var facultyLookup = new Dictionary<string, string>();
            if (meeting.Instructors != null && meeting.Instructors.Any())
            {
                if (meeting.Instructors.Any(x => x.ResponsibilityPercentage.HasValue) && meeting.Instructors.Sum(x => x.ResponsibilityPercentage) != 100)
                {
                    // Integration API error InstructionalEvent.InstructorRoster.InstructorResponsibilityOutOfRange
                    var ex = new IntegrationApiException("Validation exception");
                    ex.AddError(new IntegrationApiError()
                    {
                        Code = "InstructionalEvent.InstructorRoster.InstructorResponsibilityOutOfRange",
                        Message = "Instructor responsibility percentage does not total to 100%"
                    });
                    throw ex;
                }

                foreach (var instructor in meeting.Instructors)
                {
                    if (!string.IsNullOrEmpty(instructor.Instructor.Id))
                    {
                        string facultyId = await _personRepository.GetPersonIdFromGuidAsync(instructor.Instructor.Id);
                        if (string.IsNullOrEmpty(facultyId))
                        {
                            // Integration API error InstructionalEvent.InstructorRoster.Instructor.NotFound
                            var ex = new IntegrationApiException("Validation exception");
                            ex.AddError(new IntegrationApiError()
                            {
                                Code = "InstructionalEvent.InstructorRoster.Instructor.NotFound",
                                Message = "The instructor cannot be found in Colleague."
                            });
                            throw ex;
                        }
                        if (!await _personRepository.IsFacultyAsync(facultyId))
                        {
                            // Integration API error InstructionalEvent.InstructorRoster.Instructor.InvalidRole
                            var ex = new IntegrationApiException("Validation exception");
                            ex.AddError(new IntegrationApiError()
                            {
                                Code = "InstructionalEvent.InstructorRoster.Instructor.InvalidRole",
                                Message = "Instructor is not a faculty member."
                            });
                            throw ex;
                        }
                        // Build a lookup table to get the ID for an instructor's GUID
                        facultyLookup.Add(instructor.Instructor.Id, facultyId);
                    }
                }
            }

            // Everything looks good - create a SectionMeeting entity
            var entity = new Domain.Student.Entities.SectionMeeting(meetingId, section.Id, instructionalMethod,
                (meeting.Recurrence != null && meeting.Recurrence.TimePeriod != null && meeting.Recurrence.TimePeriod.StartOn != null ? meeting.Recurrence.TimePeriod.StartOn.Value.Date : section.StartDate),
                (meeting.Recurrence != null && meeting.Recurrence.TimePeriod != null && meeting.Recurrence.TimePeriod.EndOn != null ? meeting.Recurrence.TimePeriod.EndOn.Value.Date : section.EndDate),
                frequency)
            {
                Guid = meeting.Id,
                StartTime = (meeting.Recurrence != null && meeting.Recurrence.TimePeriod != null ? meeting.Recurrence.TimePeriod.StartOn.Value.ToLocalTime() : new DateTimeOffset?()),
                EndTime = (meeting.Recurrence != null && meeting.Recurrence.TimePeriod != null ? meeting.Recurrence.TimePeriod.EndOn.Value.ToLocalTime() : new DateTimeOffset?()),
                Days = meeting.Recurrence == null || daysOfWeek == null ? new List<DayOfWeek>() : ConvertHedmDaysToDays(daysOfWeek),
                Room = roomId,
                Load = (meeting.Workload != null ? meeting.Workload : null),
                TotalMeetingMinutes = (meeting.Recurrence != null && daysOfWeek != null && daysOfWeek.Any() && meeting.Recurrence.RepeatRule != null && meeting.Recurrence.TimePeriod.StartOn != null ? CalculateMeetingMinutes3(meeting) : new int()),
                OverrideRoomCapacity = overrideRoomCapacity,
                OverrideRoomAvailability = overrideRoomAvailability,
                OverrideFacultyAvailability = overrideFacultyAvailability,
                OverrideFacultyCapacity = overrideFacultyCapacity
            };

            // Get the contact info for the meeting's instruction method
            // var contact = section.InstructionalContacts.FirstOrDefault(x => x.InstructionalMethodCode == entity.InstructionalMethodCode);
            // decimal loadAmount = contact == null ? 0 : contact.Load.GetValueOrDefault();
            decimal loadAmount = (meeting.Workload != null ? meeting.Workload.GetValueOrDefault() : new decimal());
            // Make sure the meeting we're working on is included
            if (string.IsNullOrEmpty(entity.Id) || !section.Meetings.Any(m => m.Id == entity.Id))
            {
                section.AddSectionMeeting(entity);
            }
            else
            {

                bool validChange = true;
                if (entity.Room == null && meeting.Locations != null && meeting.Locations.Any())
                {
                    // this is caused by data security on locations.location.room.id
                    validChange = false;

                }

                if (validChange)
                {
                    if (!string.IsNullOrEmpty(entity.Id) && section.Meetings.Any(m => m.Id == entity.Id))
                    {
                        // Add Faculty ID to list of Faculty from original roster
                        // so that we can remove the Ids of faculty no longer in the roster
                        foreach (var roster in section.Meetings.Where(m => m.Id == entity.Id).First().FacultyRoster)
                        {
                            if (!string.IsNullOrEmpty(roster.FacultyId))
                            {
                                entity.AddFacultyId(roster.FacultyId);
                            }
                        }
                        // Removed the existing item since it's being replaced.
                        section.RemoveSectionMeeting(entity);
                    }
                    // Add the new item with the updated information.
                    section.AddSectionMeeting(entity);
                }
            }
            if (loadAmount > 0)
            {
                // SectionProcessor.AdjustMeetingLoads(section.Meetings.ToList(), loadAmount);
            }

            int entityPos = 0;
            try
            {
                entityPos = section.Meetings.IndexOf(section.Meetings.Where(m => m.Guid == entity.Guid).First());
            }
            catch (Exception)
            {
                // Integration API error InstructionalEvent.SectionMeeting.Guid.NotFound
                var ex = new IntegrationApiException("Validation exception");
                ex.AddError(new IntegrationApiError()
                {
                    Code = "InstructionalEvent.SectionMeeting.Id.NotFound",
                    Message = "The section meeting GUID is not valid for this meeting instance."
                });
                throw ex;
            }

            if (meeting.Instructors != null && meeting.Instructors.Any())
            {
                foreach (var instructor in meeting.Instructors)
                {
                    if (!string.IsNullOrEmpty(instructor.Instructor.Id))
                    {
                        DateTime startDate = instructor.WorkStartDate.HasValue ? instructor.WorkStartDate.GetValueOrDefault().DateTime : entity.StartDate.GetValueOrDefault();
                        DateTime endDate = instructor.WorkEndDate.HasValue ? instructor.WorkEndDate.GetValueOrDefault().DateTime : entity.EndDate.GetValueOrDefault();
                        var sectionFaculty = new Domain.Student.Entities.SectionFaculty(null, section.Id, facultyLookup[instructor.Instructor.Id],
                            instructionalMethod, startDate, endDate, instructor.ResponsibilityPercentage.GetValueOrDefault())
                        {
                            LoadFactor = instructor.WorkLoadPercentage
                        };
                        section.Meetings[entityPos].AddSectionFaculty(sectionFaculty);
                    }
                }

                // Now, check the responsibility percentage and load for the faculty on the new meeting
                SectionProcessor.AdjustFacultyPercentages(section.Meetings[entityPos].FacultyRoster, section.Meetings[entityPos].Load);
                if (section.Meetings[entityPos].Load.HasValue)
                {
                    SectionProcessor.AdjustFacultyLoads(section.Meetings[entityPos].FacultyRoster, section.Meetings[entityPos].Load.Value);
                }

                // Now verify that the faculty associated with each meeting are correct: they should total to 100% responsibility,
                // and match the load for this meeting time/section offering
                foreach (var sectionMeeting in section.Meetings)
                {
                    SectionProcessor.AdjustFacultyPercentages(sectionMeeting.FacultyRoster, sectionMeeting.Load);
                    if (sectionMeeting.Load.HasValue && sectionMeeting.Load.Value > 0)
                    {
                        //var faculty = sectionMeeting.FacultyRoster.ToList();
                        SectionProcessor.AdjustFacultyLoads(sectionMeeting.FacultyRoster, sectionMeeting.Load.Value);
                    }
                }

                // Now, build/update the section faculty on the section from those on the section meetings
                SectionProcessor.UpdateSectionFacultyFromSectionMeetings(section, entity.Guid);
            }

            return section;
        }

        private int CalculateMeetingMinutes3(Dtos.InstructionalEvent3 meeting)
        {
            if (meeting.Recurrence.TimePeriod.StartOn == null || meeting.Recurrence.TimePeriod.EndOn == null)
            {
                return 0;
            }

            // Calculate all the meeting dates of the section
            var campusCalendarId = defaultConfiguration.CampusCalendarId;
            var campusCalendar = _eventRepository.GetCalendar(campusCalendarId);
            var frequencyType = ConvertFrequencyType2EnumDtoToFrequencyTypeDomainEnum(meeting.Recurrence.RepeatRule.Type);

            var meetingDates = Ellucian.Colleague.Coordination.Base.RecurrenceUtility.GetRecurrenceDates(meeting.Recurrence, frequencyType, campusCalendar);
            //var meetingDates = RoomAvailabilityService.BuildDateList(meeting.Recurrence.TimePeriod.StartOn.Date, meeting.Recurrence.TimePeriod.EndOn.Date, frequencyType, meeting.Recurrence.RepeatRule.Interval, meeting.Recurrence.RepeatRule.Days, campusCalendar.SpecialDays, campusCalendar.BookPastNumberOfDays);

            var meetingTime = meeting.Recurrence.TimePeriod.EndOn.Value.ToLocalTime().TimeOfDay - meeting.Recurrence.TimePeriod.StartOn.Value.ToLocalTime().TimeOfDay;

            return ((meetingTime.Hours * 60) + meetingTime.Minutes) * meetingDates.Count();
        }
        #endregion

        #region Convert Instructional Events Version 11

        /// <summary>
        /// Converts DTO to entity
        /// </summary>
        /// <param name="meeting"></param>
        /// <returns></returns>
        private async Task<Domain.Student.Entities.Section> ConvertInstructionalEvent4ToSectionMeetingAsync(Dtos.InstructionalEvent4 meeting)
        {
            Domain.Student.Entities.Section section = null;
            string meetingId = string.Empty;
            string instructionalMethod = string.Empty;

            // Initialize the logger for the section processor service
            SectionProcessor.InitializeLogger(logger);

            // Check for required data - A GUID must be supplied, but it may not correspond to an ID if this is a new record
            if (string.IsNullOrEmpty(meeting.Id))
            {
                IntegrationApiExceptionAddError("Missing request body.", "Missing.Request.Body");
                return section;
            }

            if (string.IsNullOrEmpty(meeting.Id))
            {
                IntegrationApiExceptionAddError("The instructional event was not supplied.", "Missing.Required.Property");
            }
            else
            {
                try
                {
                    meetingId = await _sectionRepository.GetSectionMeetingIdFromGuidAsync(meeting.Id);
                }
                catch (KeyNotFoundException ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", meeting.Id, "",
                       System.Net.HttpStatusCode.NotFound);
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", meeting.Id);
                }
            }

            if (meeting.Section == null || string.IsNullOrEmpty(meeting.Section.Id))
            {
                IntegrationApiExceptionAddError("Section id is required in order to schedule meeting times.", "Missing.Required.Property");
            }
            else
            {

                try
                {
                    meetingId = await _sectionRepository.GetSectionMeetingIdFromGuidAsync(meeting.Id);
                    // Section ID is required

                    section = await _sectionRepository.GetSectionByGuidAsync(meeting.Section.Id);

                }
                catch (KeyNotFoundException ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", meeting.Id, "",
                       System.Net.HttpStatusCode.NotFound);
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", meeting.Id);
                }

                if (section == null || string.IsNullOrEmpty(section.Id))
                {
                    IntegrationApiExceptionAddError("Section id is required in order to schedule meeting times.", "Validation.Exception");
                }
            }


            instructionalMethod = meeting.InstructionalMethod == null ? null : ConvertGuidToCode(await InstructionalMethodsAsync(), meeting.InstructionalMethod.Id);
            // Instructional method is required - use the (first) instructional method from the section
            if (string.IsNullOrEmpty(instructionalMethod))
            {
                var firstContact = section.InstructionalContacts.FirstOrDefault();
                instructionalMethod = firstContact == null ? null : firstContact.InstructionalMethodCode;
                if (string.IsNullOrEmpty(instructionalMethod))
                {
                    var curriculumConfiguration = await GetCurriculumConfigurationAsync();
                    if (curriculumConfiguration != null && !string.IsNullOrEmpty(curriculumConfiguration.DefaultInstructionalMethodCode))
                    {
                        instructionalMethod = curriculumConfiguration.DefaultInstructionalMethodCode;
                    }                    
                }
            }
            if (string.IsNullOrEmpty(instructionalMethod))
            {
                IntegrationApiExceptionAddError("No valid instructional method found.", "Validation.Exception");
            }


            int interval = new int();
            var daysOfWeek = new List<HedmDayOfWeek?>();

            if (meeting.Recurrence != null)
            {
                if (meeting.Recurrence.RepeatRule == null)
                {

                    IntegrationApiExceptionAddError("Repeat rule is required when recurrence is not null.", "Validation.Exception");
                    // Integration API error InstructionalEvent.InvalidInstructionalMethod.NotFound
                }
                if (meeting.Recurrence.RepeatRule != null && meeting.Recurrence.RepeatRule.Type == null)
                {
                    IntegrationApiExceptionAddError("Repeat rule type was not specified and is required.", "Validation.Exception");
                }
                if (meeting.Recurrence.TimePeriod == null || (meeting.Recurrence.TimePeriod.StartOn == null && meeting.Recurrence.TimePeriod.EndOn == null))
                {
                    IntegrationApiExceptionAddError("At least one timePeriod must be specified on the recurrence pattern.", "Validation.Exception");
                }

                if (meeting.Recurrence.RepeatRule != null && meeting.Recurrence.RepeatRule.Interval.ToString() != null)
                {
                    var frequencyType = (Dtos.FrequencyType2?)meeting.Recurrence.RepeatRule.Type;
                    if (frequencyType == null)
                    {
                        IntegrationApiExceptionAddError("RepeatRule type is required in order to schedule meeting times.", "Validation.Exception");
                    }

                    interval = meeting.Recurrence.RepeatRule.Interval;
                    switch ((Dtos.FrequencyType2)meeting.Recurrence.RepeatRule.Type)
                    {
                        case Dtos.FrequencyType2.Daily:
                            var repeatRuleDaily = (RepeatRuleDaily)meeting.Recurrence.RepeatRule;
                            if (repeatRuleDaily != null)
                            {
                                daysOfWeek = Enum.GetValues(typeof(HedmDayOfWeek)).Cast<HedmDayOfWeek?>().ToList();
                            }
                            break;
                        case Dtos.FrequencyType2.Weekly:
                            var repeatRuleWeekly = (RepeatRuleWeekly)meeting.Recurrence.RepeatRule;
                            if (repeatRuleWeekly != null)
                            {
                                daysOfWeek = repeatRuleWeekly.DayOfWeek;
                            }
                            break;
                        case Dtos.FrequencyType2.Monthly:
                            var repeatRuleMonthly = (RepeatRuleMonthly)meeting.Recurrence.RepeatRule;
                            if (repeatRuleMonthly != null)
                            {
                                if ((repeatRuleMonthly.RepeatBy != null) && (repeatRuleMonthly.RepeatBy.DayOfWeek != null))
                                    daysOfWeek = new List<HedmDayOfWeek?>() { repeatRuleMonthly.RepeatBy.DayOfWeek.Day };
                            }
                            break;
                        case Dtos.FrequencyType2.Yearly:

                            break;
                        default: break;
                    }
                }

                if (meeting.Recurrence.TimePeriod == null)
                {
                    IntegrationApiExceptionAddError("Time period is required.", "Validation.Exception");
                }
                else
                {
                    if (meeting.Recurrence.TimePeriod.StartOn == null)
                    {
                        IntegrationApiExceptionAddError("Start date is required field when time period is not null.", "Validation.Exception");
                    }
                    if (meeting.Recurrence.TimePeriod.EndOn == null)
                    {
                        IntegrationApiExceptionAddError("End date is required field when time period is not null.", "Validation.Exception");
                    }

                    if (meeting.Recurrence.TimePeriod.StartOn != null && meeting.Recurrence.TimePeriod.EndOn != null)
                    {
                        // Check the date and time ranges
                        if (meeting.Recurrence.TimePeriod.EndOn.Value.Date < meeting.Recurrence.TimePeriod.StartOn.Value.Date)
                        {
                            IntegrationApiExceptionAddError("End date must be after start date.", "Validation.Exception");
                        }
                        if (meeting.Recurrence.TimePeriod.EndOn.Value.ToLocalTime().TimeOfDay < meeting.Recurrence.TimePeriod.StartOn.Value.ToLocalTime().TimeOfDay)
                        {
                            IntegrationApiExceptionAddError("End time must be after start time.", "Validation.Exception");
                        }
                    }
                }
            }


            // The frequency code in Colleague represents both frequency and the interval
            string frequency = null;
            if (meeting.Recurrence != null && meeting.Recurrence.RepeatRule != null)
            {
                if (meeting.Recurrence.RepeatRule != null && interval.ToString() != null && meeting.Recurrence.RepeatRule.Type.ToString() != null)
                {
                    var repeatCode = scheduleRepeats.FirstOrDefault(x => meeting.Recurrence.RepeatRule.Type == (Dtos.FrequencyType2)x.FrequencyType && interval == x.Interval);
                    frequency = repeatCode == null ? null : repeatCode.Code;
                }
            }

            // Determine any overrides
            bool overrideRoomCapacity = false;
            bool overrideRoomAvailability = false;
            bool overrideFacultyAvailability = false;
            bool overrideFacultyCapacity = false;
            if (meeting.Approvals != null && meeting.Approvals.Any())
            {
                overrideRoomCapacity = meeting.Approvals.Any(x => x.Type == Dtos.InstructionalEventApprovalType2.RoomCapacity && x.ApprovingEntity == Dtos.InstructionalEventApprovalEntity2.User);
                overrideRoomAvailability = meeting.Approvals.Any(x => x.Type == Dtos.InstructionalEventApprovalType2.RoomAvailability && x.ApprovingEntity == Dtos.InstructionalEventApprovalEntity2.User);
                overrideFacultyAvailability = meeting.Approvals.Any(x => x.Type == Dtos.InstructionalEventApprovalType2.InstructorAvailability && x.ApprovingEntity == Dtos.InstructionalEventApprovalEntity2.User);
                overrideFacultyCapacity = meeting.Approvals.Any(x => x.Type == Dtos.InstructionalEventApprovalType2.InstructorCapacity && x.ApprovingEntity == Dtos.InstructionalEventApprovalEntity2.User);
            }

            // The room must exist, if specified
            string roomId = null;
            if (meeting.Locations != null && meeting.Locations.Any())
            {

                // Get the first entry where the room is not null and the GUID exists
                InstructionalRoom2 classroom = null;
                foreach (var location in meeting.Locations)
                {
                    if (location.Locations != null && location.Locations.LocationType == null)
                    {
                        IntegrationApiExceptionAddError("Location type is required in order to assign a classroom.", "Validation.Exception");
                    }

                    if (location.Locations != null && location.Locations.LocationType == InstructionalLocationType.InstructionalRoom)
                    {
                        var instructionalRoom = location.Locations as Dtos.InstructionalRoom2;
                        if ((instructionalRoom != null) && (instructionalRoom.Room != null) && (instructionalRoom.Room.Id != null))
                        {
                            classroom = instructionalRoom;
                            break;
                        }
                    }
                }

                if (classroom != null && classroom.Room != null)
                {
                    var allRooms = await RoomsAsync();
                    if (allRooms == null)
                    {
                        IntegrationApiExceptionAddError("Unable to retrieve any rooms.", "GUID.Not.Found");
                    }
                    else
                    {
                        var room = allRooms.FirstOrDefault(x => x.Guid.Equals(classroom.Room.Id, StringComparison.OrdinalIgnoreCase));
                        if (room == null)
                        {
                            IntegrationApiExceptionAddError("The room (" + classroom.Room.Id + ") was not found for the room assignment.", "GUID.Not.Found");
                        }
                        else
                        {
                            roomId = room.Id;
                            if (section != null && section.Capacity.HasValue && room.Capacity < section.Capacity)
                            {
                                if (overrideRoomCapacity)
                                {
                                    logger.Info("Overriding room capacity for section " + section.Name + " and room " + room.Id);
                                }
                                else
                                {
                                    var message = string.Format("Room {0} has capacity of {1}; section {2} has capacity of {3}", room.Id, room.Capacity, section.Name, section.Capacity);
                                    IntegrationApiExceptionAddError(message, "Validation.Exception");
                                }
                            }
                        }
                    }
                }
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                return section;
            }

            Domain.Student.Entities.SectionMeeting entity = null;
            try
            {
                // Everything looks good - create a SectionMeeting entity
                entity = new Domain.Student.Entities.SectionMeeting(meetingId, section.Id, instructionalMethod,
                    (meeting.Recurrence != null && meeting.Recurrence.TimePeriod != null && meeting.Recurrence.TimePeriod.StartOn != null ? meeting.Recurrence.TimePeriod.StartOn.Value.Date : section.StartDate),
                    (meeting.Recurrence != null && meeting.Recurrence.TimePeriod != null && meeting.Recurrence.TimePeriod.EndOn != null ? meeting.Recurrence.TimePeriod.EndOn.Value.Date : section.EndDate),
                    frequency)
                {
                    Guid = meeting.Id,
                    StartTime = (meeting.Recurrence != null && meeting.Recurrence.TimePeriod != null ? meeting.Recurrence.TimePeriod.StartOn.Value.ToLocalTime() : new DateTimeOffset?()),
                    EndTime = (meeting.Recurrence != null && meeting.Recurrence.TimePeriod != null ? meeting.Recurrence.TimePeriod.EndOn.Value.ToLocalTime() : new DateTimeOffset?()),
                    Days = meeting.Recurrence == null || daysOfWeek == null ? new List<DayOfWeek>() : ConvertHedmDaysToDays(daysOfWeek),
                    Room = roomId,
                    Load = (meeting.Workload != null ? meeting.Workload : null),
                    TotalMeetingMinutes = (meeting.Recurrence != null && daysOfWeek != null && daysOfWeek.Any() && meeting.Recurrence.RepeatRule != null && meeting.Recurrence.TimePeriod.StartOn != null
                        ? CalculateMeetingMinutes4(meeting.Recurrence) : new int()),
                    OverrideRoomCapacity = overrideRoomCapacity,
                    OverrideRoomAvailability = overrideRoomAvailability,
                    OverrideFacultyAvailability = overrideFacultyAvailability,
                    OverrideFacultyCapacity = overrideFacultyCapacity
                };

                // Get the contact info for the meeting's instruction method
                // var contact = section.InstructionalContacts.FirstOrDefault(x => x.InstructionalMethodCode == entity.InstructionalMethodCode);
                // decimal loadAmount = contact == null ? 0 : contact.Load.GetValueOrDefault();
                decimal loadAmount = (meeting.Workload != null ? meeting.Workload.GetValueOrDefault() : new decimal());
                // Make sure the meeting we're working on is included
                if (string.IsNullOrEmpty(entity.Id) || !section.Meetings.Any(m => m.Id == entity.Id))
                {
                    section.AddSectionMeeting(entity);
                }
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Validation.Exception");
            }

            if (entity != null)
            {
                int entityPos = 0;
                try
                {
                    entityPos = section.Meetings.IndexOf(section.Meetings.Where(m => m.Guid == entity.Guid).FirstOrDefault());

                    var existingMeeting = section.Meetings.ElementAt(entityPos);

                    bool updateMeeting = false;

                    var dayListExisting = existingMeeting.Days; dayListExisting.Sort();
                    var dayListNew = entity.Days; dayListNew.Sort();
                    if (dayListExisting.ToString() != dayListNew.ToString()) { updateMeeting = true; }
                    if ((existingMeeting.Days.Count != entity.Days.Count) ||
                        (existingMeeting.Room != entity.Room) ||
                        (existingMeeting.StartDate != entity.StartDate) ||
                        (existingMeeting.StartTime != entity.StartTime) ||
                        (existingMeeting.EndDate != entity.EndDate) ||
                        (existingMeeting.EndTime != entity.EndTime) ||
                        (existingMeeting.Frequency != entity.Frequency) ||
                        (existingMeeting.TotalMeetingMinutes != entity.TotalMeetingMinutes) ||
                        (existingMeeting.OverrideRoomCapacity != entity.OverrideRoomCapacity) ||
                        (existingMeeting.OverrideRoomAvailability != entity.OverrideRoomAvailability) ||
                        (existingMeeting.OverrideFacultyAvailability != entity.OverrideFacultyAvailability) ||
                        (existingMeeting.OverrideFacultyCapacity != entity.OverrideFacultyCapacity) ||
                        (existingMeeting.EndTime != entity.EndTime))
                    {
                        updateMeeting = true;
                    }

                    if (updateMeeting)
                    {
                        section.RemoveSectionMeeting(existingMeeting);
                        section.AddSectionMeeting(entity);
                    }
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError("The section meeting GUID is not valid for this meeting instance." + ex.Message, "Validation.Exception");
                }
            }

            return section;
        }

        /// <summary>
        /// Calculates minutes V11
        /// </summary>
        /// <param name="meeting"></param>
        /// <returns></returns>
        private int CalculateMeetingMinutes4(Dtos.Recurrence3 meetingRecurrence)
        {
            if ((meetingRecurrence == null) || (meetingRecurrence.TimePeriod == null || meetingRecurrence.TimePeriod.StartOn == null || meetingRecurrence.TimePeriod.EndOn == null))
            {
                return 0;
            }

            // Calculate all the meeting dates of the section
            var campusCalendarId = defaultConfiguration != null ? defaultConfiguration.CampusCalendarId : null;
            if (string.IsNullOrEmpty(campusCalendarId))
            {
                IntegrationApiExceptionAddError("Unable to retrieve campus calendar.", "Validation.Exception");
                return 0;
            }

            var campusCalendar = _eventRepository.GetCalendar(campusCalendarId);

            if (campusCalendar == null)
            {
                IntegrationApiExceptionAddError("Unable to retrieve campus calendar.", "Validation.Exception");
                return 0;
            }

            var frequencyType = ConvertFrequencyType2EnumDtoToFrequencyTypeDomainEnum(meetingRecurrence.RepeatRule.Type);

            var meetingDates = Ellucian.Colleague.Coordination.Base.RecurrenceUtility.GetRecurrenceDates(meetingRecurrence, frequencyType, campusCalendar);

            if (meetingDates == null)
            {
                return 0;
            }

            var meetingTime = meetingRecurrence.TimePeriod.EndOn.Value.ToLocalTime().TimeOfDay - meetingRecurrence.TimePeriod.StartOn.Value.ToLocalTime().TimeOfDay;

            return ((meetingTime.Hours * 60) + meetingTime.Minutes) * meetingDates.Count();
        }

        #endregion

        private List<DayOfWeek> ConvertHedmDaysToDays(List<HedmDayOfWeek?> daysOfWeek)
        {
            var newDaysOfWeek = new List<DayOfWeek>();
            foreach (var day in daysOfWeek)
            {
                newDaysOfWeek.Add((DayOfWeek)day);
            }
            return newDaysOfWeek;
        }

        /// <summary>
        /// Converts a FrequencyType DTO enumeration value to its corresponding FrequencyType Domain enumeration value
        /// </summary>
        /// <param name="type">FrequencyType DTO enumeration value</param>
        /// <returns>FrequencyType Domain enumeration value</returns>
        private Ellucian.Colleague.Domain.Base.Entities.FrequencyType ConvertFrequencyTypeEnumDtoToFrequencyTypeDomainEnum(Dtos.FrequencyType type)
        {
            switch (type)
            {
                case Dtos.FrequencyType.Weekly:
                    return Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Weekly;
                case Dtos.FrequencyType.Monthly:
                    return Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Monthly;
                case Dtos.FrequencyType.Yearly:
                    return Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Yearly;
                default:
                    return Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Daily;
            }
        }

        /// <summary>
        /// Converts a FrequencyType2 DTO enumeration value to its corresponding FrequencyType Domain enumeration value
        /// </summary>
        /// <param name="type">FrequencyType2 DTO enumeration value</param>
        /// <returns>FrequencyType Domain enumeration value</returns>
        private Ellucian.Colleague.Domain.Base.Entities.FrequencyType ConvertFrequencyType2EnumDtoToFrequencyTypeDomainEnum(Dtos.FrequencyType2? type)
        {
            switch (type)
            {
                case Dtos.FrequencyType2.Weekly:
                    return Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Weekly;
                case Dtos.FrequencyType2.Monthly:
                    return Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Monthly;
                case Dtos.FrequencyType2.Yearly:
                    return Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Yearly;
                default:
                    return Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Daily;
            }
        }

        /// <summary>
        /// Verification method to ensure that all integration parameters required for course processing are defined
        /// </summary>
        /// <param name="config">Curriculum Configuration</param>
        private void VerifyCurriculumConfiguration(CurriculumConfiguration config)
        {
            if (config == null)
            {
                throw new ConfigurationException("Curriculum Configuration setup is not complete.");
            }
            if (string.IsNullOrEmpty(config.SectionActiveStatusCode))
            {
                throw new ConfigurationException("A default course section active status code must be specified.");
            }
            if (string.IsNullOrEmpty(config.SectionInactiveStatusCode))
            {
                throw new ConfigurationException("A default course section inactive status code must be specified.");
            }
        }

        /// <summary>
        /// Verification method to ensure that all integration parameters required for course processing are defined
        /// </summary>
        /// <param name="config">Curriculum Configuration</param>
        private void VerifyCurriculumConfiguration2(CurriculumConfiguration config)
        {
            if (config == null)
            {
                throw new ConfigurationException("Curriculum Configuration setup is not complete.");
            }
        }

        /// <summary>
        /// Determine if a AcademicPeriods reporting term is the same as its code
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        private bool IsReportingTermEqualCode(Ellucian.Colleague.Domain.Student.Entities.AcademicPeriod academicPeriod)
        {
            if (academicPeriod == null)
            {
                return false;
            }
            return (academicPeriod.ReportingTerm == academicPeriod.Code);
        }

        /// <summary>
        /// Build full name
        /// </summary>
        /// <param name="preferredName"></param>
        /// <param name="prefix"></param>
        /// <param name="first"></param>
        /// <param name="middle"></param>
        /// <param name="last"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        private string BuildFullName(string first, string middle, string last)
        {
            string fullName = "";
            if ((first != null) && (first.Length == 1)) first = string.Concat(first, ".");
            if ((middle != null) && (middle.Length == 1)) middle = string.Concat(middle, ".");
            var firstInitial = !string.IsNullOrEmpty(first) ? string.Concat(first.Remove(1), ". ") : string.Empty;
            var middleInitial = !string.IsNullOrEmpty(middle) ? string.Concat(middle.Remove(1), ". ") : string.Empty;
            first = !string.IsNullOrEmpty(first) ? string.Concat(first, " ") : string.Empty;
            middle = !string.IsNullOrEmpty(middle) ? string.Concat(middle, " ") : string.Empty;

            fullName = string.Concat(first, middle, last);

            return fullName.Trim();
        }

        private string BuildFullName(string preferredName, string prefix, string first, string middle, string last,
            string suffix)
        {
            string fullName = "";
            if (string.IsNullOrEmpty(preferredName)) preferredName = "FM";
            if ((first != null) && (first.Length == 1)) first = string.Concat(first, ".");
            if ((middle != null) && (middle.Length == 1)) middle = string.Concat(middle, ".");
            var firstInitial = !string.IsNullOrEmpty(first) ? string.Concat(first.Remove(1), ". ") : string.Empty;
            var middleInitial = !string.IsNullOrEmpty(middle) ? string.Concat(middle.Remove(1), ". ") : string.Empty;
            if (!string.IsNullOrEmpty(suffix)) suffix = string.Concat(", ", suffix);
            first = !string.IsNullOrEmpty(first) ? string.Concat(first, " ") : string.Empty;
            middle = !string.IsNullOrEmpty(middle) ? string.Concat(middle, " ") : string.Empty;
            prefix = !string.IsNullOrEmpty(prefix) ? string.Concat(prefix, " ") : string.Empty;

            switch (preferredName.ToUpper())
            {
                case ("IM"):
                    fullName = string.Concat(prefix, firstInitial, middle, last, suffix);
                    break;
                case ("II"):
                    fullName = string.Concat(prefix, firstInitial, middleInitial, last, suffix);
                    break;
                case ("FM"):
                    fullName = string.Concat(prefix, first, middle, last, suffix);
                    break;
                case ("FI"):
                    fullName = string.Concat(prefix, first, middleInitial, last, suffix);
                    break;
                default:
                    fullName = preferredName;
                    break;
            }
            return fullName.Trim();
        }

        /// <summary>
        /// Helper method to determine if the user has permission to manage a section's textbook assignments
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CanManageSectionTextbookAssignments(Domain.Student.Entities.Section section)
        {
            if (section != null && section.FacultyIds != null && section.FacultyIds.Contains(CurrentUser.PersonId)) return;
            string error = "Current user is not authorized to update books for the section: " + section.Id;
            logger.Error(error);
            throw new PermissionsException("Current user is not authorized to update books in the current section");
        }

        public async Task<PrivacyWrapper<Dtos.Student.Section>> GetSectionAsync(string sectionId, bool useCache)
        {
            IEnumerable<string> listOfIds = new List<string>() { sectionId };
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sectionEntities = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            var hasPrivacyRestriction = false;

            if (sectionId == null)
            {
                throw new ArgumentNullException("sectionId", "sectionId isn't provided");
            }

            if (useCache)
            {
                sectionEntities = await _sectionRepository.GetCachedSectionsAsync(listOfIds);
            }
            else
            {
                sectionEntities = await _sectionRepository.GetNonCachedSectionsAsync(listOfIds);
            }
            Ellucian.Colleague.Domain.Student.Entities.Section sectionEntity = sectionEntities.Where(s => s.Id == sectionId).FirstOrDefault();
            if (sectionEntity == null)
            {
                throw new KeyNotFoundException(string.Format("Couldn't retrieve section entity for given sectionId {0}", sectionId));
            }
            //only a faculty that belongs to that section can retrieve student Ids.There is no need for other users to see that information
            //student Ids are considered as PII at institution level
            if (sectionEntity.FacultyIds == null || !sectionEntity.FacultyIds.Contains(CurrentUser.PersonId))
            {
                hasPrivacyRestriction = true;
            }

            // Get the right adapter for the type mapping
            var sectionDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Student.Section>();

            // Map the section entity to the section Dto then set up response and cache for the single item.
            Dtos.Student.Section sectionDto = sectionDtoAdapter.MapToType(sectionEntity);
            if (hasPrivacyRestriction)
            {
                sectionDto.ActiveStudentIds = new List<string>();

            }

            return new PrivacyWrapper<Dtos.Student.Section>(sectionDto, hasPrivacyRestriction);
        }

        public async Task<PrivacyWrapper<Dtos.Student.Section2>> GetSection2Async(string sectionId, bool useCache)
        {
            IEnumerable<string> listOfIds = new List<string>() { sectionId };
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sectionEntities = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            var hasPrivacyRestriction = false;

            if (sectionId == null)
            {
                throw new ArgumentNullException("sectionId", "sectionId isn't provided");
            }

            if (useCache)
            {
                sectionEntities = await _sectionRepository.GetCachedSectionsAsync(listOfIds);
            }
            else
            {
                sectionEntities = await _sectionRepository.GetNonCachedSectionsAsync(listOfIds);
            }
            Ellucian.Colleague.Domain.Student.Entities.Section sectionEntity = sectionEntities.Where(s => s.Id == sectionId).FirstOrDefault();
            if (sectionEntity == null)
            {
                throw new KeyNotFoundException(string.Format("Couldn't retrieve section entity for given sectionId {0}", sectionId));
            }
            //only a faculty that belongs to that section can retrieve student Ids.There is no need for other users to see that information
            //student Ids are considered as PII at institution level
            if (sectionEntity.FacultyIds == null || !sectionEntity.FacultyIds.Contains(CurrentUser.PersonId))
            {
                hasPrivacyRestriction = true;
            }

            // Get the right adapter for the type mapping
            var sectionDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Student.Section2>();

            // Map the section entity to the section Dto then set up response and cache for the single item.
            Dtos.Student.Section2 sectionDto = sectionDtoAdapter.MapToType(sectionEntity);
            if (hasPrivacyRestriction)
            {
                sectionDto.ActiveStudentIds = new List<string>();

            }

            return new PrivacyWrapper<Dtos.Student.Section2>(sectionDto, hasPrivacyRestriction);
        }

        /// <summary>
        /// Retrieves a <see cref="Dtos.Student.Section3"/>
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        /// <param name="useCache">Flag indicating whether or not to retrieve the cached data for the course section</param>
        /// <returns>A <see cref="Dtos.Student.Section3"/></returns>
        [Obsolete("Obsolete as of Api version 1.31, use version 4 of this API")]
        public async Task<PrivacyWrapper<Dtos.Student.Section3>> GetSection3Async(string sectionId, bool useCache)
        {
            IEnumerable<string> listOfIds = new List<string>() { sectionId };
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sectionEntities = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            var hasPrivacyRestriction = false;

            if (sectionId == null)
            {
                throw new ArgumentNullException("sectionId", "sectionId isn't provided");
            }

            if (useCache)
            {
                sectionEntities = await _sectionRepository.GetCachedSectionsAsync(listOfIds);
            }
            else
            {
                sectionEntities = await _sectionRepository.GetNonCachedSectionsAsync(listOfIds);
            }
            Ellucian.Colleague.Domain.Student.Entities.Section sectionEntity = sectionEntities.Where(s => s.Id == sectionId).FirstOrDefault();
            if (sectionEntity == null)
            {
                throw new KeyNotFoundException(string.Format("Couldn't retrieve section entity for given sectionId {0}", sectionId));
            }
            //only a faculty that belongs to that section can retrieve student Ids.There is no need for other users to see that information
            //student Ids are considered as PII at institution level
            if (sectionEntity.FacultyIds == null || !sectionEntity.FacultyIds.Contains(CurrentUser.PersonId))
            {
                hasPrivacyRestriction = true;
            }

            // Get the right adapter for the type mapping
            var sectionDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Student.Section3>();

            // Map the section entity to the section Dto then set up response and cache for the single item.
            Dtos.Student.Section3 sectionDto = sectionDtoAdapter.MapToType(sectionEntity);
            if (hasPrivacyRestriction)
            {
                sectionDto.ActiveStudentIds = new List<string>();

            }

            return new PrivacyWrapper<Dtos.Student.Section3>(sectionDto, hasPrivacyRestriction);
        }

        /// <summary>
        /// Retrieves a <see cref="Dtos.Student.Section4"/>
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        /// <param name="useCache">Flag indicating whether or not to retrieve the cached data for the course section</param>
        /// <returns>A <see cref="Dtos.Student.Section4"/></returns>
        public async Task<PrivacyWrapper<Dtos.Student.Section4>> GetSection4Async(string sectionId, bool useCache)
        {
            IEnumerable<string> listOfIds = new List<string>() { sectionId };
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sectionEntities = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            var hasPrivacyRestriction = false;

            if (sectionId == null)
            {
                throw new ArgumentNullException("sectionId", "sectionId isn't provided");
            }

            if (useCache)
            {
                sectionEntities = await _sectionRepository.GetCachedSectionsAsync(listOfIds);
            }
            else
            {
                sectionEntities = await _sectionRepository.GetNonCachedSectionsAsync(listOfIds);
            }
            Ellucian.Colleague.Domain.Student.Entities.Section sectionEntity = sectionEntities.Where(s => s.Id == sectionId).FirstOrDefault();
            if (sectionEntity == null)
            {
                throw new KeyNotFoundException(string.Format("Couldn't retrieve section entity for given sectionId {0}", sectionId));
            }
            //only a faculty that belongs to that section can retrieve student Ids.There is no need for other users to see that information
            //student Ids are considered as PII at institution level
            if (sectionEntity.FacultyIds == null || !sectionEntity.FacultyIds.Contains(CurrentUser.PersonId))
            {
                hasPrivacyRestriction = true;
            }

            // Get the right adapter for the type mapping
            var sectionDtoAdapter = new SectionEntityToStudentSection4DtoAdapter(_adapterRegistry, logger);

            // Map the section entity to the section Dto then set up response and cache for the single item.
            Dtos.Student.Section4 sectionDto = sectionDtoAdapter.MapToType(sectionEntity);
            if (hasPrivacyRestriction)
            {
                sectionDto.ActiveStudentIds = new List<string>();

            }

            return new PrivacyWrapper<Dtos.Student.Section4>(sectionDto, hasPrivacyRestriction);
        }


        public async Task<PrivacyWrapper<List<Dtos.Student.Section>>> GetSectionsAsync(IEnumerable<string> sectionIds, bool useCache)
        {
            List<Dtos.Student.Section> sectionDtos = new List<Dtos.Student.Section>();
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sectionEntities = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            var hasPrivacyRestriction = false;

            if (sectionIds == null || sectionIds.Count() == 0)
            {
                string errorText = "At least one item in list of sectionIds must be provided.";
                throw new ArgumentNullException(errorText);
            }

            if (useCache)
            {
                sectionEntities = await _sectionRepository.GetCachedSectionsAsync(sectionIds);
            }
            else
            {
                sectionEntities = await _sectionRepository.GetNonCachedSectionsAsync(sectionIds);
            }

            var sectionDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Student.Section>();
            foreach (var section in sectionEntities)
            {
                if (section != null)
                {
                    Dtos.Student.Section sectionDto = sectionDtoAdapter.MapToType(section);
                    if (section.FacultyIds == null || !section.FacultyIds.Contains(CurrentUser.PersonId))
                    {
                        hasPrivacyRestriction = true;
                        sectionDto.ActiveStudentIds = new List<string>();
                    }
                    sectionDtos.Add(sectionDto);
                }
            }

            return new PrivacyWrapper<List<Dtos.Student.Section>>(sectionDtos, hasPrivacyRestriction);
        }
        public async Task<PrivacyWrapper<List<Dtos.Student.Section2>>> GetSections2Async(IEnumerable<string> sectionIds, bool useCache)
        {
            List<Dtos.Student.Section2> sectionDtos = new List<Dtos.Student.Section2>();
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sectionEntities = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            var hasPrivacyRestriction = false;

            if (sectionIds == null || sectionIds.Count() == 0)
            {
                string errorText = "At least one item in list of sectionIds must be provided.";
                throw new ArgumentNullException(errorText);
            }

            if (useCache)
            {
                sectionEntities = await _sectionRepository.GetCachedSectionsAsync(sectionIds);
            }
            else
            {
                sectionEntities = await _sectionRepository.GetNonCachedSectionsAsync(sectionIds);
            }

            var sectionDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Student.Section2>();
            foreach (var section in sectionEntities)
            {
                if (section != null)
                {
                    Dtos.Student.Section2 sectionDto = sectionDtoAdapter.MapToType(section);
                    if (section.FacultyIds == null || !section.FacultyIds.Contains(CurrentUser.PersonId))
                    {
                        hasPrivacyRestriction = true;
                        sectionDto.ActiveStudentIds = new List<string>();
                    }
                    sectionDtos.Add(sectionDto);
                }
            }

            return new PrivacyWrapper<List<Dtos.Student.Section2>>(sectionDtos, hasPrivacyRestriction);
        }

        [Obsolete("Obsolete as of Api version 1.31, use version 4 of this API")]
        public async Task<PrivacyWrapper<List<Dtos.Student.Section3>>> GetSections3Async(IEnumerable<string> sectionIds, bool useCache = true, bool bestFit = false)
        {
            List<Dtos.Student.Section3> sectionDtos = new List<Dtos.Student.Section3>();
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sectionEntities = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            var hasPrivacyRestriction = false;

            if (sectionIds == null || sectionIds.Count() == 0)
            {
                string errorText = "At least one item in list of sectionIds must be provided.";
                throw new ArgumentNullException(errorText);
            }

            if (useCache)
            {
                sectionEntities = await _sectionRepository.GetCachedSectionsAsync(sectionIds, bestFit);
            }
            else
            {
                sectionEntities = await _sectionRepository.GetNonCachedSectionsAsync(sectionIds, bestFit);
            }

            var sectionDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Student.Section3>();
            foreach (var section in sectionEntities)
            {
                if (section != null)
                {
                    Dtos.Student.Section3 sectionDto = sectionDtoAdapter.MapToType(section);
                    if (section.FacultyIds == null || !section.FacultyIds.Contains(CurrentUser.PersonId))
                    {
                        hasPrivacyRestriction = true;
                        sectionDto.ActiveStudentIds = new List<string>();
                    }
                    sectionDtos.Add(sectionDto);
                }
            }

            return new PrivacyWrapper<List<Dtos.Student.Section3>>(sectionDtos, hasPrivacyRestriction);
        }

        /// <summary>
        /// Retrieve a list of privacy-protected course sections
        /// </summary>
        /// <param name="sectionIds">IDs of course sections to be retrieved</param>
        /// <param name="useCache">Flag indicating whether or not to retrieve cached course section information</param>
        /// <param name="bestFit">Flag indicating whether or not to sort non-term course sections into the academic term that best fits their start and end dates</param>
        /// <returns>List of privacy-protected course sections</returns>
        public async Task<PrivacyWrapper<List<Dtos.Student.Section4>>> GetSections4Async(IEnumerable<string> sectionIds, bool useCache = true, bool bestFit = false)
        {
            List<Dtos.Student.Section4> sectionDtos = new List<Dtos.Student.Section4>();
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sectionEntities = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            var hasPrivacyRestriction = false;

            if (sectionIds == null || sectionIds.Count() == 0)
            {
                string errorText = "At least one item in list of sectionIds must be provided.";
                throw new ArgumentNullException(errorText);
            }

            if (useCache)
            {
                sectionEntities = await _sectionRepository.GetCachedSectionsAsync(sectionIds, bestFit);
            }
            else
            {
                sectionEntities = await _sectionRepository.GetNonCachedSectionsAsync(sectionIds, bestFit);
            }

            var sectionDtoAdapter = new SectionEntityToStudentSection4DtoAdapter(_adapterRegistry, logger);
            foreach (var section in sectionEntities)
            {
                if (section != null)
                {
                    Dtos.Student.Section4 sectionDto = sectionDtoAdapter.MapToType(section);
                    if (section.FacultyIds == null || !section.FacultyIds.Contains(CurrentUser.PersonId))
                    {
                        hasPrivacyRestriction = true;
                        sectionDto.ActiveStudentIds = new List<string>();
                    }
                    sectionDtos.Add(sectionDto);
                }
            }

            return new PrivacyWrapper<List<Dtos.Student.Section4>>(sectionDtos, hasPrivacyRestriction);
        }

        /// <summary>
        /// Get all midterm grading complete data for a section.
        /// </summary>
        /// <param name="sectionId">The section ID of the section for which to retrieve information</param>
        /// <returns></returns>
        public async Task<Dtos.Student.SectionMidtermGradingComplete> GetSectionMidtermGradingCompleteAsync(string sectionId)
        {
            if (sectionId == null)
            {
                throw new ArgumentNullException("sectionId", "sectionId isn't provided");
            }

            // Get the cached section record to check permissions for this service
            List<string> sectionIds = new List<string>() { sectionId };
            Ellucian.Colleague.Domain.Student.Entities.Section sectionEntity;
            sectionEntity = await _sectionRepository.GetSectionAsync(sectionId);
            if (sectionEntity == null)
            {
                throw new KeyNotFoundException(string.Format("Couldn't retrieve section information for given sectionId {0}", sectionId));
            }

            // The current user must be a faculty member of the section
            if (sectionEntity.FacultyIds == null || !sectionEntity.FacultyIds.Contains(CurrentUser.PersonId))
            {
                throw new PermissionsException();
            }


            var sectionGradingCompleteEntity = await _sectionRepository.GetSectionMidtermGradingCompleteAsync(sectionId);

            var sectionGradingCompleteDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionMidtermGradingComplete, Dtos.Student.SectionMidtermGradingComplete>();

            // Map the section entity to the section Dto then set up response and cache for the single item.
            Dtos.Student.SectionMidtermGradingComplete sectionGradingCompleteDto = sectionGradingCompleteDtoAdapter.MapToType(sectionGradingCompleteEntity);

            return sectionGradingCompleteDto;
        }

        /// <summary>
        /// Post a new  midterm grading complete indication for a given section and midterm grade number.
        /// </summary>
        /// <param name="sectionId">Section Id</param>
        /// <param name="sectionGradingComplete">Dto containing information about the grading complete indication to be posted</param>
        /// <returns></returns>
        public async Task<Dtos.Student.SectionMidtermGradingComplete> PostSectionMidtermGradingCompleteAsync(string sectionId, Dtos.Student.SectionMidtermGradingCompleteForPost sectionGradingComplete)
        {
            if (sectionId == null)
            {
                throw new ArgumentNullException("SectionId", "SectionId is not provided");
            }

            if (sectionGradingComplete == null)
            {
                throw new ArgumentNullException("sectionGradingComplete", "Section Midterm Grading information is not provided");
            }

            if (sectionGradingComplete.MidtermGradeNumber == null)
            {
                throw new ArgumentNullException("MidtermGradeNumber", "MidtermGradeNumber is not provided");
            }

            if ((sectionGradingComplete.MidtermGradeNumber < 1) || (sectionGradingComplete.MidtermGradeNumber > 6))
            {
                throw new ArgumentException("MidtermGradeNumber", "MidtermGradeNumber must be between 1 and 6");
            }

            if (string.IsNullOrEmpty(sectionGradingComplete.CompleteOperator))
            {
                throw new ArgumentNullException("CompleteOperator", "CompleteOperator is not provided");
            }

            if (sectionGradingComplete.DateAndTime == null)
            {
                throw new ArgumentNullException("DateAndTime", "DateAndTime is not provided");
            }

            // Can the user update grades for this section?
            if (!await UserCanUpdateGradesOfSectionAsync(sectionId))
            {
                throw new PermissionsException();
            }

            var sectionGradingCompleteEntity = await _sectionRepository.PostSectionMidtermGradingCompleteAsync(sectionId,
                    sectionGradingComplete.MidtermGradeNumber, sectionGradingComplete.CompleteOperator, sectionGradingComplete.DateAndTime);

            var sectionGradingCompleteDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionMidtermGradingComplete, Dtos.Student.SectionMidtermGradingComplete>();

            // Map the section entity to the section Dto then set up response and cache for the single item.
            Dtos.Student.SectionMidtermGradingComplete sectionGradingCompleteDto = sectionGradingCompleteDtoAdapter.MapToType(sectionGradingCompleteEntity);

            return sectionGradingCompleteDto;
        }

        /// <summary>
        /// Retrieves the grading status for a course section
        /// </summary>
        /// <param name="sectionId">Unique identifier for the course section</param>
        /// <returns>Grading status for the specified course section</returns>
        /// <exception cref="ArgumentNullException">A course section ID is required when retrieving course section grading status.</exception>
        /// <exception cref="PermissionsException">Authenticated user is not an assigned faculty member for the specified course section.</exception>
        public async Task<Dtos.Student.SectionGradingStatus> GetSectionGradingStatusAsync(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "A course section ID is required when retrieving course section grading status information.");
            }

            // Retrieve non-cached course section information for further validations
            var courseSection = await RetrieveCourseSectionInformation(sectionId);

            // The authenticated user is only permitted to retrieve section grading status information for a course section if they are an assigned faculty member for the course section
            VerifyAuthenticatedUserIsAssignedFacultyForSectionAsync(courseSection);

            // Retrieve section grading status information
            var sectionGradingStatusEntity = await _sectionRepository.GetSectionGradingStatusAsync(sectionId);

            // Convert entity to DTO
            var sectionGradingStatusEntityToDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionGradingStatus, Dtos.Student.SectionGradingStatus>();
            var sectionGradingStatusDto = sectionGradingStatusEntityToDtoAdapter.MapToType(sectionGradingStatusEntity);

            // Return the DTO
            return sectionGradingStatusDto;
        }

        /// <summary>
        /// Retrieve sections events ICal from calendar schedules
        /// </summary>
        /// <param name="sectionIds"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public async Task<Dtos.Base.EventsICal> GetSectionEventsICalAsync(IEnumerable<string> sectionIds, DateTime? startDate, DateTime? endDate)
        {
            if (sectionIds == null || sectionIds.Count() == 0)
            {
                string errorText = "At least one item in list of sectionIds must be provided.";
                throw new ArgumentNullException(errorText);
            }
            var events = await _sectionRepository.GetSectionEventsICalAsync("CS", sectionIds, startDate, endDate);
            Domain.Base.Entities.EventsICal eventsICalEntity = EventListToEventsICal(events);
            Dtos.Base.EventsICal eventsICalDto = new Dtos.Base.EventsICal(eventsICalEntity.iCal);
            return eventsICalDto;
        }

        public async Task<Dtos.Student.SectionCensusCertification> CreateSectionCensusCertificationAsync(string sectionId, Dtos.Student.SectionCensusToCertify sectionCensusToCertify, Dtos.Student.SectionRegistrationDate sectionRegistrationDate)
        {
            Domain.Student.Entities.Section sectionEntity = null;
            Domain.Student.Entities.SectionCensusConfiguration sectionCensusConfiguration = null;

            Dtos.Student.SectionCensusCertification certifiedCensusDto = null;
            if (sectionId == null)
            {
                throw new ArgumentNullException("sectionId", "Section Id must be provided in service layer to update its census certification details");
            }
            if (sectionCensusToCertify == null)
            {
                throw new ArgumentNullException("sectionCensusToCertify", "Section Census Certification details must be provided in service layer");
            }
            if (sectionCensusToCertify.CensusCertificationDate == null)
            {
                throw new ArgumentNullException("CensusCertificationDate", "Section Census date must be provided");
            }
            if (sectionCensusToCertify.CensusCertificationRecordedDate == null)
            {
                throw new ArgumentNullException("CensusCertificationRecordedDate", "Section Census recorded date must be provided");
            }
            if (sectionCensusToCertify.CensusCertificationRecordedTime == null)
            {
                throw new ArgumentNullException("CensusCertificationRecordedTime", "Section Census recorded time must be provided");
            }

            if (string.IsNullOrWhiteSpace(sectionCensusToCertify.CensusCertificationPosition))
            {
                throw new ArgumentNullException("CensusCertificationPosition", "Section Census position must be provided");
            }

            int censusPosition;
            sectionCensusToCertify.CensusCertificationPosition = sectionCensusToCertify.CensusCertificationPosition.Trim();
            if (!int.TryParse(sectionCensusToCertify.CensusCertificationPosition, out censusPosition))
            {
                throw new ArgumentException("CensusCertificationPosition", "CensusCertificationPosition must be a number");
            }

            //validate only a faculty of the section can certify census
            try
            {
                //this will be retrieving non-cache course section 
                sectionEntity = await _sectionRepository.GetSectionAsync(sectionId);
            }
            catch (Exception ex)
            {
                string message = string.Format("An exception occurred while retrieving section information for sectionId {0}, in order to create section's census certification", sectionId);
                logger.Error(ex, message);
                throw;
            }

            if (sectionEntity == null)
            {
                throw new KeyNotFoundException(string.Format("Couldn't retrieve section information for given sectionId {0}", sectionId));
            }

            // The current user must be a faculty member of the section
            if (sectionEntity.FacultyIds == null || !sectionEntity.FacultyIds.Contains(CurrentUser.PersonId))
            {
                throw new PermissionsException(string.Format("User must be the faculty of the section {0} in order to certify census", sectionId));
            }

            //retrieve section census configuration
            try
            {
                sectionCensusConfiguration = await _studentConfigRepository.GetSectionCensusConfigurationAsync();
            }
            catch (Exception ex)
            {
                string message = string.Format("An exception occurred while retrieving section census configuration, in order to create section {0} census certification", sectionId);
                logger.Error(ex, message);
                throw;
            }

            if (sectionCensusConfiguration == null)
            {
                throw new ConfigurationException("Could not retrieve section census configuration");
            }

            //validate that census recorded date is less than or equal to today (not in the future)
            if (sectionCensusToCertify.CensusCertificationRecordedDate.Value > DateTime.Today)
            {
                var errorMessage = string.Format("The Section Census recorded date of {0} cannot be in the future.",
                     sectionCensusToCertify.CensusCertificationRecordedDate.Value.ToShortDateString());
                logger.Error(errorMessage);
                throw new ArgumentOutOfRangeException("CensusCertificationRecordedDate", errorMessage);
            }

            //default start date for the census certification to the census date
            DateTime censusCertificationStartDate = sectionCensusToCertify.CensusCertificationDate.Value;

            //use the position to locate the current census certification to check for a number of days prior to allow a certification to take place
            Domain.Student.Entities.CensusDatePositionSubmission censusDatePositionSubmission =
                sectionCensusConfiguration.CensusDatePositionSubmissionRange.Where(c => c != null && c.Position == censusPosition).FirstOrDefault();

            //override census date if there is a number of days prior for the census position
            if (censusDatePositionSubmission != null && censusDatePositionSubmission.CertifyDaysBeforeOffset.HasValue)
            {
                censusCertificationStartDate = censusCertificationStartDate.AddDays(censusDatePositionSubmission.CertifyDaysBeforeOffset.Value * -1);
            }

            //validate that the census certification recorded date cannot be less than (before) the certification start date
            if (sectionCensusToCertify.CensusCertificationRecordedDate < censusCertificationStartDate)
            {
                var errorMessage = string.Format("The Section Census recorded date of {0} cannot occur prior to the Section Census start date of {1}",
                    sectionCensusToCertify.CensusCertificationRecordedDate.Value.ToShortDateString(), censusCertificationStartDate);
                logger.Error(errorMessage);
                throw new ArgumentOutOfRangeException("CensusCertificationRecordedDate", errorMessage);
            }

            try
            {
                Domain.Student.Entities.SectionCensusCertification certifiedCensus = sectionEntity.SectionCertifiedCensuses.Where(
                    s => s.CensusCertificationPosition == sectionCensusToCertify.CensusCertificationPosition &&
                    s.CensusCertificationDate == sectionCensusToCertify.CensusCertificationDate).FirstOrDefault();

                //if not null means that census was certified
                if (certifiedCensus != null && certifiedCensus.CensusCertificationRecordedDate != null)
                {
                    throw new ExistingResourceException(string.Format("Section {0} is already certified for Census Date {1} and Census Position {2}", sectionId, sectionCensusToCertify.CensusCertificationDate.Value.ToShortDateString(), sectionCensusToCertify.CensusCertificationPosition), sectionId);
                }

                //validate that census date passed is valid census date to certify
                if (sectionRegistrationDate != null && sectionRegistrationDate.CensusDates != null && sectionRegistrationDate.CensusDates.Any())
                {
                    bool censusDateExist = sectionRegistrationDate.CensusDates.Contains(sectionCensusToCertify.CensusCertificationDate);
                    if (censusDateExist == false)
                    {
                        throw new KeyNotFoundException(string.Format("Census Date {0} is not a valid census date for the section {1}", sectionCensusToCertify.CensusCertificationDate.Value.ToShortDateString(), sectionId));
                    }
                }

                //convert dto to entity and sent to repository
                //call repository- returned certified census will have census certification recorded date and time. 
                Domain.Student.Entities.SectionCensusCertification certifiedCensusEntity = await _sectionRepository.CreateSectionCensusCertificationAsync(sectionId, sectionCensusToCertify.CensusCertificationDate, sectionCensusToCertify.CensusCertificationPosition, sectionCensusToCertify.CensusCertificationLabel, sectionCensusToCertify.CensusCertificationRecordedDate, sectionCensusToCertify.CensusCertificationRecordedTime, CurrentUser.PersonId);

                // convert entity to dto and send back to controller.Dto does not have person Id. Person Id is only stored during inbound, for security reasons we need to marshal it for outbound.
                var entityToDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionCensusCertification, Dtos.Student.SectionCensusCertification>();
                certifiedCensusDto = entityToDtoAdapter.MapToType(certifiedCensusEntity);
                return certifiedCensusDto;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("An exception occurred while updating section {0} census certification", sectionId));
                throw;
            }
        }
        private Domain.Base.Entities.EventsICal EventListToEventsICal(IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Event> events)
        {
            if (events == null)
            {
                logger.Info("events are null, cannot create ICal");
                return new Domain.Base.Entities.EventsICal(string.Empty);
            }
            var ICalendar = new Calendar();
            foreach (var anEvent in events)
            {
                ICalendar.Events.Add(new Ical.Net.CalendarComponents.CalendarEvent
                {
                    Name = "VEVENT",
                    Summary = anEvent.Description,
                    Categories = new List<string>() { anEvent.Type },
                    Location = anEvent.Location,
                    DtStart = new CalDateTime(anEvent.StartTime.UtcDateTime, DateTimeKind.Utc.ToString()) { HasTime = true },
                    DtEnd = new CalDateTime(anEvent.EndTime.UtcDateTime, DateTimeKind.Utc.ToString()) { HasTime = true },
                    //Retain the time component of DTStart and DTEnd. Fix bug in iCal which considers midnight UTC as all-day event and drops the time portion
                });
            }
            string result = null;
            var ctx = new SerializationContext();
            var factory = new SerializerFactory();
            var serializer = factory.Build(ICalendar.GetType(), ctx) as IStringSerializer;
            if (serializer != null)
            {
                result = serializer.SerializeToString(ICalendar);
            }
            return new Domain.Base.Entities.EventsICal(result);
        }

        #region Section Seat Counts
        public async Task<IEnumerable<Dtos.Student.SectionSeats>> GetSectionsSeatsAsync(IEnumerable<string> sectionIds)
        {
            var sectionSeatsDtos = new List<Dtos.Student.SectionSeats>();
            if (sectionIds == null || !sectionIds.Any())
            {
                throw new ArgumentNullException("sectionIds", "At least one course section ID is required when retrieving seat counts for sections.");
            }

            try
            {
                var sectionSeatsDictionary = (await _sectionRepository.GetSectionsSeatsAsync(sectionIds));
                if (sectionSeatsDictionary != null && sectionSeatsDictionary.Any())
                {
                    var entityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.SectionSeats, Dtos.Student.SectionSeats>();
                    foreach (var sectionSeatCount in sectionSeatsDictionary)
                    {
                        sectionSeatsDtos.Add(entityToDtoAdapter.MapToType(sectionSeatCount.Value));
                    }
                }
                return sectionSeatsDtos;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("An error occurred while trying to retrieve seat counts for course section IDs {0}.", string.Join(",", sectionIds)));
                throw ex;
            }
        }
        #endregion

        /// <summary>
        /// Verifies that the authenticated user is an assigned faculty member for the specified course section
        /// </summary>
        /// <param name="courseSection">Course section to check</param>
        /// <exception cref="PermissionsException">Authenticated user is not an assigned faculty member for course section.</exception>
        private void VerifyAuthenticatedUserIsAssignedFacultyForSectionAsync(Domain.Student.Entities.Section courseSection)
        {
            // The current user must be a faculty member of the section
            if (courseSection.FacultyIds == null || !courseSection.FacultyIds.Contains(CurrentUser.PersonId))
            {
                throw new PermissionsException(string.Format("Authenticated user is not an assigned faculty member for course section {0}.", courseSection.Id));
            }
        }

        /// <summary>
        /// Retrieves course section information for the specified course section ID
        /// </summary>
        /// <param name="sectionId">ID of the course section whose data will be retrieved</param>
        /// <returns>Course section information for the specified course section ID</returns>
        /// <exception cref="KeyNotFoundException">Course section data could not be retrieved.</exception>
        private async Task<Domain.Student.Entities.Section> RetrieveCourseSectionInformation(string sectionId)
        {
            var courseSections = await _sectionRepository.GetSectionAsync(sectionId);
            if (courseSections == null)
            {
                throw new KeyNotFoundException(string.Format("Could not retrieve course section information for course section {0}.", sectionId));
            }
            return courseSections;
        }
    }
}
