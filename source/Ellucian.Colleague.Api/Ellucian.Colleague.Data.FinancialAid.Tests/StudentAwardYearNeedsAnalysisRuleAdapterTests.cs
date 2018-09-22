using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Data.FinancialAid.Tests
{
    [TestClass]
    public class StudentAwardYearNeedsAnalysisRuleAdapterTests
    {
        public StudentAwardYear context;

        public StudentAwardYearNeedsAnalysisRuleAdapter ruleAdapter;

        public RuleDescriptor descriptor;

        public string studentId;
        public string awardYear;
        public string federallyFlaggedIsirId;
        public decimal totalEstimatedExpenses;
        public decimal estimatedExpensesAdjustment;
        public string financialAidOfficeId;

        [TestInitialize]
        public void Initialize()
        {
            studentId = "0003914";
            awardYear = "2014";
            federallyFlaggedIsirId = "5";
            totalEstimatedExpenses = 1234;
            estimatedExpensesAdjustment = 12;
            financialAidOfficeId = "MAIN";

            context = new StudentAwardYear(studentId, awardYear, new FinancialAidOffice(financialAidOfficeId))
            {
                FederallyFlaggedIsirId = federallyFlaggedIsirId,
                TotalEstimatedExpenses = totalEstimatedExpenses,
                EstimatedExpensesAdjustment = estimatedExpensesAdjustment,
            };

            //rule descriptor called FOO that checks for existence of CS.FED.ISIR.ID
            descriptor = new RuleDescriptor()
            {
                Id = "FOO",
                PrimaryView = "CS.ACYR",
                Expressions = new List<RuleExpressionDescriptor>()
                {
                    new RuleExpressionDescriptor("WITH", "CS.FED.ISIR.ID", "NE", "\"\"")
                }
            };

            ruleAdapter = new StudentAwardYearNeedsAnalysisRuleAdapter();
        }

        [TestMethod]
        public void GetRecordIdTest()
        {
            Assert.AreEqual(studentId, ruleAdapter.GetRecordId(context));
        }

        [TestMethod]
        public void GetFileSuiteInstanceTest()
        {
            Assert.AreEqual(awardYear, ruleAdapter.GetFileSuiteInstance(context));
        }

        [TestMethod]
        public void ContextTypeTest()
        {
            Assert.AreEqual(typeof(StudentAwardYear), ruleAdapter.ContextType);
        }

        [TestMethod]
        public void ExpectedPrimaryViewTest()
        {
            Assert.AreEqual("CS.ACYR", ruleAdapter.ExpectedPrimaryView);
        }

        [TestMethod]
        public void NoExpressionForRuleTest()
        {
            descriptor.Expressions = new List<RuleExpressionDescriptor>();
            var rule = ruleAdapter.Create(descriptor) as Rule<StudentAwardYear>;
            Assert.IsFalse(rule.HasExpression);
        }

        [TestMethod]
        public void CsStudentIdTest()
        {
            descriptor = new RuleDescriptor()
            {
                Id = "FOO",
                PrimaryView = "CS.ACYR",
                Expressions = new List<RuleExpressionDescriptor>()
                {
                    new RuleExpressionDescriptor("WITH", "CS.STUDENT.ID", "EQ", "\""+studentId+"\"")
                }
            };
            var rule = ruleAdapter.Create(descriptor) as Rule<StudentAwardYear>;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(context));
        }

        [TestMethod]
        public void CsFedIsirIdTest()
        {
            descriptor = new RuleDescriptor()
            {
                Id = "FOO",
                PrimaryView = "CS.ACYR",
                Expressions = new List<RuleExpressionDescriptor>()
                {
                    new RuleExpressionDescriptor("WITH", "CS.FED.ISIR.ID", "EQ", "\""+federallyFlaggedIsirId+"\"")
                }
            };
            var rule = ruleAdapter.Create(descriptor) as Rule<StudentAwardYear>;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(context));
        }

        [TestMethod]
        public void CsOfficeTest()
        {
            descriptor = new RuleDescriptor()
            {
                Id = "FOO",
                PrimaryView = "CS.ACYR",
                Expressions = new List<RuleExpressionDescriptor>()
                {
                    new RuleExpressionDescriptor("WITH", "CS.OFFICE", "EQ", "\""+financialAidOfficeId+"\"")
                }
            };
            var rule = ruleAdapter.Create(descriptor) as Rule<StudentAwardYear>;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(context));
        }

        [TestMethod]
        public void UnsupportedFieldForRuleTest()
        {
            descriptor.Expressions.Add(new RuleExpressionDescriptor("WITH", new RuleDataElement() { Id = "FOOBAR" }, "NE", "\"\""));
            var rule = ruleAdapter.Create(descriptor) as Rule<StudentAwardYear>;
            Assert.IsFalse(rule.HasExpression);
        }


    }
}
