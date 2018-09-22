using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Data.Colleague;
using System.Runtime.Caching;
using slf4net;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Data.Base.Transactions;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class ApprovalRepositoryTests : BaseRepositorySetup
    {
        ApprovalRepository repository;
        List<ApprovalDocuments> approvalDocuments = new List<ApprovalDocuments>();
        List<ApprDocResponses> approvalResponses = new List<ApprDocResponses>();
        List<ApprovalDocument> documents = new List<ApprovalDocument>();
        List<ApprovalResponse> responses = new List<ApprovalResponse>();

        [TestInitialize]
        public void Initialize()
        {
            base.MockInitialize();
            repository = new ApprovalRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }

        #region Get Approval Document Tests

        [TestClass]
        public class ApprovalRepositoryTests_GetApprovalDocument : ApprovalRepositoryTests
        {
            [TestInitialize]
            public void ApprovalRepositoryTests_GetApprovalDocument_Initialize()
            {
                base.Initialize();
                SetupApprovalDocuments();

                dataReaderMock.Setup<ApprovalDocuments>(
                    reader => reader.ReadRecord<ApprovalDocuments>(It.IsAny<string>(), true))
                    .Returns<string, bool>((id, replaceVm) =>
                    {
                        var approvalDocument = this.approvalDocuments.FirstOrDefault(x => x.Recordkey == id);
                        return approvalDocument;
                    }
                );
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApprovalRepositoryTests_GetApprovalDocument_NullApprovalDocument()
            {
                var result = this.repository.GetApprovalDocument(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApprovalRepositoryTests_GetApprovalDocument_EmptyApprovalDocument()
            {
                var result = this.repository.GetApprovalDocument(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void ApprovalRepositoryTests_GetApprovalDocument_BadApprovalDocumentId()
            {
                var result = this.repository.GetApprovalDocument("345");
            }

            [TestMethod]
            public void ApprovalRepositoryTests_GetApprovalDocument_ValidApprovalDocument()
            {
                var random = new Random();
                var id = (random.Next(this.documents.Count) + 1).ToString();
                var result = this.repository.GetApprovalDocument(id);

                Assert.AreEqual(id, result.Id);
                Assert.IsNotNull(result.Text);
                Assert.IsTrue(result.Text.Count > 0);
            }
        }

        #endregion

        #region Get Approval Response Tests

        [TestClass]
        public class ApprovalRepositoryTests_GetApprovalResponse : ApprovalRepositoryTests
        {
            [TestInitialize]
            public void ApprovalRepositoryTests_GetApprovalResponse_Initialize()
            {
                base.Initialize();
                SetupApprovalDocuments();

                dataReaderMock.Setup<ApprDocResponses>(
                    reader => reader.ReadRecord<ApprDocResponses>(It.IsAny<string>(), true))
                    .Returns<string, bool>((id, replaceVm) =>
                    {
                        var approvalresponse = this.approvalResponses.FirstOrDefault(x => x.Recordkey == id);
                        return approvalresponse;
                    }
                );
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApprovalRepositoryTests_GetApprovalResponse_NullApprovalResponse()
            {
                var result = this.repository.GetApprovalDocument(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApprovalRepositoryTests_GetApprovalResponse_EmptyApprovalResponse()
            {
                var result = this.repository.GetApprovalDocument(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void ApprovalRepositoryTests_GetApprovalResponse_BadApprovalResponseId()
            {
                var result = this.repository.GetApprovalDocument("345");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void ApprovalRepositoryTests_GetApprovalResponse_ValidApprovalResponse()
            {
                var random = new Random();
                var id = random.Next(this.responses.Count).ToString();
                var result = this.repository.GetApprovalResponse(id);
                var expected = this.approvalResponses.First(x => x.Recordkey == id);

                Assert.AreEqual(expected.Recordkey, result.Id);
                Assert.AreEqual(expected.ApdrApprovalDocument, result.DocumentId);
                Assert.AreEqual(expected.ApdrApproved, result.IsApproved ? "Y" : "N");
                Assert.AreEqual(expected.ApdrDate, result.Received.Date);
                Assert.AreEqual(expected.ApdrTime.Value.TimeOfDay, result.Received.TimeOfDay);
                Assert.AreEqual(expected.ApdrPersonId, result.PersonId);
                Assert.AreEqual(expected.ApdrUserid, result.UserId);
            }
        }

        #endregion

        #region Create Approval Document tests
        [TestClass]
        public class CreateApprovalDocumentTests : ApprovalRepositoryTests
        {
            [TestInitialize]
            public void CreateApprovalDocument_Initialize()
            {
                base.Initialize();
                SetupApprovalDocuments();

                transManagerMock.Setup<CreateApprovalDocumentResponse>(
                    trans => trans.Execute<CreateApprovalDocumentRequest, CreateApprovalDocumentResponse>(It.IsAny<CreateApprovalDocumentRequest>()))
                    .Returns<CreateApprovalDocumentRequest>(req =>
                    {
                        var response = new CreateApprovalDocumentResponse();
                        // Use the person ID to key the response
                        switch (req.PersonId)
                        {
                            case "1":   // Return a document ID
                                response.ApprovalDocumentId = "123";
                                break;
                            case "2":   // Return no document ID and no error message
                                break;
                            default:    // Return an error message
                                response.ErrorMessage = "Error occurred.";
                                break;
                        }

                        return response;
                    }
                );
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApprovalRepository_CreateApprovalDocument_NullApprovalDocument()
            {
                var result = this.repository.CreateApprovalDocument(null);
            }

            [TestMethod]
            public void ApprovalRepository_CreateApprovalDocument_ValidApprovalDocument()
            {
                var template = this.documents[0];
                var document = new ApprovalDocument(null, template.Text);
                document.PersonId = "1";
                var result = this.repository.CreateApprovalDocument(document);

                Assert.AreEqual("123", result.Id);
                CollectionAssert.AreEqual(document.Text, result.Text);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void ApprovalRepository_CreateApprovalDocument_NoDataReturned()
            {
                var template = this.documents[0];
                var document = new ApprovalDocument(null, template.Text);
                document.PersonId = "2";
                var result = this.repository.CreateApprovalDocument(document);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ApprovalRepository_CreateApprovalDocument_ErrorReturned()
            {
                var template = this.documents[0];
                var document = new ApprovalDocument(null, template.Text);
                document.PersonId = "9";
                var result = this.repository.CreateApprovalDocument(document);
            }
        }
    
        #endregion

        #region Create Approval Response tests
        
        [TestClass]
        public class CreateApprovalResponse : ApprovalRepositoryTests
        {
            [TestInitialize]
            public void ApprovalRepositoryTests_CreateApprovalResponse_Initialize()
            {
                base.Initialize();
                SetupApprovalResponses();

                transManagerMock.Setup<CreateApprovalResponseResponse>(
                    trans => trans.Execute<CreateApprovalResponseRequest, CreateApprovalResponseResponse>(It.IsAny<CreateApprovalResponseRequest>()))
                    .Returns<CreateApprovalResponseRequest>(req =>
                    {
                        var response = new CreateApprovalResponseResponse();
                        // Use the person ID to key the response
                        switch (req.PersonId)
                        {
                            case "1":   // Return a document ID
                                response.ApprovalResponseId = "123";
                                break;
                            case "2":   // Return no document ID and no error message
                                break;
                            default:    // Return an error message
                                response.ErrorMessage = "Error occurred.";
                                break;
                        }

                        return response;
                    }
                );
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApprovalRepository_CreateApprovalResponse_NullApprovalResponse()
            {
                var result = this.repository.CreateApprovalResponse(null);
            }

            [TestMethod]
            public void ApprovalRepository_CreateApprovalResponse_ValidApprovalResponse()
            {
                var template = this.responses[0];
                var response = new ApprovalResponse(null, template.DocumentId, "1", template.UserId, template.Received, template.IsApproved);
                var result = this.repository.CreateApprovalResponse(response);

                Assert.AreEqual("123", result.Id);
                Assert.AreEqual(response.DocumentId, result.DocumentId);
                Assert.AreEqual("1", result.PersonId);
                Assert.AreEqual(response.UserId, result.UserId);
                Assert.AreEqual(response.Received, result.Received);
                Assert.AreEqual(response.IsApproved, result.IsApproved);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void ApprovalRepository_CreateApprovalResponse_NoDataReturned()
            {
                var template = this.responses[0];
                var response = new ApprovalResponse(null, template.DocumentId, "2", template.UserId, template.Received, template.IsApproved);
                var result = this.repository.CreateApprovalResponse(response);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ApprovalRepository_CreateApprovalResponse_ErrorReturned()
            {
                var template = this.responses[0];
                var response = new ApprovalResponse(null, template.DocumentId, "9", template.UserId, template.Received, template.IsApproved);
                var result = this.repository.CreateApprovalResponse(response);
            }
        }

        #endregion

        #region Get Approval Response tests

        [TestClass]
        public class ApprovalRepository_GetApprovalResponse : ApprovalRepositoryTests
        {
            [TestInitialize]
            public void ApprovalRepository_GetApprovalResponse_Initialize()
            {
                base.Initialize();
                SetupApprovalResponses();
                dataReaderMock.Setup<ApprDocResponses>(
                    reader => reader.ReadRecord<ApprDocResponses>(It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns<string, bool>((id, replaceVm) =>
                    {
                        var approvalResponse = this.approvalResponses.FirstOrDefault(x => x.Recordkey == id);
                        return approvalResponse;
                    }
                );
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApprovalRepository_GetApprovalResponse_NullId()
            {
                var resp = this.repository.GetApprovalResponse(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApprovalRepository_GetApprovalResponse_EmptyId()
            {
                var resp = this.repository.GetApprovalResponse(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void ApprovalRepository_GetApprovalResponse_InvalidId()
            {
                var resp = this.repository.GetApprovalResponse("INVALID");
            }

            [TestMethod]
            public void ApprovalRepository_GetApprovalResponse_Verify()
            {
                for (int i = 0; i < this.approvalResponses.Count; i++)
                {
                    var resp = this.repository.GetApprovalResponse(approvalResponses[i].Recordkey);
                    Assert.AreEqual(approvalResponses[i].Recordkey, resp.Id);
                    Assert.AreEqual(approvalResponses[i].ApdrUserid, resp.UserId);
                    Assert.AreEqual(approvalResponses[i].ApdrApproved == "Y", resp.IsApproved);
                }
            }
        }

        #endregion

        #region Private data setup definition

        #region Approval Documents
        protected void SetupApprovalDocuments()
        {
            string[,] approvalDocsData = GetApprovalDocumentsData();
            int approvalDocsCount = approvalDocsData.Length / 2;

            for (int i = 0; i < approvalDocsCount; i++)
            {
                string key = (i + 1).ToString();
                List<String> text = approvalDocsData[i, 0].Trim().Split(';').ToList();
                string personId = approvalDocsData[i, 1].Trim();

                // Data Contract Setup
                ApprovalDocuments approvalDocument = new ApprovalDocuments()
                {
                    Recordkey = key,
                    ApdText = string.Join(" ", text.ToArray()),
                    //ApdPersonId = personId
                };
                this.approvalDocuments.Add(approvalDocument);

                // Domain Entity Setup
                var document = new ApprovalDocument(key, text);
                this.documents.Add(document);
            }
        }

        private string[,] GetApprovalDocumentsData()
        {
            string[,] approvalDocumentsData = {    //Text                                                                                                                                                                                                                     Person ID
                                                  { "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."                                                                                         , "0003315"},
                                                  { "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat."                                                                                                          , ""},
                                                  { "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.;Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.", "0008060"}
                                              };
            return approvalDocumentsData;
        }
        #endregion

        #region Approval Responses
        private void SetupApprovalResponses()
        {
            string[,] approvalRespsData = GetApprovalResponsesData();
            int approvalRespsCount = approvalRespsData.Length / 5;
            for (int i = 0; i < approvalRespsCount; i++)
            {
                string key = (i + 1).ToString();
                string documentId = approvalRespsData[i, 0].Trim();
                string personId = approvalRespsData[i, 1].Trim();
                string userId = approvalRespsData[i, 2].Trim();
                DateTime received = DateTime.Parse(approvalRespsData[i, 3].Trim());
                bool isApproved = Boolean.Parse(approvalRespsData[i, 4].Trim());

                // Data Contract Setup
                ApprDocResponses approvalResponse = new ApprDocResponses()
                {
                    Recordkey = (i + 1).ToString(),
                    ApdrApprovalDocument = documentId,
                    ApdrPersonId = personId,
                    ApdrUserid = userId,
                    ApdrDate = received.Date,
                    //ApdrTime = null,
                    ApdrApproved = isApproved.ToString(),
                };
                this.approvalResponses.Add(approvalResponse);

                // Domain Entity Setup
                var approval = new ApprovalResponse(key, documentId, personId, userId, received, isApproved);
                this.responses.Add(approval);
            }

            //dataReaderMock.Setup<Collection<ApprDocResponses>>(
            //    reader => reader.BulkReadRecord<ApprDocResponses>(It.IsAny<string[]>()))
            //    .Returns<string[]>(ids =>
            //    {
            //        var approvalResps = new Collection<ApprDocResponses>();
            //        foreach (var id in ids)
            //        {
            //            var approvalResp = this.approvalResps.Where(x => x.Recordkey == id).FirstOrDefault();
            //            if (approvalResp != null)
            //            {
            //                approvalResps.Add(approvalResp);
            //            }
            //        }
            //        return approvalResps;
            //    }
            //);
        }

        private string[,] GetApprovalResponsesData()
        {
            string[,] approvalResponsesData = {
                                                  // Document ID   Person ID  User ID     Received Date   Approved? 
                                                  { "1234567890", "0006966", "tcarmen" , "10/01/2013",   "true"   },
                                                  { "1357924680", "0007718", "lvaldez" , "11/15/2013",   "true",  },
                                                  { "0864297531", "0007912", "jsherman", "12/31/2013",   "false", }
                                              };
            return approvalResponsesData;
        }
        #endregion

        #endregion
    }
}
