/*Copyright 2014-2017 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Web.Dependency;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Provides coordination services to get StudentAwardYears.
    /// </summary>
    [RegisterType]
    public abstract class AwardYearCoordinationService : FinancialAidCoordinationService
    {
        protected IStudentAwardYearRepository studentAwardYearRepository;
        protected IFinancialAidOfficeRepository financialAidOfficeRepository;
        protected IConfigurationRepository configurationRepository;

        /// <summary>
        /// Instantiate the AwardYearCoordinationService
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="financialAidOfficeRepository">FinancialAidOfficeRepository</param>
        /// <param name="studentAwardYearRepository">StudentAwardYearRepository</param>
        /// <param name="currentUserFactory">CurrentUserFactory</param>
        /// <param name="roleRepository">RoleRepository</param>
        /// <param name="logger">Logger</param>
        protected AwardYearCoordinationService(IAdapterRegistry adapterRegistry,
            IFinancialAidOfficeRepository financialAidOfficeRepository,
            IStudentAwardYearRepository studentAwardYearRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(configurationRepository, adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.studentAwardYearRepository = studentAwardYearRepository;
            this.financialAidOfficeRepository = financialAidOfficeRepository;
            this.configurationRepository = configurationRepository;
        }       

        /// <summary>
        /// Get a single award year with the specified award year code
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <param name="awardYearCode">award year code</param>
        /// <returns>StudentAwardYear entity</returns>
        protected async Task<Domain.FinancialAid.Entities.StudentAwardYear> GetStudentAwardYearEntityAsync(string studentId, string awardYearCode)
        {
            var offices = await financialAidOfficeRepository.GetFinancialAidOfficesAsync();
            var currentOfficeService = new CurrentOfficeService(offices);            
            return await studentAwardYearRepository.GetStudentAwardYearAsync(studentId, awardYearCode, currentOfficeService);
        }

        /// <summary>
        /// Filter active StudentAwardYear records by office parameters.
        /// </summary>
        /// <param name="studentId">The studentId for whom to get StudentAwardYear entities</param>
        /// <returns>A list of active StudentAwardYear entities</returns>
        protected async Task<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>> GetActiveStudentAwardYearEntitiesAsync(string studentId)
        {
            var studentAwardYears = await GetStudentAwardYearEntitiesAsync(studentId, true);
            if (studentAwardYears == null) { return null; }

            return ApplyConfigurationService.FilterStudentAwardYears(studentAwardYears);
        }

        /// <summary>
        /// Gets all student award years
        /// </summary>
        /// <param name="studentId">student id for whom to get award years</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active years data only</param>
        /// <returns>list of StudentAwardYear objects</returns>
        protected async Task<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>> GetStudentAwardYearEntitiesAsync(string studentId, bool getActiveYearsOnly = false)
        {
            var currentOfficeService = new CurrentOfficeService(await financialAidOfficeRepository.GetFinancialAidOfficesAsync());
            return await studentAwardYearRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService, getActiveYearsOnly);
        }
    }
}
