using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class FacultyRepositoryTests : BaseRepositorySetup
    {
        FacultyRepository repository;
        Collection<Ellucian.Colleague.Data.Base.DataContracts.Person> personResponseData;
        Collection<Ellucian.Colleague.Data.Base.DataContracts.Address> personAddressResponseData;
        IEnumerable<string> personIds;
        ICollection<Ellucian.Colleague.Domain.Student.Entities.Faculty> facultyEntities;
        IDictionary<string, Ellucian.Colleague.Domain.Student.Entities.Faculty> allFacultyDict;
        ApiSettings apiSettingsMock;

        [TestInitialize]
        public async void Initialize()
        {
            // Initialize Mock framework
            MockInitialize();

            apiSettingsMock = new ApiSettings("null");

            // Mock the call for getting multiple person records
            facultyEntities = await new TestFacultyRepository().GetAllAsync();
            allFacultyDict = new Dictionary<string, Ellucian.Colleague.Domain.Student.Entities.Faculty>();
            foreach (var fac in facultyEntities)
            {
                allFacultyDict[fac.Id] = fac; 
            }
            personIds = facultyEntities.Select(f => f.Id);

            personResponseData = BuildPersonResponseData(facultyEntities);

            dataReaderMock.Setup(acc => acc.SelectAsync("FACULTY", "")).Returns(Task.FromResult(personResponseData.Select(c => c.Recordkey).ToArray()));
            dataReaderMock.Setup<Task<Collection<Ellucian.Colleague.Data.Base.DataContracts.Person>>>(acc => acc.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), true)).Returns(Task.FromResult(personResponseData));

            // Mock the call for getting address records
            personAddressResponseData = BuildAddressResponseData();
            dataReaderMock.Setup<Task<Collection<Ellucian.Colleague.Data.Base.DataContracts.Address>>>(acc => acc.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>("ADDRESS", It.IsAny<string[]>(), true)).Returns(Task.FromResult(personAddressResponseData));

            // Build the test repository
            repository = new FacultyRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);
        }

        [TestMethod]
        public async Task SingleFacultyGet_TestProperties()
        {
            foreach (var fac in facultyEntities)
            {
                var result = await repository.GetAsync(fac.Id);
                Assert.AreEqual(fac.Id, result.Id);
                Assert.AreEqual(fac.LastName, result.LastName);
                Assert.AreEqual(fac.FirstName, result.FirstName);
                Assert.AreEqual(fac.ProfessionalName, result.ProfessionalName);
            }

        }

        [TestMethod]
        public async Task MultiGet()
        {
            var result = await repository.GetAsync(personIds);
            Assert.AreEqual(10, result.Count());
        }

        [TestMethod]
        public async Task TestFormattedNames()
        {
            var facultyResults = await repository.GetAsync(personIds);
            foreach (var fac in facultyResults)
            {
                Ellucian.Colleague.Domain.Student.Entities.Faculty faculty = facultyEntities.Where(f => f.Id == fac.Id).FirstOrDefault();
                Assert.AreEqual(faculty.ProfessionalName, fac.ProfessionalName);
            }
        }

        [TestMethod]
        public async Task Property_FacultyEmails()
        {
            var facultyResults = await repository.GetAsync(personIds);
            foreach (var fac in facultyResults)
            {
                IEnumerable<string> repofacultyEmails = fac.GetFacultyEmailAddresses("BUS");
                Ellucian.Colleague.Domain.Student.Entities.Faculty faculty = facultyEntities.Where(f => f.Id == fac.Id).FirstOrDefault();
                IEnumerable<string> testFacultyEmails = faculty.GetFacultyEmailAddresses("BUS");
                Assert.AreEqual(testFacultyEmails.Count(), repofacultyEmails.Count());
            }
            Ellucian.Colleague.Domain.Student.Entities.Faculty faculty48 = facultyResults.Where(f => f.Id == "0000048").FirstOrDefault();
            IEnumerable<string> faculty48Emails = faculty48.GetFacultyEmailAddresses("BUS");
            Assert.AreEqual("aDenardi@ccc.com", faculty48Emails.ElementAt(0));
        }

        [TestMethod]
        public async Task Property_FacultyPhones()
        {
            var facultyResults = await repository.GetAsync(personIds);
            Ellucian.Colleague.Domain.Student.Entities.Faculty faculty45 = facultyResults.Where(f => f.Id == "0000045").FirstOrDefault();
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Phone> faculty45Phones = faculty45.GetFacultyPhones("CELL");
            Assert.AreEqual("703-666-7777", faculty45Phones.ElementAt(0).Number);
        }

        [TestMethod]
        public async Task MultiGetNotFoundReturnsEmptyList()
        {
            List<string> idsNotFound = new List<string>() { "0987654", "4567890" };
            Collection<Ellucian.Colleague.Data.Base.DataContracts.Person> noPersonContracts = new Collection<Ellucian.Colleague.Data.Base.DataContracts.Person>();
            dataReaderMock.Setup<Task<Collection<Ellucian.Colleague.Data.Base.DataContracts.Person>>>(acc => acc.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(idsNotFound.ToArray(), true)).Returns(Task.FromResult(noPersonContracts));
            var result = await repository.GetAsync(new List<string>() { "0987654", "4567890" });
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetFacultyByIds()
        {
            var result = await repository.GetFacultyByIdsAsync(personIds);
            Assert.AreEqual(10, result.Count());
        }

        [TestMethod]
        public async Task GetFacultyByIds_TestFormattedNames()
        {
            var facultyResults = await repository.GetFacultyByIdsAsync(personIds);
            foreach (var fac in facultyResults)
            {
                Ellucian.Colleague.Domain.Student.Entities.Faculty faculty = facultyEntities.Where(f => f.Id == fac.Id).FirstOrDefault();
                Assert.AreEqual(faculty.ProfessionalName, fac.ProfessionalName);
            }
        }

        [TestMethod]
        public async Task GetFacultyByIds_FacultyEmails()
        {
            var facultyResults = await repository.GetFacultyByIdsAsync(personIds);
            foreach (var fac in facultyResults)
            {
                IEnumerable<string> repofacultyEmails = fac.GetFacultyEmailAddresses("BUS");
                Ellucian.Colleague.Domain.Student.Entities.Faculty faculty = facultyEntities.Where(f => f.Id == fac.Id).FirstOrDefault();
                IEnumerable<string> testFacultyEmails = faculty.GetFacultyEmailAddresses("BUS");
                Assert.AreEqual(testFacultyEmails.Count(), repofacultyEmails.Count());
            }
            Ellucian.Colleague.Domain.Student.Entities.Faculty faculty48 = facultyResults.Where(f => f.Id == "0000048").FirstOrDefault();
            IEnumerable<string> faculty48Emails = faculty48.GetFacultyEmailAddresses("BUS");
            Assert.AreEqual("aDenardi@ccc.com", faculty48Emails.ElementAt(0));
        }

        [TestMethod]
        public async Task GetFacultyByIds_FacultyPhones()
        {
            var facultyResults =await repository.GetFacultyByIdsAsync(personIds);
            Ellucian.Colleague.Domain.Student.Entities.Faculty faculty45 = facultyResults.Where(f => f.Id == "0000045").FirstOrDefault();
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Phone> faculty45Phones = faculty45.GetFacultyPhones("CELL");
            Assert.AreEqual("703-666-7777", faculty45Phones.ElementAt(0).Number);
        }

        [TestMethod]
        public async Task GetFacultyByIds_GetNotFoundReturnsEmptyList()
        {
            List<string> idsNotFound = new List<string>() { "0987654", "4567890" };
            Collection<Ellucian.Colleague.Data.Base.DataContracts.Person> noPersonContracts = new Collection<Ellucian.Colleague.Data.Base.DataContracts.Person>();
            dataReaderMock.Setup<Task<Collection<Ellucian.Colleague.Data.Base.DataContracts.Person>>>(acc => acc.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(idsNotFound.ToArray(), true)).Returns(Task.FromResult(noPersonContracts));
            var result = await repository.GetFacultyByIdsAsync(new List<string>() { "0987654", "4567890" });
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task SearchFacultyIdsAsync_AllFacultyTypes()
        {
            // In this case both arguments are false so no selection criteria is added and we want all Ids

            // Arrange
            List<string> facultyOnlyIds = new List<string>() { "faculty1", "faculty2", "faculty3" };
            List<string> advisorOnlyIds = new List<string>() { "advisor1", "advisor2" };
            List<string> allIds = new List<string>() { "faculty1", "faculty2", "faculty3", "advisor1", "advisor2" };
            dataReaderMock.Setup<Task<string[]>>(acc => acc.SelectAsync("FACULTY", "WITH FAC.ADVISE.FLAG NE 'Y'")).Returns(Task.FromResult(facultyOnlyIds.ToArray()));
            dataReaderMock.Setup<Task<string[]>>(acc => acc.SelectAsync("FACULTY", "WITH FAC.ADVISE.FLAG = 'Y'")).Returns(Task.FromResult(advisorOnlyIds.ToArray()));
            dataReaderMock.Setup<Task<string[]>>(acc => acc.SelectAsync("FACULTY", "")).Returns(Task.FromResult(allIds.ToArray()));

            // Act
            var result = await repository.SearchFacultyIdsAsync(false, false);

            // Evaluate
            Assert.AreEqual(allIds.Count(), result.Count());
        }

        [TestMethod]
        public async Task SearchFacultyIdsAsync_FacultyOnly()
        {
            // In this case we only want faculty WITH FAC.ADVISE.FLAG NE 'Y'. Second argument is false, first argument is true.
            // Arrange
            List<string> facultyOnlyIds = new List<string>() { "faculty1", "faculty2", "faculty3" };
            List<string> advisorOnlyIds = new List<string>() { "advisor1", "advisor2" };
            List<string> allIds = new List<string>() { "faculty1", "faculty2", "faculty3", "advisor1", "advisor2" };
            dataReaderMock.Setup<Task<string[]>>(acc => acc.SelectAsync("FACULTY", "WITH FAC.ADVISE.FLAG NE 'Y'")).Returns(Task.FromResult(facultyOnlyIds.ToArray()));
            dataReaderMock.Setup<Task<string[]>>(acc => acc.SelectAsync("FACULTY", "WITH FAC.ADVISE.FLAG = 'Y'")).Returns(Task.FromResult(advisorOnlyIds.ToArray()));
            dataReaderMock.Setup<Task<string[]>>(acc => acc.SelectAsync("FACULTY", "")).Returns(Task.FromResult(allIds.ToArray()));

            // Act
            var result = await repository.SearchFacultyIdsAsync(true, false);

            // Evaluate
            Assert.AreEqual(facultyOnlyIds.Count(), result.Count());
        }

        [TestMethod]
        public async Task SearchFacultyIdsAsync_AdvisorOnly()
        {
            // In this case we only want faculty "WITH FAC.ADVISE.FLAG = 'Y'".  Second argument is true. First argument doesn't matter
            // Arrange
            List<string> facultyOnlyIds = new List<string>() { "faculty1", "faculty2", "faculty3" };
            List<string> advisorOnlyIds = new List<string>() { "advisor1", "advisor2" };
            List<string> allIds = new List<string>() { "faculty1", "faculty2", "faculty3", "advisor1", "advisor2" };
            dataReaderMock.Setup<Task<string[]>>(acc => acc.SelectAsync("FACULTY", "WITH FAC.ADVISE.FLAG NE 'Y'")).Returns(Task.FromResult(facultyOnlyIds.ToArray()));
            dataReaderMock.Setup<Task<string[]>>(acc => acc.SelectAsync("FACULTY", "WITH FAC.ADVISE.FLAG = 'Y'")).Returns(Task.FromResult(advisorOnlyIds.ToArray()));
            dataReaderMock.Setup<Task<string[]>>(acc => acc.SelectAsync("FACULTY", "")).Returns(Task.FromResult(allIds.ToArray()));

            // Act
            var result = await repository.SearchFacultyIdsAsync(true, true);

            // Evaluate
            Assert.AreEqual(advisorOnlyIds.Count(), result.Count());
        }

        private Collection<Ellucian.Colleague.Data.Base.DataContracts.Person> BuildPersonResponseData(ICollection<Ellucian.Colleague.Domain.Student.Entities.Faculty> facultyEntities)
        {
            Collection<Ellucian.Colleague.Data.Base.DataContracts.Person> personContracts = new Collection<Ellucian.Colleague.Data.Base.DataContracts.Person>();
            foreach (var fac in facultyEntities)
            {
                //personContracts.Add(personItem.Value);
                Ellucian.Colleague.Data.Base.DataContracts.Person facultyPerson = new Ellucian.Colleague.Data.Base.DataContracts.Person();
                facultyPerson.Recordkey = fac.Id;
                facultyPerson.LastName = fac.LastName;
                facultyPerson.FirstName = fac.FirstName;
                facultyPerson.MiddleName = fac.MiddleName;
                facultyPerson.PersonEmailAddresses = new List<string>();
                facultyPerson.PersonEmailTypes = new List<string>();
                facultyPerson.PersonalPhoneNumber = new List<string>();
                facultyPerson.PersonalPhoneType = new List<string>();
                facultyPerson.PersonalPhoneExtension = new List<string>();
                facultyPerson.PersonAddresses = new List<string>();
                facultyPerson.PersonFormattedNames = new List<string>();
                facultyPerson.PersonFormattedNameTypes = new List<string>();
                IEnumerable<string> personalFacultyEmails = fac.GetFacultyEmailAddresses("PER");
                foreach (var fEmail in personalFacultyEmails)
                {
                    facultyPerson.PersonEmailAddresses.Add(fEmail);
                    facultyPerson.PersonEmailTypes.Add("PER");
                }
                IEnumerable<string> businessFacultyEmails = fac.GetFacultyEmailAddresses("BUS");
                foreach (var fEmail in businessFacultyEmails)
                {
                    facultyPerson.PersonEmailAddresses.Add(fEmail);
                    facultyPerson.PersonEmailTypes.Add("BUS");
                }
                IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Phone> facultyCellPhones = fac.GetFacultyPhones("CELL");
                foreach (var ph in facultyCellPhones)
                {
                    facultyPerson.PersonalPhoneType.Add(ph.TypeCode);
                    facultyPerson.PersonalPhoneNumber.Add(ph.Number);
                }
                IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Phone> facultyHomePhones = fac.GetFacultyPhones("HOME");
                foreach (var ph in facultyHomePhones)
                {
                    facultyPerson.PersonalPhoneType.Add(ph.TypeCode);
                    facultyPerson.PersonalPhoneNumber.Add(ph.Number);
                }
                // Update professional names 
                if (fac.Id == "0000036")
                {
                    facultyPerson.PersonFormattedNames.Add("Miltons Smith");
                    facultyPerson.PersonFormattedNames.Add("M.K. Smith");
                    facultyPerson.PersonFormattedNameTypes.Add("PF");
                    facultyPerson.PersonFormattedNameTypes.Add("FAC");
                }
                else
                {
                    facultyPerson.PersonFormattedNames.Add(fac.LastName + ", " + fac.FirstName);
                    facultyPerson.PersonFormattedNameTypes.Add("FAC");
                }
                // Update an additional phone in an address record.
                if (fac.Id == "0000036")
                {
                    facultyPerson.PersonAddresses.Add("10");
                }
                if (fac.Id == "0000045")
                {
                    facultyPerson.PersonAddresses.Add("11");
                }
                facultyPerson.buildAssociations();
                personContracts.Add(facultyPerson);
            }
            return personContracts;
        }

        private Collection<Ellucian.Colleague.Data.Base.DataContracts.Address> BuildAddressResponseData()
        {
            Collection<Ellucian.Colleague.Data.Base.DataContracts.Address> addressContracts = new Collection<Ellucian.Colleague.Data.Base.DataContracts.Address>();
            Ellucian.Colleague.Data.Base.DataContracts.Address address1 = new Ellucian.Colleague.Data.Base.DataContracts.Address();
            address1.Recordkey = "10";
            address1.AddressPhones = new List<string>();
            address1.AddressPhoneType = new List<string>();
            address1.AddressPhoneExtension = new List<string>();
            address1.AddressPhoneType.Add("BUS");
            address1.AddressPhones.Add("703-444-5555");
            address1.AddressPhoneExtension.Add("ext 1");
            address1.buildAssociations();
            addressContracts.Add(address1);
            Ellucian.Colleague.Data.Base.DataContracts.Address address2 = new Ellucian.Colleague.Data.Base.DataContracts.Address();
            address2.Recordkey = "11";
            address2.AddressPhones = new List<string>();
            address2.AddressPhoneType = new List<string>();
            address2.AddressPhoneExtension = new List<string>();
            address2.AddressPhoneType.Add("CELL");
            address2.AddressPhones.Add("703-666-7777");
            address2.AddressPhoneExtension.Add("ext 1");
            address2.buildAssociations();
            addressContracts.Add(address2);
            return addressContracts;
        }


    }
}