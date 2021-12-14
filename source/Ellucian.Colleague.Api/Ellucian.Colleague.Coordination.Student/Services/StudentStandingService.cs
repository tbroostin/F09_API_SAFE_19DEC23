// Copyright 2015 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class StudentStandingService : StudentCoordinationService, IStudentStandingService
    {
        private readonly IStudentStandingRepository _studentStandingRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ITermRepository _termRepository;
        private ILogger _logger;
        private readonly IConfigurationRepository _configurationRepository;

        public StudentStandingService(IAdapterRegistry adapterRegistry, IStudentRepository studentRepository, IStudentStandingRepository studentStandingRepository,ITermRepository termRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _studentStandingRepository = studentStandingRepository;
            _studentRepository = studentRepository;
            _logger = logger;
            _termRepository = termRepository;
        }

        /// <summary>
        /// Get the student standings for a set of students
        /// </summary>
        /// <param name="studentIds">Collection of student ids to get standings for</param>
        /// <param name="term">Optional term used for filtering the results</param>
        /// <param name="currentTerm">Optional term used for calculating when a standing is current</param>
        /// <returns>Returns the student standings, filtered according to the parameters if applicable</returns>
        public async Task<IEnumerable<Dtos.Student.StudentStanding>> GetAsync(IEnumerable<string> studentIds, string term = null, string currentTerm = null)
        {
            ICollection<Dtos.Student.StudentStanding> studentStandingDto = new List<Dtos.Student.StudentStanding>();

            if (HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {
                IEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentStanding> studentStandings = new List<Ellucian.Colleague.Domain.Student.Entities.StudentStanding>();
                if (string.IsNullOrWhiteSpace(currentTerm))
                    studentStandings = await _studentStandingRepository.GetAsync(studentIds, term);
                else
                    studentStandings = await GetCurrentStandingsByTermAsync(studentIds, term, currentTerm);

                foreach (var studentStanding in studentStandings)
                {
                    // Get the right adapter for the type mapping
                    var studentStandingDtoAdapter = _adapterRegistry.GetAdapter<StudentStanding, Ellucian.Colleague.Dtos.Student.StudentStanding>();

                    // Map the student standing entity to the student standing DTO
                    var standingDto = studentStandingDtoAdapter.MapToType(studentStanding);

                    studentStandingDto.Add(standingDto);
                }
            }
            else
            {
                // Person asking isn't student and isn't a valid advisor. Throw Permission exception.
                throw new PermissionsException("User does not have permissions to access these student standings.");
            }
            return studentStandingDto;
        }

        /// <summary>
        /// Get Student Academic standings
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <returns>List of Student Academic Standings</returns>
        public async Task<IEnumerable<Dtos.Student.StudentStanding>> GetStudentAcademicStandingsAsync(string studentId)
        {
            List<Dtos.Student.StudentStanding> studentAcademicStandingsDto = new List<Dtos.Student.StudentStanding>();
            if (string.IsNullOrWhiteSpace(studentId))
            {
                throw new ArgumentNullException("studentId", "Student Id must be provided in order to retrieve student's academic standings");
            }

            if (!UserIsSelf(studentId) && !(await UserIsAdvisorAsync(studentId)))
            {
                var message = "Current user is not the student to request academic standings or current user is advisor or faculty but doesn't have appropriate permissions and therefore cannot access it.";
                logger.Error(message);
                throw new PermissionsException(message);
            }
            try
            {
                IEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentStanding> studentAcademicStandingEntity = await _studentStandingRepository.GetAsync(new List<string>(){studentId }); ;
                if (studentAcademicStandingEntity != null && studentAcademicStandingEntity.Any())
                {
                    var studentAcademicLevelEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentStanding, Dtos.Student.StudentStanding>();
                    foreach (var studentAcadStandingEntity in studentAcademicStandingEntity)
                    {
                        studentAcademicStandingsDto.Add(studentAcademicLevelEntityToDtoAdapter.MapToType(studentAcadStandingEntity));
                    }
                }
                else
                {
                    logger.Warn("Repository call to retrieve student's academic standings returns null or empty entity");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Couldn't retrieve student academic standings for student {0}", studentId));
                throw;
            }
            return studentAcademicStandingsDto;
        }

        /// <summary>
        /// Get the student standings for a set of students, filtered by term and with the current flag set.
        /// </summary>
        /// <param name="studentIds">Collection of student ids to get standings for</param>
        /// <param name="term">Term used for filtering the results</param>
        /// <param name="currentTerm">Term used for calculating when a standing is current</param>
        /// <returns>Returns the student standings filtered according to the parameters, with the current flag set appropriately</returns>
        private async Task<List<Domain.Student.Entities.StudentStanding>> GetCurrentStandingsByTermAsync(IEnumerable<string> studentIds, string term, string currentTerm)
        {
            List <Domain.Student.Entities.StudentStanding> standingsResults = new List<Domain.Student.Entities.StudentStanding>();
            IEnumerable<Term> terms = await _termRepository.GetAsync(); // Get terms
            Term currentTermDomain = terms.FirstOrDefault(t => t.Code == currentTerm);
            Dictionary<string, List<Domain.Student.Entities.StudentStanding>> standings = await _studentStandingRepository.GetGroupedAsync(studentIds);// Get all standings for the students
            foreach (var studentStandingList in standings.Values) // in each iteration of this loop, studentStandingList is a list of all standings for one student.
            {
                Dictionary<string, Domain.Student.Entities.StudentStanding> mostCurrentByLevel = new Dictionary<string, Domain.Student.Entities.StudentStanding>(); // Most current student standing that is in currentTerm or earlier.
                foreach (var studentStanding in studentStandingList)
                {
                    try
                    {
                        if ((!string.IsNullOrWhiteSpace(studentStanding.Term)) && (currentTermDomain == null || TermEndDateLessOrEqual(studentStanding.Term, currentTermDomain, terms)))
                        {
                            if (!mostCurrentByLevel.ContainsKey(studentStanding.Level) || IsMoreCurrent(studentStanding, mostCurrentByLevel[studentStanding.Level], terms)) // Check if it's the most current encountered so far for the level
                                mostCurrentByLevel[studentStanding.Level] = studentStanding;
                            if (studentStanding.Term == term)
                            {
                                standingsResults.Add(studentStanding);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(string.Format("Failed to set current flag for student standing {0} for student {1}", studentStanding.Id, studentStanding.StudentId));
                        logger.Error(e.GetBaseException().Message + e.GetBaseException().StackTrace);
                    }
                }
                foreach (var mostCurrent in mostCurrentByLevel.Values)
                {
                    mostCurrent.IsCurrent = true;
                    if (mostCurrent.Term != term)
                        standingsResults.Add(mostCurrent); // only have to add ones that weren't already added (their term != parameter term but they're the most current with term <= current term)
                }
            }
            return standingsResults;
        }

        /// <summary>
        /// Compare two standings and return whether the first is more current than the second
        /// </summary>
        /// <param name="first">StudentStanding to compare against the second one</param>
        /// <param name="second">StudentStanding to compare the first one against</param>
        /// <param name="terms">Collection of Term domain objects, needed to compare dates for the terms in the standings</param>
        /// <returns>Returns true if the first standing is more current than the second, false otherwise</returns>
        private bool IsMoreCurrent(Domain.Student.Entities.StudentStanding first, Domain.Student.Entities.StudentStanding second, IEnumerable<Term> terms)
        {
            //
            // Setup sequential Ids as integers in case we need them as tiebreakers
            // when multiple standings for same student/level/term/standing date
            //
            int firstId;
            int secondId;
            Int32.TryParse(first.Id, out firstId);
            Int32.TryParse(second.Id, out secondId);
                
            bool isMore = false;
            var firstTerm = terms.FirstOrDefault(t => t.Code == first.Term);
            var secondTerm = terms.FirstOrDefault(t => t.Code == second.Term);
            if (firstTerm.EndDate > secondTerm.EndDate)
                isMore = true;
            else if (firstTerm.EndDate == secondTerm.EndDate && first.StandingDate > second.StandingDate)
                isMore = true;
            else if (firstTerm.EndDate == secondTerm.EndDate && first.StandingDate == second.StandingDate && firstId > secondId)                            
                isMore = true;
            return isMore;
        }

        /// <summary>
        /// Compares a term referenced by a term code to a Term domain object and returns whether the first has an end date earlier than or equal to the second.
        /// </summary>
        /// <param name="termToCheck">Term code as a string</param>
        /// <param name="termToCheckAgainst">Term domain object to compare the first term with</param>
        /// <param name="terms">Collection of Term domain objects, needed to compare dates for the terms</param>
        /// <returns>Returns true if the first standing is more current than the second, false otherwise</returns>
        private bool TermEndDateLessOrEqual(string termToCheck, Term termToCheckAgainst, IEnumerable<Term> terms)
        {
            bool result = false;
            var termDomain = terms.FirstOrDefault(t=>t.Code == termToCheck);
            if (termDomain.EndDate <= termToCheckAgainst.EndDate)
                result = true;
            return result;
        }
    }
}
