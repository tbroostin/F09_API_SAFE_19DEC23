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

        private ICollection<Catalog> BuildCatalogs(Collection<Catalogs> catalogData)
        {
            var catalogs = new List<Catalog>();
            if (catalogData != null && catalogData.Count() > 0)
            {
                foreach (var catalog in catalogData)
                {
                    try
                    {
                        Catalog newCatalog = new Catalog(catalog.RecordGuid, catalog.Recordkey, catalog.CatDesc, catalog.CatStartDate.GetValueOrDefault());
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
