// Copyright 2016-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class SourceService : BaseCoordinationService, ISourceService
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly ILogger _logger;
        private const string _dataOrigin = "Colleague";

        public SourceService(
            IAdapterRegistry adapterRegistry,
            IStudentReferenceDataRepository studentReferenceDataRepository,
            IReferenceDataRepository referenceDataRepository,
            ICurrentUserFactory currentUserFactory,
            IConfigurationRepository configurationRepository,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, null, configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Gets all sources
        /// </summary>
        /// <returns>Collection of Source DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Source>> GetSourcesAsync(bool bypassCache)
        {
            var sourceContextsGuidCollection = new Dictionary<string, string>();
            var sourceContextEntities = await _referenceDataRepository.GetSourceContextsAsync(bypassCache);
            if (sourceContextEntities != null && sourceContextEntities.Any())
            {
                foreach (var sourceContext in sourceContextEntities)
                {
                    if (!string.IsNullOrEmpty(sourceContext.Guid) && !string.IsNullOrEmpty(sourceContext.Code))
                    {
                        sourceContextsGuidCollection.Add(sourceContext.Code, sourceContext.Guid);
                    }
                }
            }

            var sourceCollection = new List<Ellucian.Colleague.Dtos.Source>();

            var testSourceEntities = (await _studentReferenceDataRepository.GetTestSourcesAsync(bypassCache));
            if (testSourceEntities != null && testSourceEntities.Any())
            {
                foreach (var source in testSourceEntities)
                {
                    sourceCollection.Add(ConvertTestSourceEntityToSourceDtoAsync(source, sourceContextsGuidCollection));
                }
            }

            var remarkCodeEntities = (await _referenceDataRepository.GetRemarkCodesAsync(bypassCache));
            if (remarkCodeEntities != null && remarkCodeEntities.Any())
            {
                foreach (var remarkCode in remarkCodeEntities)
                {
                    sourceCollection.Add(ConvertRemarkCodeEntityToSourceDtoAsync(remarkCode, sourceContextsGuidCollection));
                }
            }

            var addressChangeSourceEntities = (await _referenceDataRepository.GetAddressChangeSourcesAsync(bypassCache));
            if (addressChangeSourceEntities != null && addressChangeSourceEntities.Any())
            {
                foreach (var addressChangeSources in addressChangeSourceEntities)
                {
                    sourceCollection.Add(ConvertAddressChangeSourceEntityToSourceDtoAsync(addressChangeSources, sourceContextsGuidCollection));
                }
            }

            var appplicationSourceEntities = (await _studentReferenceDataRepository.GetApplicationSourcesAsync(bypassCache));
            if (appplicationSourceEntities != null && appplicationSourceEntities.Any())
            {
                foreach (var applicationSource in appplicationSourceEntities)
                {
                    sourceCollection.Add(ConvertApplicationSourceEntityToSourceDtoAsync(applicationSource, sourceContextsGuidCollection));
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
            var sourceContextsGuidCollection = new Dictionary<string, string>();
            var sourceContextEntities = await _referenceDataRepository.GetSourceContextsAsync(true);
            if (sourceContextEntities != null && sourceContextEntities.Any())
            {
                foreach (var sourceContext in sourceContextEntities)
                {
                    if (!string.IsNullOrEmpty(sourceContext.Guid) && !string.IsNullOrEmpty(sourceContext.Code))
                    {
                        sourceContextsGuidCollection.Add(sourceContext.Code, sourceContext.Guid);
                    }
                }
            }

            try
            {
                var lookupResult = await _referenceDataRepository.GetGuidLookupResultFromGuidAsync(id);
                if (lookupResult == null)
                    throw new KeyNotFoundException("No sources found for GUID " + id);


                try
                {
                    switch (lookupResult.PrimaryKey.ToUpperInvariant())
                    {
                        case "APPL.TEST.SOURCES":
                            return ConvertTestSourceEntityToSourceDtoAsync((await _studentReferenceDataRepository.GetTestSourcesAsync(true)).Where(s => s.Guid == id).First(),
                                sourceContextsGuidCollection);

                        case "REMARK.CODES":
                            return ConvertRemarkCodeEntityToSourceDtoAsync((await _referenceDataRepository.GetRemarkCodesAsync(true)).Where(s => s.Guid == id).First(),
                                sourceContextsGuidCollection);

                        case "ADDRESS.CHANGE.SOURCES":
                            return ConvertAddressChangeSourceEntityToSourceDtoAsync((await _referenceDataRepository.GetAddressChangeSourcesAsync(true)).Where(s => s.Guid == id).First(),
                                sourceContextsGuidCollection);

                        case "APPLICATION.SOURCES":
                            return ConvertApplicationSourceEntityToSourceDtoAsync((await _studentReferenceDataRepository.GetApplicationSourcesAsync(true)).Where(s => s.Guid == id).First(),
                                sourceContextsGuidCollection);

                        default:
                            throw new KeyNotFoundException("No sources found for GUID " + id);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    throw new InvalidOperationException("No sources found for GUID " + id, ex);
                }
        }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No sources found for GUID " + id, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Converts a TestSource domain entity to its corresponding Source DTO
        /// </summary>
        /// <param name="testsource">TestSource domain entity</param>
        /// <returns>Source DTO</returns>
        private Ellucian.Colleague.Dtos.Source ConvertTestSourceEntityToSourceDtoAsync(Ellucian.Colleague.Domain.Student.Entities.TestSource testsource,
            Dictionary<string, string> sourceContextsGuidCollection)
        {
            var source = new Ellucian.Colleague.Dtos.Source();

            source.Id = testsource.Guid;
            source.Code = testsource.Code;
            source.Title = testsource.Description;
            source.Description = null;
            source.Status = Dtos.LifeCycleStatus.Active;

            if (sourceContextsGuidCollection != null && sourceContextsGuidCollection.Any())
            {
                var context = sourceContextsGuidCollection.FirstOrDefault(x => x.Key == "TESTS");
                if (context.Value != null)
                {
                    source.Contexts = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2(context.Value) };
                }
            }

            return source;
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Converts a RemarkCode domain entity to its corresponding Source DTO
        /// </summary>
        /// <param name="remarkCode">RemarkCode domain entity</param>
        /// <returns>Source DTO</returns>
        private Ellucian.Colleague.Dtos.Source ConvertRemarkCodeEntityToSourceDtoAsync(Ellucian.Colleague.Domain.Base.Entities.RemarkCode remarkCode,
            Dictionary<string, string> sourceContextsGuidCollection)
        {
            var source = new Ellucian.Colleague.Dtos.Source();

            source.Id = remarkCode.Guid;
            source.Code = remarkCode.Code;
            source.Title = remarkCode.Description;
            source.Description = null;
            source.Status = Dtos.LifeCycleStatus.Active;

            if (sourceContextsGuidCollection != null && sourceContextsGuidCollection.Any())
            {
                var context = sourceContextsGuidCollection.FirstOrDefault(x => x.Key == "REMARKS");
                if (context.Value != null)
                {
                    source.Contexts = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2(context.Value) };
                }
            }

            return source;
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Converts an AddressTypeSource domain entity to its corresponding Source DTO
        /// </summary>
        /// <param name="addressChangeSource">AddressTypeSource domain entity</param>
        /// <returns>Source DTO</returns>
        private Ellucian.Colleague.Dtos.Source ConvertAddressChangeSourceEntityToSourceDtoAsync(Ellucian.Colleague.Domain.Base.Entities.AddressChangeSource addressChangeSource,
            Dictionary<string, string> sourceContextsGuidCollection)
        {
            var source = new Ellucian.Colleague.Dtos.Source();

            source.Id = addressChangeSource.Guid;
            source.Code = addressChangeSource.Code;
            source.Title = addressChangeSource.Description;
            source.Description = null;
            source.Status = Dtos.LifeCycleStatus.Active;

            if (sourceContextsGuidCollection != null && sourceContextsGuidCollection.Any())
            {
                var context = sourceContextsGuidCollection.FirstOrDefault(x => x.Key == "ADDRESSES");
                if (context.Value != null)
                {
                    source.Contexts = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2(context.Value) };
                }
            }

            return source;
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Converts an ApplicationSource domain entity to its corresponding Source DTO
        /// </summary>
        /// <param name="applicationSource">ApplicationSource domain entity</param>
        /// <returns>Source DTO</returns>
        private Ellucian.Colleague.Dtos.Source ConvertApplicationSourceEntityToSourceDtoAsync(Ellucian.Colleague.Domain.Student.Entities.ApplicationSource applicationSource,
            Dictionary<string, string> sourceContextsGuidCollection)
        {
            var source = new Ellucian.Colleague.Dtos.Source();

            source.Id = applicationSource.Guid;
            source.Code = applicationSource.Code;
            source.Title = applicationSource.Description;
            source.Description = null;
            source.Status = Dtos.LifeCycleStatus.Active;

            if (sourceContextsGuidCollection != null && sourceContextsGuidCollection.Any())
            {
                var context = sourceContextsGuidCollection.FirstOrDefault(x => x.Key == "APPLS");
                if (context.Value != null)
                {
                    source.Contexts = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2(context.Value) };
                }
            }

            return source;
        }
    }
}