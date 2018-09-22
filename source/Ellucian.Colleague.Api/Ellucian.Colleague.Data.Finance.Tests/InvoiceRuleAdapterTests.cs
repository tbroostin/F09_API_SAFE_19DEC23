// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Data.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Data.Finance;
using System;
using System.Linq;

namespace Ellucian.Colleague.Data.Finance.Tests
{
    [TestClass]
    public class InvoiceRuleAdapterTests
    {
        public Invoice context;

        public InvoiceRuleAdapter ruleAdapter;

        public RuleDescriptor descriptor;

        [TestInitialize]
        public void Initialize()
        {
            var arInvoice = TestArInvoicesRepository.ArInvoices[0];
            var arInvoiceItems = TestArInvoiceItemsRepository.ArInvoiceItems.Where(arii => arii.InviInvoice == arInvoice.Recordkey);
            var charges = new List<Charge>();
            foreach(var arii in arInvoiceItems)
            {
                charges.Add(new Charge(arii.Recordkey, arii.InviInvoice, new List<string>() { arii.InviDesc }, arii.InviArCode, 
                    arii.InviExtChargeAmt.GetValueOrDefault()));
            }

            context = new Invoice(arInvoice.Recordkey, arInvoice.InvPersonId, arInvoice.InvArType, arInvoice.InvTerm, arInvoice.InvReferenceNos[0],
                arInvoice.InvDate.GetValueOrDefault(), arInvoice.InvDueDate.GetValueOrDefault(), arInvoice.InvBillingStartDate.GetValueOrDefault(),
                arInvoice.InvBillingEndDate.GetValueOrDefault(), arInvoice.InvDesc, charges);

            //rule descriptor called FOO that checks for non-Male account holders
            descriptor = new RuleDescriptor()
            {
                Id = "FOO",
                PrimaryView = "AR.INVOICES",
                Expressions = new List<RuleExpressionDescriptor>()
                {
                    new RuleExpressionDescriptor("WITH", "INV.PERSON.ID", "EQ", "\""+arInvoice.InvPersonId+"\"")
                }
            };

            ruleAdapter = new InvoiceRuleAdapter();
        }

        [TestMethod]
        public void GetRecordIdTest()
        {
            Assert.AreEqual(context.Id, ruleAdapter.GetRecordId(context));
        }

        [TestMethod]
        public void ContextTypeTest()
        {
            Assert.AreEqual(typeof(Invoice), ruleAdapter.ContextType);
        }

        [TestMethod]
        public void ExpectedPrimaryViewTest()
        {
            Assert.AreEqual("AR.INVOICES", ruleAdapter.ExpectedPrimaryView);
        }

        [TestMethod]
        public void NoExpressionForRuleTest()
        {
            descriptor.Expressions = new List<RuleExpressionDescriptor>();
            var rule = ruleAdapter.Create(descriptor) as Rule<Invoice>;
            Assert.IsFalse(rule.HasExpression);
        }

        [TestMethod]
        public void ArInvoicesIdTest()
        {
            descriptor = new RuleDescriptor()
            {
                Id = "FOO",
                PrimaryView = "AR.INVOICES",
                Expressions = new List<RuleExpressionDescriptor>()
                {
                    new RuleExpressionDescriptor("WITH", "AR.INVOICES.ID", "EQ", "\""+context.Id+"\"")
                }
            };
            var rule = ruleAdapter.Create(descriptor) as Rule<Invoice>;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(context));
        }

        [TestMethod]
        public void InvDescTest()
        {
            descriptor = new RuleDescriptor()
            {
                Id = "FOO",
                PrimaryView = "AR.INVOICES",
                Expressions = new List<RuleExpressionDescriptor>()
                {
                    new RuleExpressionDescriptor("WITH", "INV.DESC", "EQ", "\""+context.Description+"\"")
                }
            };
            var rule = ruleAdapter.Create(descriptor) as Rule<Invoice>;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(context));
        }

        [TestMethod]
        public void InvPersonIdTest()
        {
            descriptor = new RuleDescriptor()
            {
                Id = "FOO",
                PrimaryView = "AR.INVOICES",
                Expressions = new List<RuleExpressionDescriptor>()
                {
                    new RuleExpressionDescriptor("WITH", "INV.PERSON.ID", "EQ", "\""+context.PersonId+"\"")
                }
            };
            var rule = ruleAdapter.Create(descriptor) as Rule<Invoice>;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(context));
        }

        [TestMethod]
        public void InvArTypeTest()
        {
            descriptor = new RuleDescriptor()
            {
                Id = "FOO",
                PrimaryView = "AR.INVOICES",
                Expressions = new List<RuleExpressionDescriptor>()
                {
                    new RuleExpressionDescriptor("WITH", "INV.AR.TYPE", "EQ", "\""+context.ReceivableTypeCode+"\"")
                }
            };
            var rule = ruleAdapter.Create(descriptor) as Rule<Invoice>;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(context));
        }

        [TestMethod]
        public void InvDateTest()
        {
            descriptor = new RuleDescriptor()
            {
                Id = "FOO",
                PrimaryView = "AR.INVOICES",
                Expressions = new List<RuleExpressionDescriptor>()
                {
                    new RuleExpressionDescriptor("WITH", "INV.DATE", "EQ", "\""+context.Date.ToShortDateString()+"\"")
                },
                RuleConversionOptions = new RuleConversionOptions()
                {
                    CenturyThreshhold = 68,
                    DateDelimiter = "/",
                    DateFormat = "MDY"
                }
            };
            var rule = ruleAdapter.Create(descriptor) as Rule<Invoice>;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(context));
        }

        [TestMethod]
        public void InvTermTest()
        {
            descriptor = new RuleDescriptor()
            {
                Id = "FOO",
                PrimaryView = "AR.INVOICES",
                Expressions = new List<RuleExpressionDescriptor>()
                {
                    new RuleExpressionDescriptor("WITH", "INV.TERM", "EQ", "\""+context.TermId+"\"")
                }
            };
            var rule = ruleAdapter.Create(descriptor) as Rule<Invoice>;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(context));
        }

        [TestMethod]
        public void InvDueDateTest()
        {
            descriptor = new RuleDescriptor()
            {
                Id = "FOO",
                PrimaryView = "AR.INVOICES",
                Expressions = new List<RuleExpressionDescriptor>()
                {
                    new RuleExpressionDescriptor("WITH", "INV.DUE.DATE", "EQ", "\""+context.DueDate.ToShortDateString()+"\"")
                },
                RuleConversionOptions = new RuleConversionOptions()
                {
                    CenturyThreshhold = 68,
                    DateDelimiter = "/",
                    DateFormat = "MDY"
                }
            };
            var rule = ruleAdapter.Create(descriptor) as Rule<Invoice>;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(context));
        }

        [TestMethod]
        public void InvBillingStartDateTest()
        {
            descriptor = new RuleDescriptor()
            {
                Id = "FOO",
                PrimaryView = "AR.INVOICES",
                Expressions = new List<RuleExpressionDescriptor>()
                {
                    new RuleExpressionDescriptor("WITH", "INV.BILLING.START.DATE", "EQ", "\""+context.BillingStart.ToShortDateString()+"\"")
                },
                RuleConversionOptions = new RuleConversionOptions()
                {
                    CenturyThreshhold = 68,
                    DateDelimiter = "/",
                    DateFormat = "MDY"
                }
            };
            var rule = ruleAdapter.Create(descriptor) as Rule<Invoice>;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(context));
        }

        [TestMethod]
        public void InvBillingEndDateTest()
        {
            descriptor = new RuleDescriptor()
            {
                Id = "FOO",
                PrimaryView = "AR.INVOICES",
                Expressions = new List<RuleExpressionDescriptor>()
                {
                    new RuleExpressionDescriptor("WITH", "INV.BILLING.END.DATE", "EQ", "\""+context.BillingEnd.ToShortDateString()+"\"")
                },
                RuleConversionOptions = new RuleConversionOptions()
                {
                    CenturyThreshhold = 68,
                    DateDelimiter = "/",
                    DateFormat = "MDY"
                }
            };
            var rule = ruleAdapter.Create(descriptor) as Rule<Invoice>;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(context));
        }

        [TestMethod]
        public void InvNoTest()
        {
            descriptor = new RuleDescriptor()
            {
                Id = "FOO",
                PrimaryView = "AR.INVOICES",
                Expressions = new List<RuleExpressionDescriptor>()
                {
                    new RuleExpressionDescriptor("WITH", "INV.NO", "EQ", "\""+context.ReferenceNumber+"\"")
                }
            };
            var rule = ruleAdapter.Create(descriptor) as Rule<Invoice>;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(context));
        }

        [TestMethod]
        public void UnsupportedFieldForRuleTest()
        {
            descriptor.Expressions.Add(new RuleExpressionDescriptor("WITH", new RuleDataElement() { Id = "FOOBAR" }, "NE", "\"\""));
            var rule = ruleAdapter.Create(descriptor) as Rule<Invoice>;
            Assert.IsFalse(rule.HasExpression);
        }
    }
}
