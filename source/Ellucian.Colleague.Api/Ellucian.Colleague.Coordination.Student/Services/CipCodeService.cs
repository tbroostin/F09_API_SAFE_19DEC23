//Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class CipCodeService : BaseCoordinationService, ICipCodeService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;

        public CipCodeService(

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
        /// Gets all cip-codes
        /// </summary>
        /// <returns>Collection of CipCodes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CipCode>> GetCipCodesAsync(bool bypassCache = false)
        {
            var cipCodesCollection = new List<Ellucian.Colleague.Dtos.CipCode>();

            var cipCodesEntities = await _referenceDataRepository.GetCipCodesAsync(bypassCache);
            if (cipCodesEntities != null && cipCodesEntities.Any())
            {
                foreach (var cipCodes in cipCodesEntities)
                {
                    cipCodesCollection.Add(ConvertCipCodeEntityToDto(cipCodes));
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            return cipCodesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a CipCodes from its GUID
        /// </summary>
        /// <returns>CipCodes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.CipCode> GetCipCodeByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertCipCodeEntityToDto((await _referenceDataRepository.GetCipCodesAsync(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No cip-codes was found for guid '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No cip-codes was found for guid '{0}'", guid), ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a CipCode domain entity to its corresponding CipCode DTO
        /// </summary>
        /// <param name="source">CipCode domain entity</param>
        /// <returns>CipCode DTO</returns>
        private Dtos.CipCode ConvertCipCodeEntityToDto(CipCode source)
        {
            var cipCodes = new Dtos.CipCode();

            cipCodes.Id = source.Guid;
            cipCodes.Code = source.Code;
            cipCodes.Title = source.Description;
            cipCodes.Description = null;
            cipCodes.RevisionYear = source.RevisionYear;
                                                                        
            return cipCodes;
        }
 
    }
   
}