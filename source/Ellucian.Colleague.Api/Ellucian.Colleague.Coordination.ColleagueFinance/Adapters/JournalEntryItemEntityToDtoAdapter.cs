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
    /// Adapter for mapping from the Journal Entry Item entity to DTO
    /// </summary>
    public class JournalEntryItemEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.JournalEntryItem, Ellucian.Colleague.Dtos.ColleagueFinance.JournalEntryItem>
    {
        public JournalEntryItemEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Convert a journal entry item domain entity into a DTO
        /// </summary>
        /// <param name="Source">Journal entry item domain entity to be converted</param>
        /// <param name="glMajorComponentStartPositions">List of GL major component start positions, used to format GL numbers</param>
        /// <returns>Journal entry item DTO</returns>
        public JournalEntryItem MapToType(Domain.ColleagueFinance.Entities.JournalEntryItem Source, IEnumerable<string> glMajorComponentStartPositions)
        {
            // Copy journal entry item properties
            var journalEntryItemDto = new JournalEntryItem();
            journalEntryItemDto.Description = Source.Description;
            journalEntryItemDto.GlAccount = Source.GlAccountNumber;
            journalEntryItemDto.FormattedGlAccount = Source.GetFormattedGlAccount(glMajorComponentStartPositions);
            journalEntryItemDto.GlAccountDescription = Source.GlAccountDescription;
            journalEntryItemDto.ProjectNumber = Source.ProjectNumber;
            journalEntryItemDto.ProjectLineItemCode = Source.ProjectLineItemCode;
            journalEntryItemDto.Debit = Source.Debit;
            journalEntryItemDto.Credit = Source.Credit;

            return journalEntryItemDto;
        }
    }
}

