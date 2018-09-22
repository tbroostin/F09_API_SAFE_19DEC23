// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;

namespace Ellucian.Colleague.Coordination.Base.Tests.Adapters
{
    [TestClass]
    public class PersonMatchResultEntityAdapterTests
    {
        private Domain.Base.Entities.PersonMatchResult resultEntityDefinite;
        private Domain.Base.Entities.PersonMatchResult resultEntityPotential;
        private Dtos.Base.PersonMatchResult resultDto;
        private PersonMatchResultEntityAdapter adapter;

        [TestInitialize]
        public void Initialize()
        {
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var loggerMock = new Mock<ILogger>();

            resultEntityDefinite = new Domain.Base.Entities.PersonMatchResult("0001234", 50, "D");
            resultEntityPotential = new Domain.Base.Entities.PersonMatchResult("0001234", 50, "P");
            adapter = new PersonMatchResultEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonMatchResultEntityAdapter_MapToType_NullSource()
        {
            resultDto = adapter.MapToType(null);
        }

        [TestMethod]
        public void PersonMatchResultEntityAdapter_MapToType_PotentialDuplicate()
        {
            resultDto = adapter.MapToType(resultEntityPotential);
            Assert.AreEqual(resultEntityDefinite.PersonId, resultDto.PersonId);
            Assert.AreEqual(resultEntityDefinite.MatchScore, resultDto.MatchScore);
            Assert.AreEqual(Dtos.Base.PersonMatchCategoryType.Potential, resultDto.MatchCategory);
        }

        [TestMethod]
        public void PersonMatchResultEntityAdapter_MapToType_DefiniteDuplicate()
        {
            resultDto = adapter.MapToType(resultEntityDefinite);
            Assert.AreEqual(resultEntityDefinite.PersonId, resultDto.PersonId);
            Assert.AreEqual(resultEntityDefinite.MatchScore, resultDto.MatchScore);
            Assert.AreEqual(Dtos.Base.PersonMatchCategoryType.Definite, resultDto.MatchCategory);
        }
    }
}
