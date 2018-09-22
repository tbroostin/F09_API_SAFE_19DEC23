// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders
{
    [Serializable]
    public class GeneralLedgerAccountStructureBuilder
    {
        public GeneralLedgerAccountStructure GeneralLedgerAccountStructure;

        public GeneralLedgerAccountStructureBuilder()
        {

        }

        public GeneralLedgerAccountStructure Build()
        {
            this.GeneralLedgerAccountStructure = new GeneralLedgerAccountStructure();
            return this.GeneralLedgerAccountStructure;
        }
    }
}