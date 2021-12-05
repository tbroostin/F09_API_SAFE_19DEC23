// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Coordination service for SectionCrosslistService
    /// </summary>
    [RegisterType]
    public class SectionCrosslistService : BaseCoordinationService, ISectionCrosslistService
    {
         private readonly ISectionRepository _sectionRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="sectionRepository"></param>
        /// <param name="configurationRepository"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        public SectionCrosslistService(IAdapterRegistry adapterRegistry, ISectionRepository sectionRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _sectionRepository = sectionRepository;           
        }

        /// <summary>
        /// Gets a page of SectionCrosslist's that can filtered by section guid
        /// </summary>
        /// <param name="offset">The position to start the paged return at</param>
        /// <param name="limit">the number of items to return for this page of results</param>
        /// <param name="section">The section GUID to filter SectionCrosslist list on</param>
        /// <returns>list of SectionCrosslist</returns>
        public async Task<Tuple<IEnumerable<SectionCrosslist>, int>> GetDataModelSectionCrosslistsPageAsync(int offset, int limit, string section = "")
        {
            var crosslistReturn = new List<SectionCrosslist>();
            var sectionId = string.Empty;

            if (!string.IsNullOrEmpty(section))
            {
                try
                {
                    sectionId = await _sectionRepository.GetSectionIdFromGuidAsync(section);
                    if (string.IsNullOrEmpty(sectionId))
                    {
                        return new Tuple<IEnumerable<SectionCrosslist>, int>(crosslistReturn, 0);
                    }
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<SectionCrosslist>, int>(crosslistReturn, 0);
                }  
            }

            var crosslistTuple = await _sectionRepository.GetSectionCrosslistsPageAsync(offset, limit, sectionId);

            if (crosslistTuple != null && crosslistTuple.Item1.Any())
            {
                crosslistReturn.AddRange(await ConvertSectionCrosslistEntityListToDtoListAsync(crosslistTuple.Item1));
            }

            return new Tuple<IEnumerable<SectionCrosslist>, int>(crosslistReturn, crosslistTuple.Item2);
        }

        /// <summary>
        /// Gets SectionCrosslist by Guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>SectionCrosslist</returns>
        public async Task<SectionCrosslist> GetDataModelSectionCrosslistsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("id", "id is required to get a sectioncrosslist.");
            }

            var sectionCrossListEntity = await _sectionRepository.GetSectionCrosslistByGuidAsync(guid);
            if (sectionCrossListEntity == null)
            {
                throw new KeyNotFoundException("id not valid.");
            }
            var ids = sectionCrossListEntity.SectionIds.ToList();
            var sectionGuidCollection = await _sectionRepository.GetSectionGuidsCollectionAsync(ids);

            var sectionCrossListDto = await ConvertSectionCrosslistEntityToDto(sectionCrossListEntity, sectionGuidCollection);
            return sectionCrossListDto;
        }

        /// <summary>
        /// Creates a SectionCrosslist 
        /// </summary>
        /// <param name="sectionCrosslist">SectionCrosslist to create</param>
        /// <returns>Created SectionCrosslist</returns>
        public async Task<SectionCrosslist> CreateDataModelSectionCrosslistsAsync(SectionCrosslist sectionCrosslist)
        {
            _sectionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            var entity = await ConvertSectionCrosslistDtoToEntity(sectionCrosslist);
            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
            var createdEntity = await _sectionRepository.CreateSectionCrosslistAsync(entity);
            var ids = createdEntity.SectionIds.ToList();
            var sectionGuidCollection = await _sectionRepository.GetSectionGuidsCollectionAsync(ids);
            var createdDto = await ConvertSectionCrosslistEntityToDto(createdEntity, sectionGuidCollection);

            return createdDto;
        }

        /// <summary>
        /// Updates the supplied SectionCrosslist
        /// </summary>
        /// <param name="sectionCrosslist"></param>
        /// <returns>Updated SectionCrosslist</returns>
        public async Task<SectionCrosslist> UpdateDataModelSectionCrosslistsAsync(SectionCrosslist sectionCrosslist)
        {
            _sectionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            var entity = await ConvertSectionCrosslistDtoToEntity(sectionCrosslist);
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            var updatedEntity = await _sectionRepository.UpdateSectionCrosslistAsync(entity);

            var ids = updatedEntity.SectionIds.ToList();                 
            var sectionGuidCollection = await _sectionRepository.GetSectionGuidsCollectionAsync(ids);
            var updatedDto = await ConvertSectionCrosslistEntityToDto(updatedEntity, sectionGuidCollection);

            return updatedDto;
        }

        /// <summary>
        /// Deletes the SectionCrosslist by guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task DeleteDataModelSectionCrosslistsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("id", "id is required to delete a sectioncrosslist.");
            }
            try
            {
                var sectionCrossListEntity = await _sectionRepository.GetSectionCrosslistByGuidAsync(guid);
                if (sectionCrossListEntity == null)
                {
                    throw new KeyNotFoundException();
                }
                await _sectionRepository.DeleteSectionCrosslistAsync(sectionCrossListEntity.Id);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException(string.Format("Section-crosslists not found for guid: '{0}'. ", guid));
            }
        }

        /// <summary>
        /// Convert SectionCrosslist DTO to Entity
        /// </summary>
        /// <param name="dtoSectionCrosslist">SectionCrosslist DTO</param>
        /// <returns>SectionCrosslist Entity</returns>
        private async Task<Domain.Student.Entities.SectionCrosslist> ConvertSectionCrosslistDtoToEntity(SectionCrosslist dtoSectionCrosslist)
        {         
            var sectionCrosslistGuid = string.Empty;
            var sectionCrosslistId = string.Empty;

            //check to see if the guid is the empty value, meaning this is a create
            if (!string.Equals(dtoSectionCrosslist.Id, Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                sectionCrosslistGuid = dtoSectionCrosslist.Id;

                try
                {
                    //get the colleague id for a sectioncrosslist, if it exists
                    sectionCrosslistId = await _sectionRepository.GetSectionCrosslistIdFromGuidAsync(sectionCrosslistGuid);
                }
                catch (Exception)
                {
                    //not logging or catching here because this can be called by a create and the guid wouldnt exist on a create call.
                    //id will be set to empty by default, which means it is a create
                }
            }

            var sectionIdList = new List<string>();
            var primarySectionId = string.Empty;
            var primaryFound = false;
            foreach (var sectionId in dtoSectionCrosslist.Sections)
            {
                var secId = await _sectionRepository.GetSectionIdFromGuidAsync(sectionId.Section.Id);
                if (sectionId.Type == SectionTypeForCrosslist.Primary)
                {
                    if (primaryFound == false)
                    {
                        primarySectionId = secId;
                        primaryFound = true;
                    }
                    else
                    {
                        IntegrationApiExceptionAddError("Only one section can be primary.", "Validation.Exception", sectionCrosslistGuid, sectionCrosslistId); ;
                    }
                }
                sectionIdList.Add(secId);
            }

            var sectionCrosslistEntity = new Domain.Student.Entities.SectionCrosslist(sectionCrosslistId, primarySectionId, sectionIdList, sectionCrosslistGuid);

            if (dtoSectionCrosslist.MaxEnrollment != null && dtoSectionCrosslist.MaxEnrollment < 0)
            {
                IntegrationApiExceptionAddError("Maximum enrollment cannot be less than zero.", "Validation.Exception", sectionCrosslistGuid, sectionCrosslistId);
            }
            else
            {
                sectionCrosslistEntity.Capacity = dtoSectionCrosslist.MaxEnrollment;
            }
            if (dtoSectionCrosslist.MaxWaitlist != null && dtoSectionCrosslist.MaxWaitlist < 0)
            {
                IntegrationApiExceptionAddError("Maximum waitlist cannot be less than zero.", "Validation.Exception", sectionCrosslistGuid, sectionCrosslistId);
            }
            else
            {
                sectionCrosslistEntity.WaitlistMax = dtoSectionCrosslist.MaxWaitlist;
            }

            if (dtoSectionCrosslist.Waitlist != null)
            {
                if (dtoSectionCrosslist.Waitlist.HasValue && dtoSectionCrosslist.Waitlist.Value == WaitlistForCrosslist.Combined)
                {
                    sectionCrosslistEntity.WaitlistFlag = "Y";
                }
                else
                {
                    sectionCrosslistEntity.WaitlistFlag = "N";
                }
            }
            else
            {
                sectionCrosslistEntity.WaitlistFlag = "N";
            }

            return sectionCrosslistEntity;
        }

        /// <summary>
        /// Convert SectionCrosslist Entity to DTO
        /// </summary>
        /// <param name="entityCrosslist">SectionCrosslist Entity to convert</param>
        /// <returns>SectionCrosslist DTO</returns>
        private async Task<SectionCrosslist> ConvertSectionCrosslistEntityToDto(Domain.Student.Entities.SectionCrosslist entityCrosslist,
             Dictionary<string, string> sectionGuidCollection)
        {
            var sectionCrosslistDto = new SectionCrosslist();

            sectionCrosslistDto.Id = entityCrosslist.Guid;
            sectionCrosslistDto.MaxEnrollment = entityCrosslist.Capacity;
            sectionCrosslistDto.MaxWaitlist = entityCrosslist.WaitlistMax;

            //If Waitlistflag is Y, set it to combined, else default to Separate.
            if (string.Equals(entityCrosslist.WaitlistFlag, "Y", StringComparison.OrdinalIgnoreCase))
            {
                sectionCrosslistDto.Waitlist = WaitlistForCrosslist.Combined;
            }
            else
            {
                sectionCrosslistDto.Waitlist = WaitlistForCrosslist.Separate;
            }

            var sectionIdList = new List<SectionsForCrosslistDtoProperty>();

            foreach (var sectionId in entityCrosslist.SectionIds)
            {
                var newSectionIdItem = new SectionsForCrosslistDtoProperty();

                if (string.Equals(sectionId, entityCrosslist.PrimarySectionId, StringComparison.OrdinalIgnoreCase))
                {
                    newSectionIdItem.Type = SectionTypeForCrosslist.Primary;
                }
                else
                {
                    newSectionIdItem.Type = SectionTypeForCrosslist.Secondary;
                }

                //newSectionIdItem.Section = new GuidObject2(await _sectionRepository.GetSectionGuidFromIdAsync(sectionId));
                var sectionGuid = string.Empty;
                sectionGuidCollection.TryGetValue(sectionId, out sectionGuid);
                if (string.IsNullOrEmpty(sectionGuid))
                {
                    throw new KeyNotFoundException(string.Concat("section, ", "Unable to locate guid for : '", sectionId, "'"));
                }
                newSectionIdItem.Section = new GuidObject2(sectionGuid);

                sectionIdList.Add(newSectionIdItem);
            }

            sectionCrosslistDto.Sections = sectionIdList;

            return sectionCrosslistDto;
        }

        /// <summary>
        /// Convert List of SectionCrosslist Entity to DTO List
        /// </summary>
        /// <param name="entityCrosslist">SectionCrosslist Entity list to convert</param>
        /// <returns>List of SectionCrosslist DTO</returns>
        private async Task<IEnumerable<SectionCrosslist>> ConvertSectionCrosslistEntityListToDtoListAsync(IEnumerable<Domain.Student.Entities.SectionCrosslist> entityCrosslist)
        {
            var sectionCrosslistReturn = new List<SectionCrosslist>();

            if (entityCrosslist == null || !entityCrosslist.Any())
            {
                return sectionCrosslistReturn;
            }
            var ids = entityCrosslist
                  .Where(x => (x.SectionIds != null))
                  .SelectMany(x => x.SectionIds).Distinct().ToList();

            var sectionGuidCollection =await _sectionRepository.GetSectionGuidsCollectionAsync(ids);

            foreach (var sectionCrosslist in entityCrosslist)
            {
                sectionCrosslistReturn.Add(await ConvertSectionCrosslistEntityToDto(sectionCrosslist, sectionGuidCollection));
            }

            return sectionCrosslistReturn;
        }

        
    }
}