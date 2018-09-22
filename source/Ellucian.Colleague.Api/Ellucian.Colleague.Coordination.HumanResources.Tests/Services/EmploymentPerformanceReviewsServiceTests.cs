//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Data.Colleague;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class EmploymentPerformanceReviewsServiceTests
    {
        [TestClass]
        public class EmploymentPerformanceReviewsServiceTests_GET : CurrentUserSetup
        {
            //Mock<IPositionRepository> positionRepositoryMock;
            Mock<IEmploymentPerformanceReviewsRepository> employmentPerformanceReviewsRepositoryMock;
            Mock<IHumanResourcesReferenceDataRepository> hrReferenceDataRepositoryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<IConfigurationRepository> configurationRepositoryMock;
            //Mock<IEmployeeRepository> employeeRepositoryMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            //Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            ICurrentUserFactory currentUserFactory;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            EmploymentPerformanceReviewsService employmentPerformanceReviewsService;
            IEnumerable<Domain.HumanResources.Entities.EmploymentPerformanceReview> employmentPerformanceReviewsEntities;
            Tuple<IEnumerable<Domain.HumanResources.Entities.EmploymentPerformanceReview>, int> employmentPerformanceReviewsEntityTuple;

            IEnumerable<Domain.HumanResources.Entities.EmploymentPerformanceReviewType> employmentPerformanceReviewTypeEntities;
            IEnumerable<Domain.HumanResources.Entities.EmploymentPerformanceReviewRating> employmentPerformanceReviewRatingEntities;

            private Domain.Entities.Permission permissionViewAnyPerson;
            private Domain.Entities.Permission permissionCreateUpdateAnyPerson;
            private Domain.Entities.Permission permissionDeleteAnyPerson;

            int offset = 0;
            int limit = 4;

            GuidLookupResult guidLookUpResult = new GuidLookupResult() { Entity = "PERPOS", PrimaryKey = "0012297", SecondaryKey = "123" };

            [TestInitialize]
            public void Initialize() 
            {
                //positionRepositoryMock = new Mock<IPositionRepository>();
                employmentPerformanceReviewsRepositoryMock = new Mock<IEmploymentPerformanceReviewsRepository>();
                hrReferenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                //referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();

                BuildData();
                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.ViewEmploymentPerformanceReview);
                permissionCreateUpdateAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.CreateUpdateEmploymentPerformanceReview);
                permissionDeleteAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.DeleteEmploymentPerformanceReview);
                personRole.AddPermission(permissionViewAnyPerson);
                personRole.AddPermission(permissionCreateUpdateAnyPerson);
                personRole.AddPermission(permissionDeleteAnyPerson);
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                employmentPerformanceReviewsService = new EmploymentPerformanceReviewsService(employmentPerformanceReviewsRepositoryMock.Object, hrReferenceDataRepositoryMock.Object,
                                                personRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, configurationRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup() 
            {
                employmentPerformanceReviewsEntityTuple = null;
                employmentPerformanceReviewsEntities = null;
                employmentPerformanceReviewsRepositoryMock = null;
                hrReferenceDataRepositoryMock = null;
                adapterRegistryMock = null;
                currentUserFactory = null;
                roleRepositoryMock = null;
                loggerMock = null;
            }

            #region GET
            [TestMethod]
            public async Task EmploymentPerformanceReviews_GETAllAsync()
            {
                var actualsTuple =
                    await
                        employmentPerformanceReviewsService.GetEmploymentPerformanceReviewsAsync(offset, limit, It.IsAny<bool>());

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = employmentPerformanceReviewsEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                }
            }

            [TestMethod]
            public async Task EmploymentPerformanceReviews_GETAllAsync_EmptyTuple()
            {
                employmentPerformanceReviewsEntities = new List<Domain.HumanResources.Entities.EmploymentPerformanceReview>()
                {

                };
                employmentPerformanceReviewsEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.EmploymentPerformanceReview>, int>(employmentPerformanceReviewsEntities, 0);
                employmentPerformanceReviewsRepositoryMock.Setup(i => i.GetEmploymentPerformanceReviewsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(employmentPerformanceReviewsEntityTuple);
                var actualsTuple = await employmentPerformanceReviewsService.GetEmploymentPerformanceReviewsAsync(offset, limit, It.IsAny<bool>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task EmploymentPerformanceReviews_GET_ById()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                var expected = employmentPerformanceReviewsEntities.ToList()[0];
                employmentPerformanceReviewsRepositoryMock.Setup(i => i.GetEmploymentPerformanceReviewByIdAsync(id)).ReturnsAsync(expected);
                var actual = await employmentPerformanceReviewsService.GetEmploymentPerformanceReviewsByGuidAsync(id);

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Guid, actual.Id);
            }       

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task EmploymentPerformanceReviews_GET_ById_ReturnsNullEntity_KeyNotFoundException()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                employmentPerformanceReviewsRepositoryMock.Setup(i => i.GetEmploymentPerformanceReviewByIdAsync(id)).Throws<KeyNotFoundException>();
                var actual = await employmentPerformanceReviewsService.GetEmploymentPerformanceReviewsByGuidAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task EmploymentPerformanceReviews_GET_ById_ReturnsNullEntity_InvalidOperationException()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                employmentPerformanceReviewsRepositoryMock.Setup(i => i.GetEmploymentPerformanceReviewByIdAsync(id)).Throws<InvalidOperationException>();
                var actual = await employmentPerformanceReviewsService.GetEmploymentPerformanceReviewsByGuidAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task EmploymentPerformanceReviews_GET_ById_ReturnsNullEntity_RepositoryException()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                employmentPerformanceReviewsRepositoryMock.Setup(i => i.GetEmploymentPerformanceReviewByIdAsync(id)).Throws<RepositoryException>();
                var actual = await employmentPerformanceReviewsService.GetEmploymentPerformanceReviewsByGuidAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task EmploymentPerformanceReviews_GET_ById_ReturnsNullEntity_Exception()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                employmentPerformanceReviewsRepositoryMock.Setup(i => i.GetEmploymentPerformanceReviewByIdAsync(id)).Throws<Exception>();
                var actual = await employmentPerformanceReviewsService.GetEmploymentPerformanceReviewsByGuidAsync(id);
            }
            #endregion

            #region PUT
            [TestMethod]
            public async Task EmploymentPerformanceReviews_PutEmploymentPerformanceReviewAsync()
            {
                var expected = employmentPerformanceReviewsEntities.ToList()[0];

                var actualsTuple =
                    await
                        employmentPerformanceReviewsService.GetEmploymentPerformanceReviewsAsync(offset, limit, It.IsAny<bool>());

                var result = await employmentPerformanceReviewsService.PutEmploymentPerformanceReviewsAsync(actualsTuple.Item1.FirstOrDefault().Id, actualsTuple.Item1.FirstOrDefault());

                Assert.AreEqual(actualsTuple.Item1.FirstOrDefault().Id, result.Id);
                Assert.AreEqual(actualsTuple.Item1.FirstOrDefault().Person.Id, result.Person.Id);
                Assert.AreEqual(actualsTuple.Item1.FirstOrDefault().Job.Id, result.Job.Id);
                Assert.AreEqual(actualsTuple.Item1.FirstOrDefault().CompletedOn, result.CompletedOn);
                Assert.AreEqual(actualsTuple.Item1.FirstOrDefault().Rating.Detail.Id, result.Rating.Detail.Id);
                if (result.Comment != null)
                    Assert.AreEqual(actualsTuple.Item1.FirstOrDefault().Comment, result.Comment);
                if (result.ReviewedBy != null)
                    Assert.AreEqual(actualsTuple.Item1.FirstOrDefault().ReviewedBy.Id, result.ReviewedBy.Id);
            }
            #endregion

            #region POST
            [TestMethod]
            public async Task EmploymentPerformanceReviews_PostEmploymentPerformanceReviewAsync()
            {
                var expected = employmentPerformanceReviewsEntities.ToList()[0];

                var actualsTuple =
                    await
                        employmentPerformanceReviewsService.GetEmploymentPerformanceReviewsAsync(offset, limit, It.IsAny<bool>());

                var result = await employmentPerformanceReviewsService.PostEmploymentPerformanceReviewsAsync(actualsTuple.Item1.FirstOrDefault());

                Assert.AreEqual(actualsTuple.Item1.FirstOrDefault().Id, result.Id);
                Assert.AreEqual(actualsTuple.Item1.FirstOrDefault().Person.Id, result.Person.Id);
                Assert.AreEqual(actualsTuple.Item1.FirstOrDefault().Job.Id, result.Job.Id);
                Assert.AreEqual(actualsTuple.Item1.FirstOrDefault().CompletedOn, result.CompletedOn);
                Assert.AreEqual(actualsTuple.Item1.FirstOrDefault().Rating.Detail.Id, result.Rating.Detail.Id);
                if (result.Comment != null)
                    Assert.AreEqual(actualsTuple.Item1.FirstOrDefault().Comment, result.Comment);
                if (result.ReviewedBy != null)
                    Assert.AreEqual(actualsTuple.Item1.FirstOrDefault().ReviewedBy.Id, result.ReviewedBy.Id);
            }
            #endregion

            #region DELETE
            [TestMethod]
            public async Task EmploymentPerformanceReviews_DeleteEmploymentPerformanceReviewAsync()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                //employmentPerformanceReviewsRepositoryMock.Setup(i => i.GetRecordInfoFromGuidAsync(id)).ReturnsAsync(guidLookUpResult);
                await employmentPerformanceReviewsService.DeleteEmploymentPerformanceReviewAsync(id);
            }
            #endregion

            private void BuildData()
            {

                employmentPerformanceReviewsEntities = new List<Domain.HumanResources.Entities.EmploymentPerformanceReview>() 
                {
                    new Domain.HumanResources.Entities.EmploymentPerformanceReview("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "d2253ac7-9931-4560-b42f-1fccd43c952e", new DateTime(18080), "CODE1", "CODE1"),
                    new Domain.HumanResources.Entities.EmploymentPerformanceReview("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "d2253ac7-9931-4560-b42f-1fccd43c952e", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", new DateTime(18080), "CODE2", "CODE2"),
                    new Domain.HumanResources.Entities.EmploymentPerformanceReview("d2253ac7-9931-4560-b42f-1fccd43c952e", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", new DateTime(18080), "CODE3", "CODE3")
                    //new Domain.HumanResources.Entities.EmploymentPerformanceReview("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "d2253ac7-9931-4560-b42f-1fccd43c952e", new DateTime(18080), "d2253ac7-9931-4560-b42f-1fccd43c952e", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                    //new Domain.HumanResources.Entities.EmploymentPerformanceReview("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "d2253ac7-9931-4560-b42f-1fccd43c952e", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", new DateTime(18080), "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "d2253ac7-9931-4560-b42f-1fccd43c952e"),
                    //new Domain.HumanResources.Entities.EmploymentPerformanceReview("d2253ac7-9931-4560-b42f-1fccd43c952e", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", new DateTime(18080), "d2253ac7-9931-4560-b42f-1fccd43c952e", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d")
                };

                employmentPerformanceReviewRatingEntities = new List<Domain.HumanResources.Entities.EmploymentPerformanceReviewRating>() 
                {
                    new Domain.HumanResources.Entities.EmploymentPerformanceReviewRating("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "CODE1", "DESC1"),
                    new Domain.HumanResources.Entities.EmploymentPerformanceReviewRating("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "CODE1", "DESC1"),
                    new Domain.HumanResources.Entities.EmploymentPerformanceReviewRating("d2253ac7-9931-4560-b42f-1fccd43c952e", "CODE1", "DESC1"),
                };

                employmentPerformanceReviewTypeEntities = new List<Domain.HumanResources.Entities.EmploymentPerformanceReviewType>() 
                {
                    new Domain.HumanResources.Entities.EmploymentPerformanceReviewType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "CODE1", "DESC1"),
                    new Domain.HumanResources.Entities.EmploymentPerformanceReviewType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "CODE1", "DESC1"),
                    new Domain.HumanResources.Entities.EmploymentPerformanceReviewType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CODE1", "DESC1"),
                };

                employmentPerformanceReviewsEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.EmploymentPerformanceReview>, int>(employmentPerformanceReviewsEntities, employmentPerformanceReviewsEntities.Count());
                employmentPerformanceReviewsRepositoryMock.Setup(i => i.GetEmploymentPerformanceReviewsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(employmentPerformanceReviewsEntityTuple);
                employmentPerformanceReviewsRepositoryMock.Setup(i => i.GetEmploymentPerformanceReviewByIdAsync(It.IsAny<string>())).ReturnsAsync(employmentPerformanceReviewsEntities.ToList()[0]);
                employmentPerformanceReviewsRepositoryMock.Setup(i => i.GetGuidFromIdAsync(It.IsAny<string>(), "PERSON")).ReturnsAsync("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d");
                employmentPerformanceReviewsRepositoryMock.Setup(i => i.GetGuidFromIdAsync(It.IsAny<string>(), "PERPOS")).ReturnsAsync("d2253ac7-9931-4560-b42f-1fccd43c952e");
                employmentPerformanceReviewsRepositoryMock.Setup(i => i.GetJobGuidFromIdAsync(It.IsAny<string>(), "PERPOS")).ReturnsAsync("d2253ac7-9931-4560-b42f-1fccd43c952e");
                hrReferenceDataRepositoryMock.Setup(i => i.GetEmploymentPerformanceReviewTypesAsync(It.IsAny<bool>())).ReturnsAsync(employmentPerformanceReviewTypeEntities);
                hrReferenceDataRepositoryMock.Setup(i => i.GetEmploymentPerformanceReviewRatingsAsync(It.IsAny<bool>())).ReturnsAsync(employmentPerformanceReviewRatingEntities);

                employmentPerformanceReviewsRepositoryMock.Setup(i => i.GetIdFromGuidAsync(employmentPerformanceReviewsEntities.FirstOrDefault().Guid)).ReturnsAsync("NOTNULL");
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(employmentPerformanceReviewsEntities.FirstOrDefault().PersonId)).ReturnsAsync(employmentPerformanceReviewsEntities.ToList()[0].PersonId);
                employmentPerformanceReviewsRepositoryMock.Setup(i => i.GetIdFromGuidAsync(employmentPerformanceReviewsEntities.FirstOrDefault().PerposId)).ReturnsAsync(employmentPerformanceReviewsEntities.ToList()[0].PerposId);
                employmentPerformanceReviewsRepositoryMock.Setup(i => i.GetInfoFromGuidAsync(It.IsAny<string>())).ReturnsAsync(guidLookUpResult);
                employmentPerformanceReviewsRepositoryMock.Setup(i => i.GetIdFromGuidAsync(employmentPerformanceReviewsEntities.FirstOrDefault().ReviewedById)).ReturnsAsync(employmentPerformanceReviewsEntities.ToList()[0].ReviewedById);

                employmentPerformanceReviewsRepositoryMock.Setup(i => i.UpdateEmploymentPerformanceReviewsAsync(It.IsAny<EmploymentPerformanceReview>())).ReturnsAsync(employmentPerformanceReviewsEntities.ToList()[0]);
                employmentPerformanceReviewsRepositoryMock.Setup(i => i.CreateEmploymentPerformanceReviewsAsync(It.IsAny<EmploymentPerformanceReview>())).ReturnsAsync(employmentPerformanceReviewsEntities.ToList()[0]);

            }
        }
    }
}
