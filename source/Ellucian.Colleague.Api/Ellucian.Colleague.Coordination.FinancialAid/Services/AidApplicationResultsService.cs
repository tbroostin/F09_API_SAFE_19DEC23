/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// AidApplicationResultsService class coordinates domain entities to interact with Financial Aid Application Results. 
    /// </summary>
    [RegisterType]
    public class AidApplicationResultsService : BaseCoordinationService, IAidApplicationResultsService
    {
        private readonly IAidApplicationResultsRepository _aidApplicationResultsRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IFinancialAidReferenceDataRepository _financialAidReferenceDataRepository;
        private readonly IAidApplicationDemographicsRepository _aidApplicationsDemoRepository;

        /// <summary>
        /// Constructor for AidApplicationResultsService
        /// </summary>
        /// <param name="aidApplicationResultsRepository"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        /// <param name="configurationRepository"></param>
        public AidApplicationResultsService(IAidApplicationResultsRepository aidApplicationResultsRepository,
            IStudentReferenceDataRepository studentReferenceDataRepository,
            IAidApplicationDemographicsRepository aidApplicationsDemoRepository,
            IFinancialAidReferenceDataRepository financialAidReferenceDataRepository,
            IAdapterRegistry adapterRegistry,
           ICurrentUserFactory currentUserFactory,
           IRoleRepository roleRepository,
           ILogger logger,
           IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _aidApplicationResultsRepository = aidApplicationResultsRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _financialAidReferenceDataRepository = financialAidReferenceDataRepository;
            _aidApplicationsDemoRepository = aidApplicationsDemoRepository;
        }

        /// <summary>
        /// Gets all aidApplicationResults matching the criteria
        /// </summary>
        /// <returns>Collection of AidApplicationResults DTO objects</returns>
        public async Task<Tuple<IEnumerable<AidApplicationResults>, int>> GetAidApplicationResultsAsync(int offset, int limit, AidApplicationResults criteriaFilter)
        {
            var aidApplicationResultsDtos = new List<AidApplicationResults>();

            if (criteriaFilter == null)
            {
                criteriaFilter = new AidApplicationResults();
            }
            string appDemoId = criteriaFilter.AppDemoId;
            string personIdCriteria = criteriaFilter.PersonId;
            string aidApplicationType = criteriaFilter.ApplicationType;
            string aidYear = criteriaFilter.AidYear;
            int? transactionNumber = criteriaFilter.TransactionNumber;
            string applicantAssignedId = criteriaFilter.ApplicantAssignedId;

            var aidApplicationResultsEntities = await _aidApplicationResultsRepository.GetAidApplicationResultsAsync(offset, limit, appDemoId, personIdCriteria, aidApplicationType, aidYear, transactionNumber,applicantAssignedId);
            int totalRecords = 0;
            if(aidApplicationResultsEntities != null)
            {
                totalRecords = aidApplicationResultsEntities.Item2;
                if (aidApplicationResultsEntities != null && aidApplicationResultsEntities.Item1.Any())
                {
                    foreach (var results in aidApplicationResultsEntities.Item1)
                    {
                        aidApplicationResultsDtos.Add(ConvertAidApplicationResultsEntityToDto(results, false));
                    }
                }
            }            
            return new Tuple<IEnumerable<AidApplicationResults>, int>(aidApplicationResultsDtos, totalRecords);
        }

        /// <summary>
        /// Get a AidApplicationResults from its Id
        /// </summary>
        /// <returns>AidApplicationResults DTO object</returns>
        public async Task<AidApplicationResults> GetAidApplicationResultsByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("Id required to get aid application results");
            }

            Domain.FinancialAid.Entities.AidApplicationResults aidApplicationResultsEntity;
            try
            {
                aidApplicationResultsEntity = await _aidApplicationResultsRepository.GetAidApplicationResultsByIdAsync(id);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }

            if (aidApplicationResultsEntity == null)
            {
                throw new KeyNotFoundException("No Aid application results was found for ID " + id);
            }

            return ConvertAidApplicationResultsEntityToDto(aidApplicationResultsEntity, true);
        }
               

        /// <summary>
        /// Create a new AidApplicationResults record
        /// </summary>
        /// <param name="aidApplicationResultsDto">aidApplicationResults DTO</param>
        /// <returns>AidApplicationResults domain entity</returns>
        public async Task<AidApplicationResults> PostAidApplicationResultsAsync(AidApplicationResults aidApplicationResultsDto)
        {
            _aidApplicationResultsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            Domain.FinancialAid.Entities.AidApplicationResults createdAidApplicationResults = null;
            Domain.FinancialAid.Entities.AidApplicationResults aidApplicationResults = null;
            try
            {
                ValidateAidApplicationResults(aidApplicationResultsDto);
                aidApplicationResultsDto.Id = "new";
                aidApplicationResults = await ConvertAidApplicationResultsDtoToEntity(aidApplicationResultsDto);
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("Record not created. Error extracting request. " + ex.Message, "Global.Internal.Error",
                   aidApplicationResults != null && !string.IsNullOrEmpty(aidApplicationResults.AppDemoId) ? aidApplicationResults.AppDemoId : null);
            }
            if (IntegrationApiException != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
            try
            {
                // create a AidApplicationResults entity in the database
                createdAidApplicationResults = await _aidApplicationResultsRepository.CreateAidApplicationResultsAsync(aidApplicationResults);

            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Global.Internal.Error",
                    aidApplicationResults != null && !string.IsNullOrEmpty(aidApplicationResults.AppDemoId) ? aidApplicationResults.AppDemoId : null);
            }
            if (IntegrationApiException != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
            if (createdAidApplicationResults != null)
            {
                return ConvertAidApplicationResultsEntityToDto(createdAidApplicationResults, true);
            }
            else
            {
                IntegrationApiExceptionAddError("applicationResultsEntity entity cannot be null");
            }
            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
            return null;


        }

        /// <summary>
        /// Update an existing AidApplicationResults record
        /// </summary>
        /// <param name="aidApplicationResultsDto">AidApplicationResults DTO</param>
        /// <returns>AidApplicationResults domain entity</returns>
        public async Task<AidApplicationResults> PutAidApplicationResultsAsync(string id, AidApplicationResults aidApplicationResultsDto)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new KeyNotFoundException("id required to update aid application results");
            }


            _aidApplicationResultsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
            Domain.FinancialAid.Entities.AidApplicationResults aidApplicationResults = null;
            try
            {
                
                ValidateAidApplicationResults(aidApplicationResultsDto);
                aidApplicationResults = await ConvertAidApplicationResultsDtoToEntity(aidApplicationResultsDto, true);
            }
            catch (IntegrationApiException ex )
            {
                throw ex;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("Record not updated. Error extracting request. " + ex.Message, "Global.Internal.Error",
                   aidApplicationResults != null && !string.IsNullOrEmpty(aidApplicationResults.AppDemoId) ? aidApplicationResults.AppDemoId : null);
            }
            Domain.FinancialAid.Entities.AidApplicationResults updatedAidApplicationResults = null;

            try
            {
                // create a AidApplicationResults entity in the database
                updatedAidApplicationResults = await _aidApplicationResultsRepository.UpdateAidApplicationResultsAsync(aidApplicationResults);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Global.Internal.Error",
                    aidApplicationResults != null && !string.IsNullOrEmpty(aidApplicationResults.AppDemoId) ? aidApplicationResults.AppDemoId : null);

            }
            if (IntegrationApiException != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            if (updatedAidApplicationResults != null)
                // the newly created AidApplicationResults Dto
                {
                    return ConvertAidApplicationResultsEntityToDto(updatedAidApplicationResults, true);
                }
            else
                {
                    IntegrationApiExceptionAddError("applicationResultsEntity entity cannot be null");
                }
                if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
                {
                    throw IntegrationApiException;
                }
                return null;

            }

        /// <summary>
        /// Converts a AidApplicationResults domain entity to its corresponding AidApplicationResults DTO
        /// </summary>
        /// <returns>AidApplicationResults DTO</returns>
        private AidApplicationResults ConvertAidApplicationResultsEntityToDto(Domain.FinancialAid.Entities.AidApplicationResults aidApplicationResultsEntity, bool isGetByID)
        {
            var aidApplicationResultsDto = new Dtos.FinancialAid.AidApplicationResults();

            aidApplicationResultsDto.Id = aidApplicationResultsEntity.Id;
            aidApplicationResultsDto.AppDemoId = aidApplicationResultsEntity.AppDemoId;
            aidApplicationResultsDto.PersonId = aidApplicationResultsEntity.PersonId;
            aidApplicationResultsDto.ApplicationType = aidApplicationResultsEntity.AidApplicationType;
            aidApplicationResultsDto.AidYear = aidApplicationResultsEntity.AidYear;
            aidApplicationResultsDto.ApplicantAssignedId = string.IsNullOrEmpty(aidApplicationResultsEntity.ApplicantAssignedId) ? null : aidApplicationResultsEntity.ApplicantAssignedId;
            aidApplicationResultsDto.TransactionNumber = aidApplicationResultsEntity.TransactionNumber;
            aidApplicationResultsDto.DependencyOverride = string.IsNullOrEmpty(aidApplicationResultsEntity.DependencyOverride) ? null : aidApplicationResultsEntity.DependencyOverride;
            aidApplicationResultsDto.DependencyOverSchoolCode = string.IsNullOrEmpty(aidApplicationResultsEntity.DependencyOverSchoolCode) ? null : aidApplicationResultsEntity.DependencyOverSchoolCode;
            aidApplicationResultsDto.DependencyStatus = string.IsNullOrEmpty(aidApplicationResultsEntity.DependencyStatus) ? null : aidApplicationResultsEntity.DependencyStatus;
            aidApplicationResultsDto.TransactionSource = string.IsNullOrEmpty(aidApplicationResultsEntity.TransactionSource) ? null : aidApplicationResultsEntity.TransactionSource;
            aidApplicationResultsDto.TransactionReceiptDate = aidApplicationResultsEntity.TransactionReceiptDate;
            aidApplicationResultsDto.SpecialCircumstances = string.IsNullOrEmpty(aidApplicationResultsEntity.SpecialCircumstances) ? null : aidApplicationResultsEntity.SpecialCircumstances;
            aidApplicationResultsDto.ParentAssetExceeded = aidApplicationResultsEntity.ParentAssetExceeded;
            aidApplicationResultsDto.StudentAssetExceeded = aidApplicationResultsEntity.StudentAssetExceeded;
            aidApplicationResultsDto.DestinationNumber = string.IsNullOrEmpty(aidApplicationResultsEntity.DestinationNumber) ? null : aidApplicationResultsEntity.DestinationNumber;
            aidApplicationResultsDto.StudentCurrentPseudoId = string.IsNullOrEmpty(aidApplicationResultsEntity.StudentCurrentPseudoId) ? null : aidApplicationResultsEntity.StudentCurrentPseudoId;
            aidApplicationResultsDto.CorrectionAppliedAgainst = string.IsNullOrEmpty(aidApplicationResultsEntity.CorrectionAppliedAgainst) ? null : aidApplicationResultsEntity.CorrectionAppliedAgainst;

            aidApplicationResultsDto.ProfJudgementIndicator = string.IsNullOrEmpty(aidApplicationResultsEntity.ProfJudgementIndicator) ? null : ConvertJudgementIndicatorToDto(aidApplicationResultsEntity.ProfJudgementIndicator, isGetByID);

            aidApplicationResultsDto.ApplicationDataSource = string.IsNullOrEmpty(aidApplicationResultsEntity.ApplicationDataSource) ? null : aidApplicationResultsEntity.ApplicationDataSource;
            aidApplicationResultsDto.ApplicationReceiptDate = aidApplicationResultsEntity.ApplicationReceiptDate;
            aidApplicationResultsDto.AddressOnlyChangeFlag = string.IsNullOrEmpty(aidApplicationResultsEntity.AddressOnlyChangeFlag) ? null : aidApplicationResultsEntity.AddressOnlyChangeFlag;
            aidApplicationResultsDto.PushedApplicationFlag = aidApplicationResultsEntity.PushedApplicationFlag;

            aidApplicationResultsDto.EfcChangeFlag = string.IsNullOrEmpty(aidApplicationResultsEntity.EfcChangeFlag) ? null : ConvertEfcChangeFlagToDto(aidApplicationResultsEntity.EfcChangeFlag, isGetByID);

            aidApplicationResultsDto.LastNameChange = string.IsNullOrEmpty(aidApplicationResultsEntity.LastNameChange) ? null : ConvertLastNameChangeToDto(aidApplicationResultsEntity.LastNameChange, isGetByID);

            aidApplicationResultsDto.RejectStatusChange = aidApplicationResultsEntity.RejectStatusChange;
            aidApplicationResultsDto.SarcChange = aidApplicationResultsEntity.SarcChange;
            aidApplicationResultsDto.ComputeNumber = string.IsNullOrEmpty(aidApplicationResultsEntity.ComputeNumber) ? null : aidApplicationResultsEntity.ComputeNumber;
            aidApplicationResultsDto.CorrectionSource = string.IsNullOrEmpty(aidApplicationResultsEntity.CorrectionSource) ? null : aidApplicationResultsEntity.CorrectionSource;
            aidApplicationResultsDto.DuplicateIdIndicator = aidApplicationResultsEntity.DuplicateIdIndicator;
            aidApplicationResultsDto.GraduateFlag = aidApplicationResultsEntity.GraduateFlag;
            aidApplicationResultsDto.TransactionProcessedDate = aidApplicationResultsEntity.TransactionProcessedDate;
            aidApplicationResultsDto.ProcessedRecordType = string.IsNullOrEmpty(aidApplicationResultsEntity.ProcessedRecordType) ? null : aidApplicationResultsEntity.ProcessedRecordType;
            //list of string
            if (aidApplicationResultsEntity.RejectReasonCodes != null && aidApplicationResultsEntity.RejectReasonCodes.Any())
            {
                aidApplicationResultsDto.RejectReasonCodes = aidApplicationResultsEntity.RejectReasonCodes;
            }
            aidApplicationResultsDto.AutomaticZeroIndicator = aidApplicationResultsEntity.AutomaticZeroIndicator;

            aidApplicationResultsDto.SimplifiedNeedsTest = string.IsNullOrEmpty(aidApplicationResultsEntity.SimplifiedNeedsTest) ? null : ConvertSimplifiedNeedsTestToDto(aidApplicationResultsEntity.SimplifiedNeedsTest, isGetByID);

            aidApplicationResultsDto.ParentCalculatedTaxStatus = string.IsNullOrEmpty(aidApplicationResultsEntity.ParentCalculatedTaxStatus) ? null : aidApplicationResultsEntity.ParentCalculatedTaxStatus;
            aidApplicationResultsDto.StudentCalculatedTaxStatus = string.IsNullOrEmpty(aidApplicationResultsEntity.StudentCalculatedTaxStatus) ? null : aidApplicationResultsEntity.StudentCalculatedTaxStatus;
            aidApplicationResultsDto.StudentAddlFinCalcTotal = aidApplicationResultsEntity.StudentAddlFinCalcTotal;
            aidApplicationResultsDto.studentOthUntaxIncomeCalcTotal = aidApplicationResultsEntity.studentOthUntaxIncomeCalcTotal;
            aidApplicationResultsDto.ParentAddlFinCalcTotal = aidApplicationResultsEntity.ParentAddlFinCalcTotal;
            aidApplicationResultsDto.ParentOtherUntaxIncomeCalcTotal = aidApplicationResultsEntity.ParentOtherUntaxIncomeCalcTotal;
            aidApplicationResultsDto.InvalidHighSchool = aidApplicationResultsEntity.InvalidHighSchool;
            #region assumed
            aidApplicationResultsDto.Assumed = new AssumedStudentDetails()
            {
                Citizenship = string.IsNullOrEmpty(aidApplicationResultsEntity.AssumCitizenship) ? null : ConvertCitizenshipStatusToDto(aidApplicationResultsEntity.AssumCitizenship, isGetByID),
                StudentMaritalStatus = string.IsNullOrEmpty(aidApplicationResultsEntity.AssumSMarStat) ? null : ConvertStudentMaritalStatusToDto(aidApplicationResultsEntity.AssumSMarStat, isGetByID),
                StudentAgi = aidApplicationResultsEntity.AssumSAgi,
                StudentTaxPaid = aidApplicationResultsEntity.AssumSTaxPd,
                StudentWorkIncome = aidApplicationResultsEntity.AssumSIncWork,
                SpouseWorkIncome = aidApplicationResultsEntity.AssumSpIncWork,
                StudentAddlFinInfoTotal = aidApplicationResultsEntity.AssumSAddlFinAmt,
                BirthDatePrior = string.IsNullOrEmpty(aidApplicationResultsEntity.AssumBirthDatePrior) ? null : ConvertYesNoToDto(aidApplicationResultsEntity.AssumBirthDatePrior, isGetByID, "Birth Date Prior"),
                StudentMarried = string.IsNullOrEmpty(aidApplicationResultsEntity.AssumSMarried) ? null : ConvertYesNoToDto(aidApplicationResultsEntity.AssumSMarried, isGetByID, "Student Married"),
                DependentChildren = string.IsNullOrEmpty(aidApplicationResultsEntity.AssumChildren) ? null : ConvertYesNoToDto(aidApplicationResultsEntity.AssumChildren, isGetByID, "Dependent Children"),
                OtherDependents = string.IsNullOrEmpty(aidApplicationResultsEntity.AssumLegalDep) ? null : aidApplicationResultsEntity.AssumLegalDep,
                studentFamilySize = aidApplicationResultsEntity.AssumSNbrFamily,
                StudentNumberInCollege = aidApplicationResultsEntity.AssumSNbrCollege,
                StudentAssetThreshold = aidApplicationResultsEntity.AssumSAssetTholdExc,
                ParentMaritalStatus = string.IsNullOrEmpty(aidApplicationResultsEntity.AssumPMarStat) ? null : ConvertParentMaritalStatusToDto(aidApplicationResultsEntity.AssumPMarStat, isGetByID),                
                FirstParentSsn = aidApplicationResultsEntity.AssumPar1Ssn,
                SecondParentSsn = aidApplicationResultsEntity.AssumPar2Ssn,
                ParentFamilySize = aidApplicationResultsEntity.AssumPNbrFamily,
                ParentNumCollege = aidApplicationResultsEntity.AssumPNbrCollege,
                ParentAgi = aidApplicationResultsEntity.AssumPAgi,
                ParentTaxPaid = aidApplicationResultsEntity.AssumPTaxPd,
                FirstParentWorkIncome = aidApplicationResultsEntity.AssumPar1Income,
                SecondParentWorkIncome = aidApplicationResultsEntity.AssumPar2Income,
                ParentAddlFinancial = aidApplicationResultsEntity.AssumPAddlFinAmt,
                ParentAssetThreshold = aidApplicationResultsEntity.AssumPAssetTholdExc
            };

            // check if the assumed has any property
            var isObjectNotEmpty = aidApplicationResultsDto.Assumed.GetType()
                                                            .GetProperties()
                                                            .Select(p => p.GetValue(aidApplicationResultsDto.Assumed))
                                                            .Where(x => x != null || (x is string && (string)x != null))
                                                            .ToList();

            // if assumed is an empty object, then make it null 
            if (isObjectNotEmpty.Count == 0 && (aidApplicationResultsDto.Assumed.StudentAssetThreshold != true && aidApplicationResultsDto.Assumed.FirstParentSsn != true && aidApplicationResultsDto.Assumed.SecondParentSsn != true && aidApplicationResultsDto.Assumed.ParentAssetThreshold != true))
            {
                aidApplicationResultsDto.Assumed = null;
            }
            #endregion
            aidApplicationResultsDto.PrimaryEfc = aidApplicationResultsEntity.PrimaryEfc;
            aidApplicationResultsDto.SecondaryEfc = aidApplicationResultsEntity.SecondaryEfc;
            aidApplicationResultsDto.SignatureRejectEfc = aidApplicationResultsEntity.SignatureRejectEfc;
            aidApplicationResultsDto.PrimaryEfcType = string.IsNullOrEmpty(aidApplicationResultsEntity.PrimaryEfcType) ? null : aidApplicationResultsEntity.PrimaryEfcType;
            #region alternatePrimaryEfc
            aidApplicationResultsDto.AlternatePrimaryEfc = new AlternatePrimaryEfc()
            {
                OneMonth = aidApplicationResultsEntity.PriAlt1mnthEfc,
                TwoMonths = aidApplicationResultsEntity.PriAlt2mnthEfc,
                ThreeMonths = aidApplicationResultsEntity.PriAlt3mnthEfc,
                FourMonths = aidApplicationResultsEntity.PriAlt4mnthEfc,
                FiveMonths = aidApplicationResultsEntity.PriAlt5mnthEfc,
                SixMonths = aidApplicationResultsEntity.PriAlt6mnthEfc,
                SevenMonths = aidApplicationResultsEntity.PriAlt7mnthEfc,
                EightMonths = aidApplicationResultsEntity.PriAlt8mnthEfc,
                TenMonths = aidApplicationResultsEntity.PriAlt10mnthEfc,
                ElevenMonths = aidApplicationResultsEntity.PriAlt11mnthEfc,
                TwelveMonths = aidApplicationResultsEntity.PriAlt12mnthEfc
            };

            // check if the alternatePrimaryEfc has any property
            var isEfcNotEmpty = aidApplicationResultsDto.AlternatePrimaryEfc.GetType()
                                                            .GetProperties()
                                                            .Select(p => p.GetValue(aidApplicationResultsDto.AlternatePrimaryEfc))
                                                            .Where(x => x != null || (x is string && (string)x != null))
                                                            .ToList();

            // if assumed is an empty object, then make it null 
            if (isEfcNotEmpty.Count == 0)
            {
                aidApplicationResultsDto.AlternatePrimaryEfc = null;
            }
            #endregion
            aidApplicationResultsDto.TotalIncome = aidApplicationResultsEntity.TotalIncome;
            aidApplicationResultsDto.AllowancesAgainstTotalIncome = aidApplicationResultsEntity.AllowancesAgainstTotalIncome;
            aidApplicationResultsDto.TaxAllowance = aidApplicationResultsEntity.TaxAllowance;
            aidApplicationResultsDto.EmploymentAllowance = aidApplicationResultsEntity.EmploymentAllowance;
            aidApplicationResultsDto.IncomeProtectionAllowance = aidApplicationResultsEntity.IncomeProtectionAllowance;
            aidApplicationResultsDto.AvailableIncome = aidApplicationResultsEntity.AvailableIncome;
            aidApplicationResultsDto.AvailableIncomeContribution = aidApplicationResultsEntity.AvailableIncomeContribution;
            aidApplicationResultsDto.DiscretionaryNetWorth = aidApplicationResultsEntity.DiscretionaryNetWorth;
            aidApplicationResultsDto.NetWorth = aidApplicationResultsEntity.NetWorth;
            aidApplicationResultsDto.AssetProtectionAllowance = aidApplicationResultsEntity.AssetProtectionAllowance;
            aidApplicationResultsDto.ParentContributionAssets = aidApplicationResultsEntity.ParentContributionAssets;
            aidApplicationResultsDto.AdjustedAvailableIncome = aidApplicationResultsEntity.AdjustedAvailableIncome;
            aidApplicationResultsDto.TotalPrimaryStudentContribution = aidApplicationResultsEntity.TotalPrimaryStudentContribution;
            aidApplicationResultsDto.TotalPrimaryParentContribution = aidApplicationResultsEntity.TotalPrimaryParentContribution;
            aidApplicationResultsDto.ParentContribution = aidApplicationResultsEntity.ParentContribution;
            aidApplicationResultsDto.StudentTotalIncome = aidApplicationResultsEntity.StudentTotalIncome;
            aidApplicationResultsDto.StudentAllowanceAgainstIncome = aidApplicationResultsEntity.StudentAllowanceAgainstIncome;
            aidApplicationResultsDto.DependentStudentIncContrib = aidApplicationResultsEntity.DependentStudentIncContrib;
            aidApplicationResultsDto.StudentDiscretionaryNetWorth = aidApplicationResultsEntity.StudentDiscretionaryNetWorth;
            aidApplicationResultsDto.StudentAssetContribution = aidApplicationResultsEntity.StudentAssetContribution;
            aidApplicationResultsDto.FisapTotalIncome = aidApplicationResultsEntity.FisapTotalIncome;
            aidApplicationResultsDto.CorrectionFlags = string.IsNullOrEmpty(aidApplicationResultsEntity.CorrectionFlags) ? null : aidApplicationResultsEntity.CorrectionFlags;
            aidApplicationResultsDto.HighlightFlags = string.IsNullOrEmpty(aidApplicationResultsEntity.HighlightFlags) ? null : aidApplicationResultsEntity.HighlightFlags;
            //list of string
            if (aidApplicationResultsEntity.CommentCodes != null && aidApplicationResultsEntity.CommentCodes.Any())
            {
                aidApplicationResultsDto.CommentCodes = aidApplicationResultsEntity.CommentCodes;
            }
            aidApplicationResultsDto.ElectronicFedSchoolCodeInd = string.IsNullOrEmpty(aidApplicationResultsEntity.ElectronicFedSchoolCodeInd) ? null : aidApplicationResultsEntity.ElectronicFedSchoolCodeInd;
            aidApplicationResultsDto.ElectronicTransactionIndicator = string.IsNullOrEmpty(aidApplicationResultsEntity.ElectronicTransactionIndicator) ? null : aidApplicationResultsEntity.ElectronicTransactionIndicator;
            aidApplicationResultsDto.VerificationSelected = string.IsNullOrEmpty(aidApplicationResultsEntity.VerificationSelected) ? null : aidApplicationResultsEntity.VerificationSelected;

            return aidApplicationResultsDto;
        }

        private async Task<Domain.FinancialAid.Entities.AidApplicationResults> ConvertAidApplicationResultsDtoToEntity(AidApplicationResults aidApplicationResultsDto, bool updateRecord = false)
        {
            Domain.FinancialAid.Entities.AidApplicationDemographics aidApplicationDemographics;

            aidApplicationDemographics = await _aidApplicationsDemoRepository.GetAidApplicationDemographicsByIdAsync(aidApplicationResultsDto.AppDemoId);
            Domain.FinancialAid.Entities.AidApplicationResults aidApplicationResultsEntity = new Domain.FinancialAid.Entities.AidApplicationResults(aidApplicationResultsDto.Id, aidApplicationResultsDto.AppDemoId);

            var finAidYears = await _financialAidReferenceDataRepository.GetFinancialAidYearsAsync(false);

            if (updateRecord && aidApplicationResultsDto.Id != aidApplicationResultsDto.AppDemoId)
            {
                IntegrationApiExceptionAddError(string.Format("Value of id and appDemoId does not match"));
                return null;
            }

            if (aidApplicationResultsDto.AidYear != null && aidApplicationResultsDto.AidYear != aidApplicationDemographics.AidYear)
            {
                IntegrationApiExceptionAddError(string.Format("The aidYear associated with appDemoId does not match the input aidYear"));
            }
            if (aidApplicationResultsDto.PersonId != null && aidApplicationResultsDto.PersonId != aidApplicationDemographics.PersonId)
            {
                IntegrationApiExceptionAddError(string.Format("The personId associated with appDemoId does not match the input personId"));
            }
            if (aidApplicationResultsDto.ApplicationType != null && aidApplicationResultsDto.ApplicationType != aidApplicationDemographics.ApplicationType)
            {
                IntegrationApiExceptionAddError(string.Format("The applicationType associated with appDemoId does not match the input applicationType"));
            }
            if (aidApplicationResultsDto.ApplicantAssignedId != null && aidApplicationResultsDto.ApplicantAssignedId != aidApplicationDemographics.ApplicantAssignedId)
            {
                IntegrationApiExceptionAddError(string.Format("The applicantAssignedId associated with appDemoId does not match the input applicantAssignedId"));
            }
            aidApplicationResultsDto.AidYear = aidApplicationDemographics.AidYear;
            aidApplicationResultsDto.PersonId = aidApplicationDemographics.PersonId;
            aidApplicationResultsDto.ApplicationType = aidApplicationDemographics.ApplicationType;
            aidApplicationResultsDto.ApplicantAssignedId = aidApplicationDemographics.ApplicantAssignedId;
            if (finAidYears == null || !finAidYears.Any(x => x.Code.ToLower() == aidApplicationResultsDto.AidYear))
            {
                IntegrationApiExceptionAddError(string.Format("aid year {0} not found in FA.SUITES.YEAR", aidApplicationResultsDto.AidYear));
            }
            var aidApplicationTypes = await _studentReferenceDataRepository.GetAidApplicationTypesAsync();
            if (aidApplicationTypes == null || !aidApplicationTypes.Any(x => x.Code.ToLower() == aidApplicationResultsDto.ApplicationType.ToLower()))
            {
                IntegrationApiExceptionAddError(string.Format("application type {0} is not found in ST.VALCODES - FA.APPLN.TYPES", aidApplicationResultsDto.ApplicationType));
            }

            aidApplicationResultsEntity.PersonId = aidApplicationResultsDto.PersonId;
            aidApplicationResultsEntity.AidApplicationType = aidApplicationResultsDto.ApplicationType;
            aidApplicationResultsEntity.AidYear = aidApplicationResultsDto.AidYear;
            aidApplicationResultsEntity.ApplicantAssignedId = aidApplicationResultsDto.ApplicantAssignedId;
            aidApplicationResultsEntity.TransactionNumber = aidApplicationResultsDto.TransactionNumber;
            aidApplicationResultsEntity.DependencyOverride = aidApplicationResultsDto.DependencyOverride;
            aidApplicationResultsEntity.DependencyOverSchoolCode = aidApplicationResultsDto.DependencyOverSchoolCode;
            aidApplicationResultsEntity.DependencyStatus = aidApplicationResultsDto.DependencyStatus;
            aidApplicationResultsEntity.TransactionSource = aidApplicationResultsDto.TransactionSource;
            aidApplicationResultsEntity.TransactionReceiptDate = aidApplicationResultsDto.TransactionReceiptDate;
            aidApplicationResultsEntity.SpecialCircumstances = aidApplicationResultsDto.SpecialCircumstances;
            aidApplicationResultsEntity.ParentAssetExceeded = aidApplicationResultsDto.ParentAssetExceeded;
            aidApplicationResultsEntity.StudentAssetExceeded = aidApplicationResultsDto.StudentAssetExceeded;
            aidApplicationResultsEntity.DestinationNumber = aidApplicationResultsDto.DestinationNumber;
            aidApplicationResultsEntity.StudentCurrentPseudoId = aidApplicationResultsDto.StudentCurrentPseudoId;
            aidApplicationResultsEntity.CorrectionAppliedAgainst = aidApplicationResultsDto.CorrectionAppliedAgainst;
            //enum
            if (aidApplicationResultsDto.ProfJudgementIndicator.HasValue)
            {
                aidApplicationResultsEntity.ProfJudgementIndicator = aidApplicationResultsDto.ProfJudgementIndicator.HasValue ? ValidateEnumAndConvertToEntity(typeof(JudgementIndicator), aidApplicationResultsDto.ProfJudgementIndicator, "ProfJudgementIndicator") : null;
            }
            aidApplicationResultsEntity.ApplicationDataSource = aidApplicationResultsDto.ApplicationDataSource;
            aidApplicationResultsEntity.ApplicationReceiptDate = aidApplicationResultsDto.ApplicationReceiptDate;
            aidApplicationResultsEntity.AddressOnlyChangeFlag = aidApplicationResultsDto.AddressOnlyChangeFlag;
            aidApplicationResultsEntity.PushedApplicationFlag = aidApplicationResultsDto.PushedApplicationFlag;
            //enum
            if (aidApplicationResultsDto.EfcChangeFlag.HasValue)
            {
                aidApplicationResultsEntity.EfcChangeFlag = aidApplicationResultsDto.EfcChangeFlag.HasValue ? ValidateEnumAndConvertToEntity(typeof(EfcChangeFlag), aidApplicationResultsDto.EfcChangeFlag, "EfcChangeFlag") : null;
            }
            //enum
            if (aidApplicationResultsDto.LastNameChange.HasValue)
            {
                aidApplicationResultsEntity.LastNameChange = aidApplicationResultsDto.LastNameChange.HasValue ? ValidateEnumAndConvertToEntity(typeof(LastNameChange), aidApplicationResultsDto.LastNameChange, "LastNameChange") : null;
            }
            aidApplicationResultsEntity.RejectStatusChange = aidApplicationResultsDto.RejectStatusChange;
            aidApplicationResultsEntity.SarcChange = aidApplicationResultsDto.SarcChange;
            aidApplicationResultsEntity.ComputeNumber = aidApplicationResultsDto.ComputeNumber;
            aidApplicationResultsEntity.CorrectionSource = aidApplicationResultsDto.CorrectionSource;
            aidApplicationResultsEntity.DuplicateIdIndicator = aidApplicationResultsDto.DuplicateIdIndicator;
            aidApplicationResultsEntity.GraduateFlag = aidApplicationResultsDto.GraduateFlag;
            aidApplicationResultsEntity.TransactionProcessedDate = aidApplicationResultsDto.TransactionProcessedDate;
            aidApplicationResultsEntity.ProcessedRecordType = aidApplicationResultsDto.ProcessedRecordType;
            aidApplicationResultsEntity.RejectReasonCodes = aidApplicationResultsDto.RejectReasonCodes;
            aidApplicationResultsEntity.AutomaticZeroIndicator = aidApplicationResultsDto.AutomaticZeroIndicator;
            //enum
            if (aidApplicationResultsDto.SimplifiedNeedsTest.HasValue)
            {
                aidApplicationResultsEntity.SimplifiedNeedsTest = aidApplicationResultsDto.SimplifiedNeedsTest.HasValue ? ValidateEnumAndConvertToEntity(typeof(SimplifiedNeedsTest), aidApplicationResultsDto.SimplifiedNeedsTest, "SimplifiedNeedsTest") : null;
            }
            aidApplicationResultsEntity.ParentCalculatedTaxStatus = aidApplicationResultsDto.ParentCalculatedTaxStatus;
            aidApplicationResultsEntity.StudentCalculatedTaxStatus = aidApplicationResultsDto.StudentCalculatedTaxStatus;
            aidApplicationResultsEntity.StudentAddlFinCalcTotal = aidApplicationResultsDto.StudentAddlFinCalcTotal;
            aidApplicationResultsEntity.studentOthUntaxIncomeCalcTotal = aidApplicationResultsDto.studentOthUntaxIncomeCalcTotal;
            aidApplicationResultsEntity.ParentAddlFinCalcTotal = aidApplicationResultsDto.ParentAddlFinCalcTotal;
            aidApplicationResultsEntity.ParentOtherUntaxIncomeCalcTotal = aidApplicationResultsDto.ParentOtherUntaxIncomeCalcTotal;
            aidApplicationResultsEntity.InvalidHighSchool = aidApplicationResultsDto.InvalidHighSchool;
            if (aidApplicationResultsDto.Assumed != null)
            {
                //enum                
                aidApplicationResultsEntity.AssumCitizenship = aidApplicationResultsDto.Assumed.Citizenship.HasValue ? ValidateEnumAndConvertToEntity(typeof(AssumedCitizenshipStatus), aidApplicationResultsDto.Assumed.Citizenship, "Assumed.Citizenship") : null;
                
                //enum
                aidApplicationResultsEntity.AssumSMarStat = aidApplicationResultsDto.Assumed.StudentMaritalStatus.HasValue ? ValidateEnumAndConvertToEntity(typeof(AssumedStudentMaritalStatus), aidApplicationResultsDto.Assumed.StudentMaritalStatus, "Assumed.StudentMaritalStatus") : null;
                
                aidApplicationResultsEntity.AssumSAgi = aidApplicationResultsDto.Assumed.StudentAgi;
                aidApplicationResultsEntity.AssumSTaxPd = aidApplicationResultsDto.Assumed.StudentTaxPaid;
                aidApplicationResultsEntity.AssumSIncWork = aidApplicationResultsDto.Assumed.StudentWorkIncome;
                aidApplicationResultsEntity.AssumSpIncWork = aidApplicationResultsDto.Assumed.SpouseWorkIncome;
                aidApplicationResultsEntity.AssumSAddlFinAmt = aidApplicationResultsDto.Assumed.StudentAddlFinInfoTotal;
                //enum
                aidApplicationResultsEntity.AssumBirthDatePrior = aidApplicationResultsDto.Assumed.BirthDatePrior.HasValue ? ValidateEnumAndConvertToEntity(typeof(AssumedYesNo), aidApplicationResultsDto.Assumed.BirthDatePrior, "Assumed.BirthDatePrior") : null;
                
                //enum
                aidApplicationResultsEntity.AssumSMarried = aidApplicationResultsDto.Assumed.StudentMarried.HasValue ? ValidateEnumAndConvertToEntity(typeof(AssumedYesNo), aidApplicationResultsDto.Assumed.StudentMarried, "Assumed.StudentMarried") : null;
                
                //enum
                aidApplicationResultsEntity.AssumChildren = aidApplicationResultsDto.Assumed.DependentChildren.HasValue ? ValidateEnumAndConvertToEntity(typeof(AssumedYesNo), aidApplicationResultsDto.Assumed.DependentChildren, "Assumed.DependentChildren") : null;
                
                aidApplicationResultsEntity.AssumLegalDep = aidApplicationResultsDto.Assumed.OtherDependents;
                aidApplicationResultsEntity.AssumSNbrFamily = aidApplicationResultsDto.Assumed.studentFamilySize;
                aidApplicationResultsEntity.AssumSNbrCollege = aidApplicationResultsDto.Assumed.StudentNumberInCollege;
                aidApplicationResultsEntity.AssumSAssetTholdExc = aidApplicationResultsDto.Assumed.StudentAssetThreshold;
                //enum
                aidApplicationResultsEntity.AssumPMarStat = aidApplicationResultsDto.Assumed.ParentMaritalStatus.HasValue ? ValidateEnumAndConvertToEntity(typeof(AssumedParentMaritalStatus), aidApplicationResultsDto.Assumed.ParentMaritalStatus, "Assumed.ParentMaritalStatus") : null;
                
                aidApplicationResultsEntity.AssumPar1Ssn = aidApplicationResultsDto.Assumed.FirstParentSsn;
                aidApplicationResultsEntity.AssumPar2Ssn = aidApplicationResultsDto.Assumed.SecondParentSsn;
                aidApplicationResultsEntity.AssumPNbrFamily = aidApplicationResultsDto.Assumed.ParentFamilySize;
                aidApplicationResultsEntity.AssumPNbrCollege = aidApplicationResultsDto.Assumed.ParentNumCollege;
                aidApplicationResultsEntity.AssumPAgi = aidApplicationResultsDto.Assumed.ParentAgi;
                aidApplicationResultsEntity.AssumPTaxPd = aidApplicationResultsDto.Assumed.ParentTaxPaid;
                aidApplicationResultsEntity.AssumPar1Income = aidApplicationResultsDto.Assumed.FirstParentWorkIncome;
                aidApplicationResultsEntity.AssumPar2Income = aidApplicationResultsDto.Assumed.SecondParentWorkIncome;
                aidApplicationResultsEntity.AssumPAddlFinAmt = aidApplicationResultsDto.Assumed.ParentAddlFinancial;
                aidApplicationResultsEntity.AssumPAssetTholdExc = aidApplicationResultsDto.Assumed.ParentAssetThreshold;

            }
            aidApplicationResultsEntity.PrimaryEfc = aidApplicationResultsDto.PrimaryEfc;
            aidApplicationResultsEntity.SecondaryEfc = aidApplicationResultsDto.SecondaryEfc;
            aidApplicationResultsEntity.SignatureRejectEfc = aidApplicationResultsDto.SignatureRejectEfc;
            aidApplicationResultsEntity.PrimaryEfcType = aidApplicationResultsDto.PrimaryEfcType;
            if (aidApplicationResultsDto.AlternatePrimaryEfc != null)
            {
                aidApplicationResultsEntity.PriAlt1mnthEfc = aidApplicationResultsDto.AlternatePrimaryEfc.OneMonth;
                aidApplicationResultsEntity.PriAlt2mnthEfc = aidApplicationResultsDto.AlternatePrimaryEfc.TwoMonths;
                aidApplicationResultsEntity.PriAlt3mnthEfc = aidApplicationResultsDto.AlternatePrimaryEfc.ThreeMonths;
                aidApplicationResultsEntity.PriAlt4mnthEfc = aidApplicationResultsDto.AlternatePrimaryEfc.FourMonths;
                aidApplicationResultsEntity.PriAlt5mnthEfc = aidApplicationResultsDto.AlternatePrimaryEfc.FiveMonths;
                aidApplicationResultsEntity.PriAlt6mnthEfc = aidApplicationResultsDto.AlternatePrimaryEfc.SixMonths;
                aidApplicationResultsEntity.PriAlt7mnthEfc = aidApplicationResultsDto.AlternatePrimaryEfc.SevenMonths;
                aidApplicationResultsEntity.PriAlt8mnthEfc = aidApplicationResultsDto.AlternatePrimaryEfc.EightMonths;
                aidApplicationResultsEntity.PriAlt10mnthEfc = aidApplicationResultsDto.AlternatePrimaryEfc.TenMonths;
                aidApplicationResultsEntity.PriAlt11mnthEfc = aidApplicationResultsDto.AlternatePrimaryEfc.ElevenMonths;
                aidApplicationResultsEntity.PriAlt12mnthEfc = aidApplicationResultsDto.AlternatePrimaryEfc.TwelveMonths;
            }
            aidApplicationResultsEntity.TotalIncome = aidApplicationResultsDto.TotalIncome;
            aidApplicationResultsEntity.AllowancesAgainstTotalIncome = aidApplicationResultsDto.AllowancesAgainstTotalIncome;
            aidApplicationResultsEntity.TaxAllowance = aidApplicationResultsDto.TaxAllowance;
            aidApplicationResultsEntity.EmploymentAllowance = aidApplicationResultsDto.EmploymentAllowance;
            aidApplicationResultsEntity.IncomeProtectionAllowance = aidApplicationResultsDto.IncomeProtectionAllowance;
            aidApplicationResultsEntity.AvailableIncome = aidApplicationResultsDto.AvailableIncome;
            aidApplicationResultsEntity.AvailableIncomeContribution = aidApplicationResultsDto.AvailableIncomeContribution;
            aidApplicationResultsEntity.DiscretionaryNetWorth = aidApplicationResultsDto.DiscretionaryNetWorth;
            aidApplicationResultsEntity.NetWorth = aidApplicationResultsDto.NetWorth;
            aidApplicationResultsEntity.AssetProtectionAllowance = aidApplicationResultsDto.AssetProtectionAllowance;
            aidApplicationResultsEntity.ParentContributionAssets = aidApplicationResultsDto.ParentContributionAssets;
            aidApplicationResultsEntity.AdjustedAvailableIncome = aidApplicationResultsDto.AdjustedAvailableIncome;
            aidApplicationResultsEntity.TotalPrimaryStudentContribution = aidApplicationResultsDto.TotalPrimaryStudentContribution;
            aidApplicationResultsEntity.TotalPrimaryParentContribution = aidApplicationResultsDto.TotalPrimaryParentContribution;
            aidApplicationResultsEntity.ParentContribution = aidApplicationResultsDto.ParentContribution;
            aidApplicationResultsEntity.StudentTotalIncome = aidApplicationResultsDto.StudentTotalIncome;
            aidApplicationResultsEntity.StudentAllowanceAgainstIncome = aidApplicationResultsDto.StudentAllowanceAgainstIncome;
            aidApplicationResultsEntity.DependentStudentIncContrib = aidApplicationResultsDto.DependentStudentIncContrib;
            aidApplicationResultsEntity.StudentDiscretionaryNetWorth = aidApplicationResultsDto.StudentDiscretionaryNetWorth;
            aidApplicationResultsEntity.StudentAssetContribution = aidApplicationResultsDto.StudentAssetContribution;
            aidApplicationResultsEntity.FisapTotalIncome = aidApplicationResultsDto.FisapTotalIncome;
            aidApplicationResultsEntity.CorrectionFlags = aidApplicationResultsDto.CorrectionFlags;
            aidApplicationResultsEntity.HighlightFlags = aidApplicationResultsDto.HighlightFlags;
            aidApplicationResultsEntity.CommentCodes = aidApplicationResultsDto.CommentCodes;
            aidApplicationResultsEntity.ElectronicFedSchoolCodeInd = aidApplicationResultsDto.ElectronicFedSchoolCodeInd;
            aidApplicationResultsEntity.ElectronicTransactionIndicator = aidApplicationResultsDto.ElectronicTransactionIndicator;
            aidApplicationResultsEntity.VerificationSelected = aidApplicationResultsDto.VerificationSelected;

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
            return aidApplicationResultsEntity;
        }

        private static JudgementIndicator? ConvertJudgementIndicatorToDto(string judgementIndicator, bool getByID)
        {
            JudgementIndicator? judgement = null;
            switch (judgementIndicator)
            {
                case "1":
                    judgement = JudgementIndicator.AdjustmentProcessed;
                    break;
                case "2":
                    judgement = JudgementIndicator.AdjustmentFailed;
                    break;
            }
            if (getByID && judgement == null && !string.IsNullOrEmpty(judgementIndicator))
            { throw new Exception("Value " + judgementIndicator + " in the database does not match the values listed in the judgement indicator."); }

            return judgement;
        }

        private static EfcChangeFlag? ConvertEfcChangeFlagToDto(string efcChangeFlag, bool getByID)
        {
            EfcChangeFlag? efcFlag = null;
            switch (efcChangeFlag)
            {
                case "1":
                    efcFlag = EfcChangeFlag.EfcIncrease;
                    break;
                case "2":
                    efcFlag = EfcChangeFlag.EfcDecrease;
                    break;
            }
            if (getByID && efcFlag == null && !string.IsNullOrEmpty(efcChangeFlag))
            { throw new Exception("Value " + efcChangeFlag + " in the database does not match the values listed in the Efc Change Flag"); }

            return efcFlag;
        }

        private static SimplifiedNeedsTest? ConvertSimplifiedNeedsTestToDto(string simplifiedNeedsTest, bool getByID)
        {
            SimplifiedNeedsTest? simplifiedNeeds = null;
            switch (simplifiedNeedsTest)
            {
                case "Y":
                    simplifiedNeeds = SimplifiedNeedsTest.Y;
                    break;
                case "N":
                    simplifiedNeeds = SimplifiedNeedsTest.N;
                    break;
            }
            if (getByID && simplifiedNeeds == null && !string.IsNullOrEmpty(simplifiedNeedsTest))
            { throw new Exception("Value " + simplifiedNeedsTest + " in the database does not match the values listed in the Simplified Needs Test"); }

            return simplifiedNeeds;
        }

        private static LastNameChange? ConvertLastNameChangeToDto(string lastNameChange, bool getByID)
        {
            LastNameChange? nameChange = null;
            switch (lastNameChange)
            {
                case "N":
                    nameChange = LastNameChange.N;
                    break;
            }
            if (getByID && nameChange == null && !string.IsNullOrEmpty(lastNameChange))
            { throw new Exception("Value " + lastNameChange + " in the database does not match the values listed in the Last Name Change"); }

            return nameChange;
        }

        private static AssumedCitizenshipStatus? ConvertCitizenshipStatusToDto(string status, bool getByID)
        {
            AssumedCitizenshipStatus? citizenshipStatus = null;
            switch (status)
            {
                case "1":
                    citizenshipStatus = AssumedCitizenshipStatus.AssumedCitizen;
                    break;
                case "2":
                    citizenshipStatus = AssumedCitizenshipStatus.AssumedEligibleNoncitizen;
                    break;
            }
            if (getByID && citizenshipStatus == null && !string.IsNullOrEmpty(status))
            { throw new Exception("Value " + status + " in the database does not match the values listed in the assumed citizenship status"); }

            return citizenshipStatus;
        }

        private static AssumedStudentMaritalStatus? ConvertStudentMaritalStatusToDto(string status, bool getByID)
        {
            AssumedStudentMaritalStatus? studentMaritalStatus = null;
            switch (status)
            {
                case "1":
                    studentMaritalStatus = AssumedStudentMaritalStatus.AssumedSingle;
                    break;
                case "2":
                    studentMaritalStatus = AssumedStudentMaritalStatus.AssumedMarriedRemarried;
                    break;
            }
            if (getByID && studentMaritalStatus == null && !string.IsNullOrEmpty(status))
            { throw new Exception("Value " + status + " in the database does not match the values listed in the assumed student marital status."); }

            return studentMaritalStatus;
        }

        private static AssumedYesNo? ConvertYesNoToDto(string inputValue, bool getByID, string inputField)
        {
            AssumedYesNo? yesNo = null;
            switch (inputValue)
            {
                case "1":
                    yesNo = AssumedYesNo.AssumedYes;
                    break;
                case "2":
                    yesNo = AssumedYesNo.AssumedNo;
                    break;
            }
            if (getByID && yesNo == null && !string.IsNullOrEmpty(inputValue))
            { throw new Exception("Value " + inputValue + " in the database does not match the values listed in the" + inputField); }

            return yesNo;
        }

        private static AssumedParentMaritalStatus? ConvertParentMaritalStatusToDto(string status, bool getByID)
        {
            AssumedParentMaritalStatus? parentMaritalStatus = null;
            switch (status)
            {
                case "1":
                    parentMaritalStatus = AssumedParentMaritalStatus.AssumedMarriedRemarried;
                    break;
                case "2":
                    parentMaritalStatus = AssumedParentMaritalStatus.AssumedSingle;
                    break;
            }
            if (getByID && parentMaritalStatus == null && !string.IsNullOrEmpty(status))
            { throw new Exception("Value " + status + " in the database does not match the values listed in the assumed parent marital status."); }

            return parentMaritalStatus;
        }
               

        private void ValidateAidApplicationResults(AidApplicationResults aidApplicationResultsDto)
        {

            if (aidApplicationResultsDto == null)
                IntegrationApiExceptionAddError("Must provide a aidApplicationResults", string.Format("Invalid.Data"));

            if (string.IsNullOrEmpty(aidApplicationResultsDto.AppDemoId))
                IntegrationApiExceptionAddError("Must provide a value for appDemoId", string.Format("Invalid.Data"));

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
        }

        private string ValidateEnumAndConvertToEntity(Type enumType, object value, string fieldName)
        {
            string output = null;
            if (value != null)
            {
                if (Enum.IsDefined(enumType, value))
                {
                    if (enumType.Name == "SimplifiedNeedsTest" || enumType.Name == "LastNameChange")
                        output = value.ToString();
                    else
                        output = value.GetHashCode().ToString();
                }
                else
                    IntegrationApiExceptionAddError(string.Format("Invalid " + fieldName + " : " + value.ToString()));

            }
            return output;
        }
    }
}
