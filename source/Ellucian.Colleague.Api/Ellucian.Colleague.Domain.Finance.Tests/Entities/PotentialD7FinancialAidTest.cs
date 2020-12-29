using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class PotentialD7FinancialAidTest
    {
        private string _award = "AWARD";
        private string _awardDesc = "Award Description";
        private decimal _amount = 123.45m;

        /// <summary>
        /// Valid input constructs valid object
        /// </summary>
        [TestMethod]
        public void PotentialD7FinancialAid_Constructor_valid()
        {
            var x = new PotentialD7FinancialAid(_award, _awardDesc, _amount);
            Assert.AreEqual(x.AwardPeriodAward, _award);
            Assert.AreEqual(_awardDesc, x.AwardDescription);
            Assert.AreEqual(x.AwardAmount, _amount);
        }

        /// <summary>
        /// Null award throws exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PotentialD7FinancialAid_Constructor_NullAward()
        {
            var x = new PotentialD7FinancialAid((string)null, _awardDesc, _amount);
        }

        /// <summary>
        /// Empty award throws exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PotentialD7FinancialAid_Constructor_EmptyAward()
        {
            var x = new PotentialD7FinancialAid("", _awardDesc,_amount);
        }

        /// <summary>
        /// Null award throws exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PotentialD7FinancialAid_Constructor_NullDescription()
        {
            var x = new PotentialD7FinancialAid(_award, null, _amount);
        }

        /// <summary>
        /// Empty description throws exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PotentialD7FinancialAid_Constructor_EmptyDescription()
        {
            var x = new PotentialD7FinancialAid(_award, string.Empty, _amount);
        }
    }
}
