//Copyright 2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// This class coordinates access to and from FinancialAidCredits resources.
    /// </summary>
    [RegisterType]
    public class FinancialAidCreditsService : AwardYearCoordinationService, IFinancialAidCreditsService
    {
        private readonly IFinancialAidCreditsRepository financialAidCreditsRepository;

        /// <summary>
        /// Constructor used by injection framework
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="financialAidOfficeRepository"></param>
        /// <param name="studentAwardYearRepository"></param>
        /// <param name="currentUserFactory">CurrentUserFactory</param>
        /// <param name="roleRepository">RoleRepository</param>
        /// <param name="logger">Logger</param>
        public FinancialAidCreditsService(IAdapterRegistry adapterRegistry,
            IFinancialAidCreditsRepository financialAidCreditsRepository,
            IFinancialAidOfficeRepository financialAidOfficeRepository,
            IStudentAwardYearRepository studentAwardYearRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, financialAidOfficeRepository, studentAwardYearRepository, configurationRepository, currentUserFactory, roleRepository, logger)
        {
            this.financialAidCreditsRepository = financialAidCreditsRepository;
            this.financialAidOfficeRepository = financialAidOfficeRepository;
            this.studentAwardYearRepository = studentAwardYearRepository;
            this.configurationRepository = configurationRepository;
        }

        /// <summary>
        /// Get all FinancialAidCredits for all years the student has data, or only those that are active
        /// </summary>
        /// <param name="studentId">The PERSON id of the student for whom to retrieve data </param>
        /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active years data only, defaults to true</param>
        /// <returns>A list of AwardYearCredits objects representing all of the student's course credits and how they apply to the various Financial Aid credits</returns>
        /// <exception cref="ArgumentNullException">Thrown when studentId argument is empty or null</exception>
        /// <exception cref="PermissionsException">Thrown when the accessing user does not have permission to the studentId provided</exception>
        public async Task<List<Domain.FinancialAid.Entities.AwardYearCredits>> GetFinancialAidCreditsAsync(string studentId, bool getActiveYearsOnly = true)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to get student credits for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            //Retrieve StudentAwardYears objects for either all FA years for the student, or only those that are active
            var studentAwardYears = await GetStudentAwardYearEntitiesAsync(studentId, getActiveYearsOnly);
            if (studentAwardYears == null || studentAwardYears.Count() == 0)
            {
                var message = string.Format("No award years exist for student {0}", studentId);
                logger.Error(message);
                return null;
            }
            try
            {
                return await financialAidCreditsRepository.GetStudentFinancialAidCreditsAsync(studentId, studentAwardYears);
            }
            catch (Exception e)
            {
                logger.Debug(e, e.Message);
                return null;
            }
        }
    }
}
