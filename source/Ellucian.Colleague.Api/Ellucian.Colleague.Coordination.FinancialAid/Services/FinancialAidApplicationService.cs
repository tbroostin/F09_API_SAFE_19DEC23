/*Copyright 2014-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Linq;
using System.Collections.Generic;
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
    /// <summary>
    /// Service for Financial Aid Applications
    /// </summary>    
    [RegisterType]
    public class FinancialAidApplicationService : AwardYearCoordinationService, IFinancialAidApplicationService
    {
        private readonly IFafsaRepository fafsaRepository;
        private readonly IProfileApplicationRepository profileApplicationRepository;
        private readonly IConfigurationRepository configurationRepository;

        /// <summary>
        /// Constructor used by injection-framework. 
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry object</param>
        /// <param name="financialAidApplicationRepository">FinancialAidApplicationRepository object</param>
        /// <param name="currentUserFactory">CurrentUserFactory object</param>
        /// <param name="roleRepository">RoleRepository object</param>
        /// <param name="logger">Logger object</param>
        public FinancialAidApplicationService(
            IAdapterRegistry adapterRegistry,
            IFafsaRepository fafsaRepository,
            IProfileApplicationRepository profileApplicationRepository,
            IFinancialAidOfficeRepository officeRepository,
            IStudentAwardYearRepository studentAwardYearRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, officeRepository, studentAwardYearRepository, configurationRepository, currentUserFactory, roleRepository, logger)
        {
            this.fafsaRepository = fafsaRepository;
            this.profileApplicationRepository = profileApplicationRepository;
            this.configurationRepository = configurationRepository;
        }

        /// <summary>
        /// Get financial aid application information for all years that the student has award data.
        /// This method is obsolete as of API version 1.7. Get FinancialAidApplication2s using IFafsaService and IProfileApplicationService
        /// </summary>
        /// <param name="studentId">Student's system id</param>
        /// <returns>List of application information by year</returns>
        [Obsolete("Obsolete as of API version 1.7. Deprecated. Get FinancialAidApplication2s using IFafsaService and IProfileApplicationService")]
        public IEnumerable<FinancialAidApplication> GetFinancialAidApplications(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to access application information for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var studentAwardYears = Task.Run(async() => await GetStudentAwardYearEntitiesAsync(studentId)).GetAwaiter().GetResult();

            var fafsas = Task.Run(async() => await fafsaRepository.GetFafsasAsync(new List<string>() { studentId }, studentAwardYears.Select(y => y.Code))).GetAwaiter().GetResult();
            var profileApplications = Task.Run(async() =>await profileApplicationRepository.GetProfileApplicationsAsync(studentId, studentAwardYears)).GetAwaiter().GetResult();

            //Map the document entity to the document DTO
            var financialAidApplicationDtos = new List<FinancialAidApplication>();
            foreach (var studentAwardYear in studentAwardYears)
            {
                financialAidApplicationDtos.Add(new FinancialAidApplication()
                    {
                        AwardYear = studentAwardYear.Code,
                        StudentId = studentAwardYear.StudentId,
                        IsFafsaComplete = (fafsas != null) ? fafsas.Any(f => f.AwardYear == studentAwardYear.Code) : false,
                        IsProfileComplete = (profileApplications != null) ? profileApplications.Any(p => p.AwardYear == studentAwardYear.Code) : false
                    });
            }

            return financialAidApplicationDtos;
        }
    }
}
