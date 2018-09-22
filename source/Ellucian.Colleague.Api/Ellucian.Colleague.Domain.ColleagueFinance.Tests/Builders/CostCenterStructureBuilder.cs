// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders
{
    [Serializable]
    public class CostCenterStructureBuilder
    {
        public CostCenterStructure CostCenterStructure;

        public CostCenterStructureBuilder()
        {

        }

        public CostCenterStructure Build()
        {
            this.CostCenterStructure = new CostCenterStructure();
            return this.CostCenterStructure;
        }
    }
}