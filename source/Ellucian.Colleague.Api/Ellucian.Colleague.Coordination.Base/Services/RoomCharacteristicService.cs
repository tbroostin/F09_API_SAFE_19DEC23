// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class RoomCharacteristicService : BaseCoordinationService, IRoomCharacteristicService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IPersonRepository _personRepository;

        private const string _dataOrigin = "Colleague";

        public RoomCharacteristicService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository,
                                         ICurrentUserFactory currentUserFactory, IConfigurationRepository configurationRepository,
                                         IRoleRepository roleRepository,
                                         ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, null, configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Gets all room characteristics
        /// </summary>
        /// <param name="bypassCache">bypassCache</param>
        /// <returns>IEnumerable<Dtos.RoomCharacteristic></returns>
        public async Task<IEnumerable<Dtos.RoomCharacteristic>> GetRoomCharacteristicsAsync(bool bypassCache = false)
        {
            var roomCharacteristicEntities = await _referenceDataRepository.GetRoomCharacteristicsAsync(bypassCache);

            List<Dtos.RoomCharacteristic> roomCharacteristicDtos = new List<Dtos.RoomCharacteristic>(); ;

            foreach (var roomCharacteristicEntity in roomCharacteristicEntities)
            {
                var roomCharacteristicDto = ConvertEntityToDto(roomCharacteristicEntity);
                roomCharacteristicDtos.Add(roomCharacteristicDto);
            }
            return roomCharacteristicDtos.Any()? roomCharacteristicDtos : null;
        }

        /// <summary>
        /// Gets a room characteristic by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Dtos.RoomCharacteristic</returns>
        public async Task<Dtos.RoomCharacteristic> GetRoomCharacteristicByGuidAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("Must provide a room characteristic id.");
            }
            var roomCharacteristicEntity = (await _referenceDataRepository.GetRoomCharacteristicsAsync(true)).FirstOrDefault(i => i.Guid.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (roomCharacteristicEntity == null)
            {
                throw new KeyNotFoundException("Room characteristic was not found with id: " + id);
            }
            var roomCharacteristicDto = ConvertEntityToDto(roomCharacteristicEntity);

            return roomCharacteristicDto;
        }

        /// <summary>
        /// Converts room characteristic entity to dto
        /// </summary>
        /// <param name="roomCharacteristicEntity"></param>
        /// <returns>Dtos.RoomCharacteristic</returns>
        private Dtos.RoomCharacteristic ConvertEntityToDto(RoomCharacteristic roomCharacteristicEntity)
        {
            Dtos.RoomCharacteristic roomCharacteristicDto = new Dtos.RoomCharacteristic() 
            {
                Code = roomCharacteristicEntity.Code,
                Description = null,
                Id = roomCharacteristicEntity.Guid,
                Title = roomCharacteristicEntity.Description
            };
            return roomCharacteristicDto;
        }
    }
}