//Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
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
    public class StudentFinancialAidNeedSummaryService : BaseCoordinationService, IStudentFinancialAidNeedSummaryService
    {

        private readonly IStudentFinancialAidNeedSummaryRepository _studentFinancialAidNeedSummaryRespository;
        private readonly IPersonRepository _personRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IConfigurationRepository configurationRepository;

        public StudentFinancialAidNeedSummaryService(

            IStudentFinancialAidNeedSummaryRepository StudentFinancialAidNeedSummaryRespository,
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
            _studentFinancialAidNeedSummaryRespository = StudentFinancialAidNeedSummaryRespository;
            _personRepository = personRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 9</remarks>
        /// <summary>
        /// Gets all financial-aid-applications
        /// </summary>
        /// <returns>Collection of FinancialAidApplications DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentFinancialAidNeedSummary>, int>> GetAsync(int offset, int limit, bool bypassCache = false)
        {
            CheckViewStudentFinancialAidNeedSummariesPermission();

            // Get all financial aid years
            var aidYearEntity = (await _studentReferenceDataRepository.GetFinancialAidYearsAsync(bypassCache));
            List<string> faSuiteYears = aidYearEntity.Select(k => k.Code).ToList();

            var studentFinancialAidNeedSummaryDtos = new List<Dtos.StudentFinancialAidNeedSummary>();
            var studentNeedSummaryDomainTuple = await _studentFinancialAidNeedSummaryRespository.GetAsync(offset, limit, bypassCache, faSuiteYears);
            var studentFinancialAidNeedSummaryDomainEntities = studentNeedSummaryDomainTuple.Item1;
            var totalRecords = studentNeedSummaryDomainTuple.Item2;

            if (studentFinancialAidNeedSummaryDomainEntities == null)
            {
                throw new ArgumentNullException("StudentFinancialAidNeedSummaryDomainEntity", "StudentFinancialAidNeedSummaryDomainEntity cannot be null. ");
            }

            // Convert the student financial aid need sumary and all its child objects into DTOs.
            foreach (var entity in studentFinancialAidNeedSummaryDomainEntities)
            {
                if (entity != null)
                {
                    var studentFinancialAidNeedSummaryDto = await BuildStudentFinancialAidNeedSummaryDtoAsync(entity, bypassCache);
                    studentFinancialAidNeedSummaryDtos.Add(studentFinancialAidNeedSummaryDto);
                }
            }
            return new Tuple<IEnumerable<Dtos.StudentFinancialAidNeedSummary>, int>(studentFinancialAidNeedSummaryDtos, totalRecords);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 9</remarks>
        /// <summary>
        /// Get a StudentFinancialAidNeedSummaries from its GUID
        /// </summary>
        /// <returns>FinancialAidApplications DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.StudentFinancialAidNeedSummary> GetByIdAsync(string id)
        {
            CheckViewStudentFinancialAidNeedSummariesPermission();

            try
            {
                //// Get the student financial aid awards domain entity from the repository
                var studentNeedSummaryDomainEntity = await _studentFinancialAidNeedSummaryRespository.GetByIdAsync(id);
                if (studentNeedSummaryDomainEntity == null)
                {
                    throw new ArgumentNullException("FinancialAidApplicationDomainEntity", "FinancialAidApplicationDomainEntity cannot be null. ");
                }

                //// Convert the financial aid application object into DTO.
                return await BuildStudentFinancialAidNeedSummaryDtoAsync(studentNeedSummaryDomainEntity);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("student-financial-aid-need-summaries not found for GUID " + id, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("student-financial-aid-need-summaries not found for GUID " + id, ex);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<Dtos.StudentFinancialAidNeedSummary> BuildStudentFinancialAidNeedSummaryDtoAsync(StudentNeedSummary studentNeedSummaryEntity, bool bypassCache = true)
        {
            var studentFinancialAidNeedSummaryDto = new Dtos.StudentFinancialAidNeedSummary();
            
            try
            {
                studentFinancialAidNeedSummaryDto.Id = studentNeedSummaryEntity.Guid;

                //
                // Set Applicant
                //
                var applicant = new GuidObject2((!string.IsNullOrEmpty(studentNeedSummaryEntity.StudentId)) ?
                   await _personRepository.GetPersonGuidFromIdAsync(studentNeedSummaryEntity.StudentId) :
                   string.Empty);
                studentFinancialAidNeedSummaryDto.Applicant = applicant;

                //
                // Set AidYear
                //
                if (!string.IsNullOrEmpty(studentNeedSummaryEntity.AwardYear))
                {
                    var aidYearEntity = (await _studentReferenceDataRepository.GetFinancialAidYearsAsync(bypassCache)).FirstOrDefault(t => t.Code == studentNeedSummaryEntity.AwardYear);
                    if (aidYearEntity != null && !string.IsNullOrEmpty(aidYearEntity.Guid))
                    {
                        studentFinancialAidNeedSummaryDto.AidYear = new GuidObject2(aidYearEntity.Guid);
                    }
                }

                //
                // Build NeedByMethodology for federal
                //
                var needsByMethodology = new List<Dtos.DtoProperties.StudentFinancialAidNeedsByMethodologyDtoProperty>();
                if (studentNeedSummaryEntity.FederalNeedAmount.HasValue)
                {
                    var federalNeed = new StudentFinancialAidNeedsByMethodologyDtoProperty();           
                    federalNeed.Methodology = Dtos.EnumProperties.StudentFinancialAidNeedMethodology.Federal;
                    var outcomeId = await _studentFinancialAidNeedSummaryRespository.GetIsirCalcResultsGuidFromIdAsync(studentNeedSummaryEntity.CsFederalIsirId);                    
                    if (!string.IsNullOrEmpty(outcomeId))
                    {
                        federalNeed.ApplicationOutcome = new GuidObject2(outcomeId);
                        federalNeed.BudgetDuration = studentNeedSummaryEntity.BudgetDuration;

                        var totalCostOfAttendance = new Dtos.DtoProperties.AmountDtoProperty();
                        if (studentNeedSummaryEntity.FederalTotalExpenses.HasValue)
                        {
                            if (studentNeedSummaryEntity.FederalTotalExpenses >= 0)
                            {
                                totalCostOfAttendance.Value = studentNeedSummaryEntity.FederalTotalExpenses;
                                totalCostOfAttendance.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                                federalNeed.TotalCostOfAttendance = totalCostOfAttendance;
                            }
                        }

                        var familyContribution = new Dtos.DtoProperties.AmountDtoProperty();
                        if (studentNeedSummaryEntity.FederalFamilyContribution.HasValue)
                        {
                            if (studentNeedSummaryEntity.FederalFamilyContribution >= 0)
                            {
                                familyContribution.Value = studentNeedSummaryEntity.FederalFamilyContribution;
                                familyContribution.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                                federalNeed.ExpectedFamilyContribution = familyContribution;
                            }
                        }

                        var grossNeed = new Dtos.DtoProperties.AmountDtoProperty();                        
                        grossNeed.Value = studentNeedSummaryEntity.FederalNeedAmount;
                        grossNeed.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                        federalNeed.GrossNeed = grossNeed;

                        if (studentNeedSummaryEntity.HasAward == true)
                        {
                            var totalNeedReduction = new Dtos.DtoProperties.AmountDtoProperty();
                            if (studentNeedSummaryEntity.FederalTotalNeedReduction.HasValue)
                            {
                                if (studentNeedSummaryEntity.FederalTotalNeedReduction >= 0)
                                {
                                    totalNeedReduction.Value = studentNeedSummaryEntity.FederalTotalNeedReduction;
                                    totalNeedReduction.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                                    federalNeed.TotalNeedReduction = totalNeedReduction;
                                }
                            }
                        }

                        needsByMethodology.Add(federalNeed);
                    }
                    else
                    {
                        // Error because CS.NEED exists without a CALC.ISIR.RESULTS record pointed to from CS.FED.ISIR.ID
                        var message = "Federal need in CS.NEED exists without valid CS.FED.ISIR.ID pointer to ISIR.CALC.RESULTS for ID " + studentNeedSummaryEntity.Guid;
                        logger.Error(message);
                        //throw new Exception(message);
                    }
                }

                //
                // Build NeedByMethodology for institutional
                //
                if (studentNeedSummaryEntity.InstitutionalNeedAmount != null)
                {
                    var institutionalNeed = new StudentFinancialAidNeedsByMethodologyDtoProperty();
                    institutionalNeed.Methodology = Dtos.EnumProperties.StudentFinancialAidNeedMethodology.Institutional;
                    var outcomeId = await _studentFinancialAidNeedSummaryRespository.GetIsirCalcResultsGuidFromIdAsync(studentNeedSummaryEntity.CsInstitutionalIsirId);                    
                    if (!string.IsNullOrEmpty(outcomeId))
                    {
                        institutionalNeed.ApplicationOutcome = new GuidObject2(outcomeId);
                        institutionalNeed.BudgetDuration = studentNeedSummaryEntity.BudgetDuration;

                        var totalCostOfAttendance = new Dtos.DtoProperties.AmountDtoProperty();
                        if (studentNeedSummaryEntity.InstitutionalTotalExpenses.HasValue)
                        {
                            if (studentNeedSummaryEntity.InstitutionalTotalExpenses >= 0)
                            {
                                totalCostOfAttendance.Value = studentNeedSummaryEntity.InstitutionalTotalExpenses;
                                totalCostOfAttendance.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                                institutionalNeed.TotalCostOfAttendance = totalCostOfAttendance;
                            }
                        }

                        var familyContribution = new Dtos.DtoProperties.AmountDtoProperty();
                        if (studentNeedSummaryEntity.InstitutionalFamilyContribution.HasValue)
                        {
                            if (studentNeedSummaryEntity.InstitutionalFamilyContribution >= 0)
                            {
                                familyContribution.Value = studentNeedSummaryEntity.InstitutionalFamilyContribution;
                                familyContribution.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                                institutionalNeed.ExpectedFamilyContribution = familyContribution;
                            }
                        }

                        var grossNeed = new Dtos.DtoProperties.AmountDtoProperty();
                        grossNeed.Value = studentNeedSummaryEntity.InstitutionalNeedAmount;
                        grossNeed.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                        institutionalNeed.GrossNeed = grossNeed;

                        if (studentNeedSummaryEntity.HasAward == true)
                        {
                            var totalNeedReduction = new Dtos.DtoProperties.AmountDtoProperty();
                            if (studentNeedSummaryEntity.InstitutionalTotalNeedReduction.HasValue)
                            {
                                if (studentNeedSummaryEntity.InstitutionalTotalNeedReduction >= 0)
                                {
                                    totalNeedReduction.Value = studentNeedSummaryEntity.InstitutionalTotalNeedReduction;
                                    totalNeedReduction.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                                    institutionalNeed.TotalNeedReduction = totalNeedReduction;
                                }
                            }
                        }

                        needsByMethodology.Add(institutionalNeed);
                    }
                    else
                    {
                        // Error because CS.INST.NEED exists without a CALC.ISIR.RESULTS record pointed to from CS.INST.ISIR.ID
                        var message = "Federal need in CS.NEED exists without valid CS.INST.ISIR.ID pointer to ISIR.CALC.RESULTS for ID " + studentNeedSummaryEntity.Guid;
                        logger.Error(message);
                    }
                }

                if (needsByMethodology.Any())
                {
                    studentFinancialAidNeedSummaryDto.NeedsByMethodology = needsByMethodology;
                }
                else
                {
                    var message = "Unable to build needsByMethodology for ID " + studentNeedSummaryEntity.Guid;
                    logger.Error(message); 
                    throw new Exception(message);
                }
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch
            {
                var message = "Unable to process Student Financial Aid Need Summary for ID " + studentNeedSummaryEntity.Guid + ".  Check API error log.";
                throw new Exception(message);
            }

            return studentFinancialAidNeedSummaryDto;
        }
        
        /// <summary>
        /// Helper method to determine if the user has permission to view Student StudentFinancialAidNeedSummaries.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewStudentFinancialAidNeedSummariesPermission()
        {
            bool hasPermission = HasPermission(StudentPermissionCodes.ViewStudentFinancialAidNeedSummaries);

            // User is not allowed to read StudentFinancialAidNeedSummaries without the appropriate permissions
            if (!hasPermission)
            {
                throw new PermissionsException(string.Format("User {0} does not have permission to view student-financial-aid-need-summaries.", CurrentUser.UserId));
            }
        }
    }
}