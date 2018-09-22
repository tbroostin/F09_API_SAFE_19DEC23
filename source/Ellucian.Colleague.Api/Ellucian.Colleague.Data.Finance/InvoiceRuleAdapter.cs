// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Ellucian.Colleague.Data.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Data.Finance
{
    public class InvoiceRuleAdapter : RuleAdapter
    {
        public override string GetRecordId(object context)
        {
            return (context as Invoice).Id;
        }

        public override Type ContextType { get { return typeof(Invoice); } }

        public override string ExpectedPrimaryView
        {
            get { return "AR.INVOICES"; }
        }

        protected override Rule CreateExpressionAndRule(string ruleId, Expression finalExpression, ParameterExpression contextExpression)
        {
            Expression<Func<Invoice, bool>> lambdaExpression = null;

            if (finalExpression != null && contextExpression != null)
            {
                lambdaExpression = Expression.Lambda<Func<Invoice, bool>>(finalExpression, contextExpression);
            }

            return new Rule<Invoice>(ruleId, lambdaExpression);
        }

        protected override Expression CreateDataElementExpression(RuleDataElement dataElement, Expression param, out string unsupportedMessage)
        {
            string unsupported = null;
            Expression lhs = null;
            switch (dataElement.Id)
            {
                case "AR.INVOICES.ID":
                    lhs = Expression.Property(param, "Id");
                    break;
                case "INV.DESC":
                    lhs = Expression.Property(param, "Description");
                    break;
                case "INV.PERSON.ID":
                    lhs = Expression.Property(param, "PersonId");
                    break;
                case "INV.AR.TYPE":
                    lhs = Expression.Property(param, "ReceivableTypeCode");
                    break;
                case "INV.DATE":
                    lhs = Expression.Property(param, "Date");
                    break;
                case "INV.TERM":
                    lhs = Expression.Property(param, "TermId");
                    break;
                case "INV.DUE.DATE":
                    lhs = Expression.Property(param, "DueDate");
                    break;
                case "INV.BILLING.START.DATE":
                    lhs = Expression.Property(param, "BillingStart");
                    break;
                case "INV.BILLING.END.DATE":
                    lhs = Expression.Property(param, "BillingEnd");
                    break;
                case "INV.NO":
                    lhs = Expression.Property(param, "ReferenceNumber");
                    break;
                default:
                    unsupported = "The field " + dataElement + " is not handled in .NET yet";
                    break;
            }
            unsupportedMessage = unsupported;
            return lhs;
        }
    }
}
