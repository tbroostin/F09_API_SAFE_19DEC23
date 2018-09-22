// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class GeneralLedgerComponentDescriptionTests
    {
        #region Initialize and Cleanup
        private GeneralLedgerComponentDescriptionBuilder Builder;

        [TestInitialize]
        public void Initialize()
        {
            Builder = new GeneralLedgerComponentDescriptionBuilder();
        }

        [TestCleanup]
        public void Cleanup()
        {
            Builder = null;
        }
        #endregion

        #region Constructor tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullId()
        {
            var glComponentDescription = Builder.WithId(null).Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_EmptyId()
        {
            var glComponentDescription = Builder.WithId("").Build();
        }

        [TestMethod]
        public void Constructor()
        {
            var id = "01";
            var componentType = GeneralLedgerComponentType.Location;
            var glComponentDescription = Builder.WithId(id)
                .WithComponentType(componentType).Build();

            Assert.AreEqual(id, glComponentDescription.Id);
            Assert.AreEqual(componentType, glComponentDescription.ComponentType);
        }
        #endregion
    }
}