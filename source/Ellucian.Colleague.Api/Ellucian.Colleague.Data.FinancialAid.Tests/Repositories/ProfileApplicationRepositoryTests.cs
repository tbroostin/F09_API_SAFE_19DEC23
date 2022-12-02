/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    [TestClass]
    public class ProfileApplicationRepositoryTests : BaseRepositorySetup
    {
        public TestFinancialAidOfficeRepository financialAidOfficeRepository;
        public TestStudentAwardYearRepository studentAwardYearRepository;

        public TestProfileApplicationRepository expectedRepository;
        public ProfileApplicationRepository actualRepository;

        public void ProfileApplicationRepositoyTestsInitialize()
        {
            MockInitialize();
            financialAidOfficeRepository = new TestFinancialAidOfficeRepository();
            studentAwardYearRepository = new TestStudentAwardYearRepository();
            expectedRepository = new TestProfileApplicationRepository();
        }

        [TestClass]
        public class GetProfileApplicationsTests : ProfileApplicationRepositoryTests
        {
            public IEnumerable<FinancialAidOffice> financialAidOffices { get { return financialAidOfficeRepository.GetFinancialAidOffices(); } }
            public IEnumerable<StudentAwardYear> studentAwardYears { get { return studentAwardYearRepository.GetStudentAwardYears(studentId, new CurrentOfficeService(financialAidOffices)); } }

            public List<ProfileApplication> expectedProfileApplications { get { return expectedRepository.GetProfileApplicationsAsync(studentId, studentAwardYears).Result.ToList(); } }
            public IEnumerable<ProfileApplication> actualProfileApplications;
            public string studentId;

            [TestInitialize]
            public void Initialize()
            {
                ProfileApplicationRepositoyTestsInitialize();

                studentId = TestProfileApplicationRepository.studentId;

                actualRepository = BuildRepository();
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                actualProfileApplications = await actualRepository.GetProfileApplicationsAsync(studentId, studentAwardYears);
                CollectionAssert.AreEqual(expectedProfileApplications, actualProfileApplications.ToList());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequiredTest()
            {
                await actualRepository.GetProfileApplicationsAsync(string.Empty, studentAwardYears);
            }

            [TestMethod]
            public async Task NullStudentAwardYearsReturnsEmptyListTest()
            {
                Assert.AreEqual(0, (await actualRepository.GetProfileApplicationsAsync(studentId, null)).Count());
            }

            [TestMethod]
            public async Task EmptyStudentAwardYearsReturnsEmptyListTest()
            {
                studentAwardYearRepository.FaStudentData.FaCsYears = new List<string>();
                studentAwardYearRepository.FaStudentData.FaSaYears = new List<string>();
                studentAwardYearRepository.FaStudentData.FaYsYears = new List<string>();
                actualProfileApplications = await actualRepository.GetProfileApplicationsAsync(studentId, studentAwardYears);

                CollectionAssert.AreEqual(expectedProfileApplications, actualProfileApplications.ToList());
            }

            [TestMethod]
            public async Task NoCsAcyrForStudentAwardYear_DoesNotCreateProfileApplicationsForThatYearTest()
            {
                studentAwardYearRepository.FaStudentData.FaCsYears.Add("foobar");
                studentAwardYearRepository.CsStudentData.Add(new TestStudentAwardYearRepository.CsStudent()
                    {
                        AwardYear = "foobar",
                        LocationId = "MC"
                    });
                actualProfileApplications = await actualRepository.GetProfileApplicationsAsync(studentId, studentAwardYears);

                Assert.IsNotNull(studentAwardYears.FirstOrDefault(y => y.Code == "foobar"));
                Assert.IsNull(actualProfileApplications.FirstOrDefault(p => p.AwardYear == "foobar"));
            }

            [TestMethod]
            public async Task NoCsIsirTransIdsExistTest()
            {
                expectedRepository.csStudentData.ForEach(cs => cs.isirRecordIds = new List<string>());
                actualProfileApplications = await actualRepository.GetProfileApplicationsAsync(studentId, studentAwardYears);

                Assert.AreEqual(0, actualProfileApplications.Count());
                loggerMock.Verify(l => l.Debug(string.Format("No CsIsirTransIds exist in any award year for student {0}", studentId)));
            }

            [TestMethod]
            public async Task DuplicateIsirTransIdsDoesNotCreateDuplicateObjectTest()
            {
                var originalProfiles = new List<ProfileApplication>();
                originalProfiles.AddRange(await actualRepository.GetProfileApplicationsAsync(studentId, studentAwardYears));

                expectedRepository.csStudentData.First().isirRecordIds.Concat(
                    expectedRepository.csStudentData.First().isirRecordIds);
                actualProfileApplications = await actualRepository.GetProfileApplicationsAsync(studentId, studentAwardYears);

                CollectionAssert.AreEqual(originalProfiles, actualProfileApplications.ToList());
            }

            [TestMethod]
            public async Task NoIsirFafsaRecordsExistTest()
            {
                expectedRepository.isirFafsaData = new List<TestProfileApplicationRepository.IsirFafsaRecord>();
                actualProfileApplications = await actualRepository.GetProfileApplicationsAsync(studentId, studentAwardYears);

                Assert.AreEqual(0, actualProfileApplications.Count());
                loggerMock.Verify(l => l.Debug(string.Format("Record ids in CS.ISIR.TRANS.IDS for all award years do not exist in ISIR.FASFA for student {0}", studentId)));
            }

            [TestMethod]
            public async Task NonProfileIsirTypesAreNotCreatedTest()
            {
                expectedRepository.isirFafsaData.ForEach(i => i.isirType = "ISIR");
                actualProfileApplications = await actualRepository.GetProfileApplicationsAsync(studentId, studentAwardYears);
                Assert.AreEqual(0, actualProfileApplications.Count());
            }

            [TestMethod]
            public async Task BadYearInProfileRecordDoesNotCreateObjectTest()
            {
                expectedRepository.isirFafsaData.ForEach(p => p.awardYear = "foobar");
                actualProfileApplications = await actualRepository.GetProfileApplicationsAsync(studentId, studentAwardYears);

                Assert.AreEqual(0, actualProfileApplications.Count());

                loggerMock.Verify(l => l.Debug(
                    string.Format("Error getting CsAcyr record for student {0}, awardYear {1}. Possible data corruption between CsAcyr and ProfileRecord Id {2}", studentId, It.IsAny<string>(), It.IsAny<string>())));
            }

            [TestMethod]
            public async Task BadStudentIdInProfileRecordDoesNotCreateObjectTest()
            {
                expectedRepository.isirFafsaData.ForEach(p => p.studentId = "foobar");
                actualProfileApplications = await actualRepository.GetProfileApplicationsAsync(studentId, studentAwardYears);

                Assert.AreEqual(0, actualProfileApplications.Count());
                loggerMock.Verify(l => l.Debug(
                    string.Format("Error getting CsAcyr record for student {0}, awardYear {1}. Possible data corruption between CsAcyr and ProfileRecord Id {2}", studentId, It.IsAny<string>(), It.IsAny<string>())));
            }

            [TestMethod]
            public async Task IsFederallyFlaggedTest()
            {
                var fedIds = expectedRepository.csStudentData.Select(cs => cs.federalIsirId);
                actualProfileApplications = await actualRepository.GetProfileApplicationsAsync(studentId, studentAwardYears);
                var fedProfiles = actualProfileApplications.Where(p => p.IsFederallyFlagged);

                Assert.IsTrue(fedProfiles.All(p => fedIds.Contains(p.Id)));
            }

            [TestMethod]
            public async Task IsInsitutionallyFlaggedTest()
            {
                var instIds = expectedRepository.csStudentData.Select(cs => cs.instiutionIsirId);
                actualProfileApplications = await actualRepository.GetProfileApplicationsAsync(studentId, studentAwardYears);
                var instProfiles = actualProfileApplications.Where(p => p.IsInstitutionallyFlagged);

                Assert.AreEqual(instIds.Count(), instProfiles.Count());
                Assert.IsTrue(instProfiles.All(p => instIds.Contains(p.Id)));
            }

            [TestMethod]
            public async Task InstiutionalFamilyContributionHasValueTest()
            {
                var csRecord = expectedRepository.csStudentData.First();
                csRecord.instiutionIsirId = "fff";
                csRecord.isirRecordIds.Add("fff");
                csRecord.institutionalFamilyContribution = 4567;
                expectedRepository.isirFafsaData.Add(new TestProfileApplicationRepository.IsirFafsaRecord()
                    {
                        id = "fff",
                        awardYear = csRecord.awardYear,
                        studentId = studentId,
                        isirType = "PROF"
                    });
                actualProfileApplications = await actualRepository.GetProfileApplicationsAsync(studentId, studentAwardYears);
                var actualProfile = actualProfileApplications.First(p => p.Id == "fff");

                Assert.IsTrue(actualProfile.IsInstitutionallyFlagged);
                Assert.AreEqual(4567, actualProfile.InstitutionalFamilyContribution);
            }

            [TestMethod]
            public async Task InstitutionalFamilyContributionHasNoValueTest()
            {
                expectedRepository.csStudentData.ForEach(cs => cs.instiutionIsirId = string.Empty);
                actualProfileApplications = await actualRepository.GetProfileApplicationsAsync(studentId, studentAwardYears);

                Assert.IsTrue(actualProfileApplications.All(p => !p.IsInstitutionallyFlagged));
                Assert.IsTrue(actualProfileApplications.All(p => !p.InstitutionalFamilyContribution.HasValue));
            }

            [TestMethod]
            public async Task NoFederallyFlaggedProfiles_NoFamilyConstributionTest()
            {
                expectedRepository.csStudentData.ForEach(cs => cs.federalIsirId = string.Empty);
                actualProfileApplications = await actualRepository.GetProfileApplicationsAsync(studentId, studentAwardYears);
                Assert.IsTrue(actualProfileApplications.All(p => !p.IsFederallyFlagged && !p.FamilyContribution.HasValue));
            }

            [TestMethod]
            public async Task EmptyCsFc_NoFamilyContributionTest()
            {
                expectedRepository.csStudentData.ForEach(cs => cs.federalFamilyContribution = null);
                actualProfileApplications = await actualRepository.GetProfileApplicationsAsync(studentId, studentAwardYears);

                Assert.IsTrue(actualProfileApplications.Any(p => p.IsFederallyFlagged));
                Assert.IsTrue(actualProfileApplications.All(p => !p.FamilyContribution.HasValue));
            }

            [TestMethod]
            public async Task UnableToParseCsFc_NoFamilyContributionTest()
            {
                expectedRepository.csStudentData.ForEach(cs => cs.federalFamilyContribution = "foobar");
                actualProfileApplications = await actualRepository.GetProfileApplicationsAsync(studentId, studentAwardYears);

                Assert.IsTrue(actualProfileApplications.Any(p => p.IsFederallyFlagged));
                Assert.IsTrue(actualProfileApplications.All(p => !p.FamilyContribution.HasValue));

                loggerMock.Verify(l => l.Debug(string.Format("Unable to parse CsFc - {0} - for studentId {1}, awardYear {2}", It.IsAny<string>(), studentId, It.IsAny<string>())));
            }


            private ProfileApplicationRepository BuildRepository()
            {
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<CsAcyr>(It.IsAny<string>(), It.IsAny<string>(), true))
                    .Returns<string, string, bool>((acyrFile, id, b) =>
                    {
                        var awardYear = acyrFile.Split('.')[1];
                        var csRecord = expectedRepository.csStudentData.FirstOrDefault(cs => cs.studentId == id && cs.awardYear == awardYear);
                        if (csRecord == null) return Task.FromResult((CsAcyr)null);
                        return Task.FromResult(new CsAcyr()
                        {
                            Recordkey = csRecord.studentId,
                            CsIsirTransIds = csRecord.isirRecordIds,
                            CsFedIsirId = csRecord.federalIsirId,
                            CsFc = csRecord.federalFamilyContribution,
                            CsInstIsirId = csRecord.instiutionIsirId,
                            CsInstFc = csRecord.institutionalFamilyContribution,
                        });
                    });

                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<IsirFafsa>(It.IsAny<string[]>(), true))
                    .Returns<string[], bool>((ids, b) => Task.FromResult(
                        new Collection<IsirFafsa>(
                            expectedRepository.isirFafsaData
                            .Where(i => ids.Contains(i.id))
                            .Select(i =>
                                new IsirFafsa()
                                {
                                    Recordkey = i.id,
                                    IfafImportYear = i.awardYear,
                                    IfafStudentId = i.studentId,
                                    IfafIsirType = i.isirType
                                }).ToList())
                    ));

                return new ProfileApplicationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }
        }
    }
}
