// Copyright 2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders
{
    public class GeneralLedgerComponentBuilder
    {
        public GeneralLedgerComponent Component;
        public string ComponentName;
        public bool IsPartOfDescription;
        public GeneralLedgerComponentType ComponentType;
        public string StartPosition;
        public string ComponentLength;

        public GeneralLedgerComponentBuilder()
        {
            this.ComponentName = "FUND";
            this.IsPartOfDescription = false;
            this.ComponentType = GeneralLedgerComponentType.Fund;
            this.StartPosition = "1";
            this.ComponentLength = "2";
        }

        public GeneralLedgerComponentBuilder WithComponent(string component)
        {
            this.ComponentName = component;
            return this;
        }

        public GeneralLedgerComponentBuilder WithIsPartOfDescription(bool isPartOfDescription)
        {
            this.IsPartOfDescription = isPartOfDescription;
            return this;
        }

        public GeneralLedgerComponentBuilder WithComponentType(GeneralLedgerComponentType componentType)
        {
            this.ComponentType = componentType;
            return this;
        }

        public GeneralLedgerComponentBuilder WithStartPosition(string startPosition)
        {
            this.StartPosition = startPosition;
            return this;
        }

        public GeneralLedgerComponentBuilder WithLength(string length)
        {
            this.ComponentLength = length;
            return this;
        }

        public GeneralLedgerComponent Build()
        {
            this.Component = new GeneralLedgerComponent(ComponentName, IsPartOfDescription, ComponentType, StartPosition, ComponentLength);
            return this.Component;
        }
    }
}