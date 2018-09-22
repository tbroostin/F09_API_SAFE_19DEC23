using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class RestrictionTests
    {
        [TestClass]
        public class RestrictionConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private int? severity;
            private string visibleToUsers;
            private string title;
            private string details;
            private string followUpApp;
            private string followUpLinkDef;
            private string followUpWAForm;
            private string followUpLabel;
            private string followUpIsMiscText;
            private Restriction rest;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "ACC30";
                desc = "Account 30 days past due";
                severity = 1;
                visibleToUsers = "Y";
                title = "Your account is 30 days past due";
                details = "Your account is 30 days past due. Please make a payment as soon as possible";
                followUpApp = "ST";
                followUpLinkDef = "";
                followUpWAForm = "WPMT";
                followUpLabel = "Click here to make a payment";
                followUpIsMiscText = "N";
                rest = new Restriction(guid, code, desc, severity, visibleToUsers, title, details, followUpApp, followUpLinkDef, followUpWAForm, followUpLabel, followUpIsMiscText);
            }

            [TestMethod]
            public void Restriction_Guid()
            {
                Assert.AreEqual(guid, rest.Guid);
            }

            [TestMethod]
            public void Restriction_Code()
            {
                Assert.AreEqual(code, rest.Code);
            }

            [TestMethod]
            public void Restriction_Description()
            {
                Assert.AreEqual(desc, rest.Description);
            }

            [TestMethod]
            public void Restriction_Severity()
            {
                Assert.AreEqual(severity, rest.Severity);
            }

            [TestMethod]
            public void Restriction_OfficeUseOnlyYes()
            {
                // inverted, visible to users as "Y" means office use only = false
                Assert.AreEqual(false, rest.OfficeUseOnly);
            }

            [TestMethod]
            public void Restriction_OfficeUseOnlyNo()
            {
                rest = new Restriction(guid, code, desc, severity, "N", title, details, followUpApp, followUpLinkDef, followUpWAForm, followUpLabel, followUpIsMiscText);
                Assert.AreEqual(true, rest.OfficeUseOnly);
            }

            [TestMethod]
            public void Restriction_OfficeUseOnlyNull()
            {
                rest = new Restriction(guid, code, desc, severity, null, title, details, followUpApp, followUpLinkDef, followUpWAForm, followUpLabel, followUpIsMiscText);
                Assert.AreEqual(true, rest.OfficeUseOnly);
            }

            [TestMethod]
            public void Restriction_OfficeUseOnlyEmpty()
            {
                rest = new Restriction(guid, code, desc, severity, string.Empty, title, details, followUpApp, followUpLinkDef, followUpWAForm, followUpLabel, followUpIsMiscText);
                Assert.AreEqual(true, rest.OfficeUseOnly);
            }

            [TestMethod]
            public void Restriction_Title()
            {
                Assert.AreEqual(title, rest.Title);
            }

            [TestMethod]
            public void Restriction_NullTitleUsesDescription()
            {
                rest = new Restriction(guid, code, desc, severity, visibleToUsers, null, details, followUpApp, followUpLinkDef, followUpWAForm, followUpLabel, followUpIsMiscText);
                Assert.AreEqual(desc, rest.Title);
            }

            [TestMethod]
            public void Restriction_EmptyTitleUsesDescription()
            {
                rest = new Restriction(guid, code, desc, severity, visibleToUsers, string.Empty, details, followUpApp, followUpLinkDef, followUpWAForm, followUpLabel, followUpIsMiscText);
                Assert.AreEqual(desc, rest.Title);
            }

            [TestMethod]
            public void Restriction_Details()
            {
                Assert.AreEqual(details, rest.Details);
            }

            [TestMethod]
            public void Restriction_FUApp()
            {
                Assert.AreEqual(followUpApp, rest.FollowUpApplication);
            }

            [TestMethod]
            public void Restriction_FULinkDef()
            {
                Assert.AreEqual(followUpLinkDef, rest.FollowUpLinkDefinition);
            }

            [TestMethod]
            public void Restriction_FUWAForm()
            {
                Assert.AreEqual(followUpWAForm, rest.FollowUpWebAdvisorForm);
            }

            [TestMethod]
            public void Restriction_Label()
            {
                Assert.AreEqual(followUpLabel, rest.FollowUpLabel);
            }

            [TestMethod]
            public void Restriction_MiscTextFlagNo()
            {
                Assert.AreEqual(false, rest.MiscellaneousTextFlag);
            }

            [TestMethod]
            public void Restriction_DefaultHyperlink()
            {
                Assert.AreEqual(null, rest.Hyperlink);
            }

            [TestMethod]
            public void Restriction_ChangeHyperlink()
            {
                rest.Hyperlink = "abcd";
                Assert.AreEqual("abcd", rest.Hyperlink);
            }

            [TestMethod]
            public void Restriction_ChangelABEL()
            {
                rest.FollowUpLabel = "abcd";
                Assert.AreEqual("abcd", rest.FollowUpLabel);
            }

            [TestMethod]
            public void Restriction_MiscTextFlagYes()
            {
                rest = new Restriction(guid, code, desc, severity, visibleToUsers, title, details, followUpApp, followUpLinkDef, followUpWAForm, followUpLabel, "Y");
                Assert.AreEqual(true, rest.MiscellaneousTextFlag);
            }

            [TestMethod]
            public void Restriction_MiscTextFlagNull()
            {
                rest = new Restriction(guid, code, desc, severity, visibleToUsers, title, details, followUpApp, followUpLinkDef, followUpWAForm, followUpLabel, null);
                Assert.AreEqual(false, rest.MiscellaneousTextFlag);
            }

            [TestMethod]
            public void Restriction_MiscTextFlagEmpty()
            {
                rest = new Restriction(guid, code, desc, severity, visibleToUsers, title, details, followUpApp, followUpLinkDef, followUpWAForm, followUpLabel, string.Empty);
                Assert.AreEqual(false, rest.MiscellaneousTextFlag);
            }
        }
    }
}
