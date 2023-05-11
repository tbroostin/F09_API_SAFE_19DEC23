/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    [RegisterType]
    public class ProfileApplicationService : AwardYearCoordinationService, IProfileApplicationService
    {
        private readonly IProfileApplicationRepository profileApplicationRepository;
        private readonly IConfigurationRepository configurationRepository;

        /// <summary>
        /// Constructor used by injection-framework
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="profileApplicationRepository">ProfileApplicationRepository</param>
        /// <param name="officeRepository">OfficeRepository</param>
        /// <param name="studentAwardYearRepository">StudentAwardYearRepository</param>
        /// <param name="currentUserFactory">CurrentUserFactory</param>
        /// <param name="roleRepository">RoleRepository</param>
        /// <param name="logger">Logger</param>
        public ProfileApplicationService(
            IAdapterRegistry adapterRegistry,
            IProfileApplicationRepository profileApplicationRepository,
            IFinancialAidOfficeRepository officeRepository,
            IStudentAwardYearRepository studentAwardYearRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, officeRepository, studentAwardYearRepository, configurationRepository, currentUserFactory, roleRepository, logger)
        {
            this.profileApplicationRepository = profileApplicationRepository;
            this.configurationRepository = configurationRepository;
        }


        /// <summary>
        /// Get the ProfileApplications for the given student
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom ProfileApplications are being retrieved.</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active years data only</param>
        /// <returns>A list of ProfileApplication DTOs for the given student id</returns>
        public async Task<IEnumerable<ProfileApplication>> GetProfileApplicationsAsync(string studentId, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to access profile applications for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var studentAwardYears = await GetStudentAwardYearEntitiesAsync(studentId, getActiveYearsOnly);
            if (studentAwardYears == null || studentAwardYears.Count() == 0)
            {
                logger.Debug(string.Format("Student {0} has no award years", studentId));
                return new List<ProfileApplication>();
            }

            var profileApplicationEntities = await profileApplicationRepository.GetProfileApplicationsAsync(studentId, studentAwardYears);
            if (profileApplicationEntities == null || profileApplicationEntities.Count() == 0)
            {
                logger.Debug(string.Format("Student {0} has no profile applications", studentId));
                return new List<ProfileApplication>();
            }

            var profileApplicationDtoAdapter = _adapterRegistry.GetAdapter<Colleague.Domain.FinancialAid.Entities.ProfileApplication, ProfileApplication>();

            return profileApplicationEntities.Select(profileEntity => profileApplicationDtoAdapter.MapToType(profileEntity));
        }
    }
}
