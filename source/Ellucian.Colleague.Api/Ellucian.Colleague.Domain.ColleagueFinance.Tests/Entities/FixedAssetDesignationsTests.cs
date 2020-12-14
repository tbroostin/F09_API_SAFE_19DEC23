//Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class FxaTransferFlagsTests
    {
        [TestClass]
        public class FxaTransferFlagsConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private FxaTransferFlags fixedAssetDesignations;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                fixedAssetDesignations = new FxaTransferFlags(guid, code, desc);
            }

            [TestMethod]
            public void FxaTransferFlags_Code()
            {
                Assert.AreEqual(code, fixedAssetDesignations.Code);
            }

            [TestMethod]
            public void FxaTransferFlags_Description()
            {
                Assert.AreEqual(desc, fixedAssetDesignations.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FxaTransferFlags_GuidNullException()
            {
                new FxaTransferFlags(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FxaTransferFlags_CodeNullException()
            {
                new FxaTransferFlags(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FxaTransferFlags_DescNullException()
            {
                new FxaTransferFlags(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FxaTransferFlagsGuidEmptyException()
            {
                new FxaTransferFlags(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FxaTransferFlagsCodeEmptyException()
            {
                new FxaTransferFlags(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FxaTransferFlagsDescEmptyException()
            {
                new FxaTransferFlags(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class FxaTransferFlags_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private FxaTransferFlags fixedAssetDesignations1;
            private FxaTransferFlags fixedAssetDesignations2;
            private FxaTransferFlags fixedAssetDesignations3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                fixedAssetDesignations1 = new FxaTransferFlags(guid, code, desc);
                fixedAssetDesignations2 = new FxaTransferFlags(guid, code, "Second Year");
                fixedAssetDesignations3 = new FxaTransferFlags(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void FxaTransferFlagsSameCodesEqual()
            {
                Assert.IsTrue(fixedAssetDesignations1.Equals(fixedAssetDesignations2));
            }

            [TestMethod]
            public void FxaTransferFlagsDifferentCodeNotEqual()
            {
                Assert.IsFalse(fixedAssetDesignations1.Equals(fixedAssetDesignations3));
            }
        }

        [TestClass]
        public class FxaTransferFlags_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private FxaTransferFlags fixedAssetDesignations1;
            private FxaTransferFlags fixedAssetDesignations2;
            private FxaTransferFlags fixedAssetDesignations3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                fixedAssetDesignations1 = new FxaTransferFlags(guid, code, desc);
                fixedAssetDesignations2 = new FxaTransferFlags(guid, code, "Second Year");
                fixedAssetDesignations3 = new FxaTransferFlags(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void FxaTransferFlagsSameCodeHashEqual()
            {
                Assert.AreEqual(fixedAssetDesignations1.GetHashCode(), fixedAssetDesignations2.GetHashCode());
            }

            [TestMethod]
            public void FxaTransferFlagsDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(fixedAssetDesignations1.GetHashCode(), fixedAssetDesignations3.GetHashCode());
            }
        }
    }
}