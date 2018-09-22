// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Tests.Adapters
{
    [TestClass]
    public class PersonNameDtoAdapterTests
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;
        public PersonNameDtoAdapter mapper;

        [TestInitialize]
        public void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            mapper = new PersonNameDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
        }

        [TestMethod]
        public void PersonNameDtoAdapter_MapToType_Good()
        {
            string
                given = "Given",
                middle = "Middle",
                family = "Family";
            Dtos.Base.PersonName nameDto = new Dtos.Base.PersonName() { GivenName = given, FamilyName = family, MiddleName = middle, };
            var nameEnt = mapper.MapToType(nameDto);
            Assert.AreEqual(given, nameEnt.GivenName);
            Assert.AreEqual(middle, nameEnt.MiddleName);
            Assert.AreEqual(family, nameEnt.FamilyName);
        }
    }
}
