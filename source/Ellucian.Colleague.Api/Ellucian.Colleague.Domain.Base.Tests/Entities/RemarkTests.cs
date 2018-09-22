// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class RemarkTests
    {
        private string guid;
        private string author;
        private string code;
        private string donorId;
        private string intg;      
        private string text;
        private string type;
        private DateTime remarkDate;
        private ConfidentialityType confidentialityType;
        private Remark remark;
        
        [TestInitialize]
        public void Initialize()
        {
            guid = "a3a2e49b-df50-4133-9507-ecad4e04004d";
            author = "0011905";
            code = "ST";
            donorId = "0013395";
            intg = "BSF";
            text = "Hello World";
            type = "BU";
            remarkDate = DateTime.Now;
            confidentialityType = ConfidentialityType.Public;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemarkConstructorNullGuid()
        {
            remark = new Remark(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemarkConstructorEmptyGuid()
        {
            remark = new Remark(string.Empty);
        }

        [TestMethod]
        public void RemarkConstructorGuid()
        {
            remark = new Remark(guid);
            Assert.AreEqual(guid, remark.Guid);
        }

        [TestMethod]
        public void RemarksAuthor()
        {
            remark = new Remark(guid);
            remark.RemarksAuthor = author;
            Assert.AreEqual(author, remark.RemarksAuthor);
        }

        [TestMethod]
        public void RemarksCode()
        {
            remark = new Remark(guid);
            remark.RemarksCode = code;
            Assert.AreEqual(code, remark.RemarksCode);
        }

        [TestMethod]
        public void RemarksPrivateType()
        {
            remark = new Remark(guid);
            remark.RemarksPrivateType = confidentialityType;
            Assert.AreEqual(confidentialityType, remark.RemarksPrivateType);
        }

        [TestMethod]
        public void RemarksDate()
        {
            remark = new Remark(guid);
            remark.RemarksDate = remarkDate;
            Assert.AreEqual(remarkDate, remark.RemarksDate);
        }

        [TestMethod]
        public void RemarksDonorId()
        {
            remark = new Remark(guid);
            remark.RemarksDonorId = donorId;
            Assert.AreEqual(donorId, remark.RemarksDonorId);
        }

        [TestMethod]
        public void RemarksIntgEnteredBy()
        {
            remark = new Remark(guid);
            remark.RemarksIntgEnteredBy = intg;
            Assert.AreEqual(intg, remark.RemarksIntgEnteredBy);
        }

        [TestMethod]
        public void RemarksText()
        {
            remark = new Remark(guid);
            remark.RemarksText = text;
            Assert.AreEqual(text, remark.RemarksText);
        }

        [TestMethod]
        public void RemarksType()
        {
            remark = new Remark(guid);
            remark.RemarksType = type;
            Assert.AreEqual(type, remark.RemarksType);
        }   
    }
}