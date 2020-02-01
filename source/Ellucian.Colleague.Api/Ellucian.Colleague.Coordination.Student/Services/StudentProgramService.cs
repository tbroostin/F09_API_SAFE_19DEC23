// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
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
    public class StudentProgramService : StudentCoordinationService, IStudentProgramService
    {
        private readonly IStudentProgramRepository _studentProgramRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ITermRepository _termRepository;
        private ILogger _logger;
        private IEnumerable<Term> termList;
        private readonly IConfigurationRepository _configurationRepository;

        public StudentProgramService(IAdapterRegistry adapterRegistry, IStudentRepository studentRepository, IStudentProgramRepository studentProgramRepository, ITermRepository termRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _studentProgramRepository = studentProgramRepository;
            _studentRepository = studentRepository;
            _logger = logger;
            _termRepository = termRepository;
            _configurationRepository = configurationRepository;
        }

        /// <summary>
        /// Retrieves <see cref="Dtos.Student.StudentProgram2">student programs</see> for the specified student IDs
        /// </summary>
        /// <param name="studentIds">List of student IDs</param>
        /// <param name="includeInactivePrograms">Flag indicating whether or not to include inactive programs</param>
        /// <param name="term">Optional code filtering student programs for a specific academic term</param>
        /// <param name="includeHistory">Flag indicating whether or not to include historical data</param>
        /// <returns>List of <see cref="Dtos.Student.StudentProgram2">student programs</see></returns>
        public async Task<IEnumerable<Dtos.Student.StudentProgram2>> GetStudentProgramsByIdsAsync(IEnumerable<string> studentIds, bool includeInactivePrograms = false, string term = null, bool includeHistory = false)
        {
            if (studentIds == null || !studentIds.Any())
            {
                throw new ArgumentNullException("At least one student ID must be specified when retrieving student program data.");
            }

            ICollection<Dtos.Student.StudentProgram2> studentProgramDto = new List<Dtos.Student.StudentProgram2>();

            Term termDomain = null;
            if (!string.IsNullOrWhiteSpace(term))
                termDomain = await _termRepository.GetAsync(term);
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentProgram> studentPrograms = await _studentProgramRepository.GetStudentProgramsByIdsAsync(studentIds, includeInactivePrograms, termDomain, includeHistory);

            foreach (var studentProgram in studentPrograms)
            {
                try
                {
                    if (!HasPermission(StudentPermissionCodes.ViewStudentInformation) && !(await UserIsAdvisorAsync(studentProgram.StudentId)))
                    {
                        throw new PermissionsException(string.Format("User does not have permissions to access student program for student {0}.", studentProgram.StudentId));
                    }

                    // If we are asking for a specific term, and this doesn't match, skip it.
                    if (!string.IsNullOrEmpty(term) && !includeHistory) // Except if we're returning historical data.
                    {
                        var startDate = studentProgram.StartDate.HasValue ? studentProgram.StartDate : null;
                        var endDate = studentProgram.EndDate.HasValue ? studentProgram.EndDate : null;
                        string checkTerm = FindTermForProgram(startDate, endDate, term);
                        if (checkTerm != term)
                        {
                            continue;
                        }
                    }

                    // Get the right adapter for the type mapping
                    var studentProgramDtoAdapter = _adapterRegistry.GetAdapter<StudentProgram, Ellucian.Colleague.Dtos.Student.StudentProgram2>();

                    // Map the student programs entity to the student programs DTO
                    var programDto = studentProgramDtoAdapter.MapToType(studentProgram);

                    studentProgramDto.Add(programDto);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Unable to retrieve student academic program data.");
                }
            }
            return studentProgramDto;
        }

        /// <summary>
        /// Add new academic program for a student
        /// </summary>
        /// <param name="studentAcademicProgram">Student Academic program information</param>
        /// <returns><see cref="Dtos.Student.StudentProgram2">Newly added student program</returns>
        public async Task<Dtos.Student.StudentProgram2> AddStudentProgram(Dtos.Student.StudentAcademicProgram studentAcademicProgram)
        {
            if (studentAcademicProgram == null)
            {
                throw new ArgumentNullException("studentAcademicProgram", "Academic Prgoram information must specify to add to student.");
            }
            if (string.IsNullOrEmpty(studentAcademicProgram.StudentId))
            {
                throw new ArgumentException("add program must have a student Id to add a program to student.");
            }
            //Check Permissions to be sure user is advisor.
            if (!(await UserIsAdvisorAsync(studentAcademicProgram.StudentId)))
            {
                throw new PermissionsException(string.Format("User does not have permissions to add program for student {0}.", studentAcademicProgram.StudentId));
            }
            var addProgramDtoAdapter = _adapterRegistry.GetAdapter<Dtos.Student.StudentAcademicProgram, StudentAcademicProgram>();
            var addProgramDto = addProgramDtoAdapter.MapToType(studentAcademicProgram);

            var programDto = new Dtos.Student.StudentProgram2();

            List<string> activePrograms = new List<string>();
            List<string> endDates = new List<string>();

            if (studentAcademicProgram.ActivePrograms != null && studentAcademicProgram.ActivePrograms.Any())
            {
                foreach (var prog in studentAcademicProgram.ActivePrograms)
                {
                    activePrograms.Add(prog.ProgramCode);
                    endDates.Add(!string.IsNullOrEmpty(prog.EndDate) ? DateTime.Parse(prog.EndDate).ToShortDateString() : "");
                }
            }

            var studentProgram = await _studentProgramRepository.AddStudentProgram(addProgramDto, activePrograms, endDates);

            var studentProgramDtoAdapter = _adapterRegistry.GetAdapter<StudentProgram, Dtos.Student.StudentProgram2>();
            // Map the student programs entity to the student programs DTO
            programDto = studentProgramDtoAdapter.MapToType(studentProgram);
            return programDto;
        }


        /// <summary>
        /// Update academic program for a student
        /// </summary>
        /// <param name="studentAcademicProgram">Student Academic program information</param>
        /// <returns><see cref="Dtos.Student.StudentProgram2">Updated student program</returns>
        public async Task<Dtos.Student.StudentProgram2> UpdateStudentProgram(Dtos.Student.StudentAcademicProgram studentAcademicProgram)
        {
            if (studentAcademicProgram == null)
            {
                throw new ArgumentNullException("studentAcademicProgram", "Academic Prgoram information must specify to update.");
            }
            if (string.IsNullOrEmpty(studentAcademicProgram.StudentId))
            {
                throw new ArgumentException("add program must have a student Id to update a student program.");
            }
            //Check Permissions to be sure user is advisor.
            if (!(await UserIsAdvisorAsync(studentAcademicProgram.StudentId)))
            {
                throw new PermissionsException(string.Format("User does not have permissions to update program for student {0}.", studentAcademicProgram.StudentId));
            }
            var addProgramDtoAdapter = _adapterRegistry.GetAdapter<Dtos.Student.StudentAcademicProgram, StudentAcademicProgram>();
            var addProgramDto = addProgramDtoAdapter.MapToType(studentAcademicProgram);

            var programDto = new Dtos.Student.StudentProgram2();

            List<string> activePrograms = new List<string>();
            List<string> endDates = new List<string>();

            if (studentAcademicProgram.ActivePrograms != null && studentAcademicProgram.ActivePrograms.Any())
            {
                foreach (var prog in studentAcademicProgram.ActivePrograms)
                {
                    activePrograms.Add(prog.ProgramCode);
                    endDates.Add(!string.IsNullOrEmpty(prog.EndDate) ? DateTime.Parse(prog.EndDate).ToShortDateString() : "");
                }
            }

            var studentProgram = await _studentProgramRepository.UpdateStudentProgram(addProgramDto, activePrograms, endDates);

            var studentProgramDtoAdapter = _adapterRegistry.GetAdapter<StudentProgram, Dtos.Student.StudentProgram2>();
            // Map the student programs entity to the student programs DTO
            programDto = studentProgramDtoAdapter.MapToType(studentProgram);
            return programDto;
        }


        /// <summary>
        /// Find a term for the Student Program Entity
        /// </summary>
        /// <param name="startDate">Start Date of the Student Program</param>
        /// <param name="endDate">End Date of the Student Program</param>
        /// <param name="checkTerm">Term where dates fall into place.  (Best Fit)</param>
        /// <returns>Returns the term for which there is a best fit based on start and end dates.</returns>
        private string FindTermForProgram(DateTime? startDate, DateTime? endDate, string checkTerm)
        {
            string term = "";
            if (startDate.HasValue)
            {
                // fetch this once, and only once needed
                if (termList == null) termList = _termRepository.Get();
                if (termList != null && termList.Count() > 0)
                {
                    var testTerms = termList.Where(t => ((t.StartDate.CompareTo(startDate.Value) <= 0 && t.EndDate.CompareTo(startDate.Value) >= 0) ||
                            (t.StartDate.CompareTo(startDate.Value) >= 0 && (endDate.HasValue && t.StartDate.CompareTo(endDate.Value) <= 0)) ||
                            (t.StartDate.CompareTo(startDate.Value) >= 0 && !endDate.HasValue)));
                    if (testTerms != null && testTerms.Count() > 0)
                    {
                        foreach (var singleTerm in testTerms)
                        {
                            if (singleTerm.Code == checkTerm)
                            {
                                term = singleTerm.Code;
                            }
                        }
                        if (string.IsNullOrEmpty(term))
                        {
                            term = testTerms.First().Code;
                        }
                    }
                }
            }
            return term;
        }
    }
}
