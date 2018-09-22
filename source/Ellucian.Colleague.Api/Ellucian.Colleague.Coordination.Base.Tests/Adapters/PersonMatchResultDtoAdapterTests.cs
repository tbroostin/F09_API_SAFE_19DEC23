// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Tests.Adapters
{
    [TestClass]
    public class PersonMatchResultDtoAdapterTests
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;

        private string person = "person";
        private int score = 90;
        private Dtos.Base.PersonMatchCategoryType typeD = Dtos.Base.PersonMatchCategoryType.Definite;
        private Dtos.Base.PersonMatchCategoryType typeP = Dtos.Base.PersonMatchCategoryType.Potential;
        private Dtos.Base.PersonMatchResult resultD;
        private Dtos.Base.PersonMatchResult resultP;
        private PersonMatchResultDtoAdapter mapper;

        [TestInitialize]
        public void PersonMatchResultDtoAdapterTests_Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();

            mapper = new PersonMatchResultDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
            resultD = new Dtos.Base.PersonMatchResult()
            {
                PersonId = person,
                MatchCategory = typeD,
                MatchScore = score,
            };

            resultP = new Dtos.Base.PersonMatchResult()
            {
                PersonId = person,
                MatchCategory = typeP,
                MatchScore = score,
            };
        }

        [TestMethod]
        public void PersonMatchResultDtoAdapterTests_GoodD()
        {
            var result = mapper.MapToType(resultD);
            Assert.AreEqual(person, result.PersonId);
            Assert.AreEqual(score, result.MatchScore);
            Assert.AreEqual(Domain.Base.Entities.PersonMatchCategoryType.Definite, result.MatchCategory);
        }

        [TestMethod]
        public void PersonMatchResultDtoAdapterTests_GoodP()
        {
            var result = mapper.MapToType(resultP);
            Assert.AreEqual(person, result.PersonId);
            Assert.AreEqual(score, result.MatchScore);
            Assert.AreEqual(Domain.Base.Entities.PersonMatchCategoryType.Potential, result.MatchCategory);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonMatchResultDtoAdapterTests_NullArgument()
        {
            var result = mapper.MapToType(null);
        }
    }
}
