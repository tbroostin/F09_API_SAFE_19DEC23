using Ellucian.Colleague.Coordination.Finance.Adapters;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Tests.Adapters
{
    [TestClass]
    public class NonCashPaymentEntity2Dto
    {
        string _payMethod = "PM01";
        decimal _amount = 100;

        Domain.Finance.Entities.NonCashPayment _entPayment1;
        Dtos.Finance.NonCashPayment _dtoPayment1;
        NonCashPaymentEntityAdapter adapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapter = new NonCashPaymentEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
        }

        [TestMethod]
        public void NonCashPayment_PaymentMethodAndAmount()
        {
            _entPayment1 = new Domain.Finance.Entities.NonCashPayment(_payMethod, _amount);
            _dtoPayment1 = adapter.MapToType(_entPayment1);
        }

        
    }
}
