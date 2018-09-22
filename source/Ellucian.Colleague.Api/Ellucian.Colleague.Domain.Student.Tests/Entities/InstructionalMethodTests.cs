using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class InstructionalMethodTests
    {
        [TestClass]
        public class InstructionalMethodConstructor
        {
            // Not sure these tests are even necessary since this now inherits CodeItem class.
            private string guid;
            private string code;
            private string desc;
            private InstructionalMethod instrMethod;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "LEC";
                desc = "Lecture";
                instrMethod = new InstructionalMethod(guid, code, desc, true);
            }

            [TestMethod]
            public void InstructionalMethod_Guid()
            {
                Assert.AreEqual(guid, instrMethod.Guid);
            }

            [TestMethod]
            public void InstructionalMethod_Code()
            {
                Assert.AreEqual(code, instrMethod.Code);
            }

            [TestMethod]
            public void InstructionalMethod_Description()
            {
                Assert.AreEqual(desc, instrMethod.Description);
            }

            [TestMethod]
            public void InstructionalMethod_IsOnline_True()
            {
                Assert.IsTrue(instrMethod.IsOnline);
            }

            [TestMethod]
            public void InstructionalMethod_IsOnline_False()
            {
                instrMethod = new InstructionalMethod(guid, code, desc, false);
                Assert.IsFalse(instrMethod.IsOnline);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InstructionalMethod_Guid_NullException()
            {
                new InstructionalMethod(null, code, desc, true);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InstructionalMethod_Guid_EmptyException()
            {
                new InstructionalMethod(string.Empty, code, desc, true);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InstructionalMethod_Code_NullException()
            {
                new InstructionalMethod(guid, null, desc, true);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InstructionalMethodCodeEmptyException()
            {
                new InstructionalMethod(guid, string.Empty, desc, true);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InstructionalMethod_Desc_NullException()
            {
                new InstructionalMethod(guid, code, null, true);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InstructionalMethodDescEmptyException()
            {
                new InstructionalMethod(guid, code, string.Empty, true);
            }
        }
    }
}
