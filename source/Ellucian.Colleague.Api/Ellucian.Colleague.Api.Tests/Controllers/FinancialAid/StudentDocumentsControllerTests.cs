//Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.FinancialAid;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{
    [TestClass]
    public class StudentDocumentsControllerTests
    {
        #region GetStudentDocuments Tests
        [TestClass]
        public class GetStudentDocumentsControllerTests
        {
            #region Test Context
            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }
            #endregion

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IStudentDocumentService> studentDocumentServiceMock;

            private string studentId;

            private IEnumerable<StudentDocument> expectedStudentDocuments;
            private List<StudentDocument> testStudentDocuments;
            private IEnumerable<StudentDocument> actualStudentDocuments;

            private StudentDocumentsController StudentDocumentsController;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                studentDocumentServiceMock = new Mock<IStudentDocumentService>();

                studentId = "0003914";
                expectedStudentDocuments = new List<StudentDocument>()
                {
                    new StudentDocument()
                    {
                        Code = "FA24WES",
                        DueDate = null,
                        Status = DocumentStatus.Received,
                        Instance = "Document 1",
                        StudentId = studentId
                    },
                    new StudentDocument()
                    {
                        Code = "FA83FOO",
                        DueDate = DateTime.Today,
                        Status = DocumentStatus.Incomplete,
                        Instance = "Document 2",
                        StudentId = studentId
                    }
                };

                testStudentDocuments = new List<StudentDocument>();
                foreach (var document in expectedStudentDocuments)
                {
                    var testDocument = new StudentDocument();
                    foreach (var property in typeof(StudentDocument).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        property.SetValue(testDocument, property.GetValue(document, null), null);
                    }
                    testStudentDocuments.Add(testDocument);
                }

                studentDocumentServiceMock.Setup<Task<IEnumerable<StudentDocument>>>(d => d.GetStudentDocumentsAsync(studentId)).ReturnsAsync(testStudentDocuments);
                StudentDocumentsController = new StudentDocumentsController(adapterRegistryMock.Object, studentDocumentServiceMock.Object, loggerMock.Object);
                actualStudentDocuments = await StudentDocumentsController.GetStudentDocumentsAsync(studentId);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                studentDocumentServiceMock = null;
                studentId = null;
                expectedStudentDocuments = null;
                testStudentDocuments = null;
                actualStudentDocuments = null;
                StudentDocumentsController = null;
            }

            [TestMethod]
            public void StudentDocumentTypeTest()
            {
                Assert.AreEqual(expectedStudentDocuments.GetType(), actualStudentDocuments.GetType());
                foreach (var actualDocument in actualStudentDocuments)
                {
                    Assert.AreEqual(typeof(StudentDocument), actualDocument.GetType());
                }
            }

            [TestMethod]
            public void NumberOfKnownPropertiesTest()
            {
                var studentDocumentProperties = typeof(StudentDocument).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Assert.AreEqual(8, studentDocumentProperties.Length);
            }

            [TestMethod]
            public void PropertiesAreEqualTest()
            {
                var studentDocumentProperties = typeof(StudentDocument).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var expectedDocument in expectedStudentDocuments)
                {
                    var actualDocument = expectedStudentDocuments.First(d => d.Code == expectedDocument.Code && d.StudentId == expectedDocument.StudentId);
                    foreach (var property in studentDocumentProperties)
                    {
                        var expectedPropertyValue = property.GetValue(expectedDocument, null);
                        var actualPropertyValue = property.GetValue(actualDocument, null);
                        Assert.AreEqual(expectedPropertyValue, actualPropertyValue);
                    }
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentIdRequiredTest()
            {
                await StudentDocumentsController.GetStudentDocumentsAsync(null);
            }

            [TestMethod]
            public async Task StudentIdRequired_BadRequestResponseTest()
            {
                var exceptionCaught = false;
                try
                {
                    await StudentDocumentsController.GetStudentDocumentsAsync(string.Empty);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                }
                Assert.IsTrue(exceptionCaught);
            }

            [TestMethod]
            public async Task CatchPermissionsExceptionAndLogMessageTest()
            {
                studentDocumentServiceMock.Setup(s => s.GetStudentDocumentsAsync(studentId)).Throws(new PermissionsException("Permissions Exception"));

                var exceptionCaught = false;
                try
                {
                    await StudentDocumentsController.GetStudentDocumentsAsync(studentId);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, hre.Response.StatusCode);
                }
                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task CatchKeyNotFoundExceptionAndLogMessageTest()
            {
                studentDocumentServiceMock.Setup(s => s.GetStudentDocumentsAsync(studentId)).Throws(new KeyNotFoundException("Not Found Exception"));

                var exceptionCaught = false;
                try
                {
                    await StudentDocumentsController.GetStudentDocumentsAsync(studentId);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, hre.Response.StatusCode);
                }
                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<KeyNotFoundException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task CatchExceptionAndLogMessageTest()
            {
                studentDocumentServiceMock.Setup(s => s.GetStudentDocumentsAsync(studentId)).Throws(new Exception("Generic Exception"));

                var exceptionCaught = false;
                try
                {
                    await StudentDocumentsController.GetStudentDocumentsAsync(studentId);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                }
                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
            }
        }
        #endregion

        
    }
}
