// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using System;
using System.Linq.Expressions;

namespace Ellucian.Colleague.Data.Student
{
    /// <summary>
    /// Knows how to map rules against COURSES into .NET expression trees against Course.
    /// </summary>
    public class CourseRuleAdapter : RuleAdapter
    {
        public override string GetRecordId(object context)
        {
            return (context as Course).Id;
        }

        public override Type ContextType { get { return typeof(Course); } }

        protected override Expression CreateDataElementExpression(RuleDataElement dataElement, Expression param, out string unsupportedMessage)
        {
            Expression lhs = null;
            string unsupported = null;
            switch (dataElement.Id)
            {
                case "COURSES.ID":
                    lhs = Expression.Property(param, "Id");
                    break;
                case "CRS.NO":
                    lhs = Expression.Property(param, "Number");
                    break;
                case "CRS.NAME":
                    lhs = Expression.Property(param, "Name");
                    break;
                case "CRS.SUBJECT":
                    lhs = Expression.Property(param, "SubjectCode");
                    break;
                case "CRS.LEVELS":
                    lhs = Expression.Property(param, "CourseLevelCodes");
                    break;
                case "CRS.DEPTS":
                    lhs = Expression.Property(param, "DepartmentCodes");
                    break;
                case "CRS.CRED.TYPE":
                    lhs = Expression.Property(param, "LocalCreditType");
                    break;
                case "CRS.COURSE.TYPES":
                    lhs = Expression.Property(param, "Types");
                    break;
                case "CRS.SHORT.TITLE":
                    lhs = Expression.Property(param, "Title");
                    break;
                case "CRS.LONG.TITLE":
                    lhs = Expression.Property(param, "LongTitle");
                    break;
                case "CRS.ACAD.LEVEL":
                    lhs = Expression.Property(param, "AcademicLevelCode");
                    break;
                case "CRS.SESSION.CYCLE":
                    lhs = Expression.Property(param, "TermSessionCycle");
                    break;
                case "CRS.YEARLY.CYCLE":
                    lhs = Expression.Property(param, "TermYearlyCycle");
                    break;
                case "CRS.TOPIC.CODE":
                    lhs = Expression.Property(param, "TopicCode");
                    break;
                case "CRS.CIP":
                    lhs = Expression.Property(param, "FederalCourseClassification");
                    break;
                case "CRS.GRADE.SCHEME":
                    lhs = Expression.Property(param, "GradeSchemeCode");
                    break;
                case "CRS.EXTERNAL.SOURCE":
                    lhs = Expression.Property(param, "ExternalSource");
                    break;
                default:
                    unsupported = "The field " + dataElement.Id + " is not supported yet";
                    break;
            }
            unsupportedMessage = unsupported;
            return lhs;
        }

        protected override Rule CreateExpressionAndRule(string ruleId, Expression finalExpression, ParameterExpression contextExpression)
        {
            Expression<Func<Course, bool>> lambdaExpression = null;

            if (finalExpression != null && contextExpression != null)
            {
                lambdaExpression = Expression.Lambda<Func<Course, bool>>(finalExpression, contextExpression);
            }

            return new Rule<Course>(ruleId, lambdaExpression);
        }

        public override string ExpectedPrimaryView
        {
            get { return "COURSES"; }
        }
    }
}
