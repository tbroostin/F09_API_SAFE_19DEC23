// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using slf4net;
using Ellucian.Web.Security;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class StudentDegreePlanService : StudentCoordinationService, IStudentDegreePlanService
    {
        private readonly IStudentDegreePlanRepository _studentDegreePlanRepository;
        private readonly ITermRepository _termRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IStudentProgramRepository _studentProgramRepository;
        private readonly IProgramRepository _programRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IAcademicCreditRepository _academicCreditRepository;
        private readonly IRequirementRepository _requirementRepository;
        private readonly IRuleRepository _ruleRepository;
        private readonly IProgramRequirementsRepository _programRequirementsRepository;
        private readonly ICatalogRepository _catalogRepository;
        private readonly IGradeRepository _gradeRepository;
        private readonly IAcademicHistoryService _academicHistoryService;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IStudentConfigurationRepository _studentConfigurationRepository;  

        public StudentDegreePlanService(IAdapterRegistry adapterRegistry, IStudentDegreePlanRepository studentDegreePlanRepository, ITermRepository termRepository, IStudentRepository studentRepository,
            IStudentProgramRepository studentProgramRepository, ICourseRepository courseRepository, ISectionRepository sectionRepository, IProgramRepository programRepository,
            IAcademicCreditRepository academicCreditRepository, IRequirementRepository requirementRepository, IRuleRepository ruleRepository, IProgramRequirementsRepository programRequirementsRepository,
            ICatalogRepository catalogRepository, IGradeRepository gradeRepository, IAcademicHistoryService academicHistoryService,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger,
            IConfigurationRepository configurationRepository, IStudentConfigurationRepository studentConfigurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _studentDegreePlanRepository = studentDegreePlanRepository;
            _termRepository = termRepository;
            _studentRepository = studentRepository;
            _studentProgramRepository = studentProgramRepository;
            _courseRepository = courseRepository;
            _sectionRepository = sectionRepository;
            _programRepository = programRepository;
            _academicCreditRepository = academicCreditRepository;
            _ruleRepository = ruleRepository;
            _requirementRepository = requirementRepository;
            _programRequirementsRepository = programRequirementsRepository;
            _catalogRepository = catalogRepository;
            _gradeRepository = gradeRepository;
            _academicHistoryService = academicHistoryService;
            _studentConfigurationRepository = studentConfigurationRepository;
        }

        /// <summary>
        /// VERSION 1: OBSOLETE - USE GETDEGREEPLAN2 WITH API 1.3 AND LATER. 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Obsolete("Obsolete with version 1.2 of the Api. Use GetDegreePlan3 going forward.")]
        public async Task<Dtos.Student.DegreePlans.DegreePlan> GetDegreePlanAsync(int id)
        {
            // Get the degree plan entity with an ID of id.
            var degreePlan = await _studentDegreePlanRepository.GetAsync(id);
            if (!UserIsSelf(degreePlan.PersonId))
            {
                // Make sure user has permissions to view this degree plan. 
                // If not, an PermissionsException will be thrown.
                await CheckViewPlanPermissionsAsync(degreePlan.PersonId); //TODO
            }

            // Check for conflicts in degree plan
            degreePlan = await CheckForConflictsInDegreePlanAsync(degreePlan);

            // Get the right adapter for the type mapping
            var degreePlanDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan>();

            // Map the degree plan entity to the degree plan DTO
            var degreePlanDto = degreePlanDtoAdapter.MapToType(degreePlan);

            return degreePlanDto;
        }


        /// <summary>
        /// Get a course plan by ID number
        /// </summary>
        /// <param name="id">id of plan to retrieve</param>
        /// <returns>A DegreePlan2 DTO</returns>
        [Obsolete("Obsolete as of API 1.5. Use GetDegreePlan3.")]
        public async Task<Dtos.Student.DegreePlans.DegreePlan2> GetDegreePlan2Async(int id)
        {
            // Get the degree plan entity with an ID of id.
            var degreePlan = await _studentDegreePlanRepository.GetAsync(id);
            if (!UserIsSelf(degreePlan.PersonId))
            {
                // Make sure user has permissions to view this degree plan. 
                // If not, an PermissionsException will be thrown.
                await CheckViewPlanPermissionsAsync(degreePlan.PersonId);
            }

            // Check for conflicts in degree plan
            degreePlan = await CheckForConflictsInDegreePlanAsync(degreePlan);

            // Get the right adapter for the type mapping
            var degreePlanDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan2>();

            // Map the degree plan entity to the degree plan DTO
            var degreePlanDto = degreePlanDtoAdapter.MapToType(degreePlan);

            return degreePlanDto;
        }

        //TODO
        /// <summary>
        /// Get a course plan by ID number
        /// </summary>
        /// <param name="id">id of plan to retrieve</param>
        /// <returns>A DegreePlan3 DTO</returns>
        [Obsolete("Obsolete as of API 1.6. Use GetDegreePlan4.")]
        public async Task<Dtos.Student.DegreePlans.DegreePlan3> GetDegreePlan3Async(int id)
        {
            // Get the degree plan entity with an ID of id.
            var degreePlan = await _studentDegreePlanRepository.GetAsync(id);
            if (!UserIsSelf(degreePlan.PersonId))
            {
                // Make sure user has permissions to view this degree plan. 
                // If not, an PermissionsException will be thrown.
                await CheckViewPlanPermissionsAsync(degreePlan.PersonId); //TODO
            }

            // Check for conflicts in degree plan
            degreePlan = await CheckForConflictsInDegreePlanAsync(degreePlan);

            // Get the right adapter for the type mapping
            var degreePlanDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan3>();

            // Map the degree plan entity to the degree plan DTO
            var degreePlanDto = degreePlanDtoAdapter.MapToType(degreePlan);

            return degreePlanDto;
        }

        //TODO
        /// <summary>
        /// Get a course plan by ID number
        /// </summary>
        /// <param name="id">id of plan to retrieve</param>
        /// <param name="validate">true returns a validated degree plan, false does not validate</param>
        /// <returns>A combined dto containing DegreePlan4 and AcademicHistory2 DTO</returns>
        [Obsolete("Obsolete as of API 1.11. Use GetDegreePlan5Async")]
        public async Task<Dtos.Student.DegreePlans.DegreePlanAcademicHistory> GetDegreePlan4Async(int id, bool validate = true)
        {
            Stopwatch watch = null;
            if (logger.IsInfoEnabled)
            {
                watch = new Stopwatch();
                watch.Start();
            }

            // Get the degree plan entity with an ID of id.
            var degreePlan = await _studentDegreePlanRepository.GetAsync(id);
            if (logger.IsInfoEnabled)
            {
                watch.Stop();
                logger.Info("DegreePlan Timing: (GetDegreePlan) _studentDegreePlanRepository.Get completed in " + watch.ElapsedMilliseconds.ToString() + " ms");
            }

            // Make sure user has permissions to view this degree plan. 
            // If not, an PermissionsException will be thrown.
            await CheckViewPlanPermissionsAsync(degreePlan.PersonId);

            if (logger.IsInfoEnabled)
            {
                watch.Restart();
            }

            // Get the academic credits for the given student
            var studentAcademicCredits = new List<Domain.Student.Entities.AcademicCredit>();
            var creditsByStudentDict = await _academicCreditRepository.GetAcademicCreditByStudentIdsAsync(new List<string>() { degreePlan.PersonId });
            if (creditsByStudentDict.ContainsKey(degreePlan.PersonId))
            {
                studentAcademicCredits = creditsByStudentDict[degreePlan.PersonId];
            }
            if (logger.IsInfoEnabled)
            {
                watch.Stop();
                logger.Info("DegreePlan Timing: (GetDegreePlan) _academidCreditRepository.Get completed in " + watch.ElapsedMilliseconds.ToString() + " ms");
            }

            // Check for conflicts in degree plan. This will be done only if the verify argument is true (default)
            if (validate)
            {
                if (logger.IsInfoEnabled)
                {
                    logger.Info("DegreePlan Timing: (GetDegreePlan) CheckForConflictsInDegreePlan starting");
                    watch.Restart();
                }

                degreePlan = await CheckForConflictsInDegreePlanAsync(degreePlan, studentAcademicCredits);

                if (logger.IsInfoEnabled)
                {
                    watch.Stop();
                    logger.Info("DegreePlan Timing: (GetDegreePlan) CheckForConflictsInDegreePlan completed in " + watch.ElapsedMilliseconds.ToString() + " ms");

                    watch.Restart();
                }
            }

            var resultDto = new Dtos.Student.DegreePlans.DegreePlanAcademicHistory();
            // Get the right adapter for the type mapping
            var degreePlanDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>();

            // Map the degree plan entity to the degree plan DTO
            resultDto.DegreePlan = degreePlanDtoAdapter.MapToType(degreePlan);

            // Take Student and Academic credits we already have and build the academic history object
            resultDto.AcademicHistory = await _academicHistoryService.ConvertAcademicCreditsToAcademicHistoryDtoAsync(degreePlan.PersonId, studentAcademicCredits); //TODO
            if (logger.IsInfoEnabled)
            {
                watch.Stop();
                logger.Info("DegreePlan Timing: (GetDegreePlan) conversion to dto completed in " + watch.ElapsedMilliseconds.ToString() + " ms");
            }
            return resultDto;
        }


        /// <summary>
        /// Get a course plan by ID number
        /// </summary>
        /// <param name="id">id of plan to retrieve</param>
        /// <param name="validate">true returns a validated degree plan, false does not validate</param>
        /// <returns>A combined dto containing DegreePlan4 and AcademicHistory3 DTO</returns>
        [Obsolete("Obsolete as of API 1.18. Use GetDegreePlan6Async")]
        public async Task<Dtos.Student.DegreePlans.DegreePlanAcademicHistory2> GetDegreePlan5Async(int id, bool validate = true)
        {
            Stopwatch watch = null;
            if (logger.IsInfoEnabled)
            {
                watch = new Stopwatch();
                watch.Start();
            }

            // Get the degree plan entity with an ID of id.
            var degreePlan = await _studentDegreePlanRepository.GetAsync(id);
            if (logger.IsInfoEnabled)
            {
                watch.Stop();
                logger.Info("DegreePlan Timing: (GetDegreePlan) _studentDegreePlanRepository.Get completed in " + watch.ElapsedMilliseconds.ToString() + " ms");
            }

            // Make sure user has permissions to view this degree plan. 
            // If not, an PermissionsException will be thrown.
            await CheckViewPlanPermissionsAsync(degreePlan.PersonId);

            if (logger.IsInfoEnabled)
            {
                watch.Restart();
            }

            // Get the academic credits for the given student
            var studentAcademicCredits = new List<Domain.Student.Entities.AcademicCredit>();
            var creditsByStudentDict = await _academicCreditRepository.GetAcademicCreditByStudentIdsAsync(new List<string>() { degreePlan.PersonId });
            if (creditsByStudentDict.ContainsKey(degreePlan.PersonId))
            {
                studentAcademicCredits = creditsByStudentDict[degreePlan.PersonId];
            }
            if (logger.IsInfoEnabled)
            {
                watch.Stop();
                logger.Info("DegreePlan Timing: (GetDegreePlan) _academidCreditRepository.Get completed in " + watch.ElapsedMilliseconds.ToString() + " ms");
            }

            // Check for conflicts in degree plan. This will be done only if the verify argument is true (default)
            if (validate)
            {
                if (logger.IsInfoEnabled)
                {
                    logger.Info("DegreePlan Timing: (GetDegreePlan) CheckForConflictsInDegreePlan starting");
                    watch.Restart();
                }

                degreePlan = await CheckForConflictsInDegreePlanAsync(degreePlan, studentAcademicCredits);

                if (logger.IsInfoEnabled)
                {
                    watch.Stop();
                    logger.Info("DegreePlan Timing: (GetDegreePlan) CheckForConflictsInDegreePlan completed in " + watch.ElapsedMilliseconds.ToString() + " ms");

                    watch.Restart();
                }
            }

            var resultDto = new Dtos.Student.DegreePlans.DegreePlanAcademicHistory2();
            // Get the right adapter for the type mapping
            var degreePlanDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>();

            // Map the degree plan entity to the degree plan DTO
            resultDto.DegreePlan = degreePlanDtoAdapter.MapToType(degreePlan);

            // Take Student and Academic credits we already have and build the academic history object
            resultDto.AcademicHistory = await _academicHistoryService.ConvertAcademicCreditsToAcademicHistoryDto2Async(degreePlan.PersonId, studentAcademicCredits);
            if (logger.IsInfoEnabled)
            {
                watch.Stop();
                logger.Info("DegreePlan Timing: (GetDegreePlan) conversion to dto completed in " + watch.ElapsedMilliseconds.ToString() + " ms");
            }
            return resultDto;
        }

        /// <summary>
        /// Get a course plan by ID number
        /// </summary>
        /// <param name="id">id of plan to retrieve</param>
        /// <param name="validate">true returns a validated degree plan, false does not validate</param>
        /// <param name="includeDrops">Defaults to false, If true, includes dropped academic credits in the degree plan</param>
        /// <returns>A combined dto containing DegreePlan4 and AcademicHistory4 DTO</returns>
        public async Task<Dtos.Student.DegreePlans.DegreePlanAcademicHistory3> GetDegreePlan6Async(int id, bool validate = true, bool includeDrops = false)
        {
            Stopwatch watch = null;
            if (logger.IsInfoEnabled)
            {
                watch = new Stopwatch();
                watch.Start();
            }

            // Get the degree plan entity with an ID of id.
            var degreePlan = await _studentDegreePlanRepository.GetAsync(id);
            if (logger.IsInfoEnabled)
            {
                watch.Stop();
                logger.Info("DegreePlan Timing: (GetDegreePlan) _studentDegreePlanRepository.Get completed in " + watch.ElapsedMilliseconds.ToString() + " ms");
            }
            // Make sure user has permissions to view this degree plan. 
            // If not, an PermissionsException will be thrown.
            await CheckViewPlanPermissionsAsync(degreePlan.PersonId);

            if (logger.IsInfoEnabled)
            {
                watch.Restart();
            }

            // Get the academic credits for the given student
            var studentAcademicCredits = new List<Domain.Student.Entities.AcademicCredit>();
            var creditsByStudentDict = await _academicCreditRepository.GetAcademicCreditByStudentIdsAsync(new List<string>() { degreePlan.PersonId }, false, true, includeDrops);
            if (creditsByStudentDict.ContainsKey(degreePlan.PersonId))
            {
                studentAcademicCredits = creditsByStudentDict[degreePlan.PersonId];
            }
            if (logger.IsInfoEnabled)
            {
                watch.Stop();
                logger.Info("DegreePlan Timing: (GetDegreePlan) _academidCreditRepository.Get completed in " + watch.ElapsedMilliseconds.ToString() + " ms");
            }

            // Check for conflicts in degree plan. This will be done only if the verify argument is true (default)
            if (validate)
            {
                if (logger.IsInfoEnabled)
                {
                    logger.Info("DegreePlan Timing: (GetDegreePlan) CheckForConflictsInDegreePlan starting");
                    watch.Restart();
                }

                degreePlan = await CheckForConflictsInDegreePlanAsync(degreePlan, studentAcademicCredits);

                if (logger.IsInfoEnabled)
                {
                    watch.Stop();
                    logger.Info("DegreePlan Timing: (GetDegreePlan) CheckForConflictsInDegreePlan completed in " + watch.ElapsedMilliseconds.ToString() + " ms");

                    watch.Restart();
                }
            }

            var resultDto = new Dtos.Student.DegreePlans.DegreePlanAcademicHistory3();
            // Get the right adapter for the type mapping
            var degreePlanDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>();

            // Map the degree plan entity to the degree plan DTO
            resultDto.DegreePlan = degreePlanDtoAdapter.MapToType(degreePlan);

            // Take Student and Academic credits we already have and build the academic history object
            resultDto.AcademicHistory = await _academicHistoryService.ConvertAcademicCreditsToAcademicHistoryDto4Async(degreePlan.PersonId, studentAcademicCredits);
            if (logger.IsInfoEnabled)
            {
                watch.Stop();
                logger.Info("DegreePlan Timing: (GetDegreePlan) conversion to dto completed in " + watch.ElapsedMilliseconds.ToString() + " ms");
            }
            return resultDto;
        }

        /// <summary>
        /// VERSION 1: OBSOLETE - USE CREATEDEGREEPLAN2 WITH API 1.3 AND LATER.
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns></returns>
        public async Task<Dtos.Student.DegreePlans.DegreePlan> CreateDegreePlanAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException(studentId, "Must specify a student Id to create a new plan.");
            }
            // Get student from repository
            Domain.Student.Entities.Student student = await _studentRepository.GetAsync(studentId);

            // Verify current user has the permissions to create a degree plan
            await CheckCreatePlanPermissionsAsync(student); //TODO

            // Get data needed to build a degree plan
            var planningTerms = await _termRepository.GetAsync(); //TODO
            var studentPrograms = await _studentProgramRepository.GetAsync(studentId); //TODO
            var programs = await _programRepository.GetAsync();

            // Create a degree plan for a student - business logic resides in the domain
            var degreePlan = Domain.Student.Entities.DegreePlans.DegreePlan.CreateDegreePlan(student, studentPrograms, planningTerms, programs);

            // Have Repository do the add - could throw exception if student already has a plan.
            var updatedDegreePlan = await _studentDegreePlanRepository.AddAsync(degreePlan);

            // Map the degree plan to a DTO 
            var newDegreePlanDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan>();
            var newPlanDto = newDegreePlanDtoAdapter.MapToType(updatedDegreePlan);
            return newPlanDto;
        }

        /// <summary>
        /// Creates a new degree plan for a student unless one already exists.
        /// </summary>
        /// <param name="studentId">Id of student for whom new plan is to be created</param>
        /// <returns>A DegreePlan2 Dto</returns>
        [Obsolete("Obsolete")]
        public async Task<Dtos.Student.DegreePlans.DegreePlan2> CreateDegreePlan2Async(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException(studentId, "Must specify a student Id to create a new plan.");
            }
            // Get student from repository
            Domain.Student.Entities.Student student = await _studentRepository.GetAsync(studentId);

            // Verify current user has the permissions to create a degree plan
            await CheckCreatePlanPermissionsAsync(student);

            // Get data needed to build a degree plan
            var planningTerms = await _termRepository.GetAsync();
            var studentPrograms = await _studentProgramRepository.GetAsync(studentId);
            var programs = await _programRepository.GetAsync();

            // Create a degree plan for a student - business logic resides in the domain
            var degreePlan = Domain.Student.Entities.DegreePlans.DegreePlan.CreateDegreePlan(student, studentPrograms, planningTerms, programs);

            // Have Repository do the add - could throw exception if student already has a plan.
            var updatedDegreePlan = await _studentDegreePlanRepository.AddAsync(degreePlan);

            // Map the degree plan to a DTO 
            var newDegreePlanDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan2>();
            var newPlanDto = newDegreePlanDtoAdapter.MapToType(updatedDegreePlan);
            return newPlanDto;
        }

        /// <summary>
        /// Creates a new degree plan for a student unless one already exists.
        /// </summary>
        /// <param name="studentId">Id of student for whom new plan is to be created</param>
        /// <returns>A DegreePlan3 Dto</returns>
        [Obsolete("Obsolete as of API 1.6. Use CreateDegreePlan4.")]
        public async Task<Dtos.Student.DegreePlans.DegreePlan3> CreateDegreePlan3Async(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException(studentId, "Must specify a student Id to create a new plan.");
            }
            // Get student from repository
            Domain.Student.Entities.Student student = await _studentRepository.GetAsync(studentId);

            // Verify current user has the permissions to create a degree plan
            await CheckCreatePlanPermissionsAsync(student); //TODO

            // Get data needed to build a degree plan
            var planningTerms = await _termRepository.GetAsync(); //TODO
            var studentPrograms = await _studentProgramRepository.GetAsync(studentId); //TODO
            var programs = await _programRepository.GetAsync();

            // Create a degree plan for a student - business logic resides in the domain
            var degreePlan = Domain.Student.Entities.DegreePlans.DegreePlan.CreateDegreePlan(student, studentPrograms, planningTerms, programs);

            // Have Repository do the add - could throw exception if student already has a plan.
            var updatedDegreePlan = await _studentDegreePlanRepository.AddAsync(degreePlan);

            // Map the degree plan to a DTO 
            var newDegreePlanDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan3>();
            var newPlanDto = newDegreePlanDtoAdapter.MapToType(updatedDegreePlan);
            return newPlanDto;
        }

        /// <summary>
        /// Creates a new degree plan for a student unless one already exists.
        /// </summary>
        /// <param name="studentId">Id of student for whom new plan is to be created</param>
        /// <returns>A DegreePlanAcademicHistory2 Dto which includes combination of DegreePlan4 and AcademicHistory2</returns>
        [Obsolete("Obsolete as of API 1.11. Use CreateDegreePlan5Async.")]
        public async Task<Dtos.Student.DegreePlans.DegreePlanAcademicHistory> CreateDegreePlan4Async(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException(studentId, "Must specify a student Id to create a new plan.");
            }
            // Get student from repository
            Domain.Student.Entities.Student student = await _studentRepository.GetAsync(studentId);

            // Verify current user has the permissions to create a degree plan
            await CheckCreatePlanPermissionsAsync(student);

            // Get data needed to build a degree plan
            var planningTerms = await _termRepository.GetAsync(); //TODO
            var studentPrograms = await _studentProgramRepository.GetAsync(studentId); //TODO
            var programs = await _programRepository.GetAsync();

            // Create a degree plan for a student - business logic resides in the domain
            var degreePlan = Domain.Student.Entities.DegreePlans.DegreePlan.CreateDegreePlan(student, studentPrograms, planningTerms, programs);

            // Have Repository do the add - could throw exception if student already has a plan.
            var updatedDegreePlan = await _studentDegreePlanRepository.AddAsync(degreePlan);

            // Object to return
            var degreePlanAcademicHistoryDto = new Dtos.Student.DegreePlans.DegreePlanAcademicHistory();

            // Map the degree plan to a DTO 
            var newDegreePlanDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>();
            degreePlanAcademicHistoryDto.DegreePlan = newDegreePlanDtoAdapter.MapToType(updatedDegreePlan);

            // Add the academic history dto
            var studentAcademicCredits = await _academicCreditRepository.GetAsync(student.AcademicCreditIds);
            degreePlanAcademicHistoryDto.AcademicHistory = await _academicHistoryService.ConvertAcademicCreditsToAcademicHistoryDtoAsync(studentId, studentAcademicCredits, student);

            return degreePlanAcademicHistoryDto;
        }

        /// <summary>
        /// Creates a new degree plan for a student unless one already exists.
        /// </summary>
        /// <param name="studentId">Id of student for whom new plan is to be created</param>
        /// <returns>A DegreePlanAcademicHistory2 Dto which includes combination of DegreePlan4 and AcademicHistory3</returns>
        [Obsolete("Obsolete as of API 1.18. Use CreateDegreePlan6Async.")]
        public async Task<Dtos.Student.DegreePlans.DegreePlanAcademicHistory2> CreateDegreePlan5Async(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException(studentId, "Must specify a student Id to create a new plan.");
            }
            // Get student from repository
            Domain.Student.Entities.Student student = await _studentRepository.GetAsync(studentId);
            if (student == null)
            {
                throw new Exception("Could not retrieve student details for student Id " + studentId);
            }

            // Verify current user has the permissions to create a degree plan
            await CheckCreatePlanPermissionsAsync(student);

            // Get data needed to build a degree plan
            var planningTerms = await _termRepository.GetAsync();
            var studentPrograms = await _studentProgramRepository.GetAsync(studentId);
            var programs = await _programRepository.GetAsync();

            // Create a degree plan for a student - business logic resides in the domain
            var degreePlan = Domain.Student.Entities.DegreePlans.DegreePlan.CreateDegreePlan(student, studentPrograms, planningTerms, programs);

            // Have Repository do the add - could throw exception if student already has a plan.
            var updatedDegreePlan = await _studentDegreePlanRepository.AddAsync(degreePlan);

            // Object to return
            var degreePlanAcademicHistoryDto = new Dtos.Student.DegreePlans.DegreePlanAcademicHistory2();

            // Map the degree plan to a DTO 
            var newDegreePlanDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>();
            degreePlanAcademicHistoryDto.DegreePlan = newDegreePlanDtoAdapter.MapToType(updatedDegreePlan);

            // Add the academic history dto
            var studentAcademicCredits = await _academicCreditRepository.GetAsync(student.AcademicCreditIds);
            degreePlanAcademicHistoryDto.AcademicHistory = await _academicHistoryService.ConvertAcademicCreditsToAcademicHistoryDto2Async(studentId, studentAcademicCredits, student);

            return degreePlanAcademicHistoryDto;
        }

        /// <summary>
        /// Creates a new degree plan for a student unless one already exists. Depending on the institution's Self-Service registration parameters, the plan may be created with any applicable terms based on 
        /// the Default on Plan attribute of the terms and the student's anticipated completion date.
        /// </summary>
        /// <param name="studentId">Id of student for whom new plan is to be created</param>
        /// <returns>A DegreePlanAcademicHistory3 Dto which includes combination of DegreePlan4 and AcademicHistory4</returns>
        public async Task<Dtos.Student.DegreePlans.DegreePlanAcademicHistory3> CreateDegreePlan6Async(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException(studentId, "Must specify a student Id to create a new plan.");
            }
            // Get student from repository
            Domain.Student.Entities.Student student = await _studentRepository.GetAsync(studentId);
            if (student == null)
            {
                throw new Exception("Could not retrieve student details for student Id " + studentId);
            }
            // Verify current user has the permissions to create a degree plan
            await CheckCreatePlanPermissionsAsync(student);

            // Get data needed to build a degree plan
            var planningTerms = await _termRepository.GetAsync();
            var studentPrograms = await _studentProgramRepository.GetAsync(studentId);
            var programs = await _programRepository.GetAsync();
            var registrationConfiguration = await _studentConfigurationRepository.GetRegistrationConfigurationAsync();
            var createWithDefaultTerms = registrationConfiguration == null || registrationConfiguration.AddDefaultTermsToDegreePlan == true;
            if (createWithDefaultTerms)
            {
                logger.Debug(string.Format("Degree plan for student {0} will be created with applicable terms based on terms' Default on New Course Plans flag.", studentId));
            } else
            {
                logger.Debug(string.Format("Degree plan for student {0} will be created with no terms based on institution's Self-Service Registration Parameters; Add Default Terms to Degree Plan is No.", studentId));
            }

            // Create a degree plan for a student - business logic resides in the domain
            var degreePlan = Domain.Student.Entities.DegreePlans.DegreePlan.CreateDegreePlan(student, studentPrograms, planningTerms, programs, createWithDefaultTerms);

            // Have Repository do the add - could throw exception if student already has a plan.
            var updatedDegreePlan = await _studentDegreePlanRepository.AddAsync(degreePlan);

            // Object to return
            var degreePlanAcademicHistoryDto = new Dtos.Student.DegreePlans.DegreePlanAcademicHistory3();

            // Map the degree plan to a DTO 
            var newDegreePlanDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>();
            degreePlanAcademicHistoryDto.DegreePlan = newDegreePlanDtoAdapter.MapToType(updatedDegreePlan);

            // Add the academic history dto
            var studentAcademicCredits = await _academicCreditRepository.GetAsync(student.AcademicCreditIds);
            degreePlanAcademicHistoryDto.AcademicHistory = await _academicHistoryService.ConvertAcademicCreditsToAcademicHistoryDto4Async(studentId, studentAcademicCredits, student);

            return degreePlanAcademicHistoryDto;
        }

        /// <summary>
        /// VERSION 1: OBSOLETE - USE UPDATEDEGREEPLAN2 WITH API 1.3 AND LATER.
        /// </summary>
        /// <param name="degreePlan"></param>
        /// <returns></returns>
        [Obsolete("Obsoloete with version 1.3 of the API. Use UpdateDegreePlan3.")]
        public async Task<Dtos.Student.DegreePlans.DegreePlan> UpdateDegreePlanAsync(Dtos.Student.DegreePlans.DegreePlan degreePlan)
        {
            // Get the right adapter for the type mapping and convert degree plan DTO to degree plan entity
            var degreePlanEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.DegreePlans.DegreePlan, Domain.Student.Entities.DegreePlans.DegreePlan>();
            var degreePlanToUpdate = degreePlanEntityAdapter.MapToType(degreePlan);
            // If user is not self check user permissions.
            if (!UserIsSelf(degreePlan.PersonId))
            {
                // Get student from repository
                Domain.Student.Entities.Student student = await _studentRepository.GetAsync(degreePlan.PersonId);
                await CheckUpdatePermissionsAsync(student, degreePlanToUpdate); //TODO
            }

            // Update the degree plan
            var updatedDegreePlan = await _studentDegreePlanRepository.UpdateAsync(degreePlanToUpdate);

            // Check for conflicts in degree plan and add them to plan
            updatedDegreePlan = await CheckForConflictsInDegreePlanAsync(updatedDegreePlan);

            // Get the right adapter for the type mapping and convert the degree plan entity to degree plan DTO
            var degreePlanDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan>();
            var degreePlanDto = degreePlanDtoAdapter.MapToType(updatedDegreePlan);

            return degreePlanDto;
        }

        /// <summary>
        /// Accept an updated DegreePlan2 DTO and apply it to the Colleague database.
        /// </summary>
        /// <param name="degreePlan">Degree plan to update</param>
        /// <returns>The updated degree plan - with new version number - if successful</returns>
        [Obsolete("Obsolete as of API 1.5. Use UpdateDegreePlan3")]
        public async Task<Dtos.Student.DegreePlans.DegreePlan2> UpdateDegreePlan2Async(Dtos.Student.DegreePlans.DegreePlan2 degreePlan)
        {
            // Get the right adapter for the type mapping and convert degree plan DTO to degree plan entity
            var degreePlanEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.DegreePlans.DegreePlan2, Domain.Student.Entities.DegreePlans.DegreePlan>();
            var degreePlanToUpdate = degreePlanEntityAdapter.MapToType(degreePlan);
            // Get the stored version of the degree plan for verification of changes
            var storedDegreePlan = await _studentDegreePlanRepository.GetAsync(degreePlanToUpdate.Id);
            degreePlanToUpdate.UpdateMissingProtectionFlags(storedDegreePlan);

            // Check that the user has permissions to do these particular updates
            await CheckUpdatePermissions2Async(degreePlanToUpdate, storedDegreePlan);

            // Update the degree plan
            var updatedDegreePlan = await _studentDegreePlanRepository.UpdateAsync(degreePlanToUpdate);

            // Check for conflicts in degree plan and add them to plan
            updatedDegreePlan = await CheckForConflictsInDegreePlanAsync(updatedDegreePlan);

            // Get the right adapter for the type mapping and convert the degree plan entity to degree plan DTO
            var degreePlanDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan2>();
            var degreePlanDto = degreePlanDtoAdapter.MapToType(updatedDegreePlan);

            return degreePlanDto;
        }

        //TODO
        /// <summary>
        /// Accept an updated DegreePlan3 DTO and apply it to the Colleague database.
        /// </summary>
        /// <param name="degreePlan">Degree plan to update</param>
        /// <returns>The updated degree plan - with new version number - if successful</returns>
        [Obsolete("Obsolete as of API 1.6. Use UpdateDegreePlan4")]
        public async Task<Dtos.Student.DegreePlans.DegreePlan3> UpdateDegreePlan3Async(Dtos.Student.DegreePlans.DegreePlan3 degreePlan)
        {
            // Get the right adapter for the type mapping and convert degree plan DTO to degree plan entity
            var degreePlanEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.DegreePlans.DegreePlan3, Domain.Student.Entities.DegreePlans.DegreePlan>();
            var degreePlanToUpdate = degreePlanEntityAdapter.MapToType(degreePlan);
            // Get the stored version of the degree plan for verification of changes
            var storedDegreePlan = await _studentDegreePlanRepository.GetAsync(degreePlanToUpdate.Id);
            degreePlanToUpdate.UpdateMissingProtectionFlags(storedDegreePlan);

            // Check that the user has permissions to do these particular updates
            await CheckUpdatePermissions2Async(degreePlanToUpdate, storedDegreePlan);

            // Update the degree plan
            var updatedDegreePlan = await _studentDegreePlanRepository.UpdateAsync(degreePlanToUpdate);

            // Check for conflicts in degree plan and add them to plan
            updatedDegreePlan = await CheckForConflictsInDegreePlanAsync(updatedDegreePlan);

            // Get the right adapter for the type mapping and convert the degree plan entity to degree plan DTO
            var degreePlanDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan3>();
            var degreePlanDto = degreePlanDtoAdapter.MapToType(updatedDegreePlan);

            return degreePlanDto;
        }

        /// <summary>
        /// Accept an updated DegreePlan3 DTO and apply it to the Colleague database.
        /// </summary>
        /// <param name="degreePlan">Degree plan to update</param>
        /// <returns>The updated <see cref="DegreePlan4">DegreePlan4</see> DTO - with new version number - if successful, in a combined DTO with the AcademicHistory2 dto</returns>
        [Obsolete("Obsolete as of API 1.11. Use UpdateDegreePlan5Async instead")]
        public async Task<Dtos.Student.DegreePlans.DegreePlanAcademicHistory> UpdateDegreePlan4Async(Dtos.Student.DegreePlans.DegreePlan4 degreePlan)
        {
            Stopwatch watch1 = null;
            if (logger.IsInfoEnabled)
            {
                watch1 = new Stopwatch();
                watch1.Start();
            }
            // Get the right adapter for the type mapping and convert degree plan DTO to degree plan entity
            var degreePlanEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.DegreePlans.DegreePlan4, Domain.Student.Entities.DegreePlans.DegreePlan>();
            var degreePlanToUpdate = degreePlanEntityAdapter.MapToType(degreePlan);
            var storedDegreePlan = await _studentDegreePlanRepository.GetAsync(degreePlanToUpdate.Id);

            // Check that the user has permissions to do these particular updates
            await CheckUpdatePermissions2Async(degreePlanToUpdate, storedDegreePlan);

            // Update the degree plan
            var updatedDegreePlan = await _studentDegreePlanRepository.UpdateAsync(degreePlanToUpdate);

            // Check for conflicts in degree plan and add them to plan
            var studentId = updatedDegreePlan.PersonId;
            var studentAcademicCredits = new List<Domain.Student.Entities.AcademicCredit>();
            var creditsByStudentDict = await _academicCreditRepository.GetAcademicCreditByStudentIdsAsync(new List<string>() { degreePlan.PersonId });
            if (creditsByStudentDict.ContainsKey(degreePlan.PersonId))
            {
                studentAcademicCredits = creditsByStudentDict[degreePlan.PersonId];
            }

            updatedDegreePlan = await CheckForConflictsInDegreePlanAsync(updatedDegreePlan, studentAcademicCredits);

            var degreePlanAcademicHistoryDto = new Dtos.Student.DegreePlans.DegreePlanAcademicHistory();

            // Get the right adapter for the type mapping and convert the degree plan entity to degree plan DTO
            var degreePlanDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>();

            degreePlanAcademicHistoryDto.DegreePlan = degreePlanDtoAdapter.MapToType(updatedDegreePlan);

            // Take Student and Academic credits we already have and build the academic history object
            //TODO?
            degreePlanAcademicHistoryDto.AcademicHistory = await _academicHistoryService.ConvertAcademicCreditsToAcademicHistoryDtoAsync(studentId, studentAcademicCredits);

            if (logger.IsInfoEnabled)
            {
                watch1.Stop();
                logger.Info("DegreePlan Timing: (UpdateDegreePlan) UpdateDegreePlan service method complete in " + watch1.ElapsedMilliseconds.ToString() + " ms");
            }
            return degreePlanAcademicHistoryDto;
        }

        /// <summary>
        /// Accept an updated DegreePlan4 DTO and apply it to the Colleague database.
        /// </summary>
        /// <param name="degreePlan">Degree plan to update</param>
        /// <returns>The updated <see cref="DegreePlanAcademicHistory2">DegreePlan4</see> DTO - if successful, in a combined DTO with the AcademicHistory2 dto</returns>
        [Obsolete("Obsolete as of API 1.18. Use UpdateDegreePlan6Async instead")]
        public async Task<Dtos.Student.DegreePlans.DegreePlanAcademicHistory2> UpdateDegreePlan5Async(Dtos.Student.DegreePlans.DegreePlan4 degreePlan)
        {
            Stopwatch watch1 = null;
            if (logger.IsInfoEnabled)
            {
                watch1 = new Stopwatch();
                watch1.Start();
            }
            // Get the right adapter for the type mapping and convert degree plan DTO to degree plan entity
            var degreePlanEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.DegreePlans.DegreePlan4, Domain.Student.Entities.DegreePlans.DegreePlan>();
            var degreePlanToUpdate = degreePlanEntityAdapter.MapToType(degreePlan);

            var storedDegreePlan = await _studentDegreePlanRepository.GetAsync(degreePlanToUpdate.Id);

            // Check that the user has permissions to do these particular updates
            await CheckUpdatePermissions2Async(degreePlanToUpdate, storedDegreePlan);

            // Update the degree plan
            var updatedDegreePlan = await _studentDegreePlanRepository.UpdateAsync(degreePlanToUpdate);

            // Check for conflicts in degree plan and add them to plan
            var studentId = updatedDegreePlan.PersonId;
            var studentAcademicCredits = new List<Domain.Student.Entities.AcademicCredit>();
            var creditsByStudentDict = await _academicCreditRepository.GetAcademicCreditByStudentIdsAsync(new List<string>() { degreePlan.PersonId });
            if (creditsByStudentDict.ContainsKey(degreePlan.PersonId))
            {
                studentAcademicCredits = creditsByStudentDict[degreePlan.PersonId];
            }

            updatedDegreePlan = await CheckForConflictsInDegreePlanAsync(updatedDegreePlan, studentAcademicCredits);

            var degreePlanAcademicHistoryDto = new Dtos.Student.DegreePlans.DegreePlanAcademicHistory2();

            // Get the right adapter for the type mapping and convert the degree plan entity to degree plan DTO
            var degreePlanDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>();

            degreePlanAcademicHistoryDto.DegreePlan = degreePlanDtoAdapter.MapToType(updatedDegreePlan);

            // Take Student and Academic credits we already have and build the academic history object
            degreePlanAcademicHistoryDto.AcademicHistory = await _academicHistoryService.ConvertAcademicCreditsToAcademicHistoryDto2Async(studentId, studentAcademicCredits);

            if (logger.IsInfoEnabled)
            {
                watch1.Stop();
                logger.Info("DegreePlan Timing: (UpdateDegreePlan) UpdateDegreePlan service method complete in " + watch1.ElapsedMilliseconds.ToString() + " ms");
            }
            return degreePlanAcademicHistoryDto;
        }

        /// <summary>
        /// Accept an updated DegreePlan4 DTO and apply it to the Colleague database.
        /// </summary>
        /// <param name="degreePlan">Degree plan to update</param>
        /// <returns>The updated <see cref="DegreePlanAcademicHistory3">DegreePlan4</see> DTO - if successful, in a combined DTO with the AcademicHistory2 dto</returns>
        public async Task<Dtos.Student.DegreePlans.DegreePlanAcademicHistory3> UpdateDegreePlan6Async(Dtos.Student.DegreePlans.DegreePlan4 degreePlan)
        {
            Stopwatch watch1 = null;
            if (logger.IsInfoEnabled)
            {
                watch1 = new Stopwatch();
                watch1.Start();
            }
            // Get the right adapter for the type mapping and convert degree plan DTO to degree plan entity
            var degreePlanEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.DegreePlans.DegreePlan4, Domain.Student.Entities.DegreePlans.DegreePlan>();
            var degreePlanToUpdate = degreePlanEntityAdapter.MapToType(degreePlan);

            var storedDegreePlan = await _studentDegreePlanRepository.GetAsync(degreePlanToUpdate.Id);

            // Check that the user has permissions to do these particular updates
            await CheckUpdatePermissions2Async(degreePlanToUpdate, storedDegreePlan);

            // Update the degree plan
            var updatedDegreePlan = await _studentDegreePlanRepository.UpdateAsync(degreePlanToUpdate);

            // Check for conflicts in degree plan and add them to plan
            var studentId = updatedDegreePlan.PersonId;
            var studentAcademicCredits = new List<Domain.Student.Entities.AcademicCredit>();
            var creditsByStudentDict = await _academicCreditRepository.GetAcademicCreditByStudentIdsAsync(new List<string>() { degreePlan.PersonId }, false, true, true);
            if (creditsByStudentDict.ContainsKey(degreePlan.PersonId))
            {
                studentAcademicCredits = creditsByStudentDict[degreePlan.PersonId];
            }

            updatedDegreePlan = await CheckForConflictsInDegreePlanAsync(updatedDegreePlan, studentAcademicCredits);

            var degreePlanAcademicHistoryDto = new Dtos.Student.DegreePlans.DegreePlanAcademicHistory3();

            // Get the right adapter for the type mapping and convert the degree plan entity to degree plan DTO
            var degreePlanDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>();

            degreePlanAcademicHistoryDto.DegreePlan = degreePlanDtoAdapter.MapToType(updatedDegreePlan);

            // Take Student and Academic credits we already have and build the academic history object
            degreePlanAcademicHistoryDto.AcademicHistory = await _academicHistoryService.ConvertAcademicCreditsToAcademicHistoryDto4Async(studentId, studentAcademicCredits);

            if (logger.IsInfoEnabled)
            {
                watch1.Stop();
                logger.Info("DegreePlan Timing: (UpdateDegreePlan) UpdateDegreePlan service method complete in " + watch1.ElapsedMilliseconds.ToString() + " ms");
            }
            return degreePlanAcademicHistoryDto;
        }


        /// <summary>
        /// Register the sections on the degree plan for the term specified
        /// </summary>
        /// <param name="degreePlanId">Degree Plan Id</param>
        /// <param name="termId">Term Id</param>
        /// <returns>A list of registration messages.</returns>
        [Obsolete("Obsolete with version 1.5 of the Api. Use StudentService Register method going forward.")]
        public async Task<Dtos.Student.RegistrationResponse> RegisterAsync(int degreePlanId, string termId)
        {
            if (degreePlanId == 0)
            {
                throw new ArgumentException("Invalid degreePlanId", "degreePlanId");
            }
            if (string.IsNullOrEmpty(termId))
            {
                throw new ArgumentException("Invalid termId", "termId");
            }

            var messages = new List<Dtos.Student.RegistrationMessage>();

            var sectionRegistrations = new List<Domain.Student.Entities.SectionRegistration>();

            var degreePlan = await _studentDegreePlanRepository.GetAsync(degreePlanId);
            // Prevent action without proper permissions
            if (!UserIsSelf(degreePlan.PersonId))
            {
                await CheckRegisterPermissionsAsync(degreePlan.PersonId);
            }

            var plannedCourses = degreePlan.GetPlannedCourses(termId).Where(pc => !string.IsNullOrEmpty(pc.SectionId));

            foreach (var course in plannedCourses)
            {
                if (!string.IsNullOrEmpty(course.SectionId))
                {
                    Domain.Student.Entities.RegistrationAction action = Domain.Student.Entities.RegistrationAction.Add;
                    switch (course.GradingType)
                    {
                        case Domain.Student.Entities.GradingType.Graded:
                            break;
                        case Domain.Student.Entities.GradingType.PassFail:
                            action = Domain.Student.Entities.RegistrationAction.PassFail;
                            break;
                        case Domain.Student.Entities.GradingType.Audit:
                            action = Domain.Student.Entities.RegistrationAction.Audit;
                            break;
                        default:
                            break;
                    }
                    sectionRegistrations.Add(new Domain.Student.Entities.SectionRegistration() { SectionId = course.SectionId, Action = action, Credits = course.Credits });
                }
            }

            // Next, find any "non-term" sections with a start date in the specified term and submit those to registration too.
            var term = await _termRepository.GetAsync(termId);
            var nonTermPlannedCourses = degreePlan.GetPlannedCourses(null);
            foreach (var nonCourse in nonTermPlannedCourses)
            {
                if (!string.IsNullOrEmpty(nonCourse.SectionId))
                {
                    var nonTermSection = (await _sectionRepository.GetCachedSectionsAsync(new List<string>() { nonCourse.SectionId })).FirstOrDefault();
                    if (nonTermSection != null)
                    {
                        if (nonTermSection.StartDate >= term.StartDate && nonTermSection.StartDate <= term.EndDate)
                        {
                            Domain.Student.Entities.RegistrationAction action = Domain.Student.Entities.RegistrationAction.Add;
                            switch (nonCourse.GradingType)
                            {
                                case Domain.Student.Entities.GradingType.Graded:
                                    break;
                                case Domain.Student.Entities.GradingType.PassFail:
                                    action = Domain.Student.Entities.RegistrationAction.PassFail;
                                    break;
                                case Domain.Student.Entities.GradingType.Audit:
                                    action = Domain.Student.Entities.RegistrationAction.Audit;
                                    break;
                                default:
                                    break;
                            }
                            sectionRegistrations.Add(new Domain.Student.Entities.SectionRegistration() { SectionId = nonCourse.SectionId, Action = action, Credits = nonCourse.Credits ?? default(decimal) });
                        }
                    }
                }
            }

            if (sectionRegistrations.Count() == 0)
            {
                messages.Add(new Dtos.Student.RegistrationMessage() { Message = "No sections selected within this term" });
                //return messages;
                return new Dtos.Student.RegistrationResponse() { Messages = new List<Dtos.Student.RegistrationMessage>(messages) };
            }

            var request = new Domain.Student.Entities.RegistrationRequest(degreePlan.PersonId, sectionRegistrations);
            var responseEntity = await _studentRepository.RegisterAsync(request);
            var responseDto = new Dtos.Student.RegistrationResponse();
            responseDto.Messages = new List<Dtos.Student.RegistrationMessage>();
            responseDto.PaymentControlId = responseEntity.PaymentControlId;

            foreach (var message in responseEntity.Messages)
            {
                responseDto.Messages.Add(new Dtos.Student.RegistrationMessage { Message = message.Message, SectionId = message.SectionId });
            }

            return responseDto;
        }

        /// <summary>
        /// Take a registration action on a specific section on a degree plan.
        /// </summary>
        /// <param name="degreePlanId">Degree Plan Id</param>
        /// <param name="sectionRegistrationsDto">A section registration item that describes the section and the action to take</param>
        /// <returns>A list of registration messages</returns>
        [Obsolete("Obsolete with version 1.5 of the Api. Use StudentService Register method going forward.")]
        public async Task<Dtos.Student.RegistrationResponse> RegisterSectionsAsync(int degreePlanId, IEnumerable<Dtos.Student.SectionRegistration> sectionRegistrationsDto)
        {
            if (degreePlanId == 0)
            {
                throw new ArgumentException("Invalid degreePlanId", "degreePlanId");
            }
            if (sectionRegistrationsDto == null)
            {
                throw new ArgumentException("Invalid sectionsRegistration", "sectionsRegistration");
            }

            var messages = new List<Dtos.Student.RegistrationMessage>();

            var degreePlan = await _studentDegreePlanRepository.GetAsync(degreePlanId);
            // Prevent action without proper permissions - If user is self continue - otherwise check permissions.
            if (!UserIsSelf(degreePlan.PersonId))
            {
                // Make sure user has permissions to update this degree plan. 
                // If not, an PermissionsException will be thrown.
                await CheckRegisterPermissionsAsync(degreePlan.PersonId);
            }

            var sectionRegistrations = new List<Domain.Student.Entities.SectionRegistration>();
            foreach (var sectionReg in sectionRegistrationsDto)
            {
                sectionRegistrations.Add(new Domain.Student.Entities.SectionRegistration()
                {
                    Action = (Domain.Student.Entities.RegistrationAction)sectionReg.Action,
                    Credits = sectionReg.Credits,
                    SectionId = sectionReg.SectionId
                });
            }

            var request = new Domain.Student.Entities.RegistrationRequest(degreePlan.PersonId, sectionRegistrations);
            //var messageEntities = _studentRepository.Register(request);
            var responseEntity = await _studentRepository.RegisterAsync(request);
            var responseDto = new Dtos.Student.RegistrationResponse();
            responseDto.Messages = new List<Dtos.Student.RegistrationMessage>();
            responseDto.PaymentControlId = responseEntity.PaymentControlId;

            foreach (var message in responseEntity.Messages)
            {
                responseDto.Messages.Add(new Dtos.Student.RegistrationMessage { Message = message.Message, SectionId = message.SectionId });
            }

            return responseDto;
        }

        /// <summary>
        /// Check the permissions on the user to see if they have privileges to make the update to the plan.
        /// </summary>
        /// <param name="student">Student who's plan is being updated - needed to determine assigned advisors</param>
        /// <param name="degreePlanToUpdate">The degree plan update being submitted - needed to determine the type of change being made</param>
        [Obsolete("Obsolete with version 1.7 of the Api. Use await CheckUpdatePermissions2Async method going forward.")]
        public async Task CheckUpdatePermissionsAsync(Domain.Student.Entities.Student student, Domain.Student.Entities.DegreePlans.DegreePlan degreePlanToUpdate)
        {
            // Get user permissions
            IEnumerable<string> userPermissions = await GetUserPermissionCodesAsync();

            //Access is Ok if this is an advisor with full or update access to any student.
            if (userPermissions.Contains(Domain.Student.PlanningPermissionCodes.AllAccessAnyAdvisee) || userPermissions.Contains(Domain.Student.PlanningPermissionCodes.UpdateAnyAdvisee))
            {
                return;
            }

            bool userIsAssignedAdvisor = (await UserIsAssignedAdvisorAsync(student.Id, (student == null ? null : student.ConvertToStudentAccess()))); //TODO

            // Access is Ok if this is an advisor with full or update acesss to assigned advisees AND this is an assigned advisee
            if ((userPermissions.Contains(Domain.Student.PlanningPermissionCodes.AllAccessAssignedAdvisees) && userIsAssignedAdvisor) ||
                (userPermissions.Contains(Domain.Student.PlanningPermissionCodes.UpdateAssignedAdvisees) && userIsAssignedAdvisor))
            {
                return;
            }

            // Access is Ok if this is an advisor with review access to the advisee AND this is an review type of change.
            if (userPermissions.Contains(Domain.Student.PlanningPermissionCodes.ReviewAnyAdvisee) ||
               (userPermissions.Contains(Domain.Student.PlanningPermissionCodes.ReviewAssignedAdvisees) && userIsAssignedAdvisor))
            {
                // Test the plan to find out what type of update is happening. If it is just a ReviewOnly type allow it.
                var currentDegreePlan = await _studentDegreePlanRepository.GetAsync(student.DegreePlanId.GetValueOrDefault(0));
                if (currentDegreePlan.ReviewOnlyChange(degreePlanToUpdate))
                {
                    return;
                }
            }

            // User does not have permissions and error needs to be thrown and logged
            logger.Error("User" + CurrentUser.PersonId + " does not have permissions to update this degree plan");
            throw new PermissionsException();
        }

        /// <summary>
        /// Check the permissions on the user and the changes to the plan to verify whether the given updates are legal for the particular user.
        /// Users with view-only permission have no change privilege except for create.
        /// </summary>
        /// <param name="degreePlanToUpdate">The degree plan update being submitted - needed to determine the type of change being made</param>
        /// <param name="student">Student who's plan is being updated - needed to determine assigned advisors</param>
        public async Task CheckUpdatePermissions2Async(
            Domain.Student.Entities.DegreePlans.DegreePlan degreePlanToUpdate, 
            Domain.Student.Entities.DegreePlans.DegreePlan storedDegreePlan, 
            Domain.Student.Entities.PlanningStudent student = null)
        {
            // Check updates if this is the student in the degree plan. Student cannot make move, remove, or add protected items and cannot make review changes.
            if (UserIsSelf(degreePlanToUpdate.PersonId))
            {
                // Since any user with All/Update access has been returned, make sure remaining user has not made illegal changes.
                if (degreePlanToUpdate.HasProtectedChange(storedDegreePlan))
                {
                    logger.Error("User " + CurrentUser.PersonId + " does not have permissions to update protected items on this degree plan");
                    throw new PermissionsException();
                }
                if (degreePlanToUpdate.HasReviewChange(storedDegreePlan))
                {
                    logger.Error("User " + CurrentUser.PersonId + " does not have permissions to update approvals on this degree plan");
                    throw new PermissionsException();
                }
            }
            else
            {
                // Check updates if this is not the student. 
                //    * Advisors with all access and with update access to this student may make any update change
                //    * Advisors with review access may make review changes only
                //    * Exception thrown for any other (view only) advisors
                var userPermissions = await GetUserPermissionCodesAsync();

                // Any update is Ok if this is an advisor with full or update access to any student.
                if (userPermissions.Contains(Domain.Student.PlanningPermissionCodes.AllAccessAnyAdvisee) || 
                    userPermissions.Contains(Domain.Student.PlanningPermissionCodes.UpdateAnyAdvisee))
                {
                    return;
                }

                // Any update is ok if this is an advisor with full or update access and this is an assigned advisee
                if (userPermissions.Contains(Domain.Student.PlanningPermissionCodes.AllAccessAssignedAdvisees) || 
                    userPermissions.Contains(Domain.Student.PlanningPermissionCodes.UpdateAssignedAdvisees))
                {
                    if (await UserIsAssignedAdvisorAsync(degreePlanToUpdate.PersonId, null/*(student == null ? null : student.ConvertToStudentAccess())*/)) //TODO
                    {
                        return;
                    }
                }

                // If Advisor has review access to the student, verify only a review type of change has been made.
                var hasReviewPermissions = false;
                if (userPermissions.Contains(Domain.Student.PlanningPermissionCodes.ReviewAnyAdvisee))
                {
                    hasReviewPermissions = true;
                }
                if (!hasReviewPermissions && userPermissions.Contains(Domain.Student.PlanningPermissionCodes.ReviewAssignedAdvisees))
                {
                    if (await UserIsAssignedAdvisorAsync(degreePlanToUpdate.PersonId, null /* (student == null ? null : student.ConvertToStudentAccess())*/)) //TODO
                    {
                        hasReviewPermissions = true;
                    }
                }
                if (hasReviewPermissions)
                {
                    // Test the plan to find out what type of update is happening. If it is just a ReviewOnly type allow it.
                    // If more than review-type changes are found, or protection changes are found, exception is thrown.
                    if (storedDegreePlan.ReviewOnlyChange(degreePlanToUpdate) && !degreePlanToUpdate.HasProtectedChange(storedDegreePlan))
                    {
                        return;
                    }
                }
                // Anyone who is left does not have any permissions that allow update of this student's degree plan
                logger.Error("User " + CurrentUser.PersonId + " does not have permissions to update this degree plan");
                throw new PermissionsException();
            }
        }

        /// <summary>
        /// Verify the degree plan
        /// </summary>
        /// <param name="degreePlan"></param>
        /// <returns></returns>
        private async Task<Domain.Student.Entities.DegreePlans.DegreePlan> CheckForConflictsInDegreePlanAsync(Domain.Student.Entities.DegreePlans.DegreePlan degreePlan, IEnumerable<Domain.Student.Entities.AcademicCredit> credits = null)
        {
            if (degreePlan.PlannedCourses.Count > 0)
            {
                // Simplify gathering of course and section data for conflict checking for now
                Stopwatch watch = null;
                if (logger.IsInfoEnabled)
                {
                    watch = new Stopwatch();
                    watch.Start();
                }
                var terms = await _termRepository.GetAsync();
                if (logger.IsInfoEnabled)
                {
                    watch.Stop();
                    logger.Info("DegreePlan Timing: (CheckForConflictsInDegreePlan) Completed await _termRepository.GetAsync in " + watch.ElapsedMilliseconds.ToString() + " ms");

                    watch.Restart();
                }
                var courses = await _courseRepository.GetAsync();
                if (logger.IsInfoEnabled)
                {
                    watch.Stop();
                    logger.Info("DegreePlan Timing: (CheckForConflictsInDegreePlan) Completed _courseRepository.Get in " + watch.ElapsedMilliseconds.ToString() + " ms");

                    watch.Restart();
                }
                var registrationTerms = await _termRepository.GetRegistrationTermsAsync();
                if (logger.IsInfoEnabled)
                {
                    watch.Stop();
                    logger.Info("DegreePlan Timing: (CheckForConflictsInDegreePlan) Completed await _termRepository.GetRegistrationTermsAsync in " + watch.ElapsedMilliseconds.ToString() + " ms");
                }
                var currentTermMinDate = DateTime.Today;

                if (credits == null)
                {
                    if (logger.IsInfoEnabled)
                    {

                        watch.Restart();
                    }
                    var creditsDict = await _academicCreditRepository.GetAcademicCreditByStudentIdsAsync(new List<string>() { degreePlan.PersonId });
                    if (creditsDict.ContainsKey(degreePlan.PersonId))
                    {
                        credits = creditsDict[degreePlan.PersonId];
                    }
                    else
                    {
                        credits = new List<Domain.Student.Entities.AcademicCredit>();
                    }
                    if (logger.IsInfoEnabled)
                    {
                        watch.Stop();
                        logger.Info("DegreePlan Timing: (CheckForConflictsInDegreePlan) Completed _academicCreditRepository.GetAcademicCreditByStudentIds in " + watch.ElapsedMilliseconds.ToString() + " ms");
                    }
                }

                var sections = new List<Domain.Student.Entities.Section>();
                if (registrationTerms != null && registrationTerms.Count() > 0)
                {
                    if (logger.IsInfoEnabled)
                    {

                        watch.Restart();
                    }
                    sections = (await _sectionRepository.GetRegistrationSectionsAsync(registrationTerms)).ToList();
                    if (logger.IsInfoEnabled)
                    {
                        watch.Stop();
                        logger.Info("DegreePlan Timing: (CheckForConflictsInDegreePlan) Completed __sectionRepository.GetRegistrationSections in " + watch.ElapsedMilliseconds.ToString() + " ms");
                    }
                }
                // Get list of terms/preqrequisites from degree plan
                if (logger.IsInfoEnabled)
                {
                    watch.Restart();
                }
                var requirementCodes = degreePlan.GetRequirementCodes(terms, registrationTerms, courses, credits, sections);
                if (logger.IsInfoEnabled)
                {
                    watch.Stop();
                    logger.Info("DegreePlan Timing: (CheckForConflictsInDegreePlan) Completed degreePlan.GetRequirementCodes in " + watch.ElapsedMilliseconds.ToString() + " ms");
                }


                // Get requirements needed for prerequisite validation
                if (logger.IsInfoEnabled)
                {

                    watch.Restart();
                }
                var requirements = await _requirementRepository.GetAsync(requirementCodes);
                if (logger.IsInfoEnabled)
                {
                    watch.Stop();
                    logger.Info("DegreePlan Timing: (CheckForConflictsInDegreePlan) Completed _requirementrepository.Get in " + watch.ElapsedMilliseconds.ToString() + " ms");

                    // Preprocess rules from prerequisite requirements
                    watch.Restart();
                }

                var allRules = new List<RequirementRule>();
                if (requirements != null)
                {
                    requirements.ToList().ForEach(req => allRules.AddRange(req.GetAllRules()));
                }

                var creditRequests = new List<RuleRequest<Domain.Student.Entities.AcademicCredit>>();
                var courseRequests = new List<RuleRequest<Domain.Student.Entities.Course>>();

                // Run all credits against credit rules
                foreach (var credit in credits)
                {
                    foreach (var rule in allRules.Where(rr => rr.CreditRule != null))
                    {
                        creditRequests.Add(new RuleRequest<Domain.Student.Entities.AcademicCredit>(rule.CreditRule, credit));
                    }
                }
                // Run all courses from credits against course rules
                foreach (var creditCourse in credits.Where(stc => stc.Course != null).Select(stc => stc.Course))
                {
                    foreach (var rule in allRules.Where(rr => rr.CourseRule != null))
                    {
                        courseRequests.Add(new RuleRequest<Domain.Student.Entities.Course>(rule.CourseRule, creditCourse));
                    }
                }

                // Run all planned courses against course rules
                var plannedCourseIds = degreePlan.PlannedCourses.Select(pc => pc.CourseId);
                foreach (var course in courses.Where(c => plannedCourseIds.Contains(c.Id)))
                {
                    foreach (var rule in allRules.Where(rr => rr.CourseRule != null))
                    {
                        courseRequests.Add(new RuleRequest<Domain.Student.Entities.Course>(rule.CourseRule, course));
                    }
                }
                if (logger.IsInfoEnabled)
                {
                    watch.Stop();
                    logger.Info("DegreePlan Timing: (CheckForConflictsInDegreePlan) Completed building rule requests in " + watch.ElapsedMilliseconds.ToString() + " ms");

                    watch.Restart();
                }
                // Execute all the rules; it'll skip the ones with .NET expressions
                var creditResults = await _ruleRepository.ExecuteAsync<Domain.Student.Entities.AcademicCredit>(creditRequests);
                var courseResults = await _ruleRepository.ExecuteAsync<Domain.Student.Entities.Course>(courseRequests);
                var ruleResults = creditResults.Union(courseResults);
                if (logger.IsInfoEnabled)
                {
                    watch.Stop();
                    logger.Info("DegreePlan Timing: (CheckForConflictsInDegreePlan) Completed _ruleRepository.Execute in " + watch.ElapsedMilliseconds.ToString() + " ms");

                    watch.Restart();
                }
                string studentPrimaryLocation = null;
                // If any courses have cycle restrictions by location, determine the student's best location - needed for CheckForConflicts in looking at session and yearly cycle restrictions by location
                if (courses != null && courses.Any(c => c.LocationCycleRestrictions != null && c.LocationCycleRestrictions.Any()))
                {
                    studentPrimaryLocation = await GetStudentLocationAsync(degreePlan.PersonId);
                }
                // Call domain method to validate plan and update with related messages
                degreePlan.CheckForConflicts(terms, registrationTerms, courses, sections, credits, requirements, ruleResults, studentPrimaryLocation);
                if (logger.IsInfoEnabled)
                {
                    watch.Stop();
                    logger.Info("DegreePlan Timing: (CheckForConflictsInDegreePlan) Completed degreePlan.CheckForConflicts in " + watch.ElapsedMilliseconds.ToString() + " ms");
                }
            }
            // Return validated degree plan
            return degreePlan;
        }

        /// <summary>
        /// Determines the best location to use for a specific student to use when checking location cycle restrictions 
        /// First check to see if the student has any applicable home locations (based on date ranges)
        /// If no applicable location is found in the student's home locations, take the first location from the student's active programs.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <returns>Location string or null</returns>
        private async Task<string> GetStudentLocationAsync(string studentId)
        {

            var student = await _studentRepository.GetAsync(studentId);
            // Note: the second parameter for GetStudentProgramsByIdsAsync is includeInactivePrograms. Setting that to false will only return active programs.
            var activeStudentPrograms = await _studentProgramRepository.GetStudentProgramsByIdsAsync(new List<string>() { studentId }, false);
            return student != null ? student.GetPrimaryLocation(activeStudentPrograms) : null;
        }
        /// <summary>
        /// This validates is Planning license module exists. 
        /// if User is self then its okay, doesn't need to have planning license
        /// But if user is not self; does not matter what permissions that user have; planning license should exist
        /// </summary>
        /// <param name="studentId"></param>
       
        
    }
}
