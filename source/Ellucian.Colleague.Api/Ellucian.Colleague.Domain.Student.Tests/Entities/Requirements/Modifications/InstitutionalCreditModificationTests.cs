using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Entities.Requirements.Modifications;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.Requirements.Modifications
{
    [TestClass]
    public class InstitutionalCreditModificationTests
    {
        private ProgramRequirements pr;
        private List<Requirement> requirements;
        private TestProgramRequirementsRepository tprr;
        private TestRequirementRepository trr;
        private TestStudentProgramRepository tspr;
        private List<Requirement> additionalRequirements;
        [TestInitialize]
        public async void Initialize()
        {
            tprr = new TestProgramRequirementsRepository();
            trr = new TestRequirementRepository();
            tspr = new TestStudentProgramRepository();

            pr = tprr.Get("MATHPROG", "2033");
            requirements = (await trr.GetAsync(new List<string>(){})).ToList();  // caveat: this test repo's data doesn't match the others'
            additionalRequirements = new List<Requirement>();
        }

        [TestMethod]
        public void Constructor()
        {
            InstitutionalCreditModification icm = new InstitutionalCreditModification("999", 99, "");
            Assert.AreEqual("999", icm.blockId);
            Assert.AreEqual(icm.institutionalCredits, 99);
        }
        [TestMethod]
        public void Constructor_allowsNullBlock()
        {
            InstitutionalCreditModification icm = new InstitutionalCreditModification(null, 98, "");
            Assert.IsNull(icm.blockId);
            Assert.AreEqual(icm.institutionalCredits, 98);
        }
        [TestMethod]
        public void Constructor_allowsNullCredits()
        {
            InstitutionalCreditModification icm = new InstitutionalCreditModification("997", null, "");
            Assert.AreEqual("997", icm.blockId);
            Assert.IsNull(icm.institutionalCredits);
        }
        [TestMethod]
        public void Constructor_allowsNullBlockAndCredits()
        {
            InstitutionalCreditModification icm = new InstitutionalCreditModification(null, null, "");
            Assert.IsNull(icm.blockId);
            Assert.IsNull(icm.institutionalCredits);
        }

        // Modify() for the Top level ProgramRequirements institutional cred
        [TestMethod]
        public void Modify_ProgramRequirements_SetValue()
        {
            Assert.AreEqual(40M, pr.MinimumInstitutionalCredits);  //default from test repo
            InstitutionalCreditModification icm = new InstitutionalCreditModification(null, 98, "");
            icm.Modify(pr, additionalRequirements);
            Assert.AreEqual(98, pr.MinimumInstitutionalCredits);
        }

        [TestMethod]
        public void Modify_ProgramRequirements_SetNull()
        {
            // Starts null, set it to something else.
            InstitutionalCreditModification icm = new InstitutionalCreditModification(null, 98, "");
            icm.Modify(pr, additionalRequirements);
            Assert.AreEqual(98, pr.MinimumInstitutionalCredits);
            // Now null it out
            InstitutionalCreditModification icm2 = new InstitutionalCreditModification(null, null, "");
            icm2.Modify(pr, additionalRequirements);
            Assert.IsNull(pr.MinimumInstitutionalCredits);
        }

        //Modify() for a requirement within the programrequriements 
        [TestMethod]
        public void Modify_Requirements_SetValue()
        {

            Requirement r = pr.Requirements.First(rq => rq.Id == "19000");
            Assert.IsNull(r.MinInstitutionalCredits);
            InstitutionalCreditModification icm = new InstitutionalCreditModification("19000", 96, "");
            icm.Modify(pr, additionalRequirements);
            Assert.AreEqual(96, r.MinInstitutionalCredits);         // modified the right thing to the right amount
            Assert.AreNotEqual(96, pr.MinimumInstitutionalCredits); // make sure we didn't modify anything else
        }

        //Modify() for a Subrequirement within the programrequriements 
        [TestMethod]
        public void Modify_Subrequirements_SetValue()
        {

            Subrequirement sr = pr.Requirements.First().SubRequirements.First(sbr => sbr.Id == "18000");
            Assert.IsNull(sr.MinInstitutionalCredits);
            InstitutionalCreditModification icm = new InstitutionalCreditModification("18000", 95, "");
            icm.Modify(pr, additionalRequirements);
            Assert.AreEqual(95, sr.MinInstitutionalCredits);
            Assert.AreNotEqual(95, pr.MinimumInstitutionalCredits);
            Assert.AreNotEqual(95, pr.Requirements.First().MinInstitutionalCredits);

        }

        //Modify() for a group within the programrequriements 
        [TestMethod]
        public void Modify_Group_SetValue()
        {

            Group g = pr.Requirements.First().SubRequirements.First().Groups.First(gr => gr.Id == "10056");
            Assert.IsNull(g.MinInstitutionalCredits);
            InstitutionalCreditModification icm = new InstitutionalCreditModification("10056", 94, "");
            icm.Modify(pr, additionalRequirements);
            Assert.AreEqual(94, g.MinInstitutionalCredits);
            Assert.AreNotEqual(94, pr.MinimumInstitutionalCredits);
            Assert.AreNotEqual(94, pr.Requirements.First().MinInstitutionalCredits);
            Assert.AreNotEqual(94, pr.Requirements.First().SubRequirements.First().MinInstitutionalCredits);
        }


        //Modify() for a group within the additional requirements 
        [TestMethod]
        public async Task Modify_AdditionalRequirement_Group_SetValue()
        {            
            
            Requirement r =(await trr.GetAsync(new List<string>() { "Foo", "Bar" })).First();
            Subrequirement s = new Subrequirement("SUBID", "SUBCODE");
            s.Requirement = r;
            r.SubRequirements.Add(s);
            Group g = new Group("GROUPID", "GROUPCODE", s);
            s.Groups.Add(g);
            Assert.IsNull(g.MinInstitutionalCredits);

            additionalRequirements.Add(r);

            InstitutionalCreditModification icm = new InstitutionalCreditModification("GROUPID", 93, "");
            icm.Modify(pr, additionalRequirements);
            Assert.AreEqual(93, g.MinInstitutionalCredits);
            Assert.AreNotEqual(93, r.MinInstitutionalCredits);
            Assert.AreNotEqual(93, s.MinInstitutionalCredits);
            Assert.AreNotEqual(93, pr.Requirements.First().SubRequirements.First().Groups.First().MinInstitutionalCredits);
        }


    }
}
