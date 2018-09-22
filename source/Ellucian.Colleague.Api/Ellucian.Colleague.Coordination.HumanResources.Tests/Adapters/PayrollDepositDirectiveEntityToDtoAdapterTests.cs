/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Web.Adapters;
using slf4net;
using Moq;
using Ellucian.Colleague.Coordination.HumanResources.Adapters;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Adapters
{
    [TestClass]
    public class PayrollDepositDirectiveEntityToDtoAdapterTests
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;
        public PayrollDepositDirectiveEntityToDtoAdapter adapter;

        public Dtos.Base.PayrollDepositDirective input;
        public PayrollDepositDirective output;        

        [TestInitialize]
        public void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();

            adapterRegistryMock.Setup(a => a.GetAdapter<Dtos.Base.BankAccountType, Domain.Base.Entities.BankAccountType>())
                .Returns(() => new AutoMapperAdapter<Dtos.Base.BankAccountType, Domain.Base.Entities.BankAccountType>(adapterRegistryMock.Object, loggerMock.Object));

            adapterRegistryMock.Setup(a => a.GetAdapter<Dtos.Base.Timestamp, Domain.Base.Entities.Timestamp>())
                .Returns(() => new AutoMapperAdapter<Dtos.Base.Timestamp, Domain.Base.Entities.Timestamp>(adapterRegistryMock.Object, loggerMock.Object));

            adapter = new PayrollDepositDirectiveEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
        }

        [TestMethod]
        public void DirectiveIsMappedForUSAccountAsExpected()
        {
            input = new Dtos.Base.PayrollDepositDirective()
            {
                Id = "001",
                RoutingId = "091000019",
                InstitutionId = null,
                BranchNumber = null,
                NewAccountId = null,
                AccountIdLastFour = "1235",
                BankAccountType = Dtos.Base.BankAccountType.Checking,
                DepositAmount = 12.13m,
                BankName = "nitgos",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(100),
                IsVerified = false,
                Nickname = "untitledi",
                PersonId = "24601",
                Priority = 1,
                Timestamp = new Dtos.Base.Timestamp()
                {
                    AddDateTime = DateTime.Now,
                    ChangeDateTime = DateTime.Now,
                    AddOperator = "24601",
                    ChangeOperator = "24601"
                }
            };
            output = adapter.MapToType(input);

            TestEquality(input, output);
        }

        [TestMethod]
        public void DirectiveIsMappedForCAAccountAsExpected()
        {
            input = new Dtos.Base.PayrollDepositDirective()
            {
                Id = "001",
                RoutingId = null,
                InstitutionId = "123",
                BranchNumber = "45679",
                NewAccountId = null,
                AccountIdLastFour = "1235",
                BankAccountType = Dtos.Base.BankAccountType.Checking,
                DepositAmount = 12.13m,
                BankName = "nitgos",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(100),
                IsVerified = false,
                Nickname = "untitledi",
                PersonId = "24601",
                Priority = 1,
                Timestamp = new Dtos.Base.Timestamp()
                {
                    AddDateTime = DateTime.Now,
                    ChangeDateTime = DateTime.Now,
                    AddOperator = "24601",
                    ChangeOperator = "24601"
                }
            };
            output = adapter.MapToType(input);

            TestEquality(input, output);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void NullInputDirectiveIsHandled()
        {
            input = null;
            adapter.MapToType(input);
        }

        [TestMethod]
        public void NewAccountIdIsSetIfNoneProvided()
        {
            input = new Dtos.Base.PayrollDepositDirective()
            {
                Id = "001",
                RoutingId = "091000019",
                InstitutionId = null,
                BranchNumber = null,
                NewAccountId = "Something New and Different!",
                AccountIdLastFour = "1235",
                BankAccountType = Dtos.Base.BankAccountType.Checking,
                DepositAmount = 12.13m,
                BankName = "nitgos",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(100),
                IsVerified = false,
                Nickname = "untitledi",
                PersonId = "24601",
                Priority = 1,
                Timestamp = new Dtos.Base.Timestamp()
                {
                    AddDateTime = DateTime.Now,
                    ChangeDateTime = DateTime.Now,
                    AddOperator = "24601",
                    ChangeOperator = "24601"
                }
            };
            output = adapter.MapToType(input);
            Assert.AreEqual(input.NewAccountId, output.NewAccountId);
        }   

        private void TestEquality(object input, object output)
        {
            foreach (var prop1 in input.GetType().GetProperties())
            {
                foreach (var prop2 in output.GetType().GetProperties())
                {                    
                    if (prop1.Name == prop2.Name)
                    {
                        var val1 = prop1.GetValue(input);
                        var val2 = prop2.GetValue(output);
                        if((val1 == null || val2 == null) && val1 != val2)
                        {
                            throw new Exception(string.Format("{0} and {1} are not equal",prop1.Name,prop2.Name));
                        }
                        else if (val1 != null && val2 != null)
                        {
                            if (val1.GetType() == val2.GetType())
                            {
                                Assert.AreEqual(val1, val2);
                            }
                            else
                            {
                                TestEquality(val1, val2);
                            }                            
                        }                                               
                    }
                }
            }
        }
    }
}
