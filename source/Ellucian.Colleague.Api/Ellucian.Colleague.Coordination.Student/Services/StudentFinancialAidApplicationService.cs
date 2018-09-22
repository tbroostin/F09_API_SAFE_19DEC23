//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

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

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class StudentFinancialAidApplicationService : BaseCoordinationService, IStudentFinancialAidApplicationService
    {

        private readonly IStudentFinancialAidApplicationRepository _financialAidApplicationRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IStudentReferenceDataRepository _financialAidReferenceDataRepository;
        private readonly IConfigurationRepository configurationRepository;

        public StudentFinancialAidApplicationService(

            IStudentFinancialAidApplicationRepository financialAidApplicationRepository,
            IPersonRepository personRepository,
            IStudentReferenceDataRepository financialAidReferenceDataRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            this.configurationRepository = configurationRepository;
            _financialAidApplicationRepository = financialAidApplicationRepository;
            _personRepository = personRepository;
            _financialAidReferenceDataRepository = financialAidReferenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 9</remarks>
        /// <summary>
        /// Gets all financial-aid-applications
        /// </summary>
        /// <returns>Collection of FinancialAidApplications DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplication>, int>> GetAsync(int offset, int limit, bool bypassCache = false)
        {
            CheckViewFinancialAidApplicationsPermission();

            // Get all financial aid years
            var aidYearEntity = (await _financialAidReferenceDataRepository.GetFinancialAidYearsAsync(bypassCache));
            List<string> faSuiteYears = aidYearEntity.Select(k => k.Code).ToList();
            
            var financialAidApplicationDtos = new List<Dtos.FinancialAidApplication>();
            var fafsaDomainTuple = await _financialAidApplicationRepository.GetAsync(offset, limit, bypassCache, faSuiteYears);

            if (fafsaDomainTuple == null || !fafsaDomainTuple.Item1.Any())
            {
                return new Tuple<IEnumerable<Dtos.FinancialAidApplication>, int>(new List<Dtos.FinancialAidApplication>(), 0);
            }

            var financialAidApplicationDomainEntities = fafsaDomainTuple.Item1;
            var totalRecords = fafsaDomainTuple.Item2;

            // Convert the student financial aid awards and all its child objects into DTOs.
            foreach (var entity in financialAidApplicationDomainEntities)
            {
                if (entity != null)
                {
                    var financialAidApplicationDto = await BuildFinancialAidApplicationDtoAsync(entity, bypassCache);
                    financialAidApplicationDtos.Add(financialAidApplicationDto);
                }
            }
            return new Tuple<IEnumerable<Dtos.FinancialAidApplication>, int>(financialAidApplicationDtos, totalRecords);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 9</remarks>
        /// <summary>
        /// Get a FinancialAidApplications from its GUID
        /// </summary>
        /// <returns>FinancialAidApplications DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.FinancialAidApplication> GetByIdAsync(string id)
        {
            CheckViewFinancialAidApplicationsPermission();

            // Get the student financial aid awards domain entity from the repository
            var fafsaDomainEntity = await _financialAidApplicationRepository.GetByIdAsync(id);
            if (fafsaDomainEntity == null)
            {
                throw new ArgumentNullException("FinancialAidApplicationDomainEntity", "FinancialAidApplicationDomainEntity cannot be null. ");
            }

            // Convert the financial aid application object into DTO.
            return await BuildFinancialAidApplicationDtoAsync(fafsaDomainEntity);
        }

        private async Task<Dtos.FinancialAidApplication> BuildFinancialAidApplicationDtoAsync(Domain.Student.Entities.Fafsa fafsaEntity, bool bypassCache = true)
        {
            var financialAidApplicationDto = new Dtos.FinancialAidApplication();

            financialAidApplicationDto.Id = fafsaEntity.Guid;

            try
            {
                //
                // Set Applicant
                //
                var person = new FinancialAidApplicationApplicant();
                person.Person = new GuidObject2((!string.IsNullOrEmpty(fafsaEntity.StudentId)) ?
                   await _personRepository.GetPersonGuidFromIdAsync(fafsaEntity.StudentId) :
                   string.Empty);
                financialAidApplicationDto.Applicant = person;

                //
                // Set AidYear
                //
                if (!string.IsNullOrEmpty(fafsaEntity.AwardYear))
                {
                    var aidYearEntity = (await _financialAidReferenceDataRepository.GetFinancialAidYearsAsync(bypassCache)).FirstOrDefault(t => t.Code == fafsaEntity.AwardYear);
                    if (aidYearEntity != null && !string.IsNullOrEmpty(aidYearEntity.Guid))
                    {
                        financialAidApplicationDto.AidYear = new GuidObject2(aidYearEntity.Guid);
                    }
                }

                // Set Methodology
                var fafsaId = fafsaEntity.FafsaPrimaryId;
                if (fafsaEntity.FafsaPrimaryIdCorrected != null)
                {
                    fafsaId = fafsaEntity.FafsaPrimaryIdCorrected;
                }


                financialAidApplicationDto.Methodology = FinancialAidApplicationsMethodology.NotSet;
                if (fafsaId == fafsaEntity.CsInstitutionalIsirId)
                {
                    financialAidApplicationDto.Methodology = FinancialAidApplicationsMethodology.Institutional;
                }
                if (fafsaId == fafsaEntity.CsFederalIsirId)
                {
                    financialAidApplicationDto.Methodology = FinancialAidApplicationsMethodology.Federal;
                }
                if (fafsaId == fafsaEntity.CsFederalIsirId && fafsaId == fafsaEntity.CsInstitutionalIsirId)
                {
                    financialAidApplicationDto.Methodology = FinancialAidApplicationsMethodology.Institutionalfederal;
                }
                if (financialAidApplicationDto.Methodology == FinancialAidApplicationsMethodology.NotSet)
                {
                    var errorMessage = string.Format("Unable to identify methodology for application outcome '{0}'", fafsaEntity.Guid);
                    throw new ArgumentException(errorMessage);
                }

                // Set source
                if (fafsaEntity.FafsaPrimaryType != null)
                {
                    switch (fafsaEntity.FafsaPrimaryType)
                    {
                        case ("ISIR"):
                            financialAidApplicationDto.Source = FinancialAidApplicationsSource.Isir;
                            break;
                        case ("CPSSG"):
                            financialAidApplicationDto.Source = FinancialAidApplicationsSource.Isir;
                            break;
                        case ("CORR"):
                            financialAidApplicationDto.Source = FinancialAidApplicationsSource.Isir;
                            break;
                        case ("PROF"):
                            financialAidApplicationDto.Source = FinancialAidApplicationsSource.Profile;
                            break;
                        case ("IAPP"):
                            financialAidApplicationDto.Source = FinancialAidApplicationsSource.Manualfederal;
                            break;
                        case ("SUPP"):
                            financialAidApplicationDto.Source = FinancialAidApplicationsSource.Manualinstitution;
                            break;
                        default:
                            financialAidApplicationDto.Source = FinancialAidApplicationsSource.NotSet;
                            break;
                    }
                }
                if (financialAidApplicationDto.Methodology == FinancialAidApplicationsMethodology.NotSet)
                {
                    var errorMessage = string.Format("Unable to identify methodology for application outcome '{0}'", fafsaEntity.Guid);
                    throw new ArgumentException(errorMessage);
                }

                BuildDtoFromThisFafsa(fafsaEntity, financialAidApplicationDto);
                
                if (fafsaEntity.ApplicationCompletedOn != null)
                {
                    financialAidApplicationDto.ApplicationCompletedOn = fafsaEntity.ApplicationCompletedOn;
                }
                if (!string.IsNullOrEmpty(fafsaEntity.StateOfLegalResidence))
                {
                    financialAidApplicationDto.StateOfLegalResidence = fafsaEntity.StateOfLegalResidence;
                }

                // Update based on PROF fafsa record.
                if (financialAidApplicationDto.Source == FinancialAidApplicationsSource.Profile)
                {
                    UpdateDtoFromProfileFafsa(fafsaEntity, financialAidApplicationDto);
                }
                
            }
            catch (Exception e)             
            {
                var errorMessage = string.Format("Unable to build DTO for application '{0}'", fafsaEntity.Guid);
                throw new ArgumentException(errorMessage);
            }
            return financialAidApplicationDto;
        }

        private static void BuildDtoFromThisFafsa(Domain.Student.Entities.Fafsa fafsaEntity, Dtos.FinancialAidApplication financialAidApplicationDto)
        {
            // Build DTO from corrected or current FAFSA record.
            BuildWorkStudentInterestFromThisFafsa(fafsaEntity, financialAidApplicationDto);
            BuildIndependenceCriteriaFromThisFafsa(fafsaEntity, financialAidApplicationDto);
            BuildHousingPreferenceFromThisFafsa(fafsaEntity, financialAidApplicationDto);
            BuildApplicantIncomeFromThisFafsa(fafsaEntity, financialAidApplicationDto);
            BuildCustodialParentIncomeFromThisFafsa(fafsaEntity, financialAidApplicationDto);
        }

        private static void UpdateDtoFromProfileFafsa(Domain.Student.Entities.Fafsa fafsaEntity, Dtos.FinancialAidApplication financialAidApplicationDto)
        {
            // Set based on PROF record.
            if (fafsaEntity.ApplicationCompletedOnProfile != null)
            {
                financialAidApplicationDto.ApplicationCompletedOn = fafsaEntity.ApplicationCompletedOnProfile;
            }
            if (!string.IsNullOrEmpty(fafsaEntity.StateOfLegalResidenceProfile))
            {
                financialAidApplicationDto.StateOfLegalResidence = fafsaEntity.StateOfLegalResidenceProfile;
            }
            BuildIndependenceCriteriaFromProfileFafsa(fafsaEntity, financialAidApplicationDto);
            UpdateApplicantIncomeFromProfileFafsa(fafsaEntity, financialAidApplicationDto);
            UpdateCustodialParentIncomeFromProfileFafsa(fafsaEntity, financialAidApplicationDto);
            BuildNonCustodialParentIncomeFromProfileFafsa(fafsaEntity, financialAidApplicationDto);
        }

        private static void BuildWorkStudentInterestFromThisFafsa(Domain.Student.Entities.Fafsa fafsaEntity, Dtos.FinancialAidApplication financialAidApplicationDto)
        {
            // Set work study interest
            if (fafsaEntity.WorkStudyInterest != null)
            {
                switch (fafsaEntity.WorkStudyInterest)
                {
                    case ("1"):
                        financialAidApplicationDto.WorkStudy = FinancialAidApplicationsInterest.Interested;
                        break;
                    case ("2"):
                        financialAidApplicationDto.WorkStudy = FinancialAidApplicationsInterest.NotInterested;
                        break;
                }
            }
        }

        private static void BuildIndependenceCriteriaFromThisFafsa(Domain.Student.Entities.Fafsa fafsaEntity, Dtos.FinancialAidApplication financialAidApplicationDto)
        {
            var independenceCriteria = new List<FinancialAidApplicationsIndependenceCriteria>();
            if (fafsaEntity.IsAtRiskHomeless == true)
            {
                independenceCriteria.Add(FinancialAidApplicationsIndependenceCriteria.RiskOfHomelessness);
            }
            if (fafsaEntity.IsAdvancedDegreeStudent == true)
            {
                independenceCriteria.Add(FinancialAidApplicationsIndependenceCriteria.AdvancedDegreeStudent);
            }
            if (fafsaEntity.HasDependentChildren == true)
            {
                independenceCriteria.Add(FinancialAidApplicationsIndependenceCriteria.SupportingChildren);
            }
            if (fafsaEntity.HasOtherDependents == true)
            {
                independenceCriteria.Add(FinancialAidApplicationsIndependenceCriteria.SupportingDependents);
            }
            if (fafsaEntity.IsOrphanOrWard == true)
            {
                independenceCriteria.Add(FinancialAidApplicationsIndependenceCriteria.OrphanWardOfCourtFosterCare);
            }
            if (fafsaEntity.IsEmancipatedMinor == true)
            {
                independenceCriteria.Add(FinancialAidApplicationsIndependenceCriteria.EmancipatedMinor);
            }
            if (fafsaEntity.HasGuardian == true)
            {
                independenceCriteria.Add(FinancialAidApplicationsIndependenceCriteria.LegalGuardianship);
            }
            if (fafsaEntity.IsHomelessBySchool == true)
            {
                independenceCriteria.Add(FinancialAidApplicationsIndependenceCriteria.UnaccompaniedYouthBySchool);
            }
            if (fafsaEntity.IsHomelessByHud == true)
            {
                independenceCriteria.Add(FinancialAidApplicationsIndependenceCriteria.UnaccompaniedYouthByHud);
            }
            if (fafsaEntity.IsBornBeforeDate == true)
            {
                independenceCriteria.Add(FinancialAidApplicationsIndependenceCriteria.IndependentAge);
            }
            if (fafsaEntity.IsMarried == true)
            {
                independenceCriteria.Add(FinancialAidApplicationsIndependenceCriteria.Married);
            }
            if (fafsaEntity.IsVeteran == true)
            {
                independenceCriteria.Add(FinancialAidApplicationsIndependenceCriteria.Veteran);
            }
            if (fafsaEntity.IsActiveDuty == true)
            {
                independenceCriteria.Add(FinancialAidApplicationsIndependenceCriteria.ActiveDuty);
            }
            if (independenceCriteria.Any())
            {
                financialAidApplicationDto.IndependenceCriteria = independenceCriteria;
            }
        }

        private static void BuildHousingPreferenceFromThisFafsa(Domain.Student.Entities.Fafsa fafsaEntity, Dtos.FinancialAidApplication financialAidApplicationDto)
        {
            // Set housing preference
            if (fafsaEntity.HousingCode != null)
            {
                switch (fafsaEntity.HousingCode)
                {
                    case ("1"):
                        financialAidApplicationDto.HousingPreference = FinancialAidApplicationsHousingPreference.OnCampus;
                        break;
                    case ("2"):
                        financialAidApplicationDto.HousingPreference = FinancialAidApplicationsHousingPreference.WithParents;
                        break;
                    case ("3"):
                        financialAidApplicationDto.HousingPreference = FinancialAidApplicationsHousingPreference.OffCampus;
                        break;
                }
            }
        }

        private static void BuildApplicantIncomeFromThisFafsa(Domain.Student.Entities.Fafsa fafsaEntity, Dtos.FinancialAidApplication financialAidApplicationDto)
        {
            // Set applicant total income
            var totalIncome = new Dtos.DtoProperties.AmountDtoProperty();
            if (financialAidApplicationDto.Source == FinancialAidApplicationsSource.Profile)
            {
                if (fafsaEntity.StudentTotalIncomeProfileCorrected != null)
                {
                    totalIncome.Value = fafsaEntity.StudentTotalIncomeProfileCorrected;
                }
                else
                {
                    totalIncome.Value = fafsaEntity.StudentTotalIncomeProfileOrig;
                }
            }
            else
            {
                if (fafsaEntity.StudentDependencyStatus == "D" || fafsaEntity.StudentDependencyStatus == "X")
                {
                    totalIncome.Value = fafsaEntity.StudentTotalIncome;
                }
                if (fafsaEntity.StudentDependencyStatus == "I" || fafsaEntity.StudentDependencyStatus == "Y")
                {
                    totalIncome.Value = fafsaEntity.PrimaryTotalIncome;
                }
            }
            if (totalIncome.Value != null)
            {
                totalIncome.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                financialAidApplicationDto.ApplicantIncome = new Dtos.DtoProperties.FinancialAidApplicationApplicantIncomeDtoProperty();
                financialAidApplicationDto.ApplicantIncome.TotalIncome = totalIncome;

                // Set applicant tax return status
                if (fafsaEntity.StudentTaxReturnStatus != null)
                {
                    switch (fafsaEntity.StudentTaxReturnStatus)
                    {
                        case ("1"):
                            financialAidApplicationDto.ApplicantIncome.taxReturn = FinancialAidApplicationsTaxReturnStatus.Filed;
                            break;
                        case ("2"):
                            financialAidApplicationDto.ApplicantIncome.taxReturn = FinancialAidApplicationsTaxReturnStatus.WillFile;
                            break;
                        case ("3"):
                            financialAidApplicationDto.ApplicantIncome.taxReturn = FinancialAidApplicationsTaxReturnStatus.WillNotFile;
                            break;
                    }
                }

                // Set applicant adjusted income
                var adjustedIncome = new Dtos.DtoProperties.AmountDtoProperty();
                if (fafsaEntity.StudentAdjustedGrossIncome != null)
                {
                    adjustedIncome.Value = fafsaEntity.StudentAdjustedGrossIncome;
                    adjustedIncome.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                    financialAidApplicationDto.ApplicantIncome.AdjustedGrossIncome = adjustedIncome;
                }

                // Set applicant earned income
                var earnedIncome = new Dtos.DtoProperties.AmountDtoProperty();
                if (fafsaEntity.StudentEarnedIncome != null)
                {
                    earnedIncome.Value = fafsaEntity.StudentEarnedIncome;
                    earnedIncome.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                    financialAidApplicationDto.ApplicantIncome.EarnedIncome = earnedIncome;
                }

                // Set spouse earned income
                var spouseEarnedIncome = new Dtos.DtoProperties.AmountDtoProperty();
                if (fafsaEntity.SpouseEarnedIncome != null)
                {
                    spouseEarnedIncome.Value = fafsaEntity.SpouseEarnedIncome;
                    spouseEarnedIncome.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                    financialAidApplicationDto.ApplicantIncome.SpouseEarnedIncome = spouseEarnedIncome;
                }
            }
        }

        private static void BuildCustodialParentIncomeFromThisFafsa(Domain.Student.Entities.Fafsa fafsaEntity, Dtos.FinancialAidApplication financialAidApplicationDto)
        {
            // Set custodial parent total income
            var totalParentIncome = new Dtos.DtoProperties.AmountDtoProperty();
            if (financialAidApplicationDto.Source == FinancialAidApplicationsSource.Profile)
            {
                if (fafsaEntity.ParentTotalIncomeProfileCorrected != null)
                {
                    totalParentIncome.Value = fafsaEntity.ParentTotalIncomeProfileCorrected;
                }
                else
                {
                    totalParentIncome.Value = fafsaEntity.ParentTotalIncomeProfileOrig;
                }
            }
            else
            {
                if (fafsaEntity.StudentDependencyStatus == "D" || fafsaEntity.StudentDependencyStatus == "X")
                {
                    totalParentIncome.Value = fafsaEntity.ParentPrimaryTotalIncome;
                }
            }
            if (totalParentIncome.Value != null)
            {
                totalParentIncome.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                financialAidApplicationDto.CustodialParentsIncome = new Dtos.DtoProperties.FinancialAidApplicationCustodialParentsIncomeDtoProperty();
                financialAidApplicationDto.CustodialParentsIncome.TotalIncome = totalParentIncome;

                // Set custodial parent tax return status
                if (fafsaEntity.ParentTaxReturnStatus != null)
                {
                    switch (fafsaEntity.ParentTaxReturnStatus)
                    {
                        case ("1"):
                            financialAidApplicationDto.CustodialParentsIncome.taxReturn = FinancialAidApplicationsTaxReturnStatus.Filed;
                            break;
                        case ("2"):
                            financialAidApplicationDto.CustodialParentsIncome.taxReturn = FinancialAidApplicationsTaxReturnStatus.WillFile;
                            break;
                        case ("3"):
                            financialAidApplicationDto.CustodialParentsIncome.taxReturn = FinancialAidApplicationsTaxReturnStatus.WillNotFile;
                            break;
                    }
                }

                // Set custodial parent adjusted income
                var adjustedParentIncome = new Dtos.DtoProperties.AmountDtoProperty();
                if (financialAidApplicationDto.Source != FinancialAidApplicationsSource.Profile)
                {
                    if (fafsaEntity.ParentAdjustedGrossIncome != null)
                    {
                        adjustedParentIncome.Value = fafsaEntity.ParentAdjustedGrossIncome;
                        adjustedParentIncome.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                        financialAidApplicationDto.CustodialParentsIncome.AdjustedGrossIncome = adjustedParentIncome;
                    }

                    // Set custodial parent1 earned income
                    var earnedParent1Income = new Dtos.DtoProperties.AmountDtoProperty();
                    if (fafsaEntity.Parent1EarnedIncome != null)
                    {
                        earnedParent1Income.Value = fafsaEntity.Parent1EarnedIncome;
                        earnedParent1Income.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                        financialAidApplicationDto.CustodialParentsIncome.firstParentEarnedIncome = earnedParent1Income;
                    }

                    // Set custodial parent2 earned income
                    var earnedParent2Income = new Dtos.DtoProperties.AmountDtoProperty();
                    if (fafsaEntity.Parent2EarnedIncome != null)
                    {
                        earnedParent2Income.Value = fafsaEntity.Parent2EarnedIncome;
                        earnedParent2Income.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                        financialAidApplicationDto.CustodialParentsIncome.secondParentEarnedIncome = earnedParent2Income;
                    }

                    // Set custodial parent1 education level
                    if (fafsaEntity.Parent1EducationLevel != null)
                    {
                        switch (fafsaEntity.Parent1EducationLevel)
                        {
                            case ("1"):
                                financialAidApplicationDto.CustodialParentsIncome.FirstParentHighestEducationLevel = FinancialAidApplicationsEducationLevel.MiddleSchool;
                                break;
                            case ("2"):
                                financialAidApplicationDto.CustodialParentsIncome.FirstParentHighestEducationLevel = FinancialAidApplicationsEducationLevel.HighSchool;
                                break;
                            case ("3"):
                                financialAidApplicationDto.CustodialParentsIncome.FirstParentHighestEducationLevel = FinancialAidApplicationsEducationLevel.College;
                                break;
                            case ("4"):
                                financialAidApplicationDto.CustodialParentsIncome.FirstParentHighestEducationLevel = FinancialAidApplicationsEducationLevel.Other;
                                break;
                        }
                    }

                    // Set custodial parent2 education level
                    if (fafsaEntity.Parent2EducationLevel != null)
                    {
                        switch (fafsaEntity.Parent2EducationLevel)
                        {
                            case ("1"):
                                financialAidApplicationDto.CustodialParentsIncome.SecondParentHighestEducationLevel = FinancialAidApplicationsEducationLevel.MiddleSchool;
                                break;
                            case ("2"):
                                financialAidApplicationDto.CustodialParentsIncome.SecondParentHighestEducationLevel = FinancialAidApplicationsEducationLevel.HighSchool;
                                break;
                            case ("3"):
                                financialAidApplicationDto.CustodialParentsIncome.SecondParentHighestEducationLevel = FinancialAidApplicationsEducationLevel.College;
                                break;
                            case ("4"):
                                financialAidApplicationDto.CustodialParentsIncome.SecondParentHighestEducationLevel = FinancialAidApplicationsEducationLevel.Other;
                                break;
                        }
                    }
                }
            }
        }
        
        private static void BuildIndependenceCriteriaFromProfileFafsa(Domain.Student.Entities.Fafsa fafsaEntity, Dtos.FinancialAidApplication financialAidApplicationDto)
        {
            var independenceCriteria = new List<FinancialAidApplicationsIndependenceCriteria>();
            if (fafsaEntity.HasDependentChildrenProfile == true)
            {
                independenceCriteria.Add(FinancialAidApplicationsIndependenceCriteria.SupportingDependents);
            }
            if (fafsaEntity.IsWardProfile == true)
            {
                independenceCriteria.Add(FinancialAidApplicationsIndependenceCriteria.OrphanWardOfCourtFosterCare);
            }
            if (fafsaEntity.IsHomelessProfile == true)
            {
                independenceCriteria.Add(FinancialAidApplicationsIndependenceCriteria.RiskOfHomelessness);
            }
            if (fafsaEntity.IsVeteranProfile == true)
            {
                independenceCriteria.Add(FinancialAidApplicationsIndependenceCriteria.Veteran);
            }
            if (fafsaEntity.MaritalStatusProfile == "2" || fafsaEntity.MaritalStatusProfile == "3")
            {
                independenceCriteria.Add(FinancialAidApplicationsIndependenceCriteria.Married);
            }
            if (independenceCriteria != null)
            {
                financialAidApplicationDto.IndependenceCriteria = independenceCriteria;
            }
        }

        private static void UpdateApplicantIncomeFromProfileFafsa(Domain.Student.Entities.Fafsa fafsaEntity, Dtos.FinancialAidApplication financialAidApplicationDto)
        {
            if (financialAidApplicationDto.ApplicantIncome != null)
            {
                // Set applicant income
                // Set applicant tax return status
                if (fafsaEntity.StudentTaxReturnStatusProfile != null)
                {
                    switch (fafsaEntity.StudentTaxReturnStatusProfile)
                    {
                        case ("1"):
                            financialAidApplicationDto.ApplicantIncome.taxReturn = FinancialAidApplicationsTaxReturnStatus.Filed;
                            break;
                        case ("2"):
                            financialAidApplicationDto.ApplicantIncome.taxReturn = FinancialAidApplicationsTaxReturnStatus.WillFile;
                            break;
                        case ("3"):
                            financialAidApplicationDto.ApplicantIncome.taxReturn = FinancialAidApplicationsTaxReturnStatus.WillNotFile;
                            break;
                    }
                }

                // Set applicant adjusted income
                var adjustedIncome = new Dtos.DtoProperties.AmountDtoProperty();
                if (fafsaEntity.StudentAdjustedGrossIncomeProfile != null)
                {
                    adjustedIncome.Value = fafsaEntity.StudentAdjustedGrossIncomeProfile;
                    adjustedIncome.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                    financialAidApplicationDto.ApplicantIncome.AdjustedGrossIncome = adjustedIncome;
                }

                // Set applicant earned income
                var earnedIncome = new Dtos.DtoProperties.AmountDtoProperty();
                if (fafsaEntity.StudentEarnedIncomeProfile != null)
                {
                    earnedIncome.Value = fafsaEntity.StudentEarnedIncomeProfile;
                    earnedIncome.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                    financialAidApplicationDto.ApplicantIncome.EarnedIncome = earnedIncome;
                }

                // Set spouse earned income
                var spouseEarnedIncome = new Dtos.DtoProperties.AmountDtoProperty();
                if (fafsaEntity.SpouseEarnedIncomeProfile != null)
                {
                    spouseEarnedIncome.Value = fafsaEntity.SpouseEarnedIncomeProfile;
                    spouseEarnedIncome.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                    financialAidApplicationDto.ApplicantIncome.SpouseEarnedIncome = spouseEarnedIncome;
                }
            }
        }

        private static void UpdateCustodialParentIncomeFromProfileFafsa(Domain.Student.Entities.Fafsa fafsaEntity, Dtos.FinancialAidApplication financialAidApplicationDto)
        {
            if (financialAidApplicationDto.CustodialParentsIncome != null)
            {
                // Set custodial parent tax return status
                if (fafsaEntity.ParentTaxReturnStatusProfile != null)
                {
                    switch (fafsaEntity.ParentTaxReturnStatusProfile)
                    {
                        case ("1"):
                            financialAidApplicationDto.CustodialParentsIncome.taxReturn = FinancialAidApplicationsTaxReturnStatus.Filed;
                            break;
                        case ("2"):
                            financialAidApplicationDto.CustodialParentsIncome.taxReturn = FinancialAidApplicationsTaxReturnStatus.WillFile;
                            break;
                        case ("3"):
                            financialAidApplicationDto.CustodialParentsIncome.taxReturn = FinancialAidApplicationsTaxReturnStatus.WillNotFile;
                            break;
                    }
                }

                // Set custodial parent adjusted income
                var adjustedParentIncome = new Dtos.DtoProperties.AmountDtoProperty();
                if (fafsaEntity.ParentAdjustedGrossIncomeProfile != null)
                {
                    adjustedParentIncome.Value = fafsaEntity.ParentAdjustedGrossIncomeProfile;
                    adjustedParentIncome.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                    financialAidApplicationDto.CustodialParentsIncome.AdjustedGrossIncome = adjustedParentIncome;
                }

                // Set custodial parent1 earned income
                var earnedParent1Income = new Dtos.DtoProperties.AmountDtoProperty();
                if (fafsaEntity.Parent1EarnedIncomeProfile != null)
                {
                    earnedParent1Income.Value = fafsaEntity.Parent1EarnedIncomeProfile;
                    earnedParent1Income.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                    financialAidApplicationDto.CustodialParentsIncome.firstParentEarnedIncome = earnedParent1Income;
                }

                // Set custodial parent2 earned income
                var earnedParent2Income = new Dtos.DtoProperties.AmountDtoProperty();
                if (fafsaEntity.Parent2EarnedIncomeProfile != null)
                {
                    earnedParent2Income.Value = fafsaEntity.Parent2EarnedIncomeProfile;
                    earnedParent2Income.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                    financialAidApplicationDto.CustodialParentsIncome.secondParentEarnedIncome = earnedParent2Income;
                }

                // Set custodial parent1 education level
                if (fafsaEntity.Parent1EducationLevelProfile != null)
                {
                    switch (fafsaEntity.Parent1EducationLevelProfile)
                    {
                        case ("1"):
                            financialAidApplicationDto.CustodialParentsIncome.FirstParentHighestEducationLevel = FinancialAidApplicationsEducationLevel.MiddleSchool;
                            break;
                        case ("2"):
                            financialAidApplicationDto.CustodialParentsIncome.FirstParentHighestEducationLevel = FinancialAidApplicationsEducationLevel.HighSchool;
                            break;
                        case ("3"):
                            financialAidApplicationDto.CustodialParentsIncome.FirstParentHighestEducationLevel = FinancialAidApplicationsEducationLevel.College;
                            break;
                        case ("4"):
                            financialAidApplicationDto.CustodialParentsIncome.FirstParentHighestEducationLevel = FinancialAidApplicationsEducationLevel.Other;
                            break;
                    }
                }

                // Set custodial parent2 education level
                if (fafsaEntity.Parent2EducationLevelProfile != null)
                {
                    switch (fafsaEntity.Parent2EducationLevelProfile)
                    {
                        case ("1"):
                            financialAidApplicationDto.CustodialParentsIncome.SecondParentHighestEducationLevel = FinancialAidApplicationsEducationLevel.MiddleSchool;
                            break;
                        case ("2"):
                            financialAidApplicationDto.CustodialParentsIncome.SecondParentHighestEducationLevel = FinancialAidApplicationsEducationLevel.HighSchool;
                            break;
                        case ("3"):
                            financialAidApplicationDto.CustodialParentsIncome.SecondParentHighestEducationLevel = FinancialAidApplicationsEducationLevel.College;
                            break;
                        case ("4"):
                            financialAidApplicationDto.CustodialParentsIncome.SecondParentHighestEducationLevel = FinancialAidApplicationsEducationLevel.Other;
                            break;
                    }
                }
            }
        }

        private static void BuildNonCustodialParentIncomeFromProfileFafsa(Domain.Student.Entities.Fafsa fafsaEntity, Dtos.FinancialAidApplication financialAidApplicationDto)
        {
            // Set noncustodial parent total income
            var totalNoncustodialParentIncome = new Dtos.DtoProperties.AmountDtoProperty();
            if (fafsaEntity.NoncustodialParentTotalIncomeProfile != null)
            {
                financialAidApplicationDto.NoncustodialParentsIncome = new Dtos.DtoProperties.FinancialAidApplicationNoncustodialParentsIncomeDtoProperty();
                totalNoncustodialParentIncome.Value = fafsaEntity.NoncustodialParentTotalIncomeProfile;
                totalNoncustodialParentIncome.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                financialAidApplicationDto.NoncustodialParentsIncome.TotalIncome = totalNoncustodialParentIncome;
                
                // Set noncustodial parent tax return status
                if (fafsaEntity.ParentTaxReturnStatusProfileNcp != null)
                {
                    switch (fafsaEntity.ParentTaxReturnStatusProfileNcp)
                    {
                        case ("1"):
                            financialAidApplicationDto.NoncustodialParentsIncome.taxReturn = FinancialAidApplicationsTaxReturnStatus.Filed;
                            break;
                        case ("2"):
                            financialAidApplicationDto.NoncustodialParentsIncome.taxReturn = FinancialAidApplicationsTaxReturnStatus.WillFile;
                            break;
                        case ("3"):
                            financialAidApplicationDto.NoncustodialParentsIncome.taxReturn = FinancialAidApplicationsTaxReturnStatus.WillNotFile;
                            break;
                    }
                }
                
                // Set noncustodial parent adjusted gross income
                var adjustedGrossNoncustodialParentIncome = new Dtos.DtoProperties.AmountDtoProperty();
                if (fafsaEntity.ParentAdjustedGrossIncomeProfileNcp != null)
                {
                    adjustedGrossNoncustodialParentIncome.Value = fafsaEntity.ParentAdjustedGrossIncomeProfileNcp;
                    adjustedGrossNoncustodialParentIncome.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                    financialAidApplicationDto.NoncustodialParentsIncome.AdjustedGrossIncome = adjustedGrossNoncustodialParentIncome;
                }

                // Set noncustodial parent1 earned income
                var earnedNoncustodialParent1Income = new Dtos.DtoProperties.AmountDtoProperty();
                if (fafsaEntity.NoncustodialParent1EarnedIncomeProfile != null)
                {
                    earnedNoncustodialParent1Income.Value = fafsaEntity.NoncustodialParent1EarnedIncomeProfile;
                    earnedNoncustodialParent1Income.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                    financialAidApplicationDto.NoncustodialParentsIncome.firstParentEarnedIncome = earnedNoncustodialParent1Income;
                }

                // Set noncustodial parent2 earned income
                var earnedNoncustodialParent2Income = new Dtos.DtoProperties.AmountDtoProperty();
                if (fafsaEntity.NoncustodialParent2EarnedIncomeProfile != null)
                {
                    earnedNoncustodialParent2Income.Value = fafsaEntity.NoncustodialParent2EarnedIncomeProfile;
                    earnedNoncustodialParent2Income.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                    financialAidApplicationDto.NoncustodialParentsIncome.secondParentEarnedIncome = earnedNoncustodialParent2Income;
                }
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view Student FinancialAidAwards.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewFinancialAidApplicationsPermission()
        {
            bool hasPermission = HasPermission(StudentPermissionCodes.ViewFinancialAidApplications);

            // User is not allowed to read FinancialAidApplications without the appropriate permissions
            if (!hasPermission)
            {
                throw new PermissionsException(string.Format("User {0} does not have permission to view FinancialAidApplications.", CurrentUser.UserId));
            }
        }
    }
}
