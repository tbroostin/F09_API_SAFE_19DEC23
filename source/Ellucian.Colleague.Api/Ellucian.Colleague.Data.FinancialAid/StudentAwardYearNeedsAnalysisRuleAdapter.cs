using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Ellucian.Colleague.Data.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Data.FinancialAid
{
    public class StudentAwardYearNeedsAnalysisRuleAdapter : RuleAdapter
    {
        public override string GetRecordId(object context)
        {
            return (context as StudentAwardYear).StudentId;
        }

        public override string GetFileSuiteInstance(object context)
        {
            return (context as StudentAwardYear).Code;
        }

        public override Type ContextType
        {
            get { return typeof(StudentAwardYear); }
        }

        public override string ExpectedPrimaryView
        {
            get { return "CS.ACYR"; }
        }

        protected override Rule CreateExpressionAndRule(string ruleId, Expression finalExpression, ParameterExpression contextExpression)
        {
            Expression<Func<StudentAwardYear, bool>> lambdaExpression = null;

            if (finalExpression != null && contextExpression != null)
            {
                lambdaExpression = Expression.Lambda<Func<StudentAwardYear, bool>>(finalExpression, contextExpression);
            }

            return new Rule<StudentAwardYear>(ruleId, lambdaExpression);
        }

        protected override Expression CreateDataElementExpression(RuleDataElement dataElement, Expression param, out string unsupportedMessage)
        {
            string unsupported = null;
            Expression lhs = null;
            switch (dataElement.Id)
            {
                case "CS.STUDENT.ID":
                    lhs = Expression.Property(param, "StudentId");
                    break;
                case "CS.FED.ISIR.ID":
                    lhs = Expression.Property(param, "FederallyFlaggedIsirId");
                    break;
                //can't use nullable types in expressions
                //case "CS.STD.TOTAL.EXPENSES":
                //    lhs = Expression.Property(param, "TotalEstimatedExpenses");
                //    break;
                //case "CS.BUDGET.ADJ":
                //    lhs = Expression.Property(param, "EstimatedExpensesAdjustment");
                //    break;
                case "CS.OFFICE":
                    lhs = Expression.Property(param, "FinancialAidOfficeId");
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
