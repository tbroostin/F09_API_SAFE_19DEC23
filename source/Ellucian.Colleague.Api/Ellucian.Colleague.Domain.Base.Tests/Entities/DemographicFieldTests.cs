// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class DemographicFieldTests
    {
        private string code;
        private string description;
        private DemographicFieldRequirement requirement;
        private DemographicField field;

        [TestInitialize]
        public void DemographicField()
        {
            code = "code";
            description = "desc";
            requirement = DemographicFieldRequirement.Required;
        }

        [TestCleanup]
        public void DemographicFieldTests_Cleanup()
        {
            code = null;
            description = null;
            field = null;
        }

        [TestClass]
        public class DemographicFieldTests_Constructor : DemographicFieldTests
        {
            [TestMethod]
            public void DemographicFieldTests_Constructor_Code()
            {
                field = new DemographicField(code, description, requirement);
                Assert.AreEqual(code, field.Code);
            }

            [TestMethod]
            public void DemographicFieldTests_Constructor_Description()
            {
                field = new DemographicField(code, description, requirement);
                Assert.AreEqual(description, field.Description);
            }

            [TestMethod]
            public void DemographicFieldTests_Constructor_Requirement()
            {
                field = new DemographicField(code, description, requirement);
                Assert.AreEqual(requirement, field.Requirement);
            }

            [TestMethod]
            public void DemographicFieldTests_Constructor_FieldType_Prefix()
            {
                code = "PREFIX";
                field = new DemographicField(code, description, requirement);
                Assert.AreEqual(DemographicFieldType.Prefix, field.Type);
            }

            [TestMethod]
            public void DemographicFieldTests_Constructor_FieldType_FirstName()
            {
                code = "FIRST_NAME";
                field = new DemographicField(code, description, requirement);
                Assert.AreEqual(DemographicFieldType.FirstName, field.Type);
            }

            [TestMethod]
            public void DemographicFieldTests_Constructor_FieldType_MiddleName()
            {
                code = "MIDDLE_NAME";
                field = new DemographicField(code, description, requirement);
                Assert.AreEqual(DemographicFieldType.MiddleName, field.Type);
            }
            
            [TestMethod]
            public void DemographicFieldTests_Constructor_FieldType_LastName()
            {
                code = "LAST_NAME";
                field = new DemographicField(code, description, requirement);
                Assert.AreEqual(DemographicFieldType.LastName, field.Type);
            }

            [TestMethod]
            public void DemographicFieldTests_Constructor_FieldType_Suffix()
            {
                code = "SUFFIX";
                field = new DemographicField(code, description, requirement);
                Assert.AreEqual(DemographicFieldType.Suffix, field.Type);
            }

            [TestMethod]
            public void DemographicFieldTests_Constructor_FieldType_FormerFirstName()
            {
                code = "FORMER_FIRST_NAME";
                field = new DemographicField(code, description, requirement);
                Assert.AreEqual(DemographicFieldType.FormerFirstName, field.Type);
            }

            [TestMethod]
            public void DemographicFieldTests_Constructor_FieldType_FormerMiddleName()
            {
                code = "FORMER_MIDDLE_NAME";
                field = new DemographicField(code, description, requirement);
                Assert.AreEqual(DemographicFieldType.FormerMiddleName, field.Type);
            }

            [TestMethod]
            public void DemographicFieldTests_Constructor_FieldType_FormerLastName()
            {
                code = "FORMER_LAST_NAME";
                field = new DemographicField(code, description, requirement);
                Assert.AreEqual(DemographicFieldType.FormerLastName, field.Type);
            }

            [TestMethod]
            public void DemographicFieldTests_Constructor_FieldType_EmailAddress()
            {
                code = "EMAIL_ADDRESS";
                field = new DemographicField(code, description, requirement);
                Assert.AreEqual(DemographicFieldType.EmailAddress, field.Type);
            }

            [TestMethod]
            public void DemographicFieldTests_Constructor_FieldType_EmailType()
            {
                code = "EMAIL_TYPE";
                field = new DemographicField(code, description, requirement);
                Assert.AreEqual(DemographicFieldType.EmailType, field.Type);
            }

            //[TestMethod]
            //public void DemographicFieldTests_Constructor_FieldType_AddressLines()
            //{
            //    code = "ADDRESS_LINES";
            //    field = new DemographicField(code, description, requirement);
            //    Assert.AreEqual(DemographicFieldType.AddressLines, field.Type);
            //}

            //[TestMethod]
            //public void DemographicFieldTests_Constructor_FieldType_AddressType()
            //{
            //    code = "ADDRESS_TYPE";
            //    field = new DemographicField(code, description, requirement);
            //    Assert.AreEqual(DemographicFieldType.AddressType, field.Type);
            //}

            //[TestMethod]
            //public void DemographicFieldTests_Constructor_FieldType_City()
            //{
            //    code = "CITY";
            //    field = new DemographicField(code, description, requirement);
            //    Assert.AreEqual(DemographicFieldType.City, field.Type);
            //}

            //[TestMethod]
            //public void DemographicFieldTests_Constructor_FieldType_StateProvince()
            //{
            //    code = "STATE";
            //    field = new DemographicField(code, description, requirement);
            //    Assert.AreEqual(DemographicFieldType.StateProvince, field.Type);
            //}

            //[TestMethod]
            //public void DemographicFieldTests_Constructor_FieldType_PostalCode()
            //{
            //    code = "POSTAL_CODE";
            //    field = new DemographicField(code, description, requirement);
            //    Assert.AreEqual(DemographicFieldType.PostalCode, field.Type);
            //}

            //[TestMethod]
            //public void DemographicFieldTests_Constructor_FieldType_Country()
            //{
            //    code = "COUNTRY";
            //    field = new DemographicField(code, description, requirement);
            //    Assert.AreEqual(DemographicFieldType.Country, field.Type);
            //}

            [TestMethod]
            public void DemographicFieldTests_Constructor_FieldType_Phone()
            {
                code = "PHONE";
                field = new DemographicField(code, description, requirement);
                Assert.AreEqual(DemographicFieldType.Phone, field.Type);
            }

            [TestMethod]
            public void DemographicFieldTests_Constructor_FieldType_PhoneExtension()
            {
                code = "PHONE_EXTENSION";
                field = new DemographicField(code, description, requirement);
                Assert.AreEqual(DemographicFieldType.PhoneExtension, field.Type);
            }

            [TestMethod]
            public void DemographicFieldTests_Constructor_FieldType_PhoneType()
            {
                code = "PHONE_TYPE";
                field = new DemographicField(code, description, requirement);
                Assert.AreEqual(DemographicFieldType.PhoneType, field.Type);
            }

            [TestMethod]
            public void DemographicFieldTests_Constructor_FieldType_DateOfBirth()
            {
                code = "BIRTH_DATE";
                field = new DemographicField(code, description, requirement);
                Assert.AreEqual(DemographicFieldType.BirthDate, field.Type);
            }

            [TestMethod]
            public void DemographicFieldTests_Constructor_FieldType_Gender()
            {
                code = "GENDER";
                field = new DemographicField(code, description, requirement);
                Assert.AreEqual(DemographicFieldType.Gender, field.Type);
            }

            [TestMethod]
            public void DemographicFieldTests_Constructor_FieldType_GovernmentId()
            {
                code = "SSN";
                field = new DemographicField(code, description, requirement);
                Assert.AreEqual(DemographicFieldType.GovernmentId, field.Type);
            }

            [TestMethod]
            public void DemographicFieldTests_Constructor_FieldType_Unknown()
            {
                field = new DemographicField(code, description, requirement);
                Assert.AreEqual(DemographicFieldType.Unknown, field.Type);
            }
        }

        [TestClass]
        public class DemographicFieldTests_Equals : DemographicFieldTests
        {
            [TestMethod]
            public void DemographicFieldTests_Equals_MismatchedType()
            {
                field = new DemographicField(code, description, requirement);
                string field2 = "string";
                Assert.IsFalse(field.Equals(field2));
            }

            [TestMethod]
            public void DemographicFieldTests_Equals_SameCode()
            {
                field = new DemographicField(code, description, requirement);
                var field2 = new DemographicField(code, description + "2", DemographicFieldRequirement.Optional);
                Assert.IsTrue(field2.Equals(field));
            }

            [TestMethod]
            public void DemographicFieldTests_Equals_UniqueCode()
            {
                field = new DemographicField(code, description, requirement);
                var field2 = new DemographicField(code + "2", description + "2", DemographicFieldRequirement.Optional);
                Assert.IsFalse(field2.Equals(field));
            }
        }

        [TestClass]
        public class DemographicFieldTests_GetHashCode : DemographicFieldTests
        {
            [TestMethod]
            public void DemographicFieldTests_GetHashCode_Valid()
            {
                field = new DemographicField(code, description, requirement);
                var hashcode = code.GetHashCode();
                Assert.AreEqual(hashcode, field.GetHashCode());
            }
        }
    }
}
