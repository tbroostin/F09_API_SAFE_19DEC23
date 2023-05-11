//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class FinancialAidApplicationOutcomeService : BaseCoordinationService, IFinancialAidApplicationOutcomeService
    {

        private readonly IFinancialAidApplicationOutcomeRepository _financialAidApplicationOutcomeRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IConfigurationRepository configurationRepository;

        public FinancialAidApplicationOutcomeService(

            IFinancialAidApplicationOutcomeRepository financialAidApplicationOutcomeRepository,
            IPersonRepository personRepository,
            IStudentReferenceDataRepository studentReferenceDataRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,

            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            this.configurationRepository = configurationRepository;
            _financialAidApplicationOutcomeRepository = financialAidApplicationOutcomeRepository;
            _personRepository = personRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
        }

        #region Get all reference data

        private IEnumerable<Domain.Student.Entities.FinancialAidYear> _financialAidYears;
        private async Task<IEnumerable<Domain.Student.Entities.FinancialAidYear>> GetFinancialAidYearsAsync(bool bypassCache)
        {
            if (_financialAidYears == null)
            {
                _financialAidYears = await _studentReferenceDataRepository.GetFinancialAidYearsAsync(bypassCache);
            }
            return _financialAidYears;
        }

        #endregion

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 9</remarks>
        /// <summary>
        /// Gets all financial-aid-applications
        /// </summary>
        /// <returns>Collection of FinancialAidApplications DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplicationOutcome>, int>> GetAsync(int offset, int limit, FinancialAidApplicationOutcome filterDto, bool bypassCache = false)
        {        
            // Get all financial aid years
            var aidYearEntity = (await _studentReferenceDataRepository.GetFinancialAidYearsAsync(bypassCache));
            List<string> faSuiteYears = aidYearEntity.Select(k => k.Code).ToList();

            //process filters 
            string applicantId = string.Empty;
            string aidYear = string.Empty;
            string methodology = string.Empty;
            string applicationId = string.Empty;
            if (filterDto != null)
            {
                //get aid year
                if (filterDto.AidYear != null && !string.IsNullOrEmpty(filterDto.AidYear.Id))
                {
                    aidYear = ConvertGuidToCode(aidYearEntity, filterDto.AidYear.Id);
                    if (string.IsNullOrEmpty(aidYear))
                        return new Tuple<IEnumerable<Dtos.FinancialAidApplicationOutcome>, int>(new List<Dtos.FinancialAidApplicationOutcome>(), 0);
                }
                //get applicant Id
                if (filterDto.Applicant != null && filterDto.Applicant.Person != null && !string.IsNullOrEmpty(filterDto.Applicant.Person.Id))
                {
                    try
                    {
                        applicantId = await _personRepository.GetPersonIdFromGuidAsync(filterDto.Applicant.Person.Id);
                        if (string.IsNullOrEmpty(applicantId))
                            return new Tuple<IEnumerable<Dtos.FinancialAidApplicationOutcome>, int>(new List<Dtos.FinancialAidApplicationOutcome>(), 0);
                    }
                    catch // if bad guid, return empty set
                    {
                        return new Tuple<IEnumerable<Dtos.FinancialAidApplicationOutcome>, int>(new List<Dtos.FinancialAidApplicationOutcome>(), 0);
                    }
                }
                //get methodology
                if (filterDto.Methodology != FinancialAidApplicationsMethodology.NotSet)
                {
                    methodology = filterDto.Methodology.ToString().ToLower();
                }
                //get application
                if (filterDto.Application != null && !string.IsNullOrEmpty(filterDto.Application.Id))
                {
                    try
                    {
                        applicationId = await _financialAidApplicationOutcomeRepository.GetApplicationIdFromGuidAsync(filterDto.Application.Id);
                        if (string.IsNullOrEmpty(applicationId))
                            return new Tuple<IEnumerable<Dtos.FinancialAidApplicationOutcome>, int>(new List<Dtos.FinancialAidApplicationOutcome>(), 0);
                    }
                    catch // if bad guid, return empty set
                    {
                        return new Tuple<IEnumerable<Dtos.FinancialAidApplicationOutcome>, int>(new List<Dtos.FinancialAidApplicationOutcome>(), 0);
                    }
                }
            }

            var financialAidApplicationOutcomeDtos = new List<Dtos.FinancialAidApplicationOutcome>();
            try
            {
                var fafsaDomainTuple = await _financialAidApplicationOutcomeRepository.GetAsync(offset, limit, bypassCache, applicantId, aidYear, methodology, applicationId, faSuiteYears);
                if (fafsaDomainTuple != null && fafsaDomainTuple.Item1 != null && fafsaDomainTuple.Item1.Any())
                {
                    foreach (var entity in fafsaDomainTuple.Item1)
                    {
                        if (entity != null)
                        {
                            var financialAidApplicationOutcomeDto = await BuildFinancialAidApplicationOutcomeDtoAsync(entity, bypassCache);
                            financialAidApplicationOutcomeDtos.Add(financialAidApplicationOutcomeDto);
                        }
                    }

                    if (IntegrationApiException != null)
                        throw IntegrationApiException;

                    return new Tuple<IEnumerable<Dtos.FinancialAidApplicationOutcome>, int>(financialAidApplicationOutcomeDtos, fafsaDomainTuple.Item2);
                }
                else
                {
                    return new Tuple<IEnumerable<Dtos.FinancialAidApplicationOutcome>, int>(new List<Dtos.FinancialAidApplicationOutcome>(), 0);
                }
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 9</remarks>
        /// <summary>
        /// Get a FinancialAidApplicationOutcomes from its GUID
        /// </summary>
        /// <returns>FinancialAidApplications DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.FinancialAidApplicationOutcome> GetByIdAsync(string id, bool bypassCache = true)
        {
            try
            {

                // Get the student financial aid awards domain entity from the repository
                var fafsaDomainEntity = await _financialAidApplicationOutcomeRepository.GetByIdAsync(id);
                if (fafsaDomainEntity == null)
                {
                    throw new ArgumentNullException("FinancialAidApplicationDomainEntity", "FinancialAidApplicationDomainEntity cannot be null. ");
                }
                // Convert the financial aid application object into DTO.
                var financialAidApplicationDto = await BuildFinancialAidApplicationOutcomeDtoAsync(fafsaDomainEntity, bypassCache);

                if (IntegrationApiException != null)
                    throw IntegrationApiException;

                return financialAidApplicationDto;
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }        
        }

        private async Task<Dtos.FinancialAidApplicationOutcome> BuildFinancialAidApplicationOutcomeDtoAsync(Domain.Student.Entities.Fafsa fafsaEntity, bool bypassCache = true)
        {
            var financialAidApplicationOutcomeDto = new Dtos.FinancialAidApplicationOutcome();

            if (fafsaEntity == null)
            {
                //  Should never occur.  Would have already encountered a repo error.  Don't have a GUID or record key if no entity at all.
                IntegrationApiExceptionAddError("Missing fafsa entity.");
                return financialAidApplicationOutcomeDto;
            }

            if (string.IsNullOrEmpty(fafsaEntity.CalcResultsGuid))
            {
                IntegrationApiExceptionAddError("Missing GUID for ISIR.CALC.RESULTS.", id: fafsaEntity.Id);
            }
            financialAidApplicationOutcomeDto.Id = fafsaEntity.CalcResultsGuid;

            try
            {
                //
                // Set Applicant
                //
                if (string.IsNullOrEmpty(fafsaEntity.StudentId))
                {
                    IntegrationApiExceptionAddError("Applicant is required.", guid: fafsaEntity.CalcResultsGuid, id: fafsaEntity.Id);
                }
                else
                {
                    try
                    {
                        var person = new FinancialAidApplicationApplicant();
                        person.Person = new GuidObject2(await _personRepository.GetPersonGuidFromIdAsync(fafsaEntity.StudentId));
                        financialAidApplicationOutcomeDto.Applicant = person;
                    }
                    catch
                    {
                        IntegrationApiExceptionAddError(string.Format("No GUID found, Entity '{0}', Record ID '{1}'.", "PERSON", fafsaEntity.StudentId),
                            guid: fafsaEntity.CalcResultsGuid, id: fafsaEntity.Id);
                    }
                }

                //
                // Set AidYear
                //
                if (string.IsNullOrEmpty(fafsaEntity.AwardYear))
                {
                    IntegrationApiExceptionAddError("Aid year is required.", guid: fafsaEntity.CalcResultsGuid, id: fafsaEntity.Id);
                }
                else
                {
                    try
                    {
                        var aidYearEntity = (await _studentReferenceDataRepository.GetFinancialAidYearsAsync(bypassCache)).FirstOrDefault(t => t.Code == fafsaEntity.AwardYear);
                        financialAidApplicationOutcomeDto.AidYear = new GuidObject2(aidYearEntity.Guid);
                    }
                    catch
                    {
                        IntegrationApiExceptionAddError(string.Format("No GUID found, Entity '{0}', Record ID '{1}'.", "FA.SUITES", fafsaEntity.AwardYear),
                            guid: fafsaEntity.CalcResultsGuid, id: fafsaEntity.Id);
                    }
                }

                // Set Methodology
                financialAidApplicationOutcomeDto.Methodology = FinancialAidApplicationsMethodology.NotSet;
                if (string.IsNullOrEmpty(fafsaEntity.CorrectedFromId))
                {
                    if (fafsaEntity.FafsaPrimaryId == fafsaEntity.CsInstitutionalIsirId)
                    {
                        financialAidApplicationOutcomeDto.Methodology = FinancialAidApplicationsMethodology.Institutional;
                    }
                    if (fafsaEntity.FafsaPrimaryId == fafsaEntity.CsFederalIsirId)
                    {
                        financialAidApplicationOutcomeDto.Methodology = FinancialAidApplicationsMethodology.Federal;
                    }
                    if (fafsaEntity.FafsaPrimaryId == fafsaEntity.CsFederalIsirId && fafsaEntity.FafsaPrimaryId == fafsaEntity.CsInstitutionalIsirId)
                    {
                        financialAidApplicationOutcomeDto.Methodology = FinancialAidApplicationsMethodology.Institutionalfederal;
                    }
                }
                else
                {
                    // If Methodology could not be set from primary FAFSA record, use its original (corrected from)
                    if (fafsaEntity.CorrectedFromId == fafsaEntity.CsInstitutionalIsirId)
                    {
                        financialAidApplicationOutcomeDto.Methodology = FinancialAidApplicationsMethodology.Institutional;
                    }
                    if (fafsaEntity.CorrectedFromId == fafsaEntity.CsFederalIsirId)
                    {
                        financialAidApplicationOutcomeDto.Methodology = FinancialAidApplicationsMethodology.Federal;
                    }
                    if (fafsaEntity.CorrectedFromId == fafsaEntity.CsFederalIsirId && fafsaEntity.CorrectedFromId == fafsaEntity.CsInstitutionalIsirId)
                    {
                        financialAidApplicationOutcomeDto.Methodology = FinancialAidApplicationsMethodology.Institutionalfederal;
                    }
                }
                if (financialAidApplicationOutcomeDto.Methodology == FinancialAidApplicationsMethodology.NotSet)
                {
                    IntegrationApiExceptionAddError("Methodology is required.", guid: fafsaEntity.CalcResultsGuid, id: fafsaEntity.Id);
                }
                // Set application rejection status
                if (fafsaEntity.RejectionCodes != null)
                {
                    if (fafsaEntity.RejectionCodes.Any())
                    {
                        financialAidApplicationOutcomeDto.RejectionStatus = FinancialAidApplicationOutcomesRejectionStatus.Rejected;
                    }
                    else
                    {
                        financialAidApplicationOutcomeDto.RejectionStatus = FinancialAidApplicationOutcomesRejectionStatus.NotRejected;
                    }
                }

                // Set student aid report C designation 
                if (fafsaEntity.HasStudentAidReportC != false)
                {
                    if (fafsaEntity.HasStudentAidReportC == true)
                    {
                        financialAidApplicationOutcomeDto.StudentAidReportC = FinancialAidApplicationOutcomesStudentAidReportResolution.Flagged;
                    }
                }

                // Set student dependency status
                var dependencyStatus = "";
                if (fafsaEntity.Type == "PROF")
                {
                    if (!string.IsNullOrEmpty(fafsaEntity.StudentDepdendencyStatusInas))
                    {
                        dependencyStatus = fafsaEntity.StudentDepdendencyStatusInas;
                    }
                    else
                    {
                        dependencyStatus = fafsaEntity.StudentDependencyStatus;
                    }
                }
                else
                {
                    dependencyStatus = fafsaEntity.StudentDependencyStatus;
                }
                if (!string.IsNullOrEmpty(dependencyStatus))
                {
                    switch (dependencyStatus)
                    {
                        case ("I"):
                            financialAidApplicationOutcomeDto.Dependency = FinancialAidApplicationOutcomesDependency.Independent;
                            break;
                        case ("D"):
                            financialAidApplicationOutcomeDto.Dependency = FinancialAidApplicationOutcomesDependency.Dependent;
                            break;
                        case ("Y"):
                            financialAidApplicationOutcomeDto.Dependency = FinancialAidApplicationOutcomesDependency.RejectedIndependent;
                            break;
                        case ("X"):
                            financialAidApplicationOutcomeDto.Dependency = FinancialAidApplicationOutcomesDependency.RejectedDependent;
                            break;
                        default:
                            financialAidApplicationOutcomeDto.Dependency = FinancialAidApplicationOutcomesDependency.NotSet;
                            break;
                    }
                }

                // Set student dependency override status
                if (!string.IsNullOrEmpty(fafsaEntity.StudentDependencyOverride))
                {
                    switch (fafsaEntity.StudentDependencyOverride)
                    {
                        case ("I"):
                            financialAidApplicationOutcomeDto.DependencyOverride = FinancialAidApplicationOutcomesDependencyOverride.Overridden;
                            break;
                        case ("H"):
                            financialAidApplicationOutcomeDto.DependencyOverride = FinancialAidApplicationOutcomesDependencyOverride.Overridden;
                            break;
                        default:
                            financialAidApplicationOutcomeDto.DependencyOverride = FinancialAidApplicationOutcomesDependencyOverride.NotSet;
                            break;
                    }
                }

                // Set student Pell eligiblity status
                if (fafsaEntity.Type != "PROF")
                {
                    if (fafsaEntity.Type != "CORR")
                    {
                        switch (fafsaEntity.IsPellEligible)
                        {
                            case (true):
                                financialAidApplicationOutcomeDto.PellEligibility = FinancialAidApplicationOutcomesPellEligibility.Eligible;
                                break;
                            case (false):
                                financialAidApplicationOutcomeDto.PellEligibility = FinancialAidApplicationOutcomesPellEligibility.NotEligible;
                                break;
                            default:
                                financialAidApplicationOutcomeDto.DependencyOverride = FinancialAidApplicationOutcomesDependencyOverride.NotSet;
                                break;
                        }
                    }
                    else
                    {
                        switch (fafsaEntity.IsPellEligibleOriginal)
                        {
                            case (true):
                                financialAidApplicationOutcomeDto.PellEligibility = FinancialAidApplicationOutcomesPellEligibility.Eligible;
                                break;
                            case (false):
                                financialAidApplicationOutcomeDto.PellEligibility = FinancialAidApplicationOutcomesPellEligibility.NotEligible;
                                break;
                            default:
                                financialAidApplicationOutcomeDto.DependencyOverride = FinancialAidApplicationOutcomesDependencyOverride.NotSet;
                                break;
                        }
                    }
                }

                // Set student automatic zero expected family contribution
                switch (fafsaEntity.HasAutomaticZeroExpectedFamilyContribution)
                {
                    case (true):
                        financialAidApplicationOutcomeDto.AutomaticZeroContribution = FinancialAidApplicationOutcomesAutomaticZeroContribution.Applied;
                        break;
                    case (false):
                        financialAidApplicationOutcomeDto.AutomaticZeroContribution = FinancialAidApplicationOutcomesAutomaticZeroContribution.NotApplied;
                        break;
                    default:
                        financialAidApplicationOutcomeDto.AutomaticZeroContribution = FinancialAidApplicationOutcomesAutomaticZeroContribution.NotSet;
                        break;
                }

                // Set student simple needs test status
                switch (fafsaEntity.HasMetSimpleNeed)
                {
                    case (true):
                        financialAidApplicationOutcomeDto.SimplifiedNeedsTest = FinancialAidApplicationOutcomesSimplifiedNeedsTest.Met;
                        break;
                    case (false):
                        financialAidApplicationOutcomeDto.SimplifiedNeedsTest = FinancialAidApplicationOutcomesSimplifiedNeedsTest.NotMet;
                        break;
                    default:
                        financialAidApplicationOutcomeDto.SimplifiedNeedsTest = FinancialAidApplicationOutcomesSimplifiedNeedsTest.NotSet;
                        break;
                }

                // Set professional judgement status
                switch (fafsaEntity.FinancialAidAAministratorAdjustment)
                {
                    case ("Y"):
                        financialAidApplicationOutcomeDto.ProfessionalJudgement = FinancialAidApplicationOutcomesProfessionalJudgement.Processed;
                        break;
                    case ("N"):
                        financialAidApplicationOutcomeDto.ProfessionalJudgement = FinancialAidApplicationOutcomesProfessionalJudgement.RequestFailed;
                        break;
                    default:
                        // fall through, we do not want to set/return Professional Judgement if no FAA request was made.  
                        break;
                }

                // Set verification status from ISIR.RESULTS
                if (fafsaEntity.HasIsirResults == true)
                {
                    if (fafsaEntity.Type == "ISIR" || fafsaEntity.Type == "CPPSG")
                    {
                        switch (fafsaEntity.HasVerificationSelection)
                        {
                            case (true):
                                financialAidApplicationOutcomeDto.VerificationStatus = FinancialAidApplicationOutcomesVerificationStatus.Selected;
                                break;
                            case (false):
                                financialAidApplicationOutcomeDto.VerificationStatus = FinancialAidApplicationOutcomesVerificationStatus.NotSelected;
                                break;
                            default:
                                financialAidApplicationOutcomeDto.VerificationStatus = FinancialAidApplicationOutcomesVerificationStatus.NotSet;
                                break;
                        }

                        // Set verification catgory
                        if (!string.IsNullOrEmpty(fafsaEntity.VerificationTracking))
                        {
                            financialAidApplicationOutcomeDto.VerificationCategory = fafsaEntity.VerificationTracking;
                        }
                    }
                    else
                    {
                        if (fafsaEntity.Type != "CORR")
                        {
                            financialAidApplicationOutcomeDto.VerificationStatus = FinancialAidApplicationOutcomesVerificationStatus.NotSelected;
                        }
                    }
                }

                // Set verification status from ISIR.RESULTS of original (corrected-from) record.
                if (fafsaEntity.HasIsirResultsOriginal == true)
                {
                    if (fafsaEntity.Type == "CORR")
                    {
                        switch (fafsaEntity.HasVerificationSelectionOriginal)
                        {
                            case (true):
                                financialAidApplicationOutcomeDto.VerificationStatus = FinancialAidApplicationOutcomesVerificationStatus.Selected;
                                break;
                            case (false):
                                financialAidApplicationOutcomeDto.VerificationStatus = FinancialAidApplicationOutcomesVerificationStatus.NotSelected;
                                break;
                            default:
                                financialAidApplicationOutcomeDto.VerificationStatus = FinancialAidApplicationOutcomesVerificationStatus.NotSet;
                                break;
                        }

                        // Set verification catgory
                        if (!string.IsNullOrEmpty(fafsaEntity.VerificationTrackingOriginal))
                        {
                            financialAidApplicationOutcomeDto.VerificationCategory = fafsaEntity.VerificationTrackingOriginal;
                        }
                    }
                }

                // Set expected family contribution
                var expectedFamilyContribution = new Dtos.DtoProperties.AmountDtoProperty();
                if (fafsaEntity.Type == "PROF")
                {
                    if (fafsaEntity.InstitutionFamilyContributionOverrideAmount != null)
                    {
                        expectedFamilyContribution.Value = fafsaEntity.InstitutionFamilyContributionOverrideAmount;
                    }
                }
                if (expectedFamilyContribution.Value == null)
                {
                    if (fafsaEntity.InstitutionalNeedAnalysisParentsContribution != null || fafsaEntity.InstitutionalNeedAnalysisStudentContribution != null)
                    {
                        if (fafsaEntity.InstitutionalNeedAnalysisParentsContribution != null)
                        {
                            var institutionalNeedAnalysisContribution = fafsaEntity.InstitutionalNeedAnalysisParentsContribution;
                            if (fafsaEntity.InstitutionalNeedAnalysisStudentContribution != null)
                            {
                                institutionalNeedAnalysisContribution += fafsaEntity.InstitutionalNeedAnalysisStudentContribution;
                            }
                            if (institutionalNeedAnalysisContribution <= 999999)
                            {
                                expectedFamilyContribution.Value = institutionalNeedAnalysisContribution;
                            }
                        }
                        else
                        {
                            var institutionalNeedAnalysisContribution = fafsaEntity.InstitutionalNeedAnalysisStudentContribution;
                            if (institutionalNeedAnalysisContribution <= 999999)
                            {
                                expectedFamilyContribution.Value = institutionalNeedAnalysisContribution;
                            }
                        }
                    }
                }
                if (expectedFamilyContribution.Value == null)
                {
                    if (fafsaEntity.CfsParentOptionalImCalculation != null || fafsaEntity.CfsStudentOptionalImCalculation != null)
                    {
                        if (fafsaEntity.CfsParentOptionalImCalculation != null)
                        {
                            var institutionalNeedAnalysisContribution = fafsaEntity.CfsParentOptionalImCalculation;
                            if (fafsaEntity.CfsStudentOptionalImCalculation != null)
                            {
                                institutionalNeedAnalysisContribution += fafsaEntity.CfsStudentOptionalImCalculation;
                            }
                            if (institutionalNeedAnalysisContribution <= 999999)
                            {
                                expectedFamilyContribution.Value = institutionalNeedAnalysisContribution;
                            }
                        }
                        else
                        {
                            var institutionalNeedAnalysisContribution = fafsaEntity.CfsStudentOptionalImCalculation;
                            if (institutionalNeedAnalysisContribution <= 999999)
                            {
                                expectedFamilyContribution.Value = institutionalNeedAnalysisContribution;
                            }
                        }
                    }
                }
                if (expectedFamilyContribution.Value == null)
                {
                    expectedFamilyContribution.Value = fafsaEntity.FamilyContribution;
                }
                if (expectedFamilyContribution.Value != null)
                {
                    expectedFamilyContribution.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                    financialAidApplicationOutcomeDto.ExpectedFamilyContribution = expectedFamilyContribution;
                }

                // Set expected student contribution
                var expectedStudentContribution = new Dtos.DtoProperties.AmountDtoProperty();
                if (fafsaEntity.Type == "PROF")
                {
                    if (fafsaEntity.InstitutionalNeedAnalysisStudentContribution != null)
                    {
                        expectedStudentContribution.Value = fafsaEntity.InstitutionalNeedAnalysisStudentContribution;
                    }
                    else
                    {
                        expectedStudentContribution.Value = fafsaEntity.CfsStudentOptionalImCalculation;
                    }
                }
                else
                {
                    expectedStudentContribution.Value = fafsaEntity.StudentContribution;
                }
                if (expectedStudentContribution.Value != null)
                {
                    expectedStudentContribution.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                    financialAidApplicationOutcomeDto.ExpectedStudentContribution = expectedStudentContribution;
                }

                // Set expected custodial parent contribution
                var expectedTotalParentContribution = new Dtos.DtoProperties.AmountDtoProperty();
                if (fafsaEntity.Type == "PROF")
                {
                    if (fafsaEntity.InstitutionalNeedAnalysisParentsContribution != null)
                    {
                        expectedTotalParentContribution.Value = fafsaEntity.InstitutionalNeedAnalysisParentsContribution;
                    }
                    else
                    {
                        expectedTotalParentContribution.Value = fafsaEntity.CfsParentOptionalImCalculation;
                    }
                }
                else
                {
                    expectedTotalParentContribution.Value = fafsaEntity.ParentContribution;
                }
                if (expectedTotalParentContribution.Value != null)
                {
                    expectedTotalParentContribution.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                    financialAidApplicationOutcomeDto.ExpectedTotalParentContribution = expectedTotalParentContribution;
                }

                // Set expected non custodial parent contribution
                var expectedNoncustodialParentContribution = new Dtos.DtoProperties.AmountDtoProperty();
                if (fafsaEntity.Type == "PROF")
                {
                    if (fafsaEntity.HasNonCustodialParentProfile == true)
                    {
                        if (fafsaEntity.NonCustodialParentOverrideAmountNcp != null)
                        {
                            expectedNoncustodialParentContribution.Value = fafsaEntity.NonCustodialParentOverrideAmountNcp;
                        }
                        else
                        {
                            if (fafsaEntity.NonCustodialParentCalculatedContributionNcp != null)
                            {
                                expectedNoncustodialParentContribution.Value = fafsaEntity.NonCustodialParentCalculatedContributionNcp;
                            }
                        }
                    }
                    else
                    {
                        if (fafsaEntity.NonCustodialParentOverrideAmount != null)
                        {
                            expectedNoncustodialParentContribution.Value = fafsaEntity.NonCustodialParentOverrideAmount;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(fafsaEntity.NonCustodialParentContribution))
                            {
                                var nonCustodialParentContribution = Convert.ToDecimal(fafsaEntity.NonCustodialParentContribution);
                                expectedNoncustodialParentContribution.Value = nonCustodialParentContribution;
                            }
                        }
                    }

                }
                if (expectedNoncustodialParentContribution.Value != null)
                {
                    expectedNoncustodialParentContribution.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                    financialAidApplicationOutcomeDto.ExpectedNoncustodialParentContribution = expectedNoncustodialParentContribution;
                }

                if (fafsaEntity.Type == "PROF")
                {
                    // Set applicant home equity
                    if (fafsaEntity.StudentHomeValue != null)
                    {
                        var applicantHomeEquity = new Dtos.DtoProperties.AmountDtoProperty();
                        applicantHomeEquity.Value = fafsaEntity.StudentHomeValue - fafsaEntity.StudentHomeDebt;
                        applicantHomeEquity.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                        financialAidApplicationOutcomeDto.ApplicantHomeEquity = applicantHomeEquity;
                    }

                    // Set custodial parent home equity
                    if (fafsaEntity.ParentHomeValue != null)
                    {
                        var custodialParentHomeEquity = new Dtos.DtoProperties.AmountDtoProperty();
                        custodialParentHomeEquity.Value = fafsaEntity.ParentHomeValue - fafsaEntity.ParentHomeDebt;
                        custodialParentHomeEquity.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                        financialAidApplicationOutcomeDto.CustodialParentHomeEquity = custodialParentHomeEquity;
                    }
               
                    // Set non custodial parent home equity
                    var nonCustodialParentHomeEquity = new Dtos.DtoProperties.AmountDtoProperty();
                    if (fafsaEntity.HasNonCustodialParentProfile == true)
                    {
                        nonCustodialParentHomeEquity.Value = fafsaEntity.ParentHomeValueNcp - fafsaEntity.ParentHomeDebtNcp;
                    }
                    if (nonCustodialParentHomeEquity.Value != null)
                    {
                        nonCustodialParentHomeEquity.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                        financialAidApplicationOutcomeDto.NonCustodialParentHomeEquity = nonCustodialParentHomeEquity;
                    }
                }

                var totalIncome = new Dtos.DtoProperties.AmountDtoProperty();
                if (fafsaEntity.FisapTotalIncome != null)
                {
                    totalIncome.Value = fafsaEntity.FisapTotalIncome;
                    totalIncome.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                    financialAidApplicationOutcomeDto.TotalIncome = totalIncome;
                }

                
                if (string.IsNullOrEmpty(fafsaEntity.Guid))
                {
                    IntegrationApiExceptionAddError("Application is required.", guid: fafsaEntity.CalcResultsGuid, id: fafsaEntity.Id);
                }
                financialAidApplicationOutcomeDto.Application = new GuidObject2(fafsaEntity.Guid);                    
               
            }
            catch (Exception e)
            {
                IntegrationApiExceptionAddError("Unable to build DTO for application outcome.", guid: fafsaEntity.CalcResultsGuid, id: fafsaEntity.Id);
            }
            return financialAidApplicationOutcomeDto;
        }

  
    }
}
