// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class AddAuthorizationRepositoryTests
    {
        [TestClass]
        public class AddAuthorizationRepository_GetAsync : BaseRepositorySetup
        {
            AddAuthorizationRepository AddAuthorizationRepository;
            AddAuthorizations addAuthorizationContract;


            [TestInitialize]
            public async void Initialize()
            {
                MockInitialize();

                addAuthorizationContract = new AddAuthorizations()
                {
                    Recordkey = "goodId",
                    AauCourseSection = "SectionId",
                    AauStudent = "StudentId",
                    AauAuthorizationCode = "AbCd12345",
                    AauAssignedBy = "FacultyId",
                    AauAssignedDate = DateTime.Today.AddDays(-1),
                    AauAssignedTime = DateTime.Today.AddHours(-1),
                    AauRevokedFlag = "Y",
                    AauRevokedBy = "AnotherFacultyID",
                    AauRevokedDate = DateTime.Today,
                    AauRevokedTime = DateTime.Now
                };
                // Collection of data accessor responses

                AddAuthorizationRepository = BuildValidAddAuthorizationRepository(addAuthorizationContract);
            }

            [TestCleanup]
            public void Cleanup()
            {

                MockCleanup();
                AddAuthorizationRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetAddAuthorization_NullId_ThrowsException()
            {
                var addAuth = await AddAuthorizationRepository.GetAsync(null);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetAddAuthorization_EmptyId_ThrowsException()
            {
                var addAuth = await AddAuthorizationRepository.GetAsync(string.Empty);

            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetAddAuthorization_DataReaderReturnsNull()
            {
                var addAuth = await AddAuthorizationRepository.GetAsync("badId");
            }

            [TestMethod]
            public async Task GetAddAuthorization_Success()
            {
                var addAuthorization = await AddAuthorizationRepository.GetAsync("goodId");
                Assert.IsTrue(addAuthorization is AddAuthorization);
                Assert.AreEqual(addAuthorizationContract.Recordkey, addAuthorization.Id);
                Assert.AreEqual(addAuthorizationContract.AauCourseSection, addAuthorization.SectionId);
                Assert.AreEqual(addAuthorizationContract.AauStudent, addAuthorization.StudentId);
                Assert.AreEqual(addAuthorizationContract.AauAuthorizationCode, addAuthorization.AddAuthorizationCode);
                Assert.AreEqual(addAuthorizationContract.AauAssignedBy, addAuthorization.AssignedBy);
                Assert.IsTrue(addAuthorization.IsRevoked);
                Assert.AreEqual(addAuthorizationContract.AauRevokedBy, addAuthorization.RevokedBy);
            }


            private AddAuthorizationRepository BuildValidAddAuthorizationRepository(AddAuthorizations addAuthorizationContract)
            {

                dataReaderMock.Setup(sacc => sacc.ReadRecordAsync<AddAuthorizations>("goodId", true)).ReturnsAsync(addAuthorizationContract);
                AddAuthorizations badAuthorization = null;
                dataReaderMock.Setup(sacc => sacc.ReadRecordAsync<AddAuthorizations>("badId", true)).ReturnsAsync(badAuthorization);
                AddAuthorizationRepository repository = new AddAuthorizationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }



        }

        [TestClass]
        public class AddAuthorizationRepository_UpdateAddAuthorizationAsync : BaseRepositorySetup
        {

            DateTime? assignedTime;
            DateTime? revokedTime;
            DateTimeOffset? assignedTimeOffset;
            DateTimeOffset? revokedTimeOffset;
            UpdateAddAuthorizationRequest updatedTransactionRequest;
            string sectionId;
            AddAuthorization addAuthorizationToUpdate;
            AddAuthorizations addAuthorizationResponseData;
            AddAuthorizationRepository AddAuthorizationRepository;


            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();
                assignedTime = new DateTime(2018, 1, 5, 9, 0, 0);
                revokedTime = new DateTime(2018, 1, 5, 10, 0, 0);
                sectionId = "sectionId";
                addAuthorizationToUpdate = new AddAuthorization("Id", sectionId)
                {
                    AddAuthorizationCode = "abCD1234",
                    StudentId = "studentId",
                    AssignedBy = "facultyId",
                    AssignedTime = assignedTime,
                    IsRevoked = true,
                    RevokedBy = "revokedBy",
                    RevokedTime = revokedTime

                };


                // Collection of data accessor responses
                addAuthorizationResponseData = BuildAddAuthorizationResponse(addAuthorizationToUpdate);
                AddAuthorizationRepository = BuildValidAddAuthorizationRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
                addAuthorizationResponseData = null;
                AddAuthorizationRepository = null;
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateAddAuthorization_ThrowsExceptionIfAddAuthorizationNull()
            {
                var AddAuthorizations = await AddAuthorizationRepository.UpdateAddAuthorizationAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task UpdateAddAuthorization_ThrowsExceptionIfColleagueTXThrows()
            {
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateAddAuthorizationRequest, UpdateAddAuthorizationResponse>(It.Is<UpdateAddAuthorizationRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).ThrowsAsync(new Exception());
                var AddAuthorizations = await AddAuthorizationRepository.UpdateAddAuthorizationAsync(addAuthorizationToUpdate);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task UpdateAddAuthorization_ThrowsExceptionIfColleagueTXReturnsNull()
            {
                UpdateAddAuthorizationResponse nullResponse = null;
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateAddAuthorizationRequest, UpdateAddAuthorizationResponse>(It.Is<UpdateAddAuthorizationRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).ReturnsAsync(nullResponse).Callback<UpdateAddAuthorizationRequest>(req => updatedTransactionRequest = req);
                var AddAuthorizations = await AddAuthorizationRepository.UpdateAddAuthorizationAsync(addAuthorizationToUpdate);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UpdateAddAuthorization_ThrowsKeyNotFoundException()
            {
                UpdateAddAuthorizationResponse failureResponse = new UpdateAddAuthorizationResponse()
                {
                    ErrorMessage = "Not Found: Add Authorization was not found with ID 1111",
                    ErrorOccurred = true
                };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateAddAuthorizationRequest, UpdateAddAuthorizationResponse>(It.Is<UpdateAddAuthorizationRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).ReturnsAsync(failureResponse).Callback<UpdateAddAuthorizationRequest>(req => updatedTransactionRequest = req);
                var AddAuthorizations = await AddAuthorizationRepository.UpdateAddAuthorizationAsync(addAuthorizationToUpdate);
            }

            [TestMethod]
            [ExpectedException(typeof(RecordLockException))]
            public async Task UpdateAddAuthorization_ThrowsRecordLockException()
            {
                UpdateAddAuthorizationResponse failureResponse = new UpdateAddAuthorizationResponse()
                {
                    ErrorMessage = "Locked: Add Authorization is locked.",
                    ErrorOccurred = true
                };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateAddAuthorizationRequest, UpdateAddAuthorizationResponse>(It.Is<UpdateAddAuthorizationRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).ReturnsAsync(failureResponse).Callback<UpdateAddAuthorizationRequest>(req => updatedTransactionRequest = req);
                var AddAuthorizations = await AddAuthorizationRepository.UpdateAddAuthorizationAsync(addAuthorizationToUpdate);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task UpdateAddAuthorization_ThrowsApplicationException()
            {
                UpdateAddAuthorizationResponse failureResponse = new UpdateAddAuthorizationResponse()
                {
                    ErrorMessage = "Invalid Section: Update is for a different section. Update not allowed.",
                    ErrorOccurred = true
                };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateAddAuthorizationRequest, UpdateAddAuthorizationResponse>(It.Is<UpdateAddAuthorizationRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).ReturnsAsync(failureResponse).Callback<UpdateAddAuthorizationRequest>(req => updatedTransactionRequest = req);
                var AddAuthorizations = await AddAuthorizationRepository.UpdateAddAuthorizationAsync(addAuthorizationToUpdate);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task UpdateAddAuthorization_CantFindUpdatedAuthorization()
            {
                dataReaderMock.Setup(sacc => sacc.ReadRecordAsync<AddAuthorizations>(It.IsAny<string>(), true)).ThrowsAsync(new KeyNotFoundException());
                var AddAuthorizations = await AddAuthorizationRepository.UpdateAddAuthorizationAsync(addAuthorizationToUpdate);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task UpdateAddAuthorization_Generic_Exception_thrown_on_GetAsync_of_Updated_Authorization()
            {
                dataReaderMock.Setup(sacc => sacc.ReadRecordAsync<AddAuthorizations>(It.IsAny<string>(), true)).ThrowsAsync(new ApplicationException());
                var AddAuthorizations = await AddAuthorizationRepository.UpdateAddAuthorizationAsync(addAuthorizationToUpdate);
            }

            [TestMethod]
            public async Task UpdateAddAuthorization_Success()
            {
                var updatedAddAuthorization = await AddAuthorizationRepository.UpdateAddAuthorizationAsync(addAuthorizationToUpdate);
                Assert.IsTrue(updatedAddAuthorization is AddAuthorization);
                Assert.IsTrue(updatedAddAuthorization.IsRevoked);
                Assert.AreEqual(addAuthorizationToUpdate.Id, updatedAddAuthorization.Id);
                Assert.AreEqual(addAuthorizationToUpdate.SectionId, updatedAddAuthorization.SectionId);
                Assert.AreEqual(addAuthorizationToUpdate.StudentId, updatedAddAuthorization.StudentId);
                Assert.AreEqual(addAuthorizationToUpdate.AddAuthorizationCode, updatedAddAuthorization.AddAuthorizationCode);
                Assert.AreEqual(addAuthorizationToUpdate.AssignedBy, updatedAddAuthorization.AssignedBy);
                Assert.AreEqual(addAuthorizationToUpdate.RevokedBy, updatedAddAuthorization.RevokedBy);

            }


            private AddAuthorizations BuildAddAuthorizationResponse(AddAuthorization aa)
            {

                AddAuthorizations addContract = new AddAuthorizations()
                {
                    Recordkey = aa.Id,
                    AauCourseSection = aa.SectionId,
                    AauAuthorizationCode = aa.AddAuthorizationCode,
                    AauStudent = aa.StudentId,
                    AauAssignedBy = aa.AssignedBy,
                    AauRevokedBy = aa.RevokedBy,
                    AauRevokedFlag = aa.IsRevoked ? "Y" : "N",
                    AauAssignedDate = assignedTime,
                    AauRevokedDate = revokedTime,
                    AauAssignedTime = assignedTime,
                    AauRevokedTime = revokedTime

                };
                return addContract;

            }


            private AddAuthorizationRepository BuildValidAddAuthorizationRepository()
            {



                // Successful response for an update
                var response = new UpdateAddAuthorizationResponse()
                {
                    UpdatedAddAuthorizationId = "addAuthorizationId",
                    ErrorOccurred = false

                };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateAddAuthorizationRequest, UpdateAddAuthorizationResponse>(It.Is<UpdateAddAuthorizationRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).ReturnsAsync(response).Callback<UpdateAddAuthorizationRequest>(req => updatedTransactionRequest = req);

                // Mock responses needed to get the newly updated AddAuthorization item....
                dataReaderMock.Setup(sacc => sacc.ReadRecordAsync<AddAuthorizations>(It.IsAny<string>(), true)).ReturnsAsync(addAuthorizationResponseData);

                AddAuthorizationRepository repository = new AddAuthorizationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }


        }

        [TestClass]
        public class AddAuthorizationRepository_GetAddAuthorizationByAddCodeAsync : BaseRepositorySetup
        {
            AddAuthorizationRepository AddAuthorizationRepository;
            AddAuthorizations addAuthorizationContract;


            [TestInitialize]
            public async void Initialize()
            {
                MockInitialize();

                addAuthorizationContract = new AddAuthorizations()
                {
                    Recordkey = "Id",
                    AauCourseSection = "SectionId",
                    AauStudent = "StudentId",
                    AauAuthorizationCode = "AbCd12345",
                    AauAssignedBy = "FacultyId",
                    AauAssignedDate = DateTime.Today.AddDays(-1),
                    AauAssignedTime = DateTime.Today.AddHours(-1),
                    AauRevokedFlag = "Y",
                    AauRevokedBy = "AnotherFacultyID",
                    AauRevokedDate = DateTime.Today,
                    AauRevokedTime = DateTime.Now
                };


                AddAuthorizationRepository = BuildValidAddAuthorizationRepository(addAuthorizationContract);
            }

            [TestCleanup]
            public void Cleanup()
            {

                MockCleanup();
                AddAuthorizationRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetAddAuthorization_NullSectionId_ThrowsException()
            {
                var addAuth = await AddAuthorizationRepository.GetAddAuthorizationByAddCodeAsync(null, "AddCode");

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetAddAuthorization_NullAddCode_ThrowsException()
            {
                var addAuth = await AddAuthorizationRepository.GetAddAuthorizationByAddCodeAsync("SectionId", null);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetAddAuthorization_EmptySectionId_ThrowsException()
            {
                var addAuth = await AddAuthorizationRepository.GetAddAuthorizationByAddCodeAsync(string.Empty, "AddCode");

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetAddAuthorization_EmptyAddCode_ThrowsException()
            {
                var addAuth = await AddAuthorizationRepository.GetAddAuthorizationByAddCodeAsync("SectionId", string.Empty);

            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetAddAuthorization_BulkRead_ReturnsEmptyList()
            {
                Collection<AddAuthorizations> dataCollection = new Collection<AddAuthorizations>();
                dataReaderMock.Setup(sacc => sacc.BulkReadRecordAsync<AddAuthorizations>(It.IsAny<string>(), true)).ReturnsAsync(dataCollection);
                var addAuth = await AddAuthorizationRepository.GetAddAuthorizationByAddCodeAsync("SectionId", "AddCode"); 
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetAddAuthorization_BulkRead_ReturnsNull()
            {
                Collection<AddAuthorizations> dataCollection = null;
                dataReaderMock.Setup(sacc => sacc.BulkReadRecordAsync<AddAuthorizations>(It.IsAny<string>(), true)).ReturnsAsync(dataCollection);
                var addAuth = await AddAuthorizationRepository.GetAddAuthorizationByAddCodeAsync("SectionId", "AddCode"); 
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task GetAddAuthorization_BulkRead_MoreThanOne()
            {
                Collection<AddAuthorizations> dataCollection = new Collection<AddAuthorizations>()
                    {
                        new AddAuthorizations() {  Recordkey = "1", AauAuthorizationCode = "AddCode", AauCourseSection = "SectionId"},
                         new AddAuthorizations() {  Recordkey = "100", AauAuthorizationCode = "AddCode", AauCourseSection = "SectionId"}
                    };
                dataReaderMock.Setup(sacc => sacc.BulkReadRecordAsync<AddAuthorizations>(It.IsAny<string>(), true)).ReturnsAsync(dataCollection);
                var addAuth = await AddAuthorizationRepository.GetAddAuthorizationByAddCodeAsync("SectionId", "AddCode"); 
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task GetAddAuthorizatio_BulkRead_ThrowsException()
            {
                Exception ex = new Exception();
                dataReaderMock.Setup(sacc => sacc.BulkReadRecordAsync<AddAuthorizations>(It.IsAny<string>(), true)).ThrowsAsync(ex);
                var addAuth = await AddAuthorizationRepository.GetAddAuthorizationByAddCodeAsync("SectionId", "AddCode");
                loggerMock.Verify(l => l.Error(ex, "Bulk read failed for section Id SectionId and add code AddCode"));
            }


            [TestMethod]
            public async Task GetAddAuthorization_Success()
            {
                var addAuthorization = await AddAuthorizationRepository.GetAddAuthorizationByAddCodeAsync("SectionId", "AddCode");
                Assert.IsTrue(addAuthorization is AddAuthorization);
                Assert.AreEqual(addAuthorizationContract.Recordkey, addAuthorization.Id);
                Assert.AreEqual(addAuthorizationContract.AauCourseSection, addAuthorization.SectionId);
                Assert.AreEqual(addAuthorizationContract.AauStudent, addAuthorization.StudentId);
                Assert.AreEqual(addAuthorizationContract.AauAuthorizationCode, addAuthorization.AddAuthorizationCode);
                Assert.AreEqual(addAuthorizationContract.AauAssignedBy, addAuthorization.AssignedBy);
                Assert.IsTrue(addAuthorization.IsRevoked);
                Assert.AreEqual(addAuthorizationContract.AauRevokedBy, addAuthorization.RevokedBy);
            }


            private AddAuthorizationRepository BuildValidAddAuthorizationRepository(AddAuthorizations addAuthorizationContract)
            {
                Collection<AddAuthorizations> dataCollection = new Collection<AddAuthorizations>() { addAuthorizationContract };
                dataReaderMock.Setup(sacc => sacc.BulkReadRecordAsync<AddAuthorizations>(It.IsAny<string>(), true)).ReturnsAsync(dataCollection);

                AddAuthorizationRepository repository = new AddAuthorizationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }



        }

        [TestClass]
        public class AddAuthorizationRepository_GetSectionAddAuthorizationsAsync : BaseRepositorySetup
        {
            AddAuthorizationRepository AddAuthorizationRepository;
            Collection<AddAuthorizations> addAuthorizationContracts;


            [TestInitialize]
            public async void Initialize()
            {
                MockInitialize();
                addAuthorizationContracts = new Collection<AddAuthorizations>() {
                    new AddAuthorizations()
                    {
                        Recordkey = "Id1",
                        AauCourseSection = "SectionId",
                        AauStudent = "StudentId1",
                        AauAuthorizationCode = "AbCd12345",
                        AauAssignedBy = "FacultyId",
                        AauAssignedDate = DateTime.Today.AddDays(-1),
                        AauAssignedTime = DateTime.Today.AddHours(-1),
                        AauRevokedFlag = "Y",
                        AauRevokedBy = "AnotherFacultyID",
                        AauRevokedDate = DateTime.Today,
                        AauRevokedTime = DateTime.Now
                    },
                    new AddAuthorizations()
                    {
                        Recordkey = "Id2",
                        AauCourseSection = "SectionId",
                        AauStudent = "StudentId2",
                        AauAuthorizationCode = "AbCd12346",
                        AauAssignedBy = "FacultyId",
                        AauAssignedDate = DateTime.Today.AddDays(-2),
                        AauAssignedTime = DateTime.Today.AddHours(-2)

                    },
                    new AddAuthorizations()
                    {
                        Recordkey = "Id3",
                        AauCourseSection = "SectionId",
                        AauStudent = "StudentId2",
                        AauAuthorizationCode = "AbCd12346",
                        AauAssignedBy = "FacultyId",
                        AauAssignedDate = DateTime.Today.AddDays(-2),
                        AauAssignedTime = DateTime.Today.AddHours(-2),
                        AauRevokedFlag = "y",
                        AauRevokedBy = "AnotherFacultyID",
                        AauRevokedDate = DateTime.Today.AddDays(-1),
                        AauRevokedTime = DateTime.Now.AddDays(-1)
                    },
                    new AddAuthorizations()
                    {
                        Recordkey = "Id4",
                        AauCourseSection = "SectionId",
                        AauStudent = "StudentId2",
                        AauAuthorizationCode = "AbCd12346",
                        AauAssignedBy = "FacultyId",
                        AauAssignedDate = DateTime.Today.AddDays(-2),
                        AauAssignedTime = DateTime.Today.AddHours(-2),
                        AauRevokedFlag = "N"
                    }
                };

                AddAuthorizationRepository = BuildValidAddAuthorizationRepository(addAuthorizationContracts);
            }

            [TestCleanup]
            public void Cleanup()
            {

                MockCleanup();
                AddAuthorizationRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetSectionAddAuthorizationsAsync_NullSectionId_ThrowsException()
            {
                var addAuth = await AddAuthorizationRepository.GetSectionAddAuthorizationsAsync(null);

            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetSectionAddAuthorizationsAsync_EmptySectionId_ThrowsException()
            {
                var addAuth = await AddAuthorizationRepository.GetSectionAddAuthorizationsAsync(string.Empty);

            }

            [TestMethod]
            public async Task GetSectionAddAuthorizationsAsync_BulkRead_ReturnsEmptyList()
            {
                Collection<AddAuthorizations> dataCollection = new Collection<AddAuthorizations>();
                dataReaderMock.Setup(sacc => sacc.BulkReadRecordAsync<AddAuthorizations>(It.IsAny<string>(), true)).ReturnsAsync(dataCollection);
                var addAuth = await AddAuthorizationRepository.GetSectionAddAuthorizationsAsync("SectionId");
                Assert.AreEqual(0, addAuth.Count());
            }

            [TestMethod]
            public async Task GetSectionAddAuthorizationsAsync_BulkRead_ReturnsNull()
            {
                Collection<AddAuthorizations> dataCollection = null;
                dataReaderMock.Setup(sacc => sacc.BulkReadRecordAsync<AddAuthorizations>(It.IsAny<string>(), true)).ReturnsAsync(dataCollection);
                var addAuth = await AddAuthorizationRepository.GetSectionAddAuthorizationsAsync("SectionId");
                Assert.AreEqual(0, addAuth.Count());
            }


            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task GetSectionAddAuthorizationsAsync_BulkRead_ThrowsException()
            {
                Exception ex = new Exception();
                dataReaderMock.Setup(sacc => sacc.BulkReadRecordAsync<AddAuthorizations>(It.IsAny<string>(), true)).ThrowsAsync(ex);
                var addAuth = await AddAuthorizationRepository.GetSectionAddAuthorizationsAsync("SectionId");
                loggerMock.Verify(l => l.Error(ex, "Add Authorization read failed for section Id SectionId"));
            }

            [TestMethod]
            public async Task GetAddAuthorization_Success()
            {
                var addAuthorizations = await AddAuthorizationRepository.GetSectionAddAuthorizationsAsync("SectionId");
                Assert.IsTrue(addAuthorizations is IEnumerable<AddAuthorization>);
                Assert.AreEqual(4, addAuthorizations.Count());
                foreach (var addAuth in addAuthorizations)
                {
                    var expectedDto = addAuthorizationContracts.Where(aa => aa.Recordkey == addAuth.Id).FirstOrDefault();
                    Assert.IsNotNull(expectedDto);
                    Assert.AreEqual(expectedDto.AauStudent, addAuth.StudentId);
                    Assert.AreEqual(expectedDto.AauCourseSection, addAuth.SectionId);
                    Assert.AreEqual(expectedDto.AauAuthorizationCode, addAuth.AddAuthorizationCode);
                    Assert.AreEqual(expectedDto.AauAssignedBy, addAuth.AssignedBy);
                    Assert.AreEqual(!string.IsNullOrEmpty(expectedDto.AauRevokedFlag) && expectedDto.AauRevokedFlag.Equals("Y", StringComparison.InvariantCultureIgnoreCase), addAuth.IsRevoked);
                }

            }


            private AddAuthorizationRepository BuildValidAddAuthorizationRepository(Collection<AddAuthorizations> addAuthorizationContracts)
            {
                dataReaderMock.Setup(sacc => sacc.BulkReadRecordAsync<AddAuthorizations>(It.IsAny<string>(), true)).ReturnsAsync(addAuthorizationContracts);

                AddAuthorizationRepository repository = new AddAuthorizationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }



        }

        [TestClass]
        public class AddAuthorizationRepository_GetStudentAddAuthorizationsAsync : BaseRepositorySetup
        {
            AddAuthorizationRepository AddAuthorizationRepository;
            Collection<AddAuthorizations> addAuthorizationContracts;


            [TestInitialize]
            public async void Initialize()
            {
                MockInitialize();
                addAuthorizationContracts = new Collection<AddAuthorizations>() {
                    new AddAuthorizations()
                    {
                        Recordkey = "Id1",
                        AauCourseSection = "SectionId",
                        AauStudent = "StudentId1",
                        AauAuthorizationCode = "AbCd12345",
                        AauAssignedBy = "FacultyId",
                        AauAssignedDate = DateTime.Today.AddDays(-1),
                        AauAssignedTime = DateTime.Today.AddHours(-1),
                        AauRevokedFlag = "Y",
                        AauRevokedBy = "AnotherFacultyID",
                        AauRevokedDate = DateTime.Today,
                        AauRevokedTime = DateTime.Now
                    },
                    new AddAuthorizations()
                    {
                        Recordkey = "Id2",
                        AauCourseSection = "SectionId",
                        AauStudent = "StudentId2",
                        AauAuthorizationCode = "AbCd12346",
                        AauAssignedBy = "FacultyId",
                        AauAssignedDate = DateTime.Today.AddDays(-2),
                        AauAssignedTime = DateTime.Today.AddHours(-2)
                    },
                    new AddAuthorizations()
                    {
                        Recordkey = "Id3",
                        AauCourseSection = "SectionId",
                        AauStudent = "StudentId2",
                        AauAuthorizationCode = "AbCd12346",
                        AauAssignedBy = "FacultyId",
                        AauAssignedDate = DateTime.Today.AddDays(-2),
                        AauAssignedTime = DateTime.Today.AddHours(-2),
                        AauRevokedFlag = "y",
                        AauRevokedBy = "AnotherFacultyID",
                        AauRevokedDate = DateTime.Today.AddDays(-1),
                        AauRevokedTime = DateTime.Now.AddDays(-1)
                    },
                    new AddAuthorizations()
                    {
                        Recordkey = "Id4",
                        AauCourseSection = "SectionId",
                        AauStudent = "StudentId2",
                        AauAuthorizationCode = "AbCd12346",
                        AauAssignedBy = "FacultyId",
                        AauAssignedDate = DateTime.Today.AddDays(-2),
                        AauAssignedTime = DateTime.Today.AddHours(-2),
                        AauRevokedFlag = "N"
                    }
                };
                AddAuthorizationRepository = BuildValidAddAuthorizationRepository(addAuthorizationContracts);
            }

            [TestCleanup]
            public void Cleanup()
            {

                MockCleanup();
                AddAuthorizationRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentAddAuthorizationsAsync_NullSectionId_ThrowsException()
            {
                var addAuth = await AddAuthorizationRepository.GetStudentAddAuthorizationsAsync(null);

            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentAddAuthorizationsAsync_EmptySectionId_ThrowsException()
            {
                var addAuth = await AddAuthorizationRepository.GetStudentAddAuthorizationsAsync(string.Empty);

            }

            [TestMethod]
            public async Task GetStudentAddAuthorizationsAsync_BulkRead_ReturnsEmptyList()
            {
                Collection<AddAuthorizations> dataCollection = new Collection<AddAuthorizations>();
                dataReaderMock.Setup(sacc => sacc.BulkReadRecordAsync<AddAuthorizations>(It.IsAny<string>(), true)).ReturnsAsync(dataCollection);
                var addAuth = await AddAuthorizationRepository.GetStudentAddAuthorizationsAsync("StudentId");
                Assert.AreEqual(0, addAuth.Count());
            }

            [TestMethod]
            public async Task GetStudentAddAuthorizationsAsync_BulkRead_ReturnsNull()
            {
                Collection<AddAuthorizations> dataCollection = null;
                dataReaderMock.Setup(sacc => sacc.BulkReadRecordAsync<AddAuthorizations>(It.IsAny<string>(), true)).ReturnsAsync(dataCollection);
                var addAuth = await AddAuthorizationRepository.GetStudentAddAuthorizationsAsync("StudentId");
                Assert.AreEqual(0, addAuth.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task GetStudentAddAuthorizationsAsync_BulkRead_ThrowsException()
            {
                Exception ex = new Exception();
                dataReaderMock.Setup(sacc => sacc.BulkReadRecordAsync<AddAuthorizations>(It.IsAny<string>(), true)).ThrowsAsync(ex);
                var addAuth = await AddAuthorizationRepository.GetStudentAddAuthorizationsAsync("StudentId");
                loggerMock.Verify(l => l.Error(ex, string.Format("Bulk read failed for student {0} ", "StudentId")));
            }

            [TestMethod]
            public async Task GetStudentAddAuthorizationsAsync_Success()
            {
                var addAuthorizations = await AddAuthorizationRepository.GetStudentAddAuthorizationsAsync("StudentId");
                Assert.IsTrue(addAuthorizations is IEnumerable<AddAuthorization>);
                Assert.AreEqual(4, addAuthorizations.Count());
                foreach (var addAuth in addAuthorizations)
                {
                    var expectedDto = addAuthorizationContracts.Where(aa => aa.Recordkey == addAuth.Id).FirstOrDefault();
                    Assert.IsNotNull(expectedDto);
                    Assert.AreEqual(expectedDto.AauStudent, addAuth.StudentId);
                    Assert.AreEqual(expectedDto.AauCourseSection, addAuth.SectionId);
                    Assert.AreEqual(expectedDto.AauAuthorizationCode, addAuth.AddAuthorizationCode);
                    Assert.AreEqual(expectedDto.AauAssignedBy, addAuth.AssignedBy);
                    Assert.AreEqual(!string.IsNullOrEmpty(expectedDto.AauRevokedFlag) && expectedDto.AauRevokedFlag.Equals("Y", StringComparison.InvariantCultureIgnoreCase), addAuth.IsRevoked);
                }

            }


            private AddAuthorizationRepository BuildValidAddAuthorizationRepository(Collection<AddAuthorizations> addAuthorizationContracts)
            {
                dataReaderMock.Setup(sacc => sacc.BulkReadRecordAsync<AddAuthorizations>(It.IsAny<string>(), true)).ReturnsAsync(addAuthorizationContracts);

                AddAuthorizationRepository repository = new AddAuthorizationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }



        }

        [TestClass]
        public class AddAuthorizationRepository_CreateAddAuthorizationAsync : BaseRepositorySetup
        {

            DateTime? assignedTime;
            CreateAddAuthorizationRequest newTransactionRequest;
            string sectionId;
            AddAuthorization addAuthorizationToCreate;
            AddAuthorizations addAuthorizationResponseData;
            AddAuthorizationRepository AddAuthorizationRepository;


            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();
                assignedTime = new DateTime(2018, 1, 5, 9, 0, 0);
                sectionId = "sectionId";
                addAuthorizationToCreate = new AddAuthorization(null, sectionId)
                {
                    StudentId = "studentId",
                    AssignedBy = "facultyId",
                    AssignedTime = assignedTime,
                };


                // Collection of data accessor responses
                addAuthorizationResponseData = BuildAddAuthorizationResponse(addAuthorizationToCreate);
                AddAuthorizationRepository = BuildValidAddAuthorizationRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
                addAuthorizationResponseData = null;
                AddAuthorizationRepository = null;
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CreateAddAuthorization_ThrowsExceptionIfAddAuthorizationNull()
            {
                var AddAuthorizations = await AddAuthorizationRepository.CreateAddAuthorizationAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task CreateAddAuthorization_ThrowsExceptionIfColleagueTXThrows()
            {
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreateAddAuthorizationRequest, CreateAddAuthorizationResponse>(It.Is<CreateAddAuthorizationRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).ThrowsAsync(new Exception());
                var AddAuthorizations = await AddAuthorizationRepository.CreateAddAuthorizationAsync(addAuthorizationToCreate);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task CreateAddAuthorization_ThrowsExceptionIfColleagueTXReturnsNull()
            {
                CreateAddAuthorizationResponse nullResponse = null;
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreateAddAuthorizationRequest, CreateAddAuthorizationResponse>(It.Is < CreateAddAuthorizationRequest > (r => !string.IsNullOrEmpty(r.SectionId)))).ReturnsAsync(nullResponse).Callback<CreateAddAuthorizationRequest>(req => newTransactionRequest = req);
                var AddAuthorizations = await AddAuthorizationRepository.CreateAddAuthorizationAsync(addAuthorizationToCreate);
            }

            [TestMethod]
            [ExpectedException(typeof(ExistingResourceException))]
            public async Task CreateAddAuthorization_ThrowsExistingResourceException()
            {
                CreateAddAuthorizationResponse failureResponse = new CreateAddAuthorizationResponse()
                {
                    ErrorMessage = "Conflict: StudentId StudentId already has an authorization for section SecionId.",
                    ErrorOccurred = true
                };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreateAddAuthorizationRequest, CreateAddAuthorizationResponse>(It.Is<CreateAddAuthorizationRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).ReturnsAsync(failureResponse).Callback<CreateAddAuthorizationRequest>(req => newTransactionRequest = req);
                var AddAuthorizations = await AddAuthorizationRepository.CreateAddAuthorizationAsync(addAuthorizationToCreate);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task CreateAddAuthorization_ThrowsApplicationException()
            {
                CreateAddAuthorizationResponse failureResponse = new CreateAddAuthorizationResponse()
                {
                    ErrorMessage = "Invalid Section: Update is for a different section. Update not allowed.",
                    ErrorOccurred = true
                };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreateAddAuthorizationRequest, CreateAddAuthorizationResponse>(It.Is < CreateAddAuthorizationRequest > (r => !string.IsNullOrEmpty(r.SectionId)))).ReturnsAsync(failureResponse).Callback<CreateAddAuthorizationRequest>(req => newTransactionRequest = req);
                var AddAuthorizations = await AddAuthorizationRepository.CreateAddAuthorizationAsync(addAuthorizationToCreate);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task CreateAddAuthorization_CantFindNewAuthorization()
            {
                dataReaderMock.Setup(sacc => sacc.ReadRecordAsync<AddAuthorizations>(It.IsAny<string>(), true)).ThrowsAsync(new KeyNotFoundException());
                var AddAuthorizations = await AddAuthorizationRepository.CreateAddAuthorizationAsync(addAuthorizationToCreate);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task CreateAddAuthorization_Generic_Exception_thrown_on_GetAsync_of_New_Authorization()
            {
                dataReaderMock.Setup(sacc => sacc.ReadRecordAsync<AddAuthorizations>(It.IsAny<string>(), true)).ThrowsAsync(new ApplicationException());
                var AddAuthorizations = await AddAuthorizationRepository.CreateAddAuthorizationAsync(addAuthorizationToCreate);
            }

            [TestMethod]
            public async Task CreatAreeAddAuthorization_Success()
            {
                var newAddAuthorization = await AddAuthorizationRepository.CreateAddAuthorizationAsync(addAuthorizationToCreate);
                Assert.IsTrue(newAddAuthorization is AddAuthorization);
                Assert.IsFalse(newAddAuthorization.IsRevoked);
                Assert.AreEqual("addAuthorizationId", newAddAuthorization.Id);
                Assert.AreEqual(addAuthorizationToCreate.SectionId, newAddAuthorization.SectionId);
                Assert.AreEqual(addAuthorizationToCreate.StudentId, newAddAuthorization.StudentId);
                Assert.AreEqual(addAuthorizationToCreate.AddAuthorizationCode, newAddAuthorization.AddAuthorizationCode);
                Assert.AreEqual(addAuthorizationToCreate.AssignedBy, newAddAuthorization.AssignedBy);
            }


            private AddAuthorizations BuildAddAuthorizationResponse(AddAuthorization aa)
            {

                AddAuthorizations addContract = new AddAuthorizations()
                {
                    Recordkey = "addAuthorizationId",
                    AauCourseSection = aa.SectionId,
                    AauAuthorizationCode = aa.AddAuthorizationCode,
                    AauStudent = aa.StudentId,
                    AauAssignedBy = aa.AssignedBy,
                    AauRevokedBy = aa.RevokedBy,
                    AauRevokedFlag = aa.IsRevoked ? "Y" : "N",
                    AauAssignedDate = assignedTime,
                    AauAssignedTime = assignedTime,

                };
                return addContract;

            }


            private AddAuthorizationRepository BuildValidAddAuthorizationRepository()
            {



                // Successful response for a create
                var response = new CreateAddAuthorizationResponse()
                {
                    NewAddAuthorizationId = "addAuthorizationId",
                    ErrorOccurred = false

                };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreateAddAuthorizationRequest, CreateAddAuthorizationResponse>(It.Is<CreateAddAuthorizationRequest>(r => !string.IsNullOrEmpty(r.SectionId)))).ReturnsAsync(response).Callback<CreateAddAuthorizationRequest>(req => newTransactionRequest = req);

                // Mock responses needed to get the newly created AddAuthorization item....
                dataReaderMock.Setup(sacc => sacc.ReadRecordAsync<AddAuthorizations>(It.IsAny<string>(), true)).ReturnsAsync(addAuthorizationResponseData);

                AddAuthorizationRepository repository = new AddAuthorizationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return repository;
            }


        }
    }
}


