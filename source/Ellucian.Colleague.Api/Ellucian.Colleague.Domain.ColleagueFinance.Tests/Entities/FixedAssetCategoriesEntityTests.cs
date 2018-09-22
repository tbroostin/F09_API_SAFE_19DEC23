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
    public class AssetCategoriesTests
    {
        [TestClass]
        public class AssetCategoriesConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private AssetCategories fixedAssetCategories;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                fixedAssetCategories = new AssetCategories(guid, code, desc);
            }

            [TestMethod]
            public void AssetCategories_Code()
            {
                Assert.AreEqual(code, fixedAssetCategories.Code);
            }

            [TestMethod]
            public void AssetCategories_Description()
            {
                Assert.AreEqual(desc, fixedAssetCategories.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AssetCategories_GuidNullException()
            {
                new AssetCategories(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AssetCategories_CodeNullException()
            {
                new AssetCategories(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AssetCategories_DescNullException()
            {
                new AssetCategories(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AssetCategoriesGuidEmptyException()
            {
                new AssetCategories(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AssetCategoriesCodeEmptyException()
            {
                new AssetCategories(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AssetCategoriesDescEmptyException()
            {
                new AssetCategories(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class AssetCategories_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private AssetCategories fixedAssetCategories1;
            private AssetCategories fixedAssetCategories2;
            private AssetCategories fixedAssetCategories3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                fixedAssetCategories1 = new AssetCategories(guid, code, desc);
                fixedAssetCategories2 = new AssetCategories(guid, code, "Second Year");
                fixedAssetCategories3 = new AssetCategories(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void AssetCategoriesSameCodesEqual()
            {
                Assert.IsTrue(fixedAssetCategories1.Equals(fixedAssetCategories2));
            }

            [TestMethod]
            public void AssetCategoriesDifferentCodeNotEqual()
            {
                Assert.IsFalse(fixedAssetCategories1.Equals(fixedAssetCategories3));
            }
        }

        [TestClass]
        public class AssetCategories_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private AssetCategories fixedAssetCategories1;
            private AssetCategories fixedAssetCategories2;
            private AssetCategories fixedAssetCategories3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                fixedAssetCategories1 = new AssetCategories(guid, code, desc);
                fixedAssetCategories2 = new AssetCategories(guid, code, "Second Year");
                fixedAssetCategories3 = new AssetCategories(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void AssetCategoriesSameCodeHashEqual()
            {
                Assert.AreEqual(fixedAssetCategories1.GetHashCode(), fixedAssetCategories2.GetHashCode());
            }

            [TestMethod]
            public void AssetCategoriesDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(fixedAssetCategories1.GetHashCode(), fixedAssetCategories3.GetHashCode());
            }
        }
    }
}
