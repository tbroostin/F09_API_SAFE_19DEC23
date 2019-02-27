// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class PersonHoldsRepositoryTests
    {
        [TestClass]
        public class PersonHoldGetMethods: BaseRepositorySetup
        {
            private Ellucian.Data.Colleague.DataContracts.IntlParams intlParams;
            PersonHoldsRepository personHoldRepository;
            int readSize;
            string guid = "23977f85-f200-479f-9eee-3921bb4667d3";
            string guid2 = "13977f85-f200-479f-9eee-3921bb4667d2";
            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                apiSettings = new ApiSettings("TEST") { BulkReadSize = 5000 };
                this.readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;

                InitializeMockForInternationalParams();

                personHoldRepository = new PersonHoldsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            private void InitializeMockForInternationalParams()
            {
                intlParams = new Ellucian.Data.Colleague.DataContracts.IntlParams()
                {
                    HostCountry = "USA",
                    HostShortDateFormat = "DMY",
                    HostDateDelimiter = "/"
                };
                dataReaderMock.Setup(d => d.ReadRecordAsync<Ellucian.Data.Colleague.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL", It.IsAny<bool>())).ReturnsAsync(intlParams);
            }

            [TestCleanup]
            public void Cleanup()
            {
                personHoldRepository = null;
                readSize = 0;
            }

            [TestMethod]
            public async Task PersonHoldsRepo_GetPersonHoldsAsync()
            { 
                string[] personHoldsIds = new []{"1", "2"};
                Collection<StudentRestrictions> studentHolds = new Collection<StudentRestrictions>() 
                {
                    new StudentRestrictions(){ RecordGuid = guid,  Recordkey = "1", StrStudent = "1", StrComments = "Comment 1", StrEndDate = DateTime.MaxValue, StrRestriction = "Academic", StrStartDate = DateTime.MinValue },
                    new StudentRestrictions(){ RecordGuid = guid2, Recordkey = "2", StrStudent = "2", StrComments = "Comment 2", StrEndDate = DateTime.MaxValue, StrRestriction = "Health", StrStartDate = DateTime.MinValue },
                };

                dataReaderMock.Setup(i => i.SelectAsync("STUDENT.RESTRICTIONS", It.IsAny<string>())).ReturnsAsync(personHoldsIds);
                dataReaderMock.Setup<Task<Collection<StudentRestrictions>>>(i => i.BulkReadRecordAsync<StudentRestrictions>("STUDENT.RESTRICTIONS", personHoldsIds, true)).ReturnsAsync(studentHolds);

                var results = await personHoldRepository.GetPersonHoldsAsync(0,10);

                for (int i = 0; i < 2; i++)
                {
                    StudentRestrictions restriction = studentHolds[i];
                    PersonRestriction personRestriction = results.Item1.FirstOrDefault(key => key.Id == restriction.Recordkey);

                    Assert.AreEqual(restriction.Recordkey, personRestriction.Id);
                    Assert.AreEqual(restriction.StrStudent, personRestriction.StudentId);
                    Assert.AreEqual(restriction.StrComments, personRestriction.Comment);
                    Assert.AreEqual(restriction.StrEndDate, personRestriction.EndDate);
                    Assert.AreEqual(restriction.StrStartDate, personRestriction.StartDate);
                    Assert.AreEqual(restriction.StrRestriction, personRestriction.RestrictionId);
                }
            }

            [TestMethod]
            public async Task PersonHoldsRepo_GetPersonHoldByIdAsync()
            {
                StudentRestrictions studentRestrictions = new StudentRestrictions() { RecordGuid = guid, Recordkey = "1", StrStudent = "1", StrComments = "Comment 1", StrEndDate = DateTime.MaxValue, StrRestriction = "Academic", StrStartDate = DateTime.MinValue };
                GuidLookupResult res = new GuidLookupResult(){ Entity = "STUDENT.RESTRICTIONS", PrimaryKey = "1", SecondaryKey = ""};
                Dictionary<string, GuidLookupResult> dict = new Dictionary<string, GuidLookupResult>();
                dict.Add("1", res);

                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(lookup =>
                    {
                        return Task.FromResult(dict);
                    });
                dataReaderMock.Setup(i => i.ReadRecordAsync<StudentRestrictions>("STUDENT.RESTRICTIONS", It.IsAny<string>(), true)).ReturnsAsync(studentRestrictions);

                
                var result = await personHoldRepository.GetPersonHoldByIdAsync("1");

                Assert.AreEqual(studentRestrictions.Recordkey, result.Id);
                Assert.AreEqual(studentRestrictions.StrStudent, result.StudentId);
                Assert.AreEqual(studentRestrictions.StrComments, result.Comment);
                Assert.AreEqual(studentRestrictions.StrEndDate, result.EndDate);
                Assert.AreEqual(studentRestrictions.StrStartDate, result.StartDate);
                Assert.AreEqual(studentRestrictions.StrRestriction, result.RestrictionId);
            }

            [TestMethod]
            public async Task PersonHoldsRepo_GetPersonHoldByPersonIdAsync()
            {
                Collection<StudentRestrictions> studentHolds = new Collection<StudentRestrictions>() 
                {
                    new StudentRestrictions()
                        { RecordGuid = guid, Recordkey = "1", StrStudent = "1", StrComments = "Comment 1", StrEndDate = DateTime.MaxValue, StrRestriction = "Academic", StrStartDate = DateTime.MinValue },
                    new StudentRestrictions()
                        { RecordGuid = guid2, Recordkey = "2", StrStudent = "1", StrComments = "Comment 2", StrEndDate = DateTime.MaxValue, StrRestriction = "Health", StrStartDate = DateTime.MinValue },
                };

                GuidLookupResult res = new GuidLookupResult() { Entity = "STUDENT.RESTRICTIONS", PrimaryKey = "1", SecondaryKey = "" };
                Dictionary<string, GuidLookupResult> dict = new Dictionary<string, GuidLookupResult>();
                dict.Add("1", res);
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(lookup =>
                {
                    return Task.FromResult(dict);
                });

                dataReaderMock.Setup<Task<Collection<StudentRestrictions>>>(i => i.BulkReadRecordAsync<StudentRestrictions>(It.IsAny<string>(), true)).ReturnsAsync(studentHolds);

                var results = await personHoldRepository.GetPersonHoldsByPersonIdAsync("1");

                for (int i = 0; i < 2; i++)
                {
                    StudentRestrictions restriction = studentHolds[i];
                    PersonRestriction personRestriction = results.FirstOrDefault(key => key.Id == restriction.Recordkey);

                    Assert.AreEqual(restriction.Recordkey, personRestriction.Id);
                    Assert.AreEqual(restriction.StrStudent, personRestriction.StudentId);
                    Assert.AreEqual(restriction.StrComments, personRestriction.Comment);
                    Assert.AreEqual(restriction.StrEndDate, personRestriction.EndDate);
                    Assert.AreEqual(restriction.StrStartDate, personRestriction.StartDate);
                    Assert.AreEqual(restriction.StrRestriction, personRestriction.RestrictionId);
                }
            }

            //[TestMethod]
            //public async Task PersonHoldRepository_GetUnidataFormattedDate()
            //{
            //    var result = await personHoldRepository.GetUnidataFormattedDateAsync(DateTime.Today.ToString());

            //    Assert.IsNotNull(result);
            //    Assert.AreEqual(result, DateTime.Today.ToString("dd/MM/yyyy"));

            //}

            [TestMethod]
            public async Task PersonHoldRepo_DeletePersonHoldsAsync()
            {
                List<PersonHoldResponse> warningList = new List<PersonHoldResponse>() { new PersonHoldResponse() { PersonHoldGuid = "1", PersonHoldId = "1", WarningCode = "1", WarningMessage = "WarningMessage" } };
                DeleteRestrictionResponse response = new DeleteRestrictionResponse();
                response.DeleteRestrictionWarnings =
                    new List<DeleteRestrictionWarnings>() { new DeleteRestrictionWarnings() { WarningCodes = "1", WarningMessages = "WarningMessage" } };

                transManagerMock.Setup(i => i.ExecuteAsync<DeleteRestrictionRequest, DeleteRestrictionResponse>(It.IsAny <DeleteRestrictionRequest>())).ReturnsAsync(response);

                GuidLookupResult res = new GuidLookupResult() { Entity = "STUDENT.RESTRICTIONS", PrimaryKey = "1", SecondaryKey = "" };
                Dictionary<string, GuidLookupResult> dict = new Dictionary<string, GuidLookupResult>();
                dict.Add("1", res);
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(lookup =>
                {
                    return Task.FromResult(dict);
                });

                var results = await personHoldRepository.DeletePersonHoldsAsync("1");

                foreach (var warning in warningList)
                {
                    var tempWarning = results.FirstOrDefault(i => i.WarningCode == warning.WarningCode);
                    Assert.AreEqual(warning.WarningCode, tempWarning.WarningCode);
                    Assert.AreEqual(warning.WarningMessage, tempWarning.WarningMessage);
                }
            }

            [TestMethod]
            public async Task PersonHoldRepo_UpdatePersonHoldAsync_WarnigMessages()
            {
                PersonHoldRequest request = new PersonHoldRequest("2", "1", "Academic", new DateTimeOffset(2015, 12, 1, 0, 0, 0, new TimeSpan(1, 0, 0)), new DateTimeOffset(2015, 12, 31, 0, 0, 0, new TimeSpan(1, 0, 0)), "Y");
                UpdateRestrictionResponse response = new UpdateRestrictionResponse() 
                {
                    StrGuid = "1",
                    StudentRestrictionsId = "2"
                };

                transManagerMock.Setup(i => i.ExecuteAsync<UpdateRestrictionRequest, UpdateRestrictionResponse>(It.IsAny<UpdateRestrictionRequest>())).ReturnsAsync(response);

                var result = await personHoldRepository.UpdatePersonHoldAsync(request);
                Assert.AreEqual(response.StrGuid, result.PersonHoldGuid);
                Assert.AreEqual(response.StudentRestrictionsId, result.PersonHoldId);
            }

            [TestMethod]
            public async Task PersonHoldRepo_GetStudentHoldIdFromGuidAsync()
            {
                GuidLookupResult res = new GuidLookupResult() { Entity = "STUDENT.RESTRICTIONS", PrimaryKey = "1", SecondaryKey = "" };
                Dictionary<string, GuidLookupResult> dict = new Dictionary<string, GuidLookupResult>();
                dict.Add("1", res);
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(lookup =>
                {
                    return Task.FromResult(dict);
                });

                var result = await personHoldRepository.GetStudentHoldIdFromGuidAsync("1");
                var value = dict.FirstOrDefault(i => i.Key == "1");
                Assert.AreEqual(res.PrimaryKey, value.Value.PrimaryKey);
            }

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public async Task PersonHoldsRepo_GetPersonHoldByPersonIdAsync_ArgumentNullException()
            //{
            //    var results = await personHoldRepository.GetPersonHoldsByPersonIdAsync("");               
            //}

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public async Task PersonHoldsRepo_GetPersonHoldByPersonIdAsync_PersonKeyNull_ArgumentNullException()
            //{
            //    Collection<StudentRestrictions> studentHolds = new Collection<StudentRestrictions>() 
            //    {
            //        new StudentRestrictions()
            //            { Recordkey = "1", StrStudent = "1", StrComments = "Comment 1", StrEndDate = DateTime.MaxValue, StrRestriction = "Academic", StrStartDate = DateTime.MinValue },
            //        new StudentRestrictions()
            //            { Recordkey = "2", StrStudent = "1", StrComments = "Comment 2", StrEndDate = DateTime.MaxValue, StrRestriction = "Health", StrStartDate = DateTime.MinValue },
            //    };
            //    GuidLookupResult res = new GuidLookupResult() { Entity = "STUDENT.RESTRICTIONS", PrimaryKey = "", SecondaryKey = "" };
            //    Dictionary<string, GuidLookupResult> dict = new Dictionary<string, GuidLookupResult>();
            //    dict.Add("5", res);
            //    dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(lookup =>
            //    {
            //        return Task.FromResult(dict);
            //    });

            //    var results = await personHoldRepository.GetPersonHoldsByPersonIdAsync("1");
            //}

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonHoldRepo_GetStudentHoldGuidFromIdAsync_ArgumentNullException()
            {
                var result = await personHoldRepository.GetStudentHoldGuidFromIdAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PersonHoldRepo_GetStudentHoldGuidFromIdAsync_RepositoryException()
            {
                var result = await personHoldRepository.GetStudentHoldGuidFromIdAsync("1");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonHoldRepo_GetStudentHoldIdFromGuidAsync_ArgumentNullException()
            {
                var result = await personHoldRepository.GetStudentHoldIdFromGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PersonHoldRepo_GetStudentHoldIdFromGuidAsync_RepositoryException()
            {
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ThrowsAsync(new RepositoryException());
                var result = await personHoldRepository.GetStudentHoldIdFromGuidAsync("abc");
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task PersonHoldRepo_UpdatePersonHoldAsync_ErrorMessges()
            {
                PersonHoldRequest request = new PersonHoldRequest("2", "1", "Academic", new DateTimeOffset(2015, 12, 1, 0, 0, 0, new TimeSpan(1, 0, 0)), new DateTimeOffset(2015, 12, 31, 0, 0, 0, new TimeSpan(1, 0, 0)), "Y");
                UpdateRestrictionResponse response = new UpdateRestrictionResponse()
                {
                    RestrictionErrorMessages = new List<RestrictionErrorMessages>() { new RestrictionErrorMessages() { ErrorCode = "1", ErrorMsg = "ErrorMsg" } }
                };

                transManagerMock.Setup(i => i.ExecuteAsync<UpdateRestrictionRequest, UpdateRestrictionResponse>(It.IsAny<UpdateRestrictionRequest>())).ReturnsAsync(response);

                var result = await personHoldRepository.UpdatePersonHoldAsync(request);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PersonHoldRepo_DeletePersonHoldsAsync_Exception()
            {
                List<PersonHoldResponse> warningList = new List<PersonHoldResponse>() { new PersonHoldResponse() { PersonHoldGuid = "1", PersonHoldId = "1", WarningCode = "1", WarningMessage = "WarningMessage" } };
                DeleteRestrictionResponse response = new DeleteRestrictionResponse();
                response.DeleteRestrictionErrors =
                    new List<DeleteRestrictionErrors>() { new DeleteRestrictionErrors() { ErrorCodes = "1", ErrorMessages = "ErrorMessages" } };

                transManagerMock.Setup(i => i.ExecuteAsync<DeleteRestrictionRequest, DeleteRestrictionResponse>(It.IsAny<DeleteRestrictionRequest>())).ReturnsAsync(response);

                GuidLookupResult res = new GuidLookupResult() { Entity = "STUDENT.RESTRICTIONS", PrimaryKey = "1", SecondaryKey = "" };
                Dictionary<string, GuidLookupResult> dict = new Dictionary<string, GuidLookupResult>();
                dict.Add("1", res);
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(lookup =>
                {
                    return Task.FromResult(dict);
                });

                var results = await personHoldRepository.DeletePersonHoldsAsync("1");
            }
        }
    }
}
