// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class InstructorsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IInstructorsService> instructorsServiceMock;
        private Mock<ILogger> loggerMock;
        private InstructorsController instructorsController;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Instructor> allFaculty;
        private List<Dtos.Instructor> instructorsCollection;
        private List<Dtos.Instructor2> instructor2Collection;
        private Tuple<IEnumerable<Dtos.Instructor>, int> instructorsTuple;
        private Tuple<IEnumerable<Dtos.Instructor2>, int> instructor2Tuple;
        private Paging page = new Paging(200, 0);
        private string criteriaString = "{'instructor':'51194109-9cfe-42ff-b393-07b61f61e282', 'primaryLocation':'69d4639c-f9d0-4393-adaf-b1287b71525e'}";
        private QueryStringFilter criteria;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            criteria = new QueryStringFilter("criteria", criteriaString);
            instructorsServiceMock = new Mock<IInstructorsService>();
            loggerMock = new Mock<ILogger>();
            instructorsCollection = new List<Dtos.Instructor>();
            instructor2Collection = new List<Dtos.Instructor2>();

            allFaculty = new List<Ellucian.Colleague.Domain.Student.Entities.Instructor>()
                {
                    new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT")
                    {
                        ContractType = "CT 1",
                         Departments = new List<FacultyDeptLoad>()
                         {
                            new FacultyDeptLoad()
                            {
                                DeptPcts = 50,
                                FacultyDepartment = "Dept 1"
                            }
                         }
                    },
                    new Ellucian.Colleague.Domain.Student.Entities.Instructor("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC")
                    {
                         ContractType = "CT 2",
                         Departments = new List<FacultyDeptLoad>()
                         {
                            new FacultyDeptLoad()
                            {
                                DeptPcts = 50,
                                FacultyDepartment = "Dept 2"
                            }
                         }
                    },
                    new Ellucian.Colleague.Domain.Student.Entities.Instructor("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU")
                    {
                         ContractType = "CT 3",
                         Departments = new List<FacultyDeptLoad>()
                         {
                            new FacultyDeptLoad()
                            {
                                DeptPcts = 50,
                                FacultyDepartment = "Dept 3"
                            }
                         }
                    }
                };

            foreach (var source in allFaculty)
            {
                var instructors = new Ellucian.Colleague.Dtos.Instructor
                {
                    Id = source.RecordGuid                   
                };
                instructorsCollection.Add(instructors);

                var instructor2 = new Ellucian.Colleague.Dtos.Instructor2
                {
                    Id = source.RecordGuid
                };
                instructor2Collection.Add(instructor2);
            }
            instructorsTuple = new Tuple<IEnumerable<Dtos.Instructor>, int>(instructorsCollection, instructorsCollection.Count());
            instructor2Tuple = new Tuple<IEnumerable<Dtos.Instructor2>, int>(instructor2Collection, instructorsCollection.Count());

            instructorsController = new InstructorsController(instructorsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            instructorsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            instructorsController = null;
            allFaculty = null;
            instructorsCollection = null;
            instructor2Collection = null;
            loggerMock = null;
            instructorsServiceMock = null;
        }

        #region V8 tests

        [TestMethod]
        public async Task InstructorsController_GetInstructors_ValidateFields_Nocache()
        {
            instructorsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };        

            instructorsServiceMock.Setup(x => x.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), false))
                .ReturnsAsync(instructorsTuple);

            var sourceContexts = await instructorsController.GetInstructorsAsync(null, criteria);
            Assert.IsNotNull(sourceContexts);
        }

        [TestMethod]
        public async Task InstructorsController_GetInstructors_ValidateFields_cache()
        {
            instructorsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            instructorsServiceMock.Setup(x => x.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), true))
                .ReturnsAsync(instructorsTuple);

            var sourceContexts = await instructorsController.GetInstructorsAsync(page, criteria);
            Assert.IsNotNull(sourceContexts);
        }

        [TestMethod]
        public async Task InstructorsController_GetInstructorsByGuidAsync_ValidateFields()
        {
            var expected = instructorsCollection.FirstOrDefault();
            instructorsServiceMock.Setup(x => x.GetInstructorByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await instructorsController.GetInstructorsByGuidAsync(expected.Id);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorsController_GetInstructors_Exception()
        {
            instructorsServiceMock.Setup(x => x.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), true)).ThrowsAsync(new Exception());
            await instructorsController.GetInstructorsAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorsController_GetInstructors_ArgumentException()
        {
            instructorsServiceMock.Setup(x => x.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), true)).ThrowsAsync(new ArgumentException());
            var tempCriteriaString = "{'ABC':'ABC'}";
            var tempCriteria = new QueryStringFilter("criteria", tempCriteriaString);
            await instructorsController.GetInstructorsAsync(null, tempCriteria);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorsController_GetInstructors_KeyNotFoundException()
        {
            instructorsServiceMock.Setup(x => x.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), false)).ThrowsAsync(new KeyNotFoundException());
            await instructorsController.GetInstructorsAsync(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorsController_GetInstructors_PermissionsException()
        {
            instructorsServiceMock.Setup(x => x.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
            await instructorsController.GetInstructorsAsync(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorsController_GetInstructors_ArgumentNullException()
        {
            instructorsServiceMock.Setup(x => x.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentNullException());
            await instructorsController.GetInstructorsAsync(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorsController_GetInstructorsByGuidAsync_Exception()
        {
            instructorsServiceMock.Setup(x => x.GetInstructorByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await instructorsController.GetInstructorsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorsController_GetInstructorsByGuidAsync_Service_Exception()
        {
            instructorsServiceMock.Setup(x => x.GetInstructorByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            await instructorsController.GetInstructorsByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorsController_GetInstructorsByGuidAsync_KeyNotFoundException()
        {
            instructorsServiceMock.Setup(x => x.GetInstructorByGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            await instructorsController.GetInstructorsByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorsController_GetInstructorsByGuidAsync_ArgumentNullException()
        {
            instructorsServiceMock.Setup(x => x.GetInstructorByGuidAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
            await instructorsController.GetInstructorsByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorsController_GetInstructorsByGuidAsync_PermissionsException()
        {
            instructorsServiceMock.Setup(x => x.GetInstructorByGuidAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
            await instructorsController.GetInstructorsByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorsController_PostInstructorsAsync_Exception()
        {
            await instructorsController.PostInstructorsAsync(instructorsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorsController_PutInstructorsAsync_Exception()
        {
            var sourceContext = instructorsCollection.FirstOrDefault();
            await instructorsController.PutInstructorsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorsController_DeleteInstructorsAsync_Exception()
        {
            await instructorsController.DeleteInstructorsAsync(instructorsCollection.FirstOrDefault().Id);
        }

        #endregion V8 tests

        #region V9 tests


        [TestMethod]
        public async Task InstructorsController_GetInstructors2_ValidateFields_Nocache()
        {
            instructorsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            instructorsServiceMock.Setup(x => x.GetInstructors2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), false))
                .ReturnsAsync(instructor2Tuple);

            var sourceContexts = await instructorsController.GetInstructors2Async(null, criteria);
            Assert.IsNotNull(sourceContexts);
        }

        [TestMethod]
        public async Task InstructorsController_GetInstructors2_ValidateFields_cache()
        {
            instructorsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            instructorsServiceMock.Setup(x => x.GetInstructors2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), true))
                .ReturnsAsync(instructor2Tuple);

            var sourceContexts = await instructorsController.GetInstructors2Async(page, criteria);
            Assert.IsNotNull(sourceContexts);
        }

        [TestMethod]
        public async Task InstructorsController_GetInstructorsByGuid2Async_ValidateFields()
        {
            var expected = instructor2Collection.FirstOrDefault();
            instructorsServiceMock.Setup(x => x.GetInstructorByGuid2Async(expected.Id)).ReturnsAsync(expected);

            var actual = await instructorsController.GetInstructorsByGuid2Async(expected.Id);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorsController_GetInstructors2_Exception()
        {
            instructorsServiceMock.Setup(x => x.GetInstructors2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), true)).ThrowsAsync(new Exception());
            await instructorsController.GetInstructors2Async(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorsController_GetInstructors2_ArgumentException()
        {
            instructorsServiceMock.Setup(x => x.GetInstructors2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), true)).ThrowsAsync(new ArgumentException());
            var tempCriteriaString = "{'ABC':'ABC'}";
            var tempCriteria = new QueryStringFilter("criteria", tempCriteriaString);
            await instructorsController.GetInstructors2Async(null, tempCriteria);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorsController_GetInstructors2_KeyNotFoundException()
        {
            instructorsServiceMock.Setup(x => x.GetInstructors2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), false)).ThrowsAsync(new KeyNotFoundException());
            await instructorsController.GetInstructors2Async(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorsController_GetInstructors2_PermissionsException()
        {
            instructorsServiceMock.Setup(x => x.GetInstructors2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
            await instructorsController.GetInstructors2Async(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorsController_GetInstructors2_ArgumentNullException()
        {
            instructorsServiceMock.Setup(x => x.GetInstructors2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentNullException());
            await instructorsController.GetInstructors2Async(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorsController_GetInstructorsByGuid2Async_Exception()
        {
            instructorsServiceMock.Setup(x => x.GetInstructorByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await instructorsController.GetInstructorsByGuid2Async(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorsController_GetInstructorsByGuid2Async_Service_Exception()
        {
            instructorsServiceMock.Setup(x => x.GetInstructorByGuid2Async(It.IsAny<string>())).ThrowsAsync(new Exception());
            await instructorsController.GetInstructorsByGuid2Async("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorsController_GetInstructorsByGuid2Async_KeyNotFoundException()
        {
            instructorsServiceMock.Setup(x => x.GetInstructorByGuid2Async(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            await instructorsController.GetInstructorsByGuid2Async("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorsController_GetInstructorsByGuid2Async_ArgumentNullException()
        {
            instructorsServiceMock.Setup(x => x.GetInstructorByGuid2Async(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
            await instructorsController.GetInstructorsByGuid2Async("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorsController_GetInstructorsByGuid2Async_PermissionsException()
        {
            instructorsServiceMock.Setup(x => x.GetInstructorByGuid2Async(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
            await instructorsController.GetInstructorsByGuid2Async("1");
        }

        #endregion V9 tests

    }
}