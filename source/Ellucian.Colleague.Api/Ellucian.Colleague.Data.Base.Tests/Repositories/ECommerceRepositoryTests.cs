using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Tests;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class ECommerceRepositoryTests : BaseRepositorySetup
    {
        ECommerceRepository repository;

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            // Build the test repository
            repository = new ECommerceRepository(cacheProvider, transFactory, logger);
        }

        [TestClass]
        public class ECommerceRepository_ConvenienceFees : ECommerceRepositoryTests
        {
            private Collection<ConvenienceFees> convFees;
            private List<ConvenienceFee> result;
                
            [TestInitialize]
            public void ECommerceRepository_ConvenienceFees_Initialize()
            {
                convFees = TestConvenienceFeesRepository.ConvenienceFees;
                MockRecords("CONVENIENCE.FEE", convFees);
                result = repository.ConvenienceFees.ToList();
            }

            [TestMethod]
            public void ECommerceRepository_ConvenienceFees_NotFound()
            {
                var convFee = result.FirstOrDefault(x => x.Code == "XYZ");
                Assert.IsNull(convFee);
            }

            [TestMethod]
            public void ECommerceRepository_ConvenienceFees_MatchingCount()
            {
                Assert.AreEqual(convFees.Count, result.Count());
            }

            [TestMethod]
            public void ECommerceRepository_ConvenienceFees_CorrectType()
            {
                CollectionAssert.AllItemsAreInstancesOfType(result, typeof(ConvenienceFee));
            }

            [TestMethod]
            public void ECommerceRepository_ConvenienceFees_MatchingKeys()
            {
                var sourceData = convFees.Select(cf => cf.Recordkey).ToList();
                var resultData = result.Select(cf => cf.Code).ToList();
                CollectionAssert.AreEqual(sourceData, resultData);
            }

            [TestMethod]
            public void ECommerceRepository_ConvenienceFees_MatchingDescriptions()
            {
                var sourceData = convFees.Select(cf => cf.ConvfDescription).ToList();
                var resultData = result.Select(cf => cf.Description).ToList();
                CollectionAssert.AreEqual(sourceData, resultData);
            }

            [TestMethod]
            public void ECommerceRepository_ConvenienceFees_VerifyCacheNoLock()
            {
                string cacheKey = repository.BuildFullCacheKey("AllConvenienceFees");
                MockCacheSetup(cacheKey, result.AsEnumerable());

                // Get the convenience fees
                var fees = repository.ConvenienceFees;
                Assert.IsNotNull(fees);

                // Verify that the data is now in the cache
                VerifyCache(cacheKey, result.AsEnumerable());
            }

            [TestMethod]
            public void ECommerceRepository_ConvenienceFees_VerifyCacheWithLock()
            {
                string cacheKey = repository.BuildFullCacheKey("AllConvenienceFees");
                object lockHandle = "Lock";
                MockCacheSetup(cacheKey, result.AsEnumerable(), false, lockHandle);

                // Get the convenience fees
                var fees = repository.ConvenienceFees;
                Assert.IsNotNull(fees);

                // Verify that the data is now in the cache
                VerifyCache(cacheKey, result.AsEnumerable(), false, lockHandle);
            }
        }

        [TestClass]
        public class ECommerceRepository_Distributions : ECommerceRepositoryTests
        {
            private Collection<Distributions> distributions;
            private List<Distribution> result;

            [TestInitialize]
            public void ECommerceRepository_Distributions_Initialize()
            {
                distributions = TestDistributionsRepository.Distributions;
                MockRecords("DISTRIBUTION", distributions);
                result = repository.Distributions.ToList();
            }

            [TestMethod]
            public void ECommerceRepository_Distributions_NotFound()
            {
                var distr = result.FirstOrDefault(x => x.Code == "XYZ");
                Assert.IsNull(distr);
            }

            [TestMethod]
            public void ECommerceRepository_Distributions_MatchingCount()
            {
                Assert.AreEqual(distributions.Count, result.Count());
            }

            [TestMethod]
            public void ECommerceRepository_Distributions_CorrectType()
            {
                CollectionAssert.AllItemsAreInstancesOfType(result, typeof(Distribution));
            }

            [TestMethod]
            public void ECommerceRepository_Distributions_MatchingKeys()
            {
                var sourceData = distributions.Select(cf => cf.Recordkey).ToList();
                var resultData = result.Select(cf => cf.Code).ToList();
                CollectionAssert.AreEqual(sourceData, resultData);
            }

            [TestMethod]
            public void ECommerceRepository_Distributions_MatchingDescriptions()
            {
                var sourceData = distributions.Select(cf => cf.DistrDescription).ToList();
                var resultData = result.Select(cf => cf.Description).ToList();
                CollectionAssert.AreEqual(sourceData, resultData);
            }

            [TestMethod]
            public void ECommerceRepository_Distributions_VerifyCacheNoLock()
            {
                string cacheKey = repository.BuildFullCacheKey("AllDistributions");
                MockCacheSetup(cacheKey, result.AsEnumerable());

                // Get the convenience fees
                var distrs = repository.Distributions;
                Assert.IsNotNull(distrs);

                // Verify that the data is now in the cache
                VerifyCache(cacheKey, result.AsEnumerable());
            }

            [TestMethod]
            public void ECommerceRepository_Distributions_VerifyCacheWithLock()
            {
                string cacheKey = repository.BuildFullCacheKey("AllDistributions");
                object lockHandle = "Lock";
                MockCacheSetup(cacheKey, result.AsEnumerable(), false, lockHandle);

                // Get the convenience fees
                var distrs = repository.Distributions;
                Assert.IsNotNull(distrs);

                // Verify that the data is now in the cache
                VerifyCache(cacheKey, result.AsEnumerable(), false, lockHandle);
            }
        }

        [TestClass]
        public class ECommerceRepository_PaymentMethods : ECommerceRepositoryTests
        {
            private Collection<PaymentMethods> convFees;
            private List<PaymentMethod> result;

            [TestInitialize]
            public void ECommerceRepository_PaymentMethods_Initialize()
            {
                convFees = TestPaymentMethodsRepository.PaymentMethods;
                MockRecords("PAYMENT.METHOD", convFees);
                result = repository.PaymentMethods.ToList();
            }

            [TestMethod]
            public void ECommerceRepository_PaymentMethods_NotFound()
            {
                var convFee = result.FirstOrDefault(x => x.Code == "XYZ");
                Assert.IsNull(convFee);
            }

            [TestMethod]
            public void ECommerceRepository_PaymentMethods_MatchingCount()
            {
                Assert.AreEqual(convFees.Count, result.Count());
            }

            [TestMethod]
            public void ECommerceRepository_PaymentMethods_CorrectType()
            {
                CollectionAssert.AllItemsAreInstancesOfType(result, typeof(PaymentMethod));
            }

            [TestMethod]
            public void ECommerceRepository_PaymentMethods_MatchingKeys()
            {
                var sourceData = convFees.Select(cf => cf.Recordkey).ToList();
                var resultData = result.Select(cf => cf.Code).ToList();
                CollectionAssert.AreEqual(sourceData, resultData);
            }

            [TestMethod]
            public void ECommerceRepository_PaymentMethods_MatchingDescriptions()
            {
                var sourceData = convFees.Select(cf => cf.PmthDescription).ToList();
                var resultData = result.Select(cf => cf.Description).ToList();
                CollectionAssert.AreEqual(sourceData, resultData);
            }

            [TestMethod]
            public void ECommerceRepository_PaymentMethods_VerifyCacheNoLock()
            {
                string cacheKey = repository.BuildFullCacheKey("AllPaymentMethods");
                MockCacheSetup(cacheKey, result.AsEnumerable());

                // Get the convenience fees
                var payMethods = repository.PaymentMethods;
                Assert.IsNotNull(payMethods);

                // Verify that the data is now in the cache
                VerifyCache(cacheKey, result.AsEnumerable());
            }

            [TestMethod]
            public void ECommerceRepository_PaymentMethods_VerifyCacheWithLock()
            {
                string cacheKey = repository.BuildFullCacheKey("AllPaymentMethods");
                object lockHandle = "Lock";
                MockCacheSetup(cacheKey, result.AsEnumerable(), false, lockHandle);

                // Get the convenience fees
                var payMethods = repository.PaymentMethods;
                Assert.IsNotNull(payMethods);

                // Verify that the data is now in the cache
                VerifyCache(cacheKey, result.AsEnumerable(), false, lockHandle);
            }
        }

        [TestClass]
        public class ECommerceRepository_ConvertCodeToPaymentMethodCategory : ECommerceRepositoryTests
        {
            [TestMethod]
            public void ECommerceRepository_ConvertCodeToPaymentMethodCategory_Cash()
            {
                var result = ECommerceRepository.ConvertCodeToPaymentMethodCategory("CA");
                Assert.AreEqual(PaymentMethodCategory.Cash, result);
            }

            [TestMethod]
            public void ECommerceRepository_ConvertCodeToPaymentMethodCategory_Check()
            {
                var result = ECommerceRepository.ConvertCodeToPaymentMethodCategory("CK");
                Assert.AreEqual(PaymentMethodCategory.Check, result);
            }

            [TestMethod]
            public void ECommerceRepository_ConvertCodeToPaymentMethodCategory_CreditCard()
            {
                var result = ECommerceRepository.ConvertCodeToPaymentMethodCategory("CC");
                Assert.AreEqual(PaymentMethodCategory.CreditCard, result);
            }

            [TestMethod]
            public void ECommerceRepository_ConvertCodeToPaymentMethodCategory_StocksAndSecurities()
            {
                var result = ECommerceRepository.ConvertCodeToPaymentMethodCategory("SS");
                Assert.AreEqual(PaymentMethodCategory.StocksAndSecurities, result);
            }

            [TestMethod]
            public void ECommerceRepository_ConvertCodeToPaymentMethodCategory_RealTangibleProperty()
            {
                var result = ECommerceRepository.ConvertCodeToPaymentMethodCategory("RP");
                Assert.AreEqual(PaymentMethodCategory.RealTangibleProperty, result);
            }

            [TestMethod]
            public void ECommerceRepository_ConvertCodeToPaymentMethodCategory_InsuranceAndRetirement()
            {
                var result = ECommerceRepository.ConvertCodeToPaymentMethodCategory("IR");
                Assert.AreEqual(PaymentMethodCategory.InsuranceAndRetirement, result);
            }

            [TestMethod]
            public void ECommerceRepository_ConvertCodeToPaymentMethodCategory_ContributedServices()
            {
                var result = ECommerceRepository.ConvertCodeToPaymentMethodCategory("CS");
                Assert.AreEqual(PaymentMethodCategory.ContributedServices, result);
            }

            [TestMethod]
            public void ECommerceRepository_ConvertCodeToPaymentMethodCategory_OtherInKind()
            {
                var result = ECommerceRepository.ConvertCodeToPaymentMethodCategory("OI");
                Assert.AreEqual(PaymentMethodCategory.OtherInKind, result);
            }

            [TestMethod]
            public void ECommerceRepository_ConvertCodeToPaymentMethodCategory_PayrollDeduction()
            {
                var result = ECommerceRepository.ConvertCodeToPaymentMethodCategory("PD");
                Assert.AreEqual(PaymentMethodCategory.PayrollDeduction, result);
            }

            [TestMethod]
            public void ECommerceRepository_ConvertCodeToPaymentMethodCategory_ElectronicFundsTransfer()
            {
                var result = ECommerceRepository.ConvertCodeToPaymentMethodCategory("EF");
                Assert.AreEqual(PaymentMethodCategory.ElectronicFundsTransfer, result);
            }

            [TestMethod]
            public void ECommerceRepository_ConvertCodeToPaymentMethodCategory_Other()
            {
                var result = ECommerceRepository.ConvertCodeToPaymentMethodCategory("XX");
                Assert.AreEqual(PaymentMethodCategory.Other, result);
            }

            [TestMethod]
            public void ECommerceRepository_ConvertCodeToPaymentMethodCategory_Null()
            {
                var result = ECommerceRepository.ConvertCodeToPaymentMethodCategory(null);
                Assert.AreEqual(PaymentMethodCategory.Other, result);
            }

            [TestMethod]
            public void ECommerceRepository_ConvertCodeToPaymentMethodCategory_Empty()
            {
                var result = ECommerceRepository.ConvertCodeToPaymentMethodCategory(string.Empty);
                Assert.AreEqual(PaymentMethodCategory.Other, result);
            }
        }
    }
}
