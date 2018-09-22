// Copyright 2016 Ellucian Company L.P. and its affiliates.
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
    class ProxyCandidateEntityAdapterTests
    {
        ProxyCandidateEntityAdapter mapper;
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;

        private Domain.Base.Entities.PersonMatchResult result1;
        private Domain.Base.Entities.PersonMatchResult result2;
        private Domain.Base.Entities.ProxyCandidate entCand;
        private List<string> perms;
        private List<Domain.Base.Entities.PersonMatchResult> results;

        private string person1 = "0000001";
        private string person2 = "0000002";
        private int score1 = 90;
        private int score2 = 50;
        private string cat1 = "D";
        private string cat2 = "P";
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
        public class ProxyCandidateEntityAdapter_MapToType : ProxyCandidateEntityAdapterTests
        {
            [TestInitialize]
            public void ProxyCandidateEntityAdapter_MapToType_Initialize()
            {
                result1 = new Domain.Base.Entities.PersonMatchResult(person1, score1, cat1);
                result2 = new Domain.Base.Entities.PersonMatchResult(person2, score2, cat2);
                results = new List<Domain.Base.Entities.PersonMatchResult>() { result1, result2 };
                perms = new List<string>() { "SFAA", "SFMAP", };
                entCand = new Domain.Base.Entities.ProxyCandidate(subject, relType, perms, first, last, emailAddr, results)
                {
                    BirthDate = birthDate,
                    EmailType = emailType,
                    FormerFirstName = formerFirst,
                    FormerLastName = formerLast,
                    FormerMiddleName = formerMiddle,
                    MiddleName = middle,
                    Gender = gender,
                    Phone = phone,
                    PhoneExtension = ext,
                    PhoneType = phoneType,
                    Prefix = prefix,
                    Id = id,
                    GovernmentId = ssn,
                    Suffix = suffix,
                };
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                mapper = new ProxyCandidateEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            }

            [TestMethod]
            public void ProxyCandidateEntityAdapter_MapToType_Good()
            {
                adapterRegistryMock.Setup(adapter =>
                    adapter.GetAdapter<Domain.Base.Entities.PersonMatchResult, Dtos.Base.PersonMatchResult>())
                    .Returns(new PersonMatchResultEntityAdapter(adapterRegistryMock.Object, loggerMock.Object));

                var result = mapper.MapToType(entCand);
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
