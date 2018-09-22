// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System.Collections;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders
{
    public class GeneralLedgerAccountBuilder
    {
        private string glNumber;
        private IEnumerable<string> majorComonentStartPositions;

        public GeneralLedgerAccountBuilder()
        {
        }

        public GeneralLedgerAccountBuilder WithGlNumber(string glNumber)
        {
            this.glNumber = glNumber;
            return this;
        }

        public GeneralLedgerAccountBuilder WithStartPositions(IEnumerable<string> startPositions)
        {
            this.majorComonentStartPositions = startPositions;
            return this;
        }

        public GeneralLedgerAccount Build()
        {
            return new GeneralLedgerAccount(this.glNumber, this.majorComonentStartPositions);
        }
    }
}