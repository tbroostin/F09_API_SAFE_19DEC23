/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// BankRepository
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class BankRepository : BaseColleagueRepository, IBankRepository
    {

        // maximum number of records to query
        private readonly int readSize;

        //cache key for bank objects
        private const string AllBanksCacheKey = "AllBanksInformation";

        //web request helper encapsulates webrequest objects
        private readonly HttpWebRequestHelper webRequestHelper;

        /// <summary>
        /// Instatiate a new BankRoutingInformationRepository
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public BankRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings, HttpWebRequestHelper webRequestHelper)
            : base(cacheProvider, transactionFactory, logger)
        {
            this.readSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;

            this.webRequestHelper = webRequestHelper;

            CacheTimeout = Level1CacheTimeoutValue;
        }


        /// <summary>
        /// Get a bank's routing information from a US routing id or CA institution id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Bank> GetBankAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }

            // search the cache for a matching bank
            var bankDictionary = await GetBankDictionary();


            Bank bank;
            if (bankDictionary.TryGetValue(id, out bank))
            {
                return bank;
            }
            else
            {
                var message = string.Format("Bank id {0} not found", id);
                throw new KeyNotFoundException(message);
            }
        }

        public async Task<Dictionary<string, Bank>> GetAllBanksAsync()
        {
            return await GetBankDictionary();
        }


        /// <summary>
        /// Adds US routing information or CA institution information to the cache
        /// </summary>
        /// <returns></returns>    
        private async Task<Dictionary<string, Bank>> GetBankDictionary()
        {           
            return await BuildBankDictionary();
        }

        /// <summary>
        /// Builds a payroll and federal dictionary separately, then merges the federal dictionary into the payroll
        /// dictionary, preferring payroll objects first.
        /// </summary>
        /// <returns></returns>
        private async Task<Dictionary<string, Bank>> BuildBankDictionary()
        {
            var payrollBanks = new Dictionary<string, Bank>();
            var achBanks = new Dictionary<string, Bank>();

            try
            {
                payrollBanks = await BuildPayrollBankDictionary();
            }
            catch (Exception e)
            {
                logger.Error(e, "Error getting payrollBanks");
            }

            try
            {
                achBanks = await BuildFederalDirectoryBankDictionary();
            }
            catch (Exception e)
            {
                logger.Error(e, "Error getting AchBanks");
            }

            //merge achBanks into PayrollBanks

            achBanks.ToList().ForEach(b => AddDistinctToDictionary(payrollBanks, b.Key, b.Value));

            return payrollBanks;
        }

        /// <summary>
        /// Builds a dictionary of banks in the payroll system. If payroll is not licensed, 
        /// SelectAsync will return no record ids
        /// </summary>
        /// <returns></returns>
        private async Task<Dictionary<string, Bank>> BuildPayrollBankDictionary()
        {
            var bankDictionary = new Dictionary<string, Bank>();
            var prDepositCodes = await DataReader.SelectAsync("PR.DEPOSIT.CODES", "WITH DDC.IS.ARCHIVED NE 'Y'");
            // Search through the database in partitions
            for (int i = 0; i < prDepositCodes.Count(); i += readSize)
            {
                var subList = prDepositCodes.Skip(i).Take(readSize).ToArray();
                var bulkPrDepositCodes = await DataReader.BulkReadRecordAsync<PrDepositCodes>(subList);
                if (bulkPrDepositCodes != null)
                {
                    foreach (PrDepositCodes p in bulkPrDepositCodes)
                    {
                        try
                        {
                            if (!String.IsNullOrEmpty(p.DdcTransitNo))
                            {
                                AddDistinctToDictionary(bankDictionary, p.DdcTransitNo, new Bank(p.DdcTransitNo, p.DdcDescription, p.DdcTransitNo));
                            }
                            else if (!String.IsNullOrEmpty(p.DdcFinInstNumber))
                            {
                                var key = string.Format("{0}-{1}", p.DdcFinInstNumber, p.DdcBrTransitNumber);
                                AddDistinctToDictionary(bankDictionary, key, new Bank(key, p.DdcDescription, p.DdcFinInstNumber, p.DdcBrTransitNumber));
                            }
                        }
                        catch (Exception e)
                        {
                            LogDataError("PR.DEPOSIT.CODES", p.Recordkey, p, e, "Unable to build bank from PR.DEPOSIT.CODES");
                        }
                    }
                }
                else
                {
                    logger.Info("Null PR.DEPOSIT.CODES read from database");
                }
            }

            return bankDictionary;
        }

        /// <summary>
        /// Builds a dictionary of banks from the Federal Reserve database on the web. 
        /// It will only build the dictionary if the client agrees to on BIWP
        /// </summary>
        /// <returns></returns>
        private async Task<Dictionary<string, Bank>> BuildFederalDirectoryBankDictionary()
        {
            var bankDictionary = new Dictionary<string, Bank>();
            
            var parameters = await DataReader.ReadRecordAsync<BankInfoParms>("CORE.PARMS", "BANK.INFO.PARMS");
            if (parameters == null)
            {
                var message = "Unable to find BankInfoParams record to retrieve BipUseFedRoutingDir";
                logger.Info(message);
                parameters = new BankInfoParms();
            }
            var isAchDirectoryUseAccepted = parameters.BipUseFedRoutingDir != null ?
                parameters.BipUseFedRoutingDir.Equals("Y", StringComparison.CurrentCultureIgnoreCase) :
                false;

            if (isAchDirectoryUseAccepted)
            {
                var achDirectory = await getData();
                var lines = achDirectory.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string l in lines)
                {
                    try
                    {
                        var routingId = l.Substring(0, 9);
                        var bankName = l.Substring(35, 36).TrimEnd();
                        AddDistinctToDictionary(bankDictionary, routingId, new Bank(routingId, bankName, routingId));
                    }
                    catch (Exception ex)
                    {
                        var message = string.Format("Error reading ACH Directory line:\n{0}", l);
                        logger.Error(ex, message);
                    }
                }
            }
            

            return bankDictionary;
        }

        /// <summary>
        /// Helper method to add values to the given bank dictionary if the key doesn't already exist
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void AddDistinctToDictionary(Dictionary<string, Bank> dictionary, string key, Bank value)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
            }
        }

        /// <summary>
        /// Takes a web request and uses it to access ACH data
        /// </summary>
        /// <param name="webRequest"></param>
        /// <returns></returns>
        private async Task<string> getData()
        {
            try
            {
                // first get the sessionId cookies
                var sessionCookies = await getSessionCookies();

                // next post the acceptance agreement
                bool goAhead = await postAgreement(sessionCookies);

                // now make the get request for the data
                if (goAhead)
                {
                    var directoryURL = @"https://www.frbservices.org/EPaymentsDirectory/FedACHdir.txt?AgreementSessionObject=Agree";
                    webRequestHelper.SetHttpWebRequest(WebRequest.CreateHttp(directoryURL));
                    webRequestHelper.GetHttpWebRequest().CookieContainer = sessionCookies;
                    webRequestHelper.GetHttpWebRequest().Method = "GET";
                    webRequestHelper.GetHttpWebRequest().Proxy = null;
                    var response = await webRequestHelper.GetHttpWebRequest().GetResponseAsync();

                    var retryCount = 0;
                    while (!response.ResponseUri.AbsoluteUri.ToLower().Contains("fedachdir.txt") && retryCount < 2)
                    {
                        webRequestHelper.SetHttpWebRequest(WebRequest.CreateHttp(directoryURL));
                        webRequestHelper.GetHttpWebRequest().CookieContainer = sessionCookies;
                        webRequestHelper.GetHttpWebRequest().Method = "GET";
                        webRequestHelper.GetHttpWebRequest().Proxy = null;
                        response = await webRequestHelper.GetHttpWebRequest().GetResponseAsync();
                        retryCount++;
                    }

                    var responseStream = response.GetResponseStream();
                    using (var reader = new StreamReader(responseStream))
                    {
                        try
                        {
                            return await reader.ReadToEndAsync();
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, "Error reading FedACHdir response stream");
                        }
                    }
                }
                else
                {
                    logger.Error("Session Agreement was denied. Unable to retrieve requested ACH data");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Something went wrong getting the ACH data");
            }

            return string.Empty;
        }

        /// <summary>
        /// Creates and formats a valid web request; this is the tricky one
        /// </summary>
        /// <returns></returns>
        private async Task<bool> postAgreement(CookieContainer sessionCookies)
        {
            try
            {
                // this first part adds the 'Agree' to the request body
                var parameters = new StringBuilder();
                var body = new NameValueCollection();
                body.Add("agreementValue", "Agree");
                foreach (string key in body.Keys)
                {
                    parameters.AppendFormat("{0}={1}&",
                        HttpUtility.UrlEncode(key),
                        HttpUtility.UrlEncode(body[key]));
                }
                parameters.Length -= 1;

                webRequestHelper.SetHttpWebRequest(WebRequest.CreateHttp(@"https://www.frbservices.org/EPaymentsDirectory/submitAgreement"));

                webRequestHelper.GetHttpWebRequest().Credentials = CredentialCache.DefaultCredentials;
                webRequestHelper.GetHttpWebRequest().ContentType = "application/x-www-form-urlencoded";
                webRequestHelper.Host = "www.frbservices.org";
                webRequestHelper.GetHttpWebRequest().Method = "POST";
                webRequestHelper.GetHttpWebRequest().AllowAutoRedirect = true;
                webRequestHelper.GetHttpWebRequest().Proxy = null;
                // add the session cookie we retrieved
                webRequestHelper.GetHttpWebRequest().CookieContainer = sessionCookies;

                // write the body to the request strean
                try
                {
                    using (var writer = new StreamWriter(await webRequestHelper.GetHttpWebRequest().GetRequestStreamAsync()))
                    {
                        writer.Write(parameters.ToString());
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error creating web request for ACH data");
                }

                var response = await webRequestHelper.GetHttpWebRequest().GetResponseAsync();
                if (((HttpWebResponse)response).StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get response for ACH request");
                return false;
            }
        }

        /// <summary>
        /// Gets session cookie from the access page
        /// </summary>
        /// <returns></returns>
        private async Task<CookieContainer> getSessionCookies()
        {
            try
            {
                webRequestHelper.SetHttpWebRequest(WebRequest.CreateHttp(@"https://www.frbservices.org/EPaymentsDirectory/agreement.html"));
                webRequestHelper.GetHttpWebRequest().CookieContainer = new CookieContainer();
                webRequestHelper.GetHttpWebRequest().Proxy = null;
                await webRequestHelper.GetHttpWebRequest().GetResponseAsync();
                return webRequestHelper.GetHttpWebRequest().CookieContainer;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error getting session cookies for ACH data web request");
                throw;
            }
        }




    }
}
