﻿//Copyright 2017 Ellucian Company L.P. and its affiliates.


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
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class InstructorCategoriesServiceTests
    {
        private const string instructorCategoriesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string instructorCategoriesCode = "AT";
        private ICollection<FacultySpecialStatuses> _facultySpecialStatusesCollection;
        private InstructorCategoriesService _instructorCategoriesService;
        private Mock<ILogger> _loggerMock;
        private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;



        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            _facultySpecialStatusesCollection = new List<FacultySpecialStatuses>()
                {
                    new FacultySpecialStatuses("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new FacultySpecialStatuses("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new FacultySpecialStatuses("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetFacultySpecialStatusesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_facultySpecialStatusesCollection);

            _instructorCategoriesService = new InstructorCategoriesService(_referenceRepositoryMock.Object, _adapterRegistryMock.Object, _currentUserFactoryMock.Object, 
                                                                           _roleRepositoryMock.Object, _loggerMock.Object, baseConfigurationRepository);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _instructorCategoriesService = null;
            _facultySpecialStatusesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task InstructorCategoriesService_GetInstructorCategoriesAsync()
        {
            var results = await _instructorCategoriesService.GetInstructorCategoriesAsync(true);
            Assert.IsTrue(results is IEnumerable<InstructorCategories>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task InstructorCategoriesService_GetInstructorCategoriesAsync_Count()
        {
            var results = await _instructorCategoriesService.GetInstructorCategoriesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task InstructorCategoriesService_GetInstructorCategoriesAsync_Properties()
        {
            var result =
                (await _instructorCategoriesService.GetInstructorCategoriesAsync(true)).FirstOrDefault(x => x.Code == instructorCategoriesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task InstructorCategoriesService_GetInstructorCategoriesAsync_Expected()
        {
            var expectedResults = _facultySpecialStatusesCollection.FirstOrDefault(c => c.Guid == instructorCategoriesGuid);
            var actualResult =
                (await _instructorCategoriesService.GetInstructorCategoriesAsync(true)).FirstOrDefault(x => x.Id == instructorCategoriesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task InstructorCategoriesService_GetInstructorCategoriesByGuidAsync_Empty()
        {
            await _instructorCategoriesService.GetInstructorCategoriesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task InstructorCategoriesService_GetInstructorCategoriesByGuidAsync_Null()
        {
            await _instructorCategoriesService.GetInstructorCategoriesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task InstructorCategoriesService_GetInstructorCategoriesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetFacultySpecialStatusesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _instructorCategoriesService.GetInstructorCategoriesByGuidAsync("99");
        }

        [TestMethod]
        public async Task InstructorCategoriesService_GetInstructorCategoriesByGuidAsync_Expected()
        {
            var expectedResults =
                _facultySpecialStatusesCollection.First(c => c.Guid == instructorCategoriesGuid);
            var actualResult =
                await _instructorCategoriesService.GetInstructorCategoriesByGuidAsync(instructorCategoriesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task InstructorCategoriesService_GetInstructorCategoriesByGuidAsync_Properties()
        {
            var result =
                await _instructorCategoriesService.GetInstructorCategoriesByGuidAsync(instructorCategoriesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}