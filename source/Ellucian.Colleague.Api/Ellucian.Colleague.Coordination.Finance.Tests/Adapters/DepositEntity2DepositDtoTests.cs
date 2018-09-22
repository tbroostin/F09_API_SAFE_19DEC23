using System;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Tests.Adapters
{
    [TestClass]
    public class DepositEntity2DepositDtoTests
    {
        static Mock<ILogger> loggerMock = new Mock<ILogger>();
        static Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();
        static AutoMapperAdapter<Domain.Finance.Entities.Deposit, Dtos.Finance.Deposit> adapter = 
            new AutoMapperAdapter<Domain.Finance.Entities.Deposit,
            Dtos.Finance.Deposit>(adapterRegistryMock.Object, loggerMock.Object);

        Domain.Finance.Entities.Deposit _entDeposit;
        Dtos.Finance.Deposit _dtoDeposit;

        string _id = "DEPOSIT1";
        string _depositHolder = "Holder1";
        string _depositType = "TYPE1";
        DateTime _date = DateTime.Now.Date;
        decimal _amount = 10000;
        string _externalSystem = "ACME";
        string _externalIdentifier = "EXTERNAL1";
        string _receiptId = "RECEIPT1";
        string _termId = "TERM1";

        [TestInitialize]
        public void Initialize()
        {
            _entDeposit = new Domain.Finance.Entities.Deposit(_id, _depositHolder, _date, _depositType, _amount)
                {
                    TermId = _termId,
                    ReceiptId = _receiptId
                };
            _entDeposit.AddExternalSystemAndId(_externalSystem, _externalIdentifier);
        }

        // Id transfers correctly
        [TestMethod]
        public void DepositEntity2DepositDto_Id()
        {
            _dtoDeposit = adapter.MapToType(_entDeposit);
            Assert.AreEqual(_entDeposit.Id, _dtoDeposit.Id);
        }
        // Person transfers correctly
        [TestMethod]
        public void DepositEntity2DepositDto_PersonId()
        {
            _dtoDeposit = adapter.MapToType(_entDeposit);
            Assert.AreEqual(_entDeposit.PersonId, _dtoDeposit.PersonId);
        }
        // Date transfers correctly
        [TestMethod]
        public void DepositEntity2DepositDto_Date()
        {
            _dtoDeposit = adapter.MapToType(_entDeposit);
            Assert.AreEqual(_entDeposit.Date, _dtoDeposit.Date);
        }
        // Deposit type transfers correctly
        [TestMethod]
        public void DepositEntity2DepositDto_DepositType()
        {
            _dtoDeposit = adapter.MapToType(_entDeposit);
            Assert.AreEqual(_entDeposit.DepositType, _dtoDeposit.DepositType);
        }
        // Amount transfers correctly
        [TestMethod]
        public void DepositEntity2DepositDto_Amount()
        {
            _dtoDeposit = adapter.MapToType(_entDeposit);
            Assert.AreEqual(_entDeposit.Amount, _dtoDeposit.Amount);
        }
        // Term transfers correctly
        [TestMethod]
        public void DepositEntity2DepositDto_Term()
        {
            _dtoDeposit = adapter.MapToType(_entDeposit);
            Assert.AreEqual(_entDeposit.TermId, _dtoDeposit.TermId);
        }
        // Receipt transfers correctly
        [TestMethod]
        public void DepositEntity2DepositDto_Receipt()
        {
            _dtoDeposit = adapter.MapToType(_entDeposit);
            Assert.AreEqual(_entDeposit.ReceiptId, _dtoDeposit.ReceiptId);
        }
        // External system transfers correctly
        [TestMethod]
        public void DepositEntity2DepositDto_ExternalSystem()
        {
            _dtoDeposit = adapter.MapToType(_entDeposit);
            Assert.AreEqual(_entDeposit.ExternalSystem, _dtoDeposit.ExternalSystem);
        }
        // External id transfers correctly
        [TestMethod]
        public void DepositEntity2DepositDto_ExternalIdentifier()
        {
            _dtoDeposit = adapter.MapToType(_entDeposit);
            Assert.AreEqual(_entDeposit.ExternalIdentifier, _dtoDeposit.ExternalIdentifier);
        }
        // Null external system transfers correctly
        [TestMethod]
        public void DepositEntity2DepositDto_NullExternalSystem()
        {
            _entDeposit = new Domain.Finance.Entities.Deposit(_id, _depositHolder, _date, _depositType, _amount);
            _dtoDeposit = adapter.MapToType(_entDeposit);
            Assert.AreEqual(_entDeposit.ExternalSystem, _dtoDeposit.ExternalSystem);
        }
        // Null external identifier transfers correctly
        [TestMethod]
        public void DepositEntity2DepositDto_NullExternalIdentifier()
        {
            _entDeposit = new Domain.Finance.Entities.Deposit(_id, _depositHolder, _date, _depositType, _amount);
            _dtoDeposit = adapter.MapToType(_entDeposit);
            Assert.AreEqual(_entDeposit.ExternalIdentifier, _dtoDeposit.ExternalIdentifier);
        }
    }
}
