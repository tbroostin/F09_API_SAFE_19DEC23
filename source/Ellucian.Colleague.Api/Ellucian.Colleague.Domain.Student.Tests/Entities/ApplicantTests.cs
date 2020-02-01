// Copyright 2017-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Reflection;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class ApplicantTests
    {
        [TestClass]
        public class ApplicantConstructorTests
        {
            private string id;
            private string lastName;
            private string faCounselorId;

            private Applicant actualApplicant;

            [TestInitialize]
            public void Initialize()
            {
                id = "0003914";
                lastName = "DeDiana";
                faCounselorId = "0007557";

                actualApplicant = new Applicant(id, lastName);
            }

            [TestMethod]
            public void NumberOfPropertiesTest()
            {
                var applicantProperties = typeof(Applicant).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Assert.AreEqual(43, applicantProperties.Length);
            }

            [TestMethod]
            public void Id_EqualsTest()
            {
                Assert.AreEqual(id, actualApplicant.Id);
            }

            [TestMethod]
            public void LastName_EqualsTest()
            {
                Assert.AreEqual(lastName, actualApplicant.LastName);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LastName_RequiredTest()
            {
                new Applicant(id, "");
            }

            [TestMethod]
            public void FinancialAidCounselorId_InitTest()
            {
                Assert.IsTrue(string.IsNullOrEmpty(actualApplicant.FinancialAidCounselorId));
            }

            [TestMethod]
            public void FinancialAidCounselorId_GetSetTest()
            {
                actualApplicant.FinancialAidCounselorId = faCounselorId;
                Assert.AreEqual(faCounselorId, actualApplicant.FinancialAidCounselorId);
            }
        }

        [TestClass]
        public class ApplicantEqualsTests
        {
            private string id;
            private string lastName;
            private string faCounselorId;

            private Applicant expectedApplicant;
            private Applicant actualApplicant;

            [TestInitialize]
            public void Initialize()
            {
                id = "0003914";
                lastName = "DeDiana";
                faCounselorId = null;

                expectedApplicant = new Applicant(id, lastName);
                actualApplicant = new Applicant(id, lastName);
            }

            [TestMethod]
            public void ApplicantEqualsTest()
            {
                Assert.AreEqual(expectedApplicant, actualApplicant);
            }

            [TestMethod]
            public void DiffId_NotEqualsTest()
            {
                expectedApplicant = new Applicant("foobar", lastName);
                Assert.AreNotEqual(expectedApplicant, actualApplicant);
            }

            [TestMethod]
            public void DiffLastName_EqualsTest()
            {
                expectedApplicant = new Applicant(id, "foobar");
                Assert.AreEqual(expectedApplicant, actualApplicant);
            }

            [TestMethod]
            public void DiffFaCounselorId_EqualsTest()
            {
                expectedApplicant = new Applicant(id, lastName) { FinancialAidCounselorId = faCounselorId };
                Assert.AreEqual(expectedApplicant, actualApplicant);
            }

            [TestMethod]
            public void NullObj_NotEqualTest()
            {
                expectedApplicant = null;
                Assert.AreNotEqual(expectedApplicant, actualApplicant);
            }

            [TestMethod]
            public void NonApplicantObj_NotEqualTest()
            {
                var obj = new Object();
                Assert.AreNotEqual(obj, actualApplicant);
            }
        }
    }
}
