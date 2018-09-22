// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    /// <summary>
    /// Holds a rule written for purposes of use within requirements. Such a rule must have either a
    /// Course or AcademicCredit context. This class encapsulates all business logic related to that.
    /// </summary>
    [Serializable]
    public class RequirementRule
    {
        private readonly Rule<AcademicCredit> creditRule;
        private readonly Rule<Course> courseRule;
        private readonly Dictionary<object, bool> answers = new Dictionary<object, bool>();

        public RequirementRule(Rule<AcademicCredit> creditRule)
        {
            if (creditRule == null)
            {
                throw new ArgumentNullException("creditRule");
            }
            this.creditRule = creditRule;
            this.courseRule = null;
        }

        public RequirementRule(Rule<Course> courseRule)
        {
            if (courseRule == null)
            {
                throw new ArgumentNullException("courseRule");
            }
            this.courseRule = courseRule;
            this.creditRule = null;
        }

        public bool Passes(AcadResult result, bool eligibilityRule)
        {
            if (courseRule != null)
            {
                if (result.GetCourse() == null)
                {
                    // I don't think we can allow a credit with no course to pass a course rule.
                    return false;
                }

                // This handles a planned course or a credit with a course
                return EvaluateCourseRule(result.GetCourse());
            }
            else
            {
                if (result.GetType() == typeof(CourseResult))
                {
                    if (eligibilityRule)
                    {
                        // Planned courses always pass eligibility rules
                        return true;
                    }

                    // Planned courses never pass STC rules at the group level.
                    return false;
                }
                return EvaluateCreditRule(result.GetAcadCred());
            }
        }

        //public bool CourseBased { get { return courseRule != null; } }
        //public bool CreditBased { get { return creditRule != null; } }

        public static RequirementRule TryCreate(Rule rule)
        {
            if (rule.GetType() == typeof(Rule<AcademicCredit>))
            {
                return new RequirementRule((Rule<AcademicCredit>)rule);
            }
            else if (rule.GetType() == typeof(Rule<Course>))
            {
                return new RequirementRule((Rule<Course>)rule);
            }
            return null;
        }

        /// <summary>
        /// Specifies the answer that should always be returned when asked to evaluate this rule against the given context.
        /// </summary>
        /// <param name="answer">the answer</param>
        /// <param name="context">the context, must not be null</param>
        public void SetAnswer(bool answer, object context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            answers[context] = answer;
        }

        private bool EvaluateCreditRule(AcademicCredit credit)
        {
            if (answers.ContainsKey(credit))
            {
                return answers[credit];
            }

            if (creditRule.HasExpression)
            {
                return creditRule.Passes(credit);
            }

            // Sure.. why not
            return true;
        }

        private bool EvaluateCourseRule(Course course)
        {
            if (answers.ContainsKey(course))
            {
                return answers[course];
            }

            return courseRule.Passes(course);
        }

        public Rule Rule
        {
            get
            {
                if (courseRule == null)
                {
                    return creditRule;
                }
                return courseRule;
            }
        }

        public Rule<AcademicCredit> CreditRule { get { return creditRule; } }
        public Rule<Course> CourseRule { get { return courseRule; } }

        public RequirementRule Copy()
        {
            if (this.courseRule == null)
            {
                return new RequirementRule(creditRule);
            }
            return new RequirementRule(courseRule);
        }

        public string Id
        {
            get
            {
                if (courseRule == null)
                {
                    return creditRule.Id;
                }
                return courseRule.Id;
            }
        }

        public override string ToString()
        {
            return Id;
        }
    }
}
