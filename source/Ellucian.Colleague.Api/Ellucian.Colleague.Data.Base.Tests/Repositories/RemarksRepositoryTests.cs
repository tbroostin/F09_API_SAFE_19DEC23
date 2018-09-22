// Copyright 2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
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
    public class RemarkRepositoryTests
    {
        [TestClass]
        public class RemarkGetMethods
        {
            Mock<ICacheProvider> iCacheProviderMock;
            Mock<IColleagueTransactionFactory> iColleagueTransactionFactoryMock;
            Mock<IColleagueTransactionInvoker> iColleagueTransactionInvokerMock;
            Mock<ILogger> iLoggerMock;
            Mock<IColleagueDataReader> dataReaderMock;
            IColleagueDataReader dataReader;
            ApiSettings apiSettings;

            RemarkRepository remarksRepository;
            Remark remark;
            Collection<Remarks> remarks;
            Remarks SingleRemark;
            Dictionary<string, GuidLookupResult> guidLookupResults;
            
            Ellucian.Colleague.Data.Base.DataContracts.Person personContract;
            UpdateRemarkRequest updateRequest;
            UpdateRemarkResponse updateResponse;
            DeleteRemarksRequest deleteRemarkRequest;
            DeleteRemarksResponse deleteRemarkResponse;
            string id = string.Empty;
            string recKey = string.Empty;

            [TestInitialize]
            public void Initialize()
            {
                iCacheProviderMock = new Mock<ICacheProvider>();
                iColleagueTransactionFactoryMock = new Mock<IColleagueTransactionFactory>();
                iLoggerMock = new Mock<ILogger>();
                dataReaderMock = new Mock<IColleagueDataReader>();
                iColleagueTransactionInvokerMock = new Mock<IColleagueTransactionInvoker>();
                iColleagueTransactionFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
                iColleagueTransactionFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(iColleagueTransactionInvokerMock.Object);
                
                GuidLookupResult res = new GuidLookupResult() { Entity = "REMARKS", PrimaryKey = "1", SecondaryKey = "" };
                Dictionary<string, GuidLookupResult> dict = new Dictionary<string, GuidLookupResult>();
                dict.Add("1", res);

                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(lookup =>
                {
                    return Task.FromResult(dict);
                });         

                BuildObjects();
                
                dataReaderMock.Setup(i => i.ReadRecordAsync<Remarks>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(SingleRemark);
                remarksRepository = new RemarkRepository(iCacheProviderMock.Object, iColleagueTransactionFactoryMock.Object, iLoggerMock.Object, apiSettings);
            }

            private void BuildObjects()
            {
                id = "b3d2a49b-df50-4133-9507-ecad4e04104f";
                recKey = "0012297";
                
                remark = BuildRemark();

                Remarks RemarkToAdd = new Remarks();

                RemarkToAdd = new Remarks()
                {
                    RemarksType = remark.RemarksType,
                    RemarksAuthor = remark.RemarksAuthor,
                    RemarksDate = remark.RemarksDate,
                    RemarksCode = remark.RemarksCode,
                    RemarksText = remark.RemarksText,
                    RemarksDonorId = remark.RemarksDonorId,
                    RecordGuid = remark.Guid,
                    RemarksIntgEnteredBy = remark.RemarksIntgEnteredBy,
                    Recordkey = remark.Id,
                    RemarksPrivateFlag = "Y"
                };

                SingleRemark = RemarkToAdd;

                remarks = new Collection<Remarks>();
                remarks.Add(RemarkToAdd);

                var _allRemarks = new TestRemarksRepository().GetRemarkCode().ToList();

                foreach(var thisRemark in _allRemarks)
                {
                    RemarkToAdd = new Remarks();
                    RemarkToAdd.RemarksType = thisRemark.RemarksType;
                    RemarkToAdd.RemarksAuthor = thisRemark.RemarksAuthor;
                    RemarkToAdd.RemarksDate = thisRemark.RemarksDate;
                    RemarkToAdd.RemarksCode = thisRemark.RemarksCode;
                    RemarkToAdd.RemarksText = thisRemark.RemarksText;
                    RemarkToAdd.RemarksDonorId = thisRemark.RemarksDonorId;
                    RemarkToAdd.RecordGuid = thisRemark.Guid;
                    RemarkToAdd.RemarksIntgEnteredBy = thisRemark.RemarksIntgEnteredBy;
                    RemarkToAdd.Recordkey = thisRemark.Id;
                    switch (thisRemark.RemarksPrivateType)
                    {
                        case ConfidentialityType.Public:
                            RemarkToAdd.RemarksPrivateFlag = "N";
                            break;
                        case ConfidentialityType.Private:
                            RemarkToAdd.RemarksPrivateFlag = "Y";
                            break;
                        default:
                            RemarkToAdd.RemarksPrivateFlag = "N";
                            break;
                    }

                    remarks.Add(RemarkToAdd);
                }

                personContract = new DataContracts.Person()
                {
                    Recordkey = recKey,
                    RecordGuid = id,
                    VisaType = "F1",
                    VisaIssuedDate = new DateTime(2015, 10, 17),
                    VisaExpDate = new DateTime(2017, 12, 17),
                    PersonCountryEntryDate = new DateTime(2016, 02, 05)
                };
                              
                updateRequest = new UpdateRemarkRequest()
                {
                    RmkType = remark.RemarksType,
                    RmkAuthor = remark.RemarksAuthor,
                    RmkDate = remark.RemarksDate,
                    RmkCode = remark.RemarksCode,
                    RmkText = new List<string>() { remark.RemarksText },
                    RmkPersonId = remark.RemarksDonorId,
                    RmkGuid = remark.Guid,
                    RmkEnteredBy = remark.RemarksIntgEnteredBy
                };
               
                switch (remark.RemarksPrivateType)
                {
                    case ConfidentialityType.Public:
                        updateRequest.RmkPrivateFlag = "N";
                        break;
                    case ConfidentialityType.Private:
                        updateRequest.RmkPrivateFlag = "Y";
                        break;
                    default:
                        updateRequest.RmkPrivateFlag = "N";
                        break;
                }
               
                updateResponse = new UpdateRemarkResponse()
                {
                    RmkGuid = remark.Guid,
                    RmkId = recKey
                };
               
                
                deleteRemarkRequest = new DeleteRemarksRequest()
                {
                    RmkGuid = remark.Guid, 
                    RmkId = recKey
                };
                deleteRemarkResponse = new DeleteRemarksResponse();
            }

            [TestCleanup]
            public void Cleanup()
            {
                remarksRepository = null;
                remark = null;
                guidLookupResults = null;
               
                personContract = null;
                updateRequest = null;
                updateResponse = null;
                deleteRemarkRequest = null;
                deleteRemarkResponse = null;
                id = string.Empty;
                recKey = string.Empty;
            }
            private Remark BuildRemark()
            {
                Remark remark = new Remark(id);
                remark.RemarksAuthor = "0011905";
                remark.RemarksCode = "ST";
                remark.RemarksDate = DateTime.Now;
                remark.RemarksDonorId = "0013395";
                remark.RemarksIntgEnteredBy = "BSF";
                remark.RemarksPrivateType = ConfidentialityType.Public;
                remark.RemarksText = "Hello World";
                remark.RemarksType = "BU";
                remark.Id = recKey;
                return remark;
            }


            [TestMethod]
            public async Task RemarkRepo_UpdateRemarkAsync()
            {
                remark.RemarksPrivateType = ConfidentialityType.Private;
                iColleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<UpdateRemarkRequest, UpdateRemarkResponse>(It.IsAny<UpdateRemarkRequest>())).ReturnsAsync(updateResponse);
                var result = await remarksRepository.UpdateRemarkAsync(remark);

                Assert.AreEqual(updateRequest.RmkGuid, result.Guid);
                Assert.AreEqual(updateRequest.RmkAuthor, result.RemarksAuthor);
                Assert.AreEqual(updateRequest.RmkPrivateFlag, "N");

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RemarkRepo_UpdateRemarkAsync_Empty()
            {
                iColleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<UpdateRemarkRequest, UpdateRemarkResponse>(It.IsAny<UpdateRemarkRequest>())).ReturnsAsync(updateResponse);
                var result = await remarksRepository.UpdateRemarkAsync(null);

            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task RemarkRepo_UpdateRemarkAsync_Error()
            {
                updateResponse = new UpdateRemarkResponse()
                {
                    ErrorMessage = new List<string>() { "An error has occurred." }
                }; 
                
                iColleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<UpdateRemarkRequest, UpdateRemarkResponse>(It.IsAny<UpdateRemarkRequest>())).ReturnsAsync(updateResponse);
                var result = await remarksRepository.UpdateRemarkAsync(remark);          
            }



            [TestMethod]
            public async Task RemarkRepo_DeleteRemarkAsync()
            {
                iColleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<DeleteRemarksRequest, DeleteRemarksResponse>(It.IsAny<DeleteRemarksRequest>())).ReturnsAsync(deleteRemarkResponse);
                await remarksRepository.DeleteRemarkAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task RemarkRepo_UpdateRemarkAsync_InvalidOperationException()
            {
                updateRequest.RmkId = Guid.Empty.ToString();
                iColleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<UpdateRemarkRequest, UpdateRemarkResponse>(It.IsAny<UpdateRemarkRequest>())).Throws<InvalidOperationException>();
                var result = await remarksRepository.UpdateRemarkAsync(remark);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task RemarkRepo_DeleteRemarkAsync_RepositoryException()
            {
                deleteRemarkRequest.RmkId = Guid.Empty.ToString();
                iColleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<DeleteRemarksRequest, DeleteRemarksResponse>(It.IsAny<DeleteRemarksRequest>())).Throws<RepositoryException>();
                await remarksRepository.DeleteRemarkAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RemarkRepo_DeleteRemarkAsync_ArgumentNull()
            {
                deleteRemarkRequest.RmkId = Guid.Empty.ToString();
                iColleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<DeleteRemarksRequest, DeleteRemarksResponse>(It.IsAny<DeleteRemarksRequest>())).Throws<RepositoryException>();
                await remarksRepository.DeleteRemarkAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RemarkRepo_DeleteRemarkAsync_ArgumentEmpty()
            {
                deleteRemarkRequest.RmkId = Guid.Empty.ToString();
                iColleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<DeleteRemarksRequest, DeleteRemarksResponse>(It.IsAny<DeleteRemarksRequest>())).ReturnsAsync(null);
                await remarksRepository.DeleteRemarkAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task RemarkRepo_DeleteRemarkAsync_TransactionEmpty()
            {
                iColleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<DeleteRemarksRequest, DeleteRemarksResponse>(It.IsAny<DeleteRemarksRequest>())).Throws<RepositoryException>();
                await remarksRepository.DeleteRemarkAsync(id);
            }


            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task RemarkRepo_DeleteRemarkAsync_ErrorMessages()
            {
                deleteRemarkResponse = new DeleteRemarksResponse()
                {
                
                    ErrorMessages = new List<string>() { "An error has occurred."}
                };
                
                iColleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<DeleteRemarksRequest, DeleteRemarksResponse>(It.IsAny<DeleteRemarksRequest>())).ReturnsAsync(deleteRemarkResponse);
                await remarksRepository.DeleteRemarkAsync(id);
            }

            [TestMethod]
            public async Task RemarkRepo_GetRemarks_commentSubjectArea()
            {

                var remarksCollection = new Collection<Remarks>() { SingleRemark };
                string[] remarksIds = new[] { recKey };
                dataReaderMock.Setup(i => i.SelectAsync("REMARKS", It.IsAny<string>())).ReturnsAsync(remarksIds); ;

                dataReaderMock.Setup(i => i.BulkReadRecordAsync<Remarks>("REMARKS", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(remarksCollection);
                                
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<Remarks>("REMARKS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(remarksCollection);
                
                
                var result = await remarksRepository.GetRemarksAsync(0, 100, "", "BU");

                Assert.AreEqual(updateRequest.RmkGuid, result.Item1.ElementAtOrDefault(0).Guid);
                Assert.AreEqual(updateRequest.RmkAuthor, result.Item1.ElementAtOrDefault(0).RemarksAuthor);
                Assert.AreEqual(updateRequest.RmkPrivateFlag, "N");

            }

            [TestMethod]
            public async Task RemarkRepo_GetRemarks_subjectMatter()
            {

                var remarksCollection = new Collection<Remarks>() { SingleRemark };
                string[] remarksIds = new[] { recKey };
                dataReaderMock.Setup(i => i.SelectAsync("REMARKS", It.IsAny<string>())).ReturnsAsync(remarksIds); ;

                dataReaderMock.Setup(i => i.BulkReadRecordAsync<Remarks>("REMARKS", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(remarksCollection);

                dataReaderMock.Setup(i => i.BulkReadRecordAsync<Remarks>("REMARKS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(remarksCollection);

                var result = await remarksRepository.GetRemarksAsync(0,100,"0012345", "");

                Assert.AreEqual(updateRequest.RmkGuid, result.Item1.ElementAtOrDefault(0).Guid);
                Assert.AreEqual(updateRequest.RmkAuthor, result.Item1.ElementAtOrDefault(0).RemarksAuthor);
                Assert.AreEqual(updateRequest.RmkPrivateFlag, "N");
            }

            [TestMethod]
            public async Task RemarkRepo_GetAllRemark()
            {

                var remarksCollection = remarks;

                dataReaderMock.Setup(i => i.BulkReadRecordAsync<Remarks>("REMARKS", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(remarksCollection);

                dataReaderMock.Setup(i => i.BulkReadRecordAsync<Remarks>("REMARKS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(remarksCollection);

                var results = await remarksRepository.GetRemarksAsync(0, 100, "", "");

                var expectedResult = remarks;

                foreach(var result in results.Item1)
                {
                    var expected = expectedResult.FirstOrDefault(i => i.RecordGuid.Equals(result.Guid));

                    Assert.AreEqual(expected.RecordGuid, result.Guid);
                    Assert.AreEqual(expected.RemarksCode, result.RemarksCode, "Code");
                    Assert.AreEqual(expected.RemarksDate, result.RemarksDate, "Remakrs date");
                    Assert.AreEqual(expected.RemarksDonorId, result.RemarksDonorId, "Donor ID");
                    Assert.AreEqual(expected.RemarksIntgEnteredBy, result.RemarksIntgEnteredBy, "INTG EnteredBy");
                    Assert.AreEqual(expected.RemarksType, result.RemarksType, "Type");
                    Assert.AreEqual(expected.RemarksText, result.RemarksText, "EnteredOn");

                    switch (expected.RemarksPrivateFlag)
                    {
                        case ("N"):
                            Assert.AreEqual(ConfidentialityType.Public, result.RemarksPrivateType, "Privacy");
                            break;
                        case ("Y"):
                            Assert.AreEqual(ConfidentialityType.Private, result.RemarksPrivateType, "Privacy");
                            break;
                        default:
                            Assert.AreEqual(ConfidentialityType.Public, result.RemarksPrivateType, "Privacy");
                            break;
                    }

                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RemarkRepo_GetRemarkFromGuidAsync_empty()
            {
                var result = await remarksRepository.GetRemarkFromGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RemarkRepo_BuildRemarks_empty()
            {
                var result = remarksRepository.BuildRemark(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RemarkRepo_GetRemarksAsync_empty()
            {
                var result = await remarksRepository.GetRemarkAsync("");
            }

        }
    }
}
