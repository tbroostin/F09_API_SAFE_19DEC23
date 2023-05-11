// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Utility;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PersonRestrictionRepository : BaseColleagueRepository, IPersonRestrictionRepository
    {
        public PersonRestrictionRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = 5;
        }

        /// <summary>
        /// Returns all restrictions for a specified person Id
        /// </summary>
        /// <param name="personId">Person Id for whom restrictions are requested</param>
        /// <returns>List of PersonRestrictions</returns>
        public async Task<IEnumerable<PersonRestriction>> GetAsync(string personId, bool useCache = true)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Must provide a person Id");
            }

            IEnumerable<PersonRestriction> restrictionsList = new List<PersonRestriction>();

            PersonSt personSt = null;
            try
            {
                if (useCache)
                {
                    personSt = await GetOrAddToCacheAsync("PersonSt" + personId,
                        async () =>
                        {
                            var personStData = await DataReader.ReadRecordAsync<PersonSt>(personId);
                            // If no record found, store an empty object so we don't keep trying to read it.
                            if (personStData == null)
                            {
                                personStData = new PersonSt();
                            }
                            return personStData;
                        });
                }
                else
                {
                    personSt = await DataReader.ReadRecordAsync<PersonSt>(personId);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error reading PERSON.ST record for person ID " + personId + " exception message: " + ex.Message);
            }

            // If there is no PERSON.ST record for this person in Colleague return no restrictions. 
            if (personSt != null && personSt.PstRestrictions != null && personSt.PstRestrictions.Count() > 0)
            {

                Collection<StudentRestrictions> studentRestrictions = null;
                if (useCache)
                {
                    studentRestrictions = await GetOrAddToCacheAsync("StudentRestrictionsPersonSt" + personId,
                        async () => { return await DataReader.BulkReadRecordAsync<StudentRestrictions>("STUDENT.RESTRICTIONS", personSt.PstRestrictions.ToArray()); }, 2);
                }
                else
                {
                    studentRestrictions = await DataReader.BulkReadRecordAsync<StudentRestrictions>("STUDENT.RESTRICTIONS", personSt.PstRestrictions.ToArray());
                }
                restrictionsList = BuildPersonRestrictions(studentRestrictions);
            }

            return restrictionsList;
        }

        /// <summary>
        /// Returns all restrictions for a specified list of Student Restrictions keys.
        /// </summary>
        /// <param name="ids">Keys to Student Restrictions to be returned</param>
        /// <returns>List of PersonRestrictions Objects</returns>
        public async Task<IEnumerable<PersonRestriction>> GetRestrictionsByIdsAsync(IEnumerable<string> ids)
        {
            if (ids.Count() <= 0)
            {
                throw new ArgumentNullException("ids", "Must provide at least one Student Restrictions Id");
            }

            Collection<StudentRestrictions> studentRestrictions = await DataReader.BulkReadRecordAsync<StudentRestrictions>("STUDENT.RESTRICTIONS", ids.ToArray());
            var restrictionsList = BuildPersonRestrictions(studentRestrictions);

            return restrictionsList;
        }

        /// <summary>
        /// Returns all restrictions for a specified list of Student keys.
        /// </summary>
        /// <param name="studentIds">Student Ids for whom restrictions are requested</param>
        /// <returns>List of PersonRestrictions Objects</returns>
        public async Task<IEnumerable<PersonRestriction>> GetRestrictionsByStudentIdsAsync(IEnumerable<string> studentIds)
        {
            if (studentIds.Count() <= 0)
            {
                throw new ArgumentNullException("studentIds", "Must provide at least one student Id");
            }

            List<PersonRestriction> personRestrictionsList = new List<PersonRestriction>();
            bool error = false;

            Collection<PersonSt> personStdata = await DataReader.BulkReadRecordAsync<PersonSt>(studentIds.ToArray());

            if (personStdata != null && personStdata.Count() > 0)
            {
                foreach (var personSt in personStdata)
                {
                    // If there is no PERSON.ST record for this person in Colleague return no restrictions.
                    if (personSt != null)
                    {
                        // todo: srm Move to bulk read outside of loop.
                        try
                        {
                            Collection<StudentRestrictions> studentRestrictions = await DataReader.BulkReadRecordAsync<StudentRestrictions>("STUDENT.RESTRICTIONS", personSt.PstRestrictions.ToArray());
                            var restrictionsList = BuildPersonRestrictions(studentRestrictions);
                            personRestrictionsList.AddRange(restrictionsList);
                        }
                        catch (Exception e)
                        {
                            logger.Error(string.Format("Could not build person restrictions for PERSON.ST : {0}", personSt.Recordkey));
                            logger.Error(e.GetBaseException().Message + e.GetBaseException().StackTrace);
                            error = true;
                        }
                    }
                }
            }
            else
            {
                logger.Error(string.Format("No PERSON.ST data returned for students : {0}", string.Join(",", studentIds)));
            }

            if (error && personRestrictionsList.Count() == 0)
                throw new ColleagueWebApiException("Errors prevented partial return of person restrictions batch.");

            return personRestrictionsList;
        }

        /// <summary>
        /// Build a set of PersonRestriction entities based on the collection of restriction data provided/
        /// </summary>
        /// <param name="restrictionsData">A collection of StudentRestrictions</param>
        /// <returns>A set of PersonRestriction entities</returns>
        private IEnumerable<PersonRestriction> BuildPersonRestrictions(Collection<StudentRestrictions> restrictionsData)
        {
            var personRestrictions = new List<PersonRestriction>();
            if (restrictionsData != null)
            {
                foreach (var restriction in restrictionsData)
                {
                    try
                    {
                        PersonRestriction stuRestriction = new PersonRestriction(restriction.Recordkey, restriction.StrStudent, restriction.StrRestriction, restriction.StrStartDate, restriction.StrEndDate, restriction.StrSeverity, restriction.StrPrtlDisplayFlag);
                        personRestrictions.Add(stuRestriction);
                    }
                    catch (Exception)
                    {
                        var inString = "Student Restriction Id: " + restriction.Recordkey + ", Student Id: " + restriction.StrStudent + ", Restriction Id: " + restriction.StrRestriction + " is not valid.";
                        logger.Error(inString);
                    }
                }
            }
            return personRestrictions;
        }
    }
}
