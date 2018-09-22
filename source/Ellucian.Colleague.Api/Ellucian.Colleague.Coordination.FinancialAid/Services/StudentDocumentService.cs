//Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Coordination.Student.Services;
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
    /// Coordination Service for StudentDocuments
    /// </summary>
    [RegisterType]
    public class StudentDocumentService : FinancialAidCoordinationService, IStudentDocumentService
    {
        private readonly IStudentDocumentRepository studentDocumentRepository;
        private readonly IConfigurationRepository configurationRepository;

        /// <summary>
        /// Constructor used by injection-framework. 
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry object</param>
        /// <param name="studentDocumentRepository">StudentDocumentRepository object</param>
        /// <param name="currentUserFactory">CurrentUserFactory object</param>
        /// <param name="roleRepository">RoleRepository object</param>
        /// <param name="logger">Logger object</param>
        public StudentDocumentService(IAdapterRegistry adapterRegistry,
            IStudentDocumentRepository studentDocumentRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(configurationRepository, adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.studentDocumentRepository = studentDocumentRepository;
            this.configurationRepository = configurationRepository;
        }

        /// <summary>
        /// Get all of a student's documents.
        /// </summary>
        /// <param name="studentId">The Id of the student for whom to get documents</param>
        /// <returns>A list of StudentDocument DTO objects</returns>
        /// <exception cref="ArgumentNullException">Thrown if studentId is null or empty</exception>
        /// <exception cref="PermissionsException">Thrown if Current user is requesting data for a student other than self and the Current User
        /// does not have the correct permission codes</exception>        
        public async Task<IEnumerable<Dtos.FinancialAid.StudentDocument>> GetStudentDocumentsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to access document information for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var studentDocumentEntityList = await studentDocumentRepository.GetDocumentsAsync(studentId);

            var studentDocumentDtoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.StudentDocument, Dtos.FinancialAid.StudentDocument>();

            var studentDocumentDtoList = new List<Dtos.FinancialAid.StudentDocument>();
            if (studentDocumentEntityList == null)
            {
                logger.Info("StudentDocumentRepository returned null from Get(string studentId).");
                return studentDocumentDtoList;
            }

            foreach (var documentEntity in studentDocumentEntityList)
            {
                studentDocumentDtoList.Add(studentDocumentDtoAdapter.MapToType(documentEntity));
            }

            return studentDocumentDtoList;
        }        

    }
}
