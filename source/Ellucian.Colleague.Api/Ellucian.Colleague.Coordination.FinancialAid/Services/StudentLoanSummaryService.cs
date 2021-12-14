//Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Coordination service for the StudentLoanSummary
    /// </summary>
    [RegisterType]
    public class StudentLoanSummaryService : FinancialAidCoordinationService, IStudentLoanSummaryService
    {
        private readonly IStudentLoanSummaryRepository studentLoanSummaryRepository;
        private readonly IConfigurationRepository configurationRepository;
        //private readonly IIpedsInstitutionRepository ipedsInstitutionRepository;

        /// <summary>
        /// Constructor used by injection-framework. 
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry object</param>
        /// <param name="studentLoanSummaryRepository">StudentLoanSummaryRepository object</param>
        /// <param name="currentUserFactory">CurrentUserFactory object</param>
        /// <param name="roleRepository">RoleRepository object</param>
        /// <param name="logger">Logger object</param>
        public StudentLoanSummaryService(IAdapterRegistry adapterRegistry,
            IStudentLoanSummaryRepository studentLoanSummaryRepository,
            IConfigurationRepository configurationRepository,
            //IIpedsInstitutionRepository ipedsInstitutionRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(configurationRepository, adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.configurationRepository = configurationRepository;
            this.studentLoanSummaryRepository = studentLoanSummaryRepository;            
        }

        /// <summary>
        /// Gets the given student's StudentLoanSummary object. The current user can only get
        /// their own loan summary data.
        /// </summary>
        /// <param name="studentId">The student's Colleague PERSON id</param>
        /// <returns>A StudentLoanSummary object for the given student</returns>
        /// <exception cref="ArgumentNullException">Thrown when the studentId argument is null or empty</exception>
        /// <exception cref="PermissionsException">Thrown when the current user tries to access loan summary data for a student other than self</exception>
        public async Task<Dtos.FinancialAid.StudentLoanSummary> GetStudentLoanSummaryAsync(string studentId)
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

            var studentLoanSummaryEntity = await studentLoanSummaryRepository.GetStudentLoanSummaryAsync(studentId);
            
            var studentLoanSummaryDtoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.StudentLoanSummary, Dtos.FinancialAid.StudentLoanSummary>();            

            return studentLoanSummaryDtoAdapter.MapToType(studentLoanSummaryEntity);
        }
    }
}
