//Copyright 2015 Ellucian Company L.P. and its affiliates
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Coordination service for the StudentDefaultAwardPeriod
    /// </summary>
    [RegisterType]
    public class StudentDefaultAwardPeriodService : AwardYearCoordinationService, IStudentDefaultAwardPeriodService
    {
        //private readonly IStudentAwardYearRepository studentAwardYearRepository;
        private readonly IStudentDefaultAwardPeriodRepository studentDefaultAwardPeriodRepository;
        private readonly IConfigurationRepository configurationRepository;

        /// <summary>
        /// Constructor used by injection-framework. 
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry object</param>
        /// <param name="studentAwardYearRepository">StudentAwardYearRepository object</param>
        /// <param name="studentDefaultAwardPeriodRepository">StudentDefaultAwardPeriodRepository object</param>
        /// <param name="currentUserFactory">CurrentUserFactory object</param>
        /// <param name="roleRepository">RoleRepository object</param>
        /// <param name="logger">Logger object</param>
        public StudentDefaultAwardPeriodService(IAdapterRegistry adapterRegistry,
                                         IStudentAwardYearRepository studentAwardYearRepository,
                                         IFinancialAidOfficeRepository officeRepository,
                                         IStudentDefaultAwardPeriodRepository studentDefaultAwardPeriodRepository,
                                         IConfigurationRepository configurationRepository,
                                         ICurrentUserFactory currentUserFactory,
                                         IRoleRepository roleRepository,
                                         ILogger logger)
            : base(adapterRegistry, officeRepository, studentAwardYearRepository, configurationRepository, currentUserFactory, roleRepository, logger)
        {
            this.studentDefaultAwardPeriodRepository = studentDefaultAwardPeriodRepository;
            this.configurationRepository = configurationRepository;
        }
        
        /// <summary>
        /// Gets student default award periods for all active student award years
        /// </summary>
        /// <param name="studentId">student id for whom to retrieve default periods data</param>
        /// <returns>List of StudentDefaultAwardPeriod DTOs</returns>
        public async Task<IEnumerable<StudentDefaultAwardPeriod>> GetStudentDefaultAwardPeriodsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to access award period information for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
            
            var studentAwardYears = await GetActiveStudentAwardYearEntitiesAsync(studentId);
            if (studentAwardYears == null || !studentAwardYears.Any())
            {
                var message = string.Format("Student {0} has no financial aid data or no award years are active in the configuration.", studentId);
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            var defaultAwardPeriodEntities = await studentDefaultAwardPeriodRepository.GetStudentDefaultAwardPeriodsAsync(studentId, studentAwardYears);

            if (defaultAwardPeriodEntities == null || !defaultAwardPeriodEntities.Any())
            {
                var message = string.Format("Student {0} has no default award periods", studentId);
                logger.Debug(message);
                throw new ColleagueSessionExpiredException("Session has expired, please login again.");
                //throw new InvalidOperationException(message);
            }
            
            var awardPeriodEntityAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.FinancialAid.Entities.StudentDefaultAwardPeriod,StudentDefaultAwardPeriod>();
            var defaultAwardPeriodDtoList = new List<Dtos.FinancialAid.StudentDefaultAwardPeriod>();
            foreach (var awardPeriodEntity in defaultAwardPeriodEntities)
            {
                defaultAwardPeriodDtoList.Add(awardPeriodEntityAdapter.MapToType(awardPeriodEntity));
            }

            return defaultAwardPeriodDtoList;
        }
    }
}
