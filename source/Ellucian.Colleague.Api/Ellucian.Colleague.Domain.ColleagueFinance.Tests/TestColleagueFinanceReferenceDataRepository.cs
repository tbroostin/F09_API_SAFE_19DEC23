// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using System;
using System.Linq;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests
{
    public class TestColleagueFinanceReferenceDataRepository : IColleagueFinanceReferenceDataRepository
    {
        #region AP Taxes
        public Collection<ApTaxes> ApTaxesDataContracts = new Collection<ApTaxes>()
        {
            new ApTaxes() { Recordkey = "VA", ApTaxDesc = "Virginia Tax", ApTaxAppurEntryFlag = "Y", ApUseTaxFlag = "N", ApTaxCategory = "ABC"},
            new ApTaxes() { Recordkey = "GE", ApTaxDesc = "Goods and Services Exempt 70 %", ApTaxAppurEntryFlag = "Y", ApUseTaxFlag = "N", ApTaxCategory = "ABC" }
        };

        public async Task<IEnumerable<AccountsPayableTax>> GetAccountsPayableTaxCodesAsync()
        {
            var accountsPayableTaxes = new List<AccountsPayableTax>();
            foreach (var apTax in this.ApTaxesDataContracts)
            {
                accountsPayableTaxes.Add(new AccountsPayableTax(apTax.Recordkey, apTax.ApTaxDesc)
                {
                    AllowAccountsPayablePurchaseEntry = !string.IsNullOrEmpty(apTax.ApTaxAppurEntryFlag) ? (apTax.ApTaxAppurEntryFlag.ToLowerInvariant() == "y" ? true : false) : false,
                    IsUseTaxCategory = !string.IsNullOrEmpty(apTax.ApUseTaxFlag) ? (apTax.ApUseTaxFlag.ToLowerInvariant() == "y" ? true : false) : false,
                    TaxCategory = apTax.ApTaxCategory
                });
            }

            return await Task.Run(() => accountsPayableTaxes);
        }

        #endregion

        #region AP Types
        public Collection<ApTypes> ApTypesDataContracts = new Collection<ApTypes>()
        {
            new ApTypes() { Recordkey = "AP", ApTypesDesc = "Regular vendor payments" },
            new ApTypes() { Recordkey = "CAD1", ApTypesDesc = "Canadian Accounts Payable" },
        };

        public async Task<IEnumerable<AccountsPayableType>> GetAccountsPayableTypeCodesAsync()
        {
            var accountsPayableTypes = new List<AccountsPayableType>();
            foreach (var apType in this.ApTypesDataContracts)
            {
                accountsPayableTypes.Add(new AccountsPayableType(apType.Recordkey, apType.ApTypesDesc));
            }

            return await Task.Run(() => accountsPayableTypes);
        }
        #endregion

        public Task<IEnumerable<ColleagueFinance.Entities.CommodityCode>> GetCommodityCodesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<ColleagueFinance.Entities.CommodityCode>>(new List<ColleagueFinance.Entities.CommodityCode>()
                {
                    new Domain.ColleagueFinance.Entities.CommodityCode("884a59d1-20e5-43af-94e3-f1504230bbbc", "C1", "Desc1"),
                    new Domain.ColleagueFinance.Entities.CommodityCode("bb336acf-1926-4b12-8daf-d8720280498f", "C2", "Desc2"),
                    new Domain.ColleagueFinance.Entities.CommodityCode("d118f007-c914-465e-80dc-49d39209b24f", "C3", "Desc3")
                });
        }

        public Task<IEnumerable<ColleagueFinance.Entities.ProcurementCommodityCode>> GetCFCommodityCodesAsync()
        {
            return Task.FromResult<IEnumerable<ColleagueFinance.Entities.ProcurementCommodityCode>>(new List<ColleagueFinance.Entities.ProcurementCommodityCode>()
                {
                    new Domain.ColleagueFinance.Entities.ProcurementCommodityCode( "C1", "Desc1"),
                    new Domain.ColleagueFinance.Entities.ProcurementCommodityCode( "C2", "Desc2"),
                    new Domain.ColleagueFinance.Entities.ProcurementCommodityCode( "C3", "Desc3")
                });
        }

        public Task<CommodityCodes> GetCommodityCodesDataByCodeAsync(string code)
        {
            return Task.FromResult<CommodityCodes>(new CommodityCodes()
            {
                Recordkey = code,
                CmdtyDesc = "Desc1",
                CmdtyDefaultDescFlag = "Y",
                CmdtyFixedAssetsFlag ="S",
                CmdtyPrice = 1M,
                CmdtyTaxCodes = new List<string> { "T1", "T2"}
            });
        }



        public Task<string> GetCommodityCodeGuidAsync(string code)
        {
            var records = (new List<ColleagueFinance.Entities.CommodityCode>()
                {
                    new Domain.ColleagueFinance.Entities.CommodityCode("884a59d1-20e5-43af-94e3-f1504230bbbc", "C1", "Desc1"),
                    new Domain.ColleagueFinance.Entities.CommodityCode("bb336acf-1926-4b12-8daf-d8720280498f", "C2", "Desc2"),
                    new Domain.ColleagueFinance.Entities.CommodityCode("d118f007-c914-465e-80dc-49d39209b24f", "C3", "Desc3")
                });
            return Task.FromResult(records.FirstOrDefault(c => c.Code == code).Guid);
        }

        public Task<IEnumerable<VendorTerm>> GetVendorTermsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<ColleagueFinance.Entities.VendorTerm>>(new List<ColleagueFinance.Entities.VendorTerm>()
                {
                    new ColleagueFinance.Entities.VendorTerm("e338c649-db4b-4094-bb05-30ecd56ba82f", "02", "2-10-30"),
                    new ColleagueFinance.Entities.VendorTerm("d3a915c4-7914-4048-aa17-56d62911264a", "03", "3-10-30"),
                    new ColleagueFinance.Entities.VendorTerm("88393aeb-8239-4324-8203-707aa1181122", "30", "Net 30 days"),
                    new ColleagueFinance.Entities.VendorTerm("6c5dccc2-c56b-4481-9600-824522a8224b", "CA", "Cash Only")
                });
        }

        public Task<string> GetVendorTermGuidAsync(string code)
        {
            var records = (new List<ColleagueFinance.Entities.VendorTerm>()
                {
                    new ColleagueFinance.Entities.VendorTerm("e338c649-db4b-4094-bb05-30ecd56ba82f", "02", "2-10-30"),
                    new ColleagueFinance.Entities.VendorTerm("d3a915c4-7914-4048-aa17-56d62911264a", "03", "3-10-30"),
                    new ColleagueFinance.Entities.VendorTerm("88393aeb-8239-4324-8203-707aa1181122", "30", "Net 30 days"),
                    new ColleagueFinance.Entities.VendorTerm("6c5dccc2-c56b-4481-9600-824522a8224b", "CA", "Cash Only")
                });
            return Task.FromResult(records.FirstOrDefault(c => c.Code == code).Guid);
        }

        public Task<IEnumerable<CurrencyConversion>> GetCurrencyConversionAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ColleagueFinance.Entities.CommodityUnitType>> GetCommodityUnitTypesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<ColleagueFinance.Entities.CommodityUnitType>>(new List<ColleagueFinance.Entities.CommodityUnitType>()
                {
                    new Domain.ColleagueFinance.Entities.CommodityUnitType("884a59d1-20e5-43af-94e3-f1504230bbbc", "C1", "Desc1"),
                    new Domain.ColleagueFinance.Entities.CommodityUnitType("bb336acf-1926-4b12-8daf-d8720280498f", "C2", "Desc2"),
                    new Domain.ColleagueFinance.Entities.CommodityUnitType("d118f007-c914-465e-80dc-49d39209b24f", "C3", "Desc3")
                });
        }

        public Task<string> GetCommodityUnitTypeGuidAsync(string code)
        {
            var records = (new List<ColleagueFinance.Entities.CommodityUnitType>()
                {
                    new Domain.ColleagueFinance.Entities.CommodityUnitType("884a59d1-20e5-43af-94e3-f1504230bbbc", "C1", "Desc1"),
                    new Domain.ColleagueFinance.Entities.CommodityUnitType("bb336acf-1926-4b12-8daf-d8720280498f", "C2", "Desc2"),
                    new Domain.ColleagueFinance.Entities.CommodityUnitType("d118f007-c914-465e-80dc-49d39209b24f", "C3", "Desc3")
                });
            return Task.FromResult(records.FirstOrDefault(c => c.Code == code).Guid);
        }

        public Task<IEnumerable<ColleagueFinance.Entities.AccountingStringSubcomponentValues>> GetAccountingStringSubcomponentValuesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<ColleagueFinance.Entities.AccountingStringSubcomponentValues>>(new List<ColleagueFinance.Entities.AccountingStringSubcomponentValues>()
                {
                    new Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues("884a59d1-20e5-43af-94e3-f1504230bbbc", "C1", "Desc1", "Type1"),
                    new Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues("bb336acf-1926-4b12-8daf-d8720280498f", "C2", "Desc2", "Type1"),
                    new Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues("d118f007-c914-465e-80dc-49d39209b24f", "C3", "Desc3", "Type1")
                });
        }

        #region AccountsPayableSources

        public Task<IEnumerable<AccountComponents>> GetAccountComponentsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<ColleagueFinance.Entities.AccountComponents>>(new List<ColleagueFinance.Entities.AccountComponents>()
                {
                    new Domain.ColleagueFinance.Entities.AccountComponents("994a59d1-20e5-43af-94e3-f1504230bbbc", "1", "Desc1"),
                    new Domain.ColleagueFinance.Entities.AccountComponents("cc336acf-1926-4b12-8daf-d8720280498f", "2", "Desc2")
                });
        }

        public Task<Tuple<IEnumerable<AccountingStringComponentValues>, int>> GetAccountingStringComponentValuesAsync(int Offset, int Limit, string component, string transactionStatus, string typeAccount, string typeFund, bool bypassCache)
        {
            var list = new List<ColleagueFinance.Entities.AccountingStringComponentValues>()
                {
                    new Domain.ColleagueFinance.Entities.AccountingStringComponentValues() {
                     AccountDef="GL", AccountNumber="1110-11", Description="GL desc", Guid = "994a59d1-20e5-43af-94e3-f1504230bbbc", Status = "available", Type = "asset"},
                    new Domain.ColleagueFinance.Entities.AccountingStringComponentValues()
                    {
                        AccountDef="PROJECT", AccountNumber="REFnO", Description="GL desc", Guid = "cc336acf-1926-4b12-8daf-d8720280498f", Status = "available", Type = "expense"
                    }
                };
            var tuple = new Tuple<IEnumerable<AccountingStringComponentValues>, int>(list, 2);
            return Task.FromResult<Tuple<IEnumerable<AccountingStringComponentValues>, int>>(tuple);
        }

        public Task<AccountingStringComponentValues> GetAccountingStringComponentValueByGuid(string guid)
        {
            return Task.FromResult<AccountingStringComponentValues>(
                new Domain.ColleagueFinance.Entities.AccountingStringComponentValues()
                {
                    AccountDef = "GL",
                    AccountNumber = "1110-11",
                    Description = "GL desc",
                    Guid = "994a59d1-20e5-43af-94e3-f1504230bbbc",
                    Status = "available",
                    Type = "asset"
                });
        }


        public Task<IEnumerable<AccountingFormat>> GetAccountFormatsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<ColleagueFinance.Entities.AccountingFormat>>(new List<ColleagueFinance.Entities.AccountingFormat>()
                {
                    new Domain.ColleagueFinance.Entities.AccountingFormat("994a59d1-20e5-43af-94e3-f1504230bbbc", "1", "Desc1")
                });
        }

        public Task<IEnumerable<AccountsPayableSources>> GetAccountsPayableSourcesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<ColleagueFinance.Entities.AccountsPayableSources>>(new List<ColleagueFinance.Entities.AccountsPayableSources>()
                {
                    new ColleagueFinance.Entities.AccountsPayableSources("03ef76f3-61be-4990-8a99-9a80282fc420", "code1", "title1"),
                    new ColleagueFinance.Entities.AccountsPayableSources("d2f4f0af-6714-48c7-88d5-1c40cb407b6c", "code2", "title2"),
                    new ColleagueFinance.Entities.AccountsPayableSources("c517d7a5-f06a-42c8-85ab-b6320e1c0c2a", "code3", "title3"),
                    new ColleagueFinance.Entities.AccountsPayableSources("6c591aaa-5d33-4b19-b5e9-f6cf8956ef0a", "code4", "title4"),
                    new ColleagueFinance.Entities.AccountsPayableSources("6c591aaa-5d33-4b19-b5e9-f6cf8956ef0b", "AP", "AccountsPayable")
                });
        }

        public Task<string> GetAccountsPayableSourceGuidAsync(string code)
        {
            var records = (new List<ColleagueFinance.Entities.AccountsPayableSources>()
                {
                    new ColleagueFinance.Entities.AccountsPayableSources("03ef76f3-61be-4990-8a99-9a80282fc420", "code1", "title1"),
                    new ColleagueFinance.Entities.AccountsPayableSources("d2f4f0af-6714-48c7-88d5-1c40cb407b6c", "code2", "title2"),
                    new ColleagueFinance.Entities.AccountsPayableSources("c517d7a5-f06a-42c8-85ab-b6320e1c0c2a", "code3", "title3"),
                    new ColleagueFinance.Entities.AccountsPayableSources("6c591aaa-5d33-4b19-b5e9-f6cf8956ef0a", "code4", "title4")
                });
            return Task.FromResult(records.FirstOrDefault(c => c.Code == code).Guid);
        }

        #endregion

        #region FixedAssetDesignations
        public Task<IEnumerable<ColleagueFinance.Entities.FxaTransferFlags>> GetFxaTransferFlagsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<ColleagueFinance.Entities.FxaTransferFlags>>(new List<ColleagueFinance.Entities.FxaTransferFlags>()
                {
                    new Domain.ColleagueFinance.Entities.FxaTransferFlags("884a59d1-20e5-43af-94e3-f1504230bbbc", "ET", "other")
                });
        }

        public async Task<string> GetFxaTransferFlagGuidAsync(string code)
        {
            var records = new List<ColleagueFinance.Entities.FxaTransferFlags>()
                {
                    new ColleagueFinance.Entities.FxaTransferFlags("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "code1", "title1"),
                    new ColleagueFinance.Entities.FxaTransferFlags("bd54668d-50d9-416c-81e9-2318e88571a1", "code2", "title2"),
                    new ColleagueFinance.Entities.FxaTransferFlags("5eed2bea-8948-439b-b5c5-779d84724a38", "code3", "title3"),
                    new ColleagueFinance.Entities.FxaTransferFlags("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "code4", "title4")
                };
            return await Task.FromResult(records.FirstOrDefault(c => c.Code == code).Guid);

        }
        #endregion

        public Task<IEnumerable<ColleagueFinance.Entities.VendorType>> GetVendorTypesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<ColleagueFinance.Entities.VendorType>>(new List<ColleagueFinance.Entities.VendorType>()
                {
                    new ColleagueFinance.Entities.VendorType("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "code1", "title1"),
                    new ColleagueFinance.Entities.VendorType("bd54668d-50d9-416c-81e9-2318e88571a1", "code2", "title2"),
                    new ColleagueFinance.Entities.VendorType("5eed2bea-8948-439b-b5c5-779d84724a38", "code3", "title3"),
                    new ColleagueFinance.Entities.VendorType("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "code4", "title4")
                });

        }

        /// <summary>
        /// Get guid for VendorTypes code
        /// </summary>
        /// <param name="code">VendorTypes code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetVendorTypesGuidAsync(string code)
        {
            var records = new List<ColleagueFinance.Entities.VendorType>()
                {
                    new ColleagueFinance.Entities.VendorType("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "code1", "title1"),
                    new ColleagueFinance.Entities.VendorType("bd54668d-50d9-416c-81e9-2318e88571a1", "code2", "title2"),
                    new ColleagueFinance.Entities.VendorType("5eed2bea-8948-439b-b5c5-779d84724a38", "code3", "title3"),
                    new ColleagueFinance.Entities.VendorType("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "code4", "title4")
                };
            return await Task.FromResult(records.FirstOrDefault(c => c.Code == code).Guid);

        }


        public Task<IEnumerable<ColleagueFinance.Entities.VendorHoldReasons>> GetVendorHoldReasonsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<ColleagueFinance.Entities.VendorHoldReasons>>(new List<ColleagueFinance.Entities.VendorHoldReasons>()
                {
                    new ColleagueFinance.Entities.VendorHoldReasons("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "code1", "title1"),
                    new ColleagueFinance.Entities.VendorHoldReasons("bd54668d-50d9-416c-81e9-2318e88571a1", "code2", "title2"),
                    new ColleagueFinance.Entities.VendorHoldReasons("5eed2bea-8948-439b-b5c5-779d84724a38", "code3", "title3"),
                    new ColleagueFinance.Entities.VendorHoldReasons("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "code4", "title4")
                });

        }

        public async Task<string> GetVendorHoldReasonsGuidAsync(string code)
        {
           var records = new List<ColleagueFinance.Entities.VendorHoldReasons>()
                {
                    new ColleagueFinance.Entities.VendorHoldReasons("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "code1", "title1"),
                    new ColleagueFinance.Entities.VendorHoldReasons("bd54668d-50d9-416c-81e9-2318e88571a1", "code2", "title2"),
                    new ColleagueFinance.Entities.VendorHoldReasons("5eed2bea-8948-439b-b5c5-779d84724a38", "code3", "title3"),
                    new ColleagueFinance.Entities.VendorHoldReasons("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "code4", "title4")
                };
            return await Task.FromResult(records.FirstOrDefault(c => c.Code == code).Guid);

        }
        
        public Task<IEnumerable<GlSourceCodes>> GetGlSourceCodesValcodeAsync(bool ignoreCache)
        {
            throw new NotImplementedException();
        }


        public Task<IEnumerable<FreeOnBoardType>> GetFreeOnBoardTypesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<ColleagueFinance.Entities.FreeOnBoardType>>(new List<ColleagueFinance.Entities.FreeOnBoardType>()
                {
                    new ColleagueFinance.Entities.FreeOnBoardType("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "code1", "title1"),
                    new ColleagueFinance.Entities.FreeOnBoardType("bd54668d-50d9-416c-81e9-2318e88571a1", "code2", "title2"),
                    new ColleagueFinance.Entities.FreeOnBoardType("5eed2bea-8948-439b-b5c5-779d84724a38", "code3", "title3"),
                    new ColleagueFinance.Entities.FreeOnBoardType("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "code4", "title4")
                });
        }

        public Task<string> GetFreeOnBoardTypeGuidAsync(string code)
        {
            var records = new List<ColleagueFinance.Entities.FreeOnBoardType>()
                {
                    new ColleagueFinance.Entities.FreeOnBoardType("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "code1", "title1"),
                    new ColleagueFinance.Entities.FreeOnBoardType("bd54668d-50d9-416c-81e9-2318e88571a1", "code2", "title2"),
                    new ColleagueFinance.Entities.FreeOnBoardType("5eed2bea-8948-439b-b5c5-779d84724a38", "code3", "title3"),
                    new ColleagueFinance.Entities.FreeOnBoardType("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "code4", "title4")
                };
            return Task.FromResult(records.FirstOrDefault(c => c.Code == code).Guid);
        }


        public Task<IEnumerable<ShipToDestination>> GetShipToDestinationsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<ColleagueFinance.Entities.ShipToDestination>>(new List<ColleagueFinance.Entities.ShipToDestination>()
                {
                    new ColleagueFinance.Entities.ShipToDestination("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "code1", "title1"),
                    new ColleagueFinance.Entities.ShipToDestination("bd54668d-50d9-416c-81e9-2318e88571a1", "code2", "title2"),
                    new ColleagueFinance.Entities.ShipToDestination("5eed2bea-8948-439b-b5c5-779d84724a38", "code3", "title3"),
                    new ColleagueFinance.Entities.ShipToDestination("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "code4", "title4")
                });
        }

        public Task<string> GetShipToDestinationGuidAsync(string code)
        {
            var records = (new List<ColleagueFinance.Entities.ShipToDestination>()
                {
                    new ColleagueFinance.Entities.ShipToDestination("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "code1", "title1"),
                    new ColleagueFinance.Entities.ShipToDestination("bd54668d-50d9-416c-81e9-2318e88571a1", "code2", "title2"),
                    new ColleagueFinance.Entities.ShipToDestination("5eed2bea-8948-439b-b5c5-779d84724a38", "code3", "title3"),
                    new ColleagueFinance.Entities.ShipToDestination("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "code4", "title4")
                });
            return Task.FromResult(records.FirstOrDefault(c => c.Code == code).Guid);
        }

        public Task<IEnumerable<ShippingMethod>> GetShippingMethodsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<ColleagueFinance.Entities.ShippingMethod>>(new List<ColleagueFinance.Entities.ShippingMethod>()
                {
                    new ColleagueFinance.Entities.ShippingMethod("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "code1", "title1"),
                    new ColleagueFinance.Entities.ShippingMethod("bd54668d-50d9-416c-81e9-2318e88571a1", "code2", "title2"),
                    new ColleagueFinance.Entities.ShippingMethod("5eed2bea-8948-439b-b5c5-779d84724a38", "code3", "title3"),
                    new ColleagueFinance.Entities.ShippingMethod("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "code4", "title4")
                });
        }

        public Task<string> GetShippingMethodGuidAsync(string code)
        {
            var records = (new List<ColleagueFinance.Entities.ShippingMethod>()
                {
                    new ColleagueFinance.Entities.ShippingMethod("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "code1", "title1"),
                    new ColleagueFinance.Entities.ShippingMethod("bd54668d-50d9-416c-81e9-2318e88571a1", "code2", "title2"),
                    new ColleagueFinance.Entities.ShippingMethod("5eed2bea-8948-439b-b5c5-779d84724a38", "code3", "title3"),
                    new ColleagueFinance.Entities.ShippingMethod("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "code4", "title4")
                });
            return Task.FromResult(records.FirstOrDefault(c => c.Code == code).Guid);
        }

        public Task<IEnumerable<ColleagueFinance.Entities.FiscalPeriodsIntg>> GetFiscalPeriodsIntgAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<ColleagueFinance.Entities.FiscalPeriodsIntg>>(new List<ColleagueFinance.Entities.FiscalPeriodsIntg>()
            {
                    new Domain.ColleagueFinance.Entities.FiscalPeriodsIntg("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "2015")
                    { Title = "Fiscal Year: 2015", Status = "O" },
                    new Domain.ColleagueFinance.Entities.FiscalPeriodsIntg("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "2016")
                    { Title = "Fiscal Year: 2016", Status = "O"},
                    new Domain.ColleagueFinance.Entities.FiscalPeriodsIntg("d2253ac7-9931-4560-b42f-1fccd43c952e", "2017")
                    { Title = "Fiscal Year: 2017", Status = "O" }
            });
        }

        public Task<IEnumerable<FiscalYear>> GetFiscalYearsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<ColleagueFinance.Entities.FiscalYear>>(new List<ColleagueFinance.Entities.FiscalYear>()
            {
                    new Domain.ColleagueFinance.Entities.FiscalYear("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "2015")
                    { Title = "Fiscal Year: 2015", Status = "O", InstitutionName = "Ellucian University",
                    FiscalStartMonth = 7 },
                    new Domain.ColleagueFinance.Entities.FiscalYear("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "2016")
                    { Title = "Fiscal Year: 2016", Status = "O", InstitutionName = "Ellucian University",
                    FiscalStartMonth = 7},
                    new Domain.ColleagueFinance.Entities.FiscalYear("d2253ac7-9931-4560-b42f-1fccd43c952e", "2017")
                    { Title = "Fiscal Year: 2017", Status = "O", InstitutionName = "Ellucian University",
                    FiscalStartMonth = 7}
            });
        }

        public Task<Tuple<IEnumerable<AccountingStringComponentValues>, int>> GetAccountingStringComponentValues2Async(int Offset, int Limit, string component, string transactionStatus, string typeAccount)
        {
            throw new NotImplementedException();
        }

        public Task<AccountingStringComponentValues> GetAccountingStringComponentValue2ByGuid(string Guid)
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<string, string>> GetGuidsForPooleeGLAcctsInFiscalYearsAsync(IEnumerable<string> glAccts)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ColleagueFinance.Entities.AssetCategories>> GetAssetCategoriesAsync(bool bypassCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ColleagueFinance.Entities.AssetTypes>> GetAssetTypesAsync(bool bypassCache)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetCorpNameAsync()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetAccountingStringComponentValuesGuidFromIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ColleagueFinance.Entities.AcctStructureIntg>> GetAcctStructureIntgAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<ColleagueFinance.Entities.AcctStructureIntg>>(new List<ColleagueFinance.Entities.AcctStructureIntg>()
            {
                    new Domain.ColleagueFinance.Entities.AcctStructureIntg("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "FUND", "Fund")
                    {Type = "FD",ParentSubComponent = "GL.CLASS" },
                    new Domain.ColleagueFinance.Entities.AcctStructureIntg("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "GL.CLASS", "GL Class")
                    {Type = "FC" },
                    new Domain.ColleagueFinance.Entities.AcctStructureIntg("d2253ac7-9931-4560-b42f-1fccd43c952e", "PROGRAM", "Program")
                    {Type = "OB" }
            });
        }

        public Task<Tuple<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>, int>> GetAccountingStringSubcomponentValuesAsync(int offset, int limit, bool bypassCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>> GetAllAccountingStringSubcomponentValuesAsync(bool bypassCache)
        {
            throw new NotImplementedException();
        }

        public  Task<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>> GetFdDescs(bool bypassCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>> GetUnDescs(bool bypassCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>> GetObDescs(bool bypassCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>> GetFcDescs(bool bypassCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>> GetSoDescs(bool bypassCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>> GetLoDescs(bool bypassCache)
        {
            throw new NotImplementedException();
        }

        public Task<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues> GetAccountingStringSubcomponentValuesByGuidAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public Task<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues> GetAccountingStringSubcomponentValuesByEntityKey(string entity, string key)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetGuidFromEntityInfoAsync(string entity, string primaryKey, string secondaryField = "", string secondaryKey = "")
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<AccountingStringComponentValues>, int>> GetAccountingStringComponentValues3Async(int Offset, int Limit, string component, string typeAccount, string status, IEnumerable<string> grants, DateTime? effectiveOn)
        {
            throw new NotImplementedException();
        }

        public Task<AccountingStringComponentValues> GetAccountingStringComponentValue3ByGuid(string Guid)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ShipToCode>> GetShipToCodesAsync()
        {
            return Task.FromResult<IEnumerable<ColleagueFinance.Entities.ShipToCode>>(new List<ColleagueFinance.Entities.ShipToCode>()
                {
                    new ColleagueFinance.Entities.ShipToCode("CD","Datatel - Central Dist. Office"),
                    new ColleagueFinance.Entities.ShipToCode("DT","Datatel - Downtown"),
                    new ColleagueFinance.Entities.ShipToCode("EC","Datatel - Extension Center"),
                    new ColleagueFinance.Entities.ShipToCode("MC","Datatel - Main Campus")
                });

        }

        public Task<IEnumerable<ColleagueFinance.Entities.ProcurementCommodityCode>> GetAllCommodityCodesAsync()
        {
            return Task.FromResult<IEnumerable<ColleagueFinance.Entities.ProcurementCommodityCode>>(new List<ColleagueFinance.Entities.ProcurementCommodityCode>()
                {
                    new ColleagueFinance.Entities.ProcurementCommodityCode("C1", "Desc1"),
                    new ColleagueFinance.Entities.ProcurementCommodityCode("C2", "Desc2")
                    });    
        }

        Task<IEnumerable<CommodityUnitType>> IColleagueFinanceReferenceDataRepository.GetAllCommodityUnitTypesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<FixedAssetsFlag>> GetFixedAssetTransferFlagsAsync()
        {
            return Task.FromResult<IEnumerable<ColleagueFinance.Entities.FixedAssetsFlag>>(new List<ColleagueFinance.Entities.FixedAssetsFlag>()
                {
                    new ColleagueFinance.Entities.FixedAssetsFlag("S", "Single"),
                    new ColleagueFinance.Entities.FixedAssetsFlag("M", "Multi-Valued")
                    });
        }

        public Task<ProcurementCommodityCode> GetCommodityCodeByCodeAsync(string recordKey)
        {
            var procurementCommodityCode = new ColleagueFinance.Entities.ProcurementCommodityCode("10900", "Robotics Kit");
            procurementCommodityCode.DefaultDescFlag = true;
            procurementCommodityCode.FixedAssetsFlag = "S";
            procurementCommodityCode.Price = 10;
            procurementCommodityCode.TaxCodes = new List<string>() { "taxCode1" };
            return Task.FromResult(procurementCommodityCode);
        }

        public Task<IEnumerable<TaxForm>> GetTaxFormsAsync()
        {
            return Task.FromResult<IEnumerable<ColleagueFinance.Entities.TaxForm>>(new List<ColleagueFinance.Entities.TaxForm>()
                {
                    new ColleagueFinance.Entities.TaxForm("1098T", "1098-T Taxform"),
                    new ColleagueFinance.Entities.TaxForm("1098E", "1098-E Taxform"),
                    new ColleagueFinance.Entities.TaxForm("T4", "T4 Taxform")
                    });
        }

        public Task<IEnumerable<ShipViaCode>> GetShipViaCodesAsync()
        {
            return Task.FromResult<IEnumerable<ColleagueFinance.Entities.ShipViaCode>>(new List<ColleagueFinance.Entities.ShipViaCode>()
                {
                    new ColleagueFinance.Entities.ShipViaCode("CD","Datatel - Central Dist. Office"),
                    new ColleagueFinance.Entities.ShipViaCode("DT","Datatel - Downtown"),
                    new ColleagueFinance.Entities.ShipViaCode("EC","Datatel - Extension Center"),
                    new ColleagueFinance.Entities.ShipViaCode("MC","Datatel - Main Campus")
                });
        }

        public Task<IEnumerable<IntgVendorAddressUsages>> GetIntgVendorAddressUsagesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<ColleagueFinance.Entities.IntgVendorAddressUsages>>(new List<ColleagueFinance.Entities.IntgVendorAddressUsages>()
                {
                    new ColleagueFinance.Entities.IntgVendorAddressUsages("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AU","Address Usage 1")
                });
        }

        public async Task<string> GetIntgVendorAddressUsagesGuidAsync(string code)
        {
            var records = new List<ColleagueFinance.Entities.IntgVendorAddressUsages>()
                {
                    new ColleagueFinance.Entities.IntgVendorAddressUsages("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AU","Address Usage 1")
                };
            return await Task.FromResult(records.FirstOrDefault(c => c.Code == code).Guid);
            
        }

        public Task<string> GetAssetCategoriesGuidAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetAssetTypesGuidAsync(string code)
        {
            throw new NotImplementedException();
        }
    }
}