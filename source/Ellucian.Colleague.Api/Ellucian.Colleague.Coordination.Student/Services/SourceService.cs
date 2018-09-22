// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class SourceService : ISourceService
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly ILogger _logger;
        private const string _dataOrigin = "Colleague";

        public SourceService(IStudentReferenceDataRepository studentReferenceDataRepository,
            IReferenceDataRepository referenceDataRepository, 
            ILogger logger)
        {
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _referenceDataRepository = referenceDataRepository;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Gets all sources
        /// </summary>
        /// <returns>Collection of Source DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Source>> GetSourcesAsync(bool bypassCache)
        {
            var sourceCollection = new List<Ellucian.Colleague.Dtos.Source>();

            var testSourceEntities = (await _studentReferenceDataRepository.GetTestSourcesAsync(bypassCache));
            if (testSourceEntities != null && testSourceEntities.Any())
            {
                foreach (var source in testSourceEntities)
                {
                    sourceCollection.Add(await ConvertTestSourceEntityToSourceDtoAsync(source));
                }
            }

            var remarkCodeEntities = (await _referenceDataRepository.GetRemarkCodesAsync(bypassCache));
            if (remarkCodeEntities != null && remarkCodeEntities.Any())
            {
                foreach (var remarkCode in remarkCodeEntities)
                {
                    sourceCollection.Add(await ConvertRemarkCodeEntityToSourceDtoAsync(remarkCode));
                }
            }

            var addressChangeSourceEntities = (await _referenceDataRepository.GetAddressChangeSourcesAsync(bypassCache));
            if (addressChangeSourceEntities != null && addressChangeSourceEntities.Any())
            {
                foreach (var addressChangeSources in addressChangeSourceEntities)
                {
                    sourceCollection.Add(await ConvertAddressChangeSourceEntityToSourceDtoAsync(addressChangeSources));
                }
            }

            var appplicationSourceEntities = (await _studentReferenceDataRepository.GetApplicationSourcesAsync(bypassCache));
            if (appplicationSourceEntities != null && appplicationSourceEntities.Any())
            {
                foreach (var applicationSource in appplicationSourceEntities)
                {
                    sourceCollection.Add(await ConvertApplicationSourceEntityToSourceDtoAsync(applicationSource));
                }
            }

            return sourceCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Get a source from its ID
        /// </summary>
        /// <returns>Source DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.Source> GetSourceByIdAsync(string id)
        {
            try
            {
                var lookupResult = await _referenceDataRepository.GetGuidLookupResultFromGuidAsync(id);
                if (lookupResult == null)
                    throw new KeyNotFoundException("Source not found for ID " + id);
               
                switch (lookupResult.PrimaryKey.ToUpperInvariant())
                {
                    case "APPL.TEST.SOURCES":
                        return await ConvertTestSourceEntityToSourceDtoAsync((await _studentReferenceDataRepository.GetTestSourcesAsync(true)).Where(s => s.Guid == id).First());
                        
                    case "REMARK.CODES":
                        return await ConvertRemarkCodeEntityToSourceDtoAsync((await _referenceDataRepository.GetRemarkCodesAsync(true)).Where(s => s.Guid == id).First());
                        
                    case "ADDRESS.CHANGE.SOURCES":
                        return await ConvertAddressChangeSourceEntityToSourceDtoAsync((await _referenceDataRepository.GetAddressChangeSourcesAsync(true)).Where(s => s.Guid == id).First());

                    case "APPLICATION.SOURCES":
                        return await ConvertApplicationSourceEntityToSourceDtoAsync((await _studentReferenceDataRepository.GetApplicationSourcesAsync(true)).Where(s => s.Guid == id).First());
                  

                    default:
                        throw new KeyNotFoundException("Source not found for ID " + id);
                }
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No source was found for guid  " + id, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Converts a TestSource domain entity to its corresponding Source DTO
        /// </summary>
        /// <param name="testsource">TestSource domain entity</param>
        /// <returns>Source DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.Source> ConvertTestSourceEntityToSourceDtoAsync(Ellucian.Colleague.Domain.Student.Entities.TestSource testsource)
        {
            var source = new Ellucian.Colleague.Dtos.Source();

            source.Id = testsource.Guid;
            source.Code = testsource.Code;
            source.Title = testsource.Description;
            source.Description = null;
            source.Status = Dtos.LifeCycleStatus.Active;

            var context = (await _referenceDataRepository.GetSourceContextsAsync(false)).FirstOrDefault(x => x.Code == "TESTS");
            if (context != null)
                source.Contexts = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2(context.Guid) };

            return source;
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Converts a RemarkCode domain entity to its corresponding Source DTO
        /// </summary>
        /// <param name="remarkCode">RemarkCode domain entity</param>
        /// <returns>Source DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.Source> ConvertRemarkCodeEntityToSourceDtoAsync(Ellucian.Colleague.Domain.Base.Entities.RemarkCode remarkCode)
        {
            var source = new Ellucian.Colleague.Dtos.Source();

            source.Id = remarkCode.Guid;
            source.Code = remarkCode.Code;
            source.Title = remarkCode.Description;
            source.Description = null;
            source.Status = Dtos.LifeCycleStatus.Active;

            var context = (await _referenceDataRepository.GetSourceContextsAsync(false)).FirstOrDefault(x => x.Code == "REMARKS");
            if (context != null)
                source.Contexts = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2(context.Guid) };

            return source;
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Converts an AddressTypeSource domain entity to its corresponding Source DTO
        /// </summary>
        /// <param name="addressChangeSource">AddressTypeSource domain entity</param>
        /// <returns>Source DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.Source> ConvertAddressChangeSourceEntityToSourceDtoAsync(Ellucian.Colleague.Domain.Base.Entities.AddressChangeSource addressChangeSource)
        {
            var source = new Ellucian.Colleague.Dtos.Source();

            source.Id = addressChangeSource.Guid;
            source.Code = addressChangeSource.Code;
            source.Title = addressChangeSource.Description;
            source.Description = null;
            source.Status = Dtos.LifeCycleStatus.Active;

            var context = (await _referenceDataRepository.GetSourceContextsAsync(false)).FirstOrDefault(x => x.Code == "ADDRESSES");
            if (context != null)
                source.Contexts = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2(context.Guid) };

            return source;
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Converts an ApplicationSource domain entity to its corresponding Source DTO
        /// </summary>
        /// <param name="applicationSource">ApplicationSource domain entity</param>
        /// <returns>Source DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.Source> ConvertApplicationSourceEntityToSourceDtoAsync(Ellucian.Colleague.Domain.Student.Entities.ApplicationSource applicationSource)
        {
            var source = new Ellucian.Colleague.Dtos.Source();

            source.Id = applicationSource.Guid;
            source.Code = applicationSource.Code;
            source.Title = applicationSource.Description;
            source.Description = null;
            source.Status = Dtos.LifeCycleStatus.Active;

            var context = (await _referenceDataRepository.GetSourceContextsAsync(false)).FirstOrDefault(x => x.Code == "APPLS");
            if (context != null)
                source.Contexts = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2(context.Guid) };

            return source;
        }
    }
}