using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
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
    public class EducationHistoryRepositoryTests
    {
        protected List<string> studentIds;
        protected List<string> personIds;
        protected List<string> institutionIds;
        protected List<string> acadCredentialIds;
        protected Dictionary<string, Person> personRecords;
        protected Dictionary<string, InstitutionsAttend> institutionsAttendRecords;
        protected Dictionary<string, Institutions> institutionRecords;
        protected Dictionary<string, AcadCredentials> acadCredentialsRecords;
        Collection<Person> personResponseData;
        Collection<InstitutionsAttend> institutionsAttendResponseData;
        Collection<Institutions> institutionsResponseData;
        Collection<AcadCredentials> acadCredentialsResponseData;
        EducationHistoryRepository educationHistoryRepo;

        #region Private data array setup

        private string[,] _personData = {
                                       {"0000304", "0000604,0000704", ""},
                                       {"0000404", "0000604,0000704", ""},
                                       {"0000504", "0000604", ""},
                                       {"0000604", "", "Huntington Beach High School"},
                                       {"0000704", "", "Gem City Business College"}
                                   };

        private string[,] _institutionsAttendData = {
                                       {"001", "0000304", "0000604", "2004-01-02", "2008-06-05", "2004", "2008", "D", "3.04", "42","1", "2008-06-05","Graduated with Honors"},
                                       {"002", "0000304", "0000704", "2012-10-15", "2013-05-23", "2012", "2013", "", "3.02", "36", "2,3", "2013-05-31",""},
                                       {"003", "0000404", "0000604", "2004-01-11", "2008-05-22", "2004", "2008", "CHP", "", "48", "", "",""},
                                       {"004", "0000404", "0000704", "2009-01-27", "2009-05-27", "2009", "2009", "", "", "52", "4", "2009-05-31","Graduated with Honors"},
                                       {"005", "0000504", "0000604", "2010-09-13", "2014-05-21", "2010", "2014", "D", "", "42", "", "",""},
                                   };

        private string[,] _institutionsData = {
                                       {"0000604", "H"},
                                       {"0000704", "C"}
                                   };

        private string[,] _acadCredentialsData = {
                                       {"1", "0000304", "0000604", "", "", "", "", "", "", "","","", "Honors"},
                                       {"2", "0000304", "0000704", "BA", "2013-05-23", "2013-07-01", "2014-06-30", "ACL", "ENGL,HIST", "MATH", "EMT", "CCL,EML", "Honors"},
                                       {"3", "0000304", "0000704", "BS", "2013-05-23", "", "", "", "", "", "", "", ""},
                                       {"4", "0000404", "0000704", "BA", "", "", "", "", "", "", "", "", "National Science"}
                                   };

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            institutionsAttendRecords = SetupInstitutionsAttend(out studentIds);
            personRecords = SetupPersons(out personIds);
            institutionRecords = SetupInstitutions(out institutionIds);
            acadCredentialsRecords = SetupAcadCredentials(out acadCredentialIds);
            educationHistoryRepo = BuildValidRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            educationHistoryRepo = null;
        }

        [TestMethod]
        public async Task CheckSingleEducationHistoryProperties_Valid()
        {
            IEnumerable<EducationHistory> educationHistoryEntities = await educationHistoryRepo.GetAsync(studentIds);
            EducationHistory educationHistory = educationHistoryEntities.ElementAt(0);
            Assert.AreEqual(studentIds.ElementAt(0), educationHistory.Id);
        }

        [TestMethod]
        public async Task CheckSingleHighSchoolProperties_Valid()
        {
            IEnumerable<EducationHistory> educationHistoryEntities = await educationHistoryRepo.GetAsync(studentIds);
            EducationHistory educationHistory = educationHistoryEntities.ElementAt(0);

            HighSchool highSchoolEntity = educationHistory.HighSchools.ElementAt(0);
            Assert.AreEqual("0000604", highSchoolEntity.HighSchoolId);
            Assert.AreEqual("Huntington Beach High School", highSchoolEntity.HighSchoolName);
            Assert.AreEqual(decimal.Parse("3.04"), highSchoolEntity.Gpa);
            Assert.AreEqual("Diploma", highSchoolEntity.GraduationType);
            Assert.AreEqual("2008", highSchoolEntity.LastAttendedYear);
            Assert.AreEqual(decimal.Parse("42"), highSchoolEntity.SummaryCredits);
            Assert.AreEqual(DateTime.Parse("2008-06-05"), highSchoolEntity.CredentialsEndDate);
            Assert.AreEqual("Graduated with Honors", highSchoolEntity.Comments);
        }
        [TestMethod]
        public async Task CheckSingleCollegeProperties_Valid()
        {
            IEnumerable<EducationHistory> educationHistoryEntities =await educationHistoryRepo.GetAsync(studentIds);
            EducationHistory educationHistory = educationHistoryEntities.ElementAt(0);

            College collegeEntity = educationHistory.Colleges.ElementAt(0);
            Assert.AreEqual("0000704", collegeEntity.CollegeId);
            Assert.AreEqual("Gem City Business College", collegeEntity.CollegeName);
            Assert.AreEqual(decimal.Parse("3.02"), collegeEntity.Gpa);
            Assert.AreEqual("2013", collegeEntity.LastAttendedYear);
            Assert.AreEqual(decimal.Parse("36"), collegeEntity.SummaryCredits);
            Assert.AreEqual(DateTime.Parse("2013-05-31"), collegeEntity.CredentialsEndDate);
        }

        [TestMethod]
        public async Task CheckSingleCredentialProperties_Valid()
        {
            IEnumerable<EducationHistory> educationHistoryEntities = await educationHistoryRepo.GetAsync(studentIds);
            EducationHistory educationHistory = educationHistoryEntities.ElementAt(0);

            College collegeEntity = educationHistory.Colleges.ElementAt(0);
            Credential credentialEntity = collegeEntity.Credentials.ElementAt(0);
            // todo srm 03/17/2014: Degree isn't found because we aren't reading in
            // the code tables.  They are defined in TestEducationHistoryRepository.cs
            // Assert.AreEqual("Bachelor of Art", credentialEntity.Degree);
            Assert.AreEqual(DateTime.Parse("2013-05-23"), credentialEntity.DegreeDate);
            Assert.AreEqual(DateTime.Parse("2013-07-01"), credentialEntity.StartDate);
            Assert.AreEqual(DateTime.Parse("2014-06-30"), credentialEntity.EndDate);
        }

        [TestMethod]
        public async Task MultiEducationHistoryCount_Valid()
        {
            IEnumerable<EducationHistory> educationHistoryEntities = await educationHistoryRepo.GetAsync(studentIds);
            Assert.AreEqual(5, educationHistoryEntities.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NewEducationHistory_NullStudentId()
        {
            EducationHistory educationHistory = new EducationHistory(null);
        }

        private EducationHistoryRepository BuildValidRepository()
        {
            var transFactoryMock = new Mock<IColleagueTransactionFactory>();

            var loggerMock = new Mock<ILogger>();

            // Cache mocking
            var cacheProviderMock = new Mock<ICacheProvider>();
            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            var localCacheMock = new Mock<ObjectCache>();
            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
            x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
            .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                null,
                new SemaphoreSlim(1, 1)
        )));

            //cacheProviderMock.Setup(provider => provider.GetCache(It.IsAny<string>())).Returns(localCacheMock.Object);

            // Set up data accessor for mocking 
            var dataAccessorMock = new Mock<IColleagueDataReader>();
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

            // Set up Person Response
            personResponseData = BuildPersonResponseData(personRecords);
            Collection<Person> subsetResponse = new Collection<Person>();
            subsetResponse.Add(personResponseData[0]);
            subsetResponse.Add(personResponseData[1]);
            subsetResponse.Add(personResponseData[2]);
            //dataAccessorMock.Setup<Collection<Person>>(acc => acc.BulkReadRecord<Person>(studentIds.ToArray(), true)).Returns(subsetResponse);
            dataAccessorMock.Setup<Task<Collection<Person>>>(acc => acc.BulkReadRecordAsync<Person>(It.IsAny<string[]>(), true)).Returns(Task.FromResult(personResponseData));

            // Set up InstitutionsAttend Response
            institutionsAttendResponseData = BuildInstitutionsAttendResponse(institutionsAttendRecords);
            string[] institutionsAttendIds = { "0000304*0000604", "0000304*0000704", "0000404*0000604", "0000404*0000704", "0000504*0000604" };
            //dataAccessorMock.Setup<Collection<InstitutionsAttend>>(acc => acc.BulkReadRecord<InstitutionsAttend>(institutionsAttendIds, true)).Returns(institutionsAttendResponseData);
            dataAccessorMock.Setup<Task<Collection<InstitutionsAttend>>>(acc => acc.BulkReadRecordAsync<InstitutionsAttend>(It.IsAny<string[]>(), true)).Returns(Task.FromResult(institutionsAttendResponseData));

            // Set up Institutions Response
            institutionsResponseData = BuildInstitutionsResponseData(institutionRecords);
            //dataAccessorMock.Setup<Collection<Institutions>>(acc => acc.BulkReadRecord<Institutions>(institutionIds.ToArray(), true)).Returns(institutionsResponseData);
            dataAccessorMock.Setup<Task<Collection<Institutions>>>(acc => acc.BulkReadRecordAsync<Institutions>(It.IsAny<string[]>(), true)).Returns(Task.FromResult(institutionsResponseData));
            Collection<Person> instSubsetResponse = new Collection<Person>();
            instSubsetResponse.Add(personResponseData[3]);
            instSubsetResponse.Add(personResponseData[4]);
            dataAccessorMock.Setup<Task<Collection<Person>>>(acc => acc.BulkReadRecordAsync<Person>(personResponseData.SelectMany(p => p.PersonInstitutionsAttend).ToArray(), true)).Returns(Task.FromResult(instSubsetResponse));
            //dataAccessorMock.Setup<Collection<Person>>(acc => acc.BulkReadRecord<Person>(It.IsAny<string[]>(), true)).Returns(instSubsetResponse);

            // Set up AcadCredentials Response
            acadCredentialsResponseData = BuildAcadCredentialsResponseData(acadCredentialsRecords);
            //dataAccessorMock.Setup<Collection<AcadCredentials>>(acc => acc.BulkReadRecord<AcadCredentials>(acadCredentialIds.ToArray(), true)).Returns(acadCredentialsResponseData);
            dataAccessorMock.Setup<Task<Collection<AcadCredentials>>>(acc => acc.BulkReadRecordAsync<AcadCredentials>(It.IsAny<string[]>(), true)).Returns(Task.FromResult(acadCredentialsResponseData));

            // mock data accessor INSTITUTION.TYPES
            dataAccessorMock.Setup<Task<ApplValcodes>>(a =>
                a.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INST.TYPES", true))
                .Returns(Task.FromResult(new ApplValcodes()
                {
                    ValInternalCode = new List<string>() { "H", "C" },
                    ValExternalRepresentation = new List<string>() { "High School", "College" },
                    ValsEntityAssociation = new List<ApplValcodesVals>()
                    {
                        new ApplValcodesVals() 
                        {
                            ValInternalCodeAssocMember = "H",
                            ValExternalRepresentationAssocMember = "High School",
                            ValActionCode1AssocMember = "H"
                        },
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "C",
                            ValExternalRepresentationAssocMember = "College",
                            ValActionCode1AssocMember = "C"
                        }
                    }
                }));

            // mock data accessor GRADUATION.TYPES
            dataAccessorMock.Setup<Task<ApplValcodes>>(a =>
                a.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "GRADUATION.TYPES", true))
                .Returns(Task.FromResult(new ApplValcodes()
                {
                    ValInternalCode = new List<string>() { "D", "CHP" },
                    ValExternalRepresentation = new List<string>() { "Diploma", "CA. High School Prof" },
                    ValsEntityAssociation = new List<ApplValcodesVals>()
                    {
                        new ApplValcodesVals() 
                        {
                            ValInternalCodeAssocMember = "D",
                            ValExternalRepresentationAssocMember = "Diploma"
                        },
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "CHP",
                            ValExternalRepresentationAssocMember = "CA. High School Prof"
                        }
                    }
                }));

            // mock Other Honors Code table
            dataAccessorMock.Setup<Collection<OtherHonors>>(c =>
                c.BulkReadRecord<OtherHonors>("OTHER.HONORS", "", true))
                .Returns(new Collection<OtherHonors>()
                {
                    new OtherHonors() { Recordkey = "EML", OhonDesc = "Eml" },
                    new OtherHonors() { Recordkey = "CCL", OhonDesc = "Ccl" }
                });

            // mock Other CCDs Code table
            dataAccessorMock.Setup<Collection<OtherCcds>>(c =>
                c.BulkReadRecord<OtherCcds>("OTHER.CCDS", "", true))
                .Returns(new Collection<OtherCcds>()
                {
                    new OtherCcds() { Recordkey = "ACL", OccdDesc = "ACL" },
                    new OtherCcds() { Recordkey = "CCL", OccdDesc = "Ccl" }
                });

            // mock Other Majors Code table
            dataAccessorMock.Setup<Collection<OtherMajors>>(c =>
                c.BulkReadRecord<OtherMajors>("OTHER.MAJORS", "", true))
                .Returns(new Collection<OtherMajors>()
                {
                    new OtherMajors() { Recordkey = "MATH", OmajDesc = "Math" },
                    new OtherMajors() { Recordkey = "HIST", OmajDesc = "History" },
                    new OtherMajors() { Recordkey = "ENGL", OmajDesc = "English" }
                });

            // mock Other Minors Code table
            dataAccessorMock.Setup<Collection<OtherMinors>>(c =>
                c.BulkReadRecord<OtherMinors>("OTHER.MINORS", "", true))
                .Returns(new Collection<OtherMinors>()
                {
                    new OtherMinors() { Recordkey = "MATH", OminDesc = "Math" },
                    new OtherMinors() { Recordkey = "HIST", OminDesc = "History" },
                    new OtherMinors() { Recordkey = "ENGL", OminDesc = "English" }
                });

            // mock Other Specializations Code table
            dataAccessorMock.Setup<Collection<OtherSpecials>>(c =>
                c.BulkReadRecord<OtherSpecials>("OTHER.SPECIALS", "", true))
                .Returns(new Collection<OtherSpecials>()
                {
                    new OtherSpecials() { Recordkey = "EMT", OspecDesc = "Emergency Medical Technician" }
                });

            // mock Other Degrees Code table
            dataAccessorMock.Setup<Collection<OtherDegrees>>(c =>
                c.BulkReadRecord<OtherDegrees>("OTHER.DEGREES", "", true))
                .Returns(new Collection<OtherDegrees>()
                {
                    new OtherDegrees() { Recordkey = "BA", OdegDesc = "Bachelor of Arts" },
                    new OtherDegrees() { Recordkey = "BS", OdegDesc = "Bachelor of Science" },
                    new OtherDegrees() { Recordkey = "MA", OdegDesc = "Master of Arts" }
                });

            // Construct EducationHistory repository
            educationHistoryRepo = new EducationHistoryRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            return educationHistoryRepo;
        }

        private Dictionary<string, Person> SetupPersons(out List<string> personIds)
        {
            string[,] recordData = _personData;

            personIds = new List<string>();
            int recordCount = recordData.Length / 3;
            Dictionary<string, Person> records = new Dictionary<string, Person>();
            for (int i = 0; i < recordCount; i++)
            {
                string id = recordData[i, 0].TrimEnd();
                List<string> institutionsAttend = (string.IsNullOrEmpty(recordData[i, 1])) ? new List<string>() : recordData[i, 1].TrimEnd().Split(',').ToList<string>();
                string preferredName = (recordData[i, 2] == null) ? null : recordData[i, 2].TrimEnd();

                Person record = new Person();
                record.Recordkey = id;
                record.PersonInstitutionsAttend = institutionsAttend;
                record.PreferredName = preferredName;

                personIds.Add(id);
                records.Add(id, record);
            }
            return records;
        }

        private Collection<Person> BuildPersonResponseData(Dictionary<string, Person> personRecords)
        {
            Collection<Person> personContracts = new Collection<Person>();
            foreach (var personItem in personRecords)
            {
                personContracts.Add(personItem.Value);
            }
            return personContracts;
        }

        private Dictionary<string, InstitutionsAttend> SetupInstitutionsAttend(out List<string> ids)
        {
            ids = new List<string>();
            string[,] recordData = _institutionsAttendData;

            int recordCount = recordData.Length / 13;
            Dictionary<string, InstitutionsAttend> results = new Dictionary<string, InstitutionsAttend>();
            for (int i = 0; i < recordCount; i++)
            {
                string key = recordData[i, 0].TrimEnd();
                string studentId = recordData[i, 1].TrimEnd();
                string instId = recordData[i, 2].TrimEnd();
                List<string> startDates = (string.IsNullOrEmpty(recordData[i, 3])) ? new List<string>() : recordData[i, 3].TrimEnd().Split(',').ToList();
                List<string> endDates = (string.IsNullOrEmpty(recordData[i, 4])) ? new List<string>() : recordData[i, 4].TrimEnd().Split(',').ToList();
                List<string> startYears = (string.IsNullOrEmpty(recordData[i, 5])) ? new List<string>() : recordData[i, 5].TrimEnd().Split(',').ToList();
                List<string> endYears = (string.IsNullOrEmpty(recordData[i, 6])) ? new List<string>() : recordData[i, 6].TrimEnd().Split(',').ToList();
                string gradType = (recordData[i, 7] == null) ? null : recordData[i, 7].TrimEnd();
                Decimal? gpa = (string.IsNullOrEmpty(recordData[i, 8])) ? Decimal.Parse("0") : Decimal.Parse(recordData[i, 8].TrimEnd());
                Decimal? credits = (string.IsNullOrEmpty(recordData[i, 9])) ? Decimal.Parse("0") : Decimal.Parse(recordData[i, 9].TrimEnd());
                List<string> credentialsIds = (string.IsNullOrEmpty(recordData[i, 10])) ? new List<string>() : recordData[i, 10].TrimEnd().Split(',').ToList();
                DateTime? credentialsEndDate = (string.IsNullOrEmpty(recordData[i, 11])) ? new DateTime?() : DateTime.Parse(recordData[i, 11].TrimEnd());
                string comments = (recordData[i, 12] == null) ? string.Empty : recordData[i, 12].TrimEnd();

                List<DateTime?> convertedStartDates = new List<DateTime?>();
                foreach (var date in startDates) { convertedStartDates.Add(DateTime.Parse(date)); }
                List<DateTime?> convertedEndDates = new List<DateTime?>();
                foreach (var date in endDates) { convertedEndDates.Add(DateTime.Parse(date)); }
                List<int?> convertedStartYears = new List<int?>();
                foreach (var year in startYears) { convertedStartYears.Add(int.Parse(year)); }


                InstitutionsAttend response = new InstitutionsAttend();
                response.Recordkey = studentId + "*" + instId;
                response.InstaStartDates = convertedStartDates;
                response.InstaEndDates = convertedEndDates;
                response.InstaYearAttendStart = convertedStartYears;
                response.InstaYearAttendEnd = endYears;
                response.InstaGradType = gradType;
                response.InstaExtGpa = gpa;
                response.InstaExtCredits = credits;
                response.InstaAcadCredentials = credentialsIds;
                response.InstaCredentialsEndDate = credentialsEndDate;
                response.InstaComments = comments;
                response.buildAssociations();

                if (ids.Where(id => id.Equals(studentId)).Count() == 0)
                {
                    ids.Add(studentId);
                }
                results.Add(key, response);
            }
            return results;
        }

        private Collection<InstitutionsAttend> BuildInstitutionsAttendResponse(Dictionary<string, InstitutionsAttend> institutionsAttendRecords)
        {
            Collection<InstitutionsAttend> institutionsAttendContracts = new Collection<InstitutionsAttend>();
            foreach (var institutionsAttendItem in institutionsAttendRecords)
            {
                institutionsAttendContracts.Add(institutionsAttendItem.Value);
            }
            return institutionsAttendContracts;
        }

        private Dictionary<string, Institutions> SetupInstitutions(out List<string> institutionIds)
        {
            string[,] recordData = _institutionsData;

            institutionIds = new List<string>();
            int institutionsCount = recordData.Length / 2;
            Dictionary<string, Institutions> records = new Dictionary<string, Institutions>();
            for (int i = 0; i < institutionsCount; i++)
            {
                string institutionId = recordData[i, 0].TrimEnd();
                string instType = (recordData[i, 1] == null) ? String.Empty : recordData[i, 1].TrimEnd();

                Institutions record = new Institutions();
                record.Recordkey = institutionId;
                record.InstType = instType;

                if (institutionIds.Where(id => id.Equals(institutionId)).Count() == 0)
                {
                    institutionIds.Add(institutionId);
                }
                records.Add(institutionId, record);
            }
            return records;
        }

        private Collection<Institutions> BuildInstitutionsResponseData(Dictionary<string, Institutions> institutionRecords)
        {
            Collection<Institutions> institutionContracts = new Collection<Institutions>();
            foreach (var institutionItem in institutionRecords)
            {
                institutionContracts.Add(institutionItem.Value);
            }
            return institutionContracts;
        }

        private Dictionary<string, AcadCredentials> SetupAcadCredentials(out List<string> acadCredentialsIds)
        {
            string[,] recordData = _acadCredentialsData;

            acadCredentialsIds = new List<string>();
            int institutionsCount = recordData.Length / 13;
            Dictionary<string, AcadCredentials> records = new Dictionary<string, AcadCredentials>();
            for (int i = 0; i < institutionsCount; i++)
            {
                string key = recordData[i, 0].TrimEnd();
                string personId = recordData[i, 1].TrimEnd();
                string instId = recordData[i, 2].TrimEnd();
                string degree = (recordData[i, 3] == null) ? String.Empty : recordData[i, 3].TrimEnd();
                DateTime? degreeDate = (string.IsNullOrEmpty(recordData[i, 4])) ? new DateTime?() : DateTime.Parse(recordData[i, 4].TrimEnd());
                DateTime? startDate = (string.IsNullOrEmpty(recordData[i, 5])) ? new DateTime?() : DateTime.Parse(recordData[i, 5].TrimEnd());
                DateTime? endDate = (string.IsNullOrEmpty(recordData[i, 6])) ? new DateTime?() : DateTime.Parse(recordData[i, 6].TrimEnd());
                List<string> ccds = (string.IsNullOrEmpty(recordData[i, 7])) ? new List<string>() : recordData[i, 7].TrimEnd().Split(',').ToList();
                List<string> majors = (string.IsNullOrEmpty(recordData[i, 8])) ? new List<string>() : recordData[i, 8].TrimEnd().Split(',').ToList();
                List<string> minors = (string.IsNullOrEmpty(recordData[i, 9])) ? new List<string>() : recordData[i, 9].TrimEnd().Split(',').ToList();
                List<string> specializations = (string.IsNullOrEmpty(recordData[i, 10])) ? new List<string>() : recordData[i, 10].TrimEnd().Split(',').ToList();
                List<string> honors = (string.IsNullOrEmpty(recordData[i, 11])) ? new List<string>() : recordData[i, 11].TrimEnd().Split(',').ToList();
                List<string> awards = (string.IsNullOrEmpty(recordData[i, 12])) ? new List<string>() : recordData[i, 12].TrimEnd().Split(',').ToList();

                AcadCredentials record = new AcadCredentials();
                record.Recordkey = key;
                record.AcadDegree = degree;
                record.AcadDegreeDate = degreeDate;
                record.AcadStartDate = startDate;
                record.AcadEndDate = endDate;
                record.AcadCcd = ccds;
                record.AcadMajors = majors;
                record.AcadMinors = minors;
                record.AcadSpecialization = specializations;
                record.AcadHonors = honors;
                record.AcadAwards = awards;

                if (acadCredentialsIds.Where(id => id.Equals(key)).Count() == 0)
                {
                    acadCredentialsIds.Add(key);
                }
                records.Add(key, record);
            }
            return records;
        }

        private Collection<AcadCredentials> BuildAcadCredentialsResponseData(Dictionary<string, AcadCredentials> acadCredentialsRecords)
        {
            Collection<AcadCredentials> acadCredentialsContracts = new Collection<AcadCredentials>();
            foreach (var institutionItem in acadCredentialsRecords)
            {
                acadCredentialsContracts.Add(institutionItem.Value);
            }
            return acadCredentialsContracts;
        }
    }
}
