// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Runtime.Caching;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Ellucian.Data.Colleague;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Exceptions;
using Moq;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Repositories;
using System.Threading;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class GraduationApplicationRepositoryTests
    {
        [TestClass]
        public class GraduationApplicationRepository_GetGraduationApplication
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettings;
            private IGraduationApplicationRepository graduationApplicationRepository;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");
                cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                    null,
                    new SemaphoreSlim(1, 1)
                    )));
                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                graduationApplicationRepository = new GraduationApplicationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
            }

            [TestMethod]
            public async Task GetGraduationApplication_WithAllBoolAsTrue()
            {
                var studentId = "0000011";
                var programCode = "MATH.BA";
                var graduateApplicationId = string.Concat(studentId.ToUpper(), "*", programCode.ToUpper());
                Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate = new Ellucian.Colleague.Data.Student.DataContracts.Graduates();
                graduate.Recordkey = "0000011*MATH.BA";
                graduate.GradTerm = "2015/FA";
                graduate.GradAttendCommencement = "Y";
                graduate.GradCapSize = "5";
                graduate.GradCommencementDate = DateTime.Now;
                graduate.GradCommencementSite = "college";
                graduate.GradDiplomaName = "BS in Math";
                graduate.GradGownSize = "11";
                graduate.GradHometown = "PA";
                graduate.GradIncludeName = "Y";
                graduate.GradTranscriptAddress = new List<string>() { "Address 1", "Address 2" };
                graduate.GradTranscriptCity = "fairfax";
                graduate.GradTranscriptCountry = "USA";
                graduate.GradTranscriptZip = "111111";
                graduate.GradTranscriptState = "VA";
                graduate.GradNumberOfGuests = "12";
                graduate.GradPhoneticSpelling = "aaa";
                graduate.GradWillPickupDiploma = "y";
                graduate.GradInvoice = "INV123";
                dataAccessorMock.Setup<Task<Ellucian.Colleague.Data.Student.DataContracts.Graduates>>(acc => acc.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(It.IsAny<string>(), false)).Returns(Task.FromResult(graduate));
                var graduationApplicationEntity = await graduationApplicationRepository.GetGraduationApplicationAsync(studentId, programCode);
                Assert.IsNotNull(graduationApplicationEntity);
                Assert.AreEqual(graduationApplicationEntity.Id, graduate.Recordkey);
                Assert.AreEqual(graduationApplicationEntity.StudentId, studentId);
                Assert.AreEqual(graduationApplicationEntity.ProgramCode, programCode);
                Assert.AreEqual(graduationApplicationEntity.MailDiplomaToAddressLines, graduate.GradTranscriptAddress);
                Assert.IsTrue(graduationApplicationEntity.AttendingCommencement.Value);
                Assert.IsTrue(graduationApplicationEntity.IncludeNameInProgram.Value);
                Assert.IsTrue(graduationApplicationEntity.WillPickupDiploma.Value);
                Assert.AreEqual(graduate.GradInvoice, graduationApplicationEntity.InvoiceNumber);
            }

            [TestMethod]
            public async Task GetGraduationApplication_WithAllBoolAsFalse()
            {
                var studentId = "0000011";
                var programCode = "MATH.BA";
                var graduateApplicationId = string.Concat(studentId.ToUpper(), "*", programCode.ToUpper());
                Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate = new Ellucian.Colleague.Data.Student.DataContracts.Graduates();
                graduate.Recordkey = "0000011*MATH.BA";
                graduate.GradTerm = "2015/FA";
                graduate.GradAttendCommencement = "N";
                graduate.GradCapSize = "5";
                graduate.GradCommencementDate = DateTime.Now;
                graduate.GradCommencementSite = "college";
                graduate.GradDiplomaName = "BS in Math";
                graduate.GradGownSize = "11";
                graduate.GradHometown = "PA";
                graduate.GradIncludeName = "n";
                graduate.GradTranscriptAddress = new List<string>() { "Address 1", "Address 2" };
                graduate.GradTranscriptCity = "fairfax";
                graduate.GradTranscriptCountry = "USA";
                graduate.GradTranscriptZip = "111111";
                graduate.GradTranscriptState = "VA";
                graduate.GradNumberOfGuests = "12";
                graduate.GradPhoneticSpelling = "aaa";
                graduate.GradWillPickupDiploma = "N";
                graduate.GradInvoice = string.Empty;
                dataAccessorMock.Setup<Task<Ellucian.Colleague.Data.Student.DataContracts.Graduates>>(acc => acc.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(It.IsAny<string>(), false)).Returns(Task.FromResult(graduate));
                var graduationApplicationEntity = await graduationApplicationRepository.GetGraduationApplicationAsync(studentId, programCode);
                Assert.IsNotNull(graduationApplicationEntity);
                Assert.AreEqual(graduationApplicationEntity.Id, graduate.Recordkey);
                Assert.AreEqual(graduationApplicationEntity.StudentId, studentId);
                Assert.AreEqual(graduationApplicationEntity.ProgramCode, programCode);
                Assert.AreEqual(graduationApplicationEntity.MailDiplomaToAddressLines, graduate.GradTranscriptAddress);
                Assert.IsFalse(graduationApplicationEntity.AttendingCommencement.Value);
                Assert.IsFalse(graduationApplicationEntity.IncludeNameInProgram.Value);
                Assert.IsFalse(graduationApplicationEntity.WillPickupDiploma.Value);
                Assert.AreEqual(graduate.GradInvoice, graduationApplicationEntity.InvoiceNumber);
            }

            [TestMethod]
            public async Task GetGraduationApplication_WithAllBoolAsNull()
            {
                var studentId = "0000011";
                var programCode = "MATH.BA";
                var graduateApplicationId = string.Concat(studentId.ToUpper(), "*", programCode.ToUpper());
                Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate = new Ellucian.Colleague.Data.Student.DataContracts.Graduates();
                graduate.Recordkey = "0000011*MATH.BA";
                graduate.GradTerm = "2015/FA";
                graduate.GradAttendCommencement = "";
                graduate.GradCapSize = "5";
                graduate.GradCommencementDate = DateTime.Now;
                graduate.GradCommencementSite = "college";
                graduate.GradDiplomaName = "BS in Math";
                graduate.GradGownSize = "11";
                graduate.GradHometown = "PA";
                graduate.GradIncludeName = "";
                graduate.GradTranscriptAddress = new List<string>() { "Address 1", "Address 2" };
                graduate.GradTranscriptCity = "fairfax";
                graduate.GradTranscriptCountry = "USA";
                graduate.GradTranscriptZip = "111111";
                graduate.GradTranscriptState = "VA";
                graduate.GradNumberOfGuests = "12";
                graduate.GradPhoneticSpelling = "aaa";
                graduate.GradWillPickupDiploma = null;
                graduate.GradInvoice = null;
                dataAccessorMock.Setup<Task<Ellucian.Colleague.Data.Student.DataContracts.Graduates>>(acc => acc.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(It.IsAny<string>(), false)).Returns(Task.FromResult(graduate));
                var graduationApplicationEntity = await graduationApplicationRepository.GetGraduationApplicationAsync(studentId, programCode);
                Assert.IsNotNull(graduationApplicationEntity);
                Assert.AreEqual(graduationApplicationEntity.Id, graduate.Recordkey);
                Assert.AreEqual(graduationApplicationEntity.StudentId, studentId);
                Assert.AreEqual(graduationApplicationEntity.ProgramCode, programCode);
                Assert.AreEqual(graduationApplicationEntity.MailDiplomaToAddressLines, graduate.GradTranscriptAddress);
                Assert.IsNull(graduationApplicationEntity.AttendingCommencement);
                Assert.IsNull(graduationApplicationEntity.IncludeNameInProgram);
                Assert.IsNull(graduationApplicationEntity.WillPickupDiploma);
                Assert.IsNull(graduationApplicationEntity.InvoiceNumber);
            }

            [TestMethod]
            public async Task GetGraduationApplication_WithNoAddressLines()
            {
                var studentId = "0000011";
                var programCode = "MATH.BA";
                var graduateApplicationId = string.Concat(studentId.ToUpper(), "*", programCode.ToUpper());
                Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate = new Ellucian.Colleague.Data.Student.DataContracts.Graduates();
                graduate.Recordkey = "0000011*MATH.BA";
                graduate.GradTerm = "2015/FA";
                graduate.GradAttendCommencement = "";
                graduate.GradCapSize = "5";
                graduate.GradCommencementDate = DateTime.Now;
                graduate.GradCommencementSite = "college";
                graduate.GradDiplomaName = "BS in Math";
                graduate.GradGownSize = "11";
                graduate.GradHometown = "PA";
                graduate.GradIncludeName = "";
                graduate.GradTranscriptAddress = null;
                graduate.GradTranscriptCity = "fairfax";
                graduate.GradTranscriptCountry = "USA";
                graduate.GradTranscriptZip = "111111";
                graduate.GradTranscriptState = "VA";
                graduate.GradNumberOfGuests = "12";
                graduate.GradPhoneticSpelling = "aaa";
                graduate.GradWillPickupDiploma = "";
                dataAccessorMock.Setup<Task<Ellucian.Colleague.Data.Student.DataContracts.Graduates>>(acc => acc.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(It.IsAny<string>(), false)).Returns(Task.FromResult(graduate));
                var graduationApplicationEntity = await graduationApplicationRepository.GetGraduationApplicationAsync(studentId, programCode);
                Assert.IsNotNull(graduationApplicationEntity);
                Assert.AreEqual(graduationApplicationEntity.Id, graduate.Recordkey);
                Assert.AreEqual(graduationApplicationEntity.StudentId, studentId);
                Assert.AreEqual(graduationApplicationEntity.ProgramCode, programCode);
                Assert.AreEqual(graduationApplicationEntity.MailDiplomaToAddressLines, null);
                Assert.IsNull(graduationApplicationEntity.AttendingCommencement);
                Assert.IsNull(graduationApplicationEntity.IncludeNameInProgram);
                Assert.IsNull(graduationApplicationEntity.WillPickupDiploma);
                Assert.IsNull(graduationApplicationEntity.MilitaryStatus);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetGraduationApplication_ArgumentNullexception()
            {
                var graduationApplicationEntity = await graduationApplicationRepository.GetGraduationApplicationAsync(null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetGraduationApplication_KeyNotFoundException()
            {
                Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate = null;
                dataAccessorMock.Setup<Task<Ellucian.Colleague.Data.Student.DataContracts.Graduates>>(acc => acc.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(It.IsAny<string>(), false)).Returns(Task.FromResult(graduate));
                var graduationApplicationEntity = await graduationApplicationRepository.GetGraduationApplicationAsync("0000011", "MATH.BA");
            }

            [TestMethod]
            public async Task GetGraduationApplication_RecordKeyHasMultipleSeperator()
            {
                Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate = new Ellucian.Colleague.Data.Student.DataContracts.Graduates();
                graduate.Recordkey = "0000011***MATH.BA";
                graduate.GradTerm = "2015/FA";
                dataAccessorMock.Setup<Task<Ellucian.Colleague.Data.Student.DataContracts.Graduates>>(acc => acc.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(It.IsAny<string>(), false)).Returns(Task.FromResult(graduate));
                var graduationApplicationEntity = await graduationApplicationRepository.GetGraduationApplicationAsync("0000011", "MATH.BA");
                Assert.IsNotNull(graduationApplicationEntity);
                Assert.AreEqual(graduationApplicationEntity.Id, graduate.Recordkey);
                Assert.AreEqual(graduationApplicationEntity.StudentId, "0000011");
                Assert.AreEqual(graduationApplicationEntity.ProgramCode, "MATH.BA");
            }

            [TestMethod]
            public async Task GetGraduationApplication_RecordKeyHasPrecededMultipleSeperator()
            {
                Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate = new Ellucian.Colleague.Data.Student.DataContracts.Graduates();
                graduate.Recordkey = "**0000011***MATH.BA";
                graduate.GradTerm = "2015/FA";
                dataAccessorMock.Setup<Task<Ellucian.Colleague.Data.Student.DataContracts.Graduates>>(acc => acc.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(It.IsAny<string>(), false)).Returns(Task.FromResult(graduate));
                var graduationApplicationEntity = await graduationApplicationRepository.GetGraduationApplicationAsync("0000011", "MATH.BA");
                Assert.IsNotNull(graduationApplicationEntity);
                Assert.AreEqual(graduationApplicationEntity.Id, graduate.Recordkey);
                Assert.AreEqual(graduationApplicationEntity.StudentId, "0000011");
                Assert.AreEqual(graduationApplicationEntity.ProgramCode, "MATH.BA");
            }
            [TestMethod]
            public async Task GetGraduationApplication_RecordKeyHasPostfixMultipleSeperator()
            {
                Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate = new Ellucian.Colleague.Data.Student.DataContracts.Graduates();
                graduate.Recordkey = "0000011***MATH.BA**";
                graduate.GradTerm = "2015/FA";
                dataAccessorMock.Setup<Task<Ellucian.Colleague.Data.Student.DataContracts.Graduates>>(acc => acc.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(It.IsAny<string>(), false)).Returns(Task.FromResult(graduate));
                var graduationApplicationEntity = await graduationApplicationRepository.GetGraduationApplicationAsync("0000011", "MATH.BA");
                Assert.IsNotNull(graduationApplicationEntity);
                Assert.AreEqual(graduationApplicationEntity.Id, graduate.Recordkey);
                Assert.AreEqual(graduationApplicationEntity.StudentId, "0000011");
                Assert.AreEqual(graduationApplicationEntity.ProgramCode, "MATH.BA");
            }
            [TestMethod]
            public async Task GetGraduationApplication_RecordKeyHasEverywhereMultipleSeperator()
            {
                Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate = new Ellucian.Colleague.Data.Student.DataContracts.Graduates();
                graduate.Recordkey = "**0000011***MATH.BA***";
                graduate.GradTerm = "2015/FA";
                dataAccessorMock.Setup<Task<Ellucian.Colleague.Data.Student.DataContracts.Graduates>>(acc => acc.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(It.IsAny<string>(), false)).Returns(Task.FromResult(graduate));
                var graduationApplicationEntity = await graduationApplicationRepository.GetGraduationApplicationAsync("0000011", "MATH.BA");
                Assert.IsNotNull(graduationApplicationEntity);
                Assert.AreEqual(graduationApplicationEntity.Id, graduate.Recordkey);
                Assert.AreEqual(graduationApplicationEntity.StudentId, "0000011");
                Assert.AreEqual(graduationApplicationEntity.ProgramCode, "MATH.BA");
            }

            [TestMethod]
            [ExpectedException(typeof(IndexOutOfRangeException))]
            public async Task GetGraduationApplication_RecordKeyHasOnlyOneValue()
            {
                Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate = new Ellucian.Colleague.Data.Student.DataContracts.Graduates();
                graduate.Recordkey = "0000011**";
                graduate.GradTerm = "2015/FA";
                dataAccessorMock.Setup<Task<Ellucian.Colleague.Data.Student.DataContracts.Graduates>>(acc => acc.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(It.IsAny<string>(), false)).Returns(Task.FromResult(graduate));
                var graduationApplicationEntity = await graduationApplicationRepository.GetGraduationApplicationAsync("0000011", "MATH.BA");
            }
            [TestMethod]
            [ExpectedException(typeof(IndexOutOfRangeException))]
            public async Task GetGraduationApplication_RecordKeyHasOnlyOneValueWithPrecededChars()
            {
                Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate = new Ellucian.Colleague.Data.Student.DataContracts.Graduates();
                graduate.Recordkey = "**0000011";
                graduate.GradTerm = "2015/FA";
                dataAccessorMock.Setup<Task<Ellucian.Colleague.Data.Student.DataContracts.Graduates>>(acc => acc.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(It.IsAny<string>(), false)).Returns(Task.FromResult(graduate));
                var graduationApplicationEntity = await graduationApplicationRepository.GetGraduationApplicationAsync("0000011", "MATH.BA");
            }

            [TestMethod]
            public async Task GetGraduationApplication_WithActiveMilitaryStatus()
            {
                var studentId = "0000011";
                var programCode = "MATH.BA";
                var graduateApplicationId = string.Concat(studentId.ToUpper(), "*", programCode.ToUpper());
                Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate = new Ellucian.Colleague.Data.Student.DataContracts.Graduates();
                graduate.Recordkey = "0000011*MATH.BA";
                graduate.GradTerm = "2015/FA";
                graduate.GradAttendCommencement = "";
                graduate.GradMilitaryStatus = "a";
           
                graduate.GradWillPickupDiploma = "";
                dataAccessorMock.Setup<Task<Ellucian.Colleague.Data.Student.DataContracts.Graduates>>(acc => acc.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(It.IsAny<string>(), false)).Returns(Task.FromResult(graduate));
                var graduationApplicationEntity = await graduationApplicationRepository.GetGraduationApplicationAsync(studentId, programCode);
                Assert.IsNotNull(graduationApplicationEntity);
                Assert.AreEqual(graduationApplicationEntity.Id, graduate.Recordkey);
                Assert.AreEqual(graduationApplicationEntity.StudentId, studentId);
                Assert.AreEqual(graduationApplicationEntity.ProgramCode, programCode);
                Assert.AreEqual(graduationApplicationEntity.MailDiplomaToAddressLines, null);
                Assert.IsNull(graduationApplicationEntity.AttendingCommencement);
                Assert.IsNull(graduationApplicationEntity.IncludeNameInProgram);
                Assert.IsNull(graduationApplicationEntity.WillPickupDiploma);
                Assert.AreEqual(GraduateMilitaryStatus.ActiveMilitary, graduationApplicationEntity.MilitaryStatus);
            }

            [TestMethod]
            public async Task GetGraduationApplication_WithVeteranMilitaryStatus()
            {
                var studentId = "0000011";
                var programCode = "MATH.BA";
                var graduateApplicationId = string.Concat(studentId.ToUpper(), "*", programCode.ToUpper());
                Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate = new Ellucian.Colleague.Data.Student.DataContracts.Graduates();
                graduate.Recordkey = "0000011*MATH.BA";
                graduate.GradTerm = "2015/FA";
                graduate.GradAttendCommencement = "";
                graduate.GradMilitaryStatus = "v";

                graduate.GradWillPickupDiploma = "";
                dataAccessorMock.Setup<Task<Ellucian.Colleague.Data.Student.DataContracts.Graduates>>(acc => acc.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(It.IsAny<string>(), false)).Returns(Task.FromResult(graduate));
                var graduationApplicationEntity = await graduationApplicationRepository.GetGraduationApplicationAsync(studentId, programCode);
                Assert.IsNotNull(graduationApplicationEntity);
                Assert.AreEqual(graduationApplicationEntity.Id, graduate.Recordkey);
                Assert.AreEqual(graduationApplicationEntity.StudentId, studentId);
                Assert.AreEqual(graduationApplicationEntity.ProgramCode, programCode);
                Assert.AreEqual(GraduateMilitaryStatus.Veteran, graduationApplicationEntity.MilitaryStatus);
            }

            [TestMethod]
            public async Task GetGraduationApplication_WithNoMilitaryStatus()
            {
                var studentId = "0000011";
                var programCode = "MATH.BA";
                var graduateApplicationId = string.Concat(studentId.ToUpper(), "*", programCode.ToUpper());
                Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate = new Ellucian.Colleague.Data.Student.DataContracts.Graduates();
                graduate.Recordkey = "0000011*MATH.BA";
                graduate.GradTerm = "2015/FA";
                graduate.GradMilitaryStatus = "N";

                dataAccessorMock.Setup<Task<Ellucian.Colleague.Data.Student.DataContracts.Graduates>>(acc => acc.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(It.IsAny<string>(), false)).Returns(Task.FromResult(graduate));
                var graduationApplicationEntity = await graduationApplicationRepository.GetGraduationApplicationAsync(studentId, programCode);
                Assert.IsNotNull(graduationApplicationEntity);
                Assert.AreEqual(graduationApplicationEntity.Id, graduate.Recordkey);
                Assert.AreEqual(graduationApplicationEntity.StudentId, studentId);
                Assert.AreEqual(graduationApplicationEntity.ProgramCode, programCode);
                Assert.AreEqual(GraduateMilitaryStatus.NotApplicable, graduationApplicationEntity.MilitaryStatus);
            }

            [TestMethod]
            public async Task GetGraduationApplication_WithMultiLineSpecialAccommodations()
            {
                var studentId = "0000011";
                var programCode = "MATH.BA";
                var graduateApplicationId = string.Concat(studentId.ToUpper(), "*", programCode.ToUpper());
                Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate = new Ellucian.Colleague.Data.Student.DataContracts.Graduates();
                graduate.Recordkey = "0000011*MATH.BA";
                graduate.GradSpecialAccommodations = "Will require " + Convert.ToChar(DynamicArray.VM) + "wheelchair access.";

                dataAccessorMock.Setup<Task<Ellucian.Colleague.Data.Student.DataContracts.Graduates>>(acc => acc.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(It.IsAny<string>(), false)).Returns(Task.FromResult(graduate));
                var graduationApplicationEntity = await graduationApplicationRepository.GetGraduationApplicationAsync(studentId, programCode);
                Assert.IsNotNull(graduationApplicationEntity);
                Assert.AreEqual(graduationApplicationEntity.Id, graduate.Recordkey);
                Assert.AreEqual(graduationApplicationEntity.StudentId, studentId);
                Assert.AreEqual(graduationApplicationEntity.ProgramCode, programCode);
                Assert.AreEqual(graduate.GradSpecialAccommodations.Replace(Convert.ToChar(DynamicArray.VM), '\n'), graduationApplicationEntity.SpecialAccommodations);
            }
        }

        [TestClass]
        public class GraduationApplicationRepository_CreateGraduationApplication
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettings;
            Mock<IColleagueTransactionInvoker> mockManager;
            private IGraduationApplicationRepository graduationApplicationRepository;
            AddGraduationApplicationRequest createRequest;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");
                cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                mockManager = new Mock<IColleagueTransactionInvoker>();
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                    null,
                    new SemaphoreSlim(1, 1)
                    )));
                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                // Set up successful response to a transaction request, capturing the completed request for verification
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                graduationApplicationRepository = new GraduationApplicationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object,  apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
            }
          
            [TestMethod]
            public async Task CreateGraduationApplication_WithAllBoolAsTrue()
            {
                var graduationEntity = BuildGraduationEntityRequest_WithTrueValues();
                var graduate = BuildGraduateResponse_WithYValues();
                AddGraduationApplicationResponse createResponse = new AddGraduationApplicationResponse();
                createResponse.ErrorOccurred = false;
                createResponse.ErrorMessage = "";
                createResponse.AlreadyExists = false;
                createResponse.GraduationApplicationId = "0000011*MATH.BA";
                mockManager.Setup(mgr => mgr.ExecuteAsync<AddGraduationApplicationRequest, AddGraduationApplicationResponse>(It.IsAny<AddGraduationApplicationRequest>())).Returns(Task.FromResult(createResponse)).Callback<AddGraduationApplicationRequest>(req => createRequest = req);
                dataAccessorMock.Setup<Task<Ellucian.Colleague.Data.Student.DataContracts.Graduates>>(acc => acc.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(It.IsAny<string>(), false)).Returns(Task.FromResult(graduate));
                var graduationApplication = await graduationApplicationRepository.CreateGraduationApplicationAsync(graduationEntity);
                //compare transaction request 
                Assert.AreEqual(createRequest.AttendCommencement, "Y");
                Assert.AreEqual(createRequest.IncludeNameInProgram, "Y");
                Assert.AreEqual(createRequest.WillPickupDiploma, "Y");
                //compare graduation application response
                Assert.AreEqual(graduationApplication.AttendingCommencement, true);
                Assert.AreEqual(graduationApplication.IncludeNameInProgram, true);
                Assert.AreEqual(graduationApplication.WillPickupDiploma, true);
                Assert.AreEqual(graduationApplication.MilitaryStatus, GraduateMilitaryStatus.ActiveMilitary);
                Assert.AreEqual(graduationApplication.SpecialAccommodations, "Special accommodation Line 1\nSpecial accommodation Line 2");
            }

            [TestMethod]
            public async Task CreateGraduationApplication_WithAllBoolAsFalse()
            {
                var graduationEntity = BuildGraduationEntityRequest_WithFalseValues();
                var graduate = BuildGraduateResponse_WithNValues();
                AddGraduationApplicationResponse createResponse = new AddGraduationApplicationResponse();
                createResponse.ErrorOccurred = false;
                createResponse.ErrorMessage = "";
                createResponse.AlreadyExists = false;
                createResponse.GraduationApplicationId = "0000011*MATH.BA";
                mockManager.Setup(mgr => mgr.ExecuteAsync<AddGraduationApplicationRequest, AddGraduationApplicationResponse>(It.IsAny<AddGraduationApplicationRequest>())).Returns(Task.FromResult(createResponse)).Callback<AddGraduationApplicationRequest>(req => createRequest = req);
                dataAccessorMock.Setup<Task<Ellucian.Colleague.Data.Student.DataContracts.Graduates>>(acc => acc.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(It.IsAny<string>(), false)).Returns(Task.FromResult(graduate));
                var graduationApplication = await graduationApplicationRepository.CreateGraduationApplicationAsync(graduationEntity);
                //compare transaction request 
                Assert.AreEqual(createRequest.AttendCommencement, "N");
                Assert.AreEqual(createRequest.IncludeNameInProgram, "N");
                Assert.AreEqual(createRequest.WillPickupDiploma, "N");
                //compare graduation application response
                Assert.AreEqual(graduationApplication.AttendingCommencement, false);
                Assert.AreEqual(graduationApplication.IncludeNameInProgram, false);
                Assert.AreEqual(graduationApplication.WillPickupDiploma, false);
                Assert.AreEqual(graduationApplication.MilitaryStatus, GraduateMilitaryStatus.NotApplicable);
                Assert.AreEqual(graduationApplication.SpecialAccommodations, "Special accommodation Line 1\nSpecial accommodation Line 2");
            }

            [TestMethod]
            public async Task CreateGraduationApplication_WithAllNull()
            {
                var graduationEntity = BuildGraduationEntityRequest_WithNullValues();
                var graduate = BuildGraduateResponse_WithEmptyValues();
                AddGraduationApplicationResponse createResponse = new AddGraduationApplicationResponse();
                createResponse.ErrorOccurred = false;
                createResponse.ErrorMessage = "";
                createResponse.AlreadyExists = false;
                createResponse.GraduationApplicationId = "0000011*MATH.BA";
                mockManager.Setup(mgr => mgr.ExecuteAsync<AddGraduationApplicationRequest, AddGraduationApplicationResponse>(It.IsAny<AddGraduationApplicationRequest>())).Returns(Task.FromResult(createResponse)).Callback<AddGraduationApplicationRequest>(req => createRequest = req);
                dataAccessorMock.Setup<Task<Ellucian.Colleague.Data.Student.DataContracts.Graduates>>(acc => acc.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(It.IsAny<string>(), false)).Returns(Task.FromResult(graduate));
                var graduationApplication = await graduationApplicationRepository.CreateGraduationApplicationAsync(graduationEntity);
                //compare transaction request 
                Assert.AreEqual(createRequest.AttendCommencement, string.Empty);
                Assert.AreEqual(createRequest.IncludeNameInProgram, string.Empty);
                Assert.AreEqual(createRequest.WillPickupDiploma, string.Empty);
                //compare graduation application response
                Assert.AreEqual(graduationApplication.AttendingCommencement, null);
                Assert.AreEqual(graduationApplication.IncludeNameInProgram, null);
                Assert.AreEqual(graduationApplication.WillPickupDiploma, null);
                Assert.IsNull(graduationApplication.MilitaryStatus);
                Assert.IsNull(graduationApplication.SpecialAccommodations);
            }

            [TestMethod]
            [ExpectedException(typeof(ExistingResourceException))]
            public async Task CreateGraduationApplication_AlreadyExists()
            {
                var graduationEntity = BuildGraduationEntityRequest_WithTrueValues();
                AddGraduationApplicationResponse createResponse = new AddGraduationApplicationResponse();
                createResponse.ErrorOccurred = true;
                createResponse.ErrorMessage = "Already exists";
                createResponse.AlreadyExists = true;
                createResponse.GraduationApplicationId = "0000011*MATH.BA";
                mockManager.Setup(mgr => mgr.ExecuteAsync<AddGraduationApplicationRequest, AddGraduationApplicationResponse>(It.IsAny<AddGraduationApplicationRequest>())).Returns(Task.FromResult(createResponse)).Callback<AddGraduationApplicationRequest>(req => createRequest = req);
                var graduationApplication = await graduationApplicationRepository.CreateGraduationApplicationAsync(graduationEntity);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task CreateGraduationApplication_ResponseHasErrorMessage()
            {
                var graduationEntity = BuildGraduationEntityRequest_WithTrueValues();
                AddGraduationApplicationResponse createResponse = new AddGraduationApplicationResponse();
                createResponse.ErrorOccurred = true;
                createResponse.ErrorMessage = "some kind of eror text";
                createResponse.AlreadyExists = false;
                createResponse.GraduationApplicationId = "0000011*MATH.BA";
                mockManager.Setup(mgr => mgr.ExecuteAsync<AddGraduationApplicationRequest, AddGraduationApplicationResponse>(It.IsAny<AddGraduationApplicationRequest>())).Returns(Task.FromResult(createResponse)).Callback<AddGraduationApplicationRequest>(req => createRequest = req);
                var graduationApplication = await graduationApplicationRepository.CreateGraduationApplicationAsync(graduationEntity);
            }

            private GraduationApplication BuildGraduationEntityResponse()
            {
                    var graduationEntity = new GraduationApplication("0000011*MATH.BA", "0000011", "MATH.BA");
                graduationEntity.AttendingCommencement = true;
                    graduationEntity.GraduationTerm = "2015/FA";
                graduationEntity.CapSize = "5";
                graduationEntity.CommencementDate = DateTime.Now;
                graduationEntity.CommencementLocation = "college";
                graduationEntity.DiplomaName = "BS in Math";
                graduationEntity.GownSize = "11";
                graduationEntity.Hometown = "PA";
                graduationEntity.IncludeNameInProgram = true;
                graduationEntity.MailDiplomaToAddressLines = new List<string>() { "Address 1", "Address 2" };
                graduationEntity.MailDiplomaToCity = "fairfax";
                graduationEntity.MailDiplomaToCountry = "USA";
                graduationEntity.MailDiplomaToPostalCode = "111111";
                graduationEntity.MailDiplomaToState = "VA";
                graduationEntity.NumberOfGuests = 12;
                graduationEntity.PhoneticSpellingOfName = "aaa";
                graduationEntity.WillPickupDiploma = true;
                return graduationEntity;
            }
            private GraduationApplication BuildGraduationEntityRequest_WithTrueValues()
            {
                    var graduationEntity = new GraduationApplication("", "0000011", "MATH.BA");
                graduationEntity.AttendingCommencement = true;
                graduationEntity.CapSize = "5";
                graduationEntity.CommencementDate = DateTime.Now;
                graduationEntity.CommencementLocation = "college";
                graduationEntity.DiplomaName = "BS in Math";
                graduationEntity.GownSize = "11";
                graduationEntity.Hometown = "PA";
                graduationEntity.IncludeNameInProgram = true;
                graduationEntity.MailDiplomaToAddressLines = new List<string>() { "Address 1", "Address 2" };
                graduationEntity.MailDiplomaToCity = "fairfax";
                graduationEntity.MailDiplomaToCountry = "USA";
                graduationEntity.MailDiplomaToPostalCode = "111111";
                graduationEntity.MailDiplomaToState = "VA";
                graduationEntity.NumberOfGuests = 12;
                graduationEntity.PhoneticSpellingOfName = "aaa";
                graduationEntity.WillPickupDiploma = true;
                graduationEntity.MilitaryStatus = GraduateMilitaryStatus.ActiveMilitary;
                graduationEntity.SpecialAccommodations = "Special accommodations Line 1\nSpecial accommodations Line 2";
                graduationEntity.AcadCredentialsUpdated = true;
                return graduationEntity;
            }
            private GraduationApplication BuildGraduationEntityRequest_WithFalseValues()
            {
                    var graduationEntity = new GraduationApplication("", "0000011", "MATH.BA");
                graduationEntity.AttendingCommencement = false;
                graduationEntity.CapSize = "5";
                graduationEntity.CommencementDate = DateTime.Now;
                graduationEntity.CommencementLocation = "college";
                graduationEntity.DiplomaName = "BS in Math";
                graduationEntity.GownSize = "11";
                graduationEntity.Hometown = "PA";
                graduationEntity.IncludeNameInProgram = false;
                graduationEntity.MailDiplomaToAddressLines = new List<string>() { "Address 1", "Address 2" };
                graduationEntity.MailDiplomaToCity = "fairfax";
                graduationEntity.MailDiplomaToCountry = "USA";
                graduationEntity.MailDiplomaToPostalCode = "111111";
                graduationEntity.MailDiplomaToState = "VA";
                graduationEntity.NumberOfGuests = 12;
                graduationEntity.PhoneticSpellingOfName = "aaa";
                graduationEntity.WillPickupDiploma = false;
                graduationEntity.MilitaryStatus = GraduateMilitaryStatus.NotApplicable;
                graduationEntity.SpecialAccommodations = "";
                graduationEntity.AcadCredentialsUpdated = false;
                return graduationEntity;
            }
            private GraduationApplication BuildGraduationEntityRequest_WithNullValues()
            {
                    var graduationEntity = new GraduationApplication("", "0000011", "MATH.BA");
                graduationEntity.AttendingCommencement = null;
                graduationEntity.CapSize = "5";
                graduationEntity.CommencementDate = DateTime.Now;
                graduationEntity.CommencementLocation = "college";
                graduationEntity.DiplomaName = "BS in Math";
                graduationEntity.GownSize = "11";
                graduationEntity.Hometown = "PA";
                graduationEntity.IncludeNameInProgram = null;
                graduationEntity.MailDiplomaToAddressLines = new List<string>() { "Address 1", "Address 2" };
                graduationEntity.MailDiplomaToCity = "fairfax";
                graduationEntity.MailDiplomaToCountry = "USA";
                graduationEntity.MailDiplomaToPostalCode = "111111";
                graduationEntity.MailDiplomaToState = "VA";
                graduationEntity.NumberOfGuests = 12;
                graduationEntity.PhoneticSpellingOfName = "aaa";
                graduationEntity.WillPickupDiploma = null;
                graduationEntity.MilitaryStatus = null;
                graduationEntity.SpecialAccommodations = null;
                return graduationEntity;
            }
            private Ellucian.Colleague.Data.Student.DataContracts.Graduates BuildGraduateResponse_WithYValues()
            {
                
                Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate = new Ellucian.Colleague.Data.Student.DataContracts.Graduates();
                graduate.Recordkey = "0000011*MATH.BA";
                graduate.GradTerm = "2015/FA";
                graduate.GradAttendCommencement = "Y";
                graduate.GradCapSize = "5";
                graduate.GradCommencementDate = DateTime.Now;
                graduate.GradCommencementSite = "college";
                graduate.GradDiplomaName = "BS in Math";
                graduate.GradGownSize = "11";
                graduate.GradHometown = "PA";
                graduate.GradIncludeName = "Y";
                graduate.GradTranscriptAddress = new List<string>() { "Address 1", "Address 2" };
                graduate.GradTranscriptCity = "fairfax";
                graduate.GradTranscriptCountry = "USA";
                graduate.GradTranscriptZip = "111111";
                graduate.GradTranscriptState = "VA";
                graduate.GradNumberOfGuests = "12";
                graduate.GradPhoneticSpelling = "aaa";
                graduate.GradWillPickupDiploma = "Y";
                graduate.GradMilitaryStatus = "A";
                graduate.GradSpecialAccommodations = "Special accommodation Line 1" + Convert.ToChar(DynamicArray.VM) + "Special accommodation Line 2";
                graduate.GradAcadCredentialsUpdted = "Y";
                return graduate;
            }
            private Ellucian.Colleague.Data.Student.DataContracts.Graduates BuildGraduateResponse_WithNValues()
            {

                Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate = new Ellucian.Colleague.Data.Student.DataContracts.Graduates();
                graduate.Recordkey = "0000011*MATH.BA";
                graduate.GradTerm = "2015/FA";
                graduate.GradAttendCommencement = "N";
                graduate.GradCapSize = "5";
                graduate.GradCommencementDate = DateTime.Now;
                graduate.GradCommencementSite = "college";
                graduate.GradDiplomaName = "BS in Math";
                graduate.GradGownSize = "11";
                graduate.GradHometown = "PA";
                graduate.GradIncludeName = "N";
                graduate.GradTranscriptAddress = new List<string>() { "Address 1", "Address 2" };
                graduate.GradTranscriptCity = "fairfax";
                graduate.GradTranscriptCountry = "USA";
                graduate.GradTranscriptZip = "111111";
                graduate.GradTranscriptState = "VA";
                graduate.GradNumberOfGuests = "12";
                graduate.GradPhoneticSpelling = "aaa";
                graduate.GradWillPickupDiploma = "N";
                graduate.GradMilitaryStatus = "N";
                graduate.GradSpecialAccommodations = "Special accommodation Line 1" + Convert.ToChar(DynamicArray.VM) + "Special accommodation Line 2";
                graduate.GradAcadCredentialsUpdted = "N";
                return graduate;
            }
            private Ellucian.Colleague.Data.Student.DataContracts.Graduates BuildGraduateResponse_WithEmptyValues()
            {

                Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate = new Ellucian.Colleague.Data.Student.DataContracts.Graduates();
                graduate.Recordkey = "0000011*MATH.BA";
                graduate.GradTerm = "2015/FA";
                graduate.GradAttendCommencement = "";
                graduate.GradCapSize = "5";
                graduate.GradCommencementDate = DateTime.Now;
                graduate.GradCommencementSite = "college";
                graduate.GradDiplomaName = "BS in Math";
                graduate.GradGownSize = "11";
                graduate.GradHometown = "PA";
                graduate.GradIncludeName = "";
                graduate.GradTranscriptAddress = new List<string>() { "Address 1", "Address 2" };
                graduate.GradTranscriptCity = "fairfax";
                graduate.GradTranscriptCountry = "USA";
                graduate.GradTranscriptZip = "111111";
                graduate.GradTranscriptState = "VA";
                graduate.GradNumberOfGuests = "12";
                graduate.GradPhoneticSpelling = "aaa";
                graduate.GradWillPickupDiploma = "";
                graduate.GradMilitaryStatus = "";
                graduate.GradSpecialAccommodations = "";
                return graduate;
            }
          }

          [TestClass]
          public class GraduationApplicationRepository_GetGraduationApplications
          {
               Mock<IColleagueTransactionFactory> transFactoryMock;
               Mock<IColleagueDataReader> dataAccessorMock;
               Mock<ICacheProvider> cacheProviderMock;
               Mock<ILogger> loggerMock;
               ApiSettings apiSettings;
               private IGraduationApplicationRepository graduationApplicationRepository;

               Collection<Ellucian.Colleague.Data.Student.DataContracts.Graduates> graduates; 

               [TestInitialize]
               public void Initialize()
               {
                    loggerMock = new Mock<ILogger>();
                    apiSettings = new ApiSettings("TEST");
                    cacheProviderMock = new Mock<ICacheProvider>();
                    transFactoryMock = new Mock<IColleagueTransactionFactory>();
                    dataAccessorMock = new Mock<IColleagueDataReader>();
                    cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                        null,
                        new SemaphoreSlim(1, 1)
                        )));
                    // Set up data accessor for the transaction factory 
                    transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                    graduationApplicationRepository = new GraduationApplicationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                    graduates = new Collection<Ellucian.Colleague.Data.Student.DataContracts.Graduates>();
               }

               [TestCleanup]
               public void Cleanup()
               {
                    transFactoryMock = null;
                    dataAccessorMock = null;
                    cacheProviderMock = null;
                    graduates = null;
               }

               [TestMethod]
               public async Task GetGraduationApplications_WithAllBoolAsTrue()
               {
                    string studentId = "0000011";
                    CreateApplications("Y");
                    dataAccessorMock.Setup<Task<Collection<Ellucian.Colleague.Data.Student.DataContracts.Graduates>>>(acc => acc.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(It.IsAny<string>(), false)).ReturnsAsync(graduates);
                    var graduationApplicationEntities = await graduationApplicationRepository.GetGraduationApplicationsAsync(studentId);
                    Assert.AreEqual(2, graduationApplicationEntities.Count);
                    Assert.IsNotNull(graduationApplicationEntities);
                    Assert.AreEqual(graduates[0].Recordkey, graduationApplicationEntities[0].Id);
                    Assert.AreEqual(graduates[0].GradTranscriptAddress, graduationApplicationEntities[0].MailDiplomaToAddressLines);
                    Assert.IsTrue(graduationApplicationEntities[0].AttendingCommencement.Value);
                    Assert.IsTrue(graduationApplicationEntities[0].IncludeNameInProgram.Value);
                    Assert.IsTrue(graduationApplicationEntities[0].WillPickupDiploma.Value);
                    Assert.IsTrue(graduationApplicationEntities[0].AcadCredentialsUpdated);

                    //Make sure second application comes out correctly
                    Assert.IsNotNull(graduationApplicationEntities);
                    Assert.AreEqual(graduates[1].Recordkey, graduationApplicationEntities[1].Id);
                    Assert.AreEqual(graduates[1].GradTranscriptAddress, graduationApplicationEntities[1].MailDiplomaToAddressLines);
                    Assert.IsTrue(graduationApplicationEntities[1].AttendingCommencement.Value);
                    Assert.IsTrue(graduationApplicationEntities[1].IncludeNameInProgram.Value);
                    Assert.IsTrue(graduationApplicationEntities[1].WillPickupDiploma.Value);
                    Assert.IsTrue(graduationApplicationEntities[1].AcadCredentialsUpdated);
               }

               [TestMethod]
               public async Task GetGraduationApplications_WithAllBoolAsFalse()
               {
                    string studentId = "0000011";
                    CreateApplications("N");
                    dataAccessorMock.Setup<Task<Collection<Ellucian.Colleague.Data.Student.DataContracts.Graduates>>>(acc => acc.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(It.IsAny<string>(), false)).ReturnsAsync(graduates);
                    var graduationApplicationEntities = await graduationApplicationRepository.GetGraduationApplicationsAsync(studentId);
                    Assert.AreEqual(2, graduationApplicationEntities.Count);
                    Assert.IsNotNull(graduationApplicationEntities);
                    Assert.AreEqual(graduates[0].Recordkey, graduationApplicationEntities[0].Id);
                    Assert.AreEqual(graduates[0].GradTranscriptAddress, graduationApplicationEntities[0].MailDiplomaToAddressLines);
                    Assert.IsFalse(graduationApplicationEntities[0].AttendingCommencement.Value);
                    Assert.IsFalse(graduationApplicationEntities[0].IncludeNameInProgram.Value);
                    Assert.IsFalse(graduationApplicationEntities[0].WillPickupDiploma.Value);
                    Assert.IsFalse(graduationApplicationEntities[0].AcadCredentialsUpdated);

                    //Make sure second application comes out correctly
                    Assert.IsNotNull(graduationApplicationEntities);
                    Assert.AreEqual(graduates[1].Recordkey, graduationApplicationEntities[1].Id);
                    Assert.AreEqual(graduates[1].GradTranscriptAddress, graduationApplicationEntities[1].MailDiplomaToAddressLines);
                    Assert.IsFalse(graduationApplicationEntities[1].AttendingCommencement.Value);
                    Assert.IsFalse(graduationApplicationEntities[1].IncludeNameInProgram.Value);
                    Assert.IsFalse(graduationApplicationEntities[1].WillPickupDiploma.Value);
                    Assert.IsFalse(graduationApplicationEntities[1].AcadCredentialsUpdated);
               }


               [TestMethod]
               public async Task GetGraduationApplications_WithAllBoolAsNull()
               {
                    string studentId = "0000011";
                    CreateApplications(null);
                    dataAccessorMock.Setup<Task<Collection<Ellucian.Colleague.Data.Student.DataContracts.Graduates>>>(acc => acc.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(It.IsAny<string>(), false)).ReturnsAsync(graduates);
                    var graduationApplicationEntities = await graduationApplicationRepository.GetGraduationApplicationsAsync(studentId);
                    Assert.IsNotNull(graduationApplicationEntities);
                    Assert.AreEqual(graduates[0].Recordkey, graduationApplicationEntities[0].Id);
                    Assert.AreEqual(graduates[0].GradTranscriptAddress, graduationApplicationEntities[0].MailDiplomaToAddressLines);
                    Assert.IsNull(graduationApplicationEntities[0].AttendingCommencement);
                    Assert.IsNull(graduationApplicationEntities[0].IncludeNameInProgram);
                    Assert.IsNull(graduationApplicationEntities[0].WillPickupDiploma);
                    //same assertions for second record
                    Assert.IsNotNull(graduationApplicationEntities);
                    Assert.AreEqual(graduates[1].Recordkey, graduationApplicationEntities[1].Id);
                    Assert.AreEqual(graduates[1].GradTranscriptAddress, graduationApplicationEntities[1].MailDiplomaToAddressLines);
                    Assert.IsNull(graduationApplicationEntities[1].AttendingCommencement);
                    Assert.IsNull(graduationApplicationEntities[1].IncludeNameInProgram);
                    Assert.IsNull(graduationApplicationEntities[1].WillPickupDiploma);
               }

               [TestMethod]
               public async Task GetGraduationApplications_WithNoAddressLines()
               {
                    string studentId = "0000011";
                    CreateApplications("Y");
                    //Get rid of address lines in each entry
                    graduates[0].GradTranscriptAddress = null;
                    graduates[1].GradTranscriptAddress = null;
                    dataAccessorMock.Setup<Task<Collection<Ellucian.Colleague.Data.Student.DataContracts.Graduates>>>(acc => acc.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(It.IsAny<string>(), false)).ReturnsAsync(graduates);
                    var graduationApplicationEntities = await graduationApplicationRepository.GetGraduationApplicationsAsync(studentId);
                    Assert.IsNotNull(graduationApplicationEntities);
                    Assert.AreEqual(graduates[0].Recordkey, graduationApplicationEntities[0].Id);
                    Assert.AreEqual(graduates[0].GradTranscriptAddress, graduationApplicationEntities[0].MailDiplomaToAddressLines);
                    Assert.IsNull(graduationApplicationEntities[0].MailDiplomaToAddressLines);
                    
                    //same assertions for second record
                    Assert.IsNotNull(graduationApplicationEntities);
                    Assert.AreEqual(graduates[1].Recordkey, graduationApplicationEntities[1].Id);
                    Assert.AreEqual(graduates[1].GradTranscriptAddress, graduationApplicationEntities[1].MailDiplomaToAddressLines);
                    Assert.IsNull(graduationApplicationEntities[1].MailDiplomaToAddressLines);
               }

              [TestMethod]
               public async Task GetGraduationApplications_WithRequiredValuesAsNull()
               {
                   string studentId = "0000011";
                   CreateApplicationsWithRequiredAsNull("Y");
                   dataAccessorMock.Setup<Task<Collection<Ellucian.Colleague.Data.Student.DataContracts.Graduates>>>(acc => acc.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(It.IsAny<string>(), false)).ReturnsAsync(graduates);
                   var graduationApplicationEntities = await graduationApplicationRepository.GetGraduationApplicationsAsync(studentId);
                   Assert.AreEqual(4, graduates.Count);
                   Assert.AreEqual(2, graduationApplicationEntities.Count);
                   Assert.IsNotNull(graduationApplicationEntities);
                  
                   Assert.AreEqual(graduates[1].Recordkey, graduationApplicationEntities[0].Id);
                   Assert.AreEqual(graduates[1].GradTranscriptAddress, graduationApplicationEntities[0].MailDiplomaToAddressLines);

                   //Make sure second application comes out correctly
                   Assert.AreEqual(graduates[3].Recordkey, graduationApplicationEntities[1].Id);
                   Assert.AreEqual(graduates[3].GradTranscriptAddress, graduationApplicationEntities[1].MailDiplomaToAddressLines);
               }

               [TestMethod]
               [ExpectedException(typeof(ArgumentNullException))]
               public async Task GetGraduationApplication_ArgumentNullexception()
               {
                    var graduationApplicationEntity = await graduationApplicationRepository.GetGraduationApplicationsAsync(null);
               }

               [TestMethod]
               [ExpectedException(typeof(KeyNotFoundException))]
               public async Task GetGraduationApplication_KeyNotFoundException()
               {
                    graduates = null;
                    dataAccessorMock.Setup<Task<Collection<Ellucian.Colleague.Data.Student.DataContracts.Graduates>>>(acc => acc.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(It.IsAny<string>(), false)).ReturnsAsync(graduates);
                    var graduationApplicationEntity = await graduationApplicationRepository.GetGraduationApplicationsAsync("0000011");
               }

               private void CreateApplications(string boolString)
               {
                    Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate_1 = new Ellucian.Colleague.Data.Student.DataContracts.Graduates();
                    graduate_1.Recordkey = "0000011*MATH.BA";
                    graduate_1.GradTerm = "2015/FA";
                    graduate_1.GradAttendCommencement = boolString;
                    graduate_1.GradCapSize = "5";
                    graduate_1.GradCommencementDate = DateTime.Now;
                    graduate_1.GradCommencementSite = "college";
                    graduate_1.GradDiplomaName = "BS in Math";
                    graduate_1.GradGownSize = "11";
                    graduate_1.GradHometown = "PA";
                    graduate_1.GradIncludeName = boolString;
                    graduate_1.GradTranscriptAddress = new List<string>() { "Address 1", "Address 2" };
                    graduate_1.GradTranscriptCity = "fairfax";
                    graduate_1.GradTranscriptCountry = "USA";
                    graduate_1.GradTranscriptZip = "111111";
                    graduate_1.GradTranscriptState = "VA";
                    graduate_1.GradNumberOfGuests = "12";
                    graduate_1.GradPhoneticSpelling = "aaa";
                    graduate_1.GradWillPickupDiploma = boolString;
                    graduate_1.GradAcadCredentialsUpdted = boolString;
                    graduates.Add(graduate_1);
                    Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate_2 = new Ellucian.Colleague.Data.Student.DataContracts.Graduates();
                    graduate_2.Recordkey = "0000011*CS.BS";
                    graduate_2.GradTerm = "2015/FA";
                    graduate_2.GradAttendCommencement = boolString;
                    graduate_2.GradCapSize = "5";
                    graduate_2.GradCommencementDate = DateTime.Now;
                    graduate_2.GradCommencementSite = "college";
                    graduate_2.GradDiplomaName = "BS in Math";
                    graduate_2.GradGownSize = "11";
                    graduate_2.GradHometown = "PA";
                    graduate_2.GradIncludeName = boolString;
                    graduate_2.GradTranscriptAddress = new List<string>() { "Address 1", "Address 2" };
                    graduate_2.GradTranscriptCity = "fairfax";
                    graduate_2.GradTranscriptCountry = "USA";
                    graduate_2.GradTranscriptZip = "111111";
                    graduate_2.GradTranscriptState = "VA";
                    graduate_2.GradNumberOfGuests = "12";
                    graduate_2.GradPhoneticSpelling = "aaa";
                    graduate_2.GradWillPickupDiploma = boolString;
                    graduate_2.GradAcadCredentialsUpdted = boolString;
                    graduates.Add(graduate_2);
               }
               private void CreateApplicationsWithRequiredAsNull(string boolString)
               {
                   Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate_1 = new Ellucian.Colleague.Data.Student.DataContracts.Graduates();
                   graduate_1.Recordkey = "**MATH.BA";
                   graduate_1.GradTerm = "2015/FA";
                   graduate_1.GradAttendCommencement = boolString;
                   graduate_1.GradCapSize = "5";
                   graduate_1.GradCommencementDate = DateTime.Now;
                   graduate_1.GradCommencementSite = "college";
                   graduate_1.GradDiplomaName = "BS in Math";
                   graduate_1.GradGownSize = "11";
                   graduate_1.GradHometown = "PA";
                   graduate_1.GradIncludeName = boolString;
                   graduate_1.GradTranscriptAddress = new List<string>() { "Address 1", "Address 2" };
                   graduate_1.GradTranscriptCity = "fairfax";
                   graduate_1.GradTranscriptCountry = "USA";
                   graduate_1.GradTranscriptZip = "111111";
                   graduate_1.GradTranscriptState = "VA";
                   graduate_1.GradNumberOfGuests = "12";
                   graduate_1.GradPhoneticSpelling = "aaa";
                   graduate_1.GradWillPickupDiploma = boolString;
                   graduates.Add(graduate_1);

                   Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate_2 = new Ellucian.Colleague.Data.Student.DataContracts.Graduates();
                   graduate_2.Recordkey = "0000011*CS.BS";
                   graduate_2.GradTerm = null;
                   graduate_2.GradAttendCommencement = boolString;
                   graduate_2.GradCapSize = "5";
                   graduate_2.GradCommencementDate = DateTime.Now;
                   graduate_2.GradCommencementSite = "college";
                   graduate_2.GradDiplomaName = "BS in Math";
                   graduate_2.GradGownSize = "11";
                   graduate_2.GradHometown = "PA";
                   graduate_2.GradIncludeName = boolString;
                   graduate_2.GradTranscriptAddress = new List<string>() { "Address 1", "Address 2" };
                   graduate_2.GradTranscriptCity = "fairfax";
                   graduate_2.GradTranscriptCountry = "USA";
                   graduate_2.GradTranscriptZip = "111111";
                   graduate_2.GradTranscriptState = "VA";
                   graduate_2.GradNumberOfGuests = "12";
                   graduate_2.GradPhoneticSpelling = "aaa";
                   graduate_2.GradWillPickupDiploma = boolString;
                   graduates.Add(graduate_2);
                  
                   Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate_3 = new Ellucian.Colleague.Data.Student.DataContracts.Graduates();
                   graduate_3.Recordkey = "0000011**";
                   graduate_3.GradTerm = "2015/FA";
                   graduate_3.GradAttendCommencement = boolString;
                   graduate_3.GradCapSize = "5";
                   graduate_3.GradCommencementDate = DateTime.Now;
                   graduate_3.GradCommencementSite = "college";
                   graduate_3.GradDiplomaName = "BS in Math";
                   graduate_3.GradGownSize = "11";
                   graduate_3.GradHometown = "PA";
                   graduate_3.GradIncludeName = boolString;
                   graduate_3.GradTranscriptAddress = new List<string>() { "Address 1", "Address 2" };
                   graduate_3.GradTranscriptCity = "fairfax";
                   graduate_3.GradTranscriptCountry = "USA";
                   graduate_3.GradTranscriptZip = "111111";
                   graduate_3.GradTranscriptState = "VA";
                   graduate_3.GradNumberOfGuests = "12";
                   graduate_3.GradPhoneticSpelling = "aaa";
                   graduate_3.GradWillPickupDiploma = boolString;
                   graduates.Add(graduate_3);

                   Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate_4 = new Ellucian.Colleague.Data.Student.DataContracts.Graduates();
                   graduate_4.Recordkey = "0000011*CS.BS";
                   graduate_4.GradTerm = "2015/FA";
                   graduate_4.GradAttendCommencement = boolString;
                   graduate_4.GradCapSize = "5";
                   graduate_4.GradCommencementDate = DateTime.Now;
                   graduate_4.GradCommencementSite = "college";
                   graduate_4.GradDiplomaName = "BS in Math";
                   graduate_4.GradGownSize = "11";
                   graduate_4.GradHometown = "PA";
                   graduate_4.GradIncludeName = boolString;
                   graduate_4.GradTranscriptAddress = new List<string>() { "Address 1", "Address 2" };
                   graduate_4.GradTranscriptCity = "fairfax";
                   graduate_4.GradTranscriptCountry = "USA";
                   graduate_4.GradTranscriptZip = "111111";
                   graduate_4.GradTranscriptState = "VA";
                   graduate_4.GradNumberOfGuests = "12";
                   graduate_4.GradPhoneticSpelling = "aaa";
                   graduate_4.GradWillPickupDiploma = boolString;
                   graduates.Add(graduate_4);
               }

        }

          [TestClass]
          public class GraduationApplicationRepository_UpdateGraduationApplication
          {
              Mock<IColleagueTransactionFactory> transFactoryMock;
              Mock<IColleagueDataReader> dataAccessorMock;
              Mock<ICacheProvider> cacheProviderMock;
              Mock<ILogger> loggerMock;
              ApiSettings apiSettings;
              Mock<IColleagueTransactionInvoker> mockManager;
              private IGraduationApplicationRepository graduationApplicationRepository;
              UpdateGraduationApplicationRequest updateRequest;

              [TestInitialize]
              public void Initialize()
              {
                  loggerMock = new Mock<ILogger>();
                  apiSettings = new ApiSettings("TEST");
                  cacheProviderMock = new Mock<ICacheProvider>();
                  transFactoryMock = new Mock<IColleagueTransactionFactory>();
                  dataAccessorMock = new Mock<IColleagueDataReader>();
                  mockManager = new Mock<IColleagueTransactionInvoker>();
                  cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                  x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                  .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                      null,
                      new SemaphoreSlim(1, 1)
                      )));
                  // Set up data accessor for the transaction factory 
                  transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                  // Set up successful response to a transaction request, capturing the completed request for verification
                  transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                  graduationApplicationRepository = new GraduationApplicationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object,apiSettings);
              }

              [TestCleanup]
              public void Cleanup()
              {
                  transFactoryMock = null;
                  dataAccessorMock = null;
                  cacheProviderMock = null;
              }

              //graduation application is null
              [TestMethod]
              [ExpectedException(typeof(ArgumentNullException))]
              public async Task UpdateGraduationApplication_ApplicationIsNull()
              {
                  var graduationApplication = await graduationApplicationRepository.UpdateGraduationApplicationAsync(null);
              }

              //student id and program code is null
              [TestMethod]
              [ExpectedException(typeof(ArgumentNullException))]
              public async Task UpdateGraduationApplication_IdsAreNull()
              {
                  var graduationEntity = new GraduationApplication("", "", "null");
                  var graduationApplication = await graduationApplicationRepository.UpdateGraduationApplicationAsync(graduationEntity);
              }

              //update response returns error message
              [TestMethod]
              [ExpectedException(typeof(ArgumentException))]
              public async Task UpdateGraduationApplication_ResponseHasErrorMessage()
              {
                  var graduationEntity = BuildGraduationEntityRequest_WithTrueValues();
                  Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate = new Ellucian.Colleague.Data.Student.DataContracts.Graduates();
                  graduate.Recordkey = "0000011*MATH.BA";
                  graduate.GradTerm = "2015/FA";
                  UpdateGraduationApplicationResponse updateResponse = new UpdateGraduationApplicationResponse();
                  updateResponse.ErrorOccurred = true;
                  updateResponse.ErrorMessage = "Some error message";
                  mockManager.Setup(mgr => mgr.ExecuteAsync<UpdateGraduationApplicationRequest, UpdateGraduationApplicationResponse>(It.IsAny<UpdateGraduationApplicationRequest>())).Returns(Task.FromResult(updateResponse)).Callback<UpdateGraduationApplicationRequest>(req => updateRequest = req);
                  dataAccessorMock.Setup<Task<Ellucian.Colleague.Data.Student.DataContracts.Graduates>>(acc => acc.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(It.IsAny<string>(), false)).ReturnsAsync(graduate);
                  var graduationApplication = await graduationApplicationRepository.UpdateGraduationApplicationAsync(graduationEntity);
              }

              private GraduationApplication BuildGraduationEntityRequest_WithTrueValues()
              {
                  var graduationEntity = new GraduationApplication("", "0000011", "MATH.BA");
                  graduationEntity.AttendingCommencement = true;
                  graduationEntity.CapSize = "5";
                  graduationEntity.CommencementDate = DateTime.Now;
                  graduationEntity.CommencementLocation = "college";
                  graduationEntity.DiplomaName = "BS in Math";
                  graduationEntity.GownSize = "11";
                  graduationEntity.Hometown = "PA";
                  graduationEntity.IncludeNameInProgram = true;
                  graduationEntity.MailDiplomaToAddressLines = new List<string>() { "Address 1", "Address 2" };
                  graduationEntity.MailDiplomaToCity = "fairfax";
                  graduationEntity.MailDiplomaToCountry = "USA";
                  graduationEntity.MailDiplomaToPostalCode = "111111";
                  graduationEntity.MailDiplomaToState = "VA";
                  graduationEntity.NumberOfGuests = 12;
                  graduationEntity.PhoneticSpellingOfName = "aaa";
                  graduationEntity.WillPickupDiploma = true;
                  graduationEntity.MilitaryStatus = GraduateMilitaryStatus.ActiveMilitary;
                  graduationEntity.SpecialAccommodations = "Special accommodations Line 1\nSpecial accommodations Line 2";
                  return graduationEntity;
              }
          }

          [TestClass]
          public class GraduationApplicationRepository_GetGraduationApplicationFeeAsync
          {
              private Mock<IColleagueTransactionFactory> transFactoryMock;
              private Mock<IColleagueDataReader> dataAccessorMock;
              private Mock<ICacheProvider> cacheProviderMock;
              private Mock<ILogger> loggerMock;
              private ApiSettings apiSettings;
              private Mock<IColleagueTransactionInvoker> mockManager;
              private IGraduationApplicationRepository graduationApplicationRepository;
               private GetGraduationApplicationFeeRequest getFeeRequest;

              [TestInitialize]
              public void Initialize()
              {
                  loggerMock = new Mock<ILogger>();
                  apiSettings = new ApiSettings("TEST");
                  cacheProviderMock = new Mock<ICacheProvider>();
                  transFactoryMock = new Mock<IColleagueTransactionFactory>();
                  dataAccessorMock = new Mock<IColleagueDataReader>();
                  mockManager = new Mock<IColleagueTransactionInvoker>();
                  cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                  x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                  .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                      null,
                      new SemaphoreSlim(1, 1)
                      )));
                  // Set up data accessor for the transaction factory 
                  transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                  // Set up successful response to a transaction request, capturing the completed request for verification
                  transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                  graduationApplicationRepository = new GraduationApplicationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
              }

              [TestCleanup]
              public void Cleanup()
              {
                  transFactoryMock = null;
                  dataAccessorMock = null;
                  cacheProviderMock = null;
              }

              [TestMethod]
              [ExpectedException(typeof(ArgumentNullException))]
              public async Task GetGraduationApplicationFeeAsync_NullStudent()
              {
                  var graduationApplicationEntity = await graduationApplicationRepository.GetGraduationApplicationFeeAsync(null, "programCode");
              }

              [TestMethod]
              [ExpectedException(typeof(ArgumentNullException))]
              public async Task GetGraduationApplicationFeeAsync_EmptyStudent()
              {
                  var graduationApplicationEntity = await graduationApplicationRepository.GetGraduationApplicationFeeAsync(string.Empty, "programCode");
              }

              [TestMethod]
              [ExpectedException(typeof(ArgumentNullException))]
              public async Task GetGraduationApplicationFeeAsync_NullProgramCode()
              {
                  var graduationApplicationEntity = await graduationApplicationRepository.GetGraduationApplicationFeeAsync("studentId", null);
              }

              [TestMethod]
              [ExpectedException(typeof(ArgumentNullException))]
              public async Task GetGraduationApplicationFeeAsync_EmptyProgramCode()
              {
                  var graduationApplicationEntity = await graduationApplicationRepository.GetGraduationApplicationFeeAsync("studentId", string.Empty);
              }

              [TestMethod]
              public async Task GetGraduationApplicationFeeAsync_ReturnsValidFee()
              {
                  var validResponse = BuildValidGetGraduationApplicationFeeResponse();
                  mockManager.Setup(mgr => mgr.ExecuteAsync<GetGraduationApplicationFeeRequest, GetGraduationApplicationFeeResponse>(It.IsAny<GetGraduationApplicationFeeRequest>())).Returns(Task.FromResult(validResponse)).Callback<GetGraduationApplicationFeeRequest>(req => getFeeRequest = req);
                  var graduationApplicationFee = await graduationApplicationRepository.GetGraduationApplicationFeeAsync("studentId", "programCode");
                  //compare transaction request 
                  Assert.AreEqual("studentId", graduationApplicationFee.StudentId);
                  Assert.AreEqual("programCode", graduationApplicationFee.ProgramCode);
                  Assert.AreEqual(validResponse.ApplicationFee, graduationApplicationFee.Amount);
                  Assert.AreEqual(validResponse.DistributionCode, graduationApplicationFee.PaymentDistributionCode);
              }

              [TestMethod]
              [ExpectedException(typeof(Exception))]
              public async Task GetGraduationApplicationFeeAsync_ColleagueTXThrowsException()
              {
                  var validResponse = BuildValidEmptyGraduationApplicationFeeResponse();
                  mockManager.Setup(mgr => mgr.ExecuteAsync<GetGraduationApplicationFeeRequest, GetGraduationApplicationFeeResponse>(It.IsAny<GetGraduationApplicationFeeRequest>())).Throws(new Exception());
                  var graduationApplicationFee = await graduationApplicationRepository.GetGraduationApplicationFeeAsync("studentId", "programCode");
              }

              [TestMethod]
              public async Task GetGraduationApplicationFeeAsync_ReturnsValidEmptyFee()
              {
                  var validResponse = BuildValidEmptyGraduationApplicationFeeResponse();
                  mockManager.Setup(mgr => mgr.ExecuteAsync<GetGraduationApplicationFeeRequest, GetGraduationApplicationFeeResponse>(It.IsAny<GetGraduationApplicationFeeRequest>())).Returns(Task.FromResult(validResponse)).Callback<GetGraduationApplicationFeeRequest>(req => getFeeRequest = req);
                  var graduationApplicationFee = await graduationApplicationRepository.GetGraduationApplicationFeeAsync("studentId", "programCode");
                  //compare transaction request 
                  Assert.AreEqual("studentId", graduationApplicationFee.StudentId);
                  Assert.AreEqual("programCode", graduationApplicationFee.ProgramCode);
                  Assert.IsNull(graduationApplicationFee.Amount);
                  Assert.IsNull(graduationApplicationFee.PaymentDistributionCode);
              }

              private GetGraduationApplicationFeeResponse BuildValidGetGraduationApplicationFeeResponse()
              {
                  GetGraduationApplicationFeeResponse response = new GetGraduationApplicationFeeResponse();
                  response.ApplicationFee = 100m;
                  response.DistributionCode = "DISTR";
                  return response;
              }

              private GetGraduationApplicationFeeResponse BuildValidEmptyGraduationApplicationFeeResponse()
              {
                  GetGraduationApplicationFeeResponse response = new GetGraduationApplicationFeeResponse();
                  response.ApplicationFee = null;
                  response.DistributionCode = null;
                  return response;
              }
          }
    }
}

