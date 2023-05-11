//Copyright 2014-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System.Diagnostics;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Coordination service for StudentLoanLimitations
    /// </summary>
    [RegisterType]
    public class StudentLoanLimitationService : AwardYearCoordinationService, IStudentLoanLimitationService
    {
        private readonly IStudentLoanLimitationRepository studentLoanLimitationRepository;
        private readonly IConfigurationRepository configurationRepository;

        /// <summary>
        /// Constructor used by injection-framework. 
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry object</param>
        /// <param name="studentLoanLimitationRepository">StudentLoanLimitationRepository object</param>
        /// <param name="currentUserFactory">CurrentUserFactory object</param>
        /// <param name="roleRepository">RoleRepository object</param>
        /// <param name="logger">Logger object</param>
        public StudentLoanLimitationService(IAdapterRegistry adapterRegistry,
                                         IStudentLoanLimitationRepository studentLoanLimitationRepository,
                                         IStudentAwardYearRepository studentAwardYearRepository,
                                         IFinancialAidOfficeRepository officeRepository,
                                         IConfigurationRepository configurationRepository,
                                         ICurrentUserFactory currentUserFactory,
                                         IRoleRepository roleRepository,
                                         ILogger logger)
            : base(adapterRegistry, officeRepository, studentAwardYearRepository, configurationRepository, currentUserFactory, roleRepository, logger)
        {
            this.studentLoanLimitationRepository = studentLoanLimitationRepository;
            this.configurationRepository = configurationRepository;

        }

        /// <summary>
        /// This gets all of the student's loan limitation objects from Colleague for all
        /// years that the student has award data.  The current user can only get
        /// their own loan limitation data.
        /// </summary>
        /// <param name="studentId">The id of the student for whom to retrieve the loan limitations</param>
        /// <returns>A list of StudentLoanLimitation DTO objects</returns>
        public async Task<IEnumerable<Dtos.FinancialAid.StudentLoanLimitation>> GetStudentLoanLimitationsAsync(string studentId)
        {
            Stopwatch sw = new Stopwatch();
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to access loan summary information for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            sw.Start();
            var activeStudentAwardYears = await GetActiveStudentAwardYearEntitiesAsync(studentId);
            sw.Stop();
            logger.Info(string.Format("Time elapsed to GetActiveStudentAwardYearEntities (service): {0}", sw.ElapsedMilliseconds));

            sw.Restart();
            var limitationEntityCollection = await studentLoanLimitationRepository.GetStudentLoanLimitationsAsync(studentId, activeStudentAwardYears);
            sw.Stop();
            logger.Info(string.Format("Time elapsed to GetStudentLoanLimitations (service): {0}", sw.ElapsedMilliseconds));

            var limitationDtoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.StudentLoanLimitation, Dtos.FinancialAid.StudentLoanLimitation>();

            var limitationDtoCollection = new List<Dtos.FinancialAid.StudentLoanLimitation>();
            if (limitationEntityCollection == null || !limitationEntityCollection.Any())
            {
                logger.Debug(string.Format("No LoanLimitations exist for student {0}", studentId));
                return limitationDtoCollection;
            }

            foreach (var limitEntity in limitationEntityCollection)
            {
                limitationDtoCollection.Add(limitationDtoAdapter.MapToType(limitEntity));
            }

            return limitationDtoCollection;
        }

        /// <summary>
        /// Get student loan limitation for the given student and year - not used
        /// </summary>
        /// <param name="studentId">student id for whom to get the loan limitation</param>
        /// <param name="year">award year for which to get the loan limitation</param>
        /// <returns>StudentLoanLimitation DTO</returns>
        public async Task<Dtos.FinancialAid.StudentLoanLimitation> GetLimitationAsync(string studentId, string year)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to access loan summary information for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            if (string.IsNullOrEmpty(year))
            {
                throw new ArgumentNullException("year");
            }

            //var currentOfficeService = new CurrentOfficeService(officeRepository.GetFinancialAidOffices());
            //var studentAwardYearEntities = studentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);
            var activeStudentAwardYear = (await GetActiveStudentAwardYearEntitiesAsync(studentId)).FirstOrDefault(y => y.Code == year);
            if (activeStudentAwardYear == null)
            {
                logger.Info("The year requested is not available");
                throw new InvalidOperationException("The year requested is not available");
            }

            //var limitationEntity = studentLoanLimitationRepository.GetLimitation(studentId, year);
            var limitationEntity = (await studentLoanLimitationRepository.GetStudentLoanLimitationsAsync(studentId, new List<StudentAwardYear>() { activeStudentAwardYear })).FirstOrDefault();
            if (limitationEntity == null)
            {
                logger.Debug("StudentLoanLimitationRepository returned null from GetAwardLetters(string studentId) for student {0} and year {1}.", studentId, year);
                throw new KeyNotFoundException(string.Format("year {0} is not a valid award year for student {1}", year, studentId));
            }

            var limitationDtoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.StudentLoanLimitation, Dtos.FinancialAid.StudentLoanLimitation>();

            return limitationDtoAdapter.MapToType(limitationEntity);
        }
    }
}
