// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class DefaultsConfigurationTests
    {
        static string addressDuplicateCriteriaId;
        static string addressTypeMappingId;
        static string emailAddressTypeMappingId;
        static string personDuplicateCriteriaId;
        static string subjectDepartmentMappingId;
        static string campusCalendarId;
        static DefaultsConfiguration config;

        [TestInitialize]
        public void DefaultsConfiguration_Initialize()
        {
            addressDuplicateCriteriaId = "ADDRESS.DUPLICATE";
            addressTypeMappingId = "ADDRESS.TYPE.MAP";
            emailAddressTypeMappingId = "EMAIL.TYPE.MAP";
            personDuplicateCriteriaId = "PERSON.DUPLICATE";
            subjectDepartmentMappingId = "SUBJECT.DEPARTMENT.MAP";
            campusCalendarId = "MAIN.CALENDAR";
            config = new DefaultsConfiguration()
            {
                AddressDuplicateCriteriaId = addressDuplicateCriteriaId,
                AddressTypeMappingId = addressTypeMappingId,
                EmailAddressTypeMappingId = emailAddressTypeMappingId,
                PersonDuplicateCriteriaId = personDuplicateCriteriaId,
                SubjectDepartmentMappingId = subjectDepartmentMappingId,
                CampusCalendarId = campusCalendarId,
            };
        }

        [TestClass]
        public class DefaultsConfiguration_Fields : DefaultsConfigurationTests
        {
            [TestMethod]
            public void DefaultsConfiguration_ValidAddressDuplicateCriteriaId()
            {
                Assert.AreEqual(addressDuplicateCriteriaId, config.AddressDuplicateCriteriaId);
            }

            [TestMethod]
            public void DefaultsConfiguration_ValidAddressTypeMappingId()
            {
                Assert.AreEqual(addressTypeMappingId, config.AddressTypeMappingId);
            }

            [TestMethod]
            public void DefaultsConfiguration_ValidEmailAddressTypeMappingId()
            {
                Assert.AreEqual(emailAddressTypeMappingId, config.EmailAddressTypeMappingId);
            }

            [TestMethod]
            public void DefaultsConfiguration_ValidPersonDuplicateCriteriaId()
            {
                Assert.AreEqual(personDuplicateCriteriaId, config.PersonDuplicateCriteriaId);
            }

            [TestMethod]
            public void DefaultsConfiguration_ValidSubjectDepartmentMappingId()
            {
                Assert.AreEqual(subjectDepartmentMappingId, config.SubjectDepartmentMappingId);
            }

            [TestMethod]
            public void DefaultsConfiguration_ValidCampusCalendarId()
            {
                Assert.AreEqual(campusCalendarId, config.CampusCalendarId);
            }
        }

        [TestClass]
        public class DefaultsConfiguration_AddDefaultMapping : DefaultsConfigurationTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DefaultsConfiguration_AddDefaultMapping_NullField()
            {
                config.AddDefaultMapping(null, "defaultValue");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DefaultsConfiguration_AddDefaultMapping_EmptyField()
            {
                config.AddDefaultMapping(string.Empty, "defaultValue");
            }

            [TestMethod]
            public void DefaultsConfiguration_AddDefaultMapping_Valid()
            {
                config.AddDefaultMapping("fieldName", "defaultValue");
                Assert.AreEqual(1, config.DefaultMappings.Count);
                Assert.AreEqual("defaultValue", config.DefaultMappings["fieldName"]);
            }

            [TestMethod]
            public void DefaultsConfiguration_AddDefaultMapping_Dupliate()
            {
                config.AddDefaultMapping("fieldName", "defaultValue");
                config.AddDefaultMapping("fieldName", "defaultValue2");
                Assert.AreEqual(1, config.DefaultMappings.Count);
                Assert.AreEqual("defaultValue", config.DefaultMappings["fieldName"]);
            }
        }

        [TestClass]
        public class DefaultsConfiguration_GetFieldDefault : DefaultsConfigurationTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DefaultsConfiguration_GetFieldDefault_NullField()
            {
                config.GetFieldDefault(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DefaultsConfiguration_GetFieldDefault_EmptyField()
            {
                config.GetFieldDefault(string.Empty);
            }

            [TestMethod]
            public void DefaultsConfiguration_GetFieldDefault_ValidField()
            {
                config.AddDefaultMapping("fieldName", "defaultValue");
                var result = config.GetFieldDefault("fieldName");
                Assert.AreEqual("defaultValue", result);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void DefaultsConfiguration_GetFieldDefault_FieldNotMapped()
            {
                config.AddDefaultMapping("fieldName", "defaultValue");
                var result = config.GetFieldDefault("INVALID");
            }
        }
    }
}
