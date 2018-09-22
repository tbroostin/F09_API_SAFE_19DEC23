// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using slf4net;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Dependency;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Domain.Base.Repositories;

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

        public async Task<IEnumerable<Dtos.Student.StudentProgram2>> GetStudentProgramsByIdsAsync(IEnumerable<string> studentIds, bool includeInactivePrograms = false, string term = null, bool includeHistory = false)
        {
            ICollection<Dtos.Student.StudentProgram2> studentProgramDto = new List<Dtos.Student.StudentProgram2>();

            if (HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {
                Term termDomain = null;
                if (!string.IsNullOrWhiteSpace(term))
                    termDomain = await _termRepository.GetAsync(term);
                IEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentProgram> studentPrograms = await _studentProgramRepository.GetStudentProgramsByIdsAsync(studentIds, includeInactivePrograms, termDomain, includeHistory);

                foreach (var studentProgram in studentPrograms)
                {
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
            }
            else
            {
                // Person asking isn't student and isn't a valid advisor. Throw Permission exception.
                throw new PermissionsException("User does not have permissions to access these student programs.");
            }
            return studentProgramDto;
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
