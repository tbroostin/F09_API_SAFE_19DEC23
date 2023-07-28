// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Repository for agreements data, including agreements, agreement periods, and person agreements
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class AgreementsRepository : BaseColleagueRepository, IAgreementsRepository
    {
        private readonly string _colleagueTimeZone;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataRepository"/> class.
        /// </summary>
        /// <param name="cacheProvider">Interface to cache provider</param>
        /// <param name="transactionFactory">Interface to Colleague Transaction Factory</param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="apiSettings">API settings</param>
        public AgreementsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
            if (apiSettings != null)
            {
                _colleagueTimeZone = apiSettings.ColleagueTimeZone;
            }
        }

        /// <summary>
        /// Get all <see cref="AgreementPeriod"/>agreement periods</see>
        /// </summary>
        /// <param name="ignoreCache">Flag indicating whether or not to ignore cached data</param>
        /// <returns>All agreement periods</returns>
        public async Task<IEnumerable<Domain.Base.Entities.AgreementPeriod>> GetAgreementPeriodsAsync(bool ignoreCache = false)
        {
            var agreementPeriods = await GetOrAddToCacheAsync<List<Domain.Base.Entities.AgreementPeriod>>("AllAgreementPeriods",
                async () =>
                {
                    List<Domain.Base.Entities.AgreementPeriod> agreementPeriodList = new List<Domain.Base.Entities.AgreementPeriod>();
                    try
                    {
                        Collection<Data.Base.DataContracts.AgreementPeriod> apData = await this.DataReader.BulkReadRecordAsync<Data.Base.DataContracts.AgreementPeriod>("");
                        agreementPeriodList = BuildAgreementPeriods(apData);
                    }
                    catch (Exception)
                    {
                        throw new ApplicationException("Anonymous data reader request denied. Table is not public.");
                    }
                    return agreementPeriodList;
                }, Level1CacheTimeoutValue);

            return agreementPeriods;
        }

        /// <summary>
        /// Get <see cref="PersonAgreement">person agreements</see> by person ID
        /// </summary>
        /// <param name="id">Person identifier</param>
        /// <returns>Collection of person agreements for a given person</returns>
        public async Task<IEnumerable<PersonAgreement>> GetPersonAgreementsByPersonIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "A person ID is required to retrieve person agreements by person ID.");
            }
            try
            {
                List<PersonAgreements> personAgreementsData = new List<PersonAgreements>();
                List<PersonAgreement> personAgreements = new List<PersonAgreement>();
                string criteria = string.Format("WITH PAGR.PERSON.ID EQ '{0}'", id);
                string[] ids = await DataReader.SelectAsync("PERSON.AGREEMENTS", criteria);

                // Get PERSON.AGREEMENTS data for the person
                BulkReadOutput<PersonAgreements> bulkReadOutput = await DataReader.BulkReadRecordWithInvalidKeysAndRecordsAsync<PersonAgreements>(ids, false);
                if (bulkReadOutput.Equals(default(BulkReadOutput<PersonAgreements>)))
                {
                    return personAgreements;
                }
                if (bulkReadOutput.BulkRecordsRead == null || !bulkReadOutput.BulkRecordsRead.Any())
                {
                    logger.Error(string.Format("Bulk data retrieval for PERSON.AGREEMENTS file for records {0} did not return any valid records.", string.Join(",", ids)));
                }
                if (bulkReadOutput.InvalidRecords != null && bulkReadOutput.InvalidRecords.Any())
                {
                    foreach (var ir in bulkReadOutput.InvalidRecords)
                    {
                        logger.Error("Invalid PERSON.AGREEMENTS record found. PERSON.AGREEMENTS.ID = {0}: {1}", ir.Key, ir.Value);
                    }
                }
                if (bulkReadOutput.BulkRecordsRead != null && bulkReadOutput.BulkRecordsRead.Any())
                {
                    personAgreementsData.AddRange(bulkReadOutput.BulkRecordsRead);
                }

                personAgreements = BuildPersonAgreements(personAgreementsData);
                return personAgreements;
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Person agreements data for person {0} could not be retrieved.", id);
                logger.Error(ex, errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }

        /// <summary>
        /// Updates a <see cref="PersonAgreement">person agreement</see>
        /// </summary>
        /// <param name="agreement">The <see cref="PersonAgreement">person agreement</see> to update</param>
        /// <returns>An updated <see cref="PersonAgreement">person agreement</see></returns>
        public async Task<PersonAgreement> UpdatePersonAgreementAsync(PersonAgreement agreement)
        {
            if (agreement == null)
            {
                throw new ArgumentNullException("agreement", "A person agreement is required when updating a person agreement.");
            }
            // Convert the agreement date/time to local time, then divide into date and time parts expected by the CTX.
            DateTimeOffset? requestLocalDateTime = agreement.ActionTimestamp.ToLocalDateTime(_colleagueTimeZone);
            var text = agreement.Text != null ? agreement.Text.ToList() : new List<string>();

            UpdatePersonAgreementRequest request = new UpdatePersonAgreementRequest()
            {
                APersonAgreementsId = agreement.Id,
                APagrPersonId = agreement.PersonId,
                APagrAgreeStatus = ConvertPersonAgreementStatusToStatusString(agreement.Status),
                APagrActionDate = requestLocalDateTime.Value.Date,
                APagrActionTime = requestLocalDateTime.Value.DateTime,
                APagrTitle = agreement.Title,
                AlPagrText = text
            };

            UpdatePersonAgreementResponse response = new UpdatePersonAgreementResponse();

            try
            {
                response = await transactionInvoker.ExecuteAsync<UpdatePersonAgreementRequest, UpdatePersonAgreementResponse>(request);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "The Colleague transaction failed updating person agreement {0}.", agreement.Id);
                throw new ApplicationException(string.Format("Unable to update person agreement {0}.", agreement.Id));
            }
            if (!string.IsNullOrEmpty(response.AErrorMsg))
            {
                logger.Error(string.Format("The Colleague transaction returned the following error updating person agreement {0}: " + response.AErrorMsg, agreement.Id));
                throw new ApplicationException(string.Format("Unable to update person agreement {0}.", agreement.Id));
            }

            // Return the updated person agreement
            return await GetPersonAgreementAsync(agreement.Id);
        }

        /// <summary>
        /// Builds a list of <see cref="Domain.Base.Entities.AgreementPeriod">agreement periods</see> from <see cref="DataContracts.AgreementPeriod">AGREEMENT.PERIOD</see> data
        /// </summary>
        /// <param name="apData"><see cref="DataContracts.AgreementPeriod">AGREEMENT.PERIOD</see> data</param>
        /// <returns>List of <see cref="Domain.Base.Entities.AgreementPeriod">agreement periods</see></returns>
        private List<Domain.Base.Entities.AgreementPeriod> BuildAgreementPeriods(Collection<Data.Base.DataContracts.AgreementPeriod> apData)
        {
            var agreementPeriods = new List<Domain.Base.Entities.AgreementPeriod>();
            if (apData != null)
            {
                foreach (var ap in apData)
                {
                    if (ap != null)
                    {
                        try
                        {
                            Domain.Base.Entities.AgreementPeriod agreementPeriodEntity = new Domain.Base.Entities.AgreementPeriod(ap.Recordkey, ap.AgreementPeriodDescription);
                            agreementPeriods.Add(agreementPeriodEntity);
                        }
                        catch (Exception ex)
                        {
                            var apString = "Agreement Period Id: " + ap.Recordkey + ", Description: " + ap.AgreementPeriodDescription;
                            LogDataError("Agreement Period", ap.Recordkey, ap, ex, apString);
                        }
                    }
                }
            }
            return agreementPeriods;
        }

        /// <summary>
        /// Builds a list of <see cref="Domain.Base.Entities.PersonAgreement">person agreements</see> from <see cref="DataContracts.PersonAgreements">PERSON.AGREEMENTS</see> data
        /// </summary>
        /// <param name="apData"><see cref="DataContracts.PersonAgreements">PERSON.AGREEMENTS</see> data</param>
        /// <returns>List of <see cref="Domain.Base.Entities.PersonAgreement">person agreements</see></returns>

        private List<Domain.Base.Entities.PersonAgreement> BuildPersonAgreements(List<Data.Base.DataContracts.PersonAgreements> paData)
        {
            var personAgreements = new List<Domain.Base.Entities.PersonAgreement>();
            if (paData != null)
            {
                foreach (var pa in paData)
                {
                    if (pa != null)
                    {
                        try
                        {
                            var status = ConvertStatusStringToPersonAgreementStatus(pa.PagrAgreeStatus);
                            var actionTimestamp = pa.PagrActionTime.ToPointInTimeDateTimeOffset(pa.PagrActionDate, _colleagueTimeZone);
                            var personCanDeclineAgreement = pa.PagrAllowDecline.ToUpperInvariant() == "Y";
                            var text = pa.PagrText != null ? pa.PagrText.Split(DmiString._VM).ToList() : new List<string>();
                            Domain.Base.Entities.PersonAgreement personAgreementEntity = new Domain.Base.Entities.PersonAgreement(pa.Recordkey, pa.PagrPersonId, pa.PagrCode, pa.PagrPeriod, personCanDeclineAgreement, pa.PagrTitle, pa.PagrDueDate.Value, text, status, actionTimestamp);
                            personAgreements.Add(personAgreementEntity);
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Unable to retrieve person agreement information for agreement " + pa.Recordkey + ". " + ex.Message);
                        }
                    }
                }
            }
            return personAgreements;
        }

        /// <summary>
        /// Converts a given status string to a <see cref="PersonAgreementStatus"/>
        /// </summary>
        /// <param name="status">Status</param>
        /// <returns>A <see cref="PersonAgreementStatus"/></returns>
        private PersonAgreementStatus? ConvertStatusStringToPersonAgreementStatus(string status)
        {
            PersonAgreementStatus? convertedStatus = null;
            if (string.IsNullOrEmpty(status))
            {
                return convertedStatus;
            }
            var statusToUpper = status.ToUpperInvariant();
            switch (statusToUpper)
            {
                case "A":
                    convertedStatus = PersonAgreementStatus.Accepted;
                    break;
                case "D":
                    convertedStatus = PersonAgreementStatus.Declined;
                    break;
                default:
                    throw new ArgumentException("PAGR.AGREE.STATUS '{0}' is not a valid value. Valid values are blank, 'A', and 'D'");
            }
            return convertedStatus;
        }

        /// <summary>
        /// Converts a given status string to a <see cref="PersonAgreementStatus"/>
        /// </summary>
        /// <param name="status">Status</param>
        /// <returns>A <see cref="PersonAgreementStatus"/></returns>
        private string ConvertPersonAgreementStatusToStatusString(PersonAgreementStatus? status)
        {
            string convertedStatus = string.Empty;
            if (status.HasValue)
            {
                switch (status.Value)
                {
                    case PersonAgreementStatus.Accepted:
                        convertedStatus = "A";
                        break;
                    case PersonAgreementStatus.Declined:
                        convertedStatus = "D";
                        break;
                    default:
                        break;
                }
            }
            return convertedStatus;
        }

        /// <summary>
        /// Get <see cref="PersonAgreement"/>person agreements</see> by person ID
        /// </summary>
        /// <param name="id">Person identifier</param>
        /// <returns>Collection of person agreements for a given person</returns>
        private async Task<PersonAgreement> GetPersonAgreementAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "A person agreement ID is required to retrieve a person agreements.");
            }
            try
            {
                PersonAgreements personAgreementsData = await DataReader.ReadRecordAsync<PersonAgreements>("PERSON.AGREEMENTS", id, false);
                List<PersonAgreement> personAgreements = BuildPersonAgreements(new List<PersonAgreements>() { personAgreementsData });
                return personAgreements[0];
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Person agreements data for person {0} could not be retrieved.", id);
                logger.Error(ex, errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }
    }
}
