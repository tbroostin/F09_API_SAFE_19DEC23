// Copyright 2021 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    /// <summary>
    /// This class tests the valid and invalid conditions of an ApprovedDocument domain entity.
    /// The ApprovedDocument domain entity requires an ID.
    /// </summary>
    [TestClass]
    public class ApprovedDocumentTests
    {
        #region Initialize and Cleanup
        [TestInitialize]
        public void Initialize()
        {

        }

        [TestCleanup]
        public void Cleanup()
        {

        }
        #endregion

        #region ApprovedDocument

        [TestMethod]
        public void ApprovedDocument_Base()
        {
            string Id = "123";
            var approvedDocument = new ApprovedDocument(Id);

            Assert.AreEqual(approvedDocument.Id, approvedDocument.Id);
            Assert.IsNull(approvedDocument.Number);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ApprovedDocument_NullApprovedDocumentId()
        {
            var approvedDocument = new ApprovedDocument(null);
        }

        #endregion

    }
}
