// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders
{
    public class CostCenterBuilder
    {
        public CostCenter CostCenterEntity;
        public string Id;
        public CostCenterSubtotal CostCenterSubtotalEntity;
        public CostCenterGlAccount GlAccountEntity;
        public List<GeneralLedgerComponentDescription> GlComponentDescriptions;
        
        private CostCenterSubtotalBuilder SubtotalBuilderObject = new CostCenterSubtotalBuilder();


        public CostCenterBuilder()
        {
            this.Id = "0100010133333";
            this.GlComponentDescriptions = new List<GeneralLedgerComponentDescription>();
            this.GlAccountEntity = new CostCenterGlAccount("010001013333353080",GlBudgetPoolType.None);
            this.CostCenterSubtotalEntity = SubtotalBuilderObject
                .WithId("33333")
                .WithGlAccount(GlAccountEntity).BuildWithGlAccount();
        }

        public CostCenterBuilder WithId(string id)
        {
            this.Id = id;
            return this;
        }

        public CostCenterBuilder WithSubtotal(CostCenterSubtotal subtotal)
        {
            this.CostCenterSubtotalEntity = subtotal;
            return this;
        }

        public CostCenterBuilder WithGlComponentDescriptions(IEnumerable<GeneralLedgerComponentDescription> glComponentDescriptions)
        {
            this.GlComponentDescriptions = null;

            if (glComponentDescriptions == null)
                this.GlComponentDescriptions = null;
            else
                this.GlComponentDescriptions = glComponentDescriptions.ToList();

            return this;
        }

        public CostCenter Build()
        {
            this.CostCenterEntity = new CostCenter(this.Id, this.CostCenterSubtotalEntity, this.GlComponentDescriptions);
            return this.CostCenterEntity;
        }
    }
}