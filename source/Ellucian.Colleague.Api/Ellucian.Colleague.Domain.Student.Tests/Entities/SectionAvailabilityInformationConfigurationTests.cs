// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class SectionAvailabilityInformationConfigurationTests
    {
        private bool showNegativeSeatCounts;
        private bool includeSeatsTakenInAvailabilityInformation;

        [TestInitialize]
        public void SectionAvailabilityInformationConfigurationTests_Initialize()
        {
            showNegativeSeatCounts = true;
            includeSeatsTakenInAvailabilityInformation = true;
        }

        [TestMethod]
        public void SectionAvailabilityInformationConfiguration_constructor()
        {
            SectionAvailabilityInformationConfiguration configuration = new SectionAvailabilityInformationConfiguration(showNegativeSeatCounts, includeSeatsTakenInAvailabilityInformation);

            Assert.AreEqual(showNegativeSeatCounts, configuration.ShowNegativeSeatCounts);
            Assert.AreEqual(includeSeatsTakenInAvailabilityInformation, configuration.IncludeSeatsTakenInAvailabilityInformation);
        }
    }
}
