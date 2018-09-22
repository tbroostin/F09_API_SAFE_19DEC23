// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class EvaluationNoticeTests
    {
        [TestClass]
        public class EvaluationNotice_Constructor
        {
            private string studentId = "0123456";
            private string programCode = "ABC";
            private List<string> text1 = new List<string>() { "text line 11", "text line 12" };
            private EvaluationNoticeType type1 = EvaluationNoticeType.Program;
            private List<string> text2 = null;

            [TestMethod]
            public void StudentId()
            {
                // act
                var notice = new EvaluationNotice(studentId, programCode, text1, type1);
                // assert
                Assert.AreEqual(studentId, notice.StudentId);
            }

            [TestMethod]
            public void ProgramCode()
            {
                // act
                var notice = new EvaluationNotice(studentId, programCode, text1, type1);
                // assert
                Assert.AreEqual(programCode, notice.ProgramCode);
            }

            [TestMethod]
            public void Text()
            {
                // act
                var notice = new EvaluationNotice(studentId, programCode, text1, type1);
                // assert
                for (int i = 0; i < text1.Count(); i++)
                {
                    Assert.AreEqual(text1.ElementAt(i), notice.Text.ElementAt(i));
                }               
            }

            [TestMethod]
            public void Type()
            {
                // act
                var notice = new EvaluationNotice(studentId, programCode, text1, type1);
                // assert
                Assert.AreEqual(type1, notice.Type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfStudentIdNull()
            {
                var notice = new EvaluationNotice(null, programCode, text1, type1);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfStudentIdEmpty()
            {
                var notice = new EvaluationNotice(string.Empty, programCode, text1, type1);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfProgramCodeNull()
            {
                var notice = new EvaluationNotice(studentId, null, text1, type1);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfProgramCodeEmpty()
            {
                var notice = new EvaluationNotice(studentId, string.Empty, text1, type1);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfTextNull()
            {
                var notice = new EvaluationNotice(studentId, programCode, text2, type1);
            }
        }
    }
}
