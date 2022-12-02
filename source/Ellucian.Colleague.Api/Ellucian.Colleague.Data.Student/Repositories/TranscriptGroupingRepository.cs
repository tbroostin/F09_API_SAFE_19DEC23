// Copyright 2014-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Utility;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class TranscriptGroupingRepository : BaseColleagueRepository, ITranscriptGroupingRepository
    {
        public TranscriptGroupingRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Return the set of all transcript groupings
        /// </summary>
        /// <returns>A set of transcript groupings</returns>
        public async Task<IEnumerable<TranscriptGrouping>> GetAsync()
        {
            // Get all terms from cache. If not already in cache, add them.
            var transcriptGroupings = await GetOrAddToCacheAsync<List<TranscriptGrouping>>("AllTranscriptGroupings",
                async () =>
                {
                    try
                    {
                        // Get the valid transcript grouping codes from StWebDefaults
                        Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", true);
                        if (stwebDefaults == null)
                        {
                            var errorMessage = "Unable to access student web defaults from ST.PARMS/STWEB.DEFAULTS.";
                            logger.Info(errorMessage);
                            throw new ApplicationException(errorMessage);
                        }

                        Collection<TranscriptGroupings> transcriptGroupinsData = await DataReader.BulkReadRecordAsync<TranscriptGroupings>("", true);
                        var transcriptGroupingList = BuildTranscriptGroupings(transcriptGroupinsData, stwebDefaults.StwebTranAllowedGroupings);
                        return transcriptGroupingList;
                    }
                    catch (ColleagueSessionExpiredException csee)
                    {
                        logger.Error(csee, "Colleague session expired while retrieving transcript groupings");
                        throw;
                    }
                    catch (Exception exception)
                    {
                        string errorMessage = "Unable to read all transcript groupings from the database.";
                        logger.Error(exception.ToString());
                        logger.Error(errorMessage);
                        throw new ApplicationException(errorMessage, exception);
                    }
                }
            );

            return transcriptGroupings;
        }

        /// <summary>
        /// Changes the term data contracts into Term entities
        /// </summary>
        /// <param name="termData">Term data contracts</param>
        /// <returns>Term entities</returns>
        public List<TranscriptGrouping> BuildTranscriptGroupings(Collection<TranscriptGroupings> transcriptGroupingsData, IEnumerable<string> validTranscriptGroupings)
        {
            var transcriptGroupings = new List<TranscriptGrouping>();

            if (transcriptGroupingsData != null)
            {
                foreach (var transcriptGrouping in transcriptGroupingsData)
                {
                    try
                    {
                        var isSelectable = validTranscriptGroupings.Contains(transcriptGrouping.Recordkey);
                        var transcriptGroupingDomainEntity = new TranscriptGrouping(transcriptGrouping.Recordkey, transcriptGrouping.TrgpDesc, isSelectable);

                        transcriptGroupings.Add(transcriptGroupingDomainEntity);
                    }
                    catch (Exception ex)
                    {
                        // If a domain entity cannot be created, log an error but continue loading the rest of them.
                        LogDataError("Transcript Grouping", transcriptGrouping.Recordkey, transcriptGrouping, ex);
                    }
                }
            }
            return transcriptGroupings;
        }
    }
}
