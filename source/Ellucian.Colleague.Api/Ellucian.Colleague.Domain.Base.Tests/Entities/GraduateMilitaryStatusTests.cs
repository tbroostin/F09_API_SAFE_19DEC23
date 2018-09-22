using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
     [TestClass]
     public class GraduateMilitaryStatusTests
     {
          GraduateMilitaryStatus? status;

          [TestInitialize]
          public void Initialize()
          {
               status = new GraduateMilitaryStatus();
          }

          [TestMethod]
          public void CheckActiveMilitaryIsZero()
          {
               status = GraduateMilitaryStatus.ActiveMilitary;
               Assert.AreEqual(0, (int)status);
          }

          [TestMethod]
          public void CheckVeteranIsOne()
          {
               status = GraduateMilitaryStatus.Veteran;
               Assert.AreEqual(1, (int)status);
          }

          [TestMethod]
          public void CheckNoIsTwo()
          {
               status = GraduateMilitaryStatus.NotApplicable;
               Assert.AreEqual(2, (int)status);
          }

          [TestMethod]
          public void CheckZeroIsActiveMilitary()
          {
               status = 0;
               Assert.AreEqual(GraduateMilitaryStatus.ActiveMilitary, status);
          }

          [TestMethod]
          public void CheckOneIsVeteran()
          {
               status = (GraduateMilitaryStatus)1;
               Assert.AreEqual(GraduateMilitaryStatus.Veteran, status);
          }


     }
}
