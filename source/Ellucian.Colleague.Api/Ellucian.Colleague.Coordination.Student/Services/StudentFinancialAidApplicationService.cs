//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
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

        #region Get all reference data

        public IEnumerable<Domain.Student.Entities.FinancialAidYear> _financialAidYears;
        private async Task<IEnumerable<Domain.Student.Entities.FinancialAidYear>> GetFinancialAidYearsAsync(bool bypassCache)
        {
            if (_financialAidYears == null)
            {
                _financialAidYears = await _financialAidReferenceDataRepository.GetFinancialAidYearsAsync(bypassCache);
            }
            return _financialAidYears;
        }

        #endregion

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 9</remarks>
        /// <summary>
        /// Gets all financial-aid-applications
        /// </summary>
        /// <returns>Collection of FinancialAidApplications DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplication>, int>> GetAsync(int offset, int limit, Dtos.FinancialAidApplication filterDto, bool bypassCache = false)
        {
            try
            {

                // Get all financial aid years
                var aidYearEntity = await _financialAidReferenceDataRepository.GetFinancialAidYearsAsync( bypassCache );
                List<string> faSuiteYears = aidYearEntity.Select(k => k.Code).ToList();

                //process filters 
                string studentId = string.Empty;
                string aidYear = string.Empty;
                string source = string.Empty;
                string methodology = string.Empty;

                if (filterDto != null)
                {
                    //get aid year
                    if (filterDto.AidYear != null && !string.IsNullOrEmpty(filterDto.AidYear.Id))
                    {
                        aidYear = ConvertGuidToCode(aidYearEntity, filterDto.AidYear.Id);
                        if (string.IsNullOrEmpty(aidYear))
                            return new Tuple<IEnumerable<Dtos.FinancialAidApplication>, int>(new List<Dtos.FinancialAidApplication>(), 0);
                    }
                    //get student Id
                    if (filterDto.Applicant != null && filterDto.Applicant.Person != null && !string.IsNullOrEmpty(filterDto.Applicant.Person.Id))
                    {
                        try
                        {
                            studentId = await _personRepository.GetPersonIdFromGuidAsync(filterDto.Applicant.Person.Id);
                            if (string.IsNullOrEmpty(studentId))
                                return new Tuple<IEnumerable<Dtos.FinancialAidApplication>, int>(new List<Dtos.FinancialAidApplication>(), 0);
                        }
                        catch // if bad guid, return empty set
                        {
                            return new Tuple<IEnumerable<Dtos.FinancialAidApplication>, int>(new List<Dtos.FinancialAidApplication>(), 0);
                        }
                    }

                    if(filterDto.Methodology != FinancialAidApplicationsMethodology.NotSet)
                    {
                        methodology = filterDto.Methodology.ToString().ToLower();
                    }

                    if(filterDto.Source != FinancialAidApplicationsSource.NotSet)
                    {
                        switch( filterDto.Source )
                        {
                            case FinancialAidApplicationsSource.Isir:
                                source = "ISIR";
                                break;
                            case FinancialAidApplicationsSource.Profile:
                                source = "PROF";
                                break;
                            case FinancialAidApplicationsSource.Manualfederal:
                                source = "IAPP";
                                break;
                            case FinancialAidApplicationsSource.Manualinstitution:
                                source = "SUPP";
                                break;
                        }
                    }
                }

                var financialAidApplicationDtos = new List<Dtos.FinancialAidApplication>();
                var fafsaDomainTuple = await _financialAidApplicationRepository.GetAsync(offset, limit, bypassCache, studentId, aidYear, faSuiteYears, methodology, source);

                if (fafsaDomainTuple != null && fafsaDomainTuple.Item1.Any())
                {
                    financialAidApplicationDtos = (await BuildFinancialAidApplicationDtoAsync(fafsaDomainTuple.Item1, bypassCache)).ToList();
                    if( IntegrationApiException != null )
                    {
                        throw IntegrationApiException;
                    }
                    return new Tuple<IEnumerable<Dtos.FinancialAidApplication>, int>(financialAidApplicationDtos, fafsaDomainTuple.Item2);
                }
                else
                {
                    return new Tuple<IEnumerable<Dtos.FinancialAidApplication>, int>(new List<Dtos.FinancialAidApplication>(), 0);
                }
            }
            catch( IntegrationApiException )
            {
                throw;
            }
            catch(PermissionsException)
            {
                throw;
            }
            catch( RepositoryException )
            {
                throw;
            }
            catch( Exception e)
            {
                logger.Error(e.Message);
                throw new Exception(e.Message, e);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 9</remarks>
        /// <summary>
        /// Get a FinancialAidApplications from its GUID
        /// </summary>
        /// <returns>FinancialAidApplications DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.FinancialAidApplication> GetByIdAsync(string id, bool bypassCache = true)
        {
            try
            {
                // Get the student financial aid awards domain entity from the repository
                var fafsaDomainEntity = await _financialAidApplicationRepository.GetByIdAsync(id);
                if (fafsaDomainEntity != null)
                {
                    // Convert the financial aid application object into DTO.
                    var finAidAppDto = (await BuildFinancialAidApplicationDtoAsync(new List<Domain.Student.Entities.Fafsa>() { fafsaDomainEntity }, bypassCache));
                    if( IntegrationApiException != null )
                    {
                        throw IntegrationApiException;
                    }
                    return finAidAppDto != null ? finAidAppDto.FirstOrDefault() : null;
                }
                else
                {
                    throw new KeyNotFoundException("financial-aid-applications not found for GUID " + id);
                }
            }
            catch(ArgumentNullException e)
            {
                IntegrationApiExceptionAddError( e.Message, "GUID.Not.Found", id, string.Empty );
                throw IntegrationApiException;
            }
            catch (KeyNotFoundException e)
            {
                IntegrationApiExceptionAddError( e.Message, "GUID.Not.Found", id, string.Empty, System.Net.HttpStatusCode.NotFound );
                throw IntegrationApiException;
            }
        }

        private async Task<IEnumerable<Dtos.FinancialAidApplication>> BuildFinancialAidApplicationDtoAsync(IEnumerable<Domain.Student.Entities.Fafsa> sources, bool bypassCache = true)
        {
            if ((sources == null) || (!sources.Any()))
            {
                return null;
            }

            var financialAidApplicationDtos = new List<Dtos.FinancialAidApplication>();
            //get person guid collection
            var personIds = sources
                            .Where(x => (!string.IsNullOrEmpty(x.StudentId)))
                            .Select(x => x.StudentId).Distinct().ToList();
            var personGuidCollection = await this._personRepository.GetPersonGuidsCollectionAsync(personIds);

            foreach (var source in sources)
            {
                var dto = new Dtos.FinancialAidApplication();

                dto.Id = source.Guid;
                //
                // Set Applicant
                //
                if (string.IsNullOrEmpty(source.StudentId))
                {
                    IntegrationApiExceptionAddError( string.Format( "Applicant Person Id is required. Entity:'ISIR.FAFSA', Record Id :'{0}'", source.Id ), "Bad.Data", source.Guid, source.Id );
                }
                if (personGuidCollection == null)
                {
                    IntegrationApiExceptionAddError( string.Format( "Unable to locate guid for student ID : {0}. Entity:'ISIR.FAFSA', Record Id :'{1}'", source.StudentId, source.Id ), "GUID.Not.Found", 
                        source.Guid, source.Id );
                    throw IntegrationApiException;
                }
                var personGuid = string.Empty;
                personGuidCollection.TryGetValue(source.StudentId, out personGuid);
                if( string.IsNullOrEmpty( personGuid ) )
                {
                    IntegrationApiExceptionAddError( string.Format( "Unable to locate guid for student ID : {0}. Entity:'ISIR.FAFSA', Record Id: '{1}'", source.StudentId, source.Id ), "GUID.Not.Found",
                        source.Guid, source.Id );
                }
                else
                {
                    dto.Applicant = new FinancialAidApplicationApplicant() { Person = new GuidObject2( personGuid ) };
                }
                //
                // Set AidYear
                //
                if( string.IsNullOrEmpty( source.AwardYear ) )
                {
                    IntegrationApiExceptionAddError( string.Format( "Aid Year Id is required. Entity:'ISIR.FAFSA', Record Id :'{0}'", source.Id ), "Bad.Data",
                        source.Guid, source.Id );
                }
                else
                {
                    var aidYearGuid = ConvertCodeToGuid( await GetFinancialAidYearsAsync( bypassCache ), source.AwardYear );
                    if( !string.IsNullOrEmpty( aidYearGuid ) )
                    {
                        dto.AidYear = new GuidObject2( aidYearGuid );
                    }
                    else
                    {
                        IntegrationApiExceptionAddError( string.Format( "Unable to locate guid for aid year ID : {0}. Entity:'ISIR.FAFSA', Record Id: '{1}'", source.AwardYear, source.Id ), "GUID.Not.Found",
                            source.Guid, source.Id );
                    }
                }

                try
                {
                    // Set Methodology
                    var fafsaId = source.FafsaPrimaryId;
                    if (source.FafsaPrimaryIdCorrected != null)
                    {
                        fafsaId = source.FafsaPrimaryIdCorrected;
                    }


                    dto.Methodology = FinancialAidApplicationsMethodology.NotSet;
                    if (fafsaId == source.CsInstitutionalIsirId)
                    {
                        dto.Methodology = FinancialAidApplicationsMethodology.Institutional;
                    }
                    if (fafsaId == source.CsFederalIsirId)
                    {
                        dto.Methodology = FinancialAidApplicationsMethodology.Federal;
                    }
                    if (fafsaId == source.CsFederalIsirId && fafsaId == source.CsInstitutionalIsirId)
                    {
                        dto.Methodology = FinancialAidApplicationsMethodology.Institutionalfederal;
                    }
                    if (dto.Methodology == FinancialAidApplicationsMethodology.NotSet)
                    {
                        IntegrationApiExceptionAddError( string.Format( "Unable to identify methodology for application outcome '{0}'", source.Guid ), "Bad.Data", source.Guid, source.Id );
                    }

                    // Set source
                    if (source.FafsaPrimaryType != null)
                    {
                        switch (source.FafsaPrimaryType)
                        {
                            case ("ISIR"):
                                dto.Source = FinancialAidApplicationsSource.Isir;
                                break;
                            case ("CPSSG"):
                                dto.Source = FinancialAidApplicationsSource.Isir;
                                break;
                            case ("CORR"):
                                dto.Source = FinancialAidApplicationsSource.Isir;
                                break;
                            case ("PROF"):
                                dto.Source = FinancialAidApplicationsSource.Profile;
                                break;
                            case ("IAPP"):
                                dto.Source = FinancialAidApplicationsSource.Manualfederal;
                                break;
                            case ("SUPP"):
                                dto.Source = FinancialAidApplicationsSource.Manualinstitution;
                                break;
                            default:
                                IntegrationApiExceptionAddError( string.Format( "The source '{0}' is not valid for 'isir', 'profile', 'manualFederal', or 'manualInstitution'", source.FafsaPrimaryType ), "Bad.Data", source.Guid, source.Id );
                                break;
                        }
                    }

                    BuildDtoFromThisFafsa(source, dto);

                    if (source.ApplicationCompletedOn != null)
                    {
                        dto.ApplicationCompletedOn = source.ApplicationCompletedOn;
                    }
                    if (!string.IsNullOrEmpty(source.StateOfLegalResidence))
                    {
                        dto.StateOfLegalResidence = source.StateOfLegalResidence;
                    }

                    // Update based on PROF fafsa record.
                    if (dto.Source == FinancialAidApplicationsSource.Profile)
                    {
                        UpdateDtoFromProfileFafsa(source, dto);
                    }

                    //9.1.0 changes
                    #region maritalStatus
                    /*
                     *  Notes from specs
                        Choose FA.STU.MARITAL.STATUS.20 for (import year 2020+)
                        Choose FA.STU.MARITAL.STATUS.18 for (import year s 2018, 2019)
                        Choose FA.STU.MARITAL.STATUS.14 for (import years 2014, 2015, 2016, 2017)
                        Choose FA.STU.MARITAL.STATUS.10 for (import years 2010, 2011, 2012, 2013) 
                    */
                    if (!string.IsNullOrEmpty( source.Type))
                    {

                        FinancialAidMaritalStatus finAidMarStatus = null;
                        if( source.Type.Equals( "PROF", StringComparison.InvariantCultureIgnoreCase ) && !string.IsNullOrEmpty( source.AwardYear ) && !string.IsNullOrWhiteSpace( source.ProfileMaritalStatus ) )
                        {
                            dto.MaritalStatus = new MaritalStatusDtoProperty();
                            finAidMarStatus = await _financialAidReferenceDataRepository.GetFinancialAidMaritalStatusAsync( source.AwardYearShortValue, source.ProfileMaritalStatus );
                            dto.MaritalStatus.Value = source.ProfileMaritalStatus;
                        }
                        else if( !string.IsNullOrWhiteSpace( source.FafsaMaritalStatus ) )
                        {
                            dto.MaritalStatus = new MaritalStatusDtoProperty();
                            finAidMarStatus = await _financialAidReferenceDataRepository.GetFinancialAidMaritalStatusAsync( "10", source.FafsaMaritalStatus );
                            dto.MaritalStatus.Value = source.FafsaMaritalStatus;

                        }
                        if(finAidMarStatus != null)
                        {
                            dto.MaritalStatus.Title = finAidMarStatus.Description;
                        }
                    }
                    #endregion maritalStatus

                    //applicantFamilySize
                    dto.ApplicantFamilySize = source.ApplicantFamilySize.HasValue? source.ApplicantFamilySize.Value : default(int?);

                    //parentFamilySize
                    dto.ParentFamilySize = source.ParentFamilySize.HasValue ? source.ParentFamilySize.Value : default( int? );

                    //applicantNumberInCollege
                    dto.ApplicantNumberInCollege = source.ApplicantNumberInCollege.HasValue ? source.ApplicantNumberInCollege.Value : default( int? );

                    //parentNumberInCollege
                    dto.ParentNumberInCollege = source.ParentNoInCollege.HasValue ? source.ParentNoInCollege.Value : default( int? );

                    //parentsEducationLevel
                    FinancialAidEducationLevelDtoProperty pelDto = null;
                    if(!string.IsNullOrEmpty(source.FatherEducationLevel))
                    {
                        if( pelDto == null ) pelDto = new FinancialAidEducationLevelDtoProperty();
                        pelDto.FirstParent = ConvertParentEducationLevel( source.FatherEducationLevel );
                    }

                    if( !string.IsNullOrEmpty( source.MotherEducationLevel ) )
                    {
                        if( pelDto == null ) pelDto = new FinancialAidEducationLevelDtoProperty();
                        pelDto.SecondParent = ConvertParentEducationLevel( source.MotherEducationLevel );
                    }
                    if( pelDto != null ) dto.ParentsEducationLevel = pelDto;

                    financialAidApplicationDtos.Add(dto);


                }
                catch (Exception e)
                {
                    IntegrationApiExceptionAddError( string.Format( "Unable to build DTO for application. Entity:'ISIR.FAFSA', Record Id: '{0}'", source.Id ), "Bad.Data", source.Guid, source.Id );
                }
            }
            return financialAidApplicationDtos;
        }

        /// <summary>
        /// Returns Financial Aid Education Level.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private FinancialAidApplicationsEducationLevel ConvertParentEducationLevel( string source )
        {
            FinancialAidApplicationsEducationLevel level = FinancialAidApplicationsEducationLevel.NotSet;
            switch( source )
            {
                case ( "1" ):
                    level = FinancialAidApplicationsEducationLevel.MiddleSchool; break;
                case ( "2" ):
                    level = FinancialAidApplicationsEducationLevel.HighSchool; break;
                case ( "3" ):
                    level = FinancialAidApplicationsEducationLevel.College; break;
                case ( "4" ):
                    level = FinancialAidApplicationsEducationLevel.Other; break;
            }
            return level;
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
            if (independenceCriteria != null && independenceCriteria.Any())
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
    }
}
