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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class AcademicHistoryService : StudentCoordinationService, IAcademicHistoryService
    {
        private readonly IAcademicCreditRepository _academicCreditRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ITermRepository _termRepository;
        private readonly ISectionRepository _sectionRepository;
        private ILogger _logger;
        private readonly IConfigurationRepository _configurationRepository;

        public AcademicHistoryService(IAdapterRegistry adapterRegistry, IStudentRepository studentRepository, IAcademicCreditRepository academicCreditRepository, ITermRepository termRepository, ISectionRepository sectionRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _academicCreditRepository = academicCreditRepository;
            _studentRepository = studentRepository;
            _termRepository = termRepository;
            _sectionRepository = sectionRepository;
            _logger = logger;
            _configurationRepository = configurationRepository;
        }

        public async Task<IEnumerable<Dtos.Student.AcademicHistoryLevel>> QueryAcademicHistoryLevelAsync(Dtos.Student.AcademicHistoryQueryCriteria criteria)
        {
            IEnumerable<string> studentIds = criteria.StudentIds;
            bool bestFit = criteria.BestFit;
            bool filter = criteria.Filter;
            string term = criteria.Term;

            return await GetAcademicHistoryLevelByIdsAsync(studentIds, bestFit, filter, term);
        }

        public async Task<IEnumerable<Dtos.Student.AcademicHistoryLevel2>> QueryAcademicHistoryLevel2Async(Dtos.Student.AcademicHistoryQueryCriteria criteria)
        {
            IEnumerable<string> studentIds = criteria.StudentIds;
            bool bestFit = criteria.BestFit;
            bool filter = criteria.Filter;
            string term = criteria.Term;

            return await GetAcademicHistoryLevel2ByIdsAsync(studentIds, bestFit, filter, term);
        }

        public async Task<IEnumerable<Dtos.Student.AcademicHistoryLevel3>> QueryAcademicHistoryLevel3Async(Dtos.Student.AcademicHistoryQueryCriteria criteria)
        {
            IEnumerable<string> studentIds = criteria.StudentIds;
            bool bestFit = criteria.BestFit;
            bool filter = criteria.Filter;
            string term = criteria.Term;

            return await GetAcademicHistoryLevel3ByIdsAsync(studentIds, bestFit, filter, term);
        }

        public async Task<IEnumerable<Dtos.Student.PilotAcademicHistoryLevel>> QueryPilotAcademicHistoryLevelAsync(Dtos.Student.AcademicHistoryQueryCriteria criteria)
        {
            IEnumerable<string> studentIds = criteria.StudentIds;
            bool bestFit = criteria.BestFit;
            bool filter = criteria.Filter;
            string term = criteria.Term;
            IEnumerable<Dtos.Student.PilotAcademicHistoryLevel> result;
            if (criteria.IncludeStudentSections)
                result = await GetPilotStudentSectionsAsync(studentIds, bestFit, filter, term);
            else
                result = await GetPilotAcademicHistoryLevelByIdsAsync(studentIds, bestFit, filter, term);
            return result;
        }

        [Obsolete("Obsolete as of API version 1.18, use QueryAcademicHistory2Async instead")]
        public async Task<IEnumerable<Dtos.Student.AcademicHistoryBatch>> QueryAcademicHistoryAsync(Dtos.Student.AcademicHistoryQueryCriteria criteria)
        {
            IEnumerable<string> studentIds = criteria.StudentIds;
            bool bestFit = criteria.BestFit;
            bool filter = criteria.Filter;
            string term = criteria.Term;

            return await GetAcademicHistoryByIdsAsync(studentIds, bestFit, filter, term);
        }

        /// <summary>
        /// Gets the <see cref="AcademicHistoryBatch2">academic history</see> for students based on criteria
        /// </summary>
        /// <param name="criteria"><see cref="AcademicHistoryQueryCriteria">Criteria</see> used in retrieving students' academic history</param>
        /// <returns>The <see cref="AcademicHistoryBatch2">academic history</see> for students</returns>
        public async Task<IEnumerable<Dtos.Student.AcademicHistoryBatch2>> QueryAcademicHistory2Async(Dtos.Student.AcademicHistoryQueryCriteria criteria)
        {
            IEnumerable<string> studentIds = criteria.StudentIds;
            bool bestFit = criteria.BestFit;
            bool filter = criteria.Filter;
            string term = criteria.Term;

            return await GetAcademicHistoryByIds2Async(studentIds, bestFit, filter, term);
        }

        [Obsolete("Obsolete as of API version 1.18, use GetAcademicHistoryByIds2Async instead")]
        public async Task<IEnumerable<Dtos.Student.AcademicHistoryBatch>> GetAcademicHistoryByIdsAsync(IEnumerable<string> studentIds, bool bestFit, bool filter = true, string term = null)
        {
            ICollection<Dtos.Student.AcademicHistoryBatch> academicHistoryDto = new List<Dtos.Student.AcademicHistoryBatch>();
            // If the person requesting the information has permission to
            // view all student information.
            if (HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {
                Dictionary<string, List<AcademicCredit>> studentAcadCreds = await _academicCreditRepository.GetAcademicCreditByStudentIdsAsync(studentIds.ToList(), bestFit, filter);
                foreach (var studentId in studentIds)
                {
                    // It's possible that the student didn't have any Academic Credits
                    // and therefore will not be in the list.
                    if (studentAcadCreds.ContainsKey(studentId))
                    {
                        List<AcademicCredit> studentAcademicCredits = studentAcadCreds[studentId];

                        if (studentAcadCreds != null && studentAcadCreds.Count > 0)
                        {
                            if (studentAcademicCredits != null && studentAcademicCredits.Count() > 0)
                            {
                                // Grade Restrictions removed for Batch of Students since they are used by
                                // students who are viewing their own data to determine how they see grades.
                                // srm - 09/11/2014  (This also means we use a different DTO.
                                // GradeRestriction studentGradeRestriction = _studentRepository.GetGradeRestrictions(studentId);

                                AcademicHistory studentHistory = null;

                                // Always build history without accounting for the grade restriction.
                                studentHistory = new AcademicHistory(studentAcademicCredits, new GradeRestriction(false), null);
                                // Put the real student's grade restriction back into the history
                                // studentHistory.GradeRestriction = studentGradeRestriction;

                                if (string.IsNullOrEmpty(studentHistory.StudentId))
                                {
                                    studentHistory.StudentId = studentId;
                                }
                                // Filter to return only one specific term of data if we have a term filter set
                                if (!string.IsNullOrEmpty(term))
                                {
                                    studentHistory.FilterTerm(term);
                                }

                                if (!string.IsNullOrEmpty(studentId))
                                {
                                    // Get the right adapter for the type mapping
                                    var academicHistoryDtoAdapter = _adapterRegistry.GetAdapter<AcademicHistory, Ellucian.Colleague.Dtos.Student.AcademicHistoryBatch>();

                                    // Map the AcademicHistory entity to the AcademicHistory DTO
                                    var historyDto = academicHistoryDtoAdapter.MapToType(studentHistory);
                                    academicHistoryDto.Add(historyDto);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Person asking isn't student and isn't a valid advisor. Throw Permission exception.
                throw new PermissionsException("User does not have permissions to access the student academic history.");
            }
            return academicHistoryDto;
        }

        /// <summary>
        /// Gets the <see cref="AcademicHistoryBatch2">academic history</see> for a student based on criteria
        /// </summary>
        /// <param name="studentIds">IDs of students for whom academic histories will be retrieved</param>
        /// <param name="bestFit">(Optional) If true, non-term credit is fitted into terms based on dates</param>
        /// <param name="filter">(Optional) used to filter to active credit only.</param>
        /// <param name="term">(Optional) used to return only a specific term of data.</param>
        /// <returns>The <see cref="AcademicHistoryBatch2">academic history</see> for a student</returns>
        public async Task<IEnumerable<Dtos.Student.AcademicHistoryBatch2>> GetAcademicHistoryByIds2Async(IEnumerable<string> studentIds, bool bestFit, bool filter = true, string term = null)
        {
            ICollection<Dtos.Student.AcademicHistoryBatch2> academicHistoryDto = new List<Dtos.Student.AcademicHistoryBatch2>();
            // If the person requesting the information has permission to
            // view all student information.
            if (HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {
                Dictionary<string, List<AcademicCredit>> studentAcadCreds = await _academicCreditRepository.GetAcademicCreditByStudentIdsAsync(studentIds.ToList(), bestFit, filter);
                foreach (var studentId in studentIds)
                {
                    // It's possible that the student didn't have any Academic Credits
                    // and therefore will not be in the list.
                    if (studentAcadCreds.ContainsKey(studentId))
                    {
                        List<AcademicCredit> studentAcademicCredits = studentAcadCreds[studentId];

                        if (studentAcadCreds != null && studentAcadCreds.Count > 0)
                        {
                            if (studentAcademicCredits != null && studentAcademicCredits.Count() > 0)
                            {
                                // Grade Restrictions removed for Batch of Students since they are used by
                                // students who are viewing their own data to determine how they see grades.
                                // srm - 09/11/2014  (This also means we use a different DTO.
                                // GradeRestriction studentGradeRestriction = _studentRepository.GetGradeRestrictions(studentId);

                                AcademicHistory studentHistory = null;

                                // Always build history without accounting for the grade restriction.
                                studentHistory = new AcademicHistory(studentAcademicCredits, new GradeRestriction(false), null);
                                // Put the real student's grade restriction back into the history
                                // studentHistory.GradeRestriction = studentGradeRestriction;

                                if (string.IsNullOrEmpty(studentHistory.StudentId))
                                {
                                    studentHistory.StudentId = studentId;
                                }
                                // Filter to return only one specific term of data if we have a term filter set
                                if (!string.IsNullOrEmpty(term))
                                {
                                    studentHistory.FilterTerm(term);
                                }

                                if (!string.IsNullOrEmpty(studentId))
                                {
                                    // Get the right adapter for the type mapping
                                    var academicHistoryDtoAdapter = _adapterRegistry.GetAdapter<AcademicHistory, Ellucian.Colleague.Dtos.Student.AcademicHistoryBatch2>();

                                    // Map the AcademicHistory entity to the AcademicHistory DTO
                                    var historyDto = academicHistoryDtoAdapter.MapToType(studentHistory);
                                    academicHistoryDto.Add(historyDto);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Person asking isn't student and isn't a valid advisor. Throw Permission exception.
                throw new PermissionsException("User does not have permissions to access the student academic history.");
            }
            return academicHistoryDto;
        }


        public async Task<IEnumerable<Dtos.Student.AcademicHistoryLevel>> GetAcademicHistoryLevelByIdsAsync(IEnumerable<string> studentIds, bool bestFit, bool filter = true, string term = null)
        {
            IEnumerable<Term> termData = null;
            ICollection<Dtos.Student.AcademicHistoryLevel> academicHistoryLevelDto = new List<Dtos.Student.AcademicHistoryLevel>();
            // If the person requesting the information has permission to
            // view all student information.
            if (HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {
                Dictionary<string, List<AcademicCredit>> studentAcadCreds = await _academicCreditRepository.GetAcademicCreditByStudentIdsAsync(studentIds.ToList(), bestFit, filter);
                IEnumerable<Section> sections = new List<Section>();
                // Determine if we need to use census date checking for first term enrolled
                bool useCensusDate = await _academicCreditRepository.GetPilotCensusBooleanAsync();
                HashSet<string> sectionIds = new HashSet<string>();
                foreach (var student in studentAcadCreds)
                {
                    foreach (var credit in student.Value)
                    {
                        if (credit.AcademicLevelCode != null && credit.SectionId != null)
                            sectionIds.Add(credit.SectionId);
                        if (credit.TermCode != null && termData == null)
                        {
                            termData = await _termRepository.GetAsync(); // First time we see a credit with a term, get term data.
                        }
                    }
                }
                if (useCensusDate && sectionIds.Count() != 0) // only need to read sections if we're doing census date checking while determining first term enrolled
                {
                    sections = await _sectionRepository.GetCachedSectionsAsync(sectionIds);
                }
                foreach (var studentId in studentIds)
                {
                    // It's possible that the student didn't have any Academic Credits
                    // and therefore will not be in the list.
                    if (studentAcadCreds.ContainsKey(studentId))
                    {
                        List<AcademicCredit> studentAcademicCredits = studentAcadCreds[studentId];
                        if (studentAcadCreds != null && studentAcadCreds.Count > 0)
                        {
                            if (studentAcademicCredits != null && studentAcademicCredits.Count() > 0)
                            {
                                // Get term data from credits.  We'll need term start dates to determine FTE.
                                var termCredits = studentAcademicCredits.Where(c => c.TermCode != null);
                                var termIds = (from credit in termCredits select credit.TermCode).Distinct();
                                // Loop through credits for each academic level
                                var levels = (from credit in studentAcademicCredits select credit.AcademicLevelCode).Distinct();
                                foreach (var level in levels)
                                {
                                    var credits = studentAcademicCredits.Where(c => c.AcademicLevelCode != null && c.AcademicLevelCode == level);
                                    string firstTermEnrolled = await GetFirstTermEnrolledAsync(termData, credits, sections, useCensusDate);
                                    var studentHistory = new AcademicHistory(credits, new GradeRestriction(false), firstTermEnrolled);
                                    if (string.IsNullOrEmpty(studentHistory.StudentId))
                                    {
                                        studentHistory.StudentId = studentId;
                                    }
                                    // Filter to return only one specific term of data if we have a term filter set
                                    if (!string.IsNullOrEmpty(term))
                                    {
                                        studentHistory.FilterTerm(term);
                                    }
                                    if (!string.IsNullOrEmpty(studentId))
                                    {
                                        // Add Logger Messages if we can't build AcademicHistory or the DTO
                                        try
                                        {
                                            var studentHistoryLevel = new AcademicHistoryLevel(level, studentHistory, studentId);
                                            // Get the right adapter for the type mapping
                                            var academicHistoryLevelDtoAdapter = _adapterRegistry.GetAdapter<AcademicHistoryLevel, Ellucian.Colleague.Dtos.Student.AcademicHistoryLevel>();

                                            // Map the AcademicHistory entity to the AcademicHistory DTO
                                            var historyDto = academicHistoryLevelDtoAdapter.MapToType(studentHistoryLevel);
                                            academicHistoryLevelDto.Add(historyDto);
                                        }
                                        catch (Exception ex)
                                        {
                                            // Couldn't build the AcademicHistoryLevel or the DTO.
                                            var errorMessage = "Unable to build AcademicHistoryLevel for Student '" + studentId + "', Level '" + level + "'. exception thrown: " + ex.Message;
                                            logger.Error(errorMessage);
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
                // Person doesn't have View Student Information permissions. Throw Permission exception.
                throw new PermissionsException("User does not have permissions to access the student academic history.");
            }
            return academicHistoryLevelDto;
        }

        public async Task<IEnumerable<Dtos.Student.AcademicHistoryLevel2>> GetAcademicHistoryLevel2ByIdsAsync(IEnumerable<string> studentIds, bool bestFit, bool filter = true, string term = null)
        {
            IEnumerable<Term> termData = null;
            ICollection<Dtos.Student.AcademicHistoryLevel2> academicHistoryLevelDto = new List<Dtos.Student.AcademicHistoryLevel2>();
            // If the person requesting the information has permission to
            // view all student information.
            if (HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {
                Dictionary<string, List<AcademicCredit>> studentAcadCreds = await _academicCreditRepository.GetAcademicCreditByStudentIdsAsync(studentIds.ToList(), bestFit, filter);
                IEnumerable<Section> sections = new List<Section>();
                // Determine if we need to use census date checking for first term enrolled
                bool useCensusDate = await _academicCreditRepository.GetPilotCensusBooleanAsync();
                HashSet<string> sectionIds = new HashSet<string>();
                foreach (var student in studentAcadCreds)
                {
                    foreach (var credit in student.Value)
                    {
                        if (credit.AcademicLevelCode != null && credit.SectionId != null)
                            sectionIds.Add(credit.SectionId);
                        if (credit.TermCode != null && termData == null)
                        {
                            termData = await _termRepository.GetAsync(); // First time we see a credit with a term, get term data.
                        }
                    }
                }
                if (useCensusDate && sectionIds.Count() != 0) // only need to read sections if we're doing census date checking while determining first term enrolled
                {
                    sections = await _sectionRepository.GetCachedSectionsAsync(sectionIds);
                }
                foreach (var studentId in studentIds)
                {
                    try
                    {
                        // It's possible that the student didn't have any Academic Credits
                        // and therefore will not be in the list.
                        if (studentAcadCreds.ContainsKey(studentId))
                        {
                            List<AcademicCredit> studentAcademicCredits = studentAcadCreds[studentId];
                            if (studentAcadCreds != null && studentAcadCreds.Count > 0)
                            {
                                if (studentAcademicCredits != null && studentAcademicCredits.Count() > 0)
                                {
                                    // Get term data from credits.  We'll need term start dates to determine FTE.
                                    var termCredits = studentAcademicCredits.Where(c => c.TermCode != null);
                                    var termIds = (from credit in termCredits select credit.TermCode).Distinct();
                                    // Loop through credits for each academic level
                                    var levels = (from credit in studentAcademicCredits select credit.AcademicLevelCode).Distinct();
                                    foreach (var level in levels)
                                    {
                                        var credits = studentAcademicCredits.Where(c => c.AcademicLevelCode != null && c.AcademicLevelCode == level);
                                        string firstTermEnrolled = await GetFirstTermEnrolledAsync(termData, credits, sections, useCensusDate);
                                        var studentHistory = new AcademicHistory(credits, new GradeRestriction(false), firstTermEnrolled);
                                        if (string.IsNullOrEmpty(studentHistory.StudentId))
                                        {
                                            studentHistory.StudentId = studentId;
                                        }
                                        // Filter to return only one specific term of data if we have a term filter set
                                        if (!string.IsNullOrEmpty(term))
                                        {
                                            studentHistory.FilterTerm(term);
                                        }

                                        if (!string.IsNullOrEmpty(studentId))
                                        {
                                            // Add Logger Messages if we can't build AcademicHistory or the DTO
                                            try
                                            {
                                                var studentHistoryLevel = new AcademicHistoryLevel(level, studentHistory, studentId);
                                                // Get the right adapter for the type mapping
                                                var academicHistoryLevel2DtoAdapter = _adapterRegistry.GetAdapter<AcademicHistoryLevel, Ellucian.Colleague.Dtos.Student.AcademicHistoryLevel2>();

                                                // Map the AcademicHistory entity to the AcademicHistory DTO
                                                var historyDto = academicHistoryLevel2DtoAdapter.MapToType(studentHistoryLevel);
                                                academicHistoryLevelDto.Add(historyDto);
                                            }
                                            catch (Exception ex)
                                            {
                                                // Couldn't build the AcademicHistoryLevel or the DTO.
                                                var errorMessage = "Unable to build AcademicHistoryLevel for Student '" + studentId + "', Level '" + level + "'. exception thrown: " + ex.Message;
                                                logger.Error(errorMessage);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(string.Format("Failed to build academic history levels for student {0}", studentId));
                        logger.Error(e.GetBaseException().Message);
                        logger.Error(e.GetBaseException().StackTrace);
                    }
                }
            }
            else
            {
                // Person doesn't have View Student Information permissions. Throw Permission exception.
                throw new PermissionsException("User does not have permissions to access the student academic history.");
            }
            return academicHistoryLevelDto;
        }

        public async Task<IEnumerable<Dtos.Student.AcademicHistoryLevel3>> GetAcademicHistoryLevel3ByIdsAsync(IEnumerable<string> studentIds, bool bestFit, bool filter = true, string term = null)
        {
            IEnumerable<Term> termData = null;
            ICollection<Dtos.Student.AcademicHistoryLevel3> academicHistoryLevelDto = new List<Dtos.Student.AcademicHistoryLevel3>();
            // If the person requesting the information has permission to
            // view all student information.
            if (HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {
                Dictionary<string, List<AcademicCredit>> studentAcadCreds = await _academicCreditRepository.GetAcademicCreditByStudentIdsAsync(studentIds.ToList(), bestFit, filter);
                IEnumerable<Section> sections = new List<Section>();
                // Determine if we need to use census date checking for first term enrolled
                bool useCensusDate = await _academicCreditRepository.GetPilotCensusBooleanAsync();
                HashSet<string> sectionIds = new HashSet<string>();
                foreach (var student in studentAcadCreds)
                {
                    foreach (var credit in student.Value)
                    {
                        if (credit.AcademicLevelCode != null && credit.SectionId != null)
                            sectionIds.Add(credit.SectionId);
                        if (credit.TermCode != null && termData == null)
                        {
                            termData = await _termRepository.GetAsync(); // First time we see a credit with a term, get term data.
                        }
                    }
                }
                if (useCensusDate && sectionIds.Count() != 0) // only need to read sections if we're doing census date checking while determining first term enrolled
                {
                    sections = await _sectionRepository.GetCachedSectionsAsync(sectionIds);
                }
                foreach (var studentId in studentIds)
                {
                    try
                    {
                        // It's possible that the student didn't have any Academic Credits
                        // and therefore will not be in the list.
                        if (studentAcadCreds.ContainsKey(studentId))
                        {
                            List<AcademicCredit> studentAcademicCredits = studentAcadCreds[studentId];
                            if (studentAcadCreds != null && studentAcadCreds.Count > 0)
                            {
                                if (studentAcademicCredits != null && studentAcademicCredits.Count() > 0)
                                {
                                    // Get term data from credits.  We'll need term start dates to determine FTE.
                                    var termCredits = studentAcademicCredits.Where(c => c.TermCode != null);
                                    var termIds = (from credit in termCredits select credit.TermCode).Distinct();
                                    // Loop through credits for each academic level
                                    var levels = (from credit in studentAcademicCredits select credit.AcademicLevelCode).Distinct();
                                    foreach (var level in levels)
                                    {
                                        var credits = studentAcademicCredits.Where(c => c.AcademicLevelCode != null && c.AcademicLevelCode == level);
                                        string firstTermEnrolled = await GetFirstTermEnrolledAsync(termData, credits, sections, useCensusDate);
                                        var studentHistory = new AcademicHistory(credits, new GradeRestriction(false), firstTermEnrolled);
                                        if (string.IsNullOrEmpty(studentHistory.StudentId))
                                        {
                                            studentHistory.StudentId = studentId;
                                        }
                                        // Filter to return only one specific term of data if we have a term filter set
                                        if (!string.IsNullOrEmpty(term))
                                        {
                                            studentHistory.FilterTerm(term);
                                        }

                                        if (!string.IsNullOrEmpty(studentId))
                                        {
                                            // Add Logger Messages if we can't build AcademicHistory or the DTO
                                            try
                                            {
                                                var studentHistoryLevel = new AcademicHistoryLevel(level, studentHistory, studentId);
                                                // Get the right adapter for the type mapping
                                                var academicHistoryLevel3DtoAdapter = _adapterRegistry.GetAdapter<AcademicHistoryLevel, Ellucian.Colleague.Dtos.Student.AcademicHistoryLevel3>();

                                                // Map the AcademicHistory entity to the AcademicHistory DTO
                                                var historyDto = academicHistoryLevel3DtoAdapter.MapToType(studentHistoryLevel);
                                                academicHistoryLevelDto.Add(historyDto);
                                            }
                                            catch (Exception ex)
                                            {
                                                // Couldn't build the AcademicHistoryLevel or the DTO.
                                                var errorMessage = "Unable to build AcademicHistoryLevel for Student '" + studentId + "', Level '" + level + "'. exception thrown: " + ex.Message;
                                                logger.Error(errorMessage);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(string.Format("Failed to build academic history levels for student {0}", studentId));
                        logger.Error(e.GetBaseException().Message);
                        logger.Error(e.GetBaseException().StackTrace);
                    }
                }
            }
            else
            {
                // Person doesn't have View Student Information permissions. Throw Permission exception.
                throw new PermissionsException("User does not have permissions to access the student academic history.");
            }
            return academicHistoryLevelDto;
        }


        private Stopwatch completeTime;
        //private Stopwatch getStcs;
        //private Stopwatch stcReads;
        //private Stopwatch pipaRead;
        //private Stopwatch termsReads;
        private Stopwatch sectionsReads;
        //private Stopwatch fteReads;
        //private Stopwatch effectiveCensusDatesTimer;
        //private Stopwatch censusDateCheckTimer;
        //private Stopwatch sectionFilterTimer;
        //private Stopwatch singleSectionTimer;
        //private Stopwatch studentIterationTimer;
        //private Stopwatch getFteTimer;

        /// <summary>
        /// Gets the academic histories for a list of students in a way that only reads the files necessary to fulfill Pilot's requirements.
        /// </summary>
        /// <param name="studentIds">A collection of student ids</param>
        /// <param name="bestFit">Whether to fit non-term credits into the nearest term</param>
        /// <param name="filter">Whether to filter out unknown credit types</param>
        /// <param name="term">Term to filter returned data</param>
        /// <returns></returns>
        public async Task<IEnumerable<Dtos.Student.PilotAcademicHistoryLevel>> GetPilotAcademicHistoryLevelByIdsAsync(IEnumerable<string> studentIds, bool bestFit, bool filter, string term = null)
        {
            completeTime = new Stopwatch();
            //logger.Error("GetPilotAcademicHistoryLevelByIdsAsync_START: " + DateTime.Now.ToString());
            //getStcs = new Stopwatch();
            //stcReads = new Stopwatch();
            //pipaRead = new Stopwatch();
            //termsReads = new Stopwatch();
            sectionsReads = new Stopwatch();
            //Stopwatch fteTime = new Stopwatch();
            //fteReads = new Stopwatch();
            //effectiveCensusDatesTimer = new Stopwatch();
            //censusDateCheckTimer = new Stopwatch();
            //sectionFilterTimer = new Stopwatch();
            //singleSectionTimer = new Stopwatch();
            //studentIterationTimer = new Stopwatch();
            //getFteTimer = new Stopwatch();

            completeTime.Start();
            
            IEnumerable<Term> termData = null;
            ICollection<Dtos.Student.PilotAcademicHistoryLevel> pilotAcademicHistoryLevelDto = new List<Dtos.Student.PilotAcademicHistoryLevel>();
            // If the person requesting the information has permission to
            // view all student information.
            if (HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {
                AcademicCreditDataSubset reads = AcademicCreditDataSubset.StudentCourseSec | AcademicCreditDataSubset.StudentEquivEvals; // Bitwise OR on these values gives us a value that has both bits set.

                //logger.Error("GetPilotAcademicHistoryLevelByIdsAsync_AcadCredRead_START: " + DateTime.Now.ToString());
                //getStcs.Start();
                logger.Error("Calling GetPilotAcademicCreditsByStudentIdsAsync from GetPilotAcademicHistoryLevelByIdsAsync");
                Dictionary<string, List<PilotAcademicCredit>> studentAcadCreds = await _academicCreditRepository.GetPilotAcademicCreditsByStudentIdsAsync(studentIds.ToList(), reads, bestFit, filter);
                //getStcs.Stop();
                //logger.Error("GetPilotAcademicHistoryLevelByIdsAsync_AcadeCredRead_END: " + DateTime.Now.ToString());

                bool error = false;
                IEnumerable<Section> sections = new List<Section>();
                // Determine if we need to use census date checking for first term enrolled
                //logger.Error("GetPilotAcademicHistoryLevelByIdsAsync_CensusBoolRead_START: " + DateTime.Now.ToString()); 
                //pipaRead.Start();
                bool useCensusDate = await _academicCreditRepository.GetPilotCensusBooleanAsync();
                //pipaRead.Stop();
                //logger.Error("GetPilotAcademicHistoryLevelByIdsAsync_CensusBoolRead_END: " + DateTime.Now.ToString());
                logger.Error("Census:" + useCensusDate);
                HashSet<string> sectionIds = new HashSet<string>();
                foreach (var student in studentAcadCreds)
                {
                    foreach (var credit in student.Value)
                    {
                        if (credit.AcademicLevelCode != null && credit.SectionId != null)
                            sectionIds.Add(credit.SectionId);
                        if (credit.TermCode != null && termData == null)
                        {
                            //logger.Error("TermRead_START");
                            //termsReads.Start();
                            termData = await _termRepository.GetAsync(); // First time we see a credit with a term, get term data.                           
                            //termsReads.Stop();
                            //logger.Error("TermRead_END");
                        }
                    }
                }
                if (useCensusDate && sectionIds.Count() != 0) // only need to read sections if we're doing census date checking while determining first term enrolled
                {
                    //logger.Error("SectionsRead_START");
                    sectionsReads.Start();
                    sections = await _sectionRepository.GetCachedSectionsAsync(sectionIds);
                    sectionsReads.Stop();
                    //logger.Error("SectionsRead_END");
                }
                //logger.Error("GetPilotAcademicHistoryLevelByIdsAsync_StudentIteration_START: " + DateTime.Now.ToString());
                //studentIterationTimer.Start();
                foreach (var studentId in studentIds)
                {
                    try
                    {
                        // It's possible that the student didn't have any Academic Credits
                        // and therefore will not be in the list.
                        if (studentAcadCreds.ContainsKey(studentId))
                        {
                            List<PilotAcademicCredit> studentAcademicCredits = studentAcadCreds[studentId];
                            if (studentAcadCreds != null && studentAcadCreds.Count > 0)
                            {
                                if (studentAcademicCredits != null && studentAcademicCredits.Count() > 0)
                                {
                                    // Get term data from credits.  We'll need term start dates to determine FTE.
                                    var termCredits = studentAcademicCredits.Where(c => c.TermCode != null);
                                    var termIds = (from credit in termCredits select credit.TermCode).Distinct();
                                    // Loop through credits for each academic level
                                    var levels = (from credit in studentAcademicCredits select credit.AcademicLevelCode).Distinct();
                                    foreach (var level in levels)
                                    {
                                        var credits = studentAcademicCredits.Where(c => c.AcademicLevelCode != null && c.AcademicLevelCode == level);
                                        //logger.Error("GetPilotAcademicHistoryLevelByIdsAsync_GetFte_START: " + DateTime.Now.ToString());
                                        //getFteTimer.Start();
                                        string firstTermEnrolled = await GetFirstTermEnrolledAsync(termData, credits, sections, useCensusDate);
                                        //getFteTimer.Stop();
                                        //logger.Error("GetPilotAcademicHistoryLevelByIdsAsync_GetFte_END: " + DateTime.Now.ToString());
                                        var studentHistory = new PilotAcademicHistory(credits, new GradeRestriction(false), firstTermEnrolled);
                                        if (string.IsNullOrEmpty(studentHistory.StudentId))
                                        {
                                            studentHistory.StudentId = studentId;
                                        }
                                        // Filter to return only one specific term of data if we have a term filter set
                                        if (!string.IsNullOrEmpty(term))
                                        {
                                            studentHistory.FilterTerm(term);
                                        }

                                        if (!string.IsNullOrEmpty(studentId))
                                        {
                                            // Add Logger Messages if we can't build AcademicHistory or the DTO
                                            try
                                            {
                                                var studentHistoryLevel = new PilotAcademicHistoryLevel(level, studentHistory, studentId);
                                                // Get the right adapter for the type mapping
                                                var pilotAcademicHistoryLevelDtoAdapter = _adapterRegistry.GetAdapter<PilotAcademicHistoryLevel, Ellucian.Colleague.Dtos.Student.PilotAcademicHistoryLevel>();

                                                // Map the AcademicHistory entity to the AcademicHistory DTO
                                                var historyDto = pilotAcademicHistoryLevelDtoAdapter.MapToType(studentHistoryLevel);
                                                pilotAcademicHistoryLevelDto.Add(historyDto);
                                            }
                                            catch (Exception ex)
                                            {
                                                // Couldn't build the AcademicHistoryLevel or the DTO.
                                                var errorMessage = "Unable to build AcademicHistoryLevel for Student '" + studentId + "', Level '" + level + "'. exception thrown: " + ex.Message;
                                                logger.Error(errorMessage);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(string.Format("Failed to build academic history levels for student {0}", studentId));
                        logger.Error(e.GetBaseException().Message);
                        logger.Error(e.GetBaseException().StackTrace);
                        error = true;
                    }
                }
                //studentIterationTimer.Stop();
                //logger.Error("GetPilotAcademicHistoryLevelByIdsAsync_StudentIteration_END: " + DateTime.Now.ToString());
                if (error && pilotAcademicHistoryLevelDto.Count() == 0)
                    throw new Exception("Unexpected errors occurred.  No academic history level records returned.  Check API error log.");
            }
            else
            {
                // Person doesn't have View Student Information permissions. Throw Permission exception.
                throw new PermissionsException("User does not have permissions to access the student academic history.");
            }

            completeTime.Stop();

            //logger.Error("GetPilotAcademicHistoryLevelByIdsAsync_END: " + DateTime.Now.ToString());
            logger.Error("CompleteTime      :" + completeTime.ElapsedMilliseconds);
            //logger.Error("Get STCs Time     : " + getStcs.ElapsedMilliseconds);            
            //logger.Error("PIPA Read Time    : " + pipaRead.ElapsedMilliseconds);
            //logger.Error("Terms Read Time   : " + termsReads.ElapsedMilliseconds);
            logger.Error("Sections Read Time: " + sectionsReads.ElapsedMilliseconds);
            //logger.Error("FTE time:" + fteTime.ElapsedMilliseconds);
            //logger.Error("FTE reads:" + fteReads.ElapsedMilliseconds);
            //logger.Error("EffectiveCensusDates:" + effectiveCensusDatesTimer.ElapsedMilliseconds);
            //logger.Error("CensusDateCheck:" + censusDateCheckTimer.ElapsedMilliseconds);
            //logger.Error("SectionFiltering:" + sectionFilterTimer.ElapsedMilliseconds);
            //logger.Error("singleSectionTimer:" + singleSectionTimer.ElapsedMilliseconds);
            //logger.Error("studentIterationTimer:" + studentIterationTimer.ElapsedMilliseconds);
            //logger.Error("GetFteTimer:" + getFteTimer.ElapsedMilliseconds);


            return pilotAcademicHistoryLevelDto;
        }

        /// <summary>
        /// Gets academic histories for a list of students but only builds the academic credits to have the fields needed for Pilot's student-section entity.
        /// </summary>
        /// <param name="studentIds">A collection of student ids</param>
        /// <param name="bestFit">Whether to fit non-term credits into the nearest term</param>
        /// <param name="filter">Whether to filter out unknown credit types</param>
        /// <param name="term">Term to filter returned data</param>
        /// <returns></returns>
        public async Task<IEnumerable<Dtos.Student.PilotAcademicHistoryLevel>> GetPilotStudentSectionsAsync(IEnumerable<string> studentIds, bool bestFit, bool filter, string term = null)
        {
            ICollection<Dtos.Student.PilotAcademicHistoryLevel> pilotAcademicHistoryLevelDto = new List<Dtos.Student.PilotAcademicHistoryLevel>();
            // If the person requesting the information has permission to
            // view all student information.
            if (HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {
                // We call the academic credit repository in a way that only reads student acad cred and student course sec, and also adds the term to the database query to speed things up.
                logger.Error("Calling GetPilotAcademicCreditsByStudentIdsAsync from GetPilotStudentSectionsAsync");
                Dictionary<string, List<PilotAcademicCredit>> studentAcadCreds = await _academicCreditRepository.GetPilotAcademicCreditsByStudentIdsAsync(studentIds.ToList(), AcademicCreditDataSubset.StudentCourseSec, bestFit, filter, term);
                bool error = false;
                foreach (var studentId in studentIds)
                {
                    try
                    {
                        // It's possible that the student didn't have any Academic Credits
                        // and therefore will not be in the list.
                        if (studentAcadCreds.ContainsKey(studentId))
                        {
                            List<PilotAcademicCredit> studentAcademicCredits = studentAcadCreds[studentId];
                            if (studentAcadCreds != null && studentAcadCreds.Count > 0)
                            {
                                if (studentAcademicCredits != null && studentAcademicCredits.Count() > 0)
                                {
                                    var levels = (from credit in studentAcademicCredits select credit.AcademicLevelCode).Distinct();
                                    foreach (var level in levels)
                                    {
                                        var credits = studentAcademicCredits.Where(c => c.AcademicLevelCode != null && c.AcademicLevelCode == level);
                                        var studentHistory = new PilotAcademicHistory(credits, new GradeRestriction(false), string.Empty);
                                        if (string.IsNullOrEmpty(studentHistory.StudentId))
                                        {
                                            studentHistory.StudentId = studentId;
                                        }
                                        // Filter to return only one specific term of data if we have a term filter set
                                        if (!string.IsNullOrEmpty(term))
                                        {
                                            studentHistory.FilterTerm(term);
                                        }

                                        if (!string.IsNullOrEmpty(studentId))
                                        {
                                            // Add Logger Messages if we can't build AcademicHistory or the DTO
                                            try
                                            {
                                                var studentHistoryLevel = new PilotAcademicHistoryLevel(level, studentHistory, studentId);
                                                // Get the right adapter for the type mapping
                                                var pilotAcademicHistoryLevelDtoAdapter = _adapterRegistry.GetAdapter<PilotAcademicHistoryLevel, Ellucian.Colleague.Dtos.Student.PilotAcademicHistoryLevel>();

                                                // Map the AcademicHistory entity to the AcademicHistory DTO
                                                var historyDto = pilotAcademicHistoryLevelDtoAdapter.MapToType(studentHistoryLevel);

                                                // Get student sections
                                                var studentSections = new List<Dtos.Student.PilotStudentSection>();
                                                var sectionAdapter = _adapterRegistry.GetAdapter<PilotAcademicCredit, Ellucian.Colleague.Dtos.Student.PilotStudentSection>();
                                                var termCredits = credits.Where(c => c.TermCode != null && c.TermCode == term).ToList();
                                                foreach (var credit in termCredits)
                                                {
                                                    var studentSection = sectionAdapter.MapToType(credit);
                                                    studentSections.Add(studentSection);
                                                }
                                                historyDto.StudentSections = studentSections.Where(s => !string.IsNullOrWhiteSpace(s.Section)).ToList();
                                                pilotAcademicHistoryLevelDto.Add(historyDto);
                                            }
                                            catch (Exception ex)
                                            {
                                                // Couldn't build the AcademicHistoryLevel or the DTO.
                                                var errorMessage = "Unable to build AcademicHistoryLevel for Student '" + studentId + "', Level '" + level + "'. exception thrown: " + ex.Message;
                                                logger.Error(errorMessage);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(string.Format("Failed to build academic history levels for student {0}", studentId));
                        logger.Error(e.GetBaseException().Message);
                        logger.Error(e.GetBaseException().StackTrace);
                        error = true;
                    }
                }
                if (error && pilotAcademicHistoryLevelDto.Count() == 0)
                    throw new Exception("Unexpected errors occurred.  No academic history level records returned.  Check API error log.");
            }
            else
            {
                // Person doesn't have View Student Information permissions. Throw Permission exception.
                throw new PermissionsException("User does not have permissions to access the student academic history.");
            }

            return pilotAcademicHistoryLevelDto;
        }

        /// <summary>
        /// For incoming credits filtered by academic level, determine the firstTermEnrolled.
        /// </summary>
        /// <param name="termData">Data for all terms</param>
        /// <param name="credits">Student academic credit filtered by academic level</param>
        /// <param name="sections">Data for all sections for the student's credits</param>
        /// <param name="useCensusDate">Boolean to use census date for checking active registration</param>
        /// <returns></returns>
        private async Task<string> GetFirstTermEnrolledAsync(IEnumerable<Term> termData, IEnumerable<AcademicCredit> credits, IEnumerable<Section> sections, bool useCensusDate)
        {
            Term termFirstEnrolled = null;
            if (credits != null)
            {
                if (useCensusDate == false)
                {
                    // Not using census date.  Use only active credit given most recent credit status.
                    var activeCredits = credits.Where(c => (c.Status == CreditStatus.Add || c.Status == CreditStatus.New));
                    foreach (var credit in activeCredits)
                    {
                        termFirstEnrolled = GetEarliestTermEnrolled(termData, credit, termFirstEnrolled);
                    }
                }
                else
                {
                    // Filter academic credits to those with sections.
                    var sectionCredits = credits.Where(c => c.SectionId != null && c.SectionId != string.Empty);
                    if (sectionCredits != null && sectionCredits.Count() > 0)
                    {
                        // Build a fixed list of section Ids for this student/level.
                        var sectionIds = (from credit in sectionCredits select credit.SectionId).Distinct().ToList();
                        // Build sectionEntities to contain data for just sections for this student/level, 
                        // and make it acccessible by section IDs.
                        Dictionary<string, Section> sectionEntities = new Dictionary<string, Section>();
                        foreach (var section in sections.Where(s => sectionIds.Contains(s.Id)).Distinct())
                            sectionEntities.Add(section.Id, section);
                        foreach (var credit in credits)
                        {
                            if (!string.IsNullOrWhiteSpace(credit.TermCode))
                            {
                                // Extract data for this credit's section by it section ID
                                var sectionEntity = (credit.SectionId != null && sectionEntities.Keys.Contains(credit.SectionId)) ? sectionEntities[credit.SectionId] : null;
                                if (sectionEntity != null)
                                {
                                    // Set effective census date from section, otherwise term location, otherwise term.
                                    DateTime? effectiveCensusDate = null;
                                    var sectionCensusDate = sectionEntity.RegistrationDateOverrides.CensusDates.FirstOrDefault();
                                    if (sectionCensusDate != null)
                                    {
                                        effectiveCensusDate = sectionCensusDate;
                                    }
                                    else
                                    {
                                        // For this credit, get Term location census date and if necessary Term census date.
                                        var termEntity = termData.Where(t => t.Code == credit.TermCode).FirstOrDefault();
                                        if (!string.IsNullOrWhiteSpace(credit.Location))
                                        {
                                            var termLocationRegistrationDates = termEntity.RegistrationDates.Where(l => l.Location == credit.Location).FirstOrDefault();
                                            if (termLocationRegistrationDates != null)
                                            {
                                                effectiveCensusDate = termLocationRegistrationDates.CensusDates.FirstOrDefault();
                                            }
                                        }
                                        if (effectiveCensusDate == null)
                                        {
                                            var termRegistrationDates = termEntity.RegistrationDates.Where(l => l.Location == "").FirstOrDefault();
                                            if (termRegistrationDates != null)
                                            {
                                                effectiveCensusDate = termRegistrationDates.CensusDates.FirstOrDefault();
                                            }
                                        }
                                    }
                                    bool passCensusCheck = false;
                                    if (effectiveCensusDate != null)
                                    {
                                        // Census date found.  Find the most recent status before (or on) the census date.
                                        // If it's active, pass the census check.
                                        if (credit.AcademicCreditStatuses != null)
                                        {
                                            foreach (var st in credit.AcademicCreditStatuses)
                                            {
                                                var status = st.Status;
                                                var statusType = await _academicCreditRepository.ConvertCreditStatusAsync(status);
                                                var statusDate = st.Date;
                                                if (statusDate <= effectiveCensusDate)
                                                {
                                                    if (statusType == CreditStatus.Add || statusType == CreditStatus.New)
                                                    {
                                                        passCensusCheck = true;
                                                    }
                                                    // We only care about the most recent status found
                                                    // before the census date.  So break once we find one.
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // No census date found.  Just make sure the most recent status is active.
                                        if (credit.Status == CreditStatus.Add || credit.Status == CreditStatus.New)
                                        {
                                            passCensusCheck = true;
                                        }
                                    }
                                    if (passCensusCheck == true)
                                    {
                                        if (credit.TermCode != null)
                                        {
                                            termFirstEnrolled = GetEarliestTermEnrolled(termData, credit, termFirstEnrolled);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            var firstTermEnrolled = "";
            if (termFirstEnrolled != null)
            {
                firstTermEnrolled = termFirstEnrolled.Code;
            }

            return firstTermEnrolled;
        }

        /// <summary>
        /// For incoming credits filtered by academic level, determine the firstTermEnrolled.
        /// </summary>
        /// <param name="termData">Data for all terms</param>
        /// <param name="credits">Student academic credit filtered by academic level</param>
        /// <param name="sections">Data for all sections for the student's credits</param>
        /// <param name="useCensusDate">Boolean to use census date for checking active registration</param>
        /// <returns></returns>
        private async Task<string> GetFirstTermEnrolledAsync(IEnumerable<Term> termData, IEnumerable<PilotAcademicCredit> credits, IEnumerable<Section> sections, bool useCensusDate)
        {
            Term termFirstEnrolled = null;
            if (credits != null)
            {
                if (useCensusDate == false)
                {
                    // Not using census date.  Use only active credit given most recent credit status.
                    var activeCredits = credits.Where(c => (c.Status == CreditStatus.Add || c.Status == CreditStatus.New));
                    foreach (var credit in activeCredits)
                    {
                        termFirstEnrolled = GetEarliestTermEnrolled(termData, credit, termFirstEnrolled);
                    }
                }
                else
                {
                    // Filter academic credits to those with sections.
                    var sectionCredits = credits.Where(c => c.SectionId != null && c.SectionId != string.Empty);
                    if (sectionCredits != null && sectionCredits.Count() > 0)
                    {
                        // Build a fixed list of section Ids for this student/level.
                        var sectionIds = (from credit in sectionCredits select credit.SectionId).Distinct().ToList();
                        // Build sectionEntities to contain data for just sections for this student/level, 
                        // and make it acccessible by section IDs.
                        Dictionary<string, Section> sectionEntities = new Dictionary<string, Section>();
                        foreach (var section in sections.Where(s => sectionIds.Contains(s.Id)).Distinct())
                            sectionEntities.Add(section.Id, section);
                        foreach (var credit in credits)
                        {
                            if (!string.IsNullOrWhiteSpace(credit.TermCode))
                            {
                                // Extract data for this credit's section by it section ID
                                var sectionEntity = (credit.SectionId != null && sectionEntities.Keys.Contains(credit.SectionId)) ? sectionEntities[credit.SectionId] : null;
                                if (sectionEntity != null)
                                {
                                    // Set effective census date from section, otherwise term location, otherwise term.
                                    DateTime? effectiveCensusDate = null;
                                    var sectionCensusDate = sectionEntity.RegistrationDateOverrides.CensusDates.FirstOrDefault();
                                    if (sectionCensusDate != null)
                                    {
                                        effectiveCensusDate = sectionCensusDate;
                                    }
                                    else
                                    {
                                        // For this credit, get Term location census date and if necessary Term census date.
                                        var termEntity = termData.Where(t => t.Code == credit.TermCode).FirstOrDefault();
                                        if (!string.IsNullOrWhiteSpace(credit.Location))
                                        {
                                            var termLocationRegistrationDates = termEntity.RegistrationDates.Where(l => l.Location == credit.Location).FirstOrDefault();
                                            if (termLocationRegistrationDates != null)
                                            {
                                                effectiveCensusDate = termLocationRegistrationDates.CensusDates.FirstOrDefault();
                                            }
                                        }
                                        if (effectiveCensusDate == null)
                                        {
                                            var termRegistrationDates = termEntity.RegistrationDates.Where(l => l.Location == "").FirstOrDefault();
                                            if (termRegistrationDates != null)
                                            {
                                                effectiveCensusDate = termRegistrationDates.CensusDates.FirstOrDefault();
                                            }
                                        }
                                    }
                                    bool passCensusCheck = false;
                                    if (effectiveCensusDate != null)
                                    {
                                        // Census date found.  Find the most recent status before (or on) the census date.
                                        // If it's active, pass the census check.
                                        if (credit.AcademicCreditStatuses != null)
                                        {
                                            foreach (var st in credit.AcademicCreditStatuses)
                                            {
                                                var status = st.Status;
                                                var statusType = await _academicCreditRepository.ConvertCreditStatusAsync(status);
                                                var statusDate = st.Date;
                                                if (statusDate <= effectiveCensusDate)
                                                {
                                                    if (statusType == CreditStatus.Add || statusType == CreditStatus.New)
                                                    {
                                                        passCensusCheck = true;
                                                    }
                                                    // We only care about the most recent status found
                                                    // before the census date.  So break once we find one.
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // No census date found.  Just make sure the most recent status is active.
                                        if (credit.Status == CreditStatus.Add || credit.Status == CreditStatus.New)
                                        {
                                            passCensusCheck = true;
                                        }
                                    }
                                    if (passCensusCheck == true)
                                    {
                                        if (credit.TermCode != null)
                                        {
                                            termFirstEnrolled = GetEarliestTermEnrolled(termData, credit, termFirstEnrolled);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            var firstTermEnrolled = "";
            if (termFirstEnrolled != null)
            {
                firstTermEnrolled = termFirstEnrolled.Code;
            }

            return firstTermEnrolled;
        }

        /// <summary>
        /// termFirstEnrolled is an object containing the earliest term for which student has registration.
        /// For an incoming credit, check if its term is earlier.  If so, replace termFirstEnrolled.
        /// </summary>
        /// <param name="termData">All terms' data</param>
        /// <param name="credit">A single student academic credit</param>
        /// <param name="termFirstEnrolled">Term data for first term enrolled</param>
        /// <returns></returns>
        private Term GetEarliestTermEnrolled(IEnumerable<Term> termData, AcademicCredit credit, Term termFirstEnrolled)
        {
            //  Set termFirstEnrolled to for earlier term start date.

            if (credit.TermCode != null && credit.TermCode != "")
            {
                var termEntity = termData.Where(t => t.Code == credit.TermCode).FirstOrDefault();
                if (termFirstEnrolled == null)
                {
                    termFirstEnrolled = termEntity;
                }
                else
                {
                    if (termFirstEnrolled.Code != credit.TermCode)
                    {
                        // if term for this credit has earlier start date, replace the first enrolled term
                        if (termEntity.StartDate < termFirstEnrolled.StartDate)
                        {
                            termFirstEnrolled = termEntity;
                        }
                    }
                }
            }
            return termFirstEnrolled;
        }

        /// <summary>
        /// termFirstEnrolled is an object containing the earliest term for which student has registration.
        /// For an incoming credit, check if its term is earlier.  If so, replace termFirstEnrolled.
        /// </summary>
        /// <param name="termData">All terms' data</param>
        /// <param name="credit">A single student academic credit</param>
        /// <param name="termFirstEnrolled">Term data for first term enrolled</param>
        /// <returns></returns>
        private Term GetEarliestTermEnrolled(IEnumerable<Term> termData, PilotAcademicCredit credit, Term termFirstEnrolled)
        {
            //  Set termFirstEnrolled to for earlier term start date.

            if (credit.TermCode != null && credit.TermCode != "")
            {
                var termEntity = termData.Where(t => t.Code == credit.TermCode).FirstOrDefault();
                if (termFirstEnrolled == null)
                {
                    termFirstEnrolled = termEntity;
                }
                else
                {
                    if (termFirstEnrolled.Code != credit.TermCode)
                    {
                        // if term for this credit has earlier start date, replace the first enrolled term
                        if (termEntity.StartDate < termFirstEnrolled.StartDate)
                        {
                            termFirstEnrolled = termEntity;
                        }
                    }
                }
            }
            return termFirstEnrolled;
        }

        public async Task<Ellucian.Colleague.Dtos.Student.AcademicHistory> GetAcademicHistoryAsync(string studentId, bool bestFit = false, bool filter = true, string term = null)
        {
            Ellucian.Colleague.Domain.Student.Entities.Student student = await _studentRepository.GetAsync(studentId);
            GradeRestriction studentGradeRestriction = await _studentRepository.GetGradeRestrictionsAsync(studentId);

            var studentAcademicCredits = await _academicCreditRepository.GetAsync(student.AcademicCreditIds, bestFit, filter);

            AcademicHistory studentHistory = null;
            if (studentId == CurrentUser.PersonId)
            {
                // Use a domain service to construct the student academic history - complete with term GPAs
                studentHistory = new AcademicHistory(studentAcademicCredits, studentGradeRestriction, null);
                if (string.IsNullOrEmpty(studentHistory.StudentId))
                {
                    studentHistory.StudentId = studentId;
                }
            }
            else
            {
                // If if the person requesting the information is the student's advisor, or an advisor with permission to
                // view all students.
                if ((await UserIsAdvisorAsync(student.Id, student == null ? null : student.ConvertToStudentAccess())) || HasPermission(StudentPermissionCodes.ViewStudentInformation))
                {
                    // Always build history without accounting for the grade restriction.
                    studentHistory = new AcademicHistory(studentAcademicCredits, new GradeRestriction(false), null);
                    // Put the real student's grade restriction back into the history
                    studentHistory.GradeRestriction = studentGradeRestriction;
                    if (string.IsNullOrEmpty(studentHistory.StudentId))
                    {
                        studentHistory.StudentId = studentId;
                    }
                }
                else
                {
                    // Person asking isn't student and isn't a valid advisor. Throw Permission exception.
                    throw new PermissionsException("User does not have permissions to access to this student academic history.");
                }
            }
            // Filter to return only one specific term of data if we have a term filter set
            if (!string.IsNullOrEmpty(term))
            {
                studentHistory.FilterTerm(term);
            }

            // Get the right adapter for the type mapping
            var academicHistoryDtoAdapter = _adapterRegistry.GetAdapter<AcademicHistory, Ellucian.Colleague.Dtos.Student.AcademicHistory>();

            // Map the AcademicHistory entity to the AcademicHistory DTO
            var historyDto = academicHistoryDtoAdapter.MapToType(studentHistory);

            return historyDto;
        }

        /// <summary>
        /// Returns academic history of the given student
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <param name="bestFit">If true, places all academic credits in a term</param>
        /// <returns><see cref="AcademicHistory2">Academic History</see> for the student.</returns>
        public async Task<Ellucian.Colleague.Dtos.Student.AcademicHistory2> GetAcademicHistory2Async(string studentId, bool bestFit = false, bool filter = true, string term = null)
        {
            Ellucian.Colleague.Domain.Student.Entities.Student student = await _studentRepository.GetAsync(studentId);

            var studentAcademicCredits = await _academicCreditRepository.GetAsync(student.AcademicCreditIds, bestFit);

            var historyDto = await ConvertAcademicCreditsToAcademicHistoryDtoAsync(studentId, studentAcademicCredits, student);

            return historyDto;
        }

        /// <summary>
        /// Returns academic history of the given student
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <param name="bestFit">If true, places all academic credits in a term</param>
        /// <param name="filter">If true, filter academic credits for certain statuses</param>
        /// <param name="term">(Optional) used to return only a specific term of data.</param>
        /// <returns><see cref="AcademicHistory3">Academic History</see> for the student.</returns>
        [Obsolete("Obsolete as of API 1.18. Use GetAcademicHistory4Async.")]
        public async Task<Ellucian.Colleague.Dtos.Student.AcademicHistory3> GetAcademicHistory3Async(string studentId, bool bestFit = false, bool filter = true, string term = null)
        {
            Ellucian.Colleague.Domain.Student.Entities.Student student = await _studentRepository.GetAsync(studentId);

            var studentAcademicCredits = await _academicCreditRepository.GetAsync(student.AcademicCreditIds, bestFit);

            var historyDto = await ConvertAcademicCreditsToAcademicHistoryDto2Async(studentId, studentAcademicCredits, student);

            return historyDto;
        }

        /// <summary>
        /// Returns academic history of the given student
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <param name="bestFit">If true, places all academic credits in a term</param>
        /// <param name="filter">If true, filter academic credits for certain statuses</param>
        /// <param name="term">(Optional) used to return only a specific term of data.</param>
        /// <param name="includeDrops">If true, include dropped academic credits</param>
        /// <returns><see cref="AcademicHistory4">Academic History</see> for the student.</returns>
        public async Task<Ellucian.Colleague.Dtos.Student.AcademicHistory4> GetAcademicHistory4Async(string studentId, bool bestFit = false, bool filter = true, string term = null, bool includeDrops = false)
        {
            Ellucian.Colleague.Domain.Student.Entities.Student student = await _studentRepository.GetAsync(studentId);

            var studentAcademicCredits = await _academicCreditRepository.GetAsync(student.AcademicCreditIds, bestFit, filter, includeDrops);

            var historyDto = await ConvertAcademicCreditsToAcademicHistoryDto4Async(studentId, studentAcademicCredits, student);

            return historyDto;
        }

        /// <summary>
        /// Given student and academic credits, builds academic history dto
        /// </summary>
        /// <param name="student">Student entity, if needed for checking permissions</param>
        /// <param name="studentAcademicCredits">List of academic credits for the student</param>
        /// <returns></returns>
        public async Task<Dtos.Student.AcademicHistory2> ConvertAcademicCreditsToAcademicHistoryDtoAsync(string studentId, IEnumerable<AcademicCredit> studentAcademicCredits, Domain.Student.Entities.Student student = null)
        {
            GradeRestriction studentGradeRestriction = await _studentRepository.GetGradeRestrictionsAsync(studentId);

            AcademicHistory studentHistory = null;
            if (studentId == CurrentUser.PersonId)
            {
                // Use a domain service to construct the student academic history - complete with term GPAs
                studentHistory = new AcademicHistory(studentAcademicCredits, studentGradeRestriction, null);
            }
            else
            {
                // Since this user is not the student, determine if they have permission to get this information
                bool hasPermission = false;
                hasPermission = HasPermission(StudentPermissionCodes.ViewStudentInformation);
                if (!hasPermission)
                {
                    hasPermission = (await UserIsAdvisorAsync(studentId, student == null ? null : student.ConvertToStudentAccess()));
                }
                // If if the person requesting the information is the student's advisor, or an advisor with permission to
                // view all students.
                if (hasPermission)
                {
                    // Always build history without accounting for the grade restriction.
                    studentHistory = new AcademicHistory(studentAcademicCredits, new GradeRestriction(false), null);
                    // Put the real student's grade restriction back into the history
                    studentHistory.GradeRestriction = studentGradeRestriction;
                }
                else
                {
                    // Person asking isn't student and isn't a valid advisor. Throw Permission exception.
                    throw new PermissionsException("User does not have permissions to access to this student academic history.");
                }
            }

            // Get the right adapter for the type mapping
            var academicHistoryDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicHistory, Ellucian.Colleague.Dtos.Student.AcademicHistory2>();

            // Map the degree plan entity to the degree plan DTO
            var historyDto = academicHistoryDtoAdapter.MapToType(studentHistory);

            return historyDto;
        }

        /// <summary>
        /// Given student and academic credits, builds academic history dto
        /// </summary>
        /// <param name="student">Student entity, if needed for checking permissions</param>
        /// <param name="studentAcademicCredits">List of academic credits for the student</param>
        /// <returns></returns>
        public async Task<Dtos.Student.AcademicHistory3> ConvertAcademicCreditsToAcademicHistoryDto2Async(string studentId, IEnumerable<AcademicCredit> studentAcademicCredits, Domain.Student.Entities.Student student = null)
        {
            GradeRestriction studentGradeRestriction = await _studentRepository.GetGradeRestrictionsAsync(studentId);

            AcademicHistory studentHistory = null;
            if (UserIsSelf(studentId) || HasProxyAccessForPerson(studentId))
            {
                // Use a domain service to construct the student academic history - complete with term GPAs
                studentHistory = new AcademicHistory(studentAcademicCredits, studentGradeRestriction, null);
            }
            else
            {
                // Since this user is not the student, determine if they have permission to get this information
                bool hasPermission = false;
                hasPermission = HasPermission(StudentPermissionCodes.ViewStudentInformation);
                if (!hasPermission)
                {
                    hasPermission = (await UserIsAdvisorAsync(studentId, student == null ? null : student.ConvertToStudentAccess()));
                }
                // If if the person requesting the information is the student's advisor, or an advisor with permission to
                // view all students.
                if (hasPermission)
                {
                    // Always build history without accounting for the grade restriction.
                    studentHistory = new AcademicHistory(studentAcademicCredits, new GradeRestriction(false), null);
                    // Put the real student's grade restriction back into the history
                    studentHistory.GradeRestriction = studentGradeRestriction;
                }
                else
                {
                    // Person asking isn't student and isn't a valid advisor. Throw Permission exception.
                    throw new PermissionsException("User does not have permissions to access to this student academic history.");
                }
            }

            // Get the right adapter for the type mapping
            var academicHistoryDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicHistory, Ellucian.Colleague.Dtos.Student.AcademicHistory3>();

            // Map the degree plan entity to the degree plan DTO
            var historyDto = academicHistoryDtoAdapter.MapToType(studentHistory);

            return historyDto;
        }

        /// <summary>
        /// Given student and academic credits, builds academic history dto
        /// </summary>
        /// <param name="student">Student entity, if needed for checking permissions</param>
        /// <param name="studentAcademicCredits">List of academic credits for the student</param>
        /// <returns></returns>
        public async Task<Dtos.Student.AcademicHistory4> ConvertAcademicCreditsToAcademicHistoryDto4Async(string studentId, IEnumerable<AcademicCredit> studentAcademicCredits, Domain.Student.Entities.Student student = null)
        {
            GradeRestriction studentGradeRestriction = await _studentRepository.GetGradeRestrictionsAsync(studentId);

            AcademicHistory studentHistory = null;
            if (UserIsSelf(studentId) || HasProxyAccessForPerson(studentId))
            {
                // Use a domain service to construct the student academic history - complete with term GPAs
                studentHistory = new AcademicHistory(studentAcademicCredits, studentGradeRestriction, null);
            }
            else
            {
                // Since this user is not the student, determine if they have permission to get this information
                bool hasPermission = false;
                hasPermission = HasPermission(StudentPermissionCodes.ViewStudentInformation);
                if (!hasPermission)
                {
                    hasPermission = (await UserIsAdvisorAsync(studentId, student == null ? null : student.ConvertToStudentAccess()));
                }
                // If if the person requesting the information is the student's advisor, or an advisor with permission to
                // view all students.
                if (hasPermission)
                {
                    // Always build history without accounting for the grade restriction.
                    studentHistory = new AcademicHistory(studentAcademicCredits, new GradeRestriction(false), null);
                    // Put the real student's grade restriction back into the history
                    studentHistory.GradeRestriction = studentGradeRestriction;
                }
                else
                {
                    // Person asking isn't student and isn't a valid advisor. Throw Permission exception.
                    throw new PermissionsException("User does not have permissions to access to this student academic history.");
                }
            }

            // Get the right adapter for the type mapping
            var academicHistoryDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicHistory, Ellucian.Colleague.Dtos.Student.AcademicHistory4>();

            // Map the degree plan entity to the degree plan DTO
            var historyDto = academicHistoryDtoAdapter.MapToType(studentHistory);

            return historyDto;
        }


        public async Task<IEnumerable<Dtos.Student.StudentEnrollment>> GetInvalidStudentEnrollmentAsync(IEnumerable<Dtos.Student.StudentEnrollment> enrollmentKeys)
        {
            var studentEnrollment = new List<Dtos.Student.StudentEnrollment>();
            if (HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {
                if (enrollmentKeys != null && enrollmentKeys.Count() > 0)
                {
                    var studentIds = enrollmentKeys.Select(k => k.StudentId).Distinct().ToList();
                    Dictionary<string, List<AcademicCredit>> studentAcadCreds = await _academicCreditRepository.GetAcademicCreditByStudentIdsAsync(studentIds, true, false);
                    if (studentAcadCreds != null)
                    {
                        foreach (var key in enrollmentKeys)
                        {
                            var studentId = key.StudentId;
                            var sectionId = key.SectionId;

                            if (!string.IsNullOrEmpty(studentId) && !string.IsNullOrEmpty(sectionId))
                            {
                                // It's possible that the student didn't have any Academic Credits
                                // and therefore will not be in the list.
                                if (studentAcadCreds.ContainsKey(studentId))
                                {
                                    List<AcademicCredit> studentAcademicCredits = studentAcadCreds[studentId];

                                    if (studentAcademicCredits != null && studentAcademicCredits.Count > 0)
                                    {
                                        // check to see if the student is still enrolled in the section.
                                        var credit = studentAcademicCredits.Where(ac => ac.SectionId == sectionId).FirstOrDefault();
                                        if (credit == null)
                                        {
                                            // If the Student is no longer in the section then return the StudentEnrollment DTO.
                                            studentEnrollment.Add(key);
                                        }
                                    }
                                }
                                else
                                {
                                    // This enrollment is invalid for this student.
                                    studentEnrollment.Add(key);
                                }
                            }
                            else
                            {
                                var message = "Student Id and/or Section Id is missing from the StudentEnrollment object.";
                                logger.Warn(message);
                            }
                        }
                    }
                }
            }
            else
            {
                // Person doesn't have View Student Information Permissions. Throw Permission exception.
                throw new PermissionsException("User does not have permissions to access the student academic history.");
            }
            return studentEnrollment;
        }

        /// <summary>
        /// Returns a list of academic credits for the specified section Ids in the criteria
        /// </summary>
        /// <param name="criteria">Criteria that contains a list of sections and some other options</param>
        /// <returns>List of <see cref="AcademicCredit2">AcademicCredit3</see> Dtos</returns>
        [Obsolete("Obsolete as of API 1.18. Use QueryAcademicCredits2Async.")]
        public async Task<IEnumerable<Dtos.Student.AcademicCredit2>> QueryAcademicCreditsAsync(Dtos.Student.AcademicCreditQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Must supply a criteria to query academic credits.");
            }
            if (criteria.SectionIds == null || !criteria.SectionIds.Any())
            {
                throw new ArgumentException("Must supply at least 1 section to query academic credits.");
            }
            IEnumerable<string> sectionIds = criteria.SectionIds.Distinct().ToList();
            List<Dtos.Student.AcademicCredit2> academicCreditDtos = new List<Dtos.Student.AcademicCredit2>();
            // Only include any sections for which the requestor is an assigned faculty.  There none return none instead of a permission exception.
            var sections = (await _sectionRepository.GetCachedSectionsAsync(sectionIds, false));
            if (sections != null && sections.Any())
            {
                string requestor = CurrentUser.PersonId;
                List<string> querySectionIds = new List<string>();

                // Determine the actual list of section Ids that should be used for the query. Make sure the requestor has access to see credits for the sections
                // AND add in any cross listed sections if requested.
                foreach (var section in sections)
                {
                    // Only assigned faculty of a section can get grade information for a section
                    if (section.FacultyIds.Contains(requestor))
                    {
                        querySectionIds.Add(section.Id);
                        // Add in any crosslisted section Ids if criteria requests them.
                        if (criteria.IncludeCrossListedCredits)
                        {
                            var crossListedSectionIds = section.CrossListedSections.Select(x => x.Id);
                            querySectionIds.AddRange(crossListedSectionIds);
                        }
                    }
                }
                if (querySectionIds.Any())
                {
                    try
                    {
                        // Get all academic credits for a section (all statuses). They will be filtered later.
                        var academicCredits = await _academicCreditRepository.GetAcademicCreditsBySectionIdsAsync(querySectionIds.Distinct().ToList());


                        var academicCreditAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.AcademicCredit, Dtos.Student.AcademicCredit2>();
                        foreach (var credit in academicCredits)
                        {
                            academicCreditDtos.Add(academicCreditAdapter.MapToType(credit));
                        }
                        // Now that we have the list of DTOs we can limit the results by te DTO Credit Statuses if applicable
                        if (criteria.CreditStatuses != null && criteria.CreditStatuses.Any())
                        {
                            // Reduce the results to just those of the proper type
                            academicCreditDtos = (from creditStatus in criteria.CreditStatuses
                                                   join acadCredit in academicCreditDtos
                                                   on creditStatus.ToString() equals acadCredit.Status into joinCreditAndStatuses
                                                   from credit in joinCreditAndStatuses
                                                   select credit).ToList();

                        }

                        return academicCreditDtos;
                    }
                    catch (Exception ex)
                    {
                        // Couldn't retrieve the desired academic credits or convert them to DTOs.
                        var errorMessage = "Unable to retrieve academic credits for the requested sections: " + "Exception thrown: " + ex.Message;
                        logger.Error(errorMessage);
                        throw;
                    }
                }

            }
            return academicCreditDtos;
        }

        /// <summary>
        /// Returns a list of academic credits for the specified section Ids in the criteria
        /// </summary>
        /// <param name="criteria">Criteria that contains a list of sections and some other options</param>
        /// <returns>List of <see cref="AcademicCredit3">AcademicCredit3</see> Dtos</returns>
        public async Task<IEnumerable<Dtos.Student.AcademicCredit3>> QueryAcademicCredits2Async(Dtos.Student.AcademicCreditQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Must supply a criteria to query academic credits.");
            }
            if (criteria.SectionIds == null || !criteria.SectionIds.Any())
            {
                throw new ArgumentException("Must supply at least 1 section to query academic credits.");
            }
            IEnumerable<string> sectionIds = criteria.SectionIds.Distinct().ToList();
            List<Dtos.Student.AcademicCredit3> academicCreditDtos = new List<Dtos.Student.AcademicCredit3>();
            // Only include any sections for which the requestor is an assigned faculty.  There none return none instead of a permission exception.
            var sections = (await _sectionRepository.GetCachedSectionsAsync(sectionIds, false));
            if (sections != null && sections.Any())
            {
                string requestor = CurrentUser.PersonId;
                List<string> querySectionIds = new List<string>();

                // Determine the actual list of section Ids that should be used for the query. Make sure the requestor has access to see credits for the sections
                // AND add in any cross listed sections if requested.
                foreach (var section in sections)
                {
                    // Only assigned faculty of a section can get grade information for a section
                    if (section.FacultyIds.Contains(requestor))
                    {
                        querySectionIds.Add(section.Id);
                        // Add in any crosslisted section Ids if criteria requests them.
                        if (criteria.IncludeCrossListedCredits)
                        {
                            var crossListedSectionIds = section.CrossListedSections.Select(x => x.Id);
                            querySectionIds.AddRange(crossListedSectionIds);
                        }
                    }
                }
                if (querySectionIds.Any())
                {
                    try
                    {
                        // Get all academic credits for these sections (all statuses). They will be filtered below.
                        var academicCredits = await _academicCreditRepository.GetAcademicCreditsBySectionIdsAsync(querySectionIds.Distinct().ToList());


                        var academicCreditAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.AcademicCredit, Dtos.Student.AcademicCredit3>();
                        foreach (var credit in academicCredits)
                        {
                            academicCreditDtos.Add(academicCreditAdapter.MapToType(credit));
                        }
                        // Now that we have the list of DTOs we can limit the results by te DTO Credit Statuses if applicable
                        if (criteria.CreditStatuses != null && criteria.CreditStatuses.Any())
                        {
                            // Reduce the results to just those of the proper type
                            academicCreditDtos = (from creditStatus in criteria.CreditStatuses
                                                  join acadCredit in academicCreditDtos
                                                  on creditStatus.ToString() equals acadCredit.Status into joinCreditAndStatuses
                                                  from credit in joinCreditAndStatuses
                                                  select credit).ToList();

                        }

                        return academicCreditDtos;
                    }
                    catch (Exception ex)
                    {
                        // Couldn't retrieve the desired academic credits or convert them to DTOs.
                        var errorMessage = "Unable to retrieve academic credits for the requested sections: " + "Exception thrown: " + ex.Message;
                        logger.Error(errorMessage);
                        throw;
                    }
                }

            }
            return academicCreditDtos;
        }

        /// <summary>
        /// Returns a list of academic credits records for the specified section Ids in the criteria
        /// Also returns list of invalid academic credits Ids that are missing from STUDENT.ACAD.CRED file.
        /// </summary>
        /// <param name="criteria">Criteria that contains a list of sections and some other options</param>
        /// <returns><see cref="AcademicCreditsWithInvalidKeys">AcademicCreditsWithInvalidKeys</see> Dtos</returns>
        public async Task<Dtos.Student.AcademicCreditsWithInvalidKeys> QueryAcademicCreditsWithInvalidKeysAsync(Dtos.Student.AcademicCreditQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Must supply a criteria to query academic credits.");
            }
            if (criteria.SectionIds == null || !criteria.SectionIds.Any())
            {
                throw new ArgumentException("Must supply at least 1 section to query academic credits.");
            }
            IEnumerable<string> sectionIds = criteria.SectionIds.Distinct().ToList();
            List<Dtos.Student.AcademicCredit3> academicCreditDtos = new List<Dtos.Student.AcademicCredit3>();
            List<string> invalidAcademicCredits = new List<string>();
            AcademicCreditsWithInvalidKeys academicCreditsWithInvalidKeys = new AcademicCreditsWithInvalidKeys(new List<Domain.Student.Entities.AcademicCredit>(), new List<string>());

            // Only include any sections for which the requestor is an assigned faculty.  There none return none instead of a permission exception.
            var sections = (await _sectionRepository.GetCachedSectionsAsync(sectionIds, false));
            if (sections != null && sections.Any())
            {
                string requestor = CurrentUser.PersonId;
                List<string> querySectionIds = new List<string>();

                // Determine the actual list of section Ids that should be used for the query. Make sure the requestor has access to see credits for the sections
                // AND add in any cross listed sections if requested.
                foreach (var section in sections)
                {
                    // Only assigned faculty of a section can get grade information for a section
                    if (section.FacultyIds.Contains(requestor))
                    {
                        querySectionIds.Add(section.Id);
                        // Add in any crosslisted section Ids if criteria requests them.
                        if (criteria.IncludeCrossListedCredits)
                        {
                            var crossListedSectionIds = section.CrossListedSections.Select(x => x.Id);
                            querySectionIds.AddRange(crossListedSectionIds);
                        }
                    }
                }
                if (querySectionIds.Any())
                {
                    try
                    {
                        // Get all academic credits for these sections (all statuses). They will be filtered below.
                         academicCreditsWithInvalidKeys = await _academicCreditRepository.GetAcademicCreditsBySectionIdsWithInvalidKeysAsync(querySectionIds.Distinct().ToList());


                        var academicCreditAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.AcademicCredit, Dtos.Student.AcademicCredit3>();
                        if (academicCreditsWithInvalidKeys != null)
                        {
                            if (academicCreditsWithInvalidKeys.AcademicCredits != null)
                            {
                                foreach (var credit in academicCreditsWithInvalidKeys.AcademicCredits)
                                {
                                    academicCreditDtos.Add(academicCreditAdapter.MapToType(credit));
                                }
                                // Now that we have the list of DTOs we can limit the results by te DTO Credit Statuses if applicable
                                if (criteria.CreditStatuses != null && criteria.CreditStatuses.Any())
                                {
                                    // Reduce the results to just those of the proper type
                                    academicCreditDtos = (from creditStatus in criteria.CreditStatuses
                                                          join acadCredit in academicCreditDtos
                                                          on creditStatus.ToString() equals acadCredit.Status into joinCreditAndStatuses
                                                          from credit in joinCreditAndStatuses
                                                          select credit).ToList();

                                }
                            }
                            if (academicCreditsWithInvalidKeys.InvalidAcademicCreditIds!=null)
                            {
                                invalidAcademicCredits = academicCreditsWithInvalidKeys.InvalidAcademicCreditIds.ToList();
                            }
                        }
                        return new Dtos.Student.AcademicCreditsWithInvalidKeys(academicCreditDtos, invalidAcademicCredits);
                    }
                    catch (Exception ex)
                    {
                        // Couldn't retrieve the desired academic credits or convert them to DTOs.
                        var errorMessage = "Unable to retrieve academic credits for the requested sections: " + "Exception thrown: " + ex.Message;
                        logger.Error(errorMessage);
                        throw;
                    }
                }

            }
            return new Dtos.Student.AcademicCreditsWithInvalidKeys(academicCreditDtos, invalidAcademicCredits);
        }

    }
}
