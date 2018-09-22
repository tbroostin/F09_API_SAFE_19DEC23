// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System.Threading;


namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class PersonRestrictionRepositoryTests
    {
        Mock<IColleagueTransactionFactory> transFactoryMock;
        Mock<ICacheProvider> cacheProviderMock;
        Mock<IColleagueDataReader> dataAccessorMock;
        Mock<ILogger> loggerMock;
        IEnumerable<PersonRestriction> allStudentRestrictions;
        Collection<Base.DataContracts.StudentRestrictions> stuRestResponseData;
        Collection<Base.DataContracts.StudentRestrictions> firstThree;
        Collection<Base.DataContracts.StudentRestrictions> secondOne;
        Collection<Base.DataContracts.StudentRestrictions> sixthOne;
        string personWithThree = "S0001";
        string personWithOne = "S0002";
        string personNoPersonSt = "S0003";
        string personWithInvalid = "S0004";
        PersonRestrictionRepository stuRestRepo;

        [TestInitialize]
        public void Initialize()
        {
            loggerMock = new Mock<ILogger>();
            allStudentRestrictions = new TestPersonRestrictionRepository().Get();
            stuRestResponseData = BuildStudentRestrictionsResponse(allStudentRestrictions);
            IEnumerable<PersonRestriction> three = allStudentRestrictions.Where(r => r.Id == "1" || r.Id == "2" || r.Id == "3").AsEnumerable();
            firstThree = BuildStudentRestrictionsResponse(three);
            IEnumerable<PersonRestriction> second = allStudentRestrictions.Where(r => r.Id == "2").AsEnumerable();
            secondOne = BuildStudentRestrictionsResponse(second);
            IEnumerable<PersonRestriction> sixth = allStudentRestrictions.Where(r => r.Id == "6").AsEnumerable();
            sixthOne = BuildStudentRestrictionsResponse(sixth);

            stuRestRepo = BuildPersonRestrictionRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            transFactoryMock = null;
            dataAccessorMock = null;
            cacheProviderMock = null;
            allStudentRestrictions = null;
            stuRestResponseData = null;
            firstThree = null;
            secondOne = null;
            sixthOne = null;
            stuRestRepo = null;
        }

        [TestMethod]
        public async Task Get_CountValid()
        {
            var stuRests = await stuRestRepo.GetAsync(personWithThree);
            Assert.AreEqual(3, stuRests.Count());
        }

        [TestMethod]
        public async Task Get_StudentId()
        {
            var stuRestrs = await stuRestRepo.GetAsync(personWithOne);
            var stuRestr = stuRestrs.First();
            Assert.AreEqual("S0001", stuRestr.StudentId);
        }

        [TestMethod]
        public async Task Get_RestrictionId()
        {
            var stuRestrs = await stuRestRepo.GetAsync(personWithOne);
            var stuRestr = stuRestrs.First();
            Assert.AreEqual("R0002", stuRestr.RestrictionId);
        }

        [TestMethod]
        public async Task Get_StartDate()
        {
            var stuRestrs = await stuRestRepo.GetAsync(personWithOne);
            var stuRestr = stuRestrs.First();
            DateTime date;
            DateTime.TryParse("12/01/2012", out date);
            Assert.AreEqual(date, stuRestr.StartDate);
        }

        [TestMethod]
        public async Task Get_EndDate()
        {
            var stuRestrs = await stuRestRepo.GetAsync(personWithOne);
            var stuRestr = stuRestrs.First();
            DateTime date;
            DateTime.TryParse("12/03/2012", out date);
            Assert.AreEqual(date, stuRestr.EndDate);
        }

        [TestMethod]
        public async Task Get_Severity()
        {
            var stuRestrs = await stuRestRepo.GetAsync(personWithOne);
            var stuRestr = stuRestrs.First();
            int i;
            Int32.TryParse("3", out i);
            Assert.AreEqual(i, stuRestr.Severity);
        }

        [TestMethod]
        public async Task Get_OfficeUseOnly()
        {
            var stuRestrs = await stuRestRepo.GetAsync(personWithOne);
            var stuRestr = stuRestrs.First();
            Assert.AreEqual(true, stuRestr.OfficeUseOnly);
        }

        [TestMethod]
        public async Task Get_PersonWithNone()
        {
            var stuRestrs = await stuRestRepo.GetAsync(personNoPersonSt);
            Assert.AreEqual(0, stuRestrs.Count());
        }

        [TestMethod]
        public async Task Get_PersonWithInvalidIds_ReturnsNone()
        {
            var stuRestrs = await stuRestRepo.GetAsync(personWithInvalid);
            Assert.AreEqual(0, stuRestrs.Count());
        }

        [TestMethod]
        public async Task Get_MultipleCountValid()
        {
            var multipleStudentIds = new List<string>() { personWithThree, personWithOne };
            var stuRests = await stuRestRepo.GetRestrictionsByStudentIdsAsync(multipleStudentIds.AsEnumerable());
            Assert.AreEqual(4, stuRests.Count());
        }

        [TestMethod]
        public async Task Get_MultipleFromRestrictionIdCountValid()
        {
            var multipleRestrictionIds = new List<string>() { "1", "2", "3" };
            var stuRests = await stuRestRepo.GetRestrictionsByIdsAsync(multipleRestrictionIds.AsEnumerable());
            Assert.AreEqual(3, stuRests.Count());
        }

        private Collection<StudentRestrictions> BuildStudentRestrictionsResponse(IEnumerable<PersonRestriction> stuRestrictions) 
        {
            Collection<StudentRestrictions> repoStuRestrictions = new Collection<StudentRestrictions>();
            foreach (var stuRestr in stuRestrictions)
            {
                var stuRest = new StudentRestrictions();
                stuRest.Recordkey = stuRestr.Id;
                stuRest.StrEndDate = stuRestr.EndDate;
                stuRest.StrPrtlDisplayFlag = (stuRestr.OfficeUseOnly ? "N" : "Y");
                stuRest.StrRestriction = stuRestr.RestrictionId;
                stuRest.StrSeverity = stuRestr.Severity;
                stuRest.StrStartDate = stuRestr.StartDate;
                stuRest.StrStudent = stuRestr.StudentId;
                repoStuRestrictions.Add(stuRest);
            }
            return repoStuRestrictions;
        }

        private PersonRestrictionRepository BuildPersonRestrictionRepository()
        {
            transFactoryMock = new Mock<IColleagueTransactionFactory>();
            cacheProviderMock = new Mock<ICacheProvider>();
            dataAccessorMock = new Mock<IColleagueDataReader>();
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

            // Needed to for GetOrAddToCacheAsync 
            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                null,
                new SemaphoreSlim(1, 1)
            )));

            // Mock reads of PersonSt and associated reads of the restrictions

            // Person with 3 restrictions
            PersonSt personWithThreePersonSt = new PersonSt() { PstRestrictions = new List<string>() { "1", "2", "3" } };
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<PersonSt>(personWithThree, true)).ReturnsAsync(personWithThreePersonSt);
            var temp1 = new List<string>() { "1", "2", "3" }.ToArray();
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<StudentRestrictions>("STUDENT.RESTRICTIONS", temp1, true)).ReturnsAsync(firstThree);

            // Person with 1 restriction
            PersonSt personWithOnePersonSt = new PersonSt() { PstRestrictions = new List<string>() { "2" } };
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<PersonSt>(personWithOne, true)).ReturnsAsync(personWithOnePersonSt);
            var temp2 = new List<string>() { "2" }.ToArray();
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<StudentRestrictions>("STUDENT.RESTRICTIONS", temp2, true)).ReturnsAsync(secondOne);

            // Person with invalid Ids in PersonSt
            PersonSt personStWithInvalid = new PersonSt() { PstRestrictions = new List<string>() { "999" } };
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<PersonSt>(personWithInvalid, true)).ReturnsAsync(personStWithInvalid);
            var temp3 = new List<string>() { "999" }.ToArray();
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<StudentRestrictions>("STUDENT.RESTRICTIONS", temp3, true)).ReturnsAsync(new Collection<StudentRestrictions>());

            // Person with no PersonSt
            PersonSt personWithNoPersonSt = null;
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<PersonSt>(personNoPersonSt, true)).ReturnsAsync(personWithNoPersonSt);

            // Multiple persons with multiple restrictions
            Collection<PersonSt> personsMultiple = new Collection<PersonSt>();
            personsMultiple.Add(personWithThreePersonSt);
            personsMultiple.Add(personWithOnePersonSt);
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<PersonSt>(It.IsAny<string[]>(), true)).ReturnsAsync(personsMultiple);
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<StudentRestrictions>("STUDENT.RESTRICTIONS", temp1, true)).ReturnsAsync(firstThree);

            stuRestRepo = new PersonRestrictionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            return stuRestRepo;
        }

    }
}
