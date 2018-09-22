using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Collections.Generic;
using Ellucian.Colleague.Data.Student.DataContracts;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class RegistrationGroupRepositoryTests : BaseRepositorySetup
    {
        RegistrationGroupRepository regGroupRepo;
        //IEnumerable<RegistrationGroup> allRegistrationGroups;
        Collection<RegUsers> regUsersResponseData;
        Collection<RegUserTerms> regUserTermsResponseData;
        Collection<RegUserTermLocs> regUserTermLocsResponseData;
        Collection<RegUserSections> regUserSectionsResponseData;
        DateTime? date1 = DateTime.Today.AddDays(1);
        DateTime? date2 = DateTime.Today.AddDays(2);
        DateTime? date3 = DateTime.Today.AddDays(3);
        DateTime? date4 = DateTime.Today.AddDays(4);
        DateTime? date5 = DateTime.Today.AddDays(5);
        DateTime? date6 = DateTime.Today.AddDays(6);
        DateTime? date7 = DateTime.Today.AddDays(7);
        DateTime? date8 = DateTime.Today.AddDays(8);
        DateTime? date9 = DateTime.Today.AddDays(9);
        DateTime? date10 = DateTime.Today.AddDays(10);


        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            regGroupRepo = BuildValidRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            regGroupRepo = null;
            //allRegistrationGroups = null;
        }

        [TestMethod]
        public async Task GetRegistrationGroupId_Success()
        {
            string regGroupId = await regGroupRepo.GetRegistrationGroupIdAsync("R000002");
            Assert.AreEqual("REGISTRAR", regGroupId);
        }

        [TestMethod]
        public async Task GetRegistrationGroupId_DefaultsToParameter()
        {
            string regGroupId = await regGroupRepo.GetRegistrationGroupIdAsync("XXX");
            Assert.AreEqual("WEBREG", regGroupId);
        }

        [TestMethod]
        public async Task GetRegistrationGroupId_UnableToGetStweb()
        {
            dataReaderMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", true)).Returns(Task.FromResult(new DataContracts.StwebDefaults()));
            string regGroupId = await regGroupRepo.GetRegistrationGroupIdAsync("XXX");
            Assert.AreEqual("NAMELESS", regGroupId);
        }

        // NOW THE GETREGISTRATIONGROUP Tests

        [TestMethod]
        public async Task GetRegistrationGroup_Success()
        {
            var registrationGroup = await regGroupRepo.GetRegistrationGroupAsync("WEBREG");
            Assert.AreEqual("WEBREG", registrationGroup.Id);
            
            // Test the registration section date overrides
            var sections = BuildRegUserSectionsResponse();
            Assert.AreEqual(sections.Count(), registrationGroup.SectionRegistrationDates.Count());
            foreach (var item in sections)
            {
                var parts = item.Recordkey.Split(new string[] { "*" }, StringSplitOptions.None);
                var groupSection = registrationGroup.SectionRegistrationDates.Where(s => s.SectionId == parts.ElementAt(1)).FirstOrDefault();
                Assert.AreEqual(item.RgucsAddStartDate, groupSection.AddStartDate);
                Assert.AreEqual(item.RgucsAddEndDate, groupSection.AddEndDate);
            }

            //// Test the registration term date overrides
            var termdates = BuildRegUserTermsResponse();
            Assert.AreEqual(termdates.Count(), registrationGroup.TermRegistrationDates.Count());
            foreach (var item in termdates)
            {
                var parts = item.Recordkey.Split(new string[] { "*" }, StringSplitOptions.None);
                var groupTerm = registrationGroup.TermRegistrationDates.Where(s => s.TermId == parts.ElementAt(1)).FirstOrDefault();
                Assert.AreEqual(item.RgutRegStartDate, groupTerm.RegistrationStartDate);
                Assert.AreEqual(item.RgutRegEndDate, groupTerm.RegistrationEndDate);
            }

            

            // Test the registration term location date overrides
            var termlocs = BuildRegUserTermLocsResponse();
            Assert.AreEqual(termlocs.Count(), registrationGroup.TermLocationRegistrationDates.Count());
            foreach (var item in termlocs)
            {
                var groupTermLoc = registrationGroup.TermLocationRegistrationDates.Where(s => s.TermId == item.RgutlTerm && s.Location == item.RgutlLocation).FirstOrDefault();
                Assert.AreEqual(item.RgutlPreregStartDate, groupTermLoc.PreRegistrationStartDate);
                Assert.AreEqual(item.RgutlPreregEndDate, groupTermLoc.PreRegistrationEndDate);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetRegistrationGroup_NullGroupNameThrowsException()
        {
            var registrationGroup = await regGroupRepo.GetRegistrationGroupAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetRegistrationGroup_EmptyGroupNameThrowsException()
        {
            var registrationGroup = await regGroupRepo.GetRegistrationGroupAsync(string.Empty);
        }



        private RegistrationGroupRepository BuildValidRepository()
        {
            // Set up response for grade Get request
            regUsersResponseData = BuildRegUsersResponse();
            dataReaderMock.Setup<Task<Collection<RegUsers>>>(acc => acc.BulkReadRecordAsync<RegUsers>("REG.USERS", "", true)).Returns(Task.FromResult(regUsersResponseData));

            // Mock the StwebDefaults
            var stwebDefaultsResponse = BuildStwebDefaultsValidResponse();
            dataReaderMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", true)).Returns(Task.FromResult(stwebDefaultsResponse));
            
            // Mock the RegUserSections
            regUserSectionsResponseData = BuildRegUserSectionsResponse();
            var regUserSectionIds = regUserSectionsResponseData.Select(c => c.Recordkey).ToArray();
            dataReaderMock.Setup(acc => acc.SelectAsync("REG.USER.SECTIONS", "")).Returns(Task.FromResult(regUserSectionIds));
            dataReaderMock.Setup<Task<Collection<RegUserSections>>>(acc => acc.BulkReadRecordAsync<RegUserSections>("REG.USER.SECTIONS", regUserSectionIds, true)).Returns(Task.FromResult(regUserSectionsResponseData));

            // Mock the RegUserTerms
            regUserTermsResponseData = BuildRegUserTermsResponse();
            dataReaderMock.Setup<Task<Collection<RegUserTerms>>>(acc => acc.BulkReadRecordAsync<RegUserTerms>("REG.USER.TERMS", "", true)).Returns(Task.FromResult(regUserTermsResponseData));
            
            // Mock the RegUserTermLocs
            regUserTermLocsResponseData = BuildRegUserTermLocsResponse();
            dataReaderMock.Setup<Task<Collection<RegUserTermLocs>>>(acc => acc.BulkReadRecordAsync<RegUserTermLocs>("REG.USER.TERM.LOCS", "", true)).Returns(Task.FromResult(regUserTermLocsResponseData));

            // Construct referenceData repository. (BaseRepositorySetup does basic mocking setup for all these objects.)
            regGroupRepo = new RegistrationGroupRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, null);

            return regGroupRepo;
        }

        private Collection<RegUsers> BuildRegUsersResponse()
        {
            Collection<RegUsers> regUsersData = new Collection<RegUsers>();
            
            // REG.USERS "REGISTRAR"
            var regUser1  = new RegUsers();
            regUser1.Recordkey = "REGISTRAR";
            regUser1.RguRegStaffEntityAssociation = new List<RegUsersRguRegStaff>();
            // Add Staff 1 - Active (past start date)
            var regUser1Staff1 = new RegUsersRguRegStaff();
            regUser1Staff1.RguStaffIdsAssocMember = "R000001";
            regUser1Staff1.RguStaffRegStartDatesAssocMember = DateTime.Today.AddDays(-2);
            regUser1.RguRegStaffEntityAssociation.Add(regUser1Staff1);
            // Add Staff 2 - Active (future end date)
            var regUser1Staff2 = new RegUsersRguRegStaff();
            regUser1Staff2.RguStaffIdsAssocMember = "R000002";
            regUser1Staff2.RguStaffRegEndDatesAssocMember = DateTime.Today.AddDays(10);
            regUser1.RguRegStaffEntityAssociation.Add(regUser1Staff2);
            regUsersData.Add(regUser1);

            // REG.USERS "ADVISORS"
            var regUser2 = new RegUsers();
            regUser2.Recordkey = "ADVISORS";
            regUser2.RguRegStaffEntityAssociation = new List<RegUsersRguRegStaff>();
            // Add Staff 1 - Ended
            var regUser2Staff1 = new RegUsersRguRegStaff();
            regUser2Staff1.RguStaffIdsAssocMember = "A000001";
            regUser2Staff1.RguStaffRegEndDatesAssocMember = DateTime.Today.AddDays(-2);
            regUser2.RguRegStaffEntityAssociation.Add(regUser2Staff1);
            // Add Staff 2 - Active
            var regUser2Staff2 = new RegUsersRguRegStaff();
            regUser2Staff2.RguStaffIdsAssocMember = "A000002";
            regUser2.RguRegStaffEntityAssociation.Add(regUser2Staff2);
            regUsersData.Add(regUser2);

            // REG.USERS "WEBREG"
            var regUser3 = new RegUsers();
            regUser3.Recordkey = "WEBREG";
            regUser3.RguRegStaffEntityAssociation = new List<RegUsersRguRegStaff>();
            regUsersData.Add(regUser3);

            // REG.USERS "NAMELESS"
            var regUser4 = new RegUsers();
            regUser4.Recordkey = "NAMELESS";
            regUser4.RguRegStaffEntityAssociation = new List<RegUsersRguRegStaff>();
            regUsersData.Add(regUser4);
            return regUsersData;
        }

        private StwebDefaults BuildStwebDefaultsValidResponse()
        {
            var defaults = new StwebDefaults();
            defaults.Recordkey = "STWEB.DEFAULTS";
            defaults.StwebRegUsersId = "WEBREG";
            defaults.StwebRegTermsAllowed = new List<string>() { "2015FA", "2016SP" };
            return defaults;
        }

        private Collection<RegUserTerms> BuildRegUserTermsResponse()
        {
            var response = new Collection<RegUserTerms>();
            var webRegTerm1 = new RegUserTerms();
            webRegTerm1.Recordkey = "WEBREG*2015FA";
            webRegTerm1.RgutRegStartDate = date1;
            webRegTerm1.RgutRegEndDate = date2;
            response.Add(webRegTerm1);
            var webRegTerm2 = new RegUserTerms();
            webRegTerm2.Recordkey = "WEBREG*2016SP";
            webRegTerm2.RgutRegStartDate = date3;
            webRegTerm2.RgutRegEndDate = date4;
            response.Add(webRegTerm2);
            return response;
        }

        private Collection<RegUserTermLocs> BuildRegUserTermLocsResponse()
        {
            var response = new Collection<RegUserTermLocs>();
            var webRegTerm1 = new RegUserTermLocs();
            webRegTerm1.Recordkey = "9";
            webRegTerm1.RgutlRegUser = "WEBREG";
            webRegTerm1.RgutlTerm = "2015FA";
            webRegTerm1.RgutlLocation = "MAIN";
            webRegTerm1.RgutlPreregStartDate = date5;
            webRegTerm1.RgutlPreregEndDate = date6;
            response.Add(webRegTerm1);
            var webRegTerm2 = new RegUserTermLocs();
            webRegTerm2.Recordkey = "WEBREG*2016SP*REMOTE";
            webRegTerm2.RgutlRegUser = "WEBREG";
            webRegTerm2.RgutlTerm = "2016SP";
            webRegTerm2.RgutlLocation = "REMOTE";
            webRegTerm2.RgutlPreregStartDate = date7;
            webRegTerm2.RgutlPreregEndDate = date8;
            response.Add(webRegTerm2);
            return response;
        }

        private Collection<RegUserSections> BuildRegUserSectionsResponse()
        {
            var response = new Collection<RegUserSections>();
            var webRegSection1 = new RegUserSections();
            webRegSection1.Recordkey = "WEBREG*SEC1";
            webRegSection1.RgucsAddStartDate = date9;
            webRegSection1.RgucsAddEndDate = date10;
            response.Add(webRegSection1);
            var webRegSection2 = new RegUserSections();
            webRegSection2.Recordkey = "WEBREG*SEC2";
            webRegSection2.RgucsAddStartDate = date7;
            webRegSection2.RgucsAddEndDate = date8;
            response.Add(webRegSection2);
            return response;
        }
    }
}
