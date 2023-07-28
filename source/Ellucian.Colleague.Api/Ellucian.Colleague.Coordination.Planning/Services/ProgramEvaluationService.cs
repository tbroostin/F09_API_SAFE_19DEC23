// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Domain.Planning.Repositories;
using Ellucian.Colleague.Domain.Planning.Services;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Repositories;
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

namespace Ellucian.Colleague.Coordination.Planning.Services
{
    [RegisterType]
    public class ProgramEvaluationService : StudentCoordinationService, IProgramEvaluationService
    {
        private const string _DefaultSortSpecId = "DEFAULT";
        private readonly IStudentDegreePlanRepository _studentDegreePlanRepository;
        private readonly IProgramRequirementsRepository _programRequirementsRepository;
        private readonly IRequirementRepository _requirementRepository;
        private readonly IStudentProgramRepository _studentProgramRepository;
        private readonly IAcademicCreditRepository _academicCreditRepository;
        private readonly IDegreePlanRepository _degreePlanRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IPlanningStudentRepository _planningStudentRepository;
        private readonly ITermRepository _termRepository;
        private readonly IRuleRepository _ruleRepository;
        private readonly IProgramRepository _programRepository;
        private readonly ICatalogRepository _catalogRepository;
        private readonly IPlanningConfigurationRepository _configurationRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IApplicantRepository _applicantRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger logger;
        private readonly IConfigurationRepository _baseConfigurationRepository;

        public ProgramEvaluationService(IAdapterRegistry adapterRegistry, IStudentDegreePlanRepository studentDegreePlanRepository,
            IProgramRequirementsRepository programRequirementsRepository, IStudentRepository studentRepository,
            IPlanningStudentRepository planningStudentRepository, IApplicantRepository applicantRepository, IStudentProgramRepository studentProgramRepository,
            IRequirementRepository requirementRepository, IAcademicCreditRepository academicCreditRepository,
            IDegreePlanRepository degreePlanRepository, ICourseRepository courseRepository, ITermRepository termRepository,
            IRuleRepository ruleRepository, IProgramRepository programRepository, ICatalogRepository catalogRepository,
            IPlanningConfigurationRepository configurationRepository, IReferenceDataRepository referenceDataRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger,
            IConfigurationRepository baseConfigurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, baseConfigurationRepository)
        {
            _baseConfigurationRepository = baseConfigurationRepository;
            _studentDegreePlanRepository = studentDegreePlanRepository;
            _programRequirementsRepository = programRequirementsRepository;
            _requirementRepository = requirementRepository;
            _studentRepository = studentRepository;
            _planningStudentRepository = planningStudentRepository;
            _applicantRepository = applicantRepository;
            _studentProgramRepository = studentProgramRepository;
            _academicCreditRepository = academicCreditRepository;
            _degreePlanRepository = degreePlanRepository;
            _courseRepository = courseRepository;
            _termRepository = termRepository;
            _ruleRepository = ruleRepository;
            _programRepository = programRepository;
            _catalogRepository = catalogRepository;
            _referenceDataRepository = referenceDataRepository;
            _configurationRepository = configurationRepository;
            _adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Return a list of program evaluations for the given student and program
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <param name="programCodes">List of program codes</param>
        /// <param name="catalogYear">The catalogYear code</param>
        /// <returns>A list of <see cref="ProgramEvaluation">ProgramEvaluation</see> objects</returns>
        public async Task<IEnumerable<Domain.Student.Entities.ProgramEvaluation>> EvaluateAsync(string studentId, List<string> programCodes, string catalogYear = null)
        {
            IEnumerable<Domain.Student.Entities.ProgramEvaluation> evalResults;

            // Get the student so that permissions can be checked first
            Domain.Student.Entities.PlanningStudent student;
            student = await _planningStudentRepository.GetAsync(studentId);
            if (student == null)
            {
                throw new KeyNotFoundException("Student with ID " + studentId + " not found in the repository.");
            }

            //// First, check permissions for this function. If user is not the student id, verify permissions for another user.
            if (!(UserIsSelf(studentId)))
            {
                // If permissions not found, this will throw a permissions exception. 
                await CheckStudentAdvisorUserAccessAsync(studentId, student.ConvertToStudentAccess());
            }

            var watch = new Stopwatch();
            watch.Start();
            evalResults = await EvaluateProgramsAsync(studentId, programCodes, student, catalogYear);
            watch.Stop();
            logger.Info("EvaluationTiming: Completed EvaluatePrograms in " + watch.ElapsedMilliseconds.ToString() + " ms");

            return evalResults;
        }

        /// <summary>
        /// Return a list of program evaluations for the given applicant and program
        /// </summary>
        /// <param name="applicantId">Id of the applicant</param>
        /// <param name="programCodes">List of program codes</param>
        /// <param name="catalogYear">The catalogYear code</param>
        /// <returns>A list of <see cref="ProgramEvaluation">ProgramEvaluation</see> objects</returns>
        public async Task<IEnumerable<Domain.Student.Entities.ProgramEvaluation>> EvaluateApplicantAsync(string applicantId, List<string> programCodes, string catalogYear = null)
        {
            IEnumerable<Domain.Student.Entities.ProgramEvaluation> evalResults;
            if (string.IsNullOrEmpty(applicantId))
            {
                throw new ArgumentNullException("applicantId", "Applicant id must be provided to run evaluation");
            }
            if (programCodes == null || !programCodes.Any())
            {
                throw new ArgumentNullException("programCodes", "At least one program code must be provided to run evaluation.");
            }
            //user should be self
            if (!UserIsSelf(applicantId))
            {
                var error = "User " + CurrentUser.PersonId + " does not match given applicant ID " + applicantId + " and may not run program evaluation.";
                logger.Error(error);
                throw new PermissionsException(error);
            }
            //User should have appropriate permission of EVALUATE.WHAT.IF
            if (!await CheckEvaluatePermissionAsync())
            {
                var error = "User " + CurrentUser.PersonId + " does not have EVALUATE.WHAT.IF permission to run program evaluation.";
                logger.Error(error);
                throw new PermissionsException(error);

            }
            // validate user should be an applicant
            Domain.Student.Entities.Applicant applicant;
            applicant = await _applicantRepository.GetApplicantAsync(applicantId);
            if (applicant == null)
            {
                throw new KeyNotFoundException("Applicant with ID " + applicantId + " not found in the repository.");
            }

            var watch = new Stopwatch();
            watch.Start();
            evalResults = await EvaluateApplicantProgramsAsync(applicantId, programCodes, applicant, catalogYear);
            watch.Stop();
            logger.Info("EvaluationTiming: Completed EvaluatePrograms for an applicant in " + watch.ElapsedMilliseconds.ToString() + " ms");

            return evalResults;
        }


        /// <summary>
        /// Gets notices pertaining to the evaluation for a given student and program.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <param name="programCode">Code of the program</param>
        /// <returns>List of <see cref="EvaluationNotice">EvaluationNotice</see> Dtos</returns>
        public async Task<IEnumerable<Dtos.Student.EvaluationNotice>> GetEvaluationNoticesAsync(string studentId, string programCode)
        {
            var noticeDtos = new List<Dtos.Student.EvaluationNotice>();

            // First, check permissions for this function. If user is not the student id, read student and verify permissions
            // for another user.
            if (!(UserIsSelf(studentId)))
            {
                // If permissions not found, this will throw a permissions exception. 
                await CheckUserAccessAsync(studentId);
            }

            // If still here... continue on to get student program notices from the repository, convert to dto
            var notices = await _studentProgramRepository.GetStudentProgramEvaluationNoticesAsync(studentId, programCode);
            if (notices != null)
            {
                var noticeDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.EvaluationNotice, Dtos.Student.EvaluationNotice>();

                foreach (var notice in notices)
                {
                    try
                    {
                        noticeDtos.Add(noticeDtoAdapter.MapToType(notice));
                    }
                    catch (Exception ex)
                    {
                        var message = "Error converting notice type " + notice.Type.ToString() + " for student " + studentId + " program " + programCode;
                        logger.Error(ex, message);
                        throw new ColleagueWebApiException(message);
                    }
                }
            }
            return noticeDtos;
        }
        private async Task<IEnumerable<Domain.Student.Entities.ProgramEvaluation>> EvaluateProgramsAsync(
            string studentId, List<string> programCodes, Domain.Student.Entities.PlanningStudent student, string catalogYear)
        {
            var evaluations = new List<Domain.Student.Entities.ProgramEvaluation>();

            var watch = new Stopwatch();

            // Get the student's academic credits.
            List<AcademicCredit> credits = new List<AcademicCredit>();

            var creditsDict = await _academicCreditRepository.GetAcademicCreditByStudentIdsAsync(new List<string>() { student.Id }, false, true, true);
            if (creditsDict.ContainsKey(student.Id))
            {
                credits = creditsDict[student.Id];
            }

            // List of courses needed for the evaluation (added for evaluating equated courses)
            IEnumerable<Course> courses = new List<Course>();
            courses = await _courseRepository.GetAsync();

            // Get the degree plan for this student
            Domain.Student.Entities.DegreePlans.DegreePlan degreePlan = null;
            if (student.DegreePlanId != null)
            {
                degreePlan = await _studentDegreePlanRepository.GetAsync((int)student.DegreePlanId);
                if (degreePlan == null)
                {
                    throw new KeyNotFoundException("DegreePlan");
                }
            }

            // Get all terms for later use.
            var terms = await _termRepository.GetAsync();

            // Get all of the student's programs
            var studentPrograms = await _studentProgramRepository.GetStudentProgramsByIdsAsync(new List<string>() { student.Id });
            if (studentPrograms == null || studentPrograms.Count() == 0)
            {
                throw new KeyNotFoundException("StudentPrograms");
            }

            foreach (var programCode in programCodes)
            {
                IEnumerable<AcademicCredit> FilteredCredits = new List<AcademicCredit>();
                List<AcademicCredit> CreditsExcludedFromTranscriptGrouping = new List<AcademicCredit>();
                StudentProgram StudentProgram;
                Program Program;
                IEnumerable<PlannedCredit> FilteredPlannedCourses = new List<PlannedCredit>();
                List<Override> Overrides = new List<Override>();

                Domain.Student.Entities.ProgramEvaluation eval = null;
                // Get the program itself
                Program = await _programRepository.GetAsync(programCode);
                //filter academic credits based upon transcript grouping settings- TRGR
                FilteredCredits = await FilterCreditsOnTranscriptGroupingAsync(credits, Program);


                // Get the student's program from the list of all of the student's programs
                StudentProgram = studentPrograms.Where(sp => sp.ProgramCode == programCode).FirstOrDefault();

                string catalogCode = null;
                if (StudentProgram == null || !string.IsNullOrEmpty(catalogYear))
                {
                    // If the student program isn't found, set up for a "what-if?" evaluation.
                    // Determine the program catalog year to use.
                    PlanningConfiguration planningConfiguration = await _configurationRepository.GetPlanningConfigurationAsync();

                    if (string.IsNullOrEmpty(catalogYear))
                    {
                        ICollection<Catalog> catalogs = await _catalogRepository.GetAsync();
                        catalogCode = ProgramCatalogService.DeriveDefaultCatalog(Program, studentPrograms, catalogs, planningConfiguration.DefaultCatalogPolicy, logger);
                    }
                    else
                    {
                        catalogCode = catalogYear;
                    }

                    // Build a temporary student program for what-if evaluation
                    // Note: Catalog code above could come back null - constructor for the new program will throw an exception.
                    StudentProgram = new StudentProgram(studentId, programCode, catalogCode);
                }

                // Get the student's overrides (if any)
                if (StudentProgram.Overrides != null && StudentProgram.Overrides.Count() > 0)
                {
                    Overrides = StudentProgram.Overrides.ToList();
                }

                //after retrieving overrides validate if any course that was filtered out from transcript grouping is in override credits allowed list and if it is then add to new collection
                //if credits allowed in override is not in filtered credits list but is in raw credit list then add the credit from raw to new collection

                foreach (var creditAllowed in Overrides.Where(o => o != null).SelectMany(o => o.CreditsAllowed))
                {
                    if (creditAllowed != null && FilteredCredits != null && !FilteredCredits.Any(acr => !string.IsNullOrEmpty(acr.Id) && acr.Id == creditAllowed))
                    {
                        var excludedCredit = credits.Where(cr => !string.IsNullOrEmpty(cr.Id) && cr.Id == creditAllowed).FirstOrDefault();
                        if (excludedCredit != null && !string.IsNullOrEmpty(excludedCredit.Id))
                        {
                            CreditsExcludedFromTranscriptGrouping.Add(excludedCredit);
                        }

                    }
                }

                // Get only the planned courses that are not in credits and will be complete by the given date
                if (degreePlan != null)
                {
                    // Get the planned courses that should be considered by this evaluation
                    var UnfilteredPlannedCourses = degreePlan.GetCoursesForValidation(terms, null, FilteredCredits, courses);
                    //filter planned courses based upon transcript groupings - TRGR
                    FilteredPlannedCourses = await FilterPlannedCoursesOnTranscriptGroupingAsync(UnfilteredPlannedCourses, Program);
                }

                // If the client has modified the sort spec, be sure DEFAULT is in the list of sortSpecIds
                var degreeAuditParameters = await _requirementRepository.GetDegreeAuditParametersAsync();
                bool modifiedSortSpec = degreeAuditParameters != null ? degreeAuditParameters.ModifiedDefaultSort : false;
                bool excludeCompletedReplaceInProgressCreditsFromGPA = degreeAuditParameters != null ? degreeAuditParameters.ExcludeCompletedPossibleReplaceInProgressCoursesFromGPA : false;
                bool applyRepeatedCreditsOverPlannedCourse = degreeAuditParameters != null ? degreeAuditParameters.ApplyRepeatedCreditsOverPlannedCourse : false;

                // Get the main requirements from the student's program
                ProgramRequirements ProgramRequirements = null;
                try
                {
                    ProgramRequirements = await _programRequirementsRepository.GetAsync(StudentProgram.ProgramCode, StudentProgram.CatalogCode);
                    /// If the Default sort spec has been modified, apply DEFAULT to all requirements missing a sort spec and then cascade it down to lower levels missing a sort spec
                    ApplyDefaultSortSpecWhereApplicable(modifiedSortSpec, ProgramRequirements.Requirements);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Requirements not found: Null program requirements will result in exit with empty program.");
                }

                // Get any additional requirements
                List<Requirement> AdditionalRequirements = new List<Requirement>();
                var additionalRequirementIds = StudentProgram.AdditionalRequirements.Where(r => !string.IsNullOrEmpty(r.RequirementCode)).Select(r => r.RequirementCode).ToList();
                if (additionalRequirementIds.Count() > 0)
                {
                    // Make sure to apply default sort specifications if it has been modified.
                    AdditionalRequirements = (await _requirementRepository.GetAsync(additionalRequirementIds)).ToList();

                    /// If the Default sort spec has been modified, apply DEFAULT to all addl requirements missing a sort spec 
                    ApplyDefaultSortSpecWhereApplicable(modifiedSortSpec, AdditionalRequirements);
                }

                // If we don't have any requirements or additional requirements then create an empty program evaluation for this program.
                if (ProgramRequirements == null || StudentProgram == null ||
                    (ProgramRequirements.Requirements == null && ProgramRequirements.Requirements.Count() == 0) &&
                    (StudentProgram.AdditionalRequirements == null && StudentProgram.AdditionalRequirements.Count() == 0))
                {
                    // Create a blank evaluation result and loop to the next program code.
                    eval = new Domain.Student.Entities.ProgramEvaluation(new List<AcademicCredit>(), programCode, StudentProgram != null ? StudentProgram.CatalogCode : string.Empty);
                    evaluations.Add(eval);
                    continue;
                }

                // Build rules (if any) and evaluate the program.

                // Get rules for program requirements
                var allRules = new List<RequirementRule>();

                if (ProgramRequirements != null)
                {
                    allRules.AddRange(ProgramRequirements.GetAllRules());
                }
                // Get rules for additional requirements
                if (AdditionalRequirements != null)
                {
                    foreach (var req in AdditionalRequirements)
                    {
                        allRules.AddRange(req.GetAllRules());
                    }
                }

                var creditRequests = new List<RuleRequest<AcademicCredit>>();

                // Run all credits against credit rules
                foreach (var credit in credits)
                {
                    var creditRules = allRules.Where(rr => rr.CreditRule != null).ToList();
                    foreach (var rule in creditRules)
                    {
                        creditRequests.Add(new RuleRequest<AcademicCredit>(rule.CreditRule, credit));
                    }
                }

                // Create hard list of rules that are course rules
                var courseRules = allRules.Where(rr => rr.CourseRule != null).ToList();

                // Run all courses from credits against course rules
                var courseRequests = new List<RuleRequest<Course>>();
                var creditsWithCourse = credits.Where(stc => stc.Course != null).Select(stc => stc.Course).ToList();
                foreach (var rule in courseRules)
                {
                    foreach (var creditCourse in creditsWithCourse)
                    {
                        courseRequests.Add(new RuleRequest<Course>(rule.CourseRule, creditCourse));
                    }
                }

                // Run all planned courses against course rules
                foreach (var rule in courseRules)
                {
                    foreach (var plannedCourse in FilteredPlannedCourses)
                    {
                        courseRequests.Add(new RuleRequest<Course>(rule.CourseRule, plannedCourse.Course));
                    }
                }

                // Execute all the rules. The rule repository will not need to get the ones with .NET expressions
                var courseResults = await _ruleRepository.ExecuteAsync(courseRequests);
                var creditResults = await _ruleRepository.ExecuteAsync(creditRequests);
                var ruleResults = courseResults.Union(creditResults);

                Dictionary<string, List<AcademicCredit>> filteredCreditsDict = null;
                if (FilteredCredits != null && FilteredCredits.Any())
                {
                    // If the student has any academic credits, build a dictionary of sorted filtered credits, using only unique, non-null/non-empty spec IDs
                    var reqSortSpecs = ProgramRequirements.Requirements.Select(r => r.SortSpecificationId).ToList();
                    var subreqSortSpecs = ProgramRequirements.Requirements.SelectMany(r => r.SubRequirements).Select(sub => sub.SortSpecificationId).ToList();
                    var groupSortSpecs = ProgramRequirements.Requirements.SelectMany(r => r.SubRequirements).SelectMany(sub => sub.Groups).Select(g => g.SortSpecificationId).ToList();
                    var additionalReqSortSpecs = AdditionalRequirements.Select(ar => ar.SortSpecificationId).ToList();
                    var sortSpecIds = reqSortSpecs.Union(subreqSortSpecs).Union(groupSortSpecs).Union(additionalReqSortSpecs).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();

                    if (modifiedSortSpec && !sortSpecIds.Contains(_DefaultSortSpecId))
                    {
                        sortSpecIds.Add(_DefaultSortSpecId);
                    }
                    try
                    {
                        filteredCreditsDict = await _academicCreditRepository.GetSortedAcademicCreditsBySortSpecificationIdAsync(FilteredCredits, sortSpecIds);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Either there are no credits to sort or it couldn't sort them using the sort specs. Either way, continue without sort overrides.");
                    }
                }
                //pass filteredcredits and filteredplannedcourses to identify which one will be replacing the other
                try
                {
                    UpdateCreditsReplaceStatus(FilteredCredits, FilteredPlannedCourses, terms, excludeCompletedReplaceInProgressCreditsFromGPA, applyRepeatedCreditsOverPlannedCourse);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception occurred while processing possible replace and replacement in progress for credits and planned courses.");
                }

                // All pieces in place, run the eval
                var ProgramEvaluator = new Domain.Student.Services.ProgramEvaluator(StudentProgram, ProgramRequirements, AdditionalRequirements, FilteredCredits, FilteredPlannedCourses, ruleResults, Overrides, CreditsExcludedFromTranscriptGrouping, courses, degreeAuditParameters, logger);

                if (logger.IsInfoEnabled) watch.Restart();

                eval = ProgramEvaluator.Evaluate(filteredCreditsDict);

                if (logger.IsInfoEnabled)
                {
                    watch.Stop();
                    logger.Info("EvaluationTiming: (EvaluatePrograms)(programCode " + programCode + ") Completed ProgramEvaluator.Evaluate in " + watch.ElapsedMilliseconds.ToString() + " ms");
                }

                // Optimize credit/course application
                List<string> groupsToSkip = new List<string>();
                List<string> subsToSkip = new List<string>();

                if (OptimizeEval(eval, groupsToSkip, subsToSkip))
                {
                    if (subsToSkip.Count > 0) { ProgramEvaluator.AddSubRequirementsToSkip(subsToSkip); }
                    eval = null;

                    watch.Restart();

                    eval = ProgramEvaluator.Evaluate(filteredCreditsDict);

                    watch.Stop();
                    logger.Info("EvaluationTiming: (EvaluatePrograms)(programCode " + programCode + ") Completed OptimizeEval ProgramEvaluator.Evaluate in " + watch.ElapsedMilliseconds.ToString() + " ms");
                }

                evaluations.Add(eval);
            }

            return evaluations;
        }

        /// <summary>
        /// running evaluations for an applicant
        /// </summary>
        /// <param name="applicantId"></param>
        /// <param name="programCodes"></param>
        /// <param name="applicant"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Domain.Student.Entities.ProgramEvaluation>> EvaluateApplicantProgramsAsync(
          string applicantId, List<string> programCodes, Domain.Student.Entities.Applicant applicant, string catalogYear)
        {
            var evaluations = new List<Domain.Student.Entities.ProgramEvaluation>();

            var watch = new Stopwatch();

            // Get the student's academic credits.
            List<AcademicCredit> credits = new List<AcademicCredit>();

            var creditsDict = await _academicCreditRepository.GetAcademicCreditByStudentIdsAsync(new List<string>() { applicantId }, false, true, false);
            if (creditsDict.ContainsKey(applicantId))
            {
                credits = creditsDict[applicantId];
            }

            // List of courses needed for the evaluation (added for evaluating equated courses)
            IEnumerable<Course> courses = new List<Course>();
            courses = await _courseRepository.GetAsync();


            // Get all terms for later use.
            var terms = await _termRepository.GetAsync();

            // Get all of the student's programs
            var studentPrograms = await _studentProgramRepository.GetApplicantProgramsAsync(applicantId, true, true);

            foreach (var programCode in programCodes)
            {
                IEnumerable<AcademicCredit> FilteredCredits = new List<AcademicCredit>();
                StudentProgram StudentProgram;
                Program Program;
                List<AcademicCredit> CreditsExcludedFromTranscriptGrouping = new List<AcademicCredit>();
                List<Override> Overrides = new List<Override>();

                Domain.Student.Entities.ProgramEvaluation eval = null;
                // Get the program itself
                Program = await _programRepository.GetAsync(programCode);
                //filter academic credits based upon transcript grouping settings- TRGR
                FilteredCredits = await FilterCreditsOnTranscriptGroupingAsync(credits, Program);

                // Get the student's program from the list of all of the student's programs
                StudentProgram = studentPrograms != null ? studentPrograms.Where(sp => sp.ProgramCode == programCode).FirstOrDefault() : null;

                string catalogCode = null;


                if (string.IsNullOrEmpty(catalogYear))
                {
                    ICollection<Catalog> catalogs = await _catalogRepository.GetAsync();
                    string defaultCatalog = Program.GetCurrentCatalogCode(catalogs);
                    catalogCode = defaultCatalog;

                }
                else
                {
                    catalogCode = catalogYear;
                }


                // Build a temporary student program for what-if evaluation
                // Note: Catalog code above could come back null - constructor for the new program will throw an exception.
                if (StudentProgram == null)
                {
                    StudentProgram = new StudentProgram(applicantId, programCode, catalogCode);
                }

                // Get the student's overrides (if any)
                if (StudentProgram.Overrides != null && StudentProgram.Overrides.Count() > 0)
                {
                    Overrides = StudentProgram.Overrides.ToList();
                }

                //after retrieving overrides validate if any course that was filtered out from transcript grouping is in override credits allowed list and if it is then add to new collection
                //if credits allowed in override is not in filtered credits list but is in raw credit list then add the credit from raw to new collection

                foreach (var creditAllowed in Overrides.Where(o => o != null).SelectMany(o => o.CreditsAllowed))
                {
                    if (creditAllowed != null && FilteredCredits != null && !FilteredCredits.Any(acr => !string.IsNullOrEmpty(acr.Id) && acr.Id == creditAllowed))
                    {
                        var excludedCredit = credits.Where(cr => !string.IsNullOrEmpty(cr.Id) && cr.Id == creditAllowed).FirstOrDefault();
                        if (excludedCredit != null && !string.IsNullOrEmpty(excludedCredit.Id))
                        {
                            CreditsExcludedFromTranscriptGrouping.Add(excludedCredit);
                        }

                    }
                }



                // If the client has modified the sort spec, be sure DEFAULT is in the list of sortSpecIds
                var degreeAuditParameters = await _requirementRepository.GetDegreeAuditParametersAsync();
                bool modifiedSortSpec = degreeAuditParameters != null ? degreeAuditParameters.ModifiedDefaultSort : false;
                bool excludeCompletedReplaceInProgressCreditsFromGPA = degreeAuditParameters != null ? degreeAuditParameters.ExcludeCompletedPossibleReplaceInProgressCoursesFromGPA : false;
                bool applyRepeatedCreditsOverPlannedCourse = degreeAuditParameters != null ? degreeAuditParameters.ApplyRepeatedCreditsOverPlannedCourse : false;

                // Get the main requirements from the student's program
                ProgramRequirements ProgramRequirements = null;
                try
                {
                    ProgramRequirements = await _programRequirementsRepository.GetAsync(StudentProgram.ProgramCode, StudentProgram.CatalogCode);
                    /// If the Default sort spec has been modified, apply DEFAULT to all requirements missing a sort spec and then cascade it down to lower levels missing a sort spec
                    ApplyDefaultSortSpecWhereApplicable(modifiedSortSpec, ProgramRequirements.Requirements);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Requirements not found: Null program requirements will result in exit with empty program.");
                }
                // Get any additional requirements
                List<Requirement> AdditionalRequirements = new List<Requirement>();
                var additionalRequirementIds = StudentProgram.AdditionalRequirements.Where(r => !string.IsNullOrEmpty(r.RequirementCode)).Select(r => r.RequirementCode).ToList();
                if (additionalRequirementIds.Count() > 0)
                {
                    // Make sure to apply default sort specifications if it has been modified.
                    AdditionalRequirements = (await _requirementRepository.GetAsync(additionalRequirementIds)).ToList();

                    /// If the Default sort spec has been modified, apply DEFAULT to all addl requirements missing a sort spec 
                    ApplyDefaultSortSpecWhereApplicable(modifiedSortSpec, AdditionalRequirements);
                }


                // If we don't have any requirements or additional requirements then create an empty program evaluation for this program.
                if (ProgramRequirements == null || StudentProgram == null ||
                    (ProgramRequirements.Requirements == null && ProgramRequirements.Requirements.Count() == 0))
                {
                    // Create a blank evaluation result and loop to the next program code.
                    eval = new Domain.Student.Entities.ProgramEvaluation(new List<AcademicCredit>(), programCode, StudentProgram != null ? StudentProgram.CatalogCode : string.Empty);
                    evaluations.Add(eval);
                    continue;
                }

                // Build rules (if any) and evaluate the program.

                // Get rules for program requirements
                var allRules = new List<RequirementRule>();

                if (ProgramRequirements != null)
                {
                    allRules.AddRange(ProgramRequirements.GetAllRules());
                }

                var creditRequests = new List<RuleRequest<AcademicCredit>>();

                // Run all credits against credit rules
                foreach (var credit in credits)
                {
                    var creditRules = allRules.Where(rr => rr.CreditRule != null).ToList();
                    foreach (var rule in creditRules)
                    {
                        creditRequests.Add(new RuleRequest<AcademicCredit>(rule.CreditRule, credit));
                    }
                }

                // Create hard list of rules that are course rules
                var courseRules = allRules.Where(rr => rr.CourseRule != null).ToList();

                // Run all courses from credits against course rules
                var courseRequests = new List<RuleRequest<Course>>();
                var creditsWithCourse = credits.Where(stc => stc.Course != null).Select(stc => stc.Course).ToList();
                foreach (var rule in courseRules)
                {
                    foreach (var creditCourse in creditsWithCourse)
                    {
                        courseRequests.Add(new RuleRequest<Course>(rule.CourseRule, creditCourse));
                    }
                }


                // Execute all the rules. The rule repository will not need to get the ones with .NET expressions
                var courseResults = await _ruleRepository.ExecuteAsync(courseRequests);
                var creditResults = await _ruleRepository.ExecuteAsync(creditRequests);
                var ruleResults = courseResults.Union(creditResults);

                Dictionary<string, List<AcademicCredit>> filteredCreditsDict = null;
                if (FilteredCredits != null && FilteredCredits.Any())
                {
                    // If the student has any academic credits, build a dictionary of sorted filtered credits, using only unique, non-null/non-empty spec IDs
                    var reqSortSpecs = ProgramRequirements.Requirements.Select(r => r.SortSpecificationId).ToList();
                    var subreqSortSpecs = ProgramRequirements.Requirements.SelectMany(r => r.SubRequirements).Select(sub => sub.SortSpecificationId).ToList();
                    var groupSortSpecs = ProgramRequirements.Requirements.SelectMany(r => r.SubRequirements).SelectMany(sub => sub.Groups).Select(g => g.SortSpecificationId).ToList();
                    var additionalReqSortSpecs = AdditionalRequirements.Select(ar => ar.SortSpecificationId).ToList();
                    var sortSpecIds = reqSortSpecs.Union(subreqSortSpecs).Union(groupSortSpecs).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();

                    if (modifiedSortSpec && !sortSpecIds.Contains(_DefaultSortSpecId))
                    {
                        sortSpecIds.Add(_DefaultSortSpecId);
                    }
                    try
                    {
                        filteredCreditsDict = await _academicCreditRepository.GetSortedAcademicCreditsBySortSpecificationIdAsync(FilteredCredits, sortSpecIds);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Either there are no credits to sort or it couldn't sort them using the sort specs. Either way, continue without sort overrides.");
                    }
                }
                //pass filteredcredits and filteredplannedcourses to identify which one will be replacing the other
                try
                {
                    UpdateCreditsReplaceStatus(FilteredCredits, null, terms, excludeCompletedReplaceInProgressCreditsFromGPA, applyRepeatedCreditsOverPlannedCourse);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception occurred while processing possible replace and replacement in progress for credits and planned courses.");
                }
                // All pieces in place, run the eval
                var ProgramEvaluator = new Domain.Student.Services.ProgramEvaluator(StudentProgram, ProgramRequirements, AdditionalRequirements, FilteredCredits, new List<PlannedCredit>(), ruleResults, Overrides, CreditsExcludedFromTranscriptGrouping, courses, degreeAuditParameters, logger);


                if (logger.IsInfoEnabled) watch.Restart();

                eval = ProgramEvaluator.Evaluate(filteredCreditsDict);

                if (logger.IsInfoEnabled)
                {
                    watch.Stop();
                    logger.Info("EvaluationTiming: (EvaluatePrograms)(programCode " + programCode + ") Completed ProgramEvaluator.Evaluate in " + watch.ElapsedMilliseconds.ToString() + " ms");
                }

                // Optimize credit/course application
                List<string> groupsToSkip = new List<string>();
                List<string> subsToSkip = new List<string>();

                if (OptimizeEval(eval, groupsToSkip, subsToSkip))
                {
                    if (subsToSkip.Count > 0) { ProgramEvaluator.AddSubRequirementsToSkip(subsToSkip); }
                    eval = null;

                    watch.Restart();

                    eval = ProgramEvaluator.Evaluate(filteredCreditsDict);

                    watch.Stop();
                    logger.Info("EvaluationTiming: (EvaluatePrograms)(programCode " + programCode + ") Completed OptimizeEval ProgramEvaluator.Evaluate in " + watch.ElapsedMilliseconds.ToString() + " ms");
                }

                evaluations.Add(eval);
            }

            return evaluations;
        }

        // Identifies the list of groups and subrequirements that do not need to have items applied to them because the requirement or subrequirement
        // (respectively) is satisfied. Create a list of these items that can be ignored the next time through the evaluation.
        private bool OptimizeEval(Domain.Student.Entities.ProgramEvaluation eval, List<string> groupsToSkip, List<string> subsToSkip)
        {
            // If there are any requirements that are satisfied and don't require all of their subrequirements to be completed
            var satisfiedReqsWithEnoughSubreqs = eval.RequirementResults.Where(rq => rq.IsSatisfied() && (rq.Requirement.MinSubRequirements < rq.Requirement.SubRequirements.Count)).ToList();
            foreach (var req in satisfiedReqsWithEnoughSubreqs)
            {
                // For what its worth, the first pass would have stopped evaluating groups as soona as the requirement was satisfied, but there may be some stray
                // partially satisfied subrequirements from which we can free up courses/credits.
                var unsatisfiedSubreqs = req.SubRequirementResults.Where(sb => !sb.IsSatisfied()).ToList();
                foreach (var sub in unsatisfiedSubreqs)
                {
                    // just for clarity's sake, let's only "skip" this if it had something applied to it, otherwise
                    // the optimizer will look like it is doing this a lot more often than it really needs to
                    if (sub.PlanningStatus == PlanningStatus.PartiallyPlanned || sub.CompletionStatus != CompletionStatus.PartiallyCompleted)
                    {
                        subsToSkip.Add(sub.SubRequirement.Id);
                    }
                }
            }

            return (subsToSkip.Count > 0);
        }


        private async Task<IDictionary<Course, Tuple<IEnumerable<Department>, IEnumerable<string>>>> CreateTranscriptGroupingLookupsAsync(IEnumerable<PlannedCredit> EvaluationPlannedCourses)
        {
            try
            {
                //Filter planned courses on divisions and schools. Division and Schools are set at department level
                //first retrieve departments details only for the planned courses
                //create a dictionary beforehand with course and departments associated with those course
                IEnumerable<Department> departments = (await _referenceDataRepository.DepartmentsAsync());
                IDictionary<Course, Tuple<IEnumerable<Department>, IEnumerable<string>>> deptLookup = new Dictionary<Course, Tuple<IEnumerable<Department>, IEnumerable<string>>>();
                foreach (PlannedCredit pc in EvaluationPlannedCourses)
                {
                    List<string> schools = new List<string>();
                    IEnumerable<string> courseDepartments = pc.Course.DepartmentCodes.Distinct();
                    IEnumerable<Department> academicDepartments = departments.Where(d => courseDepartments.Contains(d.Code));
                    if (!deptLookup.ContainsKey(pc.Course))
                    {
                        //get schools from divisions

                        IEnumerable<string> divisions = academicDepartments.Where(ad => !string.IsNullOrEmpty(ad.Division)).Select(a => a.Division);
                        if (divisions != null && divisions.Any())
                        {
                            //retrieve schools from divisions
                            List<Division> allDivisions = (await _referenceDataRepository.GetDivisionsAsync(false)).Where(d => divisions.Contains(d.Code)).ToList();
                            if (allDivisions != null && allDivisions.Any())
                            {
                                schools.AddRange(allDivisions.Select(d => d.SchoolCode).Distinct());
                            }
                        }

                        schools = schools.Union(academicDepartments.Where(d => !string.IsNullOrEmpty(d.School)).Select(d => d.School).Distinct()).ToList<string>();



                        deptLookup.Add(pc.Course, Tuple.Create<IEnumerable<Department>, IEnumerable<string>>(academicDepartments, schools));
                    }

                }
                return deptLookup;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Determine if the DEFAULT sort spec Id should be applied to any requirements that do not have a sort spec Id of there own.
        /// </summary>
        /// <param name="modifiedSortSpec"></param>
        /// <param name="requirements"></param>
        private void ApplyDefaultSortSpecWhereApplicable(bool modifiedSortSpec, IEnumerable<Requirement> requirements)
        {
            if (modifiedSortSpec && requirements != null && requirements.Any())
            {
                foreach (var req in requirements)
                {
                    if (string.IsNullOrEmpty(req.SortSpecificationId))
                    {
                        req.SortSpecificationId = _DefaultSortSpecId;
                        req.CascadeSortSpecificationWhenNecessary();
                    }
                }
            }
        }

        /// <summary>
        /// This method updates credits and planned courses replaced and replacement status.
        /// If a course cannot be retaken for credits but same course is taken multiple times then only one credit will be applied and marked as if its possible replacement
        /// and others credits will be marked as replace in progress.
        /// There is nothing to do for the credits that are already marked as "replaced" by Collegue on STRP screen. In this case if all the credits are completed and marked as Replaced by colleague then only one credit
        /// will be marked as "Replacement" and rest of them will remain "Replaced" .
        /// There are scenarios where credits are repeated and completed but are not marked as Replaced by Colleague like in AVG grade policy, in that case repeats is not considered.
        /// There could be scenarios where few credits are marked as Replaced and few are graded and completed but not marked as replaced like when a course was graded F and Repeat value or grade value on GRDC for F grade was empty.
        /// If there are planned courses and course retake is not allowed then planned course will also pass through replacement process and will take precedence over inprogress and completed courses
        /// </summary>


        private void UpdateCreditsReplaceStatus(IEnumerable<AcademicCredit> allCredits, IEnumerable<PlannedCredit> plannedCourses, IEnumerable<Term> terms, bool excludeCompletedReplaceInProgressCreditsFromGPA = false, bool applyRepeatedCreditsOverPlannedCourse = false)
        {
            logger.Info("Starting updating replace and replacement statuses for credits and planned courses");
            if (allCredits == null)
            {
                allCredits = new List<AcademicCredit>();
            }
            if (plannedCourses == null)
            {
                plannedCourses = new List<PlannedCredit>();
            }
            //filter out dropped credits
            IEnumerable<AcademicCredit> filteredCredits = allCredits.Where(c => c != null && c.Status != CreditStatus.Dropped && c.Status != CreditStatus.Deleted && c.Status != CreditStatus.Withdrawn && c.Status != CreditStatus.Cancelled);

            //group all the credits on course key.
            //only those credits are needed to be added to group when course does not allow retakes for credits and credits have list of other repeated credit ids was an old logic but is removed in the below condition 
            // because if there is only one in-progress or completed course, it will have canBeReplaced flag set to yes but collection will be empty.
            //inorder to use this course and compare with planned one, we need to pick it so that it can be made as possible replacement,

            ILookup<string, AcademicCredit> groupedCredits = filteredCredits.Where(c => (c.CanBeReplaced && c.ReplacedStatus == ReplacedStatus.NotReplaced && c.ReplacementStatus == ReplacementStatus.NotReplacement && c.Course != null && !c.Course.AllowToCountCourseRetakeCredits)).ToLookup(c => c.Course.Id, c => c);
            //grouping planned courses such as with each course we have associated planned course with its term start date to sort the planned credits with respect to each course
            ILookup<string, Tuple<PlannedCredit, DateTime>> groupedPlannedCourses = plannedCourses.Where(c => !c.Course.AllowToCountCourseRetakeCredits).ToLookup(c => c.Course.Id, c => Tuple.Create<PlannedCredit, DateTime>(c, terms.Where(t => t.Code == c.TermCode).Select(t => t.StartDate).First()));

            if ((groupedCredits == null || !groupedCredits.Any()) && (groupedPlannedCourses == null || !groupedPlannedCourses.Any()))
            {
                return;
            }
            //collecting all the course ids
            List<string> allTheCourses = groupedCredits.Select(g => g.Key).Union(groupedPlannedCourses.Select(s => s.Key)).Distinct().ToList();
            //process academic credits to set replaced or relacement status
            if (groupedCredits != null && groupedCredits.Any())
            {
                //process each grouped course to mark credits replace/replacement status
                foreach (var creditsLst in groupedCredits)
                {
                    logger.Info(string.Format("processing to find possible replace in progress/possible replacement for course {0} ", creditsLst.Key));
                    //add equated course credits to above list
                    //find all the credits that are equated to current course
                    var equatedCredits = filteredCredits.Where(c => c.Course != null && c.Course.EquatedCourseIds != null && c.Course.EquatedCourseIds.Contains(creditsLst.Key));
                    var creditsToProcess = groupedCredits[creditsLst.Key].ToList();
                    if (equatedCredits != null && equatedCredits.Any())
                    {
                        creditsToProcess.AddRange(equatedCredits.Where(e => e != null && e.ReplacedStatus != ReplacedStatus.Replaced && e.CanBeReplaced));
                    }
                    var orderedCreditsToProcess = creditsToProcess.OrderBy(c => c.StartDate).ToList();

                    if (orderedCreditsToProcess != null && orderedCreditsToProcess.Any())
                    {
                        var currentCredit = orderedCreditsToProcess[0];

                        //if all the credits to process are completed credits then mark all as replacement
                        int completedCredits = orderedCreditsToProcess.Where(o => o.IsCompletedCredit).Count();
                        if (completedCredits > 0 && completedCredits == orderedCreditsToProcess.Count())
                        {
                            logger.Info("All the credits are already completed for course : " + creditsLst.Key);

                            //find if any other repeated credits are replaced and completed
                            List<string> otherRepeatedCredits = currentCredit.RepeatAcademicCreditIds.Where(c => c != currentCredit.Id).ToList();
                            if (otherRepeatedCredits.Any())
                            {
                                int countOfCreditsReplaced = allCredits.Where(c => otherRepeatedCredits.Contains(c.Id) && c.ReplacedStatus == ReplacedStatus.Replaced && c.IsCompletedCredit).Count();
                                if (countOfCreditsReplaced > 0)
                                {
                                    logger.Info("All the credits are complete for course therefore updating them with replacement status for course : " + creditsLst.Key);
                                    //mark all the completed credits as Replacement
                                    orderedCreditsToProcess.ForEach(o => o.ReplacementStatus = ReplacementStatus.Replacement);
                                }
                            }
                        }
                        else if (orderedCreditsToProcess.Count > 1)
                        {
                            logger.Info("Processing credits to determine replace/replacement statuses for course: " + creditsLst.Key);
                            var nextCredit = orderedCreditsToProcess[1];
                            updateReplaceStatus(currentCredit, nextCredit, orderedCreditsToProcess, excludeCompletedReplaceInProgressCreditsFromGPA);
                        }
                    }
                    logger.Info("All the credits have appropriately set for replace/replacement statuses for the course: " + creditsLst.Key);
                }
            }

            //now process planned courses to set replaced or replacement status
            if (groupedPlannedCourses != null && groupedPlannedCourses.Any())
            {
                logger.Info("Processing repeated planned courses for finding replace/repalcement statuses");
                //process each grouped course to mark credits replace/replacement status
                foreach (var plannedCoursesLst in groupedPlannedCourses)
                {
                    var coursesToProcess = groupedPlannedCourses[plannedCoursesLst.Key].ToList();
                    //ordering all the planned credits for each grouped course on its term start date
                    coursesToProcess = coursesToProcess.OrderBy(c => c.Item2).ToList();
                    if (coursesToProcess != null && coursesToProcess.Any())
                    {
                        int index = 0;
                        //loop to continue replacing prior planned course until left with only one
                        for (index = 0; index < coursesToProcess.Count() - 1; index = index + 1)
                        {
                            var currentCourse = coursesToProcess[index];
                            logger.Info("Setting the repelace/replacement status of planned course: " + currentCourse.Item1.Course.Id);

                            currentCourse.Item1.ReplacedStatus = ReplacedStatus.ReplaceInProgress;
                            currentCourse.Item1.ReplacementStatus = ReplacementStatus.NotReplacement;
                        }
                        //if index is > 0 it means there were more than 1 repeated planned course; therefore set the last one after the loop as possible replacement
                        //otherwise only one planned course will have no replacement or replaced status set
                        if (index > 0)
                        {
                            coursesToProcess[index].Item1.ReplacedStatus = ReplacedStatus.NotReplaced;
                            coursesToProcess[index].Item1.ReplacementStatus = ReplacementStatus.PossibleReplacement;
                        }
                        else
                        {
                            coursesToProcess[index].Item1.ReplacedStatus = ReplacedStatus.NotReplaced;
                            coursesToProcess[index].Item1.ReplacementStatus = ReplacementStatus.NotReplacement;

                        }
                        logger.Info(string.Format("Replaced Status of planned course: {0} is {1} ", coursesToProcess[index].Item1.Course.Id, coursesToProcess[index].Item1.ReplacedStatus));
                        logger.Info(string.Format("Replacement Status of planned course: {0} is {1} ", coursesToProcess[index].Item1.Course.Id, coursesToProcess[index].Item1.ReplacementStatus));
                    }
                }
            }
            //now find which one to take if already there is possible replacement in academic credits - take planned one over completed/inprogress course
            foreach (string course in allTheCourses)
            {
                logger.Info("Going through each course to determine which one will apply over the other - if it planned course or credits that will replace each other. Processing for course: " + course);
                var possibleCreditReplacement = groupedCredits[course].Where(c => (c.ReplacementStatus == ReplacementStatus.PossibleReplacement || (c.ReplacedStatus == ReplacedStatus.NotReplaced && c.ReplacementStatus == ReplacementStatus.NotReplacement) || c.ReplacementStatus == ReplacementStatus.Replacement)).FirstOrDefault();
                if (possibleCreditReplacement != null)
                {
                    var possiblePlannedCourseReplacement = groupedPlannedCourses[course].Where(c => c.Item1.ReplacementStatus == ReplacementStatus.PossibleReplacement || (c.Item1.ReplacedStatus == ReplacedStatus.NotReplaced && c.Item1.ReplacementStatus == ReplacementStatus.NotReplacement)).FirstOrDefault();
                    if (possiblePlannedCourseReplacement != null)
                    {
                        //If on AEDF flag is false then planned course will replace credit course otherwise credit course will take precedence over planned course
                        if (applyRepeatedCreditsOverPlannedCourse == false)
                        {
                            logger.Info("Taking planned course over credit for course: " + course);
                            possiblePlannedCourseReplacement.Item1.ReplacedStatus = ReplacedStatus.NotReplaced;
                            possiblePlannedCourseReplacement.Item1.ReplacementStatus = ReplacementStatus.PossibleReplacement;
                            possibleCreditReplacement.ReplacedStatus = ReplacedStatus.ReplaceInProgress;
                            possibleCreditReplacement.ReplacementStatus = ReplacementStatus.NotReplacement;
                            //if planned course is replacing completed course then adjust the credits and gpa of completed course to 0
                            if (possibleCreditReplacement.IsCompletedCredit)
                            {
                                possibleCreditReplacement.AdjustedCredit = 0M;
                                if (excludeCompletedReplaceInProgressCreditsFromGPA == true)
                                {
                                    possibleCreditReplacement.AdjustedGpaCredit = 0m;
                                    possibleCreditReplacement.AdjustedGradePoints = 0m;
                                }
                            }
                        }
                        else
                        {
                            logger.Info("Taking in-progress/completed/registerd course over planned course for course: " + course);
                            possiblePlannedCourseReplacement.Item1.ReplacedStatus = ReplacedStatus.ReplaceInProgress;
                            possiblePlannedCourseReplacement.Item1.ReplacementStatus = ReplacementStatus.NotReplacement;
                            possibleCreditReplacement.ReplacedStatus = ReplacedStatus.NotReplaced;
                            possibleCreditReplacement.ReplacementStatus = ReplacementStatus.PossibleReplacement;
                        }
                        logger.Info(string.Format("Replaced Status of planned course: {0} is {1} ", possiblePlannedCourseReplacement.Item1.Course.Id, possiblePlannedCourseReplacement.Item1.ReplacedStatus));
                        logger.Info(string.Format("Replacement Status of planned course: {0} is {1} ", possiblePlannedCourseReplacement.Item1.Course.Id, possiblePlannedCourseReplacement.Item1.ReplacementStatus));
                        logger.Info(string.Format("Replaced Status of credit with Id {0} is {1} ", possibleCreditReplacement.Id, possibleCreditReplacement.ReplacedStatus));
                        logger.Info(string.Format("Replacement Status of credit with Id {0} is {1} ", possibleCreditReplacement.Id, possibleCreditReplacement.ReplacementStatus));

                    }

                }

            }
        }
        /// <summary>
        /// This is recursive method that marks the status of credits on basis of  replace and replacement.
        /// Comparison happens between current credit and next credit in sequence
        /// </summary>
        /// <param name="currentCredit"></param>
        /// <param name="nextCredit"></param>
        /// <param name="creditsToProcess"></param>
        private void updateReplaceStatus(AcademicCredit currentCredit, AcademicCredit nextCredit, List<AcademicCredit> creditsToProcess, bool excludeCompletedReplaceInProgressCreditsFromGPA = false)
        {
            AcademicCredit creditCompared = null;
            AcademicCredit creditToCompareWith = null;
            if (nextCredit == null || currentCredit == null || creditsToProcess == null)
            {
                return;
            }
            logger.Info(string.Format("Comparing credit {0} with next credit {1} to find appropriate replace/replacement statuses", currentCredit.Id, nextCredit.Id));
            if (!nextCredit.IsCompletedCredit)
            {
                currentCredit.ReplacedStatus = ReplacedStatus.ReplaceInProgress;
                currentCredit.ReplacementStatus = ReplacementStatus.NotReplacement;
                //if completed course is being replaced by inprogress course then adjust the credits of completed course to 0
                //since we don't want to count possible replace in progress completed course in total completed credits or in cumulative GPA
                if (currentCredit.IsCompletedCredit)
                {
                    currentCredit.AdjustedCredit = 0M;
                    if (excludeCompletedReplaceInProgressCreditsFromGPA == true)
                    {
                        currentCredit.AdjustedGpaCredit = 0m;
                        currentCredit.AdjustedGradePoints = 0m;
                    }
                }
                nextCredit.ReplacementStatus = ReplacementStatus.PossibleReplacement;
                nextCredit.ReplacedStatus = ReplacedStatus.NotReplaced;
                creditCompared = nextCredit;
            }

            // If one was complete and the other was in progress, then the in-progress course is the possible replacement.
            //if completed course is being replaced by inprogress course then adjust the credits of completed course to 0
            //since we don't want to count possible replace in progress completed course in total completed credits or in cumulative GPA
            else if (nextCredit.IsCompletedCredit)
            {
                currentCredit.ReplacedStatus = ReplacedStatus.NotReplaced;
                currentCredit.ReplacementStatus = ReplacementStatus.PossibleReplacement;
                nextCredit.ReplacedStatus = ReplacedStatus.ReplaceInProgress;
                nextCredit.ReplacementStatus = ReplacementStatus.NotReplacement;
                nextCredit.AdjustedCredit = 0M;
                if (excludeCompletedReplaceInProgressCreditsFromGPA == true)
                {
                    nextCredit.AdjustedGpaCredit = 0m;
                    nextCredit.AdjustedGradePoints = 0m;
                }
                creditCompared = currentCredit;
            }
            logger.Info(string.Format("Replaced Status for current credit {0} is {1} ", currentCredit.Id, currentCredit.ReplacedStatus));
            logger.Info(string.Format("Replacement Status for current credit {0} is {1} ", currentCredit.Id, currentCredit.ReplacementStatus));
            logger.Info(string.Format("Replaced Status for next credit {0} is {1} ", nextCredit.Id, nextCredit.ReplacedStatus));
            logger.Info(string.Format("Replacement Status for next credit {0} is {1} ", nextCredit.Id, nextCredit.ReplacementStatus));
            //find the next credit in list
            try
            {
                int index = creditsToProcess.FindLastIndex(a => a.Equals(nextCredit)) + 1;
                creditToCompareWith = creditsToProcess.ElementAt(index);
            }
            catch (ArgumentOutOfRangeException)
            {
                creditToCompareWith = null;
            }
            catch (Exception)
            {
                creditToCompareWith = null;
                throw;
            }

            updateReplaceStatus(creditCompared, creditToCompareWith, creditsToProcess, excludeCompletedReplaceInProgressCreditsFromGPA);
        }

        /// <summary>
        /// This will filter academic credits based upon transcript grouping settings on TRGR- like 
        /// course levels, academic levels, departments, schools, division, additional criteria.
        /// </summary>
        /// <param name="credits">Academic Credits</param>
        /// <param name="program">Program</param>
        /// <returns>List of filtered credits</returns>
        private async Task<IEnumerable<AcademicCredit>> FilterCreditsOnTranscriptGroupingAsync(IEnumerable<AcademicCredit> credits, Program program)
        {
            if (credits == null)
            {
                throw new ArgumentNullException("credits", "credits cannot be null for transcript grouping");
            }
            if (program == null)
            {
                throw new ArgumentNullException("program", "program cannot be null for transcript grouping");
            }
            IEnumerable<AcademicCredit> filteredCredits = new List<AcademicCredit>();
            // Apply the program's credit filter ("Transcript Grouping")
            filteredCredits = credits.Where(cr => program.CreditFilter.Passes(cr));
            //these filtered credits are again passed for evaluating additional criteria
            //Addition creteria is not processed by CreditFilter.Passes method because is is defined as uni query syntax in TRGR and hence
            //need DatRaeader execution calls.
            if (!string.IsNullOrEmpty(program.CreditFilter.AdditionalSelectCriteria))
            {
                filteredCredits = await _academicCreditRepository.FilterAcademicCreditsAsync(filteredCredits, program.CreditFilter.AdditionalSelectCriteria);
            }
            return filteredCredits;
        }
        /// <summary>
        /// This will filter planned courses based upon transcript grouping settings on TRGR- like 
        /// course levels, academic levels, departments, schools, division.
        /// Planned courses are not filtered on additional criteria.
        /// </summary>
        /// <param name="plannedCourses">Planned Courses</param>
        /// <param name="program">Program </param>
        /// <returns>Collection of filtered planned courses</returns>
        private async Task<IEnumerable<PlannedCredit>> FilterPlannedCoursesOnTranscriptGroupingAsync(IEnumerable<PlannedCredit> plannedCourses, Program program)
        {
            if (plannedCourses == null)
            {
                throw new ArgumentNullException("plannedCourses", "plannedCourses cannot be null for transcript grouping");
            }
            if (program == null)
            {
                throw new ArgumentNullException("program", "program cannot be null for transcript grouping");
            }
            IEnumerable<PlannedCredit> filteredPlannedCourses = new List<PlannedCredit>();
            // Apply the parts of the CreditFilter that can apply to a course
            filteredPlannedCourses = plannedCourses.Where(upc => program.CreditFilter.Passes(upc.Course));
            IDictionary<Course, Tuple<IEnumerable<Department>, IEnumerable<string>>> deptLookup = await CreateTranscriptGroupingLookupsAsync(filteredPlannedCourses);
            //validate creditFilter for divisions, schools by passing departments with respect to that course
            filteredPlannedCourses = filteredPlannedCourses.Where(upc => program.CreditFilter.Passes(deptLookup[upc.Course])).ToList();
            return filteredPlannedCourses;
        }
    }
}
