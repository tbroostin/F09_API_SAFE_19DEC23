using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Entities.Requirements.Modifications;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.Requirements.Modifications
{
    [TestClass]
    public class CreditModificationTests
    {
        private ProgramRequirements pr;
        private List<Requirement> requirements;
        private List<Requirement> additionalrequirements;
        private TestProgramRequirementsRepository tprr;
        private TestRequirementRepository trr;
        private TestStudentProgramRepository tspr;

        [TestInitialize]
        public async void Initialize()
        {
            tprr = new TestProgramRequirementsRepository();
            trr = new TestRequirementRepository();
            tspr = new TestStudentProgramRepository();

            pr = tprr.Get("MATHPROG", "2033");
            requirements =(await trr.GetAsync(new List<string>() { })).ToList();  // caveat: this test repo's data doesn't match the others'
            additionalrequirements = new List<Requirement>();
        }

        [TestMethod]
        public void Constructor()
        {
            CreditModification icm = new CreditModification("999", 99, "");
            Assert.AreEqual("999", icm.blockId);
            Assert.AreEqual(icm.Credits, 99);
        }
        [TestMethod]
        public void Constructor_allowsNullBlock()
        {
            CreditModification icm = new CreditModification(null, 98, "");
            Assert.IsNull(icm.blockId);
            Assert.AreEqual(icm.Credits, 98);
        }
        [TestMethod]
        public void Constructor_allowsNullCredits()
        {
            CreditModification icm = new CreditModification("997", null, "");
            Assert.AreEqual("997", icm.blockId);
            Assert.IsNull(icm.Credits);
        }
        [TestMethod]
        public void Constructor_allowsNullBlockAndCredits()
        {
            CreditModification icm = new CreditModification(null, null, "");
            Assert.IsNull(icm.blockId);
            Assert.IsNull(icm.Credits);
        }

        // Modify() for the Top level ProgramRequirements institutional cred
        [TestMethod]
        public void Modify_ProgramRequirements_SetValue()
        {
            Assert.AreEqual(120m, pr.MinimumCredits);  //default from test repo
            CreditModification icm = new CreditModification(null, 98, "");
            icm.Modify(pr, additionalrequirements);
            Assert.AreEqual(98, pr.MinimumCredits);
        }

        [TestMethod]
        public void Modify_ProgramRequirements_SetNull()
        {
            // Starts null, set it to something else.
            CreditModification icm = new CreditModification(null, 98, "");
            icm.Modify(pr, additionalrequirements);
            Assert.AreEqual(98, pr.MinimumCredits);
            // Now null it out
            CreditModification icm2 = new CreditModification(null, null, "");
            icm2.Modify(pr, additionalrequirements);
            Assert.IsNull(pr.MinimumCredits);
        }

        //Modify() does not work for a requirement 
        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void Modify_Requirements_SetValue()
        {
            CreditModification icm = new CreditModification("19000", 96, "");
            icm.Modify(pr, additionalrequirements);
        }

        //Modify() does not work for a Subrequirement 
        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void Modify_Subrequirements_SetValue()
        {
            CreditModification icm = new CreditModification("18000", 95, "");
            icm.Modify(pr, additionalrequirements);
        }

        //Modify() for a group within the programrequriements 
        [TestMethod]
        public void Modify_Group_SetValue()
        {
            Group g = pr.Requirements.First().SubRequirements.First().Groups.First(gr => gr.Id == "10056");
            Assert.IsNull(g.MinCredits);
            CreditModification icm = new CreditModification("10056", 94, "");
            icm.Modify(pr, additionalrequirements);
            Assert.AreEqual(94, g.MinCredits);
            Assert.AreNotEqual(94, pr.MinimumCredits);
        }
    }
}
