// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class AcademicCredentialTests
    {
        private string _guid;
        private string _code;
        private string _description;
        private AcademicCredentialType _academicCredentialType;
        private OtherHonor _otherHonor;
        private OtherDegree _otherDegree;
        private OtherCcd _otherCcd;
        private AcadCredential _acadCredential;


        [TestInitialize]
        public void Initialize()
        {
            _guid = Guid.NewGuid().ToString();
            _code = "ENGL";
            _description = "English";
            _academicCredentialType = AcademicCredentialType.Certificate;
        }

        [TestClass]
        public class AcademicCredentialConstructor : AcademicCredentialTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcadCredentialConstructorNullGuid()
            {
                _acadCredential = new AcadCredential(null, _code, _description, _academicCredentialType);
            }

            
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherHonorConstructorNullGuid()
            {
                _otherHonor = new OtherHonor(null, _code, _description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherDegreeConstructorNullGuid()
            {
                _otherDegree = new OtherDegree(null, _code, _description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherCcdConstructorNullGuid()
            {
                _otherCcd = new OtherCcd(null, _code, _description);
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcadCredentialConstructorEmptyGuid()
            {
                _acadCredential = new AcadCredential(string.Empty, _code, _description, _academicCredentialType);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherHonorConstructorEmptyGuid()
            {
                _otherHonor = new OtherHonor(string.Empty, _code, _description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherDegreeConstructorEmptyGuid()
            {
                _otherDegree = new OtherDegree(string.Empty, _code, _description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherCcdConstructorEmptyGuid()
            {
                _otherCcd = new OtherCcd(string.Empty, _code, _description);
            }

            [TestMethod]
            public void AcadCredentialConstructorValidGuid()
            {
                _acadCredential = new AcadCredential(_guid, _code, _description, _academicCredentialType);
                Assert.AreEqual(_guid, _acadCredential.Guid);
            }

            [TestMethod]
            public void OtherHonorConstructorValidGuid()
            {
                _otherHonor = new OtherHonor(_guid, _code, _description);
                Assert.AreEqual(_guid, _otherHonor.Guid);
            }

            [TestMethod]
            public void OtherDegreeConstructorValidGuid()
            {
                _otherDegree = new OtherDegree(_guid, _code, _description);
                Assert.AreEqual(_guid, _otherDegree.Guid);
            }

            [TestMethod]
            public void OtherCcdConstructorValidGuid()
            {
                _otherCcd = new OtherCcd(_guid, _code, _description);
                Assert.AreEqual(_guid, _otherCcd.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcadCredentialConstructorNullCode()
            {
                _acadCredential = new AcadCredential(_guid, null, _description, _academicCredentialType);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherHonorConstructorNullCode()
            {
                _otherHonor = new OtherHonor(_guid, null, _description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherDegreeConstructorNullCode()
            {
                _otherDegree = new OtherDegree(_guid, null, _description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherCcdConstructorNullCode()
            {
                _otherCcd = new OtherCcd(_guid, null, _description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcadCredentialConstructorEmptyCode()
            {
                _acadCredential = new AcadCredential(_guid, string.Empty, _description, _academicCredentialType);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherHonorConstructorEmptyCode()
            {
                _otherHonor = new OtherHonor(_guid, string.Empty, _description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherDegreeConstructorEmptyCode()
            {
                _otherDegree = new OtherDegree(_guid, string.Empty, _description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherCcdConstructorEmptyCode()
            {
                _otherCcd = new OtherCcd(_guid, string.Empty, _description);
            }

            [TestMethod]
            public void AcadCredentialConstructorValidCode()
            {
                _acadCredential = new AcadCredential(_guid, _code, _description, _academicCredentialType);
                Assert.AreEqual(_code, _acadCredential.Code);
            }

            [TestMethod]
            public void OtherHonorConstructorValidCode()
            {
                _otherHonor = new OtherHonor(_guid, _code, _description);
                Assert.AreEqual(_code, _otherHonor.Code);
            }

            [TestMethod]
            public void OtherDegreeConstructorValidCode()
            {
                _otherDegree = new OtherDegree(_guid, _code, _description);
                Assert.AreEqual(_code, _otherDegree.Code);
            }

            [TestMethod]
            public void OtherCcdConstructorValidCode()
            {
                _otherCcd = new OtherCcd(_guid, _code, _description);
                Assert.AreEqual(_code, _otherCcd.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcadCredentialConstructorNullDescription()
            {
                _acadCredential = new AcadCredential(_guid, _code, null, _academicCredentialType);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherHonorConstructorNullDescription()
            {
                _otherHonor = new OtherHonor(_guid, _code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherDegreeConstructorNullDescription()
            {
                _otherDegree = new OtherDegree(_guid, _code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherCcdConstructorNullDescription()
            {
                _otherCcd = new OtherCcd(_guid, _code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcadCredentialConstructorEmptyDescription()
            {
                _acadCredential = new AcadCredential(_guid, _code, string.Empty, _academicCredentialType);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherHonorConstructorEmptyDescription()
            {
                _otherHonor = new OtherHonor(_guid, _code, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherDegreeConstructorEmptyDescription()
            {
                _otherDegree = new OtherDegree(_guid, _code, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherCcdConstructorEmptyDescription()
            {
                _otherCcd = new OtherCcd(_guid, _code, string.Empty);
            }

            [TestMethod]
            public void AcadCredentialConstructorValidDescription()
            {
               _acadCredential = new AcadCredential(_guid, _code, _description, _academicCredentialType);
               Assert.AreEqual(_description, _acadCredential.Description);
            }

            [TestMethod]
            public void OtherHonorConstructorValidDescription()
            {
                _otherHonor = new OtherHonor(_guid, _code, _description);
                Assert.AreEqual(_description, _otherHonor.Description);
            }

            [TestMethod]
            public void OtherDegreeConstructorValidDescription()
            {
                _otherDegree = new OtherDegree(_guid, _code, _description);
                Assert.AreEqual(_description, _otherDegree.Description);
            }

            [TestMethod]
            public void OtherCcdConstructorValidDescription()
            {
                _otherCcd = new OtherCcd(_guid, _code, _description);
                Assert.AreEqual(_description, _otherCcd.Description);
            }

            [TestMethod]
            public void AcadCredentialConstructorValidType()
            {
                _acadCredential = new AcadCredential(_guid, _code, _description, _academicCredentialType);
                Assert.AreEqual(_academicCredentialType, _acadCredential.AcademicCredentialType);
            }

        }
    }
}
