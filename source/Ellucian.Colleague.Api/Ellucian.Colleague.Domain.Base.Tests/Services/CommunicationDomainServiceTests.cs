using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Services
{
    [TestClass]
    public class CommunicationDomainServiceTests
    {
        public TestCommunicationRepository communicationRepository;

        public List<Communication> communications
        { get { return communicationRepository.GetCommunications("0003914").ToList(); } }


        public void CommunicationDomainServiceTestsInitialize()
        {
            communicationRepository = new TestCommunicationRepository();
        }

        [TestClass]
        public class AddOrUpdateTests : CommunicationDomainServiceTests
        {
            public Communication comm;
            public List<Communication> existingCommunications;

            [TestInitialize]
            public void Initialize()
            {
                CommunicationDomainServiceTestsInitialize();
                existingCommunications = communications.ToList();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CommunicationInputRequiredTest()
            {
                existingCommunications.AddOrUpdate(null);
            }

            [TestMethod]
            public void UpdateTest()
            {
                comm = existingCommunications.First().DeepCopy();
                comm.StatusCode = "FOO";
                comm.CommentId = "BAR";

                var prevCount = existingCommunications.Count();
                existingCommunications.AddOrUpdate(comm);

                Assert.AreEqual(prevCount, existingCommunications.Count());

                Assert.AreEqual("FOO", existingCommunications.GetDuplicate(comm).StatusCode);
                Assert.AreEqual("BAR", existingCommunications.GetDuplicate(comm).CommentId);
            }

            [TestMethod]
            public void AddTest()
            {
                comm = new Communication("foo", "bar");
                var prevCount = existingCommunications.Count();
                existingCommunications.AddOrUpdate(comm);

                Assert.AreEqual(prevCount + 1, existingCommunications.Count());

                Assert.AreEqual("foo", existingCommunications.GetDuplicate(comm).PersonId);
                Assert.AreEqual("bar", existingCommunications.GetDuplicate(comm).Code);
            }
        }

        [TestClass]
        public class DuplicateTests : CommunicationDomainServiceTests
        {
            public Communication duplicate;

            [TestInitialize]
            public void Initialize()
            {
                CommunicationDomainServiceTestsInitialize();
            }

            [TestMethod]
            public void IndexOfDuplicate_FindsDuplicateTest()
            {
                var duplicate = communications[0].DeepCopy();
                var index = communications.IndexOfDuplicate(duplicate);
                Assert.AreEqual(0, index);
            }

            [TestMethod]
            public void IndexOfDuplicate_DoesNotFindDuplicateTest()
            {
                var duplicate = new Communication("foo", "bar");
                var index = communications.IndexOfDuplicate(duplicate);
                Assert.AreEqual(-1, index);
            }

            [TestMethod]
            public void ContainsDuplicate_FindsDuplicateTest()
            {
                var duplicate = communications[0].DeepCopy();
                Assert.IsTrue(communications.ContainsDuplicate(duplicate));
            }

            [TestMethod]
            public void ContainsDuplicate_DoesNotFindDuplicateTest()
            {
                var duplicate = new Communication("foo", "bar");
                Assert.IsFalse(communications.ContainsDuplicate(duplicate));
            }

            [TestMethod]
            public void GetDuplicate_FindsDuplicateTest()
            {
                var duplicate = communications[0].DeepCopy();
                Assert.AreEqual(duplicate, communications.GetDuplicate(duplicate));
            }

            [TestMethod]
            public void GetDuplicate_DoesNotFindDuplicateTest()
            {
                var duplicate = new Communication("foo", "bar");
                Assert.IsNull(communications.GetDuplicate(duplicate));
            }
        }

        [TestClass]
        public class ReviseDatesForCreateOrUpdateTests : CommunicationDomainServiceTests
        {
            public Communication comm;
            public List<Communication> existingCommunications;

            [TestInitialize]
            public void Initialize()
            {
                CommunicationDomainServiceTestsInitialize();
                existingCommunications = communications.ToList();
            }

            [TestMethod]
            public void NullExistingCommunicationsDoesCreateTest()
            {
                comm = new Communication("foo", "bar");
                var newComm = comm.ReviseDatesForCreateOrUpdate(null);
                Assert.AreEqual(DateTime.Today, newComm.AssignedDate);
            }

            [TestMethod]
            public void OriginalCommunicationNotChangedOnUpdateTest()
            {
                comm = existingCommunications[0].DeepCopy();
                comm.StatusDate = null;
                comm.AssignedDate = null;
                var newComm = comm.ReviseDatesForCreateOrUpdate(existingCommunications);
                Assert.IsNull(comm.AssignedDate);
                Assert.AreNotEqual(comm.AssignedDate, newComm.AssignedDate);                
            }

            [TestMethod]
            public void UpdateAssignedDateIfNoStatusDateTest()
            {
                existingCommunications[0].StatusDate = null;
                existingCommunications[0].AssignedDate = DateTime.Today;
                comm = existingCommunications[0].DeepCopy();    
                comm.AssignedDate = null;
                var newComm = comm.ReviseDatesForCreateOrUpdate(existingCommunications);
                Assert.AreEqual(existingCommunications[0].AssignedDate, newComm.AssignedDate);
            }

            [TestMethod]
            public void UpdateActionDateIfNoStatusDateTest()
            {
                existingCommunications[0].StatusDate = null;
                comm = existingCommunications[0].DeepCopy();
                comm.ActionDate = null;
                var newComm = comm.ReviseDatesForCreateOrUpdate(existingCommunications);
                Assert.AreEqual(existingCommunications[0].ActionDate, newComm.ActionDate);
            }

            [TestMethod]
            public void UpdateAssignedDateWithStatusDateTest()
            {
                existingCommunications[0].AssignedDate = null;
                comm = existingCommunications[0].DeepCopy();
                comm.StatusDate = new DateTime(2000, 1, 1);
                var newComm = comm.ReviseDatesForCreateOrUpdate(existingCommunications);
                Assert.AreEqual(comm.StatusDate, newComm.AssignedDate);
            }

            [TestMethod]
            public void UpdateAssignedDateWithTodayTest()
            {
                existingCommunications[0].AssignedDate = null;
                existingCommunications[0].StatusDate = new DateTime(2000, 1, 1);
                comm = existingCommunications[0].DeepCopy();
                comm.StatusDate = null;
                var newComm = comm.ReviseDatesForCreateOrUpdate(existingCommunications);
                Assert.AreEqual(DateTime.Today, newComm.AssignedDate);
            }

            [TestMethod]
            public void OriginalCommunicationNotChangedOnCreateTest()
            {
                comm = new Communication("foo", "bar");
                var newComm = comm.ReviseDatesForCreateOrUpdate(existingCommunications);
                Assert.IsNull(comm.AssignedDate);
                Assert.AreNotEqual(comm.AssignedDate, newComm.AssignedDate);
            }

            [TestMethod]
            public void NoDuplicateDoesCreateTest()
            {
                comm = new Communication("foo", "bar");
                var newComm = comm.ReviseDatesForCreateOrUpdate(existingCommunications);
                Assert.AreEqual(DateTime.Today, newComm.AssignedDate);
            }

            [TestMethod]
            public void AssignedDateAlreadyHasValueCreateTest()
            {
                var testDate = new DateTime(2000, 1, 1);
                comm = new Communication("foo", "bar") { AssignedDate = testDate };
                var newComm = comm.ReviseDatesForCreateOrUpdate(existingCommunications);
                Assert.AreEqual(testDate, newComm.AssignedDate);
            }

            [TestMethod]
            public void StatusDateHasValueAndBeforeTodayCreateTest()
            {
                var testStatusDate = DateTime.Today.AddDays(-1);
                comm = new Communication("foo", "bar") { StatusDate = testStatusDate };
                var newComm = comm.ReviseDatesForCreateOrUpdate(existingCommunications);
                Assert.AreEqual(testStatusDate, newComm.AssignedDate);
            }

            [TestMethod]
            public void StatusDateHasValueAndIsTomorrowCreateTest()
            {
                var testStatusDate = DateTime.Today.AddDays(1);
                comm = new Communication("foo", "bar") { StatusDate = testStatusDate };
                var newComm = comm.ReviseDatesForCreateOrUpdate(existingCommunications);
                Assert.AreEqual(DateTime.Today, newComm.AssignedDate);
            }
        }
    }
}
