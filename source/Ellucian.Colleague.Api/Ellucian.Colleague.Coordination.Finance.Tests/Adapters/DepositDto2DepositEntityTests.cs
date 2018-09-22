using System;
using Ellucian.Colleague.Coordination.Finance.Adapters;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Tests.Adapters
{
    [TestClass]
    public class DepositDto2DepositEntityTests
    {
        static Mock<ILogger> loggerMock = new Mock<ILogger>();
        static Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();
        static DepositDtoAdapter adapter = new DepositDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

        Domain.Finance.Entities.Deposit _entDeposit;
        Dtos.Finance.Deposit _dtoDeposit;

        string _id = "DEPOSIT1";
        string _depositHolder = "Holder1";
        string _depositType = "TYPE1";
        DateTime _date = DateTime.Now.Date;
        decimal _amount = 10000;
        string _externalSystem = "ACME";
        string _externalIdentifier = "EXTERNAL1";
        string _termId = "TERM1";
        string _receiptId = "RECEIPT1";

        [TestInitialize]
        public void Initialize()
        {
            _dtoDeposit = new Dtos.Finance.Deposit()
            {
                Id = _id,
                PersonId = _depositHolder,
                Date = _date,
                DepositType = _depositType,
                Amount = _amount,
                ExternalIdentifier = _externalIdentifier,
                ExternalSystem = _externalSystem,
                TermId = _termId,
                ReceiptId = _receiptId,
            };
        }

        // Id transfers correctly
        [TestMethod]
        public void DepositDto2DepositEntity_Id()
        {
            _entDeposit = adapter.MapToType(_dtoDeposit);
            Assert.AreEqual(_dtoDeposit.Id, _entDeposit.Id);
        }
        // Person Id transfers correctly
        [TestMethod]
        public void DepositDto2DepositEntity_Person()
        {
            _entDeposit = adapter.MapToType(_dtoDeposit);
            Assert.AreEqual(_dtoDeposit.PersonId, _entDeposit.PersonId);
        }
        // Date transfers correctly
        [TestMethod]
        public void DepositDto2DepositEntity_Date()
        {
            _entDeposit = adapter.MapToType(_dtoDeposit);
            Assert.AreEqual(_dtoDeposit.Date, _entDeposit.Date);
        }
        // Deposit type transfers correctly
        [TestMethod]
        public void DepositDto2DepositEntity_DepositType()
        {
            _entDeposit = adapter.MapToType(_dtoDeposit);
            Assert.AreEqual(_dtoDeposit.DepositType, _entDeposit.DepositType);
        }
        // Amount transfers correctly
        [TestMethod]
        public void DepositDto2DepositEntity_Amount()
        {
            _entDeposit = adapter.MapToType(_dtoDeposit);
            Assert.AreEqual(_dtoDeposit.Amount, _entDeposit.Amount);
        }
        // Term transfers correctly
        [TestMethod]
        public void DepositDto2DepositEntity_Term()
        {
            _entDeposit = adapter.MapToType(_dtoDeposit);
            Assert.AreEqual(_dtoDeposit.TermId, _entDeposit.TermId);
        }
        // receipt Id transfers correctly
        [TestMethod]
        public void DepositDto2DepositEntity_Receipt()
        {
            _entDeposit = adapter.MapToType(_dtoDeposit);
            Assert.AreEqual(_dtoDeposit.ReceiptId, _entDeposit.ReceiptId);
        }
        // External information transfers correctly when both null
        [TestMethod]
        public void DepositDto2DepositEntity_ExternalInformationIsNull()
        {
            _dtoDeposit = new Dtos.Finance.Deposit()
            {
                Id = _id,
                PersonId = _depositHolder,
                Date = _date,
                DepositType = _depositType,
                Amount = _amount,
            };
            _entDeposit = adapter.MapToType(_dtoDeposit);
        }
        // Exception when external system null and external identifier valued
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DepositDto2DepositEntity_ExternalSystemNullAndExternalIdValued()
        {
            _dtoDeposit = new Dtos.Finance.Deposit()
            {
                Id = _id,
                PersonId = _depositHolder,
                Date = _date,
                DepositType = _depositType,
                Amount = _amount,
                ExternalIdentifier = _externalIdentifier,
            };
            _entDeposit = adapter.MapToType(_dtoDeposit);
        }
        // Exception when external system valued and external identifier is null
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DepositDto2DepositEntity_ExternalSystemValuedAndExternalIdNull()
        {
            _dtoDeposit = new Dtos.Finance.Deposit()
            {
                Id = _id,
                PersonId = _depositHolder,
                Date = _date,
                DepositType = _depositType,
                Amount = _amount,
                ExternalSystem = _externalSystem,
            };
            _entDeposit = adapter.MapToType(_dtoDeposit);
        }
    }
}
