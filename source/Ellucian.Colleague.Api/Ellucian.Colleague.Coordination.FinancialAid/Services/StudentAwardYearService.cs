//Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Coordination service for the StudentAwardYear
    /// </summary>
    [RegisterType]
    public class StudentAwardYearService : AwardYearCoordinationService, IStudentAwardYearService
    {
        private readonly IConfigurationRepository configurationRepository;

        /// <summary>
        /// Constructor used by injection-framework. 
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry object</param>
        /// <param name="studentAwardYearRepository">StudentAwardYearRepository object</param>
        /// <param name="officeRepository">FinancialAidReferenceDataRepository object</param>
        /// <param name="currentUserFactory">CurrentUserFactory object</param>
        /// <param name="roleRepository">RoleRepository object</param>
        /// <param name="logger">Logger object</param>
        public StudentAwardYearService(IAdapterRegistry adapterRegistry,
            IStudentAwardYearRepository studentAwardYearRepository,
            IFinancialAidOfficeRepository officeRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(
                adapterRegistry, officeRepository, studentAwardYearRepository, configurationRepository,
                currentUserFactory, roleRepository, logger)
        {
            this.configurationRepository = configurationRepository;
        }

        /// <summary>
        /// Retrieve all of the student's financial aid award years from. The current user can only get
        /// their own award years data.
        /// </summary>
        /// <param name="studentId">The Id of the student for whom to retrieve award year data</param>
        /// <returns>A list of StudentAwardYear DTO objects</returns>
        [Obsolete("Obsolete as of API 1.8. Use GetStudentAwardYears2")]
        public IEnumerable<Dtos.FinancialAid.StudentAwardYear> GetStudentAwardYears(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to access award year information for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var activeStudentAwardYears = Task.Run(async() => await GetActiveStudentAwardYearEntitiesAsync(studentId)).GetAwaiter().GetResult();

            var studentAwardYearDtoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.StudentAwardYear, Dtos.FinancialAid.StudentAwardYear>();

            var studentAwardYearDtoList = new List<Dtos.FinancialAid.StudentAwardYear>();
            if (activeStudentAwardYears == null || activeStudentAwardYears.Count() == 0)
            {
                var message = string.Format("No active studentAwardYears are available");
                logger.Info(message);
                return studentAwardYearDtoList;
            }

            foreach (var awardYearEntity in activeStudentAwardYears)
            {
                var awardYearDto = studentAwardYearDtoAdapter.MapToType(awardYearEntity);
                studentAwardYearDtoList.Add(awardYearDto);
            }

            return studentAwardYearDtoList;
        }

        /// <summary>
        /// Retrieve all of the student's financial aid award years. The current user can only get
        /// their own award years data.
        /// </summary>
        /// <param name="studentId">The Id of the student for whom to retrieve award year data</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active years data only</param>
        /// <returns>A list of StudentAwardYear2 DTO objects</returns>
        public async Task<IEnumerable<Dtos.Student.StudentAwardYear2>> GetStudentAwardYears2Async(string studentId, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to access award year information for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var allStudentAwardYears = await GetStudentAwardYearEntitiesAsync(studentId, getActiveYearsOnly);

            var studentAwardYearDtoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.StudentAwardYear, Dtos.Student.StudentAwardYear2>();

            var studentAwardYearDtoList = new List<Dtos.Student.StudentAwardYear2>();
            if (allStudentAwardYears == null || allStudentAwardYears.Count() == 0)
            {
                logger.Info("No studentAwardYears are available");
                return studentAwardYearDtoList;
            }

            foreach (var awardYearEntity in allStudentAwardYears)
            {
                var awardYearDto = studentAwardYearDtoAdapter.MapToType(awardYearEntity);
                studentAwardYearDtoList.Add(awardYearDto);
            }

            return studentAwardYearDtoList;
        }

        /// <summary>
        /// Retrieve a specific award year from the student record
        /// </summary>
        /// <param name="studentId">studentId</param>
        /// <param name="awardYearCode">award year code</param>
        /// <returns>StudentAwardYearDto</returns>
        [Obsolete("Obsolete as of API 1.8. Use GetStudentAwardYear2")]
        public Dtos.FinancialAid.StudentAwardYear GetStudentAwardYear(string studentId, string awardYearCode)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId cannot be null or empty");
            }

            if (string.IsNullOrEmpty(awardYearCode))
            {
                throw new ArgumentNullException("awardYearCode cannot be null or empty");
            }
                       
            var studentAwardYearDto = GetStudentAwardYears(studentId).FirstOrDefault(say => say.Code == awardYearCode);

            if (studentAwardYearDto == null)
            {
                var message = string.Format("No student award year exists for {0} for student {1}", awardYearCode, studentId);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            return studentAwardYearDto;
        }

        /// <summary>
        /// Retrieve a specific award year from the student record
        /// </summary>
        /// <param name="studentId">studentId</param>
        /// <param name="awardYearCode">award year code</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active years data only</param>
        /// <returns>StudentAwardYear2 Dto</returns>
        public async Task<Dtos.Student.StudentAwardYear2> GetStudentAwardYear2Async(string studentId, string awardYearCode, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId cannot be null or empty");
            }

            if (string.IsNullOrEmpty(awardYearCode))
            {
                throw new ArgumentNullException("awardYearCode cannot be null or empty");
            }

            var studentAwardYearDto = (await GetStudentAwardYears2Async(studentId, getActiveYearsOnly)).FirstOrDefault(say => say.Code == awardYearCode);

            if (studentAwardYearDto == null)
            {
                var message = string.Format("No student award year exists for {0} for student {1}", awardYearCode, studentId);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            return studentAwardYearDto;
        }

        /// <summary>
        /// Update the paper copy option flag
        /// </summary>
        /// <param name="studentAwardYear">student award year containing the info</param>
        /// <returns>student award year dto</returns>
        [Obsolete("Obsolete as of API 1.8. Use UpdateStudentAwardYear2")]
        public Dtos.FinancialAid.StudentAwardYear UpdateStudentAwardYear(Dtos.FinancialAid.StudentAwardYear studentAwardYear)
        {
            if (studentAwardYear == null)
            {
                throw new ArgumentNullException("studentAwardYear");
            }
            if (string.IsNullOrEmpty(studentAwardYear.StudentId))
            {
                var message = "Input argument studentAwardYear is invalid. StudentId cannot be null or empty";
                logger.Error(message);
                throw new ArgumentException(message, "studentAwardYear");
            }
            if (!UserIsSelf(studentAwardYear.StudentId))
            {
                var message = string.Format("{0} does not have permission to update student award years data for {1}", CurrentUser.PersonId, studentAwardYear.StudentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var studentAwardYearDtoAdapter = _adapterRegistry.GetAdapter<Dtos.FinancialAid.StudentAwardYear, Domain.FinancialAid.Entities.StudentAwardYear>();
            var inputStudentAwardYearEntity = studentAwardYearDtoAdapter.MapToType(studentAwardYear);

            var updatedStudentAwardYearEntity = studentAwardYearRepository.UpdateStudentAwardYear(inputStudentAwardYearEntity);

            if (updatedStudentAwardYearEntity == null)
            {
                var message = string.Format("Null student award year object returned by repository update method for student {0} award year {1}", inputStudentAwardYearEntity.StudentId, studentAwardYear.Code);
                logger.Error(message);
                throw new Exception(message);
            }

            var studentAwardYearEntityAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.StudentAwardYear, Dtos.FinancialAid.StudentAwardYear>();
            return studentAwardYearEntityAdapter.MapToType(updatedStudentAwardYearEntity);
        }

        /// <summary>
        /// Update the paper copy option flag
        /// </summary>
        /// <param name="studentAwardYear">student award year containing the info</param>
        /// <returns>StudentAwardYear2 dto</returns>
        public async Task<Dtos.Student.StudentAwardYear2> UpdateStudentAwardYear2Async(Dtos.Student.StudentAwardYear2 studentAwardYear)
        {
            if (studentAwardYear == null)
            {
                throw new ArgumentNullException("studentAwardYear");
            }
            if (string.IsNullOrEmpty(studentAwardYear.StudentId))
            {
                var message = "Input argument studentAwardYear is invalid. StudentId cannot be null or empty";
                logger.Error(message);
                throw new ArgumentException(message, "studentAwardYear");
            }
            if (!UserIsSelf(studentAwardYear.StudentId))
            {
                var message = string.Format("{0} does not have permission to update student award years data for {1}", CurrentUser.PersonId, studentAwardYear.StudentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var studentAwardYearDtoToEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.StudentAwardYear2, Domain.FinancialAid.Entities.StudentAwardYear>();
            var inputStudentAwardYearEntity = studentAwardYearDtoToEntityAdapter.MapToType(studentAwardYear);

            var updatedStudentAwardYearEntity = await studentAwardYearRepository.UpdateStudentAwardYearAsync(inputStudentAwardYearEntity);

            if (updatedStudentAwardYearEntity == null)
            {
                var message = string.Format("Null student award year object returned by repository update method for student {0} award year {1}", inputStudentAwardYearEntity.StudentId, studentAwardYear.Code);
                logger.Error(message);
                throw new Exception(message);
            }

            var studentAwardYearEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.StudentAwardYear, Dtos.Student.StudentAwardYear2>();
            return studentAwardYearEntityToDtoAdapter.MapToType(updatedStudentAwardYearEntity);
        }
    }
}
