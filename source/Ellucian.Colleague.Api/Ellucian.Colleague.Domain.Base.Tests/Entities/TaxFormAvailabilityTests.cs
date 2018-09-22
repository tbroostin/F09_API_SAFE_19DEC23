// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Base.Tests.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class TaxFormAvailabilityTests
    {
        #region Initialize and Cleanup
        private TaxFormAvailabilityBuilder builder;

        [TestInitialize]
        public void Initialize()
        {
            builder = new TaxFormAvailabilityBuilder();
        }

        [TestCleanup]
        public void Cleanup()
        {
            builder = null;
        }
        #endregion

        [TestMethod]
        public void TaxFormAvailability_ConstructorWithDate_Available()
        {
            var actualAvailability = builder.WithTaxYear("2014").WithAvailabilityDate(new DateTime(2015, 02, 01)).Build();
            Assert.AreEqual(builder.TaxYear, actualAvailability.TaxYear);
            Assert.IsTrue(actualAvailability.Available);
        }

        [TestMethod]
        public void TaxFormAvailability_ConstructorWithDate_NotAvailable()
        {
            var actualAvailability = builder.WithTaxYear("2015").WithAvailabilityDate(DateTime.Now.AddDays(1)).Build();
            Assert.AreEqual(builder.TaxYear, actualAvailability.TaxYear);
            Assert.IsFalse(actualAvailability.Available);
        }

        [TestMethod]
        public void TaxFormAvailability_ConstructorWithNullDate_NotAvailable()
        {
            var actualAvailability = builder.WithTaxYear("2015").WithAvailabilityDate(null).Build();
            Assert.AreEqual(builder.TaxYear, actualAvailability.TaxYear);
            Assert.IsFalse(actualAvailability.Available);
        }

        [TestMethod]
        public void TaxFormAvailability_ConstructorWithBool_Available()
        {
            var actualAvailability = builder.WithTaxYear("2014").WithAvailableBool(true).Build();
            Assert.AreEqual(builder.TaxYear, actualAvailability.TaxYear);
            Assert.IsTrue(actualAvailability.Available);
        }

        [TestMethod]
        public void TaxFormAvailability_ConstructorWithBool_NotAvailable()
        {
            var actualAvailability = builder.WithTaxYear("2014").WithAvailableBool(false).Build();
            Assert.AreEqual(builder.TaxYear, actualAvailability.TaxYear);
            Assert.IsFalse(actualAvailability.Available);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTaxYear_WithBool()
        {
            builder.WithTaxYear(null).Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTaxYear_WithDate()
        {
            builder.WithTaxYear(null).WithAvailabilityDate(DateTime.Now).Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmptyTaxYear()
        {
            builder.WithTaxYear(string.Empty).Build();
        }
    }
}
