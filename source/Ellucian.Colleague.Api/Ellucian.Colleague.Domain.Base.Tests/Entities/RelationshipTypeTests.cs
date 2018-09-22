// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class RelationshipTypeTests
    {
        private const string _code = "CODE";
        private const string _codeDescription = "Code Description";
        private const string _inverseCode = "INVERSE";

        [TestMethod]
        public void RelationType_Constructor()
        {
            var x = new RelationshipType(_code, _codeDescription, _inverseCode);
            Assert.IsTrue(
                x.Code.Equals(_code) &&
                x.Description.Equals(_codeDescription) &&
                x.InverseCode.Equals(_inverseCode)
            );
        }

        [TestMethod]
        public void RelationType_Constructor_InverseCode_Null()
        {
            var x = new RelationshipType(_code, _codeDescription, null);
            Assert.IsTrue(
                x.Code.Equals(_code) &&
                x.Description.Equals(_codeDescription) &&
                x.InverseCode.Equals(_code)
            );
        }

        [TestMethod]
        public void RelationType_Constructor_InverseCode_Empty()
        {
            var x = new RelationshipType(_code, _codeDescription, string.Empty);
            Assert.IsTrue(
                x.Code.Equals(_code) &&
                x.Description.Equals(_codeDescription) &&
                x.InverseCode.Equals(_code)
            );
        }
    }
}
