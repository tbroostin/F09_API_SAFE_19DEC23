//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
     [TestClass]
    public class JobappsTests
    {
        [TestClass]
        public class JobappsConstructor
        {
            private string guid;
            private string personId;
            private JobApplication jobApplications;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                personId = "PID";
                jobApplications = new JobApplication(guid, personId);
            }

            [TestMethod]
            public void JobApplication_PersonId()
            {
                Assert.AreEqual(personId, jobApplications.PersonId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void JobApplication_GuidNullException()
            {
                new JobApplication(null, personId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void JobApplication_PersonIdNullException()
            {
                new JobApplication(guid, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void JobApplicationGuidEmptyException()
            {
                new JobApplication(string.Empty, personId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void JobApplicationPersonIdEmptyException()
            {
                new JobApplication(guid, string.Empty);
            }

        }

        [TestClass]
        public class JobApplication_Equals
        {
            private string guid;
            private string personId;
            private JobApplication jobApplications1;
            private JobApplication jobApplications2;
            private JobApplication jobApplications3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                personId = "PID";
                jobApplications1 = new JobApplication(guid, personId);
                jobApplications2 = new JobApplication(guid, personId);
                jobApplications3 = new JobApplication(Guid.NewGuid().ToString(), "PID2");
            }

            [TestMethod]
            public void JobApplicationSamePersonIdsEqual()
            {
                Assert.IsTrue(jobApplications1.PersonId.Equals(jobApplications2.PersonId));
            }

            [TestMethod]
            public void JobApplicationDifferentPersonIdNotEqual()
            {
                Assert.IsFalse(jobApplications1.PersonId.Equals(jobApplications3.PersonId));
            }
        }

        //[TestClass]
        //public class JobApplication_GetHashCode
        //{
        //    private string guid;
        //    private string personId;
        //    private JobApplication jobApplications1;
        //    private JobApplication jobApplications2;
        //    private JobApplication jobApplications3;

        //    [TestInitialize]
        //    public void Initialize()
        //    {
        //        guid = Guid.NewGuid().ToString();
        //        personId = "PID";
        //        jobApplications1 = new JobApplication(guid, personId);
        //        jobApplications2 = new JobApplication(guid, personId);
        //        jobApplications3 = new JobApplication(Guid.NewGuid().ToString(), "PID2");
        //    }

        //    [TestMethod]
        //    public void JobApplicationSameCodeHashEqual()
        //    {
        //        Assert.AreEqual(jobApplications1.GetHashCode(), jobApplications2.GetHashCode());
        //    }

        //    [TestMethod]
        //    public void JobApplicationDifferentCodeHashNotEqual()
        //    {
        //        Assert.AreNotEqual(jobApplications1.GetHashCode(), jobApplications3.GetHashCode());
        //    }
        //}
    }
}
