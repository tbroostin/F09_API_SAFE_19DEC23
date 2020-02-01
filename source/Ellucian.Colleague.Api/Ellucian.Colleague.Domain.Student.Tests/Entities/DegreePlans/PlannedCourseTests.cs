// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.DegreePlans
{
    [TestClass]
    public class PlannedCourseTests
    {
        [TestClass]
        public class PlannedCourseConstructor
        {
            string courseId;
            string sectionId;
            GradingType gradingType;
            Student.Entities.DegreePlans.WaitlistStatus waitlistStatus;
            string addedBy;
            DateTime? addedOn;

            [TestInitialize]
            public void Initialize()
            {
                courseId = "course";
                sectionId = "section";
                gradingType = GradingType.Audit; // something other than default "Graded"
                waitlistStatus = Student.Entities.DegreePlans.WaitlistStatus.Active; // something other than default "NotWaitlisted"
                addedBy = "somebody"; // something other than default "null"
                addedOn = DateTime.Now; // something other than default "null"
            }

            [TestMethod]
            public void PlannedCourseId()
            {
                var p = new Domain.Student.Entities.DegreePlans.PlannedCourse(courseId);
                Assert.AreEqual(courseId, p.CourseId);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PlannedCourseNullId()
            {
                var p = new Student.Entities.DegreePlans.PlannedCourse(null);
            }

            [TestMethod]
            public void PlannedCourseNullSection()
            {
                var p = new Student.Entities.DegreePlans.PlannedCourse(courseId, null);
                Assert.IsNull(p.SectionId);
            }

            [TestMethod]
            public void PlannedCourseNoSection()
            {
                var p = new Student.Entities.DegreePlans.PlannedCourse(courseId);
                Assert.IsNull(p.SectionId);
            }

            [TestMethod]
            public void PlannedCourseOtherProperties()
            {
                var p = new Student.Entities.DegreePlans.PlannedCourse(courseId, sectionId, gradingType, waitlistStatus, addedBy, addedOn);

                Assert.AreEqual(sectionId, p.SectionId);
                Assert.AreEqual(gradingType, p.GradingType);
                Assert.AreEqual(waitlistStatus, p.WaitlistedStatus);
                Assert.AreEqual(addedBy, p.AddedBy);
                Assert.AreEqual(addedOn, p.AddedOn);
            }

            [TestMethod]
            public void PlannedCourseDefaultedProperties()
            {
                var p = new Student.Entities.DegreePlans.PlannedCourse(courseId, sectionId, gradingType, waitlistStatus);

                Assert.AreEqual(sectionId, p.SectionId);
                Assert.AreEqual(null, p.AddedBy);
                Assert.AreEqual(null, p.AddedOn);
            }
        }
        [TestClass]
        public class PlannedCourseOptionalProperties
        {
            Student.Entities.DegreePlans.PlannedCourse pc; 

            [TestInitialize]
            public void Initialize()
            {

                pc = new Student.Entities.DegreePlans.PlannedCourse("course", "section", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, "somebody", DateTime.Today);
            }

            [TestMethod]
            public void PlannedCourse_IsProtectedNull()
            {
                Assert.IsNull(pc.IsProtected);
            }

            [TestMethod]
            public void PlannedCourse_IsProtectedFalse()
            {
                pc.IsProtected = false;
                Assert.AreEqual(false, pc.IsProtected);
            }

            [TestMethod]
            public void PlannedCourse_IsProtectedTrue()
            {
                pc.IsProtected = true;
                Assert.AreEqual(true, pc.IsProtected);
            }
        }

        [TestClass]
        public class PlannedCourseAddWarning
        {
            Student.Entities.DegreePlans.PlannedCourse plannedCourse;
            Student.Entities.DegreePlans.PlannedCourseWarning warning1;
            Student.Entities.DegreePlans.PlannedCourseWarning warning2;
            Student.Entities.DegreePlans.PlannedCourseWarning warning3;
            
            [TestInitialize]
            public void Initialize()
            {
                plannedCourse = new Student.Entities.DegreePlans.PlannedCourse("0001");
                Requisite req1 = new Requisite("0002", false);
                SectionRequisite req2 = new SectionRequisite("111");
                SectionRequisite req3 = new SectionRequisite("222", true);
                warning1 = new Student.Entities.DegreePlans.PlannedCourseWarning(Student.Entities.DegreePlans.PlannedCourseWarningType.UnmetRequisite) { Requisite = req1 };
                warning2 = new Student.Entities.DegreePlans.PlannedCourseWarning(Student.Entities.DegreePlans.PlannedCourseWarningType.UnmetRequisite) { SectionRequisite = req2 };
                warning3 = new Student.Entities.DegreePlans.PlannedCourseWarning(Student.Entities.DegreePlans.PlannedCourseWarningType.UnmetRequisite) { SectionRequisite = req3 };
                plannedCourse.AddWarning(warning1);
                plannedCourse.AddWarning(warning2);
                plannedCourse.AddWarning(warning3);
            }

            [TestMethod]
            public void AddWarning()
            {
                Assert.AreEqual(warning1, plannedCourse.Warnings.ElementAt(0));
                Assert.AreEqual(warning2, plannedCourse.Warnings.ElementAt(1));
                Assert.AreEqual(warning3, plannedCourse.Warnings.ElementAt(2));
            }

            [TestMethod]
            public void AddNullWarningDoesNothing()
            {
                var oldCount = plannedCourse.Warnings.Count();
                plannedCourse.AddWarning(null);
                
                Assert.AreEqual(oldCount, plannedCourse.Warnings.Count());

                Assert.IsTrue(plannedCourse.Warnings.Contains(warning1));
                Assert.IsTrue(plannedCourse.Warnings.Contains(warning2));
                Assert.IsTrue(plannedCourse.Warnings.Contains(warning3));
            }

        }

        [TestClass]
        public class PlannedCourseClearWarnings
        {
            Student.Entities.DegreePlans.PlannedCourse pc;
            Student.Entities.DegreePlans.PlannedCourseWarning warning1;
            Student.Entities.DegreePlans.PlannedCourseWarning warning2;
            Student.Entities.DegreePlans.PlannedCourseWarning warning3;

            [TestInitialize]
            public void Initialize()
            {
                pc = new Student.Entities.DegreePlans.PlannedCourse("0001");
                Requisite req1 = new Requisite("0002", false);
                SectionRequisite req2 = new SectionRequisite("111");
                SectionRequisite req3 = new SectionRequisite("222");
                warning1 = new Student.Entities.DegreePlans.PlannedCourseWarning(Student.Entities.DegreePlans.PlannedCourseWarningType.UnmetRequisite) { Requisite = req1 };
                warning2 = new Student.Entities.DegreePlans.PlannedCourseWarning(Student.Entities.DegreePlans.PlannedCourseWarningType.UnmetRequisite) { SectionRequisite = req2 };
                warning3 = new Student.Entities.DegreePlans.PlannedCourseWarning(Student.Entities.DegreePlans.PlannedCourseWarningType.UnmetRequisite) { SectionRequisite = req3 };
                pc.AddWarning(warning1);
                pc.AddWarning(warning2);
                pc.AddWarning(warning3);
            }

            [TestMethod]
            public void ClearWarnings()
            {
                pc.ClearWarnings();
                Assert.IsTrue(pc.Warnings.Count() == 0);
            }
        }

        [TestClass]
        public class PlannedCourseEquals
        {
            Student.Entities.DegreePlans.PlannedCourse pc1;
            Student.Entities.DegreePlans.PlannedCourse pc2;
            Student.Entities.DegreePlans.PlannedCourse pc3;
            Student.Entities.DegreePlans.PlannedCourse pc4;
            Student.Entities.DegreePlans.PlannedCourse pc5;
            Student.Entities.DegreePlans.PlannedCourse pc6;

            [TestInitialize]
            public void Initialize()
            {
                pc1 = new Student.Entities.DegreePlans.PlannedCourse("0001");
                pc2 = new Student.Entities.DegreePlans.PlannedCourse("0001");
                pc3 = new Student.Entities.DegreePlans.PlannedCourse("0002");

                pc4 = new Student.Entities.DegreePlans.PlannedCourse("0002", "0999");
                pc5 = new Student.Entities.DegreePlans.PlannedCourse("0002", "0999");
                pc6 = new Student.Entities.DegreePlans.PlannedCourse("0003", "0999");
            }

            [TestMethod]
            public void EqualsWithNullSection()
            {
                Assert.IsTrue(pc1.Equals(pc2));
            }

            [TestMethod]
            public void NotEqualsWithNullSection()
            {
                Assert.IsFalse(pc1.Equals(pc3));
            }

            [TestMethod]
            public void EqualsWithSection()
            {
                Assert.IsTrue(pc4.Equals(pc5));
            }

            [TestMethod]
            public void NotEqualsWithSection()
            {
                Assert.IsFalse(pc4.Equals(pc6));
            }

            [TestMethod]
            public void EqualsWithOneSection()
            {
                Assert.IsFalse(pc3.Equals(pc4));
            }


        }

    }
}
