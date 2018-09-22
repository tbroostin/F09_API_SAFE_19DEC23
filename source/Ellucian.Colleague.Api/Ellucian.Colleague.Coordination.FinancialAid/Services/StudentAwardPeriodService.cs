using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Utility.Dependency;
using Ellucian.Utility.Adapters;
using Ellucian.Utility.Security;
using slf4net;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    [RegisterType]
    public class StudentAwardPeriodService : StudentCoordinationService, IStudentAwardPeriodService
    {
        private readonly IStudentAwardPeriodRepository StudentAwardPeriodRepository;
 
        public StudentAwardPeriodService(IAdapterRegistry adapterRegistry, IStudentAwardPeriodRepository studentAwardPeriodRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            StudentAwardPeriodRepository = studentAwardPeriodRepository;
        }
        /// <summary>
        /// Get the StudentAwardPeriod data from the Repository
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <returns>StudentAwardPeriod data from TA.ACYR</returns>
        public IEnumerable<Dtos.FinancialAid.StudentAwardPeriod> Get(string studentId)
        {
            // Get the studentawardperiod entity with a student id
            var studentAwardPeriodCollection = StudentAwardPeriodRepository.Get(studentId);

            // Get the right adapter
            var studentAwardPeriodDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.FinancialAid.Entities.StudentAwardPeriod, Dtos.FinancialAid.StudentAwardPeriod>();

            var studentAwardPeriodDtoCollection = new List<Dtos.FinancialAid.StudentAwardPeriod>();
            foreach (var studentAwardPeriod in studentAwardPeriodCollection)
            {
                studentAwardPeriodDtoCollection.Add(studentAwardPeriodDtoAdapter.MapToType(studentAwardPeriod));
            }

            return studentAwardPeriodDtoCollection;
        }

        /*
        /// <summary>
        /// Accept a StudentAwardPeriod object. This service updates the AwardStatus of the student
        /// award period to the Accepted Status that the institution has specified as the 
        /// "web Accept Status"
        /// </summary>
        /// <param name="studentAwardPeriodDto">StudentAwardPeriod DTO to accept</param>
        /// <returns>A StudentAwardPeriod DTO with an accepted status.</returns>
        public Dtos.FinancialAid.StudentAwardPeriod AcceptStudentAwardPeriod(Dtos.FinancialAid.StudentAwardPeriod studentAwardPeriodDto)
        {
            //Get the adapters
            //DtoAdapter maps Domain -> Dto
            //DomainAdapter maps Dto -> Domain
            var studentAwardPeriodDtoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.StudentAwardPeriod, Dtos.FinancialAid.StudentAwardPeriod>();
            var studentAwardPeriodDomainAdapter = _adapterRegistry.GetAdapter<Dtos.FinancialAid.StudentAwardPeriod, Domain.FinancialAid.Entities.StudentAwardPeriod>();
            
            //Map DTO to Domain
            var studentAwardPeriodDomain = studentAwardPeriodDomainAdapter.MapToType(studentAwardPeriodDto);

            //Update the status
            var updatedStudentAwardPeriodDomain = StudentAwardPeriodRepository.AcceptStudentAwardPeriod(studentAwardPeriodDomain);

            //Map Domain to DTO and return
            return studentAwardPeriodDtoAdapter.MapToType(updatedStudentAwardPeriodDomain);
        }

        /// <summary>
        /// Reject a StudentAwardPeriod object. This service updates the AwardStatus of the student
        /// award period to the Rejected Status that the institution has specified as the 
        /// "web Reject Status"
        /// </summary>
        /// <param name="studentAwardPeriodDto">StudentAwardPeriod DTO to reject</param>
        /// <returns>A StudentAwardPeriod DTO with an rejected status.</returns>
        public Dtos.FinancialAid.StudentAwardPeriod RejectStudentAwardPeriod(Dtos.FinancialAid.StudentAwardPeriod studentAwardPeriodDto)
        {
            //Get the adapters
            //DtoAdapter maps Domain -> Dto
            //DomainAdapter maps Dto -> Domain
            var studentAwardPeriodDtoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.StudentAwardPeriod, Dtos.FinancialAid.StudentAwardPeriod>();
            var studentAwardPeriodDomainAdapter = _adapterRegistry.GetAdapter<Dtos.FinancialAid.StudentAwardPeriod, Domain.FinancialAid.Entities.StudentAwardPeriod>();

            //Map DTO to Domain
            var studentAwardPeriodDomain = studentAwardPeriodDomainAdapter.MapToType(studentAwardPeriodDto);

            //Update the status
            var updatedStudentAwardPeriodDomain = StudentAwardPeriodRepository.RejectStudentAwardPeriod(studentAwardPeriodDomain);

            //Map Domain to DTO and return
            return studentAwardPeriodDtoAdapter.MapToType(updatedStudentAwardPeriodDomain);
        }*/
    }
}
