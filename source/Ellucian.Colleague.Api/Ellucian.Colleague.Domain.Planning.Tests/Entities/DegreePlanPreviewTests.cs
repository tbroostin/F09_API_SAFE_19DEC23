// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;

namespace Ellucian.Colleague.Domain.Planning.Tests.Entities
{
    [TestClass]
    public class DegreePlanPreviewTests
    {
        [TestClass]
        public class DegreePlanPreviewConstructor
        {
            private string personId;
            private int degreePlanId;
            private DegreePlan degreePlan;
            private DegreePlanPreview degreePlanPreview;
            private SampleDegreePlan sampleDegreePlan;
            private IEnumerable<Term> planningTerms;

            [TestInitialize]
            public async void Initialize()
            {
                personId = "0000693";
                degreePlanId = 1;
                degreePlan = new DegreePlan(degreePlanId, personId, 1);
                degreePlan.AddTerm("2012/FA");
                degreePlan.AddTerm("2013/SP");
                // Add a planned course on the sample degree plan.
                degreePlan.AddCourse(new PlannedCourse("110"), "2013/FA");
                sampleDegreePlan =await  new TestSampleDegreePlanRepository().GetAsync("TRACK3");
                planningTerms = new TestTermRepository().Get();
                //var studentAcademicCredits = new TestAcademicCreditRepository().Get();
                // Asserts are based off this constructor statement, unless another constructor is used in the test method
                degreePlanPreview = new DegreePlanPreview(degreePlan, sampleDegreePlan, new List<AcademicCredit>(), planningTerms, string.Empty);
            }

            [TestCleanup]
            public void CleanUp()
            {

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DegreePlanPreview_NullPlanningTerms()
            {
                degreePlanPreview = new DegreePlanPreview(degreePlan, sampleDegreePlan, new List<AcademicCredit>(), null, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DegreePlanPreview_NoPlanningTerms()
            {
                degreePlanPreview = new DegreePlanPreview(degreePlan, sampleDegreePlan, new List<AcademicCredit>(), new List<Term>(), string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DegreePlanPreview_NullDegreePlan()
            {
                degreePlanPreview = new DegreePlanPreview(null, sampleDegreePlan, new List<AcademicCredit>(), planningTerms, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DegreePlanPreview_NullSamplePlan()
            {
                degreePlanPreview = new DegreePlanPreview(degreePlan, null, new List<AcademicCredit>(), planningTerms, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DegreePlanPreview_NullCredits()
            {
                degreePlanPreview = new DegreePlanPreview(degreePlan, sampleDegreePlan, null, planningTerms, string.Empty);
            }

            [TestMethod]
            public void DegreePlanPreview_NumberOfTerms()
            {
                // Verify each term on preview is also found on the merged degree plan
                foreach (var termId in degreePlanPreview.Preview.TermIds)
                {
                    Assert.IsTrue(degreePlanPreview.MergedDegreePlan.TermIds.Contains(termId));
                }
            }
        }
    }
}
