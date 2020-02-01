// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Tests.Builders;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class TaxFormConsentRepositoryTests
    {
        #region Initialize and Cleanup
        private ApiSettings apiSettings = new ApiSettings("API") { ColleagueTimeZone = "Eastern Standard Time" };
        private Mock<IColleagueDataReader> dataReaderMock = null;
        private Mock<IColleagueTransactionInvoker> transactionInvoker = null;
        private CreateDocConsentHistoryResponse createDocConsentHistoryResponse = new CreateDocConsentHistoryResponse();
        private TaxFormConsentRepository taxFormConsentRepository;
        private TaxFormConsentBuilder builder;
        private Collection<DocConsentHistory> docConsentHistoryContracts;
        private Collection<W2ConsentHistory> w2ConsentHistoryContracts;
        private Collection<T4ConsentHistory> t4ConsentHistoryContracts;
        private Collection<T4aConsentHistory> t4aConsentHistoryContracts;
        private string personId = "0001234";
        private string formInContext = "1095C";

        [TestInitialize]
        public void Initialize()
        {
            dataReaderMock = new Mock<IColleagueDataReader>();
            transactionInvoker = new Mock<IColleagueTransactionInvoker>();
            taxFormConsentRepository = BuildTaxFormConsentRepository();
            this.builder = new TaxFormConsentBuilder();
            InitializeMockStatements();
        }

        [TestCleanup]
        public void Cleanup()
        {
            docConsentHistoryContracts = null;
            dataReaderMock = null;
            transactionInvoker = null;
            taxFormConsentRepository = null;
            this.builder = null;
        }
        #endregion

        #region Tests for GetAsync
        [TestMethod]
        public async Task GetAsync_W2()
        {
            var formToSelect = TaxForms.FormW2;
            var taxFormConsentEntities = await taxFormConsentRepository.GetAsync(personId, formToSelect);
            foreach (var consentContract in w2ConsentHistoryContracts)
            {
                // For each consent history object in my input set, confirmat that 
                // there is one and only matching consent history object in the output set
                var expectedPersonId = consentContract.W2chHrperId;
                var expectedTaxForm = formToSelect;
                var expectedStatus = consentContract.W2chNewStatus == "C";
                var expectedTimeStamp = ColleagueTimeZoneUtility.ToPointInTimeDateTimeOffset(consentContract.W2chStatusTime, consentContract.W2chStatusDate, apiSettings.ColleagueTimeZone).GetValueOrDefault();

                var consentEntity = taxFormConsentEntities.FirstOrDefault(x =>
                    x.PersonId == expectedPersonId
                 && x.TaxForm == expectedTaxForm
                 && x.HasConsented == expectedStatus
                 && x.TimeStamp == expectedTimeStamp);

                Assert.IsNotNull(consentEntity);
            }
        }

        [TestMethod]
        public async Task GetAsync_1095C()
        {
            formInContext = "1095C";
            var formToSelect = TaxForms.Form1095C;
            var taxFormConsentEntities = await taxFormConsentRepository.GetAsync(personId, formToSelect);

            var form1095Contracts = docConsentHistoryContracts.Where(x => x.DchistDocument == formInContext).ToList();
            Assert.AreEqual(form1095Contracts.Count(), taxFormConsentEntities.Count());
            foreach (var consentContract in form1095Contracts)
            {
                // For each consent history object in my input set, confirmat that 
                // there is one and only matching consent history object in the output set
                var expectedPersonId = consentContract.DchistPersonId;
                var expectedTaxForm = formToSelect;
                var expectedStatus = consentContract.DchistStatus == "C";
                var expectedTimeStamp = ColleagueTimeZoneUtility.ToPointInTimeDateTimeOffset(consentContract.DchistStatusTime, consentContract.DchistStatusDate, apiSettings.ColleagueTimeZone).GetValueOrDefault();

                var consentEntity = taxFormConsentEntities.FirstOrDefault(x =>
                    x.PersonId == expectedPersonId
                    && x.TaxForm == expectedTaxForm
                    && x.HasConsented == expectedStatus
                    && x.TimeStamp == expectedTimeStamp);

                Assert.IsNotNull(consentEntity);
            }
        }

        [TestMethod]
        public async Task GetAsync_1098()
        {
            formInContext = "1098";
            var formToSelect = TaxForms.Form1098;
            var taxFormConsentEntities = await taxFormConsentRepository.GetAsync(personId, formToSelect);

            var form1098Contracts = docConsentHistoryContracts.Where(x => x.DchistDocument == formInContext).ToList();
            Assert.AreEqual(form1098Contracts.Count(), taxFormConsentEntities.Count());
            foreach (var consentContract in form1098Contracts)
            {
                // For each consent history object in my input set, confirmat that 
                // there is one and only matching consent history object in the output set
                var expectedPersonId = consentContract.DchistPersonId;
                var expectedTaxForm = formToSelect;
                var expectedStatus = consentContract.DchistStatus == "C";
                var expectedTimeStamp = ColleagueTimeZoneUtility.ToPointInTimeDateTimeOffset(consentContract.DchistStatusTime, consentContract.DchistStatusDate, apiSettings.ColleagueTimeZone).GetValueOrDefault();

                var consentEntity = taxFormConsentEntities.FirstOrDefault(x =>
                    x.PersonId == expectedPersonId
                    && x.TaxForm == expectedTaxForm
                    && x.HasConsented == expectedStatus
                    && x.TimeStamp == expectedTimeStamp);

                Assert.IsNotNull(consentEntity);
            }
        }

        [TestMethod]
        public async Task GetAsync_T4()
        {
            var formToSelect = TaxForms.FormT4;
            var taxFormConsentEntities = await taxFormConsentRepository.GetAsync(personId, formToSelect);
            Assert.AreEqual(t4ConsentHistoryContracts.Count(), taxFormConsentEntities.Count());

            foreach (var consentContract in t4ConsentHistoryContracts)
            {
                // For each consent history object in my input set, confirmat that 
                // there is one and only matching consent history object in the output set
                var expectedPersonId = consentContract.T4chHrperId;
                var expectedTaxForm = formToSelect;
                var expectedStatus = consentContract.T4chNewStatus == "C";
                var expectedTimeStamp = ColleagueTimeZoneUtility.ToPointInTimeDateTimeOffset(consentContract.T4chStatusTime, consentContract.T4chStatusDate, apiSettings.ColleagueTimeZone).GetValueOrDefault();

                var consentEntity = taxFormConsentEntities.FirstOrDefault(x =>
                    x.PersonId == expectedPersonId
                    && x.TaxForm == expectedTaxForm
                    && x.HasConsented == expectedStatus
                    && x.TimeStamp == expectedTimeStamp);

                Assert.IsNotNull(consentEntity);
            }
        }

        [TestMethod]
        public async Task GetAsync_T4A()
        {
            var formToSelect = TaxForms.FormT4A;
            var taxFormConsentEntities = await taxFormConsentRepository.GetAsync(personId, formToSelect);
            Assert.AreEqual(t4aConsentHistoryContracts.Count(), taxFormConsentEntities.Count());

            foreach (var consentContract in t4aConsentHistoryContracts)
            {
                // For each consent history object in my input set, confirmat that 
                // there is one and only matching consent history object in the output set
                var expectedPersonId = consentContract.T4achHrperId;
                var expectedTaxForm = formToSelect;
                var expectedStatus = consentContract.T4achNewStatus == "C";
                var expectedTimeStamp = ColleagueTimeZoneUtility.ToPointInTimeDateTimeOffset(consentContract.T4achStatusTime, consentContract.T4achStatusDate, apiSettings.ColleagueTimeZone).GetValueOrDefault();

                var consentEntity = taxFormConsentEntities.FirstOrDefault(x =>
                    x.PersonId == expectedPersonId
                    && x.TaxForm == expectedTaxForm
                    && x.HasConsented == expectedStatus
                    && x.TimeStamp == expectedTimeStamp);

                Assert.IsNotNull(consentEntity);
            }
        }

        [TestMethod]
        public async Task GetAsync_T2202A()
        {
            formInContext = "T2202A";
            var formToSelect = TaxForms.FormT2202A;
            var taxFormConsentEntities = await taxFormConsentRepository.GetAsync(personId, formToSelect);

            var form1098Contracts = docConsentHistoryContracts.Where(x => x.DchistDocument == formInContext).ToList();
            Assert.AreEqual(form1098Contracts.Count(), taxFormConsentEntities.Count());
            foreach (var consentContract in docConsentHistoryContracts)
            {
                // For each consent history object in my input set, confirmat that 
                // there is one and only matching consent history object in the output set
                var expectedPersonId = consentContract.DchistPersonId;
                var expectedTaxForm = formToSelect;
                var expectedStatus = consentContract.DchistStatus == "C";
                var expectedTimeStamp = ColleagueTimeZoneUtility.ToPointInTimeDateTimeOffset(consentContract.DchistStatusTime, consentContract.DchistStatusDate, apiSettings.ColleagueTimeZone).GetValueOrDefault();

                var consentEntity = taxFormConsentEntities.FirstOrDefault(x =>
                    x.PersonId == expectedPersonId
                    && x.TaxForm == expectedTaxForm
                    && x.HasConsented == expectedStatus
                    && x.TimeStamp == expectedTimeStamp);

                Assert.IsNotNull(consentEntity);
            }
        }

        [TestMethod]
        public async Task GetAsync_1099MI()
        {
            formInContext = "1099MI";
            var formToSelect = TaxForms.Form1099MI;
            var taxFormConsentEntities = await taxFormConsentRepository.GetAsync(personId, formToSelect);

            var form1099MiContracts = docConsentHistoryContracts.Where(x => x.DchistDocument == formInContext).ToList();
            Assert.AreEqual(form1099MiContracts.Count(), taxFormConsentEntities.Count());
            foreach (var consentContract in docConsentHistoryContracts)
            {
                // For each consent history object in my input set, confirmat that 
                // there is one and only matching consent history object in the output set
                var expectedPersonId = consentContract.DchistPersonId;
                var expectedTaxForm = formToSelect;
                var expectedStatus = consentContract.DchistStatus == "C";
                var expectedTimeStamp = ColleagueTimeZoneUtility.ToPointInTimeDateTimeOffset(consentContract.DchistStatusTime, consentContract.DchistStatusDate, apiSettings.ColleagueTimeZone).GetValueOrDefault();

                var consentEntity = taxFormConsentEntities.FirstOrDefault(x =>
                    x.PersonId == expectedPersonId
                    && x.TaxForm == expectedTaxForm
                    && x.HasConsented == expectedStatus
                    && x.TimeStamp == expectedTimeStamp);

                Assert.IsNotNull(consentEntity);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetAsync_NullPersonIdfor1099Mi()
        {
            await taxFormConsentRepository.GetAsync(null, TaxForms.Form1099MI);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetAsync_EmptyPersonIdfor1099Mi()
        {
            await taxFormConsentRepository.GetAsync(string.Empty, TaxForms.Form1099MI);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetAsync_NullPersonId()
        {
            await taxFormConsentRepository.GetAsync(null, TaxForms.FormW2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetAsync_EmptyPersonId()
        {
            await taxFormConsentRepository.GetAsync(string.Empty, TaxForms.FormW2);
        }

        [TestMethod]
        public async Task GetAsync_NullDateTimeOffset()
        {
            this.w2ConsentHistoryContracts[0].W2chStatusDate = null;
            this.w2ConsentHistoryContracts[0].W2chStatusTime = null;
            var consentEntities = await taxFormConsentRepository.GetAsync("0003946", TaxForms.FormW2);

            // All contracts - except one - should be processed and returned as domain entities.
            Assert.AreEqual(this.w2ConsentHistoryContracts.Count - 1, consentEntities.Count());
        }
        #endregion

        #region Tests for PostAsync
        [TestMethod]
        public async Task PostAsync_W2_HasConsented()
        {
            var incomingConsent = this.builder.WithTaxForm(TaxForms.FormW2).WithHasConsented(true).Build();
            var returnedConsent = await taxFormConsentRepository.PostAsync(incomingConsent);

            Assert.AreEqual(incomingConsent.HasConsented, returnedConsent.HasConsented);
            Assert.AreEqual(incomingConsent.PersonId, returnedConsent.PersonId);
            Assert.AreEqual(incomingConsent.TaxForm, returnedConsent.TaxForm);
            Assert.AreEqual(incomingConsent.TimeStamp, returnedConsent.TimeStamp);
        }

        [TestMethod]
        public async Task PostAsync_W2_HasWithheldConsented()
        {
            var incomingConsent = this.builder.WithTaxForm(TaxForms.FormW2).WithHasConsented(false).Build();
            var returnedConsent = await taxFormConsentRepository.PostAsync(incomingConsent);

            Assert.AreEqual(incomingConsent.HasConsented, returnedConsent.HasConsented);
            Assert.AreEqual(incomingConsent.PersonId, returnedConsent.PersonId);
            Assert.AreEqual(incomingConsent.TaxForm, returnedConsent.TaxForm);
            Assert.AreEqual(incomingConsent.TimeStamp, returnedConsent.TimeStamp);
        }

        [TestMethod]
        public async Task PostAsync_1095C()
        {
            var incomingConsent = this.builder.WithTaxForm(TaxForms.Form1095C).Build();
            var returnedConsent = await taxFormConsentRepository.PostAsync(incomingConsent);

            Assert.AreEqual(incomingConsent.HasConsented, returnedConsent.HasConsented);
            Assert.AreEqual(incomingConsent.PersonId, returnedConsent.PersonId);
            Assert.AreEqual(incomingConsent.TaxForm, returnedConsent.TaxForm);
            Assert.AreEqual(incomingConsent.TimeStamp, returnedConsent.TimeStamp);
        }

        [TestMethod]
        public async Task PostAsync_1098()
        {
            var incomingConsent = this.builder.WithTaxForm(TaxForms.Form1098).Build();
            var returnedConsent = await taxFormConsentRepository.PostAsync(incomingConsent);

            Assert.AreEqual(incomingConsent.HasConsented, returnedConsent.HasConsented);
            Assert.AreEqual(incomingConsent.PersonId, returnedConsent.PersonId);
            Assert.AreEqual(incomingConsent.TaxForm, returnedConsent.TaxForm);
            Assert.AreEqual(incomingConsent.TimeStamp, returnedConsent.TimeStamp);
        }

        [TestMethod]
        public async Task PostAsync_T4()
        {
            var incomingConsent = this.builder.WithTaxForm(TaxForms.FormT4).Build();
            var returnedConsent = await taxFormConsentRepository.PostAsync(incomingConsent);

            Assert.AreEqual(incomingConsent.HasConsented, returnedConsent.HasConsented);
            Assert.AreEqual(incomingConsent.PersonId, returnedConsent.PersonId);
            Assert.AreEqual(incomingConsent.TaxForm, returnedConsent.TaxForm);
            Assert.AreEqual(incomingConsent.TimeStamp, returnedConsent.TimeStamp);
        }

        [TestMethod]
        public async Task PostAsync_T4A()
        {
            var incomingConsent = this.builder.WithTaxForm(TaxForms.FormT4A).Build();
            var returnedConsent = await taxFormConsentRepository.PostAsync(incomingConsent);

            Assert.AreEqual(incomingConsent.HasConsented, returnedConsent.HasConsented);
            Assert.AreEqual(incomingConsent.PersonId, returnedConsent.PersonId);
            Assert.AreEqual(incomingConsent.TaxForm, returnedConsent.TaxForm);
            Assert.AreEqual(incomingConsent.TimeStamp, returnedConsent.TimeStamp);
        }

        [TestMethod]
        public async Task PostAsync_T2202A()
        {
            var incomingConsent = this.builder.WithTaxForm(TaxForms.FormT2202A).Build();
            var returnedConsent = await taxFormConsentRepository.PostAsync(incomingConsent);

            Assert.AreEqual(incomingConsent.HasConsented, returnedConsent.HasConsented);
            Assert.AreEqual(incomingConsent.PersonId, returnedConsent.PersonId);
            Assert.AreEqual(incomingConsent.TaxForm, returnedConsent.TaxForm);
            Assert.AreEqual(incomingConsent.TimeStamp, returnedConsent.TimeStamp);
        }

        [TestMethod]
        public async Task PostAsync_1099MI()
        {
            var incomingConsent = this.builder.WithTaxForm(TaxForms.Form1099MI).Build();
            var returnedConsent = await taxFormConsentRepository.PostAsync(incomingConsent);

            Assert.AreEqual(incomingConsent.HasConsented, returnedConsent.HasConsented);
            Assert.AreEqual(incomingConsent.PersonId, returnedConsent.PersonId);
            Assert.AreEqual(incomingConsent.TaxForm, returnedConsent.TaxForm);
            Assert.AreEqual(incomingConsent.TimeStamp, returnedConsent.TimeStamp);
        }
        #endregion

        #region Private methods
        private TaxFormConsentRepository BuildTaxFormConsentRepository()
        {
            // Instantiate all objects necessary to mock data reader and CTX calls.
            var cacheProviderObject = new Mock<ICacheProvider>().Object;
            var transactionFactory = new Mock<IColleagueTransactionFactory>();
            var transactionFactoryObject = transactionFactory.Object;
            var loggerObject = new Mock<ILogger>().Object;

            // The transaction factory has a method to get its data reader
            // Make sure that method returns our mock data reader
            transactionFactory.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
            transactionFactory.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transactionInvoker.Object);

            return new TaxFormConsentRepository(apiSettings, cacheProviderObject, transactionFactoryObject, loggerObject);
        }

        private void InitializeMockStatements()
        {
            #region W-2 consents
            var w2Now = DateTime.Now;
            w2ConsentHistoryContracts = new Collection<W2ConsentHistory>()
            {
                new W2ConsentHistory()
                {
                    Recordkey = "1234",
                    W2chHrperId = personId,
                    W2chNewStatus = "C",
                    W2chStatusDate = w2Now,
                    W2chStatusTime = w2Now,
                    W2ConsentHistoryAdddate = w2Now,
                    W2ConsentHistoryAddtime = w2Now
                },
                new W2ConsentHistory()
                {
                    Recordkey = "1235",
                    W2chHrperId = personId,
                    W2chNewStatus = "W",
                    W2chStatusDate = w2Now,
                    W2chStatusTime = w2Now,
                    W2ConsentHistoryAdddate = w2Now,
                    W2ConsentHistoryAddtime = w2Now
                },
            };
            dataReaderMock.Setup<Task<Collection<W2ConsentHistory>>>(datareader => datareader.BulkReadRecordAsync<W2ConsentHistory>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(w2ConsentHistoryContracts);
            });
            #endregion

            #region DOC.CONSENT.HISTORY
            var now = DateTime.Now;
            docConsentHistoryContracts = new Collection<DocConsentHistory>()
            {
                new DocConsentHistory()
                {
                    Recordkey = "998",
                    DchistDocument = "1095C",
                    DchistPersonId = personId,
                    DchistStatus = "C",
                    DchistStatusDate = now,
                    DchistStatusTime = now
                },
                new DocConsentHistory()
                {
                    Recordkey = "999",
                    DchistDocument = "1095C",
                    DchistPersonId = personId,
                    DchistStatus = "W",
                    DchistStatusDate = now,
                    DchistStatusTime = now
                },
                new DocConsentHistory()
                {
                    Recordkey = "1000",
                    DchistDocument = "1098",
                    DchistPersonId = personId,
                    DchistStatus = "C",
                    DchistStatusDate = now,
                    DchistStatusTime = now
                },
                new DocConsentHistory()
                {
                    Recordkey = "1001",
                    DchistDocument = "1098",
                    DchistPersonId = personId,
                    DchistStatus = "W",
                    DchistStatusDate = now,
                    DchistStatusTime = now
                },
                new DocConsentHistory()
                {
                    Recordkey = "1002",
                    DchistDocument = "T2202A",
                    DchistPersonId = personId,
                    DchistStatus = "C",
                    DchistStatusDate = now,
                    DchistStatusTime = now
                },
                new DocConsentHistory()
                {
                    Recordkey = "1003",
                    DchistDocument = "T2202A",
                    DchistPersonId = personId,
                    DchistStatus = "W",
                    DchistStatusDate = now,
                    DchistStatusTime = now
                },
                 new DocConsentHistory()
                {
                    Recordkey = "1004",
                    DchistDocument = "1099MI",
                    DchistPersonId = personId,
                    DchistStatus = "C",
                    DchistStatusDate = now,
                    DchistStatusTime = now
                },
                 new DocConsentHistory()
                {
                    Recordkey = "1005",
                    DchistDocument = "1099MI",
                    DchistPersonId = personId,
                    DchistStatus = "W",
                    DchistStatusDate = now,
                    DchistStatusTime = now
                },
            };
            dataReaderMock.Setup<Task<Collection<DocConsentHistory>>>(datareader => datareader.BulkReadRecordAsync<DocConsentHistory>(It.IsAny<string>(), true)).Returns(() =>
            {
                var collection = new Collection<DocConsentHistory>(docConsentHistoryContracts.Where(x => x.DchistDocument == formInContext).ToList());
                return Task.FromResult(collection);
            });
            #endregion

            #region T4 consents
            var t4Now = DateTime.Now;
            t4ConsentHistoryContracts = new Collection<T4ConsentHistory>()
            {
                new T4ConsentHistory()
                {
                    Recordkey = "1234",
                    T4chHrperId = personId,
                    T4chNewStatus = "C",
                    T4chStatusDate = t4Now,
                    T4chStatusTime = t4Now,
                    T4ConsentHistoryAdddate = t4Now,
                    T4ConsentHistoryAddtime = t4Now
                },
                new T4ConsentHistory()
                {
                    Recordkey = "1235",
                    T4chHrperId = personId,
                    T4chNewStatus = "W",
                    T4chStatusDate = t4Now,
                    T4chStatusTime = t4Now,
                    T4ConsentHistoryAdddate = t4Now,
                    T4ConsentHistoryAddtime = t4Now
                },
            };
            dataReaderMock.Setup<Task<Collection<T4ConsentHistory>>>(datareader => datareader.BulkReadRecordAsync<T4ConsentHistory>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(t4ConsentHistoryContracts);
            });
            #endregion

            #region T4A consents
            var t4aNow = DateTime.Now;
            t4aConsentHistoryContracts = new Collection<T4aConsentHistory>()
            {
                new T4aConsentHistory()
                {
                    Recordkey = "1234",
                    T4achHrperId = personId,
                    T4achNewStatus = "C",
                    T4achStatusDate = t4aNow,
                    T4achStatusTime = t4aNow,
                    T4aConsentHistoryAdddate = t4aNow,
                    T4aConsentHistoryAddtime = t4aNow
                },
                new T4aConsentHistory()
                {
                    Recordkey = "1235",
                    T4achHrperId = personId,
                    T4achNewStatus = "W",
                    T4achStatusDate = t4aNow,
                    T4achStatusTime = t4aNow,
                    T4aConsentHistoryAdddate = t4aNow,
                    T4aConsentHistoryAddtime = t4aNow
                },
            };
            dataReaderMock.Setup<Task<Collection<T4aConsentHistory>>>(datareader => datareader.BulkReadRecordAsync<T4aConsentHistory>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(t4aConsentHistoryContracts);
            });
            #endregion

            transactionInvoker.Setup(tio => tio.ExecuteAsync<CreateDocConsentHistoryRequest, CreateDocConsentHistoryResponse>(It.IsAny<CreateDocConsentHistoryRequest>())).Returns(Task.FromResult(this.createDocConsentHistoryResponse));
        }
        #endregion
    }
}