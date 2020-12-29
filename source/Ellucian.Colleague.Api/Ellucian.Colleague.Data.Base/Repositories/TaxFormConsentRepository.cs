// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
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
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Repository for tax form consents.
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class TaxFormConsentRepository : BaseColleagueRepository, ITaxFormConsentRepository
    {
        private readonly string colleagueTimeZone;

        /// <summary>
        /// Tax form consent repository constructor.
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <param name="cacheProvider">Cache Provider</param>
        /// <param name="transactionFactory">Transaction factory</param>
        /// <param name="logger">Logger</param>
        public TaxFormConsentRepository(ApiSettings settings, ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            colleagueTimeZone = settings.ColleagueTimeZone;
        }

        /// <summary>
        /// Get a set of consent records.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="taxForm">The type of tax form: 1099-MISC, W-2, etc</param>
        /// <returns>Set of tax form consent records</returns>
        public async Task<IEnumerable<TaxFormConsent2>> Get2Async(string personId, string taxForm)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "Person ID must be specified.");

            if (string.IsNullOrWhiteSpace(taxForm))
                throw new ArgumentNullException("taxForm", "The tax form type must be specified.");

            // Based on the type of tax form, we will obtain the consents from different entities.
            var taxFormConsent2Entities = new List<TaxFormConsent2>();
            switch (taxForm)
            {
                case Domain.Base.TaxFormTypes.FormW2:
                    var formW2Consents = await DataReader.BulkReadRecordAsync<W2ConsentHistory>("W2CH.HRPER.ID EQ '" + personId + "'");
                    var genericW2Consents = formW2Consents.Select(x =>
                        new TemporaryConsent2()
                        {
                            Id = x.Recordkey,
                            Status = x.W2chNewStatus,
                            PersonId = personId,
                            TaxForm = Domain.Base.TaxFormTypes.FormW2,
                            AddDate = x.W2chStatusDate,
                            AddTime = x.W2chStatusTime
                        }).ToList();
                    taxFormConsent2Entities = BuildConsent2Entities(genericW2Consents);

                    break;

                case Domain.Base.TaxFormTypes.Form1095C:
                    var form1095Consents = await DataReader.BulkReadRecordAsync<DocConsentHistory>("DCHIST.PERSON.ID EQ '" + personId + "' AND WITH DCHIST.DOCUMENT EQ '1095C'");
                    var generic1095Consents = form1095Consents.Select(x =>
                        new TemporaryConsent2()
                        {
                            Id = x.Recordkey,
                            Status = x.DchistStatus,
                            PersonId = personId,
                            TaxForm = Domain.Base.TaxFormTypes.Form1095C,
                            AddDate = x.DchistStatusDate,
                            AddTime = x.DchistStatusTime
                        }).ToList();
                    taxFormConsent2Entities = BuildConsent2Entities(generic1095Consents);

                    break;
                case Domain.Base.TaxFormTypes.Form1098:
                    var form1098Consents = await DataReader.BulkReadRecordAsync<DocConsentHistory>("DCHIST.PERSON.ID EQ '" + personId + "' AND WITH DCHIST.DOCUMENT EQ '1098'");
                    var genericT098Consents = form1098Consents.Select(x =>
                        new TemporaryConsent2()
                        {
                            Id = x.Recordkey,
                            Status = x.DchistStatus,
                            PersonId = personId,
                            TaxForm = Domain.Base.TaxFormTypes.Form1098,
                            AddDate = x.DchistStatusDate,
                            AddTime = x.DchistStatusTime
                        }).ToList();
                    taxFormConsent2Entities = BuildConsent2Entities(genericT098Consents);

                    break;
                case Domain.Base.TaxFormTypes.FormT4:
                    var t4Consents = await DataReader.BulkReadRecordAsync<T4ConsentHistory>("T4CH.HRPER.ID EQ '" + personId + "'");
                    var genericT4Consents = t4Consents.Select(x =>
                        new TemporaryConsent2()
                        {
                            Id = x.Recordkey,
                            Status = x.T4chNewStatus,
                            PersonId = personId,
                            TaxForm = Domain.Base.TaxFormTypes.FormT4,
                            AddDate = x.T4chStatusDate,
                            AddTime = x.T4chStatusTime
                        }).ToList();
                    taxFormConsent2Entities = BuildConsent2Entities(genericT4Consents);

                    break;
                case Domain.Base.TaxFormTypes.FormT4A:
                    var t4aConsents = await DataReader.BulkReadRecordAsync<T4aConsentHistory>("T4ACH.HRPER.ID EQ '" + personId + "'");
                    var genericT4aConsents = t4aConsents.Select(x =>
                        new TemporaryConsent2()
                        {
                            Id = x.Recordkey,
                            Status = x.T4achNewStatus,
                            PersonId = personId,
                            TaxForm = Domain.Base.TaxFormTypes.FormT4A,
                            AddDate = x.T4achStatusDate,
                            AddTime = x.T4achStatusTime
                        }).ToList();
                    taxFormConsent2Entities = BuildConsent2Entities(genericT4aConsents);

                    break;
                case Domain.Base.TaxFormTypes.FormT2202A:
                    var t2202aConsents = await DataReader.BulkReadRecordAsync<DocConsentHistory>("DCHIST.PERSON.ID EQ '" + personId + "' AND WITH DCHIST.DOCUMENT EQ 'T2202A'");
                    var genericT2202aConsents = t2202aConsents.Select(x =>
                        new TemporaryConsent2()
                        {
                            Id = x.Recordkey,
                            Status = x.DchistStatus,
                            PersonId = personId,
                            TaxForm = Domain.Base.TaxFormTypes.FormT2202A,
                            AddDate = x.DchistStatusDate,
                            AddTime = x.DchistStatusTime
                        }).ToList();
                    taxFormConsent2Entities = BuildConsent2Entities(genericT2202aConsents);

                    break;
                case Domain.Base.TaxFormTypes.Form1099MI:
                    var form1099Miconsents = await DataReader.BulkReadRecordAsync<DocConsentHistory>("DCHIST.PERSON.ID EQ '" + personId + "' AND WITH DCHIST.DOCUMENT EQ '1099MI'");
                    var generic1099MiConsents = form1099Miconsents.Select(x =>
                        new TemporaryConsent2()
                        {
                            Id = x.Recordkey,
                            Status = x.DchistStatus,
                            PersonId = personId,
                            TaxForm = Domain.Base.TaxFormTypes.Form1099MI,
                            AddDate = x.DchistStatusDate,
                            AddTime = x.DchistStatusTime
                        }).ToList();
                    taxFormConsent2Entities = BuildConsent2Entities(generic1099MiConsents);

                    break;
                case Domain.Base.TaxFormTypes.Form1099NEC:
                    var form1099NECconsents = await DataReader.BulkReadRecordAsync<DocConsentHistory>("DCHIST.PERSON.ID EQ '" + personId + "' AND WITH DCHIST.DOCUMENT EQ '1099NEC'");
                    var generic1099NECConsents = form1099NECconsents.Select(x =>
                        new TemporaryConsent2()
                        {
                            Id = x.Recordkey,
                            Status = x.DchistStatus,
                            PersonId = personId,
                            TaxForm = Domain.Base.TaxFormTypes.Form1099NEC,
                            AddDate = x.DchistStatusDate,
                            AddTime = x.DchistStatusTime
                        }).ToList();
                    taxFormConsent2Entities = BuildConsent2Entities(generic1099NECConsents);

                    break;
            }

            return taxFormConsent2Entities;
        }

        /// <summary>
        /// Create a new tax form consent record.
        /// </summary>
        /// <param name="newTaxFormConsent2">TaxFormConsent2 object</param>
        /// <returns>New TaxFormConsent2 record.</returns>
        public async Task<TaxFormConsent2> Post2Async(TaxFormConsent2 newTaxFormConsent2)
        {
            // Converting form names to string equivalents used in Colleague.
            var document = string.Empty;
            switch (newTaxFormConsent2.TaxForm)
            {
                case Domain.Base.TaxFormTypes.FormW2:
                    document = "W2";
                    break;
                case Domain.Base.TaxFormTypes.Form1095C:
                    document = "1095C";
                    break;
                case Domain.Base.TaxFormTypes.Form1098:
                    document = "1098";
                    break;
                case Domain.Base.TaxFormTypes.FormT4:
                    document = "T4";
                    break;
                case Domain.Base.TaxFormTypes.FormT4A:
                    document = "T4A";
                    break;
                case Domain.Base.TaxFormTypes.FormT2202A:
                    document = "T2202A";
                    break;
                case Domain.Base.TaxFormTypes.Form1099MI:
                    document = "1099MI";
                    break;
                case Domain.Base.TaxFormTypes.Form1099NEC:
                    document = "1099NEC";
                    break;
            }

            // Boolean true corresponds to "C" for "Consent" while false corresponds to "W" for "Withheld Consent."
            var consent = newTaxFormConsent2.HasConsented ? "C" : "W";
            var timeStamp = ColleagueTimeZoneUtility.ToLocalDateTime(newTaxFormConsent2.TimeStamp, colleagueTimeZone);

            CreateDocConsentHistoryRequest request = new CreateDocConsentHistoryRequest()
            {
                APersonId = newTaxFormConsent2.PersonId,
                ADocument = document,
                AConsent = consent,
                ADate = timeStamp,
                ATime = timeStamp
            };
            CreateDocConsentHistoryResponse response = await transactionInvoker.ExecuteAsync<CreateDocConsentHistoryRequest, CreateDocConsentHistoryResponse>(request);

            return newTaxFormConsent2;
        }

        private class TemporaryConsent2
        {
            public string Id { get; set; }
            public string PersonId { get; set; }
            public string TaxForm { get; set; }
            public string Status { get; set; }
            public DateTime? AddDate { get; set; }
            public DateTime? AddTime { get; set; }
        }

        private List<TaxFormConsent2> BuildConsent2Entities(IEnumerable<TemporaryConsent2> temporaryConsents)
        {
            var consents = new List<TaxFormConsent2>();

            foreach (var record in temporaryConsents)
            {
                try
                {
                    // "C" indicates consented, "W" indicates withhold consent
                    var status = false;
                    if (record.Status.ToUpperInvariant() == "C")
                    {
                        status = true;
                    }

                    var timeStamp = record.AddTime.ToPointInTimeDateTimeOffset(record.AddDate, colleagueTimeZone);
                    if (timeStamp == null || !timeStamp.HasValue)
                    {
                        throw new ApplicationException("Invalid timestamp for " + record.TaxForm.ToString() + "consent record: " + record.Id);
                    }

                    consents.Add(new TaxFormConsent2(record.PersonId, record.TaxForm, status, timeStamp.Value));
                }
                catch (Exception e)
                {
                    LogDataError("TaxFormConsent2", record.PersonId, record, e, e.Message);
                }
            }

            return consents;
        }


        #region OBSOLETE METHODS

        /// <summary>
        /// Get a set of consent records.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="taxForm">Type of tax form</param>
        /// <returns>Set of tax form consent records</returns>
        [Obsolete("Obsolete as of API 1.29.1. Use Get2Async instead.")]
        public async Task<IEnumerable<TaxFormConsent>> GetAsync(string personId, TaxForms taxForm)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "Person ID must be specified.");

            // Based on the type of tax form, we will obtain the consents from different entities.
            var taxFormConsentEntities = new List<TaxFormConsent>();
            switch (taxForm)
            {
                case TaxForms.FormW2:
                    var formW2Consents = await DataReader.BulkReadRecordAsync<W2ConsentHistory>("W2CH.HRPER.ID EQ '" + personId + "'");
                    var genericW2Consents = formW2Consents.Select(x =>
                        new TemporaryConsent()
                        {
                            Id = x.Recordkey,
                            Status = x.W2chNewStatus,
                            PersonId = personId,
                            TaxForm = TaxForms.FormW2,
                            AddDate = x.W2chStatusDate,
                            AddTime = x.W2chStatusTime
                        }).ToList();
                    taxFormConsentEntities = BuildConsentEntities(genericW2Consents);

                    break;

                case TaxForms.Form1095C:
                    var form1095Consents = await DataReader.BulkReadRecordAsync<DocConsentHistory>("DCHIST.PERSON.ID EQ '" + personId + "' AND WITH DCHIST.DOCUMENT EQ '1095C'");
                    var generic1095Consents = form1095Consents.Select(x =>
                        new TemporaryConsent()
                        {
                            Id = x.Recordkey,
                            Status = x.DchistStatus,
                            PersonId = personId,
                            TaxForm = TaxForms.Form1095C,
                            AddDate = x.DchistStatusDate,
                            AddTime = x.DchistStatusTime
                        }).ToList();
                    taxFormConsentEntities = BuildConsentEntities(generic1095Consents);

                    break;
                case TaxForms.Form1098:
                    var form1098Consents = await DataReader.BulkReadRecordAsync<DocConsentHistory>("DCHIST.PERSON.ID EQ '" + personId + "' AND WITH DCHIST.DOCUMENT EQ '1098'");
                    var genericT098Consents = form1098Consents.Select(x =>
                        new TemporaryConsent()
                        {
                            Id = x.Recordkey,
                            Status = x.DchistStatus,
                            PersonId = personId,
                            TaxForm = TaxForms.Form1098,
                            AddDate = x.DchistStatusDate,
                            AddTime = x.DchistStatusTime
                        }).ToList();
                    taxFormConsentEntities = BuildConsentEntities(genericT098Consents);

                    break;
                case TaxForms.FormT4:
                    var t4Consents = await DataReader.BulkReadRecordAsync<T4ConsentHistory>("T4CH.HRPER.ID EQ '" + personId + "'");
                    var genericT4Consents = t4Consents.Select(x =>
                        new TemporaryConsent()
                        {
                            Id = x.Recordkey,
                            Status = x.T4chNewStatus,
                            PersonId = personId,
                            TaxForm = TaxForms.FormT4,
                            AddDate = x.T4chStatusDate,
                            AddTime = x.T4chStatusTime
                        }).ToList();
                    taxFormConsentEntities = BuildConsentEntities(genericT4Consents);

                    break;
                case TaxForms.FormT4A:
                    var t4aConsents = await DataReader.BulkReadRecordAsync<T4aConsentHistory>("T4ACH.HRPER.ID EQ '" + personId + "'");
                    var genericT4aConsents = t4aConsents.Select(x =>
                        new TemporaryConsent()
                        {
                            Id = x.Recordkey,
                            Status = x.T4achNewStatus,
                            PersonId = personId,
                            TaxForm = TaxForms.FormT4A,
                            AddDate = x.T4achStatusDate,
                            AddTime = x.T4achStatusTime
                        }).ToList();
                    taxFormConsentEntities = BuildConsentEntities(genericT4aConsents);

                    break;
                case TaxForms.FormT2202A:
                    var t2202aConsents = await DataReader.BulkReadRecordAsync<DocConsentHistory>("DCHIST.PERSON.ID EQ '" + personId + "' AND WITH DCHIST.DOCUMENT EQ 'T2202A'");
                    var genericT2202aConsents = t2202aConsents.Select(x =>
                        new TemporaryConsent()
                        {
                            Id = x.Recordkey,
                            Status = x.DchistStatus,
                            PersonId = personId,
                            TaxForm = TaxForms.FormT2202A,
                            AddDate = x.DchistStatusDate,
                            AddTime = x.DchistStatusTime
                        }).ToList();
                    taxFormConsentEntities = BuildConsentEntities(genericT2202aConsents);

                    break;
                case TaxForms.Form1099MI:
                    var form1099Miconsents = await DataReader.BulkReadRecordAsync<DocConsentHistory>("DCHIST.PERSON.ID EQ '" + personId + "' AND WITH DCHIST.DOCUMENT EQ '1099MI'");
                    var generic1099MiConsents = form1099Miconsents.Select(x =>
                        new TemporaryConsent()
                        {
                            Id = x.Recordkey,
                            Status = x.DchistStatus,
                            PersonId = personId,
                            TaxForm = TaxForms.Form1099MI,
                            AddDate = x.DchistStatusDate,
                            AddTime = x.DchistStatusTime
                        }).ToList();
                    taxFormConsentEntities = BuildConsentEntities(generic1099MiConsents);

                    break;
            }

            return taxFormConsentEntities;
        }

        /// <summary>
        /// Create a new tax form consent record.
        /// </summary>
        /// <param name="newTaxFormConsent">TaxFormConsent object</param>
        /// <returns>New TaxFormConsent record.</returns>
        [Obsolete("Obsolete as of API 1.29.1. Use Post2Async instead.")]
        public async Task<TaxFormConsent> PostAsync(TaxFormConsent newTaxFormConsent)
        {
            // Converting form names from enumerable to string equivalents used in Colleague.
            var document = string.Empty;
            switch (newTaxFormConsent.TaxForm)
            {
                case TaxForms.FormW2:
                    document = "W2";
                    break;
                case TaxForms.Form1095C:
                    document = "1095C";
                    break;
                case TaxForms.Form1098:
                    document = "1098";
                    break;
                case TaxForms.FormT4:
                    document = "T4";
                    break;
                case TaxForms.FormT4A:
                    document = "T4A";
                    break;
                case TaxForms.FormT2202A:
                    document = "T2202A";
                    break;
                case TaxForms.Form1099MI:
                    document = "1099MI";
                    break;
            }

            // Boolean true corresponds to "C" for "Consent" while false corresponds to "W" for "Withheld Consent."
            var consent = newTaxFormConsent.HasConsented ? "C" : "W";
            var timeStamp = ColleagueTimeZoneUtility.ToLocalDateTime(newTaxFormConsent.TimeStamp, colleagueTimeZone);

            CreateDocConsentHistoryRequest request = new CreateDocConsentHistoryRequest()
            {
                APersonId = newTaxFormConsent.PersonId,
                ADocument = document,
                AConsent = consent,
                ADate = timeStamp,
                ATime = timeStamp
            };
            CreateDocConsentHistoryResponse response = await transactionInvoker.ExecuteAsync<CreateDocConsentHistoryRequest, CreateDocConsentHistoryResponse>(request);

            return newTaxFormConsent;
        }

        [Obsolete("Obsolete as of API 1.29.1. Use TemporaryConsent2 instead.")]
        private class TemporaryConsent
        {
            public string Id { get; set; }
            public string PersonId { get; set; }
            public TaxForms TaxForm { get; set; }
            public string Status { get; set; }
            public DateTime? AddDate { get; set; }
            public DateTime? AddTime { get; set; }
        }

        [Obsolete("Obsolete as of API 1.29.1. Use BuildConsent2Entities instead.")]
        private List<TaxFormConsent> BuildConsentEntities(IEnumerable<TemporaryConsent> temporaryConsents)
        {
            var consents = new List<TaxFormConsent>();

            foreach (var record in temporaryConsents)
            {
                try
                {
                    // "C" indicates consented, "W" indicates withhold consent
                    var status = false;
                    if (record.Status.ToUpperInvariant() == "C")
                    {
                        status = true;
                    }

                    var timeStamp = record.AddTime.ToPointInTimeDateTimeOffset(record.AddDate, colleagueTimeZone);
                    if (timeStamp == null || !timeStamp.HasValue)
                    {
                        throw new ApplicationException("Invalid timestamp for " + record.TaxForm.ToString() + "consent record: " + record.Id);
                    }

                    consents.Add(new TaxFormConsent(record.PersonId, record.TaxForm, status, timeStamp.Value));
                }
                catch (Exception e)
                {
                    LogDataError("TaxFormConsent", record.PersonId, record, e, e.Message);
                }
            }

            return consents;
        }

        #endregion
    }
}
