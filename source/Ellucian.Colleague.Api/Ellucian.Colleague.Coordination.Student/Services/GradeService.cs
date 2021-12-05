// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;


namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class GradeService : StudentCoordinationService, IGradeService
    {
        private readonly IGradeRepository _gradeRepository;
        private readonly IAcademicCreditRepository _academicCreditRepository;
        private readonly IStudentReferenceDataRepository studentReferenceDataRepository;
        private readonly ILogger repoLogger;
        private const string _dataOrigin = "Colleague";
        private IEnumerable<Domain.Student.Entities.GradeScheme> gradeSchemes = null;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IStudentConfigurationRepository _studentConfigurationRepository;
        private readonly ISectionRepository _sectionRepository;

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
        /// <param name="studentConfigurationRepository">The student configuration repository.</param>
        public GradeService(IGradeRepository gradeRepository, IStudentReferenceDataRepository referenceDataRepository, IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, IStudentRepository studentRepository,
            IAcademicCreditRepository academicCreditRepository, IConfigurationRepository configurationRepository,
            IStudentConfigurationRepository studentConfigurationRepository, ISectionRepository sectionRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _gradeRepository = gradeRepository;
            _academicCreditRepository = academicCreditRepository;
            studentReferenceDataRepository = referenceDataRepository;

            _studentConfigurationRepository = studentConfigurationRepository;
            _sectionRepository = sectionRepository;

            this.repoLogger = logger;
        }

        /// <summary>
        /// Gets GradeScheme object
        /// </summary>
        /// <returns>Returns GradeScheme object</returns>
        private async Task<IEnumerable<Domain.Student.Entities.GradeScheme>> GradeSchemesAsync()
        {
            if (gradeSchemes == null)
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
            if (gradeEntities != null && gradeEntities.Count() > 0)
            {
                foreach (var grade in gradeEntities)
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
            catch (NullReferenceException)
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
            catch (NullReferenceException)
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
            if (gradeScheme == null)
            {
                throw new ArgumentNullException("Bad.Data", string.Format("The GradeScheme '{0}' is missing or invalid.  Referenced in grade record '{1}'. ", response.GradeSchemeCode, response.Id));
            }
            else
            {
                if (string.IsNullOrEmpty(gradeScheme.Guid))
                {
                    throw new ArgumentNullException("GUID.Not.Found", string.Format("The GUID is missing for GradeScheme '{0}' referenced in grade record '{1}', GUID '{2}'. ", response.GradeSchemeCode, response.Id, response.Guid));
                }
                else
                {
                    var gradeSchemeGuid = gradeScheme != null ? gradeScheme.Guid : null;

                    grade.Id = response.Guid;
                    grade.GradeScheme = new Dtos.GuidObject2(gradeSchemeGuid);
                    grade.GradeItem = new Dtos.GradeItem() { GradeItemType = Dtos.GradeItemType.Literal, GradeValue = response.LetterGrade };
                    grade.GradeCmplCreditType = ConvertGradeCmplCode(response.Credit);
                    grade.EquivalentTo = null;
                }
            }

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
            var gradeScheme = (await GradeSchemesAsync()).FirstOrDefault(x => x.Code == response.GradeSchemeCode);
            if (gradeScheme == null)
            {
                throw new ArgumentNullException("Bad.Data", string.Format("The GradeScheme '{0}' is missing or invalid.  Referenced in grade record '{1}'. ", response.GradeSchemeCode, response.Id));
            }
            else
            {
                if (string.IsNullOrEmpty(gradeScheme.Guid))
                {
                    throw new ArgumentNullException("GUID.Not.Found", string.Format("The GUID is missing for GradeScheme '{0}' referenced in grade record '{1}', GUID '{2}'. ", response.GradeSchemeCode, response.Id, response.Guid));
                }
                else
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
            }

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
            if (!string.IsNullOrEmpty(gradeSchemeCode))
            {
                switch (gradeSchemeCode.ToUpper())
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

        public async Task<IEnumerable<Dtos.Student.StudentAnonymousGrading>> QueryAnonymousGradingIdsAsync(Dtos.Student.AnonymousGradingQueryCriteria criteria)
        {
            //student id is required
            if (criteria == null || string.IsNullOrWhiteSpace(criteria.StudentId))
            {
                throw new ArgumentNullException("studentId", "A student id is required in order to retrieve grading ids for a student.");
            }

            if ((criteria.TermIds != null && criteria.TermIds.Any()) && (criteria.SectionIds != null && criteria.SectionIds.Any()))
            {
                throw new ArgumentException("Either term ids or course section ids may be provided but not both.");
            }

            //an authenticated use can only access their own grade ids
            if (!CurrentUser.IsPerson(criteria.StudentId))
                throw new PermissionsException("User does not have permissions to access these student grading ids.");

            var studentAnonymousGradingIds = new List<Dtos.Student.StudentAnonymousGrading>();

            //get type of Anonymous Grading
            var academicRecordDefaults = await _studentConfigurationRepository.GetAcademicRecordConfigurationAsync();

            if (academicRecordDefaults == null || academicRecordDefaults.AnonymousGradingType == AnonymousGradingType.None)
            {
                logger.Info("Anonymous Grading Type configuration has not been configured and is null or empty");
                return studentAnonymousGradingIds;
            }

            var filterSectionIds = new List<string>();

            //validate section are valid
            if (criteria.SectionIds != null && criteria.SectionIds.Any())
            {
                var sections = (await _sectionRepository.GetCachedSectionsAsync(criteria.SectionIds.ToList())).ToList(); 
                
                //verify sections in criteria
                foreach (var sectionId in criteria.SectionIds)
                {
                    //Verify course section exists
                    var section = sections.Where(s => s.Id == sectionId).FirstOrDefault();
                    if (section == null)
                    {
                        studentAnonymousGradingIds.Add(new Dtos.Student.StudentAnonymousGrading()
                        {
                            AnonymousGradingId = string.Empty,
                            SectionId = sectionId,
                            TermId = string.Empty,
                            Message = "Course section " + sectionId + " is not a valid course section."
                        });
                        continue;
                    }

                    //Verify course section is configured for anonymous grading
                    if (academicRecordDefaults.AnonymousGradingType == AnonymousGradingType.Section && !section.GradeByRandomId)
                    {
                        studentAnonymousGradingIds.Add(new Dtos.Student.StudentAnonymousGrading()
                        {
                            AnonymousGradingId = string.Empty,
                            SectionId = sectionId,
                            TermId = string.Empty,
                            Message = "Course section " + sectionId + " is not configured for anonymous grading."
                        });
                        continue;
                    }

                    //valid section
                    filterSectionIds.Add(sectionId);
                }
            }

            var filterTermsIds = criteria.TermIds == null ? new List<string>() : criteria.TermIds.ToList();

            var results = await _academicCreditRepository.GetAnonymousGradingIdsAsync(academicRecordDefaults.AnonymousGradingType, 
                criteria.StudentId, filterTermsIds, filterSectionIds);

            // Get the right adapter for the type mapping and map entity to DTO
            var studentAnonymousGradingDtoAdapter = _adapterRegistry.GetAdapter<StudentAnonymousGrading, Dtos.Student.StudentAnonymousGrading>();
            foreach (var result in results)
            {
                studentAnonymousGradingIds.Add(studentAnonymousGradingDtoAdapter.MapToType(result));
            }
            return studentAnonymousGradingIds;
        }

    }

}
