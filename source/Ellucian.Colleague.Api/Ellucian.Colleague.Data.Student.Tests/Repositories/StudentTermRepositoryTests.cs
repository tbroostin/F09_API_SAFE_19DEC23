// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Data.Base.Tests.Repositories;

using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Web.Security;
using System.Threading.Tasks;
using Ellucian.Data.Colleague;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentTermRepositoryTests
    {
        [TestClass]
        public class GetStudentTermsLoads : BasePersonSetup
        {
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private IStudentTermRepository studentTermRepo;

            private string knownStudentId1;
            private string knownStudentId2;
            private string knownStudentId3;
            private IEnumerable<string> ids;

            private string knownStudentTermsId1;
            private string knownStudentTermsId2;
            private string knownStudentTermsId3;
            private string knownStudentTermsId4;
            private string knownStudentTermsId5;
            private IEnumerable<string> studentTermIds;

            private Collection<DataContracts.Students> students;
            private Collection<DataContracts.StudentTerms> studentTerms;
            private List<StudentTermsSttrStatuses> statuses;

            [TestInitialize]
            public void Initialize()
            {
                // Setup mock repositories
                base.MockInitialize();                
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                studentTermRepo = BuildValidStudentTermRepository();
                
                // Mock student data
                students = new Collection<DataContracts.Students>();
                knownStudentId1 = "0000001";
                students.Add(new DataContracts.Students() { Recordkey = knownStudentId1, StuAcadLevels = new List<string> {"UG"}, StuTerms = new List<string> {"2015/FA"}});
                knownStudentId2 = "0000002";
                students.Add(new DataContracts.Students() { Recordkey = knownStudentId2, StuAcadLevels = new List<string> { "UG" }, StuTerms = new List<string> { "2015/SP", "2015/FA" } });
                knownStudentId3 = "0000003";
                students.Add(new DataContracts.Students() { Recordkey = knownStudentId3, StuAcadLevels = new List<string> { "UG", "GR" }, StuTerms = new List<string> { "2015/FA", "2016/SP" } });

                statuses = new List<StudentTermsSttrStatuses>() 
                {
                    new StudentTermsSttrStatuses("A", new DateTime(2016, 05, 01)),
                    new StudentTermsSttrStatuses("I", new DateTime(2016, 06, 01)),
                    new StudentTermsSttrStatuses("F", new DateTime(2016, 07, 01))
                };

                //Mock student terms data
                studentTerms = new Collection<DataContracts.StudentTerms>();
                knownStudentTermsId1 = "0000001*2015/FA*UG";
                studentTerms.Add(new DataContracts.StudentTerms() { Recordkey = knownStudentTermsId1, SttrStudentLoad = "F", RecordGuid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156", SttrStatusesEntityAssociation = statuses });
                knownStudentTermsId2 = "0000002*2015/SP*UG";
                studentTerms.Add(new DataContracts.StudentTerms() { Recordkey = knownStudentTermsId2, SttrStudentLoad = "F", RecordGuid = "0279fd3d-8fbf-4d2b-92b5-1e06ea112dc4", SttrStatusesEntityAssociation = statuses });
                knownStudentTermsId3 = "0000002*2015/FA*UG";
                studentTerms.Add(new DataContracts.StudentTerms() { Recordkey = knownStudentTermsId3, SttrStudentLoad = "P", RecordGuid = "6e2e3834-1c3f-415b-9554-2722ff4dc84a", SttrStatusesEntityAssociation = statuses });
                knownStudentTermsId4 = "0000003*2015/FA*UG";
                studentTerms.Add(new DataContracts.StudentTerms() { Recordkey = knownStudentTermsId4, SttrStudentLoad = "L", RecordGuid = "2051e603-0bd1-4e54-9848-8386130eb4ba", SttrStatusesEntityAssociation = statuses });
                knownStudentTermsId5 = "0000003*2015/FA*GR";
                studentTerms.Add(new DataContracts.StudentTerms() { Recordkey = knownStudentTermsId5, SttrStudentLoad = "P", RecordGuid = "ecdba581-534b-4ca5-8d33-1ea629e56ad8", SttrStatusesEntityAssociation = statuses });

                // mock data accessor STUDENTS response 
                ids = new List<string>() { knownStudentId1, knownStudentId2, knownStudentId3 };    
                dataReaderMock.Setup<Task<Collection<Students>>>(a => a.BulkReadRecordAsync<Students>(It.IsAny<string[]>(), true)).ReturnsAsync(students);
                dataReaderMock.Setup<Task<Collection<Students>>>(a => a.BulkReadRecordAsync<Students>(ids.ToArray(), true)).ReturnsAsync(students);

                // mock data accessor STUDENT.TERMS response
                studentTermIds = new List<string>() { knownStudentTermsId1, knownStudentTermsId2, knownStudentTermsId3, knownStudentTermsId4, knownStudentTermsId5 };
                
                // Read for all student terms
                dataReaderMock.Setup<Task<Collection<StudentTerms>>>(a => a.BulkReadRecordAsync<StudentTerms>(studentTermIds.ToArray(), true)).ReturnsAsync(studentTerms);
                
                // Read for single student term of student 0000001
                dataReaderMock.Setup<Task<Collection<StudentTerms>>>(a => a.BulkReadRecordAsync<StudentTerms>(It.Is<string[]>(s => s.Count() == 1 && s[0] == knownStudentTermsId1), true)).ReturnsAsync(new Collection<StudentTerms>(studentTerms.Where(t => t.Recordkey == knownStudentTermsId1).ToList()));
                
                // Read for each student terms of student 0000002
                dataReaderMock.Setup<Task<Collection<StudentTerms>>>(a => a.BulkReadRecordAsync<StudentTerms>(It.Is<string[]>(s => s.Count() == 1 && s[0] == knownStudentTermsId2), true)).ReturnsAsync(new Collection<StudentTerms>(studentTerms.Where(t => t.Recordkey == knownStudentTermsId2).ToList()));
                dataReaderMock.Setup<Task<Collection<StudentTerms>>>(a => a.BulkReadRecordAsync<StudentTerms>(It.Is<string[]>(s => s.Count() == 1 && s[0] == knownStudentTermsId3), true)).ReturnsAsync(new Collection<StudentTerms>(studentTerms.Where(t => t.Recordkey == knownStudentTermsId3).ToList()));
                
                // Read for each student terms of student 0000003 and for both student terms together - since student has multiple academic levels
                dataReaderMock.Setup<Task<Collection<StudentTerms>>>(a => a.BulkReadRecordAsync<StudentTerms>(It.Is<string[]>(s => s.Count() == 1 && s[0] == knownStudentTermsId4), true)).ReturnsAsync(new Collection<StudentTerms>(studentTerms.Where(t => t.Recordkey == knownStudentTermsId4).ToList()));
                dataReaderMock.Setup<Task<Collection<StudentTerms>>>(a => a.BulkReadRecordAsync<StudentTerms>(It.Is<string[]>(s => s.Count() == 1 && s[0] == knownStudentTermsId5), true)).ReturnsAsync(new Collection<StudentTerms>(studentTerms.Where(t => t.Recordkey == knownStudentTermsId5).ToList()));
                //dataReaderMock.Setup<Task<Collection<StudentTerms>>>(a => a.BulkReadRecordAsync<StudentTerms>(It.Is<string[]>(s => s.Count() == 2 && s[0] == knownStudentTermsId4 && s[1] == knownStudentTermsId5), true)).ReturnsAsync(new Collection<StudentTerms>(studentTerms.Where(t => t.Recordkey == knownStudentTermsId4 || t.Recordkey == knownStudentTermsId5).ToList()));
                //dataReaderMock.Setup<Task<Collection<StudentTerms>>>(a => a.BulkReadRecordAsync<StudentTerms>(It.Is<string[]>(s => s.Count() == 2 && s[1] == knownStudentTermsId4 && s[0] == knownStudentTermsId5), true)).ReturnsAsync(new Collection<StudentTerms>(studentTerms.Where(t => t.Recordkey == knownStudentTermsId4 || t.Recordkey == knownStudentTermsId5).ToList()));
                dataReaderMock.Setup<Task<Collection<StudentTerms>>>(a => a.BulkReadRecordAsync<StudentTerms>(It.Is<string[]>(s => s.Count() == 2 && s.All(t => new string[] { knownStudentTermsId4, knownStudentTermsId5 }.Contains(t))), true)).ReturnsAsync(new Collection<StudentTerms>(studentTerms.Where(t => t.Recordkey == knownStudentTermsId4 || t.Recordkey == knownStudentTermsId5).ToList()));
            }

            private IStudentTermRepository BuildValidStudentTermRepository()
            {
                // Set up data accessor for mocking 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

                // Construct repository
                studentTermRepo = new StudentTermRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return studentTermRepo;
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                studentTermRepo = null;
            }

            [TestMethod]
            public async Task ReturnStudentTermsLoad()
            {
                // Test with input for Ids, term, academic level.
                IEnumerable<string> studentIds = new List<string>() { "0000001", "0000002", "0000003" };
                var resultUg = await studentTermRepo.GetStudentTermsByStudentIdsAsync(studentIds, "2015/FA", "UG");
                Assert.AreEqual(3, resultUg.Count());
                Assert.IsTrue(resultUg["0000001"] != null && resultUg["0000001"].Where(s => s.Term == "2015/FA" && s.AcademicLevel == "UG" && s.StudentLoad == "F").Count() == 1);
                Assert.IsTrue(resultUg["0000002"] != null && resultUg["0000002"].Where(s => s.Term == "2015/FA" && s.AcademicLevel == "UG" && s.StudentLoad == "P").Count() == 1);
                Assert.IsTrue(resultUg["0000003"] != null && resultUg["0000003"].Where(s => s.Term == "2015/FA" && s.AcademicLevel == "UG" && s.StudentLoad == "L").Count() == 1);
                Assert.IsTrue(resultUg["0000003"] != null && resultUg["0000003"].Where(s => s.Term == "2015/FA").Count() == 1);

                // Test with input for Ids, term, and no academic level (which is how Pilot CRM invokes it)
                var resultAll = await studentTermRepo.GetStudentTermsByStudentIdsAsync(studentIds, "2015/FA", null);
                Assert.AreEqual(3, resultAll.Count());
                Assert.IsTrue(resultAll["0000001"] != null && resultAll["0000001"].Where(s => s.Term == "2015/FA" && s.AcademicLevel == "UG" && s.StudentLoad == "F").Count() == 1);
                Assert.IsTrue(resultAll["0000002"] != null && resultAll["0000002"].Where(s => s.Term == "2015/FA" && s.AcademicLevel == "UG" && s.StudentLoad == "P").Count() == 1);
                Assert.IsTrue(resultAll["0000003"] != null && resultAll["0000003"].Where(s => s.Term == "2015/FA" && s.AcademicLevel == "UG" && s.StudentLoad == "L").Count() == 1);
                Assert.IsTrue(resultAll["0000003"] != null && resultAll["0000003"].Where(s => s.Term == "2015/FA" && s.AcademicLevel == "GR" && s.StudentLoad == "P").Count() == 1);
                Assert.IsTrue(resultAll["0000003"] != null && resultAll["0000003"].Where(s => s.Term == "2015/FA").Count() == 2);

                // Negative testing for a previous term input.
                var result2015Spring = await studentTermRepo.GetStudentTermsByStudentIdsAsync(studentIds, "2015/SP", null);
                Assert.AreEqual(3, result2015Spring.Count());
                Assert.IsTrue(result2015Spring["0000001"] != null && result2015Spring["0000001"].Where(s => s.Term == "2015/FA").Count() == 0);
                Assert.IsTrue(result2015Spring["0000002"] != null && result2015Spring["0000002"].Where(s => s.Term == "2015/FA" && s.AcademicLevel == "UG").Count() == 0);
                Assert.IsTrue(result2015Spring["0000002"] != null && result2015Spring["0000002"].Where(s => s.Term == "2015/SP" && s.AcademicLevel == "UG" && s.StudentLoad == "F").Count() == 1);
                Assert.IsTrue(result2015Spring["0000003"] != null && result2015Spring["0000002"].Where(s => s.Term == "2015/SP").Count() == 1);
                Assert.IsTrue(result2015Spring["0000003"] != null && result2015Spring["0000003"].Where(s => s.Term == "2015/FA").Count() == 0);
            }

            [TestMethod]
            public async Task StudentTermRepository_GetStudentTermsAsync()
            {
                IEnumerable<string> studentIds = new List<string>() { knownStudentTermsId1, knownStudentTermsId2, knownStudentTermsId3 };
                var studentTermsFiltered = studentTerms.Where(i => studentIds.Contains(i.Recordkey));
                dataReaderMock.Setup(i => i.ReadRecordAsync<Students>("STUDENTS", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new Students()
                { Recordkey = "1", StuTerms = new List<string>() { "2015/FA", "2015/SP", "2015/FA" }, StuAcadLevels = new List<string>() { "UG" } });
                dataReaderMock.Setup(i => i.SelectAsync("STUDENT.TERMS", It.IsAny<string>())).ReturnsAsync(studentIds.ToArray());
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<StudentTerms>("STUDENT.TERMS", It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<StudentTerms>(studentTermsFiltered.ToList()));

                var actuals = await studentTermRepo.GetStudentTermsAsync(0, 3, false, "1", "1");
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals.Item1)
                {
                    var expected = studentTermsFiltered.FirstOrDefault(i => i.RecordGuid.Equals(actual.Guid));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.RecordGuid, actual.Guid);
                    Assert.AreEqual(expected.Recordkey, string.Concat(actual.StudentId, "*", actual.Term, "*", actual.AcademicLevel));
                }
            }

            [TestMethod]
            public async Task StudentTermRepository_GetStudentTermByIdAsync()
            {
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("STUDENT.TERMS", new GuidLookupResult() { Entity = "STUDENT.TERMS", PrimaryKey = knownStudentTermsId1, SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

                dataReaderMock.Setup(i => i.ReadRecordAsync<StudentTerms>("STUDENT.TERMS", It.IsAny<string>(), true)).ReturnsAsync(studentTerms.First());


                var actual = await studentTermRepo.GetStudentTermByGuidAsync(guid);
                Assert.IsNotNull(actual);

                var expected = studentTerms.First();
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.RecordGuid, actual.Guid);
                Assert.AreEqual(expected.Recordkey, string.Concat(actual.StudentId, "*", actual.Term, "*", actual.AcademicLevel));
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task StudentTermRepository_GetStudentTermsAsync_RepositoryException()
            {
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<StudentTerms>("STUDENT.TERMS", It.IsAny<string[]>(), true)).ThrowsAsync(new RepositoryException());

                var actuals = await studentTermRepo.GetStudentTermsAsync(0, 3, false, "", "");               
            }

            [TestMethod]
            public async Task StudentTermRepository_GetStudentTermByIdAsync_Exception()
            {
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("STUDENT.TERMS", new GuidLookupResult() { Entity = "STUDENT.TERMS", PrimaryKey = knownStudentTermsId1, SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

                dataReaderMock.Setup(i => i.ReadRecordAsync<StudentTerms>("STUDENT.TERMS", It.IsAny<string>(), true)).ThrowsAsync(new Exception());
                var actual = await studentTermRepo.GetStudentTermByGuidAsync(guid);                
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentTermRepository_GetStudentTermByIdAsync_KeyNotFoundException()
            {
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("STUDENT.TERMS", new GuidLookupResult() { Entity = "STUDENT.TERMS", PrimaryKey = knownStudentTermsId1, SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(null);

                dataReaderMock.Setup(i => i.ReadRecordAsync<StudentTerms>("STUDENT.TERMS", It.IsAny<string>(), true)).ReturnsAsync(studentTerms.First());

                var actual = await studentTermRepo.GetStudentTermByGuidAsync(guid);
               
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentTermRepository_GetStudentTermByIdAsync_NullDictionary()
            {
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("STUDENT.TERMS", null);
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

                dataReaderMock.Setup(i => i.ReadRecordAsync<StudentTerms>("STUDENT.TERMS", It.IsAny<string>(), true)).ReturnsAsync(studentTerms.First());

                var actual = await studentTermRepo.GetStudentTermByGuidAsync(guid);               
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task StudentTermRepository_GetStudentTermByIdAsync_EntityMismatch_RepositoryException()
            {
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("STUDENT.TERMS", new GuidLookupResult() { Entity = "STUDENT.TERM", PrimaryKey = knownStudentTermsId1, SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

                dataReaderMock.Setup(i => i.ReadRecordAsync<StudentTerms>("STUDENT.TERMS", It.IsAny<string>(), true)).ReturnsAsync(studentTerms.First());
                var actual = await studentTermRepo.GetStudentTermByGuidAsync(guid);               
            }
        }
    }
}
