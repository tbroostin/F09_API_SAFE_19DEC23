//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class BillingOverrideReasonsService : StudentCoordinationService, IBillingOverrideReasonsService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public BillingOverrideReasonsService(

            IStudentReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger,
            IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, null, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all billing-override-reasons
        /// </summary>
        /// <returns>Collection of BillingOverrideReasons DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.BillingOverrideReasons>> GetBillingOverrideReasonsAsync(bool bypassCache = false)
        {
            var billingOverrideReasonsCollection = new List<Ellucian.Colleague.Dtos.BillingOverrideReasons>();

            var billingOverrideReasonsEntities = await _referenceDataRepository.GetBillingOverrideReasonsAsync(bypassCache);
            if (billingOverrideReasonsEntities != null && billingOverrideReasonsEntities.Any())
            {
                foreach (var billingOverrideReasons in billingOverrideReasonsEntities)
                {
                    billingOverrideReasonsCollection.Add(ConvertBillingOverrideReasonsEntityToDto(billingOverrideReasons));
                }
            }
            return billingOverrideReasonsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a BillingOverrideReasons from its GUID
        /// </summary>
        /// <returns>BillingOverrideReasons DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.BillingOverrideReasons> GetBillingOverrideReasonsByGuidAsync(string guid)
        {
            try
            {
                return ConvertBillingOverrideReasonsEntityToDto((await _referenceDataRepository.GetBillingOverrideReasonsAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("billing-override-reasons not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("billing-override-reasons not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a BillingOverrideReasons domain entity to its corresponding BillingOverrideReasons DTO
        /// </summary>
        /// <param name="source">BillingOverrideReasons domain entity</param>
        /// <returns>BillingOverrideReasons DTO</returns>
        private Ellucian.Colleague.Dtos.BillingOverrideReasons ConvertBillingOverrideReasonsEntityToDto(Ellucian.Colleague.Domain.Student.Entities.BillingOverrideReasons source)
        {
            var billingOverrideReasons = new Ellucian.Colleague.Dtos.BillingOverrideReasons();

            billingOverrideReasons.Id = source.Guid;
            billingOverrideReasons.Code = source.Code;
            billingOverrideReasons.Title = source.Description;
            billingOverrideReasons.Description = null;           
                                                                        
            return billingOverrideReasons;
        }      
    }   
}