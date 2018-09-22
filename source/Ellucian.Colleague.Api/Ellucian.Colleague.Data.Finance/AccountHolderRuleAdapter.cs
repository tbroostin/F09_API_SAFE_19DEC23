// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
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
    public class AccountHolderRuleAdapter : RuleAdapter
    {
        public override string GetRecordId(object context)
        {
            return (context as AccountHolder).Id;
        }

        public override Type ContextType { get { return typeof(AccountHolder); } }

        public override string ExpectedPrimaryView
        {
            get { return "PERSON.AR"; }
        }

        protected override Rule CreateExpressionAndRule(string ruleId, Expression finalExpression, ParameterExpression contextExpression)
        {
            Expression<Func<AccountHolder, bool>> lambdaExpression = null;

            if (finalExpression != null && contextExpression != null)
            {
                lambdaExpression = Expression.Lambda<Func<AccountHolder, bool>>(finalExpression, contextExpression);
            }

            return new Rule<AccountHolder>(ruleId, lambdaExpression);
        }

        protected override Expression CreateDataElementExpression(RuleDataElement dataElement, Expression param, out string unsupportedMessage)
        {
            string unsupported = null;
            Expression lhs = null;
            switch (dataElement.Id)
            {
                case "PERSON.ID":
                    lhs = Expression.Property(param, "Id");
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
