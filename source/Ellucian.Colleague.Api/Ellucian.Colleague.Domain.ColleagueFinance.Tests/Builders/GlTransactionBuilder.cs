// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders
{
    public class GlTransactionBuilder
    {
        public GlTransaction GlTransactionEntity;
        public string Id;
        public GlTransactionType TransactionType;
        public string Source;
        public string GlAccountNumber;
        public decimal Amount;
        public string ReferenceNumber;
        public DateTime TransactionDate;
        public string Description;

        public GlTransactionBuilder()
        {
            this.Id = "V0000367";
            this.TransactionType = GlTransactionType.Actual;
            this.Source = "PJ";
            this.GlAccountNumber = "11_00_01_00_33333_54005";
            this.Amount = 250m;
            this.ReferenceNumber = "RV000001";
            this.TransactionDate = DateTime.Now;
            this.Description = "Transaction description";
        }

        public GlTransactionBuilder WithId(string id)
        {
            this.Id = id;
            return this;
        }

        public GlTransactionBuilder WithTransactionType(GlTransactionType type)
        {
            this.TransactionType = type;
            return this;
        }

        public GlTransactionBuilder WithSource(string source)
        {
            this.Source = source;
            return this;
        }

        public GlTransactionBuilder WithGlAccountNumber(string glAccountNumber)
        {
            this.GlAccountNumber = glAccountNumber;
            return this;
        }

        public GlTransactionBuilder WithAmount(decimal amount)
        {
            this.Amount = amount;
            return this;
        }

        public GlTransactionBuilder WithReferenceNumber(string referenceNumber)
        {
            this.ReferenceNumber = referenceNumber;
            return this;
        }

        public GlTransactionBuilder WithTransactionDate(DateTime date)
        {
            this.TransactionDate = date;
            return this;
        }

        public GlTransactionBuilder WithDescription(string description)
        {
            this.Description = description;
            return this;
        }

        public GlTransaction Build()
        {
            this.GlTransactionEntity = new GlTransaction(this.Id, this.TransactionType, this.Source, this.GlAccountNumber, this.Amount,
                this.ReferenceNumber, this.TransactionDate, this.Description);
            return this.GlTransactionEntity;
        }
    }
}