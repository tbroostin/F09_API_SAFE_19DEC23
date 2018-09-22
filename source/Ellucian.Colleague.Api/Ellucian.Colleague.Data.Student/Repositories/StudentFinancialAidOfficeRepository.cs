//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// OfficeRepository class builds Office objects from Colleague database records
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentFinancialAidOfficeRepository : BaseColleagueRepository, IStudentFinancialAidOfficeRepository
    {
        /// <summary>
        /// Constructor instantiates OfficeRepository object
        /// </summary>
        /// <param name="cacheProvider">CacheProvider object</param>
        /// <param name="transactionFactory">ColleagueTransactionFactory object</param>
        /// <param name="logger">Logger object</param>
        public StudentFinancialAidOfficeRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }        

        /// <summary>
        /// Get a collection of financial aid offices
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid offices</returns>
        public async Task<IEnumerable<FinancialAidOfficeItem>> GetFinancialAidOfficesAsync(bool ignoreCache = false)
        {
            var defaultSystemParameters = await GetSystemParametersAsync();

            return await GetGuidCodeItemAsync<FaOffices, FinancialAidOfficeItem>("AllFinancialAidOfficeItems", "FA.OFFICES",
            (fa, g) => new FinancialAidOfficeItem(g, fa.Recordkey, fa.Recordkey, fa.FaofcName)
            {
                AddressLines = BuildOfficeAddress(fa, defaultSystemParameters),
                City = !string.IsNullOrEmpty(fa.FaofcCity) ? fa.FaofcCity : null,
                State = !string.IsNullOrEmpty(fa.FaofcState) ? fa.FaofcState : null,
                PostalCode = !string.IsNullOrEmpty(fa.FaofcZip) ? fa.FaofcZip : null,
                AidAdministrator = !string.IsNullOrEmpty(fa.FaofcPellFaDirector) ? fa.FaofcPellFaDirector : defaultSystemParameters.FspFaDirectorName,
                PhoneNumber = !string.IsNullOrEmpty(fa.FaofcPellPhoneNumber) ? fa.FaofcPellPhoneNumber : defaultSystemParameters.FspPellPhoneNumber,
                FaxNumber = !string.IsNullOrEmpty(fa.FaofcPellFaxNumber) ? fa.FaofcPellFaxNumber : defaultSystemParameters.FspPellFaxNumber,
                EmailAddress = !string.IsNullOrEmpty(fa.FaofcPellInternetAddress) ? fa.FaofcPellInternetAddress : defaultSystemParameters.FspPellInternetAddress,
            }, bypassCache: ignoreCache);
        }

        private List<string> BuildOfficeAddress(FaOffices officeRecord, FaSysParams defaultSystemParameters)
        {
            var officeAddress = new List<string>();
            if (officeRecord.FaofcAddress != null &&
                officeRecord.FaofcAddress.Any(a => !string.IsNullOrEmpty(a)) &&
                !string.IsNullOrEmpty(officeRecord.FaofcCity) &&
                !string.IsNullOrEmpty(officeRecord.FaofcState) &&
                !string.IsNullOrEmpty(officeRecord.FaofcZip))
            {
                officeAddress.AddRange(officeRecord.FaofcAddress);

                var csz = string.Format("{0}, {1} {2}", officeRecord.FaofcCity, officeRecord.FaofcState, officeRecord.FaofcZip);
                officeAddress.Add(csz);
            }
            else if (officeRecord.FaofcAddress == null || officeRecord.FaofcAddress.Any(a => string.IsNullOrEmpty(a)) &&
                !string.IsNullOrEmpty(officeRecord.FaofcCity) &&
                !string.IsNullOrEmpty(officeRecord.FaofcState) &&
                !string.IsNullOrEmpty(officeRecord.FaofcZip))
            {
                var csz = string.Format("{0}, {1} {2}", officeRecord.FaofcCity, officeRecord.FaofcState, officeRecord.FaofcZip);
                officeAddress.Add(csz);
            }
            else if (officeRecord.FaofcAddress != null && officeRecord.FaofcAddress.Any() &&
                string.IsNullOrEmpty(officeRecord.FaofcCity) &&
                string.IsNullOrEmpty(officeRecord.FaofcState) &&
                string.IsNullOrEmpty(officeRecord.FaofcZip))
            {
                officeRecord.FaofcAddress.ForEach(a => { if (!string.IsNullOrEmpty(a)) { officeAddress.Add(a); } });
                //officeAddress.AddRange(officeRecord.FaofcAddress);
            }
            else
            {
                officeAddress.AddRange(defaultSystemParameters.FspInstitutionAddress);
                officeAddress.Add(defaultSystemParameters.FspInstitutionCsz);
            }
            return officeAddress;
        }

        /// <summary>
        /// Get and Cache the FaSysParams record asynchronously
        /// </summary>
        /// <returns>FaSysParams DataContract</returns>
        private async Task<FaSysParams> GetSystemParametersAsync()
        {
            try
            {
                return await GetOrAddToCacheAsync<FaSysParams>("FinancialAidSystemParameters", async () =>
                {
                    return await DataReader.ReadRecordAsync<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS");
                }, Level1CacheTimeoutValue);
            }
            catch (Exception)
            {
                var errorMessage = "Unable to access FA.SYS.PARAMS in ST.PARMS.";
                logger.Info(errorMessage);
                throw new Exception(errorMessage);
            }
        }
    }
}
