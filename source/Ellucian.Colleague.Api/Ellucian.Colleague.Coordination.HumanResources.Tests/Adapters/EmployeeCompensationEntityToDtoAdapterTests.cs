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
    public class EmployeeCompensationEntityToDtoAdapterTests
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;
        public EmployeeCompensationEntityToDtoAdapter adapter;

        public Dtos.HumanResources.EmployeeCompensation EmpCompensationDTO;
        public EmployeeCompensation EmpCompensationEntity;


        #region EmployeeCompensation Details
        public string PersonId { get; set; }

        public string OtherBenefits { get; set; }

        public string DisplayEmployeeCosts { get; set; }

        public string TotalCompensationPageHeader { get; set; }

        public decimal? SalaryAmount { get; set; }

        public List<EmployeeBended> Bended { get; set; }

        public List<EmployeeTax> Taxes { get; set; }

        public List<EmployeeStipend> Stipends { get; set; }

        #endregion

       
        public void InitializeEmployeeCompensation()
        {
            PersonId = "0014697";
            OtherBenefits = "Additional benefits are available for all full-time employees. These benefits, including free concert and sports tickets, free parking, and use of the athletic facilities, are not listed on this form.";
            DisplayEmployeeCosts = "Y";
            TotalCompensationPageHeader = "This Total Compensation Statement is intended to summarize the estimated value of your current benefits. While every effort has been taken to accurately report this information, discrepancies are possible.";
            SalaryAmount = 19200.00m;
            Bended = new List<EmployeeBended>()
            {
                new EmployeeBended("DEP1","Dental Employee Plus One",780.72m,207.48m),
                new EmployeeBended("MEDE","Medical - Employee Only",3415.44m,379.56m)
            };
            Taxes = new List<EmployeeTax>()
            {
                new EmployeeTax("FICA","FICA Withholding",1154m,1154m),
                new EmployeeTax("FWHM","Federal Withholding - Married",null,681.30m)
            };

            Stipends = new List<EmployeeStipend>()
            {
                new EmployeeStipend("Restricted Stipend",1200.00m),
                new EmployeeStipend("Test GL  Distribution",1000.00m)
            };

            EmpCompensationEntity = new EmployeeCompensation(PersonId, OtherBenefits, DisplayEmployeeCosts, TotalCompensationPageHeader, SalaryAmount, Bended, Taxes, Stipends);
        }

        [TestInitialize]
        public void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();

            adapterRegistryMock.Setup(a => a.GetAdapter<Dtos.HumanResources.EmployeeBended, Domain.HumanResources.Entities.EmployeeBended>())
                .Returns(() => new AutoMapperAdapter<Dtos.HumanResources.EmployeeBended, Domain.HumanResources.Entities.EmployeeBended>(adapterRegistryMock.Object, loggerMock.Object));

            adapterRegistryMock.Setup(a => a.GetAdapter<Dtos.HumanResources.EmployeeTax, Domain.HumanResources.Entities.EmployeeTax>())
             .Returns(() => new AutoMapperAdapter<Dtos.HumanResources.EmployeeTax, Domain.HumanResources.Entities.EmployeeTax>(adapterRegistryMock.Object, loggerMock.Object));

            adapterRegistryMock.Setup(a => a.GetAdapter<Dtos.HumanResources.EmployeeStipend, Domain.HumanResources.Entities.EmployeeStipend>())
                        .Returns(() => new AutoMapperAdapter<Dtos.HumanResources.EmployeeStipend, Domain.HumanResources.Entities.EmployeeStipend>(adapterRegistryMock.Object, loggerMock.Object));

            InitializeEmployeeCompensation();

            adapter = new EmployeeCompensationEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

            EmpCompensationDTO = adapter.MapToType(EmpCompensationEntity);
        }

        [TestMethod]
        public void EmployeeCompensationAdpater_BasicPropertiesTests()
        {
            Assert.AreEqual(EmpCompensationEntity.PersonId, EmpCompensationDTO.PersonId);
            Assert.AreEqual(EmpCompensationEntity.OtherBenefits, EmpCompensationDTO.OtherBenefits);
            Assert.AreEqual(EmpCompensationEntity.DisplayEmployeeCosts, EmpCompensationDTO.DisplayEmployeeCosts);
            Assert.AreEqual(EmpCompensationEntity.TotalCompensationPageHeader, EmpCompensationDTO.TotalCompensationPageHeader);
            Assert.AreEqual(EmpCompensationEntity.SalaryAmount, EmpCompensationDTO.SalaryAmount);
                       
        }

        [TestMethod]
        public void EmployeeCompensationAdpater_EmpBendedTests()
        {
            Assert.IsNotNull(EmpCompensationDTO.Bended);
            Assert.AreEqual(EmpCompensationEntity.Bended.Count(), EmpCompensationDTO.Bended.Count());

            var bendedCode = "MEDE";
            Assert.IsTrue(EmpCompensationDTO.Bended.Select(b => b.BenededCode).Contains(bendedCode));
            
        }

        [TestMethod]
        public void EmployeeCompensationAdpater_EmpTaxTests()
        {
            Assert.IsNotNull(EmpCompensationDTO.Taxes);
            Assert.AreEqual(EmpCompensationEntity.Taxes.Count(), EmpCompensationDTO.Taxes.Count());

            var taxCode = "FICA";
            Assert.IsTrue(EmpCompensationDTO.Taxes.Select(t => t.TaxCode).Contains(taxCode));

        }

        [TestMethod]
        public void EmployeeCompensationAdpater_EmpStipendsTests()
        {
            Assert.IsNotNull(EmpCompensationDTO.Stipends);
            Assert.AreEqual(EmpCompensationEntity.Stipends.Count(), EmpCompensationDTO.Stipends.Count());

            var stipendDesc = "Restricted Stipend";
            var actual = EmpCompensationDTO.Stipends.Where(s => s.StipendDescription == stipendDesc).FirstOrDefault().StipendAmount;
            Assert.AreEqual(1200.00m,actual);

        }
       
    }
}
