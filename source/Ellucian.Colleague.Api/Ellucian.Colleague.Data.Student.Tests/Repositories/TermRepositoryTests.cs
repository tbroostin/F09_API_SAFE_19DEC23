// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System.Threading;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class TermRepositoryTests
    {
        TermRepository termRepo;
        IEnumerable<Term> allTerms;
        Mock<IColleagueDataReader> dataAccessorMock;

        [TestInitialize]
        public  void Initialize()
        {
            dataAccessorMock = new Mock<IColleagueDataReader>();
            termRepo = BuildValidTermRepository();
            allTerms = new TestTermRepository().Get();
        }

        [TestCleanup]
        public void Cleanup()
        {
            termRepo = null;
        }

        [TestMethod]
        public async Task TermRepository_Get_Single_TestTerms()
        {
            // Instead of hard coding to check one term - loop through all terms to test.
            foreach (var t in allTerms)
            {
                Term term = await termRepo.GetAsync(t.Code);
                Assert.AreEqual(t.ReportingYear, term.ReportingYear);
                Assert.AreEqual(t.Description, term.Description);
                Assert.AreEqual(t.DefaultOnPlan, term.DefaultOnPlan);
                Assert.AreEqual(t.Sequence, term.Sequence);
                Assert.AreEqual(t.DefaultOnPlan, term.DefaultOnPlan);
                Assert.AreEqual(t.RegistrationPriorityRequired, term.RegistrationPriorityRequired);
                foreach (var sc in t.SessionCycles)
                {
                    Assert.IsTrue(term.SessionCycles.Contains(sc));
                }
                foreach (var yc in t.YearlyCycles)
                {
                    Assert.IsTrue(term.YearlyCycles.Contains(yc));
                }
                Assert.AreEqual(t.SessionId, term.SessionId);
                Assert.AreEqual(t.Category, term.Category);
            }
        }

        [TestMethod]
        public async Task TermRepository_Get_Single_ReturnSingleTerms()
        {
            Term term = await termRepo.GetAsync("2000/S1");
            Assert.AreEqual(term.ReportingYear.ToString(), "2000");
        }

        [TestMethod]
        public async Task TermRepository_Get_Single_TermName()
        {
            Term term = await termRepo.GetAsync("2000/S1");
            Assert.AreEqual("Summer Term 1", term.Description);
        }

        [TestMethod]
        public async Task TermRepository_Get_Single_DefaultOnPlan()
        {
            Term term = await termRepo.GetAsync("2000/S1");
            Assert.AreEqual(false, term.DefaultOnPlan);
        }

        [TestMethod]
        public async Task TermRepository_Get_Single_DefaultOnPlan2()
        {
            Term term = await termRepo.GetAsync("2012/SP");
            Assert.AreEqual(true, term.DefaultOnPlan);
        }


        [TestMethod]
        public async Task TermRepository_Get_All_ReturnAllTerms()
        {
            var terms = await termRepo.GetAsync();
            Assert.AreEqual(allTerms.Count(), terms.Count());
        }

        [TestMethod]
        public async Task TermRepository_Get_Some_TermsReturnsTerms()
        {
            List<string> ids = new List<string>();
            ids.Add("2000/S1");
            ids.Add("2000/S2");
            var terms = await termRepo.GetAsync(ids);
            Assert.IsTrue(terms.Count() == 2);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TermRepository_Get_Some_ThrowsException()
        {
            IEnumerable<Term> terms = await termRepo.GetAsync(new List<string>());
        }

        [TestMethod]
        public async Task TermRepository_Get_Some_SomeTermsNotFound()
        {
            List<string> termList = new List<string>() { "2012/FA", "JUNK" }; 
            IEnumerable<Term> terms = await termRepo.GetAsync(termList);
            Assert.AreEqual(1, terms.Count());
            Assert.AreEqual("2012/FA", terms.ElementAt(0).Code);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task TermRepository_Get_Single_TermNotFoundReturnsException()
        {
            Term term = await termRepo.GetAsync("Junk");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TermRepository_Get_Empty_TermReturnsException()
        {
            Term term = await termRepo.GetAsync("");
        }

        [TestMethod]
        public async Task TermRepository_GetRegistrationTerms_ReturnsListOfTerms()
        {
            IEnumerable<Term> registrationTerms = await termRepo.GetRegistrationTermsAsync();
            Assert.AreEqual(2, registrationTerms.Count());
        }

        [TestMethod]
        public async Task TermRepository_NoRegistrationTerms_ReturnsEmptyList()
        {
            // Mock transaction factory and everything because otherwise the returned value conflicts with the other test.
            var transFactoryMock = new Mock<IColleagueTransactionFactory>();
            var loggerMock = new Mock<ILogger>();
            //var localCacheMock = new Mock<ObjectCache>();
            var cacheProviderMock = new Mock<ICacheProvider>();
            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


            //cacheProviderMock.Setup(provider => provider.GetCache(It.IsAny<string>())).Returns(localCacheMock.Object);
            // Set up transaction manager for mocking 
            var mockManager = new Mock<IColleagueTransactionInvoker>();
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
            // set up response with a null list of term Codes
            var registrationTermsResponse = new GetRegistrationTermsResponse() { AlRegistrationTerms = new List<string>() };
            mockManager.Setup(mgr => mgr.ExecuteAsync<GetRegistrationTermsRequest, GetRegistrationTermsResponse>(It.IsAny<GetRegistrationTermsRequest>())).ReturnsAsync(registrationTermsResponse);
            // Construct term repository
            termRepo = new TermRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            // Attempt to get registration terms, exception will occur if none found
            IEnumerable<Term> registrationTerms = await termRepo.GetRegistrationTermsAsync();
            Assert.IsTrue(registrationTerms.Count() == 0);
        }

        [TestMethod]
        public async Task TermRepository_GetRegistrationTerms_WritesToCache()
        {
            // Mock everything to not conflict with other test setups. Data accessor not needed for this test
            var transFactoryMock = new Mock<IColleagueTransactionFactory>();
            var loggerMock = new Mock<ILogger>();
            var cacheProviderMock = new Mock<ICacheProvider>();
            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            //var localCacheMock = new Mock<ObjectCache>();
            var mockManager = new Mock<IColleagueTransactionInvoker>();

            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
            //cacheProviderMock.Setup(provider => provider.GetCache(It.IsAny<string>())).Returns(localCacheMock.Object);
            // Construct the term repo
            termRepo = new TermRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            // Return false when checking whether "RegistrationTerms" cached item already exists
            string cacheKey = termRepo.BuildFullCacheKey("RegistrationTerms");
            cacheProviderMock.Setup(x => x.Contains(cacheKey, null)).Returns(false);
            cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(null);

            // Set up response from GetRegistrationTerms transaction when not found in cache
            var inputIds = new List<string>() { "2000/S1", "2000/S2" };
            var registrationTermsResponse = new GetRegistrationTermsResponse() { AlRegistrationTerms = inputIds };
            mockManager.Setup(mgr => mgr.ExecuteAsync<GetRegistrationTermsRequest, GetRegistrationTermsResponse>(It.IsAny<GetRegistrationTermsRequest>())).ReturnsAsync(registrationTermsResponse);

            // But after RegistrationTerms gotten, set up mocking so we can verify the list of term codes was written to the cache
//            cacheProviderMock.Setup(x => x.Add(cacheKey, It.IsAny<IEnumerable<Course>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();
            cacheProviderMock.Setup(x => x.AddAndUnlockSemaphore(cacheKey, It.IsAny<List<string>>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

            // Mock "AllTerms" retrieved from cache, used to build the list of registration terms
            cacheProviderMock.Setup(x => x.Contains(termRepo.BuildFullCacheKey("AllTerms"), null)).Returns(true);
            IEnumerable<Term> terms = new TestTermRepository().Get();
            cacheProviderMock.Setup(x => x.Get(termRepo.BuildFullCacheKey("AllTerms"), null)).Returns(terms);

            // Verify that registration terms were returned, which means it was read from the database
            var registrationTerms = await termRepo.GetRegistrationTermsAsync();
            Assert.IsTrue(registrationTerms.Count() == 2);

            // Verify that the "RegistrationTerms" item was added to the cache after it was read from the repository
//            cacheProviderMock.Verify(m => m.Add(cacheKey, inputIds, It.IsAny<CacheItemPolicy>(), null));
            cacheProviderMock.Verify(m => m.AddAndUnlockSemaphore(cacheKey, It.IsAny<List<string>>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null));
        }

        [TestMethod]
        public async Task TermRepository_Get_All_GetsCachedRegistrationTerms()
        {
            // Mock everything to not conflict with other test setups
            var transFactoryMock = new Mock<IColleagueTransactionFactory>();
            var loggerMock = new Mock<ILogger>();
            var cacheProviderMock = new Mock<ICacheProvider>();
            //var localCacheMock = new Mock<ObjectCache>();
            // Set up only cache mocking. Won't be accessing data accessor or transaction factory, so don't need to mock those.
            //cacheProviderMock.Setup(provider => provider.GetCache(It.IsAny<string>())).Returns(localCacheMock.Object);
            // Construct term repo
            termRepo = new TermRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            // Return true when checking whether "RegistrationTerms" cached item already exists
            string cacheKey = termRepo.BuildFullCacheKey("RegistrationTerms");
            cacheProviderMock.Setup(x => x.Contains(cacheKey, null)).Returns(true);
            // Mock a return of the list of term codes from cache. Verifiable() enables us to verify Get from cache occurred.
            var inputIds = new List<string>() { "2000/S1", "2000/S2" };
            cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(inputIds).Verifiable();

            // Mock "AllTerms" retrieved from cache, linq used to build the list of registration terms from the codes
            cacheProviderMock.Setup(x => x.Contains(termRepo.BuildFullCacheKey("AllTerms"), null)).Returns(true);
            IEnumerable<Term> terms = new TestTermRepository().Get();
            cacheProviderMock.Setup(x => x.Get(termRepo.BuildFullCacheKey("AllTerms"), null)).Returns(terms);

            // Verify that registration terms were returned, which means they came from cache.
            var registrationTerms = await termRepo.GetRegistrationTermsAsync();
            Assert.IsTrue(registrationTerms.Count() == 2);

            // Verify that Get was called to get the registration terms from cache
            cacheProviderMock.Verify(m => m.Get(cacheKey, null));
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task TermRepository_GetAllTerms_BulkReadFailure()
        {
            dataAccessorMock.Setup<Task<Collection<Ellucian.Colleague.Data.Student.DataContracts.Terms>>>(acc => acc.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Terms>("TERMS", "", true)).Throws(new Exception());
            var terms = await termRepo.GetAsync();
        }

        [TestMethod]
        public async Task TermRepository_Get_All_AllTerms_WithBadData()
        {
            var allTermContracts = BuildTermsResponse(allTerms);
            // Add in a bad term contract - one with no start date.
            allTermContracts.Add(new Ellucian.Colleague.Data.Student.DataContracts.Terms() { Recordkey = "2016/XX", TermDesc = "Term with no Start Date", TermReportingYear = 2016, TermSequenceNo = 1 });

            // Set up term response for "all" term requests including the bad term
            dataAccessorMock.Setup<Collection<Ellucian.Colleague.Data.Student.DataContracts.Terms>>(acc => acc.BulkReadRecord<Ellucian.Colleague.Data.Student.DataContracts.Terms>("TERMS", "", true)).Returns(allTermContracts);

            var terms = await termRepo.GetAsync();
            Assert.AreEqual(allTermContracts.Count() - 1, terms.Count());
            Assert.AreEqual(0, terms.Where(t => t.Code == "2016/XX").Count());
        }

        private TermRepository BuildValidTermRepository()
        {
            var transFactoryMock = new Mock<IColleagueTransactionFactory>();

            var loggerMock = new Mock<ILogger>();

            // Cache mocking
            var cacheProviderMock = new Mock<ICacheProvider>();
            var localCacheMock = new Mock<ObjectCache>();
            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            // Set up data accessor for mocking 
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

            // Set up transaction manager for mocking 
            var mockManager = new Mock<IColleagueTransactionInvoker>();
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);

            // Setup up Sessions Response
            Collection<Sessions> sessionResponseData = new Collection<Sessions>();
            Sessions sessionData = new Sessions()
            {
                Recordkey = "YL",
                SessDesc = "Year Long Session",
                SessIntgCategory = "year"
            };
            sessionResponseData.Add(sessionData);
            sessionData = new Sessions()
            {
                Recordkey = "OE",
                SessDesc = "Open Entry Session",
                SessIntgCategory = "term"
            };
            sessionResponseData.Add(sessionData);
            sessionData = new Sessions()
            {
                Recordkey = "WI",
                SessDesc = "Winter Session",
                SessIntgCategory = "subterm"
            };
            sessionResponseData.Add(sessionData);
            dataAccessorMock.Setup<Task<Collection<Sessions>>>(acc => acc.BulkReadRecordAsync<Sessions>("SESSIONS", "", true)).ReturnsAsync(sessionResponseData);

            // Set up Term Response
            IEnumerable<Term> terms = new TestTermRepository().Get();
            var termResponseData = BuildTermsResponse(terms);

            // Set up term response for "all" term requests
            dataAccessorMock.Setup<Task<Collection<Ellucian.Colleague.Data.Student.DataContracts.Terms>>>(acc => acc.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Terms>("TERMS", "", true)).ReturnsAsync(termResponseData);
            
            // Set up term response for a list of term Ids request
            var termsCollection = new Collection<Ellucian.Colleague.Data.Student.DataContracts.Terms>();
            termsCollection.Add(termResponseData.ElementAt(0));
            termsCollection.Add(termResponseData.ElementAt(1));
            string[] inputIds = new string[] { "2000/S1", "2000/S2"};
            dataAccessorMock.Setup<Task<Collection<Ellucian.Colleague.Data.Student.DataContracts.Terms>>>(acc => acc.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Terms>("TERMS", inputIds, true)).ReturnsAsync(termsCollection);

            // set up GetRegistrationTermsRequest & Response. Response from above will complete the mocking for this test case
            var registrationTermsResponse = new GetRegistrationTermsResponse() { AlRegistrationTerms = inputIds.ToList() };
            mockManager.Setup(mgr => mgr.ExecuteAsync<GetRegistrationTermsRequest, GetRegistrationTermsResponse>(It.IsAny<GetRegistrationTermsRequest>())).ReturnsAsync(registrationTermsResponse);
            
            // Construct term repository
            termRepo = new TermRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            return termRepo;
        }

        private Collection<Ellucian.Colleague.Data.Student.DataContracts.Terms> BuildTermsResponse(IEnumerable<Term> terms)
        {
            Collection<Ellucian.Colleague.Data.Student.DataContracts.Terms> repoTerms = new Collection<Ellucian.Colleague.Data.Student.DataContracts.Terms>();
            foreach (var term in terms)
            {
                var termData = new Ellucian.Colleague.Data.Student.DataContracts.Terms();
                termData.Recordkey = term.Code.ToString();
                termData.TermStartDate = term.StartDate;
                termData.TermEndDate = term.EndDate;
                termData.TermReportingYear = term.ReportingYear;
                termData.TermDesc = term.Description;
                termData.TermSequenceNo = term.Sequence/*.ToString()*/;
                termData.TermAcadLevels = term.AcademicLevels.ToList();
                termData.TermSessionCycles = term.SessionCycles.ToList();
                termData.TermYearlyCycles = term.YearlyCycles.ToList();
                if (term.DefaultOnPlan)
                {
                    termData.TermDefaultOnPlan = "Y"; 
                }
                if (term.ForPlanning)
                {
                    termData.TermDegreePlanning = "Y";
                }
                termData.TermReportingTerm = term.ReportingTerm;
                if (term.RegistrationPriorityRequired)
                {
                    termData.TermRegPriorityFlag = "Y";
                }
                termData.TermSession = term.SessionId;
                
                repoTerms.Add(termData);
            }
            return repoTerms;
        }

        [TestMethod]
        public void GetsAcademicPeriodAsync()
        {
            var academicPeriods = termRepo.GetAcademicPeriods(allTerms);
            for (int i = 0; i < academicPeriods.Count(); i++)
            {
                var period = academicPeriods.Where(ap => ap.Guid == allTerms.ElementAt(i).RecordGuid).FirstOrDefault();
                Assert.AreEqual(allTerms.ElementAt(i).Code, period.Code);
                Assert.AreEqual(allTerms.ElementAt(i).Description, period.Description);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void AcademicPeriodRequiredTest()
        {
            var newTerms = new List<Term>();
            var term1 = new Term(Guid.NewGuid().ToString(), "2000/FA", "Fall Term", DateTime.Parse("2000-08-28"), DateTime.Parse("2000-12-20"), 2000, 1, true, true, "2000RSU", true)
            {
                Category = "subterm",
                SessionId = "FA"
            };
            newTerms.Add(term1);
            termRepo.GetAcademicPeriods(newTerms);
        }
    }
}
