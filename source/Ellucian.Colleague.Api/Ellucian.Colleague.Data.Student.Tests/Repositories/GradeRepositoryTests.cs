// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Data.Colleague;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class GradeRepositoryTests : BaseRepositorySetup
    {
        GradeRepository gradeRepo;
        IEnumerable<Grade> allGrades;
        IEnumerable<Grade> allTestRepoGrades;
        Collection<Grades> gradesResponseData;

        [TestInitialize]
        public async void Initialize()
        {
            MockInitialize();
            allTestRepoGrades = (await new TestGradeRepository().GetHedmAsync()).AsEnumerable();
            gradesResponseData = BuildGradesResponse(allTestRepoGrades);
            gradeRepo = BuildValidRepository();

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

        }

        [TestCleanup]
        public void Cleanup()
        {
            gradeRepo = null;
            allGrades = null;
        }

        [TestMethod]
        public async Task GradeRepo_Get()
        {
            // Verify that the Get (all) returns all grades in the test repository
            // with fields properly initialized

            // This tests the comparison grade feature in several ways based on the data setup in TestGradeRepository:
            // 1. Grade "B" in the TestGradeRepository has a comparison grade that does not exist as a GRADES record. Both the real
            //    repository and the test repository drop the comparison grade attribute as a result. So the assertions below 
            //    that the two comparison grades are equal validates that the real repository drops a comparison grade that does not exist.
            //
            // 2. Grade "D" in the TestGradeRepository has a comparison grade with the same grade scheme as the "D" grade. Both the real
            //    repository and the test repository drop the comparison grade attribute, because the two grades cannot be in the same scheme
            //    So the assertions below that the two comparison grades are equal validates that the real repository drops a comparison grade 
            // that does not exist.
            //
            // 3. Grades "A" and "P" in the TestGradeRepository have valid comparison grades. The assertions below that the comparison grades
            //    are successfully read validate that the repository successfully looks up the record of the comparsion grade, and populates the 
            //    ComparisonGrade property as a result.

            allGrades = await gradeRepo.GetAsync();
            foreach (var testGrade in allTestRepoGrades)
            {
                var grade = allGrades.Where(g=>g.Id == testGrade.Id).FirstOrDefault();
                Assert.IsNotNull(grade);
                Assert.AreEqual(testGrade.Description, grade.Description);
                Assert.AreEqual(testGrade.GradeSchemeCode, grade.GradeSchemeCode);
                Assert.AreEqual(testGrade.GradeValue, grade.GradeValue);
                Assert.AreEqual(testGrade.IsWithdraw, grade.IsWithdraw);
                Assert.AreEqual(testGrade.LetterGrade, grade.LetterGrade);
                Assert.AreEqual(testGrade.ComparisonGrade == null ? null : testGrade.ComparisonGrade.ComparisonGradeId, grade.ComparisonGrade == null ? null : grade.ComparisonGrade.ComparisonGradeId);
                Assert.AreEqual(testGrade.ComparisonGrade == null ? null : testGrade.ComparisonGrade.ComparisonGradeValue, grade.ComparisonGrade == null ? null : grade.ComparisonGrade.ComparisonGradeValue);
                Assert.AreEqual(testGrade.ComparisonGrade == null ? null : testGrade.ComparisonGrade.ComparisonGradeSchemeCode, grade.ComparisonGrade == null ? null : grade.ComparisonGrade.ComparisonGradeSchemeCode);
                // Grade priority is set to 0 if incoming value is null.
                Assert.AreEqual(testGrade.GradePriority == null? 0 : testGrade.GradePriority, grade.GradePriority);
                Assert.AreEqual(testGrade.ExcludeFromFacultyGrading, grade.ExcludeFromFacultyGrading);
                Assert.AreEqual(testGrade.IncludeInWebFinalGradesList, grade.IncludeInWebFinalGradesList);
                Assert.AreEqual(testGrade.IncludeInWebMidtermGradesList, grade.IncludeInWebMidtermGradesList);
                Assert.AreEqual(testGrade.CanBeUsedAfterDropGradeRequiredDate, grade.CanBeUsedAfterDropGradeRequiredDate);
            }
        }

        [TestMethod]
        public async Task GradeRepo_GetHedm()
        {
            // Verify that the GetHedm (all) returns all grades in the test repository
            // with fields properly initialized
            allGrades = await gradeRepo.GetHedmAsync();
            foreach (var testGrade in allTestRepoGrades)
            {
                var grade = allGrades.Where(g => g.Id == testGrade.Id).FirstOrDefault();
                Assert.IsNotNull(grade);
                Assert.AreEqual(testGrade.Description, grade.Description);
                Assert.AreEqual(testGrade.GradeSchemeCode, grade.GradeSchemeCode);
                Assert.AreEqual(testGrade.GradeValue, grade.GradeValue);
                Assert.AreEqual(testGrade.IsWithdraw, grade.IsWithdraw);
                Assert.AreEqual(testGrade.LetterGrade, grade.LetterGrade);
                Assert.AreEqual(testGrade.ComparisonGrade == null ? null : testGrade.ComparisonGrade.ComparisonGradeId, grade.ComparisonGrade == null ? null : grade.ComparisonGrade.ComparisonGradeId);
                Assert.AreEqual(testGrade.ComparisonGrade == null ? null : testGrade.ComparisonGrade.ComparisonGradeValue, grade.ComparisonGrade == null ? null : grade.ComparisonGrade.ComparisonGradeValue);
                Assert.AreEqual(testGrade.ComparisonGrade == null ? null : testGrade.ComparisonGrade.ComparisonGradeSchemeCode, grade.ComparisonGrade == null ? null : grade.ComparisonGrade.ComparisonGradeSchemeCode);
                // Grade priority is set to 0 if incoming value is null.
                Assert.AreEqual(testGrade.GradePriority == null ? 0 : testGrade.GradePriority, grade.GradePriority);
                Assert.AreEqual(testGrade.IncludeInWebFinalGradesList, grade.IncludeInWebFinalGradesList);
                Assert.AreEqual(testGrade.IncludeInWebMidtermGradesList, grade.IncludeInWebMidtermGradesList);
                Assert.AreEqual(testGrade.CanBeUsedAfterDropGradeRequiredDate, grade.CanBeUsedAfterDropGradeRequiredDate);
            }
        }

        [TestMethod]
        public async Task GradeRepo_GetHedm_BypassCache()
        {
            // Verify that the GetHedm (all) returns all grades in the test repository
            // with fields properly initialized
            allGrades = await gradeRepo.GetHedmAsync(true);
            foreach (var testGrade in allTestRepoGrades)
            {
                var grade = allGrades.Where(g => g.Id == testGrade.Id).FirstOrDefault();
                Assert.IsNotNull(grade);
                Assert.AreEqual(testGrade.Description, grade.Description);
                Assert.AreEqual(testGrade.GradeSchemeCode, grade.GradeSchemeCode);
                Assert.AreEqual(testGrade.GradeValue, grade.GradeValue);
                Assert.AreEqual(testGrade.IsWithdraw, grade.IsWithdraw);
                Assert.AreEqual(testGrade.LetterGrade, grade.LetterGrade);
                Assert.AreEqual(testGrade.ComparisonGrade == null ? null : testGrade.ComparisonGrade.ComparisonGradeId, grade.ComparisonGrade == null ? null : grade.ComparisonGrade.ComparisonGradeId);
                Assert.AreEqual(testGrade.ComparisonGrade == null ? null : testGrade.ComparisonGrade.ComparisonGradeValue, grade.ComparisonGrade == null ? null : grade.ComparisonGrade.ComparisonGradeValue);
                Assert.AreEqual(testGrade.ComparisonGrade == null ? null : testGrade.ComparisonGrade.ComparisonGradeSchemeCode, grade.ComparisonGrade == null ? null : grade.ComparisonGrade.ComparisonGradeSchemeCode);

                // Grade priority is set to 0 if incoming value is null.
                Assert.AreEqual(testGrade.GradePriority == null ? 0 : testGrade.GradePriority, grade.GradePriority);
                Assert.AreEqual(testGrade.IncludeInWebFinalGradesList, grade.IncludeInWebFinalGradesList);
                Assert.AreEqual(testGrade.IncludeInWebMidtermGradesList, grade.IncludeInWebMidtermGradesList);
                Assert.AreEqual(testGrade.CanBeUsedAfterDropGradeRequiredDate, grade.CanBeUsedAfterDropGradeRequiredDate);
            }
        }

        [TestMethod]
        public async Task GradeRepo_GetHedmById()
        {
            var grade = allTestRepoGrades.FirstOrDefault();
            // Work back to a data contract record from the grade domain entity.
            Data.Student.DataContracts.Grades grdDataContract = BuildGradesResponse(grade);

            var guid = grdDataContract.RecordGuid;
            var id = grdDataContract.Recordkey;
            var guidLookupResult = new GuidLookupResult() { Entity = "GRADES", PrimaryKey = id }; 
            var guidLookupDict = new Dictionary<string, GuidLookupResult>();
            dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
            {
                if (gla.Any(gl => gl.Guid == guid))
                {
                    guidLookupDict.Add(guid, guidLookupResult);
                }
                return Task.FromResult(guidLookupDict);
            });

            // Mock a database read of the Grade ID selected above to return the data contract record we created.
            dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Grades>(grdDataContract.Recordkey, true)).ReturnsAsync(grdDataContract);

            // The "FirstOrDefault" grade select does have a comparison grade. Mock a data reader read of the comparison grade record , since
            // the repository will read the comparison grade record as well.
            // The assertions below that the comparison grade properties are returned correctly will validate that GetHedmGradeByIdAsync reads the GRADES record of the 
            // comparison grade in addition to the main GRADES record when the main GRADES record specifies a comparison grade.
            if (grdDataContract.GrdComparisonGrade != null)
            {
                var compGrade = allTestRepoGrades.Where(g => g.Id == grdDataContract.GrdComparisonGrade).FirstOrDefault();
                if (compGrade != null)
                {
                    // Work back to a data contract record from the grade domain entity.
                    Data.Student.DataContracts.Grades grdCompDataContract = BuildGradesResponse(compGrade);
                    dataReaderMock.Setup(acc => acc.ReadRecordAsync<Grades>(compGrade.Id, true)).ReturnsAsync(grdCompDataContract);
                }

            }

            // Verify that the GetById returns a grade in the test repository
            // with fields properly initialized
            Grade grade_element = await gradeRepo.GetHedmGradeByIdAsync("d874e05d-9d97-4fa3-8862-5044ef2384d0");    
           
            Assert.IsNotNull(grade_element);
            Assert.AreEqual(grade.Description, grade_element.Description);
            Assert.AreEqual(grade.GradeSchemeCode, grade_element.GradeSchemeCode);
            Assert.AreEqual(grade.GradeValue, grade_element.GradeValue);
            Assert.AreEqual(grade.IsWithdraw, grade_element.IsWithdraw);
            Assert.AreEqual(grade.LetterGrade, grade_element.LetterGrade);
            Assert.AreEqual(grade.ComparisonGrade == null ? null : grade.ComparisonGrade.ComparisonGradeId, grade_element.ComparisonGrade == null ? null : grade_element.ComparisonGrade.ComparisonGradeId);
            Assert.AreEqual(grade.ComparisonGrade == null ? null : grade.ComparisonGrade.ComparisonGradeValue, grade_element.ComparisonGrade == null ? null : grade_element.ComparisonGrade.ComparisonGradeValue);
            Assert.AreEqual(grade.ComparisonGrade == null ? null : grade.ComparisonGrade.ComparisonGradeSchemeCode, grade_element.ComparisonGrade == null ? null : grade_element.ComparisonGrade.ComparisonGradeSchemeCode);

            // Grade priority is set to 0 if incoming value is null.
            Assert.AreEqual(grade.GradePriority == null ? 0 : grade.GradePriority, grade_element.GradePriority);
            Assert.AreEqual(grade.IncludeInWebFinalGradesList, grade_element.IncludeInWebFinalGradesList);
            Assert.AreEqual(grade.IncludeInWebMidtermGradesList, grade_element.IncludeInWebMidtermGradesList);
            Assert.AreEqual(grade.CanBeUsedAfterDropGradeRequiredDate, grade_element.CanBeUsedAfterDropGradeRequiredDate);
        }

        [TestMethod]
        public async Task GradeRepo_GetIgnoresComparisonGradeThatDoesNotExist()
        {
            // The "B" grade in the mock repo references a comparison grade 99 that does not exist.
            // The repo should log that, but populating the comparison grade attribute.
            allGrades = await gradeRepo.GetAsync();

            // find the grades that have a comparison grade set
            Grade gradeB = allGrades.Where(g => g.Id == "B").FirstOrDefault();
            Assert.IsNotNull(gradeB);
            Assert.IsNull(gradeB.ComparisonGrade);
        }

        [TestMethod]
        public async Task GradeRepo_GetComparisonGradeRecordShouldHaveComparisonGrade()
        {
            allGrades = await gradeRepo.GetAsync();

            // find the grades that have a comparison grade set
            var grades = allGrades.Where(g => g.ComparisonGrade != null);
            foreach (var grade in grades)
            {
                // find the comparison grade record for the grade
                var comparisonGrade = allGrades.Where(g => g.Id == grade.ComparisonGrade.ComparisonGradeId).FirstOrDefault();
                if (comparisonGrade != null) // needed to account for the non-existant comparison grade test
                {
                    // check that the comparison grade in the comparison grade object has been set 
                    Assert.IsNotNull(comparisonGrade.ComparisonGrade);
                }
            }
        }

        [TestMethod]
        public async Task GradeRepo_GetWithNoRegDefaultsDropGrades()
        {
            var regDefaultsResponse = new RegDefaults();
            dataReaderMock.Setup(acc => acc.ReadRecordAsync<Data.Student.DataContracts.RegDefaults>("ST.PARMS", "REG.DEFAULTS", true)).ReturnsAsync(regDefaultsResponse);
            
            // Verify that the Get (all) returns all grades in the test repository
            // with fields properly initialized
            allGrades = await gradeRepo.GetAsync();
            var grade = allGrades.Where(g=>g.Id == "WD").FirstOrDefault();
            Assert.IsFalse(grade.IsWithdraw);
        }

        [TestMethod]
        public async Task GradeRepo_GetWithNoRegDefaultsRecord()
        {
            var regDefaultsResponse = new RegDefaults();
            regDefaultsResponse = null;
            dataReaderMock.Setup<RegDefaults>(acc => acc.ReadRecord<Data.Student.DataContracts.RegDefaults>("ST.PARMS", "REG.DEFAULTS", true)).Returns(regDefaultsResponse);

            // Verify that the Get (all) returns all grades in the test repository
            // with fields properly initialized
            allGrades =await gradeRepo.GetAsync();
            var grade = allGrades.Where(g => g.Id == "WD").FirstOrDefault();
            Assert.IsTrue(grade.IsWithdraw);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GradeRepo_GetHedmById_NullId()
        {
            await gradeRepo.GetHedmGradeByIdAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GradeRepo_GetHedmById_EmptyId()
        {
            await gradeRepo.GetHedmGradeByIdAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GradeRepo_GetHedmById_NullPrimaryKey()
        {
            var grade = allTestRepoGrades.FirstOrDefault();
            Grades grd = BuildGradesResponse(grade);

            var guid = grd.RecordGuid;
            var id = grd.Recordkey;
            var guidLookupResult = new GuidLookupResult() { Entity = "GRADES", PrimaryKey = null }; 
            var guidLookupDict = new Dictionary<string, GuidLookupResult>();
            dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
            {
                if (gla.Any(gl => gl.Guid == guid))
                {
                    guidLookupDict.Add(guid, guidLookupResult);
                }
                return Task.FromResult(guidLookupDict);
            });

            dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Grades>(grd.Recordkey, true)).ReturnsAsync(grd); 


            // Verify that the GetById returns a grade in the test repository
            // with fields properly initialized
            await gradeRepo.GetHedmGradeByIdAsync("d874e05d-9d97-4fa3-8862-5044ef2384d0");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GradeRepo_GetHedmById_InvalidGrade()
        {
            var grade = allTestRepoGrades.FirstOrDefault();
            Grades grd = BuildGradesResponse(grade);

            var guid = grd.RecordGuid;
            var id = grd.Recordkey;
            var guidLookupResult = new GuidLookupResult() { Entity = "GRADES", PrimaryKey = id };
            var guidLookupDict = new Dictionary<string, GuidLookupResult>();
            dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
            {
                if (gla.Any(gl => gl.Guid == guid))
                {
                    guidLookupDict.Add(guid, guidLookupResult);
                }
                return Task.FromResult(guidLookupDict);
            });

            dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Grades>(grd.Recordkey, true)).ReturnsAsync(null); 


            // Verify that the GetById returns a grade in the test repository
            // with fields properly initialized
            await gradeRepo.GetHedmGradeByIdAsync("d874e05d-9d97-4fa3-8862-5044ef2384d0");
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GradeRepo_GetHedmById_GradeMissingGradeScheme()
        {
            var grade = allTestRepoGrades.FirstOrDefault();
            Grades grd = BuildGradesResponse(grade);
            grd.GrdGradeScheme = "";

            var guid = grd.RecordGuid;
            var id = grd.Recordkey;
            var guidLookupResult = new GuidLookupResult() { Entity = "GRADES", PrimaryKey = id };
            var guidLookupDict = new Dictionary<string, GuidLookupResult>();
            dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
            {
                if (gla.Any(gl => gl.Guid == guid))
                {
                    guidLookupDict.Add(guid, guidLookupResult);
                }
                return Task.FromResult(guidLookupDict);
            });

            dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Grades>(grd.Recordkey, true)).ReturnsAsync(grd);
            await gradeRepo.GetHedmGradeByIdAsync(grade.Guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GradeRepo_GetHedm_GradeMissingGradeScheme()
        {
            var grade = allTestRepoGrades.FirstOrDefault();
            IEnumerable<Grade>  gradeList = new List<Grade>() { grade };
            Collection<Grades> gradesList = BuildGradesResponse(gradeList);
            gradesList.First().GrdGradeScheme = "";


            var guid = gradesList.First().RecordGuid;
            var id = gradesList.First().Recordkey;

            var guidLookupResult = new GuidLookupResult() { Entity = "GRADES", PrimaryKey = id };
            var guidLookupDict = new Dictionary<string, GuidLookupResult>();
            dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
            {
                if (gla.Any(gl => gl.Guid == guid))
                {
                    guidLookupDict.Add(guid, guidLookupResult);
                }
                return Task.FromResult(guidLookupDict);
            });

            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.Grades>("", true)).ReturnsAsync(gradesList);
            await gradeRepo.GetHedmAsync();
        }

        private GradeRepository BuildValidRepository()
        {
            // Set up response for grade Get request
            dataReaderMock.Setup<Task<Collection<Grades>>>(acc => acc.BulkReadRecordAsync<Grades>("", true)).Returns(Task.FromResult(gradesResponseData));

            var regDefaultsResponse = BuildRegDefaultsValidResponse();
            dataReaderMock.Setup<Task<RegDefaults>>(acc => acc.ReadRecordAsync<Data.Student.DataContracts.RegDefaults>("ST.PARMS", "REG.DEFAULTS",true)).Returns(Task.FromResult(regDefaultsResponse));

            // Construct referenceData repository. (BaseRepositorySetup does basic mocking setup for all these objects.)
            gradeRepo = new GradeRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            return gradeRepo;
        }

        private Collection<Grades> BuildGradesResponse(IEnumerable<Grade> grades)
        {
            Collection<Grades> gradeData = new Collection<Grades>();

            foreach (var grade in grades)
            {
                var repoGrade = new Grades();
                repoGrade.RecordGuid = grade.Guid;
                repoGrade.Recordkey = grade.Id;
                repoGrade.GrdGrade = grade.LetterGrade;
                repoGrade.GrdLegend = grade.Description;
                repoGrade.GrdValue = grade.GradeValue;
                repoGrade.GrdGradeScheme = grade.GradeSchemeCode;
                // Withdrawl grades must be populated in the repo response manually. Note this
                // line will be affected if the grades F, WP, WF are changed in the TestGradeRepository.
                repoGrade.GrdWithdrawGrade = repoGrade.Recordkey == "F" ? "WF" : "W";
                if (repoGrade.Recordkey == "W" || repoGrade.Recordkey == "WF" || repoGrade.Recordkey == "I")
                {
                    repoGrade.GrdWithdrawGrade = null;
                }
                repoGrade.GrdExcludeFromFacFlag = grade.ExcludeFromFacultyGrading ? "Y" : string.Empty;

                repoGrade.GrdRepeatValue = grade.GradePriority;

                repoGrade.GrdComparisonGrade = (grade.ComparisonGrade == null) ? null : grade.ComparisonGrade.ComparisonGradeId;
                repoGrade.GrdUseInFinalGrdList = grade.IncludeInWebFinalGradesList ? "Y" : "N";
                repoGrade.GrdUseInMidtermGrdList = grade.IncludeInWebMidtermGradesList ? "Y" : "N";
                repoGrade.GrdUseAfterDropGrdReqd = grade.CanBeUsedAfterDropGradeRequiredDate ? "Y" : "N";
                gradeData.Add(repoGrade);
            }

            return gradeData;
        }

        private Grades BuildGradesResponse(Grade grade)
        {
            Grades gradeData = new Grades();

            var repoGrade = new Grades();
            repoGrade.RecordGuid = grade.Guid;
            repoGrade.Recordkey = grade.Id;
            repoGrade.GrdGrade = grade.LetterGrade;
            repoGrade.GrdLegend = grade.Description;
            repoGrade.GrdValue = grade.GradeValue;
            repoGrade.GrdGradeScheme = grade.GradeSchemeCode;
            // Withdrawl grades must be populated in the repo response manually. Note this
            // line will be affected if the grades F, WP, WF are changed in the TestGradeRepository.
            repoGrade.GrdWithdrawGrade = repoGrade.Recordkey == "F" ? "WF" : "W";
            if (repoGrade.Recordkey == "W" || repoGrade.Recordkey == "WF" || repoGrade.Recordkey == "I")
            {
                repoGrade.GrdWithdrawGrade = null;
            }

            repoGrade.GrdRepeatValue = grade.GradePriority;

            // Comparison Grade records will have a comparison grade value populated in the test repositories grade array
            // Need to make sure these values are not set to allow for tests to emulate the way data will be sent from Colleague
            // line will be affected if the grades Trasfer A and Trasfer Pass are changed in the TestGradeRepository.
            if (repoGrade.Recordkey != "1" && repoGrade.Recordkey != "2")
                repoGrade.GrdComparisonGrade = grade.ComparisonGrade.ComparisonGradeId;

            repoGrade.GrdUseInFinalGrdList = grade.IncludeInWebFinalGradesList ? "Y" : "N";
            repoGrade.GrdUseInMidtermGrdList = grade.IncludeInWebMidtermGradesList ? "Y" : "N";
            repoGrade.GrdUseAfterDropGrdReqd = grade.CanBeUsedAfterDropGradeRequiredDate ? "Y" : "N";

            gradeData = repoGrade;

            return gradeData;
        }

        private RegDefaults BuildRegDefaultsValidResponse()
        {
            var regDefaults = new RegDefaults();
            regDefaults.RgdPhoneDropsEntityAssociation = new List<RegDefaultsRgdPhoneDrops>();
            regDefaults.RgdPhoneDropsEntityAssociation.Add(new RegDefaultsRgdPhoneDrops()
               {
                   RgdPhoneDropGradeAssocMember = "WD",
                   RgdPhoneDropGradeSchemeAssocMember = "UG"
               });
            return regDefaults;
        }

    }
}
