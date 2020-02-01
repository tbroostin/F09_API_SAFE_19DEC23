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
    public class StudentTermService : StudentCoordinationService, IStudentTermService
    {
        private readonly IStudentTermRepository _studentTermRepository;
        private readonly IAcademicCreditRepository _academicCreditRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private ILogger _logger;

        public StudentTermService(IAdapterRegistry adapterRegistry, IStudentTermRepository studentTermRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, IStudentRepository studentRepository, IAcademicCreditRepository academicCreditRepository, IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _studentTermRepository = studentTermRepository;
            _academicCreditRepository = academicCreditRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Dtos.Student.StudentTerm>> QueryStudentTermsAsync(Dtos.Student.StudentTermsQueryCriteria criteria)
        {
            var studentIds = criteria.StudentIds;
            var termId = criteria.Term;
            var academicLevelId = criteria.AcademicLevel;

            ICollection<Dtos.Student.StudentTerm> studentAcademicTermDto = new List<Dtos.Student.StudentTerm>();
            IDictionary<string, List<Domain.Student.Entities.StudentTerm>> studentTermsDict = await _studentTermRepository.GetStudentTermsByStudentIdsAsync(studentIds, termId, academicLevelId);

            if (HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {
                foreach (var studentId in studentIds)
                {
                    if (studentTermsDict.ContainsKey(studentId))
                    {
                        List<Domain.Student.Entities.StudentTerm> studentAcadTerms = studentTermsDict[studentId];
                        foreach (var studentAcadTerm in studentAcadTerms)
                        {
                            // Get the right adapter for the type mapping
                            var studentProgramDtoAdapter = _adapterRegistry.GetAdapter<StudentTerm, Ellucian.Colleague.Dtos.Student.StudentTerm>();

                            // Map the degree plan entity to the degree plan DTO
                            var stuTermDto = studentProgramDtoAdapter.MapToType(studentAcadTerm);

                            studentAcademicTermDto.Add(stuTermDto);
                        }
                    }
                }
            }
            else
            {
                // Person running the process doesn't have view permissions for students. Throw Permission exception.
                throw new PermissionsException("User does not have permissions to view student information.");
            }
            return studentAcademicTermDto;
        }

        /// <summary>
        /// Gets student terms GPA for Pilot.
        /// </summary>
        /// <param name="studentIds">A collection of student ids</param>
        /// <param name="term">A term used to filter results</param>
        /// <returns><see cref="Dtos.Student.PilotStudentTermLevelGpa>"/></returns>
        public async Task<IEnumerable<Dtos.Student.PilotStudentTermLevelGpa>> QueryPilotStudentTermsGpaAsync(IEnumerable<string> studentIds, string term)
        {
            ICollection<Dtos.Student.PilotStudentTermLevelGpa> pilotStudentTermLevelGpaDto = new List<Dtos.Student.PilotStudentTermLevelGpa>();
            List<PilotAcademicHistoryLevel> pilotAcademicHistoryLevel = new List<PilotAcademicHistoryLevel>();
            if(!HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {
                var message = "User does not have permissions to access student terms gpa.";
                logger.Error(message);
                throw new PermissionsException(message);
            }
            var studentAcadCreds = await _academicCreditRepository.GetPilotAcademicCreditsByStudentIdsAsync(studentIds, AcademicCreditDataSubset.None, false, false, term);           
            foreach (var student in studentAcadCreds.Keys)
            {
                    List<PilotAcademicCredit> studentAcademicCredits = studentAcadCreds[student];
                    if (studentAcadCreds != null && studentAcadCreds.Count > 0)
                    {
                        if (studentAcademicCredits != null && studentAcademicCredits.Count() > 0)
                        {                            
                            // Loop through credits for each academic level
                            var levels = (from credit in studentAcademicCredits select credit.AcademicLevelCode).Distinct();
                            foreach (var level in levels)
                            {
                                var credits = studentAcademicCredits.Where(c => c.AcademicLevelCode != null && c.AcademicLevelCode == level);
                                var studentHistory = new PilotAcademicHistory(credits, new GradeRestriction(false), null);
                                if (string.IsNullOrEmpty(studentHistory.StudentId))
                                {
                                    studentHistory.StudentId = student;
                                }
                                if (!string.IsNullOrEmpty(student))
                                {
                                    // Add Logger Messages if we can't build AcademicHistory or the DTO
                                    try
                                    {
                                        var studentHistoryLevel = new PilotAcademicHistoryLevel(level, studentHistory, student);                                        
                                        // Get the right adapter for the type mapping                                        
                                        var pilotAcademicHistoryLevelDtoAdapter = _adapterRegistry.GetAdapter<PilotAcademicHistoryLevel, Ellucian.Colleague.Dtos.Student.PilotStudentTermLevelGpa>();
                                        // Map the PilotAcademicHistoryLevel entity to the PilotStudentTermLevelGpa DTO
                                        var historyDto = pilotAcademicHistoryLevelDtoAdapter.MapToType(studentHistoryLevel);
                                        historyDto.AcademicTermCode = term;
                                        pilotStudentTermLevelGpaDto.Add(historyDto);
                                    }
                                    catch (Exception ex)
                                    {
                                        // Couldn't build the PilotStudentTermLevelGpa DTO.
                                        var errorMessage = "Unable to build PilotStudentTermLevelGpa for Student '" + student + "', Level '" + level + "'. exception thrown: " + ex.Message;
                                        logger.Error(errorMessage);
                                    }
                                }
                            }
                        }
                    }
            }
            return pilotStudentTermLevelGpaDto;            
        }

    }
}
