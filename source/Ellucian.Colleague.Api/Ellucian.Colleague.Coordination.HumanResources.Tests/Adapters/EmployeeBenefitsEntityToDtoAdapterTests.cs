/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Coordination.HumanResources.Adapters;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Adapters
{
    [TestClass]
    public class EmployeeBenefitsEntityToDtoAdapterTests
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;
        public EmployeeBenefitsEntityToDtoAdapter adapter;

        public Dtos.HumanResources.EmployeeBenefits EmpBenefitsDTO;
        public EmployeeBenefits EmpBenefitsEntity;

        #region Empolyee Benefits Details
        public string PersonId { get; set; }
        public string AdditionalInformation { get; set; }
        public List<CurrentBenefit> CurrentBenefits { get; set; }
        #endregion

        public void EmployeeBenefitsInitialize()
        {
            PersonId = "0014697";
            AdditionalInformation = "Ellucian University also provides the following non-contributory benefits to all employees: Short Term Disability, Long Term Disability, Employee Assistance Program, Tuition Reimbursement, Credit Union, Car Buying Service, Fitness Benefit up to $130/year, Fitness Facility, Entertainment Benefit up to $75/year, Free Tickets Program.";
            CurrentBenefits = new List<CurrentBenefit>()
            {
                new CurrentBenefit(
                    "Dental Employee Plus One",
                    "Employee plus One",
                    "$8.65",
                    new List<string>()
                    {
                        "Jason Richerdson",
                        "Mark Tester"
                    },
                    new List<string>()
                    {
                        "Self - Rick Dalton #1729",
                         "Jason Richerdson - Subanna and Co #98765"
                    },
                    new List<string>()
                    {
                        "Spike Stubin 100% (Beneficiary)"
                    }
                )
            };
            EmpBenefitsEntity = new EmployeeBenefits(PersonId, AdditionalInformation, CurrentBenefits);
        }

        [TestInitialize]
        public void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();

            adapterRegistryMock.Setup(a => a.GetAdapter<Dtos.HumanResources.CurrentBenefit, Domain.HumanResources.Entities.CurrentBenefit>())
                .Returns(() => new AutoMapperAdapter<Dtos.HumanResources.CurrentBenefit, Domain.HumanResources.Entities.CurrentBenefit>(adapterRegistryMock.Object, loggerMock.Object));


            EmployeeBenefitsInitialize();

            adapter = new EmployeeBenefitsEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

            EmpBenefitsDTO = adapter.MapToType(EmpBenefitsEntity);
        }

        [TestMethod]
        public void EmployeeBenefitsAdpater_BasicPropertiesTests()
        {
            Assert.AreEqual(EmpBenefitsEntity.PersonId, EmpBenefitsDTO.PersonId);
            Assert.AreEqual(EmpBenefitsEntity.AdditionalInformation, EmpBenefitsDTO.AdditionalInformation);
        }

        [TestMethod]
        public void EmployeeBenefitsAdpater_CurrentBenefitsTests()
        {
            Assert.IsNotNull(EmpBenefitsDTO.CurrentBenefits);
            Assert.AreEqual(EmpBenefitsEntity.CurrentBenefits.Count(), EmpBenefitsDTO.CurrentBenefits.Count());

            var benefitDescription = "Dental Employee Plus One";
            Assert.IsTrue(EmpBenefitsDTO.CurrentBenefits.Select(b => b.BenefitDescription).Contains(benefitDescription));

        }
    }
}
