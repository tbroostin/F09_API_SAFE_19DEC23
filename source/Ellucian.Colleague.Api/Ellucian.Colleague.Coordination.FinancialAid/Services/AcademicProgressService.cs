/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// AcademicProgressService class coordinates domain entities to interact with Financial Aid Satisfactory Academic Progress. 
    /// </summary>
    [RegisterType]
    public class AcademicProgressService : FinancialAidCoordinationService, IAcademicProgressService
    {
        private readonly IAcademicProgressRepository academicProgressRepository;
        private readonly IStudentProgramRepository studentProgramRepository;
        private readonly IProgramRequirementsRepository programRequirementsRepository;
        private readonly IAcademicProgressAppealRepository academicProgressAppealRepository;
        private readonly IConfigurationRepository configurationRepository;

        /// <summary>
        /// Constructor for AcademicProgressService
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        /// <param name="academicProgressRepository"></param>
        /// <param name="studentProgramRepository"></param>
        public AcademicProgressService(IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger,
            IAcademicProgressRepository academicProgressRepository,
            IStudentProgramRepository studentProgramRepository,
            IProgramRequirementsRepository programRequirementsRepository,
            IAcademicProgressAppealRepository academicProgressAppealRepository,
            IConfigurationRepository configurationRepository)
            : base(configurationRepository, adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.configurationRepository = configurationRepository;
            this.academicProgressRepository = academicProgressRepository;
            this.studentProgramRepository = studentProgramRepository;
            this.programRequirementsRepository = programRequirementsRepository;
            this.academicProgressAppealRepository = academicProgressAppealRepository;
        }

        /// <summary>
        /// Get a student's AcademicProgressEvaluation objects
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student</param>
        /// <returns>An enumerable of AcademicProgressEvaluation DTOs</returns>
        [Obsolete("Obsolete as of API 1.14. Use GetAcademicProgressEvaluations2Async")]
        public async Task<IEnumerable<AcademicProgressEvaluation>> GetAcademicProgressEvaluationsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            //User must be student or FA Counselor
            if(!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to get academic progress evaluations for student {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var evaluationResultEntities = await academicProgressRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
            if (evaluationResultEntities == null || !evaluationResultEntities.Any())                
            {
                var message = string.Format("No evaluationResults exist for student {0}", studentId);
                logger.Info(message);
                return new List<AcademicProgressEvaluation>();
            }

            var studentPrograms = await studentProgramRepository.GetAsync(studentId);
            if (studentPrograms == null || !studentPrograms.Any())
            {
                var message = string.Format("No StudentPrograms exist for student {0}. This is unexpected", studentId);
                logger.Error(message);
                throw new ApplicationException(message);
            }

            var studentAppeals = await academicProgressAppealRepository.GetStudentAcademicProgressAppealsAsync(studentId);
            
            var academicProgressEvaluationEntities = new List<Domain.FinancialAid.Entities.AcademicProgressEvaluation>();
            foreach (var evaluationResultEntity in evaluationResultEntities)
            {
                try
                {
                    var studentProgram = studentPrograms.FirstOrDefault(p => p.ProgramCode == evaluationResultEntity.AcademicProgramCode);
                    if (studentProgram == null)
                    {
                        logger.Warn("StudentProgram does not exist for programId {0} in AcademicProgressEvaluation {1} for student {2}", evaluationResultEntity.AcademicProgramCode, evaluationResultEntity.Id, studentId);
                        continue;
                    }

                    var programRequirements = await programRequirementsRepository.GetAsync(studentProgram.ProgramCode, studentProgram.CatalogCode);
                    if (programRequirements == null)
                    {
                        logger.Warn("ProgramRequirements do not exist for programId {0} and catalogCode {1} in AcademicProgressEvaluation {2} for student {3}", studentProgram.ProgramCode, studentProgram.CatalogCode, evaluationResultEntity.Id, studentId);
                        continue;
                    }

                    academicProgressEvaluationEntities.Add(new Domain.FinancialAid.Entities.AcademicProgressEvaluation(evaluationResultEntity, programRequirements, studentAppeals));
                }
                catch (Exception e)
                {
                    logger.Error(e, "Unable to create AcademicProgressEvaluation {0} for student {1}", evaluationResultEntity.Id, studentId);
                }
            }

            var evaluationDtoToEntityAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.AcademicProgressEvaluation, AcademicProgressEvaluation>();
            return academicProgressEvaluationEntities.Select(eval => evaluationDtoToEntityAdapter.MapToType(eval)).ToList();
        }

        /// <summary>
        /// Get student's AcademicProgressEvaluation2 objects - instead of full program requirements, these objects include
        /// program code, max and min credits only <see cref="AcademicProgressProgramDetail"/>
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student</param>
        /// <returns>An enumerable of AcademicProgressEvaluation2 DTOs</returns>
        public async Task<IEnumerable<AcademicProgressEvaluation2>> GetAcademicProgressEvaluations2Async(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            //User must be student or FA Counselor
            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to get academic progress evaluations for student {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var evaluationResultEntities = await academicProgressRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId);
            if (evaluationResultEntities == null || !evaluationResultEntities.Any())
            {
                var message = string.Format("No evaluationResults exist for student {0}", studentId);
                logger.Info(message);
                return new List<AcademicProgressEvaluation2>();
            }

            var studentPrograms = await studentProgramRepository.GetAsync(studentId);
            if (studentPrograms == null || !studentPrograms.Any())
            {
                var message = string.Format("No StudentPrograms exist for student {0}. This is unexpected", studentId);
                logger.Error(message);
                throw new ApplicationException(message);
            }

            var studentAppeals = await academicProgressAppealRepository.GetStudentAcademicProgressAppealsAsync(studentId);

            var academicProgressEvaluationEntities = new List<Domain.FinancialAid.Entities.AcademicProgressEvaluation2>();
            foreach (var evaluationResultEntity in evaluationResultEntities)
            {
                try
                {
                    var studentProgram = studentPrograms.FirstOrDefault(p => p.ProgramCode == evaluationResultEntity.AcademicProgramCode);
                    if (studentProgram == null)
                    {
                        logger.Warn("StudentProgram does not exist for programId {0} in AcademicProgressEvaluation {1} for student {2}", evaluationResultEntity.AcademicProgramCode, evaluationResultEntity.Id, studentId);
                        continue;
                    }
                    Domain.FinancialAid.Entities.AcademicProgressProgramDetail programDetail = null;
                    try
                    {
                        programDetail = await academicProgressRepository.GetStudentAcademicProgressProgramDetailAsync(studentProgram.ProgramCode, studentProgram.CatalogCode);
                    }
                    catch(KeyNotFoundException knfe)
                    {
                        logger.Warn("ProgramRequirements do not exist for programId {0} and catalogCode {1} in AcademicProgressEvaluation {2} for student {3}", studentProgram.ProgramCode, studentProgram.CatalogCode, evaluationResultEntity.Id, studentId);                        
                    }

                    academicProgressEvaluationEntities.Add(new Domain.FinancialAid.Entities.AcademicProgressEvaluation2(evaluationResultEntity, programDetail, studentAppeals));
                }
                catch (Exception e)
                {
                    logger.Error(e, "Unable to create AcademicProgressEvaluation {0} for student {1}", evaluationResultEntity.Id, studentId);
                }
            }

            var evaluationEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.AcademicProgressEvaluation2, AcademicProgressEvaluation2>();
            return academicProgressEvaluationEntities.Select(eval => evaluationEntityToDtoAdapter.MapToType(eval)).ToList();
        }
    }
}
