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
    public class InstitutionalGpaModificationTests
    {
        private ProgramRequirements pr;
        private List<Requirement> requirements;
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
            requirements = (await trr.GetAsync(new List<string>() { })).ToList();  // caveat: this test repo's data doesn't match the others'
        }

        [TestMethod]
        public void Constructor()
        {
            InstitutionalGpaModification icm = new InstitutionalGpaModification("999", 99, "");
            Assert.AreEqual("999", icm.blockId);
            Assert.AreEqual(icm.Gpa, 99);
        }
        [TestMethod]
        public void Constructor_allowsNullBlock()
        {
            InstitutionalGpaModification icm = new InstitutionalGpaModification(null, 98, "");
            Assert.IsNull(icm.blockId);
            Assert.AreEqual(icm.Gpa, 98);
        }
        [TestMethod]
        public void Constructor_allowsNullCredits()
        {
            InstitutionalGpaModification icm = new InstitutionalGpaModification("997", null, "");
            Assert.AreEqual("997", icm.blockId);
            Assert.IsNull(icm.Gpa);
        }
        [TestMethod]
        public void Constructor_allowsNullBlockAndCredits()
        {
            InstitutionalGpaModification icm = new InstitutionalGpaModification(null, null, "");
            Assert.IsNull(icm.blockId);
            Assert.IsNull(icm.Gpa);
        }

        // Modify() for the Top level ProgramRequirements institutional cred
        [TestMethod]
        public void Modify_ProgramRequirements_SetValue()
        {
            Assert.AreEqual(2.0m, pr.MinInstGpa);  //default from test repo
            InstitutionalGpaModification icm = new InstitutionalGpaModification(null, 2.3m, "");
            icm.Modify(pr, new List<Requirement>());
            Assert.AreEqual(2.3m, pr.MinInstGpa);
        }

        [TestMethod]
        public void Modify_ProgramRequirements_SetNull()
        {
            // Starts null, set it to something else.
            InstitutionalGpaModification icm = new InstitutionalGpaModification(null, 2.4m, "");
            icm.Modify(pr, new List<Requirement>());
            Assert.AreEqual(2.4m, pr.MinInstGpa);
            // Now null it out
            InstitutionalGpaModification icm2 = new InstitutionalGpaModification(null, null, "");
            icm2.Modify(pr, new List<Requirement>());
            Assert.IsNull(pr.MinInstGpa);
        }
    }
}
