// Copyright 2017-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
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
        private readonly IConfigurationRepository _configurationRepository;

        /// <summary>
        /// Initialize the service for accessing student attendance info
        /// </summary>
        /// <param name="adapterRegistry">Dto adapter registry</param>
        /// <param name="studentAttendanceRepository">Repository for student attendance</param>
        /// <param name="logger">error logging</param>
        public StudentAttendanceService(IAdapterRegistry adapterRegistry, IStudentAttendanceRepository studentAttendanceRepository, IStudentRepository studentRepository, ISectionRepository sectionRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger,
            IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            this._studentAttendanceRepository = studentAttendanceRepository;
            this._studentRepository = studentRepository;
            this._sectionRepository = sectionRepository;
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
                CanManageStudentAttendanceInformation(section);
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
                CanManageStudentAttendanceInformation(sections.ElementAt(0));
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
                CanManageStudentAttendanceInformation(sections.ElementAt(0));
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
                CanManageStudentAttendanceInformation(sections.ElementAt(0));
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
