// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class ProgramRequirementsRepositoryTests
    {
        const string _CacheName = "ProgramRequrementsRepository";
        ObjectCache localCache = new MemoryCache(_CacheName);
        private ProgramRequirements tpr;  // From test repository, for comparison
        private ProgramRequirements pr;
        private ProgramRequirementsRepository programRequirementsRepo;


        private ProgramRequirementsRepository BuildValidPRR(ProgramRequirements pr)
        {
            string prog = pr.ProgramCode;
            string cat = pr.CatalogCode;
            string filekey = prog + "*" + cat;

            var transFactoryMock = new Mock<IColleagueTransactionFactory>();

            var loggerMock = new Mock<ILogger>();

            // Set up data accessor for mocking 
            var dataAccessorMock = new Mock<IColleagueDataReader>();
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

            // Cache mocking
            var cacheProviderMock = new Mock<ICacheProvider>();
            //cacheProviderMock.Setup(provider => provider.GetCache(_CacheName)).Returns(localCache);
            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
            x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
            .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                null,
                new SemaphoreSlim(1, 1)
        )));
            // Mock up response for grade repository
            Collection<Grades> graderesp = new Collection<Grades>();  //BuildGradeResp();  // This won't actually get used.
            dataAccessorMock.Setup<Task<Collection<Grades>>>(grds => grds.BulkReadRecordAsync<Grades>("GRADES", "", true)).Returns(Task.FromResult(graderesp));

            // Build ACAD.PROGRAM.REQMTS response and mock
            AcadProgramReqmts aprResponse = BuildValidAprResponse(pr);
            dataAccessorMock.Setup<Task<AcadProgramReqmts>>(acc => acc.ReadRecordAsync<AcadProgramReqmts>("ACAD.PROGRAM.REQMTS", filekey, true)).Returns(Task.FromResult(aprResponse));

            // Return mocked up repo
            ProgramRequirementsRepository prr = new ProgramRequirementsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new TestRequirementRepository(), new TestGradeRepository(), new TestRuleRepository2());
            return prr;

        }

        private AcadProgramReqmts BuildValidAprResponse(ProgramRequirements pr)
        {
            string filekey = pr.ProgramCode + "*" + pr.CatalogCode;
            AcadProgramReqmts apr = new AcadProgramReqmts();
            apr.AcprAcadCredRules = new List<string>();
            foreach (var r in pr.ActivityEligibilityRules)
            {
                apr.AcprAcadCredRules.Add(r.CreditRule.Id);
            }
            apr.AcprAcadReqmts = new List<string>();
            apr.AcprCred = pr.MinimumCredits;
            apr.AcprInstitutionCred = pr.MinimumInstitutionalCredits;
            apr.AcprInstitutionGpa = pr.MinInstGpa;
            apr.AcprMaxCred = pr.MaximumCredits;
            apr.AcprMinGpa = pr.MinOverallGpa;
            apr.AcprMinGrade = pr.MinGrade.Id;
            apr.AcprOtherGrades = pr.AllowedGrades.Select(g => g.Id).ToList();
            apr.Recordkey = filekey;
            apr.AcprCurriculumTrack = pr.CurriculumTrackCode;
            apr.AcprAddnlCcdPl = "REQ1";
            apr.AcprAddnlMajorPl = "REQ2";
            apr.AcprAddnlMinorPl = "REQ3";
            apr.AcprAddnlSpecializationPl = "REQ4";

            return apr;

        }


        [TestClass]
        public class Get : ProgramRequirementsRepositoryTests
        {

            [TestInitialize]
            public async void Initialize()
            {
                tpr = await new TestProgramRequirementsRepository().GetAsync("EmptyRequirements", "3000");
                programRequirementsRepo = BuildValidPRR(tpr);
                pr = await programRequirementsRepo.GetAsync("EmptyRequirements", "3000");        
            }
            [TestMethod]
            public void GetReturnsValidProgramRequirements()
            {
                Assert.AreEqual(new RequirementRule(new Rule<AcademicCredit>("STCUSERX")).CreditRule, pr.ActivityEligibilityRules.First().CreditRule);
                Assert.AreEqual(null, pr.MaximumCredits);
                Assert.AreEqual("D", pr.MinGrade.Id);
                Assert.AreEqual(40m, pr.MinimumInstitutionalCredits);
                Assert.AreEqual(2m, pr.MinInstGpa);
                Assert.AreEqual(120, pr.MinimumCredits);
                Assert.AreEqual(2.1m, pr.MinOverallGpa);
                Assert.IsTrue(pr.AllowedGrades.Select(g => g.Id).Contains("P"));
                Assert.IsTrue(pr.AllowedGrades.Select(g => g.Id).Contains("AU"));
                Assert.AreEqual("TRACK1", pr.CurriculumTrackCode);
                Assert.AreEqual("REQ1", pr.RequirementToPrintCcdsAfter);
                Assert.AreEqual("REQ2", pr.RequirementToPrintMajorsAfter);
                Assert.AreEqual("REQ3", pr.RequirementToPrintMinorsAfter);
                Assert.AreEqual("REQ4", pr.RequirementToPrintSpecializationsAfter);
            }
        }


        [TestClass]
        public class GetAsync : ProgramRequirementsRepositoryTests
        {

            [TestInitialize]
            public async void Initialize()
            {
                tpr = await new TestProgramRequirementsRepository().GetAsync("EmptyRequirements", "3000");
                programRequirementsRepo = BuildValidPRR(tpr);
                pr = await programRequirementsRepo.GetAsync("EmptyRequirements", "3000");
            }

            [TestMethod]
            public void GetReturnsValidProgramRequirements()
            {
                Assert.AreEqual(new RequirementRule(new Rule<AcademicCredit>("STCUSERX")).CreditRule, pr.ActivityEligibilityRules.First().CreditRule);
                Assert.AreEqual(null, pr.MaximumCredits);
                Assert.AreEqual("D", pr.MinGrade.Id);
                Assert.AreEqual(40m, pr.MinimumInstitutionalCredits);
                Assert.AreEqual(2m, pr.MinInstGpa);
                Assert.AreEqual(120, pr.MinimumCredits);
                Assert.AreEqual(2.1m, pr.MinOverallGpa);
                Assert.IsTrue(pr.AllowedGrades.Select(g => g.Id).Contains("P"));
                Assert.IsTrue(pr.AllowedGrades.Select(g => g.Id).Contains("AU"));
                Assert.AreEqual("TRACK1", pr.CurriculumTrackCode);
            }
          
        }
    }
}
