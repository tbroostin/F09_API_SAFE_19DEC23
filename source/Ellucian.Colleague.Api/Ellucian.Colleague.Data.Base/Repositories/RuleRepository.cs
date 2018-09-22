// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Reads rules from Colleague and adapts them into expression trees where possible.
    /// </summary>
    [RegisterType]
    public class RuleRepository : BaseColleagueRepository, IRuleRepository
    {
        private readonly RuleAdapterRegistry ruleAdapterRegistry;
        private readonly RuleConfiguration configuration;
        private RuleConversionOptions conversionOptions;
        const int YEAR_CONVERSION_THRESHHOLD = 68; // Threshhold year for determining whether two digit year is 20xx or 19xx 

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleRepository"/> class.
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="transactionFactory">The transaction factory.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="ruleAdapterRegistry">The rule adapter registry.</param>
        /// <param name="configuration">The configuration.</param>
        /// <exception cref="System.ArgumentNullException">
        /// ruleAdapterRegistry
        /// or
        /// configuration
        /// </exception>
        public RuleRepository(ICacheProvider cacheProvider,
            IColleagueTransactionFactory transactionFactory,
            ILogger logger, RuleAdapterRegistry ruleAdapterRegistry, RuleConfiguration configuration)
            : base(cacheProvider, transactionFactory, logger)
        {
            if (ruleAdapterRegistry == null)
            {
                throw new ArgumentNullException("ruleAdapterRegistry");
            }
            this.ruleAdapterRegistry = ruleAdapterRegistry;
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }
            this.configuration = configuration;
        }


        /// <summary>
        /// Retrieves rules by ID.
        /// </summary>
        /// <param name="ruleIds"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Rule>> GetManyAsync(IEnumerable<string> ruleIds)
        {
            var results = new List<Rule>();
            if (ruleIds == null)
            {
                return results;
            }


            var ruleRecords = await DataReader.BulkReadRecordAsync<Rules>("RULES", ruleIds.Distinct().ToArray());
            foreach (var ruleRecord in ruleRecords)
            {
                var rd = new RuleDescriptor();
                rd.Id = ruleRecord.Recordkey;
                rd.PrimaryView = ruleRecord.RlPrimaryView;
                rd.RuleConversionOptions = await GetRuleConversionOptionsAsync();

                var message = Supported(ruleRecord);
                if (message != null)
                {
                    rd.NotSupportedMessage = message;
                }

                foreach (var check in ruleRecord.RulesCheckEntityAssociation)
                {
                    var red = new RuleExpressionDescriptor();
                    red.Connector = check.RlCheckConnectorAssocMember;
                    red.DataElement = new RuleDataElement() { Id = check.RlCheckDataElementsAssocMember };
                    red.Operator = check.RlCheckOperatorsAssocMember;
                    red.Literal = check.RlCheckValuesAssocMember;

                    rd.Expressions.Add(red);
                }


                var adapter = ruleAdapterRegistry.Get(rd.PrimaryView);
                if (adapter != null)
                {
                    var dataElements = rd.Expressions.Select(re => re.DataElement);
                    await AddDataElementMetadataAsync(dataElements);
                }

                // Try to squeeze an expression tree out of the rule
                if (adapter == null)
                {
                    results.Add(new Rule<UnknownRuleContext>(rd.Id));
                }
                else
                {
                    // Adapter may or may not succeed at creating an expression tree
                    var rule = adapter.Create(rd);
                    if (!string.IsNullOrEmpty(rule.NotSupportedMessage))
                    {
                        logger.Debug("Rule " + ruleRecord.Recordkey + " won't be processed in .NET: " + rule.NotSupportedMessage);
                    }
                    results.Add(rule);
                }
            }

            return results;
        }

        private static Dictionary<string, RtFields> metadata = new Dictionary<string, RtFields>();

        private async Task AddDataElementMetadataAsync(IEnumerable<RuleDataElement> dataElements)
        {
            foreach (var dataElement in dataElements)
            {
                RtFields record = null;
                if (metadata.ContainsKey(dataElement.Id))
                {
                    record = metadata[dataElement.Id];
                }
                else
                {
                    record = await ReadAndCacheMetadataAsync(dataElement.Id);
                }
                if (record != null && record.RtfldsVirtualFieldDef != null && record.RtfldsVirtualFieldDef.Count == 1)
                {
                    var vfd = record.RtfldsVirtualFieldDef.ElementAt(0);
                    if (!string.IsNullOrEmpty(vfd))
                    {
                        dataElement.ComputedFieldDefinition = vfd;
                    }
                }
            }
        }

        private async Task<RtFields> ReadAndCacheMetadataAsync(string dataElementId)
        {
            var record = await DataReader.ReadRecordAsync<RtFields>("RT.FIELDS", dataElementId);
            if (record != null)
            {
                metadata[dataElementId] = record;
            }
            return record;
        }

        /// <summary>
        /// Scans the rules record for any specs that we don't (yet) support on the .NET side.
        /// </summary>
        /// <param name="ruleRecord"></param>
        /// <returns></returns>
        private static string Supported(Rules ruleRecord)
        {
            if (!string.IsNullOrEmpty(ruleRecord.RlSubroutineName))
            {
                return "Uses a custom subroutine " + ruleRecord.RlSubroutineName;
            }
            // Can probably support this one later
            if (ruleRecord.RlCheckDataUsing != null)
            {
                // Make sure it's not just a list of empty strings (these things happen)
                foreach (var cdu in ruleRecord.RlCheckDataUsing)
                {
                    if (!string.IsNullOrEmpty(cdu))
                    {
                        return "Has a Check Data Using clause of " + cdu;
                    }
                }
            }

            if (ruleRecord.RlCheckDataWhen != null && ruleRecord.RlCheckDataWhen.Count > 0)
            {
                foreach (var cdw in ruleRecord.RlCheckDataWhen)
                {
                    if (!string.IsNullOrEmpty(cdw))
                    {
                        return "Has a Check Data When clause of " + cdw;
                    }
                }
            }
            if (ruleRecord.RlCheckValueWhen != null && ruleRecord.RlCheckValueWhen.Count > 0)
            {
                foreach (var cvw in ruleRecord.RlCheckValueWhen)
                {
                    if (!string.IsNullOrEmpty(cvw))
                    {
                        return "Has a Check Value When clause of " + cvw;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Retrieves a single rule
        /// </summary>
        /// <param name="ruleId"></param>
        /// <returns></returns>
        public async Task<Rule> GetAsync(string ruleId)
        {
            return (await GetManyAsync(new List<string>() { ruleId })).First();
        }

        /// <summary>
        /// Executes the specified rule requests.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ruleRequests">The rule requests.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">No rule adapter found for context type  + typeof(T)</exception>
        public async Task<IEnumerable<RuleResult>> ExecuteAsync<T>(IEnumerable<RuleRequest<T>> ruleRequests)
        {
            var results = new List<RuleResult>();

            ruleRequests = ruleRequests.Distinct();
            logger.Debug("Rule requests total: " + ruleRequests.Count());

            // Return the result for every rule that can be evaluated in .NET (result already calculated)
            var dotNetRequests = ruleRequests.Where(req => req.Rule.HasExpression);
            foreach (var ruleReq in dotNetRequests)
            {
                var result = new RuleResult()
                {
                    RuleId = ruleReq.Rule.Id,
                    Context = ruleReq.Context,
                    Passed = ruleReq.Rule.Passes(ruleReq.Context)
                };
                results.Add(result);
            }

            // Execute any rules that cannot be handled in .NET
            if (!configuration.ExecuteAllRulesInColleague)
            {
                ruleRequests = ruleRequests.Where(request => !request.Rule.HasExpression);
            }
            logger.Debug("Rule requests being sent to Colleague: " + ruleRequests.Count());

            var adapter = ruleAdapterRegistry.Get(typeof(T));
            if (adapter == null)
            {
                throw new Exception("No rule adapter found for context type " + typeof(T));
            }

            //group rules by file suite
            var groupedByFileSuite = ruleRequests.GroupBy(rr => adapter.GetFileSuiteInstance(rr.Context));
            foreach (var fileSuiteRequestGroup in groupedByFileSuite)
            {
                var groupByRuleId = fileSuiteRequestGroup.GroupBy(rg => rg.Rule);
                foreach (var requestGroup in groupByRuleId)
                {
                    //Evaluate 1 rule x n records
                    var ctx = new ExecuteRulesRequest();
                    ctx.ARuleId = requestGroup.Key.Id;
                    ctx.AlRecordIds = requestGroup.Select(rr => adapter.GetRecordId(rr.Context)).ToList();
                    ctx.AFileSuiteInstance = fileSuiteRequestGroup.Key;
                    var response = await transactionInvoker.ExecuteAsync<ExecuteRulesRequest, ExecuteRulesResponse>(ctx);

                    for (int i = 0; i < response.Results.Count; i++)
                    {
                        var result = new RuleResult();
                        result.RuleId = requestGroup.Key.Id;
                        result.Passed = response.Results[i];
                        result.Context = requestGroup.ElementAt(i).Context;
                        results.Add(result);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Executes the specified rule requests.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ruleRequests">The rule requests.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">No rule adapter found for context type  + typeof(T)</exception>
        [Obsolete("Replaced by ExecuteAsync<T>")]
        public IEnumerable<RuleResult> Execute<T>(IEnumerable<RuleRequest<T>> ruleRequests)
        {
            var results = new List<RuleResult>();

            ruleRequests = ruleRequests.Distinct();
            logger.Debug("Rule requests total: " + ruleRequests.Count());

            // Return the result for every rule that can be evaluated in .NET (result already calculated)
            var dotNetRequests = ruleRequests.Where(req => req.Rule.HasExpression);
            foreach (var ruleReq in dotNetRequests)
            {
                var result = new RuleResult()
                {
                    RuleId = ruleReq.Rule.Id,
                    Context = ruleReq.Context,
                    Passed = ruleReq.Rule.Passes(ruleReq.Context)
                };
                results.Add(result);
            }

            // Execute any rules that cannot be handled in .NET
            if (!configuration.ExecuteAllRulesInColleague)
            {
                ruleRequests = ruleRequests.Where(request => !request.Rule.HasExpression);
            }
            logger.Debug("Rule requests being sent to Colleague: " + ruleRequests.Count());

            var adapter = ruleAdapterRegistry.Get(typeof(T));
            if (adapter == null)
            {
                throw new Exception("No rule adapter found for context type " + typeof(T));
            }


            //group rules by file suite
            var groupedByFileSuite = ruleRequests.GroupBy(rr => adapter.GetFileSuiteInstance(rr.Context));
            foreach (var fileSuiteRequestGroup in groupedByFileSuite)
            {
                var groupByRuleId = fileSuiteRequestGroup.GroupBy(rg => rg.Rule);
                foreach (var requestGroup in groupByRuleId)
                {
                    //Evaluate 1 rule x n records
                    var ctx = new ExecuteRulesRequest();
                    ctx.ARuleId = requestGroup.Key.Id;
                    ctx.AlRecordIds = requestGroup.Select(rr => adapter.GetRecordId(rr.Context)).ToList();
                    ctx.AFileSuiteInstance = fileSuiteRequestGroup.Key;
                    var response = transactionInvoker.Execute<ExecuteRulesRequest, ExecuteRulesResponse>(ctx);

                    for (int i = 0; i < response.Results.Count; i++)
                    {
                        var result = new RuleResult();
                        result.RuleId = requestGroup.Key.Id;
                        result.Passed = response.Results[i];
                        result.Context = requestGroup.ElementAt(i).Context;
                        results.Add(result);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Dumps the specified rule.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <returns></returns>
        public static string Dump(Rules rule)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("Rule ID: " + rule.Recordkey);
            sb.AppendLine("Primary View: " + rule.RlPrimaryView);
            sb.AppendLine();
            foreach (var line in rule.RulesCheckEntityAssociation)
            {
                sb.Append('\t').Append(line.RlCheckConnectorAssocMember).Append(" ").Append(line.RlCheckDataElementsAssocMember).Append(" ").Append(line.RlCheckOperatorsAssocMember).Append(" ").Append(line.RlCheckValuesAssocMember).AppendLine();
            }
            sb.AppendLine();
            return sb.ToString();
        }

        /// <summary>
        /// Creates the RuleConversionOptions object that is attached to the rule descriptor and is used in certain cases to convert
        /// database-specific data formatting for accurate rule evaluation. Originally created for accurate conversion of user-formatted
        /// dates to a comparable .NET DateTime object.
        /// </summary>
        /// <returns></returns>
        private async Task<RuleConversionOptions> GetRuleConversionOptionsAsync()
        {
            if (conversionOptions != null)
            {
                return conversionOptions;
            }
            // Overriding cache timeout to be Level1 Cache time out for data that rarely changes.
            var intlParameters = await GetOrAddToCacheAsync<Ellucian.Colleague.Data.Base.DataContracts.IntlParams>("InternationalParameters",
                async () =>
                {
                    Data.Base.DataContracts.IntlParams intlParams = await DataReader.ReadRecordAsync<Data.Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL");
                    if (intlParams == null)
                    {
                        var errorMessage = "Unable to access international parameters INTL.PARAMS INTERNATIONAL.";
                        logger.Info(errorMessage);
                        // If we cannot read the international parameters default to US with a / delimiter.
                        // throw new Exception(errorMessage);
                        Data.Base.DataContracts.IntlParams newIntlParams = new Data.Base.DataContracts.IntlParams();
                        newIntlParams.HostShortDateFormat = "MDY";
                        newIntlParams.HostDateDelimiter = "/";
                        intlParams = newIntlParams;
                    }
                    return intlParams;
                }, Level1CacheTimeoutValue);

            conversionOptions = new RuleConversionOptions()
            {
                DateDelimiter = intlParameters.HostDateDelimiter,
                DateFormat = intlParameters.HostShortDateFormat,
                CenturyThreshhold = YEAR_CONVERSION_THRESHHOLD
            };

            return conversionOptions;
        }
    }
}
