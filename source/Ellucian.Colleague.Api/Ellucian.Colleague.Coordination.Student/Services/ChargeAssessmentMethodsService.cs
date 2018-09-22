//Copyright 2018 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class ChargeAssessmentMethodsService : BaseCoordinationService, IChargeAssessmentMethodsService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;

        public ChargeAssessmentMethodsService(

            IStudentReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all charge-assessment-methods
        /// </summary>
        /// <returns>Collection of ChargeAssessmentMethods DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.ChargeAssessmentMethods>> GetChargeAssessmentMethodsAsync(bool bypassCache = false)
        {
            var chargeAssessmentMethodsCollection = new List<Ellucian.Colleague.Dtos.ChargeAssessmentMethods>();

            var chargeAssessmentMethodsEntities = await _referenceDataRepository.GetChargeAssessmentMethodsAsync(bypassCache);
            if (chargeAssessmentMethodsEntities != null && chargeAssessmentMethodsEntities.Any())
            {
                foreach (var chargeAssessmentMethods in chargeAssessmentMethodsEntities)
                {
                    chargeAssessmentMethodsCollection.Add(ConvertChargeAssessmentMethodsEntityToDto(chargeAssessmentMethods));
                }
            }
            return chargeAssessmentMethodsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a ChargeAssessmentMethods from its GUID
        /// </summary>
        /// <returns>ChargeAssessmentMethods DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.ChargeAssessmentMethods> GetChargeAssessmentMethodsByGuidAsync(string guid)
        {
            try
            {
                return ConvertChargeAssessmentMethodsEntityToDto((await _referenceDataRepository.GetChargeAssessmentMethodsAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("charge-assessment-methods not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("charge-assessment-methods not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a ChargeAssessmentMethod domain entity to its corresponding ChargeAssessmentMethods DTO
        /// </summary>
        /// <param name="source">ChargeAssessmentMethod domain entity</param>
        /// <returns>ChargeAssessmentMethods DTO</returns>
        private Ellucian.Colleague.Dtos.ChargeAssessmentMethods ConvertChargeAssessmentMethodsEntityToDto(ChargeAssessmentMethod source)
        {
            var chargeAssessmentMethods = new Ellucian.Colleague.Dtos.ChargeAssessmentMethods();

            chargeAssessmentMethods.Id = source.Guid;
            chargeAssessmentMethods.Code = source.Code;
            chargeAssessmentMethods.Title = source.Description;
            chargeAssessmentMethods.Description = null;           
                                                                        
            return chargeAssessmentMethods;
        }

      
    }
   
}