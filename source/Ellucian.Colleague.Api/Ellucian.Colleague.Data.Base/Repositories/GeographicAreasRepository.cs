//Copyright 2018-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using System.Linq;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Dmi.Runtime;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Services;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class GeographicAreasRepository : BaseColleagueRepository, IGeographicAreasRepository
    {
        public static char _VM = Convert.ToChar(DynamicArray.VM);
        const int AllGeographicAreasCacheTimeout = 20; // Clear from cache every 20 minutes
        const string AllGeographicAreasCache = "AllGeographicAreas";

        /// <summary>
        /// ..ctor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public GeographicAreasRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        #region GET Method
        /// <summary>
        ///  Get a collection of GeographicArea domain entity objects
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns>collection of GeographicArea domain entity objects</returns>
        public async Task<Tuple<IEnumerable<GeographicArea>, int>> GetGeographicAreasAsync(int offset, int limit, bool bypassCache = false)
        {
            var totalCount = 0;
            var geographicAreaEntities = new List<GeographicArea>();
            var criteria = "";

            var keys = new List<string>();

            string geographicAreasCacheKey = CacheSupport.BuildCacheKey(AllGeographicAreasCache);

            var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
                    this,
                    ContainsKey,
                    GetOrAddToCacheAsync,
                    AddOrUpdateCacheAsync,
                    transactionInvoker,
                    geographicAreasCacheKey,
                    "",
                     offset,
                    limit,
                    AllGeographicAreasCacheTimeout,

                    async () =>
                    {
                        var chapterIds = await DataReader.SelectAsync("CHAPTERS", criteria.ToString());

                        var chaptersId2 = new List<string>();
                        foreach (var chaptersId in chapterIds)
                        {
                            var chaptersKey = string.Concat("CHAPTERS*" + chaptersId);
                            keys.Add(chaptersKey);
                        }

                        var countyIds = await DataReader.SelectAsync("COUNTIES", criteria.ToString());

                        var countiesId2 = new List<string>();
                        foreach (var countiesId in countyIds)
                        {
                            var countiesKey = string.Concat("COUNTIES*" + countiesId);
                            keys.Add(countiesKey);
                        }

                        var zipCodeIds = await DataReader.SelectAsync("ZIP.CODE.XLAT", criteria.ToString());

                        var zipCodesId2 = new List<string>();
                        foreach (var zipCodesId in zipCodeIds)
                        {
                            var zipCodesKey = string.Concat("ZIPCODEXLAT*" + zipCodesId);
                            keys.Add(zipCodesKey);
                        };

                        if (keys == null || !keys.Any())
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }

                        CacheSupport.KeyCacheRequirements requirements = new CacheSupport.KeyCacheRequirements()
                        {
                            limitingKeys = keys.Distinct().ToList(),
                            criteria = "",
                        };

                        return requirements;
                    });

            if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
            {
                return new Tuple<IEnumerable<Domain.Base.Entities.GeographicArea>, int>(geographicAreaEntities, 0);
            }

            var keysSubList = keyCache.Sublist;
            totalCount = keyCache.TotalCount.Value;                                  

            if (keysSubList.Any())
            {
                var chaptersSubListKey = new List<string>();
                Collection<Chapters> chaptersCollection = null;
                foreach (var key in keysSubList.Where(i => i.Split('*')[0].Equals("CHAPTERS")))
                {
                    var geographicAreaKey = key.Split('*');
                    var fileName = geographicAreaKey[0];
                    var recordKey = key.Substring(fileName.Length + 1);
                    chaptersSubListKey.Add(recordKey);
                }
                if (chaptersSubListKey.Any())
                {
                    chaptersCollection = await DataReader.BulkReadRecordAsync<Chapters>("CHAPTERS", chaptersSubListKey.ToArray());
                }

                var countiesSubListKey = new List<string>();
                Collection<Counties> countiesCollection = null;
                foreach (var key in keysSubList.Where(i => i.Split('*')[0].Equals("COUNTIES")))
                {
                    var geographicAreaKey = key.Split('*');
                    var fileName = geographicAreaKey[0];
                    var recordKey = key.Substring(fileName.Length + 1);
                    countiesSubListKey.Add(recordKey);
                }
                if (countiesSubListKey.Any())
                {
                    countiesCollection = await DataReader.BulkReadRecordAsync<Counties>("COUNTIES", countiesSubListKey.ToArray());
                }
                
                var zipCodesSubListKey = new List<string>();
                Collection<ZipCodeXlat> zipCodesCollection = null;
                foreach (var key in keysSubList.Where(i => i.Split('*')[0].Equals("ZIPCODEXLAT")))
                {
                    var geographicAreaKey = key.Split('*');
                    var fileName = geographicAreaKey[0];
                    var recordKey = key.Substring(fileName.Length + 1);
                    zipCodesSubListKey.Add(recordKey);
                }
                if (zipCodesSubListKey.Any())
                {
                    zipCodesCollection = await DataReader.BulkReadRecordAsync<ZipCodeXlat>("ZIP.CODE.XLAT", zipCodesSubListKey.ToArray());
                }

                foreach (var key in keysSubList)
                {
                    try
                    {
                        var geographicAreasKey = key.Split('*');
                        var fileName = geographicAreasKey[0];
                        var recordKey = key.Substring(fileName.Length + 1);

                        switch (fileName)
                        {
                            case "CHAPTERS":
                                if (chaptersCollection != null && chaptersCollection.Any())
                                {
                                    var chapter = chaptersCollection.FirstOrDefault(x => x.Recordkey == recordKey);
                                    if (chapter == null)
                                    {
                                        throw new KeyNotFoundException(string.Format("Geographic area not found for CHAPTERS '{0}'. ", recordKey));
                                    }
                                    geographicAreaEntities.Add(new GeographicArea(chapter.RecordGuid, chapter.Recordkey, !string.IsNullOrEmpty(chapter.ChaptersDesc) ? chapter.ChaptersDesc : chapter.Recordkey, "FUND"));
                                }
                                else
                                {
                                    throw new KeyNotFoundException(string.Format("Geographic area not found for CHAPTERS '{0}'. ", recordKey));
                                }
                                break;
                            case "COUNTIES":
                                if (countiesCollection != null && countiesCollection.Any())
                                {
                                    var county = countiesCollection.FirstOrDefault(x => x.Recordkey == recordKey);
                                    if (county == null)
                                    {
                                        throw new KeyNotFoundException(string.Format("Geographic area not found for COUNTIES '{0}'. ", recordKey));
                                    }
                                    geographicAreaEntities.Add(new GeographicArea(county.RecordGuid, county.Recordkey, !string.IsNullOrEmpty(county.CntyDesc) ? county.CntyDesc : county.Recordkey, "GOV"));
                                }
                                else
                                {
                                    throw new KeyNotFoundException(string.Format("Geographic area not found for COUNTIES '{0}'. ", recordKey));
                                }
                                break;
                            case "ZIPCODEXLAT":
                                if (zipCodesCollection != null && zipCodesCollection.Any())
                                {
                                    var zipCode = zipCodesCollection.FirstOrDefault(x => x.Recordkey == recordKey);
                                    if (zipCode == null)
                                    {
                                        throw new KeyNotFoundException(string.Format("Geographic area not found for ZIP.CODE.XLAT '{0}'. ", recordKey));
                                    }
                                    geographicAreaEntities.Add(new GeographicArea(zipCode.RecordGuid, zipCode.Recordkey, "Zipcode", "POST"));
                                }      
                                else
                                {
                                    throw new KeyNotFoundException(string.Format("Geographic area not found for ZIP.CODE.XLAT '{0}'. ", recordKey));
                                }
                                break;
                        }
                    }
                    catch (KeyNotFoundException ex)
                    {
                        throw ex;
                    }
                    catch (Exception ex)
                    {
                        var geographicAreasKey = key.Split('*');
                        var fileName = geographicAreasKey[0];
                        var recordKey = key.Substring(fileName.Length + 1);
                        var repoError = new RepositoryException();
                        repoError.AddError(new RepositoryError("RepositoryException", ex.Message));
                        repoError.AddError(new RepositoryError("RepositoryException", string.Format("Error reading from File '{0}' with a Key of '{1}'.", fileName, recordKey)));
                        throw repoError;
                    }
                }
            }
            return new Tuple<IEnumerable<GeographicArea>, int>(geographicAreaEntities, totalCount);

        }

        /// <summary>
        /// Get a single GeographicArea entity
        /// </summary>
        /// <param name="guid">Key to GeographicArea</param>
        /// <returns>GeographicArea Entity</returns>
        public async Task<GeographicArea> GetGeographicAreaByIdAsync(string guid)
        {
            var guidObject = await GetRecordInfoFromGuidAsync(guid);
            if (guidObject == null || string.IsNullOrEmpty(guidObject.Entity) || string.IsNullOrEmpty(guidObject.PrimaryKey))
            {
                throw new KeyNotFoundException("Geographic area not found for GUID " + guid);
            }
            try
            {
                var fileName = guidObject.Entity;
                var recordKey = guidObject.PrimaryKey;
                if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(recordKey))
                {
                    throw new KeyNotFoundException("Geographic area not found for GUID " + guid);
                }

                switch (fileName)
                {
                    case "CHAPTERS":
                        var chapter = await DataReader.ReadRecordAsync<Chapters>(recordKey);
                        if (chapter == null)
                        {
                            throw new KeyNotFoundException("Geographic area not found for GUID " + guid);
                        }
                        return new GeographicArea(chapter.RecordGuid, chapter.Recordkey, !string.IsNullOrEmpty(chapter.ChaptersDesc) ? chapter.ChaptersDesc : chapter.Recordkey, "FUND");
                    case "COUNTIES":
                        var county = await DataReader.ReadRecordAsync<Counties>(recordKey);
                        if (county == null)
                        {
                            throw new KeyNotFoundException("Geographic area not found for GUID " + guid);
                        }
                        return new GeographicArea(county.RecordGuid, county.Recordkey, !string.IsNullOrEmpty(county.CntyDesc) ? county.CntyDesc : county.Recordkey, "GOV");
                    case "ZIP.CODE.XLAT":
                        var zipCode = await DataReader.ReadRecordAsync<ZipCodeXlat>(recordKey);
                        if (zipCode == null)
                        {
                            throw new KeyNotFoundException("Geographic area not found for GUID " + guid);
                        }
                        return new GeographicArea(zipCode.RecordGuid, zipCode.Recordkey, "Zipcode", "POST");
                    default:
                        throw new KeyNotFoundException("Geographic area not found for GUID " + guid);
                }
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                var repoError = new RepositoryException();
                repoError.AddError(new RepositoryError("RepositoryException", ex.Message));
                repoError.AddError(new RepositoryError("RepositoryException", string.Format("Error reading from File '{0}' with a Key of '{1}'.", guidObject.Entity, guidObject.PrimaryKey)));
                throw repoError;
            }
        }

        #endregion
    }

}
