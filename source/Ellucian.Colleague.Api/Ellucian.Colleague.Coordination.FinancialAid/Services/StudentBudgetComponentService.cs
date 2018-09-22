/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// StudentBudgetComponentService gets StudentBudgetComponent entities and returns StudentBudgetComponent DTOs
    /// </summary>
    public class StudentBudgetComponentService : AwardYearCoordinationService, IStudentBudgetComponentService
    {
        private readonly IStudentBudgetComponentRepository studentBudgetComponentRepository;
        private readonly IConfigurationRepository configurationRepository;

        /// <summary>
        /// Constructor for StudentBudgetComponentService
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="studentBudgetComponentRepository">StudentBudgetComponentRepository</param>
        /// <param name="studentAwardYearRepository">StudentAwardYearRepository</param>
        /// <param name="financialAidOfficeRepository">FinancialAidOfficeRepository</param>
        /// <param name="currentUserFactory">CurrentUserFactory</param>
        /// <param name="roleRepository">RoleRepository</param>
        /// <param name="logger">Logger</param>
        public StudentBudgetComponentService(
            IAdapterRegistry adapterRegistry,
            IStudentBudgetComponentRepository studentBudgetComponentRepository,
            IStudentAwardYearRepository studentAwardYearRepository,
            IFinancialAidOfficeRepository financialAidOfficeRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, financialAidOfficeRepository, studentAwardYearRepository, configurationRepository, currentUserFactory, roleRepository, logger)
        {
            this.studentBudgetComponentRepository = studentBudgetComponentRepository;
            this.configurationRepository = configurationRepository;
        }

        /// <summary>
        /// Get all StudentBudgetComponents for the given studentId for all award years
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get budget components</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active years data only</param>
        /// <returns>A list of StudentBudgetComponents</returns>
        public async Task<IEnumerable<StudentBudgetComponent>> GetStudentBudgetComponentsAsync(string studentId, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("User {0} does not have permission to get StudentBudgetComponents for student {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var studentAwardYearEntities = await GetStudentAwardYearEntitiesAsync(studentId, getActiveYearsOnly);
            if (studentAwardYearEntities == null || studentAwardYearEntities.Count() == 0)
            {
                logger.Info(string.Format("Student {0} has no award years for which to get budget components", studentId));
                return new List<StudentBudgetComponent>();
            }

            var studentBudgetComponentEntities = await studentBudgetComponentRepository.GetStudentBudgetComponentsAsync(studentId, studentAwardYearEntities);
            if (studentBudgetComponentEntities == null || !studentBudgetComponentEntities.Any())
            {
                logger.Info(string.Format("Student {0} has no budget components for any award years", studentId));
                return new List<StudentBudgetComponent>();
            }

            var studentBudgetComponentDtoAdapter = _adapterRegistry.GetAdapter<Colleague.Domain.FinancialAid.Entities.StudentBudgetComponent, StudentBudgetComponent>();
            return studentBudgetComponentEntities.Select(budget => studentBudgetComponentDtoAdapter.MapToType(budget));
        }
    }
}
