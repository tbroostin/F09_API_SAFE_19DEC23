// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Web.Adapters;
using AutoMapper;
using slf4net;
using Ellucian.Colleague.Dtos.ColleagueFinance;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{
    /// <summary>
    /// Adapter for mapping the Journal Entry Entity into DTOs
    /// </summary>
    public class JournalEntryEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.JournalEntry, Ellucian.Colleague.Dtos.ColleagueFinance.JournalEntry>
    {
        public JournalEntryEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Convert a journal entry domain entity and all of its descendent objects into DTOs
        /// </summary>
        /// <param name="Source">Source Journal Entry domain entity</param>
        /// <param name="glMajorComponentStartPositions">General Ledger Components</param>
        /// <returns>Journal Entry DTO</returns>
        public JournalEntry MapToType(Domain.ColleagueFinance.Entities.JournalEntry Source, IEnumerable<string> glMajorComponentStartPositions)
        {
            // Copy the journal entry level properties
            var journalEntryDto = new Dtos.ColleagueFinance.JournalEntry();
            journalEntryDto.Id = Source.Id;
            journalEntryDto.EnteredDate = Source.EnteredDate;
            journalEntryDto.EnteredByName = Source.EnteredByName;
            journalEntryDto.Date = Source.Date;
            journalEntryDto.TotalDebits = Source.TotalDebits;
            journalEntryDto.TotalCredits = Source.TotalCredits;
            journalEntryDto.Author = Source.Author;
            journalEntryDto.AutomaticReversal = Source.AutomaticReversal;
            journalEntryDto.Comments = Source.Comments;

            // Translate the domain status into the DTO status
            switch (Source.Status)
            {
                case Domain.ColleagueFinance.Entities.JournalEntryStatus.Complete:
                    journalEntryDto.Status = JournalEntryStatus.Complete;
                    break;
                case Domain.ColleagueFinance.Entities.JournalEntryStatus.NotApproved:
                    journalEntryDto.Status = JournalEntryStatus.NotApproved;
                    break;
                case Domain.ColleagueFinance.Entities.JournalEntryStatus.Unfinished:
                    journalEntryDto.Status = JournalEntryStatus.Unfinished;
                    break;
            }

            // Translate the domain type into the DTO type
            switch (Source.Type)
            {
                case Domain.ColleagueFinance.Entities.JournalEntryType.General:
                    journalEntryDto.Type = JournalEntryType.General;
                    break;
                case Domain.ColleagueFinance.Entities.JournalEntryType.OpeningBalance:
                    journalEntryDto.Type = JournalEntryType.OpeningBalance;
                    break;
            }

            journalEntryDto.Items = new List<Dtos.ColleagueFinance.JournalEntryItem>();
            journalEntryDto.Approvers = new List<Dtos.ColleagueFinance.Approver>();

            // Initialize all necessary adapters to convert the descendent elements within the journal entry
            var journalEntryItemDtoAdapter = new JournalEntryItemEntityToDtoAdapter(adapterRegistry, logger);
            var approverDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.Approver, Dtos.ColleagueFinance.Approver>(adapterRegistry, logger);

            // Convert the journal entry item domain entities into DTOs
            foreach (var journalEntryItem in Source.Items)
            {
                var journalEntryItemDto = journalEntryItemDtoAdapter.MapToType(journalEntryItem, glMajorComponentStartPositions);

                // Add the journal entry item DTO to the journal entry DTO
                journalEntryDto.Items.Add(journalEntryItemDto);
            }

            // Convert the journal entry approver domain entities into DTOS
            foreach (var approver in Source.Approvers)
            {
                var approverDto = approverDtoAdapter.MapToType(approver);

                // Add the journal entry approver DTO to the journal entry DTO
                journalEntryDto.Approvers.Add(approverDto);
            }

            return journalEntryDto;
        }
    }
}
