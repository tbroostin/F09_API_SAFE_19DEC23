// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class AcademicDisciplineTests
    {
        private string _guid;
        private string _code;
        private string _description;
        private AcademicDisciplineType _academicDisciplineType;
        private OtherMajor _otherMajor;
        private OtherMinor _otherMinor;
        private OtherSpecial _otherSpecial;
        private AcademicDiscipline _academicDiscipline;

        [TestInitialize]
        public void Initialize()
        {
            _guid = Guid.NewGuid().ToString();
            _code = "ENGL";
            _description = "English";
            _academicDisciplineType = AcademicDisciplineType.Major;
        }

        [TestClass]
        public class AcademicDisciplineConstructor : AcademicDisciplineTests
        {

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicDisciplineConstructorNullGuid()
            {
                _academicDiscipline = new AcademicDiscipline(null, _code, _description, _academicDisciplineType);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherMajorConstructorNullGuid()
            {
                _otherMajor = new OtherMajor(null, _code, _description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherMinorConstructorNullGuid()
            {
                _otherMinor = new OtherMinor(null, _code, _description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherSpecialConstructorNullGuid()
            {
                _otherSpecial = new OtherSpecial(null, _code, _description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicDisciplineConstructorEmptyGuid()
            {
                _academicDiscipline = new AcademicDiscipline(string.Empty, _code, _description, _academicDisciplineType);
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherMajorConstructorEmptyGuid()
            {
                _otherMajor = new OtherMajor(string.Empty, _code, _description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherMinorConstructorEmptyGuid()
            {
                _otherMinor = new OtherMinor(string.Empty, _code, _description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherSpecialConstructorEmptyGuid()
            {
                _otherSpecial = new OtherSpecial(string.Empty, _code, _description);
            }

            [TestMethod]
            public void AcademicDisciplineConstructorValidGuid()
            {
                _academicDiscipline = new AcademicDiscipline(_guid, _code, _description, _academicDisciplineType);
                Assert.AreEqual(_guid, _academicDiscipline.Guid);
            }

            [TestMethod]
            public void OtherMajorConstructorValidGuid()
            {
                _otherMajor = new OtherMajor(_guid, _code, _description);
                Assert.AreEqual(_guid, _otherMajor.Guid);
            }

            [TestMethod]
            public void OtherMinorConstructorValidGuid()
            {
                _otherMinor = new OtherMinor(_guid, _code, _description);
                Assert.AreEqual(_guid, _otherMinor.Guid);
            }

            [TestMethod]
            public void OtherSpecialConstructorValidGuid()
            {
                _otherSpecial = new OtherSpecial(_guid, _code, _description);
                Assert.AreEqual(_guid, _otherSpecial.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicDisciplineConstructorNullCode()
            {
                _academicDiscipline = new AcademicDiscipline(_guid, null, _description, _academicDisciplineType);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherMajorConstructorNullCode()
            {
                _otherMajor = new OtherMajor(_guid, null, _description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherMinorConstructorNullCode()
            {
                _otherMinor = new OtherMinor(_guid, null, _description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherSpecialConstructorNullCode()
            {
                _otherSpecial = new OtherSpecial(_guid, null, _description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicDisciplineConstructorEmptyCode()
            {
                _academicDiscipline = new AcademicDiscipline(_guid, string.Empty, _description, _academicDisciplineType);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherMajorConstructorEmptyCode()
            {
                _otherMajor = new OtherMajor(_guid, string.Empty, _description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherMinorConstructorEmptyCode()
            {
                _otherMinor = new OtherMinor(_guid, string.Empty, _description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherSpecialConstructorEmptyCode()
            {
                _otherSpecial = new OtherSpecial(_guid, string.Empty, _description);
            }

            [TestMethod]
            public void AcademicDisciplineConstructorValidCode()
            {
               _academicDiscipline = new AcademicDiscipline(_guid, _code, _description, _academicDisciplineType);
               Assert.AreEqual(_code, _academicDiscipline.Code);
            }

            [TestMethod]
            public void OtherMajorConstructorValidCode()
            {
                _otherMajor = new OtherMajor(_guid, _code, _description);
                Assert.AreEqual(_code, _otherMajor.Code);
            }

            [TestMethod]
            public void OtherMinorConstructorValidCode()
            {
                _otherMinor = new OtherMinor(_guid, _code, _description);
                Assert.AreEqual(_code, _otherMinor.Code);
            }

            [TestMethod]
            public void OtherSpecialConstructorValidCode()
            {
                _otherSpecial = new OtherSpecial(_guid, _code, _description);
                Assert.AreEqual(_code, _otherSpecial.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicDisciplineConstructorNullDescription()
            {
                _academicDiscipline = new AcademicDiscipline(_guid, _code, null, _academicDisciplineType);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherMajorConstructorNullDescription()
            {
                _otherMajor = new OtherMajor(_guid, _code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherMinorConstructorNullDescription()
            {
                _otherMinor = new OtherMinor(_guid, _code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherSpecialConstructorNullDescription()
            {
                _otherSpecial = new OtherSpecial(_guid, _code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicDisciplineConstructorEmptyDescription()
            {
                _academicDiscipline = new AcademicDiscipline(_guid, _code, string.Empty, _academicDisciplineType);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherMajorConstructorEmptyDescription()
            {
                _otherMajor = new OtherMajor(_guid, _code, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherMinorConstructorEmptyDescription()
            {
                _otherMinor = new OtherMinor(_guid, _code, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherSpecialConstructorEmptyDescription()
            {
                _otherSpecial = new OtherSpecial(_guid, _code, string.Empty);
            }

            [TestMethod]
            public void AcademicDisciplineConstructorValidDescription()
            {
                _academicDiscipline = new AcademicDiscipline(_guid, _code, _description, _academicDisciplineType);
                Assert.AreEqual(_description, _academicDiscipline.Description);
            }

            [TestMethod]
            public void OtherMajorConstructorValidDescription()
            {
                _otherMajor = new OtherMajor(_guid, _code, _description);
                Assert.AreEqual(_description, _otherMajor.Description);
            }

            [TestMethod]
            public void OtherMinorConstructorValidDescription()
            {
                _otherMinor = new OtherMinor(_guid, _code, _description);
                Assert.AreEqual(_description, _otherMinor.Description);
            }

            [TestMethod]
            public void OtherSpecialConstructorValidDescription()
            {
                _otherSpecial = new OtherSpecial(_guid, _code, _description);
                Assert.AreEqual(_description, _otherSpecial.Description);
            }

            [TestMethod]
            public void AcademicDisciplineConstructorValidType()
            {
                _academicDiscipline = new AcademicDiscipline(_guid, _code, _description, _academicDisciplineType);
                Assert.AreEqual(_academicDisciplineType, _academicDiscipline.AcademicDisciplineType);
            }
        }
    }
}