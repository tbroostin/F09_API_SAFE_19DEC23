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
    public class GpaModificationTests
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
            requirements = (await trr.GetAsync(new List<string>(){})).ToList();  // caveat: this test repo's data doesn't match the others'
            additionalrequirements = new List<Requirement>();
        }

        [TestMethod]
        public void Constructor()
        {
            GpaModification icm = new GpaModification("999", 2.4m, "");
            Assert.AreEqual("999", icm.blockId);
            Assert.AreEqual(2.4m, icm.Gpa);
        }
        [TestMethod]
        public void Constructor_allowsNullBlock()
        {
            GpaModification icm = new GpaModification(null, 2.6m, "");
            Assert.IsNull(icm.blockId);
            Assert.AreEqual(2.6m, icm.Gpa);
        }
        [TestMethod]
        public void Constructor_allowsNullGpa()
        {
            GpaModification icm = new GpaModification("997", null, "");
            Assert.AreEqual("997", icm.blockId);
            Assert.IsNull(icm.Gpa);
        }
        [TestMethod]
        public void Constructor_allowsNullBlockAndGpa()
        {
            GpaModification icm = new GpaModification(null, null, "");
            Assert.IsNull(icm.blockId);
            Assert.IsNull(icm.Gpa);
        }

        // Modify() for the Top level ProgramRequirements institutional cred
        [TestMethod]
        public void Modify_ProgramRequirements_SetValue()
        {
            GpaModification icm = new GpaModification(null, 2.1m, "");
            icm.Modify(pr, additionalrequirements);
            Assert.AreEqual(2.1m, pr.MinOverallGpa);
        }

        [TestMethod]
        public void Modify_ProgramRequirements_SetNull()
        {
            // Starts null, set it to something else.
            GpaModification icm = new GpaModification(null, 2.2m, "");
            icm.Modify(pr, additionalrequirements);
            Assert.AreEqual(2.2m, pr.MinOverallGpa);
            // Now null it out
            GpaModification icm2 = new GpaModification(null, null, "");
            icm2.Modify(pr, additionalrequirements);
            Assert.IsNull(pr.MinOverallGpa);
        }

        //Modify() for a requirement within the programrequriements 
        [TestMethod]
        public void Modify_Requirements_SetValue()
        {

            Requirement r = pr.Requirements.First(rq => rq.Id == "19000");
            Assert.IsNull(r.MinGpa);
            GpaModification icm = new GpaModification("19000", 1.1m, "");
            icm.Modify(pr, additionalrequirements);
            Assert.AreEqual(1.1m, r.MinGpa);         // modified the right thing to the right amount
            Assert.AreNotEqual(96, pr.MinOverallGpa); // make sure we didn't modify anything else
        }

        //Modify() for a Subrequirement within the programrequriements 
        [TestMethod]
        public void Modify_Subrequirements_SetValue()
        {

            Subrequirement sr = pr.Requirements.First().SubRequirements.First(sbr => sbr.Id == "18000");
            Assert.IsNull(sr.MinGpa);
            GpaModification icm = new GpaModification("18000", 1.2m, "");
            icm.Modify(pr, additionalrequirements);
            Assert.AreEqual(1.2m, sr.MinGpa);
            Assert.AreNotEqual(1.2m, pr.MinOverallGpa);
            Assert.AreNotEqual(1.2m, pr.Requirements.First().MinGpa);

        }

        //Modify() for a group within the programrequriements 
        [TestMethod]
        public void Modify_Group_SetValue()
        {

            Group g = pr.Requirements.First().SubRequirements.First().Groups.First(gr => gr.Id == "10056");
            Assert.IsNull(g.MinInstitutionalCredits);
            GpaModification icm = new GpaModification("10056", 1.3m, "");
            icm.Modify(pr, additionalrequirements);
            Assert.AreEqual(1.3m, g.MinGpa);
            Assert.AreNotEqual(1.3m, pr.MinOverallGpa);
            Assert.AreNotEqual(1.3m, pr.Requirements.First().MinGpa);
            Assert.AreNotEqual(1.3m, pr.Requirements.First().SubRequirements.First().MinGpa);

        }
    }
}
