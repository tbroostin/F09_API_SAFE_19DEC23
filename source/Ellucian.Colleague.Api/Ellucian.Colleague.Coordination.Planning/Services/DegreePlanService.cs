// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Planning.Reports;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Planning.Repositories;
using Ellucian.Colleague.Domain.Planning.Services;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Planning;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using Microsoft.Reporting.WebForms;
using slf4net;
using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Ellucian.Colleague.Coordination.Planning.Services
{
    [RegisterType]
    public class DegreePlanService : StudentCoordinationService, IDegreePlanService
    {
        private readonly IDegreePlanRepository _degreePlanRepository;
        private readonly IStudentDegreePlanRepository _studentDegreePlanRepository;
        private readonly ITermRepository _termRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IStudentProgramRepository _studentProgramRepository;
        private readonly IPlanningStudentRepository _planningStudentRepository;
        private readonly IProgramRepository _programRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IAcademicCreditRepository _academicCreditRepository;
        private readonly IRequirementRepository _requirementRepository;
        private readonly IRuleRepository _ruleRepository;
        private readonly IProgramRequirementsRepository _programRequirementsRepository;
        private readonly ISampleDegreePlanRepository _curriculumTrackRepository;
        private readonly IPlanningConfigurationRepository _planningConfigurationRepository;
        private readonly ICatalogRepository _catalogRepository;
        private readonly IDegreePlanArchiveRepository _degreePlanArchiveRepository;
        private readonly IAdvisorRepository _advisorRepository;
        private readonly IGradeRepository _gradeRepository;
        private readonly IAcademicHistoryService _academicHistoryService;
        private readonly IStudentDegreePlanService _studentDegreePlanService;
        private readonly IConfigurationRepository _configurationRepository;

        public DegreePlanService(IAdapterRegistry adapterRegistry, IDegreePlanRepository degreePlanRepository, ITermRepository termRepository, IStudentRepository studentRepository, IPlanningStudentRepository planningStudentRepository,
            IStudentProgramRepository studentProgramRepository, ICourseRepository courseRepository, ISectionRepository sectionRepository, IProgramRepository programRepository,
            IAcademicCreditRepository academicCreditRepository, IRequirementRepository requirementRepository, IRuleRepository ruleRepository, IProgramRequirementsRepository programRequirementsRepository,
            ISampleDegreePlanRepository curriculumTrackRepository, IPlanningConfigurationRepository planningConfigurationRepository, ICatalogRepository catalogRepository,
            IDegreePlanArchiveRepository degreePlanArchiveRepository, IAdvisorRepository advisorRepository, IGradeRepository gradeRepository, IAcademicHistoryService academicHistoryService,
            IStudentDegreePlanRepository studentDegreePlanRepository, IStudentDegreePlanService studentDegreePlanService,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger,IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _degreePlanRepository = degreePlanRepository;
            _studentDegreePlanRepository = studentDegreePlanRepository;
            _termRepository = termRepository;
            _studentRepository = studentRepository;
            _planningStudentRepository = planningStudentRepository;
            _studentProgramRepository = studentProgramRepository;
            _courseRepository = courseRepository;
            _sectionRepository = sectionRepository;
            _programRepository = programRepository;
            _academicCreditRepository = academicCreditRepository;
            _ruleRepository = ruleRepository;
            _requirementRepository = requirementRepository;
            _programRequirementsRepository = programRequirementsRepository;
            _curriculumTrackRepository = curriculumTrackRepository;
            _planningConfigurationRepository = planningConfigurationRepository;
            _catalogRepository = catalogRepository;
            _degreePlanArchiveRepository = degreePlanArchiveRepository;
            _advisorRepository = advisorRepository;
            _gradeRepository = gradeRepository;
            _academicHistoryService = academicHistoryService;
            _studentDegreePlanService = studentDegreePlanService;
        }

        //TODO
        /// <summary>
        /// Returns "true" if a sample degree plan for the given program/catalog
        /// Returns "false" if only the default sample degree plan is available
        /// Throws an error if neither is found.
        /// </summary>
        /// <param name="programCode"></param>
        /// <param name="catalog"></param>
        /// <returns></returns>
        public async Task<bool> CheckForSampleAsync(string programCode, string catalog)
        {
            bool sampleFound = false;
            if (!string.IsNullOrEmpty(programCode) && !string.IsNullOrEmpty(catalog))
            {
                sampleFound = ((await GetProgramSampleAsync(programCode, catalog)) != null);
            }
            if (!sampleFound)
            {
                if (await GetDefaultSampleAsync() == null)
                {
                    throw new InvalidOperationException("There is no sample degree plan available");
                }
            }
            return sampleFound;
        }

        //TODO
        /// <summary>
        /// Returns a sample degree plan if it exists for the specified program code and catalog.
        /// </summary>
        /// <param name="programCode"></param>
        /// <param name="catalog"></param>
        /// <returns></returns>
        private async Task<Domain.Planning.Entities.SampleDegreePlan> GetProgramSampleAsync(string programCode, string catalog)
        {
            Domain.Planning.Entities.SampleDegreePlan sampleDegreePlan = null;

            string pgmCurrTrackCode = null;
            try
            {
                pgmCurrTrackCode =( await _programRequirementsRepository.GetAsync(programCode, catalog)).CurriculumTrackCode;
                sampleDegreePlan = await _curriculumTrackRepository.GetAsync(pgmCurrTrackCode);
            }
            catch { }

            return sampleDegreePlan;
        }

        //TODO
        /// <summary>
        /// Returns the default sample degree plan
        /// </summary>
        /// <returns></returns>
        private async Task<Domain.Planning.Entities.SampleDegreePlan> GetDefaultSampleAsync()
        {

            Domain.Planning.Entities.SampleDegreePlan sampleDegreePlan = null;

            string currTrackCode = null;
            try
            {
                currTrackCode = (await _planningConfigurationRepository.GetPlanningConfigurationAsync()).DefaultCurriculumTrack; 
                sampleDegreePlan = await _curriculumTrackRepository.GetAsync(currTrackCode);
            }
            catch
            { }

            return sampleDegreePlan;
        }

        //TODO
        /// <summary>
        /// Provides a temporary version of the student's plan, overlaid with the sample plan from the program provided.
        /// The catalog year used to obtain the sample plan is determined from a parameter in Colleague.
        /// The changes to the plan are not saved.
        /// </summary>
        /// <param name="degreePlanId">Id of the degree plan</param>
        /// <param name="programCode">Program code from which the sample plan should be obtained</param>
        /// <returns>The updated Degree Plan DTO</returns>
        [Obsolete("Obsolete as of API 1.5. Use PreviewSampleDegreePlan3")]
        public async Task<Dtos.Planning.DegreePlanPreview2> PreviewSampleDegreePlan2Async(int degreePlanId, string programCode, string firstTermCode)
        {
            if (string.IsNullOrEmpty(programCode))
            {
                throw new ArgumentNullException("programCode", "Program code must be provided.");
            }
            if (degreePlanId <= 0)
            {
                throw new ArgumentNullException("degreePlan", "Degree plan is required.");
            }
            Domain.Student.Entities.DegreePlans.DegreePlan degreePlan = null;
            try
            {
                degreePlan = await _studentDegreePlanRepository.GetAsync(degreePlanId);
                if (degreePlan == null)
                {
                    throw new KeyNotFoundException("Degree plan id " + degreePlanId + " not found.");
                }
            }
            catch
            {
                throw new KeyNotFoundException("Degree plan id " + degreePlanId + " not found.");
            }

            // Prevent action without proper permissions
            if (!UserIsSelf(degreePlan.PersonId))
            {
                // Make sure user has permissions to view this degree plan. 
                // If not, an PermissionsException will be thrown.
                await CheckViewPlanPermissionsAsync(degreePlan.PersonId); //TODO
            }

            // Get academic credits for the student
            // Get student
            Domain.Student.Entities.Student student = await _studentRepository.GetAsync(degreePlan.PersonId);

            var studentAcademicCredits =  await _academicCreditRepository.GetAsync(student.AcademicCreditIds);
            IEnumerable<Domain.Student.Entities.Term> planningTerms = (await _termRepository.GetAsync()).Where(t => t.ForPlanning == true);
            try
            {
                // Get the sample degree plan to use for this student and program. Student is used to figure out the catalog year.
                Domain.Planning.Entities.SampleDegreePlan sampleDegreePlan = await GetSampleDegreePlanAsync(degreePlan.PersonId, programCode);
                // Build the the degree plan for the student with the sample degree plan applied.
                Domain.Planning.Entities.DegreePlanPreview degreePlanPreview = new Domain.Planning.Entities.DegreePlanPreview(degreePlan, sampleDegreePlan, studentAcademicCredits, planningTerms, firstTermCode, CurrentUser.PersonId);
                // Convert the fully merged degree plan to a degree plan DTO
                var degreePlanPreviewDtoAdapter = _adapterRegistry.GetAdapter<Domain.Planning.Entities.DegreePlanPreview, Dtos.Planning.DegreePlanPreview2>();
                var degreePlanPreviewDto = degreePlanPreviewDtoAdapter.MapToType(degreePlanPreview);
                return degreePlanPreviewDto;
            }
            catch (Exception ex)
            {
                // If no sample plan can be found, or if it couldn't overlay this plan on the student's plan for any reason then...
                throw new Exception(ex.Message);
            }
        }

        //TODO
        /// <summary>
        /// Provides a temporary version of the student's plan, overlaid with the sample plan from the program provided.
        /// The catalog year used to obtain the sample plan is determined from a parameter in Colleague.
        /// The changes to the plan are not saved.
        /// </summary>
        /// <param name="degreePlanId">Id of the degree plan</param>
        /// <param name="programCode">Program code from which the sample plan should be obtained</param>
        /// <returns>The updated Degree Plan DTO</returns>
        [Obsolete("Obsolete as of API 1.6. Use PreviewSampleDegreePlan4")]
        public async Task<Ellucian.Colleague.Dtos.Planning.DegreePlanPreview3> PreviewSampleDegreePlan3Async(int degreePlanId, string programCode, string firstTermCode)
        {
            if (string.IsNullOrEmpty(programCode))
            {
                throw new ArgumentNullException("programCode", "Program code must be provided.");
            }
            if (degreePlanId <= 0)
            {
                throw new ArgumentNullException("degreePlan", "Degree plan is required.");
            }
            Domain.Student.Entities.DegreePlans.DegreePlan degreePlan = null;
            try
            {
                degreePlan = await _studentDegreePlanRepository.GetAsync(degreePlanId);
                if (degreePlan == null)
                {
                    throw new KeyNotFoundException("Degree plan id " + degreePlanId + " not found.");
                }
            }
            catch
            {
                throw new KeyNotFoundException("Degree plan id " + degreePlanId + " not found.");
            }

            // Make sure user has permissions to view this degree plan. 
            // If not, an PermissionsException will be thrown.
            await CheckViewPlanPermissionsAsync(degreePlan.PersonId);

            // Get the student's academic credits
            var studentAcademicCredits = new List<AcademicCredit>();
            var creditsByStudentDict =  await _academicCreditRepository.GetAcademicCreditByStudentIdsAsync(new List<string>() { degreePlan.PersonId });
            if (creditsByStudentDict.ContainsKey(degreePlan.PersonId))
            {
                studentAcademicCredits = creditsByStudentDict[degreePlan.PersonId];
            }

            IEnumerable<Domain.Student.Entities.Term> planningTerms = (await _termRepository.GetAsync()).Where(t => t.ForPlanning == true); //TODO
            try
            {
                // Get the sample degree plan to use for this student and program. Student is used to figure out the catalog year.
                Domain.Planning.Entities.SampleDegreePlan sampleDegreePlan = await GetSampleDegreePlanAsync(degreePlan.PersonId, programCode);
                // Build the the degree plan for the student with the sample degree plan applied.
                Domain.Planning.Entities.DegreePlanPreview degreePlanPreview = new Domain.Planning.Entities.DegreePlanPreview(degreePlan, sampleDegreePlan, studentAcademicCredits, planningTerms, firstTermCode, CurrentUser.PersonId);
                // Convert the fully merged degree plan to a degree plan DTO
                var degreePlanPreviewDtoAdapter = _adapterRegistry.GetAdapter<Domain.Planning.Entities.DegreePlanPreview, Dtos.Planning.DegreePlanPreview3>();
                var degreePlanPreviewDto = degreePlanPreviewDtoAdapter.MapToType(degreePlanPreview);
                return degreePlanPreviewDto;
            }
            catch (Exception ex)
            {
                // If no sample plan can be found, or if it couldn't overlay this plan on the student's plan for any reason then...
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Provides a temporary version of the student's plan, overlaid with the sample plan from the program provided.
        /// The catalog year used to obtain the sample plan is determined from a parameter in Colleague.
        /// The changes to the plan are not saved.
        /// </summary>
        /// <param name="degreePlanId">Id of the degree plan</param>
        /// <param name="programCode">Program code from which the sample plan should be obtained</param>
        /// <returns>The updated Degree Plan DTO</returns>
        [Obsolete("Obsolete. Use PreviewSampleDegreePlan6")]
        public async Task<Dtos.Planning.DegreePlanPreview4> PreviewSampleDegreePlan4Async(int degreePlanId, string programCode, string firstTermCode)
        {
            if (string.IsNullOrEmpty(programCode))
            {
                throw new ArgumentNullException("programCode", "Program code must be provided.");
            }
            if (degreePlanId <= 0)
            {
                throw new ArgumentNullException("degreePlan", "Degree plan is required.");
            }
            Domain.Student.Entities.DegreePlans.DegreePlan degreePlan = null;
            try
            {
                degreePlan = await _studentDegreePlanRepository.GetAsync(degreePlanId);
                if (degreePlan == null)
                {
                    throw new KeyNotFoundException("Degree plan id " + degreePlanId + " not found.");
                }
            }
            catch
            {
                throw new KeyNotFoundException("Degree plan id " + degreePlanId + " not found.");
            }

            // Make sure user has permissions to view this degree plan. 
            // If not, an PermissionsException will be thrown.
            await CheckViewPlanPermissionsAsync(degreePlan.PersonId);

            // Get the student's academic credits
            var studentAcademicCredits = new List<AcademicCredit>();
            var creditsByStudentDict = await _academicCreditRepository.GetAcademicCreditByStudentIdsAsync(new List<string>() { degreePlan.PersonId });
            if (creditsByStudentDict.ContainsKey(degreePlan.PersonId))
            {
                studentAcademicCredits = creditsByStudentDict[degreePlan.PersonId];
            }

            IEnumerable<Domain.Student.Entities.Term> planningTerms = (await _termRepository.GetAsync()).Where(t => t.ForPlanning == true);
            try
            {
                // Get the sample degree plan to use for this student and program. Student is used to figure out the catalog year.
                Domain.Planning.Entities.SampleDegreePlan sampleDegreePlan = await GetSampleDegreePlanAsync(degreePlan.PersonId, programCode);
                // Build the the degree plan for the student with the sample degree plan applied.
                Domain.Planning.Entities.DegreePlanPreview degreePlanPreview = new Domain.Planning.Entities.DegreePlanPreview(degreePlan, sampleDegreePlan, studentAcademicCredits, planningTerms, firstTermCode, CurrentUser.PersonId);
                // Convert the fully merged degree plan to a degree plan DTO
                var degreePlanPreviewDtoAdapter = _adapterRegistry.GetAdapter<Domain.Planning.Entities.DegreePlanPreview, Dtos.Planning.DegreePlanPreview4>();
                var degreePlanPreviewDto = degreePlanPreviewDtoAdapter.MapToType(degreePlanPreview);

                // Take Student and Academic credits we already have and build the academic history object and add to each DegreePlan3 item
                var academicHistoryDto =await _academicHistoryService.ConvertAcademicCreditsToAcademicHistoryDtoAsync(degreePlan.PersonId, studentAcademicCredits); //TODO
                degreePlanPreviewDto.AcademicHistory = academicHistoryDto;

                return degreePlanPreviewDto;
            }
            catch (Exception ex)
            {
                // If no sample plan can be found, or if it couldn't overlay this plan on the student's plan for any reason then...
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Provides a temporary version of the student's plan, overlaid with the sample plan from the program provided.
        /// The catalog year used to obtain the sample plan is determined from a parameter in Colleague.
        /// The changes to the plan are not saved.
        /// </summary>
        /// <param name="degreePlanId">Id of the degree plan</param>
        /// <param name="programCode">Program code from which the sample plan should be obtained</param>
        /// <returns>The updated Degree Plan DTO</returns>
        [Obsolete("Obsolete as of API 1.18. Use PreviewSampleDegreePlan6")]
        public async Task<Ellucian.Colleague.Dtos.Planning.DegreePlanPreview5> PreviewSampleDegreePlan5Async(int degreePlanId, string programCode, string firstTermCode)
        {
            if (string.IsNullOrEmpty(programCode))
            {
                throw new ArgumentNullException("programCode", "Program code must be provided.");
            }
            if (degreePlanId <= 0)
            {
                throw new ArgumentNullException("degreePlan", "Degree plan is required.");
            }
            Domain.Student.Entities.DegreePlans.DegreePlan degreePlan = null;
            try
            {
                degreePlan = await _studentDegreePlanRepository.GetAsync(degreePlanId);
                if (degreePlan == null)
                {
                    throw new KeyNotFoundException("Degree plan id " + degreePlanId + " not found.");
                }
            }
            catch
            {
                throw new KeyNotFoundException("Degree plan id " + degreePlanId + " not found.");
            }

            // Make sure user has permissions to view this degree plan. 
            // If not, an PermissionsException will be thrown.
            await CheckViewPlanPermissionsAsync(degreePlan.PersonId);

            // Get the student's academic credits
            var studentAcademicCredits = new List<AcademicCredit>();
            var creditsByStudentDict = await _academicCreditRepository.GetAcademicCreditByStudentIdsAsync(new List<string>() { degreePlan.PersonId });
            if (creditsByStudentDict.ContainsKey(degreePlan.PersonId))
            {
                studentAcademicCredits = creditsByStudentDict[degreePlan.PersonId];
            }

            IEnumerable<Domain.Student.Entities.Term> planningTerms = (await _termRepository.GetAsync()).Where(t => t.ForPlanning == true);
            try
            {
                // Get the sample degree plan to use for this student and program. Student is used to figure out the catalog year.
                Domain.Planning.Entities.SampleDegreePlan sampleDegreePlan = await GetSampleDegreePlanAsync(degreePlan.PersonId, programCode);
                // Build the the degree plan for the student with the sample degree plan applied.
                Domain.Planning.Entities.DegreePlanPreview degreePlanPreview = new Domain.Planning.Entities.DegreePlanPreview(degreePlan, sampleDegreePlan, studentAcademicCredits, planningTerms, firstTermCode, CurrentUser.PersonId);
                // Convert the fully merged degree plan to a degree plan DTO
                var degreePlanPreviewDtoAdapter = _adapterRegistry.GetAdapter<Domain.Planning.Entities.DegreePlanPreview, Dtos.Planning.DegreePlanPreview5>();
                var degreePlanPreviewDto = degreePlanPreviewDtoAdapter.MapToType(degreePlanPreview);

                // Take Student and Academic credits we already have and build the academic history object and add to each DegreePlan4 item
                var academicHistoryDto =await _academicHistoryService.ConvertAcademicCreditsToAcademicHistoryDto2Async(degreePlan.PersonId, studentAcademicCredits); 
                degreePlanPreviewDto.AcademicHistory = academicHistoryDto;

                return degreePlanPreviewDto;
            }
            catch (Exception ex)
            {
                // If no sample plan can be found, or if it couldn't overlay this plan on the student's plan for any reason then...
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// CURRENT VERSION: Provides a temporary version of the student's plan, overlaid with the sample plan from the program provided.
        /// The catalog year used to obtain the sample plan is determined from a parameter in Colleague.
        /// The changes to the plan are not saved.
        /// </summary>
        /// <param name="degreePlanId">Id of the degree plan</param>
        /// <param name="programCode">Program code from which the sample plan should be obtained</param>
        /// <returns>The updated Degree Plan DTO</returns>
        public async Task<Dtos.Planning.DegreePlanPreview6> PreviewSampleDegreePlan6Async(int degreePlanId, string programCode, string firstTermCode)
        {
            if (string.IsNullOrEmpty(programCode))
            {
                throw new ArgumentNullException("programCode", "Program code must be provided.");
            }
            if (degreePlanId <= 0)
            {
                throw new ArgumentNullException("degreePlan", "Degree plan is required.");
            }
            Domain.Student.Entities.DegreePlans.DegreePlan degreePlan = null;
            try
            {
                degreePlan = await _studentDegreePlanRepository.GetAsync(degreePlanId);
                if (degreePlan == null)
                {
                    throw new KeyNotFoundException("Degree plan id " + degreePlanId + " not found.");
                }
            }
            catch
            {
                throw new KeyNotFoundException("Degree plan id " + degreePlanId + " not found.");
            }

            // Make sure user has permissions to view this degree plan. 
            // If not, an PermissionsException will be thrown.
            await CheckViewPlanPermissionsAsync(degreePlan.PersonId);

            // Get the student's academic credits
            var studentAcademicCredits = new List<AcademicCredit>();
            var creditsByStudentDict = await _academicCreditRepository.GetAcademicCreditByStudentIdsAsync(new List<string>() { degreePlan.PersonId });
            if (creditsByStudentDict.ContainsKey(degreePlan.PersonId))
            {
                studentAcademicCredits = creditsByStudentDict[degreePlan.PersonId];
            }

            IEnumerable<Domain.Student.Entities.Term> planningTerms = (await _termRepository.GetAsync()).Where(t => t.ForPlanning == true);
            try
            {
                // Get the sample degree plan to use for this student and program. Student is used to figure out the catalog year.
                Domain.Planning.Entities.SampleDegreePlan sampleDegreePlan = await GetSampleDegreePlanAsync(degreePlan.PersonId, programCode);
                // Build the the degree plan for the student with the sample degree plan applied.
                Domain.Planning.Entities.DegreePlanPreview degreePlanPreview = new Domain.Planning.Entities.DegreePlanPreview(degreePlan, sampleDegreePlan, studentAcademicCredits, planningTerms, firstTermCode, CurrentUser.PersonId);
                // Convert the fully merged degree plan to a degree plan DTO
                var degreePlanPreviewDtoAdapter = _adapterRegistry.GetAdapter<Domain.Planning.Entities.DegreePlanPreview, Dtos.Planning.DegreePlanPreview6>();
                var degreePlanPreviewDto = degreePlanPreviewDtoAdapter.MapToType(degreePlanPreview);

                // Take Student and Academic credits we already have and build the academic history object and add to each DegreePlan4 item
                var academicHistoryDto = await _academicHistoryService.ConvertAcademicCreditsToAcademicHistoryDto4Async(degreePlan.PersonId, studentAcademicCredits);
                degreePlanPreviewDto.AcademicHistory = academicHistoryDto;

                return degreePlanPreviewDto;
            }
            catch (Exception ex)
            {
                // If no sample plan can be found, or if it couldn't overlay this plan on the student's plan for any reason then...
                throw new Exception(ex.Message);
            }
        }

        //TODO: needs converting to async
        /// <summary>
        /// Given a student and a program, determine the best sample plan to use
        /// </summary>
        /// <param name="studentId">student Id of the person requesting the sample plan - needed to determine catalog year</param>
        /// <param name="programCode">program from which to pull the sample degree plan</param>
        /// <returns></returns>
        private async Task<Domain.Planning.Entities.SampleDegreePlan> GetSampleDegreePlanAsync(string studentId, string programCode)
        {
            // GET SAMPLE DEGREE PLAN
            if (string.IsNullOrEmpty(programCode))
            {
                throw new ArgumentNullException("programCode", "Program code must be provided.");
            }
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student Id is required to get a sample degree plan.");
            }
            // Determine the catalog year to use to get the sample degree plan.
            string catalogCode = null;

            // See if the sample plans's program is one of the student's programs - if so use catalog code from that.
            Domain.Student.Entities.StudentProgram studentProgram =await  _studentProgramRepository.GetAsync(studentId, programCode); //TODO
            if (studentProgram == null)
            {
                // If student is not currently enrolled in the sample plan's program, find the best-match catalog for the student
                var studentPrograms = await _studentProgramRepository.GetAsync(studentId); //TODO
                if (studentPrograms == null || studentPrograms.Count() == 0)
                {
                    throw new KeyNotFoundException("StudentPrograms");
                }
                var planProgram = await _programRepository.GetAsync(programCode);
                // Determine the program catalog year to use.
                Domain.Planning.Entities.PlanningConfiguration planningConfiguration = await _planningConfigurationRepository.GetPlanningConfigurationAsync();
                ICollection<Catalog> catalogs =await _catalogRepository.GetAsync();
                catalogCode = ProgramCatalogService.DeriveDefaultCatalog(planProgram, studentPrograms, catalogs, planningConfiguration.DefaultCatalogPolicy);
            }
            else
            {
                catalogCode = studentProgram.CatalogCode;
            }
            //var catalog = "2012";
            var sampleDegreePlan = await GetProgramSampleAsync(programCode, catalogCode);
            if (sampleDegreePlan == null)
            {
                sampleDegreePlan = await GetDefaultSampleAsync();//TODO
            }
            if (sampleDegreePlan == null)
            {
                throw new ArgumentOutOfRangeException("There is no sample degree plan available");
            }
            return sampleDegreePlan;
        }

        //TODO
        /// <summary>
        /// Obsolete routine that is just temporarily needed for the now obsolete ApplySampleDegreePlan method.
        /// </summary>
        /// <param name="student"></param>
        [Obsolete("Deprecated on version 1.2 of the Api. Use CheckUpdatePermissions going forward.")]
        protected async Task CheckApplyPlanPermissionsAsync(Ellucian.Colleague.Domain.Student.Entities.Student student)
        {
            // Access is Ok if the current user is this student
            if (UserIsSelf(student.Id)) { return; }

            bool userIsAssignedAdvisor =await UserIsAssignedAdvisorAsync(student.Id, student.ConvertToStudentAccess()); //TODO
            // Get user permissions
            IEnumerable<string> userPermissions = await GetUserPermissionCodesAsync();

            // Access is Ok if this is an advisor with all access, update access or review access to any student.
            // Access is also OK if this is an advisor with all access, update access or review access to their assigned advisees and this is an assigned advisee.
            if (userPermissions.Contains(PlanningPermissionCodes.AllAccessAnyAdvisee) || userPermissions.Contains(PlanningPermissionCodes.UpdateAnyAdvisee) || (userPermissions.Contains(PlanningPermissionCodes.AllAccessAssignedAdvisees) && userIsAssignedAdvisor) || (userPermissions.Contains(PlanningPermissionCodes.UpdateAssignedAdvisees) && userIsAssignedAdvisor))
            {
                return;
            }

            // User does not have permissions and error needs to be thrown and logged
            logger.Error("User " + CurrentUser.PersonId + " does not have permissions to update this degree plan");
            throw new PermissionsException();
        }

        /// <summary>
        /// Update the student's plan with a sample degree plan, using the sample degree plan for the supplied program.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <param name="programCode">Program code from where the sample degree plan should be derived</param>
        /// <returns>An Updated Degree Plan DTO.</returns>
        [Obsolete("Deprecated on version 1.2 of the Api. Use PreviewSampleDegreePlan along with the UpdateDegreePlan methods going forward.")]
        public async Task<Dtos.Student.DegreePlans.DegreePlan> ApplySampleDegreePlanAsync(string studentId, string programCode)
        {
            // Get student from repository
            Domain.Student.Entities.Student student = await _studentRepository.GetAsync(studentId);
            // Prevent action without proper permissions
            // Make sure user has permissions to update this degree plan. 
            // If not, an PermissionsException will be thrown.

            // Intentionally using deprecated await CheckApplyPlanPermissionsAsync for this deprecated method.  We know this isn't a review type of change
            // so there is no reason to go through all the logic in CheckUpdatePermissions.             
            await CheckApplyPlanPermissionsAsync(student); //TODO

            // GET OR CREATE DEGREE PLAN

            // Get the degree plan for this student
            Domain.Student.Entities.DegreePlans.DegreePlan degreePlanToUpdate = null;
            try
            {
                degreePlanToUpdate = (await _studentDegreePlanRepository.GetAsync(new List<string>() { studentId })).FirstOrDefault();
                if (degreePlanToUpdate == null)
                {
                    throw new ArgumentException();
                }
            }
            catch // If there is an error thrown and/or degree plan returned is null, create a new one
            {
                try
                {
                    // No degree plan found for this student, call the service method to create one and write it to the repository
                    // Convert the returned, new degree plan back to the domain entity from dto
                    var degreePlan = Task.Run(async () =>
                    {
                        return await _studentDegreePlanService.CreateDegreePlanAsync(studentId);
                    }).GetAwaiter().GetResult();
                    var degreePlanEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.DegreePlans.DegreePlan, Domain.Student.Entities.DegreePlans.DegreePlan>();
                    degreePlanToUpdate = degreePlanEntityAdapter.MapToType(degreePlan);
                }
                catch
                {
                    throw new ArgumentOutOfRangeException("Cannot find or create a degree plan");
                }
            }
            // Get the sample degree plan to use for this student and program.
            Ellucian.Colleague.Domain.Planning.Entities.SampleDegreePlan sampleDegreePlan = await GetSampleDegreePlanAsync(studentId, programCode);

            // Build the the degree plan for the student with the sample degree plan applied.
            // In this version, the academic credits are immaterial, all courses on the sample plan should be loaded.
            IEnumerable<Domain.Student.Entities.AcademicCredit> studentAcademicCredits = new List<Domain.Student.Entities.AcademicCredit>();
            IEnumerable<Domain.Student.Entities.Term> planningTerms = (await _termRepository.GetAsync()).Where(t => t.ForPlanning == true); //TODO
            Domain.Planning.Entities.DegreePlanPreview degreePlanPreview = new Domain.Planning.Entities.DegreePlanPreview(degreePlanToUpdate, sampleDegreePlan, studentAcademicCredits, planningTerms, string.Empty, CurrentUser.PersonId);
            var degreePlanPreviewDtoAdapter = _adapterRegistry.GetAdapter<Domain.Planning.Entities.DegreePlanPreview, Dtos.Planning.DegreePlanPreview>();
            var degreePlanPreviewDto = degreePlanPreviewDtoAdapter.MapToType(degreePlanPreview);
            // Update the degree plan via the local service routine
            var updatedDegreePlanDto = await _studentDegreePlanService.UpdateDegreePlanAsync(degreePlanPreviewDto.MergedDegreePlan);
            return updatedDegreePlanDto;
        }

        //TODO
        /// <summary>
        /// Provides a temporary version of the student's plan, overlaid with the sample plan from the program provided.
        /// The catalog year used to obtain the sample plan is determined from a parameter in Colleague.
        /// The changes to the plan are not saved.
        /// </summary>
        /// <param name="degreePlanId">Id of the degree plan</param>
        /// <param name="programCode">Program code from which the sample plan should be obtained</param>
        /// <returns>The updated Degree Plan DTO</returns>
        public async Task<Dtos.Planning.DegreePlanPreview> PreviewSampleDegreePlanAsync(int degreePlanId, string programCode)
        {
            if (string.IsNullOrEmpty(programCode))
            {
                throw new ArgumentNullException("programCode", "Program code must be provided.");
            }
            if (degreePlanId <= 0)
            {
                throw new ArgumentNullException("degreePlan", "Degree plan is required.");
            }
            Domain.Student.Entities.DegreePlans.DegreePlan degreePlan = null;
            try
            {
                degreePlan = await _studentDegreePlanRepository.GetAsync(degreePlanId);
                if (degreePlan == null)
                {
                    throw new KeyNotFoundException("Degree plan id " + degreePlanId + " not found.");
                }
            }
            catch
            {
                throw new KeyNotFoundException("Degree plan id " + degreePlanId + " not found.");
            }

            // Get student
            Domain.Student.Entities.Student student = await _studentRepository.GetAsync(degreePlan.PersonId);

            // Prevent action without proper permissions
            if (!UserIsSelf(degreePlan.PersonId))
            {
                // Make sure user has permissions to view this degree plan. 
                // If not, an PermissionsException will be thrown.
                await CheckViewPlanPermissionsAsync(degreePlan.PersonId, student);
            }

            // Get academic credits for the student
            var studentAcademicCredits = await _academicCreditRepository.GetAsync(student.AcademicCreditIds);
            IEnumerable<Domain.Student.Entities.Term> planningTerms = (await _termRepository.GetAsync()).Where(t => t.ForPlanning == true);
            try
            {
                // Get the sample degree plan to use for this student and program. Student is used to figure out the catalog year.
                Domain.Planning.Entities.SampleDegreePlan sampleDegreePlan = await GetSampleDegreePlanAsync(degreePlan.PersonId, programCode);
                // Build the the degree plan for the student with the sample degree plan applied.
                Domain.Planning.Entities.DegreePlanPreview degreePlanPreview = new Domain.Planning.Entities.DegreePlanPreview(degreePlan, sampleDegreePlan, studentAcademicCredits, planningTerms, string.Empty, CurrentUser.PersonId);
                // Convert the fully merged degree plan to a degree plan DTO
                var degreePlanPreviewDtoAdapter = _adapterRegistry.GetAdapter<Domain.Planning.Entities.DegreePlanPreview, Dtos.Planning.DegreePlanPreview>();
                var degreePlanPreviewDto = degreePlanPreviewDtoAdapter.MapToType(degreePlanPreview);
                return degreePlanPreviewDto;
            }
            catch (Exception ex)
            {
                // If no sample plan can be found, or if it couldn't overlay this plan on the student's plan for any reason then...
                throw new Exception(ex.Message);
            }
        }

        //TODO
        /// <summary>
        /// Given a degree plan DTO, create and persist a degree plan archive, then return the archive DTO
        /// </summary>
        /// <param name="degreePlan"></param>
        /// <returns></returns>
        [Obsolete("Obsoloete with version 1.5 of the API. Use ArchiveDegreePlan2.")]
        public async Task<Dtos.Planning.DegreePlanArchive> ArchiveDegreePlanAsync(Dtos.Student.DegreePlans.DegreePlan2 degreePlan)
        {
            // Get the right adapter for the type mapping and convert degree plan DTO to degree plan entity
            var degreePlanEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.DegreePlans.DegreePlan2, Domain.Student.Entities.DegreePlans.DegreePlan>();
            var degreePlanToArchive = degreePlanEntityAdapter.MapToType(degreePlan);

            // Get the student. Need academic credits to build the degree plan archive
            Domain.Student.Entities.Student student = await _studentRepository.GetAsync(degreePlan.PersonId);

            // Make sure the current user has permission to create the archive
            if (!UserIsSelf(degreePlan.PersonId))
            {
                // Get student from repository
                await _studentDegreePlanService.CheckUpdatePermissionsAsync(student, degreePlanToArchive); //TODO
            }
            // Archive the degree plan

            // First, get the student's active programs
            var studentPrograms = await _studentProgramRepository.GetAsync(degreePlan.PersonId);
            var courses = await _courseRepository.GetAsync();
            var registrationTerms = await _termRepository.GetRegistrationTermsAsync();
            // Get all sections on the degree plan - used to get section titles and numbers for the archive
            List<string> sectionsOnPlan = degreePlan.NonTermPlannedCourses.Where(c => !string.IsNullOrEmpty(c.SectionId)).Select(s => s.SectionId).ToList();
            foreach (var term in degreePlan.Terms)
            {
                var sectionsToAdd = term.PlannedCourses.Where(c => !string.IsNullOrEmpty(c.SectionId)).Select(p => p.SectionId).ToList();
                sectionsOnPlan.AddRange(sectionsToAdd);
            }
            var degreePlanSections =  await _sectionRepository.GetCachedSectionsAsync(sectionsOnPlan);
            var academicCredits = await _academicCreditRepository.GetAsync(student.AcademicCreditIds, true);
            var grades = await _gradeRepository.GetAsync();
            var newDegreePlanArchive = Domain.Planning.Entities.DegreePlanArchive.CreateDegreePlanArchive(degreePlanToArchive, CurrentUser.PersonId, studentPrograms, courses, degreePlanSections, academicCredits, grades);
            var degreePlanArchive = await _degreePlanArchiveRepository.AddAsync(newDegreePlanArchive);

            // Get the right adapter for the type mapping and convert the degree plan entity to degree plan DTO
            var degreePlanArchiveDtoAdapter = _adapterRegistry.GetAdapter<Domain.Planning.Entities.DegreePlanArchive, Dtos.Planning.DegreePlanArchive>();
            var degreePlanArchiveDto = degreePlanArchiveDtoAdapter.MapToType(degreePlanArchive);

            return degreePlanArchiveDto;
        }

        //TODO
        /// <summary>
        /// Given a degree plan DTO, create and persist a degree plan archive, then return the archive DTO
        /// </summary>
        /// <param name="degreePlan">The degree plan to be archived</param>
        /// <returns>The <see cref="DegreePlanArchive2"/>archive of the degree plan</see></returns>
        [Obsolete("Obsoloete with version 1.7 of the API. Use ArchiveDegreePlan3.")]
        public async Task<Dtos.Planning.DegreePlanArchive2> ArchiveDegreePlan2Async(Dtos.Student.DegreePlans.DegreePlan3 degreePlan)
        {
            // Get the right adapter for the type mapping and convert degree plan DTO to degree plan entity
            var degreePlanEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.DegreePlans.DegreePlan3, Domain.Student.Entities.DegreePlans.DegreePlan>();
            var degreePlanToArchive = degreePlanEntityAdapter.MapToType(degreePlan);

            // Get the student. Need academic credits to build the degree plan archive
            Domain.Student.Entities.PlanningStudent student = await _planningStudentRepository.GetAsync(degreePlan.PersonId); //TODO

            var storedDegreePlan = await _studentDegreePlanRepository.GetAsync(degreePlan.Id);
            degreePlanToArchive.UpdateMissingProtectionFlags(storedDegreePlan); //TODO?

            // Leave the door open for student to create an archive
            if (!UserIsSelf(degreePlan.PersonId))
            {
                // Make sure this user has permissions to create an archive
                await _studentDegreePlanService.CheckUpdatePermissions2Async(degreePlanToArchive, storedDegreePlan, student); //TODO
            }

            // Archive the degree plan

            // First, get the student's active programs
            var studentPrograms = await _studentProgramRepository.GetAsync(degreePlan.PersonId);
            var courses = await _courseRepository.GetAsync();
            var registrationTerms = await _termRepository.GetRegistrationTermsAsync();
            // Get all sections on the degree plan - used to get section titles and numbers for the archive
            List<string> sectionsOnPlan = degreePlan.NonTermPlannedCourses.Where(c => !string.IsNullOrEmpty(c.SectionId)).Select(s => s.SectionId).ToList();
            foreach (var term in degreePlan.Terms)
            {
                var sectionsToAdd = term.PlannedCourses.Where(c => !string.IsNullOrEmpty(c.SectionId)).Select(p => p.SectionId).ToList();
                sectionsOnPlan.AddRange(sectionsToAdd);
            }
            var degreePlanSections = await _sectionRepository.GetCachedSectionsAsync(sectionsOnPlan);
            var academicCredits = new List<AcademicCredit>();
            var academicCreditsDict = await _academicCreditRepository.GetAcademicCreditByStudentIdsAsync(new List<string> { student.Id }, true);
            if (academicCreditsDict.ContainsKey(student.Id))
            {
                academicCredits = academicCreditsDict[student.Id];
            }
            var grades = await _gradeRepository.GetAsync();
            var newDegreePlanArchive = Domain.Planning.Entities.DegreePlanArchive.CreateDegreePlanArchive(degreePlanToArchive, CurrentUser.PersonId, studentPrograms, courses, degreePlanSections, academicCredits, grades);
            var degreePlanArchive = await _degreePlanArchiveRepository.AddAsync(newDegreePlanArchive);

            // Get the right adapter for the type mapping and convert the degree plan entity to degree plan DTO
            var degreePlanArchiveDtoAdapter = _adapterRegistry.GetAdapter<Domain.Planning.Entities.DegreePlanArchive, Dtos.Planning.DegreePlanArchive2>();
            var degreePlanArchiveDto = degreePlanArchiveDtoAdapter.MapToType(degreePlanArchive);

            return degreePlanArchiveDto;
        }

        //TODO
        /// <summary>
        /// Given a degree plan DTO, create and persist a degree plan archive, then return the archive DTO
        /// </summary>
        /// <param name="degreePlan">The degree plan to be archived</param>
        /// <returns>The <see cref="DegreePlanArchive2"/>archive of the degree plan</see></returns>
        public async Task<Dtos.Planning.DegreePlanArchive2> ArchiveDegreePlan3Async(Dtos.Student.DegreePlans.DegreePlan4 degreePlan)
        {
            // Get the right adapter for the type mapping and convert degree plan DTO to degree plan entity
            var degreePlanEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.DegreePlans.DegreePlan4, Domain.Student.Entities.DegreePlans.DegreePlan>();
            var degreePlanToArchive = degreePlanEntityAdapter.MapToType(degreePlan);

            // Get the student. Need academic credits to build the degree plan archive
            Domain.Student.Entities.PlanningStudent student = await _planningStudentRepository.GetAsync(degreePlan.PersonId); //TODO

            var storedDegreePlan = await _studentDegreePlanRepository.GetAsync(degreePlan.Id);
            degreePlanToArchive.UpdateMissingProtectionFlags(storedDegreePlan); //TODO

            // Leave the door open for student to create an archive
            if (!UserIsSelf(degreePlan.PersonId))
            {
                // Make sure this user has permissions to create an archive
                await _studentDegreePlanService.CheckUpdatePermissions2Async(degreePlanToArchive, storedDegreePlan, student); //TODO
            }

            // Archive the degree plan

            // First, get the student's active programs
            var studentPrograms = await _studentProgramRepository.GetAsync(degreePlan.PersonId);
            var courses = await _courseRepository.GetAsync();
            var registrationTerms = await _termRepository.GetRegistrationTermsAsync();
            // Get all sections on the degree plan - used to get section titles and numbers for the archive
            List<string> sectionsOnPlan = degreePlan.NonTermPlannedCourses.Where(c => !string.IsNullOrEmpty(c.SectionId)).Select(s => s.SectionId).ToList();
            foreach (var term in degreePlan.Terms)
            {
                var sectionsToAdd = term.PlannedCourses.Where(c => !string.IsNullOrEmpty(c.SectionId)).Select(p => p.SectionId).ToList();
                sectionsOnPlan.AddRange(sectionsToAdd);
            }
            var degreePlanSections = await _sectionRepository.GetCachedSectionsAsync(sectionsOnPlan);
            var academicCreditsDict = await _academicCreditRepository.GetAcademicCreditByStudentIdsAsync(new List<string> { student.Id }, true);
            var studentAcademicCredits = new List<AcademicCredit>();
            if (academicCreditsDict.ContainsKey(degreePlan.PersonId))
            {
                studentAcademicCredits = academicCreditsDict[student.Id];
            }
            //var academicCredits = await _academicCreditRepository.GetAsync(student.AcademicCreditIds, true);
            var grades = await _gradeRepository.GetAsync();
            var newDegreePlanArchive = Domain.Planning.Entities.DegreePlanArchive.CreateDegreePlanArchive(degreePlanToArchive, CurrentUser.PersonId, studentPrograms, courses, degreePlanSections, studentAcademicCredits, grades);
            var degreePlanArchive = await _degreePlanArchiveRepository.AddAsync(newDegreePlanArchive);

            // Get the right adapter for the type mapping and convert the degree plan entity to degree plan DTO
            var degreePlanArchiveDtoAdapter = _adapterRegistry.GetAdapter<Domain.Planning.Entities.DegreePlanArchive, Dtos.Planning.DegreePlanArchive2>();
            var degreePlanArchiveDto = degreePlanArchiveDtoAdapter.MapToType(degreePlanArchive);

            return degreePlanArchiveDto;
        }

        //TODO
        /// <summary>
        /// Retrieves all degree plan archives for a specified degree plan id.
        /// </summary>
        /// <param name="degreePlanId">Id of the degree plan</param>
        /// <returns></returns>
        [Obsolete("Obsolete with version 1.5 of the Api. Use GetDegreePlanArchives2.")]
        public async Task<IEnumerable<Dtos.Planning.DegreePlanArchive>> GetDegreePlanArchivesAsync(int degreePlanId)
        {
            // Get the degree plan from the repository
            Domain.Student.Entities.DegreePlans.DegreePlan degreePlan = await _studentDegreePlanRepository.GetAsync(degreePlanId);

            // Make sure the current user has permission to view the degree plan (and therefore it's archives) 
            if (!UserIsSelf(degreePlan.PersonId))
            {
                await CheckViewPlanPermissionsAsync(degreePlan.PersonId); //TODO
            }

            // Archive the degree plan  //TODO
            IEnumerable<Domain.Planning.Entities.DegreePlanArchive> degreePlanArchives = await _degreePlanArchiveRepository.GetDegreePlanArchivesAsync(degreePlanId);

            List<Dtos.Planning.DegreePlanArchive> degreePlanArchiveDtos = new List<Dtos.Planning.DegreePlanArchive>();

            // Get the right adapter for the type mapping and convert the degree plan entity to degree plan DTO
            var degreePlanArchiveDtoAdapter = _adapterRegistry.GetAdapter<Domain.Planning.Entities.DegreePlanArchive, Dtos.Planning.DegreePlanArchive>();

            foreach (var degreePlanArchive in degreePlanArchives)
            {
                var degreePlanArchiveDto = degreePlanArchiveDtoAdapter.MapToType(degreePlanArchive);
                degreePlanArchiveDtos.Add(degreePlanArchiveDto);
            }

            return degreePlanArchiveDtos;
        }

        //TODO
        /// <summary>
        /// Retrieves all degree plan archives for a specified degree plan id.
        /// </summary>
        /// <param name="degreePlanId">Id of the degree plan</param>
        /// <returns>List of <see cref="DegreePlanArchive2">Degree Plan Archive</see> objects.</returns>
        public async Task<IEnumerable<Dtos.Planning.DegreePlanArchive2>> GetDegreePlanArchives2Async(int degreePlanId)
        {
            // Get the degree plan from the repository
            Domain.Student.Entities.DegreePlans.DegreePlan degreePlan = await _studentDegreePlanRepository.GetAsync(degreePlanId);

            // Make sure the current user has permission to view the degree plan (and therefore it's archives) 
            await CheckViewPlanPermissionsAsync(degreePlan.PersonId);

            // Archive the degree plan
            IEnumerable<Domain.Planning.Entities.DegreePlanArchive> degreePlanArchives = await _degreePlanArchiveRepository.GetDegreePlanArchivesAsync(degreePlanId);

            List<Dtos.Planning.DegreePlanArchive2> degreePlanArchiveDtos = new List<Dtos.Planning.DegreePlanArchive2>();

            // Get the right adapter for the type mapping and convert the degree plan entity to degree plan DTO
            var degreePlanArchiveDtoAdapter = _adapterRegistry.GetAdapter<Domain.Planning.Entities.DegreePlanArchive, Dtos.Planning.DegreePlanArchive2>();

            foreach (var degreePlanArchive in degreePlanArchives)
            {
                var degreePlanArchiveDto = degreePlanArchiveDtoAdapter.MapToType(degreePlanArchive);
                degreePlanArchiveDtos.Add(degreePlanArchiveDto);
            }

            return degreePlanArchiveDtos;
        }

        //TODO
        /// <summary>
        /// Given an archive ID, return the archive report object, which contains the data required to generate the pdf.
        /// </summary>
        /// <param name="values">The system ID for the requested plan archive</param>
        /// <returns>A DegreePlanArchiveReport object</returns>
        public async Task<DegreePlanArchiveReport> GetDegreePlanArchiveReportAsync(int archiveId)
        {
            try
            {
                logger.Info("Begin GetDegreePlanArchiveReport");
                var degreePlanArchive = await _degreePlanArchiveRepository.GetDegreePlanArchiveAsync(archiveId);

                // Get student from repository, it's required for permission checking, and then for building the report DTO
                Domain.Student.Entities.Student student = await _studentRepository.GetAsync(degreePlanArchive.StudentId);

                // Make sure the current user has permission to view the degree plan (and therefore it's archives) 
                if (!UserIsSelf(degreePlanArchive.StudentId))
                {
                    await CheckViewPlanPermissionsAsync(degreePlanArchive.StudentId, student);
                }

                // Gather information needed to build the DegreePlanArchiveReport model.
                var programs = await _programRepository.GetAsync();
                // Set the planned term data in the report
                var terms = (await _termRepository.GetAsync()).ToList(); //TODO
                logger.Info("GetDegreePlanArchiveReport: After retrieval of programs and terms.");

                //read student program file too
                IEnumerable<Domain.Student.Entities.StudentProgram> studentPrograms = await _studentProgramRepository.GetAsync(degreePlanArchive.StudentId);

                // Get all related advisors needed to build the plan (created by, reviewed by, note writers, approvers, and added by)
                var peopleIds = new List<string>();
                if (!string.IsNullOrEmpty(degreePlanArchive.CreatedBy)) { peopleIds.Add(degreePlanArchive.CreatedBy); }
                if (!string.IsNullOrEmpty(degreePlanArchive.ReviewedBy)) { peopleIds.Add(degreePlanArchive.ReviewedBy); }
                var writtenByIds = degreePlanArchive.Notes.Select(n => n.PersonId);
                peopleIds.AddRange(writtenByIds);
                var approvedByIds = degreePlanArchive.ArchivedCourses.Select(a => a.ApprovedBy);
                peopleIds.AddRange(approvedByIds);
                var addedByIds = degreePlanArchive.ArchivedCourses.Select(a => a.AddedBy);
                peopleIds.AddRange(addedByIds);
                peopleIds = peopleIds.Distinct().ToList();
                var degreePlanPeople = new List<Domain.Planning.Entities.Advisor>();
                logger.Info("GetDegreePlanArchiveReport: Getting People for Ids ");
                foreach (var id in peopleIds)
                {
                    try
                    {
                        var advisor = await _advisorRepository.GetAsync(id, Domain.Planning.Entities.AdviseeInclusionType.NoAdvisees);
                        degreePlanPeople.Add(advisor);
                    }
                    catch (Exception)
                    {
                        // Can't find advisor 
                    }
                }
                logger.Info("GetDegreePlanArchiveReport: Call to DegreePlanArchiveReport constructor");
                // Now create the DegreePlanReportArchive
                var report = new DegreePlanArchiveReport(degreePlanArchive, student, programs, degreePlanPeople, terms, studentPrograms);
                logger.Info("GetDegreePlanArchiveReport complete.");
                return report;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public Byte[] GenerateDegreePlanArchiveReport(DegreePlanArchiveReport archiveReport, string path, string reportLogoPath)
        {

            // Create the report object, set it's path, and set permissions for the sandboxed app domain in which the report runs to unrestricted
            LocalReport report = new LocalReport();
            report.ReportPath = path;
            report.SetBasePermissionsForSandboxAppDomain(new System.Security.PermissionSet(System.Security.Permissions.PermissionState.Unrestricted));
            report.EnableExternalImages = true;

            var parameters = new List<ReportParameter>();

            parameters.Add(new ReportParameter("LogoPath", reportLogoPath));
            parameters.Add(new ReportParameter("ReportTitle", archiveReport.ReportTitle));
            parameters.Add(new ReportParameter("StudentName", archiveReport.StudentName));
            parameters.Add(new ReportParameter("Label_StudentName", "Student"));

            parameters.Add(new ReportParameter("StudentId", archiveReport.StudentId));
            parameters.Add(new ReportParameter("Label_StudentId", "ID"));

            var programStrings = new List<string>();
            foreach (var program in archiveReport.StudentPrograms)
            {
                programStrings.Add(program.Key + ", " + program.Value);
            }
            parameters.Add(new ReportParameter("StudentPrograms", programStrings.ToArray()));
            parameters.Add(new ReportParameter("Label_StudentPrograms", "Programs"));

            parameters.Add(new ReportParameter("LastReviewed", archiveReport.ReviewedBy + " on " + archiveReport.ReviewedOn.DateTime.ToShortDateString()));
            parameters.Add(new ReportParameter("Label_LastReviewed", "Reviewed By"));

            parameters.Add(new ReportParameter("Label_Notes", "Notes"));

            parameters.Add(new ReportParameter("ArchivedBy", archiveReport.ArchivedBy + " on " + archiveReport.ArchivedOn.DateTime.ToShortDateString() + " at " + archiveReport.ArchivedOn.DateTime.ToShortTimeString()));
            parameters.Add(new ReportParameter("Label_ArchivedBy", "Archived By"));
            report.SetParameters(parameters);

            report.DataSources.Add(new ReportDataSource("ArchivedCourses", archiveReport.ArchivedCoursesDataSet.Tables[0]));

            report.DataSources.Add(new ReportDataSource("ArchivedNotes", archiveReport.ArchivedNotesDataSet.Tables[0]));

            // Set up some options for the report
            string mimeType = string.Empty;
            string encoding;
            string fileNameExtension;
            Warning[] warnings;
            string[] streams;

            // Render the report as a byte array
            var renderedBytes = report.Render(
                PdfReportConstants.ReportType,
                PdfReportConstants.DeviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings);

            // Recover and free up memory
            report.DataSources.Clear();
            report.ReleaseSandboxAppDomain();
            report.Dispose();

            return renderedBytes;
        }

        /// <summary>
        /// Get review requested degree plans
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns>DegreePlanReviewRequest collection</returns>
        public async Task<IEnumerable<DegreePlanReviewRequest>> GetReviewRequestedDegreePlans(DegreePlansSearchCriteria criteria, int pageSize = int.MaxValue, int pageIndex = 1)
        {
            if(criteria == null)
            {
                throw new ArgumentNullException("criteria", "Criteria cannot be empty/null for degree plan review request query.");
            }
            Stopwatch watch = null;
            if (logger.IsInfoEnabled)
            {
                watch = new Stopwatch();
                watch.Start();
            }
           
            if (pageSize < 1) pageSize = int.MaxValue;
            if (pageIndex < 1) pageIndex = 1;

            // Check if the user has view permissions
            await CheckOpenOfficeViewPlanPermissionsAsync();

            var degreePlans = await _degreePlanRepository.GetReviewReqestedAsync();

            //Filter the results. Do not apply filter if DegreePlanRetrievalType.All
            if (criteria.DegreePlanRetrievalType == DegreePlanRetrievalType.UnAssignedAdvisees)
            {
                degreePlans = degreePlans.Where(dp => dp.AssignedReviewer == null);
            }
            else if(criteria.DegreePlanRetrievalType == DegreePlanRetrievalType.AssignedToOthers)
            {
                degreePlans = degreePlans.Where(dp => dp.AssignedReviewer != null && dp.AssignedReviewer != CurrentUser.PersonId);
            }
            else if(criteria.DegreePlanRetrievalType == DegreePlanRetrievalType.MyAssignedAdvisees)
            {
                degreePlans = degreePlans.Where(dp => dp.AssignedReviewer != null && dp.AssignedReviewer.Equals(CurrentUser.PersonId));
            }

            //Sort the records on requested date
            degreePlans = degreePlans.OrderBy(dp => dp.ReviewRequestedDate);

            var totalItems = degreePlans.Count();
            var totalPages = (int)Math.Ceiling((decimal)totalItems / pageSize);
            degreePlans = degreePlans.Skip(pageSize * (pageIndex - 1)).Take(pageSize);

            System.Collections.ObjectModel.Collection<DegreePlanReviewRequest> degreePlansad = new System.Collections.ObjectModel.Collection<DegreePlanReviewRequest>();
            var degreePlanDtoAdapter = _adapterRegistry.GetAdapter<Domain.Planning.Entities.DegreePlanReviewRequest, DegreePlanReviewRequest>();

            foreach (var degreePlan in degreePlans)
            {
                var dpRwAssignDto = degreePlanDtoAdapter.MapToType(degreePlan);
                degreePlansad.Add(dpRwAssignDto);
            }

            if (logger.IsInfoEnabled)
            {
                watch.Stop();
                logger.Info("DegreePlan Timing: (GetReviewRequestedDegreePlans) _degreePlanRepository.GetReviewRequestedDegreePlans completed in " + watch.ElapsedMilliseconds.ToString() + " ms");
            }
            return degreePlansad;
        }

        /// <summary>
        /// Get review requested degree plans
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns>DegreePlanReviewRequest collection</returns>
        public async Task<IEnumerable<DegreePlanReviewRequest>> SearchReviewRequestDegreePlans(DegreePlansSearchCriteria criteria, int pageSize = int.MaxValue, int pageIndex = 1)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Criteria cannot be empty/null for degree plan review request query.");
            }
            Stopwatch watch = null;
            if (logger.IsInfoEnabled)
            {
                watch = new Stopwatch();
                watch.Start();
            }

            if (pageSize < 1) pageSize = int.MaxValue;
            if (pageIndex < 1) pageIndex = 1;
            // Check if the user has view permissions
            await CheckOpenOfficeViewPlanPermissionsAsync();

            Dtos.Student.StudentSearchCriteria studentSearchCriteria = new Dtos.Student.StudentSearchCriteria();
            studentSearchCriteria.StudentKeyword = criteria.AdviseeKeyword;

            var degreePlans = await _degreePlanRepository.GetReviewReqestedAsync();

            string searchString = criteria.AdviseeKeyword;
            var tempString = searchString.Trim();
            Regex regEx = new Regex(@"\s+");
            searchString = regEx.Replace(tempString, @" ");
            List<Domain.Student.Entities.Student> students = null;

            double personId;
            bool isId = double.TryParse(searchString, out personId);
            if (isId && !string.IsNullOrEmpty(searchString))
            {
                students = (await _studentRepository.GetStudentsSearchAsync(new List<string>() { searchString })).ToList();
            }
            else
            {
                string lastName = null;
                string firstName = null;
                string middleName = null;
                // Regular expression for all punctuation and numbers to remove from name string
                Regex regexNotPunc = new Regex(@"[!-&(-,.-@[-`{-~]");
                Regex regexNotSpace = new Regex(@"\s");

                var nameStrings = searchString.Split(',');
                // If there was a comma, set the first item to last name
                if (nameStrings.Count() > 1)
                {
                    lastName = nameStrings.ElementAt(0).Trim();
                    if (nameStrings.Count() >= 2)
                    {
                        // parse the two items after the comma using a space. Ignore anything else
                        var nameStrings2 = nameStrings.ElementAt(1).Trim().Split(' ');
                        if (nameStrings2.Count() >= 1) { firstName = nameStrings2.ElementAt(0).Trim(); }
                        if (nameStrings2.Count() >= 2) { middleName = nameStrings2.ElementAt(1).Trim(); }
                    }
                }
                else
                {
                    // Parse entry using spaces, assume entered (last) or (first last) or (first middle last). 
                    // Blank values don't hurt anything.
                    nameStrings = searchString.Split(' ');
                    switch (nameStrings.Count())
                    {
                        case 1:
                            lastName = nameStrings.ElementAt(0).Trim();
                            break;
                        case 2:
                            firstName = nameStrings.ElementAt(0).Trim();
                            lastName = nameStrings.ElementAt(1).Trim();
                            break;
                        default:
                            firstName = nameStrings.ElementAt(0).Trim();
                            middleName = nameStrings.ElementAt(1).Trim();
                            lastName = nameStrings.ElementAt(2).Trim();
                            break;
                    }
                }

                // Remove characters that won't make sense for each name part, including all punctuation and numbers 
                if (lastName != null)
                {
                    lastName = regexNotPunc.Replace(lastName, "");
                    lastName = regexNotSpace.Replace(lastName, "");
                }
                if (firstName != null)
                {
                    firstName = regexNotPunc.Replace(firstName, "");
                    firstName = regexNotSpace.Replace(firstName, "");
                }
                if (middleName != null)
                {
                    middleName = regexNotPunc.Replace(middleName, "");
                    middleName = regexNotSpace.Replace(middleName, "");
                }

                if (!string.IsNullOrEmpty(criteria.AdviseeKeyword))
                {
                    students = (await _studentRepository.GetStudentSearchByNameAsync(lastName, firstName, middleName)).ToList();
                }
            }

            var filteredResults = (from dp in degreePlans
                                         join student in students
                                         on dp.PersonId equals student.Id                                        
                                         select new DegreePlanReviewRequest(){
                                             Id = dp.Id,
                                             AssignedReviewer = dp.AssignedReviewer,
                                             PersonId=dp.PersonId,
                                             ReviewRequestedDate = dp.ReviewRequestedDate                                           
                                       }).ToList();
           
            if (criteria.DegreePlanRetrievalType == DegreePlanRetrievalType.UnAssignedAdvisees)
            {
                filteredResults = (from student in students
                                   join dp in degreePlans
                                   on student.Id equals dp.PersonId into results
                                   from result in results.DefaultIfEmpty()
                                   select new DegreePlanReviewRequest()
                                   {
                                       Id = (result != null) ? result.Id : null,
                                       AssignedReviewer = (result != null) ? result.AssignedReviewer : null,
                                       PersonId = (result != null) ? result.PersonId : student.Id,
                                       ReviewRequestedDate = (result != null) ? result.ReviewRequestedDate : null
                                   }).ToList();
            }
            else if (criteria.DegreePlanRetrievalType == DegreePlanRetrievalType.AssignedToOthers)
            {
                filteredResults = filteredResults.Where(dp => dp.AssignedReviewer != null && dp.AssignedReviewer != CurrentUser.PersonId).ToList();
            }
            else if (criteria.DegreePlanRetrievalType == DegreePlanRetrievalType.MyAssignedAdvisees)
            {
                filteredResults = filteredResults.Where(dp => dp.AssignedReviewer != null && dp.AssignedReviewer.Equals(CurrentUser.PersonId)).ToList();
            }
            //Sort the records on requested date
            filteredResults = filteredResults.OrderBy(dp => dp.ReviewRequestedDate).ToList();

            var totalItems = filteredResults.Count();
            var totalPages = (int)Math.Ceiling((decimal)totalItems / pageSize);
            filteredResults = filteredResults.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();

            degreePlans =filteredResults.Select(x => new Domain.Planning.Entities.DegreePlanReviewRequest { Id = x.Id, ReviewRequestedDate = x.ReviewRequestedDate, AssignedReviewer = x.AssignedReviewer, PersonId = x.PersonId });
           
            System.Collections.ObjectModel.Collection<DegreePlanReviewRequest> degreePlansad = new System.Collections.ObjectModel.Collection<DegreePlanReviewRequest>();
            var degreePlanDtoAdapter = _adapterRegistry.GetAdapter<Domain.Planning.Entities.DegreePlanReviewRequest, DegreePlanReviewRequest>();
           
            foreach (var degreePlan in degreePlans)
            {
                var dpRwAssignDto = degreePlanDtoAdapter.MapToType(degreePlan);
                degreePlansad.Add(dpRwAssignDto);
            }

            if (logger.IsInfoEnabled)
            {
                watch.Stop();
                logger.Info("DegreePlan Timing: (GetReviewRequestedDegreePlans) _degreePlanRepository.GetReviewRequestedDegreePlans completed in " + watch.ElapsedMilliseconds.ToString() + " ms");
            }
            return degreePlansad;
        }

        /// <summary>
        /// Get review requested degree plans
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns>DegreePlanReviewRequest collection</returns>
        public async Task<IEnumerable<DegreePlanReviewRequest>> SearchReviewRequestDegreePlansForExactMatchAsync(DegreePlansSearchCriteria criteria, int pageSize = int.MaxValue, int pageIndex = 1)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Criteria cannot be empty/null for degree plan review request query.");
            }
            Stopwatch watch = null;
            if (logger.IsInfoEnabled)
            {
                watch = new Stopwatch();
                watch.Start();
            }

            if (pageSize < 1) pageSize = int.MaxValue;
            if (pageIndex < 1) pageIndex = 1;
            // Check if the user has view permissions
            await CheckOpenOfficeViewPlanPermissionsAsync();

            Dtos.Student.StudentSearchCriteria studentSearchCriteria = new Dtos.Student.StudentSearchCriteria();
            studentSearchCriteria.StudentKeyword = criteria.AdviseeKeyword;

            var degreePlans = await _degreePlanRepository.GetReviewReqestedAsync();

            string searchString = criteria.AdviseeKeyword;
            var tempString = searchString.Trim();
            Regex regEx = new Regex(@"\s+");
            searchString = regEx.Replace(tempString, @" ");
            List<Domain.Student.Entities.Student> students = null;

            double personId;
            bool isId = double.TryParse(searchString, out personId);
            if (isId && !string.IsNullOrEmpty(searchString))
            {
                students = (await _studentRepository.GetStudentsSearchAsync(new List<string>() { searchString })).ToList();
            }
            else
            {
                string lastName = null;
                string firstName = null;
                string middleName = null;
                // Regular expression for all punctuation and numbers to remove from name string
                Regex regexNotPunc = new Regex(@"[!-&(-,.-@[-`{-~]");
                Regex regexNotSpace = new Regex(@"\s");

                var nameStrings = searchString.Split(',');
                // If there was a comma, set the first item to last name
                if (nameStrings.Count() > 1)
                {
                    lastName = nameStrings.ElementAt(0).Trim();
                    if (nameStrings.Count() >= 2)
                    {
                        // parse the two items after the comma using a space. Ignore anything else
                        var nameStrings2 = nameStrings.ElementAt(1).Trim().Split(' ');
                        if (nameStrings2.Count() >= 1) { firstName = nameStrings2.ElementAt(0).Trim(); }
                        if (nameStrings2.Count() >= 2) { middleName = nameStrings2.ElementAt(1).Trim(); }
                    }
                }
                else
                {
                    // Parse entry using spaces, assume entered (last) or (first last) or (first middle last). 
                    // Blank values don't hurt anything.
                    nameStrings = searchString.Split(' ');
                    switch (nameStrings.Count())
                    {
                        case 1:
                            lastName = nameStrings.ElementAt(0).Trim();
                            break;
                        case 2:
                            firstName = nameStrings.ElementAt(0).Trim();
                            lastName = nameStrings.ElementAt(1).Trim();
                            break;
                        default:
                            firstName = nameStrings.ElementAt(0).Trim();
                            middleName = nameStrings.ElementAt(1).Trim();
                            lastName = nameStrings.ElementAt(2).Trim();
                            break;
                    }
                }

                // Remove characters that won't make sense for each name part, including all punctuation and numbers 
                if (lastName != null)
                {
                    lastName = regexNotPunc.Replace(lastName, "");
                    lastName = regexNotSpace.Replace(lastName, "");
                }
                if (firstName != null)
                {
                    firstName = regexNotPunc.Replace(firstName, "");
                    firstName = regexNotSpace.Replace(firstName, "");
                }
                if (middleName != null)
                {
                    middleName = regexNotPunc.Replace(middleName, "");
                    middleName = regexNotSpace.Replace(middleName, "");
                }

                if (!string.IsNullOrEmpty(criteria.AdviseeKeyword))
                {
                    students = (await _studentRepository.GetStudentSearchByNameForExactMatchAsync(lastName, firstName, middleName)).ToList();
                }
            }

            var filteredResults = (from dp in degreePlans
                                   join student in students
                                   on dp.PersonId equals student.Id
                                   select new DegreePlanReviewRequest()
                                   {
                                       Id = dp.Id,
                                       AssignedReviewer = dp.AssignedReviewer,
                                       PersonId = dp.PersonId,
                                       ReviewRequestedDate = dp.ReviewRequestedDate
                                   }).ToList();

            if (criteria.DegreePlanRetrievalType == DegreePlanRetrievalType.UnAssignedAdvisees)
            {
                filteredResults = (from student in students
                                   join dp in degreePlans
                                   on student.Id equals dp.PersonId into results
                                   from result in results.DefaultIfEmpty()
                                   select new DegreePlanReviewRequest()
                                   {
                                       Id = (result != null) ? result.Id : null,
                                       AssignedReviewer = (result != null) ? result.AssignedReviewer : null,
                                       PersonId = (result != null) ? result.PersonId : student.Id,
                                       ReviewRequestedDate = (result != null) ? result.ReviewRequestedDate : null
                                   }).ToList();
            }
            else if (criteria.DegreePlanRetrievalType == DegreePlanRetrievalType.AssignedToOthers)
            {
                filteredResults = filteredResults.Where(dp => dp.AssignedReviewer != null && dp.AssignedReviewer != CurrentUser.PersonId).ToList();
            }
            else if (criteria.DegreePlanRetrievalType == DegreePlanRetrievalType.MyAssignedAdvisees)
            {
                filteredResults = filteredResults.Where(dp => dp.AssignedReviewer != null && dp.AssignedReviewer.Equals(CurrentUser.PersonId)).ToList();
            }
            //Sort the records on requested date
            filteredResults = filteredResults.OrderBy(dp => dp.ReviewRequestedDate).ToList();

            var totalItems = filteredResults.Count();
            var totalPages = (int)Math.Ceiling((decimal)totalItems / pageSize);
            filteredResults = filteredResults.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();

            degreePlans = filteredResults.Select(x => new Domain.Planning.Entities.DegreePlanReviewRequest { Id = x.Id, ReviewRequestedDate = x.ReviewRequestedDate, AssignedReviewer = x.AssignedReviewer, PersonId = x.PersonId });

            System.Collections.ObjectModel.Collection<DegreePlanReviewRequest> degreePlansad = new System.Collections.ObjectModel.Collection<DegreePlanReviewRequest>();
            var degreePlanDtoAdapter = _adapterRegistry.GetAdapter<Domain.Planning.Entities.DegreePlanReviewRequest, DegreePlanReviewRequest>();

            foreach (var degreePlan in degreePlans)
            {
                var dpRwAssignDto = degreePlanDtoAdapter.MapToType(degreePlan);
                degreePlansad.Add(dpRwAssignDto);
            }

            if (logger.IsInfoEnabled)
            {
                watch.Stop();
                logger.Info("DegreePlan Timing: (GetReviewRequestedDegreePlans) _degreePlanRepository.GetReviewRequestedDegreePlans completed in " + watch.ElapsedMilliseconds.ToString() + " ms");
            }
            return degreePlansad;
        }

        /// <summary>
        /// Create/Update advisor review assignment
        /// </summary>
        /// <param name="degreePlanReviewRequest"></param>
        /// <returns>Newly created DegreePlanReviewRequest</returns>
        public async Task<DegreePlanReviewRequest> UpdateAdvisorAssignment(DegreePlanReviewRequest degreePlanReviewRequest)
        {
            if (degreePlanReviewRequest == null)
            {
                throw new ArgumentNullException("degreePlanReviewRequest", "degreePlanReviewRequest cannot be empty/null.");
            }

            await CheckOpenOfficeViewPlanPermissionsAsync();

            try
            {
                var degreePlanDtoAdapter = _adapterRegistry.GetAdapter<DegreePlanReviewRequest, Domain.Planning.Entities.DegreePlanReviewRequest >();
                var dpRwAssignDto = degreePlanDtoAdapter.MapToType(degreePlanReviewRequest);

                var degreePlanReviewRequestResponse = await _degreePlanRepository.UpdateAdvisorAssignment(dpRwAssignDto);

                var degreePlanEntityAdapter = _adapterRegistry.GetAdapter<Domain.Planning.Entities.DegreePlanReviewRequest, DegreePlanReviewRequest>();
                return degreePlanEntityAdapter.MapToType(degreePlanReviewRequestResponse);
            }
            catch (Exception)
            {
                logger.Error("Unable to create degree plan review assignment");
                throw new Exception("Unable to create degree plan review assignment.");
            }
        }

        /// <summary>
        /// Determines if the user has permission to view the student's degree plan
        /// </summary>
        /// <returns></returns>
        protected async Task CheckOpenOfficeViewPlanPermissionsAsync()
        {
            // Get user permissions
            IEnumerable<string> userPermissions = await GetUserPermissionCodesAsync();
            // Access is Ok if this is an advisor with all access, update access or review access to any student.
            if (userPermissions.Contains(PlanningPermissionCodes.AllAccessAnyAdvisee) ||
                userPermissions.Contains(PlanningPermissionCodes.UpdateAnyAdvisee) ||
                userPermissions.Contains(PlanningPermissionCodes.ReviewAnyAdvisee) ||
                userPermissions.Contains(PlanningPermissionCodes.ViewAnyAdvisee)  ||
                userPermissions.Contains(PlanningPermissionCodes.UpdateAdvisorAssignments))
            {
                return;
            }

            logger.Error("User " + CurrentUser.PersonId + " does not have permissions to view degree plans");
            throw new PermissionsException();
        }
    }
}
