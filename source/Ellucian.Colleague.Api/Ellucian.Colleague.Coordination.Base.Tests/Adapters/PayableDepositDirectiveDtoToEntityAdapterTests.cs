/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Adapters
{
    [TestClass]
    public class PayableDepositDirectiveDtoToEntityAdapterTests
    {
        public Dtos.Base.PayableDepositDirective inputSource;
        public Domain.Base.Entities.PayableDepositDirective actualPayableDepositDirective
        {
            get
            {
                return adapterUnderTest.MapToType(inputSource);
            }
        }

        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;

        public PayableDepositDirectiveDtoToEntityAdapter adapterUnderTest;

        [TestInitialize]
        public void Initialize()
        {
            inputSource = new Dtos.Base.PayableDepositDirective()
            {
                Id = "0003914"
            };
        }

    }
}
