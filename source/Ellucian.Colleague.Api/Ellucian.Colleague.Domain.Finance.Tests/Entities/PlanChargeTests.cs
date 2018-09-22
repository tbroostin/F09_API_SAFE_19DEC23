// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class PlanChargeTests
    {
        string planId = "12345";
        Charge charge = new Charge("45678", "123456789", new List<String>() { "Materials Fee" }, "MATFE", 125);
        Charge charge2 = new Charge("56789", "234567890", new List<String>() { "Materials Fee" }, "MATFE", 125);
        decimal amount = 152.40m;
        bool isSetupCharge = true;
        bool isAutomaticallyModifiable = true;

        [TestMethod]
        public void PlanCharge_Constructor_ValidIdWithNonNullPlanId()
        {
            var result = new PlanCharge(planId, charge, amount, isSetupCharge, isAutomaticallyModifiable);

            Assert.AreEqual(planId + "*" + charge.Id, result.Id);
        }

        [TestMethod]
        public void PlanCharge_Constructor_ValidIdWithNullPlanId()
        {
            var result = new PlanCharge(null, charge, amount, isSetupCharge, isAutomaticallyModifiable);

            Assert.AreEqual(null, result.Id);
        }

        [TestMethod]
        public void PlanCharge_Constructor_ValidPaymentPlanId()
        {
            var result = new PlanCharge(planId, charge, amount, isSetupCharge, isAutomaticallyModifiable);

            Assert.AreEqual(planId, result.PlanId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PlanCharge_Constructor_NullCharge()
        {
            var result = new PlanCharge(planId, null, amount, isSetupCharge, isAutomaticallyModifiable);
        }

        [TestMethod]
        public void PlanCharge_Constructor_ValidCharge()
        {
            var result = new PlanCharge(planId, charge, amount, isSetupCharge, isAutomaticallyModifiable);

            Assert.AreEqual(charge, result.Charge);
        }

        [TestMethod]
        public void PlanCharge_Constructor_ValidAmount()
        {
            var result = new PlanCharge(planId, charge, amount, isSetupCharge, isAutomaticallyModifiable);

            Assert.AreEqual(amount, result.Amount);
        }

        [TestMethod]
        public void PlanCharge_Constructor_SetupChargeTrue()
        {
            var result = new PlanCharge(planId, charge, amount, true, isAutomaticallyModifiable);

            Assert.IsTrue(result.IsSetupCharge);
        }

        [TestMethod]
        public void PlanCharge_Constructor_SetupChargeFalse()
        {
            var result = new PlanCharge(planId, charge, amount, false, isAutomaticallyModifiable);

            Assert.IsFalse(result.IsSetupCharge);
        }

        [TestMethod]
        public void PlanCharge_Constructor_AllowsPlanModificationTrue()
        {
            var result = new PlanCharge(planId, charge, amount, isSetupCharge, true);

            Assert.IsTrue(result.IsAutomaticallyModifiable);
        }

        [TestMethod]
        public void PlanCharge_Constructor_AllowsPlanModificationFalse()
        {
            var result = new PlanCharge(planId, charge, amount, isSetupCharge, false);

            Assert.IsFalse(result.IsAutomaticallyModifiable);
        }

        [TestMethod]
        public void PlanCharge_Equals_NullPlanCharge()
        {
            var result = new PlanCharge(planId, charge, amount, isSetupCharge, false);
            PlanCharge pc2 = null;
            Assert.IsFalse(result.Equals(pc2));
        }

        [TestMethod]
        public void PlanCharge_Equals_NonPlanChargeObject()
        {
            var result = new PlanCharge(planId, charge, amount, isSetupCharge, false);
            var nonPc = "abc123";
            Assert.IsFalse(result.Equals(nonPc));
        }

        [TestMethod]
        public void PlanCharge_Equals_SamePlanIdDifferentChargeId()
        {
            var pc1 = new PlanCharge(planId, charge, amount, isSetupCharge, false);
            var pc2 = new PlanCharge(planId, charge2, amount, isSetupCharge, false);
            Assert.IsFalse(pc1.Equals(pc2));
        }

        [TestMethod]
        public void PlanCharge_Equals_DifferentPlanIdSameChargeId()
        {
            var pc1 = new PlanCharge(planId, charge, amount, isSetupCharge, false);
            var pc2 = new PlanCharge("ABCS", charge, amount, isSetupCharge, false);
            Assert.IsFalse(pc1.Equals(pc2));
        }

        [TestMethod]
        public void PlanCharge_Equals_NullPlanIdSameChargeId()
        {
            var pc1 = new PlanCharge(null, charge, amount, isSetupCharge, false);
            var pc2 = new PlanCharge("ABCS", charge, amount, isSetupCharge, false);
            Assert.IsFalse(pc1.Equals(pc2));
        }

        [TestMethod]
        public void PlanCharge_Equals_EmptyPlanIdSameChargeId()
        {
            var pc1 = new PlanCharge(string.Empty, charge, amount, isSetupCharge, false);
            var pc2 = new PlanCharge("ABCS", charge, amount, isSetupCharge, false);
            Assert.IsFalse(pc1.Equals(pc2));
        }

        [TestMethod]
        public void PlanCharge_Equals_NullOtherPlanIdSameChargeId()
        {
            var pc1 = new PlanCharge(null, charge, amount, isSetupCharge, false);
            var pc2 = new PlanCharge("ABCS", charge, amount, isSetupCharge, false);
            Assert.IsFalse(pc2.Equals(pc1));
        }

        [TestMethod]
        public void PlanCharge_Equals_EmptyOtherPlanIdSameChargeId()
        {
            var pc1 = new PlanCharge(string.Empty, charge, amount, isSetupCharge, false);
            var pc2 = new PlanCharge("ABCS", charge, amount, isSetupCharge, false);
            Assert.IsFalse(pc2.Equals(pc1));
        }

        [TestMethod]
        public void PlanCharge_Equals_SamePlanIdSameChargeId()
        {
            var pc1 = new PlanCharge(planId, charge, amount, isSetupCharge, false);
            var pc2 = new PlanCharge(planId, charge, 250m, true, true);
            Assert.IsTrue(pc1.Equals(pc2));
        }

        [TestMethod]
        public void PlanCharge_GetHashCode_SameCodeHashEqual()
        {
            var pc1 = new PlanCharge(planId, charge, amount, isSetupCharge, false);
            var pc2 = new PlanCharge(planId, charge, 250m, true, true);
            Assert.AreEqual(pc1.GetHashCode(), pc2.GetHashCode());
        }

        [TestMethod]
        public void PlanCharge_GetHashCode_DifferentCodeHashNotEqual()
        {
            var pc1 = new PlanCharge(string.Empty, charge, amount, isSetupCharge, false);
            var pc2 = new PlanCharge("ABCS", charge, amount, isSetupCharge, false);
            Assert.AreNotEqual(pc1.GetHashCode(), pc2.GetHashCode());
        }

        [TestMethod]
        public void PlanCharge_Id_Set()
        {
            var pc1 = new PlanCharge(null, charge, amount, isSetupCharge, false); 
            pc1.PlanId = planId;
            Assert.AreEqual(planId, pc1.PlanId);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PlanCharge_Id_SetNotAllowed()
        {
            var pc1 = new PlanCharge(planId, charge, amount, isSetupCharge, false);
            pc1.PlanId = planId + "A";
        }
    }
}
