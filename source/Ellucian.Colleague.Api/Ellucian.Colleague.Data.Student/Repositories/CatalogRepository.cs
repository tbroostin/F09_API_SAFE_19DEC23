// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Utility;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class CatalogRepository : BaseColleagueRepository, ICatalogRepository
    {
        public CatalogRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache timout value for data that changes rarely
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Get all catalog items 
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<Catalog>> GetAsync()
        {
            var catalogs = await GetOrAddToCacheAsync<List<Catalog>>("AllCatalogs",
                async () =>
                {
                    Collection<Catalogs> catalogData = await DataReader.BulkReadRecordAsync<Catalogs>("");
                    var catalogList = BuildCatalogs(catalogData);
                    return catalogList.ToList();
                }
            );
            return catalogs;
        }

        /// <summary>
        /// Get all catalog items 
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<Catalog>> GetAsync(bool bypassCache = false)
        {
            if (bypassCache == false) 
            { 
            var catalogs =await GetOrAddToCacheAsync<List<Catalog>>("AllCatalogs",
                async () =>
                {
                    Collection<Catalogs> catalogData = await DataReader.BulkReadRecordAsync<Catalogs>("");
                    var catalogList = BuildCatalogs(catalogData);
                    return catalogList.ToList();
                }
            );
            return catalogs;
            }

            Collection<Catalogs> catalogData2 = await DataReader.BulkReadRecordAsync<Catalogs>("");
            var catalogList2 = BuildCatalogs(catalogData2);
            return catalogList2.ToList();

        }

        /// <summary>
        /// Build Catalog entities
        /// </summary>
        /// <param name="catalogData"></param>
        /// <returns>Colleaction of Catalog</returns>
        /// <summary>
        /// Get guid for Catalog code
        /// </summary>
        /// <param name="code">AcademicLevels code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetCatalogGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetAsync(false);
            Catalog codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }
            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetAsync(true);
                if (allCodesCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CATALOGS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CATALOGS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CATALOGS', Record ID:'", code, "'"));
            }
            return guid;

        }

        private ICollection<Catalog> BuildCatalogs(Collection<Catalogs> catalogData)
        {
            var catalogs = new List<Catalog>();
            if (catalogData != null && catalogData.Count() > 0)
            {
                foreach (var catalog in catalogData)
                {
                    try
                    {
                        Catalog newCatalog = new Catalog(catalog.RecordGuid, catalog.Recordkey, catalog.CatDesc, catalog.CatStartDate.GetValueOrDefault(), catalog.CatHideInSsWhatIf);
                        if (catalog.CatEndDate != null)
                        {
                            newCatalog.EndDate = catalog.CatEndDate.GetValueOrDefault(DateTime.MinValue);
                        }
                        newCatalog.AcadPrograms = catalog.CatAcadPrograms;
                        catalogs.Add(newCatalog);
                    }
                    catch (Exception ex)
                    {
                        LogDataError("Catalog", catalog.Recordkey, catalog, ex);
                    } 
                }
            }
            return catalogs;
        }
    }
}
