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
using Ellucian.Data.Colleague.Repositories;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class GeographicAreaService : BaseCoordinationService, IGeographicAreaService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IGeographicAreasRepository _geographicAreaRepository;
        private readonly IPersonRepository _personRepository;
        private const string _dataOrigin = "Colleague";

        public GeographicAreaService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository,
                                         IGeographicAreasRepository geographicAreaRepository,
                                         IPersonRepository personRepository,
                                         ICurrentUserFactory currentUserFactory,
                                         IRoleRepository roleRepository,
                                         ILogger logger,
                                         IConfigurationRepository configRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, null, configRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            _geographicAreaRepository = geographicAreaRepository;
            _personRepository = personRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets all geographic areas
        /// </summary>
        /// <returns>Collection of GeographicArea DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.GeographicArea>, int>> GetGeographicAreasAsync(int offset, int limit, bool bypassCache = false)
        {
            var geographicAreaCollection = new List<Ellucian.Colleague.Dtos.GeographicArea>();

            var allGeographicAreaTypes = await _referenceDataRepository.GetGeographicAreaTypesAsync(bypassCache);
            var typeGuidFund = ConvertCodeToGuid(allGeographicAreaTypes, "FUND");
            var typeGuidPost = ConvertCodeToGuid(allGeographicAreaTypes, "POST");
            var typeGuidGov = ConvertCodeToGuid(allGeographicAreaTypes, "GOV");
            if (string.IsNullOrEmpty(typeGuidFund) && string.IsNullOrEmpty(typeGuidPost) && string.IsNullOrEmpty(typeGuidGov))
            {
                throw new KeyNotFoundException("Geographic Area Type is not populated.");
            }

            var geographicAreasEntities = await _geographicAreaRepository.GetGeographicAreasAsync(offset, limit, bypassCache);
            if (geographicAreasEntities != null && geographicAreasEntities.Item1 != null && geographicAreasEntities.Item1.Any())
            {
                foreach (var geographicArea in geographicAreasEntities.Item1)
                {
                    switch (geographicArea.Type)
                    {
                        case "FUND":
                            geographicAreaCollection.Add(ConvertGeographicAreaEntityToGeographicAreaDto(geographicArea, typeGuidFund));
                            break;
                        case "GOV":
                            geographicAreaCollection.Add(ConvertGeographicAreaEntityToGeographicAreaDto(geographicArea, typeGuidGov));
                            break;
                        case "POST":
                            geographicAreaCollection.Add(ConvertGeographicAreaEntityToGeographicAreaDto(geographicArea, typeGuidPost));
                            break;
                    }
                }
            }

            if (geographicAreaCollection.Any())
            {
                return new Tuple<IEnumerable<Ellucian.Colleague.Dtos.GeographicArea>, int>(geographicAreaCollection, geographicAreasEntities.Item2);
            }
            else
            {
                return new Tuple<IEnumerable<Ellucian.Colleague.Dtos.GeographicArea>, int>(new List<Ellucian.Colleague.Dtos.GeographicArea>(), 0);
            }

        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Get an geographic area from its GUID
        /// </summary>
        /// <returns>GeographicArea DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.GeographicArea> GetGeographicAreaByGuidAsync(string guid)
        {
            try
            {
                var geographicAreaEntity = await _geographicAreaRepository.GetGeographicAreaByIdAsync(guid);
                if (geographicAreaEntity == null)
                {
                    throw new KeyNotFoundException("Geographic area not found for GUID " + guid);
                }
                
                var allGeographicAreaTypes = await _referenceDataRepository.GetGeographicAreaTypesAsync(false);

                var entityType = geographicAreaEntity.Type;
                string typeGuid = ConvertCodeToGuid(allGeographicAreaTypes, entityType);
                return ConvertGeographicAreaEntityToGeographicAreaDto(geographicAreaEntity, typeGuid);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("Geographic area not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts an Geographic Area domain entity to its corresponding GeographicArea DTO
        /// </summary>
        /// <param name="source">Geographic Area domain entity</param>
        /// <returns>GeographicArea DTO</returns>
        private Dtos.GeographicArea ConvertGeographicAreaEntityToGeographicAreaDto(GeographicArea source, string typeGuid)
        {
            var geographicArea = new Dtos.GeographicArea();
            geographicArea.Id = source.Guid;
            geographicArea.Code = source.Code;
            geographicArea.Title = source.Description;
            geographicArea.Description = null;
            switch (source.Type)
            {
                case "FUND":
                    geographicArea.Type = new Dtos.GeographicAreaTypeProperty()
                    {
                        category = Dtos.GeographicAreaTypeCategory.Fundraising,
                        detail = new Dtos.GuidObject2() { Id = typeGuid }
                    };
                    break;
                case "GOV":
                    geographicArea.Type = new Dtos.GeographicAreaTypeProperty()
                    {
                        category = Dtos.GeographicAreaTypeCategory.Governmental,
                        detail = new Dtos.GuidObject2() { Id = typeGuid }
                    };
                    break;
                case "POST":
                    geographicArea.Type = new Dtos.GeographicAreaTypeProperty()
                    {
                        category = Dtos.GeographicAreaTypeCategory.Postal,
                        detail = new Dtos.GuidObject2() { Id = typeGuid }
                    };
                    break;
            }
            
            return geographicArea;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts an Chapter domain entity to its corresponding GeographicArea DTO
        /// </summary>
        /// <param name="source">Chapter domain entity</param>
        /// <returns>GeographicArea DTO</returns>
        private Dtos.GeographicArea ConvertChapterEntityToGeographicAreaDto(Chapter source, string typeGuid)
        {
            var geographicArea = new Dtos.GeographicArea();
            geographicArea.Id = source.Guid;
            geographicArea.Code = source.Code;
            geographicArea.Title = source.Description;
            geographicArea.Description = null;
            geographicArea.Type = new Dtos.GeographicAreaTypeProperty() 
            { 
                category = Dtos.GeographicAreaTypeCategory.Fundraising, 
                detail = new Dtos.GuidObject2() { Id = typeGuid} 
            };

            return geographicArea;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts an ZipcodeXlat domain entity to its corresponding GeographicArea DTO
        /// </summary>
        /// <param name="source">ZipcodeXlat domain entity</param>
        /// <returns>GeographicArea DTO</returns>
        private Dtos.GeographicArea ConvertZipcodeXlatEntityToGeographicAreaDto(ZipcodeXlat source, string typeGuid)
        {
            var geographicArea = new Dtos.GeographicArea();
            geographicArea.Id = source.Guid;
            geographicArea.Code = source.Code;
            geographicArea.Title = source.Description;
            geographicArea.Description = null;

            geographicArea.Type = new Dtos.GeographicAreaTypeProperty()
            {
                category = Dtos.GeographicAreaTypeCategory.Postal,
                detail = new Dtos.GuidObject2() { Id = typeGuid }
            };

            return geographicArea;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts an County domain entity to its corresponding GeographicArea DTO
        /// </summary>
        /// <param name="source">County domain entity</param>
        /// <returns>GeographicArea DTO</returns>
        private Dtos.GeographicArea ConvertCountyEntityToGeographicAreaDto(County source, string typeGuid)
        {
            var geographicArea = new Dtos.GeographicArea();
            geographicArea.Id = source.Guid;
            geographicArea.Code = source.Code;
            geographicArea.Title = source.Description;
            geographicArea.Description = null;

            geographicArea.Type = new Dtos.GeographicAreaTypeProperty()
            {
                category = Dtos.GeographicAreaTypeCategory.Governmental,
                detail = new Dtos.GuidObject2() { Id = typeGuid }
            };

            return geographicArea;
        }
    }
}
