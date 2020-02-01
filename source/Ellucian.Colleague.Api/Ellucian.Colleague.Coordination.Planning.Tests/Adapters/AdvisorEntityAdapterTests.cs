// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Planning.Adapters;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Coordination.Planning.Tests.Adapters
{
    [TestClass]
    public class AdvisorEntityAdapterTests
    {
        Mock<IAdapterRegistry> adapterRegistryMock;
        Mock<ILogger> loggerMock;
        AdvisorEntityAdapter adapter;

        Advisor entity;
        string emailTypeCode;
        Dtos.Planning.Advisor dto;

        [TestInitialize]
        public void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            adapter = new AdvisorEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            emailTypeCode = "PRI";
            entity = new Advisor("0001234", "Smith") { FirstName = "John", MiddleName = "Jacob" };
            entity.AddEmailAddress(new EmailAddress("john.smith@ellucian.com", emailTypeCode));
            entity.AddEmailAddress(new EmailAddress("john.j.smith@gmail.com", "PER"));
            entity.AddAdvisee("0001235");
            entity.AddAdvisee("0001236");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AdvisorEntityAdapter_MapToType_null_source_throws_exception()
        {
            dto = adapter.MapToType(null, emailTypeCode);
        }

        [TestMethod]
        public void AdvisorEntityAdapter_MapToType_creates_correct_Advisor_DTO()
        {
            List<string> expectedEmailAddresses = entity.GetEmailAddresses(emailTypeCode).ToList();
            dto = adapter.MapToType(entity, emailTypeCode);
            Assert.IsNotNull(dto);
            Assert.AreEqual(entity.Id, dto.Id);
            Assert.AreEqual(entity.LastName, dto.LastName);
            Assert.AreEqual(entity.FirstName, dto.FirstName);
            Assert.AreEqual(entity.MiddleName, dto.MiddleName);
            Assert.IsNotNull(dto.EmailAddresses);
            CollectionAssert.AreEqual(expectedEmailAddresses, dto.EmailAddresses.ToList());
        }
    }
}
