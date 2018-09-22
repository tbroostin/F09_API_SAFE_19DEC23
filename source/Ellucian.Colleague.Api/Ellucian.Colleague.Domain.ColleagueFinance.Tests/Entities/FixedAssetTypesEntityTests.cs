//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
     [TestClass]
    public class AssetTypesTests
    {
        [TestClass]
        public class AssetTypesConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private AssetTypes fixedAssetTypes;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                fixedAssetTypes = new AssetTypes(guid, code, desc);
            }

            [TestMethod]
            public void AssetTypes_Code()
            {
                Assert.AreEqual(code, fixedAssetTypes.Code);
            }

            [TestMethod]
            public void AssetTypes_Description()
            {
                Assert.AreEqual(desc, fixedAssetTypes.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AssetTypes_GuidNullException()
            {
                new AssetTypes(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AssetTypes_CodeNullException()
            {
                new AssetTypes(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AssetTypes_DescNullException()
            {
                new AssetTypes(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AssetTypesGuidEmptyException()
            {
                new AssetTypes(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AssetTypesCodeEmptyException()
            {
                new AssetTypes(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AssetTypesDescEmptyException()
            {
                new AssetTypes(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class AssetTypes_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private AssetTypes fixedAssetTypes1;
            private AssetTypes fixedAssetTypes2;
            private AssetTypes fixedAssetTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                fixedAssetTypes1 = new AssetTypes(guid, code, desc);
                fixedAssetTypes2 = new AssetTypes(guid, code, "Second Year");
                fixedAssetTypes3 = new AssetTypes(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void AssetTypesSameCodesEqual()
            {
                Assert.IsTrue(fixedAssetTypes1.Equals(fixedAssetTypes2));
            }

            [TestMethod]
            public void AssetTypesDifferentCodeNotEqual()
            {
                Assert.IsFalse(fixedAssetTypes1.Equals(fixedAssetTypes3));
            }
        }

        [TestClass]
        public class AssetTypes_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private AssetTypes fixedAssetTypes1;
            private AssetTypes fixedAssetTypes2;
            private AssetTypes fixedAssetTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                fixedAssetTypes1 = new AssetTypes(guid, code, desc);
                fixedAssetTypes2 = new AssetTypes(guid, code, "Second Year");
                fixedAssetTypes3 = new AssetTypes(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void AssetTypesSameCodeHashEqual()
            {
                Assert.AreEqual(fixedAssetTypes1.GetHashCode(), fixedAssetTypes2.GetHashCode());
            }

            [TestMethod]
            public void AssetTypesDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(fixedAssetTypes1.GetHashCode(), fixedAssetTypes3.GetHashCode());
            }
        }
    }
}
