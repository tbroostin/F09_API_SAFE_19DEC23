// Copyright 2013-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Entities.Requirements.Modifications;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.Requirements.Modifications
{
    [TestClass]
    public class BlockReplacementTests
    {
        private ProgramRequirements pr;
        private List<Requirement> requirements;
        private List<Requirement> additionalrequirements;
        private TestProgramRequirementsRepository tprr;
        private TestRequirementRepository trr;
        private TestStudentProgramRepository tspr;
        public Requirement req1;
        public Requirement newreq;
        public Group replacementGroup;

        [TestInitialize]
        public async void Initialize()
        {
            tprr = new TestProgramRequirementsRepository();
            trr = new TestRequirementRepository();
            tspr = new TestStudentProgramRepository();

            pr = tprr.Get("MATHPROG", "2033");
            requirements = (await trr.GetAsync(new List<string>() { })).ToList();  // caveat: this test repo's data doesn't match the others'
            req1 = requirements.First();
            additionalrequirements = new List<Requirement>();

            // Create a new replacement block requirement
            newreq = new Requirement("", "", "", "", null);
            Subrequirement sub = new Subrequirement("", "");

            sub.Requirement = newreq;
            newreq.MinSubRequirements = 1;
            newreq.SubRequirements.Add(sub);
            Group replacementGroup = new Group("Group 1", "Group Code 1", sub);
            replacementGroup.Courses.Add("1100");
            replacementGroup.Courses.Add("1200");

            replacementGroup.MinCourses = 1;
            replacementGroup.GroupType = GroupType.TakeAll;
            replacementGroup.SubRequirement = sub;

            sub.Groups.Add(replacementGroup);
            sub.MinGroups = 1;

        }


        [TestClass]
        public class BlockReplacement_Modify_Tests : BlockReplacementTests
        {
            [TestInitialize]
            public void BlockReplacement_Modify_Initialize()
            {
                base.Initialize();
            }

            [TestMethod]
            public void Constructor()
            {
                BlockReplacement br = new BlockReplacement("999", req1, "");
                Assert.AreEqual("999", br.blockId);
                Assert.AreEqual(req1.Code, br.NewRequirement.Code);

            }
            [TestMethod]
            public void Constructor_allowsNullBlock()
            {
                BlockReplacement br = new BlockReplacement("999", null, "");
                Assert.AreEqual("999", br.blockId);
                Assert.IsNull(br.NewRequirement);
            }

            // Modify() works at subrequirement level with null repl block (QAD 49874)
            [TestMethod]
            public void Modify_ProgramRequirements_SetValue()
            {
                BlockReplacement br = new BlockReplacement("18000", null, "");
                br.Modify(pr, additionalrequirements);
            }

            // Modify() 
            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void Modify_ProgramRequirements_SetValue_ThrowsIfBlockNotFound()
            {
                BlockReplacement br = new BlockReplacement("999", null, "");
                br.Modify(pr, additionalrequirements);
            }

            [TestMethod]
            public void Modify_ProgramRequirements_Waived_Group()
            {
                string id = pr.Requirements.First().SubRequirements.First().Id as string;
                pr.Requirements.First().SubRequirements.First().Id = "ABC";
                pr.Requirements.First().SubRequirements.First().Groups.First().Id = id;
                string msg = "This group has been waived.";
                BlockReplacement br = new BlockReplacement("18000", null, msg);
                br.Modify(pr, additionalrequirements);
                Assert.IsTrue(pr.Requirements.First().SubRequirements.First().Groups.First().IsWaived);
                Assert.AreEqual(1, pr.Requirements.First().SubRequirements.First().Groups.First().ModificationMessages.Count);
                Assert.AreEqual(msg, pr.Requirements.First().SubRequirements.First().Groups.First().ModificationMessages[0]);

            }

            [ExpectedException(typeof(KeyNotFoundException))]
            [TestMethod]
            public void BlockReplacement_Modify_programRequirements_null()
            {
                string msg = "Replacing Requirement with Take courses 1100 and 1200";
                BlockReplacement fca = new BlockReplacement("18000", newreq, msg);
                fca.Modify(null, additionalrequirements);
            }


            [TestMethod]
            public void BlockReplacement_Modify_additionalRequirements_null()
            {

                string msg = "Replacing Requirement with Take courses 1100 and 1200";
                BlockReplacement fca = new BlockReplacement("18000", newreq, msg);
                fca.Modify(pr, null);
                var modifiedSubReq = pr.Requirements.SelectMany(r => r.SubRequirements).Where(sr => sr.Id == "18000").FirstOrDefault();
                Assert.IsNotNull(modifiedSubReq);
                Assert.AreEqual(1, modifiedSubReq.Groups.Count());
                var replacedGroup = modifiedSubReq.Groups.FirstOrDefault();
                Assert.IsNotNull(replacedGroup);
                var rGroup = newreq.SubRequirements.First().Groups.First();
                foreach (var crs in rGroup.Courses)
                {
                    Assert.IsTrue(replacedGroup.Courses.Contains(crs));
                }
            }
        }

        [TestClass]
        public class FromCourseAddition_Tests : BlockReplacementTests
        {
            [TestInitialize]
            public void FromCourseAddition_Initialize()
            {
                base.Initialize();
            }

            [ExpectedException(typeof(NotSupportedException))]
            [TestMethod]
            public void FromCourseAddition_Id_Null()
            {
                string msg = "Take courses 123 and 456";
                List<string> courses = new List<string>() { "123", "456" };
                FromCoursesAddition fca = new FromCoursesAddition(null, courses, msg);
            }

            [TestMethod]
            public void FromCourseAddition_Valid()
            {
                string msg = "Take courses 123 and 456";
                List<string> courses = new List<string>() { "123", "456" };
                FromCoursesAddition fca = new FromCoursesAddition("18000", courses, msg);
                Assert.AreEqual("18000", fca.blockId);
                CollectionAssert.AreEqual(courses, fca.AdditionalFromCourses);
            }
        }

        [TestClass]
        public class FromCourseAddition_Modify_Tests : BlockReplacementTests
        {
            [TestInitialize]
            public void FromCourseAddition_Modify_Initialize()
            {
                base.Initialize();
            }

            [ExpectedException(typeof(KeyNotFoundException))]
            [TestMethod]
            public void FromCourseAddition_Modify_ProgramRequirements_Null()
            {
                string msg = "Take courses 123 and 456";
                List<string> courses = new List<string>() { "123", "456" };
                FromCoursesAddition fca = new FromCoursesAddition("18000", courses, msg);
                fca.Modify(null, additionalrequirements);
            }

            [ExpectedException(typeof(KeyNotFoundException))]
            [TestMethod]
            public void FromCourseAddition_Modify_ProgramRequirements_Requirements_Null()
            {
                pr.Requirements = null;
                string msg = "Take courses 123 and 456";
                List<string> courses = new List<string>() { "123", "456" };
                FromCoursesAddition fca = new FromCoursesAddition("18000", courses, msg);
                fca.Modify(pr, additionalrequirements);
            }

            [TestMethod]
            public void FromCourseAddition_Modify_AdditionalRequirements_Null()
            {
                string id = pr.Requirements.First().SubRequirements.First().Id as string;
                pr.Requirements.First().SubRequirements.First().Id = "ABC";
                pr.Requirements.First().SubRequirements.First().Groups.First().Id = id;
                string msg = "Take courses 123 and 456";
                List<string> courses = new List<string>() { "123", "456" };
                FromCoursesAddition fca = new FromCoursesAddition("18000", courses, msg);
                fca.Modify(pr, null);
                CollectionAssert.AreEqual(new List<string>() { msg }, pr.Requirements.First().SubRequirements.First().Groups.First().ModificationMessages);
                CollectionAssert.AreEqual(courses, pr.Requirements.First().SubRequirements.First().Groups.First().FromCoursesException);
                Assert.IsTrue(pr.Requirements.First().SubRequirements.First().Groups.First().IsModified);
                Assert.IsFalse(pr.Requirements.First().SubRequirements.First().Groups.First().IsWaived);
            }

            [TestMethod]
            public void FromCourseAddition_Modify_ProgramRequirements_Requirements_contains_null()
            {
                string id = pr.Requirements.First().SubRequirements.First().Id as string;
                pr.Requirements.First().SubRequirements.First().Id = "ABC";
                pr.Requirements.First().SubRequirements.First().Groups.First().Id = id;
                pr.Requirements.Add(null);
                string msg = "Take courses 123 and 456";
                List<string> courses = new List<string>() { "123", "456" };
                FromCoursesAddition fca = new FromCoursesAddition("18000", courses, msg);
                fca.Modify(pr, additionalrequirements);
                CollectionAssert.AreEqual(new List<string>() { msg }, pr.Requirements.First().SubRequirements.First().Groups.First().ModificationMessages);
                CollectionAssert.AreEqual(courses, pr.Requirements.First().SubRequirements.First().Groups.First().FromCoursesException);
                Assert.IsTrue(pr.Requirements.First().SubRequirements.First().Groups.First().IsModified);
                Assert.IsFalse(pr.Requirements.First().SubRequirements.First().Groups.First().IsWaived);
            }

            [ExpectedException(typeof(KeyNotFoundException))]
            [TestMethod]
            public void FromCourseAddition_Modify_ProgramRequirements_Requirements_req_Subrequirements_null()
            {
                pr.Requirements.First().SubRequirements = null;
                string msg = "Take courses 123 and 456";
                List<string> courses = new List<string>() { "123", "456" };
                FromCoursesAddition fca = new FromCoursesAddition("18000", courses, msg);
                fca.Modify(pr, additionalrequirements);
            }

            [TestMethod]
            public void FromCourseAddition_Modify_ProgramRequirements_Requirements_req_Subrequirements_contains_null()
            {
                string id = pr.Requirements.First().SubRequirements.First().Id as string;
                pr.Requirements.First().SubRequirements.First().Id = "ABC";
                pr.Requirements.First().SubRequirements.First().Groups.First().Id = id;
                pr.Requirements.First().SubRequirements.Add(null);
                string msg = "Take courses 123 and 456";
                List<string> courses = new List<string>() { "123", "456" };
                FromCoursesAddition fca = new FromCoursesAddition("18000", courses, msg);
                fca.Modify(pr, additionalrequirements);
                CollectionAssert.AreEqual(new List<string>() { msg }, pr.Requirements.First().SubRequirements.First().Groups.First().ModificationMessages);
                CollectionAssert.AreEqual(courses, pr.Requirements.First().SubRequirements.First().Groups.First().FromCoursesException);
                Assert.IsTrue(pr.Requirements.First().SubRequirements.First().Groups.First().IsModified);
                Assert.IsFalse(pr.Requirements.First().SubRequirements.First().Groups.First().IsBlockReplacement);
                Assert.IsFalse(pr.Requirements.First().SubRequirements.First().Groups.First().IsWaived);
            }

            [ExpectedException(typeof(KeyNotFoundException))]
            [TestMethod]
            public void FromCourseAddition_Modify_ProgramRequirements_Requirements_req_Subrequirements_Groups_null()
            {
                pr.Requirements.First().SubRequirements.First().Groups = null;
                string msg = "Take courses 123 and 456";
                List<string> courses = new List<string>() { "123", "456" };
                FromCoursesAddition fca = new FromCoursesAddition("18000", courses, msg);
                fca.Modify(pr, additionalrequirements);
            }

            [TestMethod]
            public void FromCourseAddition_Modify_ProgramRequirements_Requirements_req_Subrequirements_Groups_contains_null()
            {
                string id = pr.Requirements.First().SubRequirements.First().Id as string;
                pr.Requirements.First().SubRequirements.First().Id = "ABC";
                pr.Requirements.First().SubRequirements.First().Groups.First().Id = id;
                pr.Requirements.First().SubRequirements.First().Groups.Add(null);
                string msg = "Take courses 123 and 456";
                List<string> courses = new List<string>() { "123", "456" };
                FromCoursesAddition fca = new FromCoursesAddition("18000", courses, msg);
                fca.Modify(pr, additionalrequirements);
                CollectionAssert.AreEqual(new List<string>() { msg }, pr.Requirements.First().SubRequirements.First().Groups.First().ModificationMessages);
                CollectionAssert.AreEqual(courses, pr.Requirements.First().SubRequirements.First().Groups.First().FromCoursesException);
                Assert.IsTrue(pr.Requirements.First().SubRequirements.First().Groups.First().IsModified);
                Assert.IsFalse(pr.Requirements.First().SubRequirements.First().Groups.First().IsWaived);
            }

            [ExpectedException(typeof(KeyNotFoundException))]
            [TestMethod]
            public void FromCourseAddition_Modify_AdditionalFromCourses_null()
            {
                string id = pr.Requirements.First().SubRequirements.First().Id as string;
                pr.Requirements.First().SubRequirements.First().Id = "ABC";
                pr.Requirements.First().SubRequirements.First().Groups.First().Id = id;
                pr.Requirements.First().SubRequirements.First().Groups.Add(null);
                string msg = "Take courses 123 and 456";
                List<string> courses = null;
                FromCoursesAddition fca = new FromCoursesAddition("18000", courses, msg);
                fca.Modify(pr, additionalrequirements);
            }

            [ExpectedException(typeof(KeyNotFoundException))]
            [TestMethod]
            public void FromCourseAddition_Modify_AdditionalFromCourses_empty()
            {
                string id = pr.Requirements.First().SubRequirements.First().Id as string;
                pr.Requirements.First().SubRequirements.First().Id = "ABC";
                pr.Requirements.First().SubRequirements.First().Groups.First().Id = id;
                pr.Requirements.First().SubRequirements.First().Groups.Add(null);
                string msg = "Take courses 123 and 456";
                List<string> courses = new List<string>();
                FromCoursesAddition fca = new FromCoursesAddition("18000", courses, msg);
                fca.Modify(pr, additionalrequirements);
            }

            [TestMethod]
            public void FromCourseAddition_Modify_ProgramRequirements_FromCoursesAddition_Modify()
            {
                string id = pr.Requirements.First().SubRequirements.First().Id as string;
                pr.Requirements.First().SubRequirements.First().Id = "ABC";
                pr.Requirements.First().SubRequirements.First().Groups.First().Id = id;
                string msg = "Take courses 123 and 456";
                List<string> courses = new List<string>() { "123", "456" };
                FromCoursesAddition fca = new FromCoursesAddition("18000", courses, msg);
                fca.Modify(pr, additionalrequirements);
                CollectionAssert.AreEqual(new List<string>() { msg }, pr.Requirements.First().SubRequirements.First().Groups.First().ModificationMessages);
                CollectionAssert.AreEqual(courses, pr.Requirements.First().SubRequirements.First().Groups.First().FromCoursesException);
                Assert.IsTrue(pr.Requirements.First().SubRequirements.First().Groups.First().IsModified);
                Assert.IsFalse(pr.Requirements.First().SubRequirements.First().Groups.First().IsWaived);
            }

            [TestMethod]
            public void FromCourseAddition_Modify_ProgramRequirements_Group_ModificationMessages_and_FromCoursesException_Null()
            {
                string id = pr.Requirements.First().SubRequirements.First().Id as string;
                pr.Requirements.First().SubRequirements.First().Id = "ABC";
                pr.Requirements.First().SubRequirements.First().Groups.First().Id = id;
                pr.Requirements.First().SubRequirements.First().Groups.First().ModificationMessages = null;
                pr.Requirements.First().SubRequirements.First().Groups.First().FromCoursesException = null;
                string msg = "Take courses 123 and 456";
                List<string> courses = new List<string>() { "123", "456" };
                FromCoursesAddition fca = new FromCoursesAddition("18000", courses, msg);
                fca.Modify(pr, additionalrequirements);
                CollectionAssert.AreEqual(new List<string>() { msg }, pr.Requirements.First().SubRequirements.First().Groups.First().ModificationMessages);
                CollectionAssert.AreEqual(courses, pr.Requirements.First().SubRequirements.First().Groups.First().FromCoursesException);
                Assert.IsTrue(pr.Requirements.First().SubRequirements.First().Groups.First().IsModified);
                Assert.IsFalse(pr.Requirements.First().SubRequirements.First().Groups.First().IsWaived);
            }


        }

        

    }
}
