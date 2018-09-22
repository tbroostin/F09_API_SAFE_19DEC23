//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class AdmissionApplicationWithdrawalReasonsService : BaseCoordinationService, IAdmissionApplicationWithdrawalReasonsService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly ILogger _logger;
        

        public AdmissionApplicationWithdrawalReasonsService(
            
            IStudentReferenceDataRepository referenceDataRepository,
            ILogger logger,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository) 
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)

        {
            _referenceDataRepository = referenceDataRepository;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Gets all admission-application-withdrawal-reasons
        /// </summary>
        /// <returns>Collection of AdmissionApplicationWithdrawalReasons DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplicationWithdrawalReasons>> GetAdmissionApplicationWithdrawalReasonsAsync(bool bypassCache = false)
        {
            var admissionApplicationWithdrawalReasonsCollection = new List<Ellucian.Colleague.Dtos.AdmissionApplicationWithdrawalReasons>();

            var admissionApplicationWithdrawalReasonsEntities = await _referenceDataRepository.GetWithdrawReasonsAsync(bypassCache);
            if (admissionApplicationWithdrawalReasonsEntities != null && admissionApplicationWithdrawalReasonsEntities.Any())
            {
                foreach (var admissionApplicationWithdrawalReasons in admissionApplicationWithdrawalReasonsEntities)
                {
                    admissionApplicationWithdrawalReasonsCollection.Add(ConvertWithdrawReasonEntityToDto(admissionApplicationWithdrawalReasons));
                }
            }
            return admissionApplicationWithdrawalReasonsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Get a AdmissionApplicationWithdrawalReasons from its GUID
        /// </summary>
        /// <returns>AdmissionApplicationWithdrawalReasons DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AdmissionApplicationWithdrawalReasons> GetAdmissionApplicationWithdrawalReasonsByGuidAsync(string guid)
        {
            try
            {
                return ConvertWithdrawReasonEntityToDto((await _referenceDataRepository.GetWithdrawReasonsAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("admission-application-withdrawal-reasons not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("admission-application-withdrawal-reasons not found for GUID " + guid, ex);
            }
        }

        /// <summary>
        /// Converts a WithdrawReason domain entity to its corresponding AdmissionApplicationWithdrawalReasons DTO
        /// </summary>
        /// <param name="source">WithdrawReason domain entity</param>
        /// <returns>AdmissionApplicationWithdrawalReasons DTO</returns>
        private Ellucian.Colleague.Dtos.AdmissionApplicationWithdrawalReasons ConvertWithdrawReasonEntityToDto(WithdrawReason source)
        {
            var admissionApplicationWithdrawalReasons = new Ellucian.Colleague.Dtos.AdmissionApplicationWithdrawalReasons();

            admissionApplicationWithdrawalReasons.Id = source.Guid;
            admissionApplicationWithdrawalReasons.Code = source.Code;
            admissionApplicationWithdrawalReasons.Title = source.Description;
            admissionApplicationWithdrawalReasons.Description = null;
            return admissionApplicationWithdrawalReasons;
        }
    }
}