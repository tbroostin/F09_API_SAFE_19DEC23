﻿// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
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
        private readonly IApplicantRepository _applicantRepository;
        private readonly ITermRepository _termRepository;
        private readonly ISectionRepository _sectionRepository;
        private ILogger _logger;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;

        public AcademicHistoryService(IAdapterRegistry adapterRegistry, IStudentRepository studentRepository, IApplicantRepository applicantRepository,  IAcademicCreditRepository academicCreditRepository, ITermRepository termRepository, ISectionRepository sectionRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, IConfigurationRepository configurationRepository, IReferenceDataRepository referenceDataRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _academicCreditRepository = academicCreditRepository;
            _studentRepository = studentRepository;
            _applicantRepository = applicantRepository;
            _termRepository = termRepository;
            _sectionRepository = sectionRepository;
            _logger = logger;
            _configurationRepository = configurationRepository;
            _referenceDataRepository = referenceDataRepository;
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
            sectionsReads = new Stopwatch();

            completeTime.Start();

            IEnumerable<Term> termData = null;
            ICollection<Dtos.Student.PilotAcademicHistoryLevel> pilotAcademicHistoryLevelDto = new List<Dtos.Student.PilotAcademicHistoryLevel>();
            // If the person requesting the information has permission to
            // view all student information.
            if (HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {
                if (studentIds == null || !studentIds.Any(s => !string.IsNullOrEmpty(s)))
                {
                    throw new ArgumentNullException("studentIds", "At least one student ID is required when querying academic history level by ID.");
                }
                AcademicCreditDataSubset reads = AcademicCreditDataSubset.StudentCourseSec | AcademicCreditDataSubset.StudentEquivEvals; // Bitwise OR on these values gives us a value that has both bits set.

                logger.Error("Calling GetPilotAcademicCreditsByStudentIdsAsync from GetPilotAcademicHistoryLevelByIdsAsync");
                Dictionary<string, List<PilotAcademicCredit>> studentAcadCreds = await _academicCreditRepository.GetPilotAcademicCreditsByStudentIdsAsync(studentIds.ToList(), reads, bestFit, filter);

                bool error = false;
                IEnumerable<Section> sections = new List<Section>();
                // Determine if we need to use census date checking for first term enrolled
                bool useCensusDate = await _academicCreditRepository.GetPilotCensusBooleanAsync();
                logger.Error("Census:" + useCensusDate);
                HashSet<string> sectionIds = new HashSet<string>();
                foreach (var student in studentAcadCreds)
                {
                    foreach (var credit in student.Value)
                    {
                        if (credit != null)
                        {
                            if (credit.AcademicLevelCode != null && credit.SectionId != null)
                                sectionIds.Add(credit.SectionId);
                            if (credit.TermCode != null && termData == null)
                            {
                                termData = await _termRepository.GetAsync(); // First time we see a credit with a term, get term data.                           
                            }
                        }
                    }
                }
                if (useCensusDate && sectionIds.Any()) // only need to read sections if we're doing census date checking while determining first term enrolled
                {
                    sectionsReads.Start();
                    sections = await _sectionRepository.GetCachedSectionsAsync(sectionIds);
                    sectionsReads.Stop();
                }
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
                                    // Loop through credits for each academic level
                                    var levels = (from credit in studentAcademicCredits where credit != null select credit.AcademicLevelCode).Distinct().ToList();
                                    foreach (var level in levels)
                                    {
                                        var credits = studentAcademicCredits.Where(c => c != null && c.AcademicLevelCode != null && c.AcademicLevelCode == level).ToList();
                                        string firstTermEnrolled = await GetPilotFirstTermEnrolledAsync(termData, credits, sections, useCensusDate);
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
                if (error && pilotAcademicHistoryLevelDto.Count() == 0)
                    throw new ColleagueWebApiException("Unexpected errors occurred.  No academic history level records returned.  Check API error log.");
            }
            else
            {
                // Person doesn't have View Student Information permissions. Throw Permission exception.
                throw new PermissionsException("User does not have permissions to access the student academic history.");
            }

            completeTime.Stop();

            logger.Error("CompleteTime      :" + completeTime.ElapsedMilliseconds);
            logger.Error("Sections Read Time: " + sectionsReads.ElapsedMilliseconds);

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
                    throw new ColleagueWebApiException("Unexpected errors occurred.  No academic history level records returned.  Check API error log.");
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
        /// <returns>Code for academic credit owner's first enrolled term</returns>
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
        /// For incoming pilot credits filtered by academic level, determine the firstTermEnrolled.
        /// </summary>
        /// <param name="termData">Data for all terms</param>
        /// <param name="credits">Student academic credit filtered by academic level</param>
        /// <param name="sections">Data for all sections for the student's credits</param>
        /// <param name="useCensusDate">Boolean to use census date for checking active registration</param>
        /// <returns>Code for academic credit owner's first enrolled term</returns>
        private async Task<string> GetPilotFirstTermEnrolledAsync(IEnumerable<Term> termData, IEnumerable<PilotAcademicCredit> credits, IEnumerable<Section> sections, bool useCensusDate)
        {
            logger.Debug("Entering GetPilotFirstTermEnrolledAsync...");
            Term termFirstEnrolled = null;
            if (credits != null)
            {
                if (useCensusDate == false)
                {
                    logger.Debug("GetPilotFirstTermEnrolledAsync > UseCensusDate is false.");
                    // Not using census date.  Use only active credit given most recent credit status.
                    var activeCredits = credits.Where(c => (c.Status == CreditStatus.Add || c.Status == CreditStatus.New));
                    foreach (var credit in activeCredits)
                    {
                        termFirstEnrolled = GetEarliestTermEnrolled(termData, credit, termFirstEnrolled);
                    }
                }
                else
                {
                    logger.Debug("GetPilotFirstTermEnrolledAsync > UseCensusDate is true.");
                    // Filter academic credits to those with sections.
                    var sectionCredits = credits.Where(c => c != null && !string.IsNullOrEmpty(c.SectionId)).ToList();
                    if (sectionCredits != null && sectionCredits.Any())
                    {
                        // Build a fixed list of section Ids for this student/level.
                        var sectionIds = sectionCredits.Select(sc => sc.SectionId).ToList();
                        // Build sectionEntities to contain data for just sections for this student/level, 
                        // and make it acccessible by section IDs.
                        Dictionary<string, Section> sectionEntities = new Dictionary<string, Section>();
                        var distinctSections = sections.Where(s => s != null && sectionIds.Contains(s.Id)).Distinct().ToList();
                        foreach (var section in distinctSections)
                        {
                            sectionEntities.Add(section.Id, section);
                        }
                        foreach (var credit in credits)
                        {
                            if (credit != null && !string.IsNullOrWhiteSpace(credit.TermCode))
                            {
                                // Extract data for this credit's section by it section ID
                                var sectionEntity = (!string.IsNullOrEmpty(credit.SectionId) && sectionEntities.Keys.Contains(credit.SectionId)) ? sectionEntities[credit.SectionId] : null;
                                if (sectionEntity != null)
                                {
                                    // Set effective census date from section, otherwise term location, otherwise term.
                                    DateTime? effectiveCensusDate = null;
                                    if (sectionEntity.RegistrationDateOverrides != null && sectionEntity.RegistrationDateOverrides.CensusDates != null)
                                    {
                                        var sectionCensusDate = sectionEntity.RegistrationDateOverrides.CensusDates.FirstOrDefault();
                                        if (sectionCensusDate != null)
                                        {
                                            effectiveCensusDate = sectionCensusDate;
                                            logger.Debug(string.Format("GetPilotFirstTermEnrolledAsync > Effective Census Date derived from section {0}: {1}.", sectionEntity.Id, effectiveCensusDate));
                                        }
                                    }
                                    else
                                    {
                                        // For this credit, get Term location census date and if necessary Term census date.
                                        var termEntity = termData.Where(t => t != null && t.Code == credit.TermCode).FirstOrDefault();
                                        if (!string.IsNullOrWhiteSpace(credit.Location))
                                        {
                                            if (termEntity != null)
                                            {
                                                var termLocationRegistrationDates = termEntity.RegistrationDates.Where(l => l != null && l.Location == credit.Location).FirstOrDefault();
                                                if (termLocationRegistrationDates != null && termLocationRegistrationDates.CensusDates != null)
                                                {
                                                    effectiveCensusDate = termLocationRegistrationDates.CensusDates.FirstOrDefault();
                                                    logger.Debug(string.Format("GetPilotFirstTermEnrolledAsync > Effective Census Date derived from term {0} and location {1}: {2}.", termEntity.Code, credit.Location, effectiveCensusDate));
                                                }
                                                if (effectiveCensusDate == null)
                                                {
                                                    var termRegistrationDates = termEntity.RegistrationDates.Where(l => l != null && l.Location == string.Empty).FirstOrDefault();
                                                    if (termRegistrationDates != null)
                                                    {
                                                        effectiveCensusDate = termRegistrationDates.CensusDates.FirstOrDefault();
                                                        logger.Debug(string.Format("GetPilotFirstTermEnrolledAsync > Effective Census Date derived from term {0}: {1}.", termEntity.Code, effectiveCensusDate));
                                                    }
                                                }
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
                                                if (st != null)
                                                {
                                                    var status = st.Status;
                                                    var statusType = await _academicCreditRepository.ConvertCreditStatusAsync(status);
                                                    var statusDate = st.Date;
                                                    if (statusDate <= effectiveCensusDate)
                                                    {
                                                        if (statusType == CreditStatus.Add || statusType == CreditStatus.New)
                                                        {
                                                            passCensusCheck = true;
                                                            logger.Debug(string.Format("GetPilotFirstTermEnrolledAsync > Student Academic Credit {0} passes census check; status date is on or before Effective Census Date {1}.", credit.Id, effectiveCensusDate));
                                                        }
                                                        // We only care about the most recent status found
                                                        // before the census date.  So break once we find one.
                                                        break;
                                                    }
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
                                            logger.Debug(string.Format("GetPilotFirstTermEnrolledAsync > Student Academic Credit {0} passes census check; Effective Census Date is null.", credit.Id));
                                        }
                                    }
                                    if (passCensusCheck == true)
                                    {
                                        if (credit.TermCode != null)
                                        {
                                            termFirstEnrolled = GetEarliestTermEnrolled(termData, credit, termFirstEnrolled);
                                            string firstEnrolledTermCode = termFirstEnrolled != null ? termFirstEnrolled.Code : null;
                                            if (!string.IsNullOrEmpty(firstEnrolledTermCode))
                                            {
                                                logger.Debug(string.Format("GetPilotFirstTermEnrolledAsync > First Term Enrolled = {0} after evaluating Student Academic Credit {1}.", firstEnrolledTermCode, credit.Id));
                                            }
                                            else
                                            {
                                                logger.Debug(string.Format("GetPilotFirstTermEnrolledAsync > First Term Enrolled is null after evaluating Student Academic Credit {0}.", credit.Id));
                                            }
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

            logger.Debug(string.Format("Exiting GetPilotFirstTermEnrolledAsync. First Term Enrolled = {0}.", firstTermEnrolled));
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
                var termEntity = termData.Where(t => t != null && t.Code == credit.TermCode).FirstOrDefault();
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
        /// Retrieves the academic history for the student. 
        /// This retrieves all the raw academic credits which includes:
        /// academic credits that were imported directly without student being registered to existing section.
        /// academic credits that were transfer, dropped, withdrawn or non-course credits based upon filter and includeDrop parameters.
        /// This differs from earlier versions that we read STUDENT.ACAD.CRED file for a student whereas in other versions we read PERSON.ST file to retrieve 
        /// pointers to STC record. Therefore earlier versions did not return the acad credits that were imported through any legacy system.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <param name="bestFit">If true, places all academic credits in a term</param>
        /// <param name="filter">If true, filter academic credits for certain statuses</param>
        /// <param name="term">(Optional) used to return only a specific term of data.</param>
        /// <param name="includeDrops">If true, include dropped academic credits</param>
        /// <returns><see cref="AcademicHistory4">Academic History</see> for the student.</returns>
        public async Task<Ellucian.Colleague.Dtos.Student.AcademicHistory4> GetAcademicHistory5Async(string studentId, bool bestFit = false, bool filter = true, string term = null, bool includeDrops = false)
        {
            Dtos.Student.AcademicHistory4 academicHistory = null;
            try
            {
                if (string.IsNullOrEmpty(studentId))
                {
                    string message = "studentId must be provided in order to retrieve student's academic history";
                    logger.Error(message);
                    throw new ArgumentNullException("studentId", message);
                }
                Dictionary<string, List<AcademicCredit>> studentAcademicCreditsByStudentId = await _academicCreditRepository.GetAcademicCreditByStudentIdsAsync(new List<string>() { studentId }, bestFit, filter, includeDrops);
                if (studentAcademicCreditsByStudentId == null || !studentAcademicCreditsByStudentId.ContainsKey(studentId))
                {
                    throw new ColleagueWebApiException("Either student academic credits collection was empty or it did no have the student as a key in collection");
                }
                academicHistory = await ConvertAcademicCreditsToAcademicHistoryDto4Async(studentId, studentAcademicCreditsByStudentId[studentId]);

                // Filter to return only one specific term of data if we have a term filter set
                if (!string.IsNullOrEmpty(term) && academicHistory != null && academicHistory.AcademicTerms != null)
                {
                    List<Dtos.Student.AcademicTerm4> filteredTerms = academicHistory.AcademicTerms.Where(a => a.TermId != null && a.TermId == term).ToList();
                    academicHistory.AcademicTerms = filteredTerms;
                }
                return academicHistory;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, string.Format("Colleague session expired while retrieving academic history version 5 for student with id {0}", studentId));
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }
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
        /// <param name="useCache">Use cached course section data when retrieving academic credit records of specified section(s)</param>
        /// <returns><see cref="AcademicCreditsWithInvalidKeys">AcademicCreditsWithInvalidKeys</see> Dtos</returns>
        public async Task<Dtos.Student.AcademicCreditsWithInvalidKeys> QueryAcademicCreditsWithInvalidKeysAsync(Dtos.Student.AcademicCreditQueryCriteria criteria, bool useCache = true)
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
            IEnumerable<Section> sections = null;
            if (useCache)
            {
                sections = (await _sectionRepository.GetCachedSectionsAsync(sectionIds, false));
            }
            else
            {
                sections = (await _sectionRepository.GetNonCachedSectionsAsync(sectionIds, false));
            }
            if (sections != null && sections.Any())
            {
                string requestor = CurrentUser.PersonId;
                List<string> querySectionIds = new List<string>();
                var allDepartments = await _referenceDataRepository.DepartmentsAsync();
                var userPermissions = await GetUserPermissionCodesAsync();

                // Determine the actual list of section Ids that should be used for the query. Make sure the requestor has access to see credits for the sections
                // AND add in any cross listed sections if requested.
                foreach (var section in sections)
                {
                    // Only assigned faculty or a departmental oversight member of a section can get grade information for a section
                    bool canAccessGradeInformation = false;
                    if (section != null && section.FacultyIds != null && section.FacultyIds.Contains(requestor))
                    {
                        canAccessGradeInformation = true;
                    }
                    else
                    {
                        // Check if the requestor is a departmental oversight member for this section with the required permissions
                        if (CheckDepartmentalOversightAccessForSection(section, allDepartments) &&
                           (userPermissions.Contains(DepartmentalOversightPermissionCodes.ViewSectionRoster) ||
                           userPermissions.Contains(DepartmentalOversightPermissionCodes.ViewSectionGrading) ||
                           userPermissions.Contains(DepartmentalOversightPermissionCodes.CreateSectionGrading) ||
                           userPermissions.Contains(DepartmentalOversightPermissionCodes.ViewSectionDropRoster) ||
                           userPermissions.Contains(DepartmentalOversightPermissionCodes.CreateSectionDropRoster) ||
                           userPermissions.Contains(DepartmentalOversightPermissionCodes.ViewSectionCensus) ||
                           userPermissions.Contains(DepartmentalOversightPermissionCodes.CreateSectionCensus)))

                        {
                            canAccessGradeInformation = true;
                        }
                    }

                    if (canAccessGradeInformation)
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
                            if (academicCreditsWithInvalidKeys.InvalidAcademicCreditIds != null)
                            {
                                invalidAcademicCredits = academicCreditsWithInvalidKeys.InvalidAcademicCreditIds.ToList();
                            }
                        }
                        return new Dtos.Student.AcademicCreditsWithInvalidKeys(academicCreditDtos, invalidAcademicCredits);
                    }
                    catch (ColleagueSessionExpiredException)
                    {
                        throw;
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


        /// <summary>
        /// Retrieves the academic crdits for an applicant. 
        /// This retrieves all the raw academic credits which includes:
        /// academic credits that were imported directly without student being registered to existing section.
        /// academic credits that were transfer, dropped, withdrawn or non-course credits based upon filter.
        /// if filter is true then retrieves academic credits that are New, Add, PR, TR, NC, Withdrawn status only.
        /// </summary>
        /// <param name="applicantId">Id of the applicant</param>
        /// <param name="filter">If true, filter academic credits for certain statuses</param>
        /// <returns><see cref="ApplicantAcademicCredit">Applicant Academic Credit</see> for the applicant.</returns>
        public async Task<IEnumerable<Dtos.Student.ApplicantAcademicCredit>> GetApplicantAcademicCreditsAsync(string applicantId, bool filter = true, bool includeDrops = false)
        {
            List<Dtos.Student.ApplicantAcademicCredit> academicCreditDtos = new List<Dtos.Student.ApplicantAcademicCredit>();
            try
            {
                if (string.IsNullOrEmpty(applicantId))
                {
                    string message = "applicant Id must be provided in order to retrieve applicant's academic history.";
                    logger.Error(message);
                    throw new ArgumentNullException("applicantId", message);
                }
                if(!UserIsSelf(applicantId))
                {
                    string message = "User must be self to retrieve applicant's academic history.";
                    logger.Error(message);
                    throw new PermissionsException(message);
                }
                // validate user should be an applicant
                Domain.Student.Entities.Applicant applicant;
                applicant = await _applicantRepository.GetApplicantAsync(applicantId);
                if (applicant == null)
                {
                    throw new KeyNotFoundException("Applicant with ID " + applicantId + " not found in the repository.");
                }
                Dictionary<string, List<AcademicCredit>> academicCredits = await _academicCreditRepository.GetAcademicCreditByStudentIdsAsync(new List<string>() { applicantId },false,  filter, includeDrops);
               
                if (academicCredits == null )
                {
                    throw new ColleagueWebApiException("Either applicant's academic credits collection was empty or it did no have the applicant as a key in collection");
                }
                if (academicCredits.Keys.Count == 0)
                {
                    academicCredits.Add(applicantId, new List<AcademicCredit>());
                }
                if( !academicCredits.ContainsKey(applicantId))
                {
                    throw new ColleagueWebApiException("Either applicant's academic credits collection was empty or it did no have the applicant as a key in collection");
                }

                var academicCreditAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.AcademicCredit, Dtos.Student.ApplicantAcademicCredit>();
                foreach (var credit in academicCredits[applicantId])
                {
                    academicCreditDtos.Add(academicCreditAdapter.MapToType(credit));
                }
                return academicCreditDtos;
                
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, string.Format("Colleague session expired while retrieving academic history version 5 for applicant with id {0}", applicantId));
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception occurred while retrieving applicant's academic history for applicant Id: " + applicantId);
                throw;
            }
        }

    }
}
