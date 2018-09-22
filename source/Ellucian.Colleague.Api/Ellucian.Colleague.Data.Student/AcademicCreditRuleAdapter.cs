// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq.Expressions;
using Ellucian.Colleague.Data.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Data.Student
{
    /// <summary>
    /// Knows how to map rules against STUDENT.ACAD.CRED into .NET expression trees against AcademicCredit.
    /// </summary>
    public class AcademicCreditRuleAdapter : RuleAdapter
    {
        public override string GetRecordId(object context)
        {
            return (context as AcademicCredit).Id;
        }

        public override Type ContextType { get { return typeof(AcademicCredit); } }

        public override string ExpectedPrimaryView
        {
            get { return "STUDENT.ACAD.CRED"; }
        }

        protected override Rule CreateExpressionAndRule(string ruleId, Expression finalExpression, ParameterExpression contextExpression)
        {
            Expression<Func<AcademicCredit, bool>> lambdaExpression = null;

            if (finalExpression != null && contextExpression != null)
            {
                lambdaExpression = Expression.Lambda<Func<AcademicCredit, bool>>(finalExpression, contextExpression);
            }

            return new Rule<AcademicCredit>(ruleId, lambdaExpression);
        }

        protected override Expression CreateDataElementExpression(RuleDataElement dataElement, Expression param, out string unsup)
        {
            string unsupported = null;
            Expression lhs = null;
            switch (dataElement.Id)
            {
                case "STC.COURSE.NAME":
                    lhs = Expression.Property(param, "CourseName");
                    break;
                case "STC.COURSE":
                    lhs = Expression.Condition(
                        // Check if the Course on the credit is null
                        Expression.Equal(Expression.Property(param, "Course"), Expression.Constant(null)),
                        // If null, set LHS to empty string
                        Expression.Constant(""),
                        // Otherwise set LHS to the course Id
                        Expression.Property(Expression.Property(param, "Course"), "Id"));
                    break;
                case "STC.CMPL.CRED":
                    lhs = Expression.Property(param, "CompletedCredit");
                    break;
                case "STC.SUBJECT":
                    lhs = Expression.Property(param, "SubjectCode");
                    break;
                case "STC.TERM":
                    lhs = Expression.Property(param, "TermCode");
                    break;
                case "STC.TITLE":
                    lhs = Expression.Property(param, "Title");
                    break;
                case "STC.COURSE.LEVEL":
                    lhs = Expression.Property(param, "CourseLevelCode");
                    break;
                case "STC.CRS.NAME":
                    // Computed column. Check if it's the same as delivered. If it is, we know the expression to generate.
                    if (!string.IsNullOrEmpty(dataElement.ComputedFieldDefinition)
                        && dataElement.ComputedFieldDefinition.Trim() == "TRANS(\"COURSES\",STC.COURSE,\"CRS.NAME\",\"X\")")
                    {
                        lhs = Expression.Condition(
                            // Check if the Course on the credit is null
                            Expression.Equal(Expression.Property(param, "Course"), Expression.Constant(null)),
                            // If null, set LHS to empty string
                            Expression.Constant(""),
                            // Otherwise set LHS to the course name
                            Expression.Property(Expression.Property(param, "Course"), "Name"));
                    }
                    else
                    {
                        unsupported = "The field " + dataElement + " is a computed column that is not as delivered";
                    }
                    break;
                case "STC.CRS.NUMBER":
                    if (!string.IsNullOrEmpty(dataElement.ComputedFieldDefinition)
                        && dataElement.ComputedFieldDefinition.Trim() == "TRANS(\"COURSES\",STC.COURSE,\"CRS.NO\",\"X\")")
                    {
                        lhs = Expression.Condition(
                            // Check if the Course on the credit is null
                            Expression.Equal(Expression.Property(param, "Course"), Expression.Constant(null)),
                            // If null, set LHS to empty string
                            Expression.Constant(""),
                            // Otherwise set LHS to the course number
                            Expression.Property(Expression.Property(param, "Course"), "Number"));

                    }
                    else
                    {
                        unsupported = "The field " + dataElement + " is a computed column that is not as delivered";
                    }
                    break;
                case "STC.CRED.TYPE":
                    lhs = Expression.Property(param, "LocalType");
                    break;
                // If a computed column is ever delivered that translates against the credit type category and we
                // need it evaluated in .net, this will be the logic for the computed column's rule evaluation case.
                // case "STC.CRED.TYPE.CATEGORY":
                //    // Convert enum value to corresponding string so comparison can be done
                //    var credTypeProperty = Expression.Property(param, "Type");
                //    lhs = Expression.Switch(credTypeProperty,
                //        Expression.Constant(""),
                //        new SwitchCase[] { 
                //            Expression.SwitchCase(Expression.Constant("I"), Expression.Constant(CreditType.Institutional)),
                //            Expression.SwitchCase(Expression.Constant("T"), Expression.Constant(CreditType.Transfer)),
                //            Expression.SwitchCase(Expression.Constant("C"), Expression.Constant(CreditType.ContinuingEducation))
                //        });
                case "STC.GRADE":
                    lhs = Expression.Property(param, "VerifiedLetterGrade");
                    break;
                case "STC.DEPTS":
                    lhs = Expression.Property(param, "DepartmentCodes");
                    break;
                case "STC.GPA.CRED":
                    lhs = Expression.Property(param, "GpaCredit");
                    break;
                case "STC.COURSE.SECTION":
                    lhs = Expression.Property(param, "SectionId");
                    break;
                case "STC.START.DATE":
                    lhs = Expression.Property(param, "StartDateForRules");
                    break;
                case "STC.CRED":
                    lhs = Expression.Property(param, "Credit");
                    break;
                case "STC.GRADE.SCHEME":
                    lhs = Expression.Property(param, "GradeSchemeCode");
                    break;
                case "STC.ACAD.LEVEL":
                    lhs = Expression.Property(param, "AcademicLevelCode");
                    break;
                case "STC.SECTION.NO":
                    lhs = Expression.Property(param, "SectionNumber");
                    break;
                case "STC.MARK":
                    lhs = Expression.Property(param, "Mark");
                    break;
                case "STC.PERSON.ID":
                    lhs = Expression.Property(param, "StudentId");
                    break;
                case "STC.FINAL.GRADE":
                    lhs = Expression.Property(param, "FinalGradeId");
                    break;

                default:
                    unsupported = "The field " + dataElement + " is not handled in .NET yet";
                    break;
            }
            unsup = unsupported;
            return lhs;
        }
    }

    class Expr
    {
        public string Connective { get; set; }
        public Expression Expression { get; set; }
    }
}
