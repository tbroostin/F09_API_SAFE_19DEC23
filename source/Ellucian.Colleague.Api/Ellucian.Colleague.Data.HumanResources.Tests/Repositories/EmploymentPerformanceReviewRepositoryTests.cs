using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Ellucian.Colleague.Data.HumanResources.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{
    [TestClass]
    public class EmploymentPerformanceReviewRepositoryTests_V10
    {
        [TestClass]
        public class EmploymentPerformanceReviewTests : BaseRepositorySetup
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueTransactionInvoker> transInvokerMock;
            Mock<IColleagueDataReader> dataReaderMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            Collection<DataContracts.Perpos> _perposCollection;
            List<Domain.HumanResources.Entities.EmploymentPerformanceReview> employmentPerformanceReviewsEntities;
            CreatePerposReviewRequest employmentPerformanceReviewRequest;
            CreatePerposReviewResponse updateResponse;
            DeletePerposReviewRequest deleteEmploymentPerformanceReviewRequest;
            DeletePerposReviewResponse deleteEmploymentPerformanceReviewResponse;
            string codeItemName;
            ApiSettings apiSettings;

            EmploymentPerformanceReviewsRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                transInvokerMock = new Mock<IColleagueTransactionInvoker>();
                dataReaderMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

                string id, guid, id2, guid2, id3, guid3, sid, sid2, sid3;
                GuidLookup guidLookup;
                GuidLookupResult guidLookupResult;
                Dictionary<string, GuidLookupResult> guidLookupDict;
                RecordKeyLookup recordLookup;
                RecordKeyLookupResult recordLookupResult;
                Dictionary<string, RecordKeyLookupResult> recordLookupDict;

                // Set up for GUID lookups
                id = "1";
                id2 = "2";
                id3 = "3";

                // Secondary keys for GUID lookups
                sid = "11";
                sid2 = "22";
                sid3 = "33";

                guid = "F5FC5310-17F1-49FC-926D-CC6E3DA6DAEA".ToLowerInvariant();
                guid2 = "5B35075D-14FB-45F7-858A-83F4174B76EA".ToLowerInvariant();
                guid3 = "246E16D9-8790-4D7E-ACA1-D5B1CB9D4A24".ToLowerInvariant();

                var offsetDate = DmiString.DateTimeToPickDate(new DateTime(2007, 02, 05));

                // Build responses used for mocking
                _perposCollection = new Collection<DataContracts.Perpos>()
                {
                    new DataContracts.Perpos() { RecordGuid = guid, Recordkey = id, PerposHrpId = sid ,
                        PerposEvalsEntityAssociation = new List<DataContracts.PerposPerposEvals>() { new DataContracts.PerposPerposEvals(new DateTime(2007, 02, 05), "1", sid, DateTime.Now, "1", "Comment1") } },
                    new DataContracts.Perpos() { RecordGuid = guid2, Recordkey = id2, PerposHrpId = sid2  ,
                         PerposEvalsEntityAssociation = new List<DataContracts.PerposPerposEvals>() { new DataContracts.PerposPerposEvals(new DateTime(2007, 02, 06), "2", sid2, DateTime.Now, "2", "Comment2") } },
                    new DataContracts.Perpos() { RecordGuid = guid3, Recordkey = id3, PerposHrpId = sid3 ,
                        PerposEvalsEntityAssociation = new List<DataContracts.PerposPerposEvals>() { new DataContracts.PerposPerposEvals(new DateTime(2007, 02, 07), "3", sid3, DateTime.Now, "3", "Comment3" ) } },
                };

                guidLookup = new GuidLookup(guid);
                guidLookupResult = new GuidLookupResult() { Entity = "PERPOS", PrimaryKey = id, SecondaryKey = offsetDate.ToString() };
                guidLookupDict = new Dictionary<string, GuidLookupResult>();
                recordLookup = new RecordKeyLookup("PERPOS", id, "PERPOS.EVAL.RATINGS.DATE", offsetDate.ToString(), false);
                recordLookupResult = new RecordKeyLookupResult() { Guid = guid };
                recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();
                //guidLookupDict.Add(guid, new GuidLookupResult() { Entity = "PERPOS", PrimaryKey = id, SecondaryKey = offsetDate.ToString() });
                //guidLookupDict.Add(guid2, new GuidLookupResult() { Entity = "PERPOS", PrimaryKey = id2, SecondaryKey = offsetDate.ToString() });
                //guidLookupDict.Add(guid3, new GuidLookupResult() { Entity = "PERPOS", PrimaryKey = id3, SecondaryKey = offsetDate.ToString() });

                //recordLookupDict.Add("PERPOS+" + id + "+" + offsetDate.ToString(), new RecordKeyLookupResult() { Guid = guid });
                //recordLookupDict.Add("PERPOS+" + id2 + "+" + offsetDate.ToString(), new RecordKeyLookupResult() { Guid = guid2 });
                //recordLookupDict.Add("PERPOS+" + id3 + "+" + offsetDate.ToString(), new RecordKeyLookupResult() { Guid = guid3 });

                dataReaderMock.SetupSequence(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>()))
                    .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { guid, new GuidLookupResult() { Entity = "PERPOS", PrimaryKey = id, SecondaryKey = DmiString.DateTimeToPickDate(new DateTime(2007, 02, 05)).ToString() } } }))
                    .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { guid2, new GuidLookupResult() { Entity = "PERPOS", PrimaryKey = id2, SecondaryKey = DmiString.DateTimeToPickDate(new DateTime(2007, 02, 06)).ToString() } } }))
                    .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { guid3, new GuidLookupResult() { Entity = "PERPOS", PrimaryKey = id3, SecondaryKey = DmiString.DateTimeToPickDate(new DateTime(2007, 02, 07)).ToString() } } }));
                dataReaderMock.SetupSequence(ep => ep.SelectAsync(It.IsAny<RecordKeyLookup[]>()))
                    .Returns(Task.FromResult(new Dictionary<string, RecordKeyLookupResult>() { { "PERPOS+" + id + "+" + DmiString.DateTimeToPickDate(new DateTime(2007, 02, 05)).ToString(), new RecordKeyLookupResult() { Guid = guid } } }))
                    .Returns(Task.FromResult(new Dictionary<string, RecordKeyLookupResult>() { { "PERPOS+" + id2 + "+" + DmiString.DateTimeToPickDate(new DateTime(2007, 02, 06)).ToString(), new RecordKeyLookupResult() { Guid = guid2 } } }))
                    .Returns(Task.FromResult(new Dictionary<string, RecordKeyLookupResult>() { { "PERPOS+" + id3 + "+" + DmiString.DateTimeToPickDate(new DateTime(2007, 02, 07)).ToString(), new RecordKeyLookupResult() { Guid = guid3 } } }));
                //dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(guidLookupDict);
                //dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordLookupDict);
                //dataAccessorMock.Setup(da => da.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordLookupDict);

                employmentPerformanceReviewsEntities = new List<Domain.HumanResources.Entities.EmploymentPerformanceReview>() 
                {
                    new Domain.HumanResources.Entities.EmploymentPerformanceReview("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "d2253ac7-9931-4560-b42f-1fccd43c952e", new DateTime(2007, 02, 05), "CODE1", "CODE1"),
                    new Domain.HumanResources.Entities.EmploymentPerformanceReview("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "d2253ac7-9931-4560-b42f-1fccd43c952e", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", new DateTime(2007, 02, 06), "CODE2", "CODE2"),
                    new Domain.HumanResources.Entities.EmploymentPerformanceReview("d2253ac7-9931-4560-b42f-1fccd43c952e", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", new DateTime(2007, 02, 07), "CODE3", "CODE3")
                    //new Domain.HumanResources.Entities.EmploymentPerformanceReview("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "d2253ac7-9931-4560-b42f-1fccd43c952e", new DateTime(18080), "d2253ac7-9931-4560-b42f-1fccd43c952e", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                    //new Domain.HumanResources.Entities.EmploymentPerformanceReview("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "d2253ac7-9931-4560-b42f-1fccd43c952e", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", new DateTime(18080), "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "d2253ac7-9931-4560-b42f-1fccd43c952e"),
                    //new Domain.HumanResources.Entities.EmploymentPerformanceReview("d2253ac7-9931-4560-b42f-1fccd43c952e", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", new DateTime(18080), "d2253ac7-9931-4560-b42f-1fccd43c952e", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d")
                };

                employmentPerformanceReviewRequest = new CreatePerposReviewRequest()
                {
                    PerfEvalGuid = employmentPerformanceReviewsEntities.FirstOrDefault().Guid,
                };
                updateResponse = new CreatePerposReviewResponse()
                {
                    PerfEvalGuid = employmentPerformanceReviewsEntities.FirstOrDefault().Guid,
                };
                deleteEmploymentPerformanceReviewRequest = new DeletePerposReviewRequest()
                {
                    PerfEvalGuid = guid,
                    PerfEvalPerposId = sid,
                };
                deleteEmploymentPerformanceReviewResponse = new DeletePerposReviewResponse()
                {
                    DeleteReviewErrors = new List<DeleteReviewErrors>()
                };

                List<string> employmentPerformanceReviewGuids = new List<string>();
                foreach (var mp in _perposCollection)
                {
                    employmentPerformanceReviewGuids.Add(mp.RecordGuid);
                };
                //dataReaderMock.Setup(repo => repo.SelectAsync("PERPOS", It.IsAny<string>())).ReturnsAsync(employmentPerformanceReviewGuids.ToArray());
                dataReaderMock.Setup(repo => repo.SelectAsync("PERPOS", "WITH PERPOS.EVAL.RATINGS.DATE NE '' BY.EXP PERPOS.EVAL.RATINGS.DATE")).ReturnsAsync(new List<string>() { id, id2, id3 }.ToArray());
                dataReaderMock.Setup(repo => repo.SelectAsync("PERPOS", "WITH PERPOS.EVAL.RATINGS.DATE NE '' BY.EXP PERPOS.EVAL.RATINGS.DATE SAVING PERPOS.EVAL.RATINGS.DATE")).ReturnsAsync(new List<string>() { "14281", "14282", "14283" }.ToArray());
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Perpos>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(_perposCollection);
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Perpos>("PERPOS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(_perposCollection);
                dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Perpos>("PERPOS", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(_perposCollection.FirstOrDefault());

                referenceDataRepo = BuildValidReferenceDataRepository();

            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();

                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                _perposCollection = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task EmploymentPerformanceReviewsDataRepo_GetGetEmploymentPerformanceReviewsAsync()
            {
                var result = await referenceDataRepo.GetEmploymentPerformanceReviewsAsync(0, 3, false);

                for (int i = 0; i < _perposCollection.Count(); i++)
                {
                    Assert.AreEqual(_perposCollection.ElementAt(i).RecordGuid, result.Item1.ElementAt(i).Guid);
                    Assert.AreEqual(_perposCollection.ElementAt(i).Recordkey, result.Item1.ElementAt(i).PerposId);
                    Assert.AreEqual(_perposCollection.ElementAt(i).PerposHrpId, result.Item1.ElementAt(i).PersonId);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task EmploymentPerformanceReviewsRepo_GetsGetEmploymentPerformanceReviews_Exception()
            {
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(null);
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(null);
                dataAccessorMock.Setup(da => da.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(null);

                await referenceDataRepo.GetEmploymentPerformanceReviewsAsync(0, 2, true);
            }

            [TestMethod]
            public async Task EmploymentPerformanceReviewRepo_UpdateEmploymentPerformanceReviewAsync()
            {
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(new Dictionary<string, GuidLookupResult>() { { "F5FC5310-17F1-49FC-926D-CC6E3DA6DAEA".ToLowerInvariant(), new GuidLookupResult() { Entity = "PERPOS", PrimaryKey = "1", SecondaryKey = DmiString.DateTimeToPickDate(new DateTime(2007, 02, 05)).ToString() } } });
                transInvokerMock.Setup(i => i.ExecuteAsync<CreatePerposReviewRequest, CreatePerposReviewResponse>(It.IsAny<CreatePerposReviewRequest>())).ReturnsAsync(updateResponse);
                var result = await referenceDataRepo.UpdateEmploymentPerformanceReviewsAsync(employmentPerformanceReviewsEntities.FirstOrDefault());

                Assert.AreEqual(employmentPerformanceReviewRequest.PerfEvalGuid, result.Guid);
                //Assert.AreEqual(employmentPerformanceReviewRequest.PerfEvalPersonId, result.PersonId);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task EmploymentPerformanceReviewRepo_UpdateEmploymentPerformanceReviewAsync_InvalidOperationException()
            {
                updateResponse.CreateReviewErrors = new List<CreateReviewErrors>() { new CreateReviewErrors() { ErrorCodes = "123", ErrorMessages = "Error occured" } };
                transInvokerMock.Setup(i => i.ExecuteAsync<CreatePerposReviewRequest, CreatePerposReviewResponse>(It.IsAny<CreatePerposReviewRequest>())).ReturnsAsync(updateResponse);
                var result = await referenceDataRepo.UpdateEmploymentPerformanceReviewsAsync(employmentPerformanceReviewsEntities.FirstOrDefault());
            }

            [TestMethod]
            public async Task EmploymentPerformanceReviewRepo_CreateEmploymentPerformanceReviewAsync()
            {
                transInvokerMock.Setup(i => i.ExecuteAsync<CreatePerposReviewRequest, CreatePerposReviewResponse>(It.IsAny<CreatePerposReviewRequest>())).ReturnsAsync(updateResponse);
                var result = await referenceDataRepo.CreateEmploymentPerformanceReviewsAsync(employmentPerformanceReviewsEntities.FirstOrDefault());

                Assert.AreEqual(employmentPerformanceReviewRequest.PerfEvalGuid, result.Guid);
                //Assert.AreEqual(employmentPerformanceReviewRequest.PerfEvalPersonId, result.PersonId);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task EmploymentPerformanceReviewRepo_CreateEmploymentPerformanceReviewAsync_InvalidOperationException()
            {
                updateResponse.CreateReviewErrors = new List<CreateReviewErrors>() { new CreateReviewErrors() { ErrorCodes = "123", ErrorMessages = "Error occured" } };
                transInvokerMock.Setup(i => i.ExecuteAsync<CreatePerposReviewRequest, CreatePerposReviewResponse>(It.IsAny<CreatePerposReviewRequest>())).ReturnsAsync(updateResponse);
                var result = await referenceDataRepo.CreateEmploymentPerformanceReviewsAsync(employmentPerformanceReviewsEntities.FirstOrDefault());
            }

            [TestMethod]
            public async Task EmploymentPerformanceReviewRepo_DeleteEmploymentPerformanceReviewAsync()
            {
                var guid = "F5FC5310-17F1-49FC-926D-CC6E3DA6DAEA".ToLowerInvariant();

                transInvokerMock.Setup(i => i.ExecuteAsync<DeletePerposReviewRequest, DeletePerposReviewResponse>(It.IsAny<DeletePerposReviewRequest>())).ReturnsAsync(deleteEmploymentPerformanceReviewResponse);
                await referenceDataRepo.DeleteEmploymentPerformanceReviewsAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task EmploymentPerformanceReviewsRepo_DeleteEmploymentPerformanceReviewAsync_RepositoryException()
            {
                var guid = "F5FC5310-17F1-49FC-926D-CC6E3DA6DAEA".ToLowerInvariant();
                
                deleteEmploymentPerformanceReviewResponse.DeleteReviewErrors = new List<DeleteReviewErrors>() { new DeleteReviewErrors() { ErrorCodes = "123", ErrorMessages = "Error Occured" } };

                transInvokerMock.Setup(i => i.ExecuteAsync<DeletePerposReviewRequest, DeletePerposReviewResponse>(It.IsAny<DeletePerposReviewRequest>())).ReturnsAsync(deleteEmploymentPerformanceReviewResponse);
                await referenceDataRepo.DeleteEmploymentPerformanceReviewsAsync(guid);
            }

            private EmploymentPerformanceReviewsRepository BuildValidReferenceDataRepository()
            {
                // Initialize the Mock framework
                MockInitialize();

                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transInvokerMock.Object);

                // Cache mocking
                var cacheProviderMock = new Mock<ICacheProvider>();
                /*cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                null,
                new SemaphoreSlim(1, 1)
                )));*/

             //   cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
             //x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
             //.ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, null));

                referenceDataRepo = new EmploymentPerformanceReviewsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                return referenceDataRepo;
            }
        }

        //[TestClass]
        //public class EmploymentPerformanceReviewsGetMethods_PUT_POST_DELETE
        //{
        //    Mock<ICacheProvider> iCacheProviderMock;
        //    Mock<IColleagueTransactionFactory> iColleagueTransactionFactoryMock;
        //    Mock<IColleagueTransactionInvoker> iColleagueTransactionInvokerMock;
        //    Mock<ILogger> iLoggerMock;
        //    Mock<IColleagueDataReader> dataReaderMock;

        //    EmploymentPerformanceReviewsRepository employmentPerformanceReviewRepository;
        //    EmploymentPerformanceReview employmentPerformanceReview;
        //    Dictionary<string, GuidLookupResult> guidLookupResults;
        //    //ForeignPerson foreignPersonContract;
        //    //Ellucian.Colleague.Data.Base.DataContracts.Person personContract;
        //    CreatePerposReviewRequest employmentPerformanceReviewRequest;
        //    CreatePerposReviewResponse updateResponse;
        //    DeletePerposReviewRequest deleteEmploymentPerformanceReviewRequest;
        //    DeletePerposReviewResponse deleteEmploymentPerformanceReviewResponse;
        //    string id = string.Empty;
        //    string recKey = string.Empty;

        //    [TestInitialize]
        //    public void Initialize()
        //    {
        //        iCacheProviderMock = new Mock<ICacheProvider>();
        //        iColleagueTransactionFactoryMock = new Mock<IColleagueTransactionFactory>();
        //        iLoggerMock = new Mock<ILogger>();
        //        dataReaderMock = new Mock<IColleagueDataReader>();
        //        iColleagueTransactionInvokerMock = new Mock<IColleagueTransactionInvoker>();
        //        iColleagueTransactionFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
        //        iColleagueTransactionFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(iColleagueTransactionInvokerMock.Object);

        //        BuildObjects();
        //        employmentPerformanceReviewRepository = new EmploymentPerformanceReviewsRepository(iCacheProviderMock.Object, iColleagueTransactionFactoryMock.Object, iLoggerMock.Object);
        //    }

        //    private void BuildObjects()
        //    {
        //        id = "375ef15b-f2d2-40ed-ac47-f0d2d45260f0";
        //        recKey = "0012297";
        //        guidLookupResults = new Dictionary<string, GuidLookupResult>();// 
        //        guidLookupResults.Add("FOREIGN.PERSON", new GuidLookupResult() { Entity = "FOREIGN.PERSON", PrimaryKey = "0012297", SecondaryKey = "" });
        //        foreignPersonContract = new ForeignPerson()
        //        {
        //            Recordkey = recKey,
        //            RecordGuid = id,
        //            FperVisaNo = "A123456",
        //            FperVisaRequestDate = new DateTime(2015, 09, 17)
        //        };
        //        personContract = new DataContracts.Person()
        //        {
        //            Recordkey = recKey,
        //            RecordGuid = id,
        //            VisaType = "F1",
        //            VisaIssuedDate = new DateTime(2015, 10, 17),
        //            VisaExpDate = new DateTime(2017, 12, 17),
        //            PersonCountryEntryDate = new DateTime(2016, 02, 05)
        //        };
        //        employmentPerformanceReview = BuildPersonVisa(foreignPersonContract, personContract);

        //        employmentPerformanceReviewRequest = new PersonVisaRequest(id, recKey)
        //        {
        //            EntryDate = new DateTime(2016, 01, 10),
        //            ExpireDate = new DateTime(2017, 12, 17),
        //            IssueDate = new DateTime(2015, 10, 17),
        //            PersonId = recKey,
        //            RequestDate = new DateTime(2015, 09, 17),
        //            Status = "current",
        //            StrGuid = id,
        //            VisaNo = "A123456",
        //            VisaType = "F1"
        //        };
        //        updateResponse = new UpdatePerposReviewResponse()
        //        {
        //            PersonId = recKey,
        //            StrGuid = id
        //        };
        //        deleteEmploymentPerformanceReviewRequest = new DeletePersonVisaRequest()
        //        {
        //            StrGuid = id,
        //            PersonId = recKey
        //        };
        //        deleteEmploymentPerformanceReviewResponse = new DeletePersonVisaResponse()
        //        {
        //            DeleteVisaErrors = new List<DeleteVisaErrors>()
        //        };
        //    }

        //    [TestCleanup]
        //    public void Cleanup()
        //    {
        //        employmentPerformanceReviewRepository = null;
        //        employmentPerformanceReview = null;
        //        guidLookupResults = null;
        //        foreignPersonContract = null;
        //        personContract = null;
        //        employmentPerformanceReviewRequest = null;
        //        updateResponse = null;
        //        deleteEmploymentPerformanceReviewRequest = null;
        //        deleteEmploymentPerformanceReviewResponse = null;
        //        id = string.Empty;
        //        recKey = string.Empty;
        //    }
        //    private PersonVisa BuildPersonVisa(ForeignPerson foreignPersonContract, Ellucian.Colleague.Data.Base.DataContracts.Person personContract)
        //    {
        //        PersonVisa personVisa = new PersonVisa(personContract.Recordkey, personContract.VisaType);
        //        personVisa.Guid = foreignPersonContract.RecordGuid;
        //        personVisa.PersonGuid = personContract.RecordGuid;
        //        personVisa.VisaNumber = foreignPersonContract.FperVisaNo;
        //        personVisa.RequestDate = foreignPersonContract.FperVisaRequestDate;
        //        personVisa.IssueDate = personContract.VisaIssuedDate;
        //        personVisa.ExpireDate = personContract.VisaExpDate;
        //        personVisa.EntryDate = personContract.PersonCountryEntryDate;
        //        return personVisa;
        //    }

        //    //[TestMethod]
        //    //public async Task PersonVisaRepo_UpdatePersonVisaAsync()
        //    //{
        //    //    iColleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<UpdatePersonVisaRequest, UpdatePersonVisaResponse>(It.IsAny<UpdatePersonVisaRequest>())).ReturnsAsync(updateResponse);
        //    //    var result = await employmentPerformanceReviewRepository.UpdatePersonVisaAsync(employmentPerformanceReviewRequest);

        //    //    Assert.AreEqual(employmentPerformanceReviewRequest.PersonId, result.PersonId);
        //    //    Assert.AreEqual(employmentPerformanceReviewRequest.StrGuid, result.StrGuid);
        //    //}

        //    [TestMethod]
        //    public async Task EmploymentPerformanceReviewRepo_DeleteEmploymentPerformanceReviewAsync()
        //    {
        //        iColleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<DeletePerposReviewRequest, DeletePerposReviewResponse>(It.IsAny<DeletePerposReviewRequest>())).ReturnsAsync(deleteEmploymentPerformanceReviewResponse);
        //        await employmentPerformanceReviewRepository.DeleteEmploymentPerformanceReviewsAsync(id);
        //    }

        //    //[TestMethod]
        //    //[ExpectedException(typeof(InvalidOperationException))]
        //    //public async Task PersonVisasRepo_UpdatePersonVisaAsync_InvalidOperationException()
        //    //{
        //    //    updateResponse.VisaErrorMessages = new List<VisaErrorMessages>() { new VisaErrorMessages() { ErrorCode = "123", ErrorMsg = "Error occured" } };
        //    //    iColleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<UpdatePersonVisaRequest, UpdatePersonVisaResponse>(It.IsAny<UpdatePersonVisaRequest>())).ReturnsAsync(updateResponse);
        //    //    var result = await employmentPerformanceReviewRepository.UpdatePersonVisaAsync(employmentPerformanceReviewRequest);
        //    //}

        //    [TestMethod]
        //    [ExpectedException(typeof(RepositoryException))]
        //    public async Task EmploymentPerformanceReviewsRepo_DeleteEmploymentPerformanceReviewAsync_RepositoryException()
        //    {
        //        deleteEmploymentPerformanceReviewResponse.DeleteReviewErrors = new List<DeleteReviewErrors>() { new DeleteReviewErrors() { ErrorCodes = "123", ErrorMessages = "Error Occured" } };

        //        iColleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<DeletePerposReviewRequest, DeletePerposReviewResponse>(It.IsAny<DeletePerposReviewRequest>())).ReturnsAsync(deleteEmploymentPerformanceReviewResponse);
        //        await employmentPerformanceReviewRepository.DeleteEmploymentPerformanceReviewsAsync(id);
        //    }
        //}
    }
}
