using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class SectionRegistrationStatusTests
    {
        [TestClass]
        public class SectionRegistrationStatusConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private SectionRegistrationStatusItem secRegStat;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "N";
                desc = "New";
                secRegStat = new SectionRegistrationStatusItem(guid, code, desc) { Status = new Domain.Student.Entities.SectionRegistrationStatus() { RegistrationStatus = Domain.Student.Entities.RegistrationStatus.NotRegistered, SectionRegistrationStatusReason = Domain.Student.Entities.RegistrationStatusReason.Dropped } };
            }

            [TestMethod]
            public void SectionRegistrationStatus_Guid()
            {
                Assert.AreEqual(guid, secRegStat.Guid);
            }

            [TestMethod]
            public void SectionRegistrationStatus_Code()
            {
                Assert.AreEqual(code, secRegStat.Code);
            }

            [TestMethod]
            public void SectionRegistrationStatus_Description()
            {
                Assert.AreEqual(desc, secRegStat.Description);
            }

            [TestMethod]
            public void SectionRegistrationStatus_RegistrationStatus()
            {
                RegistrationStatus regstat = Domain.Student.Entities.RegistrationStatus.NotRegistered;
                Assert.AreEqual(regstat, secRegStat.Status.RegistrationStatus);
            }

            [TestMethod]
            public void SectionRegistrationStatus_RegistrationStatusReason()
            {
                RegistrationStatusReason regstat = Domain.Student.Entities.RegistrationStatusReason.Dropped;
                Assert.AreEqual(regstat, secRegStat.Status.SectionRegistrationStatusReason);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionRegistrationStatus_Guid_NullException()
            {
                new SectionRegistrationStatusItem(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionRegistrationStatus_Guid_EmptyException()
            {
                new SectionRegistrationStatusItem(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionRegistrationStatus_Code_NullException()
            {
                new SectionRegistrationStatusItem(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionRegistrationStatusCodeEmptyException()
            {
                new SectionRegistrationStatusItem(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionRegistrationStatus_Desc_NullException()
            {
                new SectionRegistrationStatusItem(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionRegistrationStatusDescEmptyException()
            {
                new SectionRegistrationStatusItem(guid, code, string.Empty);
            }
        }
    }
}
