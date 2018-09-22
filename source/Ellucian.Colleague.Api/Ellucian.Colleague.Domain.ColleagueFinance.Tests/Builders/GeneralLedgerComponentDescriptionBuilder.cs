// Copyright 2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders
{
    public class GeneralLedgerComponentDescriptionBuilder
    {
        public GeneralLedgerComponentDescription ComponentDescriptionObject;
        public string Id;
        public GeneralLedgerComponentType ComponentType;

        public GeneralLedgerComponentDescriptionBuilder()
        {
            this.Id = "01";
            this.ComponentType = GeneralLedgerComponentType.Location;
        }

        public GeneralLedgerComponentDescriptionBuilder WithId(string id)
        {
            this.Id = id;
            return this;
        }

        public GeneralLedgerComponentDescriptionBuilder WithComponentType(GeneralLedgerComponentType componentType)
        {
            this.ComponentType = componentType;
            return this;
        }

        public GeneralLedgerComponentDescription Build()
        {
            this.ComponentDescriptionObject = new GeneralLedgerComponentDescription(this.Id, this.ComponentType);
            return this.ComponentDescriptionObject;
        }
    }
}