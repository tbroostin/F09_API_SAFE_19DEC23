// Copyright 2013-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.Requirements
{
    [TestClass]
    public class SubrequirementTests
    {
        [TestClass]
        public class EvaluateTests
        {
            private Subrequirement subReq = new Subrequirement("SREQ-1", "sub-req-multigroup.bb");

            [TestMethod]
            public void groupResults_parameter_is_null()
            {
                SubrequirementResult srr = subReq.Evaluate(null);
                Assert.IsNotNull(srr);
            }

            [TestMethod]
            public void groupResults_parameter_is_empty()
            {
                SubrequirementResult srr = subReq.Evaluate(new List<GroupResult>());
                Assert.IsNotNull(srr);
            }

        }

        [TestClass]
        public class FindGroupsToSkipPrivateMethodTests
        {
            Subrequirement subReq = null;
            MethodInfo methodInfo = null;
            List<Group> groups = new List<Group>();


            [TestInitialize]
            public void Initialize()
            {
                subReq = new Subrequirement("SREQ-1", "sub-req-multigroup.bb");
                methodInfo = typeof(Subrequirement).GetMethod("FindGroupsToSkip", BindingFlags.NonPublic | BindingFlags.Instance);
                Group gr1 = new Group("G1", "group 1", subReq);
                Group gr2 = new Group("G2", "group 2", subReq);
                groups.Add(gr2);
                groups.Add(gr1);

            }

            [TestMethod]
            public void findGroupsToSkip_ParamsAreNull()
            {

                object[] parameters = { null, null, null };
                var retVal = (IEnumerable<string>)methodInfo.Invoke(subReq, parameters);
                Assert.IsNotNull(retVal);
                Assert.AreEqual(retVal.Count(), 0);
            }

            [TestMethod]
            public void MinGroupsToTake_IsNull()
            {
                Subrequirement subReq = new Subrequirement("SREQ-1", "sub-req-multigroup.bb");

                List<GroupResult> groupResults = new List<GroupResult>();
                List<Group> groups = new List<Group>();
                Group gr1 = new Group("G1", "group 1", subReq);
                Group gr2 = new Group("G2", "group 2", subReq);
                groups.Add(gr1);
                groups.Add(gr2);
                groupResults.Add(new GroupResult(gr1));
                groupResults.Add(new GroupResult(gr2));
                object[] parameters = { groupResults, groups, null };
                var retVal = (IEnumerable<string>)methodInfo.Invoke(subReq, parameters);
                Assert.IsNotNull(retVal);
                Assert.AreEqual(retVal.Count(), 0);

            }

            [TestMethod]
            public void NotStartedNotPlanned_Take_1()
            {
                Subrequirement subReq = new Subrequirement("SREQ-1", "sub-req-multigroup.bb");

                List<GroupResult> groupResults = new List<GroupResult>();
                List<Group> groups = new List<Group>();
                Group gr1 = new Group("G1", "group 1", subReq);
                Group gr2 = new Group("G2", "group 2", subReq);
                groups.Add(gr2);
                groups.Add(gr1);
                groupResults.Add(new GroupResult(gr1));
                groupResults.Add(new GroupResult(gr2));
                object[] parameters = { groupResults, groups, 1 };
                var retVal = (IEnumerable<string>)methodInfo.Invoke(subReq, parameters);
                Assert.IsNotNull(retVal);
                Assert.AreEqual(retVal.Count(), 1);
                Assert.AreEqual(retVal.ToList()[0], "G1");

            }



            [TestMethod]
            public void Filter_WithCompletedAndNoInProgressGroupResults_Take_0()
            {
                Subrequirement subReq = new Subrequirement("SREQ-1", "sub-req-multigroup.bb");

                List<GroupResult> groupResults = new List<GroupResult>();
                Group gr1 = new Group("G1", "group 1", subReq);
                Group gr2 = new Group("G2", "group 2", subReq);
                groupResults.Add(new GroupResult(gr1));
                groupResults.Add(new GroupResult(gr2));
                groupResults[0].CompletionStatus = CompletionStatus.Completed;
                groupResults[0].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[1].CompletionStatus = CompletionStatus.Completed;
                groupResults[1].PlanningStatus = PlanningStatus.CompletelyPlanned;
                object[] parameters = { groupResults, groups, 0 };
                var retVal = (IEnumerable<string>)methodInfo.Invoke(subReq, parameters);
                Assert.IsNotNull(retVal);
                Assert.AreEqual(retVal.Count(), 2);
                Assert.AreEqual(retVal.ToList()[0], "G2");

                Assert.AreEqual(retVal.ToList()[1], "G1");

            }

            [TestMethod]
            public void Filter_WithCompletedAndNoInProgressGroupResults_Take_1()
            {
                Subrequirement subReq = new Subrequirement("SREQ-1", "sub-req-multigroup.bb");

                List<GroupResult> groupResults = new List<GroupResult>();
                Group gr1 = new Group("G1", "group 1", subReq);
                Group gr2 = new Group("G2", "group 2", subReq);
                groupResults.Add(new GroupResult(gr1));
                groupResults.Add(new GroupResult(gr2));
                groupResults[0].CompletionStatus = CompletionStatus.Completed;
                groupResults[0].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[1].CompletionStatus = CompletionStatus.Completed;
                groupResults[1].PlanningStatus = PlanningStatus.CompletelyPlanned;
                object[] parameters = { groupResults, groups, 1 };
                var retVal = (IEnumerable<string>)methodInfo.Invoke(subReq, parameters);
                Assert.IsNotNull(retVal);
                Assert.AreEqual(retVal.Count(), 1);
                Assert.AreEqual(retVal.ToList()[0], "G1");

            }
            [TestMethod]
            public void Filter_WithNoCompletedAndInProgressGroupResults_Take_1()
            {
                Subrequirement subReq = new Subrequirement("SREQ-1", "sub-req-multigroup.bb");

                List<GroupResult> groupResults = new List<GroupResult>();
                Group gr1 = new Group("G1", "group 1", subReq);
                Group gr2 = new Group("G2", "group 2", subReq);
                groupResults.Add(new GroupResult(gr1));
                groupResults.Add(new GroupResult(gr2));
                groupResults[0].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[0].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[0].Explanations.Add(GroupExplanation.Satisfied);
                groupResults[1].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[1].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[0].Explanations.Add(GroupExplanation.Satisfied);
                object[] parameters = { groupResults, groups, 1 };
                var retVal = (IEnumerable<string>)methodInfo.Invoke(subReq, parameters);
                Assert.IsNotNull(retVal);
                Assert.AreEqual(retVal.Count(), 1);
                Assert.AreEqual(retVal.ToList()[0], "G1");

            }
            [TestMethod]
            public void Filter_WithNoCompletedAndInProgressGroupResults_Take_2()
            {
                Subrequirement subReq = new Subrequirement("SREQ-1", "sub-req-multigroup.bb");

                List<GroupResult> groupResults = new List<GroupResult>();
                Group gr1 = new Group("G1", "group 1", subReq);
                Group gr2 = new Group("G2", "group 2", subReq);
                groupResults.Add(new GroupResult(gr1));
                groupResults.Add(new GroupResult(gr2));
                groupResults[0].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[0].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[0].Explanations.Add(GroupExplanation.Satisfied);
                groupResults[1].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[1].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[0].Explanations.Add(GroupExplanation.Satisfied);
                object[] parameters = { groupResults, groups, 2 };
                var retVal = (IEnumerable<string>)methodInfo.Invoke(subReq, parameters);
                Assert.IsNotNull(retVal);
                Assert.AreEqual(retVal.Count(), 0);

            }
            [TestMethod]
            public void Filter_WithNoCompletedAndInProgressGroupResults_Take_0()
            {
                Subrequirement subReq = new Subrequirement("SREQ-1", "sub-req-multigroup.bb");

                List<GroupResult> groupResults = new List<GroupResult>();
                Group gr1 = new Group("G1", "group 1", subReq);
                Group gr2 = new Group("G2", "group 2", subReq);
                groupResults.Add(new GroupResult(gr1));
                groupResults.Add(new GroupResult(gr2));
                groupResults[0].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[0].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[0].Explanations.Add(GroupExplanation.Satisfied);
                groupResults[1].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[1].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[0].Explanations.Add(GroupExplanation.Satisfied);
                object[] parameters = { groupResults, groups, 0 };
                var retVal = (IEnumerable<string>)methodInfo.Invoke(subReq, parameters);
                Assert.IsNotNull(retVal);
                Assert.AreEqual(retVal.Count(), 2);
                Assert.AreEqual(retVal.ToList()[0], "G2");
                Assert.AreEqual(retVal.ToList()[1], "G1");
            }
            [TestMethod]
            public void Filter_WithCompletedAndInProgressGroupResults_Take_0()
            {
                Subrequirement subReq = new Subrequirement("SREQ-1", "sub-req-multigroup.bb");

                List<GroupResult> groupResults = new List<GroupResult>();
                Group gr1 = new Group("G1", "group 1", subReq);
                Group gr2 = new Group("G2", "group 2", subReq);
                groupResults.Add(new GroupResult(gr1));
                groupResults.Add(new GroupResult(gr2));
                groupResults[0].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[0].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[0].Explanations.Add(GroupExplanation.Satisfied);
                groupResults[1].CompletionStatus = CompletionStatus.Completed;
                groupResults[1].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[0].Explanations.Add(GroupExplanation.Satisfied);
                object[] parameters = { groupResults, groups, 0 };
                var retVal = (IEnumerable<string>)methodInfo.Invoke(subReq, parameters);
                Assert.IsNotNull(retVal);
                Assert.AreEqual(retVal.Count(), 2);
                Assert.AreEqual(retVal.ToList()[0], "G2");
                Assert.AreEqual(retVal.ToList()[1], "G1");
            }
            [TestMethod]
            public void Filter_WithCompletedAndInProgressGroupResults_Take_1()
            {
                Subrequirement subReq = new Subrequirement("SREQ-1", "sub-req-multigroup.bb");

                List<GroupResult> groupResults = new List<GroupResult>();
                Group gr1 = new Group("G1", "group 1", subReq);
                Group gr2 = new Group("G2", "group 2", subReq);
                groupResults.Add(new GroupResult(gr1));
                groupResults.Add(new GroupResult(gr2));
                groupResults[0].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[0].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[0].Explanations.Add(GroupExplanation.Satisfied);
                groupResults[1].CompletionStatus = CompletionStatus.Completed;
                groupResults[1].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[0].Explanations.Add(GroupExplanation.Satisfied);
                object[] parameters = { groupResults, groups, 1 };
                var retVal = (IEnumerable<string>)methodInfo.Invoke(subReq, parameters);
                Assert.IsNotNull(retVal);
                Assert.AreEqual(retVal.Count(), 1);
                Assert.AreEqual(retVal.ToList()[0], "G1");
            }
            [TestMethod]
            public void Filter_WithCompletedAndInProgressGroupResults_Take_2()
            {
                Subrequirement subReq = new Subrequirement("SREQ-1", "sub-req-multigroup.bb");

                List<GroupResult> groupResults = new List<GroupResult>();
                Group gr1 = new Group("G1", "group 1", subReq);
                Group gr2 = new Group("G2", "group 2", subReq);
                groupResults.Add(new GroupResult(gr1));
                groupResults.Add(new GroupResult(gr2));
                groupResults[0].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[0].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[0].Explanations.Add(GroupExplanation.Satisfied);
                groupResults[1].CompletionStatus = CompletionStatus.Completed;
                groupResults[1].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[0].Explanations.Add(GroupExplanation.Satisfied);
                object[] parameters = { groupResults, groups, 2 };
                var retVal = (IEnumerable<string>)methodInfo.Invoke(subReq, parameters);
                Assert.IsNotNull(retVal);
                Assert.AreEqual(retVal.Count(), 0);


            }



        }


        [TestClass]
        public class FindGroupsToSkip_WithMixedStatuses_PrivateMethodTests
        {
            Subrequirement subReq = null;
            MethodInfo methodInfo = null;
            List<Group> groups = new List<Group>();
            List<GroupResult> groupResults = new List<GroupResult>();



            [TestInitialize]
            public void Initialize()
            {
                subReq = new Subrequirement("SREQ-1", "sub-req-multigroup.bb");
                methodInfo = typeof(Subrequirement).GetMethod("FindGroupsToSkip", BindingFlags.NonPublic | BindingFlags.Instance);
                Group gr1 = new Group("G1", "group 1", subReq);
                Group gr2 = new Group("G2", "group 2", subReq);
                Group gr3 = new Group("G3", "group 3", subReq);
                Group gr4 = new Group("G4", "group 4", subReq);
                Group gr5 = new Group("G5", "group 5", subReq);
                Group gr6 = new Group("G6", "group 6", subReq);
                Group gr7 = new Group("G7", "group 7", subReq);
                Group gr8 = new Group("G8", "group 8", subReq);
                Group gr9 = new Group("G9", "group 9", subReq);
                Group gr10 = new Group("G10", "group 10", subReq);
                Group gr11 = new Group("G11", "group 11", subReq);
                Group gr12 = new Group("G12", "group 12", subReq);
                groups.Add(gr1);
                groups.Add(gr2);
                groups.Add(gr3);
                groups.Add(gr4);
                groups.Add(gr5);
                groups.Add(gr6);
                groups.Add(gr7);
                groups.Add(gr8);
                groups.Add(gr9);
                groups.Add(gr10);
                groups.Add(gr11);
                groups.Add(gr12);

                groupResults.Add(new GroupResult(gr1));
                groupResults.Add(new GroupResult(gr2));
                groupResults.Add(new GroupResult(gr3));
                groupResults.Add(new GroupResult(gr4));
                groupResults.Add(new GroupResult(gr5));
                groupResults.Add(new GroupResult(gr6));
                groupResults.Add(new GroupResult(gr7));
                groupResults.Add(new GroupResult(gr8));
                groupResults.Add(new GroupResult(gr9));
                groupResults.Add(new GroupResult(gr10));
                groupResults.Add(new GroupResult(gr11));
                groupResults.Add(new GroupResult(gr12));

            }


            [TestMethod]
            public void WaivedIsPicked_Take_1()
            {
                Subrequirement subReq = new Subrequirement("SREQ-1", "sub-req-multigroup.bb");

                //group 1 have In progress and planned credits
                groupResults[0].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[0].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[0].Explanations.Add(GroupExplanation.Satisfied);
                //group 2 have all completed
                groupResults[1].CompletionStatus = CompletionStatus.Completed;
                groupResults[1].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[1].Explanations.Add(GroupExplanation.Satisfied);
                //group 3 all in progress
                groupResults[2].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[2].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[2].Explanations.Add(GroupExplanation.Satisfied);
                //group 4 not started
                groupResults[3].CompletionStatus = CompletionStatus.NotStarted;
                groupResults[3].PlanningStatus = PlanningStatus.NotPlanned;
                //group 5 plan and completed and not-started
                groupResults[4].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[4].PlanningStatus = PlanningStatus.PartiallyPlanned;
                groupResults[4].Explanations.Add(GroupExplanation.MinCourses);
                //group 6 plan completed in-progress 
                groupResults[5].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[5].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[5].Explanations.Add(GroupExplanation.PlannedSatisfied);
                //group 7 plan completed in-progress not-started
                groupResults[6].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[6].PlanningStatus = PlanningStatus.PartiallyPlanned;
                groupResults[6].Explanations.Add(GroupExplanation.MinCoursesPerDepartment);
                //group 8 completed
                groupResults[7].CompletionStatus = CompletionStatus.Completed;
                groupResults[7].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[7].Explanations.Add(GroupExplanation.Satisfied);
                //group 9 all planned
                groupResults[8].CompletionStatus = CompletionStatus.NotStarted;
                groupResults[8].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[8].Explanations.Add(GroupExplanation.PlannedSatisfied);
                //group 10 cpmpleted plan
                groupResults[9].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[9].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[9].Explanations.Add(GroupExplanation.MinCoursesPerDepartment);
                //group 11 waived
                groupResults[10].CompletionStatus = CompletionStatus.Waived;
                groupResults[10].PlanningStatus = PlanningStatus.PartiallyPlanned;
                groupResults[10].Explanations.Add(GroupExplanation.MinCoursesPerDepartment);
                //group 12 completed in-progress
                groupResults[11].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[11].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[11].Explanations.Add(GroupExplanation.PlannedSatisfied);
                object[] parameters = { groupResults, groups, 1 };
                var retVal = ((IEnumerable<string>)methodInfo.Invoke(subReq, parameters)).ToList();
                Assert.IsNotNull(retVal);
                Assert.AreEqual(retVal.Count(), 11);
                Assert.IsFalse(retVal.Contains("G11"));

                Assert.IsTrue(retVal.Contains("G2"));
                Assert.IsTrue(retVal.Contains("G8"));
                Assert.IsTrue(retVal.Contains("G1"));
                Assert.IsTrue(retVal.Contains("G3"));
                Assert.IsTrue(retVal.Contains("G6"));
                Assert.IsTrue(retVal.Contains("G10"));
                Assert.IsTrue(retVal.Contains("G12"));
                Assert.IsTrue(retVal.Contains("G9"));
                Assert.IsTrue(retVal.Contains("G5"));
                Assert.IsTrue(retVal.Contains("G7"));
                Assert.IsTrue(retVal.Contains("G4"));

            }
            [TestMethod]
            public void Filter_WithWaivedCompletedAndInProgressAndPlannedGroupResults_Take_2()
            {
                Subrequirement subReq = new Subrequirement("SREQ-1", "sub-req-multigroup.bb");


                //group 1 have In progress and planned credits
                groupResults[0].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[0].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[0].Explanations.Add(GroupExplanation.Satisfied);
                //group 2 have all completed
                groupResults[1].CompletionStatus = CompletionStatus.Completed;
                groupResults[1].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[1].Explanations.Add(GroupExplanation.Satisfied);
                //group 3 all in progress
                groupResults[2].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[2].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[2].Explanations.Add(GroupExplanation.Satisfied);
                //group 4 not started
                groupResults[3].CompletionStatus = CompletionStatus.NotStarted;
                groupResults[3].PlanningStatus = PlanningStatus.NotPlanned;
                //group 5 plan and completed and not-started
                groupResults[4].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[4].PlanningStatus = PlanningStatus.PartiallyPlanned;
                groupResults[4].Explanations.Add(GroupExplanation.MinCourses);
                //group 6 plan completed in-progress 
                groupResults[5].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[5].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[5].Explanations.Add(GroupExplanation.PlannedSatisfied);
                //group 7 plan completed in-progress not-started
                groupResults[6].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[6].PlanningStatus = PlanningStatus.PartiallyPlanned;
                groupResults[6].Explanations.Add(GroupExplanation.MinCoursesPerDepartment);

                //group 8 completed
                groupResults[7].CompletionStatus = CompletionStatus.Completed;
                groupResults[7].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[7].Explanations.Add(GroupExplanation.Satisfied);

                //group 9 all planned
                groupResults[8].CompletionStatus = CompletionStatus.NotStarted;
                groupResults[8].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[8].Explanations.Add(GroupExplanation.PlannedSatisfied);

                //group 10 cpmpleted plan
                groupResults[9].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[9].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[9].Explanations.Add(GroupExplanation.MinCoursesPerDepartment);

                //group 11 waived
                groupResults[10].CompletionStatus = CompletionStatus.Waived;
                groupResults[10].PlanningStatus = PlanningStatus.PartiallyPlanned;
                groupResults[10].Explanations.Add(GroupExplanation.MinCoursesPerDepartment);

                //group 12 completed in-progress
                groupResults[11].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[11].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[11].Explanations.Add(GroupExplanation.PlannedSatisfied);

                object[] parameters = { groupResults, groups, 2 };
                var retVal = ((IEnumerable<string>)methodInfo.Invoke(subReq, parameters)).ToList();
                Assert.IsNotNull(retVal);
                Assert.AreEqual(retVal.Count(), 10);
                Assert.IsFalse(retVal.Contains("G11"));
                Assert.IsFalse(retVal.Contains("G2"));

                Assert.IsTrue(retVal.Contains("G8"));
                Assert.IsTrue(retVal.Contains("G1"));
                Assert.IsTrue(retVal.Contains("G3"));
                Assert.IsTrue(retVal.Contains("G6"));
                Assert.IsTrue(retVal.Contains("G10"));
                Assert.IsTrue(retVal.Contains("G12"));
                Assert.IsTrue(retVal.Contains("G9"));
                Assert.IsTrue(retVal.Contains("G5"));
                Assert.IsTrue(retVal.Contains("G7"));
                Assert.IsTrue(retVal.Contains("G4"));
            }
            [TestMethod]
            public void Filter_WithCompletedAndInProgressAndPlannedGroupResults_Take_3()
            {
                Subrequirement subReq = new Subrequirement("SREQ-1", "sub-req-multigroup.bb");
                List<GroupResult> groupResults = new List<GroupResult>();
                Group gr1 = new Group("G1", "group 1", subReq);
                Group gr2 = new Group("G2", "group 2", subReq);
                Group gr3 = new Group("G3", "group 3", subReq);
                Group gr4 = new Group("G4", "group 4", subReq);
                Group gr5 = new Group("G5", "group 5", subReq);
                Group gr6 = new Group("G6", "group 6", subReq);
                Group gr7 = new Group("G7", "group 7", subReq);
                Group gr8 = new Group("G8", "group 8", subReq);
                Group gr9 = new Group("G9", "group 9", subReq);
                Group gr10 = new Group("G10", "group 10", subReq);
                Group gr11 = new Group("G11", "group 11", subReq);
                Group gr12 = new Group("G12", "group 12", subReq);
                groupResults.Add(new GroupResult(gr1));
                groupResults.Add(new GroupResult(gr2));
                groupResults.Add(new GroupResult(gr3));
                groupResults.Add(new GroupResult(gr4));
                groupResults.Add(new GroupResult(gr5));
                groupResults.Add(new GroupResult(gr6));
                groupResults.Add(new GroupResult(gr7));
                groupResults.Add(new GroupResult(gr8));
                groupResults.Add(new GroupResult(gr9));
                groupResults.Add(new GroupResult(gr10));
                groupResults.Add(new GroupResult(gr11));
                groupResults.Add(new GroupResult(gr12));
                //group 1 have In progress and planned credits
                groupResults[0].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[0].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[0].Explanations.Add(GroupExplanation.Satisfied);
                //group 2 have all completed
                groupResults[1].CompletionStatus = CompletionStatus.Completed;
                groupResults[1].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[1].Explanations.Add(GroupExplanation.Satisfied);
                //group 3 all in progress
                groupResults[2].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[2].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[2].Explanations.Add(GroupExplanation.Satisfied);
                //group 4 not started
                groupResults[3].CompletionStatus = CompletionStatus.NotStarted;
                groupResults[3].PlanningStatus = PlanningStatus.NotPlanned;
                //group 5 plan and completed and not-started
                groupResults[4].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[4].PlanningStatus = PlanningStatus.PartiallyPlanned;
                groupResults[4].Explanations.Add(GroupExplanation.MinCourses);
                //group 6 plan completed in-progress 
                groupResults[5].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[5].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[5].Explanations.Add(GroupExplanation.PlannedSatisfied);
                //group 7 plan completed in-progress not-started
                groupResults[6].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[6].PlanningStatus = PlanningStatus.PartiallyPlanned;
                groupResults[6].Explanations.Add(GroupExplanation.MinCoursesPerDepartment);
                //group 8 completed
                groupResults[7].CompletionStatus = CompletionStatus.Completed;
                groupResults[7].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[7].Explanations.Add(GroupExplanation.Satisfied);
                //group 9 all planned
                groupResults[8].CompletionStatus = CompletionStatus.NotStarted;
                groupResults[8].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[8].Explanations.Add(GroupExplanation.PlannedSatisfied);
                //group 10 cpmpleted plan
                groupResults[9].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[9].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[9].Explanations.Add(GroupExplanation.MinCoursesPerDepartment);
                //group 11 not started not planned
                groupResults[10].CompletionStatus = CompletionStatus.NotStarted;
                groupResults[10].PlanningStatus = PlanningStatus.PartiallyPlanned;
                groupResults[10].Explanations.Add(GroupExplanation.MinCoursesPerDepartment);
                //group 12 completed in-progress
                groupResults[11].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[11].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[11].Explanations.Add(GroupExplanation.PlannedSatisfied);
                object[] parameters = { groupResults, groups, 3 };
                var retVal = ((IEnumerable<string>)methodInfo.Invoke(subReq, parameters)).ToList();
                Assert.IsNotNull(retVal);
                Assert.AreEqual(retVal.Count(), 9);
                Assert.IsFalse(retVal.Contains("G2"));
                Assert.IsFalse(retVal.Contains("G8"));
                Assert.IsFalse(retVal.Contains("G1"));

                Assert.IsTrue(retVal.Contains("G3"));
                Assert.IsTrue(retVal.Contains("G6"));
                Assert.IsTrue(retVal.Contains("G10"));
                Assert.IsTrue(retVal.Contains("G12"));
                Assert.IsTrue(retVal.Contains("G9"));
                Assert.IsTrue(retVal.Contains("G5"));
                Assert.IsTrue(retVal.Contains("G7"));
                Assert.IsTrue(retVal.Contains("G11"));
                Assert.IsTrue(retVal.Contains("G4"));
            }
            [TestMethod]
            public void Filter_WithCompletedAndInProgressAndPlannedGroupResults_Take_4()
            {
                Subrequirement subReq = new Subrequirement("SREQ-1", "sub-req-multigroup.bb");
                List<GroupResult> groupResults = new List<GroupResult>();
                Group gr1 = new Group("G1", "group 1", subReq);
                Group gr2 = new Group("G2", "group 2", subReq);
                Group gr3 = new Group("G3", "group 3", subReq);
                Group gr4 = new Group("G4", "group 4", subReq);
                Group gr5 = new Group("G5", "group 5", subReq);
                Group gr6 = new Group("G6", "group 6", subReq);
                Group gr7 = new Group("G7", "group 7", subReq);
                Group gr8 = new Group("G8", "group 8", subReq);
                Group gr9 = new Group("G9", "group 9", subReq);
                Group gr10 = new Group("G10", "group 10", subReq);
                Group gr11 = new Group("G11", "group 11", subReq);
                Group gr12 = new Group("G12", "group 12", subReq);
                groupResults.Add(new GroupResult(gr1));
                groupResults.Add(new GroupResult(gr2));
                groupResults.Add(new GroupResult(gr3));
                groupResults.Add(new GroupResult(gr4));
                groupResults.Add(new GroupResult(gr5));
                groupResults.Add(new GroupResult(gr6));
                groupResults.Add(new GroupResult(gr7));
                groupResults.Add(new GroupResult(gr8));
                groupResults.Add(new GroupResult(gr9));
                groupResults.Add(new GroupResult(gr10));
                groupResults.Add(new GroupResult(gr11));
                groupResults.Add(new GroupResult(gr12));
                //group 1 have In progress and planned credits
                groupResults[0].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[0].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[0].Explanations.Add(GroupExplanation.Satisfied);
                //group 2 have all completed
                groupResults[1].CompletionStatus = CompletionStatus.Completed;
                groupResults[1].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[1].Explanations.Add(GroupExplanation.Satisfied);
                //group 3 all in progress
                groupResults[2].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[2].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[2].Explanations.Add(GroupExplanation.Satisfied);
                //group 4 not started
                groupResults[3].CompletionStatus = CompletionStatus.NotStarted;
                groupResults[3].PlanningStatus = PlanningStatus.NotPlanned;
                //group 5 plan and completed and not-started
                groupResults[4].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[4].PlanningStatus = PlanningStatus.PartiallyPlanned;
                groupResults[4].Explanations.Add(GroupExplanation.MinCourses);
                //group 6 plan completed in-progress 
                groupResults[5].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[5].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[5].Explanations.Add(GroupExplanation.PlannedSatisfied);
                //group 7 plan completed in-progress not-started
                groupResults[6].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[6].PlanningStatus = PlanningStatus.PartiallyPlanned;
                groupResults[6].Explanations.Add(GroupExplanation.MinCoursesPerDepartment);
                //group 8 completed
                groupResults[7].CompletionStatus = CompletionStatus.Completed;
                groupResults[7].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[7].Explanations.Add(GroupExplanation.Satisfied);
                //group 9 all planned
                groupResults[8].CompletionStatus = CompletionStatus.NotStarted;
                groupResults[8].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[8].Explanations.Add(GroupExplanation.PlannedSatisfied);
                //group 10 cpmpleted plan
                groupResults[9].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[9].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[9].Explanations.Add(GroupExplanation.MinCoursesPerDepartment);
                //group 11 partially planned
                groupResults[10].CompletionStatus = CompletionStatus.NotStarted;
                groupResults[10].PlanningStatus = PlanningStatus.PartiallyPlanned;
                groupResults[10].Explanations.Add(GroupExplanation.MinCoursesPerDepartment);
                //group 12 completed in-progress
                groupResults[11].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[11].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[11].Explanations.Add(GroupExplanation.PlannedSatisfied);
                object[] parameters = { groupResults, groups, 4 };
                var retVal = ((IEnumerable<string>)methodInfo.Invoke(subReq, parameters)).ToList();
                Assert.IsNotNull(retVal);
                Assert.AreEqual(retVal.Count(), 8);
                Assert.IsFalse(retVal.Contains("G2"));
                Assert.IsFalse(retVal.Contains("G8"));
                Assert.IsFalse(retVal.Contains("G1"));
                Assert.IsFalse(retVal.Contains("G3"));

                Assert.IsTrue(retVal.Contains("G6"));
                Assert.IsTrue(retVal.Contains("G10"));
                Assert.IsTrue(retVal.Contains("G12"));
                Assert.IsTrue(retVal.Contains("G9"));
                Assert.IsTrue(retVal.Contains("G5"));
                Assert.IsTrue(retVal.Contains("G7"));
                Assert.IsTrue(retVal.Contains("G11"));
                Assert.IsTrue(retVal.Contains("G4"));

            }
            [TestMethod]
            public void Filter_WithCompletedAndInProgressAndPlannedGroupResults_Take_5()
            {
                Subrequirement subReq = new Subrequirement("SREQ-1", "sub-req-multigroup.bb");
                List<GroupResult> groupResults = new List<GroupResult>();
                Group gr1 = new Group("G1", "group 1", subReq);
                Group gr2 = new Group("G2", "group 2", subReq);
                Group gr3 = new Group("G3", "group 3", subReq);
                Group gr4 = new Group("G4", "group 4", subReq);
                Group gr5 = new Group("G5", "group 5", subReq);
                Group gr6 = new Group("G6", "group 6", subReq);
                Group gr7 = new Group("G7", "group 7", subReq);
                Group gr8 = new Group("G8", "group 8", subReq);
                Group gr9 = new Group("G9", "group 9", subReq);
                Group gr10 = new Group("G10", "group 10", subReq);
                Group gr11 = new Group("G11", "group 11", subReq);
                Group gr12 = new Group("G12", "group 12", subReq);
                groupResults.Add(new GroupResult(gr1));
                groupResults.Add(new GroupResult(gr2));
                groupResults.Add(new GroupResult(gr3));
                groupResults.Add(new GroupResult(gr4));
                groupResults.Add(new GroupResult(gr5));
                groupResults.Add(new GroupResult(gr6));
                groupResults.Add(new GroupResult(gr7));
                groupResults.Add(new GroupResult(gr8));
                groupResults.Add(new GroupResult(gr9));
                groupResults.Add(new GroupResult(gr10));
                groupResults.Add(new GroupResult(gr11));
                groupResults.Add(new GroupResult(gr12));
                //group 1 have In progress and planned credits
                groupResults[0].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[0].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[0].Explanations.Add(GroupExplanation.Satisfied);
                //group 2 have all completed
                groupResults[1].CompletionStatus = CompletionStatus.Completed;
                groupResults[1].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[1].Explanations.Add(GroupExplanation.Satisfied);
                //group 3 all in progress
                groupResults[2].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[2].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[2].Explanations.Add(GroupExplanation.Satisfied);
                //group 4 not started
                groupResults[3].CompletionStatus = CompletionStatus.NotStarted;
                groupResults[3].PlanningStatus = PlanningStatus.NotPlanned;
                //group 5 plan and completed and not-started
                groupResults[4].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[4].PlanningStatus = PlanningStatus.PartiallyPlanned;
                groupResults[4].Explanations.Add(GroupExplanation.MinCourses);
                //group 6 plan completed in-progress 
                groupResults[5].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[5].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[5].Explanations.Add(GroupExplanation.PlannedSatisfied);
                //group 7 plan completed in-progress not-started
                groupResults[6].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[6].PlanningStatus = PlanningStatus.PartiallyPlanned;
                groupResults[6].Explanations.Add(GroupExplanation.MinCoursesPerDepartment);
                //group 8 completed
                groupResults[7].CompletionStatus = CompletionStatus.Completed;
                groupResults[7].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[7].Explanations.Add(GroupExplanation.Satisfied);
                //group 9 all planned
                groupResults[8].CompletionStatus = CompletionStatus.NotStarted;
                groupResults[8].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[8].Explanations.Add(GroupExplanation.PlannedSatisfied);
                //group 10 cpmpleted plan
                groupResults[9].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[9].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[9].Explanations.Add(GroupExplanation.MinCoursesPerDepartment);
                //group 11 waived
                groupResults[10].CompletionStatus = CompletionStatus.NotStarted;
                groupResults[10].PlanningStatus = PlanningStatus.PartiallyPlanned;
                groupResults[10].Explanations.Add(GroupExplanation.MinCoursesPerDepartment);
                //group 12 completed in-progress
                groupResults[11].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[11].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[11].Explanations.Add(GroupExplanation.PlannedSatisfied);
                object[] parameters = { groupResults, groups, 5 };
                var retVal = ((IEnumerable<string>)methodInfo.Invoke(subReq, parameters)).ToList();
                Assert.IsNotNull(retVal);
                Assert.AreEqual(retVal.Count(), 7);
                Assert.IsFalse(retVal.Contains("G2"));
                Assert.IsFalse(retVal.Contains("G8"));
                Assert.IsFalse(retVal.Contains("G1"));
                Assert.IsFalse(retVal.Contains("G3"));
                Assert.IsFalse(retVal.Contains("G6"));

                Assert.IsTrue(retVal.Contains("G10"));
                Assert.IsTrue(retVal.Contains("G12"));
                Assert.IsTrue(retVal.Contains("G9"));
                Assert.IsTrue(retVal.Contains("G5"));
                Assert.IsTrue(retVal.Contains("G7"));
                Assert.IsTrue(retVal.Contains("G11"));
                Assert.IsTrue(retVal.Contains("G4"));

            }

            [TestMethod]
            public void Filter_WithCompletedAndInProgressAndPlannedGroupResults_Take_6()
            {
                Subrequirement subReq = new Subrequirement("SREQ-1", "sub-req-multigroup.bb");
                List<GroupResult> groupResults = new List<GroupResult>();
                Group gr1 = new Group("G1", "group 1", subReq);
                Group gr2 = new Group("G2", "group 2", subReq);
                Group gr3 = new Group("G3", "group 3", subReq);
                Group gr4 = new Group("G4", "group 4", subReq);
                Group gr5 = new Group("G5", "group 5", subReq);
                Group gr6 = new Group("G6", "group 6", subReq);
                Group gr7 = new Group("G7", "group 7", subReq);
                Group gr8 = new Group("G8", "group 8", subReq);
                Group gr9 = new Group("G9", "group 9", subReq);
                Group gr10 = new Group("G10", "group 10", subReq);
                Group gr11 = new Group("G11", "group 11", subReq);
                Group gr12 = new Group("G12", "group 12", subReq);
                groupResults.Add(new GroupResult(gr1));
                groupResults.Add(new GroupResult(gr2));
                groupResults.Add(new GroupResult(gr3));
                groupResults.Add(new GroupResult(gr4));
                groupResults.Add(new GroupResult(gr5));
                groupResults.Add(new GroupResult(gr6));
                groupResults.Add(new GroupResult(gr7));
                groupResults.Add(new GroupResult(gr8));
                groupResults.Add(new GroupResult(gr9));
                groupResults.Add(new GroupResult(gr10));
                groupResults.Add(new GroupResult(gr11));
                groupResults.Add(new GroupResult(gr12));
                //group 1 have In progress and planned credits
                groupResults[0].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[0].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[0].Explanations.Add(GroupExplanation.Satisfied);
                //group 2 have all completed
                groupResults[1].CompletionStatus = CompletionStatus.Completed;
                groupResults[1].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[1].Explanations.Add(GroupExplanation.Satisfied);
                //group 3 all in progress
                groupResults[2].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[2].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[2].Explanations.Add(GroupExplanation.Satisfied);
                //group 4 not started
                groupResults[3].CompletionStatus = CompletionStatus.NotStarted;
                groupResults[3].PlanningStatus = PlanningStatus.NotPlanned;
                //group 5 plan and completed and not-started
                groupResults[4].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[4].PlanningStatus = PlanningStatus.PartiallyPlanned;
                groupResults[4].Explanations.Add(GroupExplanation.MinCourses);
                //group 6 plan completed in-progress 
                groupResults[5].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[5].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[5].Explanations.Add(GroupExplanation.PlannedSatisfied);
                //group 7 plan completed in-progress not-started
                groupResults[6].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[6].PlanningStatus = PlanningStatus.PartiallyPlanned;
                groupResults[6].Explanations.Add(GroupExplanation.MinCoursesPerDepartment);
                //group 8 completed
                groupResults[7].CompletionStatus = CompletionStatus.Completed;
                groupResults[7].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[7].Explanations.Add(GroupExplanation.Satisfied);
                //group 9 all planned
                groupResults[8].CompletionStatus = CompletionStatus.NotStarted;
                groupResults[8].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[8].Explanations.Add(GroupExplanation.PlannedSatisfied);
                //group 10 cpmpleted plan
                groupResults[9].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[9].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[9].Explanations.Add(GroupExplanation.MinCoursesPerDepartment);
                //group 11 partially planned
                groupResults[10].CompletionStatus = CompletionStatus.NotStarted;
                groupResults[10].PlanningStatus = PlanningStatus.PartiallyPlanned;
                groupResults[10].Explanations.Add(GroupExplanation.MinCoursesPerDepartment);
                //group 12 completed in-progress
                groupResults[11].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[11].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[11].Explanations.Add(GroupExplanation.PlannedSatisfied);
                object[] parameters = { groupResults, groups, 6 };
                var retVal = ((IEnumerable<string>)methodInfo.Invoke(subReq, parameters)).ToList();
                Assert.IsNotNull(retVal);
                Assert.AreEqual(retVal.Count(), 6);
                Assert.IsFalse(retVal.Contains("G2"));
                Assert.IsFalse(retVal.Contains("G8"));
                Assert.IsFalse(retVal.Contains("G1"));
                Assert.IsFalse(retVal.Contains("G3"));
                Assert.IsFalse(retVal.Contains("G6"));
                Assert.IsFalse(retVal.Contains("G10"));

                Assert.IsTrue(retVal.Contains("G12"));
                Assert.IsTrue(retVal.Contains("G9"));
                Assert.IsTrue(retVal.Contains("G5"));
                Assert.IsTrue(retVal.Contains("G7"));
                Assert.IsTrue(retVal.Contains("G11"));
                Assert.IsTrue(retVal.Contains("G4"));


            }

            [TestMethod]
            public void Filter_WithCompletedAndInProgressAndPlannedGroupResults_Take_7()
            {
                Subrequirement subReq = new Subrequirement("SREQ-1", "sub-req-multigroup.bb");
                List<GroupResult> groupResults = new List<GroupResult>();
                Group gr1 = new Group("G1", "group 1", subReq);
                Group gr2 = new Group("G2", "group 2", subReq);
                Group gr3 = new Group("G3", "group 3", subReq);
                Group gr4 = new Group("G4", "group 4", subReq);
                Group gr5 = new Group("G5", "group 5", subReq);
                Group gr6 = new Group("G6", "group 6", subReq);
                Group gr7 = new Group("G7", "group 7", subReq);
                Group gr8 = new Group("G8", "group 8", subReq);
                Group gr9 = new Group("G9", "group 9", subReq);
                Group gr10 = new Group("G10", "group 10", subReq);
                Group gr11 = new Group("G11", "group 11", subReq);
                Group gr12 = new Group("G12", "group 12", subReq);
                groupResults.Add(new GroupResult(gr1));
                groupResults.Add(new GroupResult(gr2));
                groupResults.Add(new GroupResult(gr3));
                groupResults.Add(new GroupResult(gr4));
                groupResults.Add(new GroupResult(gr5));
                groupResults.Add(new GroupResult(gr6));
                groupResults.Add(new GroupResult(gr7));
                groupResults.Add(new GroupResult(gr8));
                groupResults.Add(new GroupResult(gr9));
                groupResults.Add(new GroupResult(gr10));
                groupResults.Add(new GroupResult(gr11));
                groupResults.Add(new GroupResult(gr12));
                //group 1 have In progress and planned credits
                groupResults[0].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[0].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[0].Explanations.Add(GroupExplanation.Satisfied);
                //group 2 have all completed
                groupResults[1].CompletionStatus = CompletionStatus.Completed;
                groupResults[1].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[1].Explanations.Add(GroupExplanation.Satisfied);
                //group 3 all in progress
                groupResults[2].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[2].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[2].Explanations.Add(GroupExplanation.Satisfied);
                //group 4 not started
                groupResults[3].CompletionStatus = CompletionStatus.NotStarted;
                groupResults[3].PlanningStatus = PlanningStatus.NotPlanned;
                //group 5 plan and completed and not-started
                groupResults[4].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[4].PlanningStatus = PlanningStatus.PartiallyPlanned;
                groupResults[4].Explanations.Add(GroupExplanation.MinCourses);
                //group 6 plan completed in-progress 
                groupResults[5].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[5].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[5].Explanations.Add(GroupExplanation.PlannedSatisfied);
                //group 7 plan completed in-progress not-started
                groupResults[6].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[6].PlanningStatus = PlanningStatus.PartiallyPlanned;
                groupResults[6].Explanations.Add(GroupExplanation.MinCoursesPerDepartment);
                //group 8 completed
                groupResults[7].CompletionStatus = CompletionStatus.Completed;
                groupResults[7].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[7].Explanations.Add(GroupExplanation.Satisfied);
                //group 9 all planned
                groupResults[8].CompletionStatus = CompletionStatus.NotStarted;
                groupResults[8].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[8].Explanations.Add(GroupExplanation.PlannedSatisfied);
                //group 10 cpmpleted plan
                groupResults[9].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[9].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[9].Explanations.Add(GroupExplanation.MinCoursesPerDepartment);
                //group 11 partially planned
                groupResults[10].CompletionStatus = CompletionStatus.NotStarted;
                groupResults[10].PlanningStatus = PlanningStatus.PartiallyPlanned;
                groupResults[10].Explanations.Add(GroupExplanation.MinCoursesPerDepartment);
                //group 12 completed in-progress
                groupResults[11].CompletionStatus = CompletionStatus.PartiallyCompleted;
                groupResults[11].PlanningStatus = PlanningStatus.CompletelyPlanned;
                groupResults[11].Explanations.Add(GroupExplanation.PlannedSatisfied);
                object[] parameters = { groupResults, groups, 7 };
                var retVal = ((IEnumerable<string>)methodInfo.Invoke(subReq, parameters)).ToList();
                Assert.IsNotNull(retVal);
                Assert.AreEqual(retVal.Count(), 5);
                Assert.IsFalse(retVal.Contains("G2"));
                Assert.IsFalse(retVal.Contains("G8"));
                Assert.IsFalse(retVal.Contains("G1"));
                Assert.IsFalse(retVal.Contains("G3"));
                Assert.IsFalse(retVal.Contains("G6"));
                Assert.IsFalse(retVal.Contains("G10"));
                Assert.IsFalse(retVal.Contains("G12"));

                Assert.IsTrue(retVal.Contains("G9"));
                Assert.IsTrue(retVal.Contains("G5"));
                Assert.IsTrue(retVal.Contains("G7"));
                Assert.IsTrue(retVal.Contains("G11"));
                Assert.IsTrue(retVal.Contains("G4"));


            }
        }


        [TestClass]
        public class ClearGroupResultsPrivateMethodTests
        {
            Subrequirement subReq = null;
            MethodInfo methodInfo = null;
            List<GroupResult> groupResults = new List<GroupResult>();



            [TestInitialize]
            public void Initialize()
            {
                subReq = new Subrequirement("SREQ-1", "sub-req-multigroup.bb");
                methodInfo = typeof(Subrequirement).GetMethod("ClearGroupResults", BindingFlags.NonPublic | BindingFlags.Instance);

                Group gr1 = new Group("G1", "group 1", subReq);
                gr1.ExtraCourseDirective = ExtraCourses.Apply;



                Group gr2 = new Group("G2", "group 2", subReq);
                gr2.ExtraCourseDirective = ExtraCourses.SemiApply;

                Group gr3 = new Group("G3", "group 3", subReq);
                gr3.ExtraCourseDirective = ExtraCourses.Display;

                Group gr4 = new Group("G4", "group 4", subReq);
                gr4.ExtraCourseDirective = ExtraCourses.Ignore;

               
               


                GroupResult grResult1 = new GroupResult(gr1);
                grResult1.Results = new List<AcadResult>();
                grResult1.Results.Add(new CreditResult(new AcademicCredit("111")) {Result=Result.Applied, Explanation=AcadResultExplanation.Extra });
                grResult1.Results.Add(new CreditResult(new AcademicCredit("112")) { Result = Result.Applied, Explanation = AcadResultExplanation.None });
                grResult1.Results.Add(new CreditResult(new AcademicCredit("111")) { Result = Result.PlannedApplied, Explanation = AcadResultExplanation.None });

                groupResults.Add(grResult1);

                GroupResult grResult2 = new GroupResult(gr2);
                grResult2.Results = new List<AcadResult>();
                grResult2.Results.Add(new CreditResult(new AcademicCredit("111")) { Result = Result.Applied, Explanation = AcadResultExplanation.Extra });
                grResult2.Results.Add(new CreditResult(new AcademicCredit("112")) { Result = Result.Applied, Explanation = AcadResultExplanation.None });
                grResult2.Results.Add(new CreditResult(new AcademicCredit("111")) { Result = Result.PlannedApplied, Explanation = AcadResultExplanation.None });

                groupResults.Add(grResult2);

                GroupResult grResult3 = new GroupResult(gr3);
                grResult3.Results = new List<AcadResult>();
                grResult3.Results.Add(new CreditResult(new AcademicCredit("111")) { Result = Result.Applied, Explanation = AcadResultExplanation.None });
                grResult3.Results.Add(new CreditResult(new AcademicCredit("112")) { Result = Result.NotInCoursesList, Explanation = AcadResultExplanation.None });
                grResult3.Results.Add(new CreditResult(new AcademicCredit("111")) { Result = Result.PlannedApplied, Explanation = AcadResultExplanation.None });

                groupResults.Add(grResult3);

                GroupResult grResult4 = new GroupResult(gr4);
                grResult4.Results = new List<AcadResult>();
                grResult4.Results.Add(new CreditResult(new AcademicCredit("111")) { Result = Result.Applied, Explanation = AcadResultExplanation.None });
                grResult4.Results.Add(new CreditResult(new AcademicCredit("112")) { Result = Result.NotInCoursesList, Explanation = AcadResultExplanation.None });
                grResult4.Results.Add(new CreditResult(new AcademicCredit("111")) { Result = Result.PlannedApplied, Explanation = AcadResultExplanation.None });

                groupResults.Add(grResult4);

             

            }

            [TestMethod]
            public void clearGroupResults_ParamsAreNull()
            {

                object[] parameters = { null, null, null };
                var retVal = (IEnumerable<string>)methodInfo.Invoke(subReq, parameters);
            }
            [TestMethod]

            public void clearGroupResults_ParamsAreEmpty()
            {

                object[] parameters = { new List<GroupResult>(), null, null };
                var retVal = (IEnumerable<string>)methodInfo.Invoke(subReq, parameters);
            }
            [TestMethod]

            public void clearGroupResult_VerifyStatuses()
            {
                object[] parameters = { groupResults, null, null };
                var retVal = (IEnumerable<string>)methodInfo.Invoke(subReq, parameters);

                //first group result
                Assert.AreEqual(groupResults[0].MinGroupStatus, GroupResultMinGroupStatus.Extra);

                Assert.AreEqual(groupResults[0].Results[0].Explanation, AcadResultExplanation.Extra);
                Assert.AreEqual(groupResults[0].Results[0].Result,Result.Applied);
                Assert.AreEqual(groupResults[0].Results[1].Explanation, AcadResultExplanation.ExtraInGroup);
                Assert.AreEqual(groupResults[0].Results[1].Result, Result.Applied);
                Assert.AreEqual(groupResults[0].Results[2].Explanation, AcadResultExplanation.ExtraInGroup);
                Assert.AreEqual(groupResults[0].Results[2].Result, Result.PlannedApplied);

                //second group result
                Assert.AreEqual(groupResults[1].MinGroupStatus, GroupResultMinGroupStatus.Extra);

                Assert.AreEqual(groupResults[1].Results[0].Explanation, AcadResultExplanation.Extra);
                Assert.AreEqual(groupResults[1].Results[0].Result, Result.Applied);
                Assert.AreEqual(groupResults[1].Results[1].Explanation, AcadResultExplanation.ExtraInGroup);
                Assert.AreEqual(groupResults[1].Results[1].Result, Result.Applied);
                Assert.AreEqual(groupResults[1].Results[2].Explanation, AcadResultExplanation.ExtraInGroup);
                Assert.AreEqual(groupResults[1].Results[2].Result, Result.PlannedApplied);

                //third group result
                Assert.AreEqual(groupResults[2].MinGroupStatus, GroupResultMinGroupStatus.None);
                Assert.AreEqual(groupResults[2].CompletionStatus, CompletionStatus.NotStarted);
                Assert.AreEqual(groupResults[2].PlanningStatus, PlanningStatus.NotPlanned);

                Assert.AreEqual(groupResults[2].Results[0].Explanation, AcadResultExplanation.None);
                Assert.AreEqual(groupResults[2].Results[0].Result, Result.Related);
                Assert.AreEqual(groupResults[2].Results[1].Explanation, AcadResultExplanation.None);
                Assert.AreEqual(groupResults[2].Results[1].Result, Result.NotInCoursesList);
                Assert.AreEqual(groupResults[2].Results[2].Explanation, AcadResultExplanation.None);
                Assert.AreEqual(groupResults[2].Results[2].Result, Result.Related);

                //fourth group result
                Assert.AreEqual(groupResults[3].MinGroupStatus, GroupResultMinGroupStatus.Ignore);
                Assert.AreEqual(groupResults[3].CompletionStatus, CompletionStatus.NotStarted);
                Assert.AreEqual(groupResults[3].PlanningStatus, PlanningStatus.NotPlanned);


                Assert.AreEqual(groupResults[3].Results[0].Explanation, AcadResultExplanation.None);
                Assert.AreEqual(groupResults[3].Results[0].Result, Result.Related);
                Assert.AreEqual(groupResults[3].Results[1].Explanation, AcadResultExplanation.None);
                Assert.AreEqual(groupResults[3].Results[1].Result, Result.NotInCoursesList);
                Assert.AreEqual(groupResults[3].Results[2].Explanation, AcadResultExplanation.None);
                Assert.AreEqual(groupResults[3].Results[2].Result, Result.Related);




            }
        }

        [TestClass]
        public class OnlyConveysPrintText
        {
            private string id;
            private string code;
            private Subrequirement subrequirement;

            [TestInitialize]
            public void Initialize_OnlyConveysPrintText()
            {
                id = "1";
                code = "code";
                subrequirement = new Subrequirement(id + "S", code + "S");
            }

            [TestMethod]
            public void Subrequirement_OnlyConveysPrintText_True_All_Groups_OnlyConveyPrintText_True()
            {
                subrequirement.Groups.Add(new Group("g", "g", subrequirement) { MinCredits = 0m, GroupType = GroupType.TakeCredits });
                Assert.IsTrue(subrequirement.OnlyConveysPrintText);
            }

            [TestMethod]
            public void Subrequirement_OnlyConveysPrintText_False_at_least_one_Group_OnlyConveyPrintText_False()
            {
                subrequirement.Groups.Add(new Group("g", "g", subrequirement) { MaxCredits = 0m });
                Assert.IsFalse(subrequirement.OnlyConveysPrintText);
            }

            [TestMethod]
            public void Subrequirement_OnlyConveysPrintText_False_null_Groups()
            {
                subrequirement.Groups = null;
                Assert.IsFalse(subrequirement.OnlyConveysPrintText);
            }

            [TestMethod]
            public void Subrequirement_OnlyConveysPrintText_False_null_Group_in_groups_filtered()
            {
                subrequirement.Groups = new List<Group>() { null, new Group("g", "g", subrequirement) { MaxCredits = 0m } };
                Assert.IsFalse(subrequirement.OnlyConveysPrintText);
            }

        }
    }
}
