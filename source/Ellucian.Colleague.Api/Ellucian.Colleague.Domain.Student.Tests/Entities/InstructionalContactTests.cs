using System;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class InstructionalContactTests
    {
        string instructionMethod;
        InstructionalContact contact;

        [TestInitialize]
        public void Initialize()
        {
            instructionMethod = "LEC";
            contact = new InstructionalContact(instructionMethod);
        }

        [TestMethod]
        public void InstructionalContact_Constructor_Valid()
        {
            Assert.AreEqual(instructionMethod, contact.InstructionalMethodCode);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstructionalContact_Constructor_NullCode()
        {
            contact = new InstructionalContact(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstructionalContact_Constructor_EmptyCode()
        {
            contact = new InstructionalContact(string.Empty);
        }

    }
}
