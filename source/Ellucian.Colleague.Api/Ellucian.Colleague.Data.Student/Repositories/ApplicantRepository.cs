// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Web.Http.Configuration;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// Applicant Repository
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class ApplicantRepository : PersonRepository, IApplicantRepository
    {
        public ApplicantRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger, apiSettings)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Get an Applicant by id Asynchronously
        /// </summary>
        /// <param name="applicantId">Applicant's Colleague PERSON id</param>
        /// <returns>An Applicant object</returns>
        /// <exception cref="ArgumentNullException">Thrown if the applicantId argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the applicantId is not a record key in the Applicants table</exception>
        public async Task<Applicant> GetApplicantAsync(string applicantId)
        {
            if (string.IsNullOrEmpty(applicantId))
            {
                throw new ArgumentNullException("applicantId");
            }

            var applicantRecord = await DataReader.ReadRecordAsync<Applicants>(applicantId);
            if (applicantRecord == null)
            {
                var message = string.Format("Record ID {0} does not exist in Applicants table", applicantId);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            var applicant = await GetAsync<Applicant>(applicantId,
                person =>
                {
                    var applicantEntity = new Applicant(person.Recordkey, person.LastName, person.PrivacyFlag);
                    return applicantEntity;
                });

            //Add the financial aid counselor id
            var financialAidStudentRecord = await DataReader.ReadRecordAsync<Data.Student.DataContracts.FinAid>(applicantId);
            if (financialAidStudentRecord != null &&
                financialAidStudentRecord.FaCounselorsEntityAssociation != null &&
                financialAidStudentRecord.FaCounselorsEntityAssociation.Count() > 0)
            {
                foreach (var faCounselorEntity in financialAidStudentRecord.FaCounselorsEntityAssociation)
                {
                    if (
                        (!faCounselorEntity.FaCounselorEndDateAssocMember.HasValue ||
                        DateTime.Today <= faCounselorEntity.FaCounselorEndDateAssocMember.Value) &&
                        (!faCounselorEntity.FaCounselorStartDateAssocMember.HasValue ||
                        DateTime.Today >= faCounselorEntity.FaCounselorStartDateAssocMember.Value)
                       )
                    {
                        applicant.FinancialAidCounselorId = faCounselorEntity.FaCounselorAssocMember;
                        break;
                    }
                }
            }

            return applicant;
        }

        /// <summary>
        /// Wrapper around async GetApplicant method.
        /// </summary>
        /// <param name="applicantId">Applicant's Colleague PERSON id</param>
        /// <returns>An Applicant object</returns>
        /// <exception cref="ArgumentNullException">Thrown if the applicantId argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the applicantId is not a record key in the Applicants table</exception>
        public Applicant GetApplicant(string applicantId)
        {
            var x = Task.Run(async () =>
            {
                return await GetApplicantAsync(applicantId);
            }).GetAwaiter().GetResult();
            return x;
        }

        public async Task<string> GetStwebDefaultsHierarchyAsync()
        {

            var result = await GetOrAddToCacheAsync<Data.Student.DataContracts.StwebDefaults>("StudentWebDefaults",
            async () =>
            {
                Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", true);
                if (stwebDefaults == null)
                {
                    if (logger.IsInfoEnabled)
                    {
                        var errorMessage = "Unable to access student web defaults from ST.PARMS. STWEB.DEFAULTS.";
                        logger.Info(errorMessage);
                    }
                    stwebDefaults = new StwebDefaults();
                }
                return stwebDefaults;
            }, Level1CacheTimeoutValue);

            return result.StwebDisplayNameHierarchy;

        }
    }
}
