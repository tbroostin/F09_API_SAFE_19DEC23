using System;
using Ellucian.Colleague.Domain.Finance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class ReceiptSessionTests
    {
        string sessionId;
        SessionStatus status;
        string cashierId;
        bool isECommerceEnabled;
        string locationId;
        DateTime rcptDate;
        DateTimeOffset start;
        DateTimeOffset? end;
        ReceiptSession result;

        [TestInitialize]
        public void Initialize()
        {
            sessionId = "123";
            status = SessionStatus.Open;
            cashierId = "0012345";
            isECommerceEnabled = true;
            locationId = "MC";
            rcptDate = DateTime.Today;
            start = new DateTimeOffset(rcptDate.Year, rcptDate.Month, rcptDate.Day, 8, 0, 0, new TimeSpan(-5, 0, 0));
            end = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceiptSession_Constructor_NullId()
        {
            result = new ReceiptSession(null, status, cashierId, rcptDate, start, isECommerceEnabled, locationId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceiptSession_Constructor_EmptyId()
        {
            result = new ReceiptSession(string.Empty, status, cashierId, rcptDate, start, isECommerceEnabled, locationId);
        }

        [TestMethod]
        public void ReceiptSession_Constructor_ValidId()
        {
            result = new ReceiptSession(sessionId, status, cashierId, rcptDate, start, isECommerceEnabled, locationId);
            Assert.AreEqual(sessionId, result.Id);
        }

        [TestMethod]
        public void ReceiptSession_Constructor_ValidStatus()
        {
            result = new ReceiptSession(sessionId, status, cashierId, rcptDate, start, isECommerceEnabled, locationId);
            Assert.AreEqual(status, result.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceiptSession_Constructor_NullCashierId()
        {
            result = new ReceiptSession(sessionId, status, null, rcptDate, start, isECommerceEnabled, locationId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceiptSession_Constructor_EmptyCashierId()
        {
            result = new ReceiptSession(sessionId, status, string.Empty, rcptDate, start, isECommerceEnabled, locationId);
        }

        [TestMethod]
        public void ReceiptSession_Constructor_ValidCashierId()
        {
            result = new ReceiptSession(sessionId, status, cashierId, rcptDate, start, isECommerceEnabled, locationId);
            Assert.AreEqual(cashierId, result.CashierId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceiptSession_Constructor_DefaultReceiptDate()
        {
            result = new ReceiptSession(sessionId, status, cashierId, default(DateTime), start, isECommerceEnabled, locationId);
        }

        [TestMethod]
        public void ReceiptSession_Constructor_ValidReceiptDate()
        {
            result = new ReceiptSession(sessionId, status, cashierId, rcptDate, start, isECommerceEnabled, locationId);
            Assert.AreEqual(rcptDate, result.ReceiptDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceiptSession_Constructor_DefaultStart()
        {
            result = new ReceiptSession(sessionId, status, cashierId, rcptDate, default(DateTimeOffset), isECommerceEnabled, locationId);
        }

        [TestMethod]
        public void ReceiptSession_Constructor_ValidStart()
        {
            result = new ReceiptSession(sessionId, status, cashierId, rcptDate, start, isECommerceEnabled, locationId);
            Assert.AreEqual(start, result.Start);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceiptSession_Constructor_NullLocationIdECommerceEnabled()
        {
            result = new ReceiptSession(sessionId, status, cashierId, rcptDate, start, true, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceiptSession_Constructor_EmptyLocationIdECommerceEnabled()
        {
            result = new ReceiptSession(sessionId, status, cashierId, rcptDate, start, true, string.Empty);
        }

        [TestMethod]
        public void ReceiptSession_Constructor_ValidLocationIdECommerceEnabled()
        {
            result = new ReceiptSession(sessionId, status, cashierId, rcptDate, start, true, locationId);
            Assert.AreEqual(locationId, result.LocationId);
        }

        [TestMethod]
        public void ReceiptSession_Constructor_NullLocationIdECommerceDisabled()
        {
            result = new ReceiptSession(sessionId, status, cashierId, rcptDate, start, false, null);
            Assert.AreEqual(null, result.LocationId);
        }

        [TestMethod]
        public void ReceiptSession_Constructor_ECommerceEnabled()
        {
            result = new ReceiptSession(sessionId, status, cashierId, rcptDate, start, true, locationId);
            Assert.IsTrue(result.IsECommerceEnabled);
        }

        [TestMethod]
        public void ReceiptSession_Constructor_ECommerceDisabled()
        {
            result = new ReceiptSession(sessionId, status, cashierId, rcptDate, start, false, null);
            Assert.IsFalse(result.IsECommerceEnabled);
        }

        [TestMethod]
        public void ReceiptSession_Constructor_EmptyLocationIdECommerceDisabled()
        {
            result = new ReceiptSession(sessionId, status, cashierId, rcptDate, start, false, string.Empty);
            Assert.AreEqual(string.Empty, result.LocationId);
        }

        [TestMethod]
        public void ReceiptSession_Constructor_ValidLocationIdECommerceDisabled()
        {
            result = new ReceiptSession(sessionId, status, cashierId, rcptDate, start, false, locationId);
            Assert.AreEqual(locationId, result.LocationId);
        }

        [TestMethod]
        public void ReceiptSession_End_ValidUpdate()
        {
            result = new ReceiptSession(sessionId, status, cashierId, rcptDate, start, isECommerceEnabled, locationId);
            result.End = end;
            Assert.AreEqual(end, result.End);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReceiptSession_End_InvalidUpdate()
        {
            result = new ReceiptSession(sessionId, status, cashierId, rcptDate, start, isECommerceEnabled, locationId);
            result.End = DateTimeOffset.Now;
            result.End = new DateTimeOffset();
        }
    }
}
