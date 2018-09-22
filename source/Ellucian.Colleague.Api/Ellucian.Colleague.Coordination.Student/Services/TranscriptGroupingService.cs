﻿// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using slf4net;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Dependency;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class TranscriptGroupingService : ITranscriptGroupingService
    {
        private readonly ITranscriptGroupingRepository _transcriptGroupingRepository;
        private readonly IAdapterRegistry _adapterRegistry;

        public TranscriptGroupingService(IAdapterRegistry adapterRegistry, ITranscriptGroupingRepository transcriptGroupingRepository)
        {
            _adapterRegistry = adapterRegistry;
            _transcriptGroupingRepository = transcriptGroupingRepository;

        }

        /// <summary>
        /// Return the set of transcript groupings that are user selectable
        /// </summary>
        /// <returns>A set of transcript grouping DTOs</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.TranscriptGrouping>> GetSelectableTranscriptGroupingsAsync()
        {
            ICollection<Dtos.Student.TranscriptGrouping> transcriptGroupingDtos = new List<Dtos.Student.TranscriptGrouping>();
            var transcriptGroupings = await _transcriptGroupingRepository.GetAsync();

            var selectableGroupings = transcriptGroupings.Where(x => x.IsUserSelectable == true);

            // Get the right adapter for the type mapping
            var transcriptGroupingDtoAdapter = _adapterRegistry.GetAdapter<TranscriptGrouping, Ellucian.Colleague.Dtos.Student.TranscriptGrouping>();
            foreach (var transcriptGrouping in selectableGroupings)
            {
                // Map the degree plan entity to the degree plan DTO
                var transcriptGroupingDto = transcriptGroupingDtoAdapter.MapToType(transcriptGrouping);
                transcriptGroupingDtos.Add(transcriptGroupingDto);
            }

            return transcriptGroupingDtos;
        }
    }
}
