/*Copyright 2015-2021 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// The shopping sheet rule table identifies collections of rules that Financial Aid Offices
    /// use to determine a set of student-specific customized messages that can be printed
    /// on the shopping sheet.
    /// Each rule is assigned a text result <see cref="ruleResultPairs"/>. If a rule passes, it's result
    /// is included in the list of text strings in the <see cref="GetRuleTableResult"/> method.
    /// </summary>
    [Serializable]
    public class ShoppingSheetRuleTable
    {
        /// <summary>
        /// The code identifying this rule table
        /// </summary>
        public string Code { get { return code; } }
        private readonly string code;

        /// <summary>
        /// The award year this rule table belongs to. This table's rules will be executed on a record
        /// using this award year as the file suite year.
        /// </summary>
        public string AwardYear { get { return awardYear; } }
        private string awardYear;

        /// <summary>
        /// A short description of the rule table
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// This is the default result of the rule table. If no rules pass, this is the only string result from
        /// <see cref="GetRuleTableResult"/>. If any rule passes, the default result is not included in the list of results.
        /// However, if the <see cref="AlwaysUseDefault"/> flag is true, the default will always be the first string result
        /// in the list of results, regardless of whether any rules pass or not.
        /// This is a required attribute and cannot be set to null, empty or blank.
        /// </summary>        
        public string DefaultResult
        {
            get { return defaultResult; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    defaultResult = value;
                }
            }
        }
        private string defaultResult;

        /// <summary>
        /// The subroutine name for a custom text definition if one is provided
        /// </summary>
        public string RtSubrName { get; set; }

        /// <summary>
        /// The resulting verbiage from a custom subroutine
        /// </summary>
        public string RtCustomVerbiage { get; set; }

        /// <summary>
        /// Flag indicating whether the rule table will always to return the DefaultResult string in its list of results.
        /// </summary>
        public bool AlwaysUseDefault { get; set; }

        /// <summary>
        /// List of KeyValuePairs where the Key is the Rule Id, and the Value the result of the rule table if the rule passes.
        /// The keys in this list (the rule Ids) are ultimately what drive the execution of this rule table.
        /// Duplicate keys are allowed.
        /// </summary>        
        public ReadOnlyCollection<KeyValuePair<string, string>> RuleResultPairs { get; private set; }
        private List<KeyValuePair<string, string>> ruleResultPairs;


        /// <summary>
        /// Get a list of RuleIds from the RuleResultPairs.
        /// </summary>
        public List<string> RuleIds
        {
            get
            {
                return (ruleResultPairs != null) ? ruleResultPairs.Select(pair => pair.Key).ToList() : new List<string>();
            }
        }

        /// <summary>
        /// Add a Rule-Result Pair to this rule table. The ruleId is required. The result 
        /// can be empty.
        /// </summary>
        /// <param name="ruleId">Id of the rule to add to the rule table</param>
        /// <param name="result">The result of the rule table if this rule passes.</param>
        /// <exception cref="ArgumentNullException">Thrown if ruleId is null or empty</exception>
        public void AddRuleResultPair(string ruleId, string result)
        {
            if (string.IsNullOrEmpty(ruleId))
            {
                throw new ArgumentNullException("ruleId");
            }
            ruleResultPairs.Add(new KeyValuePair<string, string>(ruleId, result));
        }

        /// <summary>
        /// This rule table class uses the RuleObjects Dictionary to create RuleRequests with Rules that have expressions.
        /// </summary>
        private Dictionary<string, Rule<StudentAwardYear>> RuleObjects;

        /// <summary>
        /// Use this method to link Rule objects to this rule table. It is recommended that you link
        /// Rule objects with expressions so they can be evaluated in .NET. You can also link rules that don't
        /// have expressions, they just won't be evaluated in .NET
        /// This method will only link Rules of type <typeparamref name="StudentAwardYear"/> and Rules that are identified in the
        /// <see cref="RuleResultPairs"/> collection
        /// </summary>
        /// <param name="rules">A list of rules to potentially link to this table. Only rules of type <typeparamref name="StudentAwardYear"/>
        /// and rules with Ids in <see cref="RuleResultPairs"/> are linked to this table.</param>
        public void LinkRuleObjects(IEnumerable<Rule> rules)
        {
            if (rules != null)
            {
                var tableContextRules = rules.OfType<Rule<StudentAwardYear>>();
                var ruleResultPairIds = ruleResultPairs.Select(p => p.Key).ToList();
                foreach (var ruleToAdd in tableContextRules)
                {
                    if (ruleResultPairIds.Contains(ruleToAdd.Id) && !RuleObjects.ContainsKey(ruleToAdd.Id))
                    {
                        RuleObjects.Add(ruleToAdd.Id, ruleToAdd);
                    }
                }
            }
        }

        /// <summary>
        /// Use this method to link a single rule object to this rule table. It is recommended that you link
        /// Rule objects to this table for a better chance that the rules will be evaluated in .NET thereby improving
        /// latency performance. It's not necessary to link Rule objects to this table,
        /// but the Rule Table will definitely be evaluated with Colleague's Envision rule processor. 
        /// </summary>
        /// <param name="rule">A list of rules to potentially link to this table. Only rules of type <typeparamref name="StudentAwardYear"/>
        /// and rules with Ids in <see cref="ruleResultPairs"/> are linked to this table.</param>
        public void LinkRuleObjects(Rule rule)
        {
            LinkRuleObjects(new List<Rule>() { rule });
        }


        /// <summary>      
        /// Set the RuleProcessor to a function that evaluates input RuleRequests and returns RuleResults.
        /// If this function is not set, only the <see cref="GetRuleTableResult(IEnumerable<RuleResult> ruleResults)"/>
        /// method will evaluate the results because the caller provides the RuleResult objects. All the other
        /// <seealso cref="GetRuleTableResult"/> methods will throw exceptions if the RuleProcessor is null;
        /// </summary>
        public Func<IEnumerable<RuleRequest<StudentAwardYear>>, Task<IEnumerable<RuleResult>>> RuleProcessor
        {
            private get;
            set;
        }

        /// <summary>
        /// This method creates a set of rule requests based on the <see cref="ruleResultPairs"/>, the
        /// Rule objects linked to this rule table, and the given <typeparamref name="StudentAwardYear"/> context object
        /// </summary>
        /// <param name="ruleTableContext">The <typeparamref name="StudentAwardYear"/> object for which to create ruleRequests.</param>
        /// <returns>A list of RuleRequests for the given context object based on the rules of this rule table</returns>
        public IEnumerable<RuleRequest<StudentAwardYear>> CreateRuleRequests(StudentAwardYear ruleTableContext)
        {
            if (ruleTableContext == null)
            {
                throw new ArgumentNullException("ruleTableContext");
            }
            return ruleResultPairs.Select(pair =>
                new RuleRequest<StudentAwardYear>(
                    (RuleObjects.ContainsKey(pair.Key)) ? RuleObjects[pair.Key] : new Rule<StudentAwardYear>(pair.Key),
                    ruleTableContext));
        }

        /// <summary>
        /// This method processes the given list of <typeparamref name="RuleResult"/> objects, and returns
        /// the result of this rule table based on the values in the <see cref="ruleResultPairs"/> list. If no rules
        /// pass, or if <see cref="AlwaysUseDefault"/> is true, the DefaultResult string will be returned in the list. 
        /// </summary>
        /// <param name="ruleResults">The RuleResult objects to evaluate.</param>
        /// <returns>A list of text strings from all the passed rules from the RuleResultPairs list.</returns>
        public IEnumerable<string> GetRuleTableResult(IEnumerable<RuleResult> ruleResults)
        {
            if (ruleResults == null)
            {
                throw new ArgumentNullException("ruleResults");
            }

            var ruleTableResultList = new List<string>();
            if (AlwaysUseDefault || ruleResults.All(rr => !rr.Passed))
            {
                ruleTableResultList.Add(DefaultResult);
            }

            foreach (var ruleResultPair in ruleResultPairs)
            {
                var passedResult = ruleResults.FirstOrDefault(rr => rr.RuleId == ruleResultPair.Key && rr.Passed);
                if (passedResult != null)
                {
                    ruleTableResultList.Add(ruleResultPair.Value);
                }
            }

            return ruleTableResultList;
        }

        /// <summary>
        /// This method evaluates the rule table against the given ruleTableContext using the RuleProcessor function.
        /// You must set the RuleProcessor function to obtain a rule table result.
        /// </summary>
        /// <param name="ruleTableContext">A <typeparamref name="StudentAwardYear"/> object against which to evaluate this rule table.</param>        
        /// <returns>A list of text strings from all the passed rules from the RuleResultPairs list.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the RuleProcessor function is null</exception>
        public async Task<IEnumerable<string>> GetRuleTableResultAsync(StudentAwardYear ruleTableContext)
        {
            if (ruleTableContext == null)
            {
                throw new ArgumentNullException("ruleTableContext");
            }
            if (RuleProcessor == null)
            {
                throw new InvalidOperationException("RuleProcessor is null");
            }
            var ruleRequests = CreateRuleRequests(ruleTableContext);
            var ruleResults = await Task.Run(() => RuleProcessor.Invoke(ruleRequests));

            return GetRuleTableResult(ruleResults);
        }

        /// <summary>
        /// Create a new ShoppingSheetRuleTable. You should link Rules to this table and 
        /// set the RuleProcessor to a function that can evaluate rules with and without .NET expressions
        /// </summary>
        /// <param name="code">Required: The Code identifier of this rule table. Part 1 of unique identifier</param>
        /// <param name="awardYear">Required: The award year this rule table operates on. Part 2 of unique identifier</param>
        /// <param name="defaultResult">Required: The default result of this rule table.</param>
        public ShoppingSheetRuleTable(string code, string awardYear, string defaultResult)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }
            if (defaultResult == null)
            {
                defaultResult = " ";
            }

            this.code = code;
            this.awardYear = awardYear;
            this.defaultResult = defaultResult;
            ruleResultPairs = new List<KeyValuePair<string, string>>();
            RuleResultPairs = ruleResultPairs.AsReadOnly();
            RuleObjects = new Dictionary<string, Rule<StudentAwardYear>>();
        }

        /// <summary>
        /// Two ShoppingSheetRuleTable objects are equal when their Codes and Award Years are equal
        /// </summary>
        /// <param name="obj">The ShoppingSheetRuleTable object equal to this one</param>
        /// <returns>True, if this Code and AwardYear matches the given ShoppingSheetRuleTable's code and award year. False, otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var shoppingSheetRuleTable = obj as ShoppingSheetRuleTable;

            if (shoppingSheetRuleTable.Code == this.Code &&
                shoppingSheetRuleTable.AwardYear == this.AwardYear)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Computes a HashCode based on the Code and AwardYear of this ShoppingSheetRuleTable
        /// </summary>
        /// <returns>A HashCode representing this rule table</returns>
        public override int GetHashCode()
        {
            return Code.GetHashCode() ^ AwardYear.GetHashCode();
        }

        /// <summary>
        /// Returns a string representation of this object based on the award year and code
        /// </summary>
        /// <returns>A string representation of this object</returns>
        public override string ToString()
        {
            return string.Format("{0}*{1}", AwardYear, Code);
        }
    }
}
