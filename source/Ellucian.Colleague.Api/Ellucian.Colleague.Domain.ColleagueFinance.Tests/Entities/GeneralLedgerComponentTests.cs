// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class GeneralLedgerComponentTests
    {
        #region Initialize and Cleanup
        private GeneralLedgerComponentBuilder Builder;

        [TestInitialize]
        public void Initialize()
        {
            Builder = new GeneralLedgerComponentBuilder();
        }

        [TestCleanup]
        public void Cleanup()
        {
            Builder = null;
        }
        #endregion

        #region Tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullComponent()
        {
            var actualEntity = Builder.WithComponent(null).Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_EmptyComponent()
        {
            var actualEntity = Builder.WithComponent("").Build();
        }

        [TestMethod]
        public void Constructor_SetIsPartOfDescription()
        {
            var actualEntity = Builder.WithIsPartOfDescription(true).Build();
            Assert.IsTrue(actualEntity.IsPartOfDescription);
        }

        [TestMethod]
        public void Constructor_SetComponentType()
        {
            var componentType = GeneralLedgerComponentType.Object;
            var actualEntity = Builder.WithComponentType(componentType).Build();
            Assert.AreEqual(componentType, actualEntity.ComponentType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullStartPosition()
        {
            var actualEntity = Builder.WithStartPosition(null).Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_EmptyStartPosition()
        {
            var actualEntity = Builder.WithStartPosition("").Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void Constructor_StartPositionTooLow_ShouldBeZero()
        {
            var actualEntity = Builder.WithStartPosition("0").Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullComponentLength()
        {
            var actualEntity = Builder.WithLength(null).Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_EmptyComponentLength()
        {
            var actualEntity = Builder.WithLength("").Build();
        }

        [TestMethod]
        public void Constructor_Success()
        {
            var actualEntity = Builder.Build();
            Assert.AreEqual(Builder.ComponentName, actualEntity.ComponentName);
            Assert.AreEqual(Builder.IsPartOfDescription, actualEntity.IsPartOfDescription);
            Assert.AreEqual(Builder.ComponentType, actualEntity.ComponentType);
            Assert.AreEqual(Int32.Parse(Builder.StartPosition)-1, actualEntity.StartPosition);
            int requestedLength;
            if (Int32.TryParse(Builder.ComponentLength, out requestedLength))
            {
                Assert.AreEqual(requestedLength, actualEntity.ComponentLength);
            }
        }
        #endregion
    }
}