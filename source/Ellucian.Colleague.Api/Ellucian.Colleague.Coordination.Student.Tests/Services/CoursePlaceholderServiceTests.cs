//Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student.DegreePlans;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class CoursePlaceholderServiceTests
    {
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<ICoursePlaceholderRepository> repositoryMock;
        private Mock<ICurrentUserFactory> currentUserFactoryMock;
        private Mock<IRoleRepository> roleRepositoryMock;
        private Mock<ILogger> loggerMock;

        private IAdapterRegistry adapterRegistry;
        private ICoursePlaceholderRepository repository;
        private ICurrentUserFactory currentUserFactory;
        private IRoleRepository roleRepository;
        private ILogger logger;

        private CoursePlaceholderService service;

        private List<Domain.Student.Entities.DegreePlans.CoursePlaceholder> cachedCoursePlaceholders;
        private List<Domain.Student.Entities.DegreePlans.CoursePlaceholder> nonCachedCoursePlaceholders;

        [TestInitialize]
        public void CoursePlaceholderService_Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            repositoryMock = new Mock<ICoursePlaceholderRepository>();
            repository = repositoryMock.Object;
            currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            currentUserFactory = currentUserFactoryMock.Object;
            roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepository = roleRepositoryMock.Object;
            loggerMock = new Mock<ILogger>();
            logger = loggerMock.Object;
            service = new CoursePlaceholderService(adapterRegistry, repository, currentUserFactory, roleRepository, logger);

            adapterRegistryMock.Setup(ar => ar.GetAdapter<Domain.Student.Entities.DegreePlans.CoursePlaceholder, CoursePlaceholder>()).Returns(new CoursePlaceholderEntityToCoursePlaceholderDtoAdapter(adapterRegistry, logger));
            cachedCoursePlaceholders = new List<Domain.Student.Entities.DegreePlans.CoursePlaceholder>()
            {
                new Domain.Student.Entities.DegreePlans.CoursePlaceholder("1", "Placeholder 1", "Description of placeholder 1", DateTime.Today.AddDays(-14), DateTime.Today.AddDays(14), "3 to 5 credits", null),
                new Domain.Student.Entities.DegreePlans.CoursePlaceholder("2", "Placeholder 2", "Description of placeholder 2", DateTime.Today.AddDays(-7), DateTime.Today.AddDays(7), "4 to 6 credits", 
                new Domain.Student.Entities.Requirements.AcademicRequirementGroup("REQ2", "SUBREQ2", "1234"))
            };
            nonCachedCoursePlaceholders = new List<Domain.Student.Entities.DegreePlans.CoursePlaceholder>()
            {
                new Domain.Student.Entities.DegreePlans.CoursePlaceholder("1", "Placeholder 1 Noncached", "Description of placeholder 1 noncached", DateTime.Today.AddDays(-24), DateTime.Today.AddDays(24), "3 to 6 credits", null),
                new Domain.Student.Entities.DegreePlans.CoursePlaceholder("2", "Placeholder 2 Noncached", "Description of placeholder 2 noncached", DateTime.Today.AddDays(-17), DateTime.Today.AddDays(17), "4 to 5 credits",
                new Domain.Student.Entities.Requirements.AcademicRequirementGroup("REQ2-NC", "SUBREQ2-NC", "1234-NC"))
            };
            repositoryMock.Setup(repo => repo.GetCoursePlaceholdersByIdsAsync(It.IsAny<IEnumerable<string>>(), true)).ReturnsAsync(nonCachedCoursePlaceholders);
            repositoryMock.Setup(repo => repo.GetCoursePlaceholdersByIdsAsync(It.IsAny<IEnumerable<string>>(), false)).ReturnsAsync(cachedCoursePlaceholders);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetCoursePlaceholdersByIdsAsync_null_Ids_throws_ArgumentNullException()
        {
            var dtos = await service.GetCoursePlaceholdersByIdsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetCoursePlaceholdersByIdsAsync_empty_Ids_throws_ArgumentNullException()
        {
            var dtos = await service.GetCoursePlaceholdersByIdsAsync(new List<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task GetCoursePlaceholdersByIdsAsync_repository_exception_rethrown()
        {
            repositoryMock.Setup(repo => repo.GetCoursePlaceholdersByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>())).ThrowsAsync(new Exception("Repository exception"));
            var dtos = await service.GetCoursePlaceholdersByIdsAsync(new List<string>() { "1", "2" });
        }

        [TestMethod]
        public async Task GetCoursePlaceholdersByIdsAsync_bypassCache_true()
        {
            var dtos = await service.GetCoursePlaceholdersByIdsAsync(new List<string>() { "1", "2" }, true);
            Assert.AreEqual(nonCachedCoursePlaceholders.Count, dtos.Count());
            for (var i = 0; i < nonCachedCoursePlaceholders.Count; i++)
            {
                var expected = nonCachedCoursePlaceholders.ElementAt(i);
                var actual = dtos.ElementAt(i);
                Assert.AreEqual(expected.CreditInformation, actual.CreditInformation);
                Assert.AreEqual(expected.Description, actual.Description);
                Assert.AreEqual(expected.EndDate, actual.EndDate);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.StartDate, actual.StartDate);
                Assert.AreEqual(expected.Title, actual.Title);
                if (expected.AcademicRequirement != null)
                {
                    Assert.AreEqual(expected.AcademicRequirement.AcademicRequirementCode, actual.AcademicRequirement.AcademicRequirementCode);
                    Assert.AreEqual(expected.AcademicRequirement.SubrequirementId, actual.AcademicRequirement.SubrequirementId);
                    Assert.AreEqual(expected.AcademicRequirement.GroupId, actual.AcademicRequirement.GroupId);
                }
            }
        }

        [TestMethod]
        public async Task GetCoursePlaceholdersByIdsAsync_bypassCache_false()
        {
            var dtos = await service.GetCoursePlaceholdersByIdsAsync(new List<string>() { "1", "2" }, false);
            Assert.AreEqual(cachedCoursePlaceholders.Count, dtos.Count());
            for (var i = 0; i < cachedCoursePlaceholders.Count; i++)
            {
                var expected = cachedCoursePlaceholders.ElementAt(i);
                var actual = dtos.ElementAt(i);
                Assert.AreEqual(expected.CreditInformation, actual.CreditInformation);
                Assert.AreEqual(expected.Description, actual.Description);
                Assert.AreEqual(expected.EndDate, actual.EndDate);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.StartDate, actual.StartDate);
                Assert.AreEqual(expected.Title, actual.Title);
                if (expected.AcademicRequirement != null)
                {
                    Assert.AreEqual(expected.AcademicRequirement.AcademicRequirementCode, actual.AcademicRequirement.AcademicRequirementCode);
                    Assert.AreEqual(expected.AcademicRequirement.SubrequirementId, actual.AcademicRequirement.SubrequirementId);
                    Assert.AreEqual(expected.AcademicRequirement.GroupId, actual.AcademicRequirement.GroupId);
                }
            }
        }

        [TestMethod]
        public async Task GetCoursePlaceholdersByIdsAsync_bypassCache_default()
        {
            var dtos = await service.GetCoursePlaceholdersByIdsAsync(new List<string>() { "1", "2" });
            Assert.AreEqual(cachedCoursePlaceholders.Count, dtos.Count());
            for (var i = 0; i < cachedCoursePlaceholders.Count; i++)
            {
                var expected = cachedCoursePlaceholders.ElementAt(i);
                var actual = dtos.ElementAt(i);
                Assert.AreEqual(expected.CreditInformation, actual.CreditInformation);
                Assert.AreEqual(expected.Description, actual.Description);
                Assert.AreEqual(expected.EndDate, actual.EndDate);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.StartDate, actual.StartDate);
                Assert.AreEqual(expected.Title, actual.Title);
                if (expected.AcademicRequirement != null)
                {
                    Assert.AreEqual(expected.AcademicRequirement.AcademicRequirementCode, actual.AcademicRequirement.AcademicRequirementCode);
                    Assert.AreEqual(expected.AcademicRequirement.SubrequirementId, actual.AcademicRequirement.SubrequirementId);
                    Assert.AreEqual(expected.AcademicRequirement.GroupId, actual.AcademicRequirement.GroupId);
                }
            }
        }
    }
}
