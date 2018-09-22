// Copyright 2014 Ellucian Company L.P. and its affiliates.

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    /// <summary>
    /// This class tests the valid and invalid conditions of a transcript grouping object
    /// </summary>
    [TestClass]
    public class TranscriptGroupingTests
    {
        [TestInitialize]
        public void Initialize()
        {
        }

        [TestCleanup]
        public void Cleanup()
        {
            
        }
     
        [TestMethod]
        public void TranscriptGroupingConstructor_Success()
        {
            string id = "1";
            string description = "test";
            bool isSelectable = true;

            TranscriptGrouping transcriptGrouping = new TranscriptGrouping(id,description, isSelectable);

            Assert.AreEqual(id, transcriptGrouping.Id);
            Assert.AreEqual(description, transcriptGrouping.Description);
            Assert.AreEqual(isSelectable, transcriptGrouping.IsUserSelectable);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TranscriptGroupingConstructor_NullId()
        {
            string id = null;
            string description = "test";
            bool isSelectable = true;

            TranscriptGrouping transcriptGrouping = new TranscriptGrouping(id, description, isSelectable);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TranscriptGroupingConstructor_NullDescription()
        {
            string id = "1";
            string description = null;
            bool isSelectable = true;

            TranscriptGrouping transcriptGrouping = new TranscriptGrouping(id, description, isSelectable);
        }
               
    }
}