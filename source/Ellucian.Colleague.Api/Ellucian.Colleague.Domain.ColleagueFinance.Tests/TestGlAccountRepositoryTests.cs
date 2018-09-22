// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests
{
    [TestClass]
    public class TestGlAccountRepositoryTests
    {
        #region Initialize and Cleanup
        private TestGlAccountRepository testRepository;

        [TestInitialize]
        public void Initialize()
        {
            testRepository = new TestGlAccountRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            testRepository = null;
        }
        #endregion

        #region Setters
        [TestMethod]
        public void SetGlAccounts()
        {
            var seedAccounts = new List<string>()
            {
                "10_00_NP_00_NMK67_51001",
                "10_00_NP_01_NMK67_51001",
                "10_00_NP_02_NMK67_51001"
            };

            Assert.AreEqual(seedAccounts.Count, testRepository.SetGlNumbers(seedAccounts).AllGlNumbers.Count);
            foreach (var glAccount in seedAccounts)
            {
                Assert.IsTrue(testRepository.AllGlNumbers.Contains(glAccount));
            }
        }

        [TestMethod]
        public void ResetGlAccounts()
        {
            var seedAccounts = new List<string>()
            {
                "10_00_NP_00_NMK67_51001",
                "10_00_NP_01_NMK67_51001",
                "10_00_NP_02_NMK67_51001"
            };

            Assert.AreEqual(seedAccounts.Count, testRepository.SetGlNumbers(seedAccounts).AllGlNumbers.Count);
            testRepository.ResetGlNumbers();
            Assert.IsTrue(testRepository.AllGlNumbers.Count > seedAccounts.Count);
        }
        #endregion

        #region Getters
        [TestMethod]
        public void GetAllGlNumbers()
        {
            Assert.AreEqual(testRepository.AllGlNumbers.Count, testRepository.GetFilteredGlNumbers().Count);
        }

        [TestMethod]
        public void GetGlNumbersForOneCostCenter()
        {
            var expectedGlNumbers = testRepository.WithFund("10")
                .WithSource("00")
                .WithUnit("AJK55").GetFilteredGlNumbers();
            var actualGlNumbers = testRepository.GetGlNumbersForOneCostCenter();
            Assert.AreEqual(expectedGlNumbers.Count, actualGlNumbers.Count);
        }
        #endregion

        #region Filters
        [TestMethod]
        public void FilterByFund()
        {
            var fundValue = "10";
            var component = testRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUND_CODE);
            var expectedGlNumbers = testRepository.AllGlNumbers
                .Where(x => x.Substring(component.StartPosition, component.ComponentLength) == fundValue).ToList();
            var actualGlNumbers = testRepository.WithFund(fundValue).GetFilteredGlNumbers();
            Assert.AreEqual(expectedGlNumbers.Count, actualGlNumbers.Count);

            foreach (var expectedGlNumber in expectedGlNumbers)
            {
                var actualGlNumber = actualGlNumbers.FirstOrDefault(x => x == expectedGlNumber);

                Assert.IsNotNull(actualGlNumber);
            }
        }

        [TestMethod]
        public void FilterBySource()
        {
            var sourceValue = "01";
            var component = testRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.SOURCE_CODE);
            var expectedGlNumbers = testRepository.AllGlNumbers
                .Where(x => x.Substring(component.StartPosition, component.ComponentLength) == sourceValue).ToList();
            var actualGlNumbers = testRepository.WithSource(sourceValue).GetFilteredGlNumbers();
            Assert.AreEqual(expectedGlNumbers.Count, actualGlNumbers.Count);

            foreach (var expectedGlNumber in expectedGlNumbers)
            {
                var actualGlNumber = actualGlNumbers.FirstOrDefault(x => x == expectedGlNumber);

                Assert.IsNotNull(actualGlNumber);
                Assert.AreEqual(expectedGlNumber, actualGlNumber);
            }
        }

        [TestMethod]
        public void FilterByLocation()
        {
            var locationValue = "05";
            var component = testRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.LOCATION_CODE);
            var expectedGlNumbers = testRepository.AllGlNumbers
                .Where(x => x.Substring(component.StartPosition, component.ComponentLength) == locationValue).ToList();
            var actualGlNumbers = testRepository.WithLocation(locationValue).GetFilteredGlNumbers();
            Assert.AreEqual(expectedGlNumbers.Count, actualGlNumbers.Count);

            foreach (var expectedGlNumber in expectedGlNumbers)
            {
                var actualGlNumber = actualGlNumbers.FirstOrDefault(x => x == expectedGlNumber);

                Assert.IsNotNull(actualGlNumber);
                Assert.AreEqual(expectedGlNumber, actualGlNumber);
            }
        }

        [TestMethod]
        public void FilterByLocationSubclass()
        {
            var locationSubclassValue = "P";
            var component = testRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.LOCATION_SUBCLASS_CODE);
            var expectedGlNumbers = testRepository.AllGlNumbers
                .Where(x => x.Substring(component.StartPosition, component.ComponentLength) == locationSubclassValue).ToList();
            var actualGlNumbers = testRepository.WithLocationSubclass(locationSubclassValue).GetFilteredGlNumbers();
            Assert.AreEqual(expectedGlNumbers.Count, actualGlNumbers.Count);

            foreach (var expectedGlNumber in expectedGlNumbers)
            {
                var actualGlNumber = actualGlNumbers.FirstOrDefault(x => x == expectedGlNumber);

                Assert.IsNotNull(actualGlNumber);
                Assert.AreEqual(expectedGlNumber, actualGlNumber);
            }
        }

        [TestMethod]
        public void FilterByFunction()
        {
            var functionValue = "07";
            var component = testRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUNCTION_CODE);
            var expectedGlNumbers = testRepository.AllGlNumbers
                .Where(x => x.Substring(component.StartPosition, component.ComponentLength) == functionValue).ToList();
            var actualGlNumbers = testRepository.WithFunction(functionValue).GetFilteredGlNumbers();
            Assert.AreEqual(expectedGlNumbers.Count, actualGlNumbers.Count);

            foreach (var expectedGlNumber in expectedGlNumbers)
            {
                var actualGlNumber = actualGlNumbers.FirstOrDefault(x => x == expectedGlNumber);

                Assert.IsNotNull(actualGlNumber);
                Assert.AreEqual(expectedGlNumber, actualGlNumber);
            }
        }

        [TestMethod]
        public void FilterByUnit()
        {
            var unitValue = "33333";
            var component = testRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.UNIT_CODE);
            var expectedGlNumbers = testRepository.AllGlNumbers
                .Where(x => x.Substring(component.StartPosition, component.ComponentLength) == unitValue).ToList();
            var actualGlNumbers = testRepository.WithUnit(unitValue).GetFilteredGlNumbers();
            Assert.AreEqual(expectedGlNumbers.Count, actualGlNumbers.Count);

            foreach (var expectedGlNumber in expectedGlNumbers)
            {
                var actualGlNumber = actualGlNumbers.FirstOrDefault(x => x == expectedGlNumber);

                Assert.IsNotNull(actualGlNumber);
                Assert.AreEqual(expectedGlNumber, actualGlNumber);
            }
        }

        [TestMethod]
        public void FilterByUnitSubclass()
        {
            var unitSubclassValue = "AJK";
            var component = testRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.UNIT_SUBCLASS_CODE);
            var expectedGlNumbers = testRepository.AllGlNumbers
                .Where(x => x.Substring(component.StartPosition, component.ComponentLength) == unitSubclassValue).ToList();
            var actualGlNumbers = testRepository.WithUnitSubclass(unitSubclassValue).GetFilteredGlNumbers();
            Assert.AreEqual(expectedGlNumbers.Count, actualGlNumbers.Count);

            foreach (var expectedGlNumber in expectedGlNumbers)
            {
                var actualGlNumber = actualGlNumbers.FirstOrDefault(x => x == expectedGlNumber);

                Assert.IsNotNull(actualGlNumber);
                Assert.AreEqual(expectedGlNumber, actualGlNumber);
            }
        }

        [TestMethod]
        public void FilterByObject()
        {
            var objectValue = "51001";
            var component = testRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.OBJECT_CODE);
            var expectedGlNumbers = testRepository.AllGlNumbers
                .Where(x => x.Substring(component.StartPosition, component.ComponentLength) == objectValue).ToList();
            var actualGlNumbers = testRepository.WithObject(objectValue).GetFilteredGlNumbers();
            Assert.AreEqual(expectedGlNumbers.Count, actualGlNumbers.Count);

            foreach (var expectedGlNumber in expectedGlNumbers)
            {
                var actualGlNumber = actualGlNumbers.FirstOrDefault(x => x == expectedGlNumber);

                Assert.IsNotNull(actualGlNumber);
                Assert.AreEqual(expectedGlNumber, actualGlNumber);
            }
        }

        [TestMethod]
        public void FilterByGlSubclass()
        {
            var glSubclassValue = "41";
            var component = testRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.GL_SUBSCLASS_CODE);
            var expectedGlNumbers = testRepository.AllGlNumbers
                .Where(x => x.Substring(component.StartPosition, component.ComponentLength) == glSubclassValue).ToList();
            var actualGlNumbers = testRepository.WithGlSubclass(glSubclassValue).GetFilteredGlNumbers();
            Assert.AreEqual(expectedGlNumbers.Count, actualGlNumbers.Count);

            foreach (var expectedGlNumber in expectedGlNumbers)
            {
                Assert.IsTrue(actualGlNumbers.Contains(expectedGlNumber));
            }
        }

        [TestMethod]
        public void FilterByUnitAndObject()
        {
            var unitValue = "44444";
            var objectValue = "51001";
            var unitComponent = testRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.UNIT_CODE);
            var objectComponent = testRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.OBJECT_CODE);

            var expectedGlNumbers = testRepository.AllGlNumbers
                .Where(x => x.Substring(unitComponent.StartPosition, unitComponent.ComponentLength) == unitValue
                    && x.Substring(objectComponent.StartPosition, objectComponent.ComponentLength) == objectValue).ToList();
            var actualGlNumbers = testRepository.WithUnit(unitValue).WithObject(objectValue).GetFilteredGlNumbers();
            Assert.AreEqual(expectedGlNumbers.Count, actualGlNumbers.Count);

            foreach (var expectedGlNumber in expectedGlNumbers)
            {
                var actualGlNumber = actualGlNumbers.FirstOrDefault(x => x == expectedGlNumber);

                Assert.IsNotNull(actualGlNumber);
                Assert.AreEqual(expectedGlNumber, actualGlNumber);
            }
        }

        [TestMethod]
        public void SubsequentFilters()
        {
            // Filter by fund
            var fundValue = "10";
            var sourceValue = "00";
            var locationValue = "P1";
            var locationSubclassValue = "P";
            var functionValue = "U1";
            var unitValue = "33333";
            var unitSubclassValue = "AJK";
            var objectValue = "51001";
            var fundComponent = testRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUND_CODE);
            var sourceComponent = testRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.SOURCE_CODE);
            var locationComponent = testRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.LOCATION_CODE);
            var locationSubclassComponent = testRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.LOCATION_SUBCLASS_CODE);
            var functionComponent = testRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUNCTION_CODE);
            var unitComponent = testRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.UNIT_CODE);
            var unitSubclassComponent = testRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.UNIT_SUBCLASS_CODE);
            var objectComponent = testRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.OBJECT_CODE);
            var expectedGlNumbers = testRepository.AllGlNumbers.Where(x =>
                x.Substring(fundComponent.StartPosition, fundComponent.ComponentLength) == fundValue
                && x.Substring(sourceComponent.StartPosition, sourceComponent.ComponentLength) == sourceValue
                && x.Substring(locationComponent.StartPosition, locationComponent.ComponentLength) == locationValue
                && x.Substring(locationSubclassComponent.StartPosition, locationSubclassComponent.ComponentLength) == locationSubclassValue
                && x.Substring(functionComponent.StartPosition, functionComponent.ComponentLength) == functionValue
                && x.Substring(unitComponent.StartPosition, unitComponent.ComponentLength) == unitValue
                && x.Substring(unitSubclassComponent.StartPosition, unitSubclassComponent.ComponentLength) == unitSubclassValue
                && x.Substring(objectComponent.StartPosition, objectComponent.ComponentLength) == objectValue).ToList();
            var actualGlNumbers = testRepository.WithFund(fundValue)
                .WithSource(sourceValue)
                .WithLocation(locationValue)
                .WithLocationSubclass(locationSubclassValue)
                .WithFunction(functionValue)
                .WithUnit(unitValue)
                .WithUnitSubclass(unitSubclassValue)
                .WithObject(objectValue)
                .GetFilteredGlNumbers();
            Assert.AreEqual(expectedGlNumbers.Count, actualGlNumbers.Count);

            foreach (var expectedGlNumber in expectedGlNumbers)
            {
                var actualGlNumber = actualGlNumbers.FirstOrDefault(x => x == expectedGlNumber);

                Assert.IsNotNull(actualGlNumber);
                Assert.AreEqual(expectedGlNumber, actualGlNumber);
            }

            // Now filter by function
            functionValue = "00";
            fundComponent = testRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUNCTION_CODE);
            expectedGlNumbers = testRepository.AllGlNumbers
                .Where(x => x.Substring(fundComponent.StartPosition, fundComponent.ComponentLength) == functionValue).ToList();
            actualGlNumbers = testRepository.WithFunction(functionValue).GetFilteredGlNumbers();
            Assert.AreEqual(expectedGlNumbers.Count, actualGlNumbers.Count);

            foreach (var expectedGlNumber in expectedGlNumbers)
            {
                var actualGlNumber = actualGlNumbers.FirstOrDefault(x => x == expectedGlNumber);

                Assert.IsNotNull(actualGlNumber);
                Assert.AreEqual(expectedGlNumber, actualGlNumber);
            }
        }
        #endregion
    }
}