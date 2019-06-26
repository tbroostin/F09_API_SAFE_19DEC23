using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.F09.Transactions;
using Ellucian.Colleague.Domain.F09.Entities;
using Ellucian.Colleague.Domain.F09.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;

namespace Ellucian.Colleague.Data.F09.Repositories
{
    [RegisterType]
    public class TuitionPaymentPlanRepository : BaseColleagueRepository, ITuitionPaymentPlanRepository
    {
        private static readonly string GetTuitionFormType = "GetSignUpForm";
        private static readonly string UpdateTuitionFormType = "SubmitSignUpForm";

        public TuitionPaymentPlanRepository(ICacheProvider cacheProvider,
            IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
        }

        public async Task<F09PaymentForm> GetTuitionFormAsync(string studentId)
        {
            if (String.IsNullOrWhiteSpace(studentId))
            {
                throw new ArgumentNullException(nameof(studentId));
            }

            var req = new ctxF09PayPlanSignupRequest()
            {
                Id = studentId,
                RequestType = GetTuitionFormType
            };

            var resp =
                await transactionInvoker.ExecuteAsync<ctxF09PayPlanSignupRequest, ctxF09PayPlanSignupResponse>(req);
            if (!String.IsNullOrWhiteSpace(resp.ErrorMsg))
            {
                var errMesg = $"Failed to retrieve the tuition payment plan for '{studentId}'. {resp.ErrorMsg}";
                logger.Error(errMesg);

                // it looks like the resp.ErrorMsg is an html encoded string.
                // return that without the fluff that i was adding
                throw new ColleagueTransactionException(resp.ErrorMsg);
            }

            return GetPaymentForm(resp);
        }

        private F09PaymentForm GetPaymentForm(ctxF09PayPlanSignupResponse response)
        {
            var paymentForm = new F09PaymentForm
            {
                FinancialAidTerms = response.SuFaTerms,
                Instructions = response.SuInstructions,
                TermsConditions = response.SuTermsAndCond,
                PaymentOptions = GetPaymentOptionsDict(response)
            };

            return paymentForm;
        }

        private Dictionary<string, string> GetPaymentOptionsDict(ctxF09PayPlanSignupResponse response)
        {
            Expression<Func<object>> optionDescriptionExpression = () => response.SuOption1Desc;
            Expression<Func<object>> optionValueExpression = () => response.SuOption1Value;

            var descriptionPattern = Regex.Replace(((MemberExpression) optionDescriptionExpression.Body).Member.Name,
                "[0-9]+", "[0-9]+");
            var valuePattern = Regex.Replace(((MemberExpression) optionValueExpression.Body).Member.Name,
                "[0-9]+", "[0-9]+");

            var descriptionOrValue = $"(?:{descriptionPattern}|{valuePattern})";

            var optionParameters = response.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(opt => Regex.IsMatch(opt.Name, descriptionOrValue))
                .OrderBy(x => Regex.Replace(x.Name, @"^[^0-9]+([0-9]+).*(Desc|Value).*$", "$1-$2")) // standardize the name for sorting?
                .ToList();

            var tracker = new HashSet<string>();
            var pairwise =
                optionParameters.Zip(optionParameters.Skip(1),
                    (desc, value) =>
                    {
                        // this might not be the correct way to do this...
                        // track which values we've used, otherwise zip will reuse them
                        // zip does (a,b,c,d) => (a,b), (b,c), (c,d)
                        // when we really want (a,b,c,d) => (a,b), (c,d)

                        // and really at this point this whole thing might be overly complicated
                        // i (roger) tried to make it future proof if ever there are other options to add later
                        // because I know I'll forget. this will take care of that, but right now
                        // there are only 2 options so this is overkill
                        if (tracker.Contains(desc.Name) || tracker.Contains(value.Name)) return null;

                        tracker.Add(desc.Name);
                        tracker.Add(value.Name);
                        return Tuple.Create(value.GetValue(response, null), desc.GetValue(response, null));
                    }).Where(x => x != null)
                    .ToDictionary(option => option.Item1.ToString(), option => option.Item2.ToString());

            return pairwise;
        }
    }
}
