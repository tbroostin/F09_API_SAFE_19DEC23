// Copyright 2017-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Services;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class StudentAttendanceService : StudentCoordinationService, IStudentAttendanceService
    {
        private readonly IStudentAttendanceRepository _studentAttendanceRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IStudentConfigurationRepository _studentConfigurationRepository;
        private readonly ITermRepository _termRepository;

        /// <summary>
        /// Initialize the service for accessing student attendance info
        /// </summary>
        /// <param name="adapterRegistry">Dto adapter registry</param>
        /// <param name="studentAttendanceRepository">Repository for student attendance</param>
        /// <param name="studentRepository">Repository for student</param>
        /// <param name="sectionRepository">Repository for sections</param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository">Repository for roles</param>
        /// <param name="logger">error logging</param>
        /// <param name="configurationRepository">Repository for configurations</param>
        /// <param name="referenceDataRepository">Repository for reference data</param>
        /// <param name="studentConfigurationRepository">Repository for student configurations</param>
        /// <param name="termRepository">Repository for Terms</param>
        public StudentAttendanceService(IAdapterRegistry adapterRegistry, IStudentAttendanceRepository studentAttendanceRepository, IStudentRepository studentRepository,
            ISectionRepository sectionRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger,
            IConfigurationRepository configurationRepository, IReferenceDataRepository referenceDataRepository, IStudentConfigurationRepository studentConfigurationRepository,
            ITermRepository termRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            this._studentAttendanceRepository = studentAttendanceRepository;
            this._studentRepository = studentRepository;
            this._sectionRepository = sectionRepository;
            this._referenceDataRepository = referenceDataRepository;
            _studentConfigurationRepository = studentConfigurationRepository;
            _termRepository = termRepository;
        }

        /// <summary>
        /// Retrieves student attendance Dtos based on a set of criteria.  SectionId is required.
        /// </summary>
        /// <param name="criteria">Object that contains the section for which attendances are requested, whether to include cross listed sections and an option date</param>
        /// <param name="useCache">Use cached course section data when retrieving academic credit records of specified section(s)</param>
        /// <returns>A list of student attendance Dtos</returns>
        public async Task<IEnumerable<StudentAttendance>> QueryStudentAttendancesAsync(StudentAttendanceQueryCriteria criteria, bool useCache = true)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Query criteria must be included");
            }
            if (string.IsNullOrEmpty(criteria.SectionId))
            {
                throw new ArgumentException("Criteria must include a Section Id");
            }

            // Only allow faculty of the section to view section attendance information.
            Domain.Student.Entities.Section section = null;
            var id = new List<string>() { criteria.SectionId };
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections = null;
            if (useCache)
            {
                sections = await _sectionRepository.GetCachedSectionsAsync(id);
            }
            else
            {
                sections = await _sectionRepository.GetNonCachedSectionsAsync(id);
            }

            if (sections != null && sections.Any())
            {
                section = sections.ElementAt(0);
                var allDepartments = await _referenceDataRepository.DepartmentsAsync();
                var userPermissions = await GetUserPermissionCodesAsync();
                CanManageStudentAttendanceInformation(section, allDepartments, userPermissions);
            }
            else
            {
                throw new KeyNotFoundException("Invalid ID for section: " + id);
            }

            // Get Attendance data
            var querySectionIds = new List<string>() { criteria.SectionId };
            if (criteria.IncludeCrossListedAttendances && section.CrossListedSections != null && section.CrossListedSections.Any())
            {
                var crossListedSectionIds = section.CrossListedSections.Select(x => x.Id);
                querySectionIds.AddRange(crossListedSectionIds);
            }
            try
            {
                var attendanceEntities = await _studentAttendanceRepository.GetStudentAttendancesAsync(querySectionIds, criteria.AttendanceDate);
                List<Dtos.Student.StudentAttendance> attendanceDtos = new List<Dtos.Student.StudentAttendance>();
                var attendanceDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentAttendance, Dtos.Student.StudentAttendance>();
                foreach (var attendanceEntity in attendanceEntities)
                {
                    Dtos.Student.StudentAttendance attendanceDto = attendanceDtoAdapter.MapToType(attendanceEntity);
                    attendanceDtos.Add(attendanceDto);
                }
                return attendanceDtos;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var message = "Exception occurred while trying to get student attendances for section " + criteria.SectionId;
                logger.Error(ex, message);
                throw;
            }
        }

        /// <summary>
        /// Updates a student attendance item.
        /// </summary>
        /// <param name="studentAttendance">The Student attendance to update.</param>
        /// <returns>The updated student attendance item.</returns>
        public async Task<StudentAttendance> UpdateStudentAttendanceAsync(StudentAttendance studentAttendance)
        {
            if (studentAttendance == null)
            {
                throw new ArgumentNullException("studentAttendance", "Student attendance item to update must be included");
            }
            // Validate the Dto
            if (string.IsNullOrEmpty(studentAttendance.StudentId))
            {
                throw new ArgumentException("Student attendance item must have a student Id.");
            }
            if (string.IsNullOrEmpty(studentAttendance.SectionId))
            {
                throw new ArgumentException("Student attendance item must have a section Id.");
            }
            if (studentAttendance.MeetingDate == default(DateTime))
            {
                throw new ArgumentException("Student attendance must have a valid meeting date.");
            }

            // Check permission: Only allow faculty of the section to update section attendance information.
            List<string> id = new List<string>() { studentAttendance.SectionId };
            IEnumerable<Domain.Student.Entities.Section> sections = await _sectionRepository.GetNonCachedSectionsAsync(id);
            Domain.Student.Entities.Section section = null;
            if (sections != null && sections.Any())
            {
                section = sections.ElementAt(0);
                var allDepartments = await _referenceDataRepository.DepartmentsAsync();
                var userPermissions = await GetUserPermissionCodesAsync();
                CanUpdateStudentAttendanceInformation(sections.ElementAt(0), allDepartments, userPermissions);
            }
            else
            {
                throw new KeyNotFoundException("Section Id " + id + "does not exist.");
            }

            // The incoming Dto has been validated and the permission has been checked.
            // Convert the DTO to an entity, call the repository method, and convert it into the DTO.
            Domain.Student.Entities.StudentAttendance studentAttendanceToUpdate = null;
            try
            {
                var studentAttendanceDtoToEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.StudentAttendance, Domain.Student.Entities.StudentAttendance>();
                studentAttendanceToUpdate = studentAttendanceDtoToEntityAdapter.MapToType(studentAttendance);
            }
            catch (Exception ex)
            {
                logger.Error("Error converting incoming StudentAttendance Dto to StudentAttendance Entity: " + ex.Message);
                throw new ArgumentException("Student Attendance item is invalid", ex);
            }

            try
            {
                IEnumerable<Domain.Student.Entities.SectionMeetingInstance> sectionMeetingInstances = null;
                sectionMeetingInstances = await _sectionRepository.GetSectionMeetingInstancesAsync(studentAttendance.SectionId);
                if (sectionMeetingInstances == null || !sectionMeetingInstances.Any())
                {
                    // If section is cross listed pull meeting instances from the primary section.
                    if (!string.IsNullOrEmpty(section.PrimarySectionId))
                    {
                        sectionMeetingInstances = await _sectionRepository.GetSectionMeetingInstancesAsync(section.PrimarySectionId);
                    }

                }
                var updatedStudentAttendance = await _studentAttendanceRepository.UpdateStudentAttendanceAsync(studentAttendanceToUpdate, sectionMeetingInstances);
                var attendanceDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentAttendance, Dtos.Student.StudentAttendance>();
                StudentAttendance updatedAttendanceDto = attendanceDtoAdapter.MapToType(updatedStudentAttendance);
                return updatedAttendanceDto;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var message = "Exception occurred while trying to update student attendance for student " + studentAttendance.StudentId;
                logger.Error(ex, message);
                throw;
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view and manage the student attendances
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CanManageStudentAttendanceInformation(Domain.Student.Entities.Section section)
        {
            if (section != null && section.FacultyIds != null && section.FacultyIds.Contains(CurrentUser.PersonId)) return;
            string error = "Current user is not authorized to view or modify student attendance information for section : " + section.Id;
            logger.Error(error);
            throw new PermissionsException("Current user is not authorized to view or modify student attendance information for the section");
        }


        /// <summary>
        /// Helper method to determine if the user has faculty or department oversight permission to view and manage the student attendances
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CanManageStudentAttendanceInformation(Domain.Student.Entities.Section section, IEnumerable<Domain.Base.Entities.Department> departments, IEnumerable<string> userPermissions)
        {
            if ((section != null && section.FacultyIds != null && section.FacultyIds.Contains(CurrentUser.PersonId)) ||
                (CheckDepartmentalOversightAccessForSection(section, departments) && (userPermissions.Contains(DepartmentalOversightPermissionCodes.ViewSectionAttendance) || userPermissions.Contains(DepartmentalOversightPermissionCodes.CreateSectionAttendance))))
            {
                return;
            }
            string error = "Current user is not authorized to view or modify student attendance information for section : " + section.Id;
            logger.Error(error);
            throw new PermissionsException(error);
        }

        /// <summary>
        /// Helper method to determine if the user has faculty or department oversight permission to view and update the student attendances
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CanUpdateStudentAttendanceInformation(Domain.Student.Entities.Section section, IEnumerable<Domain.Base.Entities.Department> departments, IEnumerable<string> userPermissions)
        {
            if ((section != null && section.FacultyIds != null && section.FacultyIds.Contains(CurrentUser.PersonId)) ||
                (CheckDepartmentalOversightAccessForSection(section, departments) && userPermissions.Contains(DepartmentalOversightPermissionCodes.CreateSectionAttendance)))
            {
                return;
            }
            string error = "Current user is not authorized to modify student attendance information for section : " + section.Id;
            logger.Error(error);
            throw new PermissionsException(error);
        }

        /// <summary>
        /// Updates a student attendance item.
        /// </summary>
        /// <param name="sectionAttendance">The Student attendance to update.</param>
        /// <returns>The updated student attendance item.</returns>
        public async Task<SectionAttendanceResponse> UpdateSectionAttendance2Async(SectionAttendance sectionAttendance)
        {
            if (sectionAttendance == null)
            {
                throw new ArgumentNullException("studentAttendance", "Student attendance item to update must be included");
            }

            if (string.IsNullOrEmpty(sectionAttendance.SectionId))
            {
                throw new ArgumentException("Section attendance item must have a section Id.");
            }

            if (sectionAttendance.MeetingInstance == null || sectionAttendance.MeetingInstance.MeetingDate == default(DateTime))
            {
                throw new ArgumentException("Section attendance must have a valid meeting instance with a meeting date.");
            }

            if (sectionAttendance.StudentAttendances == null || !sectionAttendance.StudentAttendances.Any())
            {
                throw new ArgumentException("Section attendance must have at least one student attendance item to update.");
            }


            // Check permission: Only allow faculty of the section to update section attendance information.
            List<string> id = new List<string>() { sectionAttendance.SectionId };
            IEnumerable<Domain.Student.Entities.Section> sections = await _sectionRepository.GetNonCachedSectionsAsync(id);
            Domain.Student.Entities.Section section = null;
            if (sections != null && sections.Any())
            {
                section = sections.ElementAt(0);
                var allDepartments = await _referenceDataRepository.DepartmentsAsync();
                var userPermissions = await GetUserPermissionCodesAsync();
                CanUpdateStudentAttendanceInformation(section, allDepartments, userPermissions);
            }
            else
            {
                throw new KeyNotFoundException("Section Id " + sectionAttendance.SectionId + " does not exist.");
            }

            // check if attendence entry is closed for the section
            if (!section.ReopenSectionAttendance && !await SectionAttendanceIsOpen(section))
            {
                var message = "Section attendance for section id " + sectionAttendance.SectionId + " has been closed and may not be updated.";
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            // Convert the DTO to an entity
            Domain.Student.Entities.SectionAttendance sectionAttendanceToUpdate = null;
            try
            {
                var sectionAttendanceDtoToEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.SectionAttendance, Domain.Student.Entities.SectionAttendance>();
                sectionAttendanceToUpdate = sectionAttendanceDtoToEntityAdapter.MapToType(sectionAttendance);
            }
            catch (Exception ex)
            {
                logger.Error("Error converting incoming SectionAttendance Dto to SectionAttendance Entity: " + ex.Message);
                throw new ArgumentException("Section Attendance item is invalid", ex);
            }

            try
            {
                if (section.AttendanceTrackingType == Domain.Student.Entities.AttendanceTrackingType.CumulativeHours ||
                    section.AttendanceTrackingType == Domain.Student.Entities.AttendanceTrackingType.PresentAbsent)
                {
                    // Validate that the provided meeting instance is one of the valid meeting instances for this section (or its primary cross-listed section)
                    // Since it is an incoming DTO we can't guaranteee that the meeting instance has the right Id makes it so.
                    await CheckSectionMeetingInstances(sectionAttendance.SectionId, section, sectionAttendanceToUpdate);
                }
                else if (section.AttendanceTrackingType == Domain.Student.Entities.AttendanceTrackingType.HoursByDateWithoutSectionMeeting ||
                         section.AttendanceTrackingType == Domain.Student.Entities.AttendanceTrackingType.PresentAbsentWithoutSectionMeeting)
                {
                    //Validate meeting date is >= start date and <= today and <= end date (if one exists)
                    CheckSectionMeetingDate(sectionAttendance.MeetingInstance.MeetingDate, section);
                }

                var crossListedIds = section.CrossListedSections != null ? section.CrossListedSections.Select(s => s.Id).ToList() : null;
                var updatedStudentAttendance = await _studentAttendanceRepository.UpdateSectionAttendanceAsync(sectionAttendanceToUpdate, crossListedIds);
                var attendanceDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.SectionAttendanceResponse, Dtos.Student.SectionAttendanceResponse>();
                SectionAttendanceResponse updatedAttendanceDto = attendanceDtoAdapter.MapToType(updatedStudentAttendance);
                return updatedAttendanceDto;
            }
            catch (Exception ex)
            {
                var message = "Exception occurred while trying to update student attendance information for section " + sectionAttendance.SectionId;
                logger.Error(ex, message);
                throw;
            }
        }

        private async Task<bool> SectionAttendanceIsOpen(Domain.Student.Entities.Section section)
        {
            var facultyAttendanceConfiguration = await _studentConfigurationRepository.GetFacultyAttendanceConfigurationAsync();

            //default behavior is to allow updates to attendance, when parameters are blank entry is permitted
            if (facultyAttendanceConfiguration == null ||
                (!facultyAttendanceConfiguration.CloseAttendanceCensusTrackNumber.HasValue &&
                !facultyAttendanceConfiguration.CloseAttendanceNumberOfDaysPastCensusTrackDate.HasValue &&
                !facultyAttendanceConfiguration.CloseAttendanceNumberOfDaysPastSectionEndDate.HasValue))
                return true;

            DateTime? attendanceEntryClosedOnDate = null;

            if (facultyAttendanceConfiguration.CloseAttendanceCensusTrackNumber.HasValue)
            {
                var sectionTerm = await _termRepository.GetAsync(section.TermId);
                var sectionCensusDates =
                    SectionProcessor.GetSectionCensusDates(new List<Domain.Student.Entities.Section> { section }, new List<Domain.Student.Entities.Term> { sectionTerm });

                List<DateTime?> censusDates;
                if (sectionCensusDates != null && sectionCensusDates.Count > 0 && sectionCensusDates.TryGetValue(section.Id, out censusDates))
                {
                    if (censusDates != null && censusDates.Count > 0)
                    {
                        int censusDateIndex = facultyAttendanceConfiguration.CloseAttendanceCensusTrackNumber.Value - 1;

                        //to avoid index out of bounds, check that given census position exists in the list of census position dates
                        if (censusDates.Count > censusDateIndex)
                        {
                            attendanceEntryClosedOnDate = censusDates[censusDateIndex];
                            if (attendanceEntryClosedOnDate.HasValue)
                            {
                                //use the census track date to calcuate the date that attendance entry is closed
                                facultyAttendanceConfiguration.CloseAttendanceNumberOfDaysPastCensusTrackDate = facultyAttendanceConfiguration.CloseAttendanceNumberOfDaysPastCensusTrackDate ?? 1;
                                attendanceEntryClosedOnDate = attendanceEntryClosedOnDate.Value.AddDays(facultyAttendanceConfiguration.CloseAttendanceNumberOfDaysPastCensusTrackDate.Value);
                            }
                        }
                    }
                }
            }

            // the attendance closed parameters were not set or the section has no end date
            if (!attendanceEntryClosedOnDate.HasValue && section.EndDate.HasValue &&
                facultyAttendanceConfiguration.CloseAttendanceNumberOfDaysPastSectionEndDate.HasValue)
            {
                //use the section end date to calcuate the date that attendance entry is closed
                attendanceEntryClosedOnDate =
                    section.EndDate.Value.AddDays(facultyAttendanceConfiguration.CloseAttendanceNumberOfDaysPastSectionEndDate.Value);
            }

            // when cacluated closed date has a value, check if attendance entry is permitted
            if (attendanceEntryClosedOnDate.HasValue)
            {
                //when today is less than attendance closed date, entry is allowed
                return DateTime.Today < attendanceEntryClosedOnDate;
            }

            return true;
        }

        // Validate that the provided meeting instance is one of the valid meeting instances for this section (or its primary cross-listed section)
        // Since it is an incoming DTO we can't guaranteee that the meeting instance has the right Id makes it so.
        private async Task CheckSectionMeetingInstances(string sectionId, Domain.Student.Entities.Section section,
            Domain.Student.Entities.SectionAttendance sectionAttendanceToUpdate)
        {
            IEnumerable<Domain.Student.Entities.SectionMeetingInstance> sectionMeetingInstances = null;
            sectionMeetingInstances = await _sectionRepository.GetSectionMeetingInstancesAsync(sectionId);
            if (sectionMeetingInstances == null || !sectionMeetingInstances.Any())
            {
                // If section is cross listed see if there are meeting instances for the primary section.
                if (!string.IsNullOrEmpty(section.PrimarySectionId))
                {
                    sectionMeetingInstances = await _sectionRepository.GetSectionMeetingInstancesAsync(section.PrimarySectionId);
                }
            }

            if (sectionMeetingInstances == null)
            {
                string msg = "Section " + sectionId + " has no calendar schedules. Unable to update attendances.";
                logger.Error(msg);
                throw new ArgumentException(msg);
            }

            //check if meeting instance exists for the section
            else if (!sectionMeetingInstances.ToList().Contains(sectionAttendanceToUpdate.MeetingInstance))
            {
                string msg = "Meeting Instance not valid for section " + sectionId + ". Unable to update attendances.";
                logger.Error(msg);
                throw new ArgumentException(msg);
            }
        }

        //Validate meeting date is >= start date and <= today and <= end date (if one exists)
        private void CheckSectionMeetingDate(DateTime meetingDate, Domain.Student.Entities.Section section)
        {
            if (meetingDate < section.StartDate)
            {
                string msg = "Meeting date cannot be before section start date for Section " + section.Id + ".";
                logger.Error(msg);
                throw new ArgumentException(msg);
            }
            if (meetingDate > DateTime.Today)
            {
                string msg = "Meeting date cannot be in the future for Section " + section.Id + ".";
                logger.Error(msg);
                throw new ArgumentException(msg);
            }
            if (section.EndDate.HasValue && meetingDate > section.EndDate.Value)
            {
                string msg = "Meeting date cannot be after section end date for Section " + section.Id + ".";
                logger.Error(msg);
                throw new ArgumentException(msg);
            }
        }

        /// <summary>
        /// Updates a student attendance item.
        /// </summary>
        /// <param name="sectionAttendance">The Student attendance to update.</param>
        /// <returns>The updated student attendance item.</returns>
        public async Task<SectionAttendanceResponse> UpdateSectionAttendanceAsync(SectionAttendance sectionAttendance)
        {

            if (sectionAttendance == null)
            {
                throw new ArgumentNullException("studentAttendance", "Student attendance item to update must be included");
            }
            if (string.IsNullOrEmpty(sectionAttendance.SectionId))
            {
                throw new ArgumentException("Section attendance item must have a section Id.");
            }
            if (sectionAttendance.MeetingInstance == null || sectionAttendance.MeetingInstance.MeetingDate == default(DateTime))
            {
                throw new ArgumentException("Section attendance must have a valid meeting instance with a meeting date.");
            }
            if (sectionAttendance.StudentAttendances == null || !sectionAttendance.StudentAttendances.Any())
            {
                throw new ArgumentException("Section attendance must have at least one student attendance item to update.");
            }
            // Check permission: Only allow faculty of the section to update section attendance information.
            List<string> id = new List<string>() { sectionAttendance.SectionId };
            IEnumerable<Domain.Student.Entities.Section> sections = await _sectionRepository.GetNonCachedSectionsAsync(id);
            Domain.Student.Entities.Section section = null;
            if (sections != null && sections.Any())
            {
                section = sections.ElementAt(0);
                var allDepartments = await _referenceDataRepository.DepartmentsAsync();
                var userPermissions = await GetUserPermissionCodesAsync();
                CanUpdateStudentAttendanceInformation(sections.ElementAt(0), allDepartments, userPermissions);
            }
            else
            {
                throw new KeyNotFoundException("Section Id " + sectionAttendance.SectionId + " does not exist.");
            }
            // Convert the DTO to an entity
            Domain.Student.Entities.SectionAttendance sectionAttendanceToUpdate = null;
            try
            {
                var sectionAttendanceDtoToEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.SectionAttendance, Domain.Student.Entities.SectionAttendance>();
                sectionAttendanceToUpdate = sectionAttendanceDtoToEntityAdapter.MapToType(sectionAttendance);
            }
            catch (Exception ex)
            {
                logger.Error("Error converting incoming SectionAttendance Dto to SectionAttendance Entity: " + ex.Message);
                throw new ArgumentException("Section Attendance item is invalid", ex);
            }
            try
            {
                // Validate that the provided meeting instance is one of the valid meeting instances for this section (or its primary cross-listed section)
                // Since it is an incoming DTO we can't guaranteee that the meeting instance having the right Id makes it so.
                IEnumerable<Domain.Student.Entities.SectionMeetingInstance> sectionMeetingInstances = null;
                sectionMeetingInstances = await _sectionRepository.GetSectionMeetingInstancesAsync(sectionAttendance.SectionId);
                if (sectionMeetingInstances == null || !sectionMeetingInstances.Any())
                {
                    // If section is cross listed see if there are meeting instances for the primary section.
                    if (!string.IsNullOrEmpty(section.PrimarySectionId))
                    {
                        sectionMeetingInstances = await _sectionRepository.GetSectionMeetingInstancesAsync(section.PrimarySectionId);
                    }

                }

                if (sectionMeetingInstances == null)
                {
                    string msg = "Section " + sectionAttendance.SectionId + " has no calendar schedules. Unable to update attendances.";
                    logger.Error(msg);
                    throw new ArgumentException(msg);
                }
                else if (!sectionMeetingInstances.ToList().Contains(sectionAttendanceToUpdate.MeetingInstance))
                {

                    string msg = "Meeting Instance not valid for section " + sectionAttendance.SectionId + ". Unable to update attendances.";
                    logger.Error(msg);
                    throw new ArgumentException(msg);
                }

                var crossListedIds = section.CrossListedSections != null ? section.CrossListedSections.Select(s => s.Id).ToList() : null;
                var updatedStudentAttendance = await _studentAttendanceRepository.UpdateSectionAttendanceAsync(sectionAttendanceToUpdate, crossListedIds);
                var attendanceDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.SectionAttendanceResponse, Dtos.Student.SectionAttendanceResponse>();
                SectionAttendanceResponse updatedAttendanceDto = attendanceDtoAdapter.MapToType(updatedStudentAttendance);
                return updatedAttendanceDto;
            }
            catch (Exception ex)
            {
                var message = "Exception occurred while trying to update student attendance information for section " + sectionAttendance.SectionId;
                logger.Error(ex, message);
                throw;
            }


        }
    }
}
