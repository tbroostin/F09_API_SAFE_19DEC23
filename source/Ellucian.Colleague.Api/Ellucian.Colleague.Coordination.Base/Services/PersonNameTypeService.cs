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
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class PersonNameTypeService : BaseCoordinationService, IPersonNameTypeService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;       
        private const string _dataOrigin = "Colleague";

        public PersonNameTypeService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository,
                                         ICurrentUserFactory currentUserFactory,
                                         IRoleRepository roleRepository,
                                         ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _referenceDataRepository = referenceDataRepository;        
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets all person name types
        /// </summary>
        /// <returns>Collection of PersonNameType DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.PersonNameTypeItem>> GetPersonNameTypesAsync(bool bypassCache = false)
        {
            var personNameTypeCollection = new List<Ellucian.Colleague.Dtos.PersonNameTypeItem>();

            var personNameTypeEntities = await _referenceDataRepository.GetPersonNameTypesAsync(bypassCache);
            if (personNameTypeEntities != null && personNameTypeEntities.Count() > 0)
            {
                foreach (var personNameType in personNameTypeEntities)
                {
                    personNameTypeCollection.Add(ConvertPersonNameTypeEntityToPersonNameTypeDto(personNameType));
                }
            }
            return personNameTypeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets all person name types
        /// </summary>
        /// <returns>Collection of PersonNameType DTO objects</returns>
        public async Task<IEnumerable<Dtos.PersonNameTypeItem>> GetPersonNameTypes2Async(bool bypassCache)
        {
            var personNameTypeCollection = new List<Ellucian.Colleague.Dtos.PersonNameTypeItem>();

            var personNameTypeEntities = await _referenceDataRepository.GetPersonNameTypesAsync(bypassCache);
            if (personNameTypeEntities != null && personNameTypeEntities.Count() > 0)
            {
                foreach (var personNameType in personNameTypeEntities)
                {
                    personNameTypeCollection.Add(ConvertPersonNameTypeEntityToPersonNameTypeDto2(personNameType));
                }
            }
            return personNameTypeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Get a person name type from its ID
        /// </summary>
        /// <returns>PersonNameType DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.PersonNameTypeItem> GetPersonNameTypeByIdAsync(string id)
        {
            try
            {
                return ConvertPersonNameTypeEntityToPersonNameTypeDto((await _referenceDataRepository.GetPersonNameTypesAsync(true)).Where(rt => rt.Guid == id).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Person Name Type not found for ID " + id, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Get a person name type from its ID
        /// </summary>
        /// <returns>PersonNameType DTO object</returns>
        public async Task<Dtos.PersonNameTypeItem> GetPersonNameTypeById2Async(string id)
        {
            try
            {
                return ConvertPersonNameTypeEntityToPersonNameTypeDto2((await _referenceDataRepository.GetPersonNameTypesAsync(true)).Where(rt => rt.Guid == id).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Person Name Type not found for ID " + id, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a PersonNameTypeItem domain entity to its corresponding PersonNameTypeItem DTO
        /// </summary>
        /// <param name="source">Person Name Type domain entity</param>
        /// <returns>PersonNameType DTO</returns>
        private Dtos.PersonNameTypeItem ConvertPersonNameTypeEntityToPersonNameTypeDto(Domain.Base.Entities.PersonNameTypeItem source)
        {
            var personNameTypeItem = new Dtos.PersonNameTypeItem();
            personNameTypeItem.Id = source.Guid;
            personNameTypeItem.Code = source.Code;
            personNameTypeItem.Title = source.Description;
            personNameTypeItem.Description = null;
            personNameTypeItem.Type = ConvertPersonNameTypeDomainEnumToPersonPersonNameTypeDtoEnum(source.Type);

            return personNameTypeItem;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a PersonNameTypeItem domain entity to its corresponding PersonNameTypeItem DTO
        /// </summary>
        /// <param name="source">Person Name Type domain entity</param>
        /// <returns>PersonNameType DTO</returns>
        private Dtos.PersonNameTypeItem ConvertPersonNameTypeEntityToPersonNameTypeDto2(Domain.Base.Entities.PersonNameTypeItem source)
        {
            var personNameTypeItem = new Dtos.PersonNameTypeItem();
            personNameTypeItem.Id = source.Guid;
            personNameTypeItem.Code = source.Code;
            personNameTypeItem.Title = source.Description;
            personNameTypeItem.Description = null;
            personNameTypeItem.Type = ConvertPersonNameTypeDomainEnumToPersonPersonNameTypeDtoEnum2(source.Type);

            return personNameTypeItem;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a PersonNameType domain enumeration value to its corresponding PersonNameType DTO enumeration value
        /// </summary>
        /// <param name="source">PersonNameType domain enumeration value</param>
        /// <returns>PersonNameType DTO enumeration value</returns>
        private Dtos.EnumProperties.PersonNameType2 ConvertPersonNameTypeDomainEnumToPersonPersonNameTypeDtoEnum(Domain.Base.Entities.PersonNameType source)
        {
            switch (source)
            {
                case Domain.Base.Entities.PersonNameType.Birth:
                    return Dtos.EnumProperties.PersonNameType2.Birth;
                case Domain.Base.Entities.PersonNameType.Legal:
                    return Dtos.EnumProperties.PersonNameType2.Legal;
                default:
                    return Dtos.EnumProperties.PersonNameType2.Personal;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a PersonNameType domain enumeration value to its corresponding PersonNameType DTO enumeration value
        /// </summary>
        /// <param name="source">PersonNameType domain enumeration value</param>
        /// <returns>PersonNameType DTO enumeration value</returns>
        private Dtos.EnumProperties.PersonNameType2 ConvertPersonNameTypeDomainEnumToPersonPersonNameTypeDtoEnum2(Domain.Base.Entities.PersonNameType source)
        {
            switch (source)
            {
                case Domain.Base.Entities.PersonNameType.Birth:
                    return Dtos.EnumProperties.PersonNameType2.Birth;
                case Domain.Base.Entities.PersonNameType.Legal:
                    return Dtos.EnumProperties.PersonNameType2.Legal;
                case Domain.Base.Entities.PersonNameType.Chosen:
                    return Dtos.EnumProperties.PersonNameType2.Favored;
                default:
                    return Dtos.EnumProperties.PersonNameType2.Personal;
            }
        }
    }
}