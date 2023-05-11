// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Ellucian.Data.Colleague;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Data.Colleague.DataContracts;
using slf4net;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Utility;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Repository for important numbers
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class ImportantNumberRepository : BaseColleagueRepository, IImportantNumberRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportantNumberRepository"/> class.
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="transactionFactory">The transaction factory.</param>
        /// <param name="logger">The logger.</param>
        public ImportantNumberRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue; // 24 hrs
        }

        /// <summary>
        /// Important Numbers
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ImportantNumber> Get()
        {
            string cacheKeyToUse = string.Empty;
            if (this.DataReader.IsAnonymous)
            {
                cacheKeyToUse = "ImportantNumbers_Anonymous";
            }
            else
            {
                cacheKeyToUse = "ImportantNumbers";
            }
            var importantNumbers = GetOrAddToCache<List<ImportantNumber>>(cacheKeyToUse,
                () =>
                {
                    List<ImportantNumber> importantNumberList = new List<ImportantNumber>();
                    string criteria = string.Empty;
                    // If the DataReader is Anonymous, and if the IMPORTANT.NUMBERS table is not defined in Colleague as a "public" file,
                    // then an exception will be thrown.  
                    try
                    {
                        Collection<ImportantNumbers> impData = this.DataReader.BulkReadRecord<ImportantNumbers>(criteria);
                        if (impData != null)
                        {
                            importantNumberList = BuildImportantNumbers(impData);
                        }
                    }
                    catch (Exception)
                    {
                        throw new ApplicationException("Anonymous data reader request denied. Table is not public.");
                    }
                    
                    return importantNumberList;
                });

            return importantNumbers;
        }

        /// <summary>
        /// Important Number Categories
        /// </summary>
        public IEnumerable<ImportantNumberCategory> ImportantNumberCategories
        {
            get
            {
                var impNumCategories = GetOrAddToCache<List<ImportantNumberCategory>>("AllImportantNumberCategories",
                    () =>
                    {
                        string cacheKeyToUse = string.Empty;
                        if (this.DataReader.IsAnonymous)
                        {
                            cacheKeyToUse = "ImportantNumberCategories_Anonymous";
                        }
                        else
                        {
                            cacheKeyToUse = "ImportantNumberCategories";
                        }
                        List<ImportantNumberCategory> impNumCats = new List<ImportantNumberCategory>();
                        try
                        {
                            ApplValcodes impNumCatValcode = this.DataReader.ReadRecord<ApplValcodes>("CORE.VALCODES", "MOBILE.DIRECTORY.CATEGORIES");
                            if (impNumCatValcode != null)
                            {
                                foreach (ApplValcodesVals applVal in impNumCatValcode.ValsEntityAssociation)
                                {
                                    impNumCats.Add(new ImportantNumberCategory(applVal.ValInternalCodeAssocMember, applVal.ValExternalRepresentationAssocMember));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // if the valcode is not public, don't throw, but return an empty list
                            //throw new ApplicationException("Anonymous data reader request denied. Table is not public.");
                            logger.Error(ex.Message, "Valcode is not public.");
                        }
                        return impNumCats;
                    }
                );
                return impNumCategories;
            }
        }

        private List<ImportantNumber> BuildImportantNumbers(Collection<ImportantNumbers> inData)
        {
            var importantNumbers = new List<ImportantNumber>();
            if (inData != null)
            {
                foreach (var impNum in inData)
                {
                    try
                    {
                        ImportantNumber importantNumber = null;
                        if (!string.IsNullOrEmpty(impNum.ImpnumBuilding))
                        {
                            importantNumber = new ImportantNumber(impNum.Recordkey,
                                impNum.ImpnumName,
                                impNum.ImpnumPhone,
                                impNum.ImpnumPhoneExt,
                                impNum.ImpnumCategory,
                                impNum.ImpnumEmailAddress,
                                impNum.ImpnumBuilding,
                                impNum.ImpnumExportToMobile);
                        }
                        else
                        {
                            importantNumber = new ImportantNumber(impNum.Recordkey,
                                impNum.ImpnumName,
                                impNum.ImpnumCity,
                                impNum.ImpnumState,
                                impNum.ImpnumZip,
                                impNum.ImpnumCountry,
                                impNum.ImpnumPhone,
                                impNum.ImpnumPhoneExt,
                                impNum.ImpnumCategory,
                                impNum.ImpnumEmailAddress,
                                impNum.ImpnumAddressLines,
                                impNum.ImpnumExportToMobile,
                                impNum.ImpnumLatitude,
                                impNum.ImpnumLongitude,
                                impNum.ImpnumLocation);
                        }
                        importantNumbers.Add(importantNumber);
                    }
                    catch (Exception ex)
                    {
                        var inString = "Important Number Id: " + impNum.Recordkey + ", Name: " + impNum.ImpnumName + ", Category: " + impNum.ImpnumCategory;
                        LogDataError("Important Number", impNum.Recordkey, impNum, ex, inString);
                    }
                }
            }
            return importantNumbers;
        }
    }
}
