using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Tests.Adapters
{
    [TestClass]
    public class ProxyCandidateDtoAdapterTests
    {
        ProxyCandidateDtoAdapter mapper;
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;

        private Dtos.Base.PersonMatchResult result1;
        private Dtos.Base.PersonMatchResult result2;
        private Dtos.Base.ProxyCandidate dtoCand;
        private List<string> perms;
        private List<Dtos.Base.PersonMatchResult> results;

        private string person1 = "0000001";
        private string person2 = "0000002";
        private int score1 = 90;
        private int score2 = 50;
        private Dtos.Base.PersonMatchCategoryType cat1 = Dtos.Base.PersonMatchCategoryType.Definite;
        private Dtos.Base.PersonMatchCategoryType cat2 = Dtos.Base.PersonMatchCategoryType.Potential;
        private string subject = "0000003";
        private DateTime birthDate = DateTime.Today;
        private string emailAddr = "my.email@ellucian.edu";
        private string emailType = "emailType";
        private string first = "First";
        private string formerFirst = "Former First";
        private string formerMiddle = "Former Middle";
        private string formerLast = "Former Last";
        private string last = "Last";
        private string middle = "Middle";
        private string gender = "M";
        private string phone = "Phone";
        private string ext = "Ext";
        private string phoneType = "Phonetype";
        private string prefix = "Mr.";
        private string id = "CandId";
        private string relType = "RelType";
        private string ssn = "SSN";
        private string suffix = "Jr.";

        [TestClass]
        public class ProxyCandidateDtoAdapter_MapToType : ProxyCandidateDtoAdapterTests
        {
            [TestInitialize]
            public void ProxyCandidateDtoAdapter_MapToType_Initialize()
            {
                result1 = new Dtos.Base.PersonMatchResult() { PersonId = person1, MatchScore = score1, MatchCategory = cat1 };
                result2 = new Dtos.Base.PersonMatchResult() { PersonId = person2, MatchScore = score2, MatchCategory = cat2 };
                results = new List<Dtos.Base.PersonMatchResult>() { result1, result2 };
                perms = new List<string>() { "SFAA", "SFMAP", };
                dtoCand = new Dtos.Base.ProxyCandidate()
                {
                    BirthDate = birthDate,
                    EmailAddress = emailAddr,
                    EmailType = emailType,
                    FirstName = first,
                    FormerFirstName = formerFirst,
                    FormerLastName = formerLast,
                    FormerMiddleName = formerMiddle,
                    LastName = last,
                    MiddleName = middle,
                    Gender = gender,
                    GrantedPermissions = perms,
                    Phone = phone,
                    PhoneExtension = ext,
                    PhoneType = phoneType,
                    Prefix = prefix,
                    Id = id,
                    ProxyMatchResults = results,
                    ProxySubject = subject,
                    RelationType = relType,
                    GovernmentId = ssn,
                    Suffix = suffix,
                };
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                mapper = new ProxyCandidateDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
            }

            [TestMethod]
            public void ProxyCandidateDtoAdapter_MapToType_Good()
            {
                adapterRegistryMock.Setup(adapter =>
                    adapter.GetAdapter<Dtos.Base.PersonMatchResult, Domain.Base.Entities.PersonMatchResult>())
                    .Returns(new PersonMatchResultDtoAdapter(adapterRegistryMock.Object, loggerMock.Object));

                var result = mapper.MapToType(dtoCand);
                Assert.AreEqual(result.BirthDate, birthDate);
                Assert.AreEqual(result.EmailAddress, emailAddr);
                Assert.AreEqual(result.EmailType, emailType);
                Assert.AreEqual(result.FirstName, first);
                Assert.AreEqual(result.FormerFirstName, formerFirst);
                Assert.AreEqual(result.FormerLastName, formerLast);
                Assert.AreEqual(result.FormerMiddleName, formerMiddle);
                Assert.AreEqual(result.LastName, last);
                Assert.AreEqual(result.MiddleName, middle);
                Assert.AreEqual(result.Gender, gender);
                CollectionAssert.AreEquivalent(result.GrantedPermissions.ToList(), perms);
                Assert.AreEqual(result.Phone, phone);
                Assert.AreEqual(result.PhoneExtension, ext);
                Assert.AreEqual(result.PhoneType, phoneType);
                Assert.AreEqual(result.Prefix, prefix);
                Assert.AreEqual(result.Id, id);
                Assert.AreEqual(result.ProxySubject, subject);
                Assert.AreEqual(result.RelationType, relType);
                Assert.AreEqual(result.GovernmentId, ssn);
                Assert.AreEqual(result.Suffix, suffix);
            }
        }
    }
}
