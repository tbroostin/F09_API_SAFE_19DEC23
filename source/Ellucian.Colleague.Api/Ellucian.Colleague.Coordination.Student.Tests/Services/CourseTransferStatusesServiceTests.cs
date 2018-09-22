//Copyright 2018 Ellucian Company L.P. and its affiliates.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{    
    [TestClass]
    public class CourseTransferStatusesServiceTests
    {
        private const string courseTransferStatusesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string courseTransferStatusesCode = "AT";
        private ICollection<SectionRegistrationStatusItem> _courseTransferStatusesCollection;

        private CourseTransferStatusesService _courseTransferStatusesService;
        
        private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;
       

        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
           _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();



            _courseTransferStatusesCollection = new List<SectionRegistrationStatusItem>()
                {                   
                    new SectionRegistrationStatusItem("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic")
                    {
                        Status = new Domain.Student.Entities.SectionRegistrationStatus()
                        { RegistrationStatus = Domain.Student.Entities.RegistrationStatus.NotRegistered,
                            SectionRegistrationStatusReason = Domain.Student.Entities.RegistrationStatusReason.Transfer }
                    },
                    new SectionRegistrationStatusItem("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic")
                    {
                        Status = new Domain.Student.Entities.SectionRegistrationStatus()
                        { RegistrationStatus = Domain.Student.Entities.RegistrationStatus.NotRegistered,
                            SectionRegistrationStatusReason = Domain.Student.Entities.RegistrationStatusReason.Transfer }
                    },
                    new SectionRegistrationStatusItem("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                    {
                        Status = new Domain.Student.Entities.SectionRegistrationStatus()
                        { RegistrationStatus = Domain.Student.Entities.RegistrationStatus.NotRegistered,
                            SectionRegistrationStatusReason = Domain.Student.Entities.RegistrationStatusReason.Transfer }
                    },
                };

                       
            _referenceRepositoryMock.Setup(repo => repo.GetStudentAcademicCreditStatusesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_courseTransferStatusesCollection);

            _courseTransferStatusesService = new CourseTransferStatusesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _courseTransferStatusesService = null;
            _courseTransferStatusesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock= null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task CourseTransferStatusesService_GetCourseTransferStatusesAsync()
        {
            var results = await _courseTransferStatusesService.GetCourseTransferStatusesAsync(true);
            Assert.IsTrue(results is IEnumerable<CourseTransferStatuses>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task CourseTransferStatusesService_GetCourseTransferStatusesAsync_Count()
        {
            var results = await _courseTransferStatusesService.GetCourseTransferStatusesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task CourseTransferStatusesService_GetCourseTransferStatusesAsync_Properties()
        {
            var result =
                (await _courseTransferStatusesService.GetCourseTransferStatusesAsync(true)).FirstOrDefault(x => x.Code == courseTransferStatusesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task CourseTransferStatusesService_GetCourseTransferStatusesAsync_Expected()
        {
            var expectedResults = _courseTransferStatusesCollection.FirstOrDefault(c => c.Guid == courseTransferStatusesGuid);
            var actualResult =
                (await _courseTransferStatusesService.GetCourseTransferStatusesAsync(true)).FirstOrDefault(x => x.Id == courseTransferStatusesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task CourseTransferStatusesService_GetCourseTransferStatusesByGuidAsync_Empty()
        {
            await _courseTransferStatusesService.GetCourseTransferStatusesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task CourseTransferStatusesService_GetCourseTransferStatusesByGuidAsync_Null()
        {
            await _courseTransferStatusesService.GetCourseTransferStatusesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task CourseTransferStatusesService_GetCourseTransferStatusesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetStudentAcademicCreditStatusesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _courseTransferStatusesService.GetCourseTransferStatusesByGuidAsync("99");
        }

        [TestMethod]
        public async Task CourseTransferStatusesService_GetCourseTransferStatusesByGuidAsync_Expected()
        {
            var expectedResults =
                _courseTransferStatusesCollection.First(c => c.Guid == courseTransferStatusesGuid);
            var actualResult =
                await _courseTransferStatusesService.GetCourseTransferStatusesByGuidAsync(courseTransferStatusesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task CourseTransferStatusesService_GetCourseTransferStatusesByGuidAsync_Properties()
        {
            var result =
                await _courseTransferStatusesService.GetCourseTransferStatusesByGuidAsync(courseTransferStatusesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}