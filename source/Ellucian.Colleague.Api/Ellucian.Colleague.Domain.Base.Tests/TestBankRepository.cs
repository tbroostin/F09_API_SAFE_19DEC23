using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestBankRepository : IBankRepository
    {
        #region FIELDS & PROPERTIES & THE LIKE
        public int readSize { get; set; }
        #endregion

        #region TEST DATA
        public List<PrDepositCodes> DatabaseBanks = new List<PrDepositCodes>()
        {
                new PrDepositCodes
                {
                    Recordkey = "0001",
                    DdcTransitNo = "091000019",
                    DdcDescription = "CHASE BANK",
                    DdcIsArchived = ""
                },
                new PrDepositCodes
                {
                    Recordkey = "0003",
                    DdcTransitNo = "091000019",
                    DdcIsArchived = "Y",
                    DdcDescription = "FORMER CHASE BANK"
                },
                new PrDepositCodes
                {
                    Recordkey = "0004",
                    DdcIsArchived = "",
                    DdcTransitNo = "011000015",
                    DdcDescription = "Federal Reserve Bank Payroll Description"
                },
                new PrDepositCodes
                {
                    Recordkey = "0002",
                    DdcFinInstNumber = "123",
                    DdcBrTransitNumber = "12345",
                    DdcDescription = "CHASE BANK",
                    DdcIsArchived = ""
                },
        };

        public string AchBanks = "011000015O0110000150020802000000000FEDERAL RESERVE BANK                1000 PEACHTREE ST N.E.              ATLANTA             GA303094470866234568111     \r\n011000028O0110000151072811000000000STATE STREET BANK AND TRUST COMPANY JAB2NW                              N. QUINCY           MA021710000617664240011     \r\n011000138O0110000151101310000000000BANK OF AMERICA, N.A.               8001 VILLA PARK DRIVE               HENRICO             VA232280000800446013511     \r\n011000206O0110000151072505000000000BANK OF AMERICA N.A                 PO BOX 27025                        RICHMOND            VA232617025800446013511   ";


        #endregion

        public async Task<Bank> GetBankAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }


            // search the cache for a matching bank
            var bankDictionary = await BankTransferInformation();
            Bank bank;
            if (bankDictionary.TryGetValue(id, out bank))
            {
                return bank;
            }

            else
            {
                throw new KeyNotFoundException("id");
            }

        }

        public async Task<Dictionary<string, Bank>> GetAllBanksAsync()
        {
            return await BankTransferInformation();
        }

        public async Task<Dictionary<string, Bank>> BankTransferInformation()
        {
            var idsAndBankNames = new Dictionary<string, Bank>();

            // PR Deposit Codes Section
            // Select all PR Deposit Codes
            var dbBanks = DatabaseBanks.Where(db => db.DdcIsArchived != null && !db.DdcIsArchived.Equals("Y", StringComparison.InvariantCultureIgnoreCase));

            foreach (PrDepositCodes p in dbBanks)
            {
                if (!String.IsNullOrEmpty(p.DdcTransitNo))
                {
                    AddDistinctToDictionary(p.DdcTransitNo, new Bank(p.DdcTransitNo, p.DdcDescription, p.DdcTransitNo), idsAndBankNames);
                }
                else if (!String.IsNullOrEmpty(p.DdcFinInstNumber))
                {
                    var key = string.Format("{0}-{1}", p.DdcFinInstNumber, p.DdcBrTransitNumber);
                    AddDistinctToDictionary(p.DdcFinInstNumber, new Bank(key, p.DdcDescription, p.DdcFinInstNumber, p.DdcBrTransitNumber), idsAndBankNames);
                }
            }


            // ACH section
            var achDirectory = await Task.FromResult(this.AchBanks);
            var lines = achDirectory.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string l in lines)
            {
                try
                {
                    var routingId = l.Substring(0, 9);
                    var bankName = l.Substring(35, 36).TrimEnd();
                    AddDistinctToDictionary(routingId, new Bank(routingId, bankName, routingId), idsAndBankNames);
                }
                catch (Exception)
                {

                }
            }

            return idsAndBankNames;
        }

        #region HELPERS
        private void AddDistinctToDictionary(string key, Bank value, Dictionary<string, Bank> dictionary)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
            }
        }
        #endregion



    }
}
