//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
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
    public class FinancialAidFundsService : BaseCoordinationService, IFinancialAidFundsService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IFinancialAidFundRepository _fundRepository;
        private readonly IStudentFinancialAidOfficeRepository _officeRepository;
        private readonly IConfigurationRepository configurationRepository;

        public FinancialAidFundsService(

            IStudentReferenceDataRepository referenceDataRepository,
            IFinancialAidFundRepository fundRepository,
            IStudentFinancialAidOfficeRepository officeRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            this.configurationRepository = configurationRepository;
            _referenceDataRepository = referenceDataRepository;
            _fundRepository = fundRepository;
            _officeRepository = officeRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Gets all financial-aid-funds
        /// </summary>
        /// <returns>Collection of FinancialAidFunds DTO objects</returns>
        public async Task<Tuple<IEnumerable<Dtos.FinancialAidFunds>, int>> GetFinancialAidFundsAsync(int offset, int limit, Dtos.Filters.FinancialAidFundsFilter criteriaFilter, bool bypassCache = false)
        {
            var financialAidFundsCollection = new List<Dtos.FinancialAidFunds>();

            string code = "", source = "", aidType = "", awardCategory = "";
            List<string> classifications = new List<string>();
            if (criteriaFilter != null)
            {
                if (!string.IsNullOrEmpty(criteriaFilter.Code)) code = criteriaFilter.Code;
                if (criteriaFilter.Source != Dtos.EnumProperties.FinancialAidFundsSource.NotSet) source = criteriaFilter.Source.ToString();
                if (criteriaFilter.AidType != Dtos.EnumProperties.FinancialAidFundsAidType.NotSet) aidType = criteriaFilter.AidType.ToString();
                var classificationGuids = criteriaFilter.Classifications;

                if (classificationGuids != null && classificationGuids.Any())
                {
                    var financialAidFundClassifications = await _referenceDataRepository.GetFinancialAidFundClassificationsAsync(bypassCache);

                    if (financialAidFundClassifications != null && financialAidFundClassifications.Any())
                    {
                        foreach (var classGuid in classificationGuids)
                        {
                            if (classGuid != null && !string.IsNullOrEmpty(classGuid.Id))
                            {
                                var faClass = financialAidFundClassifications.Where(a => a.Guid == classGuid.Id).FirstOrDefault();

                                if (faClass != null && !string.IsNullOrEmpty(faClass.FundingTypeCode))
                                {
                                    classifications.Add(faClass.FundingTypeCode);
                                }
                                else
                                {
                                    return new Tuple<IEnumerable<FinancialAidFunds>, int>(new List<Dtos.FinancialAidFunds>(), 0);
                                }
                            }
                        }
                    }

                    if (classifications == null || !classifications.Any())
                    {
                        return new Tuple<IEnumerable<FinancialAidFunds>, int>(new List<Dtos.FinancialAidFunds>(), 0);
                    }
                }

                var categories = criteriaFilter.Category;
                if (categories != null)
                {
                    var financialAidFundAwardCategories = await FinancialAidFundCategories(bypassCache);
                    if (financialAidFundAwardCategories != null && financialAidFundAwardCategories.Any())
                    {
                        if (categories.CategoryName != null && categories.CategoryName != Dtos.EnumProperties.FinancialAidFundAidCategoryType.NotSet)
                        {
                            var categoryName = ConvertFinancialAidFundsAwardCategoryDtoEnumToFinancialAidFundsCategoryDomainEnum(categories.CategoryName);
                            var awardCategories = financialAidFundAwardCategories.Where(a => a.AwardCategoryName == categoryName);
                            if (awardCategories != null && awardCategories.Any())
                            {
                                foreach (var faCat in awardCategories)
                                {
                                    awardCategory = string.Concat(awardCategory, "'", faCat.Code, "'");
                                }
                            }
                            else
                            {
                                return new Tuple<IEnumerable<FinancialAidFunds>, int>(new List<Dtos.FinancialAidFunds>(), 0);
                            }
                        }
                        else
                        {
                            if (categories != null && categories.Detail != null && !string.IsNullOrEmpty(categories.Detail.Id))
                            {
                                var faCat = financialAidFundAwardCategories.Where(a => a.Guid == categories.Detail.Id).FirstOrDefault();
                                if (faCat != null)
                                {
                                    awardCategory = string.Concat("'", faCat.Code, "'");
                                }
                                else
                                {
                                    return new Tuple<IEnumerable<FinancialAidFunds>, int>(new List<Dtos.FinancialAidFunds>(), 0);
                                }
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(awardCategory))
                    {
                        return new Tuple<IEnumerable<FinancialAidFunds>, int>(new List<Dtos.FinancialAidFunds>(), 0);
                    }
                }
            }

            try
            {
                var pageOfItems = await _fundRepository.GetFinancialAidFundsAsync(offset, limit, code, source, aidType, classifications, awardCategory, bypassCache);

                var financialAidFundsEntities = pageOfItems.Item1;
                int totalRecords = pageOfItems.Item2;

                var aidYearsEntities = await _referenceDataRepository.GetFinancialAidYearsAsync(bypassCache);
                var aidYears = aidYearsEntities.Where(ay => ay.status != "D").Select(ay => ay.Code).Distinct().ToList();
                var hostCountry = await _referenceDataRepository.GetHostCountryAsync();

                var financialAidFundFinancials = await _fundRepository.GetFinancialAidFundFinancialsAsync("", aidYears, hostCountry);

                if (financialAidFundsEntities != null && financialAidFundsEntities.Any())
                {
                    foreach (var financialAidFunds in financialAidFundsEntities)
                    {
                        financialAidFundsCollection.Add(await ConvertFinancialAidFundsEntityToDto(financialAidFunds, financialAidFundFinancials, bypassCache));
                    }
                    return new Tuple<IEnumerable<Dtos.FinancialAidFunds>, int>(financialAidFundsCollection, totalRecords);
                }
                else
                {
                    return new Tuple<IEnumerable<FinancialAidFunds>, int>(new List<Dtos.FinancialAidFunds>(), 0);
                }
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Get a FinancialAidFunds from its GUID
        /// </summary>
        /// <returns>FinancialAidFunds DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.FinancialAidFunds> GetFinancialAidFundsByGuidAsync(string guid)
        {
            try
            {
                var entity = await _fundRepository.GetFinancialAidFundByIdAsync(guid);
                //var entity = (await _fundRepository.GetFinancialAidFundsAsync(true)).Where(r => r.Guid == guid).First();

                var aidYearsEntities = await _referenceDataRepository.GetFinancialAidYearsAsync(true);
                var aidYears = aidYearsEntities.Where(ay => ay.status != "D").Select(ay => ay.Code).Distinct().ToList();
                var hostCountry = await _referenceDataRepository.GetHostCountryAsync();

                var financialAidFundFinancials = await _fundRepository.GetFinancialAidFundFinancialsAsync(entity.Code, aidYears, hostCountry);

                return await ConvertFinancialAidFundsEntityToDto(entity, financialAidFundFinancials, true);
            }
            catch (Exception)
            {
                IntegrationApiExceptionAddError(string.Format("financial-aid-funds not found for GUID '{0}'", guid), "GUID.Not.Found", guid, httpStatusCode: System.Net.HttpStatusCode.NotFound);
                throw IntegrationApiException;
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a FinancialAidFunds domain entity to its corresponding FinancialAidFunds DTO
        /// </summary>
        /// <param name="source">FinancialAidFunds domain entity</param>
        /// <returns>FinancialAidFunds DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.FinancialAidFunds> ConvertFinancialAidFundsEntityToDto(FinancialAidFund source, IEnumerable<FinancialAidFundsFinancialProperty> financialSource, bool bypassCache = false)
        {
            var financialAidFunds = new Ellucian.Colleague.Dtos.FinancialAidFunds();

            financialAidFunds.Id = source.Guid;
            financialAidFunds.Code = source.Code;
            financialAidFunds.Title = source.Description;
            if (!string.IsNullOrEmpty(source.Description2))
            {
                financialAidFunds.Description = source.Description2;
            }
            financialAidFunds.Source = ConvertFinancialAidFundsSourceDomainEnumToFinancialAidFundsSourceDtoEnum(source.Source);

            var financialAidFundAwardCategories = await FinancialAidFundCategories(bypassCache);

            Domain.Student.Entities.FinancialAidFundCategory faCat = null;
            if (financialAidFundAwardCategories != null && financialAidFundAwardCategories.Any())
            {
                faCat = financialAidFundAwardCategories.Where(a => a.Code == source.CategoryCode).FirstOrDefault();
            }

           
            if (faCat != null) {
                financialAidFunds.Category = new Dtos.DtoProperties.FinancialAidFundsCategoryProperty();
                financialAidFunds.Category.Detail = new GuidObject2(faCat.Guid);
                financialAidFunds.Category.CategoryName = ConvertFinancialAidFundsAwardCategoryDomainEnumToFinancialAidFundsCategoryDtoEnum(faCat.AwardCategoryName);
                if (faCat.AwardCategoryType != null)
                {
                    financialAidFunds.AidType = ConvertFinancialAidFundsAidTypeDomainEnumToFinancialAidFundsAidTypeDtoEnum(faCat.AwardCategoryType);
                }
                financialAidFunds.Privacy = faCat.restrictedFlag ? Dtos.EnumProperties.FinancialAidFundsPrivacy.Restricted : Dtos.EnumProperties.FinancialAidFundsPrivacy.Nonrestricted;
            }

            var financialAidFundClassifications = await _referenceDataRepository.GetFinancialAidFundClassificationsAsync(bypassCache);
           
            Domain.Student.Entities.FinancialAidFundClassification faClass = null;
            if (financialAidFundClassifications != null && financialAidFundClassifications.Any())
            {
                faClass = financialAidFundClassifications.Where(a => a.FundingTypeCode == source.FundingType).FirstOrDefault();
            }

            if (faClass != null && !string.IsNullOrEmpty(faClass.Guid))
            {
                financialAidFunds.Classifications = new List<GuidObject2>();
                financialAidFunds.Classifications.Add(new GuidObject2(faClass.Guid));
            }

            var financialAidFundFinancials = financialSource.Where(f => f.AwardCode == source.Code);

            if (financialAidFundFinancials != null && financialAidFundFinancials.Any())
            {
                financialAidFunds.Financials = new List<Dtos.DtoProperties.FinancialAidFundsFinancialProperty>();

                foreach (var financials in financialAidFundFinancials)
                {
                    financialAidFunds.Financials.Add(await ConvertFinancialAidFundFinancialsEntityToDto(financials));
                }
            }
                                    
            return financialAidFunds;
        }
       

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a FinancialAidFunds domain entity to its corresponding FinancialAidFunds DTO
        /// </summary>
        /// <param name="source">FinancialAidFunds domain entity</param>
        /// <returns>FinancialAidFunds DTO</returns>
        private async Task<Dtos.DtoProperties.FinancialAidFundsFinancialProperty> ConvertFinancialAidFundFinancialsEntityToDto(Domain.Student.Entities.FinancialAidFundsFinancialProperty source)
        {
            var financialDtos = new Dtos.DtoProperties.FinancialAidFundsFinancialProperty();

            financialDtos.AidYear = await ConvertEntityToAidYearGuidObjectAsync(source.AidYear);
            financialDtos.Office = await ConvertEntityToOfficeGuidObjectAsync(source.Office);
            financialDtos.BudgetedAmount = new Dtos.DtoProperties.FinancialDtoProperty();
            financialDtos.BudgetedAmount.Value = source.BudgetedAmount;
            financialDtos.BudgetedAmount.Currency = (await _referenceDataRepository.GetHostCountryAsync()).ToUpper() == "USA" ? Dtos.EnumProperties.CurrencyCodes.USD : Dtos.EnumProperties.CurrencyCodes.CAD;
            financialDtos.MaximumOfferedBudgetAmount = new Dtos.DtoProperties.FinancialDtoProperty();
            financialDtos.MaximumOfferedBudgetAmount.Value = source.MaximumOfferedBudgetAmount;
            if (source.MaximumOfferedBudgetAmount != null)
            {
                financialDtos.MaximumOfferedBudgetAmount.Currency = (await _referenceDataRepository.GetHostCountryAsync()).ToUpper() == "USA" ? Dtos.EnumProperties.CurrencyCodes.USD : Dtos.EnumProperties.CurrencyCodes.CAD;
            }

            return financialDtos;
        }

        private async Task<GuidObject2> ConvertEntityToAidYearGuidObjectAsync(string sourceCode, bool bypassCache = false)
        {
            if (string.IsNullOrEmpty(sourceCode))
            {
                return null;
            }
            var source = (await _referenceDataRepository.GetFinancialAidYearsAsync(bypassCache)).FirstOrDefault(i => i.Code.Equals(sourceCode, StringComparison.OrdinalIgnoreCase));
            if (source == null)
            {
                throw new KeyNotFoundException("Aid year not found for code " + sourceCode);
            }
            return string.IsNullOrEmpty(source.Guid) ? null : new GuidObject2(source.Guid);
        }

        private async Task<GuidObject2> ConvertEntityToOfficeGuidObjectAsync(string sourceCode, bool bypassCache = false)
        {
            if (string.IsNullOrEmpty(sourceCode))
            {
                return null;
            }
            var source = (await _officeRepository.GetFinancialAidOfficesAsync(bypassCache)).FirstOrDefault(i => i.Code.Equals(sourceCode, StringComparison.OrdinalIgnoreCase));
            if (source == null)
            {
                throw new KeyNotFoundException("Office not found for code " + sourceCode);
            }
            return string.IsNullOrEmpty(source.Guid) ? null : new GuidObject2(source.Guid);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a FinancialAidFundsSource domain enumeration value to its corresponding FinancialAidFundsSource DTO enumeration value
        /// </summary>
        /// <param name="source">FinancialAidFundsSource domain enumeration value</param>
        /// <returns>FinancialAidFundsSource DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.EnumProperties.FinancialAidFundAidCategoryType ConvertFinancialAidFundsAwardCategoryDomainEnumToFinancialAidFundsCategoryDtoEnum(FinancialAidFundAidCategoryType? source)
        {
            switch (source)
            {

                case FinancialAidFundAidCategoryType.AcademicCompetitivenessGrant:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.AcademicCompetitivenessGrant;
                case FinancialAidFundAidCategoryType.BureauOfIndianAffairsFederalGrant:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.BureauOfIndianAffairsFederalGrant;
                case FinancialAidFundAidCategoryType.FederalPerkinsLoan:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.FederalPerkinsLoan;
                case FinancialAidFundAidCategoryType.FederalSubsidizedLoan:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.FederalSubsidizedLoan;
                case FinancialAidFundAidCategoryType.FederalSupplementaryEducationalOpportunityGrant:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.FederalSupplementaryEducationalOpportunityGrant;
                case FinancialAidFundAidCategoryType.FederalSupplementaryLoanForParent:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.FederalSupplementaryLoanForParent;
                case FinancialAidFundAidCategoryType.FederalUnsubsidizedLoan:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.FederalUnsubsidizedLoan;
                case FinancialAidFundAidCategoryType.FederalWorkStudyProgram:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.FederalWorkStudyProgram;
                case FinancialAidFundAidCategoryType.GeneralTitleIVloan:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.GeneralTitleIVloan;
                case FinancialAidFundAidCategoryType.GraduatePlusLoan:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.GraduatePlusLoan;
                case FinancialAidFundAidCategoryType.GraduateTeachingGrant:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.GraduateTeachGrant;
                case FinancialAidFundAidCategoryType.HealthEducationAssistanceLoan:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.HealthEducationAssistanceLoan;
                case FinancialAidFundAidCategoryType.HealthProfessionalStudentLoan:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.HealthProfessionalStudentLoan;
                case FinancialAidFundAidCategoryType.IncomeContingentLoan:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.IncomeContingentLoan;
                case FinancialAidFundAidCategoryType.IraqAfghanistanServiceGrant:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.IraqAfghanistanServiceGrant;
                case FinancialAidFundAidCategoryType.LeveragingEducationalAssistancePartnership:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.LeveragingEducationalAssistancePartnership;
                case FinancialAidFundAidCategoryType.LoanForDisadvantagesStudent:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.LoanForDisadvantagesStudent;
                case FinancialAidFundAidCategoryType.NationalHealthServicesCorpsScholarship:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.NationalHealthServicesCorpsScholarship;
                case FinancialAidFundAidCategoryType.NationalSmartGrant:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.NationalSmartGrant;
                case FinancialAidFundAidCategoryType.NotSet:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.NotSet;
                case FinancialAidFundAidCategoryType.NursingStudentLoan:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.NursingStudentLoan;
                case FinancialAidFundAidCategoryType.ParentPlusLoan:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.ParentPlusLoan;
                case FinancialAidFundAidCategoryType.PaulDouglasTeacherScholarship:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.PaulDouglasTeacherScholarship;
                case FinancialAidFundAidCategoryType.PellGrant:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.PellGrant;
                case FinancialAidFundAidCategoryType.PrimaryCareLoan:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.PrimaryCareLoan;
                case FinancialAidFundAidCategoryType.RobertCByrdScholarshipProgram:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.RobertCByrdScholarshipProgram;
                case FinancialAidFundAidCategoryType.RotcScholarship:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.RotcScholarship;
                case FinancialAidFundAidCategoryType.StateStudentIncentiveGrant:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.StateStudentIncentiveGrant;
                case FinancialAidFundAidCategoryType.StayInSchoolProgram:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.StayInSchoolProgram;
                case FinancialAidFundAidCategoryType.UndergraduateTeachingGrant:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.UndergraduateTeachGrant;
                case FinancialAidFundAidCategoryType.VaHealthProfessionsScholarship:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.VaHealthProfessionsScholarship;
         
                default:
                    return Dtos.EnumProperties.FinancialAidFundAidCategoryType.NonGovernmental;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a FinancialAidFundsSource domain enumeration value to its corresponding FinancialAidFundsSource DTO enumeration value
        /// </summary>
        /// <param name="source">FinancialAidFundsSource domain enumeration value</param>
        /// <returns>FinancialAidFundsSource DTO enumeration value</returns>
        private FinancialAidFundAidCategoryType ConvertFinancialAidFundsAwardCategoryDtoEnumToFinancialAidFundsCategoryDomainEnum(Dtos.EnumProperties.FinancialAidFundAidCategoryType? source)
        {
            switch (source)
            {
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.AcademicCompetitivenessGrant:
                    return FinancialAidFundAidCategoryType.AcademicCompetitivenessGrant;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.BureauOfIndianAffairsFederalGrant:
                    return FinancialAidFundAidCategoryType.BureauOfIndianAffairsFederalGrant;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.FederalPerkinsLoan:
                    return FinancialAidFundAidCategoryType.FederalPerkinsLoan;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.FederalSubsidizedLoan:
                    return FinancialAidFundAidCategoryType.FederalSubsidizedLoan;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.FederalSupplementaryEducationalOpportunityGrant:
                    return FinancialAidFundAidCategoryType.FederalSupplementaryEducationalOpportunityGrant;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.FederalSupplementaryLoanForParent:
                    return FinancialAidFundAidCategoryType.FederalSupplementaryLoanForParent;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.FederalUnsubsidizedLoan:
                    return FinancialAidFundAidCategoryType.FederalUnsubsidizedLoan;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.FederalWorkStudyProgram:
                    return FinancialAidFundAidCategoryType.FederalWorkStudyProgram;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.GeneralTitleIVloan:
                    return FinancialAidFundAidCategoryType.GeneralTitleIVloan;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.GraduatePlusLoan:
                    return FinancialAidFundAidCategoryType.GraduatePlusLoan;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.GraduateTeachGrant:
                    return FinancialAidFundAidCategoryType.GraduateTeachingGrant;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.HealthEducationAssistanceLoan:
                    return FinancialAidFundAidCategoryType.HealthEducationAssistanceLoan;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.HealthProfessionalStudentLoan:
                    return FinancialAidFundAidCategoryType.HealthProfessionalStudentLoan;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.IncomeContingentLoan:
                    return FinancialAidFundAidCategoryType.IncomeContingentLoan;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.IraqAfghanistanServiceGrant:
                    return FinancialAidFundAidCategoryType.IraqAfghanistanServiceGrant;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.LeveragingEducationalAssistancePartnership:
                    return FinancialAidFundAidCategoryType.LeveragingEducationalAssistancePartnership;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.LoanForDisadvantagesStudent:
                    return FinancialAidFundAidCategoryType.LoanForDisadvantagesStudent;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.NationalHealthServicesCorpsScholarship:
                    return FinancialAidFundAidCategoryType.NationalHealthServicesCorpsScholarship;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.NationalSmartGrant:
                    return FinancialAidFundAidCategoryType.NationalSmartGrant;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.NotSet:
                    return FinancialAidFundAidCategoryType.NotSet;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.NursingStudentLoan:
                    return FinancialAidFundAidCategoryType.NursingStudentLoan;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.ParentPlusLoan:
                    return FinancialAidFundAidCategoryType.ParentPlusLoan;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.PaulDouglasTeacherScholarship:
                    return FinancialAidFundAidCategoryType.PaulDouglasTeacherScholarship;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.PellGrant:
                    return FinancialAidFundAidCategoryType.PellGrant;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.PrimaryCareLoan:
                    return FinancialAidFundAidCategoryType.PrimaryCareLoan;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.RobertCByrdScholarshipProgram:
                    return FinancialAidFundAidCategoryType.RobertCByrdScholarshipProgram;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.RotcScholarship:
                    return FinancialAidFundAidCategoryType.RotcScholarship;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.StateStudentIncentiveGrant:
                    return FinancialAidFundAidCategoryType.StateStudentIncentiveGrant;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.StayInSchoolProgram:
                    return FinancialAidFundAidCategoryType.StayInSchoolProgram;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.UndergraduateTeachGrant:
                    return FinancialAidFundAidCategoryType.UndergraduateTeachingGrant;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.VaHealthProfessionsScholarship:
                    return FinancialAidFundAidCategoryType.VaHealthProfessionsScholarship;
                case Dtos.EnumProperties.FinancialAidFundAidCategoryType.NonGovernmental:
                    return FinancialAidFundAidCategoryType.NonGovernmental;

                default:
                    return FinancialAidFundAidCategoryType.NotSet;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a FinancialAidFundsSource domain enumeration value to its corresponding FinancialAidFundsSource DTO enumeration value
        /// </summary>
        /// <param name="source">FinancialAidFundsSource domain enumeration value</param>
        /// <returns>FinancialAidFundsSource DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.EnumProperties.FinancialAidFundsSource ConvertFinancialAidFundsSourceDomainEnumToFinancialAidFundsSourceDtoEnum(string source)
        {
            switch (source)
            {

                case "F":
                    return Dtos.EnumProperties.FinancialAidFundsSource.Federal;
                case "I":
                    return Dtos.EnumProperties.FinancialAidFundsSource.Institutional;
                case "S":
                    return Dtos.EnumProperties.FinancialAidFundsSource.State;
                case "O":
                    return Dtos.EnumProperties.FinancialAidFundsSource.Other;
                default:
                    return Dtos.EnumProperties.FinancialAidFundsSource.Other;
            }
        }
   
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a FinancialAidFundsAidType domain enumeration value to its corresponding FinancialAidFundsAidType DTO enumeration value
        /// </summary>
        /// <param name="source">FinancialAidFundsAidType domain enumeration value</param>
        /// <returns>FinancialAidFundsAidType DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.EnumProperties.FinancialAidFundsAidType ConvertFinancialAidFundsAidTypeDomainEnumToFinancialAidFundsAidTypeDtoEnum(Domain.Student.Entities.AwardCategoryType? source)
        {
            switch (source)
            {

                case Domain.Student.Entities.AwardCategoryType.Loan:
                    return Dtos.EnumProperties.FinancialAidFundsAidType.Loan;
                case Domain.Student.Entities.AwardCategoryType.Grant:
                    return Dtos.EnumProperties.FinancialAidFundsAidType.Grant;
                case Domain.Student.Entities.AwardCategoryType.Scholarship:
                    return Dtos.EnumProperties.FinancialAidFundsAidType.Scholarship;
                case Domain.Student.Entities.AwardCategoryType.Work:
                    return Dtos.EnumProperties.FinancialAidFundsAidType.Work;
                default:
                    return Dtos.EnumProperties.FinancialAidFundsAidType.Loan;
            }
        }

        /// <summary>
        /// Financial Aid Fund Categories
        /// </summary>
        private IEnumerable<Domain.Student.Entities.FinancialAidFundCategory> _financialAidFundCategories;
        private async Task<IEnumerable<Domain.Student.Entities.FinancialAidFundCategory>> FinancialAidFundCategories(bool bypassCache)
        {
            if (_financialAidFundCategories == null)
            {
                _financialAidFundCategories = await _referenceDataRepository.GetFinancialAidFundCategoriesAsync(bypassCache);
            }
            return _financialAidFundCategories;
        }

   }
}