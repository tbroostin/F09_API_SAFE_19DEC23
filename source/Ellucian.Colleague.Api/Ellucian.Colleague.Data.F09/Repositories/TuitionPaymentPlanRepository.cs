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
using static System.String;

namespace Ellucian.Colleague.Data.F09.Repositories
{
    [RegisterType]
    public class TuitionPaymentPlanRepository : BaseColleagueRepository, ITuitionPaymentPlanRepository
    {
        private static readonly string GetTuitionFormType = "GetSignUpForm";
        private static readonly string UpdateTuitionFormType = "SubmitSignUpForm";

        private static readonly string GetChangeFormRequestType = "GetChangeForm";
        private static readonly string UpdateChangeFormRequestType = "SubmitChangeForm";

        public TuitionPaymentPlanRepository(ICacheProvider cacheProvider,
            IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
        }

        public async Task<F09PaymentForm> GetChangeTuitionFormAsync(string studentId)
        {
            return await GetPaymentFormBaseAsync(studentId, GetChangeFormRequestType);
        }

        public async Task<string> SubmitChangeTuitionFormAsync(F09TuitionPaymentPlan  paymentPlan)
        {
            var resp = await SubmitFormBaseAsync(paymentPlan, UpdateChangeFormRequestType);
            return resp.RespondType == "Confirmation" ? resp.Msg : resp.RespondType;
        }

        public async Task<F09PaymentInvoice> SubmitTuitionFormAsync(F09TuitionPaymentPlan paymentPlan)
        {
            var resp = await SubmitFormBaseAsync(paymentPlan, UpdateTuitionFormType);

            var invoice = new F09PaymentInvoice()
            {
                PaymentMethod = paymentPlan.PaymentMethod,
                StudentId = paymentPlan.StudentId,
                InvoiceId = resp.SuInvoiceId,
                Distribution = "WEB" // hard code this for now, may want to get the data from the transaction in the future
            };

            return invoice;
        }

        public async Task<F09PaymentForm> GetTuitionFormAsync(string studentId)
        {
            return await GetPaymentFormBaseAsync(studentId, GetTuitionFormType);
        }

        private async Task<F09PaymentForm> GetPaymentFormBaseAsync(string studentId, string requestType)
        {
            if (String.IsNullOrWhiteSpace(studentId))
            {
                throw new ArgumentNullException(nameof(studentId));
            }

            var req = new ctxF09PayPlanSignupRequest()
            {
                Id = studentId,
                RequestType = requestType
            };

            var resp =
                await transactionInvoker.ExecuteAsync<ctxF09PayPlanSignupRequest, ctxF09PayPlanSignupResponse>(req);
            if (!String.IsNullOrWhiteSpace(resp.ErrorMsg) || resp.RespondType == "Error")
            {
                var errMesg = $"Failed to retrieve the tuition payment plan for '{studentId}'. {resp.ErrorMsg}";
                logger.Error(errMesg);

                // it looks like the resp.ErrorMsg is an html encoded string.
                // return that without the fluff that i was adding
                throw new ColleagueTransactionException(resp.ErrorMsg);
            }

            return GetPaymentForm(resp);
        }

        private async Task<ctxF09PayPlanSignupResponse> SubmitFormBaseAsync(F09TuitionPaymentPlan paymentPlan, string requestType)
        {
            if (paymentPlan == null) throw new ArgumentNullException(nameof(paymentPlan));
            if (IsNullOrWhiteSpace(paymentPlan.StudentId))
                throw new ArgumentNullException(nameof(paymentPlan.StudentId), "Student Id is required");

            var submitReq = new ctxF09PayPlanSignupRequest()
            {
                Id = paymentPlan.StudentId,
                RequestType = requestType,
                SuOptionSelected = paymentPlan.PaymentOption,
                PayMethodSelected = paymentPlan.PaymentMethod
            };

            var resp =
                await
                    transactionInvoker.ExecuteAsync<ctxF09PayPlanSignupRequest, ctxF09PayPlanSignupResponse>(submitReq);

            if (!String.IsNullOrWhiteSpace(resp.ErrorMsg) || resp.RespondType == "Error")
            {
                var errMesg =
                    $"Failed to retrieve the payment plan invoice id for '{paymentPlan.StudentId}'. {resp.ErrorMsg}";
                logger.Error(errMesg);

                // it looks like the resp.ErrorMsg is an html encoded string.
                // return that without the fluff that i was adding
                throw new ColleagueTransactionException(resp.ErrorMsg);
            }

            return resp;
        }

        private static F09PaymentForm GetPaymentForm(ctxF09PayPlanSignupResponse response)
        {
            var paymentForm = new F09PaymentForm
            {
                FinancialAidTerms = response.SuFaTerms,
                Instructions = response.SuInstructions,
                TermsConditions = response.SuTermsAndCond,
                PaymentOptions = GetPaymentOptionsDict(response),
                PaymentMethods = GetPaymentMethodsDict(response),
                UnderstandingStatements = GetUnderstandingStatements(response),
                PaymentOptionSelected = response?.SuOptionSelected ?? String.Empty,
            };

            return paymentForm;
        }

        private static List<string> GetUnderstandingStatements(ctxF09PayPlanSignupResponse response)
        {
            var understandingPattern = Regex.Replace(GetReflectionPropertyName(() => response.SuUnderstand1), "[0-9]+",
                "[0-9]+");
            var understandingProperties = GetPropertyInfo(response, opt => Regex.IsMatch(opt.Name, understandingPattern));
            return understandingProperties.Select(u => u.GetValue(response, null) as string).ToList();
        }

        private static Dictionary<string, string> GetPaymentMethodsDict(ctxF09PayPlanSignupResponse response)
            => response.PayMethods.ToDictionary(p => p.PayMethodsCode, p => p.PayMethodsDesc);

        private static Dictionary<string, string> GetPaymentOptionsDict(ctxF09PayPlanSignupResponse response)
        {
            var descriptionPattern = Regex.Replace(GetReflectionPropertyName(()=> response.SuOption1Desc),
                "[0-9]+", "[0-9]+");
            var valuePattern = Regex.Replace(GetReflectionPropertyName(()=>response.SuOption1Value),
                "[0-9]+", "[0-9]+");

            var descriptionOrValue = $"(?:{descriptionPattern}|{valuePattern})";

            var optionParameters = GetPropertyInfo(response,
                opt => Regex.IsMatch(opt.Name, descriptionOrValue),
                x => Regex.Replace(x.Name, @"^[^0-9]+([0-9]+).*(Desc|Value).*$", "$1-$2")); // standardize the name for sorting?

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

        private static string GetReflectionPropertyName(Expression<Func<object>> expr) => ((MemberExpression) expr.Body).Member.Name;

        private static List<PropertyInfo> GetPropertyInfo<T>(T obj, Func<PropertyInfo, bool> where, Func<PropertyInfo, string> orderby = null)
        {
            if (orderby == null) orderby = (t) => t.Name;
            return typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(where)
                .OrderBy(orderby) // standardize the name for sorting?
                .ToList();
        }
    }
}
