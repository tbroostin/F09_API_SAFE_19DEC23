// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class GraduationQuestionTests
    {
        [TestClass]
        public class GraduationQuestion_Constructor
        {
            private GraduationQuestion question;
            private GraduationQuestionType type;

            [TestInitialize]
            public void Initialize()
            {
                type = GraduationQuestionType.PhoneticSpelling;
                question = new GraduationQuestion(type, true);
                
            }

            [TestCleanup]
            public void Cleanup()
            {
                question = null;
                
            }

            [TestMethod]
            public void GraduationQuestion_Type()
            {
                Assert.AreEqual(type, question.Type);
            }

            [TestMethod]
            public void GraduationQuestion_IsRequired()
            {
                Assert.IsTrue(question.IsRequired);
            }

            [TestMethod]
            public void GraduationQuestion_IsRequired_Default()
            {
                var question2 = new GraduationQuestion(type);
                Assert.IsFalse(question2.IsRequired);
            }
        }

        [TestClass]
        public class GraduationQuestion_Equals
        {
            private GraduationQuestion question;
            private GraduationQuestion question2;
            private GraduationQuestionType type;

            [TestInitialize]
            public void Initialize()
            {
                type = GraduationQuestionType.PhoneticSpelling;
                question = new GraduationQuestion(type, true);
                question2 = new GraduationQuestion(type);
            }

            [TestCleanup]
            public void Cleanup()
            {
                question = null;
                question2 = null;
            }

            [TestMethod]
            public void GraduationQuestion_AreEqual()
            {
                Assert.IsTrue(question.Equals(question2));
            }

            [TestMethod]
            public void GraduationQuestion_Null_NotEqual()
            {
                Assert.IsFalse(question.Equals(null));
            }

            [TestMethod]
            public void GraduationQuestion_WrongType_NotEqual()
            {
                var config = new GraduationConfiguration();
                Assert.IsFalse(question.Equals(config));
            }
        }
    }
}
