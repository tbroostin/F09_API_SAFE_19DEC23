/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    /// <summary>
    /// Test class for Communication code entity
    /// </summary>
    [TestClass]
    public class CommunicationCodeTests
    {
        private List<CommunicationCodeHyperlink> hyperlinks;
        private string explanation;
        private string awardYear;
        private string officeCodeId;
        private bool isViewable;
        private bool allowsAttachments;

        private string guid;
        private string code;
        private string description;

        private CommunicationCode ccCode;

        [TestInitialize]
        public void Initialize()
        {
            guid = "440cd328-7404-408f-80af-7b56c6731271";
            code = "FA2014";
            description = "FA2014 description";
            hyperlinks = new List<CommunicationCodeHyperlink>()
            {
                new CommunicationCodeHyperlink("google.com", "Google"),
                new CommunicationCodeHyperlink("ellucian.com", "Ellucian")
            };
            explanation = "document explanation";
            awardYear = "2014";
            officeCodeId = "office2";
            isViewable = true;
            allowsAttachments = true;
            ccCode = new CommunicationCode(guid, code, description);

            ccCode.AwardYear = awardYear;
            ccCode.Explanation = explanation;
            ccCode.IsStudentViewable = isViewable;
            ccCode.OfficeCodeId = officeCodeId;
            ccCode.Hyperlinks = hyperlinks;
            ccCode.AllowsAttachments = allowsAttachments;
        }

        /// <summary>
        /// Tests if a communication code was successfully created and is not
        /// null
        /// </summary>
        [TestMethod]
        public void CommunicationCodeNotNullTest()
        {
            Assert.IsNotNull(ccCode);
        }

        /// <summary>
        /// Tests if all the attributes of the created communication code match 
        /// the expected ones
        /// </summary>
        [TestMethod]
        public void CommunicationCodeAttributesEqualTest()
        {
            Assert.AreEqual(code, ccCode.Code);
            Assert.AreEqual(description, ccCode.Description);
            CollectionAssert.AreEqual(hyperlinks, ccCode.Hyperlinks);
            Assert.AreEqual(explanation, ccCode.Explanation);
            Assert.AreEqual(awardYear, ccCode.AwardYear);
            Assert.AreEqual(officeCodeId, ccCode.OfficeCodeId);
            Assert.AreEqual(isViewable, ccCode.IsStudentViewable);
            Assert.AreEqual(allowsAttachments, ccCode.AllowsAttachments);
        }

        /// <summary>
        /// Tests if passing null in place of a code argument to the constructor
        /// throws ArgumentNullException
        /// </summary>
        [TestMethod]
        [ExpectedException (typeof(ArgumentNullException))]
        public void NullCodeThrowsExceptionTest()
        {
            new CommunicationCode(null, null, description);
        }

        /// <summary>
        /// Tests if passing null in place of a description argument to the 
        /// constructor throws ArgumentNullException
        /// </summary>
        [TestMethod]
        [ExpectedException (typeof(ArgumentNullException))]
        public void NullDescriptionThrowsExceptionTest()
        {
            new CommunicationCode(guid, code, null);
        }

        [TestMethod]
        public void HyperlinksListInitializedTest()
        {
            ccCode = new CommunicationCode(guid, code, description);
            Assert.IsNotNull(ccCode.Hyperlinks);
            Assert.AreEqual(0, ccCode.Hyperlinks.Count());
        }
        [TestMethod]
        public void AllowsAttachment_FalseByDefault()
        {
            var ccCode1 = new CommunicationCode(guid, code, description);
            Assert.IsFalse(ccCode1.AllowsAttachments);
        }

    }
}
