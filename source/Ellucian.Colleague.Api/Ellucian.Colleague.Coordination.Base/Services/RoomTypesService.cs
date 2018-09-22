// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.Base.Adapters;
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
    public class RoomTypesService : BaseCoordinationService, IRoomTypesService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository configurationRepository;
        private const string _dataOrigin = "Colleague";

        public RoomTypesService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository,
                                         IConfigurationRepository configurationRepository,
                                         ICurrentUserFactory currentUserFactory,
                                         IRoleRepository roleRepository,
                                         ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository : configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            this.configurationRepository = configurationRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets all Room Types
        /// </summary>
        /// <returns>Collection of Room Types DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.RoomTypes>> GetRoomTypesAsync(bool bypassCache = false)
        {
            var roomTypesCollection = new List<Ellucian.Colleague.Dtos.RoomTypes>();

            var roomTypesEntities = await _referenceDataRepository.GetRoomTypesAsync(bypassCache);
            if (roomTypesEntities != null && roomTypesEntities.Count() > 0)
            {
                foreach (var roomTypes in roomTypesEntities)
                {
                    roomTypesCollection.Add(ConvertRoomTypesEntityToDto(roomTypes));
                }
            }
            return roomTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Get an Room Types from its GUID
        /// </summary>
        /// <returns>RoomTypes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.RoomTypes> GetRoomTypesByGuidAsync(string id)
        {
            try
            {
                return ConvertRoomTypesEntityToDto((await _referenceDataRepository.GetRoomTypesAsync(true)).Where(al => al.Guid == id).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("Room Types not found for ID " + id, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Room Types not found for ID " + id, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Room Types not found for ID " + id, ex);
            }
        }
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts an RoomTypes domain entity to its corresponding RoomTypes DTO
        /// </summary>
        /// <param name="source">RoomTypes domain entity</param>
        /// <returns>RoomTypes DTO</returns>
        private Ellucian.Colleague.Dtos.RoomTypes ConvertRoomTypesEntityToDto(RoomTypes source)
        {
            var roomTypes = new Ellucian.Colleague.Dtos.RoomTypes();

            roomTypes.Id = source.Guid;
            roomTypes.Code = source.Code;
            roomTypes.Title = source.Description;
            roomTypes.Description = null;
            roomTypes.Type = ConvertRoomTypeDomainEnumToRoomLayoutTypeDtoEnum(source.Type);
            return roomTypes;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a RoomType domain enumeration value to its corresponding RoomLayoutType DTO enumeration value
        /// </summary>
        /// <param name="source">RoomType domain enumeration value</param>
        /// <returns>RoomType DTO enumeration value</returns>
        private Dtos.RoomTypeTypes ConvertRoomTypeDomainEnumToRoomLayoutTypeDtoEnum(RoomType? source)
        {
            if (source == null)
                return Dtos.RoomTypeTypes.Other;

            switch (source)
            {
                case Domain.Base.Entities.RoomType.Amphitheater:
                    return Dtos.RoomTypeTypes.Amphitheater;
                case Domain.Base.Entities.RoomType.Animalquarters:
                    return Dtos.RoomTypeTypes.Animalquarters;
                case Domain.Base.Entities.RoomType.Apartment:
                    return Dtos.RoomTypeTypes.Apartment;
                case Domain.Base.Entities.RoomType.Artstudio:
                    return Dtos.RoomTypeTypes.Artstudio;
                case Domain.Base.Entities.RoomType.Atrium:
                    return Dtos.RoomTypeTypes.Atrium;
                case Domain.Base.Entities.RoomType.Audiovisuallab:
                    return Dtos.RoomTypeTypes.Audiovisuallab;
                case Domain.Base.Entities.RoomType.Auditorium:
                    return Dtos.RoomTypeTypes.Auditorium;
                case Domain.Base.Entities.RoomType.Ballroom:
                    return Dtos.RoomTypeTypes.Ballroom;
                case Domain.Base.Entities.RoomType.Booth:
                    return Dtos.RoomTypeTypes.Booth;
                case Domain.Base.Entities.RoomType.Classroom:
                    return Dtos.RoomTypeTypes.Classroom;
                case Domain.Base.Entities.RoomType.Clinic:
                    return Dtos.RoomTypeTypes.Clinic;
                case Domain.Base.Entities.RoomType.Computerlaboratory:
                    return Dtos.RoomTypeTypes.Computerlaboratory;
                case Domain.Base.Entities.RoomType.Conferenceroom:
                    return Dtos.RoomTypeTypes.Conferenceroom;
                case Domain.Base.Entities.RoomType.Daycare:
                    return Dtos.RoomTypeTypes.Daycare;
                case Domain.Base.Entities.RoomType.Foodfacility:
                    return Dtos.RoomTypeTypes.Foodfacility;
                case Domain.Base.Entities.RoomType.Generalusefacility:
                    return Dtos.RoomTypeTypes.Generalusefacility;
                case Domain.Base.Entities.RoomType.Greenhouse:
                    return Dtos.RoomTypeTypes.Greenhouse;
                case Domain.Base.Entities.RoomType.Healthcarefacility:
                    return Dtos.RoomTypeTypes.Healthcarefacility;
                case Domain.Base.Entities.RoomType.House:
                    return Dtos.RoomTypeTypes.House;
                case Domain.Base.Entities.RoomType.Lecturehall:
                    return Dtos.RoomTypeTypes.Lecturehall;
                case Domain.Base.Entities.RoomType.Lounge:
                    return Dtos.RoomTypeTypes.Lounge;
                case Domain.Base.Entities.RoomType.Mechanicslab:
                    return Dtos.RoomTypeTypes.Mechanicslab;
                case Domain.Base.Entities.RoomType.Merchandisingroom:
                    return Dtos.RoomTypeTypes.Merchandisingroom;
                case Domain.Base.Entities.RoomType.Musicroom:
                    return Dtos.RoomTypeTypes.Musicroom;
                case Domain.Base.Entities.RoomType.Office:
                    return Dtos.RoomTypeTypes.Office;
                case Domain.Base.Entities.RoomType.Other:
                    return Dtos.RoomTypeTypes.Other;
                case Domain.Base.Entities.RoomType.Performingartsstudio:
                    return Dtos.RoomTypeTypes.Performingartsstudio;
                case Domain.Base.Entities.RoomType.Residencehallroom:
                    return Dtos.RoomTypeTypes.Residencehallroom;
                case Domain.Base.Entities.RoomType.Residentialdoubleroom:
                    return Dtos.RoomTypeTypes.Residentialdoubleroom;
                case Domain.Base.Entities.RoomType.Residentialsingleroom:
                    return Dtos.RoomTypeTypes.Residentialsingleroom;
                case Domain.Base.Entities.RoomType.Residentialsuiteroom:
                    return Dtos.RoomTypeTypes.Residentialsuiteroom;
                case Domain.Base.Entities.RoomType.Residentialtripleroom:
                    return Dtos.RoomTypeTypes.Residentialtripleroom;
                case Domain.Base.Entities.RoomType.Sciencelaboratory:
                    return Dtos.RoomTypeTypes.Sciencelaboratory;
                case Domain.Base.Entities.RoomType.Seminarroom:
                    return Dtos.RoomTypeTypes.Seminarroom;
                case Domain.Base.Entities.RoomType.Specialusefacility:
                    return Dtos.RoomTypeTypes.Specialusefacility;
                case Domain.Base.Entities.RoomType.Studyfacility:
                    return Dtos.RoomTypeTypes.Studyfacility;
                case Domain.Base.Entities.RoomType.Supportfacility:
                    return Dtos.RoomTypeTypes.Supportfacility;
                default:
                    return Dtos.RoomTypeTypes.Other;
               
            }
        }
    }
}
