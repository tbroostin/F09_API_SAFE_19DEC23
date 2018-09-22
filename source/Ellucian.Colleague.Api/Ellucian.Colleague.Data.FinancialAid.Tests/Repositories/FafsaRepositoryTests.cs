/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    [TestClass]
    public class FafsaRepositoryTests : BaseRepositorySetup
    {
        public TestFafsaRepository expectedRepository;
        public FafsaRepository actualRepository;

        public int csAcyrBulkRecordReadCount;
        public int isirFafsaBulkRecordReadCount;
        public int isirResultBulkRecordReadCount;

        [TestClass]
        public class GetFafsasByStudentIdTests : FafsaRepositoryTests
        {
            public List<Fafsa> expectedFafsas
            {
                get
                {
                    return expectedRepository.GetFafsaByStudentIdsAsync(inputStudentIds, inputAwardYearCode).Result.ToList();
                }
            }


            public IEnumerable<Fafsa> actualFafsas;
            
            public List<string> inputStudentIds;
            public string inputAwardYearCode;

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();
                inputStudentIds = new List<string>() { "0003914", "0004791" };
                inputAwardYearCode = "2014";
                expectedRepository = new TestFafsaRepository();
                actualRepository = BuildRepository();
            }

            [TestMethod]
            public async Task ActualFafsaRecordsAreEqualExpectedTest()
            {
                actualFafsas = await actualRepository.GetFafsaByStudentIdsAsync(inputStudentIds, inputAwardYearCode);
                CollectionAssert.AreEqual(expectedFafsas, actualFafsas.ToList());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullStudentIdsThrowsExceptionTest()
            {
                await actualRepository.GetFafsaByStudentIdsAsync(null, inputAwardYearCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task EmptyStudentIdsListThrowsExceptionTest()
            {
                await actualRepository.GetFafsaByStudentIdsAsync(new List<string>(), inputAwardYearCode);
            }

            /// <summary>
            /// Tests if an argumentNullException is thrown when the award year parameter
            /// passed is null
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullAwardYearThrowsExceptionTest()
            {
                await actualRepository.GetFafsaByStudentIdsAsync(inputStudentIds, null);
            }

            /// <summary>
            /// Tests if an argumentNullException is thrown when the award year parameter
            /// passed is an empty string
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task EmptyStringAwardYearThrowsExceptionTest()
            {
                await actualRepository.GetFafsaByStudentIdsAsync(inputStudentIds, string.Empty);
            }

            /// <summary>
            /// Tests if an empty fafsa list is returned when no student id in the student ids list
            /// matched any cs record student id
            /// </summary>
            [TestMethod]
            public async Task NoCsRecordMatchedIdReturnsEmptyFafsaListTest()
            {
                inputStudentIds = new List<string> { "student" };
                actualFafsas = await actualRepository.GetFafsaByStudentIdsAsync(inputStudentIds, inputAwardYearCode);

                Assert.IsNotNull(actualFafsas);
                Assert.IsTrue(actualFafsas.Count() == 0);
            }
        }

        [TestClass]
        public class GetFafsasTests : FafsaRepositoryTests
        {
            public List<Fafsa> expectedFafsas
            {
                get
                {
                    return expectedRepository.GetFafsasAsync(inputStudentIds, inputAwardYearCodes).Result.ToList();
                }
            }

            public IEnumerable<Fafsa> actualFafsas;
            
            public List<string> inputStudentIds;

            public List<string> customInputAwardYearCodes;
            public IEnumerable<string> inputAwardYearCodes
            {
                get
                {
                    return expectedRepository.csStudentData.Select(c => c.awardYear).Concat(customInputAwardYearCodes);
                }
            }



            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();
                expectedRepository = new TestFafsaRepository();
                actualRepository = BuildRepository();

                inputStudentIds = new List<string>();
                inputStudentIds.AddRange(TestFafsaRepository.inputStudentIds);
                customInputAwardYearCodes = new List<string>();

                csAcyrBulkRecordReadCount = 0;
                isirFafsaBulkRecordReadCount = 0;
                isirResultBulkRecordReadCount = 0;
            }

            [TestCleanup]
            public void Cleanup()
            {
                dataReaderMock = null;
                loggerMock = null;
                expectedRepository = null;
                actualRepository = null;
                inputStudentIds = null;
                customInputAwardYearCodes = null;

                csAcyrBulkRecordReadCount = 0;
                isirFafsaBulkRecordReadCount = 0;
                isirResultBulkRecordReadCount = 0;
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                actualFafsas = await actualRepository.GetFafsasAsync(inputStudentIds, inputAwardYearCodes);
                CollectionAssert.AreEqual(expectedFafsas, actualFafsas.ToList());
            }

            [TestMethod]
            public async Task NullStudentIdsReturnsEmptyListTest()
            {
                Assert.AreEqual(0, (await actualRepository.GetFafsasAsync(null, inputAwardYearCodes)).Count());
            }

            [TestMethod]
            public async Task EmptyStudentIdsReturnsEmptyListTest()
            {
                Assert.AreEqual(0, (await actualRepository.GetFafsasAsync(new List<string>(), inputAwardYearCodes)).Count());
            }

            [TestMethod]
            public async Task NullAwardYearsReturnsEmptyListTest()
            {
                Assert.AreEqual(0, (await actualRepository.GetFafsasAsync(inputStudentIds, null)).Count());
            }

            [TestMethod]
            public async Task EmptyAwardYearsReturnsEmptyListTest()
            {
                Assert.AreEqual(0, (await actualRepository.GetFafsasAsync(inputStudentIds, new List<string>())).Count());
            }

            [TestMethod]
            public async Task IgnoreDuplicateStudentIdsAndAwardYears_NonDuplicatedRecordsTest()
            {
                customInputAwardYearCodes.Add(expectedRepository.csStudentData.First().awardYear);
                inputStudentIds.Add(inputStudentIds.First());
                actualFafsas = await actualRepository.GetFafsasAsync(inputStudentIds, inputAwardYearCodes);

                var distinctActualFafsas = actualFafsas.Distinct();
                CollectionAssert.AreEqual(actualFafsas.ToList(), actualFafsas.Distinct().ToList());
            }

            //Test that fafsas are created for years with cs.acyr record
            [TestMethod]
            public async Task InputAwardYearWithNoCsAcyrRecordIsIgnored()
            {
                customInputAwardYearCodes.Add("foobar");
                actualFafsas = await actualRepository.GetFafsasAsync(inputStudentIds, inputAwardYearCodes);
                Assert.IsNull(actualFafsas.FirstOrDefault(f => f.AwardYear == "foobar"));

                actualFafsas.ToList().ForEach(actualFafsa =>
                    {
                        Assert.AreNotEqual("foobar", actualFafsa);
                        Assert.IsTrue(inputAwardYearCodes.Contains(actualFafsa.AwardYear));
                    });
            }

            //test that bulkReadSize works (set a callback that updates a counter and verify the counter count)
            [TestMethod]
            public async Task CsAcyrBulkReadSizeTest()
            {
                apiSettings.BulkReadSize = 1;
                await actualRepository.GetFafsasAsync(inputStudentIds, inputAwardYearCodes);

                Assert.AreEqual(expectedRepository.csStudentData.Count(), csAcyrBulkRecordReadCount);
            }

            //test null or empty isirfafsaids
            [TestMethod]
            public async Task NoIsirTransIds_ReturnEmptyListTest()
            {
                expectedRepository.csStudentData.ForEach(cs => cs.isirRecordIds = null);
                actualFafsas = await actualRepository.GetFafsasAsync(inputStudentIds, inputAwardYearCodes);

                Assert.AreEqual(0, actualFafsas.Count());
                loggerMock.Verify(l => l.Info("No CsIsirTransIds exist for the given lists of students and awardYears"));
            }

            //test bulkReadSize on ISIR.FAFSA with no corrections
            [TestMethod]
            public async Task IsirFafsaWithNoCorrectionsBulkReadSizeTest()
            {
                apiSettings.BulkReadSize = 1;
                actualRepository = BuildRepository();

                expectedRepository.isirFafsaData.ForEach(isir => isir.correctionId = string.Empty);
                await actualRepository.GetFafsasAsync(inputStudentIds, inputAwardYearCodes);

                Assert.AreEqual(expectedRepository.csStudentData.SelectMany(cs => cs.isirRecordIds).Count(), isirFafsaBulkRecordReadCount);

            }

            //test null or empty isir.fafsa records
            [TestMethod]
            public async Task NoIsirFafsaRecords_ReturnEmptyListTest()
            {
                expectedRepository.isirFafsaData = new List<TestFafsaRepository.IsirFafsaRecord>();
                actualFafsas = await actualRepository.GetFafsasAsync(inputStudentIds, inputAwardYearCodes);

                Assert.AreEqual(0, actualFafsas.Count());
                loggerMock.Verify(l => l.Info("Record Ids in CS.ISIR.TRANS.IDS for students and award years do not exist in ISIR.FAFSA"));
            }

            //test a profile does not exist
            [TestMethod]
            public async Task ProfileApplicationsAreNotReturnedTest()
            {
                expectedRepository.isirFafsaData.Add(new TestFafsaRepository.IsirFafsaRecord()
                {
                    awardYear = expectedRepository.isirFafsaData.First().awardYear,
                    id = "foobar",
                    isirType = "PROF",
                });
                actualFafsas = await actualRepository.GetFafsasAsync(inputStudentIds, inputAwardYearCodes);

                Assert.IsNull(actualFafsas.FirstOrDefault(fafsa => fafsa.Id == "foobar"));
            }

            //test an initial app does not exist
            [TestMethod]
            public async Task InitialApplicationsAreNotReturnedTest()
            {
                expectedRepository.isirFafsaData.Add(new TestFafsaRepository.IsirFafsaRecord()
                {
                    awardYear = expectedRepository.isirFafsaData.First().awardYear,
                    id = "foobar",
                    isirType = "IAPP",
                });
                actualFafsas = await actualRepository.GetFafsasAsync(inputStudentIds, inputAwardYearCodes);

                Assert.IsNull(actualFafsas.FirstOrDefault(fafsa => fafsa.Id == "foobar"));
            }

            //test bulkreadsize on isir.restults
            [TestMethod]
            public async Task IsirResultsBulkReadSizeTest()
            {
                apiSettings.BulkReadSize = 1;
                actualRepository = BuildRepository();

                expectedRepository.isirFafsaData.ForEach(isir => isir.correctionId = string.Empty);
                await actualRepository.GetFafsasAsync(inputStudentIds, inputAwardYearCodes);

                Assert.AreEqual(expectedRepository.csStudentData.SelectMany(cs => cs.isirRecordIds).Count(), isirFafsaBulkRecordReadCount);

            }

            //test null csAcyrRecord from dictionary (set ifaf.import.year to foobar)
            //nothing is federally flagged or institutionally flagged
            [TestMethod]
            public async Task CsAcyrRecordNotFound_NoFederalOrInstitutionalFlags()
            {
                expectedRepository.isirFafsaData.ForEach(isir => isir.awardYear = "foobar");
                actualFafsas = await actualRepository.GetFafsasAsync(inputStudentIds, inputAwardYearCodes);

                Assert.IsFalse(actualFafsas.All(f => f.IsFederallyFlagged));
                Assert.IsFalse(actualFafsas.All(f => f.IsInstitutionallyFlagged));
            }

            //test if no corrections that ids are all from list
            //and expected equals actual
            [TestMethod]
            public async Task NoCorrectionsIsirIdsComeFromCsAcyrIsirTransIdsTest()
            {
                var expectedIsirTransIds = expectedRepository.csStudentData.SelectMany(cs => cs.isirRecordIds);
                expectedRepository.isirFafsaData.ForEach(isir => isir.correctionId = string.Empty);
                actualFafsas = await actualRepository.GetFafsasAsync(inputStudentIds, inputAwardYearCodes);
                var actualIsirIds = actualFafsas.Select(f => f.Id);

                CollectionAssert.IsSubsetOf(expectedIsirTransIds.ToList(), actualIsirIds.ToList());
                CollectionAssert.AreEqual(expectedFafsas, actualFafsas.ToList());
            }

            //test if correction exists that correction id is in actual, but not original
            //and expected equals actual

            [Ignore]
            [TestMethod]
            public async Task CorrectionExists_OriginalIdNotReturnedTest()
            {
                expectedRepository.isirFafsaData.First().correctionId = "foobar";
                expectedRepository.isirFafsaData.Add(new TestFafsaRepository.IsirFafsaRecord()
                    {
                        id = "foobar",
                        awardYear = expectedRepository.isirFafsaData.First().awardYear,
                        isirType = expectedRepository.isirFafsaData.First().isirType,
                        correctedFromId = expectedRepository.isirFafsaData.First().id,
                        studentId = expectedRepository.isirFafsaData.First().studentId
                    });
                actualFafsas = await actualRepository.GetFafsasAsync(inputStudentIds, inputAwardYearCodes);
                CollectionAssert.AreEqual(expectedFafsas, actualFafsas.ToList());
            }

            //test if correction id exists, but no corresponding isir.fafsa record, correction and orig ids not in actual
            [TestMethod]
            public async Task CorrectionIdExistsButNotCorrectionRecord_FafsaNotReturnedTest()
            {
                expectedRepository.isirFafsaData.First().correctionId = "foobar";
                actualFafsas = await actualRepository.GetFafsasAsync(inputStudentIds, inputAwardYearCodes);
                var actualIsirIds = actualFafsas.Select(f => f.Id);

                Assert.IsFalse(actualIsirIds.Contains(expectedRepository.isirFafsaData.First().id));
                Assert.IsFalse(actualIsirIds.Contains(expectedRepository.isirFafsaData.First().correctionId));
            }

            //test pell.eligibilty true if Y, else false
            [TestMethod]
            public async Task PellEligibilityFalseTest()
            {
                expectedRepository.isirResultData.ForEach(isir => isir.isPellEligible = false);
                actualFafsas = await actualRepository.GetFafsasAsync(inputStudentIds, inputAwardYearCodes);
                Assert.IsTrue(actualFafsas.All(fafsa => !fafsa.IsPellEligible));
            }

            [TestMethod]
            public async Task PellEligibilityTrueTest()
            {
                expectedRepository.isirResultData.ForEach(isir => isir.isPellEligible = true);
                actualFafsas = await actualRepository.GetFafsasAsync(inputStudentIds, inputAwardYearCodes);
                Assert.IsTrue(actualFafsas.All(fafsa => fafsa.IsPellEligible));
            }

            //test the fed isir id equals federally flagged isir id

            [TestMethod]
            public async Task CsFedIsirIdIsFederallyFlaggedFafsa_NoCorrectionsTest()
            {
                expectedRepository.isirFafsaData.ForEach(isir => isir.correctionId = string.Empty);
                var expectedIsirIds = expectedRepository.csStudentData.Select(i => i.federalIsirId);
                actualFafsas = await actualRepository.GetFafsasAsync(inputStudentIds, inputAwardYearCodes);
                var actualIds = actualFafsas.Where(f => f.IsFederallyFlagged).Select(f => f.Id);

                CollectionAssert.AreEquivalent(expectedIsirIds.ToList(), actualIds.ToList());
            }

            [TestMethod]
            public async Task CsFedIsirIdIsFederallyFlaggedFafsa_WithCorrectionTest()
            {
                var expectedId = expectedRepository.csStudentData.First().federalIsirId;
                var origFafsa = expectedRepository.isirFafsaData.First(i => i.id == expectedId);
                origFafsa.correctionId = "foobar";
                expectedRepository.isirFafsaData.Add(new TestFafsaRepository.IsirFafsaRecord()
                    {
                        id = "foobar",
                        awardYear = origFafsa.awardYear,
                        correctedFromId = origFafsa.id,
                        studentId = inputStudentIds.First()
                    });
                actualFafsas = await actualRepository.GetFafsasAsync(inputStudentIds, inputAwardYearCodes);

                Assert.IsTrue(actualFafsas.First(f => f.Id == "foobar").IsFederallyFlagged);
            }

            //test the fedIsir's csFc is not a number, familyContribution is null;
            [TestMethod]
            public async Task ErrorParsingFamilyContribution_SetToNullTest()
            {
                expectedRepository.csStudentData.ForEach(cs => cs.federalFamilyContribution = null);
                actualFafsas = await actualRepository.GetFafsasAsync(inputStudentIds, inputAwardYearCodes);

                Assert.IsTrue(actualFafsas.All(fafsa => !fafsa.FamilyContribution.HasValue));
            }

            //test the inst isir id equals inst flagged isir id
            [TestMethod]
            public async Task CsInstIsirIdIsInstitutionallyFlaggedFafsa_NoCorrectionsTest()
            {
                expectedRepository.isirFafsaData.ForEach(isir => isir.correctionId = string.Empty);
                var expectedIsirIds = expectedRepository.csStudentData.Select(i => i.insitutionIsirId);
                actualFafsas = await actualRepository.GetFafsasAsync(inputStudentIds, inputAwardYearCodes);
                var actualIds = actualFafsas.Where(f => f.IsInstitutionallyFlagged).Select(f => f.Id);

                CollectionAssert.AreEquivalent(expectedIsirIds.ToList(), actualIds.ToList());
            }

            [TestMethod]
            public async Task CsInstIsirIdIsInstitutionallyFlaggedFafsa_WithCorrectionTest()
            {
                var expectedId = expectedRepository.csStudentData.First().insitutionIsirId;
                var origFafsa = expectedRepository.isirFafsaData.First(i => i.id == expectedId);
                origFafsa.correctionId = "foobar";
                expectedRepository.isirFafsaData.Add(new TestFafsaRepository.IsirFafsaRecord()
                {
                    id = "foobar",
                    awardYear = origFafsa.awardYear,
                    correctedFromId = origFafsa.id,
                    studentId = inputStudentIds.First()
                });
                actualFafsas = await actualRepository.GetFafsasAsync(inputStudentIds, inputAwardYearCodes);

                Assert.IsTrue(actualFafsas.First(f => f.Id == "foobar").IsInstitutionallyFlagged);
            }

            //test isirFafsaToUse.import year is null (should throw/catch exception in BuildFafsa, and log error
            [TestMethod]
            public async Task UnableToBuildFafsaTest()
            {
                var expectedFafsa = expectedRepository.isirFafsaData.First(i => string.IsNullOrEmpty(i.correctionId));
                expectedFafsa.awardYear = null;
                actualFafsas = await actualRepository.GetFafsasAsync(inputStudentIds, inputAwardYearCodes);

                Assert.IsNull(actualFafsas.FirstOrDefault(f => f.Id == expectedFafsa.id));

                loggerMock.Verify(l => l.Info(It.IsAny<Exception>(),
                    string.Format("Unable to build Fafsa object with record id {0}, studentId {1}, awardYear {2}.", expectedFafsa.id, It.IsAny<string>(), expectedFafsa.awardYear)));
            }
        }

        public FafsaRepository BuildRepository()
        {
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<CsAcyr>(It.IsAny<string>(), It.IsAny<string[]>(), true))
                .Returns<string, string[], bool>((acyrFile, ids, b) =>
                {
                    var awardYear = acyrFile.Split('.')[1];
                    return Task.FromResult(new Collection<CsAcyr>(
                        ids.SelectMany(studentId =>
                            expectedRepository.csStudentData
                            .Where(cs => cs.awardYear == awardYear)
                            .Select(cs =>
                                new CsAcyr()
                                {
                                    Recordkey = studentId,
                                    CsIsirTransIds = cs.isirRecordIds,
                                    CsFedIsirId = cs.federalIsirId,
                                    CsFc = cs.federalFamilyContribution.HasValue ? cs.federalFamilyContribution.Value.ToString() : string.Empty,
                                    CsInstIsirId = cs.insitutionIsirId,
                                    CsInstFc = cs.institutionalFamilyContribution
                                }
                            )).ToList()));
                }
                ).Callback(() => csAcyrBulkRecordReadCount++);

            dataReaderMock.Setup(d => d.BulkReadRecordAsync<IsirFafsa>(It.IsAny<string[]>(), true))
                .Returns<string[], bool>((ids, b) => Task.FromResult(
                    new Collection<IsirFafsa>(
                        expectedRepository.isirFafsaData
                        .Where(i => ids.Contains(i.id))
                        .Select(i =>
                            new IsirFafsa()
                            {
                                Recordkey = i.id,
                                IfafCorrectedFromId = i.correctedFromId,
                                IfafCorrectionId = i.correctionId,
                                IfafImportYear = i.awardYear,
                                IfafIsirType = i.isirType,
                                IfafPAgi = i.parentAgi,
                                IfafSAgi = i.studentAgi,
                                IfafStudentId = i.studentId,
                                IfafHousing1 = i.housingCode1,
                                IfafHousing2 = i.housingCode2,
                                IfafHousing3 = i.housingCode3,
                                IfafHousing4 = i.housingCode4,
                                IfafHousing5 = i.housingCode5,
                                IfafHousing6 = i.housingCode6,
                                IfafTitleiv1 = i.titleIVCode1,
                                IfafTitleiv2 = i.titleIVCode2,
                                IfafTitleiv3 = i.titleIVCode3,
                                IfafTitleiv4 = i.titleIVCode4,
                                IfafTitleiv5 = i.titleIVCode5,
                                IfafTitleiv6 = i.titleIVCode6
                            }
                        ).ToList()))
                ).Callback(() => isirFafsaBulkRecordReadCount++);

            dataReaderMock.Setup(d => d.BulkReadRecordAsync<IsirResults>(It.IsAny<string[]>(), true))
                .Returns<string[], bool>((ids, b) => Task.FromResult(
                    new Collection<IsirResults>(
                        expectedRepository.isirResultData
                        .Where(i => ids.Contains(i.id))
                        .Select(i =>
                            new IsirResults()
                            {
                                Recordkey = i.id,
                                IresCpsPellElig = i.isPellEligible ? "Y" : string.Empty
                            }
                        ).ToList()))
                ).Callback(() => isirResultBulkRecordReadCount++);

            return new FafsaRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }
    }
}
