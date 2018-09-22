// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// Repository for accessing nonacademic attendance data
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class NonAcademicAttendanceRepository : BaseColleagueRepository, INonAcademicAttendanceRepository
    {
        /// <summary>
        /// Creates a new <see cref="INonAcademicAttendanceRepository"/> object
        /// </summary>
        /// <param name="cacheProvider">Interface to cache provider</param>
        /// <param name="transactionFactory">Interface to Colleague transaction factory</param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="apiSettings">Colleague Web API settings</param>
        public NonAcademicAttendanceRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
        }

        /// <summary>
        /// Retrieves all <see cref="NonAcademicAttendanceRequirement">nonacademic attendance requirements</see> for a person
        /// </summary>
        /// <param name="personId">Unique identifier for the person whose requirements are being retrieved</param>
        /// <returns>All <see cref="NonAcademicAttendanceRequirement">nonacademic attendance requirements</see> for a person</returns>
        public async Task<IEnumerable<NonAcademicAttendanceRequirement>> GetNonacademicAttendanceRequirementsAsync(string personId)
        {
            logger.Info("Entering NonAcademicAttendanceRepository.GetNonAcademicAttendanceRequirementsAsync...");
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "A unique identifier for a person must be specified in order to retrieve nonacademic attendance requirements.");
            }
            string criteriaString = "NAAPERRQ.PERSON EQ '" + personId + "'";
            logger.Debug("Reading PERSON.NAA.REQMTS records with " + criteriaString + "...");
            Collection<PersonNaaReqmts> personNonAcademicAttendanceRequirementData = await DataReader.BulkReadRecordAsync<PersonNaaReqmts>(criteriaString);
            var nonAcademicAttendanceRequirementsForPerson = BuildNonAcademicAttendanceRequirementsForPerson(personNonAcademicAttendanceRequirementData);
            logger.Info("Leaving NonAcademicAttendanceRepository.GetNonAcademicAttendanceRequirementsAsync...");
            return nonAcademicAttendanceRequirementsForPerson;
        }

        /// <summary>
        /// Retrieves all <see cref="NonAcademicAttendances">nonacademic events attended</see> for a person
        /// </summary>
        /// <param name="personId">Unique identifier for the person whose nonacademic attendances are being retrieved</param>
        /// <returns>All <see cref="NonAcademicAttendances">nonacademic events attended</see> for a person</returns>
        public async Task<IEnumerable<NonAcademicAttendance>> GetNonacademicAttendancesAsync(string personId)
        {
            logger.Info("Entering NonAcademicAttendanceRepository.GetNonAcademicAttendancesAsync...");
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "A unique identifier for a person must be specified in order to retrieve nonacademic attendances.");
            }
            string criteriaString = "PERNAA.PERSON EQ '" + personId + "'";
            logger.Debug("Reading PERSON.NAA.ATTEND records with " + criteriaString + "...");
            Collection<PersonNaaAttend> personNonAcademicAttendanceData = await DataReader.BulkReadRecordAsync<PersonNaaAttend>(criteriaString);
            var nonAcademicAttendancesForPerson = BuildNonAcademicAttendancesForPerson(personNonAcademicAttendanceData);
            logger.Info("Leaving NonAcademicAttendanceRepository.GetNonAcademicAttendancesAsync...");
            return nonAcademicAttendancesForPerson;
        }


        /// <summary>
        /// Builds a collection of <see cref="NonAcademicAttendanceRequirement">nonacademic attendance requirements</see> from a collection of <see cref="PersonNaaReqmts"/>PERSON.NAA.REQMTS records</see>
        /// </summary>
        /// <param name="personNonAcademicAttendanceRequirementData">A collection of <see cref="Books"/>PERSON.NAA.REQMTS records</see></param>
        /// <returns>A collection of <see cref="NonAcademicAttendanceRequirement">nonacademic attendance requirements</see></returns>
        private List<NonAcademicAttendanceRequirement> BuildNonAcademicAttendanceRequirementsForPerson(ICollection<PersonNaaReqmts> personNonAcademicAttendanceRequirementData)
        {
            logger.Debug("Entering NonAcademicAttendanceRepository.BuildNonAcademicAttendanceRequirementsForPerson...");
            List<NonAcademicAttendanceRequirement> nonAcademicAttendanceRequirementsForPerson = new List<NonAcademicAttendanceRequirement>();
            if (personNonAcademicAttendanceRequirementData != null)
            {
                logger.Debug(String.Format("Processing {0} PERSON.NAA.REQMTS records...", personNonAcademicAttendanceRequirementData.Count));
                foreach (var personNaaReqmts in personNonAcademicAttendanceRequirementData)
                {
                    if (personNaaReqmts != null)
                    {
                        try
                        {
                            nonAcademicAttendanceRequirementsForPerson.Add(new NonAcademicAttendanceRequirement(personNaaReqmts.Recordkey, personNaaReqmts.NaaperrqPerson,
                                personNaaReqmts.NaaperrqTerm, personNaaReqmts.NaaperrqAttendances, personNaaReqmts.NaaperrqRequiredUnits, personNaaReqmts.NaaperrqOverrideUnits));
                        }
                        catch (Exception ex)
                        {
                            LogDataError("PERSON.NAA.REQMTS", personNaaReqmts.Recordkey, personNaaReqmts, ex);
                        }
                    }
                }
                logger.Debug(String.Format("PERSON.NAA.REQMTS record processing complete. {0} records processed succesfully. {1} records had errors.", 
                    nonAcademicAttendanceRequirementsForPerson.Count,
                    personNonAcademicAttendanceRequirementData.Count - nonAcademicAttendanceRequirementsForPerson.Count));
            }
            else
            {
                logger.Debug("Database retrieval of PERSON.NAA.REQMTS records returned null.");
            }
            return nonAcademicAttendanceRequirementsForPerson;
        }

        /// <summary>
        /// Builds a collection of <see cref="NonAcademicAttendance">nonacademic events attended</see> from a collection of <see cref="PersonNaaAttend"/>PERSON.NAA.ATTEND records</see>
        /// </summary>
        /// <param name="personNonAcademicAttendanceData">A collection of <see cref="Books"/>PERSON.NAA.ATTEND records</see></param>
        /// <returns>A collection of <see cref="NonAcademicAttendance">nonacademic events attended</see></returns>
        private List<NonAcademicAttendance> BuildNonAcademicAttendancesForPerson(ICollection<PersonNaaAttend> personNonAcademicAttendanceData)
        {
            logger.Debug("Entering NonAcademicAttendanceRepository.BuildNonAcademicAttendancesForPerson...");
            List<NonAcademicAttendance> nonAcademicAttendancesForPerson = new List<NonAcademicAttendance>();
            if (personNonAcademicAttendanceData != null)
            {
                logger.Debug(String.Format("Processing {0} PERSON.NAA.ATTEND records...", personNonAcademicAttendanceData.Count));
                foreach (var personNaaAttend in personNonAcademicAttendanceData)
                {
                    if (personNaaAttend != null)
                    {
                        try
                        {
                            nonAcademicAttendancesForPerson.Add(new NonAcademicAttendance(personNaaAttend.Recordkey, personNaaAttend.PernaaPerson,
                                personNaaAttend.PernaaEvent, personNaaAttend.PernaaEarnedUnits));
                        }
                        catch (Exception ex)
                        {
                            LogDataError("PERSON.NAA.ATTEND", personNaaAttend.Recordkey, personNaaAttend, ex);
                        }
                    }
                }
                logger.Debug(String.Format("PERSON.NAA.ATTEND record processing complete. {0} records processed succesfully. {1} records had errors.",
                    nonAcademicAttendancesForPerson.Count,
                    personNonAcademicAttendanceData.Count - nonAcademicAttendancesForPerson.Count));
            }
            else
            {
                logger.Debug("Database retrieval of PERSON.NAA.ATTEND records returned null.");
            }
            return nonAcademicAttendancesForPerson;
        }
    }
}