using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
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
using System.Text;
using System.Threading.Tasks;
using Moq.Protected;
using System.Linq.Expressions;
using System.Threading;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentAffiliationRepositoryTests
    {
        protected List<string> studentIds;
        protected List<string> campusOrgIds;
        protected List<string> campusOrgMemberIds;
        protected Dictionary<string, PersonSt> personStRecords;
        protected Dictionary<string, CampusOrgs> campusOrgRecords;
        protected Dictionary<string, CampusOrgMembers> campusOrgMemberRecords;
        Collection<PersonSt> personStResponseData;
        Collection<CampusOrgs> campusOrgResponseData;
        Collection<CampusOrgMembers> campusOrgMemberResponseData;
        StudentAffiliationRepository studentAffiliationRepo;

        #region Private data array setup

        private string[,] _personStData = {
                                       {"0000304", "CH,FB", "Chess,Football"},
                                       {"0000404", "FB,SC", "Football,Soccer"},
                                       {"0000504", "CH", "Chess"},
                                       {"0000604", "FB", "Football"},
                                       {"0000704", "SC", "Soccer"}
                                   };

        private string[,] _campusOrgData = {
                                       {"CH", "SOC"},
                                       {"FB", "ATH"},
                                       {"SC", "ATH"}
                                   };

        private string[,] _campusOrgMemberData = {
                                       {"CH", "0000304", "PL", "2015-01-01", "", "A"},
                                       {"FB", "0000304", "PL", "2015-01-01", "", "A"},
                                       {"FB", "0000404", "PL,CA", "2015-01-01,2015-04-21", "2015-05-21", "I,A"},
                                       {"SC", "0000404", "CA", "2015-01-01", "", "A"},
                                       {"CH", "0000504", "MB", "2015-01-01", "", "A"},
                                       {"FB", "0000604", "PL", "2015-01-01", "", "A"},
                                       {"SC", "0000604", "PL,CP", "2015-01-01,2015-04-21", "2015-05-21", "I,A"},
                                       {"FB", "0000704", "PL,CP", "2015-01-01,2015-02-21", "2015-05-21", "I,A"},
                                       {"SC", "0000704", "PL", "2015-01-01", "", "A"}
                                   };

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            personStRecords = SetupPersonSt(out studentIds);
            campusOrgRecords = SetupCampusOrgs(out campusOrgIds);
            campusOrgMemberRecords = SetupCampusOrgMembers(out campusOrgMemberIds);
            studentAffiliationRepo = BuildValidRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentAffiliationRepo = null;
        }

        [TestMethod]
        public async Task CheckSingleStudentAffiliationProperties_Valid()
        {
            var termData = new Domain.Student.Entities.Term("2015/SP", "Spring 2015 Term", DateTime.Parse("2015-01-13"), DateTime.Parse("2015-04-12"), 2015, 1, false, false, "2015/SP", false);

            IEnumerable<StudentAffiliation> studentAffiliationEntities = await studentAffiliationRepo.GetStudentAffiliationsByStudentIdsAsync(studentIds, termData, "");
            StudentAffiliation studentAffiliation = studentAffiliationEntities.ElementAt(0);
            Assert.AreEqual(studentIds.ElementAt(0), studentAffiliation.StudentId);
        }

        [TestMethod]
        public async Task CheckMultiStudentAffiliationCount_Valid()
        {
            var termData = new Domain.Student.Entities.Term(
                "2015/SP", "Spring 2015 Term", DateTime.Parse("2015-01-13"),
                DateTime.Parse("2015-04-12"), 2015, 1, false, false, "2015/SP", false);
            IEnumerable<StudentAffiliation> studentAffiliationEntities =
                await studentAffiliationRepo.GetStudentAffiliationsByStudentIdsAsync(studentIds, termData, "");

            Assert.AreEqual(8, studentAffiliationEntities.Count());
        }

        [TestMethod]
        public async Task FindStudentAffiliationRoleForTerm()
        {
            var termData = new Domain.Student.Entities.Term("2015/S2", "Summer II 2015 Term", DateTime.Parse("2015-06-13"), DateTime.Parse("2015-10-12"), 2015, 1, false, false, "2015/SU", false);
            IEnumerable<StudentAffiliation> studentAffiliationEntities = await studentAffiliationRepo.GetStudentAffiliationsByStudentIdsAsync(studentIds, termData, "");
            // Confirm that repository returns expected role when multiple roles exists for the
            // student and campus org, but only one intersects with the term.
            Assert.AreEqual("0000604", studentAffiliationEntities.ElementAt(4).StudentId);
            Assert.AreEqual("SC", studentAffiliationEntities.ElementAt(4).AffiliationCode);
            Assert.AreEqual("CP", studentAffiliationEntities.ElementAt(4).RoleCode);
            Assert.AreEqual("A", studentAffiliationEntities.ElementAt(4).StatusCode);

        }

        [TestMethod]
        public async Task FindStudentAffiliationRoleForTermByPriority()
        {
            var termData = new Domain.Student.Entities.Term("2015/SP", "Spring 2015 Term", DateTime.Parse("2015-01-13"), DateTime.Parse("2015-04-12"), 2015, 1, false, false, "2015/SP", false);
            IEnumerable<StudentAffiliation> studentAffiliationEntities = await studentAffiliationRepo.GetStudentAffiliationsByStudentIdsAsync(studentIds, termData, "");
            // Confirm that repository returns expected role when multiple roles exists for the
            // student and campus org, and multiple roles intersect with the term; so 
            // role with higher priority is returned.
            Assert.AreEqual("0000704", studentAffiliationEntities.ElementAt(6).StudentId);
            Assert.AreEqual("FB", studentAffiliationEntities.ElementAt(6).AffiliationCode);
            Assert.AreEqual("PL", studentAffiliationEntities.ElementAt(6).RoleCode);
            Assert.AreEqual("I", studentAffiliationEntities.ElementAt(6).StatusCode);

        }


        private StudentAffiliationRepository BuildValidRepository()
        {
            var transFactoryMock = new Mock<IColleagueTransactionFactory>();

            var loggerMock = new Mock<ILogger>();

            // Cache mocking
            var cacheProviderMock = new Mock<ICacheProvider>(MockBehavior.Loose);
            var localCacheMock = new Mock<ObjectCache>(MockBehavior.Loose);
            // Set up data accessor for mocking 
            var dataAccessorMock = new Mock<IColleagueDataReader>(MockBehavior.Loose);

            // Set up PersonSt Response
            personStResponseData = BuildPersonStResponseData(personStRecords);
            dataAccessorMock.Setup<Task<Collection<PersonSt>>>(acc => acc.BulkReadRecordAsync<PersonSt>(It.IsAny<string[]>(), true)).Returns(Task.FromResult(personStResponseData));

            // Set up CampusOrgs Response
            campusOrgResponseData = BuildCampusOrgsResponseData(campusOrgRecords);
            dataAccessorMock.Setup<Task<Collection<CampusOrgs>>>(acc => acc.BulkReadRecordAsync<CampusOrgs>(It.IsAny<string[]>(), true)).Returns(Task.FromResult(campusOrgResponseData));

            // Set up CampusOrgMembers Response
            campusOrgMemberResponseData = BuildCampusOrgMembersResponseData(campusOrgMemberRecords);
            dataAccessorMock.Setup<Task<Collection<CampusOrgMembers>>>(acc => acc.BulkReadRecordAsync<CampusOrgMembers>(It.IsAny<string[]>(), true)).Returns(Task.FromResult(campusOrgMemberResponseData));

            // mock data accessor CAMPUS.ORG.MEMBER.STATUSES
            dataAccessorMock.Setup<Task<ApplValcodes>>(a =>
                a.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "CAMPUS.ORG.MEMBER.STATUSES", true))
                .Returns(Task.FromResult(new ApplValcodes()
                {
                    ValInternalCode = new List<string>() { "A", "I" },
                    ValExternalRepresentation = new List<string>() { "Active", "Inactive" },
                    ValsEntityAssociation = new List<ApplValcodesVals>()
                    {
                        new ApplValcodesVals() 
                        {
                            ValInternalCodeAssocMember = "A",
                            ValExternalRepresentationAssocMember = "Active"
                        },
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "I",
                            ValExternalRepresentationAssocMember = "Inactive"
                        }
                    }
                }));


            // mock OrgRoles Code table
            dataAccessorMock.Setup<Task<Collection<Roles>>>(c =>
                c.BulkReadRecordAsync<Roles>("ROLES", "", true))
                .Returns(Task.FromResult(new Collection<Roles>()
                {
                    new Roles() { Recordkey = "PL", RolesDesc = "Player",   RolesPilotPriority = "1" },
                    new Roles() { Recordkey = "CP", RolesDesc = "Captain",  RolesPilotPriority = "2" },
                    new Roles() { Recordkey = "CO", RolesDesc = "Coach",    RolesPilotPriority = "2" },
                    new Roles() { Recordkey = "MB", RolesDesc = "Member",   RolesPilotPriority = "2" }
                }));

            // mock OrgTypes Code table
            dataAccessorMock.Setup<Task<Collection<OrgTypes>>>(c =>
                c.BulkReadRecordAsync<OrgTypes>("ORG.TYPES", "", true))
                .Returns(Task.FromResult(new Collection<OrgTypes>()
                {
                    new OrgTypes() { Recordkey = "ATH", OrgtDesc = "Athletics", OrgtPilotFlag = "Y" },
                    new OrgTypes() { Recordkey = "SOC", OrgtDesc = "Social Club", OrgtPilotFlag = "N" }
                }));

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                    null,
                    new SemaphoreSlim(1, 1)
                    )));

            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

            // Construct StudentAffiliation repository
            studentAffiliationRepo = new StudentAffiliationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            return studentAffiliationRepo;
        }

        private Dictionary<string, PersonSt> SetupPersonSt(out List<string> studentIds)
        {
            string[,] recordData = _personStData;

            studentIds = new List<string>();
            int recordCount = recordData.Length / 3;
            Dictionary<string, PersonSt> records = new Dictionary<string, PersonSt>();
            for (int i = 0; i < recordCount; i++)
            {
                string id = recordData[i, 0].TrimEnd();
                List<string> orgIdList = (string.IsNullOrEmpty(recordData[i, 1])) ? new List<string>() : recordData[i, 1].TrimEnd().Split(',').ToList();

                PersonSt record = new PersonSt();
                record.Recordkey = id;
                record.PstCampusOrgsMember = orgIdList;

                studentIds.Add(id);
                records.Add(id, record);
            }
            return records;
        }

        private Collection<PersonSt> BuildPersonStResponseData(Dictionary<string, PersonSt> personRecords)
        {
            Collection<PersonSt> personContracts = new Collection<PersonSt>();
            foreach (var personItem in personRecords)
            {
                personContracts.Add(personItem.Value);
            }
            return personContracts;
        }

        private Dictionary<string, CampusOrgs> SetupCampusOrgs(out List<string> ids)
        {
            ids = new List<string>();
            string[,] recordData = _campusOrgData;

            int recordCount = recordData.Length / 2;
            Dictionary<string, CampusOrgs> results = new Dictionary<string, CampusOrgs>();
            for (int i = 0; i < recordCount; i++)
            {
                string key = recordData[i, 0].TrimEnd();
                string orgType = recordData[i, 1].TrimEnd();

                CampusOrgs response = new CampusOrgs();
                response.Recordkey = key;
                response.CmpOrgType = orgType;

                if (ids.Where(id => id.Equals(key)).Count() == 0)
                {
                    ids.Add(key);
                }
                results.Add(key, response);
            }
            return results;
        }

        private Collection<CampusOrgs> BuildCampusOrgsResponseData(Dictionary<string, CampusOrgs> campusOrgRecords)
        {
            Collection<CampusOrgs> campusOrgContracts = new Collection<CampusOrgs>();
            foreach (var campusOrgItem in campusOrgRecords)
            {
                campusOrgContracts.Add(campusOrgItem.Value);
            }
            return campusOrgContracts;
        }

        private Dictionary<string, CampusOrgMembers> SetupCampusOrgMembers(out List<string> campusOrgMemberIds)
        {
            string[,] recordData = _campusOrgMemberData;

            campusOrgMemberIds = new List<string>();
            int campusOrgMembersCount = recordData.Length / 6;
            Dictionary<string, CampusOrgMembers> records = new Dictionary<string, CampusOrgMembers>();
            for (int i = 0; i < campusOrgMembersCount; i++)
            {
                string campusOrgId = recordData[i, 0].TrimEnd();
                string memberId = (string.IsNullOrEmpty(recordData[i, 1]) ? string.Empty : recordData[i, 1]).TrimEnd();
                List<string> roles = (string.IsNullOrEmpty(recordData[i, 2])) ? new List<string>() : recordData[i, 2].TrimEnd().Split(',').ToList();
                List<string> startDates = (string.IsNullOrEmpty(recordData[i, 3])) ? new List<string>() : recordData[i, 3].TrimEnd().Split(',').ToList();
                List<string> endDates = (string.IsNullOrEmpty(recordData[i, 4])) ? new List<string>() : recordData[i, 4].TrimEnd().Split(',').ToList();
                List<string> statuses = (string.IsNullOrEmpty(recordData[i, 5])) ? new List<string>() : recordData[i, 5].TrimEnd().Split(',').ToList();

                CampusOrgMembers record = new CampusOrgMembers();
                record.Recordkey = campusOrgId + '*' + memberId;
                record.CmpmRoles = new List<string>();
                record.CmpmRoleStartDates = new List<DateTime?>();
                record.CmpmRoleEndDates = new List<DateTime?>();
                record.CmpmRoleStatuses = new List<string>();

                List<DateTime?> convertedStartDates = new List<DateTime?>();
                foreach (var date in startDates) { convertedStartDates.Add(DateTime.Parse(date)); }
                List<DateTime?> convertedEndDates = new List<DateTime?>();
                foreach (var date in endDates) { convertedEndDates.Add(DateTime.Parse(date)); }

                record.CmpmRoles = roles;
                record.CmpmRoleStartDates = convertedStartDates;
                record.CmpmRoleEndDates = convertedEndDates;
                record.CmpmRoleStatuses = statuses;
                record.buildAssociations();

                if (campusOrgMemberIds.Where(id => id.Equals(record.Recordkey)).Count() == 0)
                {
                    campusOrgMemberIds.Add(record.Recordkey);
                }
                records.Add(record.Recordkey, record);
            }
            return records;
        }

        private Collection<CampusOrgMembers> BuildCampusOrgMembersResponseData(Dictionary<string, CampusOrgMembers> campusOrgMemberRecords)
        {
            Collection<CampusOrgMembers> campusOrgMemberContracts = new Collection<CampusOrgMembers>();
            foreach (var institutionItem in campusOrgMemberRecords)
            {
                campusOrgMemberContracts.Add(institutionItem.Value);
            }
            return campusOrgMemberContracts;
        }
    }
}
