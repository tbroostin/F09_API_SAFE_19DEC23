/*Copyright 2022 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
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

    public class AidApplicationsRepository : BaseColleagueRepository, IAidApplicationsRepository, IEthosExtended
    {
        protected const int AllAidApplicationFilterTimeout = 20; // Clear from cache every 20 minutes
        protected const string AllAidApplicationFilterCache = "AllAidApplicationFilter";

        /// <summary>
        /// Constructor for AidApplicationsRepository
        /// </summary>
        /// <param name="cacheProvider">CacheProvider</param>
        /// <param name="transactionFactory">TransactionFactory</param>
        /// <param name="logger">Logger</param>
        /// 
        public AidApplicationsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        #region Get By ID AidApplicationsAsyc
        /// <summary>
        /// GetAidApplicationsByIdAsync - Get Aid Applications by ID API
        /// </summary> 
        public async Task<AidApplications> GetAidApplicationsByIdAsync(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Financial aid applications ID is required for record retrieval.");
            }

            FaappApps faappAppsDataContract = await DataReader.ReadRecordAsync<FaappApps>(id);
            if (faappAppsDataContract == null)
            {
                throw new KeyNotFoundException("No aid-applications was found for ID "+ id);
            }

            FaappDemo faappDemo = await DataReader.ReadRecordAsync<FaappDemo>(id);
            return ConvertToAidApplications(faappAppsDataContract, faappDemo);
        }
        #endregion

        #region Get All AidApplicationsAsync
        /// <summary>
        /// GetAidApplicationsAsync - Get all Aid Applications API
        /// </summary>
        public async Task<Tuple<IEnumerable<AidApplications>, int>> GetAidApplicationsAsync(int offset, int limit, string appDemoId = "", string personId = "",
            string aidApplicationType = "", string aidYear = null, string assignedId = "")
        {
            try
            {
                int totalCount = 0;
                string[] subList = null;
                var repositoryException = new RepositoryException();
                string aidApplicationCacheKey = CacheSupport.BuildCacheKey(AllAidApplicationFilterCache, appDemoId, personId, aidApplicationType, aidYear, assignedId);
                var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
                       this,
                       ContainsKey,
                       GetOrAddToCacheAsync,
                       AddOrUpdateCacheAsync,
                       transactionInvoker,
                       aidApplicationCacheKey,
                       "FAAPP.APPS",
                       offset,
                       limit,
                       AllAidApplicationFilterTimeout,
                       () =>
                       {
                           return GetAidApplicationFilterCriteria(appDemoId, personId, aidApplicationType, aidYear, assignedId);
                       }
                   );

                if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
                {
                    return new Tuple<IEnumerable<AidApplications>, int>(new List<AidApplications>(), 0);
                }

                subList = keyCache.Sublist.ToArray();
                totalCount = keyCache.TotalCount.Value;
                var exception = new RepositoryException();
                if (subList != null && subList.Any())
                {
                    var faappAppsDataContracts = await DataReader.BulkReadRecordAsync<FaappApps>(subList);
                    var faappDemoDataContracts = await DataReader.BulkReadRecordAsync<FaappDemo>(subList);
                    Dictionary<string, FaappDemo> faapDemoDictionary = new Dictionary<string, FaappDemo>();
                    if (faappDemoDataContracts != null && faappDemoDataContracts.Any())
                    {
                        faapDemoDictionary = faappDemoDataContracts.ToDictionary(x => x.Recordkey);
                    }
                    var aidApplicationsEntities = new List<AidApplications>();
                    if (faappAppsDataContracts != null && faappAppsDataContracts.Count > 0)
                    {
                        var key = "";
                        foreach (var item in faappAppsDataContracts)
                        {
                            key = item.Recordkey;
                            if (!faapDemoDictionary.ContainsKey(key))
                            {
                                logger.Error("GetAidApplicationsAsync: Aid application demographics record is missing for record- " + item.Recordkey);
                                exception.AddError(new RepositoryError("FAAPP.DEMO.Record.Not.Found", string.Format("Aid application demographics record is missing for record- " + item.Recordkey)));
                                continue;
                            }

                            var applicationAndDemoEntity = ConvertToAidApplications(item, faapDemoDictionary[key]);
                            aidApplicationsEntities.Add(applicationAndDemoEntity);
                        }
                        if(exception.Errors != null && exception.Errors.Any())
                        {            
                              throw exception;            
                        }            
                    }
                    return new Tuple<IEnumerable<AidApplications>, int>(aidApplicationsEntities, totalCount);
                }
                else
                {
                    return new Tuple<IEnumerable<AidApplications>, int>(new List<AidApplications>(), totalCount);
                }

            }
            catch (ArgumentException )
            {
                throw ;
            }
            catch (RepositoryException )
            {
                throw ;
            }
        }
        #endregion

        #region GetAidApplicationFilterCriteria
        /// <summary>
        /// Get criteria and limiting list.
        /// </summary>
        /// <returns></returns>
        private async Task<CacheSupport.KeyCacheRequirements> GetAidApplicationFilterCriteria(string appDemoId,string personId, string aidApplicationType, string aidYear, string applicantAssignedId)
        {
            string criteria = string.Empty;
            var criteriaBuilder = new StringBuilder();
            List<string> aidApplicationLimitingKeys = new List<string>();
            if (!string.IsNullOrEmpty(appDemoId))
            {
                //criteriaBuilder.AppendFormat("WITH FAAPP.DEMO.ID = '{0}'", appDemoId);
                criteriaBuilder.AppendFormat("WITH FAAPP.APPS.ID = '{0}'", appDemoId);

            }
            if (!string.IsNullOrEmpty(personId))
            {
                if (criteriaBuilder.Length > 0)
                {
                    criteriaBuilder.Append(" AND ");
                }
                criteriaBuilder.AppendFormat("WITH FAAA.STUDENT.ID = '{0}'", personId);
            }

            if (!string.IsNullOrEmpty(aidApplicationType))
            {
                if (criteriaBuilder.Length > 0)
                {
                    criteriaBuilder.Append(" AND ");
                }
                criteriaBuilder.AppendFormat("WITH FAAA.TYPE = '{0}'", aidApplicationType);
            }

            
            if (!string.IsNullOrEmpty(aidYear))
            {
                if (criteriaBuilder.Length > 0)
                {
                    criteriaBuilder.Append(" AND ");
                }
                criteriaBuilder.AppendFormat("WITH FAAA.YEAR = '{0}'", aidYear);
            }

            if (criteriaBuilder.Length > 0)
            {
                criteria = criteriaBuilder.ToString();
            }

            aidApplicationLimitingKeys = (await DataReader.SelectAsync("FAAPP.APPS", criteria)).ToList();

            if (!string.IsNullOrEmpty(applicantAssignedId) && aidApplicationLimitingKeys != null && aidApplicationLimitingKeys.Any())
            {
                string assignedIdCriteria = string.Format("WITH FAAD.ASSIGNED.ID = '{0}'", applicantAssignedId);
                aidApplicationLimitingKeys = (await DataReader.SelectAsync("FAAPP.DEMO", aidApplicationLimitingKeys.ToArray(), assignedIdCriteria)).ToList();
            }

            if (aidApplicationLimitingKeys == null || !aidApplicationLimitingKeys.Any())
            {
                return new CacheSupport.KeyCacheRequirements() { criteria = string.Empty, limitingKeys = new List<string>(), NoQualifyingRecords = true };
            }
            if (criteriaBuilder.Length > 0)
            {
                criteria = criteriaBuilder.ToString();
            }
            return new CacheSupport.KeyCacheRequirements()
            {
                limitingKeys = aidApplicationLimitingKeys,
                criteria = criteria
            };

        }
        #endregion

        #region ConvertToAidApplicationsEntity (Data contract to Entity)
        private static AidApplications ConvertToAidApplications(FaappApps faappAppsDataContract, FaappDemo faappDemo)
        {
            if (faappAppsDataContract == null || faappDemo == null)
            {
                throw new ArgumentNullException("faappAppsDataContract & faappDemo is required.");
            }
            AidApplications aidApplicationsEntity = new AidApplications(faappAppsDataContract.Recordkey, faappAppsDataContract.Recordkey);
            aidApplicationsEntity.PersonId = faappAppsDataContract.FaaaStudentId;
            aidApplicationsEntity.AidYear = faappAppsDataContract.FaaaYear;
            aidApplicationsEntity.AidApplicationType = faappAppsDataContract.FaaaType;
            // get the assigned ID from faappDemo data contract
            aidApplicationsEntity.AssignedID                    = faappDemo.FaadAssignedId;
            aidApplicationsEntity.StudentMaritalStatus          = faappAppsDataContract.FaaaSMaritalStatus;
            aidApplicationsEntity.StudentMaritalDate            = faappAppsDataContract.FaaaSMaritalDate.ToString();
            aidApplicationsEntity.StudentLegalResSt             = faappAppsDataContract.FaaaSLegalResSt;
            aidApplicationsEntity.StudentLegalResB4             = ConvertStringToBool(faappAppsDataContract.FaaaSLegalResB4);
            //aidApplicationsEntity.StudentLegalResB4             = !string.IsNullOrEmpty(faappAppsDataContract.FaaaSLegalResB4) && faappAppsDataContract.FaaaSLegalResB4.ToUpper() == "Y";
            aidApplicationsEntity.StudentLegalResDate           = faappAppsDataContract.FaaaSLegalResDate.ToString();
            aidApplicationsEntity.P1GradeLvl                    = faappAppsDataContract.FaaaP1GradeLvl;
            aidApplicationsEntity.P1Ssn                         = faappAppsDataContract.FaaaP1Ssn;
            aidApplicationsEntity.P1LastName                    = faappAppsDataContract.FaaaP1LastName;
            aidApplicationsEntity.P1FirstInit                   = faappAppsDataContract.FaaaP1FirstInit;
            aidApplicationsEntity.P1Dob                         = faappAppsDataContract.FaaaP1Dob;
            aidApplicationsEntity.P2GradeLvl                    = faappAppsDataContract.FaaaP2GradeLvl;
            aidApplicationsEntity.P2Ssn                         = faappAppsDataContract.FaaaP2Ssn;
            aidApplicationsEntity.P2LastName                    = faappAppsDataContract.FaaaP2LastName;
            aidApplicationsEntity.P2FirstInit                   = faappAppsDataContract.FaaaP2FirstInit;
            aidApplicationsEntity.P2Dob                         = faappAppsDataContract.FaaaP2Dob;
            aidApplicationsEntity.PMaritalStatus                = faappAppsDataContract.FaaaPMaritalStatus;
            aidApplicationsEntity.PMaritalDate                  = faappAppsDataContract.FaaaPMaritalDate.ToString();
            aidApplicationsEntity.ParentEmail                   = faappAppsDataContract.FaaaParentEmail;
            aidApplicationsEntity.PLegalResSt                   = faappAppsDataContract.FaaaPLegalResSt;
            aidApplicationsEntity.PLegalResB4                   = ConvertStringToBool(faappAppsDataContract.FaaaPLegalResB4);
            aidApplicationsEntity.PLegalResDate                 = faappAppsDataContract.FaaaPLegalResDate.ToString();
            aidApplicationsEntity.PNbrFamily                    = faappAppsDataContract.FaaaPNbrFamily;
            aidApplicationsEntity.PNbrCollege                   = faappAppsDataContract.FaaaPNbrCollege;
            aidApplicationsEntity.PSsiBen                       = ConvertStringToBool(faappAppsDataContract.FaaaPSsiBen); 
            aidApplicationsEntity.PFoodStamps                   = ConvertStringToBool(faappAppsDataContract.FaaaPFoodStamps);
            aidApplicationsEntity.PLunchBen                     = ConvertStringToBool(faappAppsDataContract.FaaaPLunchBen); 
            aidApplicationsEntity.PTanf                         = ConvertStringToBool(faappAppsDataContract.FaaaPTanf); 
            aidApplicationsEntity.PWic                          = ConvertStringToBool(faappAppsDataContract.FaaaPWic); 
            aidApplicationsEntity.PTaxReturnFiled               = faappAppsDataContract.FaaaPTaxReturnFiled;
            aidApplicationsEntity.PTaxFormType                  = faappAppsDataContract.FaaaPTaxFormType;
            aidApplicationsEntity.PTaxFilingStatus              = faappAppsDataContract.FaaaPTaxFilingStatus;
            aidApplicationsEntity.PSched1                       = faappAppsDataContract.FaaaPSched1; 
            aidApplicationsEntity.PDisWorker                    = faappAppsDataContract.FaaaPDisWorker; 
            aidApplicationsEntity.PAgi                          = ConvertStringToInt(faappAppsDataContract.FaaaPAgi);
            aidApplicationsEntity.PUsTaxPaid                    = ConvertStringToInt(faappAppsDataContract.FaaaPUsTaxPaid);
            aidApplicationsEntity.P1Income                      = ConvertStringToInt(faappAppsDataContract.FaaaP1Income);
            aidApplicationsEntity.P2Income                      = ConvertStringToInt(faappAppsDataContract.FaaaP2Income);
            aidApplicationsEntity.PCash                         = faappAppsDataContract.FaaaPCash;
            aidApplicationsEntity.PInvNetWorth                  = faappAppsDataContract.FaaaPInvNetWorth;
            aidApplicationsEntity.PBusNetWorth                  = faappAppsDataContract.FaaaPBusNetWorth;
            aidApplicationsEntity.PEduCredit                    = faappAppsDataContract.FaaaPEduCredit;
            aidApplicationsEntity.PChildSupportPd               = faappAppsDataContract.FaaaPChildSupportPd;
            aidApplicationsEntity.PNeedBasedEmp                 = faappAppsDataContract.FaaaPNeedBasedEmp;
            aidApplicationsEntity.PGrantScholAid                = faappAppsDataContract.FaaaPGrantScholAid;
            aidApplicationsEntity.PCombatPay                    = faappAppsDataContract.FaaaPCombatPay;
            aidApplicationsEntity.PCoOpEarnings                 = faappAppsDataContract.FaaaPCoOpEarnings;
            aidApplicationsEntity.PPensionPymts                 = faappAppsDataContract.FaaaPPensionPymts;
            aidApplicationsEntity.PIraPymts                     = faappAppsDataContract.FaaaPIraPymts;
            aidApplicationsEntity.PChildSupRcvd                 = faappAppsDataContract.FaaaPChildSupRcvd;
            aidApplicationsEntity.PUntxIntInc                   = faappAppsDataContract.FaaaPUntxIntInc;
            aidApplicationsEntity.PUntxIraPen                   = faappAppsDataContract.FaaaPUntxIraPen;
            aidApplicationsEntity.PMilClerAllow                 = faappAppsDataContract.FaaaPMilClerAllow;
            aidApplicationsEntity.PVetNonEdBen                  = faappAppsDataContract.FaaaPVetNonEdBen;
            aidApplicationsEntity.POtherUntxInc                 = faappAppsDataContract.FaaaPOtherUntxInc;
            aidApplicationsEntity.HsGradType                    = faappAppsDataContract.FaaaHsGradType;
            aidApplicationsEntity.HsName                        = faappAppsDataContract.FaaaHsName;
            aidApplicationsEntity.HsCity                        = faappAppsDataContract.FaaaHsCity;
            aidApplicationsEntity.HsState                       = faappAppsDataContract.FaaaHsState;
            aidApplicationsEntity.HsCode                        = faappAppsDataContract.FaaaHsCode.ToString();
            aidApplicationsEntity.DegreeBy                      = ConvertStringToBool(faappAppsDataContract.FaaaDegreeBy);
            aidApplicationsEntity.GradeLevelInCollege           = faappAppsDataContract.FaaaGradeLevel;
            aidApplicationsEntity.DegreeOrCertificate           = faappAppsDataContract.FaaaDegOrCert;
            aidApplicationsEntity.StudentTaxReturnFiled         = faappAppsDataContract.FaaaSTaxReturnFiled;
            aidApplicationsEntity.StudentTaxFormType            = faappAppsDataContract.FaaaSTaxFormType;
            aidApplicationsEntity.StudentTaxFilingStatus        = faappAppsDataContract.FaaaSTaxFilingStatus;
            aidApplicationsEntity.StudentSched1                 = faappAppsDataContract.FaaaSSched1;
            aidApplicationsEntity.StudentAgi                    = ConvertStringToInt(faappAppsDataContract.FaaaSAgi);
            aidApplicationsEntity.StudentUsTaxPd                = faappAppsDataContract.FaaaSUsTaxPd;
            aidApplicationsEntity.SStudentInc                   = ConvertStringToInt(faappAppsDataContract.FaaaSStudentInc);
            aidApplicationsEntity.SpouseInc                     = ConvertStringToInt(faappAppsDataContract.FaaaSpouseInc);
            aidApplicationsEntity.StudentCash                   = faappAppsDataContract.FaaaSCash;
            aidApplicationsEntity.StudentInvNetWorth            = faappAppsDataContract.FaaaSInvNetWorth;
            aidApplicationsEntity.StudentBusNetWorth            = faappAppsDataContract.FaaaSBusNetWorth;
            aidApplicationsEntity.StudentEduCredit              = faappAppsDataContract.FaaaSEduCredit;
            aidApplicationsEntity.StudentChildSupPaid           = faappAppsDataContract.FaaaSChildSupPaid;
            aidApplicationsEntity.StudentNeedBasedEmp           = faappAppsDataContract.FaaaSNeedBasedEmp;
            aidApplicationsEntity.StudentGrantScholAid          = faappAppsDataContract.FaaaSGrantScholAid;
            aidApplicationsEntity.StudentCombatPay              = faappAppsDataContract.FaaaSCombatPay;
            aidApplicationsEntity.StudentCoOpEarnings           = faappAppsDataContract.FaaaSCoOpEarnings;
            aidApplicationsEntity.StudentPensionPayments        = faappAppsDataContract.FaaaSPensionPayments;
            aidApplicationsEntity.StudentIraPayments            = faappAppsDataContract.FaaaSIraPayments;
            aidApplicationsEntity.StudentChildSupRecv           = faappAppsDataContract.FaaaSChildSupRecv;
            aidApplicationsEntity.StudentInterestIncome         = faappAppsDataContract.FaaaSInterestIncome;
            aidApplicationsEntity.StudentUntxIraPen             = faappAppsDataContract.FaaaSUntxIraPen;
            aidApplicationsEntity.StudentMilitaryClergyAllow    = faappAppsDataContract.FaaaSMilitaryClergyAllow;
            aidApplicationsEntity.StudentVetNonEdBen            = faappAppsDataContract.FaaaSVetNonEdBen;
            aidApplicationsEntity.StudentOtherUntaxedInc        = faappAppsDataContract.FaaaSOtherUntaxedInc;
            aidApplicationsEntity.StudentOtherNonRepMoney       = faappAppsDataContract.FaaaSOtherNonRepMoney;
            aidApplicationsEntity.SSsiBen                       = ConvertStringToBool(faappAppsDataContract.FaaaSSsiBen); 
            aidApplicationsEntity.SFoodStamps                   = ConvertStringToBool(faappAppsDataContract.FaaaSFoodStamps); 
            aidApplicationsEntity.SLunchBen                     = ConvertStringToBool(faappAppsDataContract.FaaaSLunchBen); 
            aidApplicationsEntity.STanf                         = ConvertStringToBool(faappAppsDataContract.FaaaSTanf); 
            aidApplicationsEntity.SWic                          = ConvertStringToBool(faappAppsDataContract.FaaaSWic)   ; 
            aidApplicationsEntity.SDislWorker                   = faappAppsDataContract.FaaaSDislWorker; 
            aidApplicationsEntity.BornBefore                    = ConvertStringToBool(faappAppsDataContract.FaaaBornB4Dt);
            aidApplicationsEntity.Married                       = ConvertStringToBool(faappAppsDataContract.FaaaMarried);
            aidApplicationsEntity.GradOrProfProgram             = ConvertStringToBool(faappAppsDataContract.FaaaGradProf);
            aidApplicationsEntity.ActiveDuty                    = ConvertStringToBool(faappAppsDataContract.FaaaActiveDuty);
            aidApplicationsEntity.UsVeteran                     = ConvertStringToBool(faappAppsDataContract.FaaaVeteran) ;
            aidApplicationsEntity.DependentChildren             = ConvertStringToBool(faappAppsDataContract.FaaaDependChildren) ;
            aidApplicationsEntity.OtherDependents               = ConvertStringToBool(faappAppsDataContract.FaaaOtherDepend) ;
            aidApplicationsEntity.OrphanWardFoster              = ConvertStringToBool(faappAppsDataContract.FaaaOrphanWard) ;
            aidApplicationsEntity.EmancipatedMinor              = ConvertStringToBool(faappAppsDataContract.FaaaEmancipatedMinor) ;
            aidApplicationsEntity.LegalGuardianship             = ConvertStringToBool(faappAppsDataContract.FaaaLegalGuardianship) ;
            aidApplicationsEntity.HomelessBySchool              = ConvertStringToBool(faappAppsDataContract.FaaaHomelessBySchool) ;
            aidApplicationsEntity.HomelessByHud                 = ConvertStringToBool(faappAppsDataContract.FaaaHomelessByHud) ;
            aidApplicationsEntity.HomelessAtRisk                = ConvertStringToBool(faappAppsDataContract.FaaaHomelessAtRisk);
            aidApplicationsEntity.StudentNumberInFamily         = faappAppsDataContract.FaaaSNbrFamily;
            aidApplicationsEntity.StudentNumberInCollege        = faappAppsDataContract.FaaaSNbrCollege;
            aidApplicationsEntity.SchoolCode1                   = faappAppsDataContract.FaaaHousing1;
            aidApplicationsEntity.HousingPlan1                  = faappAppsDataContract.FaaaHousing1Plan;
            
            aidApplicationsEntity.SchoolCode2                   = faappAppsDataContract.FaaaHousing2;
            aidApplicationsEntity.HousingPlan2                  = faappAppsDataContract.FaaaHousing2Plan;
            
            aidApplicationsEntity.SchoolCode3                   = faappAppsDataContract.FaaaHousing3;
            aidApplicationsEntity.HousingPlan3                  = faappAppsDataContract.FaaaHousing3Plan;
            
            aidApplicationsEntity.SchoolCode4                   = faappAppsDataContract.FaaaHousing4;
            aidApplicationsEntity.HousingPlan4                  = faappAppsDataContract.FaaaHousing4Plan;
            
            aidApplicationsEntity.SchoolCode5                   = faappAppsDataContract.FaaaHousing5;
            aidApplicationsEntity.HousingPlan5                  = faappAppsDataContract.FaaaHousing5Plan;
            
            aidApplicationsEntity.SchoolCode6                   = faappAppsDataContract.FaaaHousing6;
            aidApplicationsEntity.HousingPlan6                  = faappAppsDataContract.FaaaHousing6Plan;
            
            aidApplicationsEntity.SchoolCode7                   = faappAppsDataContract.FaaaHousing7;
            aidApplicationsEntity.HousingPlan7                  = faappAppsDataContract.FaaaHousing7Plan;
           
            aidApplicationsEntity.SchoolCode8                   = faappAppsDataContract.FaaaHousing8;
            aidApplicationsEntity.HousingPlan8                  = faappAppsDataContract.FaaaHousing8Plan;
            
            aidApplicationsEntity.SchoolCode9                   = faappAppsDataContract.FaaaHousing9;
            aidApplicationsEntity.HousingPlan9                  = faappAppsDataContract.FaaaHousing9Plan;
            
            aidApplicationsEntity.SchoolCode10                  = faappAppsDataContract.FaaaHousing10;
            aidApplicationsEntity.HousingPlan10                 = faappAppsDataContract.FaaaHousing10Plan;
            

            aidApplicationsEntity.ApplicationCompleteDate       = faappAppsDataContract.FaaaDateCmpl;
            aidApplicationsEntity.SignedFlag                    = faappAppsDataContract.FaaaSignedFlag;
            aidApplicationsEntity.PreparerSsn                   = faappAppsDataContract.FaaaPreparerSsn;
            aidApplicationsEntity.PreparerEin                   = faappAppsDataContract.FaaaPreparerEin;
            aidApplicationsEntity.PreparerSigned                = faappAppsDataContract.FaaaPreparerSigned;

            return aidApplicationsEntity;
        }
        #endregion

        private static bool? ConvertStringToBool(string inputValue)
        {
            bool? output = null;
            if(!string.IsNullOrEmpty(inputValue))
            {
                if(inputValue.ToUpper() == "Y")
                    output = true;
                else
                    output = false;
            }
            return output;
        }
        private static int? ConvertStringToInt(string input)
        {
            int? output = null;

            if (!string.IsNullOrEmpty(input))
            {
                // output = int.ToInt32(input);
                int varOutput;
                if (int.TryParse(input, out varOutput))
                {
                    output = varOutput;
                }
            }
            return output;
        }


        #region Post AidApplicationsAsync
        /// <summary>
        /// Post request - Create Aid Applications record
        /// </summary>
        /// <param name="aidApplicationsEntity"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<AidApplications> CreateAidApplicationsAsync(AidApplications aidApplicationsEntity)
        {
            if (aidApplicationsEntity == null)
                throw new ArgumentNullException("aidApplicationsEntity", "Must provide a aidApplications Entity to create.");
            var repositoryException = new RepositoryException();
            AidApplications createdEntity = null;
            UpdateAidApplRequest createRequest;
            try
            {
                createRequest = await BuildUpdateAidApplRequest(aidApplicationsEntity);

            }
            catch (Exception ex)
            {
                repositoryException.AddError(
                    new RepositoryError("Bad.Data", ex.Message)
                    {
                        Id = aidApplicationsEntity.Id,
                        SourceId = aidApplicationsEntity.Id
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

                var createResponse = await transactionInvoker.ExecuteAsync<UpdateAidApplRequest, UpdateAidApplResponse>(createRequest);

                if (createResponse != null && createResponse.ErrorMessage != null && createResponse.ErrorMessage.Any())
                {
                    for(int i =0; i< createResponse.ErrorMessage.Count; i++)
                    {
                        repositoryException.AddError(new RepositoryError("Create.Update.Exception", string.Concat(createResponse.ErrorCode[i], " - ", createResponse.ErrorMessage[i]))
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
                        createdEntity = await GetAidApplicationsByIdAsync(createRequest.IdemId);
                    }
                    catch (Exception ex)
                    {
                        repositoryException.AddError(
                       new RepositoryError("Bad.Data", ex.Message)
                       {
                           Id = aidApplicationsEntity.Id,
                           SourceId = aidApplicationsEntity.Id
                       });
                        throw repositoryException;
                    }
                }
            }
            // get the newly created record from the database
            return createdEntity;
        }
        #endregion


        #region Put AidApplicationsAsync
        /// <summary>
        /// Update an existing AidApplications domain entity
        /// </summary>
        /// <param name="aidApplicationsEntity">AidApplications domain entity</param>
        /// <returns>AidApplications domain entity</returns>
        public async Task<AidApplications> UpdateAidApplicationsAsync(AidApplications aidApplicationsEntity)
        {
            if (aidApplicationsEntity == null)
                throw new ArgumentNullException("aidApplicationsEntity", "Must provide a aidApplicationsEntity to update.");
            if (string.IsNullOrWhiteSpace(aidApplicationsEntity.Id))
                throw new ArgumentNullException("aidApplicationsEntity", "Must provide the id of the aidApplicationsEntity to update.");

            var repositoryException = new RepositoryException();
            AidApplications updatedEntity = null;
            UpdateAidApplRequest updateRequest;
            try
            {
                updateRequest = await BuildUpdateAidApplRequest(aidApplicationsEntity);
                updateRequest.ApplId = aidApplicationsEntity.Id;
            }
            catch (Exception ex)
            {
                repositoryException.AddError(
                    new RepositoryError("Bad.Data", ex.Message)
                    {
                        Id = aidApplicationsEntity.Id,
                        SourceId = aidApplicationsEntity.Id
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

                // write the  data
                var updateResponse = await transactionInvoker.ExecuteAsync<UpdateAidApplRequest, UpdateAidApplResponse>(updateRequest);

                if (updateResponse != null && updateResponse.ErrorMessage.Any())
                {
                    for (int i = 0; i < updateResponse.ErrorMessage.Count; i++)
                    {

                        repositoryException.AddError(new RepositoryError("Create.Update.Exception", string.Concat(updateResponse.ErrorCode[i], " - ", updateResponse.ErrorMessage[i]))
                        {
                            SourceId = updateResponse.ApplId,
                            Id = updateResponse.ApplId

                        });
                    }
                    throw repositoryException;
                }
                else
                {
                    try
                    {
                        updatedEntity = await GetAidApplicationsByIdAsync(updateResponse.ApplId);
                    }
                    catch (Exception ex)
                    {
                        repositoryException.AddError(
                       new RepositoryError("Bad.Data", ex.Message)
                       {
                           Id = aidApplicationsEntity.Id,
                           SourceId = aidApplicationsEntity.Id
                       });
                        throw repositoryException;
                    }
                }
            }
            // get the updated entity from the database
            return updatedEntity;
        }
        #endregion


        #region BuildUpdateAidApplrequest (entity to Transaction objects)
        /// <summary>
        /// Create an UpdateAidApplRequest from a AidApplications domain entity
        /// </summary>
        /// <param name="aidApplicationsEntity">AidApplications domain entity</param>
        /// <returns>UpdateAidApplRequest transaction object</returns>
        private async Task<UpdateAidApplRequest> BuildUpdateAidApplRequest(AidApplications aidApplicationsEntity)
        {
            var request = new UpdateAidApplRequest();

            var personRecord = await DataReader.ReadRecordAsync<Base.DataContracts.Person>(aidApplicationsEntity.PersonId);
            if (personRecord == null)
            {
                throw new ArgumentNullException(string.Format("person record not found with id {0}.", aidApplicationsEntity.PersonId));
            }
            if (aidApplicationsEntity.Id != "New")
            {
                request.ApplId = aidApplicationsEntity.Id;
            }
            request.IdemId = aidApplicationsEntity.AppDemoId;
            request.Year = aidApplicationsEntity.AidYear;
            request.Type = aidApplicationsEntity.AidApplicationType;
            request.StudentId = aidApplicationsEntity.PersonId;

            request.SMaritalStatus = aidApplicationsEntity.StudentMaritalStatus;
            request.SMaritalDate = aidApplicationsEntity.StudentMaritalDate;
            request.SLegalResSt = aidApplicationsEntity.StudentLegalResSt;
            request.SLegalResB4 = ConvertBoolToString(aidApplicationsEntity.StudentLegalResB4);
            request.SLegalResDate = aidApplicationsEntity.StudentLegalResDate;
            request.DegreeBy = ConvertBoolToString(aidApplicationsEntity.DegreeBy);
            request.P1FirstInit = aidApplicationsEntity.P1FirstInit;
            request.P1GradeLvl = aidApplicationsEntity.P1GradeLvl;
            request.P1LastName = aidApplicationsEntity.P1LastName;
            request.P1Ssn = aidApplicationsEntity.P1Ssn;
            if (aidApplicationsEntity.P1Dob.HasValue)
            {
                request.P1Dob = DateTime.SpecifyKind(aidApplicationsEntity.P1Dob.Value, DateTimeKind.Unspecified);
            }
            request.P2FirstInit = aidApplicationsEntity.P2FirstInit;
            request.P2GradeLvl = aidApplicationsEntity.P2GradeLvl;
            request.P2LastName = aidApplicationsEntity.P2LastName;
            request.P2Ssn = aidApplicationsEntity.P2Ssn;
            if (aidApplicationsEntity.P2Dob.HasValue)
            {
                request.P2Dob = DateTime.SpecifyKind(aidApplicationsEntity.P2Dob.Value, DateTimeKind.Unspecified);
            }
            request.PMaritalStatus = aidApplicationsEntity.PMaritalStatus;
            request.PMaritalDate = aidApplicationsEntity.PMaritalDate;
            request.ParentEmail = aidApplicationsEntity.ParentEmail;
            request.PLegalResB4 = ConvertBoolToString(aidApplicationsEntity.PLegalResB4);
            request.PLegalResDate = aidApplicationsEntity.PLegalResDate;
            request.PLegalResSt = aidApplicationsEntity.PLegalResSt;
            request.PNbrCollege = aidApplicationsEntity.PNbrCollege;
            request.PNbrFamily = aidApplicationsEntity.PNbrFamily;
            request.PSsiBen = ConvertBoolToString(aidApplicationsEntity.PSsiBen);
            request.PFoodStamps = ConvertBoolToString(aidApplicationsEntity.PFoodStamps);
            request.PLunchBen = ConvertBoolToString(aidApplicationsEntity.PLunchBen);
            request.PTanf = ConvertBoolToString(aidApplicationsEntity.PTanf);
            request.PWic = ConvertBoolToString(aidApplicationsEntity.PWic);
            request.PTaxFilingStatus = aidApplicationsEntity.PTaxFilingStatus;
            request.PTaxFormType = aidApplicationsEntity.PTaxFormType;
            request.PTaxReturnFiled = aidApplicationsEntity.PTaxReturnFiled;
            request.PSched1 = aidApplicationsEntity.PSched1;
            request.PDisWorker = aidApplicationsEntity.PDisWorker;
            request.PAgi = aidApplicationsEntity.PAgi;
            request.PUsTaxPaid = aidApplicationsEntity.PUsTaxPaid;
            request.P1Income = aidApplicationsEntity.P1Income;
            request.P2Income = aidApplicationsEntity.P2Income;
            request.PCash = aidApplicationsEntity.PCash;
            request.PInvNetWorth = aidApplicationsEntity.PInvNetWorth;
            request.PBusNetWorth = aidApplicationsEntity.PBusNetWorth;
            request.PEduCredit = aidApplicationsEntity.PEduCredit;
            request.PChildSupportPd = aidApplicationsEntity.PChildSupportPd;
            request.PNeedBasedEmp = aidApplicationsEntity.PNeedBasedEmp;
            request.PGrantScholAid = aidApplicationsEntity.PGrantScholAid;
            request.PCombatPay = aidApplicationsEntity.PCombatPay;
            request.PCoOpEarnings = aidApplicationsEntity.PCoOpEarnings;
            request.PPensionPymts = aidApplicationsEntity.PPensionPymts;
            request.PIraPymts = aidApplicationsEntity.PIraPymts;
            request.PChildSupRcvd = aidApplicationsEntity.PChildSupRcvd;
            request.PUntxIntInc = aidApplicationsEntity.PUntxIntInc;
            request.PUntxIraPen = aidApplicationsEntity.PUntxIraPen;
            request.PMilClerAllow = aidApplicationsEntity.PMilClerAllow;
            request.PVetNonEdBen = aidApplicationsEntity.PVetNonEdBen;
            request.POtherUntxInc = aidApplicationsEntity.POtherUntxInc;
            request.HsGradType = aidApplicationsEntity.HsGradType;
            request.HsName = aidApplicationsEntity.HsName;
            request.HsCity = aidApplicationsEntity.HsCity;
            request.HsState = aidApplicationsEntity.HsState;
            request.HsCode = aidApplicationsEntity.HsCode;

            request.DegreeBy = ConvertBoolToString(aidApplicationsEntity.DegreeBy);
            request.GradeLevel = aidApplicationsEntity.GradeLevelInCollege;
            request.DegOrCert = aidApplicationsEntity.DegreeOrCertificate;
            request.STaxReturnFiled = aidApplicationsEntity.StudentTaxReturnFiled;
            request.STaxFormType = aidApplicationsEntity.StudentTaxFormType;
            request.STaxFilingStatus = aidApplicationsEntity.StudentTaxFilingStatus;
            request.SSched1 = aidApplicationsEntity.StudentSched1;
            request.SAgi = aidApplicationsEntity.StudentAgi;
            request.SUsTaxPd = aidApplicationsEntity.StudentUsTaxPd;
            request.SStudentInc = aidApplicationsEntity.SStudentInc;
            request.SpouseInc = aidApplicationsEntity.SpouseInc;
            request.SCash = aidApplicationsEntity.StudentCash;
            request.SInvNetWorth = aidApplicationsEntity.StudentInvNetWorth;
            request.SBusNetWorth = aidApplicationsEntity.StudentBusNetWorth;
            request.SEduCredit = aidApplicationsEntity.StudentEduCredit;
            request.SChildSupportPd = aidApplicationsEntity.StudentChildSupPaid;
            request.SNeedBasedEmp = aidApplicationsEntity.StudentNeedBasedEmp;
            request.SGrantScholAid = aidApplicationsEntity.StudentGrantScholAid;
            request.SCombatPay = aidApplicationsEntity.StudentCombatPay;
            request.SCoOpEarnings = aidApplicationsEntity.StudentCoOpEarnings;
            request.SPensionPayments = aidApplicationsEntity.StudentPensionPayments;
            request.SIraPayments = aidApplicationsEntity.StudentIraPayments;
            request.SInterestIncome = aidApplicationsEntity.StudentInterestIncome;
            request.SChildSupRecv = aidApplicationsEntity.StudentChildSupRecv;
            request.SOtherUntaxedInc = aidApplicationsEntity.StudentOtherUntaxedInc;
            request.SUntxIraPen = aidApplicationsEntity.StudentUntxIraPen;
            request.SMilitaryClergyAllow = aidApplicationsEntity.StudentMilitaryClergyAllow;
            request.SVetNonEdBen = aidApplicationsEntity.StudentVetNonEdBen;
            request.SOtherNonRepMoney = aidApplicationsEntity.StudentOtherNonRepMoney;
            request.SSsiBen = ConvertBoolToString(aidApplicationsEntity.SSsiBen);
            request.SFoodStamps = ConvertBoolToString(aidApplicationsEntity.SFoodStamps);
            request.SLunchBen = ConvertBoolToString(aidApplicationsEntity.SLunchBen);
            request.STanf = ConvertBoolToString(aidApplicationsEntity.STanf);
            request.SWic = ConvertBoolToString(aidApplicationsEntity.SWic);
            request.SDislWorker = aidApplicationsEntity.SDislWorker;

            request.BornB4Dt = ConvertBoolToString(aidApplicationsEntity.BornBefore);
            request.Married = ConvertBoolToString(aidApplicationsEntity.Married);
            request.GradProf = ConvertBoolToString(aidApplicationsEntity.GradOrProfProgram);
            request.ActiveDuty = ConvertBoolToString(aidApplicationsEntity.ActiveDuty);
            request.Veteran = ConvertBoolToString(aidApplicationsEntity.UsVeteran);
            request.DependChildren = ConvertBoolToString(aidApplicationsEntity.DependentChildren);
            request.OtherDepend = ConvertBoolToString(aidApplicationsEntity.OtherDependents);
            request.OrphanWard = ConvertBoolToString(aidApplicationsEntity.OrphanWardFoster);
            request.EmancipatedMinor = ConvertBoolToString(aidApplicationsEntity.EmancipatedMinor);
            request.LegalGuardianship = ConvertBoolToString(aidApplicationsEntity.LegalGuardianship);
            request.HomelessBySchool = ConvertBoolToString(aidApplicationsEntity.HomelessBySchool);
            request.HomelessByHud = ConvertBoolToString(aidApplicationsEntity.HomelessByHud);
            request.HomelessAtRisk = ConvertBoolToString(aidApplicationsEntity.HomelessAtRisk);
            request.SNbrCollege = aidApplicationsEntity.StudentNumberInCollege;
            request.SNbrFamily = aidApplicationsEntity.StudentNumberInFamily;
            request.Housing1 = aidApplicationsEntity.SchoolCode1;
            request.Housing1Plan = aidApplicationsEntity.HousingPlan1;
            request.Housing2 = aidApplicationsEntity.SchoolCode2;
            request.Housing2Plan = aidApplicationsEntity.HousingPlan2;
            request.Housing3 = aidApplicationsEntity.SchoolCode3;
            request.Housing3Plan = aidApplicationsEntity.HousingPlan3;
            request.Housing4 = aidApplicationsEntity.SchoolCode4;
            request.Housing4Plan = aidApplicationsEntity.HousingPlan4;
            request.Housing5 = aidApplicationsEntity.SchoolCode5;
            request.Housing5Plan = aidApplicationsEntity.HousingPlan5;
            request.Housing6 = aidApplicationsEntity.SchoolCode6;
            request.Housing6Plan = aidApplicationsEntity.HousingPlan6;
            request.Housing7 = aidApplicationsEntity.SchoolCode7;
            request.Housing7Plan = aidApplicationsEntity.HousingPlan7;
            request.Housing8 = aidApplicationsEntity.SchoolCode8;
            request.Housing8Plan = aidApplicationsEntity.HousingPlan8;
            request.Housing9 = aidApplicationsEntity.SchoolCode9;
            request.Housing9Plan = aidApplicationsEntity.HousingPlan9;
            request.Housing10 = aidApplicationsEntity.SchoolCode10;
            request.Housing10Plan = aidApplicationsEntity.HousingPlan10;

            if (aidApplicationsEntity.ApplicationCompleteDate.HasValue)
            {
                request.DateCmpl = DateTime.SpecifyKind(aidApplicationsEntity.ApplicationCompleteDate.Value, DateTimeKind.Unspecified);
            }
            request.SignedFlag = aidApplicationsEntity.SignedFlag;
            request.PreparerSsn = aidApplicationsEntity.PreparerSsn;
            request.PreparerEin = aidApplicationsEntity.PreparerEin;
            request.PreparerSigned = aidApplicationsEntity.PreparerSigned;

            return request;
        }
        #endregion

        private string ConvertBoolToString(bool? input)
        {
            string output = string.Empty;
            if (input.HasValue)
                output = input.Value ? "Y" : "N";

            return output;
        }

    }
}
