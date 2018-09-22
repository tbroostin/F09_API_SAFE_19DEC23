// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// This class implements the IPaymentTransactionsRepository interface.
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PaymentTransactionsRepository : BaseColleagueRepository, IPaymentTransactionsRepository
    {

        private Ellucian.Data.Colleague.DataContracts.IntlParams _internationalParameters;

        /// <summary>
        /// This constructor allows us to instantiate a PaymentTransactions repository object.
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object.</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object.</param>
        /// <param name="logger">Pass in an ILogger object.</param>
        public PaymentTransactionsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
        }

        /// <summary>
        /// Get PaymentTransactions Domain Entity
        /// </summary>
        /// <param name="offset">item number to start at</param>
        /// <param name="limit">number of items to return on page</param>
        /// <param name="documentId">Id for VOUCHER</param>
        /// <param name="documentTypeValue">invoice or refund</param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<PaymentTransaction>, int>> GetPaymentTransactionsAsync(
            int offset, int limit, string documentId, InvoiceOrRefund invoiceOrRefund)
        {
            var checkCriteria = "";
            var voucherCriteria = "WITH VOU.PMT.TXN.INTG.IDX NE ''";
            var checkIds = new List<string>();
            var voucherIds = new List<string>();

            if ((!string.IsNullOrEmpty(documentId)) && (invoiceOrRefund != InvoiceOrRefund.NotSet))
            {
                if (invoiceOrRefund == InvoiceOrRefund.Invoice)
                {
                    checkCriteria = string.Format("WITH CHK.VOUCHERS.IDS = '{0}'", documentId);
                    checkIds = (await DataReader.SelectAsync("CHECKS", checkCriteria)).ToList();
                }
                else if (invoiceOrRefund == InvoiceOrRefund.Refund)
                {
                   voucherCriteria = string.Format("{0} AND WITH VOUCHERS.ID = '{1}'", voucherCriteria, documentId);
                    voucherIds = (await DataReader.SelectAsync("VOUCHERS", voucherCriteria)).ToList();
                }
            }
            else
            {
                checkIds = (await DataReader.SelectAsync("CHECKS", checkCriteria)).ToList();
                voucherIds = (await DataReader.SelectAsync("VOUCHERS", voucherCriteria)).ToList();
            }
            var totalCount = voucherIds.Count() + checkIds.Count();

            var mergedChecksVouchers = checkIds.Union(voucherIds).ToArray();

            Array.Sort(mergedChecksVouchers);
            var subList = mergedChecksVouchers.Skip(offset).Take(limit).ToArray();

            var voucherData = await DataReader.BulkReadRecordAsync<DataContracts.Vouchers>("VOUCHERS", subList);
            var checkData = await DataReader.BulkReadRecordAsync<DataContracts.Checks>("CHECKS", subList);


            var voucherPersonSubList = voucherData.Where(v => !string.IsNullOrEmpty(v.VouVendor)).Select(v => v.VouVendor);
            var checkPersonSubList = checkData.Where(c => !string.IsNullOrEmpty(c.ChkVendor)).Select(c => c.ChkVendor);
            var personSubList = voucherPersonSubList.Union(checkPersonSubList).ToArray();
            var personsData = await DataReader.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", personSubList);

            var chkVouchersSubList = checkData.Where(cv => cv.ChkVouchersIds != null && cv.ChkVouchersIds.Any())
                .SelectMany(cv => cv.ChkVouchersIds).Distinct();
         
            var chkVouchers = (await DataReader.BulkReadRecordAsync<Vouchers>(chkVouchersSubList.ToArray())).ToList();

            var paymentTransactions = await BuildPaymentTransactions(checkData, voucherData, personsData, chkVouchers);

            return new Tuple<IEnumerable<PaymentTransaction>, int>(paymentTransactions, totalCount);
        }

        /// <summary>
        /// Get a single voucher using a GUID
        /// </summary>
        /// <param name="guid">The voucher guid</param>
        /// <returns>The PaymentTransactions domain entity</returns>
        public async Task<PaymentTransaction> GetPaymentTransactionsByGuidAsync(string guid)
        {
            try
            {
                Tuple<string, string> voucherID = await GetVoucherIdFromGuidAsync(guid);
                if (voucherID != null && string.IsNullOrEmpty(voucherID.Item1))
                {
                    throw new KeyNotFoundException(string.Concat("Id not found for voucher guid:", guid));
                }
                return await GetPaymentTransactionsAsync(voucherID.Item1, voucherID.Item2);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(e.Message);
            }
            catch (ApplicationException ae)
            {
                throw new ApplicationException(ae.Message);
            }
            catch (Exception e)
            {
                throw new KeyNotFoundException("Voucher GUID " + guid + " lookup failed.");
            }
        }

        /// <summary>
        /// Get PaymentTransactions id from Guid
        /// </summary>
        /// <param name="guid">guid</param>
        /// <returns>id</returns>
        public async Task<string> GetPaymentTransactionsIdFromGuidAsync(string guid)
        {
            return await GetRecordKeyFromGuidAsync(guid);
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        private async Task<Tuple<string,string>> GetVoucherIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("Vouchers GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("Vouchers GUID " + guid + " lookup failed.");
            }

            if ((foundEntry.Value.Entity != "CHECKS") &&
                (foundEntry.Value.Entity == "VOUCHERS") && (string.IsNullOrEmpty(foundEntry.Value.SecondaryKey)))
            {
                throw new RepositoryException("GUID " + guid + " has different entity, " + foundEntry.Value.Entity + ", than expected, VENDORS");
            }

            return new Tuple<string, string>(foundEntry.Value.PrimaryKey, foundEntry.Value.Entity);
        }

        /// <summary>
        /// Get a single PaymentTransaction using an ID
        /// </summary>
        /// <param name="id">The PaymentTransaction ID</param>
        /// <returns>The PaymentTransactions</returns>
        private async Task<PaymentTransaction> GetPaymentTransactionsAsync(string id, string entity)
        {
            PaymentTransaction paymentTransaction = null;
            Ellucian.Colleague.Data.Base.DataContracts.Person person = null;
            //System.Collections.ObjectModel.Collection<Vouchers> vouchers = null;
            List<Vouchers> vouchers = null;

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a voucher.");
            }

            if (entity == "VOUCHERS")
            {
                // Now we have an ID, so we can read the record
                var voucher = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vouchers>(id);
                if (voucher == null)
                {
                    throw new KeyNotFoundException(string.Concat("Record not found for voucher with ID ", id, "invalid."));
                }

                if (!string.IsNullOrEmpty(voucher.VouVendor))
                {
                    person = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", voucher.VouVendor);
                    if (person == null)
                    {
                        throw new ArgumentOutOfRangeException("Person Id " + voucher.VouVendor + " is not returning any data. Person may be corrupted.");
                    }
                }
                paymentTransaction = await BuildPaymentTransactionFromVoucher(voucher, person);
            }
            else if (entity == "CHECKS")
            {
                // Now we have an ID, so we can read the record
                var check = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Checks>(id);
                if (check == null)
                {
                    throw new KeyNotFoundException(string.Concat("Record not found for check with ID ", id, "invalid."));
                }

                if (!string.IsNullOrEmpty(check.ChkVendor))
                {
                    person = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", check.ChkVendor);
                    if (person == null)
                    {
                        throw new ArgumentOutOfRangeException("Person Id " + check.ChkVendor + " is not returning any data. Person may be corrupted.");
                    }
                }

                if (check.ChkVouchersIds != null && check.ChkVouchersIds.Any())
                {
                     vouchers = (await DataReader.BulkReadRecordAsync<Vouchers>(check.ChkVouchersIds.ToArray())).ToList();
                    if (vouchers == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Records not found for voucher with id(s) :",  string.Join(", ", check.ChkVouchersIds)));
                    }
                }

                paymentTransaction = await BuildPaymentTransactionFromCheck(check, person, vouchers);
            }

            return paymentTransaction;
        }

        /// <summary>
        ///  Build collection of PaymentTransactions domain entities from collections of associated 
        ///  vouchers and persons
        /// </summary>
        /// <param name="vouchers">Collection of Vouchers data contracts</param>
        /// <param name="persons">Collection of Persons data contracts</param>
        /// <returns>PaymentTransactions domain entity</returns>
        private async Task<IEnumerable<PaymentTransaction>> BuildPaymentTransactions(
            IEnumerable<Checks> checks,
            IEnumerable<Vouchers> vouchers,
            IEnumerable<Base.DataContracts.Person> persons, IEnumerable<Vouchers> checkVouchers
            )
        {
            var paymentTransactionsCollection = new List<PaymentTransaction>();

            foreach (var voucher in vouchers)
            {
                Base.DataContracts.Person person = null;
                if (!(string.IsNullOrWhiteSpace(voucher.VouVendor)))
                {
                    if (persons == null)
                    {
                        throw new ArgumentNullException("Expected person record for: " + voucher.Recordkey);
                    }
                    person = persons.FirstOrDefault(p => p.Recordkey == voucher.VouVendor);
                }

                paymentTransactionsCollection.Add(await BuildPaymentTransactionFromVoucher(voucher, person));
            }

            foreach (var check in checks)
            {
                Base.DataContracts.Person person = null;
                if (!(string.IsNullOrWhiteSpace(check.ChkVendor)))
                {
                    if (persons == null)
                    {
                        throw new ArgumentNullException("Expected person record for: " + check.Recordkey);
                    }
                    person = persons.FirstOrDefault(p => p.Recordkey == check.ChkVendor);
                }

                List<Vouchers> chkVouchers = null;
                if (check.ChkVouchersIds != null && check.ChkVouchersIds.Any())
                {
                    chkVouchers = checkVouchers.Where(r => check.ChkVouchersIds.Contains(r.Recordkey)).ToList();

                    if (chkVouchers == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Records not found for voucher with id(s) :", string.Join(", ", check.ChkVouchersIds)));
                    }
                }
                paymentTransactionsCollection.Add(await BuildPaymentTransactionFromCheck(check, person, chkVouchers));
            }
            return paymentTransactionsCollection.AsEnumerable();
        }

        /// <summary>
        /// Build PaymentTransactions domain entity from associated voucher and person data contracts
        /// </summary>
        /// <param name="voucher">Voucher data contract</param>
        /// <param name="person">Person data contract</param>
        /// <returns>PaymentTransactions domain entity</returns>
        private async Task<PaymentTransaction> BuildPaymentTransactionFromVoucher(Vouchers voucher,
            Ellucian.Colleague.Data.Base.DataContracts.Person person)
        {
            if (voucher == null)
            {
                throw new KeyNotFoundException(string.Format("Voucher record does not exist."));
            }

            var guid = await GetGuidFromIdAsync("VOUCHERS", voucher.Recordkey, "VOU.PMT.TXN.INTG.IDX", voucher.Recordkey);

            if (string.IsNullOrEmpty(guid))
            {
                throw new KeyNotFoundException(string.Concat(@"Guid not found. Entity: 'VOUCHERS', Secondary Field: 'VOU.PMT.TXN.INTG.IDX' Record ID: '", voucher.Recordkey, "'"));
            }

            // Translate the status code into a VoucherStatus enumeration value
            DateTime? voucherStatusDate = default(DateTime);
            DateTime? voidDate = default(DateTime);
            VoucherStatus? voucherStatus = null;
            if (voucher.VoucherStatusEntityAssociation != null && voucher.VoucherStatusEntityAssociation.Any())
            {
                var voucherStatusEntity =
                    voucher.VoucherStatusEntityAssociation.FirstOrDefault();
                // Get the first status in the list of voucher statuses, and check that it has a value
                if (voucherStatusEntity == null)
                {
                    throw new ApplicationException(string.Concat(@"Missing status,  Entity: 'VOUCHERS',  Record ID: '", voucher.Recordkey, "'"));
                }

                var voidPaymentTransaction = voucher.VoucherStatusEntityAssociation.FirstOrDefault(x => x.VouStatusAssocMember.ToUpper() == "V");
                var paidPaymentTransaction = voucher.VoucherStatusEntityAssociation.FirstOrDefault(x => x.VouStatusAssocMember.ToUpper() == "P");

                if(paidPaymentTransaction != null && paidPaymentTransaction.VouStatusDateAssocMember.HasValue)
                {
                    voucherStatusDate = paidPaymentTransaction.VouStatusDateAssocMember;
                }
                if (voidPaymentTransaction != null)
                {
                    voidDate = voidPaymentTransaction.VouStatusDateAssocMember;
                }

                var vouStatus = voucherStatusEntity.VouStatusAssocMember;
                if (vouStatus != null)
                {
                    voucherStatus = GetVoucherStatusVoucher(vouStatus, voucher.Recordkey);
                }
            }
            var paymentTransactionEntity = new PaymentTransaction(voucher.Recordkey, guid, voucherStatusDate.Value);
            paymentTransactionEntity.Check = false;
            paymentTransactionEntity.VoucherStatus = voucherStatus;
            paymentTransactionEntity.VoidDate = voidDate;
          
            var paymentMethod = PaymentMethod.NotSet;
            if ((string.IsNullOrEmpty(voucher.VouArPayment)) && (voucher.VouArDepositItems == null))
            {
                paymentMethod = PaymentMethod.Creditcard;
            }
            else if (!string.IsNullOrEmpty(voucher.VouArPayment))
            {
                var arPayments = await DataReader.ReadRecordAsync<ArPayments>(voucher.VouArPayment);
                if (!string.IsNullOrEmpty(arPayments.ArpOrigPayMethod))
                {
                    var paymentMethods = await DataReader.ReadRecordAsync<Base.DataContracts.PaymentMethods>(arPayments.ArpOrigPayMethod);
                    paymentMethod =
                        ((paymentMethods != null) && (paymentMethods.PmthCategory == "CC"))
                            ? PaymentMethod.Creditcard : PaymentMethod.Debitcard;
                }
            }
            else if ((voucher.VouArDepositItems != null) && (voucher.VouArDepositItems.Any()))
            {
                var depositItem = voucher.VouArDepositItems.Where(x => !string.IsNullOrEmpty(x)).FirstOrDefault();
                if (!string.IsNullOrEmpty(depositItem))
                {
                    var arDepositItem = await DataReader.ReadRecordAsync<ArDepositItems>(depositItem);
                    if (!string.IsNullOrEmpty(arDepositItem.ArdiOrigPayMethod))
                    {
                        var paymentMethods = await DataReader.ReadRecordAsync<Base.DataContracts.PaymentMethods>(arDepositItem.ArdiOrigPayMethod);
                        paymentMethod =
                            ((paymentMethods != null) && (paymentMethods.PmthCategory == "CC"))
                                ? PaymentMethod.Creditcard : PaymentMethod.Debitcard;
                    }
                }
            }
            paymentTransactionEntity.PaymentMethod = (paymentMethod == PaymentMethod.NotSet)
                ? PaymentMethod.Creditcard : paymentMethod;

            string[] eCommerceStrings = new string[] { voucher.VouEcommerceSession, voucher.VouEcommerceTransNo };
            paymentTransactionEntity.ReferenceNumber = string.Join("-", eCommerceStrings
                .Where(str => !string.IsNullOrEmpty(str)));

            paymentTransactionEntity.HostCountry = await GetHostCountryAsync();

            paymentTransactionEntity.Vendor = voucher.VouVendor;
            if (person != null)
            {
                paymentTransactionEntity.IsOrganization = (person.PersonCorpIndicator == "Y");
            }

            paymentTransactionEntity.CurrencyCode = voucher.VouCurrencyCode;
            paymentTransactionEntity.PaymentAmount = voucher.VouTotalAmt;

            var miscName = new List<string>();
            
            if ((voucher.VouAltFlag == "Y") || (string.IsNullOrEmpty(voucher.VouVendor)))
            {
                miscName.AddRange(voucher.VouMiscName);
                paymentTransactionEntity.Address = voucher.VouMiscAddress;
                paymentTransactionEntity.State = voucher.VouMiscState;
                paymentTransactionEntity.City = voucher.VouMiscCity;
                paymentTransactionEntity.Zip = voucher.VouMiscZip;
                paymentTransactionEntity.Country = voucher.VouMiscCountry;
            }
            else if (person != null)
            {
                if (person.PersonCorpIndicator == "Y")
                {
                    miscName.Add(person.PreferredName);
                }
                else
                    miscName.Add(string.Concat(person.FirstName, " ", person.LastName));

                if (!(string.IsNullOrEmpty(person.PreferredAddress)))
                {
                    var prefAddress = await DataReader.ReadRecordAsync<Base.DataContracts.Address>(person.PreferredAddress);
                    if (prefAddress != null)
                    {
                        paymentTransactionEntity.Address = prefAddress.AddressLines;
                        paymentTransactionEntity.State = prefAddress.State;
                        paymentTransactionEntity.City = prefAddress.City;
                        paymentTransactionEntity.Zip = prefAddress.Zip;

                    }
                }
            }
            paymentTransactionEntity.MiscName = miscName;
          
            return paymentTransactionEntity;
        }

        private VoucherStatus? GetVoucherStatusCheck(string vouStatus, string recordKey)
        {
            VoucherStatus? voucherStatus;
            switch (vouStatus.ToUpper())
            {
                case "U":
                    voucherStatus = VoucherStatus.InProgress;
                    break;
                case "N":
                    voucherStatus = VoucherStatus.NotApproved;
                    break;
                case "O":
                    voucherStatus = VoucherStatus.Outstanding;
                    break;
                case "P":
                    voucherStatus = VoucherStatus.Paid;
                    break;
                case "R":
                    voucherStatus = VoucherStatus.Reconciled;
                    break;
                case "V":
                    voucherStatus = VoucherStatus.Voided;
                    break;
                case "X":
                    voucherStatus = VoucherStatus.Cancelled;
                    break;
                default:
                    // if we get here, we have corrupt data.
                   throw new ApplicationException(string.Concat(@"Invalid status value 
                        , Status: '" , vouStatus, "' Record ID: '", recordKey, "'"));
            }
            return voucherStatus;
        }

        private VoucherStatus? GetVoucherStatusVoucher(string vouStatus, string recordKey)
        {
            VoucherStatus? voucherStatus;
            switch (vouStatus.ToUpper())
            {
        
                case "P":
                    voucherStatus = VoucherStatus.Paid;
                    break;
                case "R":
                    voucherStatus = VoucherStatus.Reconciled;
                    break;
                case "V":
                    voucherStatus = VoucherStatus.Voided;
                    break;
                default:
                    voucherStatus = VoucherStatus.Voided;
                    break;
            }
            return voucherStatus;
        }

        private async Task<PaymentTransaction> BuildPaymentTransactionFromCheck(Checks check,
            Ellucian.Colleague.Data.Base.DataContracts.Person person, List<Vouchers> vouchers)
        {
            if (check == null)
            {
                throw new KeyNotFoundException(string.Format("Check record does not exist."));
            }

            // Translate the status code into a VoucherStatus enumeration value
            VoucherStatus? voucherStatus = null;
            DateTime? voucherStatusDate = default(DateTime);
            DateTime? voidDate = default(DateTime);
            var voucherStatusEntity = check.ChkStatEntityAssociation.FirstOrDefault();

            // Get the first status in the list of voucher statuses, and check that it has a value
            if (voucherStatusEntity != null)
            {
                var vouStatus = voucherStatusEntity.ChkStatusAssocMember;
                if (vouStatus != null)
                {
                    voucherStatus = GetVoucherStatusCheck(vouStatus, check.Recordkey);
                }              
            }

            var voidPaymentTransaction = check.ChkStatEntityAssociation.FirstOrDefault(x => x.ChkStatusAssocMember.ToUpper() == "V");
            var paidPaymentTransaction = check.ChkStatEntityAssociation.FirstOrDefault(x => x.ChkStatusAssocMember.ToUpper() == "O");

            if(paidPaymentTransaction != null && paidPaymentTransaction.ChkStatusDateAssocMember.HasValue)
            {
                voucherStatusDate = paidPaymentTransaction.ChkStatusDateAssocMember;
            }

            if (voidPaymentTransaction != null)
            {
                voidDate = voidPaymentTransaction.ChkStatusDateAssocMember;
            }

            var paymentTransactionEntity = new PaymentTransaction(check.Recordkey, check.RecordGuid, 
                voucherStatusDate.HasValue ? Convert.ToDateTime(voucherStatusDate) : default(DateTime));

            paymentTransactionEntity.VoucherStatus = voucherStatus;
            paymentTransactionEntity.VoidDate = voidDate;
            paymentTransactionEntity.HostCountry = await GetHostCountryAsync();
            paymentTransactionEntity.Check = true;
            paymentTransactionEntity.PaymentMethod = check.ChkEcheckFlag == "Y" 
                ? PaymentMethod.Directdeposit : PaymentMethod.Check;  
            paymentTransactionEntity.PaymentAmount = check.ChkAmount;
            paymentTransactionEntity.Vendor = check.ChkVendor;
        
            if (check.ChkVouchersIds != null && check.ChkVouchersIds.Any())
            {
                if (vouchers == null)
                {
                    throw new ApplicationException(string.Concat(@"Unable to extract voucher records for ChkVouchersId, 
                        Entity: 'CHECKS', Record ID: '", check.Recordkey, "'"));
                }
                var voucherAmountAndCurrency = new Dictionary<string, AmountAndCurrency>();
                if (vouchers != null && vouchers.Any())
                {
                    foreach (var voucher in vouchers)
                    {
                        if (voucher.VouTotalAmt == 0)
                            continue;
                        CurrencyCodes currencyCode = CurrencyCodes.USD;
                        switch (voucher.VouCurrencyCode)
                        {
                            case "CAD":
                                currencyCode = CurrencyCodes.CAD; break;
                            case "EUR":
                                currencyCode = CurrencyCodes.EUR; break;
                           default:
                                currencyCode =  CurrencyCodes.USD; break;
                        }

                        var amountAndCurrency = new AmountAndCurrency(voucher.VouTotalAmt, currencyCode);

                        voucherAmountAndCurrency.Add(voucher.Recordkey, amountAndCurrency);
                    }
                }
                paymentTransactionEntity.VoucherAmountAndCurrency = voucherAmountAndCurrency;

            }

            if (person != null)
            {
                paymentTransactionEntity.IsOrganization = (person.PersonCorpIndicator == "Y");
            }

            paymentTransactionEntity.CurrencyCode = check.ChkCurrencyCode;

            paymentTransactionEntity.MiscName = check.ChkMiscName;
            paymentTransactionEntity.Address = check.ChkAddress;
            paymentTransactionEntity.City = check.ChkCity;
            paymentTransactionEntity.State = check.ChkState;
            paymentTransactionEntity.Zip = check.ChkZip;
            paymentTransactionEntity.Country = check.ChkCountry;
            
            return paymentTransactionEntity;
        }

        /// <summary>
        /// Get Host Country from international parameters
        /// </summary>
        /// <returns>HOST.COUNTRY</returns>
        private async Task<string> GetHostCountryAsync()
        {
            if (_internationalParameters == null)
                _internationalParameters = await GetInternationalParametersAsync();
            return _internationalParameters.HostCountry;
        }

        /// <summary>
        /// Return a GUID for an Entity and Record Key
        /// </summary>
        /// <param name="entity">Entity Name</param>
        /// <param name="id">Record Key</param>
        /// <returns>GUID associated to the entity and key</returns>
        public async Task<string> GetGuidFromIdAsync(string entity, string id, string secondaryField = "", string secondaryKey = "")
        {
            var criteria = string.Format("WITH LDM.GUID.ENTITY = '{0}' AND WITH LDM.GUID.PRIMARY.KEY = '{1}' AND WITH LDM.GUID.SECONDARY.FLD = '{2}' AND WITH LDM.GUID.SECONDARY.KEY = '{3}'", entity, id, secondaryField, secondaryKey);
            var ldmGuid = await DataReader.SelectAsync("LDM.GUID", criteria);
            if (ldmGuid != null && ldmGuid.Any())
            {
                return ldmGuid.ElementAt(0).ToString();
            }
            return string.Empty;
        }
    }
}