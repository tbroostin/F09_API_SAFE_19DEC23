//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.HumanResources.Adapters;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
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

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class LeaveTypesService : BaseCoordinationService, ILeaveTypesService
    {

        private readonly IHumanResourcesReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public LeaveTypesService(

            IHumanResourcesReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
            _configurationRepository = configurationRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all leave-types
        /// </summary>
        /// <returns>Collection of LeaveTypes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.LeaveTypes>> GetLeaveTypesAsync(bool bypassCache = false)
        {
            var leaveTypesCollection = new List<Ellucian.Colleague.Dtos.LeaveTypes>();

            var leaveTypesEntities = await _referenceDataRepository.GetLeaveTypesAsync(bypassCache);
            if (leaveTypesEntities != null && leaveTypesEntities.Any())
            {
                foreach (var leaveTypes in leaveTypesEntities)
                {
                    leaveTypesCollection.Add(ConvertLeaveTypesEntityToDto(leaveTypes));
                }
            }
            return leaveTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a LeaveTypes from its GUID
        /// </summary>
        /// <returns>LeaveTypes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.LeaveTypes> GetLeaveTypesByGuidAsync(string guid)
        {
            try
            {
                return ConvertLeaveTypesEntityToDto((await _referenceDataRepository.GetLeaveTypesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("leave-types not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("leave-types not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a LeaveType domain entity to its corresponding LeaveTypes DTO
        /// </summary>
        /// <param name="source">LeaveType domain entity</param>
        /// <returns>LeaveTypes DTO</returns>
        private Ellucian.Colleague.Dtos.LeaveTypes ConvertLeaveTypesEntityToDto(LeaveType source)
        {
            var leaveTypes = new Ellucian.Colleague.Dtos.LeaveTypes();

            leaveTypes.Id = source.Guid;
            leaveTypes.Code = source.Code;
            leaveTypes.Title = source.Description;
            leaveTypes.Description = null;           
                                                                        
            return leaveTypes;
        }

      
    }
   
}