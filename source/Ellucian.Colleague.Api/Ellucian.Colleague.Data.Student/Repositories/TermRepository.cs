// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.

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
using slf4net;
using Ellucian.Colleague.Data.Base.DataContracts;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class TermRepository : BaseColleagueRepository, ITermRepository
    {
        public TermRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }

         /// <summary>
        /// Retrieve all terms
        /// </summary>
        /// <returns>All terms</returns>
        public async Task<IEnumerable<Term>> GetAsync()
        {
            return await GetAsync(false);
        }

        /// <summary>
        /// Retrieve all terms
        /// </summary>
        /// <returns>All terms</returns>
        public async Task<IEnumerable<Term>> GetAsync(bool clearCache)
        {
           
            if (clearCache && ContainsKey(BuildFullCacheKey("AllTerms")))
            {
                ClearCache(new List<string>{"AllTerms"});
            }
            
            // Get all terms from cache. If not already in cache, add them.
            var terms = await GetOrAddToCacheAsync<List<Term>>("AllTerms",
                async () =>
                {
                    try
                    {
                        // Get terms from the database if not in cache. 
                        // Limiting this to terms that are "available" for planning based on new field. 
                        // 
                        Collection<Ellucian.Colleague.Data.Student.DataContracts.Terms> termData =await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Terms>("TERMS", "");
                        Collection<TermsLocations> termLocationData = await DataReader.BulkReadRecordAsync<TermsLocations>("TERMS.LOCATIONS", "");
                        Collection<Locations> locationData = await DataReader.BulkReadRecordAsync<Locations>("LOCATIONS", "");
                        Collection<Sessions> sessionData = await DataReader.BulkReadRecordAsync<Sessions>("SESSIONS", "");
                    var termList = BuildTerms(termData, termLocationData, locationData, sessionData);
                        return termList;
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = "Unable to read all Terms from the database.";
                        logger.Error(ex.ToString());
                        logger.Error(errorMessage);
                        throw new ApplicationException(errorMessage);
                    }
                }
            );
            return terms;
        }

        public async Task<Term> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("Term Code must be specified");
            }
            try
            {
                return (await GetAsync()).Where(t => t.Code == id).First();
            }
            catch
            {
                // Not including the id of the record in the exception sent back but will include it in the message that is logged.
                logger.Error("No Term found for Code" + id);
                throw new KeyNotFoundException("Term not found");
            }
        }

        /// <summary>
        /// Retrieve specific term codes
        /// </summary>
        /// <param name="termCodes">Ids of the Terms desired </param>
        /// <returns>The requested terms.</returns>
        public async Task<IEnumerable<Term>> GetAsync(IEnumerable<string> termCodes)
        {
            if (termCodes == null || termCodes.Count() == 0)
            {
                throw new ArgumentNullException("termCodes", "Must provide at least one term code.");
            }
            var terms = new List<Term>();
            if ((termCodes != null) && (termCodes.Count() > 0))
            {
                IEnumerable<Term> allTerms = (await GetAsync()).AsEnumerable();
                StringBuilder termsNotFound = new StringBuilder();
                foreach (var termCode in termCodes)
                {
                    try
                    {
                        terms.Add(allTerms.Where(t => t.Code == termCode).First());
                    }
                    catch
                    {
                        termsNotFound.Append(" " + termCode);
                    }
                }
                if (termsNotFound.Length > 0)
                {
                    logger.Info("Term not found for term codes" + termsNotFound.ToString());
                }
            }
            return terms;
        }

        /// <summary>
        /// Returns the terms that have been defined as open for section search and registration
        /// </summary>
        /// <returns>Registration Terms</returns>
        public async Task<IEnumerable<Term>> GetRegistrationTermsAsync()
        {
            // Get list of terms from cache. If not there, read from database.
            var registrationTermsCodes = await GetOrAddToCacheAsync<List<string>>("RegistrationTerms",
                async () =>
                {
                    GetRegistrationTermsRequest registrationTermsRequest = new GetRegistrationTermsRequest();
                    GetRegistrationTermsResponse registrationTermsResponse = await transactionInvoker.ExecuteAsync<GetRegistrationTermsRequest, GetRegistrationTermsResponse>(registrationTermsRequest);
                    return registrationTermsResponse.AlRegistrationTerms;
                });
            if ((registrationTermsCodes == null) || (registrationTermsCodes.Count() == 0))
            {
                // If the list is null or empty, throw an exception
                //throw new KeyNotFoundException("No registration terms found");
                return new List<Term>();
            }
            else
            {
                // If the list has codes, use the "get many" method to return the term objects
                return await GetAsync(registrationTermsCodes);
            }
        }

        /// <summary>
        /// Changes the term data contracts into Term entities
        /// </summary>
        /// <param name="termData">Term data contracts</param>
        /// <returns>Term entities</returns>
        private List<Term> BuildTerms(Collection<Ellucian.Colleague.Data.Student.DataContracts.Terms> termData, Collection<TermsLocations> termLocationData, Collection<Locations> locationData, Collection<Sessions> sessionData)
        {
            var terms = new List<Term>();

            if (termData != null)
            {
                foreach (var term in termData)
                {
                    try
                    {
                        bool defaultOnPlan = false;
                        if (!string.IsNullOrEmpty(term.TermDefaultOnPlan) && term.TermDefaultOnPlan.ToUpper() == "Y")
                        {
                            defaultOnPlan = true;
                        }
                        bool useInBestFitTermCalc = false;
                        if (!string.IsNullOrEmpty(term.TermUseInBestFitCalc) && term.TermUseInBestFitCalc.Equals("Y", StringComparison.OrdinalIgnoreCase))
                        {
                            useInBestFitTermCalc = true;
                        }
                        bool forPlanning = false;
                        if (!string.IsNullOrEmpty(term.TermDegreePlanning) && term.TermDegreePlanning.ToUpper() == "Y")
                        {
                            forPlanning = true;
                        }
                        bool activeFlag = false;
                        if (term.TermStartDate <= DateTime.Now && term.TermEndDate >= DateTime.Now)
                        {
                            activeFlag = true;
                        }

                        bool checkRegPriority = false;
                        if (!string.IsNullOrEmpty(term.TermRegPriorityFlag) && term.TermRegPriorityFlag.ToUpper() == "Y")
                        {
                            checkRegPriority = true;
                        }
                        PeriodType? financialPeriod = null;
                        switch (term.TermPcfPeriod)
                        {
                            case "1":
                                financialPeriod = PeriodType.Past;
                                break;
                            case "2":
                                financialPeriod = PeriodType.Current;
                                break;
                            case "3":
                                financialPeriod = PeriodType.Future;
                                break;
                        }

                        Term newTerm = new Term(term.RecordGuid, 
                            term.Recordkey, term.TermDesc,
                            term.TermStartDate.GetValueOrDefault(),
                            term.TermEndDate.GetValueOrDefault(),
                            term.TermReportingYear.GetValueOrDefault(0),
                            Convert.ToInt32(term.TermSequenceNo),
                            defaultOnPlan,
                            forPlanning,
                            term.TermReportingTerm,
                            checkRegPriority)
                                {
                                    FinancialPeriod = financialPeriod,    
                                    IsActive = activeFlag,
                                    FinancialAidYears = term.TermFaYears,
                                    UseTermInBestFitCalculations = useInBestFitTermCalc
                                };

                        RegistrationDate registrationDates = new RegistrationDate(string.Empty,
                            term.TermRegStartDate, term.TermRegEndDate,
                            term.TermPreregStartDate, term.TermPreregEndDate,
                            term.TermAddStartDate, term.TermAddEndDate,
                            term.TermDropStartDate, term.TermDropEndDate,
                            term.TermDropGradeReqdDate,
                            term.TermCensusDates);

                        newTerm.AddRegistrationDates(registrationDates);

                        // Check to see if we have term specific dates
                        if (locationData != null)
                        {
                            foreach (var location in locationData)
                            {
                                var key = term.Recordkey + "*" + location.Recordkey;
                                TermsLocations termLocation = termLocationData.Where(t => t.Recordkey == key).FirstOrDefault();
                                if (termLocation != null)
                                {
                                    DateTime? registrationStartDate = (termLocation.TlocRegStartDate.HasValue) ? termLocation.TlocRegStartDate.Value : term.TermRegStartDate;
                                    DateTime? registrationEndDate = (termLocation.TlocRegEndDate.HasValue) ? termLocation.TlocRegEndDate.Value : term.TermRegEndDate;
                                    DateTime? preRegistrationStartDate = (termLocation.TlocPreregStartDate.HasValue) ? termLocation.TlocPreregStartDate.Value : term.TermPreregStartDate;
                                    DateTime? preRegistrationEndDate = (termLocation.TlocPreregEndDate.HasValue) ? termLocation.TlocPreregEndDate.Value : term.TermPreregEndDate;
                                    DateTime? addStartDate = (termLocation.TlocAddStartDate.HasValue) ? termLocation.TlocAddStartDate.Value : term.TermAddStartDate;
                                    DateTime? addEndDate = (termLocation.TlocAddEndDate.HasValue) ? termLocation.TlocAddEndDate.Value : term.TermAddEndDate;
                                    DateTime? dropStartDate = (termLocation.TlocDropStartDate.HasValue) ? termLocation.TlocDropStartDate.Value : term.TermDropStartDate;
                                    DateTime? dropEndDate = (termLocation.TlocDropEndDate.HasValue) ? termLocation.TlocDropEndDate.Value : term.TermDropEndDate;
                                    DateTime? dropGradeRequiredDate = (termLocation.TlocDropGradeReqdDate.HasValue) ? termLocation.TlocDropGradeReqdDate.Value : term.TermDropGradeReqdDate;
                                    List<DateTime?> censusDates = (termLocation.TlocCensusDates != null) ? termLocation.TlocCensusDates : term.TermCensusDates;
                                                                        
                                    RegistrationDate locationRegistrationDates = new RegistrationDate(location.Recordkey,
                                        registrationStartDate, registrationEndDate,
                                        preRegistrationStartDate, preRegistrationEndDate,
                                        addStartDate, addEndDate, dropStartDate, dropEndDate,
                                        dropGradeRequiredDate, censusDates);

                                    newTerm.AddRegistrationDates(locationRegistrationDates);
                                }
                            }
                        }

                        // Add the term academic levels
                        foreach (var level in term.TermAcadLevels)
                        {
                            newTerm.AddAcademicLevel(level);
                        }
                        // Add the term session cycles
                        foreach (var cycle in term.TermSessionCycles)
                        {
                            if (!string.IsNullOrEmpty(cycle))
                            {
                                newTerm.AddSessionCycle(cycle);
                            }
                            
                        }
                        // Add the term yearly cycles
                        foreach (var cycle in term.TermYearlyCycles)
                        {
                            if (!string.IsNullOrEmpty(cycle))
                            {
                                newTerm.AddYearlyCycle(cycle);
                            }
                        }
                        // Add the session category
                        if (!string.IsNullOrEmpty(term.TermSession))
                        {
                            var session = sessionData.FirstOrDefault(sd => sd.Recordkey == term.TermSession);
                            if (session != null && !string.IsNullOrEmpty(session.SessIntgCategory))
                            {
                                var category = session.SessIntgCategory.ToLower();
                                switch (session.SessIntgCategory)
                                {
                                    case "1":
                                        {
                                            category = "year";
                                            break;
                                        }
                                    case "2":
                                        {
                                            category = "term";
                                            break;
                                        }
                                    case "3":
                                        {
                                            category = "subterm";
                                            break;
                                        }
                                }
                                if (category == "year" || category == "term" || category == "subterm")
                                    newTerm.Category = category;
                            }
                        }
                        if (string.IsNullOrEmpty(newTerm.Category))
                        {
                            if (term.TermReportingTerm == term.Recordkey)
                            {
                                newTerm.Category = "term";
                            }
                            else
                            {
                                newTerm.Category = "subterm";
                            }
                        }
                        newTerm.SessionId = term.TermSession;
                        // Add this term to the list
                        terms.Add(newTerm);
                    }
                    catch (Exception ex)
                    {
                        // If a term cannot be added to the domain log an error but continue loading the rest of the terms.
                        LogDataError("Term", term.Recordkey, term, ex);
                    }
                }
            }
            return terms;
        }

        /// <summary>
        /// Wrapper around Async, used by FinancialAid branch for AcademicProgressService
        /// </summary>
        /// <param name="prog"></param>
        /// <param name="cat"></param>
        /// <returns></returns>
        public IEnumerable<Term> Get()
        {
                var x = Task.Run(async () =>
                {
                    return await GetAsync();
                }).GetAwaiter().GetResult();
                return x;
        }

        /// <summary>
        /// Wrapper around Async, used by FinancialAid branch for AcademicProgressService
        /// </summary>
        /// <param name="prog"></param>
        /// <param name="cat"></param>
        /// <returns></returns>
        public Term Get(string id)
        {
            var x = Task.Run(async () =>
            {
                return await GetAsync(id);
            }).GetAwaiter().GetResult();
            return x;
        }

        /// <summary>
        /// Get guid for AcademicPeriods code
        /// </summary>
        /// <param name="code">AcademicPeriods code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetAcademicPeriodsGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetAsync(false);
            Term codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }
            
            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'TERMS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.RecordGuid))
                    guid = codeNoCache.RecordGuid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'TERMS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.RecordGuid))
                    guid = codeCache.RecordGuid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'TERMS', Record ID:'", code, "'"));
            }
            return guid;

        }



        /// <summary>
        /// Get code for AcademicPeriods guid
        /// </summary>
        /// <param name="code">AcademicPeriods guid</param>
        /// <returns>code</returns>
        public async Task<string> GetAcademicPeriodsCodeFromGuidAsync(string guid)
        {
            //get all the codes from the cache
             string code = string.Empty;
            if (string.IsNullOrEmpty(guid))
                return code;
            var allCodesCache = await GetAsync(false);
            Term codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.RecordGuid.Equals(guid, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No code found, Entity:'TERMS', Record ID:'", guid, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.RecordGuid.Equals(guid, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.RecordGuid))
                    code = codeNoCache.Code;
                else
                    throw new RepositoryException(string.Concat("No code found, Entity:'TERMS', Record ID:'", guid, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Code))
                    code = codeCache.Code;
                else
                    throw new RepositoryException(string.Concat("No code found, Entity:'TERMS', Record ID:'", guid, "'"));
            }
            return code;
        }

        /// <summary>
        /// Using a collection of  ids, get a dictionary collection of associated guids
        /// </summary>
        /// <param name="ids">collection of  ids</param>
        /// <returns>Dictionary consisting of a ids (key) and guids (value)</returns>
        public async Task<Dictionary<string, string>> GetGuidsCollectionAsync( IEnumerable<string> ids )
        {
            if( ( ids == null ) || ( ids != null && !ids.Any() ) )
            {
                return new Dictionary<string, string>();
            }
            var guidCollection = new Dictionary<string, string>();
            try
            {
                var guidLookup = ids
                    .Where( s => !string.IsNullOrWhiteSpace( s ) )
                    .Distinct().ToList()
                    .ConvertAll( p => new RecordKeyLookup( "TERMS", p, false ) ).ToArray();

                var recordKeyLookupResults = await DataReader.SelectAsync( guidLookup );

                if( ( recordKeyLookupResults != null ) && ( recordKeyLookupResults.Any() ) )
                {
                    foreach( var recordKeyLookupResult in recordKeyLookupResults )
                    {
                        if( recordKeyLookupResult.Value != null )
                        {
                            var splitKeys = recordKeyLookupResult.Key.Split( new[] { "+" }, StringSplitOptions.RemoveEmptyEntries );
                            if( !guidCollection.ContainsKey( splitKeys[ 1 ] ) )
                            {
                                guidCollection.Add( splitKeys[ 1 ], recordKeyLookupResult.Value.Guid );
                            }
                        }
                    }
                }
            }
            catch( Exception ex )
            {
                throw new Exception( string.Format( "Error occured while getting guids for {0}.", "TERMS" ), ex ); ;
            }
            return guidCollection;
        }

        /// <summary>
        /// Retrieve all Academic Periods
        /// </summary>
        /// <returns>All Academic Periods</returns>
        public IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicPeriod> GetAcademicPeriods(IEnumerable<Term> terms)
        {
            var academicPeriods = new List<Ellucian.Colleague.Domain.Student.Entities.AcademicPeriod>();
            terms = terms.OrderBy(r => r.ReportingYear).ThenBy(s => s.Sequence);
            var allAcademicYears = terms.Where(t => t.Category == "year").OrderBy(t => t.ReportingYear).ThenBy(t => t.Sequence).ToList();
            var allAcademicTerms = terms.Where(t => t.Category == "term").OrderBy(t => t.ReportingYear).ThenBy(t => t.Sequence).ToList();
            var allAcademicSubterms = terms.Where(t => t.Category == "subterm").OrderBy(t => t.ReportingYear).ThenBy(t => t.Sequence).ToList();

            if (terms != null && terms.Any())
            {
                // First item in the first reporting year will have no preceeding term
                Term preceedingYear = null;
                Term preceedingSubterm = null;
                Term preceedingTerm = null;
                foreach (var period in terms)
                {
                    string parentId = string.Empty;

                    switch (period.Category)
                    {
                        case "year":
                            {
                                var index = allAcademicYears.IndexOf(period);
                                if (index > 0)
                                {
                                    preceedingYear = allAcademicYears.ElementAt(index - 1);
                                }
                                break;
                            }
                        case "term":
                            {
                                var parentYear = allAcademicYears.Where(t => t.ReportingYear == period.ReportingYear).FirstOrDefault();
                                if (parentYear != null)
                                { 
                                    parentId = parentYear.RecordGuid;
                                }
                                var index = allAcademicTerms.IndexOf(period);
                                if (index > 0)
                                {
                                    preceedingTerm = allAcademicTerms.ElementAt(index - 1);
                                }
                                break;
                            }
                        case "subterm":
                            {
                                var parentReportingTerm = terms.Where(t => t.Code == period.ReportingTerm).FirstOrDefault();
                                if (parentReportingTerm == null)
                                {
                                    string errorMessage = "Unable to locate parent reporting term: " + period.ReportingTerm + " for " + period.Code;
                                    logger.Error(errorMessage);
                                    throw new KeyNotFoundException(errorMessage);
                                }
                                else
                                {
                                    parentId = parentReportingTerm.RecordGuid;
                                }
                                var index = allAcademicSubterms.IndexOf(period);
                                if (index > 0)
                                {
                                    preceedingSubterm = allAcademicSubterms.ElementAt(index - 1);
                                }
                                break;
                            }
                    }
                    var preceedingYearID = preceedingYear == null ? null : preceedingYear.RecordGuid;
                    var preceedingSubtermID = preceedingSubterm == null ? null : preceedingSubterm.RecordGuid;
                    var preceedingTermID = preceedingTerm == null ? null : preceedingTerm.RecordGuid;
                    try
                    {
                        var academicPeriod = new Ellucian.Colleague.Domain.Student.Entities.AcademicPeriod(period.RecordGuid,
                            period.Code, period.Description, period.StartDate, period.EndDate, period.ReportingYear, period.Sequence,
                            period.ReportingTerm, period.Category == "year" ? preceedingYearID : (period.Category == "term" ? preceedingTermID : preceedingSubtermID),
                            parentId, period.RegistrationDates)
                        {
                            SessionId = period.SessionId,
                            Category = period.Category
                        };

                        academicPeriods.Add(academicPeriod);
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException(string.Format("Academic Period: '{0}' missing required data.  {1}", period.Code, ex.Message));
                    }
                }
            }

            return academicPeriods;
        }
    }   
}
