/* Copyright 2017-2022 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PayScalesRepository : BaseColleagueRepository, IPayScalesRepository
    {
        private Ellucian.Data.Colleague.DataContracts.IntlParams _internationalParameters;

        public PayScalesRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Get PayScales objects for all PayScales bypassing cache and reading directly from the database.
        /// </summary>
        /// <param name="bypassCache">Bypass cache if set to true.</param>
        /// <returns>Tuple of PayScales Entity objects <see cref="PayScales"/> and a count for paging.</returns>
        public async Task<IEnumerable<PayScale>> GetPayScalesAsync(bool bypassCache = false)
        {
            if (bypassCache)
            {
                return await GetPayScalesNoCacheAsync();
            }

            return await GetOrAddToCacheAsync<IEnumerable<PayScale>>("AllPayScales",
               async () =>
               {
                   return await GetPayScalesNoCacheAsync();
               });
        }

        private async Task<IEnumerable<PayScale>> GetPayScalesNoCacheAsync()
        {
            try
            {
                string criteria = string.Empty;
                var payScalesKeys = await DataReader.SelectAsync("SWVER", criteria);
                var payScalesRecords = new List<PayScale>();

                if (payScalesKeys != null && payScalesKeys.Any())
                {
                    Array.Sort(payScalesKeys);

                    try
                    {
                        var payScaleRecords = await DataReader.BulkReadRecordAsync<DataContracts.Swver>(payScalesKeys);
                        var payTableIds = payScaleRecords.Select(ps => ps.SwvSwtablesId).ToArray();
                        var payTableRecords = await DataReader.BulkReadRecordAsync<DataContracts.Swtables>(payTableIds);
                        foreach (var scale in payScaleRecords)
                        {
                            var payTable = payTableRecords.Where(pt => pt.Recordkey == scale.SwvSwtablesId).FirstOrDefault();
                            payScalesRecords.Add(BuildPayScales(payTable, scale));
                        }
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
                return payScalesRecords;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get PayScales entity for a specific id
        /// </summary>   
        /// <param name="id">id of the PayScales record.</param>
        /// <returns>PayScales Entity <see cref="PayScales"./></returns>
        public async Task<Ellucian.Colleague.Domain.HumanResources.Entities.PayScale> GetPayScalesByIdAsync(string id)
        {
            var entity = await this.GetRecordInfoFromGuidAsync(id);
            if (entity == null || entity.Entity != "SWVER")
            {
                throw new KeyNotFoundException(string.Format("PayScales not found for id {0}", id));
            }
            var payScaleId = entity.PrimaryKey;
            if (string.IsNullOrWhiteSpace(payScaleId))
            {
                throw new KeyNotFoundException("PayScale id " + id + "does not exist");
            }
            var payScaleRecord = await DataReader.ReadRecordAsync<DataContracts.Swver>("SWVER", payScaleId);
            if (payScaleRecord == null)
            {
                throw new KeyNotFoundException("PayScale not found with id " + id);
            }
            var payTableRecord = await DataReader.ReadRecordAsync<DataContracts.Swtables>("SWTABLES", payScaleRecord.SwvSwtablesId);
            if (payTableRecord == null)
            {
                throw new KeyNotFoundException("PayTable not found with id " + payScaleRecord.SwvSwtablesId);
            }
            return BuildPayScales(payTableRecord, payScaleRecord);
        }

        /// <summary>
        /// Helper to build PersonPosition objects
        /// </summary>
        /// <param name="employRecord">the Perpos db record</param>
        /// <returns></returns>
        private Ellucian.Colleague.Domain.HumanResources.Entities.PayScale BuildPayScales(Swtables payTableRecord, Swver payScaleRecord)
        {
            PayScale payScaleEntity = new PayScale(payScaleRecord.RecordGuid, payScaleRecord.Recordkey, payScaleRecord.SwvDesc, payScaleRecord.SwvStartDate, payScaleRecord.SwvEndDate);
            payScaleEntity.WageTableId = payScaleRecord.SwvSwtablesId;
            payScaleEntity.WageTableGuid = payTableRecord.RecordGuid;

            var divisor = 100;
            if (payTableRecord.SwtHrlyOrSlry.ToUpper() == "H")
                divisor = 10000;

            var scales = new List<PayScalesScales>();
            foreach (var scale in payScaleRecord.SwvWageEntityAssociation)
            {
                string step = string.Empty;
                string grade = string.Empty;
                if (scale.SwvWageGradeStepAssocMember.Contains('*'))
                {
                    var stepGrade = scale.SwvWageGradeStepAssocMember.Split('*');
                    step = stepGrade[1];
                    grade = stepGrade[0];
                }

                var newScale = new PayScalesScales()
                {
                    Step = step,
                    Grade = grade,
                    Amount = !string.IsNullOrEmpty(scale.SwvWageAmountAssocMember) ? Convert.ToDecimal(scale.SwvWageAmountAssocMember) / divisor : 0
                };
                scales.Add(newScale);
            }
            if (scales != null && scales.Any())
            {
                payScaleEntity.Scales = scales;
            }

            return payScaleEntity;
        }

        /// <summary>
        /// Get Host Country from international parameters
        /// </summary>
        /// <returns>HOST.COUNTRY</returns>
        public async Task<string> GetHostCountryAsync()
        {
            if (_internationalParameters == null)
                _internationalParameters = await GetInternationalParametersAsync();
            return _internationalParameters.HostCountry;
        }
    }
}
