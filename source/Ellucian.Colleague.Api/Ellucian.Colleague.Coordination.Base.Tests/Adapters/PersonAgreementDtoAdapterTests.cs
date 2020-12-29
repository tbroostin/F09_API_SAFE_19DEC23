// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Coordination.Base.Tests.Adapters
{
    [TestClass]
    public class PersonAgreementDtoAdapterTests
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;

        public PersonAgreementDtoAdapter adapter;

        public Dtos.Base.PersonAgreement dto;

        [TestInitialize]
        public void PersonAgreementDtoAdapter_Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();

            adapterRegistryMock.Setup(r => r.GetAdapter<Dtos.Base.PersonAgreement, Domain.Base.Entities.PersonAgreement>())
                .Returns(() => new AutoMapperAdapter<Dtos.Base.PersonAgreement, Domain.Base.Entities.PersonAgreement>(adapterRegistryMock.Object, loggerMock.Object));

            adapter = new PersonAgreementDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

            dto = new Dtos.Base.PersonAgreement()
            {
                Id = "1",
                PersonId = "0001234",
                ActionTimestamp = DateTime.Now.AddDays(-3),
                AgreementCode = "AGR1",
                AgreementPeriodCode = "PER1",
                Title ="Agreement 1",
                DueDate = DateTime.Today.AddDays(30),
                PersonCanDeclineAgreement = true,
                Status = Dtos.Base.PersonAgreementStatus.Accepted,
                Text = new List<string>() { "Agreement 1 Text" }
            };
        }

        [TestMethod]
        public void PersonAgreementDtoAdapter_IsBaseAdapter()
        {
            Assert.IsInstanceOfType(adapter, typeof(BaseAdapter<Dtos.Base.PersonAgreement, Domain.Base.Entities.PersonAgreement>));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonAgreementDtoAdapterTests_Null_Dto_throws_ArgumentNullException()
        {
            var entity = adapter.MapToType(null);
        }

        [TestMethod]
        public void PersonAgreementDtoAdapterTests_Dto_converts_to_Entity()
        {
            var entity = adapter.MapToType(dto);
            Assert.IsNotNull(entity);
            Assert.AreEqual(dto.ActionTimestamp, entity.ActionTimestamp);
            Assert.AreEqual(dto.AgreementCode, entity.AgreementCode);
            Assert.AreEqual(dto.AgreementPeriodCode, entity.AgreementPeriodCode);
            Assert.AreEqual(dto.Title, entity.Title);
            Assert.AreEqual(dto.DueDate, entity.DueDate);
            Assert.AreEqual(dto.Id, entity.Id);
            Assert.AreEqual(dto.PersonCanDeclineAgreement, entity.PersonCanDeclineAgreement);
            Assert.AreEqual(dto.PersonId, entity.PersonId);
            Assert.AreEqual(dto.Status.ToString(), entity.Status.ToString());
            CollectionAssert.AreEqual(dto.Text.ToList(), entity.Text.ToList());
        }
    }
}
