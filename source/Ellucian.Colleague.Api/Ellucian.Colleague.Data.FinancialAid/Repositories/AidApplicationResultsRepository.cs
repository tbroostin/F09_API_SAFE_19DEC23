/*Copyright 2023 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class AidApplicationResultsRepository : BaseColleagueRepository, IAidApplicationResultsRepository, IEthosExtended
    {
        protected const int AllAidApplicationResultsFilterTimeout = 20; // Clear from cache every 20 minutes
        protected const string AllAidApplicationResultsFilterCache = "AllAidApplicationResultsFilter";

        public AidApplicationResultsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        public async Task<AidApplicationResults> GetAidApplicationResultsByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "aid-application-results ID is required for record retrieval.");
            }

            FaappCalcResults faappCalcResultsDataContract = await DataReader.ReadRecordAsync<FaappCalcResults>(id);
            if (faappCalcResultsDataContract == null)
            {
                throw new KeyNotFoundException("No aid-application-results record was found for ID: " + id);
            }
            FaappDemo faappDemo = await DataReader.ReadRecordAsync<FaappDemo>(id);
            if (faappDemo == null)
            {
                throw new KeyNotFoundException("No aid-application-demographics record was found for ID: " + id);
            }
            return ConvertToAidApplicationResults(faappCalcResultsDataContract, faappDemo);
        }

        public async Task<Tuple<IEnumerable<AidApplicationResults>, int>> GetAidApplicationResultsAsync(int offset, int limit, string appDemoId = "", string personId = "",
            string aidApplicationType = "", string aidYear = "", int? transactionNumber = null, string applicantAssignedId = "")
        {
            try
            {
                int totalCount = 0;
                string[] subList = null;
                var repositoryException = new RepositoryException();
                string aidApplicationResultsCacheKey = CacheSupport.BuildCacheKey(AllAidApplicationResultsFilterCache, appDemoId, personId, aidApplicationType, aidYear, transactionNumber,applicantAssignedId);
                var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
                       this,
                       ContainsKey,
                       GetOrAddToCacheAsync,
                       AddOrUpdateCacheAsync,
                       transactionInvoker,
                       aidApplicationResultsCacheKey,
                       "FAAPP.CALC.RESULTS",
                       offset,
                       limit,
                       AllAidApplicationResultsFilterTimeout,
                       () =>
                       {
                           return GetAidApplicationResultsFilterCriteria(appDemoId, personId, aidApplicationType, aidYear, transactionNumber, applicantAssignedId);
                       }
                   );

                if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
                {
                    return new Tuple<IEnumerable<AidApplicationResults>, int>(new List<AidApplicationResults>(), 0);
                }

                subList = keyCache.Sublist.ToArray();
                totalCount = keyCache.TotalCount.Value;
                var exception = new RepositoryException();
                if (subList != null && subList.Any())
                {
                    var faappCalcResultsDataContracts = await DataReader.BulkReadRecordAsync<FaappCalcResults>(subList);
                    var faappDemoDataContracts = await DataReader.BulkReadRecordAsync<FaappDemo>(subList);
                    Dictionary<string, FaappDemo> faapDemoDictionary = new Dictionary<string, FaappDemo>();
                    if (faappDemoDataContracts != null && faappDemoDataContracts.Any())
                    {
                        faapDemoDictionary = faappDemoDataContracts.ToDictionary(x => x.Recordkey);
                    }
                    var aidApplicationResultsEntities = new List<AidApplicationResults>();
                    if (faappCalcResultsDataContracts != null && faappCalcResultsDataContracts.Count > 0)
                    {
                        var key = "";
                        foreach (var item in faappCalcResultsDataContracts)
                        {
                            key = item.Recordkey;
                            if (!faapDemoDictionary.ContainsKey(key))
                            {
                                logger.Error("GetAidApplicationResultsAsync: Aid application demographics record is missing for record- " + item.Recordkey);
                                exception.AddError(new RepositoryError("FAAPP.DEMO.Record.Not.Found", string.Format("Aid application demographics record is missing for record- " + item.Recordkey)));
                                continue;
                            }

                            var applicationResultsEntity = ConvertToAidApplicationResults(item, faapDemoDictionary[key]);
                            aidApplicationResultsEntities.Add(applicationResultsEntity);
                        }
                        if(exception.Errors != null && exception.Errors.Any())
                        {            
                              throw exception;            
                        }
                    }
                    return new Tuple<IEnumerable<AidApplicationResults>, int>(aidApplicationResultsEntities, totalCount);
                }
                else
                {
                    return new Tuple<IEnumerable<AidApplicationResults>, int>(new List<AidApplicationResults>(), totalCount);
                }

            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (RepositoryException)
            {
                throw;
            }
        }


        /// <summary>
        /// Get criteria and limiting list.
        /// </summary>
        /// <returns></returns>
        private async Task<CacheSupport.KeyCacheRequirements> GetAidApplicationResultsFilterCriteria(string appDemoId, string personId, string aidApplicationType, string aidYear, int? transactionNumber,string applicantAssignedId)
        {
            string criteria = string.Empty;
            var criteriaBuilder = new StringBuilder();
            List<string> aidApplicationResultLimitingKeys = new List<string>();
            if (!string.IsNullOrEmpty(appDemoId))
            {
                criteriaBuilder.AppendFormat("WITH FAAPP.CALC.RESULTS.ID = '{0}'", appDemoId);

            }
            if (!string.IsNullOrEmpty(personId))
            {
                if (criteriaBuilder.Length > 0)
                {
                    criteriaBuilder.Append(" AND ");
                }
                criteriaBuilder.AppendFormat("WITH FAPR.STUDENT.ID = '{0}'", personId);
            }

            if (!string.IsNullOrEmpty(aidApplicationType))
            {
                if (criteriaBuilder.Length > 0)
                {
                    criteriaBuilder.Append(" AND ");
                }
                criteriaBuilder.AppendFormat("WITH FAPR.TYPE = '{0}'", aidApplicationType);
            }

            if (!string.IsNullOrEmpty(aidYear))
            {
                if (criteriaBuilder.Length > 0)
                {
                    criteriaBuilder.Append(" AND ");
                }
                criteriaBuilder.AppendFormat("WITH FAPR.YEAR = '{0}'", aidYear);
            }

            if (transactionNumber!=null)
            {
                if (criteriaBuilder.Length > 0)
                {
                    criteriaBuilder.Append(" AND ");
                }
                criteriaBuilder.AppendFormat("WITH FAPR.TRANS.NBR = '{0}'", transactionNumber);
            }

            if (criteriaBuilder.Length > 0)
            {
                criteria = criteriaBuilder.ToString();
            }

            aidApplicationResultLimitingKeys = (await DataReader.SelectAsync("FAAPP.CALC.RESULTS", criteria)).ToList();

            if (!string.IsNullOrEmpty(applicantAssignedId) && aidApplicationResultLimitingKeys != null && aidApplicationResultLimitingKeys.Any())
            {
                string assignedIdCriteria = string.Format("WITH FAAD.ASSIGNED.ID = '{0}'", applicantAssignedId);
                aidApplicationResultLimitingKeys = (await DataReader.SelectAsync("FAAPP.DEMO", aidApplicationResultLimitingKeys.ToArray(), assignedIdCriteria)).ToList();
            }

            if (aidApplicationResultLimitingKeys == null || !aidApplicationResultLimitingKeys.Any())
            {
                return new CacheSupport.KeyCacheRequirements() { criteria = string.Empty, limitingKeys = new List<string>(), NoQualifyingRecords = true };
            }
            if (criteriaBuilder.Length > 0)
            {
                criteria = criteriaBuilder.ToString();
            }
            return new CacheSupport.KeyCacheRequirements()
            {
                limitingKeys = aidApplicationResultLimitingKeys,
                criteria = criteria
            };

        }

        public async Task<AidApplicationResults> CreateAidApplicationResultsAsync(AidApplicationResults aidApplicationResultsEntity)
        {
            if (aidApplicationResultsEntity == null)
                throw new ArgumentNullException("aidApplicationResultsEntity", "Must provide a aidApplicationResultsEntity to create.");
            var repositoryException = new RepositoryException();
            AidApplicationResults createdEntity = null;
            UpdateAidApplResultRequest createRequest;
            try
            {
                createRequest = await BuildUpdateAidApplResultRequestAsync(aidApplicationResultsEntity);
            }
            catch (Exception ex)
            {
                repositoryException.AddError(
                    new RepositoryError("Bad.Data", ex.Message)
                    {
                        Id = aidApplicationResultsEntity.Id,
                        SourceId = aidApplicationResultsEntity.Id
                    });
                throw repositoryException;
            }
            if (createRequest != null)
            {

                var extendedDataTuple = GetEthosExtendedDataLists();
                if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
                {
                    createRequest.ExtendedNames = extendedDataTuple.Item1;
                    createRequest.ExtendedValues = extendedDataTuple.Item2;
                }

                var createResponse = await transactionInvoker.ExecuteAsync<UpdateAidApplResultRequest, UpdateAidApplResultResponse>(createRequest);

                if (createResponse != null && createResponse.UpdateAidApplResultErrors.Any())
                {
                    foreach (var error in createResponse.UpdateAidApplResultErrors)
                    {

                        repositoryException.AddError(new RepositoryError("Create.Update.Exception", string.Concat(error.ErrorCode, " - ", error.ErrorMessage))
                        {
                            SourceId = createRequest.IdemId,
                            Id = createRequest.IdemId

                        });
                    }
                    throw repositoryException;
                }
                else
                {
                    try
                    {
                        createdEntity = await GetAidApplicationResultsByIdAsync(createRequest.IdemId);
                    }
                    catch (Exception ex)
                    {
                        repositoryException.AddError(
                       new RepositoryError("Bad.Data", ex.Message)
                       {
                           Id = aidApplicationResultsEntity.Id,
                           SourceId = aidApplicationResultsEntity.Id
                       });
                        throw repositoryException;
                    }
                }
            }

            // get the newly created record from the database
            return createdEntity;
        }

        public async Task<AidApplicationResults> UpdateAidApplicationResultsAsync(AidApplicationResults aidApplicationResultsEntity)
        {

            if (aidApplicationResultsEntity == null)
                throw new ArgumentNullException("aidApplicationResultsEntity", "Must provide an aidApplicationResultsEntity to update.");
            if (string.IsNullOrWhiteSpace(aidApplicationResultsEntity.Id))
                throw new ArgumentNullException("aidApplicationResultsEntity", "Must provide the id of the aidApplicationResultsEntity to update.");

            var repositoryException = new RepositoryException();
            AidApplicationResults updatedEntity = null;
            UpdateAidApplResultRequest updateRequest;
            try
            {
                updateRequest = await BuildUpdateAidApplResultRequestAsync(aidApplicationResultsEntity);
            }
            catch (Exception ex)
            {
                repositoryException.AddError(
                    new RepositoryError("Bad.Data", ex.Message)
                    {
                        Id = aidApplicationResultsEntity.Id,
                        SourceId = aidApplicationResultsEntity.Id
                    });
                throw repositoryException;
            }
            if (updateRequest != null)
            {
                var extendedDataTuple = GetEthosExtendedDataLists();
                if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
                {
                    updateRequest.ExtendedNames = extendedDataTuple.Item1;
                    updateRequest.ExtendedValues = extendedDataTuple.Item2;
                }

                //write the data
                var updateResponse = await transactionInvoker.ExecuteAsync<UpdateAidApplResultRequest, UpdateAidApplResultResponse>(updateRequest);

                if (updateResponse != null && updateResponse.UpdateAidApplResultErrors.Any())
                {
                    foreach (var error in updateResponse.UpdateAidApplResultErrors)
                    {

                        repositoryException.AddError(new RepositoryError("Create.Update.Exception", string.Concat(error.ErrorCode, " - ", error.ErrorMessage))
                        {
                            SourceId = updateRequest.IdemId,
                            Id = updateRequest.IdemId

                        });
                    }
                    throw repositoryException;
                }
                else
                {
                    try
                    {
                        updatedEntity = await GetAidApplicationResultsByIdAsync(updateRequest.IdemId);
                    }
                    catch (Exception ex)
                    {
                        repositoryException.AddError(
                       new RepositoryError("Bad.Data", ex.Message)
                       {
                           Id = aidApplicationResultsEntity.Id,
                           SourceId = aidApplicationResultsEntity.Id
                       });
                        throw repositoryException;
                    }
                }

            }
            // get the updated entity from the database
            return updatedEntity;
        }

        private async Task<UpdateAidApplResultRequest> BuildUpdateAidApplResultRequestAsync(AidApplicationResults aidApplicationResultsEntity)
        {
            var request = new UpdateAidApplResultRequest();

            var personRecord = await DataReader.ReadRecordAsync<Base.DataContracts.Person>(aidApplicationResultsEntity.PersonId);
            if (personRecord == null)
            {
                throw new ArgumentNullException(string.Format("person record not found with id {0}.", aidApplicationResultsEntity.PersonId));
            }
            if (aidApplicationResultsEntity.Id != "new")
                request.ApplResultsId = aidApplicationResultsEntity.Id;
            request.IdemId = aidApplicationResultsEntity.AppDemoId;
            request.Year = aidApplicationResultsEntity.AidYear;
            request.Type = aidApplicationResultsEntity.AidApplicationType;
            request.StudentId = aidApplicationResultsEntity.PersonId;
            request.TransNbr = aidApplicationResultsEntity.TransactionNumber;            
            request.DepOver = aidApplicationResultsEntity.DependencyOverride;
            request.DepOverSchoolCode = aidApplicationResultsEntity.DependencyOverSchoolCode;          
            request.DepStatus = aidApplicationResultsEntity.DependencyStatus;
            request.TransDataSourceType = aidApplicationResultsEntity.TransactionSource;
            if (aidApplicationResultsEntity.TransactionReceiptDate.HasValue)
            {
                request.TransRcptDate = DateTime.SpecifyKind(aidApplicationResultsEntity.TransactionReceiptDate.Value, DateTimeKind.Unspecified);
            }
            request.SpecialCircumstances = aidApplicationResultsEntity.SpecialCircumstances;
            request.PAssetTholdExc = ConvertBoolToString(aidApplicationResultsEntity.ParentAssetExceeded);
            request.SAssetTholdExc = ConvertBoolToString(aidApplicationResultsEntity.StudentAssetExceeded);
            
            request.EtiDestNbr = aidApplicationResultsEntity.DestinationNumber;
            request.CurrPseudoId = aidApplicationResultsEntity.StudentCurrentPseudoId;
            request.CorrAppliedAgainst = aidApplicationResultsEntity.CorrectionAppliedAgainst;
            request.ProfJudgInd = aidApplicationResultsEntity.ProfJudgementIndicator;
            request.ApplDataSourceType = aidApplicationResultsEntity.ApplicationDataSource;
            if (aidApplicationResultsEntity.ApplicationReceiptDate.HasValue)
            {
                request.ApplRcptDate = DateTime.SpecifyKind(aidApplicationResultsEntity.ApplicationReceiptDate.Value, DateTimeKind.Unspecified);
            }
            request.AddrOnlyChgFlag = aidApplicationResultsEntity.AddressOnlyChangeFlag;
            request.PushedIsirFlag = ConvertBoolToString(aidApplicationResultsEntity.PushedApplicationFlag);
            
            request.EfcChgFlag = aidApplicationResultsEntity.EfcChangeFlag;
            request.SLastNameChgFlag = aidApplicationResultsEntity.LastNameChange;
            request.RejChgFlag = ConvertBoolToString(aidApplicationResultsEntity.RejectStatusChange);
            request.SarcChgFlag = ConvertBoolToString(aidApplicationResultsEntity.SarcChange);
            
            request.ComputeNbr = aidApplicationResultsEntity.ComputeNumber;
            request.CorrSource = aidApplicationResultsEntity.CorrectionSource;
            request.DupPid = ConvertBoolToString(aidApplicationResultsEntity.DuplicateIdIndicator);
            request.GradFlag = ConvertBoolToString(aidApplicationResultsEntity.GraduateFlag);
            
            if (aidApplicationResultsEntity.TransactionProcessedDate.HasValue)
            {
                request.TransProcDate = DateTime.SpecifyKind(aidApplicationResultsEntity.TransactionProcessedDate.Value, DateTimeKind.Unspecified);
            }
            request.ProcRecordType = aidApplicationResultsEntity.ProcessedRecordType;
            request.RejectCodes = aidApplicationResultsEntity.RejectReasonCodes;
            request.AzeInd = ConvertBoolToString(aidApplicationResultsEntity.AutomaticZeroIndicator);
            
            request.SntInd = aidApplicationResultsEntity.SimplifiedNeedsTest;
            request.PCalcTaxStatus = aidApplicationResultsEntity.ParentCalculatedTaxStatus;
            request.SCalcTaxStatus = aidApplicationResultsEntity.StudentCalculatedTaxStatus;
            request.SAddlFinInfoTotal = aidApplicationResultsEntity.StudentAddlFinCalcTotal;
            request.SOthUntxInc = aidApplicationResultsEntity.studentOthUntaxIncomeCalcTotal;
            request.PAddlFinInfoTotal = aidApplicationResultsEntity.ParentAddlFinCalcTotal;
            request.POthUntxInc = aidApplicationResultsEntity.ParentOtherUntaxIncomeCalcTotal;
            request.HsInvalidFlag = ConvertBoolToString(aidApplicationResultsEntity.InvalidHighSchool);
            
                request.AssumCitizenship = aidApplicationResultsEntity.AssumCitizenship;
                request.AssumSMarStat = aidApplicationResultsEntity.AssumSMarStat;
                request.AssumSAgi = aidApplicationResultsEntity.AssumSAgi;
                request.AssumSTaxPd = aidApplicationResultsEntity.AssumSTaxPd;
                request.AssumSIncWork = aidApplicationResultsEntity.AssumSIncWork;
                request.AssumSpIncWork = aidApplicationResultsEntity.AssumSpIncWork;
                request.AssumSAddlFinAmt = aidApplicationResultsEntity.AssumSAddlFinAmt;
                request.AssumBirthDatePrior = aidApplicationResultsEntity.AssumBirthDatePrior;
                request.AssumSMarried = aidApplicationResultsEntity.AssumSMarried;
                request.AssumChildren = aidApplicationResultsEntity.AssumChildren;
                request.AssumLegalDep = aidApplicationResultsEntity.AssumLegalDep;
                request.AssumSNbrFamily = aidApplicationResultsEntity.AssumSNbrFamily;
                request.AssumSNbrCollege = aidApplicationResultsEntity.AssumSNbrCollege;
                request.AssumSAssetTholdExc = ConvertBoolToString(aidApplicationResultsEntity.AssumSAssetTholdExc);
                
                request.AssumPMarStat = aidApplicationResultsEntity.AssumPMarStat;
                request.AssumPar1Ssn = ConvertBoolToString(aidApplicationResultsEntity.AssumPar1Ssn);
                request.AssumPar2Ssn = ConvertBoolToString(aidApplicationResultsEntity.AssumPar2Ssn);
                
                request.AssumPNbrFamily = aidApplicationResultsEntity.AssumPNbrFamily;
                request.AssumPNbrCollege = aidApplicationResultsEntity.AssumPNbrCollege;
                request.AssumPAgi = aidApplicationResultsEntity.AssumPAgi;
                request.AssumPTaxPd = aidApplicationResultsEntity.AssumPTaxPd;
                request.AssumPar1Income = aidApplicationResultsEntity.AssumPar1Income;
                request.AssumPar2Income = aidApplicationResultsEntity.AssumPar2Income;
                request.AssumPAddlFinAmt = aidApplicationResultsEntity.AssumPAddlFinAmt;
                request.AssumPAssetTholdExc = ConvertBoolToString(aidApplicationResultsEntity.AssumPAssetTholdExc);
                
                        
            request.PriEfc = aidApplicationResultsEntity.PrimaryEfc;
            request.SecEfc = aidApplicationResultsEntity.SecondaryEfc;
            request.SignRejEfc = aidApplicationResultsEntity.SignatureRejectEfc;
            request.PriEfcType = aidApplicationResultsEntity.PrimaryEfcType;
            
            request.PriAlt1mnthEfc = aidApplicationResultsEntity.PriAlt1mnthEfc;
            request.PriAlt2mnthEfc = aidApplicationResultsEntity.PriAlt2mnthEfc;
            request.PriAlt3mnthEfc = aidApplicationResultsEntity.PriAlt3mnthEfc;
            request.PriAlt4mnthEfc = aidApplicationResultsEntity.PriAlt4mnthEfc;
            request.PriAlt5mnthEfc = aidApplicationResultsEntity.PriAlt5mnthEfc;
            request.PriAlt6mnthEfc = aidApplicationResultsEntity.PriAlt6mnthEfc;
            request.PriAlt7mnthEfc = aidApplicationResultsEntity.PriAlt7mnthEfc;
            request.PriAlt8mnthEfc = aidApplicationResultsEntity.PriAlt8mnthEfc;
            request.PriAlt10mnthEfc = aidApplicationResultsEntity.PriAlt10mnthEfc;
            request.PriAlt11mnthEfc = aidApplicationResultsEntity.PriAlt11mnthEfc;
            request.PriAlt12mnthEfc = aidApplicationResultsEntity.PriAlt12mnthEfc;


            request.PriTi = aidApplicationResultsEntity.TotalIncome;
            request.PriAti = aidApplicationResultsEntity.AllowancesAgainstTotalIncome;
            request.PriStx = aidApplicationResultsEntity.TaxAllowance;
            request.PriEa = aidApplicationResultsEntity.EmploymentAllowance;
            request.PriIpa = aidApplicationResultsEntity.IncomeProtectionAllowance;
            request.PriAi = aidApplicationResultsEntity.AvailableIncome;
            request.PriCai = aidApplicationResultsEntity.AvailableIncomeContribution;
            request.PriDnw = aidApplicationResultsEntity.DiscretionaryNetWorth;
            request.PriNw = aidApplicationResultsEntity.NetWorth;
            request.PriApa = aidApplicationResultsEntity.AssetProtectionAllowance;
            request.PriPca = aidApplicationResultsEntity.ParentContributionAssets;
            request.PriAai = aidApplicationResultsEntity.AdjustedAvailableIncome;
            request.PriTsc = aidApplicationResultsEntity.TotalPrimaryStudentContribution;
            request.PriTpc = aidApplicationResultsEntity.TotalPrimaryParentContribution;
            request.PriPc = aidApplicationResultsEntity.ParentContribution;
            request.PriSti = aidApplicationResultsEntity.StudentTotalIncome;
            request.PriSati = aidApplicationResultsEntity.StudentAllowanceAgainstIncome;
            request.PriSic = aidApplicationResultsEntity.DependentStudentIncContrib;
            request.PriSdnw = aidApplicationResultsEntity.StudentDiscretionaryNetWorth;
            request.PriSca = aidApplicationResultsEntity.StudentAssetContribution;
            request.PriFti = aidApplicationResultsEntity.FisapTotalIncome;
            request.CorrectionFlags = aidApplicationResultsEntity.CorrectionFlags;
            request.HighlightFlags = aidApplicationResultsEntity.HighlightFlags;
            request.CommentCodes = aidApplicationResultsEntity.CommentCodes;
            request.ElecSchoolInd = aidApplicationResultsEntity.ElectronicFedSchoolCodeInd;
            request.ElecTranIndicator = aidApplicationResultsEntity.ElectronicTransactionIndicator;
            request.SelectedForVerif = aidApplicationResultsEntity.VerificationSelected;
            return request;
        }

        private static AidApplicationResults ConvertToAidApplicationResults(FaappCalcResults faappCalcResults, FaappDemo faappDemo)
        {
            if (faappCalcResults == null || faappDemo == null)
            {
                throw new ArgumentNullException("faappCalcResults & faappDemo is required.");
            }
            AidApplicationResults aidApplicationResultsEntity = new AidApplicationResults(faappCalcResults.Recordkey, faappDemo.Recordkey);
            aidApplicationResultsEntity.PersonId = CheckAndAssignValue(faappCalcResults.FaprStudentId);
            aidApplicationResultsEntity.AidYear = CheckAndAssignValue(faappCalcResults.FaprYear);
            aidApplicationResultsEntity.AidApplicationType = CheckAndAssignValue(faappCalcResults.FaprType);
            aidApplicationResultsEntity.ApplicantAssignedId = CheckAndAssignValue(faappDemo.FaadAssignedId);
            aidApplicationResultsEntity.TransactionNumber = faappCalcResults.FaprTransNbr;
            aidApplicationResultsEntity.DependencyOverride = CheckAndAssignValue(faappCalcResults.FaprDepOver);
            aidApplicationResultsEntity.DependencyOverSchoolCode = CheckAndAssignValue(faappCalcResults.FaprDepOverSchoolCode);
            aidApplicationResultsEntity.DependencyStatus = CheckAndAssignValue(faappCalcResults.FaprDepStatus);
            aidApplicationResultsEntity.TransactionSource = CheckAndAssignValue(faappCalcResults.FaprTransDataSourceType);
            aidApplicationResultsEntity.TransactionReceiptDate = faappCalcResults.FaprTransRcptDate;
            aidApplicationResultsEntity.SpecialCircumstances = CheckAndAssignValue(faappCalcResults.FaprSpecialCircumstances);
            aidApplicationResultsEntity.ParentAssetExceeded = ConvertStringToBool(faappCalcResults.FaprPAssetTholdExc);
            aidApplicationResultsEntity.StudentAssetExceeded = ConvertStringToBool(faappCalcResults.FaprSAssetTholdExc);
            aidApplicationResultsEntity.DestinationNumber = CheckAndAssignValue(faappCalcResults.FaprEtiDestNbr);
            aidApplicationResultsEntity.StudentCurrentPseudoId = CheckAndAssignValue(faappCalcResults.FaprCurrPseudoId);
            aidApplicationResultsEntity.CorrectionAppliedAgainst = CheckAndAssignValue(faappCalcResults.FaprCorrAppliedAgainst);
            //ENUM
            aidApplicationResultsEntity.ProfJudgementIndicator = CheckAndAssignValue(faappCalcResults.FaprProfJudgInd);

            aidApplicationResultsEntity.ApplicationDataSource = CheckAndAssignValue(faappCalcResults.FaprApplDataSourceType);
            aidApplicationResultsEntity.ApplicationReceiptDate = faappCalcResults.FaprApplRcptDate;
            aidApplicationResultsEntity.AddressOnlyChangeFlag = CheckAndAssignValue(faappCalcResults.FaprAddrOnlyChgFlag);
            aidApplicationResultsEntity.PushedApplicationFlag = ConvertStringToBool(faappCalcResults.FaprPushedIsirFlag);
            //ENUM
            aidApplicationResultsEntity.EfcChangeFlag = CheckAndAssignValue(faappCalcResults.FaprEfcChgFlag);

            aidApplicationResultsEntity.LastNameChange = CheckAndAssignValue(faappCalcResults.FaprSLastNameChgFlag);
            aidApplicationResultsEntity.RejectStatusChange = ConvertStringToBool(faappCalcResults.FaprRejChgFlag);
            aidApplicationResultsEntity.SarcChange = ConvertStringToBool(faappCalcResults.FaprSarcChgFlag);
            aidApplicationResultsEntity.ComputeNumber = CheckAndAssignValue(faappCalcResults.FaprComputeNbr);
            aidApplicationResultsEntity.CorrectionSource = CheckAndAssignValue(faappCalcResults.FaprCorrSource);
            aidApplicationResultsEntity.DuplicateIdIndicator = ConvertStringToBool(faappCalcResults.FaprDupPid);
            aidApplicationResultsEntity.GraduateFlag = ConvertStringToBool(faappCalcResults.FaprGradFlag);
            aidApplicationResultsEntity.TransactionProcessedDate = faappCalcResults.FaprTransProcDate;
            aidApplicationResultsEntity.ProcessedRecordType = CheckAndAssignValue(faappCalcResults.FaprProcRecordType);
            if (faappCalcResults.FaprRejectCodes != null && faappCalcResults.FaprRejectCodes.Any())
            {
                aidApplicationResultsEntity.RejectReasonCodes = faappCalcResults.FaprRejectCodes;
            }
            aidApplicationResultsEntity.AutomaticZeroIndicator = ConvertStringToBool(faappCalcResults.FaprAzeInd);
            aidApplicationResultsEntity.SimplifiedNeedsTest = CheckAndAssignValue(faappCalcResults.FaprSntInd);
            aidApplicationResultsEntity.ParentCalculatedTaxStatus = CheckAndAssignValue(faappCalcResults.FaprPCalcTaxStatus);
            aidApplicationResultsEntity.StudentCalculatedTaxStatus = CheckAndAssignValue(faappCalcResults.FaprSCalcTaxStatus);
            aidApplicationResultsEntity.StudentAddlFinCalcTotal = faappCalcResults.FaprSAddlFinInfoTotal;
            aidApplicationResultsEntity.studentOthUntaxIncomeCalcTotal = faappCalcResults.FaprSOthUntxInc;
            aidApplicationResultsEntity.ParentAddlFinCalcTotal = faappCalcResults.FaprPAddlFinInfoTotal;
            aidApplicationResultsEntity.ParentOtherUntaxIncomeCalcTotal = faappCalcResults.FaprPOthUntxInc;
            aidApplicationResultsEntity.InvalidHighSchool = ConvertStringToBool(faappCalcResults.FaprHsInvalidFlag);

            #region assumed start
            aidApplicationResultsEntity.AssumCitizenship = CheckAndAssignValue(faappCalcResults.FaprAssumCitizenship);
            aidApplicationResultsEntity.AssumSMarStat = CheckAndAssignValue(faappCalcResults.FaprAssumSMarStat);
            aidApplicationResultsEntity.AssumSAgi = faappCalcResults.FaprAssumSAgi;
            aidApplicationResultsEntity.AssumSTaxPd = faappCalcResults.FaprAssumSTaxPd;
            aidApplicationResultsEntity.AssumSIncWork = faappCalcResults.FaprAssumSIncWork;
            aidApplicationResultsEntity.AssumSpIncWork = faappCalcResults.FaprAssumSpIncWork;
            aidApplicationResultsEntity.AssumSAddlFinAmt = faappCalcResults.FaprAssumSAddlFinAmt;
            aidApplicationResultsEntity.AssumBirthDatePrior = CheckAndAssignValue(faappCalcResults.FaprAssumBirthDatePrior);
            aidApplicationResultsEntity.AssumSMarried = CheckAndAssignValue(faappCalcResults.FaprAssumSMarried);
            aidApplicationResultsEntity.AssumChildren = CheckAndAssignValue(faappCalcResults.FaprAssumChildren);
            aidApplicationResultsEntity.AssumLegalDep = CheckAndAssignValue(faappCalcResults.FaprAssumLegalDep);
            aidApplicationResultsEntity.AssumSNbrFamily = faappCalcResults.FaprAssumSNbrFamily;
            aidApplicationResultsEntity.AssumSNbrCollege = faappCalcResults.FaprAssumSNbrCollege;
            aidApplicationResultsEntity.AssumSAssetTholdExc = ConvertStringToBool(faappCalcResults.FaprAssumSAssetTholdExc);

            aidApplicationResultsEntity.AssumPMarStat = CheckAndAssignValue(faappCalcResults.FaprAssumPMarStat);
            aidApplicationResultsEntity.AssumPar1Ssn = ConvertStringToBool(faappCalcResults.FaprAssumPar1Ssn);
            aidApplicationResultsEntity.AssumPar2Ssn = ConvertStringToBool(faappCalcResults.FaprAssumPar2Ssn);
            aidApplicationResultsEntity.AssumPNbrFamily = faappCalcResults.FaprAssumPNbrFamily;
            aidApplicationResultsEntity.AssumPNbrCollege = faappCalcResults.FaprAssumPNbrCollege;
            aidApplicationResultsEntity.AssumPAgi = faappCalcResults.FaprAssumPAgi;
            aidApplicationResultsEntity.AssumPTaxPd = faappCalcResults.FaprAssumPTaxPd;
            aidApplicationResultsEntity.AssumPar1Income = faappCalcResults.FaprAssumPar1Income;
            aidApplicationResultsEntity.AssumPar2Income = faappCalcResults.FaprAssumPar2Income;
            aidApplicationResultsEntity.AssumPAddlFinAmt = faappCalcResults.FaprAssumPAddlFinAmt;
            aidApplicationResultsEntity.AssumPAssetTholdExc = ConvertStringToBool(faappCalcResults.FaprAssumPAssetTholdExc);

            #endregion assumed end

            #region efc start
            aidApplicationResultsEntity.PrimaryEfc = faappCalcResults.FaprPriEfc;
            aidApplicationResultsEntity.SecondaryEfc = faappCalcResults.FaprSecEfc;
            aidApplicationResultsEntity.SignatureRejectEfc = faappCalcResults.FaprSignRejEfc;
            aidApplicationResultsEntity.PrimaryEfcType = CheckAndAssignValue(faappCalcResults.FaprPriEfcType);

            aidApplicationResultsEntity.PriAlt1mnthEfc = faappCalcResults.FaprPriAlt1mnthEfc;
            aidApplicationResultsEntity.PriAlt2mnthEfc = faappCalcResults.FaprPriAlt2mnthEfc;
            aidApplicationResultsEntity.PriAlt3mnthEfc = faappCalcResults.FaprPriAlt3mnthEfc;
            aidApplicationResultsEntity.PriAlt4mnthEfc = faappCalcResults.FaprPriAlt4mnthEfc;
            aidApplicationResultsEntity.PriAlt5mnthEfc = faappCalcResults.FaprPriAlt5mnthEfc;
            aidApplicationResultsEntity.PriAlt6mnthEfc = faappCalcResults.FaprPriAlt6mnthEfc;
            aidApplicationResultsEntity.PriAlt7mnthEfc = faappCalcResults.FaprPriAlt7mnthEfc;
            aidApplicationResultsEntity.PriAlt8mnthEfc = faappCalcResults.FaprPriAlt8mnthEfc;
            aidApplicationResultsEntity.PriAlt10mnthEfc = faappCalcResults.FaprPriAlt10mnthEfc;
            aidApplicationResultsEntity.PriAlt11mnthEfc = faappCalcResults.FaprPriAlt11mnthEfc;
            aidApplicationResultsEntity.PriAlt12mnthEfc = faappCalcResults.FaprPriAlt12mnthEfc;

            #endregion efc end

            aidApplicationResultsEntity.TotalIncome = faappCalcResults.FaprPriTi;
            aidApplicationResultsEntity.AllowancesAgainstTotalIncome = faappCalcResults.FaprPriAti;
            aidApplicationResultsEntity.TaxAllowance = faappCalcResults.FaprPriStx;
            aidApplicationResultsEntity.EmploymentAllowance = faappCalcResults.FaprPriEa;
            aidApplicationResultsEntity.IncomeProtectionAllowance = faappCalcResults.FaprPriIpa;
            aidApplicationResultsEntity.AvailableIncome = faappCalcResults.FaprPriAi;
            aidApplicationResultsEntity.AvailableIncomeContribution = faappCalcResults.FaprPriCai;
            aidApplicationResultsEntity.DiscretionaryNetWorth = faappCalcResults.FaprPriDnw;
            aidApplicationResultsEntity.NetWorth = faappCalcResults.FaprPriNw;
            aidApplicationResultsEntity.AssetProtectionAllowance = faappCalcResults.FaprPriApa;
            aidApplicationResultsEntity.ParentContributionAssets = faappCalcResults.FaprPriPca;
            aidApplicationResultsEntity.AdjustedAvailableIncome = faappCalcResults.FaprPriAai;
            aidApplicationResultsEntity.TotalPrimaryStudentContribution = faappCalcResults.FaprPriTsc;
            aidApplicationResultsEntity.TotalPrimaryParentContribution = faappCalcResults.FaprPriTpc;
            aidApplicationResultsEntity.ParentContribution = faappCalcResults.FaprPriPc;
            aidApplicationResultsEntity.StudentTotalIncome = faappCalcResults.FaprPriSti;
            aidApplicationResultsEntity.StudentAllowanceAgainstIncome = faappCalcResults.FaprPriSati;
            aidApplicationResultsEntity.DependentStudentIncContrib = faappCalcResults.FaprPriSic;
            aidApplicationResultsEntity.StudentDiscretionaryNetWorth = faappCalcResults.FaprPriSdnw;
            aidApplicationResultsEntity.StudentAssetContribution = faappCalcResults.FaprPriSca;
            aidApplicationResultsEntity.FisapTotalIncome = faappCalcResults.FaprPriFti;
            aidApplicationResultsEntity.CorrectionFlags = CheckAndAssignValue(faappCalcResults.FaprCorrectionFlags);
            aidApplicationResultsEntity.HighlightFlags = CheckAndAssignValue(faappCalcResults.FaprHighlightFlags);
            if (faappCalcResults.FaprCommentCodes != null && faappCalcResults.FaprCommentCodes.Any())
            {
                aidApplicationResultsEntity.CommentCodes = faappCalcResults.FaprCommentCodes;
            }
            aidApplicationResultsEntity.ElectronicFedSchoolCodeInd = CheckAndAssignValue(faappCalcResults.FaprElecSchoolInd);
            aidApplicationResultsEntity.ElectronicTransactionIndicator = CheckAndAssignValue(faappCalcResults.FaprEtiFlag);
            aidApplicationResultsEntity.VerificationSelected = CheckAndAssignValue(faappCalcResults.FaprSelectedForVerif);

            return aidApplicationResultsEntity;
        }

        private static bool? ConvertStringToBool(string inputValue)
        {
            bool? output = null;
            if (!string.IsNullOrEmpty(inputValue))
            {
                if (inputValue.ToUpper() == "Y")
                    output = true;
                else
                    output = false;
            }
            return output;
        }

        private string ConvertBoolToString(bool? input)
        {
            string output = string.Empty;
            if (input.HasValue)
                output = input.Value ? "Y" : "N";

            return output;
        }

        private static string CheckAndAssignValue(string inputValue)
        {
            return !string.IsNullOrEmpty(inputValue) ? inputValue : null;
        }         

        
    }
}
