// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DDay.iCal;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Services;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class GradeService: StudentCoordinationService, IGradeService
    {
        private readonly IGradeRepository _gradeRepository;
        private readonly IAcademicCreditRepository _academicCreditRepository;
        private readonly IStudentReferenceDataRepository studentReferenceDataRepository;
        private readonly ILogger repoLogger;
        private const string _dataOrigin = "Colleague";
        private IEnumerable<Domain.Student.Entities.GradeScheme> gradeSchemes = null;
        private readonly IConfigurationRepository _configurationRepository;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FacilitiesService"/> class.
        /// </summary>
        /// <param name="gradeRepository">The grade data repository.</param>
        /// <param name="configurationRepository">The configuration repository.</param>
        /// <param name="eventRepository">The event repository.</param>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="currentUserFactory">The current user factory.</param>
        /// <param name="roleRepository">The role repository.</param>
        /// <param name="logger">The logger.</param>
        public GradeService(IGradeRepository gradeRepository, IStudentReferenceDataRepository referenceDataRepository, IAdapterRegistry adapterRegistry, 
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, IStudentRepository studentRepository, IAcademicCreditRepository academicCreditRepository,
            IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _gradeRepository = gradeRepository;
            _academicCreditRepository = academicCreditRepository;
            studentReferenceDataRepository = referenceDataRepository;
            this.repoLogger = logger;
        }

        /// <summary>
        /// Gets GradeScheme object
        /// </summary>
        /// <returns>Returns GradeScheme object</returns>
        private async Task<IEnumerable<Domain.Student.Entities.GradeScheme>> GradeSchemesAsync()
        {
            if(gradeSchemes == null)
            {
                gradeSchemes = await studentReferenceDataRepository.GetGradeSchemesAsync();
            }
            return gradeSchemes;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Gets all grades
        /// </summary>
        /// <returns>Collection of Grade DTO objects</returns>
        public async Task<IEnumerable<Dtos.Grade>> GetAsync(bool bypassCache = false)
        {
            var gradeCollection = new List<Ellucian.Colleague.Dtos.Grade>();

            var gradeEntities = await _gradeRepository.GetHedmAsync(bypassCache);
            if(gradeEntities != null && gradeEntities.Count() > 0)
            {
                foreach(var grade in gradeEntities)
                {
                    gradeCollection.Add(await ConvertGradeEntityToGradeDtoAsync(grade));
                }
            }
            return gradeCollection;
        }

        /// <summary>
        /// Returns Grade by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns a single Grade DTO object</returns>
        public async Task<Dtos.Grade> GetGradeByIdAsync(string id)
        {
            try
            {
                var grade = await _gradeRepository.GetHedmGradeByIdAsync(id);
                if (grade != null)
                {
                    var gradeDto = await ConvertGradeEntityToGradeDtoAsync(grade);
                    return gradeDto;
                }
                else
                {
                    throw new NullReferenceException("Grade not found.");
                }

            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Grade not found for GUID " + id, ex);
            }
            catch (NullReferenceException nex)
            {
                throw new NullReferenceException("Id must be specified.");
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Gets all grades-definitions-maximum
        /// </summary>
        /// <returns>Collection of GradeDefinitionsMaximum DTO objects</returns>
        public async Task<IEnumerable<Dtos.GradeDefinitionsMaximum>> GetGradesDefinitionsMaximumAsync(bool bypassCache = false)
        {
            var gradeCollection = new List<Ellucian.Colleague.Dtos.GradeDefinitionsMaximum>();

            var gradeEntities = await _gradeRepository.GetHedmAsync(bypassCache);
            if (gradeEntities != null && gradeEntities.Count() > 0)
            {
                foreach (var grade in gradeEntities)
                {
                    gradeCollection.Add(await ConvertGradeEntityToGradeDefinitionsMaximumDtoAsync(grade));
                }
            }
            return gradeCollection;
        }

        /// <summary>
        /// Returns grade-definitions-maximum by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns a single GradeDefinitionsMaximum DTO object</returns>
        public async Task<Dtos.GradeDefinitionsMaximum> GetGradesDefinitionsMaximumIdAsync(string id)
        {
            try
            {
                var grade = await _gradeRepository.GetHedmGradeByIdAsync(id);

                var gradeDto = await ConvertGradeEntityToGradeDefinitionsMaximumDtoAsync(grade);

                return gradeDto;
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Grade not found for GUID " + id, ex);
            }
            catch (NullReferenceException nex)
            {
                throw new NullReferenceException("Id must be specified.");
            }
        }

        /// <summary>
        /// Gets all grades in dictionary form
        /// </summary>
        /// <returns>Dictionary of grades keyed by student ids</returns>
        private async Task<IDictionary<string, Grade>> GetGradesDictionaryAsync()
        {
            var grades = new Dictionary<string, Grade>();
            var gradeEntities = (await _gradeRepository.GetAsync()).ToList();
            foreach (var grade in gradeEntities)
            {
                grades.Add(grade.Id, grade);
            }
            return grades;
        }

        /// <summary>
        /// Gets grades and returns them in the PilotGrade form, possibly filtering results by term.
        /// </summary>
        /// <param name="studentIds">A collection of student ids</param>
        /// <param name="term">A term used to filter results</param>
        /// <returns></returns>
        public async Task<IEnumerable<PilotGrade>> GetPilotGradesAsync(IEnumerable<string> studentIds, string term)
        {
            if (HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {



                List<PilotGrade> pilotGrades = new List<PilotGrade>(); ;
                var studentAcademicCredits = await _academicCreditRepository.GetPilotAcademicCreditsByStudentIdsAsync(studentIds, AcademicCreditDataSubset.StudentCourseSec, false, true, term);
                Dictionary<string, Grade> grades = (Dictionary<string, Grade>)await GetGradesDictionaryAsync();

                foreach (var student in studentAcademicCredits.Keys)
                {
                    var studentCredits = studentAcademicCredits[student];

                    foreach (var credit in studentCredits)
                    {
                        if (credit.MidTermGrades != null && credit.MidTermGrades.Count() > 0)
                        {
                            foreach (var midtermGrade in credit.MidTermGrades)
                            {
                                var grade = new PilotGrade(student, credit.SectionId, string.Empty);
                                grade.Id = midtermGrade.Position.ToString();
                                if (grades.ContainsKey(midtermGrade.GradeId))
                                {
                                    var midterm = grades[midtermGrade.GradeId];
                                    grade.GradeType = "Midterm";
                                    grade.GradeValue = midterm.GradeValue;
                                    grade.Grade = midterm.LetterGrade;
                                    pilotGrades.Add(grade);
                                }
                            }
                        }
                        if (credit.HasVerifiedGrade)
                        {
                            var grade = new PilotGrade(student, credit.SectionId, string.Empty);
                            if (grades.ContainsKey(credit.VerifiedGradeId))
                            {
                                var verified = grades[credit.VerifiedGradeId];
                                grade.GradeType = "Final";
                                grade.GradeValue = verified.GradeValue;
                                grade.Grade = verified.LetterGrade;
                                pilotGrades.Add(grade);
                            }
                        }
                    }
                }
                return pilotGrades.Where(g => !string.IsNullOrWhiteSpace(g.SectionId)).ToList(); // Unlike Colleague, Pilot grades must be related to sections.
            }
            else
            {
                throw new PermissionsException("User does not have permissions to access these student grades.");
            }            
        }

        /// <summary>
        /// Gets grades and returns them in the PilotGrade form, possibly filtering results by term.
        /// </summary>
        /// <param name="studentIds">A collection of student ids</param>
        /// <param name="term">A term used to filter results</param>
        /// <returns></returns>
        public async Task<IEnumerable<PilotGrade>> GetPilotGrades2Async(IEnumerable<string> studentIds, string term)
        {
            if (HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {
                List<PilotGrade> pilotGrades = new List<PilotGrade>(); ;
                var studentAcademicCredits = await _academicCreditRepository.GetPilotAcademicCreditsByStudentIds2Async(studentIds, AcademicCreditDataSubset.StudentCourseSec, false, true, term);
                Dictionary<string, Grade> grades = (Dictionary<string, Grade>)await GetGradesDictionaryAsync();

                foreach (var student in studentAcademicCredits.Keys)
                {
                    var studentCredits = studentAcademicCredits[student];

                    foreach (var credit in studentCredits)
                    {
                        if (credit.MidTermGrades != null && credit.MidTermGrades.Count() > 0)
                        {
                            foreach (var midtermGrade in credit.MidTermGrades)
                            {
                                var grade = new PilotGrade(student, credit.SectionId, string.Empty);
                                grade.Id = midtermGrade.Position.ToString();
                                if (grades.ContainsKey(midtermGrade.GradeId))
                                {
                                    var midterm = grades[midtermGrade.GradeId];
                                    grade.GradeType = "Midterm";
                                    grade.GradeValue = midterm.GradeValue;
                                    grade.Grade = midterm.LetterGrade;
                                    pilotGrades.Add(grade);
                                }
                            }
                        }
                        if (credit.HasVerifiedGrade)
                        {
                            var grade = new PilotGrade(student, credit.SectionId, string.Empty);
                            if (grades.ContainsKey(credit.VerifiedGradeId))
                            {
                                var verified = grades[credit.VerifiedGradeId];
                                grade.GradeType = "Final";
                                grade.GradeValue = verified.GradeValue;
                                grade.Grade = verified.LetterGrade;
                                pilotGrades.Add(grade);
                            }
                        }
                    }
                }
                return pilotGrades.Where(g => !string.IsNullOrWhiteSpace(g.SectionId)).ToList(); // Unlike Colleague, Pilot grades must be related to sections.
            }
            else
            {
                throw new PermissionsException("User does not have permissions to access these student grades.");
            }
        }


        /// <summary>
        /// Convert Grade entity to Grade DTO
        /// </summary>
        /// <param name="response"></param>
        /// <returns>Returns a Grade DTO object</returns>
        private async Task<Dtos.Grade> ConvertGradeEntityToGradeDtoAsync(Domain.Student.Entities.Grade response)
        {
            var grade = new Ellucian.Colleague.Dtos.Grade();
            var gradeScheme = (await GradeSchemesAsync()).Where(gs => gs != null && gs.Code == response.GradeSchemeCode).FirstOrDefault();
            var gradeSchemeGuid = gradeScheme != null ? gradeScheme.Guid : null;

            grade.Id = response.Guid;
            grade.GradeScheme = new Dtos.GuidObject2(gradeSchemeGuid);
            grade.GradeItem = new Dtos.GradeItem() { GradeItemType = Dtos.GradeItemType.Literal, GradeValue = response.LetterGrade };
            grade.GradeCmplCreditType = ConvertGradeCmplCode(response.Credit);
            grade.EquivalentTo = null;
            
            return grade;
        }

        /// <summary>
        /// Convert Grade entity to GradeDefinitionsMaximum DTO
        /// </summary>
        /// <param name="response"></param>
        /// <returns>Returns a GradeDefinitionsMaximum DTO object</returns>
        private async Task<Dtos.GradeDefinitionsMaximum> ConvertGradeEntityToGradeDefinitionsMaximumDtoAsync(Domain.Student.Entities.Grade response)
        {
            var gradeDefinitionsMaximum = new Dtos.GradeDefinitionsMaximum();
            var gradeSchemeProperty = new Dtos.GradeSchemeProperty();
            var gradeScheme = (await GradeSchemesAsync()).FirstOrDefault(x=> x.Code == response.GradeSchemeCode);
            if (gradeScheme != null)
            {
                gradeSchemeProperty.EndOn = gradeScheme.EffectiveEndDate;
                gradeSchemeProperty.StartOn = gradeScheme.EffectiveStartDate;
                gradeSchemeProperty.Detail = new Dtos.GuidObject2() { Id = gradeScheme.Guid };
                gradeSchemeProperty.Code = gradeScheme.Code;
                gradeSchemeProperty.Title = gradeScheme.Description;

                var acadLevel = (await studentReferenceDataRepository.GetAcademicLevelsAsync()).FirstOrDefault(al => al.GradeScheme == gradeScheme.Code);
                if (acadLevel != null)
                {
                    var acadLevelProperty = new Dtos.AcademicLevelProperty();
                    acadLevelProperty.Code = acadLevel.Code;
                    acadLevelProperty.Title = acadLevel.Description;
                    acadLevelProperty.Detail = new Dtos.GuidObject2() { Id = acadLevel.Guid };
                    gradeSchemeProperty.AcademicLevel = acadLevelProperty;          
                }                
            }
            gradeDefinitionsMaximum.Id = response.Guid;
            gradeDefinitionsMaximum.GradeScheme = gradeSchemeProperty;
            gradeDefinitionsMaximum.GradeItem = new Dtos.GradeItem() { GradeItemType = Dtos.GradeItemType.Literal, GradeValue = response.LetterGrade };
            gradeDefinitionsMaximum.GradeCmplCreditType = ConvertGradeCmplCode(response.Credit);
            gradeDefinitionsMaximum.EquivalentTo = null;

            return gradeDefinitionsMaximum;
        }

        /// <summary>
        /// Converts "Y" or "N" to GradeCmplCreditType
        /// </summary>
        /// <param name="gradeSchemeCode"></param>
        /// <returns>Returns GradeCmplCreditType</returns>
        private Dtos.GradeCmplCreditType ConvertGradeCmplCode(string gradeSchemeCode)
        {
            var gradeCmplType = Dtos.GradeCmplCreditType.None;
            if(!string.IsNullOrEmpty(gradeSchemeCode))
            {
                switch(gradeSchemeCode.ToUpper())
                {
                    case "Y":
                        gradeCmplType = Dtos.GradeCmplCreditType.Full;
                        break;
                    case "N":
                        gradeCmplType = Dtos.GradeCmplCreditType.None;
                        break;
                }
            }
            return gradeCmplType;
        }
    }
}
