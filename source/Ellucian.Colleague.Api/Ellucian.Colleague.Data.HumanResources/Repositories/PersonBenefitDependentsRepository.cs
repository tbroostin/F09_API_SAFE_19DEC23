//Copyright 2017-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using System.Linq;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Dmi.Runtime;
using Ellucian.Colleague.Data.HumanResources.Transactions;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PersonBenefitDependentsRepository : BaseColleagueRepository, IPersonBenefitDependentsRepository
    {
        private static char _VM = Convert.ToChar(DynamicArray.VM);

        /// <summary>
        /// ..ctor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public PersonBenefitDependentsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        #region GET Method
        /// <summary>
        ///  Get a collection of PersonBenefitDependent domain entity objects
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns>collection of PersonBenefitDependent domain entity objects</returns>
        public async Task<Tuple<IEnumerable<PersonBenefitDependent>, int>> GetPersonBenefitDependentsAsync(int offset, int limit, bool bypassCache = false)
        {
            var totalCount = 0;
            var personBenefitDependentEntities = new List<PersonBenefitDependent>();
            var criteria = "WITH PERBEN.DEPEND.ID NE '' BY.EXP PERBEN.DEPEND.ID";

            var perbenIds = await DataReader.SelectAsync("PERBEN", criteria);

            var perbenIds2 = new List<string>();
            foreach (var perbenId in perbenIds)
            {
                var perbenKey = perbenId.Split(_VM)[0];
                perbenIds2.Add(perbenKey);
            }

            totalCount = perbenIds2.Count();
            perbenIds2.Sort();

            var keysSubList = perbenIds2.Skip(offset).Take(limit).ToArray().Distinct();

            if (keysSubList.Any())
            {

                var perbensCollection = await DataReader.BulkReadRecordAsync<Perben>("PERBEN", perbenIds2.ToArray());

                foreach (var key in keysSubList)
                {

                    //var perposKey = key.Split('|');
                    //var perpos = jobappsCollection.FirstOrDefault(x => x.Recordkey == perposKey[0]);
                    var perbens = perbensCollection.FirstOrDefault(x => x.Recordkey == key);

                    try
                    {
                        int index = 0;
                        foreach (var perbensIndex in perbens.PerbenDependId)
                        {

                            var perbenGuidInfo = await GetGuidFromRecordInfoAsync("PERBEN", perbens.Recordkey, "PERBEN.DEPEND.ID", perbensIndex);

                            personBenefitDependentEntities.Add(new PersonBenefitDependent(perbenGuidInfo, perbens.Recordkey, perbensIndex)
                            {
                                ProviderName = (perbens.PerbenDepProviderName.Any() && perbens.PerbenDepProviderName.Count >= index + 1 && !string.IsNullOrEmpty(perbens.PerbenDepProviderName[index])) ? perbens.PerbenDepProviderName[index] : null,
                                ProviderIdentification = (perbens.PerbenDepProviderId.Any() && perbens.PerbenDepProviderId.Count >= index + 1 && !string.IsNullOrEmpty(perbens.PerbenDepProviderId[index])) ? perbens.PerbenDepProviderId[index] : null,
                                CoverageStartOn = (perbens.PerbenDepStartDate.Any() && perbens.PerbenDepStartDate.Count >= index + 1) ? perbens.PerbenDepStartDate[index] : null,
                                CoverageEndOn = (perbens.PerbenDepEndDate.Any() && perbens.PerbenDepEndDate.Count >= index + 1) ? perbens.PerbenDepEndDate[index] : null,
                                StudentStatus = (perbens.PerbenDepFullTimeStudent.Any() && perbens.PerbenDepFullTimeStudent.Count >= index + 1) ? perbens.PerbenDepFullTimeStudent[index] : null,
                            });

                            index++;
                        }
                    }
                    catch (Exception e)
                    {
                        throw new ColleagueWebApiException(e.Message);
                    }
                }
            }

            return new Tuple<IEnumerable<PersonBenefitDependent>, int>(personBenefitDependentEntities, totalCount);

        }

        /// <summary>
        /// Returns a review for a specified Person Benefit Dependent key.
        /// </summary>
        /// <param name="ids">Key to Person Benefit Dependent to be returned</param>
        /// <returns>PersonBenefitDependent Objects</returns>
        public async Task<PersonBenefitDependent> GetPersonBenefitDependentByIdAsync(string id)
        {
            var personBenefitDependentId = await GetRecordInfoFromGuidAsync(id);

            if (personBenefitDependentId == null)
                throw new KeyNotFoundException();
            if (personBenefitDependentId.Entity != "PERBEN")
                throw new KeyNotFoundException();

            var perbens = await DataReader.ReadRecordAsync<Perben>("PERBEN", personBenefitDependentId.PrimaryKey);

            int index = 0;
            foreach (var perbensIndex in perbens.PerbenDependId)
            {
                ////convert a datetime to a unidata internal value 
                //var offsetDate = DmiString.DateTimeToPickDate(effectiveDate);

                //if (offsetDate.ToString().Equals(perposKey[1]))
                //{
                var perbenGuidInfo = await GetGuidFromRecordInfoAsync("PERBEN", perbens.Recordkey, "PERBEN.DEPEND.ID", perbensIndex);

                if (perbenGuidInfo.Equals(id))
                {
                    return new PersonBenefitDependent(perbenGuidInfo, perbens.Recordkey, perbensIndex)
                    {
                        ProviderName = (perbens.PerbenDepProviderName.Count >= index + 1 && !string.IsNullOrEmpty(perbens.PerbenDepProviderName[index])) ? perbens.PerbenDepProviderName[index] : null,
                        ProviderIdentification = (perbens.PerbenDepProviderId.Count >= index + 1 && !string.IsNullOrEmpty(perbens.PerbenDepProviderId[index])) ? perbens.PerbenDepProviderId[index] : null,
                        CoverageStartOn = (perbens.PerbenDepStartDate.Count >= index + 1) ? perbens.PerbenDepStartDate[index] : null,
                        CoverageEndOn = (perbens.PerbenDepEndDate.Count >= index + 1) ? perbens.PerbenDepEndDate[index] : null,
                        StudentStatus = (perbens.PerbenDepFullTimeStudent.Count >= index + 1) ? perbens.PerbenDepFullTimeStudent[index] : null,
                    };
                }
                index++;
                //}
            }

            throw new KeyNotFoundException(String.Format("No person benefit dependent was found for guid '{0}'.", id));
        }

        #endregion

        /// <summary>
        /// Get the GUID for a entity using its ID
        /// </summary>
        /// <param name="id">entity ID</param>
        /// <param name="entity">entity</param>
        /// <returns>entity GUID</returns>
        public async Task<string> GetGuidFromIdAsync(string id, string entity)
        {
            try
            {
                return await GetGuidFromRecordInfoAsync(entity, id);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (RepositoryException ex)
            {
                ex.AddError(new RepositoryError("perpos.guid.NotFound", "GUID not found for employment performance review " + id));
                throw ex;
            }
        }

        /// <summary>
        /// Gets id from guid input
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<string> GetIdFromGuidAsync(string id)
        {
            try
            {
                return await GetRecordKeyFromGuidAsync(id);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (RepositoryException ex)
            {
                ex.AddError(new RepositoryError("review.guid.NotFound", "GUID not found for employment performance review " + id));
                throw ex;
            }
        }

        /// <summary>
        /// Gets id from guid input
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<GuidLookupResult> GetInfoFromGuidAsync(string id)
        {
            try
            {
                return await GetRecordInfoFromGuidAsync(id);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (RepositoryException ex)
            {
                ex.AddError(new RepositoryError("review.guid.NotFound", "GUID not found for employment performance review " + id));
                throw ex;
            }
        }

    }

}
