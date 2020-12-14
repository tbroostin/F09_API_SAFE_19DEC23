// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student.TransferWork;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Coordination service for Transfer Work functionality
    /// </summary>
    [RegisterType]
    public class TransferWorkService : StudentCoordinationService, ITransferWorkService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IStudentTransferWorkRepository _studentTransferWorkRepository;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for Retention Alert Service
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="referenceDataRepository"></param>
        /// <param name="studentRepository"></param>
        /// <param name="configurationRepository"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        public TransferWorkService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, IStudentRepository studentRepository, IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, IStudentTransferWorkRepository studentTransferWorkRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository: studentRepository, configurationRepository: configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            _studentTransferWorkRepository = studentTransferWorkRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get student transfer equivalency work for a student
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <returns>Returns a list of transfer equivalencies for a student.</returns>
        public async Task<IEnumerable<TransferEquivalencies>> GetStudentTransferWorkAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student Id must be specified to get Student transfer summary.");
            }

            var transferEquivalencyDtos = new List<TransferEquivalencies>();
            // Check the person requesting the transfer summary information is the student or advisor
            if (studentId != CurrentUser.PersonId)
            {
                var canAccess = await UserIsAdvisorAsync(studentId);
                if (!canAccess)
                {
                    var message = "Current user is not the student for the requested transfer summary information and therefore cannot access it.";
                    logger.Info(message);
                    throw new PermissionsException(message);
                }
            }
            var transferEquivalencyEntities = await _studentTransferWorkRepository.GetStudentTransferWorkAsync(studentId);
            var adapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.TransferWork.TransferEquivalencies, TransferEquivalencies>();

            if (transferEquivalencyEntities != null && transferEquivalencyEntities.Any())
            {
                foreach (var equivalency in transferEquivalencyEntities)
                {
                    transferEquivalencyDtos.Add(adapter.MapToType(equivalency));
                }
            }

            return transferEquivalencyDtos;
        }
    }
}
