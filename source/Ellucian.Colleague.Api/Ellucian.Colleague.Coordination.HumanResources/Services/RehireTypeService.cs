/* Copyright 2016 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class RehireTypeService : BaseCoordinationService, IRehireTypeService
    {
        private readonly IHumanResourcesReferenceDataRepository _hrReferenceDataRepository;

        public RehireTypeService(

            IHumanResourcesReferenceDataRepository hrReferenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {

            _hrReferenceDataRepository = hrReferenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Gets all rehire types
        /// </summary>
        /// <returns>Collection of RehireTypes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.RehireType>> GetRehireTypesAsync(bool bypassCache = false)
        {
            var rehireTypeCollection = new List<Ellucian.Colleague.Dtos.RehireType>();

            var rehireTypeEntities = await _hrReferenceDataRepository.GetRehireTypesAsync(bypassCache);
            if (rehireTypeEntities != null && rehireTypeEntities.Count() > 0)
            {
                foreach (var rehireType in rehireTypeEntities)
                {
                    rehireTypeCollection.Add(ConvertRehireTypeEntityToDto(rehireType));
                }
            }
            return rehireTypeCollection;
        } 
        
        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Get a rehire type from its GUID
        /// </summary>
        /// <returns>RehireType DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.RehireType> GetRehireTypeByGuidAsync(string guid)
        {
            try
            {
                return ConvertRehireTypeEntityToDto((await _hrReferenceDataRepository.GetRehireTypesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Rehire Type not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a RehireType domain entity to its corresponding RehireType DTO
        /// </summary>
        /// <param name="source">RehireType domain entity</param>
        /// <returns>RehireType DTO</returns>
        private Ellucian.Colleague.Dtos.RehireType ConvertRehireTypeEntityToDto(Ellucian.Colleague.Domain.HumanResources.Entities.RehireType source)
        {
            var rehireType = new Ellucian.Colleague.Dtos.RehireType();

            rehireType.Id = source.Guid;
            rehireType.Code = source.Code;
            rehireType.Title = source.Description;
            rehireType.Description = null;

            return rehireType;
        }
    }
}