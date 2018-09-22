//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    [RegisterType]
    public class FreeOnBoardTypesService : BaseCoordinationService, IFreeOnBoardTypesService
    {

        private readonly IColleagueFinanceReferenceDataRepository _referenceDataRepository;

        public FreeOnBoardTypesService(

            IColleagueFinanceReferenceDataRepository referenceDataRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository : configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all free-on-board-types
        /// </summary>
        /// <returns>Collection of FreeOnBoardTypes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.FreeOnBoardTypes>> GetFreeOnBoardTypesAsync(bool bypassCache = false)
        {
            var freeOnBoardTypesCollection = new List<Ellucian.Colleague.Dtos.FreeOnBoardTypes>();

            var freeOnBoardTypesEntities = await _referenceDataRepository.GetFreeOnBoardTypesAsync(bypassCache);
            if (freeOnBoardTypesEntities != null && freeOnBoardTypesEntities.Any())
            {
                foreach (var freeOnBoardTypes in freeOnBoardTypesEntities)
                {
                    freeOnBoardTypesCollection.Add(ConvertFreeOnBoardTypesEntityToDto(freeOnBoardTypes));
                }
            }
            return freeOnBoardTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a FreeOnBoardTypes from its GUID
        /// </summary>
        /// <returns>FreeOnBoardTypes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.FreeOnBoardTypes> GetFreeOnBoardTypesByGuidAsync(string guid)
        {
            try
            {
                return ConvertFreeOnBoardTypesEntityToDto((await _referenceDataRepository.GetFreeOnBoardTypesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("free-on-board-types not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("free-on-board-types not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a FreeOnBoardTypes domain entity to its corresponding FreeOnBoardTypes DTO
        /// </summary>
        /// <param name="source">FreeOnBoardTypes domain entity</param>
        /// <returns>FreeOnBoardTypes DTO</returns>
        private Ellucian.Colleague.Dtos.FreeOnBoardTypes ConvertFreeOnBoardTypesEntityToDto(FreeOnBoardType source)
        {
            var freeOnBoardTypes = new Ellucian.Colleague.Dtos.FreeOnBoardTypes();

            freeOnBoardTypes.Id = source.Guid;
            freeOnBoardTypes.Code = source.Code;
            freeOnBoardTypes.Title = source.Description;
            freeOnBoardTypes.Description = null;           
                                                                        
            return freeOnBoardTypes;
        }

      
    }
  
}