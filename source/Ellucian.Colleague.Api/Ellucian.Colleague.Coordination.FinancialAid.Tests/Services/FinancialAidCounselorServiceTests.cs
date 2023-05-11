//Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.FinancialAid;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{
    [TestClass]
    public class FinancialAidCounselorServiceTests
    {
        [TestClass]
        public class GetSingleCounselorTests : FinancialAidServiceTestsSetup
        {
            private string counselorId;

            private Staff inputStaffEntity;

            private FinancialAidCounselorEntityToDtoAdapter financialAidCounselorDtoAdapter;

            private Dtos.FinancialAid.FinancialAidCounselor expectedCounselor;
            private Dtos.FinancialAid.FinancialAidCounselor actualCounselor;

            private Mock<IStaffRepository> staffRepositoryMock;

            private FinancialAidCounselorService financialAidCounselorService;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();

                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                counselorId = "0010477";
                inputStaffEntity = new Staff(counselorId, "lastName")
                {
                    PreferredName = "FirstName LastName",
                    IsActive = true
                };

                inputStaffEntity.AddEmailAddress(new EmailAddress("email.address@ellucian.edu", "P") { IsPreferred = false });
                inputStaffEntity.AddEmailAddress(new EmailAddress("contact.me@ellucian.edu", "B") { IsPreferred = true });

                staffRepositoryMock = new Mock<IStaffRepository>();
                staffRepositoryMock.Setup(r => r.Get(counselorId)).Returns(inputStaffEntity);

                financialAidCounselorDtoAdapter = new FinancialAidCounselorEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                expectedCounselor = financialAidCounselorDtoAdapter.MapToType(inputStaffEntity);

                adapterRegistryMock.Setup(a => a.GetAdapter<Domain.Base.Entities.Staff, Dtos.FinancialAid.FinancialAidCounselor>()).Returns(financialAidCounselorDtoAdapter);

                financialAidCounselorService = new FinancialAidCounselorService(
                    adapterRegistryMock.Object,
                    staffRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                actualCounselor = financialAidCounselorService.GetCounselor(counselorId);
            }

            [TestCleanup]
            public void Cleanup()
            {
                counselorId = null;
                inputStaffEntity = null;
                financialAidCounselorDtoAdapter = null;
                expectedCounselor = null;
                actualCounselor = null;
                staffRepositoryMock = null;
                financialAidCounselorService = null;
            }

            [TestMethod]
            public void ObjectsAreNotNullTest()
            {
                Assert.IsNotNull(expectedCounselor);
                Assert.IsNotNull(actualCounselor);
            }

            [TestMethod]
            public void ObjectsAreEqualTest()
            {
                Assert.AreEqual(expectedCounselor.Id, actualCounselor.Id);
                Assert.AreEqual(expectedCounselor.Name, actualCounselor.Name);
                Assert.AreEqual(expectedCounselor.EmailAddress, actualCounselor.EmailAddress);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullCounselorIdArgumentThrowsExceptionTest()
            {
                financialAidCounselorService.GetCounselor(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void NullReturnedFromRepositoryThrowsExceptionTest()
            {
                inputStaffEntity = null;
                staffRepositoryMock.Setup(r => r.Get(counselorId)).Returns(inputStaffEntity);

                financialAidCounselorService.GetCounselor(counselorId);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void InactiveStaffThrowsExceptionTest()
            {
                inputStaffEntity.IsActive = false;
                financialAidCounselorService.GetCounselor(counselorId);
            }

            [TestMethod]
            public void CatchRepositoryException_LogsErrorAndThrowsExceptionTest()
            {
                var exceptionErrorMessage = "foobar";
                staffRepositoryMock.Setup(r => r.Get(counselorId)).Throws(new Exception(exceptionErrorMessage));
                var exceptionCaught = false;

                try
                {
                    financialAidCounselorService.GetCounselor(counselorId);
                }
                catch (ApplicationException)
                {
                    exceptionCaught = true;
                }

                Assert.IsTrue(exceptionCaught);

                loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), exceptionErrorMessage));
            }

        }

        [TestClass]
        public class GetMultipleCounselorsTests : FinancialAidServiceTestsSetup
        {
            private List<string> counselorIds;

            private List<Staff> inputStaffEntities;

            private FinancialAidCounselorEntityToDtoAdapter financialAidCounselorDtoAdapter;

            private List<Dtos.FinancialAid.FinancialAidCounselor> expectedCounselors;
            
            private Mock<IStaffRepository> staffRepositoryMock;

            private FinancialAidCounselorService financialAidCounselorService;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();

                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                counselorIds = new List<string>() { "0004791", "0005647", "7685467", "0000000"};
                inputStaffEntities = new List<Staff>()
                {
                   new Staff("0004791", "lastName1"){
                       IsActive = true
                   },
                   new Staff("0005647", "lastName2"){
                       IsActive = true
                   },
                   new Staff("7685467", "lastName3"){
                       IsActive = true
                   },
                   new Staff("0000000", "lastName4"){
                       IsActive = true
                   }
                };

                
                staffRepositoryMock = new Mock<IStaffRepository>();
                staffRepositoryMock.Setup(r => r.Get(counselorIds)).Returns(inputStaffEntities);

                financialAidCounselorDtoAdapter = new FinancialAidCounselorEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                expectedCounselors = new List<FinancialAidCounselor>();
                foreach (var inputEntity in inputStaffEntities)
                {
                    expectedCounselors.Add(financialAidCounselorDtoAdapter.MapToType(inputEntity));
                }
                
                adapterRegistryMock.Setup(a => a.GetAdapter<Domain.Base.Entities.Staff, Dtos.FinancialAid.FinancialAidCounselor>()).Returns(financialAidCounselorDtoAdapter);

                financialAidCounselorService = new FinancialAidCounselorService(
                    adapterRegistryMock.Object,
                    staffRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);                
            }

            [TestCleanup]
            public void Cleanup()
            {
                counselorIds = null;
                inputStaffEntities = null;
                financialAidCounselorDtoAdapter = null;
                expectedCounselors = null;                
                staffRepositoryMock = null;
                financialAidCounselorService = null;
            }

            [TestMethod]
            public async Task ObjectsAreNotNullTest()
            {
                var actualCounselors = await financialAidCounselorService.GetCounselorsByIdAsync(counselorIds);
                Assert.IsTrue(actualCounselors != null);
                Assert.IsTrue(expectedCounselors != null);
                foreach (var counselor in actualCounselors)
                {
                    Assert.IsNotNull(counselor);
                }
                foreach (var counselor in expectedCounselors)
                {
                    Assert.IsNotNull(counselor);
                }
            }

            [TestMethod]
            public async Task ObjectsAreEqualTest() {
                var actualCounselors = await financialAidCounselorService.GetCounselorsByIdAsync(counselorIds);
                foreach (var actualCounselor in actualCounselors)
                {
                    var expectedCounselor = expectedCounselors.Find(ec => ec.Id == actualCounselor.Id);
                    Assert.IsNotNull(expectedCounselor);
                    Assert.AreEqual(expectedCounselor.Name, actualCounselor.Name);
                   
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task NullReturnedFromRepositoryThrowsExceptionTest()
            {
                inputStaffEntities = null;
                staffRepositoryMock.Setup(r => r.Get(counselorIds)).Returns(inputStaffEntities);

                await financialAidCounselorService.GetCounselorsByIdAsync(counselorIds);
            }

            [TestMethod]
            public async Task NoIsActiveStaff_ReturnedListIsEmptyTest()
            {
                inputStaffEntities.ForEach(s => s.IsActive = false);
                var actualCounselors = await financialAidCounselorService.GetCounselorsByIdAsync(counselorIds);
                Assert.IsTrue(actualCounselors.Count() == 0);
            }

            [TestMethod]
            public async Task NotIsActiveStaff_DoesNotGetIncludedAndLogsErrorTest()
            {                
                var inactiveCounselor = inputStaffEntities.First();
                var errorMessage = string.Format("Counselor {0} is not a valid staff member", inactiveCounselor.Id);
                inactiveCounselor.IsActive = false;
                var actualCounselors = await financialAidCounselorService.GetCounselorsByIdAsync(counselorIds);
                Assert.IsTrue(!actualCounselors.Any(c => c.Id == inactiveCounselor.Id));
                loggerMock.Verify(l => l.Debug(errorMessage));
            }
            
        }
    }
}
