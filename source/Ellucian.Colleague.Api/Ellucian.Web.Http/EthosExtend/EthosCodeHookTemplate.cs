﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using slf4net;
using Ellucian.Colleague.Configuration;
using Ellucian.Colleague.Domain;
using Ellucian.Data.Colleague;
using Ellucian.Dmi.Client;
using Ellucian.Dmi.Runtime;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Web.Cache;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Ellucian.Web.Utility;
using Ellucian.Web.Http.Configuration;
namespace Ellucian.Colleague.Domain.Base.Services
{
    public class CSCodeEvaler
    {
        char _VM = Convert.ToChar(DynamicArray.VM);
        char _SM = Convert.ToChar(DynamicArray.SM);
        char _TM = Convert.ToChar(DynamicArray.TM);
        char _XM = Convert.ToChar(250);
        public async Task<CodeBuilderObject> EvalCode(CodeBuilderObject inputData, IColleagueTransactionInvoker transactionInvoker, IColleagueDataReader dataReader, BaseCachingRepository baseCachingRepository, Func<string, Func<object>, double?, object> GetOrAddToCacheFunc, double? cacheTimeOut, bool bypassCache)
        {
            var outputData = new CodeBuilderObject();
            try
            {
                // All code hooks start here.  When writing code, you have access to the data reader and the transaction invoker.
                // If the code works here, in the template then you can cut and paste into the code hook form and execute your API
                // to validate and test.
                var limitingKeys = dataReader.Select("PERSON.HEALTH", inputData.LimitingKeys, "");
                outputData.LimitingKeys = dataReader.Select("PERSON", limitingKeys, "WITH PERSON.CORP.INDICATOR NE 'Y'");
                outputData.SelectEntity = inputData.SelectEntity;
                // This is the end of the code hook.
                var vlPersonArIds = new List<string>();
                if (inputData != null && inputData.DataDictionary != null && inputData.DataDictionary.Any())
                {
                    if (inputData.DataDictionary.TryGetValue("PERSON.AR.ID", out vlPersonArIds))
                    {
                        foreach (var vPersonArId in vlPersonArIds)
                        {
                            var parCacheKey = "PERSON.AR+" + vPersonArId;
                            var personArData = GetOrAddToCacheFunc(parCacheKey,
                               () =>
                               {
                                   return dataReader.ReadRecordColumns("PERSON.AR", vPersonArId, new string[] { "PAR.AR.TYPES" });
                               }
                            , cacheTimeOut) as Dictionary<string, string>;
                            var arTypes = string.Empty;
                            if (personArData != null && personArData.TryGetValue("PAR.AR.TYPES", out arTypes))
                            {
                                var vlArTypes = arTypes.Split(_VM);
                                foreach (var vArType in vlArTypes)
                                {
                                    var vArAcctsId = string.Concat(vPersonArId, "*", vArType);
                                    var arCacheKey = "AR.ACCTS+" + vArAcctsId;
                                    var arAcctsData = GetOrAddToCacheFunc(arCacheKey,
                                       () =>
                                       {
                                           return dataReader.ReadRecordColumns("AR.ACCTS", vArAcctsId, new string[] { "ARA.PAYMENTS" });
                                       }
                                    , cacheTimeOut) as Dictionary<string, string>;
                                    if (arAcctsData != null && arAcctsData.Any())
                                    {
                                        var arPaymentIds = string.Empty;
                                        if (arAcctsData.TryGetValue("ARA.PAYMENTS", out arPaymentIds))
                                        {
                                            var vlPaymentIds = arPaymentIds.Split(_VM);
                                            foreach (var paymentId in vlPaymentIds)
                                            {
                                                if (!string.IsNullOrEmpty(paymentId))
                                                {
                                                    var arpayCacheKey = "AR.PAYMENTS+" + paymentId;
                                                    var arPaymentsData = GetOrAddToCacheFunc(arpayCacheKey,
                                                       () =>
                                                       {
                                                           return dataReader.ReadRecordColumns("AR.PAYMENTS", paymentId, new string[] { "ARP.AR.TYPE", "ARP.AMT", "ARP.DATE", "ARP.TERM" });
                                                       }
                                                    , cacheTimeOut) as Dictionary<string, string>;
                                                    var type = string.Empty;
                                                    if (arPaymentsData.TryGetValue("ARP.AR.TYPE", out type))
                                                    {
                                                        if (!string.IsNullOrEmpty(type))
                                                        {
                                                            var artCacheKey = "AR.TYPES+" + type;
                                                            var arTypesData = GetOrAddToCacheFunc(artCacheKey,
                                                               () =>
                                                               {
                                                                   return dataReader.ReadRecordColumns("AR.TYPES", type, new string[] { "ART.DESC" });
                                                               }
                                                            , cacheTimeOut) as Dictionary<string, string>;
                                                            var typeDesc = string.Empty;
                                                            if (arTypesData.TryGetValue("ART.DESC", out typeDesc))
                                                            {
                                                                outputData.DataValues.Add(typeDesc);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                // This hook gets the list of extended address types and returns a list of categories "home".
                var vlAddressTypes = new List<string>();
                if (inputData != null && inputData.DataDictionary != null && inputData.DataDictionary.Any())
                {
                    if (inputData.DataDictionary.TryGetValue("ADDR.TYPE", out vlAddressTypes))
                    {
                        foreach (var typeId in vlAddressTypes)
                        {
                            var category = string.Empty;
                            if (!string.IsNullOrEmpty(typeId))
                            {
                                var type = typeId.Split(_SM)[0];
                                if (!string.IsNullOrEmpty(type))
                                {
                                    var ldmGuidColumns = dataReader.ReadRecordColumns("LDM.GUID", type, new string[] { "LDM.GUID.SECONDARY.KEY" });
                                    if (ldmGuidColumns != null && ldmGuidColumns.Any())
                                    {
                                        var addrType = string.Empty;
                                        ldmGuidColumns.TryGetValue("LDM.GUID.SECONDARY.KEY", out addrType);
                                        switch (addrType)
                                        {
                                            case "H":
                                                category = "home";
                                                break;
                                            case "B":
                                                category = "business";
                                                break;
                                            default:
                                                category = "other";
                                                break;
                                        }
                                    }
                                }
                            }
                            outputData.DataValues.Add(category);
                        }
                    }
                }
                // This is the end of the code hook.
            }
            catch (Exception ex)
            {
                outputData.ErrorFlag = true;
                outputData.ErrorMessages.Add(ex.Message);
            }
            return outputData;
        }
    }
}
