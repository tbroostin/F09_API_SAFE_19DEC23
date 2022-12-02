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
    public class PersonBeneficiariesRepository : BaseColleagueRepository, IPersonBeneficiariesRepository
    {
        private static char _VM = Convert.ToChar(DynamicArray.VM);

        /// <summary>
        /// ..ctor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public PersonBeneficiariesRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        ///  Get a collection of PersonBenefitDependent domain entity objects
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns>collection of PersonBenefitDependent domain entity objects</returns>
        public async Task<Tuple<IEnumerable<PersonBeneficiary>, int>> GetPersonBeneficiariesAsync(int offset, int limit, bool bypassCache = false)
        {
            var totalCount = 0;
            var personBeneficiariesEntities = new List<PersonBeneficiary>();
            var criteria = "WITH PERBEN.INTG.BENEFICIARY.IDX NE '' BY.EXP PERBEN.INTG.BENEFICIARY.IDX";

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
                var personsIds = perbensCollection.SelectMany(p => p.PerbenBeneficiaryId).Distinct().ToArray();
                var personsCollection = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", personsIds);
                var institutionsCollection = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Institutions>("INSTITUTIONS", personsIds);
                
                foreach (var key in keysSubList)
                {

                    //var perposKey = key.Split('|');
                    //var perpos = jobappsCollection.FirstOrDefault(x => x.Recordkey == perposKey[0]);
                    var perbens = perbensCollection.FirstOrDefault(x => x.Recordkey == key);

                    try
                    {
                        foreach (var perbensIndex in perbens.PerbenIntgBeneficiaryIdx)
                        {

                            var perbenGuidInfo = await GetGuidFromRecordInfoAsync("PERBEN", perbens.Recordkey, "PERBEN.INTG.BENEFICIARY.IDX", perbensIndex);

                            var splitIndex = perbensIndex.Split('*');
                            var personId = "";
                            var date = "";
                            if (splitIndex.Count() > 2)
                            {
                                date = splitIndex[splitIndex.Count() - 1];
                                for (int i = 0; i < splitIndex.Count() - 1; i++)
                                {
                                    if (i == 0)
                                    {
                                        personId = splitIndex[i];
                                    }
                                    else
                                    {
                                        string.Concat(personId, "*", splitIndex[i]);
                                    }
                                }
                            }
                            else
                            {
                                personId = splitIndex[0];
                                date = splitIndex[1];
                            }
                            var offsetDate = new DateTime();

                            if (!string.IsNullOrEmpty(date))
                            {
                                offsetDate = DmiString.PickDateToDateTime(Convert.ToInt16(date));
                            }

                            int index = 0;

                            foreach (var beneficiaryId in perbens.PerbenBeneficiaryId)
                            {
                                if (beneficiaryId.Equals(personId) && ((!perbens.PerbenBfcyStartDate.Any() || (perbens.PerbenBfcyStartDate[index] == null) && string.IsNullOrEmpty(date)) || perbens.PerbenBfcyStartDate[index].Value.Equals(offsetDate)))
                                {
                                    //Determine if person, corp, or institution. Three different personBeneficiaries.Add based on if person, corp, or org
                                    //To determine, look through INSTITUTIONS to see if personId is in file. If so, then it's an institution, and get guid from 
                                    //institution record. If not an institution, check PERSON record CORP.INDICATOR. If set to Y, then it's a corporation, 
                                    //otherwise, it's a person. In both cases, get guid from PERSON.
                                    if (institutionsCollection != null && institutionsCollection.Where(i => i.Recordkey == personId) != null)
                                    {
                                        var institutionRecord = institutionsCollection.FirstOrDefault(i => i.Recordkey == personId);
                                        personBeneficiariesEntities.Add(new PersonBeneficiary(perbenGuidInfo, perbens.Recordkey)
                                        {
                                            PerbenBeneficiaryId = beneficiaryId,
                                            Institution = true,
                                            PerbenBeneficiaryType = perbens.PerbenBeneficiaryType.Any() && perbens.PerbenBeneficiaryType.Count >= index + 1 && !string.IsNullOrEmpty(perbens.PerbenBeneficiaryType[index]) ? perbens.PerbenBeneficiaryType[index] : null,
                                            PerbenBfcyDesgntnPct = perbens.PerbenBfcyDesgntnPct.Any() && perbens.PerbenBfcyDesgntnPct.Count >= index + 1 ? perbens.PerbenBfcyDesgntnPct[index] : null,
                                            PerbenBfcyStartDate =  perbens.PerbenBfcyStartDate.Any() && perbens.PerbenBfcyStartDate.Count >= index + 1 ? perbens.PerbenBfcyStartDate[index] : null, 
                                            PerbenBfcyEndDate = perbens.PerbenBfcyEndDate.Any() && perbens.PerbenBfcyEndDate.Count >= index + 1 ? perbens.PerbenBfcyEndDate[index] : null
                                        });
                                    }
                                    else if (personsCollection != null && personsCollection.Where(p => p.Recordkey == personId) != null)
                                    {   
                                        var personRecord = personsCollection.FirstOrDefault(p => p.Recordkey == personId);
                                        if (personRecord.PersonCorpIndicator != null && personRecord.PersonCorpIndicator.Equals("Y"))
                                        {
                                            personBeneficiariesEntities.Add(new PersonBeneficiary(perbenGuidInfo, perbens.Recordkey)
                                            {
                                                PerbenBeneficiaryId = beneficiaryId,
                                                Organization = true,
                                                PerbenBeneficiaryType = perbens.PerbenBeneficiaryType.Any() && perbens.PerbenBeneficiaryType.Count >= index + 1 && !string.IsNullOrEmpty(perbens.PerbenBeneficiaryType[index]) ? perbens.PerbenBeneficiaryType[index] : null,
                                                PerbenBfcyDesgntnPct = perbens.PerbenBfcyDesgntnPct.Any() && perbens.PerbenBfcyDesgntnPct.Count >= index + 1 ? perbens.PerbenBfcyDesgntnPct[index] : null,
                                                PerbenBfcyStartDate = perbens.PerbenBfcyStartDate.Any() && perbens.PerbenBfcyStartDate.Count >= index + 1 ? perbens.PerbenBfcyStartDate[index] : null,
                                                PerbenBfcyEndDate = perbens.PerbenBfcyEndDate.Any() && perbens.PerbenBfcyEndDate.Count >= index + 1 ? perbens.PerbenBfcyEndDate[index] : null
                                            });
                                        }
                                        else
                                        {
                                            personBeneficiariesEntities.Add(new PersonBeneficiary(perbenGuidInfo, perbens.Recordkey)
                                            {
                                                PerbenBeneficiaryId = beneficiaryId,
                                                Person = true,
                                                PerbenBeneficiaryType = perbens.PerbenBeneficiaryType.Any() && perbens.PerbenBeneficiaryType.Count >= index + 1 && !string.IsNullOrEmpty(perbens.PerbenBeneficiaryType[index]) ? perbens.PerbenBeneficiaryType[index] : null,
                                                PerbenBfcyDesgntnPct = perbens.PerbenBfcyDesgntnPct.Any() && perbens.PerbenBfcyDesgntnPct.Count >= index + 1 ? perbens.PerbenBfcyDesgntnPct[index] : null,
                                                PerbenBfcyStartDate = perbens.PerbenBfcyStartDate.Any() && perbens.PerbenBfcyStartDate.Count >= index + 1 ? perbens.PerbenBfcyStartDate[index] : null,
                                                PerbenBfcyEndDate = perbens.PerbenBfcyEndDate.Any() && perbens.PerbenBfcyEndDate.Count >= index + 1 ? perbens.PerbenBfcyEndDate[index] : null
                                            });
                                        }
                                    }
                                }
                                index++;
                            }

                            index = 0;

                            foreach (var orgBeneficiary in perbens.PerbenOrgBeneficiary)
                            {
                                if (orgBeneficiary.Equals(personId) && (((!perbens.PerbenOrgStartDate.Any() || perbens.PerbenOrgStartDate[index] == null) && string.IsNullOrEmpty(date)) || perbens.PerbenOrgStartDate[index].Value.Equals(offsetDate)))
                                {
                                    personBeneficiariesEntities.Add(new PersonBeneficiary(perbenGuidInfo, perbens.Recordkey)
                                    {
                                        PerbenOrgBeneficiary = orgBeneficiary,
                                        PerbenOrgBfcyType = perbens.PerbenOrgBfcyType.Any() && perbens.PerbenOrgBfcyType.Count >= index + 1 && !string.IsNullOrEmpty(perbens.PerbenOrgBfcyType[index]) ? perbens.PerbenOrgBfcyType[index] : null,
                                        PerbenOrgBfcyDesgntnPct = perbens.PerbenOrgBfcyDesgntnPct.Any() && perbens.PerbenOrgBfcyDesgntnPct.Count >= index + 1 ? perbens.PerbenOrgBfcyDesgntnPct[index] : null,
                                        PerbenOrgStartDate = perbens.PerbenOrgStartDate.Any() && perbens.PerbenOrgStartDate.Count >= index + 1 ? perbens.PerbenOrgStartDate[index] : null,
                                        PerbenOrgEndDate = perbens.PerbenOrgEndDate.Any() && perbens.PerbenOrgEndDate.Count >= index + 1 ? perbens.PerbenOrgEndDate[index] : null
                                    });
                                }
                                index++;
                            }
                            //personBeneficiariesEntities.Add(new PersonBeneficiary(perbenGuidInfo, perbens.Recordkey)
                            //{
                                
                            //    ProviderName = (perbens.PerbenDepProviderName.Any() && perbens.PerbenDepProviderName.Count >= index + 1 && !string.IsNullOrEmpty(perbens.PerbenDepProviderName[index])) ? perbens.PerbenDepProviderName[index] : null,
                            //    ProviderIdentification = (perbens.PerbenDepProviderId.Any() && perbens.PerbenDepProviderId.Count >= index + 1 && !string.IsNullOrEmpty(perbens.PerbenDepProviderId[index])) ? perbens.PerbenDepProviderId[index] : null,
                            //    CoverageStartOn = (perbens.PerbenDepStartDate.Any() && perbens.PerbenDepStartDate.Count >= index + 1) ? perbens.PerbenDepStartDate[index] : null,
                            //    CoverageEndOn = (perbens.PerbenDepEndDate.Any() && perbens.PerbenDepEndDate.Count >= index + 1) ? perbens.PerbenDepEndDate[index] : null,
                            //    StudentStatus = (perbens.PerbenDepFullTimeStudent.Any() && perbens.PerbenDepFullTimeStudent.Count >= index + 1) ? perbens.PerbenDepFullTimeStudent[index] : null,
                            //});

                            index++;
                        }
                    }
                    catch (Exception e)
                    {
                        throw new ColleagueWebApiException(e.Message);
                    }
                }
            }

            return new Tuple<IEnumerable<PersonBeneficiary>, int>(personBeneficiariesEntities, totalCount);

        }

        /// <summary>
        /// Returns a review for a specified Person Beneficiary key.
        /// </summary>
        /// <param name="ids">Key to Person Beneficiary to be returned</param>
        /// <returns>PersonBeneficiary Objects</returns>
        public async Task<PersonBeneficiary> GetPersonBeneficiaryByIdAsync(string id)
        {
            var personBeneficiariesEntities = new List<PersonBeneficiary>();
            var criteria = "WITH PERBEN.INTG.BENEFICIARY.IDX NE '' BY.EXP PERBEN.INTG.BENEFICIARY.IDX";

            var perbenIds = await DataReader.SelectAsync("PERBEN", criteria);

            var perbenIds2 = new List<string>();
            foreach (var perbenId in perbenIds)
            {
                var perbenKey = perbenId.Split(_VM)[0];
                perbenIds2.Add(perbenKey);
            }

            perbenIds2.Sort();

            if (perbenIds2.Any())
            {

                var perbensCollection = await DataReader.BulkReadRecordAsync<Perben>("PERBEN", perbenIds2.ToArray());
                var personsIds = perbensCollection.SelectMany(p => p.PerbenBeneficiaryId).Distinct().ToArray();
                var personsCollection = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", personsIds);
                var institutionsCollection = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Institutions>("INSTITUTIONS", personsIds);

                foreach (var key in perbenIds2)
                {

                    var perbens = perbensCollection.FirstOrDefault(x => x.Recordkey == key);

                    try
                    {
                        foreach (var perbensIndex in perbens.PerbenIntgBeneficiaryIdx)
                        {

                            var perbenGuidInfo = await GetGuidFromRecordInfoAsync("PERBEN", perbens.Recordkey, "PERBEN.INTG.BENEFICIARY.IDX", perbensIndex);

                            var splitIndex = perbensIndex.Split('*');
                            var personId = "";
                            var date = "";
                            if (splitIndex.Count() > 2)
                            {
                                date = splitIndex[splitIndex.Count() - 1];
                                for (int i = 0; i < splitIndex.Count() - 1; i++)
                                {
                                    if (i == 0)
                                    {
                                        personId = splitIndex[i];
                                    }
                                    else
                                    {
                                        string.Concat(personId, "*", splitIndex[i]);
                                    }
                                }
                            }
                            else
                            {
                                personId = splitIndex[0];
                                date = splitIndex[1];
                            }
                            var offsetDate = DmiString.PickDateToDateTime(Convert.ToInt16(date));
                            int index = 0;

                            foreach (var beneficiaryId in perbens.PerbenBeneficiaryId)
                            {
                                if (beneficiaryId.Equals(personId) && perbens.PerbenBfcyStartDate[index].Value.Equals(offsetDate))
                                {
                                    //Determine if person, corp, or institution. Three different personBeneficiaries.Add based on if person, corp, or org
                                    //To determine, look through INSTITUTIONS to see if personId is in file. If so, then it's an institution, and get guid from 
                                    //institution record. If not an institution, check PERSON record CORP.INDICATOR. If set to Y, then it's a corporation, 
                                    //otherwise, it's a person. In both cases, get guid from PERSON.
                                    if (institutionsCollection != null && institutionsCollection.Where(i => i.Recordkey == personId) != null)
                                    {
                                        var institutionRecord = institutionsCollection.FirstOrDefault(i => i.Recordkey == personId);
                                        return new PersonBeneficiary(perbenGuidInfo, perbens.Recordkey)
                                        {
                                            PerbenBeneficiaryId = beneficiaryId,
                                            Institution = true,
                                            PerbenBeneficiaryType = perbens.PerbenBeneficiaryType.Any() && perbens.PerbenBeneficiaryType.Count >= index + 1 && !string.IsNullOrEmpty(perbens.PerbenBeneficiaryType[index]) ? perbens.PerbenBeneficiaryType[index] : null,
                                            PerbenBfcyDesgntnPct = perbens.PerbenBfcyDesgntnPct.Any() && perbens.PerbenBfcyDesgntnPct.Count >= index + 1 ? perbens.PerbenBfcyDesgntnPct[index] : null,
                                            PerbenBfcyStartDate = perbens.PerbenBfcyStartDate.Any() && perbens.PerbenBfcyStartDate.Count >= index + 1 ? perbens.PerbenBfcyStartDate[index] : null,
                                            PerbenBfcyEndDate = perbens.PerbenBfcyEndDate.Any() && perbens.PerbenBfcyEndDate.Count >= index + 1 ? perbens.PerbenBfcyEndDate[index] : null
                                        };
                                    }
                                    else if (personsCollection != null && personsCollection.Where(p => p.Recordkey == personId) != null)
                                    {
                                        var personRecord = personsCollection.FirstOrDefault(p => p.Recordkey == personId);
                                        if (personRecord.PersonCorpIndicator != null && personRecord.PersonCorpIndicator.Equals("Y"))
                                        {
                                            return new PersonBeneficiary(perbenGuidInfo, perbens.Recordkey)
                                            {
                                                PerbenBeneficiaryId = beneficiaryId,
                                                Organization = true,
                                                PerbenBeneficiaryType = perbens.PerbenBeneficiaryType.Any() && perbens.PerbenBeneficiaryType.Count >= index + 1 && !string.IsNullOrEmpty(perbens.PerbenBeneficiaryType[index]) ? perbens.PerbenBeneficiaryType[index] : null,
                                                PerbenBfcyDesgntnPct = perbens.PerbenBfcyDesgntnPct.Any() && perbens.PerbenBfcyDesgntnPct.Count >= index + 1 ? perbens.PerbenBfcyDesgntnPct[index] : null,
                                                PerbenBfcyStartDate = perbens.PerbenBfcyStartDate.Any() && perbens.PerbenBfcyStartDate.Count >= index + 1 ? perbens.PerbenBfcyStartDate[index] : null,
                                                PerbenBfcyEndDate = perbens.PerbenBfcyEndDate.Any() && perbens.PerbenBfcyEndDate.Count >= index + 1 ? perbens.PerbenBfcyEndDate[index] : null
                                            };
                                        }
                                        else
                                        {
                                            return new PersonBeneficiary(perbenGuidInfo, perbens.Recordkey)
                                            {
                                                PerbenBeneficiaryId = beneficiaryId,
                                                Person = true,
                                                PerbenBeneficiaryType = perbens.PerbenBeneficiaryType.Any() && perbens.PerbenBeneficiaryType.Count >= index + 1 && !string.IsNullOrEmpty(perbens.PerbenBeneficiaryType[index]) ? perbens.PerbenBeneficiaryType[index] : null,
                                                PerbenBfcyDesgntnPct = perbens.PerbenBfcyDesgntnPct.Any() && perbens.PerbenBfcyDesgntnPct.Count >= index + 1 ? perbens.PerbenBfcyDesgntnPct[index] : null,
                                                PerbenBfcyStartDate = perbens.PerbenBfcyStartDate.Any() && perbens.PerbenBfcyStartDate.Count >= index + 1 ? perbens.PerbenBfcyStartDate[index] : null,
                                                PerbenBfcyEndDate = perbens.PerbenBfcyEndDate.Any() && perbens.PerbenBfcyEndDate.Count >= index + 1 ? perbens.PerbenBfcyEndDate[index] : null
                                            };
                                        }
                                    }
                                }
                                index++;
                            }

                            index = 0;

                            foreach (var orgBeneficiary in perbens.PerbenOrgBeneficiary)
                            {
                                if (orgBeneficiary.Equals(personId) && perbens.PerbenOrgStartDate[index].Value.Equals(offsetDate))
                                {
                                    return new PersonBeneficiary(perbenGuidInfo, perbens.Recordkey)
                                    {
                                        PerbenOrgBeneficiary = orgBeneficiary,
                                        PerbenOrgBfcyType = perbens.PerbenOrgBfcyType.Any() && perbens.PerbenOrgBfcyType.Count >= index + 1 && !string.IsNullOrEmpty(perbens.PerbenOrgBfcyType[index]) ? perbens.PerbenOrgBfcyType[index] : null,
                                        PerbenOrgBfcyDesgntnPct = perbens.PerbenOrgBfcyDesgntnPct.Any() && perbens.PerbenOrgBfcyDesgntnPct.Count >= index + 1 ? perbens.PerbenOrgBfcyDesgntnPct[index] : null,
                                        PerbenOrgStartDate = perbens.PerbenOrgStartDate.Any() && perbens.PerbenOrgStartDate.Count >= index + 1 ? perbens.PerbenOrgStartDate[index] : null,
                                        PerbenOrgEndDate = perbens.PerbenOrgEndDate.Any() && perbens.PerbenOrgEndDate.Count >= index + 1 ? perbens.PerbenOrgEndDate[index] : null
                                    };
                                }
                                index++;
                            }

                            index++;
                        }
                    }
                    catch (Exception e)
                    {
                        throw new ColleagueWebApiException(e.Message);
                    }
                }
            }

            return null;

        }

        /// <summary>
        /// Get the GUID for an Entity
        /// </summary>
        /// <param name="key"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<string> GetGuidFromID(string key, string entity)
        {
            try
            {
                return await GetGuidFromRecordInfoAsync(entity, key);
            }
            catch (RepositoryException REX)
            {
                REX.AddError(new RepositoryError(entity + ".guid.NotFound", "GUID not found for " + entity + "id " + key));
                throw REX;
            }

        }

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
                ex.AddError(new RepositoryError(entity + ".guid.NotFound", "GUID not found for " + entity + "id " + id));
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
