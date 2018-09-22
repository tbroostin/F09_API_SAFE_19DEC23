// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class RegistrationOptionsTests
    {
        RegistrationOptions registrationOptions;
        string personId;
        List<GradingType> gradingTypes;
                
        [TestInitialize]
        public void Initialize()
        {
            personId = "0000899";
            gradingTypes = new List<GradingType>(){GradingType.Audit, GradingType.PassFail};
            registrationOptions = new RegistrationOptions(personId, gradingTypes);
        }

        [TestMethod]
        public void PersonId()
        {
            Assert.AreEqual(personId, registrationOptions.PersonId);
        }

        [TestMethod]
        public void GradingTypes()
        {
            Assert.AreEqual(gradingTypes.Count(), registrationOptions.GradingTypes.Count());
            Assert.AreEqual(gradingTypes.ElementAt(0), registrationOptions.GradingTypes.ElementAt(0));
            Assert.AreEqual(gradingTypes.ElementAt(1), registrationOptions.GradingTypes.ElementAt(1));
        }

        [TestMethod]
        //[ExpectedException(typeof(ArgumentNullException))]
        public void AllowsEmptyListOfGradingTypes()
        {
            gradingTypes = new List<GradingType>();
            registrationOptions = new RegistrationOptions(personId, gradingTypes);
            Assert.AreEqual(0, registrationOptions.GradingTypes.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowsExceptionIfPersonIdNull()
        {
            personId = null;
            registrationOptions = new RegistrationOptions(personId, gradingTypes);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowsExceptionIfPersonIdEmpty()
        {
            personId = string.Empty;
            registrationOptions = new RegistrationOptions(personId, gradingTypes);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowsExceptionIfGradingTypesNull()
        {
            gradingTypes = null;
            registrationOptions = new RegistrationOptions(personId, gradingTypes);
        }

    }
}
